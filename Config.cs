using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreamStartingTimer {
    public partial class Config : Form {
        bool SettingsChanged = false;
        public Config() {
            InitializeComponent();
        }

        private void CloseForm(DialogResult result) {
            if (SettingsChanged) {
                if (MessageBox.Show( "You have not saved your settings.\r\n" +
                                     "They will work for this session only\r\n" +
                                     "\r\n" +
                                     "Close config without saving?",
                                     "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                                     == DialogResult.Yes) {
                    this.DialogResult = result;
                    this.Close();
                }
            } else {
                this.DialogResult = result;
                this.Close();
            }
        }

        private void Config_Load(object sender, EventArgs e) {
            propertyGrid1.SelectedObject = Shared.CurSettings;
        }

        private void btnOk_Click(object sender, EventArgs e) {
            CloseForm(DialogResult.OK);
        }
        
        private void btnCancel_Click(object sender, EventArgs e) {
            CloseForm(DialogResult.Cancel);
        }

        private void btnLoad_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                Shared.CurSettings.LoadConfig(openFileDialog1.FileName);
                Shared.frmClock.Location = Shared.CurSettings.Location;
                Shared.frmClock.Size = Shared.CurSettings.Dimensions;
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                Shared.CurSettings.SaveConfig(saveFileDialog1.FileName, Shared.frmClock.Location, Shared.frmClock.Size);
                SettingsChanged = false;
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            SettingsChanged = true;
        }
    }
}
