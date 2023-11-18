using System.Net;
using System.Net.Sockets;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer.API;

/// <summary>
/// Pierwsza warstwa komunikacji serwera z jego klientami, w tym kamerami i użytkownikami.<br/>
/// Nasłuchuje na porty ustawione w <see cref="Settings.TcpListeningPorts"/> i przekazuje wszelkie odebrane dane jako surowe ciągi bitów. W przypadku odebrania takiego ciągu, inwokuje wydarznie <see cref="OnSignalReceived"/>. Nasłuch włącza metoda <see cref="BeginListening"/>, a kończy <see cref="StopListening"/>.<br/>
/// <br/>
/// <see cref="TcpHandler"/> pobiera zestaw portów do nasłuchiwania tylko w momencie gdy rozpoczyna nasłuch. Aby wszelkie zmiany w <see cref="Settings.TcpListeningPorts"/> były uwzględnione w już działającym <see cref="TcpHandler"/>, należy go najpierw zatrzymać i uruchomić ponownie.
/// <para>
/// Użyteczne linki:
/// <see href="https://www.youtube.com/watch?v=TAGoid4u6PY">Mastering TCPListener in C#: Building Network Applications from Scratch</see>, 
/// <see href="https://www.youtube.com/watch?v=qtZTf1L5v0E"> Building a TCP Client in C#</see>
/// </para>
/// </summary>
public class TcpHandler
{
    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;
    private readonly TcpListener[] _listeners;
    private readonly Task[] _listenerTasks;

    private readonly SemaphoreSlim _semaphore;
    private int _connectionsInitialized = 0;
    private int _connectionsHandled = 0;

    /// <summary>
    /// Wskazuje czy <see cref="TcpHandler"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Zwraca liczbę instancji <see cref="TcpListener"/>, na których <see cref="TcpHandler"/> nasłuchuje połączeń.
    /// </summary>
    public int ListenersCount => _listeners.Length;

    /// <summary>
    /// Zwraca liczbę <b>aktywnych</b> instancji <see cref="TcpListener"/>, na których <see cref="TcpHandler"/> nasłuchuje połączeń.
    /// </summary>
    public int ActiveListenersCount()
    {
        int activeListeners = 0;
        foreach (var listener in _listeners)
        {
            if (listener.IsActive())
                activeListeners++;
        }
        return activeListeners;
    }

    /// <summary>
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpHandler"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpHandlerEventArgs>? OnSignalReceived;

    public TcpHandler(IPAddress address, int listenPort, Logger? logger = null) :
        this(address, new int[] { listenPort }, logger)
    {
    }

    public TcpHandler(IPAddress address, int[] listenPorts, Logger? logger = null)
    {
        if (listenPorts is null)
            throw new ArgumentException(null, nameof(listenPorts));
        if (listenPorts.Length == 0)
            throw new ArgumentException($"{nameof(listenPorts)} is empty.");
        if (listenPorts.Distinct().Count() != listenPorts.Length)
            throw new ArgumentException($"{nameof(listenPorts)} contained duplicate port numbers.");
        foreach (var port in listenPorts)
        {
            if (port < 1024 || 65535 < port)
                throw new ArgumentException($"{nameof(listenPorts)} contained an invalid TCP port number.");
        }

        _token = new CancellationTokenSource();
        _semaphore = new SemaphoreSlim(1, 1);
        _logger = logger;
        _listeners = new TcpListener[listenPorts.Length];
        _listenerTasks = new Task[listenPorts.Length];

        for (int i = 0; i < listenPorts.Length; i++)
        {
            int index = i;
            _listeners[index] = new TcpListener(address, listenPorts[index]);
            _listenerTasks[index] = new Task(async () =>
            {
                while (!_token.IsCancellationRequested)
                {
                    await HandleConnectionAsync(_listeners[index]);
                }
            });
        }

        Command.OnExecuted += ShowStatus;
    }

    ~TcpHandler()
    {
        Command.OnExecuted -= ShowStatus;
    }

    /// <summary>
    /// Pobiera obecną wartość <see cref="Settings.TcpListeningPorts"/> i rozpoczyna nasłuch na podanych portach. Jeżeli jeden z portów okaże się być zajętym, <see cref="TcpHandler"/> kontynuuje pracę bez nasłuchu na danym porcie i informuje o tym fakcie w <see cref="Logger"/>ze.<br/>
    /// Jeżeli wszystkie porty jakie zostały pobrane okażą się zajęte, wyjątek <see cref="IOException"/> jest rzucony.
    /// </summary>
    public void BeginListening()
    {
        if (IsListening)
            return;

        _logger?.WriteLine("Starting up.", nameof(TcpHandler));
        IsListening = true;
        int inactiveListeners = 0;
        for (int i = 0; i < _listeners.Length; i++)
        {
            try
            {
                _listeners[i].Start();
                _listenerTasks[i].Start();
            }
            catch (SocketException)
            {
                _logger?.WriteLine($"Could not start the TcpListener on port {_listeners[i].GetLocalPort()} - port already in use! {nameof(TcpHandler)} won't be able to listen for connections on that port.", nameof(TcpHandler));
                inactiveListeners++;
            }
        }
        if (inactiveListeners == _listeners.Length)
            throw new IOException($"All ports {nameof(TcpHandler)} was registered on were occupied!");
        else
            _logger?.WriteLine($"ALERT! Failed to start TcpListener on {inactiveListeners} port(s).", nameof(TcpHandler));
    }

