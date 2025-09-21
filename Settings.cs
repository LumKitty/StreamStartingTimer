using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }

    public class CSettings : Settings, INotifyPropertyChanged {
        private static Settings Settings = new Settings();

        public override string VNyanURL {
            get { return Settings.VNyanURL; }
            set {
                if (Settings.VNyanURL == value) return;
                Settings.VNyanURL = value;
                RaisePropertyChanged("VNyanURL");
            }
        }
        public override string MixItUpURL {
            get { return Settings.MixItUpURL; }
            set {
                if (Settings.MixItUpURL == value) return;
                Settings.MixItUpURL = value;
                RaisePropertyChanged("MixItUpURL");
            }
        }
        public override Font Font {
            get { return Settings.Font; }
            set {
                if (Settings.Font == value) return;
                Settings.Font = value;
                RaisePropertyChanged("Font");
            }
        }
        public override Color FGCol {
            get { return Settings.FGCol; }
            set {
                if (Settings.FGCol == value) return;
                Settings.FGCol = value;
                RaisePropertyChanged("FGCol");
            }
        }
        public override Color BGCol {
            get { return Settings.BGCol; }
            set {
                if (Settings.BGCol == value) return;
                Settings.BGCol = value;
                RaisePropertyChanged("BGCol");
            }
        }
        public override ContentAlignment Alignment {
            get { return Settings.Alignment; }
            set {
                if (Settings.Alignment == value) return;
                Settings.Alignment = value;
                RaisePropertyChanged("Alignment");
            }
        }
        public override Point Location {
            get { return Settings.Location; }
            set {
                if (Settings.Location == value) return;
                Settings.Location = value;
                RaisePropertyChanged("Location");
            }
        }
        public override Size Dimensions {
            get { return Settings.Dimensions; }
            set {
                if (Settings.Dimensions == value) return;
                Settings.Dimensions = value;
                RaisePropertyChanged("Dimensions");
            }
        }
        public override MIUPlatforms MixItUpPlatform {
            get { return Settings.MixItUpPlatform; }
            set {
                if (Settings.MixItUpPlatform == value) return;
                Settings.MixItUpPlatform = value;
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
