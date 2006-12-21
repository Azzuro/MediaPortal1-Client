namespace SetupTv.Sections
{
  partial class TvCombinations
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
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addToFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.btnCombine = new System.Windows.Forms.Button();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewChannels = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.mpComboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.contextMenuStrip1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToFavoritesToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(159, 26);
      // 
      // addToFavoritesToolStripMenuItem
      // 
      this.addToFavoritesToolStripMenuItem.Name = "addToFavoritesToolStripMenuItem";
      this.addToFavoritesToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
      this.addToFavoritesToolStripMenuItem.Text = "Add to favorites";
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(465, 400);
      this.tabControl1.TabIndex = 8;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.btnCombine);
      this.tabPage2.Controls.Add(this.mpLabel3);
      this.tabPage2.Controls.Add(this.mpListViewMapped);
      this.tabPage2.Controls.Add(this.mpLabel2);
      this.tabPage2.Controls.Add(this.mpListViewChannels);
      this.tabPage2.Controls.Add(this.mpComboBoxCard);
      this.tabPage2.Controls.Add(this.mpLabel1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(457, 374);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Combinations";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // btnCombine
      // 
      this.btnCombine.Location = new System.Drawing.Point(372, 345);
      this.btnCombine.Name = "btnCombine";
      this.btnCombine.Size = new System.Drawing.Size(75, 23);
      this.btnCombine.TabIndex = 14;
      this.btnCombine.Text = "Combine";
      this.btnCombine.UseVisualStyleBackColor = true;
      this.btnCombine.Click += new System.EventHandler(this.btnCombine_Click);
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(251, 48);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(154, 13);
      this.mpLabel3.TabIndex = 13;
      this.mpLabel3.Text = "Similar channels on other cards";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = true;
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader5});
      this.mpListViewMapped.Location = new System.Drawing.Point(243, 67);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(204, 274);
      this.mpListViewMapped.TabIndex = 12;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "Ranking";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Name";
      this.columnHeader5.Width = 120;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(9, 48);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(54, 13);
      this.mpLabel2.TabIndex = 11;
      this.mpLabel2.Text = "Channels:";
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = true;
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6});
      this.mpListViewChannels.HideSelection = false;
      this.mpListViewChannels.Location = new System.Drawing.Point(12, 67);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(193, 274);
      this.mpListViewChannels.TabIndex = 10;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      this.mpListViewChannels.SelectedIndexChanged += new System.EventHandler(this.mpListViewChannels_SelectedIndexChanged);
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Name";
      this.columnHeader6.Width = 180;
      // 
      // mpComboBoxCard
      // 
      this.mpComboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCard.FormattingEnabled = true;
      this.mpComboBoxCard.Location = new System.Drawing.Point(67, 10);
      this.mpComboBoxCard.Name = "mpComboBoxCard";
      this.mpComboBoxCard.Size = new System.Drawing.Size(249, 21);
      this.mpComboBoxCard.TabIndex = 8;
      this.mpComboBoxCard.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCard_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(9, 13);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(32, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Card:";
      // 
      // TvCombinations
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvCombinations";
      this.Size = new System.Drawing.Size(474, 412);
      this.Load += new System.EventHandler(this.TvCombinations_Load);
      this.contextMenuStrip1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewMapped;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCard;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.ColumnHeader columnHeader8;
    private System.Windows.Forms.Button btnCombine;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem addToFavoritesToolStripMenuItem;
  }
}