using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using ZPICommunicationModels.Models;

namespace ZPIClient;
public static class HostDeviceExtensions
{
    public static string StateToString(this HostDevice hostDevice)
    {
        switch (hostDevice.LastFireStatus)
        {
            case HostDevice.FireStatus.OK:
                return "Active";

            case HostDevice.FireStatus.Suspected:
                return "Alert";

            case HostDevice.FireStatus.Confirmed:
                return "Fire";

            default:
                return "Null";
        }
    }
    public static Color StateToColor(this HostDevice hostDevice)
    {
        switch (hostDevice.LastFireStatus)
        {
            case HostDevice.FireStatus.OK:
                return Color.Lime;

            case HostDevice.FireStatus.Suspected:
                return Color.Orange;

            case HostDevice.FireStatus.Confirmed:
                return Color.Red;

            default:
                return Color.RoyalBlue;
        }
    }
}
