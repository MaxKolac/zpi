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
            }
            while (selectedMode.Key != ConsoleKey.D1 && selectedMode.Key != ConsoleKey.D2);

            //Building the message
            CameraDataMessage? jsonMessage = null;
            switch (selectedMode.Key)
            {
                case ConsoleKey.D1:
                    jsonMessage = new()
                    {
                        Status = HostDevice.DeviceStatus.OK,
                        Image = HostDevice.ToByteArray(Image.FromFile("imageToSend.png"), ImageFormat.Png) ?? Array.Empty<byte>(),
                        LargestTemperature = 123.456m,
                        ImageVisibleDangerPercentage = 0.68m
                    };

                    try
                    {
                        using var reader = new StreamReader(File.OpenRead("message.json"));
                        string json = reader.ReadToEnd();
                        var deserializedMessage = JsonConvert.DeserializeObject<CameraDataMessage>(json);

                        if (deserializedMessage is not null)
                            jsonMessage = deserializedMessage;
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("Nie odnaleziono wiadomości tekstowej o nazwie 'message.json'.");
                        using var writer = File.CreateText("message.json");
                        writer.Write(JsonConvert.SerializeObject(jsonMessage));
                        Console.WriteLine("Utworzono świeży szablon.");
                    }
                    catch (JsonSerializationException)
                    {
                        Console.WriteLine("Nie udało się deserializować wiadomości tekstowej o nazwie 'message.json'.");
                        using var writer = File.CreateText("message.json");
                        writer.Write(JsonConvert.SerializeObject(jsonMessage));
                        Console.WriteLine("Utworzono świeży szablon.");
                    }

                    Console.WriteLine("Dane, które symulator spróbuje wysłać na serwer:");
                    Console.WriteLine($"Status: {jsonMessage.Status}");
                    Console.WriteLine($"Obraz: {jsonMessage.Image.Length} bajtów");
                    Console.WriteLine($"Największa temperatura: {jsonMessage.LargestTemperature}");
                    Console.WriteLine($"Procent obrazu uznany za niebezpieczeństwo: {jsonMessage.ImageVisibleDangerPercentage}");
                    Console.WriteLine();
                    break;
                case ConsoleKey.D2:
                    break;
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
            int port;
            do
            {
                Console.Write("Podaj port serwera, na który wysłać wiadomość. Domyślnie, serwer nasłuchuje na portach 25565, 25566 i 25567: ");
                input = Console.ReadLine();
            }
            while (!int.TryParse(input, out port));

            //Attempt to send the message and log any exceptions
            try
            {
                Console.WriteLine($"Próba wysłania wiadomości do {address}:{port}...");
                byte[]? message = selectedMode.Key switch
                {
                    ConsoleKey.D1 => ZPIEncoding.Encode(jsonMessage),
                    ConsoleKey.D2 => OpenThermalImage(),
                    _ => null
                };
                if (message is not null)
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
        byte[]? result = null;
        try
        {
            using var reader = File.Open(path, FileMode.Open);
            reader.Read(result);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return result;
    }
}
