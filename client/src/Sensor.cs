using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZPIClient;
internal class Sensor
{
    public enum SensorState
    {
        Active,
        Inactive,
        Alert,
        Fire,
        Null
    }

    public int SensorX { get; set; }
    public int SensorY { get; set; }
    public string SensorName { get; set; }
    public string CurrentSensorStateString { get; set; }
    internal SensorState CurrentSensorState { get; set; }
    public string SensorSegment { get; set; }
    public string SensorLocation { get; set; }
    public float SensorTemperature { get; set; }
    public string SensorDetails { get; set; }
    public int SensorLastUpdate { get; set; }

    #region Constructor
    public Sensor()
    {
        this.SensorX = 0;
        this.SensorY = 0;
        this.SensorName = string.Empty;
        this.CurrentSensorState = SensorState.Null;
        this.SensorSegment = string.Empty;
        this.SensorLocation = string.Empty;
        this.SensorTemperature = 0;
        this.SensorDetails = string.Empty;
        this.SensorLastUpdate = 0;
    }
    public Sensor(int sensorX, int sensorY, string sensorName, string currentSensorStateString, string sensorSegment, string sensorLocation, float sensorTemperature, string sensorDetails, int sensorLastUpdate)
    {
        this.SensorX = sensorX;
        this.SensorY = sensorY;
        this.SensorName = sensorName;
        this.CurrentSensorStateString = currentSensorStateString;
        this.CurrentSensorState = StringToState(currentSensorStateString);
        this.SensorSegment = sensorSegment;
        this.SensorLocation = sensorLocation;
        this.SensorTemperature = sensorTemperature;
        this.SensorDetails = sensorDetails;
        this.SensorLastUpdate = sensorLastUpdate;
    }
    #endregion
    #region ChangeLocation
    public void SensorChangeLocation(int sensorX, int sensorY)
    {
        this.SensorX = sensorX;
        this.SensorY = sensorY;
    }
    public void SensorChangeLocation(string sensorName, string sensorSector, string sensorLocation)
    {
        this.SensorName = sensorName;
        this.SensorSegment = sensorSector;
        this.SensorLocation = sensorLocation;
    }
    public void SensorChangeLocation(int sensorX, int sensorY, string sensorName, string sensorSector, string sensorLocation)
    {
        this.SensorX = sensorX;
        this.SensorY = sensorY;
        this.SensorName = sensorName;
        this.SensorSegment = sensorSector;
        this.SensorLocation = sensorLocation;
    }
    #endregion
    #region UpdateInformation
    public void Update(SensorState sensorState)
    {
        this.CurrentSensorState = sensorState;
        this.SensorLastUpdate = 0;
    }
    public void Update(SensorState sensorState, float sensorTemperature)
    {
        this.CurrentSensorState = sensorState;
        this.SensorTemperature = sensorTemperature;
        this.SensorLastUpdate = 0;
    }
    #endregion
    #region Utilities
    public static SensorState StringToState(string inputType)
    {
        switch (inputType)
        {
            case "Active":
                return SensorState.Active;

            case "Inactive":
                return SensorState.Inactive;

            case "Alert":
                return SensorState.Alert;

            case "Fire":
                return SensorState.Fire;

            default:
                return SensorState.Null;
        }
    }
    
    public string StateToString()
    {
        switch (CurrentSensorState)
        {
            case SensorState.Active:
                return "Active";

            case SensorState.Inactive:
                return "Inactive";

            case SensorState.Alert:
                return "Alert";

            case SensorState.Fire:
                return "Fire";

            default:
                return "Null";
        }
    }

    public Color StateToColor()
    {
        switch (CurrentSensorState)
        {
            case SensorState.Active:
                return Color.Lime;

            case SensorState.Inactive:
                return Color.Lavender;

            case SensorState.Alert:
                return Color.Orange;

            case SensorState.Fire:
                return Color.Red;

            default:
                return Color.Lavender;
        }
    }
    #endregion
}
