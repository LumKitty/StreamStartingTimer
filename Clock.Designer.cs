namespace StreamStartingTimer
{
    partial class Clock
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
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Clock));
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            txtBackColor = new ToolStripTextBox();
            toolStripSeparator1 = new ToolStripSeparator();
            btnFont = new ToolStripButton();
            txtForeColor = new ToolStripTextBox();
            toolStripSeparator2 = new ToolStripSeparator();
            btnTest = new ToolStripButton();
            statusStrip1 = new StatusStrip();
            lblCountdown = new Label();
            fontDialog = new FontDialog();
            timer1 = new System.Windows.Forms.Timer(components);
            toolStripSeparator3 = new ToolStripSeparator();
            btnEvents = new ToolStripButton();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, txtBackColor, toolStripSeparator1, btnFont, txtForeColor, toolStripSeparator2, btnEvents, toolStripSeparator3, btnTest });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(794, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(71, 22);
            toolStripLabel1.Text = "Background";
            // 
            // txtBackColor
            // 
            txtBackColor.MaxLength = 6;
            txtBackColor.Name = "txtBackColor";
            txtBackColor.Size = new Size(43, 25);
            txtBackColor.Text = "00FF00";
            txtBackColor.KeyPress += txtColor_KeyPress;
            txtBackColor.TextChanged += txtBackColor_TextChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnFont
            // 
            btnFont.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFont.Image = (Image)resources.GetObject("btnFont.Image");
            btnFont.ImageTransparentColor = Color.Magenta;
            btnFont.Name = "btnFont";
            btnFont.Size = new Size(68, 22);
            btnFont.Text = "Clock Font";
            btnFont.Click += btnFont_Click;
            // 
            // txtForeColor
            // 
            txtForeColor.MaxLength = 6;
            txtForeColor.Name = "txtForeColor";
            txtForeColor.Size = new Size(43, 25);
            txtForeColor.Text = "000000";
            txtForeColor.KeyPress += txtColor_KeyPress;
            txtForeColor.TextChanged += txtForeColor_TextChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnTest
            // 
            btnTest.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTest.Image = (Image)resources.GetObject("btnTest.Image");
            btnTest.ImageTransparentColor = Color.Magenta;
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(59, 22);
            btnTest.Text = "Test (5m)";
            btnTest.Click += btnTest_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 392);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(794, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblCountdown
            // 
            lblCountdown.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblCountdown.BackColor = Color.Lime;
            lblCountdown.Font = new Font("Bedstead", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCountdown.Location = new Point(0, 25);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(794, 367);
            lblCountdown.TabIndex = 2;
            lblCountdown.Text = "00:00";
            lblCountdown.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // fontDialog
            // 
            fontDialog.FontMustExist = true;
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnEvents
            // 
            btnEvents.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEvents.Image = (Image)resources.GetObject("btnEvents.Image");
            btnEvents.ImageTransparentColor = Color.Magenta;
            btnEvents.Name = "btnEvents";
            btnEvents.Size = new Size(45, 22);
            btnEvents.Text = "Events";
            btnEvents.Click += btnEvents_Click;
            // 
            // Clock
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(794, 414);
            Controls.Add(lblCountdown);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            MaximizeBox = false;
            Name = "Clock";
            Text = "Stream Startup Timer";
            Load += Clock_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }



        #endregion

        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private Label lblCountdown;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox txtBackColor;
        private MaskedTextBox maskedTextBox1;
        private ToolStripButton btnFont;
        private FontDialog fontDialog;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripTextBox txtForeColor;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnTest;
        private System.Windows.Forms.Timer timer1;
        private ToolStripButton btnEvents;
        private ToolStripSeparator toolStripSeparator3;
    }
}
