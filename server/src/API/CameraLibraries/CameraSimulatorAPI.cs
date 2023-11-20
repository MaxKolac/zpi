using Newtonsoft.Json;
using System.Text;
using ZPICommunicationModels;

namespace ZPIServer.API.CameraLibraries;

public class CameraSimulatorAPI : ICamera
{
    private HostDevice? _device;

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        string decodedString = Encoding.UTF8.GetString(bytes);
        _device = JsonConvert.DeserializeObject<HostDevice>(decodedString);
    }

    public HostDevice? GetHostDevice() => _device;
}
