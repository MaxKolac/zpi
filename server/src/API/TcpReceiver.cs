using System.Net;
using System.Net.Sockets;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer.API;

/// <summary>
/// Nasłuchuje na porty ustawione w <see cref="Settings.TcpListeningPorts"/> i przekazuje wszelkie odebrane dane jako surowe ciągi bitów. W przypadku odebrania takiego ciągu, inwokuje wydarznie <see cref="OnSignalReceived"/>. Nasłuch włącza metoda <see cref="Enable"/>, a kończy <see cref="Disable"/>.<br/>
/// <br/>
/// <see cref="TcpReceiver"/> pobiera zestaw portów do nasłuchiwania tylko w momencie gdy jest tworzony. Aby wszelkie zmiany w <see cref="Settings.TcpListeningPorts"/> były uwzględnione w już działającym <see cref="TcpReceiver"/>, wymagane jest utworzenie nowej instancji.
/// <para>
/// Użyteczne linki:
/// <see href="https://www.youtube.com/watch?v=TAGoid4u6PY">Mastering TCPListener in C#: Building Network Applications from Scratch</see>, 
/// <see href="https://www.youtube.com/watch?v=qtZTf1L5v0E"> Building a TCP Client in C#</see>
/// </para>
/// </summary>
public class TcpReceiver
{
    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;
    private readonly TcpListener[] _listeners;
    private readonly Task[] _listenerTasks;

    private readonly SemaphoreSlim _semaphore;
    private int _connectionsInitialized = 0;
    private int _connectionsHandled = 0;

    /// <summary>
    /// Wskazuje czy <see cref="TcpReceiver"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Zwraca liczbę instancji <see cref="TcpListener"/>, na których <see cref="TcpReceiver"/> nasłuchuje połączeń.
    /// </summary>
    public int ListenersCount => _listeners.Length;

    /// <summary>
    /// Zwraca liczbę <b>aktywnych</b> instancji <see cref="TcpListener"/>, na których <see cref="TcpReceiver"/> nasłuchuje połączeń.
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
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpReceiver"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpReceiverEventArgs>? OnSignalReceived;

    public TcpReceiver(IPAddress address, int listenPort, Logger? logger = null) :
        this(address, new int[] { listenPort }, logger)
    {
    }

    public TcpReceiver(IPAddress address, int[] listenPorts, Logger? logger = null)
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

    ~TcpReceiver()
    {
        Command.OnExecuted -= ShowStatus;
        Disable();
    }

    /// <summary>
    /// Rozpoczyna nasłuch na podanych wcześniej portach. Jeżeli jeden z portów okaże się być zajętym, <see cref="TcpReceiver"/> kontynuuje pracę bez nasłuchu na danym porcie i informuje o tym fakcie w <see cref="Logger"/>ze.<br/>
    /// Jeżeli wszystkie porty jakie zostały pobrane okażą się zajęte, rzuci wyjątkiem <see cref="IOException"/>.
    /// </summary>
    public void Enable()
    {
        if (IsListening)
            return;

        _logger?.WriteLine("Starting up.", nameof(TcpReceiver));
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
                _logger?.WriteLine($"Could not start the TcpListener on port {_listeners[i].GetLocalPort()} - port already in use! {nameof(TcpReceiver)} won't be able to listen for connections on that port.", nameof(TcpReceiver), Logger.MessageType.Warning);
                inactiveListeners++;
            }
        }
        if (inactiveListeners == _listeners.Length)
        {
            throw new IOException($"All ports {nameof(TcpReceiver)} was registered on were occupied!");
        }
        else if (inactiveListeners > 0)
        {
            _logger?.WriteLine($"Failed to start TcpListener on {inactiveListeners} port(s).", nameof(TcpReceiver), Logger.MessageType.Warning);
        }
    }

    /// <summary>
    /// Kończy nasłuch na zajętych portach.
    /// </summary>
    public void Disable()
    {
        if (!IsListening)
            return;

        _logger?.WriteLine("Shutting down.", nameof(TcpReceiver));
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

    /// <summary>
    /// Obsługuje połączenia przychodzące na jednym obiekcie <see cref="TcpListener"/>.
    /// </summary>
    private async Task HandleConnectionAsync(TcpListener listener)
    {
        _logger?.WriteLine($"Ready to accept connection on port {listener.GetLocalPort()}.", nameof(TcpReceiver));
        try
        {
            using TcpClient incomingClient = await listener.AcceptTcpClientAsync(_token.Token);
            IPEndPoint clientEndPoint = (IPEndPoint)incomingClient.Client.RemoteEndPoint!;
            IPAddress clientAddress = clientEndPoint.Address;
            int clientPort = clientEndPoint.Port;
            _logger?.WriteLine($"Accepted connection from {clientAddress}:{clientPort}.", nameof(TcpReceiver));

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
                fullMessage.AddRange(buffer);
                //Clear buffer so no duplicated bytes make it through
                buffer = new byte[bufferLength];
            }
            _logger?.WriteLine($"Closed the connection from {clientAddress}:{clientPort}.", nameof(TcpReceiver));

            //Trim excessive unfilled buffer bites AFTER receiving the full message
            //Note to self: You absolute cretin. You bumbling idiot. Fuck you. - @MaxKolac.
            int emptyBytesStartPosition = 0;
            for (int i = fullMessage.Count - 1; i >= 0; i--)
            {
                if (fullMessage[i] != 0)
                {
                    emptyBytesStartPosition = i;
                    break;
                }
            }

            //Log that shit
            _logger?.WriteLine($"Received {fullMessage.Count} bytes ({emptyBytesStartPosition + 1} after trimming) from {clientAddress}:{clientPort} on port {listener.GetLocalPort()}.", nameof(TcpReceiver));
            OnSignalReceived?.Invoke(this, new TcpReceiverEventArgs(clientAddress, clientPort, fullMessage.ToArray()[..(emptyBytesStartPosition + 1)]));

            await _semaphore.WaitAsync();
            _connectionsHandled++;
            _semaphore.Release();
        }
        catch (IOException ex)
        {
            //Usually thrown when the other end abruptly closes the connection
            _logger?.WriteLine($"IOException thrown on port {listener.GetLocalPort()}.", nameof(TcpReceiver), Logger.MessageType.Warning);
            _logger?.WriteLine($"{ex.Message}.", nameof(TcpReceiver), Logger.MessageType.Warning);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is SocketException)
        {
            if (!_token.IsCancellationRequested)
                throw;

            _logger?.WriteLine($"Cancelling connection handling on port {listener.GetLocalPort()} due to cancellation token.", nameof(TcpReceiver), Logger.MessageType.Warning);
            return;
        }
    }

    private void ShowStatus(object? sender, System.EventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpReceiverArgument)
        {
            _logger?.WriteLine($"Running: {IsListening}");
            _logger?.WriteLine($"Logging: {_logger is not null}");
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}");
            _logger?.WriteLine($"Connections fully handled: {_connectionsHandled}");
            _logger?.WriteLine($"Listeners:", null);
            for (int i = 0; i < _listeners.Length; i++)
            {
                _logger?.WriteLine($"\tAddress: {_listeners[i].GetLocalAddress()}");
                _logger?.WriteLine($"\tPort: {_listeners[i].GetLocalPort()}");
                _logger?.WriteLine($"\tIsActive: {_listeners[i].IsActive()}");
                _logger?.WriteLine($"\tTask Status: {_listenerTasks[i].Status}");
                _logger?.WriteLine("\t --- ");
            }
        }
    }
}