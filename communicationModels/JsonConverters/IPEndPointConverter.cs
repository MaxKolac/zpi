using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ZPICommunicationModels.JsonConverters;

/// <summary>
/// <see cref="IPEndPoint"/> nie jest klasą przyjazną dla serializowania - wymaga osobnego konwertera.<br/>
/// Zarejestruj konwerter przed serializowaniem/deserializowaniem w taki sposób:
/// <code>
/// var settings = new JsonSerializerSettings();
/// settings.Converters.Add(new <see cref="IPEndPointConverter"/>());
/// string json = JsonConvert.SerializeObject(object, settings);
/// </code>
/// Źródło: <see href="https://gist.github.com/marcbarry/2e7a64fed2ae539cf415"/>
/// </summary>
public class IPEndPointConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IPEndPoint);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return JToken.Load(reader).ToString().ToIPEndPoint();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is IPEndPoint ipEndPoint)
        {
            if (ipEndPoint.Address != null || ipEndPoint.Port != 0)
            {
                JToken.FromObject(string.Format("{0}:{1}", ipEndPoint.Address, ipEndPoint.Port)).WriteTo(writer);
                return;
            }
        }
        writer.WriteNull();
    }
}

public static class IPAddressExtensions
{
    public static IPEndPoint? ToIPEndPoint(this string ipEndPoint)
    {
        if (string.IsNullOrWhiteSpace(ipEndPoint))
        {
            return null;
        }
        var components = ipEndPoint.Split(':');
        return new IPEndPoint(IPAddress.Parse(components[0]), Convert.ToInt32(components[1]));
    }
}