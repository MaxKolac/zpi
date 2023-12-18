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
using Windows.ApplicationModel.Background;
using System.Runtime;
using System.Drawing.Imaging;

namespace ZPIClient
{
    public partial class FormMain : Form
    {
        //Connection settings
        private string clientIP;
        private string serverIP;
        private int port = 25566;
        private TcpClient tcpClient;
        private List<HostDevice> devices;
        private ClientListener listener;
        private TaskCompletionSource<bool> signalReceivedTaskCompletionSource;

        //Sensor variables
        private List<HostDevice> sensorList = new List<HostDevice>();
        private List<int> sensorTimerList = new List<int>();
        private int currentSensorIndex = -1;

        //Thermal image display
        private bool isThermal = true;

        //Timer settings
        private int timerInterval = 30, timerElapsedTime = 30;

        //Debug
        private bool debug = false;

        //Dynamic objects
        TableLayoutPanel[] panelSensorContainer; //List
        Label[] labelSensor;
        Panel[] panelSensorInformation;
        PictureBox[] pictureBoxSensorStatus;
        Label[] labelSensorStatus;
        Label[] labelSensorSegment;

        TableLayoutPanel[] panelMapSensorInformation; //Map
        Label[] labelMapSensor;
        PictureBox[] pictureBoxMapSensorStatus;

        public FormMain()
        {
            InitializeIP();
            InitializeComponent();
            Initialize();
        }

