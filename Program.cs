using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
        VNyan = 0,
        MixItUp = 1,
        EXE = 2
    }

    public class TimerEvent {
        const string TimeFormat = @"mm\:ss";
        private bool _Enabled;
        private bool _ReFire;
        private TimeSpan _Time;
        private EventType _EventType;
        private string _Payload;
        public string MiuCmdID;
        public string MiuCmdArgs;
        public bool HasFired;
        [CategoryAttribute("Event"), DescriptionAttribute("Will this event fire")]
        public bool Enabled {
            get {
                return _Enabled;
            }
            set {
                _Enabled = value;
            }
        }
        [CategoryAttribute("Event"), DescriptionAttribute("Remaining time when this event fires")]
        public TimeSpan Time {
            get {
                return _Time;
            }
            set {
                _Time = value;
            }
        }
        [CategoryAttribute("Event"), DescriptionAttribute("Application to launch")]
        public EventType EventType {
            get {
                return _EventType;
            }
            set {
                _EventType = value;
            }
        }
        [CategoryAttribute("Event"), DescriptionAttribute("What to run")]
        public string Payload {
            get {
                return _Payload;
            }
            set {
                _Payload = value;
            }
        }
        [CategoryAttribute("Event"), DescriptionAttribute("If timer is increased, will this fire a second (or more) time?")]
        public bool Refire {
            get {
                return _ReFire;
            }
            set {
                _ReFire = value;
            }
        }
        [Browsable(false)]
        public string[] Columns {
            get {
                string[] Result = new string[3];
                Result[0] = _Time.ToString(TimeFormat);
                Result[1] = _EventType.ToString();
                Result[2] = _Payload;
                return Result;
            }
        }
        [Browsable(false)]
        public JObject JSON {
            get {
                return new JObject(
                    new JProperty("Enabled", _Enabled),
                    new JProperty("ReFire", _ReFire),
                    new JProperty("Time", _Time.TotalSeconds),
                    new JProperty("EventType", _EventType.ToString()),
                    new JProperty("Payload", _Payload)
                );
            }
        }
        private async void _Fire() {
            switch (_EventType) {
                case EventType.VNyan:
                    Shared.wsClient.SendAsync(_Payload, WebSocketMessageType.Text, Shared.CT);
                    break;
                case EventType.MixItUp:
                    if (MiuCmdID != "") {
                        string Content = "{ \"Platform\": \"" + Shared.MixItUpPlatform + "\", \"Arguments\": \"" + MiuCmdArgs + "\" }";
                        var jsonData = new StringContent(Content, Encoding.ASCII);
                        jsonData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        string Response = "";
                        int httpStatus = 0;
                        var PostResult = await Shared.client.PostAsync(Shared.MixItUpURL + "/commands/" + MiuCmdID, jsonData);
                        Response = PostResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PostResult.StatusCode);
                        Console.WriteLine(PostResult.ToString());
                        PostResult.Dispose();
                    } else {
                        Shared.AutoClosingMessageBox.Show(MiuCmdID, "Can't fire "+_Payload, 10000);
                    }
                        break;
                case EventType.EXE:
                    string FileName;
                    string Parameters;
                    int i;
                    if (_Payload[0] == '"') {
                        FileName = _Payload.Substring(1);
                        i = FileName.IndexOf('"');
                        Parameters = FileName.Substring(i + 1).Trim();
                        FileName = FileName.Substring(0, i);
                    } else {
                        i = _Payload.IndexOf(' ');
                        if (i > 0) {
                            Parameters = _Payload.Substring(i + 1).Trim();
                            FileName = _Payload.Substring(0, i);
                        } else {
                            Parameters = "";
                            FileName = _Payload;
                        }
                    }
                    Process EXE = new Process();
                    EXE.StartInfo.FileName = FileName;
                    EXE.StartInfo.Arguments = Parameters;
                    EXE.StartInfo.UseShellExecute = false;
                    EXE.StartInfo.RedirectStandardInput = false;
                    EXE.StartInfo.RedirectStandardOutput = false;
                    EXE.StartInfo.RedirectStandardError = false;
                    EXE.EnableRaisingEvents = true;
                    //Log("Start process");
                    EXE.Start();

                    break;
            }
        }
        public void Fire() {
            Task.Run(() => _Fire());
        }
        public void UpdateMiuCmdId() {
            string Command;
            if (_Payload.Contains(' ')) {
                int n = _Payload.IndexOf(' ');
                Command = _Payload.Substring(0, n).Trim().ToLower();
                MiuCmdArgs = _Payload.Substring(n + 1).Trim();
            } else {
                Command = _Payload.Trim().ToLower();
                MiuCmdArgs = "";
            }
            if (Shared.miuCommands.ContainsKey(Command)) {
                MiuCmdID = Shared.miuCommands[Command];
            } else if (Shared.miuCommands.Values.Contains(Command) ) { 
                MiuCmdID = Command;
            //} else { 
            //    Shared.AutoClosingMessageBox.Show("command not found: "+Command,"MixItUpError", 10000); return;
            }
        }

        public TimerEvent(bool Enabled, bool ReFire, int Time, EventType EventType, string Payload) {
            _Enabled = Enabled;
            _ReFire = ReFire;
            _Time = TimeSpan.FromSeconds(Time);
            _EventType = EventType;
            _Payload = Payload;
            if ((EventType == EventType.MixItUp) && (_Payload != "") && Shared.MixItUpConnected) { 
                UpdateMiuCmdId(); 
            } else {
                MiuCmdID = "";
                MiuCmdArgs = "";
            }
        }
        public TimerEvent() {
            _Enabled = false;
            _ReFire = false;
            _Time = TimeSpan.FromSeconds(0);
            _EventType = EventType.VNyan;
            _Payload = "";
        }
    }
 
    public static class Shared {
        public static bool VNyanConnected = false;
        public static bool MixItUpConnected = false;
        public static string VNyanURL;
        public static string MixItUpURL;
        public static string MixItUpPlatform;
        public static WatsonWsClient wsClient;
        public static CancellationToken CT = new System.Threading.CancellationToken();
        public static HttpClient client = new HttpClient();
        public static ConcurrentDictionary<String, String> miuCommands = new ConcurrentDictionary<string, string>();

        public static bool InitMIU(string URL) {
            miuCommands.Clear();
            MixItUpURL = URL;
            var GetResult = new HttpResponseMessage(HttpStatusCode.Forbidden);
            int skip = 0;
            int count = 0;
            do {
                try {
                    GetResult = client.GetAsync(MixItUpURL + "/commands?pagesize=10&skip=" + skip.ToString()).GetAwaiter().GetResult();
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

        public static List<TimerEvent> LoadEvents(string filename) {
            bool tempEnabled = false;
            bool tempReFire = false;
            int tempTime;
            EventType tempEventType=EventType.VNyan;
            string tempPayload;
            bool Success;
            List<TimerEvent> result = new List<TimerEvent>();
            dynamic TimerConfig = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(filename));
            foreach (dynamic timerEvent in TimerConfig.Events) {
                Success = true;
                switch (timerEvent.EventType.ToString().ToLower()) {
                    case "vnyan": tempEventType = EventType.VNyan; break;
                    case "mixitup": tempEventType = EventType.MixItUp; break;
                    case "exe": tempEventType = EventType.EXE; break;
                    default: Success = false; break;
                }

                try { bool.TryParse(timerEvent.Enabled.ToString(), out tempEnabled); } catch { }
                try { bool.TryParse(timerEvent.ReFire.ToString(), out tempReFire); } catch { }
                if (!(Int32.TryParse(timerEvent.Time.ToString(), out tempTime))) { Success = false; }
                tempPayload = timerEvent.Payload.ToString();
                if (Success) {
                    result.Add(new TimerEvent(tempEnabled, tempReFire, tempTime, tempEventType, tempPayload));
                } else {
                    MessageBox.Show("Could not parse:\r\n"+timerEvent.ToString()+"Skipping this event");
                }
                    
            }
            return result;
        }
        public static void SaveEvents(string filename, List<TimerEvent> TimerEvents) {
            
            JArray EventsJson = new JArray();
            foreach (TimerEvent timerEvent in TimerEvents) {
                EventsJson.Add(timerEvent.JSON);
            }
            JObject Config = new JObject(
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

                    }
                    if (o.Seconds != null ) {
                        StartTime += (int)o.Seconds;
                    }
                    if (o.Minutes!= null ) {
                        StartTime += (int)o.Minutes*60;
                    }
                }
            );
            
            Application.Run(new Clock(StartTime, EventsFile, ConfigFile));
        }
    }

}