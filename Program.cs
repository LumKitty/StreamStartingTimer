using System.ComponentModel;
using System.Xml.Linq;

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
        private TimeSpan _Time;
        private EventType _EventType;
        private string _Payload;
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

        public TimerEvent(bool Enabled, int Time, EventType EventType, string Payload) {
            _Enabled = Enabled;
            _Time = TimeSpan.FromSeconds(Time);
            _EventType = EventType;
            _Payload = Payload;
        }
    }
 


    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        

        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
                
            Application.Run(new Clock());
        }
    }

}