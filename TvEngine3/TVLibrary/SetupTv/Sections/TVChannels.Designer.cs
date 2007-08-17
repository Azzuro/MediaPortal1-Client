namespace SetupTv.Sections
{
  partial class TvChannels
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvChannels));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.hdrhekje = new System.Windows.Forms.ColumnHeader();
      this.hdrName = new System.Windows.Forms.ColumnHeader();
      this.hdrProvider = new System.Windows.Forms.ColumnHeader();
      this.hdrTypes = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail1 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail2 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail3 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addToFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteThisChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.renameMarkedChannelsBySIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.buttonDown = new System.Windows.Forms.Button();
      this.mpButtonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonExpert = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpButtonDeleteEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonPreview = new MediaPortal.UserInterface.Controls.MPButton();
      this.addSIDInFrontOfNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStrip1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrhekje,
            this.hdrName,
            this.hdrProvider,
            this.hdrTypes,
            this.hdrDetail1,
            this.hdrDetail2,
            this.hdrDetail3});
      this.mpListView1.ContextMenuStrip = this.contextMenuStrip1;
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LabelEdit = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(9, 11);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(438, 311);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 0;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListView1_MouseDoubleClick);
      this.mpListView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListView1_ColumnClick);
      this.mpListView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.mpListView1_AfterLabelEdit);
      this.mpListView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mpListView1_ItemDrag);
      // 
      // hdrhekje
      // 
      this.hdrhekje.Text = "#";
      // 
      // hdrName
      // 
      this.hdrName.Text = "Name";
      this.hdrName.Width = 114;
      // 
      // hdrProvider
      // 
      this.hdrProvider.Text = "Provider";
      // 
      // hdrTypes
      // 
      this.hdrTypes.Text = "Types";
      this.hdrTypes.Width = 50;
      // 
      // hdrDetail1
      // 
      this.hdrDetail1.Text = "Details";
      this.hdrDetail1.Width = 66;
      // 
      // hdrDetail2
      // 
      this.hdrDetail2.Text = "Details";
      this.hdrDetail2.Width = 50;
      // 
      // hdrDetail3
      // 
      this.hdrDetail3.Text = "Details";
      this.hdrDetail3.Width = 50;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToFavoritesToolStripMenuItem,
            this.deleteThisChannelToolStripMenuItem,
            this.editChannelToolStripMenuItem,
            this.toolStripMenuItem1,
            this.renameMarkedChannelsBySIDToolStripMenuItem,
            this.addSIDInFrontOfNameToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(256, 142);
      // 
      // addToFavoritesToolStripMenuItem
      // 
      this.addToFavoritesToolStripMenuItem.Name = "addToFavoritesToolStripMenuItem";
      this.addToFavoritesToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.addToFavoritesToolStripMenuItem.Text = "Add to favorites";
      // 
      // deleteThisChannelToolStripMenuItem
      // 
      this.deleteThisChannelToolStripMenuItem.Name = "deleteThisChannelToolStripMenuItem";
      this.deleteThisChannelToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.deleteThisChannelToolStripMenuItem.Text = "Delete this channel";
      this.deleteThisChannelToolStripMenuItem.Click += new System.EventHandler(this.deleteThisChannelToolStripMenuItem_Click);
      // 
      // editChannelToolStripMenuItem
      // 
      this.editChannelToolStripMenuItem.Name = "editChannelToolStripMenuItem";
      this.editChannelToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.editChannelToolStripMenuItem.Text = "Edit channel";
      this.editChannelToolStripMenuItem.Click += new System.EventHandler(this.editChannelToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(252, 6);
      // 
      // renameMarkedChannelsBySIDToolStripMenuItem
      // 
      this.renameMarkedChannelsBySIDToolStripMenuItem.Name = "renameMarkedChannelsBySIDToolStripMenuItem";
      this.renameMarkedChannelsBySIDToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.renameMarkedChannelsBySIDToolStripMenuItem.Text = "Rename selected channel(s) by SID";
      this.renameMarkedChannelsBySIDToolStripMenuItem.Click += new System.EventHandler(this.renameMarkedChannelsBySIDToolStripMenuItem_Click);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(1, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(2, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(3, "radio_fta_.png");
      // 
      // mpButtonClear
      // 
      this.mpButtonClear.Location = new System.Drawing.Point(394, 327);
      this.mpButtonClear.Name = "mpButtonClear";
      this.mpButtonClear.Size = new System.Drawing.Size(55, 23);
      this.mpButtonClear.TabIndex = 1;
      this.mpButtonClear.Text = "Clear";
      this.mpButtonClear.UseVisualStyleBackColor = true;
      this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(76, 350);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(55, 23);
      this.mpButtonDel.TabIndex = 1;
      this.mpButtonDel.Text = "Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Location = new System.Drawing.Point(9, 326);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(55, 23);
      this.buttonUtp.TabIndex = 3;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Location = new System.Drawing.Point(9, 350);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(55, 23);
      this.buttonDown.TabIndex = 4;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Location = new System.Drawing.Point(76, 326);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(55, 23);
      this.mpButtonEdit.TabIndex = 5;
      this.mpButtonEdit.Text = "Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      this.mpButtonEdit.Click += new System.EventHandler(this.mpButtonEdit_Click);
      // 
      // mpButtonExpert
      // 
      this.mpButtonExpert.Location = new System.Drawing.Point(142, 350);
      this.mpButtonExpert.Name = "mpButtonExpert";
      this.mpButtonExpert.Size = new System.Drawing.Size(55, 23);
      this.mpButtonExpert.TabIndex = 6;
      this.mpButtonExpert.Text = "Export";
      this.mpButtonExpert.UseVisualStyleBackColor = true;
      this.mpButtonExpert.Click += new System.EventHandler(this.mpButtonExpert_Click);
      // 
      // mpButtonImport
      // 
      this.mpButtonImport.Location = new System.Drawing.Point(142, 326);
      this.mpButtonImport.Name = "mpButtonImport";
      this.mpButtonImport.Size = new System.Drawing.Size(55, 23);
      this.mpButtonImport.TabIndex = 7;
      this.mpButtonImport.Text = "Import";
      this.mpButtonImport.UseVisualStyleBackColor = true;
      this.mpButtonImport.Click += new System.EventHandler(this.mpButtonImport_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(465, 400);
      this.tabControl1.TabIndex = 8;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpButtonDeleteEncrypted);
      this.tabPage1.Controls.Add(this.mpButtonAdd);
      this.tabPage1.Controls.Add(this.mpButtonPreview);
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.mpButtonImport);
      this.tabPage1.Controls.Add(this.mpButtonClear);
      this.tabPage1.Controls.Add(this.mpButtonExpert);
      this.tabPage1.Controls.Add(this.mpButtonDel);
      this.tabPage1.Controls.Add(this.mpButtonEdit);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.buttonDown);
      this.tabPage1.Controls.Add(this.buttonUtp);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(457, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Channels";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonDeleteEncrypted
      // 
      this.mpButtonDeleteEncrypted.Location = new System.Drawing.Point(339, 351);
      this.mpButtonDeleteEncrypted.Name = "mpButtonDeleteEncrypted";
      this.mpButtonDeleteEncrypted.Size = new System.Drawing.Size(110, 23);
      this.mpButtonDeleteEncrypted.TabIndex = 10;
      this.mpButtonDeleteEncrypted.Text = "Delete Scrambled";
      this.mpButtonDeleteEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonDeleteEncrypted.Click += new System.EventHandler(this.mpButtonDeleteEncrypted_Click);
      // 
      // mpButtonAdd
      // 
      this.mpButtonAdd.Location = new System.Drawing.Point(221, 351);
      this.mpButtonAdd.Name = "mpButtonAdd";
      this.mpButtonAdd.Size = new System.Drawing.Size(55, 23);
      this.mpButtonAdd.TabIndex = 9;
      this.mpButtonAdd.Text = "Add";
      this.mpButtonAdd.UseVisualStyleBackColor = true;
      this.mpButtonAdd.Click += new System.EventHandler(this.mpButtonAdd_Click);
      // 
      // mpButtonPreview
      // 
      this.mpButtonPreview.Location = new System.Drawing.Point(221, 326);
      this.mpButtonPreview.Name = "mpButtonPreview";
      this.mpButtonPreview.Size = new System.Drawing.Size(55, 23);
      this.mpButtonPreview.TabIndex = 8;
      this.mpButtonPreview.Text = "Preview";
      this.mpButtonPreview.UseVisualStyleBackColor = true;
      this.mpButtonPreview.Click += new System.EventHandler(this.mpButtonPreview_Click);
      // 
      // addSIDInFrontOfNameToolStripMenuItem
      // 
      this.addSIDInFrontOfNameToolStripMenuItem.Name = "addSIDInFrontOfNameToolStripMenuItem";
      this.addSIDInFrontOfNameToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.addSIDInFrontOfNameToolStripMenuItem.Text = "Add SID in front of name";
      this.addSIDInFrontOfNameToolStripMenuItem.Click += new System.EventHandler(this.addSIDInFrontOfNameToolStripMenuItem_Click);
      // 
      // TvChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvChannels";
      this.Size = new System.Drawing.Size(474, 412);
      this.Load += new System.EventHandler(this.TvChannels_Load);
      this.contextMenuStrip1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader hdrName;
    private System.Windows.Forms.ColumnHeader hdrTypes;
    private System.Windows.Forms.ColumnHeader hdrDetail1;
    private System.Windows.Forms.ColumnHeader hdrDetail2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClear;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private System.Windows.Forms.Button buttonUtp;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.ColumnHeader hdrhekje;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEdit;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonExpert;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonImport;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonPreview;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem addToFavoritesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem deleteThisChannelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editChannelToolStripMenuItem;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAdd;
    private System.Windows.Forms.ColumnHeader hdrDetail3;
    private System.Windows.Forms.ColumnHeader hdrProvider;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDeleteEncrypted;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem renameMarkedChannelsBySIDToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem addSIDInFrontOfNameToolStripMenuItem;
  }
}