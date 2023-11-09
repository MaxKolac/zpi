using ZPIServer.EventArgs;

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

        try
        {
            IsTranslating = true;
            TcpHandler.OnSignalReceived += HandleReceivedSignal;

            //
        }
        catch 
        {

            TcpHandler.OnSignalReceived -= HandleReceivedSignal;
            IsTranslating = false;
        }
    }

    /// <summary>
    /// Kończy pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public void StopTranslating()
    {
        if (!IsTranslating)
            return;

        try
        {
            //

            TcpHandler.OnSignalReceived -= HandleReceivedSignal;
            IsTranslating = false;
        }
        catch
        {

            TcpHandler.OnSignalReceived += HandleReceivedSignal;
            IsTranslating = true;
        }

    }

    void HandleReceivedSignal(object? sender, TcpHandlerEventArgs e)
    {
        
    }
}
