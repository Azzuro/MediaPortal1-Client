using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public abstract class FileExtensions : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox extensionTextBox;
		private System.Windows.Forms.ListBox extensionsListBox;
		private System.ComponentModel.IContainer components = null;

		public string Extensions
		{
			get { 
				string extensions = String.Empty;

				foreach(string extension in extensionsListBox.Items)
				{
					if(extensions.Length > 0)
						extensions += ",";

					extensions += extension;
				}

				return extensions; 
			}
			set 
			{ 
				string[] extensions = ((string)value).Split(',');
				extensionsListBox.Items.AddRange(extensions);
			}
		}

		public FileExtensions() : base("<Unknown>")
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		public FileExtensions(string name) : base(name)
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
      this.label1 = new System.Windows.Forms.Label();
      this.addButton = new System.Windows.Forms.Button();
      this.removeButton = new System.Windows.Forms.Button();
      this.extensionsListBox = new System.Windows.Forms.ListBox();
      this.extensionTextBox = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.removeButton);
      this.groupBox1.Controls.Add(this.extensionsListBox);
      this.groupBox1.Controls.Add(this.extensionTextBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(256, 264);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Extensions";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(224, 40);
      this.label1.TabIndex = 4;
      this.label1.Text = "Files matching an extension listed below will be considered a known media type.";
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.addButton.Location = new System.Drawing.Point(168, 72);
      this.addButton.Name = "addButton";
      this.addButton.TabIndex = 0;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // removeButton
      // 
      this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.removeButton.Enabled = false;
      this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.removeButton.Location = new System.Drawing.Point(168, 104);
      this.removeButton.Name = "removeButton";
      this.removeButton.TabIndex = 3;
      this.removeButton.Text = "Remove";
      this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
      // 
      // extensionsListBox
      // 
      this.extensionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionsListBox.Location = new System.Drawing.Point(16, 96);
      this.extensionsListBox.Name = "extensionsListBox";
      this.extensionsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.extensionsListBox.Size = new System.Drawing.Size(144, 147);
      this.extensionsListBox.TabIndex = 1;
      this.extensionsListBox.SelectedIndexChanged += new System.EventHandler(this.extensionsListBox_SelectedIndexChanged);
      // 
      // extensionTextBox
      // 
      this.extensionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionTextBox.Location = new System.Drawing.Point(16, 72);
      this.extensionTextBox.Name = "extensionTextBox";
      this.extensionTextBox.Size = new System.Drawing.Size(144, 20);
      this.extensionTextBox.TabIndex = 0;
      this.extensionTextBox.Text = "";
      // 
      // FileExtensions
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "FileExtensions";
      this.Size = new System.Drawing.Size(272, 280);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void addButton_Click(object sender, System.EventArgs e)
		{
			string extension = extensionTextBox.Text;
			
			if(extension != null && extension.Length != 0)
			{
				//
				// Only grab what we got after the first .
				//
				int dotPosition = extension.IndexOf(".");

				if(dotPosition < 0)
				{
					//
					// We got no dot in the extension, append it
					//
					extension = String.Format(".{0}", extension);
				}
				else
				{
					//
					// Remove everything before the dot
					//
					extension = extension.Substring(dotPosition);
				}

				//
				// Remove unwanted characters
				//
				extension = extension.Replace("*", "");

				//
				// Add extension to the list
				//
				extensionsListBox.Items.Add(extension);

				//
				// Clear text
				//
				extensionTextBox.Text = String.Empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void removeButton_Click(object sender, System.EventArgs e)
		{
			int itemsSelected = extensionsListBox.SelectedIndices.Count;

			//
			// Make sure we have a valid item selected
			//
			for(int index = 0; index < itemsSelected; index++)
			{
				extensionsListBox.Items.RemoveAt(extensionsListBox.SelectedIndices[0]);
			}
		}

    private void extensionsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      removeButton.Enabled = (extensionsListBox.SelectedItems.Count > 0);
    }
	}
}

