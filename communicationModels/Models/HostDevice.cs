using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using ZPICommunicationModels.JsonConverters;

namespace ZPICommunicationModels.Models;

/// <summary>
/// Urządzenie sieciowe. Może to być kamera, użytkownik lub coś innego. Lista urządzeń znajduje się w <see cref="HostType"/>.
/// </summary>
public class HostDevice
{
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
        /// <summary>
        /// Nie udało się odczytać ostatniej wiadomości odebranej z urządzenia.
        /// </summary>
        DataCorrupted = 202
    }

    //Fields for both Users and Cameras
    /// <summary>
    /// Klucz podstawowy. Lepiej tego nie edytować manualnie.
    /// </summary>
    public int Id { get; set; } //Primary key
    /// <summary>
    /// Nazwa urządzenia.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Wskazuje czym jest to urządzenie.
    /// </summary>
    public required HostType Type { get; set; }
    /// <summary>
    /// Adres IP urządzenia.
    /// </summary>
    [JsonConverter(typeof(IPAddressConverter))]
    public required IPAddress Address { get; set; }

    //Camera specific fields
    /// <summary>
    /// Właściwość tylko dla kamer. <b>Klucz obcy</b><br/>
    /// ID rekordu powiązanej instancji <see cref="Sector"/>.
    /// </summary>
    public int? SectorId { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer. <b>Klucz obcy</b><br/>
    /// Właściwość dostępowa do sektora powiązanego z tym urządzeniem.
    /// </summary>
    [JsonIgnore]
    public Sector? Sector { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer. <b>Tą właściwość serwer będzie próbował odczytać.</b><br/>
    /// Ostatni znany status kamery.
    /// </summary>
    public DeviceStatus? LastKnownStatus { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer. <b>Tą właściwość serwer będzie próbował odczytać.</b><br/>
    /// Ostatnia znana najwyższa wykryta temperatura.
    /// </summary>
    public decimal LastKnownTemperature { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer.<br/>
    /// Wysokość geograficzna.
    /// </summary>
    public decimal LocationAltitude { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer.<br/>
    /// Szerokość geograficza.
    /// </summary>
    public decimal LocationLatitude { get; set; }
    /// <summary>
    /// Właściwość tylko dla kamer. <b>Tą właściwość serwer będzie próbował odczytać.</b><br/>
    /// EF Core nie może przechowywać <see cref="Image"/> jako kolumny. Użyj <see cref="ToImage(byte[]?)"/> i <see cref="ToByteArray(Image?, ImageFormat)"/> aby konwertować obraz na ciąg bitów i vice versa.
    /// </summary>
    public byte[]? LastImage { get; set; }

    public static Image? ToImage(byte[]? bytes)
    {
        if (!OperatingSystem.IsWindows()) //supresses CA1416
            return null;

        return bytes is null ? null : Image.FromStream(new MemoryStream(bytes));
    }

    public static byte[]? ToByteArray(Image? bitmap, ImageFormat format)
    {
        if (!OperatingSystem.IsWindows()) //supresses CA1416
            return null;

        if (bitmap is null)
            return null;
        using var stream = new MemoryStream();
        bitmap.Save(stream, format);
        return stream.ToArray();
    }

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
