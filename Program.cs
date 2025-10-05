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
    public static class Shared {
        public static CSettings CurSettings = new CSettings();
        public static List<TimerEvent> TimerEvents = new();
        public const string Version = "v0.7";
        public const string TimeFormat = @"mm\:ss";
        public const string MutexName = "uk.lum.streamstartingtimer";
        
        public static Clock frmClock;
        public static Mutex Mutex;

        public static VNyanConnector VNyanConnector = new VNyanConnector();
        public static MIUConnector MIUConnector = new MIUConnector();

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
        public static bool IsSingleInstance() {
            try {
                Mutex.OpenExisting(Shared.MutexName);
            } catch {
                Shared.Mutex = new Mutex(true, Shared.MutexName);
                return true;
            }
            // More than one instance.
            return false;
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
            while (!Shared.IsSingleInstance()) {
                MessageBox.Show("Another copy of this program is already running", "Mutex Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int StartTime = 0;
            string ConfigFile = Application.StartupPath + "DefaultConfig.json";
            string EventsFile = Application.StartupPath + "DefaultEvents.json";
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
            Shared.CurSettings = new CSettings();
            if (!File.Exists(ConfigFile)) {
                if (MessageBox.Show("This appears to be the first time you have run this program. Would you like to view the instructions", "Welcome", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    Process myProcess = new Process();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = "https://github.com/LumKitty/StreamStartingTimer/blob/master/README.md";
                    myProcess.Start();
                    myProcess.Dispose();
                }
            } else { 
                Shared.CurSettings.LoadConfig(ConfigFile);
            }
            if (File.Exists(EventsFile)) {
                Shared.TimerEvents = Shared.LoadEvents(EventsFile);
            }
            Shared.frmClock = new Clock(StartTime, EventsFile);
            Application.Run(Shared.frmClock);
        }
    }

}