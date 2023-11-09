using System.Net;
using System.Net.Sockets;
using ZPIServer.EventArgs;
using System;

namespace ZPIServer.API;

/// <summary>
/// Pierwsza warstwa komunikacji serwera z jego klientami, w tym kamerami i użytkownikami.<br/>
/// Nasłuchuje na porty ustawione w <see cref="Settings.TcpListeningPorts"/> i przekazuje wszelkie odebrane dane jako surowe ciągi bitów.<br/>
/// W przypadku odebrania takiego ciągu, inwokuje wydarznie <see cref="OnSignalReceived"/>. Nasłuch włącza metoda <see cref="BeginListening"/>, a kończy <see cref="StopListening"/>.<br/>
/// <br/>
/// <see cref="TcpHandler"/> pobiera zestaw portów do nasłuchiwania tylko w momencie gdy rozpoczyna nasłuch. Aby wszelkie zmiany w <see cref="Settings.TcpListeningPorts"/> były uwzględnione w już działającym <see cref="TcpHandler"/>, należy go najpierw zatrzymać i uruchomić ponownie.
/// </summary>
public static class TcpHandler
{
    static Task[]? listeningTasks;
    static readonly CancellationTokenSource cancellationToken = new();

    /// <summary>
    /// Wskazuje czy <see cref="TcpHandler"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public static bool IsListening { get; private set; } = false;

    /// <summary>
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpHandler"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpHandlerEventArgs> OnSignalReceived;

    /// <summary>
    /// Pobiera obecną wartość <see cref="Settings.TcpListeningPorts"/> i rozpoczyna nasłuch na podanych portach.
    /// </summary>
    public static async Task BeginListening()
    {
        if (IsListening)
            return;

        try
        {
            Console.WriteLine("TcpHandler is starting up.");
            IsListening = true;

            //Load current Settings.TcpListeningPorts
            await Settings.SettingsAccess.WaitAsync();
            {
                listeningTasks = new Task[Settings.TcpListeningPorts.Length];
                for (int i = 0; i < listeningTasks.Length; i++)
                {
                    int port = Settings.TcpListeningPorts[i];
                    IPAddress address = new(Settings.ServerAddress.GetAddressBytes());
                    listeningTasks[i] = new Task(async () =>
                    {
                        await WaitForConnectionAsync(new TcpListener(address, port), cancellationToken.Token);
                    });
                    Console.WriteLine($"Started a new TcpListener on {address}:{port}");
                }
            }
            Settings.SettingsAccess.Release();

            //Start listening
            foreach (var task in listeningTasks)
                task.Start();
        }
        catch (Exception ex)
        {
            StopListening();
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Kończy nasłuch na wszystkich portach i je zwalnia.
    /// </summary>
    public static void StopListening()
    {
        if (!IsListening)
            return;

        Console.WriteLine("Shutting down TcpHandler.");
        if (listeningTasks is not null)
        {
            cancellationToken.Cancel();
            Task.WaitAll(listeningTasks);
            listeningTasks = null;
        }
        IsListening = false;
    }

    static async Task WaitForConnectionAsync(TcpListener listener, CancellationToken token)
    {
        listener.Start();
        while (true)
        {
            try
            {
                Console.WriteLine($"Listener {listener.LocalEndpoint} ready to accept connections.");
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                await HandleIncomingConnectionAsync(client, token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Listener {listener.LocalEndpoint} requested to close by cancellation token.");
                listener.Stop();
                return;
            }
        }
    }

    static async Task HandleIncomingConnectionAsync(TcpClient client, CancellationToken token)
    {
        IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
        IPAddress clientAddress = clientEndPoint.Address;
        Console.WriteLine($"Accepted connection from {clientAddress}:{clientEndPoint.Port}.");

        using (NetworkStream stream = client.GetStream())
        {
            byte[] bytes = new byte[4096];
            while (true)
            {
                int bytesCount = await stream.ReadAsync(bytes, token);
                if (token.IsCancellationRequested || bytesCount == 0) //TODO: is 2nd condition the right one to use??
                    break;
                Console.WriteLine($"Received {bytesCount} bytes from {clientAddress}:{clientEndPoint.Port}.");
                OnSignalReceived?.Invoke(null, new TcpListenerEventArgs(clientAddress, clientEndPoint.Port, bytes));
            }
        }
        client.Close();
        if (token.IsCancellationRequested)
        {
            Console.WriteLine($"Closed the connection from {clientAddress}:{clientEndPoint.Port} due to cancellation token.");
            throw new TaskCanceledException();
        }
        else
        {
            Console.WriteLine($"Closed the connection from {clientAddress}:{clientEndPoint.Port}.");
        }
    }
}
