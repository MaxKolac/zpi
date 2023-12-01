using ZPICommunicationModels;
using ZPICommunicationModels.Messages;

namespace ZPIServer.API.CameraLibraries;

public class CameraSimulatorAPI : ICamera
{
    private CameraDataMessage? _message;

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        _message = ZPIEncoding.Decode<CameraDataMessage>(bytes);
    }

    public CameraDataMessage? GetDecodedMessage() => _message;
}
