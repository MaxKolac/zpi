namespace ZPICommunicationModels;

/// <summary>
/// Identyfikuje jakie dokładnie urządzenie kryje się za danym rekordem.
/// </summary>
public enum HostType
{
    /// <summary>
    /// HostDevice jest nieznany.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// HostDevice to symulator kamery stworzony do testów.
    /// </summary>
    CameraSimulator = 1,

    /// <summary>
    /// HostDevice to lokalny klient PuTTY.
    /// </summary>
    PuTTYClient = 2,

    /// <summary>
    /// HostDevice to jeden z użytkowników korzystający z aplikacji desktopowej.
    /// </summary>
    User = 3,
}
