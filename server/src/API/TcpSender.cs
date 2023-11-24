using System.Net;
using System.Net.Sockets;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer.API;

public class TcpSender
{
    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;
    //private readonly TcpListener[] _listeners;
    private readonly Task[] _listenerTasks;

    private readonly SemaphoreSlim _semaphore;
    private int _connectionsInitialized = 0;
    private int _connectionsHandled = 0;

    /// <summary>
    /// Wskazuje czy <see cref="TcpSender"/> został uruchomiony i może wysyłać wiadomości.
    /// </summary>
    public bool CanSendMessages { get; private set; } = false;

    public TcpSender(IPAddress address, int[] listenPorts, Logger? logger = null)
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
            if (Settings.TcpSenderPorts.Contains(port))
                throw new ArgumentException($"{nameof(listenPorts)} contained a port reserved for {nameof(TcpSender)}.");
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

    ~TcpSender()
    {
        Command.OnExecuted -= ShowStatus;
    }

    public void BeginListening()
    {
        if (CanSendMessages)
            return;

        _logger?.WriteLine("Starting up.", nameof(TcpSender));
        CanSendMessages = true;
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
                _logger?.WriteLine($"Could not start the TcpListener on port {_listeners[i].GetLocalPort()} - port already in use! {nameof(TcpSender)} won't be able to listen for connections on that port.", nameof(TcpSender), Logger.MessageType.Warning);
                inactiveListeners++;
            }
        }
        if (inactiveListeners == _listeners.Length)
        {
            throw new IOException($"All ports {nameof(TcpSender)} was registered on were occupied!");
        }
        else if (inactiveListeners > 0)
        {
            _logger?.WriteLine($"Failed to start TcpListener on {inactiveListeners} port(s).", nameof(TcpSender), Logger.MessageType.Warning);
        }
    }

    private void ShowStatus(object? sender, CommandEventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpSenderArgument)
        {
            _logger?.WriteLine($"Running: {CanSendMessages}");
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
