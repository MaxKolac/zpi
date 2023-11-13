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
            panelNavigation = new Panel();
            panelInfo = new Panel();
            panelTimer = new Panel();
            SuspendLayout();
            // 
            // panelDisplay
            // 
            panelDisplay.Location = new Point(0, 0);
            panelDisplay.Name = "panelDisplay";
            panelDisplay.Size = new Size(1056, 652);
            panelDisplay.TabIndex = 0;
            // 
            // panelNavigation
            // 
            panelNavigation.Location = new Point(0, 649);
            panelNavigation.Name = "panelNavigation";
            panelNavigation.Size = new Size(1056, 80);
            panelNavigation.TabIndex = 1;
            // 
            // panelInfo
            // 
            panelInfo.Location = new Point(1055, 0);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(302, 651);
            panelInfo.TabIndex = 2;
            // 
            // panelTimer
            // 
            panelTimer.Location = new Point(1055, 649);
            panelTimer.Name = "panelTimer";
            panelTimer.Size = new Size(302, 80);
            panelTimer.TabIndex = 3;
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
            ResumeLayout(false);
        }

        #endregion

        private Panel panelDisplay;
        private Panel panelNavigation;
        private Panel panelInfo;
        private Panel panelTimer;
    }
}