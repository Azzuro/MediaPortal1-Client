using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using DShowNET;
using DirectX.Capture;
using MediaPortal.Player;
using SQLite.NET;
using MediaPortal.GUI.Library;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.Configuration.Sections
{
	public class RadioStations : MediaPortal.Configuration.SectionSettings
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
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button addButton;
		private MediaPortal.UserInterface.Controls.MPListView stationsListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button upButton;
    private System.Windows.Forms.Button downButton;

		//
		// Private members
		//
		//bool isDirty = false;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button btnMapChannelToCard;
		private System.Windows.Forms.Button btnUnmapChannelFromCard;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox comboBoxCard;
		private System.Windows.Forms.ListView listviewCardChannels;
		private System.Windows.Forms.ListView listViewRadioChannels;
    ListViewItem currentlyCheckedItem = null;

		public RadioStations() : this("Stations")
		{
		}

		public RadioStations(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
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

		public override void OnSectionActivated()
		{


		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.upButton = new System.Windows.Forms.Button();
			this.downButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.editButton = new System.Windows.Forms.Button();
			this.addButton = new System.Windows.Forms.Button();
			this.stationsListView = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.btnMapChannelToCard = new System.Windows.Forms.Button();
			this.btnUnmapChannelFromCard = new System.Windows.Forms.Button();
			this.listviewCardChannels = new System.Windows.Forms.ListView();
			this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
			this.listViewRadioChannels = new System.Windows.Forms.ListView();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.comboBoxCard = new System.Windows.Forms.ComboBox();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Frequency";
			this.columnHeader3.Width = 54;
			// 
			// upButton
			// 
			this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.upButton.Enabled = false;
			this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.upButton.Location = new System.Drawing.Point(280, 304);
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(48, 23);
			this.upButton.TabIndex = 5;
			this.upButton.Text = "Up";
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.downButton.Enabled = false;
			this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.downButton.Location = new System.Drawing.Point(344, 304);
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(48, 23);
			this.downButton.TabIndex = 6;
			this.downButton.Text = "Down";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(192, 304);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.TabIndex = 3;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// editButton
			// 
			this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editButton.Enabled = false;
			this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editButton.Location = new System.Drawing.Point(112, 304);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 2;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(32, 304);
			this.addButton.Name = "addButton";
			this.addButton.TabIndex = 1;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// stationsListView
			// 
			this.stationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.stationsListView.CheckBoxes = true;
			this.stationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																											 this.columnHeader1,
																																											 this.columnHeader2,
																																											 this.columnHeader3,
																																											 this.columnHeader4,
																																											 this.columnHeader5,
																																											 this.columnHeader6});
			this.stationsListView.FullRowSelect = true;
			this.stationsListView.HideSelection = false;
			this.stationsListView.Location = new System.Drawing.Point(24, 8);
			this.stationsListView.Name = "stationsListView";
			this.stationsListView.Size = new System.Drawing.Size(432, 288);
			this.stationsListView.TabIndex = 0;
			this.stationsListView.View = System.Windows.Forms.View.Details;
			this.stationsListView.DoubleClick += new System.EventHandler(this.stationsListView_DoubleClick);
			this.stationsListView.SelectedIndexChanged += new System.EventHandler(this.stationsListView_SelectedIndexChanged);
			this.stationsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.stationsListView_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Type";
			this.columnHeader1.Width = 65;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Station name";
			this.columnHeader2.Width = 117;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Genre";
			this.columnHeader4.Width = 72;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Bitrate";
			this.columnHeader5.Width = 42;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Server";
			this.columnHeader6.Width = 94;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(8, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(472, 392);
			this.tabControl1.TabIndex = 1;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.stationsListView);
			this.tabPage1.Controls.Add(this.deleteButton);
			this.tabPage1.Controls.Add(this.editButton);
			this.tabPage1.Controls.Add(this.addButton);
			this.tabPage1.Controls.Add(this.upButton);
			this.tabPage1.Controls.Add(this.downButton);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(464, 366);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Stations";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.btnMapChannelToCard);
			this.tabPage2.Controls.Add(this.btnUnmapChannelFromCard);
			this.tabPage2.Controls.Add(this.listviewCardChannels);
			this.tabPage2.Controls.Add(this.listViewRadioChannels);
			this.tabPage2.Controls.Add(this.label4);
			this.tabPage2.Controls.Add(this.label5);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.comboBoxCard);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(464, 366);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Cards";
			// 
			// btnMapChannelToCard
			// 
			this.btnMapChannelToCard.Location = new System.Drawing.Point(216, 155);
			this.btnMapChannelToCard.Name = "btnMapChannelToCard";
			this.btnMapChannelToCard.Size = new System.Drawing.Size(32, 23);
			this.btnMapChannelToCard.TabIndex = 23;
			this.btnMapChannelToCard.Text = ">>";
			this.btnMapChannelToCard.Click += new System.EventHandler(this.btnMapChannelToCard_Click);
			// 
			// btnUnmapChannelFromCard
			// 
			this.btnUnmapChannelFromCard.Location = new System.Drawing.Point(216, 187);
			this.btnUnmapChannelFromCard.Name = "btnUnmapChannelFromCard";
			this.btnUnmapChannelFromCard.Size = new System.Drawing.Size(32, 23);
			this.btnUnmapChannelFromCard.TabIndex = 22;
			this.btnUnmapChannelFromCard.Text = "<<";
			this.btnUnmapChannelFromCard.Click += new System.EventHandler(this.btnUnmapChannelFromCard_Click);
			// 
			// listviewCardChannels
			// 
			this.listviewCardChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																													 this.columnHeader10});
			this.listviewCardChannels.Location = new System.Drawing.Point(260, 67);
			this.listviewCardChannels.Name = "listviewCardChannels";
			this.listviewCardChannels.Size = new System.Drawing.Size(168, 277);
			this.listviewCardChannels.TabIndex = 21;
			this.listviewCardChannels.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Radio Station";
			this.columnHeader10.Width = 161;
			// 
			// listViewRadioChannels
			// 
			this.listViewRadioChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																														this.columnHeader11});
			this.listViewRadioChannels.Location = new System.Drawing.Point(36, 67);
			this.listViewRadioChannels.Name = "listViewRadioChannels";
			this.listViewRadioChannels.Size = new System.Drawing.Size(168, 277);
			this.listViewRadioChannels.TabIndex = 20;
			this.listViewRadioChannels.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Radio station";
			this.columnHeader11.Width = 159;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(264, 43);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(156, 16);
			this.label4.TabIndex = 19;
			this.label4.Text = "Stations assigned to card";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(40, 43);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 16);
			this.label5.TabIndex = 18;
			this.label5.Text = "Available Stations";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(36, 19);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(36, 16);
			this.label6.TabIndex = 17;
			this.label6.Text = "Card:";
			// 
			// comboBoxCard
			// 
			this.comboBoxCard.Location = new System.Drawing.Point(80, 11);
			this.comboBoxCard.Name = "comboBoxCard";
			this.comboBoxCard.Size = new System.Drawing.Size(280, 21);
			this.comboBoxCard.TabIndex = 16;
			this.comboBoxCard.SelectedIndexChanged += new System.EventHandler(this.comboBoxCard_SelectedIndexChanged);
			// 
			// RadioStations
			// 
			this.Controls.Add(this.tabControl1);
			this.Name = "RadioStations";
			this.Size = new System.Drawing.Size(488, 432);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void addButton_Click(object sender, EventArgs e)
		{
//			isDirty = true;

			RadioStation newStation = new RadioStation();
			newStation.Type="Radio";
			newStation.Frequency = new Frequency(0);
			EditRadioStationForm editStation = new EditRadioStationForm();
			editStation.Station =newStation ;

			DialogResult dialogResult = editStation.ShowDialog(this);

			if(dialogResult == DialogResult.OK)
			{

				MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
				station.Scrambled= false;
				station.Name	= editStation.Station.Name;
				station.Genre	= editStation.Station.Genre;
				station.BitRate	= editStation.Station.Bitrate;
				station.URL		= editStation.Station.URL;
				station.Frequency= editStation.Station.Frequency.Herz;
				if(station.Frequency < 1000)
					station.Frequency *= 1000000L;

				ListViewItem listItem = new ListViewItem(new string[] { editStation.Station.Type, 
																																editStation.Station.Name,
																																editStation.Station.Frequency.ToString(Frequency.Format.MegaHerz),
																																editStation.Station.Genre, 
																																editStation.Station.Bitrate.ToString(),
																																editStation.Station.URL 
																															} );
				

				listItem.Tag = editStation.Station;
				stationsListView.Items.Add(listItem);
				station.Channel = listItem.Index;
				editStation.Station.ID=RadioDatabase.AddStation(ref station);
			}
		}

		private void editButton_Click(object sender, System.EventArgs e)
		{
//			isDirty = true;

			foreach(ListViewItem listItem in stationsListView.SelectedItems)
			{
				EditRadioStationForm editStation = new EditRadioStationForm();
				editStation.Station = listItem.Tag as RadioStation;

				DialogResult dialogResult = editStation.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					listItem.Tag = editStation.Station;

					//
					// Remove URL if we have a normal radio station
					//
					if(editStation.Station.Type.Equals("Radio"))
						editStation.Station.URL = String.Empty;

					listItem.SubItems[0].Text = editStation.Station.Type;
					listItem.SubItems[1].Text = editStation.Station.Name;
					listItem.SubItems[2].Text = editStation.Station.Frequency.ToString(Frequency.Format.MegaHerz);
					listItem.SubItems[3].Text = editStation.Station.Genre;
					listItem.SubItems[4].Text = editStation.Station.Bitrate.ToString();
					listItem.SubItems[5].Text = editStation.Station.URL;

					MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
					station.Scrambled= editStation.Station.Scrambled;
					station.ID= editStation.Station.ID;
					station.Name	= editStation.Station.Name;
					station.Genre	= editStation.Station.Genre;
					station.BitRate	= editStation.Station.Bitrate;
					station.URL		= editStation.Station.URL;
					station.Frequency= editStation.Station.Frequency.Herz;
					if(station.Frequency < 1000)
						station.Frequency *= 1000000L;
					station.Channel = listItem.Index;
					RadioDatabase.UpdateStation(station);
				}
				}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
