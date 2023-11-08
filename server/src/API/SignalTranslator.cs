using ZPIServer.EventArgs;

namespace ZPIServer.API;

/// <summary>
/// Klasa, która subskrybuje wydarzenie <see cref="TcpHandler.OnSignalReceived"/> i odpowiednio reaguje na jej inwokacje.
/// </summary>
public static class SignalTranslator
{
    /// <summary>
    /// Wskazuje czy <see cref="SignalTranslator"/> został uruchomiony i obsługuje inwokacje wydarzenia <see cref="TcpHandler.OnSignalReceived"/>.
    /// </summary>
    public static bool IsTranslating { get; private set; } = false;

    /// <summary>
    /// Rozpoczyna pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public static void BeginTranslating()
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
    public static void StopTranslating()
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

    static void HandleReceivedSignal(object? sender, TcpListenerEventArgs e)
    {
        
    }
}
