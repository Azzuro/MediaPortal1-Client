using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Management; 

namespace MediaPortal.Configuration.Sections
{
	public class TVRecording : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox startTextBox;
		private System.Windows.Forms.TextBox endTextBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.CheckBox cbDeleteWatchedShows;
		private System.ComponentModel.IContainer components = null;

		public TVRecording() : this("Recording")
		{
		}

		public TVRecording(string name) : base(name)
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
			this.endTextBox = new System.Windows.Forms.TextBox();
			this.startTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.cbDeleteWatchedShows = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.endTextBox);
			this.groupBox1.Controls.Add(this.startTextBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.cbDeleteWatchedShows);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 120);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "TV Recording Settings";
			// 
			// endTextBox
			// 
			this.endTextBox.Location = new System.Drawing.Point(104, 64);
			this.endTextBox.MaxLength = 3;
			this.endTextBox.Name = "endTextBox";
			this.endTextBox.Size = new System.Drawing.Size(40, 20);
			this.endTextBox.TabIndex = 3;
			this.endTextBox.Text = "";
			this.endTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.endTextBox_KeyPress);
			// 
			// startTextBox
			// 
			this.startTextBox.Location = new System.Drawing.Point(104, 32);
			this.startTextBox.MaxLength = 3;
			this.startTextBox.Name = "startTextBox";
			this.startTextBox.Size = new System.Drawing.Size(40, 20);
			this.startTextBox.TabIndex = 2;
			this.startTextBox.Text = "";
			this.startTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.startTextBox_KeyPress);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 23);
			this.label4.TabIndex = 8;
			this.label4.Text = "Stop recording";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(144, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(208, 23);
			this.label3.TabIndex = 11;
			this.label3.Text = "minute(s) after program stops";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(144, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(207, 23);
			this.label2.TabIndex = 10;
			this.label2.Text = "minute(s) before program starts";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 9;
			this.label1.Text = "Start recording";
			// 
			// cbDeleteWatchedShows
			// 
			this.cbDeleteWatchedShows.Location = new System.Drawing.Point(24, 88);
			this.cbDeleteWatchedShows.Name = "cbDeleteWatchedShows";
			this.cbDeleteWatchedShows.Size = new System.Drawing.Size(304, 24);
			this.cbDeleteWatchedShows.TabIndex = 3;
			this.cbDeleteWatchedShows.Text = "Automaticly delete recordings after you watched them";
			// 
			// TVRecording
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "TVRecording";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 1));
				endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 1));				
				cbDeleteWatchedShows.Checked= xmlreader.GetValueAsBool("capture", "deletewatchedshows", true);

			}		
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "prerecord", startTextBox.Text);
				xmlwriter.SetValue("capture", "postrecord", endTextBox.Text);

				xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbDeleteWatchedShows.Checked);
				
			}
		}

		private void startTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		private void endTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}


		
	}
}