//			isDirty = true;

			int itemCount = stationsListView.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				RadioStation station = stationsListView.SelectedItems[0].Tag as RadioStation;
				RadioDatabase.RemoveStation(station.Name);
				stationsListView.Items.RemoveAt(stationsListView.SelectedIndices[0]);
			}
		}



		public override void LoadSettings()
		{
			LoadRadioStations();
		}

		public override void SaveSettings()
		{
			SaveRadioStations();
		}

		private void SaveRadioStations()
		{

				//
				// Start by removing the currently available stations from the database
				//

        string strDefaultStation="";
        foreach(ListViewItem listItem in stationsListView.Items)
        {
          RadioStation radioStation = listItem.Tag as RadioStation;
					if(listItem.Checked == true)
					{
						strDefaultStation=radioStation.Name;
					}
        }
        using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          xmlwriter.SetValue("myradio", "default", strDefaultStation);
        }
		}

		private void LoadRadioStations()
		{
      string defaultStation = string.Empty;

      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        defaultStation = xmlreader.GetValueAsString("myradio", "default", "");
      }

			ArrayList stations = new ArrayList();
			RadioDatabase.GetStations(ref stations);

			foreach(MediaPortal.Radio.Database.RadioStation station in stations)
			{
				RadioStation radioStation = new RadioStation();

				radioStation.ID=station.ID;
				radioStation.Type = station.URL.Length == 0 ? "Radio" : "Stream";
				radioStation.Name = station.Name;
				radioStation.Frequency = station.Frequency;
				radioStation.Genre = station.Genre;
				radioStation.Bitrate = station.BitRate;
				radioStation.URL = station.URL;
				radioStation.Scrambled = station.Scrambled;

				ListViewItem listItem = new ListViewItem(new string[] { radioStation.Type, 
																		radioStation.Name,
																		radioStation.Frequency.ToString(Frequency.Format.MegaHerz),
																		radioStation.Genre,
																		radioStation.Bitrate.ToString(),
																		radioStation.URL
																	  } );

        //
        // Check default station
        //
        listItem.Checked = radioStation.Name.Equals(defaultStation);

				listItem.Tag = radioStation;

				stationsListView.Items.Add(listItem);
			}
		}

		private void stationsListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);
		}

    private void MoveSelectionDown()
    {
//      isDirty = true;

      for(int index = stationsListView.Items.Count - 1; index >= 0; index--)
      {
        if(stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if(index < stationsListView.Items.Count - 1)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);

            if(index + 1 < stationsListView.Items.Count)
            {
              stationsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              stationsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
//      isDirty = true;

      for(int index = 0; index < stationsListView.Items.Count; index++)
      {
        if(stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if(index > 0)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);
            stationsListView.Items.Insert(index - 1, listItem);
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

    private void stationsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      deleteButton.Enabled = editButton.Enabled = upButton.Enabled = downButton.Enabled = (stationsListView.SelectedItems.Count > 0);
    }

    private void stationsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
//      isDirty = true;

      if(e.NewValue == CheckState.Checked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if(stationsListView.Items[e.Index] != currentlyCheckedItem)
        {
          //
          // We have a new selection
          //
          if(currentlyCheckedItem != null)
            currentlyCheckedItem.Checked = false;
          currentlyCheckedItem = stationsListView.Items[e.Index];
        }
      }

      if(e.NewValue == CheckState.Unchecked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if(stationsListView.Items[e.Index] == currentlyCheckedItem)
        {
          currentlyCheckedItem = null;
        }
      }        
			SaveSettings();
    }

		private void btnMapChannelToCard_Click(object sender, System.EventArgs e)
		{
			if (listViewRadioChannels.SelectedItems==null) return;
			int card=1;
			int index =comboBoxCard.SelectedIndex;
			if (index >=0)
			{
				ComboCard combo=(ComboCard )comboBoxCard.Items[index];
				card=combo.ID;
			}
			
			for(int i=0; i < listViewRadioChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewRadioChannels.SelectedItems[i];
				MediaPortal.Radio.Database.RadioStation chan=(MediaPortal.Radio.Database.RadioStation)listItem.Tag;
				
				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listviewCardChannels.Items.Add(listItem);
				if (chan != null)
					RadioDatabase.MapChannelToCard(chan.ID,card);
			}
			
			for(int i=listViewRadioChannels.SelectedItems.Count-1; i >=0 ;i--)
			{
				ListViewItem listItem=listViewRadioChannels.SelectedItems[i];

				listViewRadioChannels.Items.Remove(listItem);
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
			if (listviewCardChannels.SelectedItems==null) return;
			for(int i=0; i < listviewCardChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listviewCardChannels.SelectedItems[i];
				MediaPortal.Radio.Database.RadioStation chan=(MediaPortal.Radio.Database.RadioStation)listItem.Tag;

				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewRadioChannels.Items.Add(listItem);
			}		

			for(int i=listviewCardChannels.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listviewCardChannels.SelectedItems[i];
				MediaPortal.Radio.Database.RadioStation channel=listItem.Tag as MediaPortal.Radio.Database.RadioStation;
				if (channel != null)
					RadioDatabase.UnmapChannelFromCard(channel,card);
				listviewCardChannels.Items.Remove(listItem);
			}
		}

		private void comboBoxCard_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
			FillInChannelCardMappings();
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

			listViewRadioChannels.Items.Clear();
			listviewCardChannels.Items.Clear();
			ArrayList cardChannels = new ArrayList();
			RadioDatabase.GetStationsForCard(ref cardChannels, card);
			
			ArrayList channels = new ArrayList();
			RadioDatabase.GetStations(ref channels);
			foreach (MediaPortal.Radio.Database.RadioStation chan in channels)
			{
				bool mapped=false;
				foreach (MediaPortal.Radio.Database.RadioStation chanCard in cardChannels)
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
					listViewRadioChannels.Items.Add(newItem);
				}
			}

			foreach (MediaPortal.Radio.Database.RadioStation chanCard in cardChannels)
			{
				ListViewItem newItemCard = new ListViewItem(chanCard.Name);
				newItemCard.Tag=chanCard;
				listviewCardChannels.Items.Add(newItemCard);
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			LoadCards();
		}


  }
}
