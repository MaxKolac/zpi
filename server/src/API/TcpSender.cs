using System.Net;
using System.Net.Sockets;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer.API;

/// <summary>
/// Klasa odpowiedzialna za wysyłanie wiadomości z powrotem do hostów oraz obsługę ewentualnych niepowodzeń.<br/>
/// W przeciwieństwie do <see cref="TcpReceiver"/>, ta klasa ma dynamicznie zmieniające rozmiar listy obecnych <see cref="Task"/>ów oraz przypisanych im obiektów <see cref="TcpClient"/>. 
/// </summary>
public class TcpSender
{
    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;

    /// <summary>
    /// Semaphore used to reserve access to <see cref="_clients"/>, <see cref="_clientTasks"/>, <see cref="_connectionsInitialized"/> and <see cref="_connectionsHandled"/>.
    /// To avoid potential IndexOutOfArray or InvalidOperation in foreach loops, use the semaphore when accessing Lists' objects.
    /// </summary>
    private readonly SemaphoreSlim _accessSemaphore;
    private readonly List<TcpClient> _clients;
    private readonly List<Task> _clientTasks;
    private int _connectionsInitialized = 0;
    private int _connectionsHandled = 0;

    /// <summary>
    /// Wskazuje czy <see cref="TcpSender"/> został uruchomiony i może wysyłać wiadomości.
    /// </summary>
    public bool CanSendMessages { get; private set; } = false;

    public TcpSender(Logger? logger = null)
    {
        _token = new CancellationTokenSource();
        _accessSemaphore = new SemaphoreSlim(1, 1);
        _logger = logger;
        _clients = new();
        _clientTasks = new();

        Command.OnExecuted += ShowStatus;
    }

    ~TcpSender()
    {
        Command.OnExecuted -= ShowStatus;
        Disable();
    }

    public void Enable()
    {
        if (CanSendMessages)
            return;

        _logger?.WriteLine("Starting up.", nameof(TcpSender));
        CanSendMessages = true;

        //Subscribe to static events that will request data to be sent
    }

    /// <summary>
    /// Oczekuje na zakończenie działania wszystkich zadań wysyłania wiadomości oraz usuwa subskrybcje do statycznych wydarzeń innych klas.
    /// </summary>
    public void Disable()
    {
        if (!CanSendMessages)
            return;
        
        _accessSemaphore.Wait();
        CanSendMessages = false;

        _logger?.WriteLine("Shutting down.", nameof(TcpSender));
        _token.Cancel();
        foreach (var unstartedTask in _clientTasks)
        {
            //Starting unstarted tasks after signaling cancellation through a token, so that they will immediately run into a while() condition and finish execution without ever invoking HandleConnectionAsync()
            if (unstartedTask.Status == TaskStatus.Created)
                unstartedTask.Start();
        }
        Task.WaitAll(_clientTasks.ToArray());
        _accessSemaphore.Release();
    }

    /// <summary>
    /// Obsługuje żądania wysyłania sygnałów inwokowane przez inne klasy. Wywołuje metodę <see cref="SendMessageAsync"/> i reaguje na zwracaną przez nią wartość. W razie potrzeby, podejmuje ponowne próby wysłania wiadomości
    /// </summary>
    private void HandleSendingRequest(object sender, TcpSenderEventArgs e)
    {

    }

    /// <summary>
    /// Podejmuje próbę wysłania wiadomości na podanego hosta.
    /// </summary>
    /// <param name="recipientAddress">Adres hosta, na który wiadomość ma być wysłana.</param>
    /// <param name="recipientPort">Numer portu hosta, na który wiadomość ma być wysłana.</param>
    /// <returns><c>true</c> jeśli wiadomość została przesłana oraz połączenie bezpiecznie zakończone. Jeśli jedna z tych rzeczy nie nastąpiła, zwraca <c>false</c>.</returns>
    private async Task<bool> SendMessageAsync(IPAddress recipientAddress, int recipientPort)
    {

    }

    private void ShowStatus(object? sender, CommandEventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpSenderArgument)
        {
            _accessSemaphore.Wait();
            _logger?.WriteLine($"Running: {CanSendMessages}");
            _logger?.WriteLine($"Logging: {_logger is not null}");
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}");
            _logger?.WriteLine($"Connections fully handled: {_connectionsHandled}");
            _logger?.WriteLine($"Current clients ({_clients.Count}):", null);
            for (int i = 0; i < _clients.Count; i++)
            {
                _logger?.WriteLine($"\tAddress: {_clients[i].GetRemoteAddress()}");
                _logger?.WriteLine($"\tClient's Port: {_clients[i].GetRemotePort()}");
                _logger?.WriteLine($"\tServer's Port: {_clients[i].GetLocalPort()}");
                _logger?.WriteLine($"\tTask Status: {_clientTasks[i].Status}");
                _logger?.WriteLine("\t --- ");
            }
            _accessSemaphore.Release();
        }
    }
}
