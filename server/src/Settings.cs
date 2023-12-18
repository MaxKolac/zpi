using System.Net;
using ZPICommunicationModels.Models;
using ZPICommunicationModels.Messages;

namespace ZPIServer;

public static class Settings
{
    /// <summary>
    /// Tabela portów, na których <see cref="API.TcpReceiver"/> będzie nasłuchiwał przychodzących połączeń od kamer i użytkowników.<br/>
    /// Domyślne porty to 25565, 25566, 25567.
    /// </summary>
    public static int[] TcpReceiverPorts { get; set; } = new int[] { 25565, 25566, 25567 };

    /// <summary>
    /// Adres IP serwera w sieci lokalnej. Na tym adresie będą nasłuchiwane porty.<br/>
    /// Domyślna wartość to 127.0.0.1.
    /// </summary>
    public static IPAddress ServerAddress { get; set; } = IPAddress.Parse("127.0.0.1");

    /// <summary>
    /// Wskazuje, czy serwer może uruchamiać <see cref="API.CameraLibraries.PythonCameraSimulatorAPI"/>, a tym samym odbierać dane w formie plików RJPG.<br/>
    /// true oznacza, że <see cref="API.CameraLibraries.PythonCameraSimulatorAPI.CheckIfScriptsCanBeRun(Commands.Logger?)"/> nie zgłosił żadnych błędów.<br/>
    /// Domyślna wartość to false.
    /// </summary>
    public static bool CanPythonCameraAPIScriptsRun { get; set; } = false;

    /// <summary>
    /// Jeśli zdjęcie/wiadomość odebrane z kamery będzie posiadać tą lub wyższą wartość <see cref="CameraDataMessage.ImageVisibleDangerPercentage"/>, serwer zmieni wartość <see cref="HostDevice.LastFireStatus"/> na <see cref="HostDevice.FireStatus.Suspected"/>.<br/>
    /// Wartość domyślna do 5%.
    /// </summary>
    public static decimal ImagePercentageWarning { get; set; } = 0.05m;

    public static void ResetToDefault()
    {
        TcpReceiverPorts = new int[] { 25565, 25566, 25567 };
        ServerAddress = IPAddress.Parse("127.0.0.1");
        CanPythonCameraAPIScriptsRun = false;
        ImagePercentageWarning = 0.05m;
    }
}
