using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using ZPICommunicationModels;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;

namespace ZPICameraSimulator;

public class Program
{
    private record class CameraDataWithoutImage(HostDevice.DeviceStatus Status, decimal LargestTemperature, decimal DangerPercentage);

    static void Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
            return;

        while (true)
        {
            //Choose which camera to simulate
            ConsoleKeyInfo selectedMode;
            do
            {
                Console.WriteLine("Jaką wiadomość wysłać: ");
                Console.WriteLine("1 - CameraDataMessage (symuluj kamerę zwykłą)");
                Console.WriteLine("2 - Zdjęcie RJPG (symuluj kamerę Python'ową)");
                selectedMode = Console.ReadKey();
                Console.WriteLine();
            }
            while (selectedMode.Key != ConsoleKey.D1 && selectedMode.Key != ConsoleKey.D2);

            //Building the message
            byte[] message = Array.Empty<byte>();
            if (selectedMode.Key == ConsoleKey.D1)
            {
                //Load the CameraDataWithoutImage JSON file
                var jsonFile = new CameraDataWithoutImage(HostDevice.DeviceStatus.OK, 123.456m, 0.123m);
                try
                {
                    using var reader = new StreamReader(File.OpenRead("message.json"));
                    string json = reader.ReadToEnd();
                    var deserializedMessage = JsonConvert.DeserializeObject<CameraDataWithoutImage>(json);

                    if (deserializedMessage is not null)
                        jsonFile = deserializedMessage;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Nie odnaleziono wiadomości tekstowej o nazwie 'message.json'.");
                    using var writer = File.CreateText("message.json");
                    writer.Write(JsonConvert.SerializeObject(jsonFile));
                    Console.WriteLine("Utworzono świeży szablon.");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine("Nie udało się deserializować wiadomości tekstowej o nazwie 'message.json'.");
                    using var writer = File.CreateText("message.json");
                    writer.Write(JsonConvert.SerializeObject(jsonFile));
                    Console.WriteLine("Utworzono świeży szablon.");
                }

                //Load the image as byte array
                byte[] loadedImage = Array.Empty<byte>();
                try
                {
                    loadedImage = HostDevice.ToByteArray(
                        Image.FromFile(Path.Combine(Environment.CurrentDirectory, "imageToSend.png")),
                        ImageFormat.Png);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Nie odnaleziono obrazu. Dodaj nowy obraz o nazwie 'imageToSend.png'.");
                }

                //Put the image and json together into a CameraDataMessage
                var cameraData = new CameraDataMessage()
                {
                    LargestTemperature = jsonFile.LargestTemperature,
                    Status = jsonFile.Status,
                    Image = loadedImage,
                    ImageVisibleDangerPercentage = jsonFile.DangerPercentage
                };
                message = ZPIEncoding.Encode(cameraData);

                Console.WriteLine("Dane, które symulator spróbuje wysłać na serwer:");
                Console.WriteLine($"Status: {cameraData.Status}");
                Console.WriteLine($"Obraz: {cameraData.Image.Length} bajtów");
                Console.WriteLine($"Największa temperatura: {cameraData.LargestTemperature}");
                Console.WriteLine($"Procent obrazu uznany za niebezpieczeństwo: {cameraData.ImageVisibleDangerPercentage}");
                Console.WriteLine();
            }
            else if (selectedMode.Key == ConsoleKey.D2)
            {
                message = OpenThermalImage() ?? Array.Empty<byte>();
                Console.WriteLine("Dane, które symulator spróbuje wysłać na serwer:");
                Console.WriteLine($"Zdjęcie termiczne: {message.Length} bajtów");
            }

            //Getting target IP - entering empty address will set it to loopback
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

            //Getting target port
            int port = 25565;
            conversionSuccessful = false;
            do
            {
                Console.Write("Podaj port serwera, na który wysłać wiadomość. Domyślnie, serwer nasłuchuje na portach 25565, 25566 i 25567. Wprowadzenie pustej wartości wyśle wiadomość na port 25565: ");
                try
                {
                    input = Console.ReadLine();
                    port = string.IsNullOrEmpty(input) ? 25565 : int.Parse(input);
                    conversionSuccessful = true;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Nieprawidłowy port.");
                }
            }
            while (!conversionSuccessful);

            //Attempt to send the message and log any exceptions
            try
            {
                Console.WriteLine($"Próba wysłania wiadomości do {address}:{port}...");
                if (message.Length != 0)
                {
                    Send(message, address, port);
                    Console.WriteLine("Wiadomość wysłana bez wyjątku!");
                }
                else
                {
                    Console.WriteLine("Coś poszło nie tak, wiadomość była pusta przed wysłaniem.");
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

    static void Send(byte[] bytes, IPAddress address, int port)
    {
        using var server = new TcpClient();
        server.Connect(address.MapToIPv4(), port);
        using (var stream = server.GetStream())
        {
            stream.Write(bytes);
        }
        server.Close();
    }

    static byte[]? OpenThermalImage()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "thermalImage.jpg");
        try
        {
            var imageAsBytes = new List<byte>();
            using (var reader = File.Open(path, FileMode.Open))
            {
                byte[] buffer = new byte[4096];
                while (reader.Read(buffer) != 0)
                {
                    imageAsBytes.AddRange(buffer);
                    buffer = new byte[4096];
                }
            }

            int emptyBytesPosition = 0;
            for (int i = imageAsBytes.Count - 1; i >= 0; i--)
            {
                if (imageAsBytes[i] != 0)
                {
                    emptyBytesPosition = i;
                    break;
                }
            }
            return imageAsBytes.ToArray()[..emptyBytesPosition];
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}
