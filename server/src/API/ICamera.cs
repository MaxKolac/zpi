using ZPICommunicationModels.Messages;

namespace ZPIServer.API;

public interface ICamera
{
    /// <summary>
    /// Pobiera surowy ciąg bitów przekazany z <see cref="SignalTranslator"/> i konwertuje je na swój sposób na odczytywalną wiadomość <see cref="CameraDataMessage"/>.<br/>
    /// Jeżeli konwersja się nie udała, ustawia instancję na <c>null</c>.
    /// </summary>
    /// <param name="bytes">Ciąg bitów z obsługiwanego urządzenia.</param>
    void DecodeReceivedBytes(byte[]? bytes);

    /// <summary>
    /// Zwraca wynik przekonwertowania ciągu bitów na wiadomość <see cref="CameraDataMessage"/>. Jeżeli konwersja się nie udała, zwraca <c>null</c>.
    /// </summary>
    CameraDataMessage? GetDecodedMessage();
}
