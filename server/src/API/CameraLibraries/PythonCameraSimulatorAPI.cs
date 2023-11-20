using ZPICommunicationModels.Messages;

namespace ZPIServer.API.CameraLibraries;

public class PythonCameraSimulatorAPI : ICamera
{
    public void DecodeReceivedBytes(byte[]? bytes)
    {
        throw new NotImplementedException();
    }

    public CameraDataMessage? GetDecodedMessage()
    {
        throw new NotImplementedException();
    }
}
