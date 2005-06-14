using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using MWCommon;
using MWControls;
using Microsoft.Win32;
using MediaPortal.Configuration.Controls;

using SQLite.NET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using DShowNET;

namespace MediaPortal.Configuration.Sections
{
	public class TVChannels : MediaPortal.Configuration.SectionSettings
	{
		public class ComboCard
		{
			public string FriendlyName;
			public string VideoDevice;
			public int    ID;
			public override string ToString()
			{
				return String.Format("{0} - {1}", FriendlyName, VideoDevice);
			}
		};
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private MediaPortal.UserInterface.Controls.MPListView channelsListView;
		private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button downButton;
    private System.Windows.Forms.Button upButton;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.Button btnClear;
		static bool reloadList=false;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.Button buttonAddGroup;
		private System.Windows.Forms.Button buttonDeleteGroup;
		private System.Windows.Forms.Button buttonEditGroup;
		private System.Windows.Forms.Button buttonGroupUp;
		private System.Windows.Forms.Button btnGroupDown;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ListView listViewGroups;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView listViewTVGroupChannels;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.Button btnUnmap;
		private System.Windows.Forms.Button buttonMap;
		private System.Windows.Forms.Button buttonCVS;
		private System.Windows.Forms.Button btnGrpChnUp;
		private System.Windows.Forms.Button btnGrpChnDown;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox comboBoxCard;
		private System.Windows.Forms.ListView listViewTVChannelsCard;
		private System.Windows.Forms.ListView listViewTVChannelsForCard;
		private System.Windows.Forms.Button btnMapChannelToCard;
		private System.Windows.Forms.Button btnUnmapChannelFromCard;
		private MWTreeView treeViewChannels;
		private System.Windows.Forms.OpenFileDialog XMLOpenDialog;
		private System.Windows.Forms.SaveFileDialog XMLSaveDialog;
		private System.Windows.Forms.Button xmlImport;
		private System.Windows.Forms.Button xmlExport;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Button buttonLookup;

		//
		// Private members
		//
		bool isDirty = false;

		public TVChannels() : this("Channels")
		{
		}

