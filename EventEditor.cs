using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace StreamStartingTimer {

    public partial class EventEditor : Form {
        public EventEditor() {
            InitializeComponent();
        }
        const string TimeFormat = @"mm\:ss";
        public List<TimerEvent> FormTimerEvents = new List<TimerEvent>();

        private int SelectedTimerEvent;
        private bool ChangesMade = false;

        private static readonly frmMiuCommands FrmMiuCommands = new frmMiuCommands();

        private void UpdateListView() {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (TimerEvent timerEvent in FormTimerEvents) {
                ListViewItem item = new ListViewItem();
                //item.SubItems.Add(timerEvent.Columns[0]);
                item.SubItems.Add(timerEvent.Columns[1]);
                item.SubItems.Add(timerEvent.Columns[2]);
                item.Checked = timerEvent.Enabled;
                item.SubItems[0].Text = timerEvent.Columns[0];
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        private void UpdatePropertyGrid() {
            if (listView1.SelectedIndices.Count > 0) {
                SelectedTimerEvent = listView1.SelectedIndices[0];
                propertyGrid1.SelectedObject = FormTimerEvents[SelectedTimerEvent];
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            UpdatePropertyGrid();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            /*switch (e.ChangedItem.Label) {
                case "Enabled":
                    listView1.Items[SelectedTimerEvent].Checked = Convert.ToBoolean(e.ChangedItem.Value);
                    break;
                case "Time":
                    TimeSpan temp = (TimeSpan)e.ChangedItem.Value;
                    listView1.Items[SelectedTimerEvent].SubItems[0].Text = temp.ToString(TimeFormat);
                    break;
                case "EventType":
                    listView1.Items[SelectedTimerEvent].SubItems[1].Text = Convert.ToString(e.ChangedItem.Value);
                    break;
                case "Payload":
                    listView1.Items[SelectedTimerEvent].SubItems[2].Text = Convert.ToString(e.ChangedItem.Value);
                    break;
            }*/
            FormTimerEvents = FormTimerEvents.OrderByDescending(o => o.Time.TotalSeconds).ToList();
            UpdateListView();
            // FormTimerEvents[SelectedTimerEvent].UpdateMiuCmdId();
            ChangesMade = true;
            //MessageBox.Show(e.ChangedItem.Label);

            //listView1.Items[SelectedTimerEvent].SubItems[0].Text = propertyGrid1.Get
            //listView1.Items[SelectedTimerEvent].SubItems[1].Text = TimerEvents[SelectedTimerEvent].Columns[1];
            //listView1.Items[SelectedTimerEvent].SubItems[2].Text = TimerEvents[SelectedTimerEvent].Columns[2];
        }

        private void btnLoad_MouseClick(object sender, MouseEventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                FormTimerEvents = Shared.LoadEvents(openFileDialog1.FileName);
                UpdateListView();
                ChangesMade = false;
            }
            listView1.Focus();
        }

        private void btnSave_MouseClick(object sender, MouseEventArgs e) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                Shared.SaveEvents(saveFileDialog1.FileName, FormTimerEvents);
                ChangesMade = false;
            }
            listView1.Focus();
        }

        private void CloseForm(DialogResult result) {
            if (ChangesMade) {
                if (MessageBox.Show("You have not saved your events.\r\n" +
                                     "They will work for this session only\r\n" +
                                     "\r\n" +
                                     "Close editor without saving?",
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

        private void btnOK_MouseClick(object sender, MouseEventArgs e) {
            CloseForm(DialogResult.OK);
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            CloseForm(DialogResult.Cancel);
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e) {
            int n = 0;
            foreach (ListViewItem item in listView1.Items) {
                FormTimerEvents[n].Enabled = item.Checked;
                n++;
            }
            UpdatePropertyGrid();
        }

        private void btnTest_Click(object sender, EventArgs e) {
            if (listView1.SelectedIndices.Count > 0) {
                FormTimerEvents[listView1.SelectedIndices[0]].TestFire();
            }
            propertyGrid1.Focus();
        }

        private void btnShowMIU_Click(object sender, EventArgs e) {
            FrmMiuCommands.Show();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            if (listView1.SelectedIndices.Count > 0) {
                SelectedTimerEvent = listView1.SelectedIndices[0];
                FormTimerEvents.Remove(FormTimerEvents[SelectedTimerEvent]);
                UpdateListView();
                listView1.Focus();
            }
        }

        private void AddEvent(TimerEvent timerEvent) {
            FormTimerEvents.Add(timerEvent);
            ListViewItem item = new ListViewItem();
            //item.SubItems.Add(timerEvent.Columns[0]);
            item.SubItems.Add(timerEvent.EventType.ToString());
            item.SubItems.Add(timerEvent.Payload);
            listView1.Items.Add(item);
            listView1.Items[listView1.Items.Count - 1].Checked = false;
            listView1.Items[listView1.Items.Count - 1].SubItems[0].Text = "00:00";
            listView1.Items[listView1.Items.Count - 1].SubItems[1].Text = timerEvent.EventType.ToString();
            listView1.Items[listView1.Items.Count - 1].SubItems[2].Text = "";
            //for (int n = 0; n < listView1.Items.Count - 1; n++) {
            //    listView1.Items[n].Selected = false;
            //}
            listView1.Items[listView1.Items.Count - 1].Selected = true;
            ChangesMade = true;
            propertyGrid1.Focus();
        }

        private void AddVNyan_Click(object sender, EventArgs e) {
            AddEvent(new VNyanEvent());
        }

        private void AddMIU_Click(object sender, EventArgs e) {
            AddEvent(new MIUEvent());
        }

        private void AddExe_Click(object sender, EventArgs e) {
            AddEvent(new ExeEvent());
        }

        private void EventEditor_Shown(object sender, EventArgs e) {
            UpdateListView();
        }
    }
}
