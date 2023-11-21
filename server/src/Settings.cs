using System.Net;

namespace ZPIServer;

public static class Settings
{
    /// <summary>
    /// Tabela portów, na których <see cref="API.TcpHandler"/> będzie nasłuchiwał przychodzących połączeń od kamer i użytkowników.<br/>
    /// Domyślne porty to 25565, 25566, 25567.
    /// </summary>
    public static int[] TcpListeningPort { get; set; } = new int[] { 25565, 25566, 25567 };

    /// <summary>
    /// Adres IP serwera w sieci lokalnej. Na tym adresie będą nasłuchiwane porty.<br/>
    /// Domyślna wartość to 127.0.0.1.
    /// </summary>
    public static IPAddress ServerAddress { get; set; } = IPAddress.Parse("127.0.0.1");

    /// <summary>
    /// Wskazuje, czy serwer wykrył instalację Python'a potrzebnego do <see cref="API.CameraLibraries.PythonCameraSimulatorAPI"/>.<br/>
    /// 1 oznacza, że wykryto wersję Python'a nowszą od 3.0.0.<br/>
    /// 0 oznacza, że wykryto przestarzałą wersję Python'a, starszą od 3.0.0.<br/>
    /// -1 oznacza, że nie wykryto żadnej zainstalowanej wersji Python'a.<br/>
    /// Domyślna wartość to -1.
    /// </summary>
    public static int PythonInstallationStatus { get; set; } = -1;

    public static void ResetToDefault()
    {
        TcpListeningPort = new int[] { 25565, 25566, 25567 };
        ServerAddress = IPAddress.Parse("127.0.0.1");
        PythonInstallationStatus = -1;
    }
}
