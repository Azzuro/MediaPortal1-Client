namespace MediaPortal.MPInstaller
{
    partial class controlp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
          this.components = new System.ComponentModel.Container();
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(controlp));
          this.listView1 = new System.Windows.Forms.ListView();
          this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
          this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
          this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
          this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
          this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
          this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
          this.informationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.imageList1 = new System.Windows.Forms.ImageList(this.components);
          this.button1 = new System.Windows.Forms.Button();
          this.button2 = new System.Windows.Forms.Button();
          this.button3 = new System.Windows.Forms.Button();
          this.button4 = new System.Windows.Forms.Button();
          this.tabControl1 = new System.Windows.Forms.TabControl();
          this.tabPage1 = new System.Windows.Forms.TabPage();
          this.label3 = new System.Windows.Forms.Label();
          this.comboBox3 = new System.Windows.Forms.ComboBox();
          this.button6 = new System.Windows.Forms.Button();
          this.mozPane1 = new Pabo.MozBar.MozPane();
          this.mozItem1 = new Pabo.MozBar.MozItem();
          this.label2 = new System.Windows.Forms.Label();
          this.button5 = new System.Windows.Forms.Button();
          this.comboBox2 = new System.Windows.Forms.ComboBox();
          this.linkLabel1 = new System.Windows.Forms.LinkLabel();
          this.contextMenuStrip1.SuspendLayout();
          this.tabControl1.SuspendLayout();
          this.tabPage1.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.mozPane1)).BeginInit();
          this.mozPane1.SuspendLayout();
          this.SuspendLayout();
          // 
          // listView1
          // 
          this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader9});
          this.listView1.ContextMenuStrip = this.contextMenuStrip1;
          this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.listView1.FullRowSelect = true;
          this.listView1.GridLines = true;
          this.listView1.HideSelection = false;
          this.listView1.LargeImageList = this.imageList1;
          this.listView1.Location = new System.Drawing.Point(164, 0);
          this.listView1.MultiSelect = false;
          this.listView1.Name = "listView1";
          this.listView1.ShowItemToolTips = true;
          this.listView1.Size = new System.Drawing.Size(619, 343);
          this.listView1.SmallImageList = this.imageList1;
          this.listView1.TabIndex = 0;
          this.listView1.UseCompatibleStateImageBehavior = false;
          this.listView1.View = System.Windows.Forms.View.Details;
          this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
          this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
          // 
          // columnHeader1
          // 
          this.columnHeader1.Text = "Name";
          this.columnHeader1.Width = 179;
          // 
          // columnHeader2
          // 
          this.columnHeader2.Text = "Author";
          this.columnHeader2.Width = 194;
          // 
          // columnHeader3
          // 
          this.columnHeader3.Text = "Version";
          this.columnHeader3.Width = 83;
          // 
          // columnHeader4
          // 
          this.columnHeader4.Text = "File";
          // 
          // columnHeader9
          // 
          this.columnHeader9.Text = "Group";
          this.columnHeader9.Width = 124;
          // 
          // contextMenuStrip1
          // 
          this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.informationToolStripMenuItem});
          this.contextMenuStrip1.Name = "contextMenuStrip1";
          this.contextMenuStrip1.Size = new System.Drawing.Size(142, 26);
          this.contextMenuStrip1.Text = "Is plugin";
          // 
          // informationToolStripMenuItem
          // 
          this.informationToolStripMenuItem.Name = "informationToolStripMenuItem";
          this.informationToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
          this.informationToolStripMenuItem.Text = "Information";
          this.informationToolStripMenuItem.Click += new System.EventHandler(this.informationToolStripMenuItem_Click);
          // 
          // imageList1
          // 
          this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
          this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
          this.imageList1.Images.SetKeyName(0, "application.ico");
          // 
          // button1
          // 
          this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.button1.Enabled = false;
          this.button1.Location = new System.Drawing.Point(8, 358);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(75, 23);
          this.button1.TabIndex = 1;
          this.button1.Text = "Uninstall";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.button2.Location = new System.Drawing.Point(700, 358);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(75, 23);
          this.button2.TabIndex = 2;
          this.button2.Text = "Close";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // button3
          // 
          this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.button3.Enabled = false;
          this.button3.Location = new System.Drawing.Point(86, 358);
          this.button3.Name = "button3";
          this.button3.Size = new System.Drawing.Size(75, 23);
          this.button3.TabIndex = 3;
          this.button3.Text = "Update";
          this.button3.UseVisualStyleBackColor = true;
          this.button3.Click += new System.EventHandler(this.button3_Click);
          // 
          // button4
          // 
          this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.button4.Enabled = false;
          this.button4.Location = new System.Drawing.Point(167, 358);
          this.button4.Name = "button4";
          this.button4.Size = new System.Drawing.Size(75, 23);
          this.button4.TabIndex = 4;
          this.button4.Text = "Reinstall";
          this.button4.UseVisualStyleBackColor = true;
          this.button4.Click += new System.EventHandler(this.button4_Click);
          // 
          // tabControl1
          // 
          this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.tabControl1.Controls.Add(this.tabPage1);
          this.tabControl1.Location = new System.Drawing.Point(1, 1);
          this.tabControl1.Name = "tabControl1";
          this.tabControl1.SelectedIndex = 0;
          this.tabControl1.Size = new System.Drawing.Size(791, 415);
          this.tabControl1.TabIndex = 5;
          // 
          // tabPage1
          // 
          this.tabPage1.Controls.Add(this.label3);
          this.tabPage1.Controls.Add(this.comboBox3);
          this.tabPage1.Controls.Add(this.button6);
          this.tabPage1.Controls.Add(this.mozPane1);
          this.tabPage1.Controls.Add(this.label2);
          this.tabPage1.Controls.Add(this.button5);
          this.tabPage1.Controls.Add(this.comboBox2);
          this.tabPage1.Controls.Add(this.linkLabel1);
          this.tabPage1.Controls.Add(this.listView1);
          this.tabPage1.Controls.Add(this.button2);
          this.tabPage1.Controls.Add(this.button4);
          this.tabPage1.Controls.Add(this.button1);
          this.tabPage1.Controls.Add(this.button3);
          this.tabPage1.Location = new System.Drawing.Point(4, 22);
          this.tabPage1.Name = "tabPage1";
          this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
          this.tabPage1.Size = new System.Drawing.Size(783, 389);
          this.tabPage1.TabIndex = 0;
          this.tabPage1.Text = "Extensions";
          this.tabPage1.UseVisualStyleBackColor = true;
          this.tabPage1.Enter += new System.EventHandler(this.tabPage1_Enter);
          // 
          // label3
          // 
          this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.label3.AutoSize = true;
          this.label3.Location = new System.Drawing.Point(483, 347);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(29, 13);
          this.label3.TabIndex = 11;
          this.label3.Text = "Filter";
          // 
          // comboBox3
          // 
          this.comboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.comboBox3.FormattingEnabled = true;
          this.comboBox3.Items.AddRange(new object[] {
            "None",
            "Local",
            "Updates",
            "Online"});
          this.comboBox3.Location = new System.Drawing.Point(482, 358);
          this.comboBox3.Name = "comboBox3";
          this.comboBox3.Size = new System.Drawing.Size(121, 21);
          this.comboBox3.TabIndex = 10;
          this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
          // 
          // button6
          // 
          this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.button6.Location = new System.Drawing.Point(362, 358);
          this.button6.Name = "button6";
          this.button6.Size = new System.Drawing.Size(108, 23);
          this.button6.TabIndex = 9;
          this.button6.Text = "Load online list";
          this.button6.UseVisualStyleBackColor = true;
          this.button6.Click += new System.EventHandler(this.button6_Click);
          // 
          // mozPane1
          // 
          this.mozPane1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)));
          this.mozPane1.ImageList = this.imageList1;
          this.mozPane1.Items.AddRange(new Pabo.MozBar.MozItem[] {
            this.mozItem1});
          this.mozPane1.Location = new System.Drawing.Point(3, 6);
          this.mozPane1.Name = "mozPane1";
          this.mozPane1.Size = new System.Drawing.Size(155, 337);
          this.mozPane1.TabIndex = 8;
          this.mozPane1.ItemSelected += new Pabo.MozBar.MozItemEventHandler(this.mozPane1_ItemSelected);
          // 
          // mozItem1
          // 
          this.mozItem1.Images.Focus = 0;
          this.mozItem1.Images.FocusImage = ((System.Drawing.Image)(resources.GetObject("resource.FocusImage")));
          this.mozItem1.Images.Normal = 0;
          this.mozItem1.Images.NormalImage = ((System.Drawing.Image)(resources.GetObject("resource.NormalImage")));
          this.mozItem1.Images.Selected = 0;
          this.mozItem1.Images.SelectedImage = ((System.Drawing.Image)(resources.GetObject("resource.SelectedImage")));
          this.mozItem1.Location = new System.Drawing.Point(2, 2);
          this.mozItem1.Name = "mozItem1";
          this.mozItem1.Size = new System.Drawing.Size(151, 40);
          this.mozItem1.TabIndex = 0;
          this.mozItem1.Text = "mozItem1";
          this.mozItem1.TextAlign = Pabo.MozBar.MozTextAlign.Right;
          // 
          // label2
          // 
          this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(609, 346);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(36, 13);
          this.label2.TabIndex = 7;
          this.label2.Text = "View :";
          // 
          // button5
          // 
          this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.button5.Location = new System.Drawing.Point(248, 358);
          this.button5.Name = "button5";
          this.button5.Size = new System.Drawing.Size(108, 23);
          this.button5.TabIndex = 3;
          this.button5.Text = "Download && Install";
          this.button5.UseVisualStyleBackColor = true;
          this.button5.Click += new System.EventHandler(this.button5_Click);
          // 
          // comboBox2
          // 
          this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.comboBox2.FormattingEnabled = true;
          this.comboBox2.Items.AddRange(new object[] {
            "List",
            "Icons"});
          this.comboBox2.Location = new System.Drawing.Point(612, 358);
          this.comboBox2.Name = "comboBox2";
          this.comboBox2.Size = new System.Drawing.Size(82, 21);
          this.comboBox2.TabIndex = 6;
          this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
          // 
          // linkLabel1
          // 
          this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.linkLabel1.AutoSize = true;
          this.linkLabel1.Location = new System.Drawing.Point(7, 410);
          this.linkLabel1.Name = "linkLabel1";
          this.linkLabel1.Size = new System.Drawing.Size(113, 13);
          this.linkLabel1.TabIndex = 5;
          this.linkLabel1.TabStop = true;
          this.linkLabel1.Text = "http://dukus.extra.hu/";
          // 
          // controlp
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(792, 416);
          this.Controls.Add(this.tabControl1);
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.Name = "controlp";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "Control panel";
          this.Load += new System.EventHandler(this.controlp_Load);
          this.contextMenuStrip1.ResumeLayout(false);
          this.tabControl1.ResumeLayout(false);
          this.tabPage1.ResumeLayout(false);
          this.tabPage1.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.mozPane1)).EndInit();
          this.mozPane1.ResumeLayout(false);
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TabControl tabControl1;
      private System.Windows.Forms.TabPage tabPage1;
      private System.Windows.Forms.Button button5;
      private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Label label2;
      private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
      private Pabo.MozBar.MozPane mozPane1;
      private Pabo.MozBar.MozItem mozItem1;
      private System.Windows.Forms.Button button6;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ComboBox comboBox3;
      private System.Windows.Forms.ToolStripMenuItem informationToolStripMenuItem;
    }
}