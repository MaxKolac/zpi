using System.Net;
using System.Net.Sockets;
using ZPICommunicationModels.Messages;

namespace ZPICommunicationModels;
public class ClientToServerExample
{
    public static void HowToSendUserRequests()
    {
        //Uruchom nasłuchiwanie np. używając ClientListener
        var listener = new ClientListener(IPAddress.Parse("127.0.0.1"), 12000);

        //Kiedy ClientListener otrzyma dane z zewnątrz, inwokuje OnSignalReceived
        //Podepnij pod to wydarzenie metodę, która je obsłuży
        listener.OnSignalReceived += (sender, e) =>
        {
            Console.WriteLine(ZPIEncoding.Decode<CameraDataMessage>(e));
        };

        //Stwórz nowe żądanie:
        //Poproś serwer o CameraDataMessage z kamery, która ma ID = 1
        var request = new UserRequest()
        {
            Request = UserRequest.RequestType.CameraDataAsJson,
            ModelObjectId = 1
        };

        //Wyślij żądanie
        using (var server = new TcpClient())
        {
            server.Connect("127.0.0.1", 25565);
            using (var stream = server.GetStream())
            {
                byte[] buffer = ZPIEncoding.Encode(request);
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}

/// <summary>
/// Uproszczona wersja klasy TcpReceiver z serwera. Kiedy otrzyma jakiś sygnał z zewntąrz na nasłuchiwany port, iwokuje <see cref="OnSignalReceived"/>.
/// </summary>
public class ClientListener
{
    private readonly TcpListener _listener;
    private readonly Task _listenerTask;
    private readonly CancellationTokenSource _token;

    public event EventHandler<byte[]>? OnSignalReceived;

    /// <summary>
    /// Tworzy i od razu uruchamia <see cref="ClientListener"/> na podanym adresie i porcie.
    /// </summary>
    public ClientListener(IPAddress address, int port)
    {
        _token = new CancellationTokenSource();
        _listener = new(address, port);
        _listener.Start();
        _listenerTask = Task.Run(async () =>
        {
            while (!_token.IsCancellationRequested)
            {
                await HandleConnectionAsync();
            }
        });
    }

    /// <summary>
    /// Wyłącza nasłuch.
    /// </summary>
    public void Disable()
    {
        _token.Cancel();
        _listenerTask.Wait();
        _listener.Stop();
    }

    private async Task HandleConnectionAsync()
    {
        using TcpClient incomingClient = await _listener.AcceptTcpClientAsync();
        using var stream = incomingClient.GetStream();
        const int bufferLength = 2048;
        List<byte> fullMessage = new();
        byte[] buffer = new byte[bufferLength];

        //NewtorkStream.Read populates the buffer with received raw bytes and returns their amount
        //If that amount reaches 0, it means the connection was safely closed
        while (stream.Read(buffer, 0, bufferLength) != 0)
        {
            //Trim excessive unfilled buffer bites
            int i = bufferLength - 1;
            List<byte> sanitizedBuffer = buffer.ToList();
            while (i >= 0 && buffer[i] == 0)
            {
                sanitizedBuffer.RemoveAt(i);
                i--;
            }

            //Add trimmed buffer to the full message
            fullMessage.AddRange(sanitizedBuffer);

            //Clear buffer so no duplicated bytes make it through
            buffer = new byte[bufferLength];
        }

        OnSignalReceived?.Invoke(this, fullMessage.ToArray());
    }
}
