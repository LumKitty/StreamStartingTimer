using Microsoft.VisualBasic;
using System;

namespace StreamStartingTimer {
    public partial class Clock : Form {
        public Clock() {
            InitializeComponent();
        }
        const string TimeFormat = "hh:mm";
        Color TextBackgroundColor;
        private int SecondsToGo;
        public List<TimerEvent> TimerEvents = new();

        private void Clock_Load(object sender, EventArgs e) {
            TextBackgroundColor = txtBackColor.BackColor;
            TimerEvents.Add(new TimerEvent(true, 60, EventType.VNyan, "nyaaaa"));
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

        private void btnTest_Click(object sender, EventArgs e) {
            StartCountdown(60 * 5);
        }
        public void StartCountdown(int CountdownTime) {
            SecondsToGo = CountdownTime;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e) {

            SecondsToGo--;
            lblCountdown.Text = TimeSpan.FromSeconds(SecondsToGo).ToString(TimeFormat); 
            if (SecondsToGo <= 0) {
                timer1.Stop();
            }
        }

        private void btnEvents_Click(object sender, EventArgs e) {
            EventEditor EventEditor = new EventEditor();
            EventEditor.TimerEvents = TimerEvents;
            EventEditor.Show();
        }
    }
}
