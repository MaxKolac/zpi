using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;

namespace ZPICameraSimulator;

internal class CameraSimulator
{
    public static void SendJson(CameraDataMessage message)
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var server = new TcpClient();
        server.Connect("127.0.0.1", 25566);

        string json = JsonConvert.SerializeObject(message);
        using (var stream = server.GetStream())
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            stream.Write(buffer, 0, buffer.Length);
        }
        server.Close();
    }
}
