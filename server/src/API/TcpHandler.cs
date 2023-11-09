using System.Net;
using System.Net.Sockets;
using ZPIServer.EventArgs;

namespace ZPIServer.API;

/// <summary>
/// Pierwsza warstwa komunikacji serwera z jego klientami, w tym kamerami i użytkownikami.<br/>
/// Nasłuchuje na porty ustawione w <see cref="Settings.TcpListeningPorts"/> i przekazuje wszelkie odebrane dane jako surowe ciągi bitów.<br/>
/// W przypadku odebrania takiego ciągu, inwokuje wydarznie <see cref="OnSignalReceived"/>. Nasłuch włącza metoda <see cref="BeginListening"/>, a kończy <see cref="StopListening"/>.<br/>
/// <br/>
/// <see cref="TcpHandler"/> pobiera zestaw portów do nasłuchiwania tylko w momencie gdy rozpoczyna nasłuch. Aby wszelkie zmiany w <see cref="Settings.TcpListeningPorts"/> były uwzględnione w już działającym <see cref="TcpHandler"/>, należy go najpierw zatrzymać i uruchomić ponownie.
/// </summary>
public class TcpHandler
{
    private readonly CancellationTokenSource cancellationToken = new();
    private Task? listeningTask;
    private readonly IPAddress address;
    private readonly int port;

    /// <summary>
    /// Wskazuje czy <see cref="TcpHandler"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpHandler"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpHandlerEventArgs> OnSignalReceived;

    public TcpHandler(IPAddress listenAddress, int listenPort)
    {
        address = listenAddress; 
        port = listenPort;
    }

    /// <summary>
    /// Pobiera obecną wartość <see cref="Settings.TcpListeningPorts"/> i rozpoczyna nasłuch na podanych portach.
    /// </summary>
    public void BeginListening()
    {
        if (IsListening)
            return;

        try
        {
            Console.WriteLine($"{nameof(TcpHandler)} is starting up.");
            IsListening = true;
            listeningTask = new Task(async () =>
            {
                await WaitForConnectionAsync(new TcpListener(address, port), cancellationToken.Token);
            });
            listeningTask.Start();
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
    public void StopListening()
    {
        if (!IsListening)
            return;

        Console.WriteLine($"Shutting down {nameof(TcpHandler)}.");
        if (listeningTask is not null)
        {
            cancellationToken.Cancel();
            listeningTask.Wait();
            listeningTask = null;
        }
        IsListening = false;
    }

    async Task WaitForConnectionAsync(TcpListener listener, CancellationToken token)
    {
        listener.Start();
        while (true)
        {
            try
            {
                Console.WriteLine($"Listener {listener.LocalEndpoint} ready to accept connections.");
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                await HandleIncomingConnectionAsync(client, token);
                client.Dispose();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Listener {listener.LocalEndpoint} requested to close by cancellation token.");
                listener.Stop();
                return;
            }
        }
    }

    async Task HandleIncomingConnectionAsync(TcpClient client, CancellationToken token)
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
                if (token.IsCancellationRequested || bytesCount == 0) 
                    break;
                Console.WriteLine($"Received {bytesCount} bytes from {clientAddress}:{clientEndPoint.Port}.");
                OnSignalReceived?.Invoke(null, new TcpHandlerEventArgs(clientAddress, clientEndPoint.Port, bytes));
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
