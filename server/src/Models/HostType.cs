namespace ZPIServer.Models;

/// <summary>
/// Identyfikuje jak traktować sygnał z danego hosta.
/// </summary>
public enum HostType
{
    /// <summary>
    /// Host jest nieznany.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Host to symulator kamery stworzony do testów.
    /// </summary>
    CameraSimulator = 1,

    /// <summary>
    /// Host to jeden z użytkowników korzystający z aplikacji desktopowej.
    /// </summary>
    User = 2,
}