        #region Timer Functions
        private void timerRefresh_Tick(object sender, EventArgs e) //Funkcja wywoływana co każdy "tick" (1 sekundę) zegara. Zwiększa wartość każdego widocznego zegara, oraz wywołuje updateSensors co 30 sekund
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
                labelLastUpdateInfo.Text = sensorTimerList[currentSensorIndex].ToString() + " sekund temu";
            }
            labelTimer.Text = "Następna aktualizacja za: " + timerElapsedTime + " sekund";
        }
        private void incrementTimers() //Zwiększa wartość zegara wszystkich czujników. Niezbędne do wyświetlenia komórki "Ostatnia aktualizacja"
        {
            int count = sensorTimerList.Count;
            for (int i = 0; i < count; i++)
            {
                sensorTimerList[i] += 1;
            }
        }
        private void updateSensors() //Funkcja wywoływana co 30 sekund. Pobiera nowe dane od serwera oraz (jeśli się zmieniły) je aktualizuje
        {
            serverRequest(UserRequest.RequestType.SingleHostDeviceAsJson);
            foreach (HostDevice device in devices)
            {
                if (device.Type == HostDevice.HostType.CameraSimulator || device.Type == HostDevice.HostType.PythonCameraSimulator)
                {
                    int index = sensorList.FindIndex(a => a.Id == device.Id);
                    if (sensorList[index].LastFireStatus != device.LastFireStatus || sensorList[index].LastKnownTemperature != device.LastKnownTemperature)
                    {
                        sensorList[index] = device;
                        sensorTimerList[index] = 0;
                    }
                }
            }
            updateAll();
        }
        #endregion
        #region Buttons
        private void buttonDebug_Click(object sender, EventArgs e) //Przycisk włącza pokyzwanie współrzędnych kursora na mapie (debug) przez panelMap_MouseMove
        {
            if (debug)
            {
                buttonDebug.BackColor = SystemColors.Control;
                buttonDebug.Text = "Debug";
            }
            else
            {
                buttonDebug.Text = "Skaner współrzędnych aktywny";
                buttonDebug.BackColor = Color.SkyBlue;
            }
            debug = !debug;

        }
        private void buttonSwitch_Click(object sender, EventArgs e) //Przycisk przełącza widoczność między mapą a listą
        {
            panelDisplay.Visible = !panelDisplay.Visible;
            panelDisplay.Enabled = !panelDisplay.Enabled;
            panelMap.Visible = !panelMap.Visible;
            panelMap.Enabled = !panelMap.Enabled;
        }
        private async void buttonOverview_Click(object sender, EventArgs e) //Przycisk przełącza tryb wyświetlania zdjęć między termicznymi a zwykłymi
        {
            isThermal = !isThermal;
            buttonOverview.Text = new string("Tryb termiczny: " + isThermal.ToString());
            if (currentSensorIndex != -1)
            {
                updateCurrentImage();
            }
        }
        private void sensorContainer_Click(object sender, EventArgs e) //Zawartość tej funkcji jest przypięta do każdego obiektu widoku listy. Pełni rolę przycisku
        {
            if (currentSensorIndex != -1)
            {
                panelSensorContainer[currentSensorIndex].BackColor = SystemColors.ControlLightLight;
                panelMapSensorInformation[currentSensorIndex].BackColor = SystemColors.Control;
            }
            Control control = (Control)sender;
            currentSensorIndex = (int)control.Tag;
            updateInfoPanel();
        } 
        private void buttonFire_Click(object sender, EventArgs e) //Funkcja potwierdza wystąpienie/zwalczenie pożaru dla danej kamery oraz wysyła zapytanie o zmianę stanu do serwera
        {
            if (currentSensorIndex != -1)
            {
                if (sensorList[currentSensorIndex].LastFireStatus == HostDevice.FireStatus.Suspected)
                {
                    sensorList[currentSensorIndex].LastFireStatus = HostDevice.FireStatus.Confirmed;
                }
                else
                {
                    sensorList[currentSensorIndex].LastFireStatus = HostDevice.FireStatus.Suspected;
                }
                serverRequest(UserRequest.RequestType.UpdateFireStatusFromJson);
                updateAll();
            }
        }
        #endregion
        #region Initialize Functions
        private void InitializeIP() //Inicjalizacja adresu IP z okna dialogowego
        {
            try
            {
                var result = Prompt.ShowDialog("Wprowadź adres IP komputera", "Wprowadź adres IP serwera", "Nawiązywanie połączenia");
                clientIP = result.Item1;
                serverIP = result.Item2;
                listener = new ClientListener(IPAddress.Parse(serverIP), 12000);
            }catch(Exception ex)
            {
                MessageBox.Show("Wprowadzono nieprawidłowy adres IP (" + clientIP + ": " + port + "). " + ex.Message, "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                clientIP = "127.0.0.1";
                try
                {
                    serverIP = "127.0.0.1";
                    listener = new ClientListener(IPAddress.Parse("127.0.0.1"), 12000);
                }catch(Exception exc)
                {
                    MessageBox.Show(exc.Message, "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }
        }
        private void InitializeSensors() //Inicjalizacja listy czujników z danych od serwera (wewnątrz zmiennej)
        {
            if (devices != null)
            {
                foreach (HostDevice device in devices)
                {
                    if (device.Type == HostDevice.HostType.PythonCameraSimulator || device.Type == HostDevice.HostType.CameraSimulator)
                    {
                        sensorList.Add(device);
                        sensorTimerList.Add(0);
                    }
                }
            }
        }
        private void InitializeListFormControls() //Generowanie klilalnej listy czujników z utworzonej wcześniej listy
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
                labelSensor[i].Text = sensorList[i].Name;
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
                labelSensorSegment[i].Text = "Segment: " + sensorList[i].SectorId;
                labelSensorSegment[i].Tag = i;
                labelSensorSegment[i].Click += sensorContainer_Click;
                panelSensorInformation[i].Controls.Add(labelSensorSegment[i]);
                #endregion

                #region Map Sensor Panel Container
                panelMapSensorInformation[i] = new TableLayoutPanel();
                panelMapSensorInformation[i].Name = "tableLayoutPanelMapInfo" + i;
                panelMapSensorInformation[i].Location = new Point((int)sensorList[i].LocationLatitude, (int)sensorList[i].LocationAltitude);
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
                labelMapSensor[i].Text = sensorList[i].Name;
                labelMapSensor[i].Font = new Font(labelMapSensor[i].Font.Name, fontSize);
                labelMapSensor[i].Tag = i;
                labelMapSensor[i].Click += sensorContainer_Click;
                panelMapSensorInformation[i].Controls.Add(labelMapSensor[i]);
                #endregion


                panelY += rowHeight * 2;
            }
        }
        private void InitializeSideScroll() //Modyfikacja parametrów panelu listy aby odpowiadała specyfikacji
        {
            panelDisplay.AutoScroll = false;
            panelDisplay.HorizontalScroll.Enabled = false;
            panelDisplay.HorizontalScroll.Visible = false;
            panelDisplay.AutoScroll = true;
        }
        private void InitializeMapStateDisplay() //Inicjalizacja panelu stanów (widok mapy)
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
        private async void Initialize() //Połączenie wszystkich funkcji initialize w jedną całość. Funkcja uruchamia również timer
        {
            await serverRequest(UserRequest.RequestType.AllHostDevicesAsJson);
            InitializeSensors();
            InitializeListFormControls();
            InitializeSideScroll();
            InitializeMapStateDisplay();
            updateAll();
            timerRefresh.Start();
        }
        #endregion
        #region Utilities
        private void updateColors() //Aktualizacja kolorów dla wszystkich "diod" przy czujnikach
        {
            int count = sensorList.Count;
            for (int i = 0; i < count; i++)
            {
                pictureBoxSensorStatus[i].BackColor = sensorList[i].StateToColor();
                pictureBoxMapSensorStatus[i].BackColor = sensorList[i].StateToColor();
            }
        }
        private int[] getStateCounts() //Funkcja pomocnicza. Tworzy listę aktualnych stanów czujników
        {
            int stateActive = 0; //0
            int stateInactive = 0; //1
            int stateAlert = 0; //2
            int stateFire = 0; //3
            foreach (HostDevice sensor in sensorList)
            {
                switch (sensor.LastFireStatus)
                {
                    case HostDevice.FireStatus.OK:
                        stateActive++;
                        break;

                    case HostDevice.FireStatus.Suspected:
                        stateAlert++;
                        break;

                    case HostDevice.FireStatus.Confirmed:
                        stateFire++;
                        break;

                    default:
                        stateInactive++;
                        break;
                }
            }
            return new int[] { stateActive, stateInactive, stateAlert, stateFire };
        }
        private void updateStateCounts() //Aktualizacja tekstu panelu stanów (widok mapy)
        {
            int[] stateCounts = getStateCounts();
            labelMapStateCount1.Text = stateCounts[0].ToString();
            labelMapStateCount2.Text = stateCounts[1].ToString();
            labelMapStateCount3.Text = stateCounts[2].ToString();
            labelMapStateCount4.Text = stateCounts[3].ToString();
        }
        private void updateInfoPanel() //Aktualizacja wszystkich danych wewnątrz panelu bocznego. Wywoływana co każdą aktualizację oraz kliknięcie na czujnik
        {
            if (currentSensorIndex != -1)
            {
                panelSensorContainer[currentSensorIndex].BackColor = Color.SkyBlue;
                panelMapSensorInformation[currentSensorIndex].BackColor = Color.SkyBlue;
                labelSensorName.Text = sensorList[currentSensorIndex].Name;
                labelStateInfo.Text = sensorList[currentSensorIndex].StateToString();
                labelSegmentInfo.Text = sensorList[currentSensorIndex].SectorId.ToString();
                labelLocationInfo.Text = sensorList[currentSensorIndex].LocationDescription;
                labelTemperatureInfo.Text = sensorList[currentSensorIndex].LastKnownTemperature.ToString() + "°C";
                labelLastUpdateInfo.Text = sensorTimerList[currentSensorIndex].ToString() + " sekund temu.";
                try
                {
                    updateCurrentImage();
                }
                catch (Exception)
                {
                    pictureBoxCamera.Image = pictureBoxCamera.ErrorImage;
                }
                switch (sensorList[currentSensorIndex].LastFireStatus)
                {
                    case HostDevice.FireStatus.Suspected:
                        buttonFire.BackColor = Color.Red;
                        buttonFire.ForeColor = Color.White;
                        buttonFire.Enabled = true;
                        buttonFire.Text = "Potwierdź pożar";
                        break;

                    case HostDevice.FireStatus.Confirmed:
                        buttonFire.BackColor = Color.Red;
                        buttonFire.ForeColor = Color.White;
                        buttonFire.Enabled = true;
                        buttonFire.Text = "Potwierdź zwalczenie pożaru";
                        break;

                    default:
                        buttonFire.BackColor = SystemColors.Control;
                        buttonFire.ForeColor = SystemColors.ControlText;
                        buttonFire.Enabled = false;
                        buttonFire.Text = "Brak problemów";
                        break;
                }
            }
        }
        private void updateStateList() //Aktualizacja tekstu przy statusie każdego czujnika (widok listy) 
        {
            int count = sensorList.Count;
            for (int i = 0; i < count; i++)
            {
                labelSensorStatus[i].Text = "Status: " + sensorList[i].StateToString();
            }
        }
        private void updateAll() //Połączenie wszystkich funkcji update w jedną całość.
        {
            updateColors();
            updateStateCounts();
            updateInfoPanel();
            updateStateList();
        }
        private async void updateCurrentImage() //Aktualizacja obecnego obrazka widoku kamery (tryb termiczny/zwykły)
        {
            buttonOverview.Enabled = false; //Failsafe

            Image image = null;
            while (image == null)
            {
                if (!isThermal)
                {
                    image = await convertThermalImage(sensorList[currentSensorIndex].LastImage);
                }
                else
                {
                    image = HostDevice.ToImage(sensorList[currentSensorIndex].LastImage);
                }
                await Task.Delay(100);
            }
            pictureBoxCamera.Image = image;
            pictureBoxCamera.Refresh();

            buttonOverview.Enabled = true;
        }
        private async Task<Image> convertThermalImage(byte[] imageBytes) //Funkcja pomocnicza konwertująca obrazek termiczny wybranego czujnika w obrazek zwykły
        {
            string path = Environment.CurrentDirectory;
            string filename = "temp.jpg";

            using (var stream = File.Create(Path.Combine(path, filename)))
            {
                stream.Write(imageBytes);
            };
            var realImage = ImageExtracter.GetEmbeddedImage(path, filename);
            File.Delete(Path.Combine(path, filename));

            return realImage;
        }
        private void roundPictureBox(ref PictureBox pb) //Funkcja pomocnicza. Zmienia kwadratowy picture box w kółko. Odpowiada za wygląd "diód" przy czujnikach.
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pb.Width - 3, pb.Height - 3);
            Region rg = new Region(gp);
            pb.Region = rg;
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) //Funkcja wywoływana przy zamknięciu programu. Upewnia się, że połączenie tcp z serwerem zostało zamknięte
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
        #endregion
        #region Server Connection
        private async Task serverRequest(UserRequest.RequestType request) //Funkcja tworzy połączenie z serwerem, a następnie wysyła jedno z dostępnych zapytań
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(clientIP, port);
                switch (request)
                {
                    case UserRequest.RequestType.AllHostDevicesAsJson:
                        await serverRequestInitialize();
                        break;

                    case UserRequest.RequestType.SingleHostDeviceAsJson:
                        await serverRequestUpdate();
                        break;

                    case UserRequest.RequestType.UpdateFireStatusFromJson:
                        await serverRequestStatusChange();
                        break;
                }
                await signalReceivedTaskCompletionSource.Task; //Client będzie czekał na odpowiedź od serwera
                tcpClient.Close();
            }
            catch (Exception ex) //Jeżeli serverRequest nie połączy się z serwerem, zostanie wyświetlone okienko z kodem błędu
            {
                MessageBox.Show("Nie udało się nawiązać połączenia z serwerem (" + serverIP + ": " + port + "). " + ex.Message, "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private async Task serverRequestInitialize() //Zapytanie "populuje" listę czujników na których będzie operował client
        {
            signalReceivedTaskCompletionSource = new TaskCompletionSource<bool>();
            listener.OnSignalReceived += (sender, e) =>
            {
                devices = ZPIEncoding.Decode<List<HostDevice>>(e);
                signalReceivedTaskCompletionSource.TrySetResult(true);
            };

            var request = new UserRequest()
            {
                Request = UserRequest.RequestType.AllHostDevicesAsJson
            };

            using (var stream = tcpClient.GetStream())
            {
                byte[] buffer = ZPIEncoding.Encode(request);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        private async Task serverRequestUpdate() //Zapytanie pobiera dane czujników, które zostały pobrane podczas inicjalizacji
        {
            signalReceivedTaskCompletionSource = new TaskCompletionSource<bool>();
            listener.OnSignalReceived += (sender, e) =>
            {
                devices = ZPIEncoding.Decode<List<HostDevice>>(e);
                signalReceivedTaskCompletionSource.TrySetResult(true);
            };

            var request = new UserRequest()
            {
                Request = UserRequest.RequestType.AllHostDevicesAsJson
            };

            using (var stream = tcpClient.GetStream())
            {
                byte[] buffer = ZPIEncoding.Encode(request);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        private async Task serverRequestStatusChange() //Zapytanie zmienia stan czujnika o wskazanym indeksie w bazie danych
        {
            signalReceivedTaskCompletionSource = new TaskCompletionSource<bool>();
            var request = new UserRequest()
            {
                Request = UserRequest.RequestType.UpdateFireStatusFromJson,
                ModelObjectId = sensorList[currentSensorIndex].Id,
                NewStatus = sensorList[currentSensorIndex].LastFireStatus
            };

            using (var stream = tcpClient.GetStream())
            {
                byte[] buffer = ZPIEncoding.Encode(request);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            signalReceivedTaskCompletionSource.TrySetResult(true);
        }
        #endregion
        #region Debug
        private void panelMap_MouseMove(object sender, MouseEventArgs e) //Funkcja pomocnicza trybu debug. Pokazuje aktualne współrzędne kursora dla trybu mapy
        {
            if (debug)
            {
                buttonDebug.Text = "X: " + e.Location.X.ToString() + " Y: " + e.Location.Y.ToString();
            }
        }
        #endregion
    }
}