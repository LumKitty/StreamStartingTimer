namespace StreamStartingTimer {
    partial class Config {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            colorDialog1 = new ColorDialog();
            fontDialog = new FontDialog();
            propertyGrid1 = new PropertyGrid();
            SuspendLayout();
            // 
            // fontDialog
            // 
            fontDialog.AllowScriptChange = false;
            fontDialog.FontMustExist = true;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid1.Location = new Point(12, 12);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(776, 426);
            propertyGrid1.TabIndex = 0;
            // 
            // Config
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(propertyGrid1);
            Name = "Config";
            Text = "Config";
            Load += Config_Load;
            ResumeLayout(false);
        }

        #endregion

        private ColorDialog colorDialog1;
        private FontDialog fontDialog;
        private PropertyGrid propertyGrid1;
    }
}