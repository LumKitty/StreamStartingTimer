using CppSharp.AST;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using WatsonWebsocket;

namespace StreamStartingTimer {
    public partial class Clock : Form {
        public int SecondsToGo { get; set; }
        private Binding bndTestTime;
        private ClockSpout ImageClock = null;
        private Task ImageClockUpdaterTask;
        public bool TimerRunning;

        private void TimeSpanToString(object sender, ConvertEventArgs cevent) {
            string TimeText;
            if (cevent.DesiredType != typeof(string)) return;
            TimeText = ((TimeSpan)cevent.Value).ToString(Shared.TimeFormat);
            if (!Shared.CurSettings.ShowLeadingZero && TimeText[0] == '0') {
                TimeText = ' ' + TimeText.Substring(1);
            }

            cevent.Value = TimeText;
        }

        public Clock(int StartTime, string EventsFile) {
            InitializeComponent();
            SecondsToGo = 0;
            lblCountdown.DataBindings.Add("BackColor", Shared.CurSettings, "BGCol");
            lblCountdown.DataBindings.Add("ForeColor", Shared.CurSettings, "FGCol");
            lblCountdown.DataBindings.Add("Font", Shared.CurSettings, "Font");
            lblCountdown.DataBindings.Add("TextAlign", Shared.CurSettings, "Alignment");
            bndTestTime = new Binding("Text", Shared.CurSettings, "TestTime");
            bndTestTime.Format += new ConvertEventHandler(TimeSpanToString);
            lblCountdown.DataBindings.Add(bndTestTime);

            if (StartTime > 0) {
                SecondsToGo = StartTime;
                QuitWhenDone = true;
            } else {
                QuitWhenDone = false;
                this.Text = this.Text += " (Setup/test mode)";
            }
        }

        const string DefaultStatusBar = Shared.Version + " - github.com/LumKitty";

        private bool QuitWhenDone = false;

        void VNyanConnected(object sender, EventArgs args) {
            lblVNyan.BackColor = Color.Green;
            Shared.VNyanConnected = true;
        }
        void VNyanDisconnected(object sender, EventArgs args) {
            lblVNyan.BackColor = Color.Red;
            Shared.VNyanConnected = false;
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
            lblNextEvent.Text = DefaultStatusBar;
            this.Location = Shared.CurSettings.Location;
            this.Size = Shared.CurSettings.Dimensions;
        }

        public void StartCountdown(int CountdownTime) {
            if (Shared.CurSettings.SpoutEnabled) {
                TimerRunning = true;
                ImageClock = new ClockSpout(Shared.CurSettings.FontDir);
            }
            lblCountdown.DataBindings.Remove(bndTestTime);
            SecondsToGo = CountdownTime;
            timer1.Enabled = true;
        }

        private void UpdateClock(TimeSpan SecondsToGo) {
            string TimeText = SecondsToGo.ToString(Shared.TimeFormat);
            if (!Shared.CurSettings.ShowLeadingZero && TimeText[0] == '0') {
                TimeText = ' ' + TimeText.Substring(1);
            }
            lblCountdown.Text = TimeText;
            if (Shared.CurSettings.SpoutEnabled) { ImageClock.UpdateTexture(); }
        }
        private void UpdateClock(int SecondsToGo) {
            UpdateClock(TimeSpan.FromSeconds(SecondsToGo));
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
            for (n = 0; n < Shared.TimerEvents.Count; n++) {
                if (Shared.TimerEvents[n].Enabled) {
                    if ((Shared.TimerEvents[n].Time.TotalSeconds < SecondsToGo) && !Shared.TimerEvents[n].HasFired) {
                        StatusLabel = "Next Event in " + (SecondsToGo - Shared.TimerEvents[n].Time.TotalSeconds) + "s: " + Shared.TimerEvents[n].Time.ToString(Shared.TimeFormat) + " (" + Shared.TimerEvents[n].EventType + ") " + Shared.TimerEvents[n].Payload;
                        i = n + 1;
                        while (i < Shared.TimerEvents.Count && Shared.TimerEvents[i].Time.TotalSeconds == Shared.TimerEvents[n].Time.TotalSeconds) {
                            if (!Shared.TimerEvents[i].HasFired) { ExtraSimultaneousEvents++; }
                            i++;
                        }
                        if (ExtraSimultaneousEvents > 0) {
                            StatusLabel += " +" + ExtraSimultaneousEvents.ToString();
                        }
                        break;
                    } else if ((Shared.TimerEvents[n].Time.TotalSeconds == SecondsToGo) && !Shared.TimerEvents[n].HasFired) {
                        i = n;
                        StatusLabel = "Firing event:";
                        while (i < Shared.TimerEvents.Count && Shared.TimerEvents[i].Time.TotalSeconds == SecondsToGo) {
                            if (!Shared.TimerEvents[i].HasFired) {
                                StatusLabel += " (" + Shared.TimerEvents[i].EventType + ") " + Shared.TimerEvents[i].Payload;
                                if (!Shared.TimerEvents[i].Refire) { Shared.TimerEvents[i].HasFired = true; }
                                Shared.TimerEvents[i].Fire();
                            }
                            i++;
                        }
                        break;
                    }
                }
            }
            lblNextEvent.Text = StatusLabel;
            if (SecondsToGo <= 0) {
                TimerRunning = false;
                timer1.Stop();
                if (Shared.CurSettings.SpoutEnabled) { ImageClock.Dispose(); }
                if (QuitWhenDone) {
                    Thread.Sleep(1000);
                    this.Close();
                }
            }
        }

