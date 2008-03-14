namespace SetupTv.Sections
{
  partial class TvCards
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvCards));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.colEnabled = new System.Windows.Forms.ColumnHeader();
      this.colPriority = new System.Windows.Forms.ColumnHeader();
      this.colType = new System.Windows.Forms.ColumnHeader();
      this.colCAMLimit = new System.Windows.Forms.ColumnHeader();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colInit = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.placeInHybridCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUp = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.deleteCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteEntireHybridCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip2.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colEnabled,
            this.colPriority,
            this.colType,
            this.colCAMLimit,
            this.colName,
            this.colInit});
      this.mpListView1.ContextMenuStrip = this.contextMenuStrip2;
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(9, 11);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(437, 328);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 0;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView1_ItemChecked);
      this.mpListView1.SelectedIndexChanged += new System.EventHandler(this.mpListView1_SelectedIndexChanged);
      // 
      // colEnabled
      // 
      this.colEnabled.Text = "Enabled";
      this.colEnabled.Width = 85;
      // 
      // colPriority
      // 
      this.colPriority.Text = "Priority";
      this.colPriority.Width = 50;
      // 
      // colType
      // 
      this.colType.Text = "Type";
      this.colType.Width = 50;
      // 
      // colCAMLimit
      // 
      this.colCAMLimit.Text = "Cam Limit";
      this.colCAMLimit.Width = 65;
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 130;
      // 
      // colInit
      // 
      this.colInit.Text = "Init";
      this.colInit.Width = 50;
      // 
      // contextMenuStrip2
      // 
      this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.placeInHybridCardToolStripMenuItem});
      this.contextMenuStrip2.Name = "contextMenuStrip2";
      this.contextMenuStrip2.Size = new System.Drawing.Size(153, 26);
      // 
      // placeInHybridCardToolStripMenuItem
      // 
      this.placeInHybridCardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem});
      this.placeInHybridCardToolStripMenuItem.Name = "placeInHybridCardToolStripMenuItem";
      this.placeInHybridCardToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.placeInHybridCardToolStripMenuItem.Text = "Place in group";
      // 
      // newToolStripMenuItem
      // 
      this.newToolStripMenuItem.Name = "newToolStripMenuItem";
      this.newToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
      this.newToolStripMenuItem.Text = "new";
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "card.gif");
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(90, 345);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(75, 23);
      this.buttonDown.TabIndex = 1;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Location = new System.Drawing.Point(9, 345);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(75, 23);
      this.buttonUp.TabIndex = 2;
      this.buttonUp.Text = "Up";
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(465, 400);
      this.tabControl1.TabIndex = 3;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.buttonRemove);
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.buttonEdit);
      this.tabPage1.Controls.Add(this.buttonUp);
      this.tabPage1.Controls.Add(this.buttonDown);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(457, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Cards";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonRemove.Enabled = false;
      this.buttonRemove.Location = new System.Drawing.Point(371, 345);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(75, 23);
      this.buttonRemove.TabIndex = 3;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonEdit.Location = new System.Drawing.Point(290, 345);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(75, 23);
      this.buttonEdit.TabIndex = 2;
      this.buttonEdit.Text = "Edit";
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.treeView1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(457, 374);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Hybrid cards";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // treeView1
      // 
      this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
      this.treeView1.ImageIndex = 0;
      this.treeView1.ImageList = this.imageList1;
      this.treeView1.Location = new System.Drawing.Point(9, 11);
      this.treeView1.Name = "treeView1";
      this.treeView1.SelectedImageIndex = 0;
      this.treeView1.Size = new System.Drawing.Size(437, 328);
      this.treeView1.TabIndex = 0;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteCardToolStripMenuItem,
            this.deleteEntireHybridCardToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(205, 48);
      // 
      // deleteCardToolStripMenuItem
      // 
      this.deleteCardToolStripMenuItem.Name = "deleteCardToolStripMenuItem";
      this.deleteCardToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.deleteCardToolStripMenuItem.Text = "Delete card";
      this.deleteCardToolStripMenuItem.Click += new System.EventHandler(this.deleteCardToolStripMenuItem_Click);
      // 
      // deleteEntireHybridCardToolStripMenuItem
      // 
      this.deleteEntireHybridCardToolStripMenuItem.Name = "deleteEntireHybridCardToolStripMenuItem";
      this.deleteEntireHybridCardToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.deleteEntireHybridCardToolStripMenuItem.Text = "Delete entire hybrid card";
      this.deleteEntireHybridCardToolStripMenuItem.Click += new System.EventHandler(this.deleteEntireHybridCardToolStripMenuItem_Click);
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Enabled";
      this.columnHeader5.Width = 110;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Priority";
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Type";
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "Name";
      this.columnHeader8.Width = 200;
      // 
      // TvCards
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvCards";
      this.Size = new System.Drawing.Size(474, 412);
      this.contextMenuStrip2.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader colPriority;
    private System.Windows.Forms.ColumnHeader colType;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUp;
    private System.Windows.Forms.ColumnHeader colEnabled;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private System.Windows.Forms.ColumnHeader columnHeader8;
    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem deleteCardToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem deleteEntireHybridCardToolStripMenuItem;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
    private System.Windows.Forms.ToolStripMenuItem placeInHybridCardToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
    private System.Windows.Forms.ColumnHeader colCAMLimit;
    private System.Windows.Forms.Button buttonEdit;
		private System.Windows.Forms.ColumnHeader colInit;
    private System.Windows.Forms.Button buttonRemove;
  }
}