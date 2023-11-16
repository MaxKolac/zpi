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
    private SensorState sensorState;
    private string sensorSector;
    private string sensorLocation;
    private int sensorTemperature;
    private string sensorDetails;
    private int sensorLastUpdate;

    #region Constructor
    public Sensor(int sensorX, int sensorY, string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorName = sensorName;
        this.sensorState = SensorState.Inactive;
        this.sensorSector = sensorSector;
        this.sensorLocation = sensorLocation;
    }
    public Sensor(string sensorName, string sensorSector, string sensorLocation)
    {
        this.sensorX = 0;
        this.sensorY = 0;
        this.sensorName = sensorName;
        this.sensorState = SensorState.Inactive;
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
    #region ChangeInfo
    public void SensorChangeName(string sensorName)
    {
        this.sensorName = sensorName;
    }
    #endregion
    #region UpdateInformation
    public void Update(SensorState sensorState)
    {
        this.sensorState = sensorState;
        this.sensorLastUpdate = 0;
    }
    public void Update(SensorState sensorState, int sensorTemperature, string sensorDetails)
    {
        this.sensorState = sensorState;
        this.sensorTemperature = sensorTemperature;
        this.sensorDetails = sensorDetails;
        this.sensorLastUpdate = 0;
    }
    public void Update(string sensorState, int sensorTemperature, string sensorDetails)
    {
        this.sensorState = StringToState(sensorState);
        this.sensorTemperature = sensorTemperature;
        this.sensorDetails = sensorDetails;
        this.sensorLastUpdate = 0;
    }
    #endregion
    #region GetInfo
    public (int, int) SensorGetCoordinates()
    {
        return (sensorX, sensorY);
    }
    public (string, string, string) SensorGetInfo()
    {
        return (sensorName, sensorLocation, sensorSector);
    }

    public (SensorState, int, string) SensorGetDetails()
    {
        return (sensorState, sensorTemperature, sensorDetails);
    }

    public int SensorGetLastUpdate()
    {
        return this.sensorLastUpdate;
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
