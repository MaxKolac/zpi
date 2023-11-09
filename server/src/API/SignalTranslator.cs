using ZPIServer.API.CameraLibraries;
using ZPIServer.EventArgs;
using ZPIServer.Models;

namespace ZPIServer.API;

/// <summary>
/// Klasa, która subskrybuje wydarzenie <see cref="TcpHandler.OnSignalReceived"/> i odpowiednio reaguje na jej inwokacje.
/// </summary>
public class SignalTranslator
{
    /// <summary>
    /// Wskazuje czy <see cref="SignalTranslator"/> został uruchomiony i obsługuje inwokacje wydarzenia <see cref="TcpHandler.OnSignalReceived"/>.
    /// </summary>
    public bool IsTranslating { get; private set; } = false;

    /// <summary>
    /// Rozpoczyna pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public void BeginTranslating()
    {
        if (IsTranslating)
            return;

        IsTranslating = true;
        TcpHandler.OnSignalReceived += HandleReceivedSignal;
        Console.WriteLine($"{nameof(SignalTranslator)} is starting up.");
    }

    /// <summary>
    /// Kończy pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public void StopTranslating()
    {
        if (!IsTranslating)
            return;

        Console.WriteLine($"Shutting down {nameof(SignalTranslator)}");
        TcpHandler.OnSignalReceived -= HandleReceivedSignal;
        IsTranslating = false;
    }

    void HandleReceivedSignal(object? sender, TcpHandlerEventArgs e)
    {
        //Query the DB for the proper record based on received IP
        var datasender = new HostDevice() 
        { 
            Address = e.SenderIp 
        };
        datasender ??= new HostDevice()
        {
            Address = e.SenderIp,
            Type = HostType.Unknown
        };

        Console.Write($"Received {e.Data.Length} bytes of data from {datasender.Type} device. Address = {e.SenderIp}:{e.SenderPort}. ");
        switch (datasender.Type)
        {
            case HostType.Unknown:
                Console.WriteLine("Ignoring...");
                break;
            case HostType.CameraSimulator:
                Console.WriteLine($"Forwarding to {nameof(CameraSimulatorAPI)}.");
                break;
            case HostType.User:
                Console.WriteLine($"User recognized: {datasender.Name}.");
                break;
        }
    }
}
