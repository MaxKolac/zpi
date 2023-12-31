﻿using System.Net;

namespace ZPIServer.EventArgs;

public class TcpReceiverEventArgs : System.EventArgs
{
    /// <summary>
    /// Adres IP urządzenia, od którego otrzymano dane.
    /// </summary>
    public IPAddress SenderIp { get; private set; }
    /// <summary>
    /// Numer portu, z którego urządzenie przysłało dane.
    /// </summary>
    public int SenderPort { get; private set; }
    /// <summary>
    /// Otrzymany surowy ciąg bajtów.
    /// </summary>
    public byte[] Data { get; private set; }

    ///<summary></summary>
    /// <param name="senderIp">Adres IP urządzenia, od którego otrzymano dane.</param>
    /// <param name="data">Otrzymany surowy ciąg bajtów.</param>
    public TcpReceiverEventArgs(IPAddress senderIp, int senderPort, byte[] data) : base()
    {
        SenderIp = senderIp;
        SenderPort = senderPort;
        Data = data;
    }
}
