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

    private int sensorX, sensorY;
    private string sensorName;
    private string currentSensorStateString;
    private SensorState currentSensorState;
    private string sensorSegment;
    private string sensorLocation;
    private float sensorTemperature;
    private string sensorDetails;
    private int sensorLastUpdate;
    public int SensorX { get => sensorX; set => sensorX = value; }
    public int SensorY { get => sensorY; set => sensorY = value; }
    public string SensorName { get => sensorName; set => sensorName = value; }
    public string CurrentSensorStateString { get => currentSensorStateString; set => currentSensorStateString = value; }
    internal SensorState CurrentSensorState { get => currentSensorState; set => currentSensorState = value; }
    public string SensorSegment { get => sensorSegment; set => sensorSegment = value; }
    public string SensorLocation { get => sensorLocation; set => sensorLocation = value; }
    public float SensorTemperature { get => sensorTemperature; set => sensorTemperature = value; }
    public string SensorDetails { get => sensorDetails; set => sensorDetails = value; }
    public int SensorLastUpdate { get => sensorLastUpdate; set => sensorLastUpdate = value; }
    #region Constructor
    public Sensor()
    {
        this.sensorX = 0;
        this.sensorY = 0;
        this.sensorName = string.Empty;
        this.currentSensorState = SensorState.Null;
        this.sensorSegment = string.Empty;
        this.sensorLocation = string.Empty;
        this.sensorTemperature = 0;
        this.sensorDetails = string.Empty;
        this.sensorLastUpdate = 0;
    }
    public Sensor(int sensorX, int sensorY, string sensorName, string currentSensorStateString, string sensorSegment, string sensorLocation, float sensorTemperature, string sensorDetails, int sensorLastUpdate)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorName = sensorName;
        this.currentSensorStateString = currentSensorStateString;
        this.currentSensorState = StringToState(currentSensorStateString);
        this.sensorSegment = sensorSegment;
        this.sensorLocation = sensorLocation;
        this.sensorTemperature = sensorTemperature;
        this.sensorDetails = sensorDetails;
        this.sensorLastUpdate = sensorLastUpdate;
    }
    #endregion
    #region ChangeLocation
    public void SensorChangeLocation(int sensorX, int sensorY)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
    }
    public void SensorChangeLocation(string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorName = sensorName;
        this.sensorSegment = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    public void SensorChangeLocation(int sensorX, int sensorY, string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorName = sensorName;
        this.sensorSegment = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    #endregion
    #region UpdateInformation
    public void Update(SensorState sensorState)
    {
        this.currentSensorState = sensorState;
        this.sensorLastUpdate = 0;
    }
    public void Update(SensorState sensorState, float sensorTemperature, string sensorDetails)
    {
        this.currentSensorState = sensorState;
        this.sensorTemperature = sensorTemperature;
        this.sensorDetails = sensorDetails;
        this.sensorLastUpdate = 0;
    }
    public void Update(string sensorState, int sensorTemperature, string sensorDetails)
    {
        this.currentSensorState = StringToState(sensorState);
        this.sensorTemperature = sensorTemperature;
        this.sensorDetails = sensorDetails;
        this.sensorLastUpdate = 0;
    }
    #endregion
    #region Utilities
    public SensorState StringToState(string inputType)
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
        switch (currentSensorState)
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
        switch (currentSensorState)
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
