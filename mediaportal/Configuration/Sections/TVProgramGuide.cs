using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using MediaPortal.TV.Database;

namespace MediaPortal.Configuration.Sections
{
	public class TVProgramGuide : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox useColorCheckBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox GrabbercomboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox AdvancedDaystextBox;
    private System.Windows.Forms.Button parametersButton;
    private System.Windows.Forms.TextBox parametersTextBox;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox daysToKeepTextBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button browseButton;
    private System.Windows.Forms.TextBox folderNameTextBox;
    private System.Windows.Forms.Label folderNameLabel;
    private System.Windows.Forms.TextBox compensateTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox useTimeZoneCheckBox;
    private System.Windows.Forms.Label label1;
		protected System.Windows.Forms.RadioButton advancedRadioButton;
		protected System.Windows.Forms.RadioButton basicRadioButton;
    private MediaPortal.UserInterface.Controls.MPCheckBox createScheduleCheckBox;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox hoursTextBox;
    private System.Windows.Forms.TextBox dayIntervalTextBox;
    private System.Windows.Forms.TextBox minutesTextBox;
    private System.Windows.Forms.Button RunGrabberButton;
    private System.Windows.Forms.TextBox UserTextBox;
    private System.Windows.Forms.TextBox PasswordTextBox;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Button DeleteTaskButton;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Button btnUpdateTvGuide;
		private System.ComponentModel.IContainer components = null;
    bool  OldTimeZoneCompensation=false;
    private System.Windows.Forms.Button btnClearTVDatabase;
    private System.Windows.Forms.TextBox textBoxMinutes;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label15;
    int   OldTimeZoneOffsetHours=0;
    int   OldTimeZoneOffsetMins=0;

		public TVProgramGuide() : this("Program Guide")
		{
		}

