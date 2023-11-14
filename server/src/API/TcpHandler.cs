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
    private readonly TcpListener _listener;
    private readonly Task _listenerTask;
    private int _connectionsInitialized = 0;
    private int _connectionsHandled = 0;

    /// <summary>
    /// Wskazuje czy <see cref="TcpHandler"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpHandler"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpHandlerEventArgs>? OnSignalReceived;

    public TcpHandler(IPAddress address, int listenPort, Logger? logger = null)
    {
        _token = new CancellationTokenSource();
        _logger = logger;
        _listener = new TcpListener(address, listenPort);
        _listenerTask = new Task(async () =>
        {
            while (!_token.IsCancellationRequested)
            {
                await HandleConnectionAsync();
            }
        });
        Command.OnExecuted += ShowStatus;
    }

    ~TcpHandler()
    {
        Command.OnExecuted -= ShowStatus;
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
            _logger?.WriteLine("Starting up.", nameof(TcpHandler));
            IsListening = true;
            _listener.Start();
            _listenerTask.Start();
        }
        catch (Exception ex)
        {
            StopListening();
            _logger?.WriteLine(ex.ToString(), nameof(TcpHandler));
        }
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
        _listenerTask.Wait();
        _listener.Stop();
        IsListening = false;
    }

    private async Task HandleConnectionAsync()
    {
        _logger?.WriteLine($"Ready to accept connection on port {((IPEndPoint)_listener.LocalEndpoint).Port}.", nameof(TcpHandler));
        try
        {
            using TcpClient incomingClient = await _listener.AcceptTcpClientAsync(_token.Token);
            IPEndPoint clientEndPoint = (IPEndPoint)incomingClient.Client.RemoteEndPoint!;
            IPAddress clientAddress = clientEndPoint.Address;
            int clientPort = clientEndPoint.Port;
            _logger?.WriteLine($"Accepted connection from {clientAddress}:{clientPort}.", nameof(TcpHandler));
            _connectionsInitialized++;

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
                _logger?.WriteLine($"Received {sanitizedBuffer.Count} bytes from {clientAddress}:{clientPort} on port {((IPEndPoint)_listener.LocalEndpoint).Port}.", nameof(TcpHandler));
            }
            _logger?.WriteLine($"Closed the connection from {clientAddress}:{clientEndPoint.Port}.", nameof(TcpHandler));
            OnSignalReceived?.Invoke(this, new TcpHandlerEventArgs(clientAddress, clientPort, fullMessage.ToArray()));
            _connectionsHandled++;
        }
        catch (IOException ex)
        {
            //Usually thrown when the other end abruptly closes the connection
            _logger?.WriteLine($"{ex.Message}.", nameof(TcpHandler));
        }
        catch (OperationCanceledException)
        {
            if (_token.IsCancellationRequested)
            {
                _logger?.WriteLine($"Cancelling connection handling due to cancellation token.", nameof(TcpHandler));
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
                _logger?.WriteLine($"Cancelling connection handling due to cancellation token.", nameof(TcpHandler));
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
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.SignalTranslatorArgument)
        {
            var endPoint = (IPEndPoint)_listener.LocalEndpoint;
            _logger?.WriteLine($"Running: {IsListening}", null);
            _logger?.WriteLine($"Logging: {_logger is not null}", null);
            _logger?.WriteLine($"Listening on: {endPoint.Address}", null);
            _logger?.WriteLine($"\t{endPoint.Port}", null);
            _logger?.WriteLine($"ListenerTask: {_listenerTask.Status}", null);
            _logger?.WriteLine($"Connections initialized: {_connectionsInitialized}", null);
            _logger?.WriteLine($"Connections fully handled: {_connectionsHandled}", null);
        }
    }
}
