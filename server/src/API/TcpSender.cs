using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
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
    private record class LastClientInfo(IPAddress ClientAddress, int ClientPort, int? LocalPort, int MessageSize, bool WasSuccesful);

    /// <summary>
    /// Do testowania.
    /// </summary>
    public static class TestEvents
    {
        public static event EventHandler<TcpSenderEventArgs>? TestEvent1;

        public static void InvokeEvent1(object? sender, TcpSenderEventArgs e) => TestEvent1?.Invoke(sender, e);
    }

    private readonly CancellationTokenSource _token;
    private readonly Logger? _logger;
    private readonly Task _managerTask;

    /// <summary>
    /// Semafor dostępu do listy <see cref="_currentConnections"/>.
    /// </summary>
    private readonly SemaphoreSlim _tasksSemaphore;
    /// <summary>
    /// Typ generyczny listy: <see cref="Task"/>, który obsługuje połączenie, <see cref="TcpSenderEventArgs"/> zawierające dane o obsługiwanym hoście, <see cref="int"/> liczba podjętych prób.
    /// </summary>
    private readonly List<(Task<bool>, TcpSenderEventArgs, int)> _currentConnections;

    /// <summary>
    /// Semafor dostępu do właściwości statystycznych: <see cref="_connectionsInitialized"/>, <see cref="_connectionsSuccessfullyHandled"/> oraz <see cref="_lastClient"/>.
    /// </summary>
    private readonly SemaphoreSlim _statsSemaphore;
    private int _connectionsInitialized = 0;
    private int _connectionsSuccessfullyHandled = 0;
    private LastClientInfo? _lastClient;

    /// <summary>
    /// Wskazuje czy <see cref="TcpSender"/> został uruchomiony i może wysyłać wiadomości.
    /// </summary>
    public bool CanSendMessages { get; private set; } = false;
    /// <summary>
    /// Zwraca liczbę wszystkich połączeń, które <see cref="TcpSender"/> próbował obsłużyć. Wskazuje też na liczbę wszystkich żądań wysłania danych.
    /// </summary>
    public int ConnectionsInitialized
    {
        get
        {
            _statsSemaphore.Wait();
            int value = _connectionsInitialized;
            _statsSemaphore.Release();
            return value;
        }
    }
    /// <summary>
    /// Zwraca liczbę wszystkich połączeń, które <see cref="TcpSender"/> pomyślnie i w pełni obsłużył. Pomyślnie obsłużone połączenie to takie, przez które wysłany został cały ciąg bitów oraz połączenie zostało bezpiecznie zakończone.
    /// </summary>
    public int ConnectionsHandled
    {
        get
        {
            _statsSemaphore.Wait();
            int value = _connectionsSuccessfullyHandled;
            _statsSemaphore.Release();
            return value;
        }
    }

    public TcpSender(Logger? logger = null)
    {
        _token = new CancellationTokenSource();
        _tasksSemaphore = new SemaphoreSlim(1, 1);
        _statsSemaphore = new SemaphoreSlim(1, 1);
        _logger = logger;

        _currentConnections = new();
        _managerTask = new Task(() =>
        {
            while (!_token.IsCancellationRequested)
            {
                ManageCurrentTasks();
                Task.Delay(1000).Wait();
            }
        });

        Command.OnExecuted += ShowStatus;
    }

    ~TcpSender()
    {
        Command.OnExecuted -= ShowStatus;
        Disable();
    }

    /// <summary>
    /// Włącza obsługę żądań wysyłania wiadomości. Zarejestruj wydarzenia, które będą chciały wysłać dane metodą <see cref="RegisterEvent(EventHandler{TcpSenderEventArgs})"/>.
    /// </summary>
    public void Enable()
    {
        if (CanSendMessages)
            return;

        _logger?.WriteLine("Starting up.", nameof(TcpSender));
        _managerTask.Start();

        //Subscribe to static events here
        TestEvents.TestEvent1 += HandleSendingRequest;
        Command.OnExecuted += HandleSendingRequest;

        CanSendMessages = true;
    }

    /// <summary>
    /// Oczekuje na zakończenie działania wszystkich zadań wysyłania wiadomości oraz usuwa subskrybcje do wszystkich wydarzeń.
    /// </summary>
    public void Disable()
    {
        if (!CanSendMessages)
            return;

        CanSendMessages = false;
        _logger?.WriteLine("Shutting down.", nameof(TcpSender));


        //Unsubscribe from static events here
        TestEvents.TestEvent1 -= HandleSendingRequest;
        Command.OnExecuted -= HandleSendingRequest;

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
    private void HandleSendingRequest(object? sender, System.EventArgs e)
    {
        if (!CanSendMessages)
            return;

        if (e is not TcpSenderEventArgs args)
            return;

        if (sender is PingCommand command && command.FirstArg == PingCommand.IcmpArgument)
        {
            //Handling for PingCommand's `ping icmp`
            _logger?.WriteLine($"{sender?.GetType().Name} is requesting an ICMP ping to {command.SecondArg?.MapToIPv4()}.", nameof(TcpSender));
            Task.Run(() => SendICMPMessage(command.SecondArg!.MapToIPv4(), 4));
        }
        else
        {
            _logger?.WriteLine($"{sender?.GetType().Name} is requesting to send {args.Data.Length} byte(s) of data to {args.RecipientAddress}:{args.RecipientPort}.", nameof(TcpSender));
            var messageTask = new Task<bool>(() => SendMessageAsync(args.RecipientAddress, args.RecipientPort, args.Data).Result);

            _tasksSemaphore.Wait();
            _currentConnections.Add((messageTask, args, 0));
            messageTask.Start();
            _tasksSemaphore.Release();
        }
    }

    /// <summary>
    /// Wysyła wiadomości ping protokołem ICMP i wypisuje odpowiedź.
    /// </summary>
    /// <param name="address">Adres IP do zpingowania.</param>
    /// <param name="attempts">Liczba prób.</param>
    private async Task SendICMPMessage(IPAddress address, int attempts)
    {
        for (int i = 0; i < attempts && !_token.IsCancellationRequested; i++)
        {
            _logger?.WriteLine($"[{i + 1}/{attempts}] Pinging {address}...", nameof(TcpSender));
            try
            {
                var reply = await new Ping().SendPingAsync(address);
                _logger?.WriteLine($"[{i + 1}/{attempts}] Received a reply from {reply.Address} in {reply.RoundtripTime} ms - {reply.Status}", nameof(TcpSender), reply.Status == 0 ? Normal : Warning);
            }
            catch (Exception ex) when (ex is PingException || ex is SocketException)
            {
                _logger?.WriteLine($"[{i + 1}/{attempts}] Failed to send a ping message to {address} - {ex.Message}", nameof(TcpSender), Error);
            }
        }
    }

    /// <summary>
    /// Nadrzędne zadanie typu "menedżer". W równych odstępach czasu, sprawdza wszystkie obecnie obsługiwane połączenia.<br/>
    /// Jeśli jakiś <see cref="Task"/> zakończył działanie, usuwa go z listy.<br/>
    /// Jeśli jego działanie nie zakończyło się powodzeniem, ponawia próbę.
    /// </summary>
    private void ManageCurrentTasks()
    {
        //Search the current connections for tasks that are done and/or failed
        _tasksSemaphore.Wait();

        //If there's no tasks to manage, just skip this check
        if (_currentConnections.Count == 0)
        {
            _tasksSemaphore.Release();
            return;
        }

        int runningTasks = 0;
        int completedTasks = 0;
        int reattemptedTasks = 0;
        int faultedTasks = 0;
        for (int i = _currentConnections.Count - 1; i >= 0; i--)
        {
            //Look only for tasks that are completed
            if (!_currentConnections[i].Item1.IsCompleted)
            {
                runningTasks++;
                continue;
            }

            //If it returned true, the task has done its job - it can be removed
            if (_currentConnections[i].Item1.Result)
            {
                _currentConnections.RemoveAt(i);
                completedTasks++;
            }
            //Otherwise, something went wrong. Set up a new entry on the list and try to send data again.
            else if (!_currentConnections[i].Item1.Result && _currentConnections[i].Item3 < 3)
            {
                _currentConnections[i].Item1.Dispose();
                var argsCopy = _currentConnections[i].Item2;
                int previousAttemps = _currentConnections[i].Item3;

                Task<bool> newTask = new(() => SendMessageAsync(argsCopy.RecipientAddress, argsCopy.RecipientPort, argsCopy.Data).Result);
                _currentConnections[i] = new(newTask, argsCopy, previousAttemps + 1);
                newTask.Start();
                reattemptedTasks++;
            }
            //If this task failed 3 times already, just drop it.
            else
            {
                _logger?.WriteLine($"Aborting the send request to {_currentConnections[i].Item2.RecipientAddress}:{_currentConnections[i].Item2.RecipientPort}. Dropping {_currentConnections[i].Item2.Data.Length} bytes of data. Too many failed attempts.", nameof(TcpSender), Error);
                _currentConnections.RemoveAt(i);
                faultedTasks++;
            }
        }
        if (completedTasks > 0 || reattemptedTasks > 0 || faultedTasks > 0)
            _logger?.WriteLine($"Ran connections' task list routine. Running: {runningTasks}, Completed: {completedTasks}, Reattempted: {reattemptedTasks}, Faulted: {faultedTasks}.", nameof(TcpSender));
        _tasksSemaphore.Release();
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

        int? localServerPort = null;
        try
        {
            //Attempt to connect
            using var client = new TcpClient();
            _logger?.WriteLine($"Attempting to connect to {recipientAddress}:{recipientPort}...", nameof(TcpSender));
            await client.ConnectAsync(recipientAddress, recipientPort, _token.Token);
            _logger?.WriteLine($"Established connection to {recipientAddress}:{recipientPort}.", nameof(TcpSender));
            localServerPort = client.GetRemotePort();

            //Attempt to send data
            _logger?.WriteLine($"Attempting to send {message.Length} bytes of data...", nameof(TcpSender));
            using var netStream = client.GetStream();
            await netStream.WriteAsync(message, _token.Token);
            _logger?.WriteLine($"Sent {message.Length} bytes to {recipientAddress}:{recipientPort}.", nameof(TcpSender));

            //Note the success in the stats
            wasSuccesful = true;
            _statsSemaphore.Wait();
            _connectionsSuccessfullyHandled++;
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
        finally
        {
            _statsSemaphore.Wait();
            _lastClient = new(recipientAddress, recipientPort, localServerPort, message.Length, wasSuccesful);
            _statsSemaphore.Release();
        }

        return wasSuccesful;
    }

    private void ShowStatus(object? sender, System.EventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.TcpSenderArgument)
        {
            _statsSemaphore.Wait();
            _logger?.WriteLine($"Running: {CanSendMessages}");
            _logger?.WriteLine($"Logging: {_logger is not null}");
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}");
            _logger?.WriteLine($"Connections successfully handled: {_connectionsSuccessfullyHandled}");
            _logger?.WriteLine("Most recently handled request: ");
            _logger?.WriteLine($"\tClient: {(_lastClient is null ? "null" : $"{_lastClient.ClientAddress}:{_lastClient.ClientPort}")}");
            _logger?.WriteLine($"\tServer port: {(_lastClient is null ? "null" : _lastClient.LocalPort)}");
            _logger?.WriteLine($"\tMessage size: {(_lastClient is null ? "null" : _lastClient.MessageSize)} byte(s)");
            _logger?.WriteLine($"\tWas successful?: {(_lastClient is null ? "null" : _lastClient.WasSuccesful)}");
            _statsSemaphore.Release();
        }
    }

    /// <summary>
    /// Konwertuje podany tekst na ciąg bitów zakodowanych w systemie UTF8. Metoda przeciwna do <see cref="TcpReceiver.Decode(byte[])"/>.
    /// </summary>
    /// <param name="text">Tekst do zakodowania.</param>
    public static byte[] Encode(string text) => Encoding.UTF8.GetBytes(text);
}
