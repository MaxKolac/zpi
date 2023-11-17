using System.Text.Json;
using System.Windows.Forms;

namespace ZPIClient
{
    public partial class FormMain : Form
    {
        private string jsonPath = "../../../sensors/sensorData.json";
        private List<Sensor> sensorList = new List<Sensor>();
        private int currentSensorIndex = -1;


        public FormMain()
        {
            InitializeComponent();
            initializeSensors();
            populateSensorList();
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
        public void populateSensorList()
        {
            int panelX = panelDisplay.Location.X;
            int panelY = panelDisplay.Location.Y;
            int panelWidth = panelDisplay.Width;
            int panelHeight = 100;

            #region Style Parameters
            int headerWidth = 15; //Value in %
            int rowHeight = 50; //Value in pixels
            int pictureBoxSize = 15; //Value in pixels
            int pictureBoxMargin = 20; //Value in pixels
            int elementMargin = 50; //Value in pixels
            int fontSize = 16;
            #endregion

            //int count = sensorList.Count;
            int count = 2;

            var panelContainer = new TableLayoutPanel[count];
            var labelSensor = new Label[count];
            var panelInfo = new Panel[count];

            var pictureBoxStatus = new PictureBox[count];
            var labelStatus = new Label[count];
            var labelStatusDisplay = new Label[count];
            var labelSegment = new Label[count];
            var labelSegmentDisplay = new Label[count];

            for (int i = 0; i < count; i++)
            {
                #region Sensor Panel Container
                panelContainer[i] = new TableLayoutPanel();
                panelContainer[i].Name = "panelLabelSensor" + i;
                panelContainer[i].Location = new Point(panelX, panelY);
                panelContainer[i].Width = panelWidth;
                panelContainer[i].Height = panelHeight;
                panelContainer[i].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelContainer[i].BackColor = SystemColors.ControlLightLight;
                panelContainer[i].ColumnCount = 2;
                panelContainer[i].RowCount = 1;
                panelContainer[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, headerWidth));
                panelContainer[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - headerWidth));
                panelContainer[i].RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
                panelDisplay.Controls.Add(panelContainer[i]);
                #endregion
                #region Sensor Name Label
                labelSensor[i] = new Label();
                labelSensor[i].Name = "labelSensor" + i;
                labelSensor[i].Location = new Point(0, 0);
                labelSensor[i].AutoSize = false;
                labelSensor[i].TextAlign = ContentAlignment.MiddleCenter;
                labelSensor[i].Dock = DockStyle.Fill;
                //labelSensor[i].Text = sensorList[i].SensorName;
                labelSensor[i].Text = "Test";
                panelContainer[i].Controls.Add(labelSensor[i]);
                #endregion
                #region Sensor Info Panel
                panelInfo[i] = new Panel();
                panelInfo[i].Name = "panelInfo" + i;
                panelInfo[i].Location = new Point(0, 0);
                panelInfo[i].Width = (int)panelContainer[i].ColumnStyles[1].Width;
                panelInfo[i].Height = (int)panelContainer[i].RowStyles[0].Height;
                panelInfo[i].AutoSize = false;
                panelInfo[i].Dock = DockStyle.Fill;
                panelContainer[i].Controls.Add(panelInfo[i]);
                #endregion
                #region Sensor Status Picture
                pictureBoxStatus[i] = new PictureBox();
                pictureBoxStatus[i].Name = "pictureBoxStatus" + i;
                pictureBoxStatus[i].Location = new Point(pictureBoxMargin, panelInfo[i].Height / 4);
                pictureBoxStatus[i].Width = pictureBoxSize;
                pictureBoxStatus[i].Height = pictureBoxSize;
                pictureBoxStatus[i].BackColor = Color.Lime;
                roundPictureBox(ref pictureBoxStatus[i]);
                panelInfo[i].Controls.Add(pictureBoxStatus[i]);
                #endregion
                #region Sensor Status Label
                labelStatus[i] = new Label();
                labelStatus[i].Name = "labelStatus" + i;
                labelStatus[i].Location = new Point(pictureBoxStatus[i].Location.X + pictureBoxMargin, panelInfo[i].Height / 4);
                labelStatus[i].MaximumSize = new Size(150, rowHeight);
                labelStatus[i].AutoSize = true;
                labelStatus[i].Font = new Font(labelStatus[i].Font.Name, fontSize);
                labelStatus[i].TextAlign = ContentAlignment.TopLeft;
                labelStatus[i].Text = "Status:";
                panelInfo[i].Controls.Add(labelStatus[i]);
                #endregion
                #region Sensor Status Display Label
                labelStatusDisplay[i] = new Label();
                labelStatusDisplay[i].Name = "labelStatusDisplay" + i;
                labelStatusDisplay[i].Location = new Point(labelStatus[i].Location.X + labelStatus[i].Width, panelInfo[i].Height / 4);
                labelStatusDisplay[i].MaximumSize = new Size(300, rowHeight);
                labelStatusDisplay[i].AutoSize = true;
                labelStatusDisplay[i].Font = new Font(labelStatusDisplay[i].Font.Name, fontSize);
                labelStatusDisplay[i].TextAlign = ContentAlignment.TopLeft;
                //labelStatusDisplay[i].Text = sensorList[i].getSensorStateAsString();
                labelStatusDisplay[i].Text = "PRZYK£ADOWY STAN";
                panelInfo[i].Controls.Add(labelStatusDisplay[i]);
                #endregion
                #region Sensor Segment Label
                labelSegment[i] = new Label();
                labelSegment[i].Name = "labelSegment" + i;
                labelSegment[i].Location = new Point(labelStatusDisplay[i].Location.X + labelStatusDisplay[i].Width + elementMargin, panelInfo[i].Height / 4);
                labelSegment[i].MaximumSize = new Size(150, rowHeight);
                labelSegment[i].AutoSize = true;
                labelSegment[i].Font = new Font(labelSegment[i].Font.Name, fontSize);
                labelSegment[i].TextAlign = ContentAlignment.TopLeft;
                labelSegment[i].Text = "Segment:";
                panelInfo[i].Controls.Add(labelSegment[i]);
                #endregion
                #region Sensor Segment Display Label
                labelSegmentDisplay[i] = new Label();
                labelSegmentDisplay[i].Name = "labelSegmentDisplay" + i;
                labelSegmentDisplay[i].Location = new Point(labelSegment[i].Location.X + labelSegment[i].Width, panelInfo[i].Height / 4);
                labelSegmentDisplay[i].MaximumSize = new Size(300, rowHeight);
                labelSegmentDisplay[i].AutoSize = true;
                labelSegmentDisplay[i].Font = new Font(labelSegmentDisplay[i].Font.Name, fontSize);
                labelSegmentDisplay[i].TextAlign = ContentAlignment.TopLeft;
                //labelStatusDisplay[i].Text = sensorList[i].getSensorStateAsString();
                labelSegmentDisplay[i].Text = "A0";
                panelInfo[i].Controls.Add(labelSegmentDisplay[i]);
                #endregion

                panelY += rowHeight * 2;
            }


        }

        private void roundPictureBox(ref PictureBox pb)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pb.Width - 3, pb.Height - 3);
            Region rg = new Region(gp);
            pb.Region = rg;
        }
    }
}