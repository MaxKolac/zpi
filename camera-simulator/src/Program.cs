using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
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

            IPAddress address = IPAddress.Parse("127.0.0.1");
            bool conversionSuccessful = false;
            string? input;
            do
            {
                Console.WriteLine("Podaj adres IP serwera. Wprowadzenie pustej wartości wyśle wiadomość na 127.0.0.1:");
                try
                {
                    input = Console.ReadLine();
                    address = string.IsNullOrEmpty(input) ? IPAddress.Parse("127.0.0.1") : IPAddress.Parse(input);
                    conversionSuccessful = true;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Nieprawidłowy adres.");
                }
            }
            while (!conversionSuccessful);

            int port;
            do
            {
                Console.WriteLine("Podaj port serwera, na który wysłać wiadomość.");
                Console.WriteLine("Domyślnie, serwer nasłuchuje na portach 25565, 25566 i 25567.");
                input = Console.ReadLine();
            }
            while (!int.TryParse(input, out port));

            //Console.WriteLine("1. Wyślij jako JSON.");
            //var key = Console.ReadKey();
            var key = ConsoleKey.D1;

            Console.WriteLine($"Wiadomość będzie wysłana do {address}:{port}.");
            try
            {
                switch (key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("Próba wysłania wiadomości w formacie JSON...");
                        CameraSimulator.SendJson(message, address, port);
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
