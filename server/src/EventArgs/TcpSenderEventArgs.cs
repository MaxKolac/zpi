﻿using System.Net;

namespace ZPIServer.EventArgs;

public class TcpSenderEventArgs : System.EventArgs
{
    /// <summary>
    /// Adres IP, na który wysłana ma być wiadomość.
    /// </summary>
    public IPAddress RecipientIp { get; private set; }
    /// <summary>
    /// Numer portu TCP, na który wysłana ma być wiadomość.
    /// </summary>
    public int RecipientPort { get; private set; }
    /// <summary>
    /// Surowy ciąg bitów do wysłania.
    /// </summary>
    public byte[] Data {  get; private set; }

    public TcpSenderEventArgs(IPAddress recipientIp, int recipientPort, byte[] data)
    {
        RecipientIp = recipientIp;
        RecipientPort = recipientPort;
        Data = data;
    }
}
