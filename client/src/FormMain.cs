using System.Text.Json;

namespace ZPIClient
{
    public partial class FormMain : Form
    {
        private string jsonPath = "sensors/sensorData.json";
        private List<Sensor> sensorList = new List<Sensor>();
        private int currentSensorIndex = -1;


        public FormMain()
        {
            InitializeComponent();
        }



        public void initializeSensors()
        {
            string json = File.ReadAllText(jsonPath);
            List<Sensor> dataSet = JsonSerializer.Deserialize<List<Sensor>>(json);
            foreach (var data in dataSet)
            {
                var (x,y) = data.SensorGetCoordinates();
                var (name, location, sector) = data.SensorGetInfo();
                sensorList.Add(new Sensor(x, y, name, sector, location));
            }
        }
        public void updateSensors()
        {
            string json = File.ReadAllText(jsonPath);
            List<Sensor> dataSet = JsonSerializer.Deserialize<List<Sensor>>(json);
            foreach (var data in dataSet)
            {
                int currentSensorIndex = sensorList.FindIndex(obiekt => obiekt.SensorGetInfo().Item1 == data.SensorGetInfo().Item1);
                var (state, temperature, details) = data.SensorGetDetails();
                sensorList[currentSensorIndex].Update(state, temperature, details);
            }
        }
    }
}