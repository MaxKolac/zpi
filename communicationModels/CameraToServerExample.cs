using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;

namespace ZPICommunicationModels;

internal class CameraToServerExample
{
    public static void HowToSendCameraDataMessageToServer()
    {
        //Tego wymagają metody manipulowania na klasie Image
        if (!OperatingSystem.IsWindows())
            return;

        //Połącz z serwerem - serwer nasłuchuje połączeń na portach 25565, 25566 i 25567.
        using var server = new TcpClient();
        server.Connect("127.0.0.1", 25566);

        //Stwórz nowy pakiet z danymi
        CameraDataMessage message = new()
        {
            LargestTemperature = -69.6969m,
            ImageVisibleDangerPercentage = 0.25m,
            Image = HostDevice.ToByteArray(Image.FromFile("ścieżkaBezSpacjiDoZdjęcia.png"), ImageFormat.Png)!,
            Status = HostDevice.DeviceStatus.OK
        };

        //Serializuj klasą ZPIEncoding i wyślij
        using (var stream = server.GetStream())
        {
            byte[] buffer = ZPIEncoding.Encode(message);
            stream.Write(buffer, 0, buffer.Length);
        }
        server.Close();
    }
}
