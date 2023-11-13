﻿using System.Net;
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
    private readonly Logger _logger;
    private readonly TcpListener _listener;
    private readonly Task _listenerTask;

    /// <summary>
    /// Wskazuje czy <see cref="TcpHandler"/> został uruchomiony i nasłuchuje przychodzących połączeń.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Wydarzenie, które jest inwokowane gdy <see cref="TcpHandler"/> otrzyma pełny ciąg bajtów z nasłuchiwanego portu.
    /// </summary>
    public static event EventHandler<TcpHandlerEventArgs>? OnSignalReceived;

    public TcpHandler(IPAddress address, int listenPort, Logger logger)
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
            _logger.WriteLine("Starting up.", nameof(TcpHandler));
            IsListening = true;
            _listener.Start();
            _listenerTask.Start();
        }
        catch (Exception ex)
        {
            StopListening();
            _logger.WriteLine(ex.ToString(), nameof(TcpHandler));
        }
    }

    /// <summary>
    /// Kończy nasłuch na zajętych portach.
    /// </summary>
    public void StopListening()
    {
        if (!IsListening)
            return;

        _logger.WriteLine("Shutting down.", nameof(TcpHandler));
        _token.Cancel();
        _listenerTask.Wait();
        _listener.Stop();
        IsListening = false;
    }

    private async Task HandleConnectionAsync()
    {
        _logger.WriteLine($"Ready to accept connection on port {((IPEndPoint)_listener.LocalEndpoint).Port}.", nameof(TcpHandler));
        try
        {
            using TcpClient incomingClient = await _listener.AcceptTcpClientAsync(_token.Token);
            IPEndPoint clientEndPoint = (IPEndPoint)incomingClient.Client.RemoteEndPoint!;
            IPAddress clientAddress = clientEndPoint.Address;
            int clientPort = clientEndPoint.Port;
            _logger.WriteLine($"Accepted connection from {clientAddress}:{clientPort}.", nameof(TcpHandler));

            using var stream = incomingClient.GetStream();
            int receivedBytesCount;
            byte[] buffer = new byte[2048];
            while ((receivedBytesCount = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                _logger.WriteLine($"Received {receivedBytesCount} bytes from {clientAddress}:{clientPort} on port {((IPEndPoint)_listener.LocalEndpoint).Port}.", nameof(TcpHandler));
                OnSignalReceived?.Invoke(this, new TcpHandlerEventArgs(clientAddress, clientPort, buffer));
            }
            _logger.WriteLine($"Closed the connection from {clientAddress}:{clientEndPoint.Port}.", nameof(TcpHandler));
        }
        catch (OperationCanceledException)
        {
            if (_token.IsCancellationRequested)
            {
                _logger.WriteLine($"Cancelling connection handling due to cancellation token.", nameof(TcpHandler));
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
                _logger.WriteLine($"Cancelling connection handling due to cancellation token.", nameof(TcpHandler));
                return;
            }
            else
            {
                throw;
            }
        }
    }
}
