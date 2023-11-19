namespace ZPIServer.Models;

/// <summary>
/// Jeden obszar organizacyjny obserwowany przez jedną kamerę termowizyjną.
/// </summary>
public class Sector
{
    /// <summary>
    /// Obecny stan pożarowy
    /// </summary>
    public enum FireStatus
    {
        /// <summary>
        /// Nie wykryto żadnego pożaru.
        /// </summary>
        OK = 0,

        /// <summary>
        /// Istnieje podejrzenie o możliwym pożarze.
        /// </summary>
        Suspected = 1,

        /// <summary>
        /// Istnienie pożaru potwierdzone.
        /// </summary>
        Confirmed = 2,
    }

    public int Id { get; set; } //Primary key
    public required string Name { get; set; }
    public required FireStatus LastStatus { get; set; }
    public string? Description { get; set; }

    #region Foreign Key
    public IList<HostDevice>? HostDevices { get; set; }
    #endregion
}
