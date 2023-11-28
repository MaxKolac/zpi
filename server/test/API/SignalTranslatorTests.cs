using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;
using ZPIServer;
using ZPIServer.API;
using ZPIServer.EventArgs;

namespace ZPIServerTests.API;

public class SignalTranslatorTests
{
    [Fact]
    public static void CheckIsTranslating()
    {
        var translator = new SignalTranslator();
        translator.BeginTranslating();
        Assert.True(translator.IsTranslating);

        translator.StopTranslating();
        Assert.False(translator.IsTranslating);
    }

    static void DatabaseSetup(HostDevice.HostType typeToAdd)
    {
        //DB setup
        using var context = new DatabaseContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        var hostDevice = new HostDevice()
        {
            Name = "test",
            Address = IPAddress.Parse("127.0.0.1"),
            Type = typeToAdd
        };
        context.HostDevices.Add(hostDevice);
        context.SaveChanges();
    }

    static void SendSerializedJsonToLoopback(int portToSendTo, string json)
    {
        using var client = new TcpClient();
        client.Connect("127.0.0.1", portToSendTo);

        using var stream = client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        stream.Write(buffer, 0, buffer.Length);
    }

    [Fact]
    public static async Task CheckReceivingValidSignal_CameraSimulator()
    {
        DatabaseSetup(HostDevice.HostType.CameraSimulator);

        //Prepare to send the signal
        var handler = new TcpReceiver(IPAddress.Loopback, 25565);
        handler.Enable();
        var translator = new SignalTranslator();
        translator.BeginTranslating();

        //Prepare a message to send
        var bitmap = HostDevice.ToByteArray(new Bitmap(100, 100), ImageFormat.Bmp);
        var message = new CameraDataMessage()
        {
            LargestTemperature = 1234.56m,
            Image = bitmap,
            Status = HostDevice.DeviceStatus.OK
        };

        //Setup a check that OnSignalReceived was invoked
        int invocations = 0;
        EventHandler<TcpReceiverEventArgs> eventHandler = (sender, e) =>
        {
            invocations++;
        };
        TcpReceiver.OnSignalReceived += eventHandler;

        //Act
        SendSerializedJsonToLoopback(25565, JsonConvert.SerializeObject(message));

        //Give SignalTranslator some time to process the message and make changes
        await Task.Delay(2000).WaitAsync(CancellationToken.None);

        //Check that data has been written into DB
        using (var context = new DatabaseContext())
        {
            var record = context.HostDevices.Where((host) => host.Name == "test").FirstOrDefault();

            Assert.NotNull(record);
            Assert.Equal(1, invocations);
            Assert.Equal(message.LargestTemperature, record.LastKnownTemperature);
            Assert.Equal(message.Status, record.LastKnownStatus);
            Assert.Equal(bitmap.Length, record.LastImage?.Length);
        }

        //Turn things off
        TcpReceiver.OnSignalReceived -= eventHandler;
        translator.StopTranslating();
        handler.Disable();
    }

    [Theory]
    [MemberData(nameof(GetInvalidMessage_CameraSimulator), MemberType = typeof(SignalTranslatorTests))]
    public static async Task CheckReceivingInvalidSignal_CameraSimulator(string invalidMessage)
    {
        DatabaseSetup(HostDevice.HostType.CameraSimulator);

        //Prepare to send the signal
        var handler = new TcpReceiver(IPAddress.Loopback, 25565);
        handler.Enable();
        var translator = new SignalTranslator();
        translator.BeginTranslating();

        //Setup a check that OnSignalReceived was invoked
        int invocations = 0;
        EventHandler<TcpReceiverEventArgs> eventHandler = (sender, e) =>
        {
            invocations++;
        };
        TcpReceiver.OnSignalReceived += eventHandler;

        //Act
        SendSerializedJsonToLoopback(25565, JsonConvert.SerializeObject(invalidMessage));

        //Give SignalTranslator some time to process the message and make changes
        await Task.Delay(1000);

        //Check that data has been written into DB
        using (var context = new DatabaseContext())
        {
            var record = context.HostDevices.Where((host) => host.Name == "test").FirstOrDefault();

            Assert.NotNull(record);
            Assert.Equal(1, invocations);
            Assert.Equal(0.0m, record.LastKnownTemperature);
            Assert.Equal(HostDevice.DeviceStatus.DataCorrupted, record.LastKnownStatus);
            Assert.Null(record.LastImage);
        }

        //Turn things off
        TcpReceiver.OnSignalReceived -= eventHandler;
        translator.StopTranslating();
        handler.Disable();
    }

    public static IEnumerable<object?[]> GetInvalidMessage_CameraSimulator()
    {
        //Random gargle
        yield return new object?[] { "urn2f89n2498j9s 8fn9jg98kqwoidw98gh3 9fj3r9jeje;23r93;;////" };

        var messageInvalidImage = new CameraDataMessage()
        {
            LargestTemperature = 1234.56m,
            Image = new byte[] { 69, 69, 255, 255, 69, 0, 0 },
            Status = HostDevice.DeviceStatus.OK
        };
        yield return new object?[] { JsonConvert.SerializeObject(messageInvalidImage) };

        var messageWrongEncoding = new CameraDataMessage()
        {
            LargestTemperature = 1234.56m,
            Image = HostDevice.ToByteArray(new Bitmap(10, 10), ImageFormat.Png),
            Status = HostDevice.DeviceStatus.OK
        };
        yield return new object?[] { Encoding.UTF8.GetString(Encoding.UTF32.GetBytes(JsonConvert.SerializeObject(messageWrongEncoding))) };
    }
}
