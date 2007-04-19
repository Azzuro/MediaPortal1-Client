namespace SetupTv.Sections
{
  partial class TvEpgGrabber
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvEpgGrabber));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonAll = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonNone = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAllChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonNoneChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCheckBoxStoreOnlySelected = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButtonAllGrouped = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(12, 53);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(208, 347);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView1_ItemChecked);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 100;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Types";
      this.columnHeader3.Width = 90;
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
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(9, 33);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(119, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Grab EPG for channels:";
      // 
      // mpListView2
      // 
      this.mpListView2.AllowDrop = true;
      this.mpListView2.AllowRowReorder = true;
      this.mpListView2.CheckBoxes = true;
      this.mpListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListView2.Location = new System.Drawing.Point(256, 53);
      this.mpListView2.Name = "mpListView2";
      this.mpListView2.Size = new System.Drawing.Size(208, 347);
      this.mpListView2.TabIndex = 3;
      this.mpListView2.UseCompatibleStateImageBehavior = false;
      this.mpListView2.View = System.Windows.Forms.View.Details;
      this.mpListView2.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView2_ItemChecked);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 180;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(253, 33);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(131, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Filter for these languages: ";
      // 
      // mpButtonAll
      // 
      this.mpButtonAll.Location = new System.Drawing.Point(256, 406);
      this.mpButtonAll.Name = "mpButtonAll";
      this.mpButtonAll.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAll.TabIndex = 5;
      this.mpButtonAll.Text = "All";
      this.mpButtonAll.UseVisualStyleBackColor = true;
      this.mpButtonAll.Click += new System.EventHandler(this.mpButtonAll_Click);
      // 
      // mpButtonNone
      // 
      this.mpButtonNone.Location = new System.Drawing.Point(337, 406);
      this.mpButtonNone.Name = "mpButtonNone";
      this.mpButtonNone.Size = new System.Drawing.Size(75, 23);
      this.mpButtonNone.TabIndex = 6;
      this.mpButtonNone.Text = "None";
      this.mpButtonNone.UseVisualStyleBackColor = true;
      this.mpButtonNone.Click += new System.EventHandler(this.mpButtonNone_Click);
      // 
      // mpButtonAllChannels
      // 
      this.mpButtonAllChannels.Location = new System.Drawing.Point(12, 406);
      this.mpButtonAllChannels.Name = "mpButtonAllChannels";
      this.mpButtonAllChannels.Size = new System.Drawing.Size(61, 23);
      this.mpButtonAllChannels.TabIndex = 5;
      this.mpButtonAllChannels.Text = "All";
      this.mpButtonAllChannels.UseVisualStyleBackColor = true;
      this.mpButtonAllChannels.Click += new System.EventHandler(this.mpButtonAllChannels_Click);
      // 
      // mpButtonNoneChannels
      // 
      this.mpButtonNoneChannels.Location = new System.Drawing.Point(160, 406);
      this.mpButtonNoneChannels.Name = "mpButtonNoneChannels";
      this.mpButtonNoneChannels.Size = new System.Drawing.Size(60, 23);
      this.mpButtonNoneChannels.TabIndex = 6;
      this.mpButtonNoneChannels.Text = "None";
      this.mpButtonNoneChannels.UseVisualStyleBackColor = true;
      this.mpButtonNoneChannels.Click += new System.EventHandler(this.mpButtonNoneChannels_Click);
      // 
      // mpCheckBoxStoreOnlySelected
      // 
      this.mpCheckBoxStoreOnlySelected.AutoSize = true;
      this.mpCheckBoxStoreOnlySelected.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxStoreOnlySelected.Location = new System.Drawing.Point(12, 8);
      this.mpCheckBoxStoreOnlySelected.Name = "mpCheckBoxStoreOnlySelected";
      this.mpCheckBoxStoreOnlySelected.Size = new System.Drawing.Size(199, 17);
      this.mpCheckBoxStoreOnlySelected.TabIndex = 7;
      this.mpCheckBoxStoreOnlySelected.Text = "Store data only for selected channels";
      this.mpCheckBoxStoreOnlySelected.UseVisualStyleBackColor = true;
      // 
      // mpButtonAllGrouped
      // 
      this.mpButtonAllGrouped.Location = new System.Drawing.Point(79, 406);
      this.mpButtonAllGrouped.Name = "mpButtonAllGrouped";
      this.mpButtonAllGrouped.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAllGrouped.TabIndex = 8;
      this.mpButtonAllGrouped.Text = "All Grouped";
      this.mpButtonAllGrouped.UseVisualStyleBackColor = true;
      this.mpButtonAllGrouped.Click += new System.EventHandler(this.mpButtonAllGrouped_Click);
      // 
      // TvEpgGrabber
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpButtonAllGrouped);
      this.Controls.Add(this.mpCheckBoxStoreOnlySelected);
      this.Controls.Add(this.mpButtonNoneChannels);
      this.Controls.Add(this.mpButtonAllChannels);
      this.Controls.Add(this.mpButtonNone);
      this.Controls.Add(this.mpButtonAll);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpListView2);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpListView1);
      this.Name = "TvEpgGrabber";
      this.Size = new System.Drawing.Size(474, 442);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAll;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonNone;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAllChannels;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonNoneChannels;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxStoreOnlySelected;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAllGrouped;

  }
}