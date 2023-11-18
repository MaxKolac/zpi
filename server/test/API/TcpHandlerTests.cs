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
            var handler = new TcpHandler(address, ports);
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
        var handler = new TcpHandler(address, ports);
        handler.BeginListening();

        Assert.Equal(ports.Length - 1, handler.ActiveListenersCount());
        portOccupant.Stop();
        handler.StopListening();
    }

    [Fact]
    public static void CheckThatBeginListeningThrowsOnAllPortsOccupied()
    {
        int samePort = 13223;
        IPAddress address = IPAddress.Parse("127.0.0.1");
        var portOccupant = new TcpListener(address, samePort);
        portOccupant.Start();
        var handler = new TcpHandler(address, samePort);

        Assert.Throws<IOException>(() =>
        {
            handler.BeginListening();
        });
    }

    //Ten test działa tylko w DebugMode (nie wiem czemu??)
    [Fact]
    public static void CheckMessagesAreComingThrough()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);
        handler.BeginListening();
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

    //Działa tylko podczas debugowania (nadal nie wiem czemu)
    [Fact]
    public static void CheckManyMessagesAreComingThrough()
    {
        IPAddress address = IPAddress.Parse("127.0.0.1");
        int[] ports = new int[] { 25565, 25566, 25567 };
        const string messageToSend = "Hello World!";

        int timesInvoked = 0;
        List<string> receivedMessages = new();
        List<(IPAddress?, int)> connectedClientInfo = new();

        //Create TcpHandler
        var handler = new TcpHandler(address, ports);
        handler.BeginListening();

        //Create one TcpClient per handler's port and connect them to the server
        var clientMocks = new TcpClient[ports.Length];
        for (int i = 0; i < ports.Length; i++)
        {
            clientMocks[i] = new TcpClient();
            clientMocks[i].Connect(address, ports[i]);
        }

        //Subscribe to the event with a EventHandler that will fire once a full message has been received
        //This event will modify variables which will be validated with Asserts later on
        EventHandler<TcpHandlerEventArgs> eventHandler = (sender, e) =>
        {
            receivedMessages.Add(Encoding.UTF8.GetString(e.Data));
            connectedClientInfo.Add((e.SenderIp, e.SenderPort));
            timesInvoked++;
        };
        TcpHandler.OnSignalReceived += eventHandler;

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

        TcpHandler.OnSignalReceived -= eventHandler;
    }

    [Theory]
    [InlineData(new int[] { 25565 })]
    [InlineData(new int[] { 25565, 25566 })]
    [InlineData(new int[] { 25565, 25566, 25567 })]
    public static void CheckPortsAreOccupied(int[] ports)
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), ports);
        handler.BeginListening();

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

        handler.StopListening();
        listeningPorts = GetCurrentlyListeningTcpPorts();
        foreach (var port in ports)
        {
            Assert.DoesNotContain(port, listeningPorts);
        }

        //Check if another TcpListener could use those ports once TcpHandler stops
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
