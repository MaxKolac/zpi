using System.Drawing;
using System.Net;
using System.Text;

namespace ZPIServer.Models;

public class HostDevice
{
    /// <summary>
    /// Status urządzenia kamery. Numery zaczynające się od 100 oznaczają ostrzeżenia, a od 200 oznaczają błędy krytyczne. Uzytkownicy mają zawsze status 1.
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

    //Fields for both Users and Cameras
    public int Id { get; set; } //Primary key
    public required string Name { get; set; }
    public required HostType Type { get; set; }
    public required IPAddress Address { get; set; }

    //Camera specific fields
    #region Foreign Key
    public int? SectorId { get; set; }
    public Sector? Sector { get; set; }
    #endregion
    public DeviceStatus? LastKnownStatus { get; set; }
    public decimal LastKnownTemperature { get; set; }
    /// <summary>
    /// Wysokość geograficzna.
    /// </summary>
    public decimal LocationAltitude { get; set; }
    /// <summary>
    /// Szerokość geograficza.
    /// </summary>
    public decimal LocationLatitude { get; set; }
    public byte[]? LastImage { get; set; }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("( ");
        builder.Append(nameof(Id) + $": {Id} | ");
        builder.Append(nameof(Name) + $": {Name} | ");
        builder.Append(nameof(Type) + $": {Type} | ");
        builder.Append(nameof(Address) + $": {Address} | ");
        builder.Append(nameof(SectorId) + $": {SectorId} | ");
        builder.Append(nameof(LastKnownStatus) + $": {LastKnownStatus} | ");
        builder.Append(nameof(LastKnownTemperature) + $": {LastKnownTemperature} | ");
        builder.Append(nameof(LocationAltitude) + $": {LocationAltitude} | ");
        builder.Append(nameof(LocationLatitude) + $": {LocationLatitude} | ");
        builder.Append(nameof(LastImage) + $": {(LastImage is null ? 0 : LastImage.Length)} byte(s) ");
        builder.Append(')');
        return builder.ToString();
    }
}
