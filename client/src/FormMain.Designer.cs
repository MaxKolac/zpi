﻿namespace ZPIClient
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
            panelDisplay = new Panel();
            panelNavigation = new Panel();
            buttonOverview = new Button();
            buttonMenu = new Button();
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
            panelNavigation.SuspendLayout();
            panelInfo.SuspendLayout();
            panelCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamera).BeginInit();
            tableLayoutPanel.SuspendLayout();
            panelSensorName.SuspendLayout();
            panelTimer.SuspendLayout();
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
            panelNavigation.Controls.Add(buttonMenu);
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
            buttonOverview.Text = "Podgląd zgłoszeń";
            buttonOverview.UseVisualStyleBackColor = true;
            // 
            // buttonMenu
            // 
            buttonMenu.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            buttonMenu.Location = new Point(12, 9);
            buttonMenu.Name = "buttonMenu";
            buttonMenu.Size = new Size(324, 59);
            buttonMenu.TabIndex = 1;
            buttonMenu.Text = "Menu";
            buttonMenu.UseVisualStyleBackColor = true;
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
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 729);
            Controls.Add(panelTimer);
            Controls.Add(panelInfo);
            Controls.Add(panelNavigation);
            Controls.Add(panelDisplay);
            MaximizeBox = false;
            Name = "FormMain";
            Text = "ZPI";
            panelNavigation.ResumeLayout(false);
            panelInfo.ResumeLayout(false);
            panelCamera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamera).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            panelSensorName.ResumeLayout(false);
            panelTimer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panelNavigation;
        private Panel panelInfo;
        private Panel panelTimer;
        private Button buttonOverview;
        private Button buttonMenu;
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
    }
}