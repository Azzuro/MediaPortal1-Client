namespace SetupTv.Sections
{
  partial class TvRecording
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvRecording));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tpSettings = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBoxRecording = new System.Windows.Forms.GroupBox();
      this.label48 = new System.Windows.Forms.Label();
      this.numericUpDownMaxFreeCardsToTry = new System.Windows.Forms.NumericUpDown();
      this.lblMaxFreeCards = new System.Windows.Forms.Label();
      this.groupBoxScheduler = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblWeekend = new System.Windows.Forms.Label();
      this.comboBoxWeekend = new System.Windows.Forms.ComboBox();
      this.checkBoxPreventDupes = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxEpisodeKey = new System.Windows.Forms.ComboBox();
      this.labelEpisodeKey = new System.Windows.Forms.Label();
      this.groupBoxGap = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.numericUpDownPostRec = new System.Windows.Forms.NumericUpDown();
      this.label3 = new System.Windows.Forms.Label();
      this.numericUpDownPreRec = new System.Windows.Forms.NumericUpDown();
      this.label4 = new System.Windows.Forms.Label();
      this.checkBoxAutoDelete = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tpCustom = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.labelCustomFormat = new System.Windows.Forms.Label();
      this.textBoxCustomFormat = new System.Windows.Forms.TextBox();
      this.comboBoxFormat = new System.Windows.Forms.ComboBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSample = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.comboBoxMovies = new System.Windows.Forms.ComboBox();
      this.tpDiskQuota = new System.Windows.Forms.TabPage();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.enableDiskQuota = new System.Windows.Forms.CheckBox();
      this.mpNumericTextBoxDiskQuota = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.label14 = new System.Windows.Forms.Label();
      this.comboBoxDrive = new System.Windows.Forms.ComboBox();
      this.labelFreeDiskspace = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.labelTotalDiskSpace = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.tpRecording = new System.Windows.Forms.TabPage();
      this.groupBoxRecordSettings = new System.Windows.Forms.GroupBox();
      this.buttonSameRecFolder = new System.Windows.Forms.Button();
      this.textBoxRecordingFormat = new System.Windows.Forms.TextBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.comboBoxCards = new System.Windows.Forms.ComboBox();
      this.label23 = new System.Windows.Forms.Label();
      this.textBoxFolder = new System.Windows.Forms.TextBox();
      this.label13 = new System.Windows.Forms.Label();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.tpRecordImport = new System.Windows.Forms.TabPage();
      this.buttonChangeChannel = new System.Windows.Forms.Button();
      this.btnRemoveInvalidFiles = new System.Windows.Forms.Button();
      this.btnImport = new System.Windows.Forms.Button();
      this.lblImportItems = new System.Windows.Forms.Label();
      this.tvTagRecs = new System.Windows.Forms.TreeView();
      this.lblRecFolders = new System.Windows.Forms.Label();
      this.cbRecPaths = new System.Windows.Forms.ComboBox();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabControl1.SuspendLayout();
      this.tpSettings.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBoxRecording.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFreeCardsToTry)).BeginInit();
      this.groupBoxScheduler.SuspendLayout();
      this.groupBoxGap.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPostRec)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreRec)).BeginInit();
      this.tpCustom.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tpDiskQuota.SuspendLayout();
      this.tpRecording.SuspendLayout();
      this.groupBoxRecordSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.tpRecordImport.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tpSettings);
      this.tabControl1.Controls.Add(this.tpCustom);
      this.tabControl1.Controls.Add(this.tpDiskQuota);
      this.tabControl1.Controls.Add(this.tpRecording);
      this.tabControl1.Controls.Add(this.tpRecordImport);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(471, 527);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tpSettings
      // 
      this.tpSettings.Controls.Add(this.groupBox1);
      this.tpSettings.Location = new System.Drawing.Point(4, 22);
      this.tpSettings.Name = "tpSettings";
      this.tpSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tpSettings.Size = new System.Drawing.Size(463, 501);
      this.tpSettings.TabIndex = 0;
      this.tpSettings.Text = "General";
      this.tpSettings.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBoxRecording);
      this.groupBox1.Controls.Add(this.groupBoxScheduler);
      this.groupBox1.Controls.Add(this.groupBoxGap);
      this.groupBox1.Controls.Add(this.checkBoxAutoDelete);
      this.groupBox1.Location = new System.Drawing.Point(6, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(451, 492);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // groupBoxRecording
      // 
      this.groupBoxRecording.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRecording.Controls.Add(this.label48);
      this.groupBoxRecording.Controls.Add(this.numericUpDownMaxFreeCardsToTry);
      this.groupBoxRecording.Controls.Add(this.lblMaxFreeCards);
      this.groupBoxRecording.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxRecording.Location = new System.Drawing.Point(15, 19);
      this.groupBoxRecording.Name = "groupBoxRecording";
      this.groupBoxRecording.Size = new System.Drawing.Size(418, 62);
      this.groupBoxRecording.TabIndex = 18;
      this.groupBoxRecording.TabStop = false;
      this.groupBoxRecording.Text = "Recording";
      // 
      // label48
      // 
      this.label48.AutoSize = true;
      this.label48.Location = new System.Drawing.Point(246, 29);
      this.label48.Name = "label48";
      this.label48.Size = new System.Drawing.Size(29, 13);
      this.label48.TabIndex = 37;
      this.label48.Text = "0=all";
      // 
      // numericUpDownMaxFreeCardsToTry
      // 
      this.numericUpDownMaxFreeCardsToTry.Location = new System.Drawing.Point(153, 26);
      this.numericUpDownMaxFreeCardsToTry.Name = "numericUpDownMaxFreeCardsToTry";
      this.numericUpDownMaxFreeCardsToTry.Size = new System.Drawing.Size(87, 20);
      this.numericUpDownMaxFreeCardsToTry.TabIndex = 36;
      this.numericUpDownMaxFreeCardsToTry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // lblMaxFreeCards
      // 
      this.lblMaxFreeCards.AutoSize = true;
      this.lblMaxFreeCards.Location = new System.Drawing.Point(6, 28);
      this.lblMaxFreeCards.Name = "lblMaxFreeCards";
      this.lblMaxFreeCards.Size = new System.Drawing.Size(127, 13);
      this.lblMaxFreeCards.TabIndex = 35;
      this.lblMaxFreeCards.Text = "Maximum free cards to try";
      // 
      // groupBoxScheduler
      // 
      this.groupBoxScheduler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxScheduler.Controls.Add(this.lblWeekend);
      this.groupBoxScheduler.Controls.Add(this.comboBoxWeekend);
      this.groupBoxScheduler.Controls.Add(this.checkBoxPreventDupes);
      this.groupBoxScheduler.Controls.Add(this.comboBoxEpisodeKey);
      this.groupBoxScheduler.Controls.Add(this.labelEpisodeKey);
      this.groupBoxScheduler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScheduler.Location = new System.Drawing.Point(15, 180);
      this.groupBoxScheduler.Name = "groupBoxScheduler";
      this.groupBoxScheduler.Size = new System.Drawing.Size(418, 142);
      this.groupBoxScheduler.TabIndex = 14;
      this.groupBoxScheduler.TabStop = false;
      this.groupBoxScheduler.Text = "Scheduler";
      // 
      // lblWeekend
      // 
      this.lblWeekend.AutoSize = true;
      this.lblWeekend.Location = new System.Drawing.Point(15, 103);
      this.lblWeekend.Name = "lblWeekend";
      this.lblWeekend.Size = new System.Drawing.Size(57, 13);
      this.lblWeekend.TabIndex = 20;
      this.lblWeekend.Text = "Weekend:";
      // 
      // comboBoxWeekend
      // 
      this.comboBoxWeekend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxWeekend.FormattingEnabled = true;
      this.comboBoxWeekend.Items.AddRange(new object[] {
            "Saturday/Sunday",
            "Sunday/Monday",
            "Monday/Tuesday",
            "Tuesday/Wednesday",
            "Wednesday/Thursday",
            "Thursday/Friday",
            "Friday/Saturday"});
      this.comboBoxWeekend.Location = new System.Drawing.Point(180, 103);
      this.comboBoxWeekend.Name = "comboBoxWeekend";
      this.comboBoxWeekend.Size = new System.Drawing.Size(137, 21);
      this.comboBoxWeekend.TabIndex = 15;
      this.comboBoxWeekend.SelectedIndexChanged += new System.EventHandler(this.comboBoxWeekend_SelectedIndexChanged);
      // 
      // checkBoxPreventDupes
      // 
      this.checkBoxPreventDupes.AutoSize = true;
      this.checkBoxPreventDupes.Checked = true;
      this.checkBoxPreventDupes.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxPreventDupes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPreventDupes.Location = new System.Drawing.Point(15, 28);
      this.checkBoxPreventDupes.Name = "checkBoxPreventDupes";
      this.checkBoxPreventDupes.Size = new System.Drawing.Size(335, 30);
      this.checkBoxPreventDupes.TabIndex = 13;
      this.checkBoxPreventDupes.Text = "Check episode of already recorded programs to prevent duplicate \r\nrecording of re" +
          "peated broadcasts";
      this.checkBoxPreventDupes.UseVisualStyleBackColor = true;
      this.checkBoxPreventDupes.CheckedChanged += new System.EventHandler(this.checkBoxPreventDupes_CheckedChanged);
      // 
      // comboBoxEpisodeKey
      // 
      this.comboBoxEpisodeKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxEpisodeKey.Enabled = this.checkBoxPreventDupes.Checked;
      this.comboBoxEpisodeKey.FormattingEnabled = true;
      this.comboBoxEpisodeKey.Items.AddRange(new object[] {
            "Episode Title",
            "Episode Number"});
      this.comboBoxEpisodeKey.Location = new System.Drawing.Point(180, 65);
      this.comboBoxEpisodeKey.Name = "comboBoxEpisodeKey";
      this.comboBoxEpisodeKey.Size = new System.Drawing.Size(137, 21);
      this.comboBoxEpisodeKey.TabIndex = 13;
      // 
      // labelEpisodeKey
      // 
      this.labelEpisodeKey.AutoSize = true;
      this.labelEpisodeKey.Location = new System.Drawing.Point(12, 68);
      this.labelEpisodeKey.Name = "labelEpisodeKey";
      this.labelEpisodeKey.Size = new System.Drawing.Size(71, 13);
      this.labelEpisodeKey.TabIndex = 1;
      this.labelEpisodeKey.Text = "Episode key: ";
      // 
      // groupBoxGap
      // 
      this.groupBoxGap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGap.Controls.Add(this.label1);
      this.groupBoxGap.Controls.Add(this.label2);
      this.groupBoxGap.Controls.Add(this.numericUpDownPostRec);
      this.groupBoxGap.Controls.Add(this.label3);
      this.groupBoxGap.Controls.Add(this.numericUpDownPreRec);
      this.groupBoxGap.Controls.Add(this.label4);
      this.groupBoxGap.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGap.Location = new System.Drawing.Point(15, 87);
      this.groupBoxGap.Name = "groupBoxGap";
      this.groupBoxGap.Size = new System.Drawing.Size(418, 84);
      this.groupBoxGap.TabIndex = 13;
      this.groupBoxGap.TabStop = false;
      this.groupBoxGap.Text = "Pre- and post recording";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Start recording";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(76, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Stop recording";
      // 
      // numericUpDownPostRec
      // 
      this.numericUpDownPostRec.Location = new System.Drawing.Point(94, 54);
      this.numericUpDownPostRec.Name = "numericUpDownPostRec";
      this.numericUpDownPostRec.Size = new System.Drawing.Size(66, 20);
      this.numericUpDownPostRec.TabIndex = 11;
      this.numericUpDownPostRec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownPostRec.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(166, 26);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(151, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "minute(s) before program starts";
      // 
      // numericUpDownPreRec
      // 
      this.numericUpDownPreRec.Location = new System.Drawing.Point(94, 24);
      this.numericUpDownPreRec.Name = "numericUpDownPreRec";
      this.numericUpDownPreRec.Size = new System.Drawing.Size(66, 20);
      this.numericUpDownPreRec.TabIndex = 10;
      this.numericUpDownPreRec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownPreRec.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(166, 56);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(140, 13);
      this.label4.TabIndex = 5;
      this.label4.Text = "minute(s) after program ends";
      // 
      // checkBoxAutoDelete
      // 
      this.checkBoxAutoDelete.AutoSize = true;
      this.checkBoxAutoDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAutoDelete.Location = new System.Drawing.Point(30, 328);
      this.checkBoxAutoDelete.Name = "checkBoxAutoDelete";
      this.checkBoxAutoDelete.Size = new System.Drawing.Size(252, 17);
      this.checkBoxAutoDelete.TabIndex = 6;
      this.checkBoxAutoDelete.Text = "Automatically delete a recording after watching it";
      this.checkBoxAutoDelete.UseVisualStyleBackColor = true;
      this.checkBoxAutoDelete.Visible = false;
      // 
      // tpCustom
      // 
      this.tpCustom.Controls.Add(this.groupBox2);
      this.tpCustom.Location = new System.Drawing.Point(4, 22);
      this.tpCustom.Name = "tpCustom";
      this.tpCustom.Padding = new System.Windows.Forms.Padding(3);
      this.tpCustom.Size = new System.Drawing.Size(463, 501);
      this.tpCustom.TabIndex = 1;
      this.tpCustom.Text = "Custom paths and filenames";
      this.tpCustom.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.AutoSize = true;
      this.groupBox2.Controls.Add(this.labelCustomFormat);
      this.groupBox2.Controls.Add(this.textBoxCustomFormat);
      this.groupBox2.Controls.Add(this.comboBoxFormat);
      this.groupBox2.Controls.Add(this.groupBox3);
      this.groupBox2.Controls.Add(this.textBoxSample);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.comboBoxMovies);
      this.groupBox2.Location = new System.Drawing.Point(6, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(448, 374);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      // 
      // labelCustomFormat
      // 
      this.labelCustomFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelCustomFormat.AutoSize = true;
      this.labelCustomFormat.Location = new System.Drawing.Point(85, 124);
      this.labelCustomFormat.Name = "labelCustomFormat";
      this.labelCustomFormat.Size = new System.Drawing.Size(323, 13);
      this.labelCustomFormat.TabIndex = 11;
      this.labelCustomFormat.Text = "Use blockquotes[] to specify optional fields and \\\\ for relative paths";
      // 
      // textBoxCustomFormat
      // 
      this.textBoxCustomFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxCustomFormat.Location = new System.Drawing.Point(88, 101);
      this.textBoxCustomFormat.Name = "textBoxCustomFormat";
      this.textBoxCustomFormat.Size = new System.Drawing.Size(331, 20);
      this.textBoxCustomFormat.TabIndex = 10;
      this.textBoxCustomFormat.TextChanged += new System.EventHandler(this.textBoxCustomFormat_TextChanged);
      this.textBoxCustomFormat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxCustomFormat_KeyPress);
      // 
      // comboBoxFormat
      // 
      this.comboBoxFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFormat.FormattingEnabled = true;
      this.comboBoxFormat.Location = new System.Drawing.Point(88, 74);
      this.comboBoxFormat.Name = "comboBoxFormat";
      this.comboBoxFormat.Size = new System.Drawing.Size(331, 21);
      this.comboBoxFormat.TabIndex = 9;
      this.comboBoxFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxFormat_SelectedIndexChanged);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.mpLabel1);
      this.groupBox3.Controls.Add(this.mpLabel2);
      this.groupBox3.Controls.Add(this.mpLabel3);
      this.groupBox3.Controls.Add(this.mpLabel4);
      this.groupBox3.Location = new System.Drawing.Point(29, 183);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(390, 165);
      this.groupBox3.TabIndex = 7;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Available tags";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(284, 22);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(66, 130);
      this.mpLabel1.TabIndex = 19;
      this.mpLabel1.Text = "start day\r\nstart month\r\nstart year\r\nstart hours\r\nstart minutes\r\nend day\r\nend mont" +
          "h\r\nend year\r\nend hours\r\nend minutes";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(94, 22);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(82, 130);
      this.mpLabel2.TabIndex = 17;
      this.mpLabel2.Text = "channel name\r\ntitle\r\nepisode name\r\ngenre\r\nseries number\r\nepisode number\r\nepisode " +
          "part\r\ndate\r\nstart time\r\nend time";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(20, 22);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(70, 130);
      this.mpLabel3.TabIndex = 16;
      this.mpLabel3.Text = "%channel% =\r\n%title% =\r\n%name% =\r\n%genre% =\r\n%series% =\r\n%episode% =\r\n%part% =\r\n%" +
          "date% =\r\n%start% =\r\n%end% =\r\n";
      this.mpLabel3.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(196, 22);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(81, 130);
      this.mpLabel4.TabIndex = 18;
      this.mpLabel4.Text = "%startday% =\r\n%startmonth% =\r\n%startyear% =\r\n%starthh% =\r\n%startmm% =\r\n%endday% =" +
          "\r\n%endmonth% =\r\n%endyear% =\r\n%endhh% =\r\n%endmm% =\r\n";
      this.mpLabel4.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxSample
      // 
      this.textBoxSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSample.Enabled = false;
      this.textBoxSample.Location = new System.Drawing.Point(88, 148);
      this.textBoxSample.Name = "textBoxSample";
      this.textBoxSample.ReadOnly = true;
      this.textBoxSample.Size = new System.Drawing.Size(331, 20);
      this.textBoxSample.TabIndex = 6;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(27, 151);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(42, 13);
      this.label8.TabIndex = 5;
      this.label8.Text = "Sample";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(27, 77);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(39, 13);
      this.label7.TabIndex = 4;
      this.label7.Text = "Format";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(27, 27);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(56, 13);
      this.label5.TabIndex = 1;
      this.label5.Text = "Recording";
      // 
      // comboBoxMovies
      // 
      this.comboBoxMovies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxMovies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMovies.FormattingEnabled = true;
      this.comboBoxMovies.Items.AddRange(new object[] {
            "Movies  (manual or single type recordings)",
            "Series"});
      this.comboBoxMovies.Location = new System.Drawing.Point(88, 24);
      this.comboBoxMovies.Name = "comboBoxMovies";
      this.comboBoxMovies.Size = new System.Drawing.Size(331, 21);
      this.comboBoxMovies.TabIndex = 0;
      this.comboBoxMovies.SelectedIndexChanged += new System.EventHandler(this.comboBoxMovies_SelectedIndexChanged);
      // 
      // tpDiskQuota
      // 
      this.tpDiskQuota.Controls.Add(this.mpLabel5);
      this.tpDiskQuota.Controls.Add(this.enableDiskQuota);
      this.tpDiskQuota.Controls.Add(this.mpNumericTextBoxDiskQuota);
      this.tpDiskQuota.Controls.Add(this.label14);
      this.tpDiskQuota.Controls.Add(this.comboBoxDrive);
      this.tpDiskQuota.Controls.Add(this.labelFreeDiskspace);
      this.tpDiskQuota.Controls.Add(this.label9);
      this.tpDiskQuota.Controls.Add(this.labelTotalDiskSpace);
      this.tpDiskQuota.Controls.Add(this.label10);
      this.tpDiskQuota.Controls.Add(this.label11);
      this.tpDiskQuota.Location = new System.Drawing.Point(4, 22);
      this.tpDiskQuota.Name = "tpDiskQuota";
      this.tpDiskQuota.Size = new System.Drawing.Size(463, 501);
      this.tpDiskQuota.TabIndex = 2;
      this.tpDiskQuota.Text = "Disk quota";
      this.tpDiskQuota.UseVisualStyleBackColor = true;
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(302, 163);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(79, 13);
      this.mpLabel5.TabIndex = 10;
      this.mpLabel5.Text = "MB free space.";
      // 
      // enableDiskQuota
      // 
      this.enableDiskQuota.AccessibleName = "";
      this.enableDiskQuota.AutoSize = true;
      this.enableDiskQuota.Location = new System.Drawing.Point(30, 75);
      this.enableDiskQuota.Name = "enableDiskQuota";
      this.enableDiskQuota.Size = new System.Drawing.Size(111, 17);
      this.enableDiskQuota.TabIndex = 11;
      this.enableDiskQuota.Text = "Enable disk quota";
      this.enableDiskQuota.UseVisualStyleBackColor = true;
      this.enableDiskQuota.CheckedChanged += new System.EventHandler(this.enableDiskQuota_CheckedChanged);
      // 
      // mpNumericTextBoxDiskQuota
      // 
      this.mpNumericTextBoxDiskQuota.Location = new System.Drawing.Point(234, 160);
      this.mpNumericTextBoxDiskQuota.Name = "mpNumericTextBoxDiskQuota";
      this.mpNumericTextBoxDiskQuota.Size = new System.Drawing.Size(62, 20);
      this.mpNumericTextBoxDiskQuota.TabIndex = 9;
      this.mpNumericTextBoxDiskQuota.Text = "13";
      this.mpNumericTextBoxDiskQuota.Value = 13;
      this.mpNumericTextBoxDiskQuota.Leave += new System.EventHandler(this.mpNumericTextBoxDiskQuota_Leave);
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(27, 163);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(201, 13);
      this.label14.TabIndex = 6;
      this.label14.Text = "Delete recordings when there is less than";
      // 
      // comboBoxDrive
      // 
      this.comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDrive.FormattingEnabled = true;
      this.comboBoxDrive.Location = new System.Drawing.Point(30, 43);
      this.comboBoxDrive.Name = "comboBoxDrive";
      this.comboBoxDrive.Size = new System.Drawing.Size(355, 21);
      this.comboBoxDrive.TabIndex = 1;
      this.comboBoxDrive.SelectedIndexChanged += new System.EventHandler(this.comboBoxDrive_SelectedIndexChanged);
      // 
      // labelFreeDiskspace
      // 
      this.labelFreeDiskspace.AutoSize = true;
      this.labelFreeDiskspace.Location = new System.Drawing.Point(147, 134);
      this.labelFreeDiskspace.Name = "labelFreeDiskspace";
      this.labelFreeDiskspace.Size = new System.Drawing.Size(93, 13);
      this.labelFreeDiskspace.TabIndex = 5;
      this.labelFreeDiskspace.Text = "Checking space...";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(27, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(35, 13);
      this.label9.TabIndex = 0;
      this.label9.Text = "Drive:";
      // 
      // labelTotalDiskSpace
      // 
      this.labelTotalDiskSpace.AutoSize = true;
      this.labelTotalDiskSpace.Location = new System.Drawing.Point(147, 116);
      this.labelTotalDiskSpace.Name = "labelTotalDiskSpace";
      this.labelTotalDiskSpace.Size = new System.Drawing.Size(93, 13);
      this.labelTotalDiskSpace.TabIndex = 4;
      this.labelTotalDiskSpace.Text = "Checking space...";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(27, 116);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(85, 13);
      this.label10.TabIndex = 2;
      this.label10.Tag = "";
      this.label10.Text = "Total diskspace:";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(27, 134);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(82, 13);
      this.label11.TabIndex = 3;
      this.label11.Tag = "";
      this.label11.Text = "Free diskspace:";
      // 
      // tpRecording
      // 
      this.tpRecording.Controls.Add(this.groupBoxRecordSettings);
      this.tpRecording.Location = new System.Drawing.Point(4, 22);
      this.tpRecording.Name = "tpRecording";
      this.tpRecording.Size = new System.Drawing.Size(463, 501);
      this.tpRecording.TabIndex = 3;
      this.tpRecording.Text = "Folders";
      this.tpRecording.UseVisualStyleBackColor = true;
      // 
      // groupBoxRecordSettings
      // 
      this.groupBoxRecordSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRecordSettings.AutoSize = true;
      this.groupBoxRecordSettings.Controls.Add(this.buttonSameRecFolder);
      this.groupBoxRecordSettings.Controls.Add(this.textBoxRecordingFormat);
      this.groupBoxRecordSettings.Controls.Add(this.pictureBox1);
      this.groupBoxRecordSettings.Controls.Add(this.comboBoxCards);
      this.groupBoxRecordSettings.Controls.Add(this.label23);
      this.groupBoxRecordSettings.Controls.Add(this.textBoxFolder);
      this.groupBoxRecordSettings.Controls.Add(this.label13);
      this.groupBoxRecordSettings.Controls.Add(this.buttonBrowse);
      this.groupBoxRecordSettings.Location = new System.Drawing.Point(6, 3);
      this.groupBoxRecordSettings.Name = "groupBoxRecordSettings";
      this.groupBoxRecordSettings.Size = new System.Drawing.Size(445, 365);
      this.groupBoxRecordSettings.TabIndex = 26;
      this.groupBoxRecordSettings.TabStop = false;
      this.groupBoxRecordSettings.Text = "Card settings";
      // 
      // buttonSameRecFolder
      // 
      this.buttonSameRecFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSameRecFolder.Location = new System.Drawing.Point(110, 258);
      this.buttonSameRecFolder.Name = "buttonSameRecFolder";
      this.buttonSameRecFolder.Size = new System.Drawing.Size(185, 20);
      this.buttonSameRecFolder.TabIndex = 28;
      this.buttonSameRecFolder.Text = "Same recording folder for all cards";
      this.buttonSameRecFolder.UseVisualStyleBackColor = true;
      this.buttonSameRecFolder.Click += new System.EventHandler(this.buttonSameRecFolder_Click);
      // 
      // textBoxRecordingFormat
      // 
      this.textBoxRecordingFormat.Location = new System.Drawing.Point(23, 109);
      this.textBoxRecordingFormat.Name = "textBoxRecordingFormat";
      this.textBoxRecordingFormat.ReadOnly = true;
      this.textBoxRecordingFormat.Size = new System.Drawing.Size(326, 20);
      this.textBoxRecordingFormat.TabIndex = 26;
      this.textBoxRecordingFormat.Text = " Transport Stream (.ts)";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(23, 43);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(33, 23);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 18;
      this.pictureBox1.TabStop = false;
      // 
      // comboBoxCards
      // 
      this.comboBoxCards.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCards.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCards.FormattingEnabled = true;
      this.comboBoxCards.Location = new System.Drawing.Point(74, 43);
      this.comboBoxCards.Name = "comboBoxCards";
      this.comboBoxCards.Size = new System.Drawing.Size(352, 21);
      this.comboBoxCards.TabIndex = 0;
      this.comboBoxCards.SelectedIndexChanged += new System.EventHandler(this.comboBoxCards_SelectedIndexChanged);
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Location = new System.Drawing.Point(20, 93);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(91, 13);
      this.label23.TabIndex = 23;
      this.label23.Text = "Recording format:";
      // 
      // textBoxFolder
      // 
      this.textBoxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFolder.Location = new System.Drawing.Point(23, 186);
      this.textBoxFolder.Name = "textBoxFolder";
      this.textBoxFolder.ReadOnly = true;
      this.textBoxFolder.Size = new System.Drawing.Size(326, 20);
      this.textBoxFolder.TabIndex = 2;
      this.textBoxFolder.TextChanged += new System.EventHandler(this.textBoxFolder_TextChanged);
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(20, 168);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(88, 13);
      this.label13.TabIndex = 3;
      this.label13.Text = "Recording folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(355, 186);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(51, 20);
      this.buttonBrowse.TabIndex = 4;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // tpRecordImport
      // 
      this.tpRecordImport.Controls.Add(this.buttonChangeChannel);
      this.tpRecordImport.Controls.Add(this.btnRemoveInvalidFiles);
      this.tpRecordImport.Controls.Add(this.btnImport);
      this.tpRecordImport.Controls.Add(this.lblImportItems);
      this.tpRecordImport.Controls.Add(this.tvTagRecs);
      this.tpRecordImport.Controls.Add(this.lblRecFolders);
      this.tpRecordImport.Controls.Add(this.cbRecPaths);
      this.tpRecordImport.Location = new System.Drawing.Point(4, 22);
      this.tpRecordImport.Name = "tpRecordImport";
      this.tpRecordImport.Padding = new System.Windows.Forms.Padding(3);
      this.tpRecordImport.Size = new System.Drawing.Size(463, 501);
      this.tpRecordImport.TabIndex = 4;
      this.tpRecordImport.Text = "Database import";
      this.tpRecordImport.UseVisualStyleBackColor = true;
      // 
      // buttonChangeChannel
      // 
      this.buttonChangeChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChangeChannel.Location = new System.Drawing.Point(171, 345);
      this.buttonChangeChannel.Name = "buttonChangeChannel";
      this.buttonChangeChannel.Size = new System.Drawing.Size(100, 23);
      this.buttonChangeChannel.TabIndex = 5;
      this.buttonChangeChannel.Text = "Change channel";
      this.buttonChangeChannel.UseVisualStyleBackColor = true;
      this.buttonChangeChannel.Click += new System.EventHandler(this.buttonChangeChannel_Click);
      // 
      // btnRemoveInvalidFiles
      // 
      this.btnRemoveInvalidFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRemoveInvalidFiles.Location = new System.Drawing.Point(298, 345);
      this.btnRemoveInvalidFiles.Name = "btnRemoveInvalidFiles";
      this.btnRemoveInvalidFiles.Size = new System.Drawing.Size(135, 23);
      this.btnRemoveInvalidFiles.TabIndex = 4;
      this.btnRemoveInvalidFiles.Text = "Remove invalid entries";
      this.btnRemoveInvalidFiles.UseVisualStyleBackColor = true;
      this.btnRemoveInvalidFiles.Click += new System.EventHandler(this.btnRemoveInvalidFiles_Click);
      // 
      // btnImport
      // 
      this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnImport.Location = new System.Drawing.Point(30, 345);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new System.Drawing.Size(135, 23);
      this.btnImport.TabIndex = 3;
      this.btnImport.Text = "Import selected files";
      this.btnImport.UseVisualStyleBackColor = true;
      this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
      // 
      // lblImportItems
      // 
      this.lblImportItems.AutoSize = true;
      this.lblImportItems.Location = new System.Drawing.Point(27, 75);
      this.lblImportItems.Name = "lblImportItems";
      this.lblImportItems.Size = new System.Drawing.Size(307, 13);
      this.lblImportItems.TabIndex = 4;
      this.lblImportItems.Text = "Recorded files which are currently not present in your database:";
      // 
      // tvTagRecs
      // 
      this.tvTagRecs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tvTagRecs.CheckBoxes = true;
      this.tvTagRecs.FullRowSelect = true;
      this.tvTagRecs.Location = new System.Drawing.Point(30, 94);
      this.tvTagRecs.Name = "tvTagRecs";
      this.tvTagRecs.ShowPlusMinus = false;
      this.tvTagRecs.ShowRootLines = false;
      this.tvTagRecs.Size = new System.Drawing.Size(403, 245);
      this.tvTagRecs.TabIndex = 2;
      this.tvTagRecs.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvTagRecs_AfterCheck);
      this.tvTagRecs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTagRecs_AfterSelect);
      // 
      // lblRecFolders
      // 
      this.lblRecFolders.AutoSize = true;
      this.lblRecFolders.Location = new System.Drawing.Point(27, 24);
      this.lblRecFolders.Name = "lblRecFolders";
      this.lblRecFolders.Size = new System.Drawing.Size(88, 13);
      this.lblRecFolders.TabIndex = 2;
      this.lblRecFolders.Text = "Recording folder:";
      // 
      // cbRecPaths
      // 
      this.cbRecPaths.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbRecPaths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbRecPaths.FormattingEnabled = true;
      this.cbRecPaths.Location = new System.Drawing.Point(30, 43);
      this.cbRecPaths.Name = "cbRecPaths";
      this.cbRecPaths.Size = new System.Drawing.Size(403, 21);
      this.cbRecPaths.Sorted = true;
      this.cbRecPaths.TabIndex = 1;
      this.cbRecPaths.SelectedIndexChanged += new System.EventHandler(this.cbRecPaths_SelectedIndexChanged);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // TvRecording
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvRecording";
      this.Size = new System.Drawing.Size(474, 530);
      this.tabControl1.ResumeLayout(false);
      this.tpSettings.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBoxRecording.ResumeLayout(false);
      this.groupBoxRecording.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxFreeCardsToTry)).EndInit();
      this.groupBoxScheduler.ResumeLayout(false);
      this.groupBoxScheduler.PerformLayout();
      this.groupBoxGap.ResumeLayout(false);
      this.groupBoxGap.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPostRec)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreRec)).EndInit();
      this.tpCustom.ResumeLayout(false);
      this.tpCustom.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tpDiskQuota.ResumeLayout(false);
      this.tpDiskQuota.PerformLayout();
      this.tpRecording.ResumeLayout(false);
      this.tpRecording.PerformLayout();
      this.groupBoxRecordSettings.ResumeLayout(false);
      this.groupBoxRecordSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.tpRecordImport.ResumeLayout(false);
      this.tpRecordImport.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tpSettings;
    private System.Windows.Forms.TabPage tpCustom;
    private System.Windows.Forms.GroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAutoDelete;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.TextBox textBoxSample;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox comboBoxMovies;
    private System.Windows.Forms.TabPage tpDiskQuota;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label labelFreeDiskspace;
    private System.Windows.Forms.Label labelTotalDiskSpace;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.ComboBox comboBoxDrive;
    private System.Windows.Forms.Label label9;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.TabPage tpRecording;
    private System.Windows.Forms.ComboBox comboBoxCards;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.TextBox textBoxFolder;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Label label23;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox mpNumericTextBoxDiskQuota;
    private System.Windows.Forms.NumericUpDown numericUpDownPostRec;
    private System.Windows.Forms.NumericUpDown numericUpDownPreRec;
    private System.Windows.Forms.CheckBox enableDiskQuota;
    private System.Windows.Forms.TabPage tpRecordImport;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.Label lblImportItems;
    private System.Windows.Forms.TreeView tvTagRecs;
    private System.Windows.Forms.Label lblRecFolders;
    private System.Windows.Forms.ComboBox cbRecPaths;
    private System.Windows.Forms.Button btnRemoveInvalidFiles;
    private System.Windows.Forms.GroupBox groupBoxRecordSettings;
    private System.Windows.Forms.TextBox textBoxRecordingFormat;
    private System.Windows.Forms.Button buttonChangeChannel;
    private System.Windows.Forms.ComboBox comboBoxEpisodeKey;
    private System.Windows.Forms.Label labelEpisodeKey;
    private System.Windows.Forms.Button buttonSameRecFolder;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxScheduler;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGap;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxPreventDupes;
    private System.Windows.Forms.ComboBox comboBoxFormat;
    private System.Windows.Forms.TextBox textBoxCustomFormat;
    private System.Windows.Forms.Label labelCustomFormat;
    private System.Windows.Forms.GroupBox groupBoxRecording;
    private System.Windows.Forms.Label label48;
    private System.Windows.Forms.NumericUpDown numericUpDownMaxFreeCardsToTry;
    private System.Windows.Forms.Label lblMaxFreeCards;
    private System.Windows.Forms.ComboBox comboBoxWeekend;
    private System.Windows.Forms.Label lblWeekend;
  }
}