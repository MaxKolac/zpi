using System.Net;
using System.Net.Sockets;
using ZPIServer.Commands;
using ZPIServer.EventArgs;
using static ZPIServer.Commands.Logger.MessageType;

namespace ZPIServer.API;

/// <summary>
/// Klasa odpowiedzialna za wysyłanie wiadomości z powrotem do hostów oraz obsługę ewentualnych niepowodzeń.<br/>
/// W przeciwieństwie do <see cref="TcpReceiver"/>, ta klasa ma dynamicznie zmieniające rozmiar listy obecnych <see cref="Task"/>ów oraz przypisanych im obiektów <see cref="TcpClient"/>. 
/// </summary>
public class TcpSender
{
    private record class TcpSenderLastClientInfo(IPAddress ClientAddress, int ClientPort, int LocalPort, int MessageSize)
    {
        public bool WasSuccesful { get; set; } = false;
    }

    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;
    private readonly Task _managerTask;

    /// <summary>
    /// Semafor dostępu do listy <see cref="_currentConnections"/>.
    /// </summary>
    private readonly SemaphoreSlim _tasksSemaphore;
    private readonly List<(Task<bool>, TcpSenderEventArgs)> _currentConnections;

    /// <summary>
    /// Semafor dostępu do właściwości statystycznych: <see cref="_connectionsInitialized"/>, <see cref="_connectionsSuccessfullyHandled"/> oraz <see cref="_lastClient"/>.
    /// </summary>
    private readonly SemaphoreSlim _statsSemaphore;
    private int _connectionsInitialized = 0;
    private int _connectionsSuccessfullyHandled = 0;
    private TcpSenderLastClientInfo? _lastClient;

    /// <summary>
    /// Wskazuje czy <see cref="TcpSender"/> został uruchomiony i może wysyłać wiadomości.
    /// </summary>
    public bool CanSendMessages { get; private set; } = false;

    public TcpSender(Logger? logger = null)
    {
        _token = new CancellationTokenSource();
        _tasksSemaphore = new SemaphoreSlim(1, 1);
        _statsSemaphore = new SemaphoreSlim(1, 1);
        _logger = logger;
        _currentConnections = new();
        _managerTask = ManageCurrentTasksAsync();

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
        _managerTask.Start();
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

        CanSendMessages = false;
        _logger?.WriteLine("Shutting down.", nameof(TcpSender));

        //Unsubscribe from static events

        //Send cancellation signal to tasks. First wait for manager task.
        _token.Cancel();
        _managerTask.Wait();

        //Then, wait for all current connections to end.
        _tasksSemaphore.Wait();
        List<Task<bool>> tasksToWaitFor = new();
        foreach (var tuple in _currentConnections)
            tasksToWaitFor.Add(tuple.Item1);
        Task.WaitAll(tasksToWaitFor.ToArray());
        _tasksSemaphore.Release();
    }

    /// <summary>
    /// Reaguje na inwokowane żądania. Tworzy i uruchamia dla nich nowy <see cref="Task"/>.
    /// </summary>
    private void HandleSendingRequest(object sender, TcpSenderEventArgs e)
    {
        if (!CanSendMessages)
            return;

        _logger?.WriteLine($"{sender.GetType().Name} is requesting to send {e.Data.Length} byte(s) of data to {e.RecipientAddress}:{e.RecipientPort}.", nameof(TcpSender));
        Task<bool> messageTask = SendMessageAsync(e.RecipientAddress, e.RecipientPort, e.Data);

        _tasksSemaphore.Wait();
        _currentConnections.Add((messageTask, e));
        messageTask.Start();
        _tasksSemaphore.Release();
    }

    /// <summary>
    /// Nadrzędne zadanie typu "menedżer". W równych odstępach czasu, sprawdza wszystkie obecnie obsługiwane połączenia.<br/>
    /// Jeśli jakiś <see cref="Task"/> zakończył działanie, usuwa go z listy.<br/>
    /// Jeśli jego działanie nie zakończyło się powodzeniem, ponawia próbę.
    /// </summary>
    private async Task ManageCurrentTasksAsync()
    {
        while (_token.IsCancellationRequested)
        {
            //Search the current connections for tasks that are done and/or failed
            _tasksSemaphore.Wait();
            int completedTasks = 0;
            int faultedTasks = 0;
            for (int i = _currentConnections.Count - 1; i >= 0; i--)
            {
                //Look only for tasks that are completed
                if (_currentConnections[i].Item1.IsCompleted)
                {
                    //If it returned true, the task has done its job - it can be removed
                    if (_currentConnections[i].Item1.Result)
                    {
                        _currentConnections.RemoveAt(i);
                        completedTasks++;
                    }
                    else
                    {
                        //Otherwise, something went wrong. Set up a new entry on the list and try to send data again.
                        _currentConnections[i].Item1.Dispose();
                        var argsCopy = _currentConnections[i].Item2;

                        Task<bool> newTask = SendMessageAsync(argsCopy.RecipientAddress, argsCopy.RecipientPort, argsCopy.Data);
                        _currentConnections[i] = new(newTask, argsCopy);
                        newTask.Start();
                        faultedTasks++;
                    }
                }
            }
            _logger?.WriteLine($"Ran connections' task list routine. Completed: {completedTasks}, Faulted: {faultedTasks}", nameof(TcpSender));
            _tasksSemaphore.Release();

            //Wait a bit until next check
            await Task.Delay(1000);
        }
    }

