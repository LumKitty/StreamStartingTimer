using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer.StreamerApps {
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
