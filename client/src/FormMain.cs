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
            populateList();
        }
        public void initializeSensors()
        {
            string json = File.ReadAllText(jsonPath);
            List<Sensor> dataSet = JsonSerializer.Deserialize<List<Sensor>>(json);
            foreach (var data in dataSet)
            {
                sensorList.Add(new Sensor(data.SensorX, data.SensorY, data.SensorName, data.SensorSector, data.SensorLocation));
            }
        }
        public void updateSensors()
        {
            string json = File.ReadAllText(jsonPath);
            List<Sensor> dataSet = JsonSerializer.Deserialize<List<Sensor>>(json);
            foreach (var data in dataSet)
            {
                int currentSensorIndex = sensorList.FindIndex(obiekt => obiekt.SensorName == data.SensorName);
                sensorList[currentSensorIndex].Update(data.CurrentSensorState, data.SensorTemperature, data.SensorDetails);
            }
        }
        public void populateList()
        {
            int panelX = panelDisplay.Location.X;
            int panelY = panelDisplay.Location.Y;
            int panelWidth = panelDisplay.Width;
            int panelHeight = 100;

            //int count = sensorList.Count;
            int count = 1;
            var container = new TableLayoutPanel[count];
            var labelSensor = new Label[count];

            for (int i = 0; i < count; i++)
            {
                //Container
                container[i] = new TableLayoutPanel();
                container[i].Name = "labelSensorContainer" + i;
                container[i].Location = new Point(panelX, panelY);
                container[i].Width = panelWidth;
                container[i].Height = panelHeight;
                container[i].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                container[i].BackColor = SystemColors.ControlLightLight;
                container[i].ColumnCount = 2;
                container[i].RowCount = 1;
                container[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
                container[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85));
                container[i].RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
                panelDisplay.Controls.Add(container[i]);

                //Label Sensor
                labelSensor[i] = new Label();


            }


        }

    }
}