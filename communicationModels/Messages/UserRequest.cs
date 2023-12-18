using ZPICommunicationModels.Models;

namespace ZPICommunicationModels.Messages;

/// <summary>
/// Wiadomość, którą front-end wysyła na serwer z żądaniem o odesłanie specyficznej wiadomości, np. wszystkich informacji o danej kamerze lub tylko jej ostatni stan jako <see cref="CameraDataMessage"/>.
/// </summary>
public class UserRequest
{
    public enum RequestType
    {
        /// <summary>
        /// Klient prosi o zestaw danych z danej kamery w postaci obiektu <see cref="CameraDataMessage"/> zserializowanej w formacie Json.<br/>
        /// W przypadku tego typu, serwer przeszuka swoją bazę danych w poszukiwaniu <see cref="HostDevice"/> z wartością ID podaną w <see cref="ModelObjectId"/>.
        /// </summary>
        CameraDataAsJson,
        /// <summary>
        /// Klient prosi o zestaw wszystkich danych obiektu klasy <see cref="HostDevice"/> zserializowanej w formacie Json.<br/>
        /// W przypadku tego typu, serwer przeszuka swoją bazę danych w poszukiwaniu <see cref="HostDevice"/> z wartością ID podaną w <see cref="ModelObjectId"/>.
        /// </summary>
        SingleHostDeviceAsJson,
        /// <summary>
        /// Klient prosi o listę wszystkich rekordów klasy <see cref="HostDevice"/> jakie serwer ma w swojej bazie danych.
        /// Serwer wyśle odpowiedź w formie listy List&lt;<see cref="HostDevice"/>&gt; zserializowanej w formacie Json.
        /// </summary>
        AllHostDevicesAsJson,
        /// <summary>
        /// Klient prosi o zestaw wszystkich danych obiektu klasy <see cref="Sector"/>  zserializowanej w formacie Json.<br/>
        /// W przypadku tego typu, serwer przeszuka swoją bazę danych w poszukiwaniu <see cref="Sector"/> z wartością ID podaną w <see cref="ModelObjectId"/>.
        /// </summary>
        SingleSectorAsJson,
        /// <summary>
        /// Klient prosi o listę wszystkich rekordów klasy <see cref="Sector"/> jakie serwer ma w swojej bazie danych.
        /// Serwer wyśle odpowiedź w formie listy List&lt;<see cref="Sector"/>&gt; zserializowanej w formacie Json.
        /// </summary>
        AllSectorsAsJson,
        /// <summary>
        /// Klient prosi o aktualizację rekordu <see cref="HostDevice"/> o danym ID <see cref="ModelObjectId"/> o nową wartość <see cref="HostDevice.LastFireStatus"/>.
        /// Serwer wyśle odpowiedź w formie listy List&lt;<see cref="HostDevice"/>&gt; zserializowanej w formacie Json.
        /// </summary>
        UpdateFireStatusFromJson
    }

    /// <summary>
    /// Jakiej odpowiedzi front-end będzie oczekiwał od serwera.
    /// </summary>
    public required RequestType Request { get; set; }

    /// <summary>
    /// Używane tylko dla <see cref="RequestType.CameraDataAsJson"/>, <see cref="RequestType.SingleHostDeviceAsJson"/> i <see cref="RequestType.SingleSectorAsJson"/>.<br/>
    /// Mówi serwerowi jaką wartość ID ma kamera/sektor, o którego dane klient wysłał zapytanie.
    /// </summary>
    public int? ModelObjectId { get; set; }

    /// <summary>
    /// Używane tylko dla <see cref="RequestType.UpdateFireStatusFromJson"/>.
    /// Mówi serwerowi na jaką wartośc ustawić pole <see cref="HostDevice.LastFireStatus"/> danego rekordu <see cref="HostDevice"/>.
    /// </summary>
    public HostDevice.FireStatus? NewStatus { get; set; }
}
