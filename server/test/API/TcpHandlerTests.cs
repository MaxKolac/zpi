using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using ZPIServer.API;
using ZPIServer.EventArgs;

namespace ZPIServerTests.API;

public class TcpHandlerTests
{
    [Fact]
    public static void CheckIsListening()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);

        handler.BeginListening();
        Assert.True(handler.IsListening);

        handler.StopListening();
        Assert.False(handler.IsListening);
    }

    //Ten test działa tylko w DebugMode (nie wiem czemu??)
    [Fact]
    public static void CheckMessagesAreComingThrough()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);
        handler.BeginListening();
        int timesInvoked = 0;
        byte[] receivedBytes = new byte[1024];
        IPAddress? senderIp = null;
        string messageToSend = "Hello World!";
        int senderPort = 0;
        int localClientPort = -1;

        using (var clientMock = new TcpClient())
        {
            clientMock.Connect(IPAddress.Parse("127.0.0.1"), 25565);
            localClientPort = ((IPEndPoint)clientMock.Client.LocalEndPoint!).Port;
            EventHandler<TcpHandlerEventArgs> eventHandler = (sender, e) =>
            {
                receivedBytes = e.Data;
                senderIp = e.SenderIp;
                senderPort = e.SenderPort;
                timesInvoked++;
            };
            TcpHandler.OnSignalReceived += eventHandler;
            using (var stream = clientMock.GetStream())
            {
                byte[] message = Encoding.UTF8.GetBytes(messageToSend);
                stream.Write(message, 0, message.Length);
            }
            TcpHandler.OnSignalReceived -= eventHandler;
        }
        handler.StopListening();
        string receivedMessage = Encoding.UTF8.GetString(receivedBytes).TrimEnd('\0');

        Assert.Equal(IPAddress.Parse("127.0.0.1"), senderIp);
        Assert.Equal(senderPort, localClientPort);
        Assert.Equal(messageToSend, receivedMessage);
        Assert.Equal(1, timesInvoked);
    }

    [Fact]
    public static void CheckPortsAreOccupied()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);
        handler.BeginListening();
        List<int> listeningPorts = GetCurrentlyListeningTcpPorts();

        Assert.Contains(25565, listeningPorts);
        Assert.Throws<SocketException>(() =>
        {
            var conflictingListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 25565);
            conflictingListener.Start();
        });

        handler.StopListening();
        listeningPorts = GetCurrentlyListeningTcpPorts();
        Assert.DoesNotContain(25565, listeningPorts);

        //Check if another TcpListener could use this port once TcpHandler stops
        var anotherListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 25565);
        anotherListener.Start();
        anotherListener.Stop();
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
