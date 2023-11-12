using System.Net;

namespace ZPIServer.Models;

public class HostDevice
{
    /// <summary>
    /// Status urządzenia kamery. Numery zaczynające się od 100 oznaczają ostrzeżenia, a od 200 oznaczają błędy krytyczne. Uzytkownicy mają zawsze status 0.
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// Obecny stan urządzenia jest nieznany lub urządzenia nie rozpoznano
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Urządzenie jest w pełni sprawne.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Urządzenie ostrzega o braku zasilania/niskim poziomie naładowania.
        /// </summary>
        LowPower = 100,

        /// <summary>
        /// Nie udało nawiązać się połączenia sieciowego z urządzeniem.
        /// </summary>
        Disconnected = 200,
        /// <summary>
        /// Urzadzenie jest widoczne z poziomu połączenia sieciowego, ale nie odpowiedziało na ostatnie żądanie.
        /// </summary>
        Unresponsive = 201,
    }

    //TODO: Integrate into EF Core
    //Fields for both Users and Cameras
    public int Id { get; set; }
    public string Name { get; set; }
    public HostType Type { get; set; }
    public IPAddress Address { get; set; }

    //Camera specific fields
    public int SectorId { get; set; }
    public DeviceStatus LastKnownStatus { get; set; }
    public decimal LastHighestTemperature { get; set; }
    public string ExactLocation { get; set; }
}