    /// <summary>
    /// Kończy nasłuch na zajętych portach.
    /// </summary>
    public void StopListening()
    {
        if (!IsListening)
            return;

        _logger?.WriteLine("Shutting down.", nameof(TcpHandler));
        _token.Cancel();
        foreach (var unstartedTask in _listenerTasks)
        {
            //Starting unstarted tasks after signaling cancellation through a token, so that they will immediately run into a while() condition and finish execution without ever invoking HandleConnectionAsync()
            if (unstartedTask.Status == TaskStatus.Created)
                unstartedTask.Start();
        }
        Task.WaitAll(_listenerTasks);
        foreach (var listener in _listeners)
            listener.Stop();
        IsListening = false;
    }

    private async Task HandleConnectionAsync(TcpListener listener)
    {
        _logger?.WriteLine($"Ready to accept connection on port {listener.GetLocalPort()}.", nameof(TcpHandler));
        try
        {
            using TcpClient incomingClient = await listener.AcceptTcpClientAsync(_token.Token);
            IPEndPoint clientEndPoint = (IPEndPoint)incomingClient.Client.RemoteEndPoint!;
            IPAddress clientAddress = clientEndPoint.Address;
            int clientPort = clientEndPoint.Port;
            _logger?.WriteLine($"Accepted connection from {clientAddress}:{clientPort}.", nameof(TcpHandler));

            await _semaphore.WaitAsync();
            _connectionsInitialized++;
            _semaphore.Release();

            using var stream = incomingClient.GetStream();
            int receivedBytesCount;
            const int bufferLength = 2048;
            List<byte> fullMessage = new();
            byte[] buffer = new byte[bufferLength];

            //NewtorkStream.Read populates the buffer with received raw bytes and returns their amount
            //If that amount reaches 0, it means the connection was closed
            while ((receivedBytesCount = stream.Read(buffer, 0, bufferLength)) != 0)
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

                //Log that shit
                _logger?.WriteLine($"Received {sanitizedBuffer.Count} bytes from {clientAddress}:{clientPort} on port {listener.GetLocalPort()}.", nameof(TcpHandler));
            }
            _logger?.WriteLine($"Closed the connection from {clientAddress}:{clientPort}.", nameof(TcpHandler));
            OnSignalReceived?.Invoke(this, new TcpHandlerEventArgs(clientAddress, clientPort, fullMessage.ToArray()));

            await _semaphore.WaitAsync();
            _connectionsHandled++;
            _semaphore.Release();
        }
        catch (IOException ex)
        {
            //Usually thrown when the other end abruptly closes the connection
            _logger?.WriteLine($"IOException thrown on port {listener.GetLocalPort()}.", nameof(TcpHandler));
            _logger?.WriteLine($"{ex.Message}.", nameof(TcpHandler));
        }
        catch (OperationCanceledException)
        {
            if (_token.IsCancellationRequested)
            {
                _logger?.WriteLine($"Cancelling connection handling on port {listener.GetLocalPort()} due to cancellation token.", nameof(TcpHandler));
                return;
            }
            else
            {
                throw;
            }
        }
        catch (SocketException)
        {
            if (_token.IsCancellationRequested)
            {
                _logger?.WriteLine($"Cancelling connection handling on port {listener.GetLocalPort()} due to cancellation token.", nameof(TcpHandler));
                return;
            }
            else
            {
                throw;
            }
        }
    }

    private void ShowStatus(object? sender, CommandEventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpHandlerArgument)
        {
            _logger?.WriteLine($"Running: {IsListening}", null);
            _logger?.WriteLine($"Logging: {_logger is not null}", null);
            _logger?.WriteLine($"Listeners:", null);
            for (int i = 0; i < _listeners.Length; i++)
            {
                _logger?.WriteLine($"\tPort: {_listeners[i].GetLocalPort()}", null);
                _logger?.WriteLine($"\tIsActive: {_listeners[i].IsActive()}", null);
                _logger?.WriteLine($"\tTask Status: {_listenerTasks[i].Status}", null);
                _logger?.WriteLine("---", null);
            }
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}", null);
            _logger?.WriteLine($"Connections fully handled: {_connectionsHandled}", null);
        }
    }

}

public static class TcpListenerExtensions
{
    public static int GetLocalPort(this TcpListener listener)
    {
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    /// <summary>
    /// <see href="https://stackoverflow.com/a/59482929/21342746"/>
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    public static bool IsActive(this TcpListener listener)
    {
        return listener.Server.IsBound;
    }
}