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
            panelDisplay = new Panel();
            pictureBoxDisplay = new PictureBox();
            panelNavigation = new Panel();
            buttonOverview = new Button();
            buttonMenu = new Button();
            buttonSwitch = new Button();
            panelInfo = new Panel();
            panelTimer = new Panel();
            labelTimer = new Label();
            panelDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxDisplay).BeginInit();
            panelNavigation.SuspendLayout();
            panelTimer.SuspendLayout();
            SuspendLayout();
            // 
            // panelDisplay
            // 
            panelDisplay.BorderStyle = BorderStyle.FixedSingle;
            panelDisplay.Controls.Add(pictureBoxDisplay);
            panelDisplay.Location = new Point(0, 0);
            panelDisplay.Name = "panelDisplay";
            panelDisplay.Size = new Size(1018, 652);
            panelDisplay.TabIndex = 0;
            // 
            // pictureBoxDisplay
            // 
            pictureBoxDisplay.BackColor = SystemColors.ControlDark;
            pictureBoxDisplay.Location = new Point(3, 3);
            pictureBoxDisplay.Name = "pictureBoxDisplay";
            pictureBoxDisplay.Size = new Size(1010, 643);
            pictureBoxDisplay.TabIndex = 4;
            pictureBoxDisplay.TabStop = false;
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
            buttonOverview.Location = new Point(686, 9);
            buttonOverview.Name = "buttonOverview";
            buttonOverview.Size = new Size(324, 59);
            buttonOverview.TabIndex = 3;
            buttonOverview.Text = "Podgląd Zgłoszeń";
            buttonOverview.UseVisualStyleBackColor = true;
            // 
            // buttonMenu
            // 
            buttonMenu.Location = new Point(12, 9);
            buttonMenu.Name = "buttonMenu";
            buttonMenu.Size = new Size(324, 59);
            buttonMenu.TabIndex = 1;
            buttonMenu.Text = "Menu";
            buttonMenu.UseVisualStyleBackColor = true;
            // 
            // buttonSwitch
            // 
            buttonSwitch.Location = new Point(348, 9);
            buttonSwitch.Name = "buttonSwitch";
            buttonSwitch.Size = new Size(324, 59);
            buttonSwitch.TabIndex = 2;
            buttonSwitch.Text = "Przełącz Widok";
            buttonSwitch.UseVisualStyleBackColor = true;
            // 
            // panelInfo
            // 
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.Location = new Point(1016, 0);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(341, 651);
            panelInfo.TabIndex = 2;
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
            labelTimer.AutoSize = true;
            labelTimer.Location = new Point(60, 31);
            labelTimer.Name = "labelTimer";
            labelTimer.Size = new Size(221, 15);
            labelTimer.TabIndex = 0;
            labelTimer.Text = "W tym miejscu będzie wyświetlany zegar";
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
            Name = "FormMain";
            Text = "ZPI";
            panelDisplay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxDisplay).EndInit();
            panelNavigation.ResumeLayout(false);
            panelTimer.ResumeLayout(false);
            panelTimer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelDisplay;
        private Panel panelNavigation;
        private Panel panelInfo;
        private Panel panelTimer;
        private PictureBox pictureBoxDisplay;
        private Button buttonOverview;
        private Button buttonMenu;
        private Button buttonSwitch;
        private Label labelTimer;
    }
}