namespace StreamStartingTimer {
    partial class frmMiuCommands {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            ListViewItem listViewItem1 = new ListViewItem("fdfd");
            ListViewItem listViewItem2 = new ListViewItem("fdsf");
            listView1 = new ListView();
            MiuCmdName = new ColumnHeader();
            MiuCmdId = new ColumnHeader();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.AllowDrop = true;
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.Columns.AddRange(new ColumnHeader[] { MiuCmdName, MiuCmdId });
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2 });
            listView1.Location = new Point(-1, -1);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(414, 453);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            // 
            // MiuCmdName
            // 
            MiuCmdName.Text = "Command Name";
            MiuCmdName.Width = 160;
            // 
            // MiuCmdId
            // 
            MiuCmdId.Text = "ID";
            MiuCmdId.Width = 250;
            // 
            // frmMiuCommands
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(412, 450);
            Controls.Add(listView1);
            Name = "frmMiuCommands";
            Text = "Mix It Up Commands (double click to copy command name)";
            Load += frmMiuCommands_Load;
            Shown += frmMiuCommands_Shown;
            ResumeLayout(false);
        }

        #endregion

        private ListView listView1;
        private ColumnHeader MiuCmdName;
        private ColumnHeader MiuCmdId;
    }
}