using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using WatsonWebsocket;

namespace StreamStartingTimer {
    public partial class Clock : Form {
        private int SecondsToGo;
        public Clock(int StartTime=0, string EventsFile = "::", string ConfigFile = "::") {
            InitializeComponent();
            if (ConfigFile != "::") {
                _ConfigFile = ConfigFile;
            } else {
                _ConfigFile = Application.StartupPath + "\\DefaultConfig.json";
            }
            if (EventsFile == "::") {
                EventsFile = Application.StartupPath + "\\DefaultEvents.json";
            } else if (!File.Exists(EventsFile)) {
                MessageBox.Show(EventsFile, "File not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (File.Exists(EventsFile)) {
                TimerEvents = Shared.LoadEvents(EventsFile);
            }
            LoadConfig();
            if (StartTime > 0) {
                txtSeconds.Text = StartTime.ToString();
                StartTimer();
                QuitWhenDone = true;
            } else {
                QuitWhenDone = false;
                this.Text = this.Text += " (Setup/test mode)";
            }
            // StartTimer();
        }

        const string Version = "v0.1";
        const string TimeFormat = @"mm\:ss";
        const string DefaultStatusBar = Version + " - github.com/LumKitty";
        string _ConfigFile;
        Color TextBackgroundColor;
        private List<TimerEvent> TimerEvents = new();
        private bool QuitWhenDone = false;

        private void SaveConfig() {
            JObject Config = new JObject(
                new JProperty("FontName", fontDialog.Font.Name),
                new JProperty("FontSize", fontDialog.Font.Size),
                new JProperty("FontStyle", (int)fontDialog.Font.Style),
                new JProperty("BackgroundColor", txtBackColor.Text),
                new JProperty("ForegroundColor", txtForeColor.Text),
                new JProperty("Alignment", cmbAlign.SelectedIndex),
                new JProperty("X", this.Location.X),
                new JProperty("Y", this.Location.Y),
                new JProperty("Width", this.Width),
                new JProperty("Height", this.Height),
                new JProperty("VNyanURL", Shared.VNyanURL),
                new JProperty("MixItUpURL", Shared.MixItUpURL),
                new JProperty("MixItUpPlatform", Shared.MixItUpPlatform)
            );
            File.WriteAllText(_ConfigFile, Config.ToString());
        }

        void VNyanConnected(object sender, EventArgs args) {
            lblVNyan.BackColor = Color.Green;
            Shared.VNyanConnected = true;
        }
        void VNyanDisconnected(object sender, EventArgs args) {
            lblVNyan.BackColor = Color.Red;
            Shared.VNyanConnected = false;
        }

        private void LoadConfig() {
            if (File.Exists(_ConfigFile)) {
                dynamic Config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(_ConfigFile));
                if ((int)Config.X < SystemInformation.VirtualScreen.Width - 5 &&  // Safety in case monitor
                    (int)Config.Y < SystemInformation.VirtualScreen.Height - 5) { // size has changed
                    this.Location = new Point((int)Config.X, (int)Config.Y);
                    this.Size = new Size((int)Config.Width, (int)Config.Height);
                }
                txtBackColor.Text = Config.BackgroundColor;
                txtForeColor.Text = Config.ForegroundColor;
                lblCountdown.BackColor = ValidateTextColor((string)Config.BackgroundColor);
                lblCountdown.ForeColor = ValidateTextColor((string)Config.ForegroundColor);
                cmbAlign.SelectedIndex = (int)Config.Alignment;
                SetTextAlignment((int)Config.Alignment);
                Font font = new Font((string)Config.FontName, (int)Config.FontSize, (FontStyle)Config.FontStyle);
                fontDialog.Font = font;
                lblCountdown.Font = font;
                Shared.VNyanURL = Config.VNyanURL;
                Shared.MixItUpURL = Config.MixItUpURL;
                Shared.MixItUpPlatform = Config.MixItUpPlatform;
            } else {
                Font font = lblCountdown.Font;
                fontDialog.Font = font;
                txtBackColor.Text = "00FF00";
                txtForeColor.Text = "000000";
                cmbAlign.SelectedIndex = 0;
                SetTextAlignment(0);
                Shared.VNyanURL = "ws://localhost:8000/vnyan";
                Shared.MixItUpURL = "http://localhost:8911/api/v2";
                Shared.MixItUpPlatform = "twitch";
                if (MessageBox.Show("This appears to be the first time you have run this program. Would you like to view the instructions", "Welcome", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    Process myProcess = new Process();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = "https://github.com/LumKitty/StreamStartingTimer/blob/master/README.md";
                    myProcess.Start();
                    myProcess.Dispose();
                }
            }
        }

        private async Task ConnectVNyan() {
            int n = 0;
            lblVNyan.Enabled = true;
            lblVNyan.BackColor = Color.Goldenrod;
            do {
                Shared.wsClient = new WatsonWsClient(new Uri(Shared.VNyanURL));
                Shared.wsClient.KeepAliveInterval = 1000;
                Shared.wsClient.ServerConnected += VNyanConnected;
                Shared.wsClient.ServerDisconnected += VNyanDisconnected;
                Shared.wsClient.Start();
                while (!Shared.VNyanConnected && n < 50) {
                    Thread.Sleep(100);
                    n++;
                }
                if (!Shared.VNyanConnected) {
                    lblVNyan.BackColor = Color.Red;
                }
            } while(!Shared.VNyanConnected);
        }
        private async Task ConnectMixItUp() {
            lblMixItUp.Enabled = true;
            lblMixItUp.BackColor = Color.Goldenrod;
            do {
                if (Shared.InitMIU(Shared.MixItUpURL)) {
                    lblMixItUp.BackColor = Color.Green;
                    Shared.MixItUpConnected = true;
                    UpdateMiuTimerEvents(ref TimerEvents);
                } else {
                    lblMixItUp.BackColor = Color.Red;
                    Shared.MixItUpConnected = false;
                }
            } while (!Shared.MixItUpConnected);
        }

        private void UpdateMiuTimerEvents(ref List<TimerEvent> timerEvents) {
            foreach (TimerEvent timerEvent in timerEvents) {
                if ((timerEvent.EventType == EventType.MixItUp) && (timerEvent.MiuCmdID == "")) {
                    timerEvent.UpdateMiuCmdId();
                }
            }
        }

        private void Connect() {
            if (Shared.VNyanURL.Length > 0) {
                Task.Run(() => ConnectVNyan());
            } else {
                lblVNyan.BackColor = SystemColors.Control;
                lblVNyan.Enabled = false;
                Shared.VNyanConnected = false;
            }
            if (Shared.MixItUpURL.Length > 0) {
                Task.Run(() => ConnectMixItUp());
            } else {
                lblMixItUp.BackColor = SystemColors.Control;
                lblMixItUp.Enabled = false;
            }
        }

        private void Clock_Load(object sender, EventArgs e) {
            TextBackgroundColor = txtBackColor.BackColor;
            lblNextEvent.Text = DefaultStatusBar;
            UpdateClock(txtSeconds.Text);
        }

        private static Color ValidateTextColor(string strColor) {
            if (strColor.Length == 6) {
                try {
                    Byte r = Convert.ToByte(strColor.Substring(0, 2), 16);
                    Byte g = Convert.ToByte(strColor.Substring(2, 2), 16);
                    Byte b = Convert.ToByte(strColor.Substring(4, 2), 16);
                    return Color.FromArgb(r, g, b);
                } catch {
                    throw new Exception("InvalidColor");
                }
            }
            throw new Exception("InvalidColor");
        }

        private void txtBackColor_TextChanged(object sender, EventArgs e) {
            try {
                lblCountdown.BackColor = ValidateTextColor(txtBackColor.Text);
                txtBackColor.BackColor = TextBackgroundColor;
            } catch {
                txtBackColor.BackColor = Color.FromArgb(255, 127, 127);
            }
        }

        private void txtForeColor_TextChanged(object sender, EventArgs e) {
            try {
                lblCountdown.ForeColor = ValidateTextColor(txtForeColor.Text);
                txtForeColor.BackColor = TextBackgroundColor;
            } catch {
                txtForeColor.BackColor = Color.FromArgb(255, 127, 127);
            }
        }

        private void txtColor_KeyPress(object sender, KeyPressEventArgs e) {
            if (
                 (e.KeyChar >= 'A' && e.KeyChar <= 'F') ||
                 (e.KeyChar >= '0' && e.KeyChar <= '9') ||
                 (e.KeyChar < (char)32)) {
                e.Handled = false;
            } else if (e.KeyChar >= 'a' && e.KeyChar <= 'f') {
                e.KeyChar = (char)((int)e.KeyChar - 32);
            } else {
                e.Handled = true;
            }
        }

        private void btnFont_Click(object sender, EventArgs e) {
            this.fontDialog.Font = lblCountdown.Font;
            if (this.fontDialog.ShowDialog() == DialogResult.OK) {
                lblCountdown.Font = this.fontDialog.Font;
            }
        }


        public void StartCountdown(int CountdownTime) {
            SecondsToGo = CountdownTime;
            timer1.Enabled = true;
        }

        private void UpdateClock(int SecondsToGo) {
            lblCountdown.Text = TimeSpan.FromSeconds(SecondsToGo).ToString(TimeFormat);
        }
        private void UpdateClock(string SecondsToGo) {
            int Seconds;
            if (Int32.TryParse(SecondsToGo, out Seconds)) {
                UpdateClock(Seconds);
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            string StatusLabel = "Next Event: Go live!";
            int n;
            int i;
            int ExtraSimultaneousEvents = 0;
            SecondsToGo--;
            UpdateClock(SecondsToGo);

            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum - SecondsToGo;
            for (n = 0; n < TimerEvents.Count; n++) {
                if ((TimerEvents[n].Time.TotalSeconds < SecondsToGo) && !TimerEvents[n].HasFired) {
                    StatusLabel = "Next Event in " + (SecondsToGo - TimerEvents[n].Time.TotalSeconds) + "s: " + TimerEvents[n].Time.ToString(TimeFormat) + " (" + TimerEvents[n].EventType + ") " + TimerEvents[n].Payload;
                    i = n + 1;
                    while (i < TimerEvents.Count && TimerEvents[i].Time.TotalSeconds == TimerEvents[n].Time.TotalSeconds) {
                        if (!TimerEvents[i].HasFired) { ExtraSimultaneousEvents++; }
                        i++;
                    }
                    if (ExtraSimultaneousEvents > 0) {
                        StatusLabel += " +" + ExtraSimultaneousEvents.ToString();
                    }
                    break;
                } else if ((TimerEvents[n].Time.TotalSeconds == SecondsToGo) && !TimerEvents[n].HasFired) {
                    i = n;
                    StatusLabel = "Firing event:";
                    while (i < TimerEvents.Count && TimerEvents[i].Time.TotalSeconds == SecondsToGo) {
                        if (!TimerEvents[i].HasFired) {
                            StatusLabel += " (" + TimerEvents[i].EventType + ") " + TimerEvents[i].Payload;
                            if (!TimerEvents[i].Refire) { TimerEvents[i].HasFired = true; }
                            TimerEvents[i].Fire();
                        }
                        i++;
                    }
                    break;
                }
            }
            lblNextEvent.Text = StatusLabel;
            if (SecondsToGo <= 0) {
                timer1.Stop();
                if (QuitWhenDone) {
                    Thread.Sleep(1000);
                    this.Close();
                }
            }
        }

        private void btnEvents_Click(object sender, EventArgs e) {
            EventEditor EventEditor = new EventEditor();
            EventEditor.FormTimerEvents = new List<TimerEvent>(TimerEvents);
            EventEditor.ShowDialog();
            if (EventEditor.DialogResult == DialogResult.OK) {
                TimerEvents = new List<TimerEvent>(EventEditor.FormTimerEvents);
            }
        }

        private void StartTimer() {
            int Seconds;
            if (Int32.TryParse(txtSeconds.Text, out Seconds)) {
                StartCountdown(Seconds);
                txtSeconds.Enabled = false;
                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnReset.Enabled = true;
                btnAdd30s.Enabled = true;
                btnAdd60s.Enabled = true;
                btnEvents.Enabled = false;
                toolStripProgressBar1.Maximum = Seconds;
                toolStripProgressBar1.Value = 0;
                foreach (TimerEvent timerEvent in TimerEvents) {
                    timerEvent.HasFired = false;
                }
            } else {
                MessageBox.Show("Invalid time: " + txtSeconds.Text);
            }
        }
        private void btnStart_Click(object sender, EventArgs e) {
            StartTimer();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            timer1.Enabled = !timer1.Enabled;
        }

        private void btnReset_Click(object sender, EventArgs e) {
            btnAdd30s.Enabled = false;
            btnAdd30s.Enabled = false;
            btnPause.Enabled = false;
            btnReset.Enabled = false;
            btnStart.Enabled = true;
            btnEvents.Enabled = true;
            txtSeconds.Enabled = true;
            timer1.Enabled = false;
            UpdateClock(txtSeconds.Text);
            lblNextEvent.Text = DefaultStatusBar;
        }

        private void txtSeconds_KeyPress(object sender, KeyPressEventArgs e) {
            if (
                (e.KeyChar >= '0' && e.KeyChar <= '9') ||
                (e.KeyChar < (char)32)) {
                e.Handled = false;
            } else {
                e.Handled = true;
            }
        }

        private void txtSeconds_TextChanged(object sender, EventArgs e) {
            UpdateClock(txtSeconds.Text);
        }

        private void btnAdd30s_Click(object sender, EventArgs e) {
            SecondsToGo += 30;
        }

        private void btnAdd60s_Click(object sender, EventArgs e) {
            SecondsToGo += 60;
        }

        private void Clock_FormClosed(object sender, FormClosedEventArgs e) {
            SaveConfig();
            //Shared.wsClient.Stop();
        }

        private void SetTextAlignment(int Position) {
            switch (Position) {
                case 0: lblCountdown.TextAlign = ContentAlignment.TopLeft; break;
                case 1: lblCountdown.TextAlign = ContentAlignment.TopCenter; break;
                case 2: lblCountdown.TextAlign = ContentAlignment.TopRight; break;
                case 3: lblCountdown.TextAlign = ContentAlignment.MiddleLeft; break;
                case 4: lblCountdown.TextAlign = ContentAlignment.MiddleCenter; break;
                case 5: lblCountdown.TextAlign = ContentAlignment.MiddleRight; break;
                case 6: lblCountdown.TextAlign = ContentAlignment.BottomLeft; break;
                case 7: lblCountdown.TextAlign = ContentAlignment.BottomCenter; break;
                case 8: lblCountdown.TextAlign = ContentAlignment.BottomRight; break;
            }
        }


        private void cmbAlign_DropDownClosed(object sender, EventArgs e) {
            SetTextAlignment(cmbAlign.SelectedIndex);
        }

        private void Clock_Shown(object sender, EventArgs e) {
            Connect();
        }
    }
}

