using System.Net;

namespace ZPIServer.Models;

public class HostDevice
{
    /// <summary>
    /// Status urządzenia kamery. Numery zaczynające się od 100 oznaczają ostrzeżenia, a od 200 oznaczają błędy krytyczne. Uzytkownicy mają zawsze status 0.
    /// </summary>
    public enum CameraStatus
    {
        /// <summary>
        /// Urządzenie jest w pełni sprawna.
        /// </summary>
        OK = 0,

        /// <summary>
        /// Urządzenie ostrzega o braku zasilania/niskim poziomie naładowania.
        /// </summary>
        LowPower = 100,

        /// <summary>
        /// Nie udało nawiązać się połączenia sieciowego z urządzeniem.
        /// </summary>
        Disconnected = 200,
        /// <summary>
        /// Urzadzenie jest widoczna z poziomu połączenia sieciowego, ale nie odpowiada na żadne żądania.
        /// </summary>
        Unresponsive = 201,
    }

    //TODO: Integrate into EF Core
    public int Id { get; set; }
    public string Name { get; set; }
    public HostType Type { get; set; }
    public IPAddress Address { get; set; }

    //HostDevice specific fields
    public int SectorId { get; set; }
    public CameraStatus LastKnownStatus { get; set; }
    public decimal LastHighestTemperature { get; set; }
    public string ExactLocation { get; set; }
}
