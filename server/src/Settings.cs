using System.Net;

namespace ZPIServer;

public static class Settings
{
    /// <summary>
    /// Tabela portów, na których <see cref="API.TcpHandler"/> będzie nasłuchiwał przychodzących połączeń od kamer i użytkowników.<br/>
    /// Domyślny port to 25565.
    /// </summary>
    public static int TcpListeningPort { get; set; } = 25565;

    /// <summary>
    /// Adres IP serwera w sieci lokalnej. Na tym adresie będą nasłuchiwane porty.<br/>
    /// Domyślna wartość to 127.0.0.1.
    /// </summary>
    public static IPAddress ServerAddress { get; set; } = IPAddress.Parse("127.0.0.1");

    public static void ResetToDefault()
    {
        TcpListeningPort = 25565;
        ServerAddress = IPAddress.Parse("127.0.0.1");
    }
}
