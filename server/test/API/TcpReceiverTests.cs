using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using ZPIServer.API;
using ZPIServer.EventArgs;

namespace ZPIServerTests.API;

public class TcpReceiverTests
{
    [Fact]
    public static void CheckIsListening()
    {
        var handler = new TcpReceiver(IPAddress.Parse("127.0.0.1"), 25565);

        handler.Enable();
        Assert.True(handler.IsListening);

        handler.Disable();
        Assert.False(handler.IsListening);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[] { })]
    [InlineData(new int[] { 25565, 25565 })]
    [InlineData(new int[] { 1023 })]
    [InlineData(new int[] { 65536 })]
    public static void CheckListenPortConditionsInConstructor(int[] ports)
    {
        IPAddress address = IPAddress.Parse("127.0.0.1");
        Assert.Throws<ArgumentException>(() =>
        {
            var handler = new TcpReceiver(address, ports);
        });
    }

    [Theory]
    [InlineData(new int[] { 13131, 12345, 11131 })]
    [InlineData(new int[] { 11111, 22222, 12345 })]
    public static void CheckThatOccupiedPortsAreOmitted(int[] ports)
    {
        IPAddress address = IPAddress.Parse("127.0.0.1");
        var portOccupant = new TcpListener(address, 12345);
        portOccupant.Start();
        var handler = new TcpReceiver(address, ports);
        handler.Enable();

        Assert.Equal(ports.Length - 1, handler.ActiveListenersCount());
        portOccupant.Stop();
        handler.Disable();
    }

    [Fact]
    public static void CheckThatBeginListeningThrowsOnAllPortsOccupied()
    {
        int samePort = 13223;
        IPAddress address = IPAddress.Parse("127.0.0.1");
        var portOccupant = new TcpListener(address, samePort);
        portOccupant.Start();
        var handler = new TcpReceiver(address, samePort);

        Assert.Throws<IOException>(() =>
        {
            handler.Enable();
        });
    }

    [Fact]
    public static async Task CheckMessagesAreComingThrough()
    {
        var handler = new TcpReceiver(IPAddress.Parse("127.0.0.1"), 25565);
        handler.Enable();
        string messageToSend = "Hello World!";

        int timesInvoked = 0;
        byte[] receivedBytes = new byte[1024];
        IPAddress? senderIp = null;
        int senderPort = 0;
        int localClientPort = -1;

        using (var clientMock = new TcpClient())
        {
            clientMock.Connect(IPAddress.Parse("127.0.0.1"), 25565);
            localClientPort = ((IPEndPoint)clientMock.Client.LocalEndPoint!).Port;
            EventHandler<TcpReceiverEventArgs> eventHandler = (sender, e) =>
            {
                receivedBytes = e.Data;
                senderIp = e.SenderIp;
                senderPort = e.SenderPort;
                timesInvoked++;
            };
            TcpReceiver.OnSignalReceived += eventHandler;
            using (var stream = clientMock.GetStream())
            {
                byte[] message = Encoding.UTF8.GetBytes(messageToSend);
                stream.Write(message, 0, message.Length);
            }
            await Task.Delay(100);
            TcpReceiver.OnSignalReceived -= eventHandler;
        }
        handler.Disable();
        string receivedMessage = Encoding.UTF8.GetString(receivedBytes).TrimEnd('\0');

        Assert.Equal(IPAddress.Parse("127.0.0.1"), senderIp);
        Assert.Equal(senderPort, localClientPort);
        Assert.Equal(messageToSend, receivedMessage);
        Assert.Equal(1, timesInvoked);
    }

    [Fact]
    public static async void CheckManyMessagesAreComingThrough()
    {
        IPAddress address = IPAddress.Parse("127.0.0.1");
        int[] ports = new int[] { 25565, 25566, 25567 };
        const string messageToSend = "Hello World!";

        int timesInvoked = 0;
        List<string> receivedMessages = new();
        List<(IPAddress?, int)> connectedClientInfo = new();

        //Create TcpReceiver
        var handler = new TcpReceiver(address, ports);
        handler.Enable();

        //Create one TcpClient per handler's port and connect them to the server
        var clientMocks = new TcpClient[ports.Length];
        for (int i = 0; i < ports.Length; i++)
        {
            clientMocks[i] = new TcpClient();
            clientMocks[i].Connect(address, ports[i]);
        }

        //Subscribe to the event with a EventHandler that will fire once a full message has been received
        //This event will modify variables which will be validated with Asserts later on
        EventHandler<TcpReceiverEventArgs> eventHandler = (sender, e) =>
        {
            receivedMessages.Add(Encoding.UTF8.GetString(e.Data));
            connectedClientInfo.Add((e.SenderIp, e.SenderPort));
            timesInvoked++;
        };
        TcpReceiver.OnSignalReceived += eventHandler;

        //Send the bytes of messageToSend on each client
        //Message will be appended with client's port to make sure all received messages are unique.
        //Since mocks will be disposed with Close() method, their ports will be preserved separately
        List<int> clientMockPorts = new();
        foreach (var mock in clientMocks)
        {
            int mockPort = ((IPEndPoint)mock.Client.LocalEndPoint!).Port;
            clientMockPorts.Add(mockPort);

            byte[] message = Encoding.UTF8.GetBytes(messageToSend + mockPort);
            using var stream = mock.GetStream();
            stream.Write(message, 0, message.Length);
            await Task.Delay(100);
        }

        //Signal to close the TCP connection and dispose mocks
        foreach (var mock in clientMocks)
        {
            mock.Close();
        }

        //Assert that everything went well
        foreach (var clientMockPort in clientMockPorts)
        {
            Assert.Contains(messageToSend + clientMockPort, receivedMessages);
            Assert.Contains((address, clientMockPort), connectedClientInfo);
        }
        Assert.Equal(3, timesInvoked);

        TcpReceiver.OnSignalReceived -= eventHandler;
    }

    [Theory]
    [InlineData(new int[] { 25565 })]
    [InlineData(new int[] { 25565, 25566 })]
    [InlineData(new int[] { 25565, 25566, 25567 })]
    public static void CheckPortsAreOccupied(int[] ports)
    {
        var handler = new TcpReceiver(IPAddress.Parse("127.0.0.1"), ports);
        handler.Enable();

        List<int> listeningPorts = GetCurrentlyListeningTcpPorts();
        foreach (var port in ports)
        {
            Assert.Contains(port, listeningPorts);
            Assert.Throws<SocketException>(() =>
            {
                var conflictingListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                conflictingListener.Start();
            });
            Assert.Equal(ports.Length, handler.ListenersCount);
        }

        handler.Disable();
        listeningPorts = GetCurrentlyListeningTcpPorts();
        foreach (var port in ports)
        {
            Assert.DoesNotContain(port, listeningPorts);
        }

        //Check if another TcpListener could use those ports once TcpReceiver stops
        foreach (var port in ports)
        {
            var anotherListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            anotherListener.Start();
            anotherListener.Stop();
        }
    }

    private static List<int> GetCurrentlyListeningTcpPorts()
    {
        IPEndPoint[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
        List<int> ports = new();
        IPAddress address = IPAddress.Parse("127.0.0.1");
        for (int i = 0; i < tcpConnections.Length; i++)
        {
            if (tcpConnections[i].Address.Equals(address))
                ports.Add(tcpConnections[i].Port);
        }
        return ports;
    }
}
