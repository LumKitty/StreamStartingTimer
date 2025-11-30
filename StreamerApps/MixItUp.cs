using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer.StreamerApps {
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
                    Enabled = false;
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
                //string Content = "{ \"Platform\": \"" + Platform + "\", \"Arguments\": \"" + Arguments + "\" }";
                JObject Content = new JObject(
                    new JProperty("Platform", Platform),
                    new JProperty("Arguments", Arguments)
                );

                var jsonData = new StringContent(JsonConvert.SerializeObject(Content, Shared.SerialSettings), Encoding.ASCII);
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
            Shared.MIUConnector.Send(Payload, Platform.ToString(), Arguments);
        }

        public override void TestFire() {
            Task.Run(() => _Fire());
        }

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
                Platform = Shared.MIUConnector.GetMiuPlatform(JSON.Platform.ToString(), Shared.CurSettings.MixItUpPlatform);
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
}