    /// <summary>
    /// Podejmuje próbę wysłania wiadomości na podanego hosta.
    /// </summary>
    /// <param name="recipientAddress">Adres hosta, na który wiadomość ma być wysłana.</param>
    /// <param name="recipientPort">Numer portu hosta, na który wiadomość ma być wysłana.</param>
    /// <param name="message">Ciąg bitów do wysłania.</param>
    /// <returns><c>true</c> jeśli wiadomość została przesłana oraz połączenie bezpiecznie zakończone. Jeśli jedna z tych rzeczy nie nastąpiła, zwraca <c>false</c>.</returns>
    private async Task<bool> SendMessageAsync(IPAddress recipientAddress, int recipientPort, byte[] message)
    {
        bool wasSuccesful = false;
        _statsSemaphore.Wait();
        _connectionsInitialized++;
        _statsSemaphore.Release();

        try
        {
            //Attempt to connect
            using var client = new TcpClient();
            _logger?.WriteLine($"Attempting to connect to {recipientAddress}:{recipientPort}...", nameof(TcpSender));
            await client.ConnectAsync(recipientAddress, recipientPort, _token.Token);
            _logger?.WriteLine($"Established connection to {recipientAddress}:{recipientPort}.", nameof(TcpSender));

            //If succesful, note the connection as the last client
            _statsSemaphore.Wait();
            _lastClient = new(recipientAddress, recipientPort, client.GetRemotePort(), message.Length);
            _statsSemaphore.Release();

            //Attempt to send data
            _logger?.WriteLine($"Attempting to send {message.Length} bytes of data...", nameof(TcpSender));
            using var netStream = client.GetStream();
            await netStream.WriteAsync(message, _token.Token);
            _logger?.WriteLine($"Sent {message.Length} bytes to {recipientAddress}:{recipientPort}.", nameof(TcpSender));

            //Note the success in the stats
            wasSuccesful = true;
            _statsSemaphore.Wait();
            _connectionsSuccessfullyHandled++;
            _lastClient.WasSuccesful = true;
            _statsSemaphore.Release();
        }

        catch (Exception ex) when ((ex is OperationCanceledException || ex is SocketException) && _token.IsCancellationRequested)
        {
            _logger?.WriteLine($"Cancelling data send request to {recipientAddress}:{recipientPort} due to cancellation token.", nameof(TcpReceiver), Warning);
        }
        catch (SocketException ex)
        {
            _logger?.WriteLine($"Failed to connect to {recipientAddress}:{recipientPort}. {ex.Message}.", nameof(TcpSender), Warning);
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
        {
            _logger?.WriteLine($"Failed to send data over to {recipientAddress}:{recipientPort}. {ex.Message}.", nameof(TcpSender), Warning);
        }

        return wasSuccesful;
    }

    private void ShowStatus(object? sender, CommandEventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpSenderArgument)
        {
            _statsSemaphore.Wait();
            _logger?.WriteLine($"Running: {CanSendMessages}");
            _logger?.WriteLine($"Logging: {_logger is not null}");
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}");
            _logger?.WriteLine($"Connections successfully handled: {_connectionsSuccessfullyHandled}");
            _logger?.WriteLine("Most recently handled request: ");
            _logger?.WriteLine($"\tClient address: {(_lastClient is null ? "null" : _lastClient.ClientAddress)}");
            _logger?.WriteLine($"\tClient port: {(_lastClient is null ? "null" : _lastClient.ClientPort)}");
            _logger?.WriteLine($"\tServer port: {(_lastClient is null ? "null" : _lastClient.LocalPort)}");
            _logger?.WriteLine($"\tMessage size: {(_lastClient is null ? "null" : _lastClient.MessageSize)} byte(s)");
            _logger?.WriteLine($"\tWas successful?: {(_lastClient is null ? "null" : _lastClient.WasSuccesful)}");
            _statsSemaphore.Release();
        }
    }
}
