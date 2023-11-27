using System.Net;
using System.Net.Sockets;

namespace ZPIServer.API;

public static partial class Extensions
{
    /// <summary>
    /// Gets the port of the local endpoint.
    /// </summary>
    public static int GetLocalPort(this TcpListener listener)
    {
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    /// <inheritdoc cref="GetLocalPort(TcpListener)"/>
    public static int GetLocalPort(this TcpClient listener)
    {
        return (listener.Client.LocalEndPoint as IPEndPoint)!.Port;
    }

    /// <summary>
    /// Gets the IP address of the local endpoint.
    /// </summary>
    public static IPAddress GetLocalAddress(this TcpListener listener)
    {
        return ((IPEndPoint)listener.LocalEndpoint).Address;
    }

    /// <summary>
    /// Gets the port of the connected <see cref="TcpClient"/>.
    /// </summary>
    public static int GetRemotePort(this TcpClient listener)
    {
        return (listener.Client.RemoteEndPoint as IPEndPoint)!.Port;
    }
    
    /// <summary>
    /// Gets the IP address of the connected <see cref="TcpClient"/>.
    /// </summary>
    public static IPAddress GetRemoteAddress(this TcpClient listener)
    {
        return (listener.Client.RemoteEndPoint as IPEndPoint)!.Address;
    }

    /// <summary>
    /// Checks if the <see cref="TcpListener"/> is active and listening on the given port.
    /// <see href="https://stackoverflow.com/a/59482929/21342746"/>
    /// </summary>
    public static bool IsActive(this TcpListener listener)
    {
        return listener.Server.IsBound;
    }
}