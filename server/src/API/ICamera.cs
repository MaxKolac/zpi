using ZPICommunicationModels;

namespace ZPIServer.API;

public interface ICamera
{
    /// <summary>
    /// Pobiera surowy ciąg bitów przekazany z <see cref="SignalTranslator"/> i konwertuje je na swój sposób na instancję <see cref="HostDevice"/>.<br/>
    /// Jeżeli konwersja się nie udała, ustawia instancję na <c>null</c>.
    /// </summary>
    /// <param name="bytes">Ciąg bitów z obsługiwanego urządzenia.</param>
    void DecodeReceivedBytes(byte[]? bytes);

    /// <summary>
    /// Zwraca wynik przekonwertowania ciągu bitów na instancję <see cref="HostDevice"/>. Jeżeli konwersja się nie udała, zwraca <c>null</c>.
    /// </summary>
    HostDevice? GetHostDevice();
}
