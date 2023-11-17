using System;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.Design.AxImporter;
using System.Diagnostics;

namespace ZPIClient
{
    public partial class FormMain : Form
    {
        private string jsonPath = "../../../sensors/sensorData.json";
        private List<Sensor> sensorList = new List<Sensor>();
        private int currentSensorIndex = -1;
        private JsonSerializerOptions options = new JsonSerializerOptions();


        //Dynamic objects
        TableLayoutPanel[] panelSensorContainer;
        Label[] labelSensor;
        Panel[] panelSensorInformation;
        PictureBox[] pictureBoxSensorStatus;
        Label[] labelSensorStatus;
        Label[] labelSensorStatusDisplay;
        Label[] labelSensorSegment;
        Label[] labelSensorSegmentDisplay;

        public FormMain()
        {
            InitializeComponent();
            initializeOptions();
            initializeSensors();
            populateSensorList();
            updateColors();
        }
        public void initializeSensors()
        {
            List<Sensor> dataSet = readJSON();
            foreach (var data in dataSet)
            {
                sensorList.Add(new Sensor(data.SensorX, data.SensorY, data.SensorName, data.CurrentSensorStateString, data.SensorSegment, data.SensorLocation, data.SensorTemperature, data.SensorDetails, data.SensorLastUpdate));
            }
        }
        public void updateSensors()
        {
            List<Sensor> dataSet = readJSON();
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

            int count = sensorList.Count;

            panelSensorContainer = new TableLayoutPanel[count];
            labelSensor = new Label[count];
            panelSensorInformation = new Panel[count];
            pictureBoxSensorStatus = new PictureBox[count];
            labelSensorStatus = new Label[count];
            labelSensorStatusDisplay = new Label[count];
            labelSensorSegment = new Label[count];
            labelSensorSegmentDisplay = new Label[count];

            for (int i = 0; i < count; i++)
            {
                #region Sensor Panel Container
                panelSensorContainer[i] = new TableLayoutPanel();
                panelSensorContainer[i].Name = "tableLayoutPanel" + i;
                panelSensorContainer[i].Location = new Point(panelX, panelY);
                panelSensorContainer[i].Width = panelWidth;
                panelSensorContainer[i].Height = panelHeight;
                panelSensorContainer[i].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelSensorContainer[i].BackColor = SystemColors.ControlLightLight;
                panelSensorContainer[i].ColumnCount = 2;
                panelSensorContainer[i].RowCount = 1;
                panelSensorContainer[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, headerWidth));
                panelSensorContainer[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - headerWidth));
                panelSensorContainer[i].RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
                panelSensorContainer[i].Tag = i;
                panelSensorContainer[i].Click += sensorContainer_Click;
                panelDisplay.Controls.Add(panelSensorContainer[i]);
                #endregion
                #region Sensor Name Label
                labelSensor[i] = new Label();
                labelSensor[i].Name = "labelSensor" + i;
                labelSensor[i].Location = new Point(0, 0);
                labelSensor[i].AutoSize = false;
                labelSensor[i].TextAlign = ContentAlignment.MiddleCenter;
                labelSensor[i].Dock = DockStyle.Fill;
                labelSensor[i].Text = sensorList[i].SensorName;
                labelSensor[i].Tag = i;
                labelSensor[i].Click += sensorContainer_Click;
                panelSensorContainer[i].Controls.Add(labelSensor[i]);
                #endregion
                #region Sensor Info Panel
                panelSensorInformation[i] = new Panel();
                panelSensorInformation[i].Name = "panelInfo" + i;
                panelSensorInformation[i].Location = new Point(0, 0);
                panelSensorInformation[i].Width = (int)panelSensorContainer[i].ColumnStyles[1].Width;
                panelSensorInformation[i].Height = (int)panelSensorContainer[i].RowStyles[0].Height;
                panelSensorInformation[i].AutoSize = false;
                panelSensorInformation[i].Dock = DockStyle.Fill;
                panelSensorInformation[i].Tag = i;
                panelSensorInformation[i].Click += sensorContainer_Click;
                panelSensorContainer[i].Controls.Add(panelSensorInformation[i]);
                #endregion
                #region Sensor Status Picture
                pictureBoxSensorStatus[i] = new PictureBox();
                pictureBoxSensorStatus[i].Name = "pictureBoxStatus" + i;
                pictureBoxSensorStatus[i].Location = new Point(pictureBoxMargin, panelSensorInformation[i].Height / 4);
                pictureBoxSensorStatus[i].Width = pictureBoxSize;
                pictureBoxSensorStatus[i].Height = pictureBoxSize;
                pictureBoxSensorStatus[i].BackColor = Color.Lime;
                roundPictureBox(ref pictureBoxSensorStatus[i]);
                pictureBoxSensorStatus[i].Tag = i;
                pictureBoxSensorStatus[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(pictureBoxSensorStatus[i]);
                #endregion
                #region Sensor Status Label
                labelSensorStatus[i] = new Label();
                labelSensorStatus[i].Name = "labelStatus" + i;
                labelSensorStatus[i].Location = new Point(pictureBoxSensorStatus[i].Location.X + pictureBoxMargin, panelSensorInformation[i].Height / 4);
                labelSensorStatus[i].MaximumSize = new Size(150, rowHeight);
                labelSensorStatus[i].AutoSize = true;
                labelSensorStatus[i].Font = new Font(labelSensorStatus[i].Font.Name, fontSize);
                labelSensorStatus[i].TextAlign = ContentAlignment.TopLeft;
                labelSensorStatus[i].Text = "Status:";
                labelSensorStatus[i].Tag = i;
                labelSensorStatus[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorStatus[i]);
                #endregion
                #region Sensor Status Display Label
                labelSensorStatusDisplay[i] = new Label();
                labelSensorStatusDisplay[i].Name = "labelStatusDisplay" + i;
                labelSensorStatusDisplay[i].Location = new Point(labelSensorStatus[i].Location.X + labelSensorStatus[i].Width, panelSensorInformation[i].Height / 4);
                labelSensorStatusDisplay[i].MaximumSize = new Size(300, rowHeight);
                labelSensorStatusDisplay[i].AutoSize = true;
                labelSensorStatusDisplay[i].Font = new Font(labelSensorStatusDisplay[i].Font.Name, fontSize);
                labelSensorStatusDisplay[i].TextAlign = ContentAlignment.TopLeft;
                labelSensorStatusDisplay[i].Text = sensorList[i].StateToString();
                labelSensorStatusDisplay[i].Tag = i;
                labelSensorStatusDisplay[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorStatusDisplay[i]);
                #endregion
                #region Sensor Segment Label
                labelSensorSegment[i] = new Label();
                labelSensorSegment[i].Name = "labelSegment" + i;
                labelSensorSegment[i].Location = new Point(labelSensorStatusDisplay[i].Location.X + labelSensorStatusDisplay[i].Width + elementMargin, panelSensorInformation[i].Height / 4);
                labelSensorSegment[i].MaximumSize = new Size(150, rowHeight);
                labelSensorSegment[i].AutoSize = true;
                labelSensorSegment[i].Font = new Font(labelSensorSegment[i].Font.Name, fontSize);
                labelSensorSegment[i].TextAlign = ContentAlignment.TopLeft;
                labelSensorSegment[i].Text = "Segment:";
                labelSensorSegment[i].Tag = i;
                labelSensorSegment[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorSegment[i]);
                #endregion
                #region Sensor Segment Display Label
                labelSensorSegmentDisplay[i] = new Label();
                labelSensorSegmentDisplay[i].Name = "labelSegmentDisplay" + i;
                labelSensorSegmentDisplay[i].Location = new Point(labelSensorSegment[i].Location.X + labelSensorSegment[i].Width, panelSensorInformation[i].Height / 4);
                labelSensorSegmentDisplay[i].MaximumSize = new Size(300, rowHeight);
                labelSensorSegmentDisplay[i].AutoSize = true;
                labelSensorSegmentDisplay[i].Font = new Font(labelSensorSegmentDisplay[i].Font.Name, fontSize);
                labelSensorSegmentDisplay[i].TextAlign = ContentAlignment.TopLeft;
                labelSensorSegmentDisplay[i].Text = sensorList[i].SensorSegment;
                labelSensorSegmentDisplay[i].Tag = i;
                labelSensorSegmentDisplay[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorSegmentDisplay[i]);
                #endregion

                panelY += rowHeight * 2;
            }


        }
        public void initializeOptions()
        {
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
        }
        private void roundPictureBox(ref PictureBox pb)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pb.Width - 3, pb.Height - 3);
            Region rg = new Region(gp);
            pb.Region = rg;
        }
        private List<Sensor> readJSON()
        {
            using FileStream json = File.OpenRead(jsonPath);
            List<Sensor> sensors = JsonSerializer.Deserialize<List<Sensor>>(json, options);
            return sensors;
        }
        private void updateColors()
        {
            int count = sensorList.Count;
            for (int i = 0; i < count; i++)
            {
                pictureBoxSensorStatus[i].BackColor = sensorList[i].StateToColor();
            }
        }

        private void sensorContainer_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            currentSensorIndex = (int)control.Tag;
            if (currentSensorIndex != -1)
            {
                labelSensorName.Text = sensorList[currentSensorIndex].SensorName;
                labelStateInfo.Text = sensorList[currentSensorIndex].StateToString();
                labelSegmentInfo.Text = sensorList[currentSensorIndex].SensorSegment;
                labelLocationInfo.Text = sensorList[currentSensorIndex].SensorLocation;
                labelTemperatureInfo.Text = sensorList[currentSensorIndex].SensorTemperature.ToString() + "°C";
                labelLastUpdateInfo.Text = sensorList[currentSensorIndex].SensorLastUpdate.ToString();

                try
                {
                    Image cameraImage = Image.FromFile("../../../sensors/" + sensorList[currentSensorIndex].SensorDetails);
                    pictureBoxCamera.Image = cameraImage;
                }
                catch(FileNotFoundException)
                {
                    pictureBoxCamera.Image = pictureBoxCamera.ErrorImage;
                }

                switch (sensorList[currentSensorIndex].CurrentSensorState)
                {
                    case Sensor.SensorState.Alert:
                        buttonFire.BackColor = Color.Red;
                        buttonFire.ForeColor = Color.White;
                        buttonFire.Enabled = true;
                        buttonFire.Text = "PotwierdŸ Po¿ar";
                        break;

                    case Sensor.SensorState.Fire:
                        buttonFire.BackColor = Color.Red;
                        buttonFire.ForeColor = Color.White;
                        buttonFire.Enabled = true;
                        buttonFire.Text = "PotwierdŸ Zwalczenie Po¿aru";
                        break;

                    default:
                        buttonFire.BackColor = SystemColors.Control;
                        buttonFire.ForeColor = SystemColors.ControlText;
                        buttonFire.Enabled = false;
                        buttonFire.Text = "Brak Problemów";
                        break;
                }
            }

        }
    }
}