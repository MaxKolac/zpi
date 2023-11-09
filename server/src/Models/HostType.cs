namespace ZPIServer.Models;

/// <summary>
/// Identyfikuje jak traktować sygnał z danego hosta.
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
    /// HostDevice to jeden z użytkowników korzystający z aplikacji desktopowej.
    /// </summary>
    User = 2,
}
