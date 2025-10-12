using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamStartingTimer.StreamerApps;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace StreamStartingTimer {
    public static class Shared {
        public static CSettings CurSettings = new CSettings();
        public static List<TimerEvent> TimerEvents = new();
        public const string Version = "v1.0-RC1";
        public const string TimeFormat = @"mm\:ss";
        public const string MutexName = "uk.lum.streamstartingtimer";
        private const string MMFName = "uk.lum.streamstartingtimer.seconds";
        private const int MMFSize = 4;
        private static MemoryMappedFile mmf;
        private static MemoryMappedViewAccessor mmfAccess = null;

        public static Clock frmClock = null;
        private static EventWaitHandle s_event;

        public static VNyanConnector VNyanConnector = new VNyanConnector();
        public static MIUConnector MIUConnector = new MIUConnector();
        public static uint SecondsToGo {
            get {
                uint temp = 0;
                if (mmfAccess == null) {
                    InitialiseMMF();
                }
                mmfAccess.Read<uint>(0, out temp);
                return temp;
            }
            set {
                if (mmfAccess == null) { InitialiseMMF(); }
                mmfAccess.Write<uint>(0, ref value);
            }
        }

        private static void InitialiseMMF() {
            bool Created = false;
            try {
                mmf = MemoryMappedFile.OpenExisting(MMFName);
            } catch (FileNotFoundException) {
                mmf = MemoryMappedFile.CreateOrOpen(MMFName, MMFSize);
                Created = true;
            }
            mmfAccess = mmf.CreateViewAccessor(0, MMFSize);
            if (Created) {
                uint value = 0;
                mmfAccess.Write<uint>(0, ref value);
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
        public static bool GetROStatus() {
            if (frmClock != null) {
                return frmClock.TimerRunning;
            } else {
                return false;
            }
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
            bool created;
            s_event = new EventWaitHandle(false,
                EventResetMode.ManualReset, MutexName, out created);
            if (!created) {
                s_event.Reset();
                s_event.Dispose();
            }
            return created;
        }
    }
}
