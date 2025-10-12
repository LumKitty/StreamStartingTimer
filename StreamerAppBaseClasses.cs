using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace StreamStartingTimer {
    public enum ConnectStatus {
        Disabled = -1,
        Disconnected = 0,
        Connecting = 1,
        Error = 2,
        Connected = 3
    }
    public enum MIUPlatforms {
        Twitch = 0,
        YouTube = 1,
        Trovo = 2
    }
    public enum EventType {
        None = -1,
        VNyan = 0,
        MixItUp = 1,
        EXE = 2
    }

    public static class StatusColors {
        public static readonly Color Connecting = Color.Goldenrod;
        public static readonly Color Error = Color.Red;
        public static readonly Color Connected = Color.Green;
        public static readonly Color Disabled = SystemColors.Control;
    }

    public abstract class Connector : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void NotifyStatusChange() {
            RaisePropertyChanged("Enabled");
            RaisePropertyChanged("StatusColor");
        }
        protected static ConnectStatus _Status = ConnectStatus.Disabled;
        protected static Color _StatusColor = StatusColors.Disabled;
        protected static bool _Enabled = false;
        public ConnectStatus Status {
            get { return _Status; }
            set { _Status = value; }
        }
        public Color StatusColor {
            get { return _StatusColor; }
            set { _StatusColor = value; }
        }
        public bool Enabled {
            get { return _Enabled; }
            set {_Enabled = value; }
        }

        protected abstract Task _Connect();
        protected abstract Task _Disconnect();
        public void Connect() {
            Task.Run(() => _Connect());
        }
        public void Disconnect() {
            Task.Run(() => _Disconnect());
        }
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
}
