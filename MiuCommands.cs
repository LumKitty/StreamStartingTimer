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
    public partial class frmMiuCommands : Form {
        public frmMiuCommands() {
            InitializeComponent();
        }

        private void UpdateListView() {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var MiuCmd in Shared.MIUConnector.GetMiuCommands()) {

                ListViewItem item = new ListViewItem(MiuCmd.Key);
                //item.SubItems.Add(timerEvent.Columns[0]);
                item.SubItems.Add(MiuCmd.Value);
                listView1.Items.Add(item);
                //MessageBox.Show(MiuCmd.Key);
            }
            listView1.EndUpdate();
        }

        private void frmMiuCommands_Shown(object sender, EventArgs e) {
            UpdateListView();
        }

        private void frmMiuCommands_Load(object sender, EventArgs e) {
            UpdateListView();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e) {
            System.Windows.Forms.Clipboard.SetText(listView1.Items[listView1.SelectedIndices[0]].Text);
        }
    }
}
