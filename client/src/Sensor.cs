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
    public enum SensorState
    {
        Active,
        Inactive,
        Alert,
        Fire,
        Null
    }
    internal SensorState CurrentSensorState { get; set; }
    public int SensorLastUpdate { get; set; }
    public string SensorDetails {  get; set; }
    public bool Override { get; set; }

    #region Constructor
    public Sensor() : base()
    {
        this.CurrentSensorState = SensorState.Null;
        this.SensorLastUpdate = 0;
        this.SensorDetails = String.Empty;
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
        this.SensorDetails = sensorLocation;
    }
    public void SensorChangeLocation(int sensorX, int sensorY, string sensorName, Sector sensorSector, string sensorLocation)
    {
        this.LocationAltitude = sensorX;
        this.LocationLatitude = sensorY;
        this.Name = sensorName;
        this.Sector = sensorSector;
        this.SensorDetails = sensorLocation;
    }
    #endregion
    #region UpdateInformation
    public void Update(SensorState sensorState)
    {
        this.CurrentSensorState = sensorState;
        this.SensorLastUpdate = 0;
    }
    public void Update(SensorState sensorState, int sensorTemperature)
    {
        this.CurrentSensorState = sensorState;
        this.LastKnownTemperature = sensorTemperature;
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
                return Color.RoyalBlue;

            case SensorState.Alert:
                return Color.Orange;

            case SensorState.Fire:
                return Color.Red;

            default:
                return Color.Lavender;
        }
    }
    public void StateFromStatus()
    {
        if (Override)
        {
            CurrentSensorState = SensorState.Fire;
            return;
        }
        if(LastKnownStatus == DeviceStatus.OK)
        {
            if(LastKnownTemperature > 40)
            {
                CurrentSensorState = SensorState.Alert;
                return;
            }
            CurrentSensorState = SensorState.Active;
            return;
        }
        else
        {
            CurrentSensorState = SensorState.Inactive;
            return;
        }
    }
    #endregion
}
