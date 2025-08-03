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
            btnAdd = new Button();
            SuspendLayout();
            // 
            // propertyGrid1
            // 
            propertyGrid1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Location = new Point(12, 334);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(776, 104);
            propertyGrid1.TabIndex = 0;
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
            listView1.Location = new Point(12, 12);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(776, 286);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
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
            // btnAdd
            // 
            btnAdd.Location = new Point(12, 304);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // EventEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnAdd);
            Controls.Add(listView1);
            Controls.Add(propertyGrid1);
            Name = "EventEditor";
            Text = "EventEditor";
            Load += EventEditor_Load;
            ResumeLayout(false);
        }

        #endregion

        private PropertyGrid propertyGrid1;
        private ListView listView1;
        private ColumnHeader App;
        private ColumnHeader Payload;
        private ColumnHeader Enabled;
        private Button btnAdd;
    }
}