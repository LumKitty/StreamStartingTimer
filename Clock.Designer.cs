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
            btnStart = new ToolStripButton();
            btnPause = new ToolStripButton();
            btnReset = new ToolStripButton();
            btnAdd30s = new ToolStripButton();
            btnAdd60s = new ToolStripButton();
            txtSeconds = new ToolStripTextBox();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            txtBackColor = new ToolStripTextBox();
            toolStripSeparator1 = new ToolStripSeparator();
            btnFont = new ToolStripButton();
            txtForeColor = new ToolStripTextBox();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripLabel2 = new ToolStripLabel();
            cmbAlign = new ToolStripComboBox();
            toolStripSeparator2 = new ToolStripSeparator();
            btnEvents = new ToolStripButton();
            statusStrip1 = new StatusStrip();
            lblVNyan = new ToolStripStatusLabel();
            lblMixItUp = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            lblNextEvent = new ToolStripStatusLabel();
            lblCountdown = new Label();
            fontDialog = new FontDialog();
            timer1 = new System.Windows.Forms.Timer(components);
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnStart, btnPause, btnReset, btnAdd30s, btnAdd60s, txtSeconds, toolStripSeparator3, toolStripLabel1, txtBackColor, toolStripSeparator1, btnFont, txtForeColor, toolStripSeparator4, toolStripLabel2, cmbAlign, toolStripSeparator2, btnEvents });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(794, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnStart
            // 
            btnStart.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnStart.Font = new Font("Webdings", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 2);
            btnStart.Image = (Image)resources.GetObject("btnStart.Image");
            btnStart.ImageTransparentColor = Color.Magenta;
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(28, 24);
            btnStart.Text = "4";
            btnStart.Click += btnStart_Click;
            // 
            // btnPause
            // 
            btnPause.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPause.Enabled = false;
            btnPause.Font = new Font("Webdings", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 2);
            btnPause.Image = (Image)resources.GetObject("btnPause.Image");
            btnPause.ImageTransparentColor = Color.Magenta;
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(28, 24);
            btnPause.Text = ";";
            btnPause.Click += btnPause_Click;
            // 
            // btnReset
            // 
            btnReset.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnReset.Enabled = false;
            btnReset.Font = new Font("Webdings", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 2);
            btnReset.Image = (Image)resources.GetObject("btnReset.Image");
            btnReset.ImageTransparentColor = Color.Magenta;
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(28, 24);
            btnReset.Text = "q";
            btnReset.Click += btnReset_Click;
            // 
            // btnAdd30s
            // 
            btnAdd30s.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAdd30s.Enabled = false;
            btnAdd30s.Image = (Image)resources.GetObject("btnAdd30s.Image");
            btnAdd30s.ImageTransparentColor = Color.Magenta;
            btnAdd30s.Name = "btnAdd30s";
            btnAdd30s.Size = new Size(36, 24);
            btnAdd30s.Text = "+30s";
            btnAdd30s.Click += btnAdd30s_Click;
            // 
            // btnAdd60s
            // 
            btnAdd60s.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAdd60s.Enabled = false;
            btnAdd60s.Image = (Image)resources.GetObject("btnAdd60s.Image");
            btnAdd60s.ImageTransparentColor = Color.Magenta;
            btnAdd60s.Name = "btnAdd60s";
            btnAdd60s.Size = new Size(36, 24);
            btnAdd60s.Text = "+1m";
            btnAdd60s.Click += btnAdd60s_Click;
            // 
            // txtSeconds
            // 
            txtSeconds.MaxLength = 5;
            txtSeconds.Name = "txtSeconds";
            txtSeconds.Size = new Size(40, 27);
            txtSeconds.Text = "300";
            txtSeconds.KeyPress += txtSeconds_KeyPress;
            txtSeconds.TextChanged += txtSeconds_TextChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 27);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(71, 24);
            toolStripLabel1.Text = "Background";
            // 
            // txtBackColor
            // 
            txtBackColor.MaxLength = 6;
            txtBackColor.Name = "txtBackColor";
            txtBackColor.Size = new Size(43, 27);
            txtBackColor.Text = "00FF00";
            txtBackColor.KeyPress += txtColor_KeyPress;
            txtBackColor.TextChanged += txtBackColor_TextChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 27);
            // 
            // btnFont
            // 
            btnFont.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFont.Image = (Image)resources.GetObject("btnFont.Image");
            btnFont.ImageTransparentColor = Color.Magenta;
            btnFont.Name = "btnFont";
            btnFont.Size = new Size(68, 24);
            btnFont.Text = "Clock Font";
            btnFont.Click += btnFont_Click;
            // 
            // txtForeColor
            // 
            txtForeColor.MaxLength = 6;
            txtForeColor.Name = "txtForeColor";
            txtForeColor.Size = new Size(43, 27);
            txtForeColor.Text = "000000";
            txtForeColor.KeyPress += txtColor_KeyPress;
            txtForeColor.TextChanged += txtForeColor_TextChanged;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 27);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(35, 24);
            toolStripLabel2.Text = "Align";
            // 
            // cmbAlign
            // 
            cmbAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAlign.IntegralHeight = false;
            cmbAlign.Items.AddRange(new object[] { "Top Left", "Top Centre", "Top Right", "Left", "Centre", "Right", "Bottom Left", "Bottom Centre", "Bottom Right" });
            cmbAlign.Name = "cmbAlign";
            cmbAlign.Size = new Size(121, 27);
            cmbAlign.DropDownClosed += cmbAlign_DropDownClosed;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 27);
            // 
            // btnEvents
            // 
            btnEvents.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEvents.Image = (Image)resources.GetObject("btnEvents.Image");
            btnEvents.ImageTransparentColor = Color.Magenta;
            btnEvents.Name = "btnEvents";
            btnEvents.Size = new Size(68, 24);
            btnEvents.Text = "Edit Events";
            btnEvents.Click += btnEvents_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblVNyan, lblMixItUp, toolStripProgressBar1, lblNextEvent });
            statusStrip1.Location = new Point(0, 392);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(794, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblVNyan
            // 
            lblVNyan.Name = "lblVNyan";
            lblVNyan.Size = new Size(42, 17);
            lblVNyan.Text = "VNyan";
            // 
            // lblMixItUp
            // 
            lblMixItUp.Name = "lblMixItUp";
            lblMixItUp.Size = new Size(49, 17);
            lblMixItUp.Text = "MixItUp";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 16);
            // 
            // lblNextEvent
            // 
            lblNextEvent.Name = "lblNextEvent";
            lblNextEvent.Size = new Size(12, 17);
            lblNextEvent.Text = "-";
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
            fontDialog.AllowScriptChange = false;
            fontDialog.FontMustExist = true;
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
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
            FormClosed += Clock_FormClosed;
            Load += Clock_Load;
            Shown += Clock_Shown;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
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
        private System.Windows.Forms.Timer timer1;
        private ToolStripButton btnEvents;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel lblNextEvent;
        private ToolStripTextBox txtSeconds;
        private ToolStripButton btnStart;
        private ToolStripButton btnPause;
        private ToolStripButton btnReset;
        private ToolStripButton btnAdd30s;
        private ToolStripButton btnAdd60s;
        private ToolStripLabel toolStripLabel2;
        private ToolStripComboBox cmbAlign;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripStatusLabel lblVNyan;
        private ToolStripStatusLabel lblMixItUp;
    }
}
