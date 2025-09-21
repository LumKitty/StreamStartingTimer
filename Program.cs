using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Net;
using System.Net.WebSockets;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using WatsonWebsocket;

namespace StreamStartingTimer
{

    public enum EventType {
        None = -1,
        VNyan = 0,
        MixItUp = 1,
        EXE = 2
    }
    public enum MIUPlatforms {
        Twitch = 0,
        YouTube = 1,
        Trovo = 2
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

    public class Settings {
        public virtual string VNyanURL { get; set; }
        public virtual string MixItUpURL { get; set; }
        public virtual Font Font { get; set; }
        public virtual Color FGCol { get; set; }
        public virtual Color BGCol { get; set; }
        public virtual ContentAlignment Alignment { get; set; }
        public virtual Point Location { get; set; }
        public virtual Size Dimensions { get; set; }
        public virtual MIUPlatforms MixItUpPlatform { get; set; }
        public TimeSpan TestTime { get; set; }
    }

    public class CSettings : Settings, INotifyPropertyChanged {
        private static Settings Settings = new Settings();

        public string VNyanURL {
            get { return Settings.VNyanURL; }
            set {
                if (Settings.VNyanURL == value) return;
                Settings.VNyanURL = value;
                RaisePropertyChanged("VNyanURL");
            }
        }
        public string MixItUpURL {
            get { return Settings.MixItUpURL; }
            set {
                if (Settings.MixItUpURL == value) return;
                Settings.MixItUpURL = value;
                RaisePropertyChanged("MixItUpURL");
            }
        }
        public Font Font {
            get { return Settings.Font; }
            set {
                if (Settings.Font == value) return;
                Settings.Font = value;
                RaisePropertyChanged("Font");
            }
        }
        public Color FGCol {
            get { return Settings.FGCol; }
            set {
                if (Settings.FGCol == value) return;
                Settings.FGCol = value;
                RaisePropertyChanged("FGCol");
            }
        }
        public Color BGCol {
            get { return Settings.BGCol; }
            set {
                if (Settings.BGCol == value) return;
                Settings.BGCol = value;
                RaisePropertyChanged("BGCol");
            }
        }
        public ContentAlignment Alignment {
            get { return Settings.Alignment; }
            set {
                if (Settings.Alignment == value) return;
                Settings.Alignment = value;
                RaisePropertyChanged("Alignment");
            }
        }
        public Point Location {
            get { return Settings.Location; }
            set {
                if (Settings.Location == value) return;
                Settings.Location = value;
                RaisePropertyChanged("Location");
            }
        }
        public Size Dimensions {
            get { return Settings.Dimensions; }
            set {
                if (Settings.Dimensions == value) return;
                Settings.Dimensions = value;
                RaisePropertyChanged("Dimensions");
            }
        }
        public MIUPlatforms MixItUpPlatform {
            get { return Settings.MixItUpPlatform; }
            set {
                if (Settings.MixItUpPlatform == value) return;
                Settings.MixItUpPlatform = value;
                RaisePropertyChanged("MixItUpPlatform");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
    }

    public static class Shared {
        public static CSettings CurSettings = new CSettings();
        public const string Version = "v0.2";
        public const string TimeFormat = @"mm\:ss";
        public static bool VNyanConnected = false;
        public static bool MixItUpConnected = false;
        
        public static WatsonWsClient wsClient;
        public static CancellationToken CT = new System.Threading.CancellationToken();
        public static HttpClient client = new HttpClient();
        public static ConcurrentDictionary<String, String> miuCommands = new ConcurrentDictionary<string, string>();

        public static bool InitMIU(string URL) {
            miuCommands.Clear();
            CurSettings.MixItUpURL = URL;
            var GetResult = new HttpResponseMessage(HttpStatusCode.Forbidden);
            int skip = 0;
            int count = 0;
            do {
                try {
                    GetResult = client.GetAsync(CurSettings.MixItUpURL + "/commands?pagesize=10&skip=" + skip.ToString()).GetAwaiter().GetResult();
                } catch (Exception e) {
                    return false;
                }
                string Response = GetResult.Content.ReadAsStringAsync().Result;

                dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
                count = Results.Commands.Count;
                // Console.WriteLine("Count: " + count.ToString());
                foreach (dynamic Result in Results.Commands) {
                    // Console.WriteLine(Result.ToString());
                    // Console.WriteLine(Result.Name + " : " + Result.ID);
                    // If this command already exists, then we're fine, so can ignore the fail
                    miuCommands.TryAdd(Result.Name.ToString().Trim().ToLower(), Result.ID.ToString());
                    // Console.WriteLine("Added");
                }
                GetResult.Dispose();
                skip += 10;
            } while (count >= 10);
            return (miuCommands.Count > 0);
        }

        public static MIUPlatforms GetMiuPlatform(string Platform, MIUPlatforms Default) {
            switch (Platform.ToLower()) {
                case "twitch":  return MIUPlatforms.Twitch;
                case "youtube": return MIUPlatforms.YouTube;
                case "trovo":   return MIUPlatforms.Trovo;
                default:        return Default;
            }
        }

        public static List<TimerEvent> LoadEvents(string filename) {
            int FileVersion = 0;
            List<TimerEvent> result = new List<TimerEvent>();
            dynamic JSON = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(filename));

            if (!(Int32.TryParse(JSON.FileVersion.ToString(), out FileVersion))) { FileVersion = 0; }

            switch (FileVersion) {
                case 0:
                    MessageBox.Show("Unfortunately we can't read event lists from beta 1.0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 1:
                    foreach (dynamic JSONEvent in JSON.Events) {
                        switch (JSONEvent.EventType.ToString().ToLower()) {
                            case "vnyan": result.Add(new VNyanEvent(JSONEvent)); break;
                            case "mixitup": result.Add(new MIUEvent(JSONEvent)); break;
                            case "exe": result.Add(new ExeEvent(JSONEvent)); break;
                        }
                    }
                    break;
            }
            return result;
        }
        public static void SaveEvents(string filename, List<TimerEvent> TimerEvents) {
            JArray EventsJson = new JArray();
            foreach (TimerEvent timerEvent in TimerEvents) {
                EventsJson.Add(timerEvent.JSON);
            }
            JObject Config = new JObject(
                new JProperty("FileVersion", 1),
                new JProperty("Events", EventsJson)
            );
            File.WriteAllText(filename, Config.ToString());
        }
        public class AutoClosingMessageBox {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout) {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout) {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state) {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
    }

    class Options {
        [Option('s', "seconds", Required = false, HelpText = "Run clock for this number of seconds")] public int? Seconds { get; set; }
        [Option('m', "minutes", Required = false, HelpText = "Run clock for this number of minutes")] public int? Minutes { get; set; }
        [Option('p', "past", Required = false, HelpText = "Run clock until time is this many minutes past the hour")] public int? Past { get; set; }
        [Option('e', "events", Required = false, HelpText = "Load the startup events from this file instead of default")] public string? Events { get; set; }
        [Option('c', "config", Required = false, HelpText = "Load the clock configuation from this file instead of default")] public string? Config { get; set; }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]



        static void Main(string[] args) {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            int StartTime = 0;
            string ConfigFile="::";
            string EventsFile="::";
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o => {
                    if (o.Events != null) {
                        EventsFile = o.Events;
                    }
                    if (o.Config != null) {
                        ConfigFile = o.Config;
                    }
                    if (o.Past != null) {
                        DateTime dateTime = DateTime.Now;
                        DateTime trgTime;
                        if (dateTime.Minute < o.Past) {
                            trgTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, (int)o.Past, 0, 0);
                        } else {
                            trgTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + 1, (int)o.Past, 0, 0);
                        }
                        StartTime = (int)(trgTime - DateTime.Now).TotalSeconds;
                    }
                    if (o.Seconds != null) {
                        StartTime += (int)o.Seconds;
                    }
                    if (o.Minutes != null) {
                        StartTime += (int)o.Minutes * 60;
                    }
                }
            );
            Clock frmClock = new Clock(StartTime, EventsFile, ConfigFile);
            Application.Run(frmClock);

            

        }
    }

}