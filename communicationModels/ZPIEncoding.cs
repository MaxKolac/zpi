using Newtonsoft.Json;
using System.Text;
using ZPICommunicationModels.JsonConverters;

namespace ZPICommunicationModels;

/// <summary>
/// Klasa statyczna którą można zakodować i odkodować różne obiekty jako ciągi bajtów od przesyłania przez sieć.<br/>
/// </summary>
public static class ZPIEncoding
{
    /// <summary>
    /// Koduje <typeparamref name="T"/> na ciąg bitów gotowych do przesłania przez sieć.
    /// </summary>
    /// <exception cref="JsonSerializationException"/>
    public static byte[] Encode<T>(T message)
    {
        string json = JsonConvert.SerializeObject(message, Formatting.Indented, new IPAddressConverter(), new IPEndPointConverter());
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Odkodowuje ciąg bitów z powrotem na <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="JsonSerializationException"/>
    public static T? Decode<T>(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(json, new IPAddressConverter(), new IPEndPointConverter());
    }

    /// <summary>
    /// Konwertuje ciąg bitów na tekst koderem formatu UTF-8.
    /// </summary>
    public static string GetString(byte[] utf8bytes) => Encoding.UTF8.GetString(utf8bytes);

    /// <summary>
    /// Konwertuje tekst na ciąg bitów koderem formatu UTF-8.
    /// </summary>
    public static byte[] GetBytes(string text) => Encoding.UTF8.GetBytes(text);
}
