/*
 * Created by SharpDevelop.
 * User: Josh
 * Date: 6/18/2004
 * Time: 11:59 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.RSS;

namespace GUIRSSFeed
{
	/// <summary>
	/// A setup form for the My News Plugin
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form, ISetupForm
	{
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button buttonAdd;
		private System.Windows.Forms.ListBox listBox;
		private System.Windows.Forms.Button buttonEdit;
		private System.Windows.Forms.Button buttonDelete;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.CheckBox checkAutoRefresh;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textRefreshInterval;
		private System.Windows.Forms.Label label2;
		private DetailsForm form;

		public SetupForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}

		~SetupForm()
		{
			listBox.Items.Clear();
		}

		#region ButtonHandlers

		/// <summary>
		/// Opens up the Details for so you can add a new feed.
		/// </summary>
		void addSite(object obj, System.EventArgs ea)
		{
			form = new DetailsForm(this, -1);
			form.ShowDialog(this.Parent);
			listBox.Items.Clear();
			PopulateFields();
		}

		/// <summary>
		/// Opens up the DetailsForm so you can edit a feed
		/// </summary>
		void editSite(object obj, System.EventArgs ea)
		{
			int iItem = listBox.SelectedIndices[0];
			int ID = 0;
			//find the index of the item in question
			string tempText;
			for (int i=0; i<20; i++)
			{
				using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
  				{
					tempText = xmlreader.GetValueAsString("rss","siteName"+i,"");
  					if (tempText == (string)listBox.Items[iItem])
  					{
  						ID = i;
  						break;
  					}
   				}
			}
			DetailsForm form = new DetailsForm(this, ID);
			form.ShowDialog(this.Parent);
      listBox.Items.Clear();
			PopulateFields();
		}

		/// <summary>
		/// Deletes the feed that is currently selected in the listbox
		/// </summary>
		void deleteSite(object obj, System.EventArgs ea)
		{
			int iItem = listBox.SelectedIndices[0];
			int ID = 0;
			//find the index of the item in question
			string tempText;
			for (int i=0; i<20; i++)
			{
				using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
  				{
					tempText = xmlreader.GetValueAsString("rss","siteName"+i,"");
  					if (tempText == (string)listBox.Items[iItem])
  					{
  						ID = i;
  						break;
  					}
   				}
			}

			string strNameTag=String.Format("siteName{0}",ID);
 			string strURLTag=String.Format("siteURL{0}",ID);
 			string strDescriptionTag=String.Format("siteDescription{0}",ID);
			using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
   		{
				xmlwriter.SetValue("rss",strNameTag,"");
     		xmlwriter.SetValue("rss",strURLTag,"");
     		xmlwriter.SetValue("rss",strDescriptionTag,"");
   		}

   		listBox.Items.Clear();
			PopulateFields();

			if (GUIWindowManager.ActiveWindow == (int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS)
			{
				Console.WriteLine("RSS active window, need to somehow refresh");
				MediaPortal.GUI.RSS.GUIRSSFeed rss = (MediaPortal.GUI.RSS.GUIRSSFeed)GUIWindowManager.GetWindow((int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS);
				rss.refreshFeeds();
			}
		}
		#endregion

		/// <summary>
		/// Initialise the form
		/// </summary>
		private void SetupForm_Load(object sender, System.EventArgs e)
   	{
   		listBox.Items.Clear();
   		PopulateFields();
   	}

		/// <summary>
		/// Adds the names of available newsfeeds to the listbox
		/// </summary>
		public void PopulateFields()
		{
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
     	{
				for (int i=0; i < 20; i++)
				{
					string strNameTag=String.Format("siteName{0}",i);
					string strURLTag=String.Format("siteURL{0}",i);

					string strName=xmlreader.GetValueAsString("rss",strNameTag,"");
					string strURL=xmlreader.GetValueAsString("rss",strURLTag,"");

					if (strName.Length>0 && strURL.Length>0)
					{
						this.listBox.Items.Add(strName);
					}
				}

				// general settings
				textRefreshInterval.Text=xmlreader.GetValueAsString("rss","iRefreshTime", "15");			
				checkAutoRefresh.Checked=false;
				if (xmlreader.GetValueAsInt("rss", "bAutoRefresh", 0) != 0) checkAutoRefresh.Checked=true;
    	}
		}


		#region ISetup Interface methods
		/// <summary>
		/// See ISetupForm interface
		/// </summary>
		public bool	CanEnable()		// Indicates whether plugin can be enabled/disabled
		{
			return true;
		}

    public bool HasSetup()
    {
      return true;
    }
		/// <summary>
		/// See ISetupForm interface
		/// </summary>
		public bool DefaultEnabled()
		{
			return false;
		}

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
		public int  GetWindowId()
		{
			return (int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS;
		}

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText=GUILocalizeStrings.Get(9); // My News
			strButtonImage="";
			strButtonImageFocus="";
			strPictureImage="";			//the big image seen when hovering over My Radio
			return true;
		}

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
    public string PluginName()
    {
      return "My News";
    }

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
    public string Description()
    {
      return "Plugin to read RSS feeds";
    }

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
    public string Author()
    {
      return "Gilthalas";
    }

		/// <summary>
		/// See ISetupForm interface
		/// </summary>
    public void ShowPlugin()
    {
      ShowDialog();
    }
		#endregion

		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.buttonDelete = new System.Windows.Forms.Button();
			this.buttonEdit = new System.Windows.Forms.Button();
			this.listBox = new System.Windows.Forms.ListBox();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.label = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.checkAutoRefresh = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textRefreshInterval = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonDelete
			// 
			this.buttonDelete.Location = new System.Drawing.Point(264, 176);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(88, 23);
			this.buttonDelete.TabIndex = 3;
			this.buttonDelete.Text = "Delete Site";
			this.buttonDelete.Click += new System.EventHandler(this.deleteSite);
			// 
			// buttonEdit
			// 
			this.buttonEdit.Location = new System.Drawing.Point(144, 176);
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.Size = new System.Drawing.Size(88, 23);
			this.buttonEdit.TabIndex = 2;
			this.buttonEdit.Text = "Edit Site";
			this.buttonEdit.Click += new System.EventHandler(this.editSite);
			// 
			// listBox
			// 
			this.listBox.Location = new System.Drawing.Point(24, 32);
			this.listBox.Name = "listBox";
			this.listBox.Size = new System.Drawing.Size(328, 134);
			this.listBox.TabIndex = 5;
			// 
			// buttonAdd
			// 
			this.buttonAdd.Location = new System.Drawing.Point(24, 176);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(88, 23);
			this.buttonAdd.TabIndex = 1;
			this.buttonAdd.Text = "Add Site";
			this.buttonAdd.Click += new System.EventHandler(this.addSite);
			// 
			// label
			// 
			this.label.Location = new System.Drawing.Point(24, 8);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(368, 23);
			this.label.TabIndex = 4;
			this.label.Text = "Add sites to get news for, and edit options here";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(280, 256);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(72, 23);
			this.button3.TabIndex = 12;
			this.button3.Text = "Done";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// checkAutoRefresh
			// 
			this.checkAutoRefresh.Location = new System.Drawing.Point(24, 224);
			this.checkAutoRefresh.Name = "checkAutoRefresh";
			this.checkAutoRefresh.Size = new System.Drawing.Size(112, 16);
			this.checkAutoRefresh.TabIndex = 4;
			this.checkAutoRefresh.Text = "Auto refresh";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 248);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "Refresh interval:";
			// 
			// textRefreshInterval
			// 
			this.textRefreshInterval.Location = new System.Drawing.Point(113, 245);
			this.textRefreshInterval.Name = "textRefreshInterval";
			this.textRefreshInterval.Size = new System.Drawing.Size(31, 20);
			this.textRefreshInterval.TabIndex = 14;
			this.textRefreshInterval.Text = "15";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(144, 248);
			this.label2.Name = "label2";
			this.label2.TabIndex = 15;
			this.label2.Text = "minutes";
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(376, 294);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textRefreshInterval);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkAutoRefresh);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.listBox);
			this.Controls.Add(this.label);
			this.Controls.Add(this.buttonDelete);
			this.Controls.Add(this.buttonEdit);
			this.Controls.Add(this.buttonAdd);
			this.Name = "SetupForm";
			this.Text = "My News Settings";
			this.Load += new System.EventHandler(this.SetupForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void button3_Click(object sender, System.EventArgs e)
		{
			using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("rss","iRefreshTime",textRefreshInterval.Text);

				int iAutoRefresh=0;
				if (checkAutoRefresh.Checked)iAutoRefresh=1;
				xmlwriter.SetValue("rss", "bAutoRefresh", iAutoRefresh.ToString());
			}
			this.Close();
		}

	}
}
