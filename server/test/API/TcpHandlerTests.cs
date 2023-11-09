using System.Net;
using System.Net.NetworkInformation;
using ZPIServer.API;

namespace ZPIServerTests.API;

public class TcpHandlerTests
{
    [Fact]
    static void CheckIsListening()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);

        handler.BeginListening();
        Assert.True(handler.IsListening);

        handler.StopListening();
        Assert.False(handler.IsListening);
    }

    [Fact]
    static void CheckPortsAreListening()
    {
        var handler = new TcpHandler(IPAddress.Parse("127.0.0.1"), 25565);

        handler.BeginListening();
        List<int> listeningPorts = GetCurrentlyListeningTcpPorts();
        Assert.Contains(25565, listeningPorts);

        handler.StopListening();
        listeningPorts = GetCurrentlyListeningTcpPorts();
        Assert.DoesNotContain(25565, listeningPorts);
    }

    static List<int> GetCurrentlyListeningTcpPorts()
    {
        IPEndPoint[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
        List<int> ports = new();
        IPAddress address = IPAddress.Parse("127.0.0.1");
        for (int i = 0; i < tcpConnections.Length; i++)
        {
            if (tcpConnections[i].Address == address)
                ports.Add(i);
        }
        return ports;
    }
}
