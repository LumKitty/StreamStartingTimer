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
        public List<TimerEvent> TimerEvents;

        private int SelectedTimerEvent;

        private void EventEditor_Load(object sender, EventArgs e) {
            foreach (TimerEvent timerEvent in TimerEvents) {
                ListViewItem item = new ListViewItem();
                //item.SubItems.Add(timerEvent.Columns[0]);
                item.SubItems.Add(timerEvent.Columns[1]);
                item.SubItems.Add(timerEvent.Columns[2]);
                listView1.Items.Add(item);
                listView1.Items[listView1.Items.Count - 1].Checked = timerEvent.Enabled;
                listView1.Items[listView1.Items.Count - 1].SubItems[0].Text = timerEvent.Columns[0];
            }

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listView1.SelectedIndices.Count > 0) {
                SelectedTimerEvent = listView1.SelectedIndices[0];
                propertyGrid1.SelectedObject = TimerEvents[SelectedTimerEvent];
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            switch (e.ChangedItem.Label) {
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
            }
            MessageBox.Show(e.ChangedItem.Label);

            //listView1.Items[SelectedTimerEvent].SubItems[0].Text = propertyGrid1.Get
            //listView1.Items[SelectedTimerEvent].SubItems[1].Text = TimerEvents[SelectedTimerEvent].Columns[1];
            //listView1.Items[SelectedTimerEvent].SubItems[2].Text = TimerEvents[SelectedTimerEvent].Columns[2];
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            TimerEvents.Add(new TimerEvent(false, 0, EventType.VNyan, ""));
            ListViewItem item = new ListViewItem();
            //item.SubItems.Add(timerEvent.Columns[0]);
            item.SubItems.Add("VNyan");
            item.SubItems.Add("");
            listView1.Items.Add(item);
            listView1.Items[listView1.Items.Count - 1].Checked = false;
            listView1.Items[listView1.Items.Count - 1].SubItems[0].Text = "00:00";
            listView1.Items[listView1.Items.Count - 1].SubItems[1].Text = "VNyan";
            listView1.Items[listView1.Items.Count - 1].SubItems[2].Text = "";
            //for (int n = 0; n < listView1.Items.Count - 1; n++) {
            //    listView1.Items[n].Selected = false;
            //}
            listView1.Items[listView1.Items.Count - 1].Selected = true;

        }
    }
}
