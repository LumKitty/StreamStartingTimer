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
        public Clock(int StartTime = 0, string EventsFile = "::", string ConfigFile = "::") {
            InitializeComponent();
            lblCountdown.DataBindings.Add("BackColor", Shared.CurSettings, "BGCol");
            lblCountdown.DataBindings.Add("ForeColor", Shared.CurSettings, "FGCol");
            lblCountdown.DataBindings.Add("Font", Shared.CurSettings, "Font");
            lblCountdown.DataBindings.Add("TextAlign", Shared.CurSettings, "Alignment");
            //this.DataBindings.Add("Location", Shared.CurSettings, "Location");
            //this.DataBindings.Add("Size", Shared.CurSettings, "Dimensions");

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

        //const string TimeFormat = @"mm\:ss";
        const string DefaultStatusBar = Shared.Version + " - github.com/LumKitty";
        string _ConfigFile;
        Color TextBackgroundColor;
        private List<TimerEvent> TimerEvents = new();
        private bool QuitWhenDone = false;

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
                    Shared.CurSettings.Location = new Point((int)Config.X, (int)Config.Y);
                    Shared.CurSettings.Dimensions = new Size((int)Config.Width, (int)Config.Height);
                }
                try {
                    Shared.CurSettings.BGCol = ColorTranslator.FromHtml((string)Config.BackgroundColor);
                } catch {
                    Shared.CurSettings.BGCol = Color.Green;
                }
                try {
                    Shared.CurSettings.FGCol = ColorTranslator.FromHtml((string)Config.ForegroundColor);
                } catch {
                    Shared.CurSettings.FGCol = Color.Black;
                }
                Shared.CurSettings.Alignment = (ContentAlignment)Config.Alignment;
                Shared.CurSettings.Font = new Font((string)Config.FontName, (int)Config.FontSize, (FontStyle)Config.FontStyle);
                Shared.CurSettings.VNyanURL = Config.VNyanURL;
                Shared.CurSettings.MixItUpURL = Config.MixItUpURL;
                Shared.CurSettings.MixItUpPlatform = Shared.GetMiuPlatform(Config.MixItUpPlatform.ToString(), MIUPlatforms.Twitch);
            } else {
                Font font = lblCountdown.Font;
                Shared.CurSettings.BGCol = Color.FromArgb(0, 255, 0);
                Shared.CurSettings.FGCol = Color.FromArgb(0, 0, 0);
                Shared.CurSettings.Alignment = ContentAlignment.TopLeft;
                Shared.CurSettings.VNyanURL = "ws://localhost:8000/vnyan";
                Shared.CurSettings.MixItUpURL = "http://localhost:8911/api/v2";
                Shared.CurSettings.MixItUpPlatform = MIUPlatforms.Twitch;
                if (MessageBox.Show("This appears to be the first time you have run this program. Would you like to view the instructions", "Welcome", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    Process myProcess = new Process();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = "https://github.com/LumKitty/StreamStartingTimer/blob/master/README.md";
                    myProcess.Start();
                    myProcess.Dispose();
                }
            }
            //txtBackColor.Text = ColorTranslator.To
            //txtForeColor.Text = Config.ForegroundColor;
        }
        private void SaveConfig() {
            JObject Config = new JObject(
                new JProperty("FontName", Shared.CurSettings.Font.Name),
                new JProperty("FontSize", Shared.CurSettings.Font.Size),
                new JProperty("FontStyle", (int)Shared.CurSettings.Font.Style),
                new JProperty("BackgroundColor", ColorTranslator.ToHtml(Shared.CurSettings.BGCol)),
                new JProperty("ForegroundColor", ColorTranslator.ToHtml(Shared.CurSettings.FGCol)),
                new JProperty("Alignment", Convert.ToInt32(Shared.CurSettings.Alignment)),
                new JProperty("X", this.Location.X),
                new JProperty("Y", this.Location.Y),
                new JProperty("Width", this.Size.Width),
                new JProperty("Height", this.Size.Height),
                new JProperty("VNyanURL", Shared.CurSettings.VNyanURL),
                new JProperty("MixItUpURL", Shared.CurSettings.MixItUpURL),
                new JProperty("MixItUpPlatform", Shared.CurSettings.MixItUpPlatform.ToString())
            );
            File.WriteAllText(_ConfigFile, Config.ToString());
        }


        private async Task ConnectVNyan() {
            int n = 0;
            lblVNyan.Enabled = true;
            lblVNyan.BackColor = Color.Goldenrod;
            do {
                Shared.wsClient = new WatsonWsClient(new Uri(Shared.CurSettings.VNyanURL));
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
            } while (!Shared.VNyanConnected);
        }
        private async Task ConnectMixItUp() {
            lblMixItUp.Enabled = true;
            lblMixItUp.BackColor = Color.Goldenrod;
            do {
                if (Shared.InitMIU(Shared.CurSettings.MixItUpURL)) {
                    lblMixItUp.BackColor = Color.Green;
                    Shared.MixItUpConnected = true;
                    //UpdateMiuTimerEvents(ref TimerEvents);
                } else {
                    lblMixItUp.BackColor = Color.Red;
                    Shared.MixItUpConnected = false;
                }
            } while (!Shared.MixItUpConnected);
        }

        /*private void UpdateMiuTimerEvents(ref List<TimerEvent> timerEvents) {
            foreach (MIUEvent timerEvent in timerEvents.OfType<MIUEvent>().ToList()  ) {
                timerEvent.UpdateMiuCmdId();
            }
        }*/

        private void Connect() {
            if (Shared.CurSettings.VNyanURL.Length > 0) {
                Task.Run(() => ConnectVNyan());
            } else {
                lblVNyan.BackColor = SystemColors.Control;
                lblVNyan.Enabled = false;
                Shared.VNyanConnected = false;
            }
            if (Shared.CurSettings.MixItUpURL.Length > 0) {
                Task.Run(() => ConnectMixItUp());
            } else {
                lblMixItUp.BackColor = SystemColors.Control;
                lblMixItUp.Enabled = false;
            }
        }

        private void Clock_Load(object sender, EventArgs e) {
            TextBackgroundColor = Shared.CurSettings.BGCol;
            lblNextEvent.Text = DefaultStatusBar;
            UpdateClock(txtSeconds.Text);
            this.Location = Shared.CurSettings.Location;
            this.Size = Shared.CurSettings.Dimensions;
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

        public void StartCountdown(int CountdownTime) {
            SecondsToGo = CountdownTime;
            timer1.Enabled = true;
        }

        private void UpdateClock(int SecondsToGo) {
            lblCountdown.Text = TimeSpan.FromSeconds(SecondsToGo).ToString(Shared.TimeFormat);
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

            n = toolStripProgressBar1.Maximum - SecondsToGo;
            if (n < 0) {
                toolStripProgressBar1.Value = 0;
            } else {
                toolStripProgressBar1.Value = n;
            }
            for (n = 0; n < TimerEvents.Count; n++) {
                if (TimerEvents[n].Enabled) {
                    if ((TimerEvents[n].Time.TotalSeconds < SecondsToGo) && !TimerEvents[n].HasFired) {
                        StatusLabel = "Next Event in " + (SecondsToGo - TimerEvents[n].Time.TotalSeconds) + "s: " + TimerEvents[n].Time.ToString(Shared.TimeFormat) + " (" + TimerEvents[n].EventType + ") " + TimerEvents[n].Payload;
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

        private void Clock_Shown(object sender, EventArgs e) {
            Connect();
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            Config frmConfig = new Config();
            frmConfig.ShowDialog();
            //if (EventEditor.DialogResult == DialogResult.OK) {
            //    TimerEvents = new List<TimerEvent>(EventEditor.FormTimerEvents);
            //}
        }
    }
}

