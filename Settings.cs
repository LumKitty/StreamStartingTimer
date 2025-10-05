using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer {
    static class Defaults {
        public const string VNyanURL = "ws://localhost:8000/vnyan";
        public const string MixItUpURL = "http://localhost:8911/api/v2";
        public const ContentAlignment Alignment = ContentAlignment.TopLeft;
        public const MIUPlatforms MixItUpPlatform = MIUPlatforms.Twitch;
        public static readonly Color FGCol = Color.Black;
        public static readonly Color BGCol = Color.Lime;
        public static readonly Font Font = new Font("Arial", 128);
        public const bool ShowLeadingZero = true;
        public static readonly TimeSpan TestTime = TimeSpan.FromMinutes(5);
        public static readonly Size Dimensions = new Size(600, 300);
        public static readonly Point Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width  - Dimensions.Width)  / 2 + Screen.PrimaryScreen.Bounds.X ,
                                                          (Screen.PrimaryScreen.Bounds.Size.Height - Dimensions.Height) / 2 + Screen.PrimaryScreen.Bounds.Y);
        public const bool SpoutEnabled = false;
        public const string SpoutName = "StreamStartingTimer";
        public static readonly string FontDir = Application.StartupPath + "\\DefaultFont";
        public const int ProgressYellow = 60;
        public const int ProgressRed = 30;
        //public static readonly string FontDir = "D:\\Twitch\\Software\\StreamStartingTimer\\DefaultFont";
    }

    public class Settings {

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Clock font")]
        public virtual Font Font { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Clock font colour")]
        [DisplayName("Foreground colour")]
        public virtual Color FGCol { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Please remember to greenscreen this in OBS using a Chroma Key filter\r\nYou can type a custom colour using the form: 42,0,69")]
        [DisplayName("Background colour")]
        public virtual Color BGCol { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Please set this to match your \"Positional Alignment\" setting in the OBS transform")]
        [DisplayName("Text alignment")]
        public virtual ContentAlignment Alignment { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Display 04:20 or   4:20 (does not take effect until restarting the timer)")]
        [DisplayName("Show Leading Zero")]
        public virtual bool ShowLeadingZero { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Progress bar changes to yellow when this many seconds are left")]
        [DisplayName("Progress bar yellow time")]
        public virtual int ProgressYellow { get; set; }

        [CategoryAttribute("Clock Appearance"), DescriptionAttribute("Progress bar changes to red when this many seconds are left")]
        [DisplayName("Progress bar red time")]
        public virtual int ProgressRed { get; set; }

        [Browsable(false)]
        public virtual Point Location { get; set; }

        [Browsable(false)]
        public virtual Size Dimensions { get; set; }

        [DisplayName("Default MIU platform")]
        [CategoryAttribute("Settings"), DescriptionAttribute("Set to your primary streaming platform. Can be overridden per-event if necessary")]
        public virtual MIUPlatforms MixItUpPlatform { get; set; }

        [DisplayName("Default time")]
        [CategoryAttribute("Settings"), DescriptionAttribute("When the timer is reset, what do we reset to?\r\nAlso used if the app is launched without specifying a time on the commandline")]
        public TimeSpan TestTime { get; set; }

        [DisplayName("VNyan URL")]
        [CategoryAttribute("Settings"), DescriptionAttribute("The URL of VNyan's websocket API")]
        public virtual string VNyanURL { get; set; }

        [CategoryAttribute("Settings"), DescriptionAttribute("The URL of MixItUp's HTTP API\r\nPlease remember to enable this in MIU: Settings -> Developer API -> Connect")]
        [DisplayName("MixItUp URL")]
        public virtual string MixItUpURL { get; set; }

        [CategoryAttribute("Spout Image Font"), DescriptionAttribute("Enable output of image fonts via Spout2. Requires obs-spout to capture this")]
        [DisplayName("Enable Spout2 output")]
        public virtual bool SpoutEnabled { get; set; }

        [CategoryAttribute("Spout Image Font"), DescriptionAttribute("Will be show in in OBS Spout2 source selector")]
        [DisplayName("Spout2 sender name")]
        public virtual string SpoutName { get; set; }

        [CategoryAttribute("Spout Image Font"), DescriptionAttribute("Folder with a series of images named 0.png - 9.png, colon.png and space.png. All must be the same height and width (colon.png may have a different width). Once the timer has been started the application must be restarted to change this")]
        [DisplayName("Font directory")]
        public virtual string FontDir { get; set; }

        [CategoryAttribute("Spout Image Font"), DescriptionAttribute("Show memory writes in real-time (this setting is not saved)")]
        public virtual bool Debug { get; set; }

        private void Init() {
            Font = Defaults.Font;
            BGCol = Defaults.BGCol;
            FGCol = Defaults.FGCol;
            Alignment = Defaults.Alignment;
            ShowLeadingZero = Defaults.ShowLeadingZero;
            VNyanURL = Defaults.VNyanURL;
            MixItUpURL = Defaults.MixItUpURL;
            MixItUpPlatform = Defaults.MixItUpPlatform;
            TestTime = Defaults.TestTime;
            Location = Defaults.Location;
            Dimensions = Defaults.Dimensions;
            SpoutEnabled = Defaults.SpoutEnabled;
            SpoutName = Defaults.SpoutName;
            FontDir = Defaults.FontDir;
            ProgressYellow = Defaults.ProgressYellow;
            ProgressRed = Defaults.ProgressRed;
            Debug = false;
        }
        
        public Settings() {
            Init();
        }

        public void LoadConfig(string ConfigFile) {
            if (File.Exists(ConfigFile)) {
                dynamic Config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(ConfigFile));
                try {
                    if ((int)Config.X < SystemInformation.VirtualScreen.Width - 5 &&  // Safety in case monitor
                    (int)Config.Y < SystemInformation.VirtualScreen.Height - 5) { // size has changed
                        Location = new Point((int)Config.X, (int)Config.Y);
                    }
                } catch (RuntimeBinderException) { }
                try { Dimensions = new Size((int)Config.Width, (int)Config.Height);                                    } catch { }
                try { BGCol = ColorTranslator.FromHtml((string)Config.BackgroundColor);                                } catch { }
                try { FGCol = ColorTranslator.FromHtml((string)Config.ForegroundColor);                                } catch { }
                try { Alignment = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), (string)Config.Alignment);    } catch { }
                try { Font = new Font((string)Config.FontName, (int)Config.FontSize, (FontStyle)Config.FontStyle);     } catch { }
                try { ShowLeadingZero = Config.ShowLeadingZero; } catch { }
                try { VNyanURL = Config.VNyanURL;                                                                      } catch { }
                try { MixItUpURL = Config.MixItUpURL;                                                                  } catch { }
                try { MixItUpPlatform = (MIUPlatforms)Enum.Parse(typeof(MIUPlatforms), (string)Config.MixItUpPlatform);} catch { }
                try { TestTime = TimeSpan.FromSeconds((int)Config.TestTime);                                           } catch { }
                try { SpoutEnabled = Config.SpoutEnabled; } catch { }
                try { SpoutName = Config.SpoutName; } catch { }
                try { FontDir = Config.FontDir; } catch { }
                try { ProgressYellow = Config.ProgressYellow; } catch { }
                try { ProgressRed = Config.ProgressRed; } catch { }
            } 
        }

        public void SaveConfig(string ConfigFile, Point Location, Size Dimensions) {
            JObject Config = new JObject(
                new JProperty("FontName", Font.Name),
                new JProperty("FontSize", Font.Size),
                new JProperty("FontStyle", Font.Style.ToString()),
                new JProperty("BackgroundColor", ColorTranslator.ToHtml(BGCol)),
                new JProperty("ForegroundColor", ColorTranslator.ToHtml(FGCol)),
                new JProperty("Alignment", Alignment.ToString()),
                new JProperty("X", Location.X),
                new JProperty("Y", Location.Y),
                new JProperty("Width", Dimensions.Width),
                new JProperty("Height", Dimensions.Height),
                new JProperty("ShowLeadingZero", ShowLeadingZero),
                new JProperty("VNyanURL", VNyanURL),
                new JProperty("MixItUpURL", MixItUpURL),
                new JProperty("MixItUpPlatform", MixItUpPlatform.ToString()),
                new JProperty("TestTime", (int)TestTime.TotalSeconds),
                new JProperty("SpoutEnabled", SpoutEnabled),
                new JProperty("SpoutName", SpoutName),
                new JProperty("FontDir", FontDir),
                new JProperty("ProgressYellow", ProgressYellow),
                new JProperty("ProgressRed", ProgressRed)
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
            TempSettings.SpoutEnabled = this.SpoutEnabled;
            TempSettings.FontDir = this.FontDir;
            return TempSettings;
        }
    }

    public class CSettings : Settings, INotifyPropertyChanged {
        private static Settings _Settings = new Settings();

        public CSettings() {
            //_Settings = new Settings();
        }
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
        public override bool ShowLeadingZero {
            get { return _Settings.ShowLeadingZero; }
            set {
                if (_Settings.ShowLeadingZero == value) return;
                _Settings.ShowLeadingZero = value;
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
