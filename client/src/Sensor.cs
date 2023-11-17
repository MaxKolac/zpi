using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private SensorState currentSensorState;
    private string sensorSector;
    private string sensorLocation;
    private int sensorTemperature;
    private string sensorDetails;
    private int sensorLastUpdate;
    public int SensorX { get => sensorX; set => sensorX = value; }
    public int SensorY { get => sensorY; set => sensorY = value; }
    public string SensorName { get => sensorName; set => sensorName = value; }
    internal SensorState CurrentSensorState { get => currentSensorState; set => currentSensorState = value; }
    public string SensorSector { get => sensorSector; set => sensorSector = value; }
    public string SensorLocation { get => sensorLocation; set => sensorLocation = value; }
    public int SensorTemperature { get => sensorTemperature; set => sensorTemperature = value; }
    public string SensorDetails { get => sensorDetails; set => sensorDetails = value; }
    public int SensorLastUpdate { get => sensorLastUpdate; set => sensorLastUpdate = value; }
    #region Constructor
    public Sensor(int sensorX, int sensorY, string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorName = sensorName;
        this.currentSensorState = SensorState.Inactive;
        this.sensorSector = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    public Sensor(string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = 0;
        this.sensorY = 0;
        this.sensorName = sensorName;
        this.currentSensorState = SensorState.Inactive;
        this.sensorSector = sensorSector;
        this.sensorLocation = sensorLocation;
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
        this.sensorSector = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    public void SensorChangeLocation(int sensorX, int sensorY, string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorName = sensorName;
        this.sensorSector = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    #endregion
    #region UpdateInformation
    public void Update(SensorState sensorState)
    {
        this.currentSensorState = sensorState;
        this.sensorLastUpdate = 0;
    }
    public void Update(SensorState sensorState, int sensorTemperature, string sensorDetails)
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
    private SensorState StringToState(string inputType)
    {
        switch (inputType)
        {
            case "Active":
                return SensorState.Active;

            case "Incative":
                return SensorState.Inactive;

            case "Alert":
                return SensorState.Alert;

            case "Fire":
                return SensorState.Fire;

            default:
                return SensorState.Null;
        }
    }
    #endregion
}
