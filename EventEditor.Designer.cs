namespace StreamStartingTimer {
    partial class EventEditor {
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
            propertyGrid1 = new PropertyGrid();
            listView1 = new ListView();
            Enabled = new ColumnHeader();
            App = new ColumnHeader();
            Payload = new ColumnHeader();
            btnLoad = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            btnOK = new Button();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            btnTest = new Button();
            btnShowMIU = new Button();
            btnDelete = new Button();
            AddVNyan = new Button();
            AddMIU = new Button();
            AddExe = new Button();
            splitContainer1 = new SplitContainer();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // propertyGrid1
            // 
            propertyGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Location = new Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.PropertySort = PropertySort.Categorized;
            propertyGrid1.Size = new Size(193, 403);
            propertyGrid1.TabIndex = 1;
            propertyGrid1.ToolbarVisible = false;
            propertyGrid1.PropertyValueChanged += propertyGrid1_PropertyValueChanged;
            // 
            // listView1
            // 
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.CheckBoxes = true;
            listView1.Columns.AddRange(new ColumnHeader[] { Enabled, App, Payload });
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(0, 0);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(624, 403);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.ItemChecked += listView1_ItemChecked;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // Enabled
            // 
            Enabled.Text = "Enabled";
            // 
            // App
            // 
            App.Text = "App";
            // 
            // Payload
            // 
            Payload.Text = "Payload";
            Payload.Width = 400;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLoad.Location = new Point(515, 421);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(75, 23);
            btnLoad.TabIndex = 6;
            btnLoad.Text = "&Load";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.MouseClick += btnLoad_MouseClick;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(596, 421);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 7;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.MouseClick += btnSave_MouseClick;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(758, 421);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(677, 421);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 8;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.MouseClick += btnOK_MouseClick;
            // 
            // openFileDialog1
            // 
            openFileDialog1.DefaultExt = "json";
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "JSON files|*.json";
            openFileDialog1.Title = "Load Event List";
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "json";
            saveFileDialog1.Filter = "JSON files|*.json";
            saveFileDialog1.Title = "Save Event List";
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTest.Location = new Point(291, 421);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(75, 23);
            btnTest.TabIndex = 4;
            btnTest.Text = "&Test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // btnShowMIU
            // 
            btnShowMIU.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnShowMIU.Location = new Point(372, 421);
            btnShowMIU.Name = "btnShowMIU";
            btnShowMIU.Size = new Size(75, 23);
            btnShowMIU.TabIndex = 5;
            btnShowMIU.Text = "Sho&w MIU";
            btnShowMIU.UseVisualStyleBackColor = true;
            btnShowMIU.Click += btnShowMIU_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(210, 421);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "&Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // AddVNyan
            // 
            AddVNyan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AddVNyan.Location = new Point(12, 421);
            AddVNyan.Name = "AddVNyan";
            AddVNyan.Size = new Size(60, 23);
            AddVNyan.TabIndex = 10;
            AddVNyan.Text = "+&VNyan";
            AddVNyan.UseVisualStyleBackColor = true;
            AddVNyan.Click += AddVNyan_Click;
            // 
            // AddMIU
            // 
            AddMIU.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AddMIU.Location = new Point(78, 421);
            AddMIU.Name = "AddMIU";
            AddMIU.Size = new Size(60, 23);
            AddMIU.TabIndex = 11;
            AddMIU.Text = "+&MIU";
            AddMIU.UseVisualStyleBackColor = true;
            AddMIU.Click += AddMIU_Click;
            // 
            // AddExe
            // 
            AddExe.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AddExe.Location = new Point(144, 421);
            AddExe.Name = "AddExe";
            AddExe.Size = new Size(60, 23);
            AddExe.TabIndex = 12;
            AddExe.Text = "+&EXE";
            AddExe.UseVisualStyleBackColor = true;
            AddExe.Click += AddExe_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(12, 12);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(propertyGrid1);
            splitContainer1.Size = new Size(821, 403);
            splitContainer1.SplitterDistance = 624;
            splitContainer1.TabIndex = 13;
            // 
            // EventEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(845, 456);
            ControlBox = false;
            Controls.Add(splitContainer1);
            Controls.Add(AddExe);
            Controls.Add(AddMIU);
            Controls.Add(AddVNyan);
            Controls.Add(btnDelete);
            Controls.Add(btnShowMIU);
            Controls.Add(btnTest);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnLoad);
            MinimizeBox = false;
            MinimumSize = new Size(799, 200);
            Name = "EventEditor";
            Text = "EventEditor";
            Shown += EventEditor_Shown;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private ListView listView1;
        private ColumnHeader App;
        private ColumnHeader Payload;
        private ColumnHeader Enabled;
        private Button btnLoad;
        private Button btnSave;
        private Button btnCancel;
        private Button btnOK;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private Button btnTest;
        private Button btnShowMIU;
        internal PropertyGrid propertyGrid1;
        private Button btnDelete;
        private Button AddVNyan;
        private Button AddMIU;
        private Button AddExe;
        private SplitContainer splitContainer1;
    }
}