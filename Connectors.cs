using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using WatsonWebsocket;

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

    public class VNyanConnector : Connector {
        public static WatsonWsClient wsClient;
        public static CancellationToken CT = new System.Threading.CancellationToken();

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

    public class MIUConnector : Connector {
        private static ConcurrentDictionary<String, String> miuCommands = new ConcurrentDictionary<string, string>();
        private static HttpClient client = new HttpClient();

        public ConcurrentDictionary<String, String> GetMiuCommands() { return miuCommands; }

        private string GetMiuCmdId(string Payload) {
            Payload = Payload.Trim().ToLower();
            if (miuCommands.ContainsKey(Payload)) {
                return miuCommands[Payload];
            } else {
                Shared.AutoClosingMessageBox.Show("command not found: " + Payload, "MixItUpError", 10000);
                return "";
            }
        }
        public MIUPlatforms GetMiuPlatform(string Platform, MIUPlatforms Default) {
            switch (Platform.ToLower()) {
                case "twitch": return MIUPlatforms.Twitch;
                case "youtube": return MIUPlatforms.YouTube;
                case "trovo": return MIUPlatforms.Trovo;
                default: return Default;
            }
        }

        protected override async Task _Connect() {
            miuCommands.Clear();
            Enabled = true;
            StatusColor = StatusColors.Connecting;
            NotifyStatusChange();
            var GetResult = new HttpResponseMessage(HttpStatusCode.Forbidden);
            int skip = 0;
            int count = 0;
            do {
                try {
                    GetResult = client.GetAsync(Shared.CurSettings.MixItUpURL + "/commands?pagesize=10&skip=" + skip.ToString()).GetAwaiter().GetResult();
                } catch (Exception e) {
                    Enabled=false;
                    Status = ConnectStatus.Error;
                    StatusColor = StatusColors.Error;
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
            if (miuCommands.Count <= 0) {
                Status = ConnectStatus.Error;
                StatusColor = StatusColors.Error;
                NotifyStatusChange();
            } else {
                Status = ConnectStatus.Connected;
                StatusColor = StatusColors.Connected;
                NotifyStatusChange();
            }
        }
        protected override async Task _Disconnect() {
            miuCommands.Clear();
            Status = ConnectStatus.Disabled;
            StatusColor = StatusColors.Disabled;
        }
        public async Task Send(string Payload, string Platform, string Arguments) {
            string MiuCmdID = GetMiuCmdId(Payload);
            //if (GetMiuCmdId() == "") { Shared.InitMIU(Shared.CurSettings.MixItUpURL); }
            if (MiuCmdID != "") {
                string Content = "{ \"Platform\": \"" + Platform + "\", \"Arguments\": \"" + Arguments + "\" }";
                var jsonData = new StringContent(Content, Encoding.ASCII);
                jsonData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Response = "";
                int httpStatus = 0;
                var PostResult = await client.PostAsync(Shared.CurSettings.MixItUpURL + "/commands/" + MiuCmdID, jsonData);
                Response = PostResult.Content.ReadAsStringAsync().Result;
                httpStatus = ((int)PostResult.StatusCode);
                Console.WriteLine(PostResult.ToString());
                PostResult.Dispose();
            } else {
                Shared.AutoClosingMessageBox.Show(MiuCmdID, "Can't fire " + Payload, 10000);
            }
        }
    }
}
