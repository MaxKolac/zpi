using System;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.Design.AxImporter;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Net.Sockets;
using ZPICommunicationModels;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;
using System.Net;

namespace ZPIClient
{
    public partial class FormMain : Form
    {
        //Path settings
        private string jsonPath = "../../../sensors/sensorData.json";
        private string ipAddress = "127.0.0.1";
        private int port = 25566;
        private TcpClient tcpClient;
        private JsonSerializerOptions options = new JsonSerializerOptions();

        //Sensor variables
        private List<Sensor> sensorList = new List<Sensor>();
        private int currentSensorIndex = -1;

        //Timer settings
        private int timerInterval = 30, timerElapsedTime = 30;

        //Debug
        private bool debug = false;

        //Dynamic objects
        TableLayoutPanel[] panelSensorContainer;
        Label[] labelSensor;
        Panel[] panelSensorInformation;
        PictureBox[] pictureBoxSensorStatus;
        Label[] labelSensorStatus;
        Label[] labelSensorSegment;

        TableLayoutPanel[] panelMapSensorInformation;
        Label[] labelMapSensor;
        PictureBox[] pictureBoxMapSensorStatus;

        public FormMain()
        {
            //Initialize
            InitializeComponent();
            serverRequest(UserRequest.RequestType.AllHostDevicesAsJson);
            initializeOptions();
            initializeSensors();
            initializeListFormControls();
            initializeSideScroll();
            initializeMapStateDisplay();
            updateAll();
            timerRefresh.Start();
        }
        private void updateSensors()
        {
            List<Sensor> dataSet = readJSON();
            int i = 0;
            foreach (var data in dataSet)
            {
                if (sensorList[i].CurrentSensorState != Sensor.StringToState(data.CurrentSensorStateString) || sensorList[i].SensorTemperature != data.SensorTemperature)
                {
                    sensorList[i].Update(Sensor.StringToState(data.CurrentSensorStateString), data.SensorTemperature);
                    labelSensorStatus[i].Text = "Status: " + data.CurrentSensorStateString;
                    if(i == currentSensorIndex)
                    {
                        updateInfoPanel();
                    }
                }
                i++;
            }
            updateAll();
        }
        private List<Sensor> readJSON()
        {
            using FileStream json = File.OpenRead(jsonPath);
            List<Sensor> sensors = JsonSerializer.Deserialize<List<Sensor>>(json, options);
            return sensors;
        }
        #region Timer Functions
        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            incrementTimers();
            if (timerElapsedTime > 0)
            {
                timerElapsedTime -= 1;
            }
            else
            {
                timerElapsedTime = timerInterval - 1;
                updateSensors();
            }
            if (currentSensorIndex != -1)
            {
                labelLastUpdateInfo.Text = sensorList[currentSensorIndex].SensorLastUpdate.ToString() + " sekund temu";
            }
            labelTimer.Text = "Nastêpna aktualizacja za: " + timerElapsedTime + " sekund";
        }
        private void incrementTimers()
        {
            foreach (Sensor sensor in sensorList)
            {
                sensor.SensorLastUpdate += 1;
            }
        }
        #endregion
        #region Buttons
        private void buttonDebug_Click(object sender, EventArgs e)
        {
            if (debug)
            {
                buttonDebug.BackColor = SystemColors.Control;
                buttonDebug.Text = "Debug";
            }
            else
            {
                buttonDebug.Text = "Skaner wspó³rzêdnych aktywny";
                buttonDebug.BackColor = Color.SkyBlue;
            }
            debug = !debug;

        }
        private void buttonSwitch_Click(object sender, EventArgs e)
        {
            panelDisplay.Visible = !panelDisplay.Visible;
            panelDisplay.Enabled = !panelDisplay.Enabled;
            panelMap.Visible = !panelMap.Visible;
            panelMap.Enabled = !panelMap.Enabled;
        }
        private void buttonOverview_Click(object sender, EventArgs e)
        {

        }
        private void sensorContainer_Click(object sender, EventArgs e)
        {
            if (currentSensorIndex != -1)
            {
                panelSensorContainer[currentSensorIndex].BackColor = SystemColors.ControlLightLight;
                panelMapSensorInformation[currentSensorIndex].BackColor = SystemColors.Control;
            }
            Control control = (Control)sender;
            currentSensorIndex = (int)control.Tag;
            if (currentSensorIndex != -1)
            {
                updateInfoPanel();
            }
        }
        #endregion
        #region Initialize Functions
        private void initializeSensors()
        {
            List<Sensor> dataSet = readJSON();
            foreach (var data in dataSet)
            {
                sensorList.Add(new Sensor(data.SensorX, data.SensorY, data.SensorName, data.CurrentSensorStateString, data.SensorSegment, data.SensorLocation, data.SensorTemperature, data.SensorDetails, data.SensorLastUpdate));
            }
        }
        private void initializeListFormControls()
        {
            int panelX = panelDisplay.Location.X;
            int panelY = panelDisplay.Location.Y;
            int panelWidth = panelDisplay.Width;
            int panelHeight = 100; //Value in pixels

            int mapPanelWidth = 100; //Value in pixels
            int mapPanelHeight = 25; //Value in pixels
            int mapHeaderWidth = 20; //Value in %

            #region Style Parameters
            int headerWidth = 15; //Value in %
            int rowHeight = 50; //Value in pixels
            int pictureBoxSize = 15; //Value in pixels
            int pictureBoxMargin = 20; //Value in pixels
            int elementMargin = 25; //Value in pixels
            int fontSize = (int)labelSensorName.Font.Size;
            #endregion

            int count = sensorList.Count;

            panelSensorContainer = new TableLayoutPanel[count];
            labelSensor = new Label[count];
            panelSensorInformation = new Panel[count];
            pictureBoxSensorStatus = new PictureBox[count];
            labelSensorStatus = new Label[count];
            labelSensorSegment = new Label[count];

            panelMapSensorInformation = new TableLayoutPanel[count];
            pictureBoxMapSensorStatus = new PictureBox[count];
            labelMapSensor = new Label[count];

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
                pictureBoxSensorStatus[i].Location = new Point(pictureBoxMargin, panelSensorInformation[i].Height / 2 - pictureBoxSize / 2);
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
                labelSensorStatus[i].Location = new Point(pictureBoxSensorStatus[i].Location.X + pictureBoxMargin, 0);
                labelSensorStatus[i].AutoSize = false;
                labelSensorStatus[i].Width = panelSensorInformation[i].Width / 3;
                labelSensorStatus[i].Height = panelSensorInformation[i].Height;
                labelSensorStatus[i].Font = new Font(labelSensorStatus[i].Font.Name, fontSize);
                labelSensorStatus[i].TextAlign = ContentAlignment.MiddleLeft;
                labelSensorStatus[i].Text = "Status: " + sensorList[i].StateToString();
                labelSensorStatus[i].Tag = i;
                labelSensorStatus[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorStatus[i]);
                #endregion
                #region Sensor Segment Label
                labelSensorSegment[i] = new Label();
                labelSensorSegment[i].Name = "labelSegment" + i;
                labelSensorSegment[i].Location = new Point(labelSensorStatus[i].Location.X + labelSensorStatus[i].Width + elementMargin, 0);
                labelSensorSegment[i].AutoSize = false;
                labelSensorSegment[i].Width = panelSensorInformation[i].Width / 3;
                labelSensorSegment[i].Height = panelSensorInformation[i].Height;
                labelSensorSegment[i].Font = new Font(labelSensorSegment[i].Font.Name, fontSize);
                labelSensorSegment[i].TextAlign = ContentAlignment.MiddleLeft;
                labelSensorSegment[i].Text = "Segment: " + sensorList[i].SensorSegment;
                labelSensorSegment[i].Tag = i;
                labelSensorSegment[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorSegment[i]);
                #endregion

                #region Map Sensor Panel Container
                panelMapSensorInformation[i] = new TableLayoutPanel();
                panelMapSensorInformation[i].Name = "tableLayoutPanelMapInfo" + i;
                panelMapSensorInformation[i].Location = new Point(sensorList[i].SensorX, sensorList[i].SensorY);
                panelMapSensorInformation[i].Width = mapPanelWidth;
                panelMapSensorInformation[i].Height = mapPanelHeight;
                panelMapSensorInformation[i].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelMapSensorInformation[i].BackColor = SystemColors.Control;
                panelMapSensorInformation[i].ColumnCount = 2;
                panelMapSensorInformation[i].RowCount = 1;
                panelMapSensorInformation[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, mapHeaderWidth));
                panelMapSensorInformation[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - mapHeaderWidth));
                panelMapSensorInformation[i].RowStyles.Add(new RowStyle(SizeType.Absolute, mapPanelHeight));
                panelMapSensorInformation[i].Tag = i;
                panelMapSensorInformation[i].Click += sensorContainer_Click;
                pictureBoxMap.Controls.Add(panelMapSensorInformation[i]);
                #endregion
                #region Map Sensor Status Picture
                pictureBoxMapSensorStatus[i] = new PictureBox();
                pictureBoxMapSensorStatus[i].Name = "pictureBoxMapStatus" + i;
                pictureBoxMapSensorStatus[i].Location = new Point(0, 0);
                pictureBoxMapSensorStatus[i].Width = pictureBoxSize;
                pictureBoxMapSensorStatus[i].Height = pictureBoxSize;
                pictureBoxMapSensorStatus[i].Anchor = AnchorStyles.Right;
                pictureBoxMapSensorStatus[i].BackColor = Color.Lime;
                roundPictureBox(ref pictureBoxMapSensorStatus[i]);
                pictureBoxMapSensorStatus[i].Tag = i;
                pictureBoxMapSensorStatus[i].Click += sensorContainer_Click;
                panelMapSensorInformation[i].Controls.Add(pictureBoxMapSensorStatus[i]);
                #endregion
                #region Map Sensor Label
                labelMapSensor[i] = new Label();
                labelMapSensor[i].Name = "labelMapSensor" + i;
                labelMapSensor[i].Location = new Point(0, 0);
                labelMapSensor[i].AutoSize = true;
                labelMapSensor[i].Dock = DockStyle.Fill;
                labelMapSensor[i].Anchor = AnchorStyles.None;
                labelMapSensor[i].TextAlign = ContentAlignment.MiddleCenter;
                labelMapSensor[i].Text = sensorList[i].SensorName;
                labelMapSensor[i].Font = new Font(labelMapSensor[i].Font.Name, fontSize);
                labelMapSensor[i].Tag = i;
                labelMapSensor[i].Click += sensorContainer_Click;
                panelMapSensorInformation[i].Controls.Add(labelMapSensor[i]);
                #endregion


                panelY += rowHeight * 2;
            }
        }
        private void initializeOptions()
        {
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
        }
        private void initializeSideScroll()
        {
            panelDisplay.AutoScroll = false;
            panelDisplay.HorizontalScroll.Enabled = false;
            panelDisplay.HorizontalScroll.Visible = false;
            panelDisplay.AutoScroll = true;
        }
        private void initializeMapStateDisplay()
        {
            pictureBoxMapState1.BackColor = Color.Lime;
            pictureBoxMapState1.Width = pictureBoxMapState1.Height;
            roundPictureBox(ref pictureBoxMapState1);

            pictureBoxMapState2.BackColor = Color.RoyalBlue;
            pictureBoxMapState2.Width = pictureBoxMapState1.Width;
            pictureBoxMapState2.Height = pictureBoxMapState1.Height;
            roundPictureBox(ref pictureBoxMapState2);

            pictureBoxMapState3.BackColor = Color.Orange;
            pictureBoxMapState3.Width = pictureBoxMapState1.Width;
            pictureBoxMapState3.Height = pictureBoxMapState1.Height;
            roundPictureBox(ref pictureBoxMapState3);

            pictureBoxMapState4.BackColor = Color.Red;
            pictureBoxMapState4.Width = pictureBoxMapState1.Width;
            pictureBoxMapState4.Height = pictureBoxMapState1.Height;
            roundPictureBox(ref pictureBoxMapState4);
        }
        #endregion
        #region Utilities
        private void updateColors()
        {
            int count = sensorList.Count;
            for (int i = 0; i < count; i++)
            {
                pictureBoxSensorStatus[i].BackColor = sensorList[i].StateToColor();
                pictureBoxMapSensorStatus[i].BackColor = sensorList[i].StateToColor();
            }
        }
        private void roundPictureBox(ref PictureBox pb)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pb.Width - 3, pb.Height - 3);
            Region rg = new Region(gp);
            pb.Region = rg;
        }
        private int[] getStateCounts()
        {
            int stateActive = 0; //0
            int stateInactive = 0; //1
            int stateAlert = 0; //2
            int stateFire = 0; //3
            foreach (Sensor sensor in sensorList)
            {
                switch (sensor.CurrentSensorState)
                {
                    case Sensor.SensorState.Active:
                        stateActive++;
                        break;

                    case Sensor.SensorState.Inactive:
                        stateInactive++;
                        break;

                    case Sensor.SensorState.Alert:
                        stateAlert++;
                        break;

                    case Sensor.SensorState.Fire:
                        stateFire++;
                        break;

                    default:
                        stateInactive++;
                        break;
                }
            }
            return new int[] { stateActive, stateInactive, stateAlert, stateFire };
        }
        private void updateStateCounts()
        {
            int[] stateCounts = getStateCounts();
            labelMapStateCount1.Text = stateCounts[0].ToString();
            labelMapStateCount2.Text = stateCounts[1].ToString();
            labelMapStateCount3.Text = stateCounts[2].ToString();
            labelMapStateCount4.Text = stateCounts[3].ToString();

        }
        private void updateInfoPanel()
        {
            panelSensorContainer[currentSensorIndex].BackColor = Color.SkyBlue;
            panelMapSensorInformation[currentSensorIndex].BackColor = Color.SkyBlue;
            labelSensorName.Text = sensorList[currentSensorIndex].SensorName;
            labelStateInfo.Text = sensorList[currentSensorIndex].StateToString();
            labelSegmentInfo.Text = sensorList[currentSensorIndex].SensorSegment;
            labelLocationInfo.Text = sensorList[currentSensorIndex].SensorLocation;
            labelTemperatureInfo.Text = sensorList[currentSensorIndex].SensorTemperature.ToString() + "°C";
            labelLastUpdateInfo.Text = sensorList[currentSensorIndex].SensorLastUpdate.ToString() + " sekund temu.";
            try
            {
                Image cameraImage = Image.FromFile("../../../sensors/" + sensorList[currentSensorIndex].SensorDetails);
                pictureBoxCamera.Image = cameraImage;
            }
            catch (FileNotFoundException)
            {
                pictureBoxCamera.Image = pictureBoxCamera.ErrorImage;
            }
            switch (sensorList[currentSensorIndex].CurrentSensorState)
            {
                case Sensor.SensorState.Alert:
                    buttonFire.BackColor = Color.Red;
                    buttonFire.ForeColor = Color.White;
                    buttonFire.Enabled = true;
                    buttonFire.Text = "PotwierdŸ po¿ar";
                    break;

                case Sensor.SensorState.Fire:
                    buttonFire.BackColor = Color.Red;
                    buttonFire.ForeColor = Color.White;
                    buttonFire.Enabled = true;
                    buttonFire.Text = "PotwierdŸ zwalczenie po¿aru";
                    break;

                default:
                    buttonFire.BackColor = SystemColors.Control;
                    buttonFire.ForeColor = SystemColors.ControlText;
                    buttonFire.Enabled = false;
                    buttonFire.Text = "Brak problemów";
                    break;
            }
        }
        private void updateAll() //Updates all controls to match current data. Does not change data itself.
        {
            updateColors();
            updateStateCounts();
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(tcpClient != null)
            {
                tcpClient.Close();
            }
        }
        #endregion
        #region Server Connection
        private void serverRequest(UserRequest.RequestType request)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, port);
                switch (request)
                {
                    case UserRequest.RequestType.AllHostDevicesAsJson:
                        serverRequestInitialize();
                        break;

                    case UserRequest.RequestType.SingleHostDeviceAsJson:
                        serverRequestUpdate();
                        break;
                }
                tcpClient.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nie uda³o siê nawi¹zaæ po³¹czenia z serwerem ("+ipAddress+": "+port+"). "+ex.Message, "B³¹d po³¹czenia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void serverRequestInitialize()
        {
            var listener = new ClientListener(IPAddress.Parse(ipAddress), 12000);
            listener.OnSignalReceived += (sender, e) =>
            {
                Console.WriteLine(ZPIEncoding.Decode<CameraDataMessage>(e));
            };
            var request = new UserRequest()
            {
                Request = UserRequest.RequestType.AllHostDevicesAsJson
            };
            /*
            var request = new UserRequest()
            {
                Request = UserRequest.RequestType.CameraDataAsJson,
                ModelObjectId = 1
            };
            */
            using (var stream = tcpClient.GetStream())
            {
                byte[] buffer = ZPIEncoding.Encode(request);
                stream.Write(buffer);

                /*
                string message = "";
                foreach(char b in buffer)
                {
                    message += b;
                }
                MessageBox.Show(message);
                */

            }
        }
        private void serverRequestUpdate()
        {

        }
        #endregion
        #region Debug
        private void panelMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (debug)
            {
                buttonDebug.Text = "X: " + e.Location.X.ToString() + " Y: " + e.Location.Y.ToString();
            }
        }
        #endregion
    }
}