namespace ZPIServer.API;

/// <summary>
/// Wspólny interfejs, pod który wszystkie sygnały kamer termowizyjnych będzie tłumaczony przez odpowiednią klasę w <see cref="CameraLibraries"/>.<br/>
/// Metody z prefiksami 'Request' wysyłają żądania do kamer. Metody z prefiksami 'Decode' zwracają dane otrzymane z ostatniego żądania.
/// </summary>
public interface ICamera
{
    void RequestPicture();

    void DecodePicture(); 
}