        private void btnEvents_Click(object sender, EventArgs e) {
            EventEditor EventEditor = new EventEditor();
            EventEditor.FormTimerEvents = new List<TimerEvent>(Shared.TimerEvents);
            EventEditor.ShowDialog();
            if (EventEditor.DialogResult == DialogResult.OK) {
                Shared.TimerEvents = new List<TimerEvent>(EventEditor.FormTimerEvents);
            }
        }

        private void StartTimer(int Seconds) {
            StartCountdown(Seconds);
            UpdateClock(Seconds);
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnReset.Enabled = true;
            btnAdd30s.Enabled = true;
            btnAdd60s.Enabled = true;
            btnEvents.Enabled = false;
            toolStripProgressBar1.Maximum = Seconds;
            toolStripProgressBar1.Value = 0;
            foreach (TimerEvent timerEvent in Shared.TimerEvents) {
                timerEvent.HasFired = false;
            }
        }
        private void btnStart_Click(object sender, EventArgs e) {
            StartTimer((int)Shared.CurSettings.TestTime.TotalSeconds);
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
            timer1.Enabled = false;
            TimerRunning = false;
            if (Shared.CurSettings.SpoutEnabled) { ImageClock.Dispose(); }
            lblNextEvent.Text = DefaultStatusBar;
            lblCountdown.DataBindings.Add(bndTestTime);
        }

        private void btnAdd30s_Click(object sender, EventArgs e) {
            SecondsToGo += 30;
        }

        private void btnAdd60s_Click(object sender, EventArgs e) {
            SecondsToGo += 60;
        }

        private void Clock_Shown(object sender, EventArgs e) {
            Connect();
            OpenGLHandler.InitGL();
            if (SecondsToGo > 0) {
                StartTimer(SecondsToGo);
            }
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            Settings TempSettings = Shared.CurSettings.Clone();
            Config frmConfig = new Config();
            frmConfig.ShowDialog();
            if (frmConfig.DialogResult == DialogResult.Cancel) {
                Shared.CurSettings.Font = TempSettings.Font;
                Shared.CurSettings.BGCol = TempSettings.BGCol;
                Shared.CurSettings.FGCol = TempSettings.FGCol;
                Shared.CurSettings.Alignment = TempSettings.Alignment;
                Shared.CurSettings.Location = TempSettings.Location;
                Shared.CurSettings.Dimensions = TempSettings.Dimensions;
                Shared.CurSettings.VNyanURL = TempSettings.VNyanURL;
                Shared.CurSettings.MixItUpURL = TempSettings.MixItUpURL;
                Shared.CurSettings.MixItUpPlatform = TempSettings.MixItUpPlatform;
                Shared.CurSettings.TestTime = TempSettings.TestTime;
                Shared.CurSettings.SpoutEnabled = TempSettings.SpoutEnabled;
                Shared.CurSettings.FontDir = TempSettings.FontDir;
            }
        }

        private void Clock_FormClosing(object sender, FormClosingEventArgs e) {
            if (ImageClock != null) { ImageClock.Dispose(); }
            OpenGLHandler.CloseGL();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}

