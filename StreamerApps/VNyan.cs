using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace StreamStartingTimer.StreamerApps {

    public class VNyanConnector : Connector {
        public static WatsonWsClient wsClient;
        public static CancellationToken CT = new CancellationToken();

        void VNyanConnected(object sender, EventArgs args) {
            StatusColor = StatusColors.Connected;
            Status = ConnectStatus.Connected;
            RaisePropertyChanged("StatusColor");
        }
        void VNyanDisconnected(object sender, EventArgs args) {
            StatusColor = StatusColors.Error;
            Status = ConnectStatus.Disconnected;
            RaisePropertyChanged("StatusColor");
        }

        public void Send(string Payload) {
            if (Status != ConnectStatus.Connected) {
                MessageBox.Show("VNyan not connected");
            } else {
                wsClient.SendAsync(Payload, WebSocketMessageType.Text, CT);
            }
        }

        protected override async Task _Connect() {
            int n = 0;
            Enabled = true;
            StatusColor = StatusColors.Connecting;
            NotifyStatusChange();
            wsClient = new WatsonWsClient(new Uri(Shared.CurSettings.VNyanURL));
            wsClient.KeepAliveInterval = 1000;
            wsClient.ServerConnected += VNyanConnected;
            wsClient.ServerDisconnected += VNyanDisconnected;
            wsClient.Start();
            while (!(Status != ConnectStatus.Connected) && n < 50) {
                Thread.Sleep(100);
                n++;
            }
            if (Status != ConnectStatus.Connected) {
                StatusColor = StatusColors.Error;
                Status = ConnectStatus.Error;
                NotifyStatusChange();
            }
        }
        protected override async Task _Disconnect() {
            wsClient.Stop();
            wsClient.Dispose();
            Status = ConnectStatus.Disabled;
            StatusColor = SystemColors.Control;
            NotifyStatusChange();
        }
    }

    public class VNyanEvent : TimerEvent {
        [Category("Event"), Description("Event Type")]
        public override EventType EventType { get; } = EventType.VNyan;

        [Category("Event"), Description("Websocket message to send")]
        public override string Payload { get; set; }

        protected override async void _Fire() {
            Shared.VNyanConnector.Send(Payload);
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
}
