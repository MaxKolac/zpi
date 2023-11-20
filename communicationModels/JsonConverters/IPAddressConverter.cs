using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ZPICommunicationModels.JsonConverters;

/// <summary>
/// <see cref="IPAddress"/> nie jest klasą przyjazną dla serializowania - wymaga osobnego konwertera.<br/>
/// Zarejestruj konwerter przed serializowaniem/deserializowaniem w taki sposób:
/// <code>
/// var settings = new JsonSerializerSettings();
/// settings.Converters.Add(new <see cref="IPAddressConverter"/>());
/// string json = JsonConvert.SerializeObject(object, settings);
/// </code>
/// Źródło: <see href="https://pingfu.net/how-to-serialise-ipaddress-ipendpoint"/>
/// </summary>
public class IPAddressConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(IPAddress));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var address = value.ToString();
        JToken.FromObject(address).WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var address = JToken.Load(reader).ToString();
        return IPAddress.Parse(address);
    }
}
