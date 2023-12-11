namespace ZPIClient
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            panelDisplay = new Panel();
            panelNavigation = new Panel();
            buttonOverview = new Button();
            buttonDebug = new Button();
            buttonSwitch = new Button();
            panelInfo = new Panel();
            buttonFire = new Button();
            panelCamera = new Panel();
            pictureBoxCamera = new PictureBox();
            tableLayoutPanel = new TableLayoutPanel();
            labelState = new Label();
            labelStateInfo = new Label();
            labelSegment = new Label();
            labelSegmentInfo = new Label();
            labelLocation = new Label();
            labelLocationInfo = new Label();
            labelTemperature = new Label();
            labelTemperatureInfo = new Label();
            labelLastUpdate = new Label();
            labelLastUpdateInfo = new Label();
            panelSensorName = new Panel();
            labelSensorName = new Label();
            panelTimer = new Panel();
            labelTimer = new Label();
            timerRefresh = new System.Windows.Forms.Timer(components);
            panelMap = new Panel();
            tableLayoutPanelSummary = new TableLayoutPanel();
            labelMapStateCount4 = new Label();
            labelMapState4 = new Label();
            pictureBoxMapState4 = new PictureBox();
            labelMapStateCount3 = new Label();
            labelMapState3 = new Label();
            pictureBoxMapState3 = new PictureBox();
            labelMapStateCount2 = new Label();
            labelMapState2 = new Label();
            pictureBoxMapState2 = new PictureBox();
            labelMapStateCount1 = new Label();
            pictureBoxMapState1 = new PictureBox();
            labelMapState1 = new Label();
            pictureBoxMap = new PictureBox();
            panelNavigation.SuspendLayout();
            panelInfo.SuspendLayout();
            panelCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamera).BeginInit();
            tableLayoutPanel.SuspendLayout();
            panelSensorName.SuspendLayout();
            panelTimer.SuspendLayout();
            panelMap.SuspendLayout();
            tableLayoutPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMap).BeginInit();
            SuspendLayout();
            // 
            // panelDisplay
            // 
            panelDisplay.AutoScroll = true;
            panelDisplay.BackColor = SystemColors.Control;
            panelDisplay.BorderStyle = BorderStyle.FixedSingle;
            panelDisplay.Location = new Point(0, 0);
            panelDisplay.Name = "panelDisplay";
            panelDisplay.Size = new Size(1018, 652);
            panelDisplay.TabIndex = 0;
            // 
            // panelNavigation
            // 
            panelNavigation.BorderStyle = BorderStyle.FixedSingle;
            panelNavigation.Controls.Add(buttonOverview);
            panelNavigation.Controls.Add(buttonDebug);
            panelNavigation.Controls.Add(buttonSwitch);
            panelNavigation.Location = new Point(0, 649);
            panelNavigation.Name = "panelNavigation";
            panelNavigation.Size = new Size(1018, 80);
            panelNavigation.TabIndex = 1;
            // 
            // buttonOverview
            // 
            buttonOverview.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonOverview.Location = new Point(686, 9);
            buttonOverview.Name = "buttonOverview";
            buttonOverview.Size = new Size(324, 59);
            buttonOverview.TabIndex = 3;
            buttonOverview.Text = "Tryb termiczny: True";
            buttonOverview.UseVisualStyleBackColor = true;
            buttonOverview.Click += buttonOverview_Click;
            // 
            // buttonDebug
            // 
            buttonDebug.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonDebug.Location = new Point(12, 9);
            buttonDebug.Name = "buttonDebug";
            buttonDebug.Size = new Size(324, 59);
            buttonDebug.TabIndex = 1;
            buttonDebug.Text = "Debug";
            buttonDebug.UseVisualStyleBackColor = true;
            buttonDebug.Click += buttonDebug_Click;
            // 
            // buttonSwitch
            // 
            buttonSwitch.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonSwitch.Location = new Point(348, 9);
            buttonSwitch.Name = "buttonSwitch";
            buttonSwitch.Size = new Size(324, 59);
            buttonSwitch.TabIndex = 2;
            buttonSwitch.Text = "Przełącz widok";
            buttonSwitch.UseVisualStyleBackColor = true;
            buttonSwitch.Click += buttonSwitch_Click;
            // 
            // panelInfo
            // 
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.Controls.Add(buttonFire);
            panelInfo.Controls.Add(panelCamera);
            panelInfo.Controls.Add(tableLayoutPanel);
            panelInfo.Controls.Add(panelSensorName);
            panelInfo.Location = new Point(1016, 0);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(341, 651);
            panelInfo.TabIndex = 2;
            // 
            // buttonFire
            // 
            buttonFire.Enabled = false;
            buttonFire.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonFire.Location = new Point(7, 619);
            buttonFire.Name = "buttonFire";
            buttonFire.Size = new Size(314, 23);
            buttonFire.TabIndex = 0;
            buttonFire.Text = "Potwierdź";
            buttonFire.UseVisualStyleBackColor = true;
            buttonFire.Click += buttonFire_Click;
            // 
            // panelCamera
            // 
            panelCamera.BackColor = SystemColors.ControlLightLight;
            panelCamera.BorderStyle = BorderStyle.FixedSingle;
            panelCamera.Controls.Add(pictureBoxCamera);
            panelCamera.Location = new Point(7, 299);
            panelCamera.Name = "panelCamera";
            panelCamera.Size = new Size(314, 314);
            panelCamera.TabIndex = 0;
            // 
            // pictureBoxCamera
            // 
            pictureBoxCamera.BackColor = SystemColors.ControlDark;
            pictureBoxCamera.Location = new Point(3, 3);
            pictureBoxCamera.Name = "pictureBoxCamera";
            pictureBoxCamera.Size = new Size(306, 306);
            pictureBoxCamera.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxCamera.TabIndex = 0;
            pictureBoxCamera.TabStop = false;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.BackColor = SystemColors.ControlLightLight;
            tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayoutPanel.Controls.Add(labelState, 0, 0);
            tableLayoutPanel.Controls.Add(labelStateInfo, 1, 0);
            tableLayoutPanel.Controls.Add(labelSegment, 0, 1);
            tableLayoutPanel.Controls.Add(labelSegmentInfo, 1, 1);
            tableLayoutPanel.Controls.Add(labelLocation, 0, 2);
            tableLayoutPanel.Controls.Add(labelLocationInfo, 1, 2);
            tableLayoutPanel.Controls.Add(labelTemperature, 0, 3);
            tableLayoutPanel.Controls.Add(labelTemperatureInfo, 1, 3);
            tableLayoutPanel.Controls.Add(labelLastUpdate, 0, 4);
            tableLayoutPanel.Controls.Add(labelLastUpdateInfo, 1, 4);
            tableLayoutPanel.Location = new Point(7, 53);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.Size = new Size(314, 240);
            tableLayoutPanel.TabIndex = 0;
            // 
            // labelState
            // 
            labelState.Dock = DockStyle.Fill;
            labelState.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelState.Location = new Point(4, 1);
            labelState.Name = "labelState";
            labelState.Size = new Size(87, 46);
            labelState.TabIndex = 0;
            labelState.Text = "Stan";
            labelState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelStateInfo
            // 
            labelStateInfo.Dock = DockStyle.Fill;
            labelStateInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelStateInfo.Location = new Point(98, 1);
            labelStateInfo.Name = "labelStateInfo";
            labelStateInfo.Size = new Size(212, 46);
            labelStateInfo.TabIndex = 1;
            labelStateInfo.Text = "-";
            labelStateInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelSegment
            // 
            labelSegment.Dock = DockStyle.Fill;
            labelSegment.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelSegment.Location = new Point(4, 48);
            labelSegment.Name = "labelSegment";
            labelSegment.Size = new Size(87, 46);
            labelSegment.TabIndex = 2;
            labelSegment.Text = "Segment";
            labelSegment.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelSegmentInfo
            // 
            labelSegmentInfo.Dock = DockStyle.Fill;
            labelSegmentInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelSegmentInfo.Location = new Point(98, 48);
            labelSegmentInfo.Name = "labelSegmentInfo";
            labelSegmentInfo.Size = new Size(212, 46);
            labelSegmentInfo.TabIndex = 3;
            labelSegmentInfo.Text = "-";
            labelSegmentInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelLocation
            // 
            labelLocation.Dock = DockStyle.Fill;
            labelLocation.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelLocation.Location = new Point(4, 95);
            labelLocation.Name = "labelLocation";
            labelLocation.Size = new Size(87, 46);
            labelLocation.TabIndex = 4;
            labelLocation.Text = "Lokalizacja";
            labelLocation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelLocationInfo
            // 
            labelLocationInfo.Dock = DockStyle.Fill;
            labelLocationInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelLocationInfo.Location = new Point(98, 95);
            labelLocationInfo.Name = "labelLocationInfo";
            labelLocationInfo.Size = new Size(212, 46);
            labelLocationInfo.TabIndex = 5;
            labelLocationInfo.Text = "-";
            labelLocationInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTemperature
            // 
            labelTemperature.Dock = DockStyle.Fill;
            labelTemperature.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelTemperature.Location = new Point(4, 142);
            labelTemperature.Name = "labelTemperature";
            labelTemperature.Size = new Size(87, 46);
            labelTemperature.TabIndex = 6;
            labelTemperature.Text = "Temperatura";
            labelTemperature.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTemperatureInfo
            // 
            labelTemperatureInfo.Dock = DockStyle.Fill;
            labelTemperatureInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelTemperatureInfo.Location = new Point(98, 142);
            labelTemperatureInfo.Name = "labelTemperatureInfo";
            labelTemperatureInfo.Size = new Size(212, 46);
            labelTemperatureInfo.TabIndex = 7;
            labelTemperatureInfo.Text = "-";
            labelTemperatureInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelLastUpdate
            // 
            labelLastUpdate.Dock = DockStyle.Fill;
            labelLastUpdate.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelLastUpdate.Location = new Point(4, 189);
            labelLastUpdate.Name = "labelLastUpdate";
            labelLastUpdate.Size = new Size(87, 50);
            labelLastUpdate.TabIndex = 8;
            labelLastUpdate.Text = "Ostatnia aktualizacja";
            labelLastUpdate.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelLastUpdateInfo
            // 
            labelLastUpdateInfo.Dock = DockStyle.Fill;
            labelLastUpdateInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelLastUpdateInfo.Location = new Point(98, 189);
            labelLastUpdateInfo.Name = "labelLastUpdateInfo";
            labelLastUpdateInfo.Size = new Size(212, 50);
            labelLastUpdateInfo.TabIndex = 9;
            labelLastUpdateInfo.Text = "-";
            labelLastUpdateInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelSensorName
            // 
            panelSensorName.BackColor = SystemColors.ControlLightLight;
            panelSensorName.BorderStyle = BorderStyle.FixedSingle;
            panelSensorName.Controls.Add(labelSensorName);
            panelSensorName.Location = new Point(7, 3);
            panelSensorName.Name = "panelSensorName";
            panelSensorName.Size = new Size(314, 44);
            panelSensorName.TabIndex = 0;
            // 
            // labelSensorName
            // 
            labelSensorName.Dock = DockStyle.Fill;
            labelSensorName.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelSensorName.Location = new Point(0, 0);
            labelSensorName.Name = "labelSensorName";
            labelSensorName.Size = new Size(312, 42);
            labelSensorName.TabIndex = 0;
            labelSensorName.Text = "Czujnik";
            labelSensorName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelTimer
            // 
            panelTimer.BorderStyle = BorderStyle.FixedSingle;
            panelTimer.Controls.Add(labelTimer);
            panelTimer.Location = new Point(1016, 649);
            panelTimer.Name = "panelTimer";
            panelTimer.Size = new Size(341, 80);
            panelTimer.TabIndex = 3;
            // 
            // labelTimer
            // 
            labelTimer.Dock = DockStyle.Fill;
            labelTimer.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            labelTimer.Location = new Point(0, 0);
            labelTimer.Name = "labelTimer";
            labelTimer.Size = new Size(339, 78);
            labelTimer.TabIndex = 0;
            labelTimer.Text = "Następna aktualizacja: 30";
            labelTimer.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 1000;
            timerRefresh.Tick += timerRefresh_Tick;
            // 
            // panelMap
            // 
            panelMap.BackColor = SystemColors.ControlDark;
            panelMap.BackgroundImageLayout = ImageLayout.Stretch;
            panelMap.BorderStyle = BorderStyle.FixedSingle;
            panelMap.Controls.Add(tableLayoutPanelSummary);
            panelMap.Controls.Add(pictureBoxMap);
            panelMap.Enabled = false;
            panelMap.Location = new Point(0, 0);
            panelMap.Name = "panelMap";
            panelMap.Size = new Size(1017, 650);
            panelMap.TabIndex = 0;
            panelMap.Visible = false;
            // 
            // tableLayoutPanelSummary
            // 
            tableLayoutPanelSummary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tableLayoutPanelSummary.AutoSize = true;
            tableLayoutPanelSummary.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanelSummary.BackColor = SystemColors.Control;
            tableLayoutPanelSummary.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanelSummary.ColumnCount = 3;
            tableLayoutPanelSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanelSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tableLayoutPanelSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanelSummary.Controls.Add(labelMapStateCount4, 2, 3);
            tableLayoutPanelSummary.Controls.Add(labelMapState4, 1, 3);
            tableLayoutPanelSummary.Controls.Add(pictureBoxMapState4, 0, 3);
            tableLayoutPanelSummary.Controls.Add(labelMapStateCount3, 2, 2);
            tableLayoutPanelSummary.Controls.Add(labelMapState3, 1, 2);
            tableLayoutPanelSummary.Controls.Add(pictureBoxMapState3, 0, 2);
            tableLayoutPanelSummary.Controls.Add(labelMapStateCount2, 2, 1);
            tableLayoutPanelSummary.Controls.Add(labelMapState2, 1, 1);
            tableLayoutPanelSummary.Controls.Add(pictureBoxMapState2, 0, 1);
            tableLayoutPanelSummary.Controls.Add(labelMapStateCount1, 2, 0);
            tableLayoutPanelSummary.Controls.Add(pictureBoxMapState1, 0, 0);
            tableLayoutPanelSummary.Controls.Add(labelMapState1, 1, 0);
            tableLayoutPanelSummary.Location = new Point(736, 0);
            tableLayoutPanelSummary.Name = "tableLayoutPanelSummary";
            tableLayoutPanelSummary.RowCount = 4;
            tableLayoutPanelSummary.RowStyles.Add(new RowStyle());
            tableLayoutPanelSummary.RowStyles.Add(new RowStyle());
            tableLayoutPanelSummary.RowStyles.Add(new RowStyle());
            tableLayoutPanelSummary.RowStyles.Add(new RowStyle());
            tableLayoutPanelSummary.Size = new Size(281, 100);
            tableLayoutPanelSummary.TabIndex = 1;
            // 
            // labelMapStateCount4
            // 
            labelMapStateCount4.AutoSize = true;
            labelMapStateCount4.Dock = DockStyle.Fill;
            labelMapStateCount4.Location = new Point(213, 73);
            labelMapStateCount4.Name = "labelMapStateCount4";
            labelMapStateCount4.Size = new Size(64, 26);
            labelMapStateCount4.TabIndex = 11;
            labelMapStateCount4.Text = "0";
            labelMapStateCount4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelMapState4
            // 
            labelMapState4.AutoSize = true;
            labelMapState4.Dock = DockStyle.Fill;
            labelMapState4.Location = new Point(32, 73);
            labelMapState4.Name = "labelMapState4";
            labelMapState4.Size = new Size(174, 26);
            labelMapState4.TabIndex = 10;
            labelMapState4.Text = "Pożar";
            labelMapState4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBoxMapState4
            // 
            pictureBoxMapState4.BackColor = Color.Red;
            pictureBoxMapState4.Dock = DockStyle.Right;
            pictureBoxMapState4.Location = new Point(4, 76);
            pictureBoxMapState4.Name = "pictureBoxMapState4";
            pictureBoxMapState4.Size = new Size(21, 20);
            pictureBoxMapState4.TabIndex = 9;
            pictureBoxMapState4.TabStop = false;
            // 
            // labelMapStateCount3
            // 
            labelMapStateCount3.AutoSize = true;
            labelMapStateCount3.Dock = DockStyle.Fill;
            labelMapStateCount3.Location = new Point(213, 49);
            labelMapStateCount3.Name = "labelMapStateCount3";
            labelMapStateCount3.Size = new Size(64, 23);
            labelMapStateCount3.TabIndex = 8;
            labelMapStateCount3.Text = "0";
            labelMapStateCount3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelMapState3
            // 
            labelMapState3.AutoSize = true;
            labelMapState3.Dock = DockStyle.Fill;
            labelMapState3.Location = new Point(32, 49);
            labelMapState3.Name = "labelMapState3";
            labelMapState3.Size = new Size(174, 23);
            labelMapState3.TabIndex = 7;
            labelMapState3.Text = "Wysłano odczyt";
            labelMapState3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBoxMapState3
            // 
            pictureBoxMapState3.BackColor = Color.Orange;
            pictureBoxMapState3.Dock = DockStyle.Right;
            pictureBoxMapState3.Location = new Point(4, 52);
            pictureBoxMapState3.Name = "pictureBoxMapState3";
            pictureBoxMapState3.Size = new Size(21, 17);
            pictureBoxMapState3.TabIndex = 6;
            pictureBoxMapState3.TabStop = false;
            // 
            // labelMapStateCount2
            // 
            labelMapStateCount2.AutoSize = true;
            labelMapStateCount2.Dock = DockStyle.Fill;
            labelMapStateCount2.Location = new Point(213, 25);
            labelMapStateCount2.Name = "labelMapStateCount2";
            labelMapStateCount2.Size = new Size(64, 23);
            labelMapStateCount2.TabIndex = 5;
            labelMapStateCount2.Text = "0";
            labelMapStateCount2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelMapState2
            // 
            labelMapState2.AutoSize = true;
            labelMapState2.Dock = DockStyle.Fill;
            labelMapState2.Location = new Point(32, 25);
            labelMapState2.Name = "labelMapState2";
            labelMapState2.Size = new Size(174, 23);
            labelMapState2.TabIndex = 4;
            labelMapState2.Text = "Nieaktywne";
            labelMapState2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBoxMapState2
            // 
            pictureBoxMapState2.BackColor = Color.RoyalBlue;
            pictureBoxMapState2.Dock = DockStyle.Right;
            pictureBoxMapState2.Location = new Point(4, 28);
            pictureBoxMapState2.Name = "pictureBoxMapState2";
            pictureBoxMapState2.Size = new Size(21, 17);
            pictureBoxMapState2.TabIndex = 3;
            pictureBoxMapState2.TabStop = false;
            // 
            // labelMapStateCount1
            // 
            labelMapStateCount1.AutoSize = true;
            labelMapStateCount1.Dock = DockStyle.Fill;
            labelMapStateCount1.Location = new Point(213, 1);
            labelMapStateCount1.Name = "labelMapStateCount1";
            labelMapStateCount1.Size = new Size(64, 23);
            labelMapStateCount1.TabIndex = 2;
            labelMapStateCount1.Text = "0";
            labelMapStateCount1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBoxMapState1
            // 
            pictureBoxMapState1.BackColor = Color.Lime;
            pictureBoxMapState1.Dock = DockStyle.Right;
            pictureBoxMapState1.Location = new Point(4, 4);
            pictureBoxMapState1.Name = "pictureBoxMapState1";
            pictureBoxMapState1.Size = new Size(21, 17);
            pictureBoxMapState1.TabIndex = 0;
            pictureBoxMapState1.TabStop = false;
            // 
            // labelMapState1
            // 
            labelMapState1.AutoSize = true;
            labelMapState1.Dock = DockStyle.Fill;
            labelMapState1.Location = new Point(32, 1);
            labelMapState1.Name = "labelMapState1";
            labelMapState1.Size = new Size(174, 23);
            labelMapState1.TabIndex = 1;
            labelMapState1.Text = "Aktywne";
            labelMapState1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBoxMap
            // 
            pictureBoxMap.BackgroundImage = Properties.Resources.Kabacki_Segment;
            pictureBoxMap.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBoxMap.InitialImage = (Image)resources.GetObject("pictureBoxMap.InitialImage");
            pictureBoxMap.Location = new Point(-1, -1);
            pictureBoxMap.Name = "pictureBoxMap";
            pictureBoxMap.Size = new Size(1018, 651);
            pictureBoxMap.TabIndex = 2;
            pictureBoxMap.TabStop = false;
            pictureBoxMap.MouseMove += panelMap_MouseMove;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 729);
            Controls.Add(panelMap);
            Controls.Add(panelTimer);
            Controls.Add(panelInfo);
            Controls.Add(panelNavigation);
            Controls.Add(panelDisplay);
            MaximizeBox = false;
            Name = "FormMain";
            Text = "ZPI";
            FormClosing += FormMain_FormClosing;
            panelNavigation.ResumeLayout(false);
            panelInfo.ResumeLayout(false);
            panelCamera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamera).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            panelSensorName.ResumeLayout(false);
            panelTimer.ResumeLayout(false);
            panelMap.ResumeLayout(false);
            panelMap.PerformLayout();
            tableLayoutPanelSummary.ResumeLayout(false);
            tableLayoutPanelSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMapState1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMap).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelNavigation;
        private Panel panelInfo;
        private Panel panelTimer;
        private Button buttonOverview;
        private Button buttonDebug;
        private Button buttonSwitch;
        private Label labelTimer;
        private Button buttonFire;
        private Panel panelCamera;
        private PictureBox pictureBoxCamera;
        private TableLayoutPanel tableLayoutPanel;
        private Panel panelSensorName;
        private Label labelState;
        private Label labelStateInfo;
        private Label labelSegment;
        private Label labelSegmentInfo;
        private Label labelLocation;
        private Label labelLocationInfo;
        private Label labelTemperature;
        private Label labelTemperatureInfo;
        private Label labelLastUpdate;
        private Label labelLastUpdateInfo;
        private Label labelSensorName;
        public Panel panelDisplay;
        private System.Windows.Forms.Timer timerRefresh;
        private Panel panelMap;
        private TableLayoutPanel tableLayoutPanelSummary;
        private PictureBox pictureBoxMapState1;
        private Label labelMapState1;
        private Label labelMapStateCount4;
        private Label labelMapState4;
        private PictureBox pictureBoxMapState4;
        private Label labelMapStateCount3;
        private Label labelMapState3;
        private PictureBox pictureBoxMapState3;
        private Label labelMapStateCount2;
        private Label labelMapState2;
        private PictureBox pictureBoxMapState2;
        private Label labelMapStateCount1;
        private PictureBox pictureBoxMap;
    }
}