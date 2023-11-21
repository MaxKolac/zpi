using System.Drawing;
using System.Drawing.Imaging;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;

namespace ZPICameraSimulator;

public class Program
{
    static void Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
            return;

        while (true)
        {
            CameraDataMessage message = new()
            {
                LargestTemperature = -69.6969m,
                Image = HostDevice.ToByteArray(Image.FromFile("test.png"), ImageFormat.Png) ?? Array.Empty<byte>(),
                Status = HostDevice.DeviceStatus.OK
            };

            Console.WriteLine("Dane, które symulator spróbuje wysłać na serwer:");
            Console.WriteLine($"Największa temperatura: {message.LargestTemperature}");
            Console.WriteLine($"Obraz: {message.Image.Length} bajtów");
            Console.WriteLine($"Status: {message.Status}");
            Console.WriteLine();
            Console.WriteLine("1. Wyślij jako JSON.");
            var key = Console.ReadKey();

            try
            {
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        CameraSimulator.SendJson(message);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
            Console.WriteLine("------");
        }
    }
}
