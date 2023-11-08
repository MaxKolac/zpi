using System.Net;

namespace ZPIServer.Models;

public class Camera
{
    /// <summary>
    /// Status urządzenia kamery. Numery zaczynające się od 100 oznaczają ostrzeżenia, a od 200 oznaczają błędy krytyczne.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Kamera jest w pełni sprawna.
        /// </summary>
        OK = 0,

        /// <summary>
        /// Kamera ostrzega o braku zasilania/niskim poziomie naładowania.
        /// </summary>
        LowPower = 100,

        /// <summary>
        /// Nie udało nawiązać się połączenia sieciowego z kamerą.
        /// </summary>
        Disconnected = 200,
        /// <summary>
        /// Kamera jest widoczna z poziomu połączenia sieciowego, ale nie odpowiada na żadne żądania.
        /// </summary>
        Unresponsive = 201,
    }

    //TODO: Integrate into EF Core
    public int Id { get; set; }
    public int SectorId { get; set; }
    public HostType Host { get; set; }
    public Status LastKnownStatus { get; set; }
    public required decimal LastHighestTemperature { get; set; }
    public required string Name { get; set; }
    public required IPAddress Address { get; set; }
    public required string ExactLocation { get; set; }
}
