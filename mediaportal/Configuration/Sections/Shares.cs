using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public abstract class Shares : MediaPortal.Configuration.SectionSettings
	{
    protected class ShareData
    {

      public string Name;
      public string Folder;
      public string PinCode;

      public bool   IsRemote=false;
      public string Server=String.Empty;
      public string LoginName=String.Empty;
      public string PassWord=String.Empty;
      public string RemoteFolder=String.Empty;
      public int    Port=21;

      public bool   HasPinCode
      {
        get { return(PinCode.Length > 0); }
      }

      public ShareData(string name, string folder, string pinCode)
      {
        this.Name = name;
        this.Folder = folder;
        this.PinCode = pinCode;
      }
    }

    protected const int MaximumShares = 20;

		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button addButton;
		private MediaPortal.UserInterface.Controls.MPListView sharesListView;
    private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.ComponentModel.IContainer components = null;

		public Shares() : base("<Unknown>")
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		public Shares(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
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
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.deleteButton = new System.Windows.Forms.Button();
			this.editButton = new System.Windows.Forms.Button();
			this.addButton = new System.Windows.Forms.Button();
			this.sharesListView = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.deleteButton);
			this.groupBox1.Controls.Add(this.editButton);
			this.groupBox1.Controls.Add(this.addButton);
			this.groupBox1.Controls.Add(this.sharesListView);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(384, 352);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Folders";
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(176, 312);
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
			this.editButton.Location = new System.Drawing.Point(96, 312);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 2;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(16, 312);
			this.addButton.Name = "addButton";
			this.addButton.TabIndex = 1;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// sharesListView
			// 
			this.sharesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.sharesListView.CheckBoxes = true;
			this.sharesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																										 this.columnHeader1,
																																										 this.columnHeader3,
																																										 this.columnHeader2});
			this.sharesListView.FullRowSelect = true;
			this.sharesListView.Location = new System.Drawing.Point(16, 24);
			this.sharesListView.Name = "sharesListView";
			this.sharesListView.Size = new System.Drawing.Size(352, 280);
			this.sharesListView.TabIndex = 0;
			this.sharesListView.View = System.Windows.Forms.View.Details;
			this.sharesListView.SelectedIndexChanged += new System.EventHandler(this.sharesListView_SelectedIndexChanged);
			this.sharesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListView_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 106;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Pin";
			this.columnHeader3.Width = 57;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Folder";
			this.columnHeader2.Width = 185;
			// 
			// Shares
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Shares";
			this.Size = new System.Drawing.Size(400, 368);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		ListViewItem currentlyCheckedItem = null;

		private void addButton_Click(object sender, System.EventArgs e)
		{
			EditShareForm editShare = new EditShareForm();

			DialogResult dialogResult = editShare.ShowDialog(this);

			if(dialogResult == DialogResult.OK)
			{
        ShareData shareData=new ShareData(editShare.ShareName, editShare.Folder, editShare.PinCode);
        shareData.IsRemote=editShare.IsRemote;
        shareData.Server=editShare.Server;
        shareData.LoginName=editShare.LoginName;
        shareData.PassWord=editShare.PassWord;
        shareData.Port=editShare.Port;
        shareData.RemoteFolder=editShare.RemoteFolder;
            

				AddShare(shareData, currentlyCheckedItem == null);
			}
		}

		protected void AddShare(ShareData shareData, bool check)
		{
			ListViewItem listItem = new ListViewItem(new string[] { shareData.Name, shareData.HasPinCode ? "Yes" : "No", shareData.Folder });

      if (shareData.IsRemote)
      {
        listItem.SubItems[2].Text=String.Format("ftp://{0}:{1}{2}",shareData.Server,shareData.Port,shareData.RemoteFolder);
      }
      listItem.Tag = shareData;
			listItem.Checked = check;
			if(check) currentlyCheckedItem = listItem;

			sharesListView.Items.Add(listItem);
		}

		private void editButton_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem selectedItem in sharesListView.SelectedItems)
			{
        ShareData shareData = selectedItem.Tag as ShareData;

        if(shareData != null)
        {
          EditShareForm editShare = new EditShareForm();
	
          editShare.ShareName = shareData.Name;
          editShare.PinCode = shareData.PinCode;
          editShare.Folder = shareData.Folder;
          
          editShare.IsRemote=shareData.IsRemote;
          editShare.Server=shareData.Server;
          editShare.Port=shareData.Port;
          editShare.LoginName=shareData.LoginName;
          editShare.PassWord=shareData.PassWord;
          editShare.RemoteFolder=shareData.RemoteFolder;

          DialogResult dialogResult = editShare.ShowDialog(this);

          if(dialogResult == DialogResult.OK)
          {
            shareData.Name = editShare.ShareName;
            shareData.Folder = editShare.Folder;
            shareData.PinCode = editShare.PinCode;

            shareData.IsRemote=editShare.IsRemote;
            shareData.Server=editShare.Server;
            shareData.LoginName=editShare.LoginName;
            shareData.PassWord=editShare.PassWord;
            shareData.Port=editShare.Port;
            shareData.RemoteFolder=editShare.RemoteFolder;
            

            selectedItem.Tag = shareData;

            selectedItem.SubItems[0].Text = shareData.Name;
            selectedItem.SubItems[1].Text = shareData.HasPinCode ? "Yes" : "No";
            selectedItem.SubItems[2].Text = shareData.Folder;
						if (shareData.IsRemote) selectedItem.SubItems[2].Text = String.Format("ftp://{0}:{1}{2}",shareData.Server,shareData.Port,shareData.RemoteFolder);

          }
        }
			}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			int selectedItems = sharesListView.SelectedIndices.Count;

			for(int index = 0; index < selectedItems; index++)
			{
				sharesListView.Items.RemoveAt(sharesListView.SelectedIndices[0]);
			}
		}

		private void sharesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if(e.NewValue == CheckState.Checked)
			{
				//
				// Check if the new selected item is the same as the current one
				//
				if(sharesListView.Items[e.Index] != currentlyCheckedItem)
				{
					//
					// We have a new selection
					//
					if(currentlyCheckedItem != null)
						currentlyCheckedItem.Checked = false;
					currentlyCheckedItem = sharesListView.Items[e.Index];
				}
			}

			if(e.NewValue == CheckState.Unchecked)
			{
				//
				// Check if the new selected item is the same as the current one
				//
				if(sharesListView.Items[e.Index] == currentlyCheckedItem)
				{
					currentlyCheckedItem = null;
				}
			}
		}

		public ListView.ListViewItemCollection CurrentShares
		{
			get 
			{
				return sharesListView.Items;
			}
		}

		public ListViewItem DefaultShare
		{
			get
			{
				return currentlyCheckedItem;
			}
		}

    public enum DriveType
    {
      Removable = 2,
      Fixed = 3,
      RemoteDisk = 4,
      CD = 5,
      DVD = 5,
      RamDisk = 6
    }

    public void AddStaticShares(DriveType driveType, string defaultName)
    {
      string[] drives = Environment.GetLogicalDrives();

      foreach(string drive in drives)
      {
        if(Util.Utils.getDriveType(drive) == (int)driveType)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);

          if(driveName.Length == 0)
          {
            string driveLetter = drive.Substring(0, 1).ToUpper();
            driveName = String.Format("{0} {1}:", defaultName, driveLetter);            
          }

          //
          // Check if the share already exists
          //
          foreach(ListViewItem listItem in CurrentShares)
          {
            if(listItem.SubItems[2].Text == drive)
            {
              driveFound = true;
              break;
            }
          }

          if(driveFound == false)
          {
            //
            // Add share
            //
						string name="";
						switch (driveType)
						{
							case DriveType.Removable:
								name=String.Format("Removable {0}:",drive.Substring(0, 1).ToUpper());								
								break;
							case DriveType.Fixed:
								name=String.Format("Fixed {0}:",drive.Substring(0, 1).ToUpper());
								break;
							case DriveType.RemoteDisk:
								name=String.Format("Remote {0}:",drive.Substring(0, 1).ToUpper());
								break;
							case DriveType.DVD: // or cd
								name=String.Format("CD/DVD {0}:",drive.Substring(0, 1).ToUpper());
								break;
							case DriveType.RamDisk:
								name=String.Format("Ram {0}:",drive.Substring(0, 1).ToUpper());
								break;
						}
	          AddShare(new ShareData(name, drive, String.Empty), false);
          }
        }
      }
    }

    private void sharesListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      editButton.Enabled  = deleteButton.Enabled = (sharesListView.SelectedItems.Count > 0);
    }
	}
}

