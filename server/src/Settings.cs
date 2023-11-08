using System.Net;

namespace ZPIServer;

/// <summary>
/// Zestaw globalnie dostępnych ustawień.
/// </summary>
public static class Settings
{
    /// <summary>
    /// Kontroluje asynchroniczny dostęp do zmiany obecnych ustawień.
    /// </summary>
    public static SemaphoreSlim SettingsAccess { get; } = new SemaphoreSlim(1);

    /// <summary>
    /// Tabela portów, na których <see cref="API.TcpHandler"/> będzie nasłuchiwał przychodzących połączeń od kamer i użytkowników.<br/>
    /// Domyślne porty to 10000, 10001 i 10002.
    /// </summary>
    public static int[] TcpListeningPorts { get; set; } = new int[] { 10000, 10001, 10002 };

    /// <summary>
    /// Adres IP serwera w sieci lokalnej. Na tym adresie będą nasłuchiwane porty.<br/>
    /// Domyślna wartość to 127.0.0.1.
    /// </summary>
    public static IPAddress ServerAddress { get; set; } = IPAddress.Parse("127.0.0.1");
}
