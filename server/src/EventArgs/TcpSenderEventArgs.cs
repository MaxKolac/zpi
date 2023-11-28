using System.Net;

namespace ZPIServer.EventArgs;

public class TcpSenderEventArgs : System.EventArgs
{
    /// <summary>
    /// Adres IP, na który wysłana ma być wiadomość.
    /// </summary>
    public IPAddress RecipientAddress { get; private set; }
    /// <summary>
    /// Numer portu TCP, na który wysłana ma być wiadomość.
    /// </summary>
    public int RecipientPort { get; private set; }
    /// <summary>
    /// Surowy ciąg bitów do wysłania.
    /// </summary>
    public byte[] Data { get; private set; }

    public static new TcpSenderEventArgs Empty => new(IPAddress.Any, 0, new byte[1]);

    public TcpSenderEventArgs(IPAddress recipientIp, int recipientPort, byte[] data)
    {
        RecipientAddress = recipientIp;
        RecipientPort = recipientPort;
        Data = data;
    }
}