		public TVProgramGuide(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
      // Setup grabbers
      //
      SetupGrabbers();
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
			this.useColorCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.label15 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxMinutes = new System.Windows.Forms.TextBox();
			this.btnClearTVDatabase = new System.Windows.Forms.Button();
			this.btnUpdateTvGuide = new System.Windows.Forms.Button();
			this.RunGrabberButton = new System.Windows.Forms.Button();
			this.advancedRadioButton = new System.Windows.Forms.RadioButton();
			this.compensateTextBox = new System.Windows.Forms.TextBox();
			this.useTimeZoneCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.browseButton = new System.Windows.Forms.Button();
			this.folderNameTextBox = new System.Windows.Forms.TextBox();
			this.folderNameLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.daysToKeepTextBox = new System.Windows.Forms.TextBox();
			this.parametersButton = new System.Windows.Forms.Button();
			this.parametersTextBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.AdvancedDaystextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.GrabbercomboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.basicRadioButton = new System.Windows.Forms.RadioButton();
			this.label14 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.dayIntervalTextBox = new System.Windows.Forms.TextBox();
			this.minutesTextBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.hoursTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.createScheduleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.UserTextBox = new System.Windows.Forms.TextBox();
			this.PasswordTextBox = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.label13 = new System.Windows.Forms.Label();
			this.DeleteTaskButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.useColorCheckBox);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 48);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General Settings";
			// 
			// useColorCheckBox
			// 
			this.useColorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useColorCheckBox.Location = new System.Drawing.Point(16, 16);
			this.useColorCheckBox.Name = "useColorCheckBox";
			this.useColorCheckBox.Size = new System.Drawing.Size(308, 24);
			this.useColorCheckBox.TabIndex = 0;
			this.useColorCheckBox.Text = "Use colors in program guide";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.label15);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.textBoxMinutes);
			this.groupBox2.Controls.Add(this.btnClearTVDatabase);
			this.groupBox2.Controls.Add(this.btnUpdateTvGuide);
			this.groupBox2.Controls.Add(this.RunGrabberButton);
			this.groupBox2.Controls.Add(this.advancedRadioButton);
			this.groupBox2.Controls.Add(this.compensateTextBox);
			this.groupBox2.Controls.Add(this.useTimeZoneCheckBox);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.browseButton);
			this.groupBox2.Controls.Add(this.folderNameTextBox);
			this.groupBox2.Controls.Add(this.folderNameLabel);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.daysToKeepTextBox);
			this.groupBox2.Controls.Add(this.parametersButton);
			this.groupBox2.Controls.Add(this.parametersTextBox);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.AdvancedDaystextBox);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.GrabbercomboBox);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.basicRadioButton);
			this.groupBox2.Controls.Add(this.label14);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 56);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 256);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "XMLTV Settings";
			this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(224, 80);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(56, 16);
			this.label15.TabIndex = 69;
			this.label15.Text = "Hours";
			this.label15.Click += new System.EventHandler(this.label15_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(160, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(8, 16);
			this.label2.TabIndex = 68;
			this.label2.Text = ":";
			// 
			// textBoxMinutes
			// 
			this.textBoxMinutes.Location = new System.Drawing.Point(176, 80);
			this.textBoxMinutes.Name = "textBoxMinutes";
			this.textBoxMinutes.Size = new System.Drawing.Size(32, 20);
			this.textBoxMinutes.TabIndex = 4;
			this.textBoxMinutes.Text = "0";
			// 
			// btnClearTVDatabase
			// 
			this.btnClearTVDatabase.Location = new System.Drawing.Point(216, 224);
			this.btnClearTVDatabase.Name = "btnClearTVDatabase";
			this.btnClearTVDatabase.Size = new System.Drawing.Size(120, 24);
			this.btnClearTVDatabase.TabIndex = 11;
			this.btnClearTVDatabase.Text = "Remove all programs";
			this.btnClearTVDatabase.Click += new System.EventHandler(this.btnClearTVDatabase_Click);
			// 
			// btnUpdateTvGuide
			// 
			this.btnUpdateTvGuide.Location = new System.Drawing.Point(288, 56);
			this.btnUpdateTvGuide.Name = "btnUpdateTvGuide";
			this.btnUpdateTvGuide.Size = new System.Drawing.Size(120, 48);
			this.btnUpdateTvGuide.TabIndex = 5;
			this.btnUpdateTvGuide.Text = "Update TV database with new time zone compensation";
			this.btnUpdateTvGuide.Click += new System.EventHandler(this.btnUpdateTvGuide_Click);
			// 
			// RunGrabberButton
			// 
			this.RunGrabberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RunGrabberButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RunGrabberButton.Location = new System.Drawing.Point(344, 224);
			this.RunGrabberButton.Name = "RunGrabberButton";
			this.RunGrabberButton.Size = new System.Drawing.Size(80, 20);
			this.RunGrabberButton.TabIndex = 12;
			this.RunGrabberButton.Text = "Run Grabber";
			this.RunGrabberButton.Click += new System.EventHandler(this.RunGrabberButton_Click);
			// 
			// advancedRadioButton
			// 
			this.advancedRadioButton.Enabled = false;
			this.advancedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.advancedRadioButton.Location = new System.Drawing.Point(16, 200);
			this.advancedRadioButton.Name = "advancedRadioButton";
			this.advancedRadioButton.Size = new System.Drawing.Size(160, 24);
			this.advancedRadioButton.TabIndex = 55;
			this.advancedRadioButton.Text = "Advanced Single Day Grabs";
			this.advancedRadioButton.CheckedChanged += new System.EventHandler(this.advancedRadioButton_CheckedChanged);
			// 
			// compensateTextBox
			// 
			this.compensateTextBox.Location = new System.Drawing.Point(136, 80);
			this.compensateTextBox.MaxLength = 3;
			this.compensateTextBox.Name = "compensateTextBox";
			this.compensateTextBox.Size = new System.Drawing.Size(24, 20);
			this.compensateTextBox.TabIndex = 3;
			this.compensateTextBox.Text = "0";
			// 
			// useTimeZoneCheckBox
			// 
			this.useTimeZoneCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useTimeZoneCheckBox.Location = new System.Drawing.Point(16, 50);
			this.useTimeZoneCheckBox.Name = "useTimeZoneCheckBox";
			this.useTimeZoneCheckBox.Size = new System.Drawing.Size(240, 24);
			this.useTimeZoneCheckBox.TabIndex = 2;
			this.useTimeZoneCheckBox.Text = "Use time zone information from XMLTV";
			this.useTimeZoneCheckBox.CheckedChanged += new System.EventHandler(this.useTimeZoneCheckBox_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 28);
			this.label1.TabIndex = 50;
			this.label1.Text = "Compensate time zone with";
			// 
			// browseButton
			// 
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.browseButton.Location = new System.Drawing.Point(369, 22);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(56, 20);
			this.browseButton.TabIndex = 1;
			this.browseButton.Text = "Browse";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// folderNameTextBox
			// 
			this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.folderNameTextBox.Location = new System.Drawing.Point(96, 22);
			this.folderNameTextBox.Name = "folderNameTextBox";
			this.folderNameTextBox.Size = new System.Drawing.Size(265, 20);
			this.folderNameTextBox.TabIndex = 0;
			this.folderNameTextBox.Text = "";
			// 
			// folderNameLabel
			// 
			this.folderNameLabel.Location = new System.Drawing.Point(16, 25);
			this.folderNameLabel.Name = "folderNameLabel";
			this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
			this.folderNameLabel.TabIndex = 47;
			this.folderNameLabel.Text = "XMLTV folder";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(184, 204);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(104, 16);
			this.label6.TabIndex = 44;
			this.label6.Text = "Days to download";
			// 
			// daysToKeepTextBox
			// 
			this.daysToKeepTextBox.Enabled = false;
			this.daysToKeepTextBox.Location = new System.Drawing.Point(312, 176);
			this.daysToKeepTextBox.MaxLength = 3;
			this.daysToKeepTextBox.Name = "daysToKeepTextBox";
			this.daysToKeepTextBox.Size = new System.Drawing.Size(40, 20);
			this.daysToKeepTextBox.TabIndex = 9;
			this.daysToKeepTextBox.Text = "";
			this.daysToKeepTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.daysToKeepTextBox_KeyPress);
			// 
			// parametersButton
			// 
			this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.parametersButton.Enabled = false;
			this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.parametersButton.Location = new System.Drawing.Point(369, 138);
			this.parametersButton.Name = "parametersButton";
			this.parametersButton.Size = new System.Drawing.Size(56, 20);
			this.parametersButton.TabIndex = 8;
			this.parametersButton.Text = "List";
			this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
			// 
			// parametersTextBox
			// 
			this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.parametersTextBox.Enabled = false;
			this.parametersTextBox.Location = new System.Drawing.Point(168, 137);
			this.parametersTextBox.Name = "parametersTextBox";
			this.parametersTextBox.Size = new System.Drawing.Size(192, 20);
			this.parametersTextBox.TabIndex = 7;
			this.parametersTextBox.Text = "";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 139);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 16);
			this.label7.TabIndex = 37;
			this.label7.Text = "Parameters";
			// 
			// AdvancedDaystextBox
			// 
			this.AdvancedDaystextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.AdvancedDaystextBox.Enabled = false;
			this.AdvancedDaystextBox.Location = new System.Drawing.Point(312, 200);
			this.AdvancedDaystextBox.Name = "AdvancedDaystextBox";
			this.AdvancedDaystextBox.Size = new System.Drawing.Size(112, 20);
			this.AdvancedDaystextBox.TabIndex = 10;
			this.AdvancedDaystextBox.Text = "";
			this.AdvancedDaystextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AdvancedDaystextBox_KeyPress);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(184, 178);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(144, 16);
			this.label4.TabIndex = 30;
			this.label4.Text = "Days to keep in guide";
			// 
			// GrabbercomboBox
			// 
			this.GrabbercomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.GrabbercomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.GrabbercomboBox.Location = new System.Drawing.Point(168, 112);
			this.GrabbercomboBox.Name = "GrabbercomboBox";
			this.GrabbercomboBox.Size = new System.Drawing.Size(256, 21);
			this.GrabbercomboBox.TabIndex = 6;
			this.GrabbercomboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GrabbercomboBox_KeyPress);
			this.GrabbercomboBox.SelectedIndexChanged += new System.EventHandler(this.GrabbercomboBox_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 115);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(144, 23);
			this.label3.TabIndex = 28;
			this.label3.Text = "Grabber";
			// 
			// basicRadioButton
			// 
			this.basicRadioButton.Enabled = false;
			this.basicRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.basicRadioButton.Location = new System.Drawing.Point(16, 174);
			this.basicRadioButton.Name = "basicRadioButton";
			this.basicRadioButton.Size = new System.Drawing.Size(192, 24);
			this.basicRadioButton.TabIndex = 54;
			this.basicRadioButton.Text = "Basic Multiday Grab";
			this.basicRadioButton.CheckedChanged += new System.EventHandler(this.basicRadioButton_CheckedChanged);
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(184, 176);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(144, 16);
			this.label14.TabIndex = 30;
			this.label14.Text = "Days to keep in guide";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(312, 26);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(40, 16);
			this.label10.TabIndex = 63;
			this.label10.Text = "day(s)";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(244, 26);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(34, 16);
			this.label9.TabIndex = 62;
			this.label9.Text = "Every";
			// 
			// dayIntervalTextBox
			// 
			this.dayIntervalTextBox.Location = new System.Drawing.Point(279, 24);
			this.dayIntervalTextBox.Name = "dayIntervalTextBox";
			this.dayIntervalTextBox.Size = new System.Drawing.Size(32, 20);
			this.dayIntervalTextBox.TabIndex = 3;
			this.dayIntervalTextBox.Text = "";
			this.dayIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dayIntervalTextBox_KeyPress);
			// 
			// minutesTextBox
			// 
			this.minutesTextBox.Location = new System.Drawing.Point(211, 24);
			this.minutesTextBox.Name = "minutesTextBox";
			this.minutesTextBox.Size = new System.Drawing.Size(32, 20);
			this.minutesTextBox.TabIndex = 2;
			this.minutesTextBox.Text = "";
			this.minutesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.minutesTextBox_KeyPress);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(170, 26);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(40, 16);
			this.label8.TabIndex = 1;
			this.label8.Text = "Minute";
			// 
			// hoursTextBox
			// 
			this.hoursTextBox.Location = new System.Drawing.Point(137, 24);
			this.hoursTextBox.Name = "hoursTextBox";
			this.hoursTextBox.Size = new System.Drawing.Size(32, 20);
			this.hoursTextBox.TabIndex = 1;
			this.hoursTextBox.Text = "";
			this.hoursTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.hoursTextBox_KeyPress);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(104, 26);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 16);
			this.label5.TabIndex = 57;
			this.label5.Text = "Hours";
			// 
			// createScheduleCheckBox
			// 
			this.createScheduleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.createScheduleCheckBox.Location = new System.Drawing.Point(16, 26);
			this.createScheduleCheckBox.Name = "createScheduleCheckBox";
			this.createScheduleCheckBox.Size = new System.Drawing.Size(88, 16);
			this.createScheduleCheckBox.TabIndex = 0;
			this.createScheduleCheckBox.Text = "Create Task";
			this.createScheduleCheckBox.CheckedChanged += new System.EventHandler(this.createScheduleCheckBox_CheckedChanged);
			// 
			// UserTextBox
			// 
			this.UserTextBox.Location = new System.Drawing.Point(168, 56);
			this.UserTextBox.Name = "UserTextBox";
			this.UserTextBox.Size = new System.Drawing.Size(96, 20);
			this.UserTextBox.TabIndex = 5;
			this.UserTextBox.Text = "";
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.Location = new System.Drawing.Point(328, 56);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = new System.Drawing.Size(96, 20);
			this.PasswordTextBox.TabIndex = 6;
			this.PasswordTextBox.Text = "";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(88, 57);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(72, 18);
			this.label11.TabIndex = 67;
			this.label11.Text = "User account";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(272, 58);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(56, 16);
			this.label12.TabIndex = 68;
			this.label12.Text = "Password";
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.label13);
			this.groupBox3.Controls.Add(this.DeleteTaskButton);
			this.groupBox3.Controls.Add(this.createScheduleCheckBox);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.dayIntervalTextBox);
			this.groupBox3.Controls.Add(this.minutesTextBox);
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Controls.Add(this.hoursTextBox);
			this.groupBox3.Controls.Add(this.label11);
			this.groupBox3.Controls.Add(this.label12);
			this.groupBox3.Controls.Add(this.PasswordTextBox);
			this.groupBox3.Controls.Add(this.UserTextBox);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(8, 312);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(440, 104);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Windows Task Scheduler Settings";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 80);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(415, 16);
			this.label13.TabIndex = 70;
			this.label13.Text = "NOTE: Windows will not run a task if the user does not have a password assigned";
			// 
			// DeleteTaskButton
			// 
			this.DeleteTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteTaskButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteTaskButton.Location = new System.Drawing.Point(352, 24);
			this.DeleteTaskButton.Name = "DeleteTaskButton";
			this.DeleteTaskButton.Size = new System.Drawing.Size(74, 20);
			this.DeleteTaskButton.TabIndex = 4;
			this.DeleteTaskButton.Text = "Delete Task";
			this.DeleteTaskButton.Click += new System.EventHandler(this.DeleteTaskButton_Click);
			// 
			// TVProgramGuide
			// 
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "TVProgramGuide";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    private void SetupGrabbers()
    {
      GrabbercomboBox.Items.Add("tv_grab_de_tvtoday");
      GrabbercomboBox.Items.Add("tv_grab_dk");
      GrabbercomboBox.Items.Add("tv_grab_es");
      GrabbercomboBox.Items.Add("tv_grab_es_digital");
      GrabbercomboBox.Items.Add("tv_grab_fi");
      GrabbercomboBox.Items.Add("tv_grab_fr");
      GrabbercomboBox.Items.Add("tv_grab_huro");
      GrabbercomboBox.Items.Add("tv_grab_it");
      GrabbercomboBox.Items.Add("tv_grab_it_lt");
      GrabbercomboBox.Items.Add("tv_grab_na_dd");
      GrabbercomboBox.Items.Add("tv_grab_nl");
      GrabbercomboBox.Items.Add("tv_grab_nl_wolf");
      GrabbercomboBox.Items.Add("tv_grab_no");
      GrabbercomboBox.Items.Add("tv_grab_pt");
      GrabbercomboBox.Items.Add("tv_grab_se");
      GrabbercomboBox.Items.Add("tv_grab_se_swedb");
      GrabbercomboBox.Items.Add("tv_grab_uk_bleb");
      GrabbercomboBox.Items.Add("tv_grab_uk_rt");
      GrabbercomboBox.Items.Add("TVguide.xml File");
    }

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				useColorCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "colors", false);

				useTimeZoneCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "usetimezone", true);
        OldTimeZoneCompensation=useTimeZoneCheckBox.Checked;
        OldTimeZoneOffsetHours=xmlreader.GetValueAsInt("xmltv", "timezonecorrectionhours", 0);
        OldTimeZoneOffsetMins=xmlreader.GetValueAsInt("xmltv", "timezonecorrectionmins", 0);

        compensateTextBox.Text = OldTimeZoneOffsetHours.ToString();
        textBoxMinutes.Text = OldTimeZoneOffsetMins.ToString();

        string strDir=System.IO.Directory.GetCurrentDirectory();
        strDir+=@"\xmltv";
				folderNameTextBox.Text = xmlreader.GetValueAsString("xmltv", "folder", strDir);

				GrabbercomboBox.SelectedItem = xmlreader.GetValueAsString("xmltv","grabber","");
				AdvancedDaystextBox.Text=xmlreader.GetValueAsString("xmltv","days","1,2,3,5");
				parametersTextBox.Text=xmlreader.GetValueAsString("xmltv","args","");
				daysToKeepTextBox.Text = xmlreader.GetValueAsString("xmltv","daystokeep", "7");
				advancedRadioButton.Checked = xmlreader.GetValueAsBool("xmltv", "advanced", false);
				basicRadioButton.Checked = !advancedRadioButton.Checked;
        btnUpdateTvGuide.Enabled =useTimeZoneCheckBox.Checked;
			}						
      short[] taskSettings = new short[3];
      string userAccount = null;
      int index = 0;
      bool taskExists = TaskScheduler.GetTask(ref taskSettings, ref userAccount);
      hoursTextBox.Text = taskSettings[0].ToString();
      if (hoursTextBox.Text.Length==1) hoursTextBox.Text="0"+taskSettings[0];
      minutesTextBox.Text = taskSettings[1].ToString();
      if (minutesTextBox.Text.Length==1) minutesTextBox.Text="0"+taskSettings[1];
      dayIntervalTextBox.Text = taskSettings[2].ToString();
      if (userAccount != null && userAccount != "")
      {  
        index = userAccount.IndexOf(@"\");
        if (index > 0) index++;
        UserTextBox.Text=userAccount.Substring(index);
      }
      if (taskExists)
      {
        DeleteTaskButton.Enabled=true;
      }
      else
      {
        DeleteTaskButton.Enabled=false;
      }
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("xmltv", "colors", useColorCheckBox.Checked);
				xmlwriter.SetValueAsBool("xmltv", "usetimezone", useTimeZoneCheckBox.Checked);

				xmlwriter.SetValue("xmltv", "timezonecorrectionhours", compensateTextBox.Text);
        xmlwriter.SetValue("xmltv", "timezonecorrectionmins", textBoxMinutes.Text);
				xmlwriter.SetValue("xmltv", "folder", folderNameTextBox.Text);

				xmlwriter.SetValue("xmltv", "grabber",GrabbercomboBox.Text);
				xmlwriter.SetValue("xmltv", "daystokeep",daysToKeepTextBox.Text);
				xmlwriter.SetValueAsBool("xmltv", "advanced", advancedRadioButton.Checked);
				xmlwriter.SetValue("xmltv", "days",AdvancedDaystextBox.Text);
				xmlwriter.SetValue("xmltv", "args",parametersTextBox.Text);
			}

      if (createScheduleCheckBox.Checked)
      {
        int hours=0,minutes=0,days=1;
        try
        {
          hours=System.Convert.ToInt32(hoursTextBox.Text);
          minutes=System.Convert.ToInt32(minutesTextBox.Text);
          days=System.Convert.ToInt32(dayIntervalTextBox.Text);

        }
        catch(Exception)
        {
        }
        if (hours>23) hours=23;
        if (hours<0) hours=0;
        if (minutes>59) minutes=59;
        if (minutes<0) minutes=0;
        if (days < 1) days=1;

        TaskScheduler.CreateTask( (short)hours,
                                  (short)minutes,
                                  (short)days,
                                  UserTextBox.Text,PasswordTextBox.Text);
      }
		}

		private void compensateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
      //
      // Allow only numbers, '-' and backspace.
      //
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != '-')
			{
				e.Handled = true;
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where the XMLTV data is stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}							
		}

    private void daysToKeepTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, '-' and backspace.
      //
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }    
    }

    private void GrabbercomboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      basicRadioButton.Enabled = advancedRadioButton.Enabled = AdvancedDaystextBox.Enabled = daysToKeepTextBox.Enabled = parametersButton.Enabled = parametersTextBox.Enabled = (GrabbercomboBox.SelectedItem != null); 
      parametersTextBox.Text="";

      if(GrabbercomboBox.Text=="tv_grab_fi") 
        daysToKeepTextBox.Text="10";
      else if(GrabbercomboBox.Text=="tv_grab_uk_rt") 
        daysToKeepTextBox.Text="14";
      else if(GrabbercomboBox.Text=="tv_grab_huro") 
        daysToKeepTextBox.Text="8";
      else if((GrabbercomboBox.Text=="tv_grab_es")|(GrabbercomboBox.Text=="tv_grab_es_digital")|(GrabbercomboBox.Text=="tv_grab_pt")) 
        daysToKeepTextBox.Text="3";
      else if((GrabbercomboBox.Text=="tv_grab_se")|(GrabbercomboBox.Text=="tv_grab_se_swedb"))
        daysToKeepTextBox.Text="5";
      else 
        daysToKeepTextBox.Text="7";

      if(advancedRadioButton.Enabled == true || basicRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = advancedRadioButton.Checked;
        daysToKeepTextBox.Enabled = basicRadioButton.Checked;
      }
      if(GrabbercomboBox.Text=="TVguide.xml File")
      {
        advancedRadioButton.Enabled = false;
        basicRadioButton.Enabled = false;
        AdvancedDaystextBox.Enabled = false;
        daysToKeepTextBox.Enabled = false;
        parametersTextBox.Enabled = false;
        parametersButton.Enabled =false;
      }    
    }

    private void GrabbercomboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == (char)System.Windows.Forms.Keys.Delete || e.KeyChar == (char)System.Windows.Forms.Keys.Back)
      {
        GrabbercomboBox.SelectedItem = null;
        GrabbercomboBox.Text = String.Empty;
      }        
    }

    private void parametersButton_Click(object sender, System.EventArgs e)
    {
		  parametersTextBox.Text="";
		  ParameterForm parameters = new ParameterForm();
			
      if(GrabbercomboBox.Text==("tv_grab_dk") | GrabbercomboBox.Text==("tv_grab_es") | GrabbercomboBox.Text==("tv_grab_es_digital")
        | GrabbercomboBox.Text==("tv_grab_fi") | GrabbercomboBox.Text==("tv_grab_huro") | GrabbercomboBox.Text==("tv_grab_no")
        | GrabbercomboBox.Text==("tv_grab_pt") | GrabbercomboBox.Text==("tv_grab_se") | GrabbercomboBox.Text==("tv_grab_nl_wolf")
        |(GrabbercomboBox.Text=="tv_grab_se_swedb")|(GrabbercomboBox.Text=="tv_grab_uk_bleb")|(GrabbercomboBox.Text=="tv_grab_uk_rt"))
      {
        parameters.AddParameter("", "No options available for this grabber");
      }
      else if(GrabbercomboBox.Text==("tv_grab_fr") | GrabbercomboBox.Text==("tv_grab_it") | GrabbercomboBox.Text==("tv_grab_nl"))
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
      }
      else if(GrabbercomboBox.Text=="tv_grab_de_tvtoday")
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--nosqueezeout", "Don't parse program descriptions for adiitional information (actors,director,etc");
        parameters.AddParameter("--slow --nosqueezeout", "Fetch full program details and don't parse descriptions");
      }
      else if(GrabbercomboBox.Text=="tv_grab_na_dd")
      {
        parameters.AddParameter("--auto-config add", "Appends new channels to the config file");
        parameters.AddParameter("--auto-config ignore", "Ignore new channels");
        parameters.AddParameter("--old-chan-id", "Use old tv_grab_na style channel ids");
        parameters.AddParameter("--old-chan-id --auto-config add", "Old tv_grab_na style channel ids and append new channels");
        parameters.AddParameter("--old-chan-id --auto-config ignore", "Old tv_grab_na style channel ids and ignore new channels");
      }
      else if(GrabbercomboBox.Text=="tv_grab_it_lt")
      {
        parameters.AddParameter("--password-file", "Use password file - tv_grab_it_lt_password.txt in XMLTV folder");
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--slow --password-file", "Use password file  - tv_grab_it_lt_password.txt in XMLTV folder and fetch full program details");
      }

      if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    private void AdvancedDaystextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == ',')
      {
        if(AdvancedDaystextBox.Text.EndsWith(","))
        {
          e.Handled = true;
          return;
        }
      }

      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != ',')
      {
        e.Handled = true;
      }        
    }

    protected void basicRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      if(basicRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = false;
        daysToKeepTextBox.Enabled = true;
      }
    }

    protected void advancedRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      if(advancedRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = true;
        daysToKeepTextBox.Enabled = false;
      }
    }
    protected void createScheduleCheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
      if (createScheduleCheckBox.Checked)
      {
        hoursTextBox.Enabled = true;
        minutesTextBox.Enabled = true;
        dayIntervalTextBox.Enabled = true;
        UserTextBox.Enabled = true;
        PasswordTextBox.Enabled = true;
      }
      else
      {
        hoursTextBox.Enabled = false;
        minutesTextBox.Enabled = false;
        dayIntervalTextBox.Enabled = false;
        UserTextBox.Enabled = false;
        PasswordTextBox.Enabled = false;
      }
    }
    private void DeleteTaskButton_Click(object sender, System.EventArgs e)
    {
      TaskScheduler.DeleteTask();
      hoursTextBox.Text="01";
      minutesTextBox.Text="00";
      dayIntervalTextBox.Text="1";
      UserTextBox.Text="";
      PasswordTextBox.Text="";
      createScheduleCheckBox.Checked=false;
      DeleteTaskButton.Enabled=false;
    }
    private void dayIntervalTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }
    private void hoursTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }
    private void minutesTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void RunGrabberButton_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      if((File.Exists(folderNameTextBox.Text + @"\xmltv.exe"))|(GrabbercomboBox.Text=="TVguide.xml File"))
      {
        SetupGrabber.LaunchGuideScheduler();
      }
      else
      {
        MessageBox.Show("XMLTV.exe cannot be found in the directory you have setup as the XMLTV folder."+ "\n\n" +"Ensure that you have installed the XMLTV application, and that the XMLTV folder" + "\n" + "setting points to the directory where XMLTV.exe is installed" +"\n"+"XMLTV can be downloaded from http://sourceforge.net/projects/xmltv",
                        "MediaPortal Configuration",MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void groupBox2_Enter(object sender, System.EventArgs e)
    {
    
    }

    private void btnUpdateTvGuide_Click(object sender, System.EventArgs e)
    {
      if (!useTimeZoneCheckBox.Checked ) return;
      try
      {
        int iNewTimeZoneCompensationHours=Int32.Parse(compensateTextBox.Text);
        int iNewTimeZoneCompensationMins=Int32.Parse(textBoxMinutes.Text);
        if (iNewTimeZoneCompensationHours==OldTimeZoneOffsetHours &&
            iNewTimeZoneCompensationMins==OldTimeZoneOffsetMins) return;
        int oldoffset=OldTimeZoneOffsetHours*60 + OldTimeZoneOffsetMins;
        int newoffset=iNewTimeZoneCompensationHours*60 + iNewTimeZoneCompensationMins;

        int offset=newoffset-oldoffset;

        TVDatabase.OffsetProgramsByMinutes(offset);
        OldTimeZoneOffsetHours=iNewTimeZoneCompensationHours;
        OldTimeZoneOffsetMins=iNewTimeZoneCompensationMins;
        MessageBox.Show("TVDatabase is updated with new timezone offset",
          "MediaPortal Configuration",MessageBoxButtons.OK, MessageBoxIcon.Information);
        SaveSettings();

      }
      catch(Exception)
      {
      }
    }

    private void useTimeZoneCheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
      btnUpdateTvGuide.Enabled=useTimeZoneCheckBox.Checked;
    }

    private void btnClearTVDatabase_Click(object sender, System.EventArgs e)
    {
      TVDatabase.RemovePrograms();
      MessageBox.Show("All programs are removed from the tv database",
        "MediaPortal Configuration",MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void label15_Click(object sender, System.EventArgs e)
    {
    
    }
	}
}