		public TVChannels(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			treeViewChannels.MultiSelect=TreeViewMultiSelect.MultiSameBranchAndLevel;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TVChannels));
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.buttonLookup = new System.Windows.Forms.Button();
      this.xmlImport = new System.Windows.Forms.Button();
      this.xmlExport = new System.Windows.Forms.Button();
      this.buttonCVS = new System.Windows.Forms.Button();
      this.channelsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.btnImport = new System.Windows.Forms.Button();
      this.btnClear = new System.Windows.Forms.Button();
      this.addButton = new System.Windows.Forms.Button();
      this.deleteButton = new System.Windows.Forms.Button();
      this.editButton = new System.Windows.Forms.Button();
      this.upButton = new System.Windows.Forms.Button();
      this.downButton = new System.Windows.Forms.Button();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.treeViewChannels = new MWControls.MWTreeView();
      this.btnGrpChnDown = new System.Windows.Forms.Button();
      this.btnGrpChnUp = new System.Windows.Forms.Button();
      this.buttonMap = new System.Windows.Forms.Button();
      this.btnUnmap = new System.Windows.Forms.Button();
      this.listViewTVGroupChannels = new System.Windows.Forms.ListView();
      this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.btnGroupDown = new System.Windows.Forms.Button();
      this.buttonGroupUp = new System.Windows.Forms.Button();
      this.buttonEditGroup = new System.Windows.Forms.Button();
      this.buttonDeleteGroup = new System.Windows.Forms.Button();
      this.buttonAddGroup = new System.Windows.Forms.Button();
      this.listViewGroups = new System.Windows.Forms.ListView();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.btnMapChannelToCard = new System.Windows.Forms.Button();
      this.btnUnmapChannelFromCard = new System.Windows.Forms.Button();
      this.listViewTVChannelsForCard = new System.Windows.Forms.ListView();
      this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
      this.listViewTVChannelsCard = new System.Windows.Forms.ListView();
      this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.comboBoxCard = new System.Windows.Forms.ComboBox();
      this.XMLOpenDialog = new System.Windows.Forms.OpenFileDialog();
      this.XMLSaveDialog = new System.Windows.Forms.SaveFileDialog();
      this.groupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Channel";
      this.columnHeader2.Width = 57;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.tabControl1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(464, 440);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Location = new System.Drawing.Point(16, 16);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(440, 416);
      this.tabControl1.TabIndex = 8;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.buttonLookup);
      this.tabPage1.Controls.Add(this.xmlImport);
      this.tabPage1.Controls.Add(this.xmlExport);
      this.tabPage1.Controls.Add(this.buttonCVS);
      this.tabPage1.Controls.Add(this.channelsListView);
      this.tabPage1.Controls.Add(this.btnImport);
      this.tabPage1.Controls.Add(this.btnClear);
      this.tabPage1.Controls.Add(this.addButton);
      this.tabPage1.Controls.Add(this.deleteButton);
      this.tabPage1.Controls.Add(this.editButton);
      this.tabPage1.Controls.Add(this.upButton);
      this.tabPage1.Controls.Add(this.downButton);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(432, 390);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "TV Channels";
      // 
      // buttonLookup
      // 
      this.buttonLookup.Location = new System.Drawing.Point(232, 352);
      this.buttonLookup.Name = "buttonLookup";
      this.buttonLookup.Size = new System.Drawing.Size(75, 24);
      this.buttonLookup.TabIndex = 13;
      this.buttonLookup.Text = "Lookup";
      this.buttonLookup.Click += new System.EventHandler(this.buttonLookup_Click);
      // 
      // xmlImport
      // 
      this.xmlImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.xmlImport.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.xmlImport.Location = new System.Drawing.Point(328, 344);
      this.xmlImport.Name = "xmlImport";
      this.xmlImport.Size = new System.Drawing.Size(88, 16);
      this.xmlImport.TabIndex = 12;
      this.xmlImport.Text = "Import from XML";
      this.xmlImport.Click += new System.EventHandler(this.xmlImport_Click_1);
      // 
      // xmlExport
      // 
      this.xmlExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.xmlExport.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.xmlExport.Location = new System.Drawing.Point(328, 360);
      this.xmlExport.Name = "xmlExport";
      this.xmlExport.Size = new System.Drawing.Size(88, 16);
      this.xmlExport.TabIndex = 11;
      this.xmlExport.Text = "Export to XML";
      this.xmlExport.Click += new System.EventHandler(this.xmlExport_Click_1);
      // 
      // buttonCVS
      // 
      this.buttonCVS.Location = new System.Drawing.Point(16, 344);
      this.buttonCVS.Name = "buttonCVS";
      this.buttonCVS.Size = new System.Drawing.Size(160, 23);
      this.buttonCVS.TabIndex = 8;
      this.buttonCVS.Text = "Add CVBS/SVHS channels";
      this.buttonCVS.Click += new System.EventHandler(this.buttonCVS_Click);
      // 
      // channelsListView
      // 
      this.channelsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.channelsListView.CheckBoxes = true;
      this.channelsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                       this.columnHeader1,
                                                                                       this.columnHeader2,
                                                                                       this.columnHeader5,
                                                                                       this.columnHeader4});
      this.channelsListView.FullRowSelect = true;
      this.channelsListView.HideSelection = false;
      this.channelsListView.Location = new System.Drawing.Point(8, 8);
      this.channelsListView.Name = "channelsListView";
      this.channelsListView.Size = new System.Drawing.Size(416, 304);
      this.channelsListView.SmallImageList = this.imageList1;
      this.channelsListView.TabIndex = 0;
      this.channelsListView.View = System.Windows.Forms.View.Details;
      this.channelsListView.DoubleClick += new System.EventHandler(this.channelsListView_DoubleClick);
      this.channelsListView.SelectedIndexChanged += new System.EventHandler(this.channelsListView_SelectedIndexChanged);
      this.channelsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.channelsListView_ColumnClick);
      this.channelsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.channelsListView_ItemCheck);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Channel name";
      this.columnHeader1.Width = 137;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Standard";
      this.columnHeader5.Width = 63;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Type";
      this.columnHeader4.Width = 155;
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
      this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // btnImport
      // 
      this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImport.Location = new System.Drawing.Point(232, 320);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new System.Drawing.Size(112, 23);
      this.btnImport.TabIndex = 6;
      this.btnImport.Text = "Import from tvguide";
      this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnClear.Location = new System.Drawing.Point(144, 320);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(32, 23);
      this.btnClear.TabIndex = 7;
      this.btnClear.Text = "Clear";
      this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.addButton.Location = new System.Drawing.Point(16, 320);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(32, 23);
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.deleteButton.Enabled = false;
      this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.deleteButton.Location = new System.Drawing.Point(96, 320);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(40, 23);
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.editButton.Enabled = false;
      this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.editButton.Location = new System.Drawing.Point(56, 320);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(32, 23);
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // upButton
      // 
      this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.upButton.Enabled = false;
      this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.upButton.Location = new System.Drawing.Point(344, 320);
      this.upButton.Name = "upButton";
      this.upButton.Size = new System.Drawing.Size(32, 23);
      this.upButton.TabIndex = 5;
      this.upButton.Text = "Up";
      this.upButton.Click += new System.EventHandler(this.upButton_Click);
      // 
      // downButton
      // 
      this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.downButton.Enabled = false;
      this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.downButton.Location = new System.Drawing.Point(376, 320);
      this.downButton.Name = "downButton";
      this.downButton.Size = new System.Drawing.Size(40, 23);
      this.downButton.TabIndex = 4;
      this.downButton.Text = "Down";
      this.downButton.Click += new System.EventHandler(this.downButton_Click);
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.treeViewChannels);
      this.tabPage3.Controls.Add(this.btnGrpChnDown);
      this.tabPage3.Controls.Add(this.btnGrpChnUp);
      this.tabPage3.Controls.Add(this.buttonMap);
      this.tabPage3.Controls.Add(this.btnUnmap);
      this.tabPage3.Controls.Add(this.listViewTVGroupChannels);
      this.tabPage3.Controls.Add(this.label3);
      this.tabPage3.Controls.Add(this.label2);
      this.tabPage3.Controls.Add(this.label1);
      this.tabPage3.Controls.Add(this.comboBox1);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(432, 390);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Map channels";
      // 
      // treeViewChannels
      // 
      this.treeViewChannels.FullRowSelect = true;
      this.treeViewChannels.ImageIndex = -1;
      this.treeViewChannels.Location = new System.Drawing.Point(16, 88);
      this.treeViewChannels.Name = "treeViewChannels";
      this.treeViewChannels.SelectedImageIndex = -1;
      this.treeViewChannels.Size = new System.Drawing.Size(168, 248);
      this.treeViewChannels.Sorted = true;
      this.treeViewChannels.TabIndex = 10;
      // 
      // btnGrpChnDown
      // 
      this.btnGrpChnDown.Location = new System.Drawing.Point(304, 344);
      this.btnGrpChnDown.Name = "btnGrpChnDown";
      this.btnGrpChnDown.Size = new System.Drawing.Size(56, 23);
      this.btnGrpChnDown.TabIndex = 9;
      this.btnGrpChnDown.Text = "Down";
      this.btnGrpChnDown.Click += new System.EventHandler(this.btnGrpChnDown_Click);
      // 
      // btnGrpChnUp
      // 
      this.btnGrpChnUp.Location = new System.Drawing.Point(264, 344);
      this.btnGrpChnUp.Name = "btnGrpChnUp";
      this.btnGrpChnUp.Size = new System.Drawing.Size(32, 23);
      this.btnGrpChnUp.TabIndex = 8;
      this.btnGrpChnUp.Text = "Up";
      this.btnGrpChnUp.Click += new System.EventHandler(this.btnGrpChnUp_Click);
      // 
      // buttonMap
      // 
      this.buttonMap.Location = new System.Drawing.Point(192, 184);
      this.buttonMap.Name = "buttonMap";
      this.buttonMap.Size = new System.Drawing.Size(32, 23);
      this.buttonMap.TabIndex = 7;
      this.buttonMap.Text = ">>";
      this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
      // 
      // btnUnmap
      // 
      this.btnUnmap.Location = new System.Drawing.Point(192, 224);
      this.btnUnmap.Name = "btnUnmap";
      this.btnUnmap.Size = new System.Drawing.Size(32, 23);
      this.btnUnmap.TabIndex = 6;
      this.btnUnmap.Text = "<<";
      this.btnUnmap.Click += new System.EventHandler(this.btnUnmap_Click);
      // 
      // listViewTVGroupChannels
      // 
      this.listViewTVGroupChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                              this.columnHeader9});
      this.listViewTVGroupChannels.FullRowSelect = true;
      this.listViewTVGroupChannels.HideSelection = false;
      this.listViewTVGroupChannels.Location = new System.Drawing.Point(240, 88);
      this.listViewTVGroupChannels.Name = "listViewTVGroupChannels";
      this.listViewTVGroupChannels.Size = new System.Drawing.Size(168, 240);
      this.listViewTVGroupChannels.TabIndex = 5;
      this.listViewTVGroupChannels.View = System.Windows.Forms.View.Details;
      this.listViewTVGroupChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewTVGroupChannels_ColumnClick);
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "TV Channel";
      this.columnHeader9.Width = 161;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(240, 64);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(120, 16);
      this.label3.TabIndex = 3;
      this.label3.Text = "TV channels in group";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(128, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "TVChannels available";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Group:";
      // 
      // comboBox1
      // 
      this.comboBox1.Location = new System.Drawing.Point(40, 32);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(280, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.btnGroupDown);
      this.tabPage2.Controls.Add(this.buttonGroupUp);
      this.tabPage2.Controls.Add(this.buttonEditGroup);
      this.tabPage2.Controls.Add(this.buttonDeleteGroup);
      this.tabPage2.Controls.Add(this.buttonAddGroup);
      this.tabPage2.Controls.Add(this.listViewGroups);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(432, 390);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Groups";
      // 
      // btnGroupDown
      // 
      this.btnGroupDown.Location = new System.Drawing.Point(240, 344);
      this.btnGroupDown.Name = "btnGroupDown";
      this.btnGroupDown.Size = new System.Drawing.Size(48, 23);
      this.btnGroupDown.TabIndex = 5;
      this.btnGroupDown.Text = "Down";
      this.btnGroupDown.Click += new System.EventHandler(this.btnGroupDown_Click);
      // 
      // buttonGroupUp
      // 
      this.buttonGroupUp.Location = new System.Drawing.Point(200, 344);
      this.buttonGroupUp.Name = "buttonGroupUp";
      this.buttonGroupUp.Size = new System.Drawing.Size(32, 23);
      this.buttonGroupUp.TabIndex = 4;
      this.buttonGroupUp.Text = "Up";
      this.buttonGroupUp.Click += new System.EventHandler(this.buttonGroupUp_Click);
      // 
      // buttonEditGroup
      // 
      this.buttonEditGroup.Location = new System.Drawing.Point(112, 344);
      this.buttonEditGroup.Name = "buttonEditGroup";
      this.buttonEditGroup.Size = new System.Drawing.Size(40, 23);
      this.buttonEditGroup.TabIndex = 3;
      this.buttonEditGroup.Text = "Edit";
      this.buttonEditGroup.Click += new System.EventHandler(this.buttonEditGroup_Click);
      // 
      // buttonDeleteGroup
      // 
      this.buttonDeleteGroup.Location = new System.Drawing.Point(56, 344);
      this.buttonDeleteGroup.Name = "buttonDeleteGroup";
      this.buttonDeleteGroup.Size = new System.Drawing.Size(48, 23);
      this.buttonDeleteGroup.TabIndex = 2;
      this.buttonDeleteGroup.Text = "Delete";
      this.buttonDeleteGroup.Click += new System.EventHandler(this.buttonDeleteGroup_Click);
      // 
      // buttonAddGroup
      // 
      this.buttonAddGroup.Location = new System.Drawing.Point(8, 344);
      this.buttonAddGroup.Name = "buttonAddGroup";
      this.buttonAddGroup.Size = new System.Drawing.Size(40, 23);
      this.buttonAddGroup.TabIndex = 1;
      this.buttonAddGroup.Text = "Add";
      this.buttonAddGroup.Click += new System.EventHandler(this.buttonAddGroup_Click);
      // 
      // listViewGroups
      // 
      this.listViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                     this.columnHeader6,
                                                                                     this.columnHeader7});
      this.listViewGroups.FullRowSelect = true;
      this.listViewGroups.HideSelection = false;
      this.listViewGroups.Location = new System.Drawing.Point(8, 8);
      this.listViewGroups.Name = "listViewGroups";
      this.listViewGroups.Size = new System.Drawing.Size(416, 304);
      this.listViewGroups.TabIndex = 0;
      this.listViewGroups.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Group name";
      this.columnHeader6.Width = 342;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Pincode";
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.btnMapChannelToCard);
      this.tabPage4.Controls.Add(this.btnUnmapChannelFromCard);
      this.tabPage4.Controls.Add(this.listViewTVChannelsForCard);
      this.tabPage4.Controls.Add(this.listViewTVChannelsCard);
      this.tabPage4.Controls.Add(this.label4);
      this.tabPage4.Controls.Add(this.label5);
      this.tabPage4.Controls.Add(this.label6);
      this.tabPage4.Controls.Add(this.comboBoxCard);
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(432, 390);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Cards";
      // 
      // btnMapChannelToCard
      // 
      this.btnMapChannelToCard.Location = new System.Drawing.Point(200, 152);
      this.btnMapChannelToCard.Name = "btnMapChannelToCard";
      this.btnMapChannelToCard.Size = new System.Drawing.Size(32, 23);
      this.btnMapChannelToCard.TabIndex = 15;
      this.btnMapChannelToCard.Text = ">>";
      this.btnMapChannelToCard.Click += new System.EventHandler(this.btnMapChannelToCard_Click);
      // 
      // btnUnmapChannelFromCard
      // 
      this.btnUnmapChannelFromCard.Location = new System.Drawing.Point(200, 184);
      this.btnUnmapChannelFromCard.Name = "btnUnmapChannelFromCard";
      this.btnUnmapChannelFromCard.Size = new System.Drawing.Size(32, 23);
      this.btnUnmapChannelFromCard.TabIndex = 14;
      this.btnUnmapChannelFromCard.Text = "<<";
      this.btnUnmapChannelFromCard.Click += new System.EventHandler(this.btnUnmapChannelFromCard_Click);
      // 
      // listViewTVChannelsForCard
      // 
      this.listViewTVChannelsForCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                this.columnHeader10});
      this.listViewTVChannelsForCard.Location = new System.Drawing.Point(244, 64);
      this.listViewTVChannelsForCard.Name = "listViewTVChannelsForCard";
      this.listViewTVChannelsForCard.Size = new System.Drawing.Size(168, 288);
      this.listViewTVChannelsForCard.TabIndex = 13;
      this.listViewTVChannelsForCard.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader10
      // 
      this.columnHeader10.Text = "TV Channel";
      this.columnHeader10.Width = 161;
      // 
      // listViewTVChannelsCard
      // 
      this.listViewTVChannelsCard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                             this.columnHeader11});
      this.listViewTVChannelsCard.Location = new System.Drawing.Point(20, 64);
      this.listViewTVChannelsCard.Name = "listViewTVChannelsCard";
      this.listViewTVChannelsCard.Size = new System.Drawing.Size(168, 288);
      this.listViewTVChannelsCard.TabIndex = 12;
      this.listViewTVChannelsCard.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "TV Channel";
      this.columnHeader11.Width = 159;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(248, 40);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(156, 16);
      this.label4.TabIndex = 11;
      this.label4.Text = "TV channels assigned to card";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(24, 40);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(136, 16);
      this.label5.TabIndex = 10;
      this.label5.Text = "TVChannels available";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(20, 16);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(36, 16);
      this.label6.TabIndex = 9;
      this.label6.Text = "Card:";
      // 
      // comboBoxCard
      // 
      this.comboBoxCard.Location = new System.Drawing.Point(64, 8);
      this.comboBoxCard.Name = "comboBoxCard";
      this.comboBoxCard.Size = new System.Drawing.Size(280, 21);
      this.comboBoxCard.TabIndex = 8;
      this.comboBoxCard.SelectedIndexChanged += new System.EventHandler(this.comboBoxCard_SelectedIndexChanged);
      // 
      // XMLOpenDialog
      // 
      this.XMLOpenDialog.DefaultExt = "xml";
      this.XMLOpenDialog.FileName = "ChannelList";
      this.XMLOpenDialog.Filter = "xml|*.xml";
      this.XMLOpenDialog.InitialDirectory = ".";
      this.XMLOpenDialog.Title = "Open....";
      // 
      // XMLSaveDialog
      // 
      this.XMLSaveDialog.CreatePrompt = true;
      this.XMLSaveDialog.DefaultExt = "xml";
      this.XMLSaveDialog.FileName = "ChannelList";
      this.XMLSaveDialog.Filter = "xml|*.xml";
      this.XMLSaveDialog.InitialDirectory = ".";
      this.XMLSaveDialog.Title = "Save to....";
      // 
      // TVChannels
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVChannels";
      this.Size = new System.Drawing.Size(472, 448);
      this.Load += new System.EventHandler(this.TVChannels_Load);
      this.groupBox1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion


		private void addButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			EditTVChannelForm editChannel = new EditTVChannelForm();

			editChannel.SortingPlace=channelsListView.Items.Count;
			DialogResult dialogResult = editChannel.ShowDialog(this);

			if(dialogResult == DialogResult.OK)
			{
				TelevisionChannel editedChannel = editChannel.Channel;

				ListViewItem listItem = new ListViewItem(new string[] { editedChannel.Name, 
																		editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString(),
                                    GetStandardName(editedChannel.standard),
                                    editedChannel.External ? "External" : "Internal"
																	  } );
				listItem.Tag = editedChannel;
				listItem.ImageIndex=0;
				if (editedChannel.Scrambled)
					listItem.ImageIndex=1;

				channelsListView.Items.Add(listItem);
				SaveSettings();
				UpdateGroupChannels(null,true);
			}
		}

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }

		private void editButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			foreach(ListViewItem listItem in channelsListView.SelectedItems)
			{
				EditTVChannelForm editChannel = new EditTVChannelForm();
				editChannel.Channel = listItem.Tag as TelevisionChannel;
				editChannel.SortingPlace=listItem.Index;;

				DialogResult dialogResult = editChannel.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					TelevisionChannel editedChannel = editChannel.Channel;
					listItem.Tag = editedChannel;

					listItem.SubItems[0].Text = editedChannel.Name;
					listItem.SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
          listItem.SubItems[2].Text = GetStandardName(editedChannel.standard);
          listItem.SubItems[3].Text = editedChannel.External ? "External" : "Internal";
					listItem.ImageIndex=0;
					if (editedChannel.Scrambled)
						listItem.ImageIndex=1;

					SaveSettings();
					UpdateGroupChannels(null,true);
				}
			}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			int itemCount = channelsListView.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				channelsListView.Items.RemoveAt(channelsListView.SelectedIndices[0]);
			}
			SaveSettings();
			UpdateGroupChannels(null,true);
		}

		public override object GetSetting(string name)
		{
			switch(name)
			{
				case "channel.highest":
					return HighestChannelNumber;
			}

			return null;
		}


		private int HighestChannelNumber
		{
			get 
			{
				int highestChannelNumber = 0;

				foreach(ListViewItem item in channelsListView.Items)
				{
					TelevisionChannel channel = item.Tag as TelevisionChannel;

					if(channel != null)
					{
						if(channel.Channel < (int)ExternalInputs.svhs && channel.Channel > highestChannelNumber)
							highestChannelNumber = channel.Channel;
					}
				}

				return highestChannelNumber;
			}
		}

		public override void LoadSettings()
		{
			LoadTVChannels();
			LoadGroups();
			LoadCards();
		}

		public override void SaveSettings()
		{
			if (reloadList)
			{
				LoadTVChannels();
				LoadGroups();
				LoadCards();
				reloadList=false;
				isDirty=true;
			}
			SaveTVChannels();
			SaveGroups();
		}

		private void SaveTVChannels()
		{
			if(isDirty == true)
			{
				SectionSettings section = SectionSettings.GetSection("Television");

				if(section != null)
				{
					int countryCode = (int)section.GetSetting("television.country");

					RegistryKey registryKey = Registry.LocalMachine;

					string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")
															  };

					//
					// Start by removing any old tv channels from the database and from the registry.
					// Information stored in the registry is the channel frequency.
					//
					ArrayList channels = new ArrayList();
					TVDatabase.GetChannels(ref channels);

					if(channels != null && channels.Count > 0)
					{
						foreach(MediaPortal.TV.Database.TVChannel channel in channels)
						{
							bool found=false;
							foreach(ListViewItem listItem in channelsListView.Items)
							{
								TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;
								if (channel.Name.ToLower() == tvChannel.Name.ToLower())
								{
									found=true;
									break;
								}
							}
							if (!found)
								TVDatabase.RemoveChannel(channel.Name);
						}

						//
						// Remove channel frequencies from the registry
						//
						for(int index = 0; index < registryLocations.Length; index++)
						{
							registryKey = Registry.LocalMachine;
							registryKey = registryKey.CreateSubKey(registryLocations[index]);

							for(int channelIndex = 0; channelIndex < 200; channelIndex++)
							{
								registryKey.DeleteValue(channelIndex.ToString(), false);
							}

							registryKey.Close();
						}
					}

					//
					// Add current channels
					//
					TVDatabase.GetChannels(ref channels);
					foreach(ListViewItem listItem in channelsListView.Items)
					{
						MediaPortal.TV.Database.TVChannel channel = new MediaPortal.TV.Database.TVChannel();
						TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

						if(tvChannel != null)
						{
							channel.Name = tvChannel.Name;
							channel.Number = tvChannel.Channel;
              channel.VisibleInGuide = tvChannel.VisibleInGuide;
							channel.Country=tvChannel.Country;
							channel.ID=tvChannel.ID;
							
							//
							// Calculate frequency
							//
							if(tvChannel.Frequency.Herz < 1000)
								tvChannel.Frequency.Herz *= 1000000L;

							channel.Frequency = tvChannel.Frequency.Herz;
              
              channel.External = tvChannel.External;
              channel.ExternalTunerChannel = tvChannel.ExternalTunerChannel;
              channel.TVStandard = tvChannel.standard;
							channel.Scrambled=tvChannel.Scrambled;

							//does channel already exists in database?
							bool exists=false;
							foreach (TVChannel chan in channels)
							{
								if (chan.Name.ToLower() == channel.Name.ToLower())
								{
									exists=true;
									break;
								}
							}
							
							if (exists)
							{
								TVDatabase.UpdateChannel(channel, listItem.Index);
							}
							else
							{
								TVDatabase.AddChannel(channel);

								//
								// Set the sort order
								//
								TVDatabase.SetChannelSort(channel.Name, listItem.Index);
							}
						}
					}

					//
					// Add frequencies to the registry
					//
					for(int index = 0; index < registryLocations.Length; index++)
					{
						registryKey = Registry.LocalMachine;
						registryKey = registryKey.CreateSubKey(registryLocations[index]);

						foreach(ListViewItem listItem in channelsListView.Items)
						{
							TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

							if(tvChannel != null)
							{
                //
                // Don't add frequency to the registry if it has no frequency or if we have the predefined
                // channels for Composite and SVIDEO
                //
                if(tvChannel.Frequency.Herz > 0 && 
                  tvChannel.Channel != (int)ExternalInputs.svhs && 
                  tvChannel.Channel != (int)ExternalInputs.cvbs1 &&
                  tvChannel.Channel != (int)ExternalInputs.cvbs2 &&
                  tvChannel.Channel != (int)ExternalInputs.rgb)
                {
                  registryKey.SetValue(tvChannel.Channel.ToString(), (int)tvChannel.Frequency.Herz);
                }
							}
						}

						registryKey.Close();
					}
				}
			}
		}

    private void AddChannel(ref ArrayList channels, string strName, int iNumber)
    {
      isDirty = true;

      TVChannel channel = new TVChannel();
      channel.Number=iNumber;
      channel.Name  =strName;
      channels.Add(channel);
    }

		/// <summary>
		/// 
		/// </summary>
		private void LoadTVChannels()
		{
      channelsListView.Items.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);

			foreach(TVChannel channel in channels)
			{
				TelevisionChannel tvChannel = new TelevisionChannel();

				tvChannel.ID=channel.ID;
				tvChannel.Channel	= channel.Number;
				tvChannel.Name		= channel.Name;
				tvChannel.Frequency	= channel.Frequency;
        tvChannel.External = channel.External;
        tvChannel.ExternalTunerChannel = channel.ExternalTunerChannel;
        tvChannel.VisibleInGuide = channel.VisibleInGuide;
				tvChannel.Country=channel.Country;
        tvChannel.standard = channel.TVStandard;
				tvChannel.Scrambled=channel.Scrambled;
				ListViewItem listItem = new ListViewItem(new string[] { tvChannel.Name, 
																		tvChannel.External ? String.Format("{0}/{1}", tvChannel.Channel, tvChannel.ExternalTunerChannel) : tvChannel.Channel.ToString(),
                                    GetStandardName(tvChannel.standard),
                                    tvChannel.External ? "External" : "Internal"
																	  } );

        listItem.Checked = tvChannel.VisibleInGuide;
				listItem.ImageIndex=0;
				if (tvChannel.Scrambled)
					listItem.ImageIndex=1;

				listItem.Tag = tvChannel;

				channelsListView.Items.Add(listItem);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void channelsListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);		
		}

    private void MoveSelectionDown()
    {
      isDirty = true;

      for(int index = channelsListView.Items.Count - 1; index >= 0; index--)
      {
        if(channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if(index < channelsListView.Items.Count - 1)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);

            if(index + 1 < channelsListView.Items.Count)
            {
              channelsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              channelsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
      isDirty = true;

      for(int index = 0; index < channelsListView.Items.Count; index++)
      {
        if(channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if(index > 0)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);
            channelsListView.Items.Insert(index - 1, listItem);
          }
        }
      }    
    }

    private void upButton_Click(object sender, System.EventArgs e)
    {
			MoveSelectionUp();
    }

    private void downButton_Click(object sender, System.EventArgs e)
    {
			MoveSelectionDown();
    }

    private void channelsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      deleteButton.Enabled = editButton.Enabled = upButton.Enabled = downButton.Enabled = (channelsListView.SelectedItems.Count > 0);
    }

    private void channelsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      isDirty = true;

      //
      // Fetch checked item
      //
      if(e.Index < channelsListView.Items.Count)
      {
        TelevisionChannel tvChannel = channelsListView.Items[e.Index].Tag as TelevisionChannel;

        tvChannel.VisibleInGuide = (e.NewValue == System.Windows.Forms.CheckState.Checked);

        channelsListView.Items[e.Index].Tag = tvChannel;
      }
    }

    private void btnImport_Click(object sender, System.EventArgs e)
    {
      using(MediaPortal.Profile.Xml  xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string strTVGuideFile=xmlreader.GetValueAsString("xmltv","folder","xmltv");
        strTVGuideFile=RemoveTrailingSlash(strTVGuideFile);
        strTVGuideFile+=@"\tvguide.xml";
        XMLTVImport import = new XMLTVImport();
        bool bSucceeded=import.Import(strTVGuideFile,true);
        if (bSucceeded)
        {
          string strtext=String.Format("Imported:{0} channels\r{1} programs\r{2}", 
                                import.ImportStats.Channels,
                                import.ImportStats.Programs,
                                import.ImportStats.Status);
          MessageBox.Show(this,strtext,"tvguide",MessageBoxButtons.OK,MessageBoxIcon.Error);

          isDirty =true;
          LoadTVChannels();
        }
        else
        {
          string strError=String.Format("Error importing tvguide from:\r{0}\rerror:{1}",
                              strTVGuideFile,import.ImportStats.Status);
          MessageBox.Show(this,strError,"Error importing tvguide",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
      }
    }
    string RemoveTrailingSlash(string strLine)
    {
      string strPath=strLine;
      while (strPath.Length>0)
      {
        if ( strPath[strPath.Length-1]=='\\' || strPath[strPath.Length-1]=='/')
        {
          strPath=strPath.Substring(0,strPath.Length-1);
        }
        else break;
      }
      return strPath;
    }

    private void btnClear_Click(object sender, System.EventArgs e)
    {
      DialogResult result=MessageBox.Show(this,"Are you sure you want to delete all channels?","Delete channels",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
      if (result!=DialogResult.Yes) return;
      channelsListView.Items.Clear();
			SaveSettings();
			UpdateGroupChannels(null,true);
    }

    private void buttonAutoTune_Click(object sender, System.EventArgs e)
    {
      isDirty =true;
      SaveTVChannels();
      AnalogTVTuningForm form = new AnalogTVTuningForm();
      form.ShowDialog(this);


      isDirty =true;
      LoadTVChannels();
    
    }
		static public void UpdateList()
		{
			reloadList=true;
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (reloadList)
			{
				reloadList=false;
				LoadTVChannels();
				LoadGroups();
				LoadCards();
			}
			base.OnPaint (e);
		}

		public void LoadGroups()
		{
			listViewGroups.Items.Clear();
			ArrayList groups = new ArrayList();
			TVDatabase.GetGroups(ref groups);
			foreach (TVGroup group in groups)
			{
				string pincode="No";
				if (group.Pincode!=0)
					pincode="Yes";
				ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode,} );
				listItem.Tag=group;
				listViewGroups.Items.Add(listItem);
				
			}
			UpdateGroupChannels(null,true);
		}

		private void buttonEditGroup_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			foreach(ListViewItem listItem in listViewGroups.SelectedItems)
			{
				EditGroupForm editgroup = new EditGroupForm();
				editgroup.Group = listItem.Tag as TVGroup;
				DialogResult dialogResult = editgroup.ShowDialog(this);
				if(dialogResult == DialogResult.OK)
				{
					TVGroup group = editgroup.Group;
					listItem.Tag = group;
					TVDatabase.DeleteGroup(group);
					group.ID=-1;
					
					string pincode="No";
					if (group.Pincode!=0)
						pincode="Yes";

					listItem.SubItems[0].Text = group.GroupName;
					listItem.SubItems[1].Text = pincode;
					TVDatabase.AddGroup(group);

					SaveTVChannels();
					SaveGroups();
					UpdateGroupChannels(group,true);
				}
			}				
		}

		private void buttonDeleteGroup_Click(object sender, System.EventArgs e)
		{

			int itemCount = listViewGroups.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				isDirty = true;
				ListViewItem item=listViewGroups.SelectedItems[0];
				TVGroup group=item.Tag as TVGroup;
				if(group!=null) TVDatabase.DeleteGroup(group);
				listViewGroups.Items.RemoveAt(listViewGroups.SelectedIndices[0]);
			}		

			SaveTVChannels();
			SaveGroups();
			UpdateGroupChannels(null,true);
		}

		private void buttonAddGroup_Click(object sender, System.EventArgs e)
		{

			EditGroupForm editGroup = new EditGroupForm();
			DialogResult dialogResult = editGroup.ShowDialog(this);
			if(dialogResult == DialogResult.OK)
			{
				isDirty = true;
				TVGroup group = editGroup.Group;
				string pincode="No";
				if (group.Pincode!=0)
					pincode="Yes";
				ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode,} );
				listItem.Tag=group;
				listViewGroups.Items.Add(listItem);
				
				SaveGroups();
				LoadGroups();
				LoadCards();
				SaveTVChannels();
				UpdateGroupChannels(group,true);

			}		

		}

		private void buttonGroupUp_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = 0; index < listViewGroups.Items.Count; index++)
			{
				if(listViewGroups.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't smaller than the lowest index (0) in the list view
					//
					if(index > 0)
					{
						ListViewItem listItem = listViewGroups.Items[index];
						listViewGroups.Items.RemoveAt(index);
						listViewGroups.Items.Insert(index - 1, listItem);
					}
				}
			}    
		}

		private void btnGroupDown_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = listViewGroups.Items.Count - 1; index >= 0; index--)
			{
				if(listViewGroups.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't greater than the highest index in the list view
					//
					if(index < listViewGroups.Items.Count - 1)
					{
						ListViewItem listItem = listViewGroups.Items[index];
						listViewGroups.Items.RemoveAt(index);

						if(index + 1 < listViewGroups.Items.Count)
						{
							listViewGroups.Items.Insert(index + 1, listItem);
						}
						else
						{
							listViewGroups.Items.Add(listItem);
						}
					}
				}
			}
		}

		private void SaveGroups()
		{
			if(isDirty == true)
			{
				for(int index = 0; index < listViewGroups.Items.Count ; index++)
				{
					ListViewItem listItem = listViewGroups.Items[index];
					TVGroup group = listItem.Tag as TVGroup;
					if (group!=null)
					{
						group.Sort=index;
						TVDatabase.AddGroup(group);
					}
				}
			}
		}

		private void buttonMap_Click(object sender, System.EventArgs e)
		{
			if (treeViewChannels.SelNodes==null) return;
			Hashtable htSelNodes = treeViewChannels.SelNodes.Clone() as Hashtable;
			treeViewChannels.SelNodes=null;
			foreach(MWTreeNodeWrapper node in htSelNodes.Values)
			{
				TVChannel chan=node.Node.Tag as TVChannel;
				if (chan==null) return;
				TVGroup group = comboBox1.SelectedItem as TVGroup;
				ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVGroupChannels.Items.Add(listItem);
				if (group!=null && chan != null)
					TVDatabase.MapChannelToGroup(group, chan);
				treeViewChannels.Nodes.Remove(node.Node);
			}

		}

		private void btnUnmap_Click(object sender, System.EventArgs e)
		{
			if (listViewTVGroupChannels.SelectedItems==null) return;
			for(int i=0; i < listViewTVGroupChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVGroupChannels.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;

				foreach (TreeNode node in treeViewChannels.Nodes)
				{ 
					if (node.Text==chan.ProviderName)
					{
						TreeNode subnode = new TreeNode(chan.Name);
						subnode.Tag=chan;
						node.Nodes.Add(subnode);
					}
				}
			}		
			TVGroup group = comboBox1.SelectedItem as TVGroup;
			for(int i=listViewTVGroupChannels.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewTVGroupChannels.SelectedItems[i];
				TVChannel channel=listItem.Tag as TVChannel;
				if (group!=null && channel != null)
					TVDatabase.UnmapChannelFromGroup(group, channel);
				listViewTVGroupChannels.Items.Remove(listItem);
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			UpdateGroupChannels(group,false);		
		}

		void UpdateGroupChannels(TVGroup group, bool reloadgroups)
		{
			
			if (reloadgroups || comboBox1.Items.Count==0)
			{
				comboBox1.Items.Clear();
				ArrayList groups = new ArrayList();
				TVDatabase.GetGroups(ref groups);
				foreach (TVGroup grp in groups)
				{
					comboBox1.Items.Add(grp);
				}
				if (comboBox1.Items.Count>0)
				{
					comboBox1.SelectedIndex=0;
					group=comboBox1.SelectedItem as TVGroup;
				}
			}

			ArrayList groupChannels = new ArrayList();
			listViewTVGroupChannels.Items.Clear();
			if (group!=null)
			{
				TVDatabase.GetTVChannelsForGroup(group);
				foreach (TVChannel chan in group.tvChannels)
				{
					ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
					listItem.Tag=chan;
					listViewTVGroupChannels.Items.Add(listItem);
					groupChannels.Add(chan);
				}
			}

			//fill in treeview with provider/channels
			string lastProvider="";
			TreeNode node=null;
			treeViewChannels.Nodes.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannelsByProvider(ref channels);
			foreach (TVChannel chan in channels)
			{
				bool add=true;
				foreach (TVChannel grpChan in groupChannels)
				{
					if (grpChan.Name == chan.Name)
					{
						add=false;
						break;
					}
				}
				if (add)
				{
					if (lastProvider!=chan.ProviderName)
					{
						lastProvider=chan.ProviderName;
						if(node!=null)
							treeViewChannels.Nodes.Add(node);
						node=new TreeNode(chan.ProviderName);
						node.Tag="";
					}
					TreeNode nodeChan = new TreeNode(chan.Name);
					nodeChan.Tag=chan;
					node.Nodes.Add(nodeChan);
				}
			}
			if(node!=null && node.Nodes.Count>0)
				treeViewChannels.Nodes.Add(node);
		}

		private void listViewTVGroupChannels_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			switch (listViewTVGroupChannels.Sorting)
			{
				case SortOrder.Ascending: listViewTVGroupChannels.Sorting = SortOrder.Descending; break;
				case SortOrder.Descending: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
				case SortOrder.None: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
			}	
			listViewTVGroupChannels.Sort();
			listViewTVGroupChannels.Update();
		
		}

		private void channelsListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			switch (channelsListView.Sorting)
			{
				case SortOrder.Ascending: channelsListView.Sorting = SortOrder.Descending; break;
				case SortOrder.Descending: channelsListView.Sorting = SortOrder.Ascending; break;
				case SortOrder.None: channelsListView.Sorting = SortOrder.Ascending; break;
			}	
			if (e.Column==1)
				channelsListView.ListViewItemSorter = new ListViewItemComparerInt(e.Column);
			else
				channelsListView.ListViewItemSorter = new ListViewItemComparer(e.Column);
			channelsListView.Sort();
			channelsListView.Update();
		}

		private void buttonCVS_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			UpdateGroupChannels(null,true);
			TVChannel chan = new TVChannel();
			chan.Name="CVBS#1"; chan.Number=(int)ExternalInputs.cvbs1; TVDatabase.AddChannel(chan);
			chan.Name="CVBS#2"; chan.Number=(int)ExternalInputs.cvbs2; TVDatabase.AddChannel(chan);
			chan.Name="SVHS";   chan.Number=(int)ExternalInputs.svhs; TVDatabase.AddChannel(chan);
			chan.Name="RGB";   chan.Number=(int)ExternalInputs.rgb; TVDatabase.AddChannel(chan);
			LoadSettings();
		}

		private void btnGrpChnUp_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				if(listViewTVGroupChannels.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't smaller than the lowest index (0) in the list view
					//
					if(index > 0)
					{
						ListViewItem listItem = listViewTVGroupChannels.Items[index];
						listViewTVGroupChannels.Items.RemoveAt(index);
						listViewTVGroupChannels.Items.Insert(index - 1, listItem);
					}
				}
			}    
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			TVDatabase.DeleteChannelsFromGroup(group);
			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				group.tvChannels.Clear();
				ListViewItem listItem = listViewTVGroupChannels.Items[index];
				group.tvChannels.Add (listItem.Tag);
				TVDatabase.MapChannelToGroup(group, (TVChannel)listItem.Tag);
			}
		}

		private void btnGrpChnDown_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = listViewTVGroupChannels.Items.Count - 1; index >= 0; index--)
			{
				if(listViewTVGroupChannels.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't greater than the highest index in the list view
					//
					if(index < listViewTVGroupChannels.Items.Count - 1)
					{
						ListViewItem listItem = listViewTVGroupChannels.Items[index];
						listViewTVGroupChannels.Items.RemoveAt(index);

						if(index + 1 < listViewTVGroupChannels.Items.Count)
						{
							listViewTVGroupChannels.Items.Insert(index + 1, listItem);
						}
						else
						{
							listViewTVGroupChannels.Items.Add(listItem);
						}
					}
				}
			}
			
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			TVDatabase.DeleteChannelsFromGroup(group);
			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				group.tvChannels.Clear();
				ListViewItem listItem = listViewTVGroupChannels.Items[index];
				group.tvChannels.Add (listItem.Tag);
				TVDatabase.MapChannelToGroup(group, (TVChannel)listItem.Tag);
			}
		
		}

		void LoadCards()
		{
			comboBoxCard.Items.Clear();
			if(File.Exists("capturecards.xml"))
			{
				using(FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					try
					{
						//
						// Create Soap Formatter
						//
						SoapFormatter formatter = new SoapFormatter();

						//
						// Serialize
						//
						ArrayList captureCards = new ArrayList();
						captureCards = (ArrayList)formatter.Deserialize(fileStream);
						for (int i=0; i < captureCards.Count; i++)
						{
							((TVCaptureDevice)captureCards[i]).ID=(i+1);
							((TVCaptureDevice)captureCards[i]).LoadDefinitions();


							TVCaptureDevice device=(TVCaptureDevice)captureCards[i];
							ComboCard combo = new ComboCard();
							combo.FriendlyName=device.FriendlyName;
							combo.VideoDevice=device.VideoDevice;
							combo.ID=device.ID;
							comboBoxCard.Items.Add(combo);

						}
						//
						// Finally close our file stream
						//
						fileStream.Close();
					}
					catch
					{
						MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}

			
			if (comboBoxCard.Items.Count!=0)
				comboBoxCard.SelectedIndex=0;
			FillInChannelCardMappings();
		}
		
		void FillInChannelCardMappings()
		{
			int card=1;
			int index =comboBoxCard.SelectedIndex;
			if (index >=0)
			{
				ComboCard combo=(ComboCard )comboBoxCard.Items[index];
				card=combo.ID;
			}

			listViewTVChannelsCard.Items.Clear();
			listViewTVChannelsForCard.Items.Clear();
			ArrayList cardChannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref cardChannels, card);
			
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				bool mapped=false;
				foreach (TVChannel chanCard in cardChannels)
				{
					if (chanCard.Name==chan.Name) 
					{
						mapped=true;
						break;
					}
				}
				if (!mapped)
				{
					ListViewItem newItem = new ListViewItem(chan.Name);
					newItem.Tag=chan;
					listViewTVChannelsCard.Items.Add(newItem);
				}
			}

			foreach (TVChannel chanCard in cardChannels)
			{
				ListViewItem newItemCard = new ListViewItem(chanCard.Name);
				newItemCard.Tag=chanCard;
				listViewTVChannelsForCard.Items.Add(newItemCard);
			}
		}

		private void comboBoxCard_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			FillInChannelCardMappings();
		}

		private void btnMapChannelToCard_Click(object sender, System.EventArgs e)
		{
			if (listViewTVChannelsCard.SelectedItems==null) return;
			int card=1;
			int index =comboBoxCard.SelectedIndex;
			if (index >=0)
			{
				ComboCard combo=(ComboCard )comboBoxCard.Items[index];
				card=combo.ID;
			}
			
			for(int i=0; i < listViewTVChannelsCard.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVChannelsCard.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;
				
				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVChannelsForCard.Items.Add(listItem);
				if (chan != null)
					TVDatabase.MapChannelToCard(chan.ID,card);
			}
			
			for(int i=listViewTVChannelsCard.SelectedItems.Count-1; i >=0 ;i--)
			{
				ListViewItem listItem=listViewTVChannelsCard.SelectedItems[i];

				listViewTVChannelsCard.Items.Remove(listItem);
			}		
		}

		private void btnUnmapChannelFromCard_Click(object sender, System.EventArgs e)
		{
			int card=1;
			int index =comboBoxCard.SelectedIndex;
			if (index >=0)
			{
				ComboCard combo=(ComboCard )comboBoxCard.Items[index];
				card=combo.ID;
			}
			if (listViewTVChannelsForCard.SelectedItems==null) return;
			for(int i=0; i < listViewTVChannelsForCard.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVChannelsForCard.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;

				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVChannelsCard.Items.Add(listItem);
			}		

			for(int i=listViewTVChannelsForCard.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewTVChannelsForCard.SelectedItems[i];
				TVChannel channel=listItem.Tag as TVChannel;
				if (channel != null)
					TVDatabase.UnmapChannelFromCard(channel,card);
				listViewTVChannelsForCard.Items.Remove(listItem);
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SaveSettings();
			LoadSettings();			
		}

		private void Export_to_XML(string fileStr)
		{
			//Create flags to delete file if there is no data to export
			bool CHANNEL_EXPORT = false;
			bool GROUP_EXPORT = false;
			bool CARD_EXPORT = false;
			bool RECORDED_EXPORT = false;
			bool RECORDINGS_EXPORT = false;
			
			//Current version number of this exporter (change when needed)
			int CURRENT_VERSION = 1;  //<--- Make sure this same number is given to Import_from_XML
			
			using(MediaPortal.Profile.Xml channels = new MediaPortal.Profile.Xml(fileStr))
			{
				//Channel Data
				channels.Clear();
				channels.SetValue("MP channel export list","version",CURRENT_VERSION.ToString());
				if(channelsListView.Items.Count==0)
				{
					MessageBox.Show("No channels to export");
					CHANNEL_EXPORT=false;
				}
				else
				{
					foreach(ListViewItem listItem in channelsListView.Items)
					{
						TelevisionChannel Selected_Chan = listItem.Tag as TelevisionChannel;

						//Set index
						channels.SetValue(listItem.Index.ToString(),"INDEX",listItem.Index.ToString());
				
						//Channel data
						channels.SetValueAsBool(listItem.Index.ToString(),"Scrambled",Selected_Chan.Scrambled);
						channels.SetValue(listItem.Index.ToString(),"ID",Selected_Chan.ID.ToString());
						channels.SetValue(listItem.Index.ToString(),"Number",Selected_Chan.Channel.ToString());
						channels.SetValue(listItem.Index.ToString(),"Name",Selected_Chan.Name.ToString());
						channels.SetValue(listItem.Index.ToString(),"Country",Selected_Chan.Country.ToString());
						channels.SetValueAsBool(listItem.Index.ToString(),"External",Selected_Chan.External);
						channels.SetValue(listItem.Index.ToString(),"External Tuner Channel",Selected_Chan.ExternalTunerChannel.ToString());
						channels.SetValue(listItem.Index.ToString(),"Frequency",Selected_Chan.Frequency.ToString());
						channels.SetValue(listItem.Index.ToString(),"Analog Standard Index",Selected_Chan.standard.ToString());
						channels.SetValueAsBool(listItem.Index.ToString(),"Visible in Guide",Selected_Chan.VisibleInGuide);	
						if (Selected_Chan.Channel>=0)
						{
							int bandWidth,freq,ONID,TSID,SID,symbolrate,innerFec,modulation, audioPid,videoPid,teletextPid,pmtPid;
							string provider;
							int audio1, audio2, audio3, ac3Pid;
							string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
						
							//DVB-T
							TVDatabase.GetDVBTTuneRequest(Selected_Chan.ID,out provider,out freq,out ONID, out TSID,out SID, out audioPid,out videoPid,out teletextPid, out pmtPid,out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
							channels.SetValue(listItem.Index.ToString(),"DVBTFreq",freq.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTONID",ONID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTTSID",TSID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTSID",SID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTProvider",provider);
							channels.SetValue(listItem.Index.ToString(),"DVBTAudioPid",audioPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTVideoPid",videoPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTTeletextPid",teletextPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTPmtPid",pmtPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTBandwidth",bandWidth.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTAudio1Pid",audio1.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTAudio2Pid",audio2.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTAudio3Pid",audio3.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTAC3Pid",ac3Pid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBTAudioLanguage",audioLanguage);
							channels.SetValue(listItem.Index.ToString(),"DVBTAudioLanguage1",audioLanguage1);
							channels.SetValue(listItem.Index.ToString(),"DVBTAudioLanguage2",audioLanguage2);
							channels.SetValue(listItem.Index.ToString(),"DVBTAudioLanguage3",audioLanguage3);

							//DVB-C
							TVDatabase.GetDVBCTuneRequest(Selected_Chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
							channels.SetValue(listItem.Index.ToString(),"DVBCFreq",freq.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCONID",ONID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCTSID",TSID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCSID",SID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCSR",symbolrate.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCInnerFeq",innerFec.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCModulation",modulation.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCProvider",provider);
							channels.SetValue(listItem.Index.ToString(),"DVBCAudioPid",audioPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCVideoPid",videoPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCTeletextPid",teletextPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCPmtPid",pmtPid.ToString());

							channels.SetValue(listItem.Index.ToString(),"DVBCAudio1Pid",audio1.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCAudio2Pid",audio2.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCAudio3Pid",audio3.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCAC3Pid",ac3Pid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCAudioLanguage",audioLanguage);
							channels.SetValue(listItem.Index.ToString(),"DVBCAudioLanguage1",audioLanguage1);
							channels.SetValue(listItem.Index.ToString(),"DVBCAudioLanguage2",audioLanguage2);
							channels.SetValue(listItem.Index.ToString(),"DVBCAudioLanguage3",audioLanguage3);
							//DVB-S
							DVBChannel ch = new DVBChannel();
							TVDatabase.GetSatChannel(Selected_Chan.ID,1,ref ch);
							channels.SetValue(listItem.Index.ToString(),"DVBSFreq",ch.Frequency.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSONID",ch.NetworkID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSTSID",ch.TransportStreamID.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSSID",ch.ProgramNumber.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSSymbolrate",ch.Symbolrate.ToString());
							channels.SetValue(listItem.Index.ToString(),"DvbSInnerFec",ch.FEC.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSPolarisation",ch.Polarity.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSProvider",ch.ServiceProvider);
							channels.SetValue(listItem.Index.ToString(),"DVBSAudioPid",ch.AudioPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSVideoPid",ch.VideoPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSTeletextPid",ch.TeletextPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSECMpid",ch.ECMPid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBCPmtPid",ch.PMTPid.ToString());

							channels.SetValue(listItem.Index.ToString(),"DVBSAudio1Pid",ch.Audio1.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSAudio2Pid",ch.Audio2.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSAudio3Pid",ch.Audio3.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSAC3Pid",ch.AC3Pid.ToString());
							channels.SetValue(listItem.Index.ToString(),"DVBSAudioLanguage",ch.AudioLanguage);
							channels.SetValue(listItem.Index.ToString(),"DVBSAudioLanguage1",ch.AudioLanguage1);
							channels.SetValue(listItem.Index.ToString(),"DVBSAudioLanguage2",ch.AudioLanguage2);
							channels.SetValue(listItem.Index.ToString(),"DVBSAudioLanguage3",ch.AudioLanguage3);

						}
					}
					CHANNEL_EXPORT=true;
				}

				//Group Data and channel maping
				ArrayList groups = new ArrayList();
				TVDatabase.GetGroups(ref groups);
				//Save total groups for reference
				if(groups.Count==0)
				{
					MessageBox.Show("No groups to export");
					GROUP_EXPORT=false;
				}
				else
				{
					channels.SetValue("GROUPS","TOTAL",groups.Count.ToString());
					GROUP_EXPORT=true;
				}
				for(int i = 0;i<groups.Count;i++)
				{
					foreach (TVGroup group in groups)
					{
						if(group.Sort==i)
						{
							channels.SetValue("Group "+i.ToString(),"ID",group.ID.ToString());
							channels.SetValue("Group "+i.ToString(),"NAME",group.GroupName);
							channels.SetValue("Group "+i.ToString(),"PINCODE",group.Pincode.ToString());
							channels.SetValue("Group "+i.ToString(),"SORT",group.Sort.ToString());
					
							//Save total channels added to this group
							TVDatabase.GetTVChannelsForGroup(group);
							channels.SetValue("Group "+i.ToString(),"TOTAL CHANNELS",group.tvChannels.Count.ToString());
							
							//Save current channel ID's under this group
							int channel_index=0;
							foreach (TVChannel tvChan in group.tvChannels)
							{
								channels.SetValue("Group "+i.ToString(),"CHANNEL "+channel_index.ToString(),tvChan.ID.ToString());
								channel_index++;
							}
							break;
						}
					}
				}

				//Card mapping data
				ArrayList Cards = new ArrayList();
				TVDatabase.GetCards(ref Cards);
				
				//If we have no cards skip this
				if(Cards.Count==0)
				{
					MessageBox.Show("No card data to export");
					CARD_EXPORT=false;
				}
				else
				{
					channels.SetValue("CARDS","TOTAL",Cards.Count.ToString());
					CARD_EXPORT=true;
				}
				for(int i = 1;i<Cards.Count+1;i++)
				{
					try
					{
						//ArrayList Channels = new ArrayList();
						//TVDatabase.GetChannels(ref Channels);
						ArrayList tmpChannels = new ArrayList();
						TVDatabase.GetChannelsForCard(ref tmpChannels,i);
						channels.SetValue("Card "+ i.ToString(),"ID",i.ToString());
						channels.SetValue("Card "+ i.ToString(),"TOTAL CHANNELS",tmpChannels.Count.ToString());
						int channel_index=0;
						foreach(TVChannel tmpChan in tmpChannels)
						{
							channels.SetValue("Card "+ i.ToString(),"CHANNEL "+channel_index.ToString(),tmpChan.ID.ToString());
							channel_index++;
						}
					}
					catch{}
				}

				//Backup recorded shows information
				ArrayList Recorded = new ArrayList();
				TVDatabase.GetRecordedTV(ref Recorded);

				if(Recorded.Count==0)
				{
					MessageBox.Show("There is no Recorded TV data to export");
					RECORDED_EXPORT=false;
				}
				else
				{
					channels.SetValue("RECORDED","TOTAL",Recorded.Count.ToString());
					RECORDED_EXPORT=true;
				}
				
				int count=0;
				foreach(TVRecorded show in Recorded)
				{	
					channels.SetValue("Recorded "+count.ToString(),"ID",show.ID.ToString());
					channels.SetValue("Recorded "+count.ToString(),"TITLE",show.Title);
					channels.SetValue("Recorded "+count.ToString(),"CHANNEL",show.Channel);
					channels.SetValue("Recorded "+count.ToString(),"DESC",show.Description);
					channels.SetValue("Recorded "+count.ToString(),"GENRE",show.Genre);
					channels.SetValue("Recorded "+count.ToString(),"FILENAME",show.FileName);
					channels.SetValue("Recorded "+count.ToString(),"START",show.Start.ToString());
					channels.SetValue("Recorded "+count.ToString(),"ENDTIME",show.End.ToString());
					channels.SetValue("Recorded "+count.ToString(),"PLAYED",show.Played.ToString());
					count++;
				}

				//Backup recording shows information
				ArrayList Recordings = new ArrayList();
				TVDatabase.GetRecordings(ref Recordings);

				if(Recordings.Count==0)
				{
					MessageBox.Show("There is no Recording TV data to export");
					RECORDINGS_EXPORT=false;
				}
				else
				{
					channels.SetValue("RECORDINGS","TOTAL",Recordings.Count.ToString());
					RECORDINGS_EXPORT=true;
				}

				for(int i=1;i<Recordings.Count+1;i++)
				{
					MediaPortal.TV.Database.TVRecording show=(MediaPortal.TV.Database.TVRecording )Recordings[i-1];
					channels.SetValue("Recording "+i.ToString(),"ID",show.ID.ToString());
					channels.SetValue("Recording "+i.ToString(),"TITLE",show.Title);
					channels.SetValue("Recording "+i.ToString(),"CHANNEL",show.Channel);
					channels.SetValue("Recording "+i.ToString(),"STARTTIME",show.Start.ToString());
					channels.SetValue("Recording "+i.ToString(),"ENDTIME",show.End.ToString());
					channels.SetValue("Recording "+i.ToString(),"CANCELEDTIME",show.Canceled.ToString());
					channels.SetValue("Recording "+i.ToString(),"TYPE",show.RecType.ToString());
					channels.SetValue("Recording "+i.ToString(),"PRIORITY",show.Priority.ToString());
					channels.SetValue("Recording "+i.ToString(),"QUALITY",show.Quality.ToString());
					//channels.SetValue("Recording "+i.ToString(),"STATUS",show.Status.ToString());
					channels.SetValueAsBool("Recording "+i.ToString(),"ISCONTENTREC",show.IsContentRecording);
					channels.SetValueAsBool("Recording "+i.ToString(),"SERIES",show.Series);
					channels.SetValue("Recording "+i.ToString(),"EPISODES",show.EpisodesToKeep.ToString());
					
					
					//Check if this recording has had any cancels
					channels.SetValue("Recording "+i.ToString(),"CANCELED SERIES TOTAL",show.CanceledSeries.Count.ToString());
					if(show.CanceledSeries.Count>0)
					{	
						int canx_count = 0;
						ArrayList get_show = (ArrayList)show.CanceledSeries;
						foreach(long canx_show in get_show)
						{
							channels.SetValue("Recording "+i.ToString(),"CANCELED SERIES CANCELEDTIME "+canx_count.ToString(),canx_show.ToString());
							canx_count++;
						}
					}
				}

			}

			//Check to see if we need to delete file
			if(!CHANNEL_EXPORT&&!GROUP_EXPORT&&!CARD_EXPORT&&!RECORDED_EXPORT&&!RECORDINGS_EXPORT)
			{
				//Delete file
				File.Delete(fileStr);
				return;
			}
		}

		private void Import_From_XML(string fileStr)
		{
			//Check if we have a file just in case
			if(!File.Exists(fileStr))return;

			//Current Version change to reflect the above exporter in order for compatibility
			int CURRENT_VER=1;   //<--- Make sure that is the same number as in Export_to_XML
			int VER=1;			 //Set to:  0 = old ; 1 = current ; 2 = newer
			using(MediaPortal.Profile.Xml channels = new MediaPortal.Profile.Xml(fileStr))
			{
				//Check version if not the right version prompt/do stuff/accomodate/change
				int version_check = channels.GetValueAsInt("MP channel export list","version",-1);
				if(version_check==-1)
				{
					//Not a valid channel list
					MessageBox.Show("This is not a valid channel list!");
					return;
				}
				else if(version_check>=0&&version_check<CURRENT_VER)
				{
					//Older file
					MessageBox.Show("This is an older channel list file I will attempt to import what I can.");	
					VER=0;
				}
				else if(version_check==CURRENT_VER)
				{
					//Current file, this is good stuff
					VER=1;
				}
				else if(version_check>CURRENT_VER)
				{
					//Newer? This person lives in a cave
					MessageBox.Show("This is a newer channel list file I will attempt to get what I can.\nConsider upgrading to a newer version of MP.");	
					VER=2;
				}

				//Count how many channels we have to import
				int counter=0;
				for(int i=0;;i++)
				{
					if(channels.GetValueAsInt(i.ToString(),"INDEX",-1)==-1)
					{
						if(counter==0)
						{
							MessageBox.Show("No channels found");
							return;
						}
						else break;
					}
					else counter++;
				}
				MessageBox.Show("There is a total of "+counter.ToString()+" stations to import");
				
				for(int i=0;i<counter;i++)
				{	
					int overwrite = 0;
					int overwrite_index=0;
					TelevisionChannel Import_Chan = new TelevisionChannel();
					Import_Chan.ID=channels.GetValueAsInt(i.ToString(),"ID",0);
					Import_Chan.Channel=channels.GetValueAsInt(i.ToString(),"Number",0);
					Import_Chan.Name=channels.GetValueAsString(i.ToString(),"Name","");
					Import_Chan.Country=channels.GetValueAsInt(i.ToString(),"Country",0);
					Import_Chan.External=channels.GetValueAsBool(i.ToString(),"External",false);
					Import_Chan.ExternalTunerChannel=channels.GetValueAsString(i.ToString(),"External Tuner Channel","");
					Import_Chan.Frequency.MegaHerz=Convert.ToDouble(channels.GetValueAsFloat(i.ToString(),"Frequency",0));
					Import_Chan.standard=Convert_AVS(channels.GetValueAsString(i.ToString(),"Analog Standard Index","None"));
					Import_Chan.VisibleInGuide=channels.GetValueAsBool(i.ToString(),"Visible in Guide",false);	
					Import_Chan.Scrambled=channels.GetValueAsBool(i.ToString(),"Scrambled",false);
					
					//Check to see if this channel exists prompt to overwrite
					foreach(ListViewItem listItem in channelsListView.Items)
					{
						TelevisionChannel Check_Chan = listItem.Tag as TelevisionChannel;
						if(Check_Chan.ID==Import_Chan.ID&&Check_Chan.Name==Import_Chan.Name)
						{
							if(MessageBox.Show(Import_Chan.Name+" (Channel "+Import_Chan.Channel.ToString()+") Already exists.\nWould you like to overwrite?","Warning!",MessageBoxButtons.YesNo)==DialogResult.Yes)
							{
								overwrite = 1;
								overwrite_index=listItem.Index;
							}
							else 
							{
								overwrite = -1;
							}
							break;
						}
					}

					if(overwrite==0||overwrite==1)
					{
						if(overwrite==1)
						{
							TelevisionChannel editedChannel = Import_Chan;
							channelsListView.Items[overwrite_index].Tag = editedChannel;

							channelsListView.Items[overwrite_index].SubItems[0].Text = editedChannel.Name;
							channelsListView.Items[overwrite_index].SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
							channelsListView.Items[overwrite_index].SubItems[2].Text = GetStandardName(editedChannel.standard);
							channelsListView.Items[overwrite_index].SubItems[3].Text = editedChannel.External ? "External" : "Internal";
						}
						else 
						{
							TelevisionChannel editedChannel = Import_Chan;
							ListViewItem listItem = new ListViewItem(new string[] { editedChannel.Name, 
																					  editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString(),
																					  GetStandardName(editedChannel.standard),
																					  editedChannel.External ? "External" : "Internal"
																				  } );
							listItem.Tag = editedChannel;

							channelsListView.Items.Add(listItem);
							channelsListView.Items[channelsListView.Items.IndexOf(listItem)].Checked = true;
						}

						//Check if required to do anything specific for compatibility reasons
						switch(VER)
						{
								//Do stuff for backward compatibility if needed
							case 0:

								break;
								//Do stuff for current version only
							case 1:

								break;
								//Do stuff for forward compatibility if needed
							case 2:

								break;
						}
						
						//This is done for every version regardless
						if(VER==0||VER==1||VER==2)
						{
							if (Import_Chan.Channel>=0)
							{
								int freq,ONID,TSID,SID,symbolrate,innerFec,modulation,polarisation;
								int bandWidth,pmtPid,audioPid,videoPid,teletextPid;
								int audio1, audio2, audio3, ac3Pid;
								string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
								string provider;
								//dvb-T
								try
								{
									freq=channels.GetValueAsInt(i.ToString(),"DVBTFreq",0);
									ONID=channels.GetValueAsInt(i.ToString(),"DVBTONID",0);
									TSID=channels.GetValueAsInt(i.ToString(),"DVBTTSID",0);
									SID=channels.GetValueAsInt(i.ToString(),"DVBTSID",0);
									audioPid=channels.GetValueAsInt(i.ToString(),"DVBTAudioPid",0);
									videoPid=channels.GetValueAsInt(i.ToString(),"DVBTVideoPid",0);
									teletextPid=channels.GetValueAsInt(i.ToString(),"DVBTTeletextPid",0);
									pmtPid=channels.GetValueAsInt(i.ToString(),"DVBTPmtPid",0);
									provider=channels.GetValueAsString(i.ToString(),"DVBTProvider","");
									bandWidth=channels.GetValueAsInt(i.ToString(),"DVBTBandwidth",-1);
									audio1=channels.GetValueAsInt(i.ToString(),"DVBTAudio1Pid",-1);
									audio2=channels.GetValueAsInt(i.ToString(),"DVBTAudio2Pid",-1);
									audio3=channels.GetValueAsInt(i.ToString(),"DVBTAudio3Pid",-1);
									ac3Pid=channels.GetValueAsInt(i.ToString(),"DVBTAC3Pid",-1);
									audioLanguage=channels.GetValueAsString(i.ToString(),"DVBTAudioLanguage","");
									audioLanguage1=channels.GetValueAsString(i.ToString(),"DVBTAudioLanguage1","");
									audioLanguage2=channels.GetValueAsString(i.ToString(),"DVBTAudioLanguage2","");
									audioLanguage3=channels.GetValueAsString(i.ToString(),"DVBTAudioLanguage3","");
									if (ONID>0 && TSID>0 && SID > 0 && freq>0)
									{
										TVDatabase.MapDVBTChannel(Import_Chan.Name,provider,Import_Chan.ID,freq,ONID,TSID,SID, audioPid,videoPid,teletextPid, pmtPid,bandWidth,audio1,audio2,audio3, ac3Pid,audioLanguage,audioLanguage1, audioLanguage2, audioLanguage3);
									}
								}
								catch(Exception)
								{
									MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-T data.");
								}

								//dvb-C
								try
								{
									freq=channels.GetValueAsInt(i.ToString(),"DVBCFreq",0);
									ONID=channels.GetValueAsInt(i.ToString(),"DVBCONID",0);
									TSID=channels.GetValueAsInt(i.ToString(),"DVBCTSID",0);
									SID=channels.GetValueAsInt(i.ToString(),"DVBCSID",0);
									symbolrate=channels.GetValueAsInt(i.ToString(),"DVBCSR",0);
									innerFec=channels.GetValueAsInt(i.ToString(),"DVBCInnerFeq",0);
									modulation=channels.GetValueAsInt(i.ToString(),"DVBCModulation",0);
									provider=channels.GetValueAsString(i.ToString(),"DVBCProvider","");
									audioPid=channels.GetValueAsInt(i.ToString(),"DVBCAudioPid",0);
									videoPid=channels.GetValueAsInt(i.ToString(),"DVBCVideoPid",0);
									teletextPid=channels.GetValueAsInt(i.ToString(),"DVBCTeletextPid",0);
									pmtPid=channels.GetValueAsInt(i.ToString(),"DVBCPmtPid",0);
									audio1=channels.GetValueAsInt(i.ToString(),"DVBCAudio1Pid",-1);
									audio2=channels.GetValueAsInt(i.ToString(),"DVBCAudio2Pid",-1);
									audio3=channels.GetValueAsInt(i.ToString(),"DVBCAudio3Pid",-1);
									ac3Pid=channels.GetValueAsInt(i.ToString(),"DVBCAC3Pid",-1);
									audioLanguage=channels.GetValueAsString(i.ToString(),"DVBCAudioLanguage","");
									audioLanguage1=channels.GetValueAsString(i.ToString(),"DVBCAudioLanguage1","");
									audioLanguage2=channels.GetValueAsString(i.ToString(),"DVBCAudioLanguage2","");
									audioLanguage3=channels.GetValueAsString(i.ToString(),"DVBCAudioLanguage3","");
									if (ONID>0 && TSID>0 && SID > 0 && freq>0)
									{
										TVDatabase.MapDVBCChannel(Import_Chan.Name,provider,Import_Chan.ID,freq,symbolrate,innerFec,modulation,ONID,TSID,SID, audioPid,videoPid,teletextPid, pmtPid,audio1,audio2,audio3,ac3Pid,audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3);
									}
								}
								catch(Exception)
								{
									MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-C data.");
								}
								
								//dvb-S
								try
								{
									DVBChannel ch = new DVBChannel();
									TVDatabase.GetSatChannel(Import_Chan.ID,1,ref ch);

									freq=channels.GetValueAsInt(i.ToString(),"DVBSFreq",0);
									ONID=channels.GetValueAsInt(i.ToString(),"DVBSONID",0);
									TSID=channels.GetValueAsInt(i.ToString(),"DVBSTSID",0);
									SID=channels.GetValueAsInt(i.ToString(),"DVBSSID",0);
									symbolrate=channels.GetValueAsInt(i.ToString(),"DVBSSymbolrate",0);
									innerFec=channels.GetValueAsInt(i.ToString(),"DvbSInnerFec",0);
									polarisation=channels.GetValueAsInt(i.ToString(),"DVBSPolarisation",0);
									provider=channels.GetValueAsString(i.ToString(),"DVBSProvider","");
									audioPid=channels.GetValueAsInt(i.ToString(),"DVBSAudioPid",0);
									videoPid=channels.GetValueAsInt(i.ToString(),"DVBSVideoPid",0);
									teletextPid=channels.GetValueAsInt(i.ToString(),"DVBSTeletextPid",0);
									pmtPid=channels.GetValueAsInt(i.ToString(),"DVBSPmtPid",0);
									audio1=channels.GetValueAsInt(i.ToString(),"DVBSAudio1Pid",-1);
									audio2=channels.GetValueAsInt(i.ToString(),"DVBSAudio2Pid",-1);
									audio3=channels.GetValueAsInt(i.ToString(),"DVBSAudio3Pid",-1);
									ac3Pid=channels.GetValueAsInt(i.ToString(),"DVBSAC3Pid",-1);
									audioLanguage=channels.GetValueAsString(i.ToString(),"DVBSAudioLanguage","");
									audioLanguage1=channels.GetValueAsString(i.ToString(),"DVBSAudioLanguage1","");
									audioLanguage2=channels.GetValueAsString(i.ToString(),"DVBSAudioLanguage2","");
									audioLanguage3=channels.GetValueAsString(i.ToString(),"DVBSAudioLanguage3","");
									if (ONID>0 && TSID>0 && SID > 0 && freq>0)
									{
										ch.ServiceType=1;
										ch.Frequency=freq;
										ch.NetworkID=ONID;
										ch.TransportStreamID=TSID;
										ch.ProgramNumber=SID;
										ch.Symbolrate=symbolrate;
										ch.FEC=innerFec;
										ch.Polarity=polarisation;
										ch.ServiceProvider=provider;
										ch.ServiceName=Import_Chan.Name;
										ch.ID=Import_Chan.ID;
										ch.AudioPid=audioPid;
										ch.VideoPid=videoPid;
										ch.TeletextPid=teletextPid;
										ch.ECMPid = channels.GetValueAsInt(i.ToString(),"DVBSECMpid",0);
										ch.Audio1=audio1;
										ch.Audio2=audio2;
										ch.Audio3=audio3;
										ch.AudioLanguage=audioLanguage;
										ch.AudioLanguage1=audioLanguage1;
										ch.AudioLanguage2=audioLanguage2;
										ch.AudioLanguage3=audioLanguage3;
										TVDatabase.UpdateSatChannel(ch);
									}
								}
								catch(Exception)
								{
									MessageBox.Show("OOPS! Something odd happened.\nCouldn't import DVB-S data.");
								}
							}
						}
					}
					else if(overwrite==-1)
					{
						//Go to the next channel do nothing
					}
				}
				SaveTVChannels();
				SaveSettings();
				UpdateGroupChannels(null,true);


				//Grab group Data and channel maping
				int group_index=0,channel_index=0;
				
				//Grab total groups for reference
				group_index=channels.GetValueAsInt("GROUPS","TOTAL",-1);
				//Check if we have any groups
				if(group_index==-1||group_index==0)
				{
					MessageBox.Show("There are no groups to import");
				}
				else
				{
					MessageBox.Show("There is a total of "+group_index.ToString()+" groups to import");
				
					for(int i = 0;i<group_index;i++)
					{
						int overwrite =0;
						int overwrite_index= 0;	
						TVGroup Import_Group = new TVGroup();
						Import_Group.ID=channels.GetValueAsInt("Group "+i.ToString(),"ID",0);
						Import_Group.GroupName=channels.GetValueAsString("Group "+i.ToString(),"NAME","");
						Import_Group.Pincode=channels.GetValueAsInt("Group "+i.ToString(),"PINCODE",0);
						Import_Group.Sort=channels.GetValueAsInt("Group "+i.ToString(),"SORT",0);
					
						//If there are existing groups and are the same prompt for overwrite
						foreach (ListViewItem listItem in listViewGroups.Items)
						{
							TVGroup group = listItem.Tag as TVGroup;
							//if(group.GroupName==Import_Group.GroupName)
							if(group.ID==Import_Group.ID)
							{
								if(MessageBox.Show(Import_Group.GroupName+" Already exists.\nWould you like to overwrite?","Warning!",MessageBoxButtons.YesNo)==DialogResult.Yes)
								{
									overwrite = 1;
									overwrite_index=listItem.Index;
								}
								else 
								{
									overwrite = -1;
								}
								break;
							}
						}

						if(overwrite==0||overwrite==1)
						{
							if(overwrite==1)
							{
								listViewGroups.Items[overwrite_index].Tag = Import_Group;
									
								listViewGroups.Items[overwrite_index].SubItems[0].Text = Import_Group.GroupName;
								listViewGroups.Items[overwrite_index].SubItems[1].Text = Import_Group.Pincode>0 ? "Yes" : "No";
							
								//Add Group to database
								TVDatabase.AddGroup(Import_Group);

								UpdateGroupChannels(Import_Group,true);
							}
							else 
							{
								string pincode="No";
								if (Import_Group.Pincode!=0)
									pincode="Yes";
								ListViewItem listItem = new ListViewItem(new string[] { Import_Group.GroupName, pincode,} );
								listItem.Tag = Import_Group;

								listViewGroups.Items.Add(listItem);

								//Add Group to database
								TVDatabase.AddGroup(Import_Group);
							
								UpdateGroupChannels((TVGroup)listItem.Tag,true);
							}

							//Check if required to do anything specific for compatibility reasons
							switch(VER)
							{
									//Do stuff for backward compatibility if needed
								case 0:

									break;
									//Do stuff for current version only
								case 1:

									break;
									//Do stuff for forward compatibility if needed
								case 2:

									break;
							}
						
							//This is done for every version regardless
							if(VER==0||VER==1||VER==2)
							{
								//Add channels to this group
								ArrayList Group_Channels = new ArrayList();
								TVDatabase.GetChannels(ref Group_Channels);
								channel_index =channels.GetValueAsInt("Group "+i.ToString(),"TOTAL CHANNELS",0);
								if(channel_index>0)
								{	
								
									for(int j = 0; j<channel_index;j++)
									{
										int tmpID = channels.GetValueAsInt("Group "+i.ToString(),"CHANNEL "+j.ToString(),0);
								
										//Locate Channel so it can be added to group
										foreach(TVChannel FindChan in Group_Channels)
										{
											if(FindChan.ID==tmpID)
											{
												//Add channel to group
												Import_Group.tvChannels.Add(FindChan);	
											
												//Have to re-grab group from database in order to map correctly :|
												ArrayList GrabGroup = new ArrayList();
												TVDatabase.GetGroups(ref GrabGroup);
											
												TVDatabase.MapChannelToGroup((TVGroup)GrabGroup[i], FindChan);
											}
										}
									}
								}
								else
								{
									//Add Group to database
									//TVDatabase.AddGroup(Import_Group);
								}
							}	
						}
						else if(overwrite==-1)
						{
							//Go to the next group do nothing
						}
					}	
					SaveSettings();
					UpdateGroupChannels(null,true);
				}

				//Grab Saved Card mapping
				
				//Check if we have cards first
				ArrayList Cards = new ArrayList();
				TVDatabase.GetCards(ref Cards);
				if(Cards.Count==0)
				{
					MessageBox.Show("There are no cards configured.\nCannot map channels without at least one card.");
					return;
				}
				else
				{
					int cards_index=0;
					channel_index=0;
				
					//Grab total cards for reference
					cards_index=channels.GetValueAsInt("CARDS","TOTAL",-1);
				
					//Check if we have any cards
					if(cards_index==-1||cards_index==0)
					{
						MessageBox.Show("No card related data was saved for this listing");
						return;
					}
					
					if(cards_index>Cards.Count)
					{
						MessageBox.Show("There is a total of "+cards_index.ToString()+" card(s) channel mappings to import\nHowever I can only import "+Cards.Count.ToString());
						cards_index=Cards.Count;
					}
					else 
					{
						MessageBox.Show("There is a total of "+cards_index.ToString()+" card(s) channel mappings to import");
					}
					for(int i=1;i<cards_index+1;i++)
					{
						//Check if required to do anything specific for compatibility reasons
						switch(VER)
						{
								//Do stuff for backward compatibility if needed
							case 0:

								break;
								//Do stuff for current version only
							case 1:

								break;
								//Do stuff for forward compatibility if needed
							case 2:

								break;
						}
						
						//This is done for every version regardless
						if(VER==0||VER==1||VER==2)
						{
							//Re-Map channels to available cards
							ArrayList Card_Channels = new ArrayList();
							TVDatabase.GetChannels(ref Card_Channels);
							channel_index=channels.GetValueAsInt("Card "+i.ToString(),"TOTAL CHANNELS",0);	
							
							if(channel_index>0)
							{
								for(int j=0;j<channel_index;j++)
								{
									int tmpID = channels.GetValueAsInt("Card "+i.ToString(),"CHANNEL "+j.ToString(),0);
								
									//Locate Channel so it can be added to Card
									foreach(TVChannel FindChan in Card_Channels)
									{
										if(FindChan.ID==tmpID)
										{
											//Map it
											TVDatabase.MapChannelToCard(FindChan.ID,i);
										}
									}
								}
							}
						}
					}
				}

				//Grab recorded show information
				int recorded_count=0;
				
				//Grab recorded shows saved for referrence
				recorded_count = channels.GetValueAsInt("RECORDED","TOTAL",-1);
				if(recorded_count==-1||recorded_count==0)
				{
					MessageBox.Show("There is no Recorded TV data to import");
				}
				else
				{
					MessageBox.Show("There is a total of "+recorded_count.ToString()+" recorded items to import");
					
					//Check if required to do anything specific for compatibility reasons
					switch(VER)
					{
							//Do stuff for backward compatibility if needed
						case 0:

							break;
							//Do stuff for current version only
						case 1:

							break;
							//Do stuff for forward compatibility if needed
						case 2:

							break;
					}
						
					//This is done for every version regardless
					if(VER==0||VER==1||VER==2)
					{
						for(int i=1;i<recorded_count+1;i++)
						{
							//Create temp TVRecorded to hold data to import
							TVRecorded temp_recorded = new TVRecorded();
							temp_recorded.ID=channels.GetValueAsInt("Recorded "+i.ToString(),"ID",0);
							temp_recorded.Title=channels.GetValueAsString("Recorded "+i.ToString(),"TITLE","");
							temp_recorded.Channel=channels.GetValueAsString("Recorded "+i.ToString(),"CHANNEL","");
							temp_recorded.Description=channels.GetValueAsString("Recorded "+i.ToString(),"DESC","");
							temp_recorded.Genre=channels.GetValueAsString("Recorded "+i.ToString(),"GENRE","");
							temp_recorded.FileName=channels.GetValueAsString("Recorded "+i.ToString(),"FILENAME","");
							temp_recorded.Start=Convert.ToInt64(channels.GetValueAsString("Recorded "+i.ToString(),"STARTTIME","0"));
							temp_recorded.End=Convert.ToInt64(channels.GetValueAsString("Recorded "+i.ToString(),"ENDTIME","0"));
							temp_recorded.Played=channels.GetValueAsInt("Recorded "+i.ToString(),"PLAYED",0);
							
							//Add or gathered info to the TVDatabase
							bool recorded_overwrite = false;
							ArrayList check_recorded_list = new ArrayList();
							TVDatabase.GetRecordedTV(ref check_recorded_list);
							TVRecorded check_recorded = new TVRecorded();
							foreach(TVRecorded check_me in check_recorded_list)
							{
								if(check_me.ID==temp_recorded.ID&&check_me.Start.ToString()==temp_recorded.Start.ToString())
								{
									check_recorded = check_me;
									recorded_overwrite = true;
									break;
								}
							}
							if(recorded_overwrite)
							{
								//Ask if user if overwrite ok
								if(MessageBox.Show("Would you like to overwrite the entry for "+check_recorded.Title+" - Start Time: "+check_recorded.Start.ToString()+"\nWith the entry "+temp_recorded.Title+" - Start Time: "+temp_recorded.Start.ToString()+"\nProceed with overwrite?",
								"Overwrite?",MessageBoxButtons.YesNo)==DialogResult.Yes)
								{
									//Check if this file exists first, if not ask user to locate it or no update
									if(File.Exists(temp_recorded.FileName))
									{
										TVDatabase.RemoveRecordedTV(check_recorded);
										TVDatabase.AddRecordedTV(temp_recorded);
									}
									else
									{
										if(MessageBox.Show("Could not find the file for "+temp_recorded.Title+"\nDo you want to find the file? (No will not add this recorded entry)","Cannot Find File..",MessageBoxButtons.YesNo)==DialogResult.Yes)
										{
											bool quit=false;
											//Build open dialog for user to find file
											System.Windows.Forms.OpenFileDialog find_file = new OpenFileDialog();
                      find_file.RestoreDirectory = true;
											find_file.DefaultExt = "dvr-ms";
											find_file.Filter = "dvr-ms|*.dvr-ms";
											find_file.InitialDirectory = ".";
											find_file.Title= "Find Recorded File for "+ temp_recorded.Title;
											while(!quit)
											{
												if(find_file.ShowDialog(this)==DialogResult.OK)
												{
													temp_recorded.FileName=find_file.FileName;
													//Add the recorded data to database
													TVDatabase.RemoveRecordedTV(check_recorded);
													TVDatabase.AddRecordedTV(temp_recorded);
												}
												else
												{
													if(MessageBox.Show("Are you positive you don't want to add:\n"+temp_recorded.Title,"Are you sure?",MessageBoxButtons.YesNo)==DialogResult.Yes)
													{
														quit=true;
													}
													else quit=false;
												}
											}
										}
									}
								}
							}
							else
							{
								//Check if this file exists first, if not ask user to locate it or no update
								if(File.Exists(temp_recorded.FileName))
								{
									TVDatabase.AddRecordedTV(temp_recorded);
								}
								else
								{
									if(MessageBox.Show("Could not find the file for "+temp_recorded.Title+"\nDo you want to find the file? (No will not add this recorded entry)","Cannot Find File..",MessageBoxButtons.YesNo)==DialogResult.Yes)
									{
										bool quit=false;
										//Build open dialog for user to find file
										System.Windows.Forms.OpenFileDialog find_file = new OpenFileDialog();
                    find_file.RestoreDirectory = true;
										find_file.DefaultExt = "dvr-ms";
										find_file.Filter = "dvr-ms|*.dvr-ms";
										find_file.InitialDirectory = ".";
										find_file.Title= "Find Recorded File for "+ temp_recorded.Title;
										while(!quit)
										{
											if(find_file.ShowDialog(this)==DialogResult.OK)
											{
												temp_recorded.FileName=find_file.FileName;
												//Add the recorded data to database
												TVDatabase.AddRecordedTV(temp_recorded);
											}
											else
											{
												if(MessageBox.Show("Are you positive you don't want to add:\n"+temp_recorded.Title,"Are you sure?",MessageBoxButtons.YesNo)==DialogResult.Yes)
												{
													quit=true;
												}
												else quit=false;
											}
										}
									}
								}
							}
						}	
					}
					
				}
				

				//Grab recording shows information
				int recordings_count=0;
				
				//Grab recorded shows saved for referrence
				recordings_count = channels.GetValueAsInt("RECORDINGS","TOTAL",-1);
				
				if(recordings_count==-1||recordings_count==0)
				{
					MessageBox.Show("There is no Recording TV data to import");
				}
				else
				{
					MessageBox.Show("There is "+recordings_count.ToString()+" Recording TV data items to import");
					
					//Check if required to do anything specific for compatibility reasons
					switch(VER)
					{
							//Do stuff for backward compatibility if needed
						case 0:

							break;
							//Do stuff for current version only
						case 1:

							break;
							//Do stuff for forward compatibility if needed
						case 2:

							break;
					}
						
					//This is done for every version regardless
					if(VER==0||VER==1||VER==2)
					{
						for(int i=1;i<recordings_count+1;i++)
						{
							//Create temp TVRecording to hold data to import
							MediaPortal.TV.Database.TVRecording temp_recording= new MediaPortal.TV.Database.TVRecording();
							temp_recording.ID=channels.GetValueAsInt("Recording "+i.ToString(),"ID",0);
							temp_recording.Title=channels.GetValueAsString("Recording "+i.ToString(),"TITLE","");
							temp_recording.Channel=channels.GetValueAsString("Recording "+i.ToString(),"CHANNEL","");
							temp_recording.Start=Convert.ToInt64(channels.GetValueAsString("Recording "+i.ToString(),"STARTTIME","0"));
							temp_recording.End=Convert.ToInt64(channels.GetValueAsString("Recording "+i.ToString(),"ENDTIME","0"));
							temp_recording.Canceled=Convert.ToInt64(channels.GetValueAsString("Recording "+i.ToString(),"CANCELEDTIME","0"));
							temp_recording.RecType=Convert_RecordingType(channels.GetValueAsString("Recording "+i.ToString(),"TYPE",""));
							temp_recording.Priority=channels.GetValueAsInt("Recording "+i.ToString(),"PRIORITY",0);
							temp_recording.Quality=Convert_QualityType(channels.GetValueAsString("Recording "+i.ToString(),"QUALITY",""));
							//temp_recording.Status=(MediaPortal.TV.Database.TVRecording.RecordingStatus)channels.GetValue("Recording "+i.ToString(),"STATUS");
							temp_recording.IsContentRecording=channels.GetValueAsBool("Recording "+i.ToString(),"ISCONTENTREC",false);
							temp_recording.Series=channels.GetValueAsBool("Recording "+i.ToString(),"SERIES",false);
							temp_recording.EpisodesToKeep=channels.GetValueAsInt("Recording "+i.ToString(),"EPISODES",Int32.MaxValue);
							
							//Add this recording to TVDatabase
							bool recording_overwrite = false;
							ArrayList check_recording_list = new ArrayList();
							TVDatabase.GetRecordings(ref check_recording_list);
							MediaPortal.TV.Database.TVRecording check_recording = new MediaPortal.TV.Database.TVRecording();
							foreach(MediaPortal.TV.Database.TVRecording check_me in check_recording_list)
							{
								if(check_me.ID==temp_recording.ID&&check_me.Start.ToString()==temp_recording.Start.ToString())
								{
									check_recording = check_me;
									recording_overwrite = true;
									break;
								}
							}
							if(recording_overwrite)
							{
								//Ask if user if overwrite ok
								if(MessageBox.Show("Would you like to overwrite the entry for "+check_recording.Title+" - Start Time: "+check_recording.Start.ToString()+"\nWith the entry "+temp_recording.Title+" - Start Time: "+temp_recording.Start.ToString()+"\nProceed with overwrite?",
									"Overwrite?",MessageBoxButtons.YesNo)==DialogResult.Yes)
								{
									//Delete Canceled series information
									foreach(long del_canx in check_recording.CanceledSeries)
									{
										TVDatabase.DeleteCanceledSeries(check_recording);
									}
									//Check if this recording has had any cancels
									int canx_count = 0;
									canx_count=channels.GetValueAsInt("Recording "+i.ToString(),"CANCELED SERIES TOTAL",0);
									if(canx_count>0)
									{	
										temp_recording.CanceledSeries.Clear();
										long last_canx_time=0;
										for(int j=0;j<canx_count;j++)
										{
											//Add the canceled time to TVDatabase
											long canx_time=0;
											canx_time=Convert.ToInt64(channels.GetValueAsString("Recording "+i.ToString(),"CANCELED SERIES CANCELEDTIME "+j.ToString(),"0"));
											//Check if we had the same time from before if so stop adding
											if(canx_time==last_canx_time)break;
											//TVDatabase.AddCanceledSerie(temp_recording,canx_time);
											temp_recording.CanceledSeries.Add((long)canx_time);
											last_canx_time=canx_time;
										}
									}
									//Delete old entry
									TVDatabase.RemoveRecording(check_recording);
									//Add new overwrite entry
									TVDatabase.AddRecording(ref temp_recording);
								}
							}
							else
							{
								//Check if this recording has had any cancels
								int canx_count = 0;
								canx_count=channels.GetValueAsInt("Recording "+i.ToString(),"CANCELED SERIES TOTAL",0);
								if(canx_count>0)
								{	
									temp_recording.CanceledSeries.Clear();
									long last_canx_time=0;
									for(int j=0;j<canx_count;j++)
									{
										//Add the canceled time to TVDatabase
										long canx_time=0;
										canx_time=Convert.ToInt64(channels.GetValueAsString("Recording "+i.ToString(),"CANCELED SERIES CANCELEDTIME "+j.ToString(),"0"));
										//Check if we had the same time from before if so stop adding
										if(canx_time==last_canx_time)break;
										//TVDatabase.AddCanceledSerie(temp_recording,canx_time);
										temp_recording.CanceledSeries.Add((long)canx_time);
										last_canx_time=canx_time;
									}
								}
								//Add new entry
								TVDatabase.AddRecording(ref temp_recording);
							}
						}
					}		
				}
			}
		}

		private AnalogVideoStandard Convert_AVS(object avs)
		{
			if ((string)avs=="None") return AnalogVideoStandard.None;
			if ((string)avs=="NTSC_M") return AnalogVideoStandard.NTSC_M;
			if ((string)avs=="NTSC_M_J") return AnalogVideoStandard.NTSC_M_J;
			if ((string)avs=="NTSC_433") return AnalogVideoStandard.NTSC_433;
			if ((string)avs=="PAL_B") return AnalogVideoStandard.PAL_B;
			if ((string)avs=="PAL_D") return AnalogVideoStandard.PAL_D;
			if ((string)avs=="PAL_G") return AnalogVideoStandard.PAL_G;
			if ((string)avs=="PAL_H") return AnalogVideoStandard.PAL_H;
			if ((string)avs=="PAL_I") return AnalogVideoStandard.PAL_I;
			if ((string)avs=="PAL_M") return AnalogVideoStandard.PAL_M;
			if ((string)avs=="PAL_N") return AnalogVideoStandard.PAL_N;
			if ((string)avs=="PAL_60") return AnalogVideoStandard.PAL_60;
			if ((string)avs=="SECAM_B") return AnalogVideoStandard.SECAM_B;
			if ((string)avs=="SECAM_D") return AnalogVideoStandard.SECAM_D;
			if ((string)avs=="SECAM_G") return AnalogVideoStandard.SECAM_G;
			if ((string)avs=="SECAM_H") return AnalogVideoStandard.SECAM_H;
			if ((string)avs=="SECAM_K") return AnalogVideoStandard.SECAM_K;
			if ((string)avs=="SECAM_K1") return AnalogVideoStandard.SECAM_K1;
			if ((string)avs=="SECAM_L") return AnalogVideoStandard.SECAM_L;
			if ((string)avs=="SECAM_L1") return AnalogVideoStandard.SECAM_L1;
			if ((string)avs=="PAL_N_COMBO") return AnalogVideoStandard.PAL_N_COMBO;
			
			//If nothing return Default
			return AnalogVideoStandard.None;
		}

		private TV.Database.TVRecording.QualityType Convert_QualityType(object quality)
		{
			if ((string)quality=="NotSet") return TV.Database.TVRecording.QualityType.NotSet;
			if ((string)quality=="Portable") return TV.Database.TVRecording.QualityType.Portable;
			if ((string)quality=="Low") return TV.Database.TVRecording.QualityType.Low;
			if ((string)quality=="Medium") return TV.Database.TVRecording.QualityType.Medium;
			if ((string)quality=="High") return TV.Database.TVRecording.QualityType.High;
			
			//If nothing return Default
			return TV.Database.TVRecording.QualityType.NotSet;
		}

		private TV.Database.TVRecording.RecordingType Convert_RecordingType(object recType)
		{
			if ((string)recType=="Once") return TV.Database.TVRecording.RecordingType.Once;
			if ((string)recType=="EveryTimeOnThisChannel") return TV.Database.TVRecording.RecordingType.EveryTimeOnThisChannel;
			if ((string)recType=="EveryTimeOnEveryChannel") return TV.Database.TVRecording.RecordingType.EveryTimeOnEveryChannel;
			if ((string)recType=="Daily") return TV.Database.TVRecording.RecordingType.Daily;
			if ((string)recType=="Weekly") return TV.Database.TVRecording.RecordingType.Weekly;
			if ((string)recType=="WeekDays") return TV.Database.TVRecording.RecordingType.WeekDays;
      
			//If nothing return Default
			return TV.Database.TVRecording.RecordingType.Once;
		}
		private void xmlExport_Click_1(object sender, System.EventArgs e)
		{

      XMLSaveDialog.RestoreDirectory = true;
			if(XMLSaveDialog.ShowDialog(this)==DialogResult.OK)
			{
				Export_to_XML(XMLSaveDialog.FileName.ToString());
			}
		}

		private void xmlImport_Click_1(object sender, System.EventArgs e)
		{
      XMLOpenDialog.RestoreDirectory = true;
			if(XMLOpenDialog.ShowDialog(this)==DialogResult.OK)
			{
				Import_From_XML(XMLOpenDialog.FileName.ToString());
			}
		}

		private void buttonLookup_Click(object sender, System.EventArgs e)
		{
			TvChannelLookupService dlg = new TvChannelLookupService();
			dlg.ShowDialog(this);
			reloadList=true;
			RadioStations.UpdateList();
		}

		private void TVChannels_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}

