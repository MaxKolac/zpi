using Newtonsoft.Json;
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
                LargestTemperature = 0m,
                Image = HostDevice.ToByteArray(Image.FromFile("imageToSend.png"), ImageFormat.Png) ?? Array.Empty<byte>(),
                Status = HostDevice.DeviceStatus.OK
            };

            try
            {
                using var reader = new StreamReader(File.OpenRead("message.json"));
                string json = reader.ReadToEnd();
                var deserializedMessage = JsonConvert.DeserializeObject<CameraDataMessage>(json);

                if (deserializedMessage is not null)
                    message = deserializedMessage;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Nie odnaleziono wiadomości tekstowej o nazwie 'message.json'.");
                using var writer = File.CreateText("message.json");
                writer.Write(JsonConvert.SerializeObject(message));
                Console.WriteLine("Utworzono świeży szablon.");
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine("Nie udało się deserializować wiadomości tekstowej o nazwie 'message.json'.");
                using var writer = File.CreateText("message.json");
                writer.Write(JsonConvert.SerializeObject(message));
                Console.WriteLine("Utworzono świeży szablon.");
            }

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
                Console.Write("Podaj adres IP serwera. Wprowadzenie pustej wartości wyśle wiadomość na 127.0.0.1: ");
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
                Console.Write("Podaj port serwera, na który wysłać wiadomość. Domyślnie, serwer nasłuchuje na portach 25565, 25566 i 25567: ");
                input = Console.ReadLine();
            }
            while (!int.TryParse(input, out port));

            try
            {
                Console.WriteLine($"Próba wysłania wiadomości w formacie JSON do {address}:{port}...");
                CameraSimulator.SendJson(message, address, port);
                Console.WriteLine("Wiadomość wysłana bez wyjątku!");
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
