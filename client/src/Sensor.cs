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
internal class Sensor : HostDevice
{
    public int SensorLastUpdate { get; set; }
    public bool Override { get; set; }

    #region Constructor
    public Sensor() : base()
    {
        this.SensorLastUpdate = 0;
        this.Override = false;
    }
    #endregion
    #region ChangeLocation
    public void SensorChangeLocation(int sensorX, int sensorY)
    {
        this.LocationLatitude = sensorX;
        this.LocationAltitude = sensorY;
    }
    public void SensorChangeLocation(string sensorName, Sector sensorSector, string sensorLocation)
    {
        this.Name = sensorName;
        this.Sector = sensorSector;
        this.LocationDescription = sensorLocation;
    }
    public void SensorChangeLocation(int sensorX, int sensorY, string sensorName, Sector sensorSector, string sensorLocation)
    {
        this.LocationAltitude = sensorX;
        this.LocationLatitude = sensorY;
        this.Name = sensorName;
        this.Sector = sensorSector;
        this.LocationDescription = sensorLocation;
    }
    #endregion
    #region Utilities
    public string StateToString()
    {
        switch (LastFireStatus)
        {
            case FireStatus.OK:
                return "Active";

            case FireStatus.Suspected:
                return "Alert";

            case FireStatus.Confirmed:
                return "Fire";

            default:
                return "Null";
        }
    }
    public Color StateToColor()
    {
        switch (LastFireStatus)
        {
            case FireStatus.OK:
                return Color.Lime;

            case FireStatus.Suspected:
                return Color.Orange;

            case FireStatus.Confirmed:
                return Color.Red;

            default:
                return Color.RoyalBlue;
        }
    }
    #endregion
}
