using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer {
    public enum MIUPlatforms {
        Twitch = 0,
        YouTube = 1,
        Trovo = 2
    }
    public class Settings {
        [DisplayName("VNyan URL")]
        [CategoryAttribute("Config"), DescriptionAttribute("The URL of VNyan's websocket API")]
        public virtual string VNyanURL { get; set; }

        [CategoryAttribute("Config"), DescriptionAttribute("The URL of MixItUp's HTTP API\r\nPlease remember to enable this in MIU: Settings -> Developer API -> Connect")]
        [DisplayName("MixItUp URL")]
        public virtual string MixItUpURL { get; set; }

        [CategoryAttribute("Appearance"), DescriptionAttribute("Clock font")]
        public virtual Font Font { get; set; }

        [CategoryAttribute("Appearance"), DescriptionAttribute("Clock font colour")]
        [DisplayName("Clock foreground colour")]
        public virtual Color FGCol { get; set; }

        [DisplayName("Clock background colour")]
        [CategoryAttribute("Appearance"), DescriptionAttribute("Please remember to greenscreen this in OBS using a Chroma Key filter")]
        public virtual Color BGCol { get; set; }

        [DisplayName("Clock text alignment")]
        [CategoryAttribute("Appearance"), DescriptionAttribute("Please set this to match your \"Positional Alignment\" setting in the OBS transform")]
        public virtual ContentAlignment Alignment { get; set; }

        [Browsable(false)]
        public virtual Point Location { get; set; }

        [Browsable(false)]
        public virtual Size Dimensions { get; set; }

        [DisplayName("Default MIU platform")]
        [CategoryAttribute("Config"), DescriptionAttribute("Set to your primary streaming platform. Can be overridden per-event if necessary")]
        public virtual MIUPlatforms MixItUpPlatform { get; set; }

        [DisplayName("Default time")]
        [CategoryAttribute("Config"), DescriptionAttribute("When the app starts or the timer is reset, what do we reset to?")]
        public TimeSpan TestTime { get; set; }

        private void Init() {
            Font font = new Font("Arial", 72);
            BGCol = Color.FromArgb(0, 255, 0);
            FGCol = Color.FromArgb(0, 0, 0);
            Alignment = ContentAlignment.TopLeft;
            VNyanURL = "ws://localhost:8000/vnyan";
            MixItUpURL = "http://localhost:8911/api/v2";
            MixItUpPlatform = MIUPlatforms.Twitch;
            TestTime = TimeSpan.FromMinutes(5);
        }
        
        public Settings() {
            Init();
        }
        public Settings(string ConfigFile) {
            Init();
            LoadConfig(ConfigFile);
        }

        public void LoadConfig(string ConfigFile) {
            if (File.Exists(ConfigFile)) {
                dynamic Config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(ConfigFile));
                try {
                    if ((int)Config.X < SystemInformation.VirtualScreen.Width - 5 &&  // Safety in case monitor
                    (int)Config.Y < SystemInformation.VirtualScreen.Height - 5) { // size has changed
                        Location = new Point((int)Config.X, (int)Config.Y);
                        Dimensions = new Size((int)Config.Width, (int)Config.Height);
                    }
                } catch { }
                try { BGCol = ColorTranslator.FromHtml((string)Config.BackgroundColor); } catch { }
                try { FGCol = ColorTranslator.FromHtml((string)Config.ForegroundColor); } catch { }
                try { Alignment = (ContentAlignment)Config.Alignment; } catch { }
                try { Font = new Font((string)Config.FontName, (int)Config.FontSize, (FontStyle)Config.FontStyle); } catch { }
                try { VNyanURL = Config.VNyanURL; } catch { }
                try { MixItUpURL = Config.MixItUpURL; } catch { }
                try { MixItUpPlatform = Shared.GetMiuPlatform(Config.MixItUpPlatform.ToString(), MIUPlatforms.Twitch); } catch { }
                try { TestTime = TimeSpan.FromSeconds((int)Config.TestTime); } catch { }
            } 
        }

        public void SaveConfig(string ConfigFile, Point Location, Size Dimensions) {
            JObject Config = new JObject(
                new JProperty("FontName", Font.Name),
                new JProperty("FontSize", Font.Size),
                new JProperty("FontStyle", (int)Font.Style),
                new JProperty("BackgroundColor", ColorTranslator.ToHtml(BGCol)),
                new JProperty("ForegroundColor", ColorTranslator.ToHtml(FGCol)),
                new JProperty("Alignment", Convert.ToInt32(Alignment)),
                new JProperty("X", Location.X),
                new JProperty("Y", Location.Y),
                new JProperty("Width", Dimensions.Width),
                new JProperty("Height", Dimensions.Height),
                new JProperty("VNyanURL", VNyanURL),
                new JProperty("MixItUpURL", MixItUpURL),
                new JProperty("MixItUpPlatform", MixItUpPlatform.ToString()),
                new JProperty("TestTime", TestTime.TotalSeconds)
            );
            File.WriteAllText(ConfigFile, Config.ToString());
        }
        public Settings Clone() {
            Settings TempSettings = new Settings();
            TempSettings.Font = this.Font;
            TempSettings.BGCol = this.BGCol;
            TempSettings.FGCol = this.FGCol;
            TempSettings.Alignment = this.Alignment;
            TempSettings.Location = this.Location;
            TempSettings.Dimensions = this.Dimensions;
            TempSettings.VNyanURL = this.VNyanURL;
            TempSettings.MixItUpURL = this.MixItUpURL;
            TempSettings.MixItUpPlatform = this.MixItUpPlatform;
            TempSettings.TestTime = this.TestTime;
            return TempSettings;
        }
    }

    public class CSettings : Settings, INotifyPropertyChanged {
        private static Settings _Settings = new Settings();

        public CSettings() { }
        public CSettings(string ConfigFile) {
            _Settings.LoadConfig(ConfigFile);
        }

        public override string VNyanURL {
            get { return _Settings.VNyanURL; }
            set {
                if (_Settings.VNyanURL == value) return;
                _Settings.VNyanURL = value;
                RaisePropertyChanged("VNyanURL");
            }
        }
        public override string MixItUpURL {
            get { return _Settings.MixItUpURL; }
            set {
                if (_Settings.MixItUpURL == value) return;
                _Settings.MixItUpURL = value;
                RaisePropertyChanged("MixItUpURL");
            }
        }
        public override Font Font {
            get { return _Settings.Font; }
            set {
                if (_Settings.Font == value) return;
                _Settings.Font = value;
                RaisePropertyChanged("Font");
            }
        }
        public override Color FGCol {
            get { return _Settings.FGCol; }
            set {
                if (_Settings.FGCol == value) return;
                _Settings.FGCol = value;
                RaisePropertyChanged("FGCol");
            }
        }
        public override Color BGCol {
            get { return _Settings.BGCol; }
            set {
                if (_Settings.BGCol == value) return;
                _Settings.BGCol = value;
                RaisePropertyChanged("BGCol");
            }
        }
        public override ContentAlignment Alignment {
            get { return _Settings.Alignment; }
            set {
                if (_Settings.Alignment == value) return;
                _Settings.Alignment = value;
                RaisePropertyChanged("Alignment");
            }
        }
        public override Point Location {
            get { return _Settings.Location; }
            set {
                if (_Settings.Location == value) return;
                _Settings.Location = value;
                RaisePropertyChanged("Location");
            }
        }
        public override Size Dimensions {
            get { return _Settings.Dimensions; }
            set {
                if (_Settings.Dimensions == value) return;
                _Settings.Dimensions = value;
                RaisePropertyChanged("Dimensions");
            }
        }
        public override MIUPlatforms MixItUpPlatform {
            get { return _Settings.MixItUpPlatform; }
            set {
                if (_Settings.MixItUpPlatform == value) return;
                _Settings.MixItUpPlatform = value;
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
}
