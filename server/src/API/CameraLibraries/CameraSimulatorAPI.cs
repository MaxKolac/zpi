using Newtonsoft.Json;
using System.Text;
using ZPICommunicationModels.Messages;

namespace ZPIServer.API.CameraLibraries;

public class CameraSimulatorAPI : ICamera
{
    private CameraDataMessage? _message;

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        string decodedString = Encoding.UTF8.GetString(bytes);
        _message = JsonConvert.DeserializeObject<CameraDataMessage>(decodedString);
    }

    public CameraDataMessage? GetDecodedMessage() => _message;
}
