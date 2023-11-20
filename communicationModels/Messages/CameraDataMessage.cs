using ZPICommunicationModels.Models;

namespace ZPICommunicationModels.Messages;

/// <summary>
/// Klasa działająca jako "pakiet" informacji wysłany z kamery do serwera.
/// </summary>
public class CameraDataMessage
{
    public required decimal LargestTemperature { get; set; }
    public required byte[] Image { get; set; }
    public required HostDevice.DeviceStatus Status { get; set; }
}
