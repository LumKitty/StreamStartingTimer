using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer {

    public enum EventType {
        None = -1,
        VNyan = 0,
        MixItUp = 1,
        EXE = 2
    }
    public abstract class TimerEvent {
        [CategoryAttribute("Timing"), DescriptionAttribute("Will this event fire")]
        public bool Enabled { get; set; }

        [CategoryAttribute("Event"), DescriptionAttribute("What to run")]
        public abstract string Payload { get; set; }

        [CategoryAttribute("Timing"), DescriptionAttribute("If timer is increased, will this fire a second (or more) time?")]
        public bool Refire { get; set; }

        [CategoryAttribute("Timing"), DescriptionAttribute("Remaining time when this event fires")]
        public TimeSpan Time { get; set; }

        public bool HasFired;
        public abstract EventType EventType { get; }
        protected abstract void _Fire();
        public void Fire() {
            Task.Run(() => _Fire());
        }
        public virtual void TestFire() {
            Task.Run(() => _Fire());
        }

        [Browsable(false)]
        public abstract JObject JSON { get; }

        [Browsable(false)]
        public string[] Columns {
            get {
                string[] Result = new string[3];
                Result[0] = Time.ToString(Shared.TimeFormat);
                Result[1] = EventType.ToString();
                Result[2] = Payload;
                return Result;
            }
        }
        protected void SetEnabled(dynamic JSON) {
            bool tempEnabled = false;
            try { bool.TryParse(JSON.Enabled.ToString(), out tempEnabled); } catch { }
            Enabled = tempEnabled;
        }
        protected void SetRefire(dynamic JSON) {
            bool tempRefire = false;
            try { bool.TryParse(JSON.ReFire.ToString(), out tempRefire); } catch { }
            Refire = tempRefire;
        }
        protected void SetTime(dynamic JSON) {
            Int32 tempTime = 0;
            if (!(Int32.TryParse(JSON.Time.ToString(), out tempTime))) { tempTime = 0; }
            Time = TimeSpan.FromSeconds(tempTime);
        }
        protected void SetPayload(dynamic JSON) {
            try { Payload = JSON.Payload.ToString(); } catch { Payload = ""; }
        }
    }

    public class VNyanEvent : TimerEvent {
        [CategoryAttribute("Event"), DescriptionAttribute("Event Type")]
        public override EventType EventType { get; } = EventType.VNyan;

        [CategoryAttribute("Event"), DescriptionAttribute("Websocket message to send")]
        public override string Payload { get; set; }

        protected override async void _Fire() {
            Shared.wsClient.SendAsync(Payload, WebSocketMessageType.Text, Shared.CT);
        }
        public VNyanEvent(bool _Enabled, bool _ReFire, int _Time, string _Payload) {
            Enabled = _Enabled;
            Refire = _ReFire;
            Time = TimeSpan.FromSeconds(_Time);
            Payload = _Payload;
        }
        public VNyanEvent() {
            Enabled = false;
            Refire = false;
            Time = TimeSpan.FromSeconds(0);
            Payload = "";
        }
        public VNyanEvent(dynamic JSON) {
            SetEnabled(JSON);
            SetRefire(JSON);
            SetTime(JSON);
            SetPayload(JSON);
        }
        public override JObject JSON {
            get {
                return new JObject(
                    new JProperty("EventType", EventType.ToString()),
                    new JProperty("Enabled", Enabled),
                    new JProperty("ReFire", Refire),
                    new JProperty("Time", Time.TotalSeconds),
                    new JProperty("Payload", Payload)
                );
            }
        }
    }

    public class MIUEvent : TimerEvent {

        [CategoryAttribute("Event"), DescriptionAttribute("Event Type")]
        public override EventType EventType { get; } = EventType.MixItUp;
        [CategoryAttribute("Event"), DescriptionAttribute("MIU Command to run")]
        public override string Payload { get; set; }
        [CategoryAttribute("Event"), DescriptionAttribute("Command arguments")]
        public string Arguments { get; set; }
        [CategoryAttribute("Event"), DescriptionAttribute("Streaming Platform")]
        public MIUPlatforms Platform { get; set; }

        protected override async void _Fire() {
            string MiuCmdID = GetMiuCmdId();
            if (MiuCmdID != "") {
                string Content = "{ \"Platform\": \"" + Platform + "\", \"Arguments\": \"" + Arguments + "\" }";
                var jsonData = new StringContent(Content, Encoding.ASCII);
                jsonData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Response = "";
                int httpStatus = 0;
                var PostResult = await Shared.client.PostAsync(Shared.CurSettings.MixItUpURL + "/commands/" + MiuCmdID, jsonData);
                Response = PostResult.Content.ReadAsStringAsync().Result;
                httpStatus = ((int)PostResult.StatusCode);
                Console.WriteLine(PostResult.ToString());
                PostResult.Dispose();
            } else {
                Shared.AutoClosingMessageBox.Show(MiuCmdID, "Can't fire " + Payload, 10000);
            }
        }

        private string GetMiuCmdId() {
            if (Shared.miuCommands.ContainsKey(Payload)) {
                return Shared.miuCommands[Payload];
            } else {
                Shared.AutoClosingMessageBox.Show("command not found: " + Payload, "MixItUpError", 10000);
                return "";
            }
        }

        public override void TestFire() {
            if (GetMiuCmdId() == "") { Shared.InitMIU(Shared.CurSettings.MixItUpURL); }
            Task.Run(() => _Fire());
        }

        /*public void UpdateMiuCmdId() {
            if (MiuCmdID == "") {
                if (Shared.miuCommands.ContainsKey(Payload)) {
                    MiuCmdID = Shared.miuCommands[Payload];
                } else {
                    Shared.AutoClosingMessageBox.Show("command not found: " + Payload, "MixItUpError", 10000); return;
                }
            }
        }*/

        public MIUEvent(bool _Enabled, bool _ReFire, int _Time, string _Payload, string _Arguments, MIUPlatforms _Platform) {
            Enabled = _Enabled;
            Refire = _ReFire;
            Time = TimeSpan.FromSeconds(_Time);
            Payload = _Payload;
            Arguments = _Arguments;
            Platform = _Platform;
        }
        public MIUEvent() {
            Enabled = false;
            Refire = false;
            Time = TimeSpan.FromSeconds(0);
            Payload = "";
            Arguments = "";
            Platform = Shared.CurSettings.MixItUpPlatform;
        }
        public MIUEvent(dynamic JSON) {
            SetEnabled(JSON);
            SetRefire(JSON);
            SetTime(JSON);
            SetPayload(JSON);
            try { Arguments = JSON.Arguments.ToString(); } catch { Arguments = ""; }
            try {
                Shared.GetMiuPlatform(JSON.Platform.ToString(), Shared.CurSettings.MixItUpPlatform);
            } catch { Platform = Shared.CurSettings.MixItUpPlatform; }

        }
        public override JObject JSON {
            get {
                return new JObject(
                    new JProperty("EventType", EventType.ToString()),
                    new JProperty("Enabled", Enabled),
                    new JProperty("ReFire", Refire),
                    new JProperty("Time", Time.TotalSeconds),
                    new JProperty("Payload", Payload),
                    new JProperty("Arguments", Arguments),
                    new JProperty("Platform", Platform.ToString())
                );
            }
        }
    }

    public class ExeEvent : TimerEvent {
        [CategoryAttribute("Event"), DescriptionAttribute("Event Type")]
        public override EventType EventType { get; } = EventType.EXE;
        [CategoryAttribute("Event"), DescriptionAttribute("Full path to EXE to run")]
        public override string Payload { get; set; }
        [CategoryAttribute("Event"), DescriptionAttribute("Parameters to pass to EXE")]
        public string Arguments { get; set; }


        [CategoryAttribute("Event"), DescriptionAttribute("Working directory of the process")]
        public string StartIn { get; set; }

        [CategoryAttribute("Event"), DescriptionAttribute("How to show the window")]
        public ProcessWindowStyle WindowStyle { get; set; }

        [CategoryAttribute("Event"), DescriptionAttribute("Use Shell Execute")]
        public bool ShellExecute { get; set; }


        protected override async void _Fire() {
            Process EXE = new Process();
            EXE.StartInfo.FileName = Payload;
            EXE.StartInfo.Arguments = Arguments;
            EXE.StartInfo.UseShellExecute = ShellExecute;
            EXE.StartInfo.RedirectStandardInput = false;
            EXE.StartInfo.RedirectStandardOutput = false;
            EXE.StartInfo.RedirectStandardError = false;
            EXE.StartInfo.WindowStyle = WindowStyle;
            EXE.StartInfo.WorkingDirectory = StartIn;
            EXE.EnableRaisingEvents = true;
            //Log("Start process");
            EXE.Start();
        }
        public ExeEvent(bool _Enabled, bool _ReFire, int _Time, string _Payload, string _Arguments, string _StartIn, bool _ShellExecute, ProcessWindowStyle _WindowStyle) {
            Enabled = _Enabled;
            Refire = _ReFire;
            Time = TimeSpan.FromSeconds(_Time);
            Payload = _Payload;
            Arguments = _Arguments;
            StartIn = _StartIn;
            ShellExecute = _ShellExecute;
            WindowStyle = _WindowStyle;
        }
        public ExeEvent() {
            Enabled = false;
            Refire = false;
            Time = TimeSpan.FromSeconds(0);
            Payload = "";
            Arguments = "";
            StartIn = "";
            ShellExecute = false;
            WindowStyle = ProcessWindowStyle.Minimized;
        }
        public ExeEvent(dynamic JSON) {
            SetEnabled(JSON);
            SetRefire(JSON);
            SetTime(JSON);
            SetPayload(JSON);
            try { Arguments = JSON.Arguments.ToString(); } catch { Arguments = ""; }
            try { StartIn = JSON.StartIn.ToString(); } catch { StartIn = ""; }
            string tempWindowStyle;
            try { tempWindowStyle = JSON.WindowStyle.ToString(); } catch { tempWindowStyle = "Normal"; }
            switch (tempWindowStyle) {
                case "Normal": WindowStyle = ProcessWindowStyle.Normal; break;
                case "Minimized": WindowStyle = ProcessWindowStyle.Minimized; break;
                case "Maximized": WindowStyle = ProcessWindowStyle.Maximized; break;
                case "Hidden": WindowStyle = ProcessWindowStyle.Hidden; break;
            }
            bool tempShellExecute = false;
            try { bool.TryParse(JSON.ShellExecute.ToString(), out tempShellExecute); } catch { }
            ShellExecute = tempShellExecute;
        }
        public override JObject JSON {
            get {
                return new JObject(
                    new JProperty("EventType", EventType.ToString()),
                    new JProperty("Enabled", Enabled),
                    new JProperty("ReFire", Refire),
                    new JProperty("Time", Time.TotalSeconds),
                    new JProperty("Payload", Payload),
                    new JProperty("Arguments", Arguments),
                    new JProperty("StartIn", StartIn),
                    new JProperty("ShellExecute", ShellExecute),
                    new JProperty("WindowStyle", WindowStyle.ToString())
                );
            }
        }
    }

}
