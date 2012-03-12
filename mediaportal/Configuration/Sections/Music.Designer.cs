﻿using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  partial class Music
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }

        // Close eventually open Winamp stuff
        if (_visParam != null)
        {
          BassVis_Api.BassVis.BASSVIS_Quit(_visParam);
        }

        // Make sure we shut down the viz engine
        if (IVizMgr != null)
        {
          IVizMgr.Stop();
          IVizMgr.ShutDown();
        }
      }

      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.MusicSettingsTabCtl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.PlayerTabPg = new System.Windows.Forms.TabPage();
      this.tabControlPlayerSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageBassPlayerSettings = new System.Windows.Forms.TabPage();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.BufferingSecondsLbl = new System.Windows.Forms.Label();
      this.CrossFadeSecondsLbl = new System.Windows.Forms.Label();
      this.hScrollBarBuffering = new System.Windows.Forms.HScrollBar();
      this.hScrollBarCrossFade = new System.Windows.Forms.HScrollBar();
      this.label12 = new System.Windows.Forms.Label();
      this.GaplessPlaybackChkBox = new System.Windows.Forms.CheckBox();
      this.StreamOutputLevelNud = new System.Windows.Forms.NumericUpDown();
      this.CrossFadingLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.FadeOnStartStopChkbox = new System.Windows.Forms.CheckBox();
      this.tabPageASIOPlayerSettings = new System.Windows.Forms.TabPage();
      this.lbBalance = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarBalance = new System.Windows.Forms.HScrollBar();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btAsioDeviceSettings = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPageWASAPIPLayerSettings = new System.Windows.Forms.TabPage();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.soundDeviceComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioPlayerComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.PlaySettingsTabPg = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.PlayNowJumpToCmbBox = new System.Windows.Forms.ComboBox();
      this.label8 = new System.Windows.Forms.Label();
      this.grpSelectOptions = new System.Windows.Forms.GroupBox();
      this.cmbSelectOption = new System.Windows.Forms.ComboBox();
      this.chkAddAllTracks = new System.Windows.Forms.CheckBox();
      this.tabPageNowPlaying = new System.Windows.Forms.TabPage();
      this.groupBoxVUMeter = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonVULed = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonVUAnalog = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonVUNone = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxDynamicContent = new System.Windows.Forms.GroupBox();
      this.checkBoxSwitchArtistOnLastFMSubmit = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDisableTagLookups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDisableAlbumLookups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDisableCoverLookups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxVizOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ShowVizInNowPlayingChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ShowLyricsCmbBox = new System.Windows.Forms.ComboBox();
      this.label9 = new System.Windows.Forms.Label();
      this.PlaylistTabPg = new System.Windows.Forms.TabPage();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.PlaylistCurrentCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ResumePlaylistChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.SavePlaylistOnExitChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.playlistButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.playlistFolderTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.VisualizationsTabPg = new System.Windows.Forms.TabPage();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxWinampVis = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btWinampConfig = new MediaPortal.UserInterface.Controls.MPButton();
      this.EnableStatusOverlaysChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ShowTrackInfoChkBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label11 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.VizPresetsCmbBox = new System.Windows.Forms.ComboBox();
      this.VisualizationsCmbBox = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.VisualizationFpsNud = new System.Windows.Forms.NumericUpDown();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.WasapiExclusiveModeCkBox = new System.Windows.Forms.CheckBox();
      this.MusicSettingsTabCtl.SuspendLayout();
      this.PlayerTabPg.SuspendLayout();
      this.tabControlPlayerSettings.SuspendLayout();
      this.tabPageBassPlayerSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.StreamOutputLevelNud)).BeginInit();
      this.tabPageASIOPlayerSettings.SuspendLayout();
      this.tabPageWASAPIPLayerSettings.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.PlaySettingsTabPg.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.grpSelectOptions.SuspendLayout();
      this.tabPageNowPlaying.SuspendLayout();
      this.groupBoxVUMeter.SuspendLayout();
      this.groupBoxDynamicContent.SuspendLayout();
      this.groupBoxVizOptions.SuspendLayout();
      this.PlaylistTabPg.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.VisualizationsTabPg.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.groupBoxWinampVis.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VisualizationFpsNud)).BeginInit();
      this.SuspendLayout();
      // 
      // MusicSettingsTabCtl
      // 
      this.MusicSettingsTabCtl.Controls.Add(this.PlayerTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.PlaySettingsTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.tabPageNowPlaying);
      this.MusicSettingsTabCtl.Controls.Add(this.PlaylistTabPg);
      this.MusicSettingsTabCtl.Controls.Add(this.VisualizationsTabPg);
      this.MusicSettingsTabCtl.Location = new System.Drawing.Point(0, 8);
      this.MusicSettingsTabCtl.Name = "MusicSettingsTabCtl";
      this.MusicSettingsTabCtl.SelectedIndex = 0;
      this.MusicSettingsTabCtl.Size = new System.Drawing.Size(472, 447);
      this.MusicSettingsTabCtl.TabIndex = 1;
      this.MusicSettingsTabCtl.SelectedIndexChanged += new System.EventHandler(this.MusicSettingsTabCtl_SelectedIndexChanged);
      // 
      // PlayerTabPg
      // 
      this.PlayerTabPg.Controls.Add(this.tabControlPlayerSettings);
      this.PlayerTabPg.Controls.Add(this.mpGroupBox1);
      this.PlayerTabPg.Location = new System.Drawing.Point(4, 22);
      this.PlayerTabPg.Name = "PlayerTabPg";
      this.PlayerTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.PlayerTabPg.Size = new System.Drawing.Size(464, 421);
      this.PlayerTabPg.TabIndex = 1;
      this.PlayerTabPg.Text = "Player settings";
      this.PlayerTabPg.UseVisualStyleBackColor = true;
      // 
      // tabControlPlayerSettings
      // 
      this.tabControlPlayerSettings.Controls.Add(this.tabPageBassPlayerSettings);
      this.tabControlPlayerSettings.Controls.Add(this.tabPageASIOPlayerSettings);
      this.tabControlPlayerSettings.Controls.Add(this.tabPageWASAPIPLayerSettings);
      this.tabControlPlayerSettings.Location = new System.Drawing.Point(16, 109);
      this.tabControlPlayerSettings.Name = "tabControlPlayerSettings";
      this.tabControlPlayerSettings.SelectedIndex = 0;
      this.tabControlPlayerSettings.Size = new System.Drawing.Size(432, 306);
      this.tabControlPlayerSettings.TabIndex = 14;
      // 
      // tabPageBassPlayerSettings
      // 
      this.tabPageBassPlayerSettings.Controls.Add(this.mpLabel3);
      this.tabPageBassPlayerSettings.Controls.Add(this.BufferingSecondsLbl);
      this.tabPageBassPlayerSettings.Controls.Add(this.CrossFadeSecondsLbl);
      this.tabPageBassPlayerSettings.Controls.Add(this.hScrollBarBuffering);
      this.tabPageBassPlayerSettings.Controls.Add(this.hScrollBarCrossFade);
      this.tabPageBassPlayerSettings.Controls.Add(this.label12);
      this.tabPageBassPlayerSettings.Controls.Add(this.GaplessPlaybackChkBox);
      this.tabPageBassPlayerSettings.Controls.Add(this.StreamOutputLevelNud);
      this.tabPageBassPlayerSettings.Controls.Add(this.CrossFadingLbl);
      this.tabPageBassPlayerSettings.Controls.Add(this.mpLabel1);
      this.tabPageBassPlayerSettings.Controls.Add(this.FadeOnStartStopChkbox);
      this.tabPageBassPlayerSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageBassPlayerSettings.Name = "tabPageBassPlayerSettings";
      this.tabPageBassPlayerSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBassPlayerSettings.Size = new System.Drawing.Size(424, 280);
      this.tabPageBassPlayerSettings.TabIndex = 0;
      this.tabPageBassPlayerSettings.Text = "General Settings";
      this.tabPageBassPlayerSettings.UseVisualStyleBackColor = true;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(17, 18);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(188, 13);
      this.mpLabel3.TabIndex = 14;
      this.mpLabel3.Text = "Standard BASS Player via DirectShow";
      // 
      // BufferingSecondsLbl
      // 
      this.BufferingSecondsLbl.Location = new System.Drawing.Point(310, 196);
      this.BufferingSecondsLbl.Name = "BufferingSecondsLbl";
      this.BufferingSecondsLbl.Size = new System.Drawing.Size(80, 13);
      this.BufferingSecondsLbl.TabIndex = 9;
      this.BufferingSecondsLbl.Text = "00.0 Seconds";
      this.BufferingSecondsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // CrossFadeSecondsLbl
      // 
      this.CrossFadeSecondsLbl.Location = new System.Drawing.Point(310, 172);
      this.CrossFadeSecondsLbl.Name = "CrossFadeSecondsLbl";
      this.CrossFadeSecondsLbl.Size = new System.Drawing.Size(80, 13);
      this.CrossFadeSecondsLbl.TabIndex = 6;
      this.CrossFadeSecondsLbl.Text = "00.0 Seconds";
      this.CrossFadeSecondsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // hScrollBarBuffering
      // 
      this.hScrollBarBuffering.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarBuffering.LargeChange = 500;
      this.hScrollBarBuffering.Location = new System.Drawing.Point(96, 196);
      this.hScrollBarBuffering.Maximum = 8499;
      this.hScrollBarBuffering.Minimum = 1000;
      this.hScrollBarBuffering.Name = "hScrollBarBuffering";
      this.hScrollBarBuffering.Size = new System.Drawing.Size(188, 17);
      this.hScrollBarBuffering.SmallChange = 100;
      this.hScrollBarBuffering.TabIndex = 11;
      this.hScrollBarBuffering.Value = 5000;
      this.hScrollBarBuffering.ValueChanged += new System.EventHandler(this.hScrollBarBuffering_ValueChanged);
      // 
      // hScrollBarCrossFade
      // 
      this.hScrollBarCrossFade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarCrossFade.LargeChange = 500;
      this.hScrollBarCrossFade.Location = new System.Drawing.Point(96, 172);
      this.hScrollBarCrossFade.Maximum = 16499;
      this.hScrollBarCrossFade.Name = "hScrollBarCrossFade";
      this.hScrollBarCrossFade.Size = new System.Drawing.Size(188, 17);
      this.hScrollBarCrossFade.SmallChange = 100;
      this.hScrollBarCrossFade.TabIndex = 10;
      this.hScrollBarCrossFade.Value = 4000;
      this.hScrollBarCrossFade.ValueChanged += new System.EventHandler(this.hScrollBarCrossFade_ValueChanged);
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(20, 196);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(52, 13);
      this.label12.TabIndex = 7;
      this.label12.Text = "Buffering:";
      // 
      // GaplessPlaybackChkBox
      // 
      this.GaplessPlaybackChkBox.AutoSize = true;
      this.GaplessPlaybackChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.GaplessPlaybackChkBox.Location = new System.Drawing.Point(17, 118);
      this.GaplessPlaybackChkBox.Name = "GaplessPlaybackChkBox";
      this.GaplessPlaybackChkBox.Size = new System.Drawing.Size(108, 17);
      this.GaplessPlaybackChkBox.TabIndex = 3;
      this.GaplessPlaybackChkBox.Text = "Gapless playback";
      this.GaplessPlaybackChkBox.UseVisualStyleBackColor = true;
      this.GaplessPlaybackChkBox.CheckedChanged += new System.EventHandler(this.GaplessPlaybackChkBox_CheckedChanged);
      // 
      // StreamOutputLevelNud
      // 
      this.StreamOutputLevelNud.Location = new System.Drawing.Point(87, 48);
      this.StreamOutputLevelNud.Name = "StreamOutputLevelNud";
      this.StreamOutputLevelNud.Size = new System.Drawing.Size(52, 20);
      this.StreamOutputLevelNud.TabIndex = 1;
      this.StreamOutputLevelNud.Value = new decimal(new int[] {
            85,
            0,
            0,
            0});
      // 
      // CrossFadingLbl
      // 
      this.CrossFadingLbl.AutoSize = true;
      this.CrossFadingLbl.Location = new System.Drawing.Point(20, 172);
      this.CrossFadingLbl.Name = "CrossFadingLbl";
      this.CrossFadingLbl.Size = new System.Drawing.Size(68, 13);
      this.CrossFadingLbl.TabIndex = 4;
      this.CrossFadingLbl.Text = "Cross-fading:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(14, 50);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(67, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Output level:";
      // 
      // FadeOnStartStopChkbox
      // 
      this.FadeOnStartStopChkbox.AutoSize = true;
      this.FadeOnStartStopChkbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.FadeOnStartStopChkbox.Location = new System.Drawing.Point(17, 84);
      this.FadeOnStartStopChkbox.Name = "FadeOnStartStopChkbox";
      this.FadeOnStartStopChkbox.Size = new System.Drawing.Size(97, 17);
      this.FadeOnStartStopChkbox.TabIndex = 2;
      this.FadeOnStartStopChkbox.Text = "Fade-in on start";
      this.FadeOnStartStopChkbox.UseVisualStyleBackColor = true;
      // 
      // tabPageASIOPlayerSettings
      // 
      this.tabPageASIOPlayerSettings.Controls.Add(this.lbBalance);
      this.tabPageASIOPlayerSettings.Controls.Add(this.hScrollBarBalance);
      this.tabPageASIOPlayerSettings.Controls.Add(this.mpLabel6);
      this.tabPageASIOPlayerSettings.Controls.Add(this.mpLabel7);
      this.tabPageASIOPlayerSettings.Controls.Add(this.mpLabel4);
      this.tabPageASIOPlayerSettings.Controls.Add(this.btAsioDeviceSettings);
      this.tabPageASIOPlayerSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageASIOPlayerSettings.Name = "tabPageASIOPlayerSettings";
      this.tabPageASIOPlayerSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageASIOPlayerSettings.Size = new System.Drawing.Size(424, 280);
      this.tabPageASIOPlayerSettings.TabIndex = 1;
      this.tabPageASIOPlayerSettings.Text = "ASIO";
      this.tabPageASIOPlayerSettings.UseVisualStyleBackColor = true;
      // 
      // lbBalance
      // 
      this.lbBalance.AutoSize = true;
      this.lbBalance.Location = new System.Drawing.Point(357, 186);
      this.lbBalance.Name = "lbBalance";
      this.lbBalance.Size = new System.Drawing.Size(28, 13);
      this.lbBalance.TabIndex = 16;
      this.lbBalance.Text = "0.00";
      // 
      // hScrollBarBalance
      // 
      this.hScrollBarBalance.Location = new System.Drawing.Point(69, 181);
      this.hScrollBarBalance.Minimum = -100;
      this.hScrollBarBalance.Name = "hScrollBarBalance";
      this.hScrollBarBalance.Size = new System.Drawing.Size(262, 18);
      this.hScrollBarBalance.TabIndex = 15;
      this.hScrollBarBalance.ValueChanged += new System.EventHandler(this.hScrollBarBalance_ValueChanged);
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(72, 218);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(282, 26);
      this.mpLabel6.TabIndex = 14;
      this.mpLabel6.Text = "In case of multi-channel (not stereo) the left/right positions \r\nare interleaved " +
    "between the additional channels.";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(15, 181);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(49, 13);
      this.mpLabel7.TabIndex = 13;
      this.mpLabel7.Text = "Balance:";
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(21, 20);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(184, 13);
      this.mpLabel4.TabIndex = 1;
      this.mpLabel4.Text = "Playback via BASS using ASIO driver";
      // 
      // btAsioDeviceSettings
      // 
      this.btAsioDeviceSettings.Location = new System.Drawing.Point(21, 54);
      this.btAsioDeviceSettings.Name = "btAsioDeviceSettings";
      this.btAsioDeviceSettings.Size = new System.Drawing.Size(140, 23);
      this.btAsioDeviceSettings.TabIndex = 0;
      this.btAsioDeviceSettings.Text = "Asio Device Settings";
      this.btAsioDeviceSettings.UseVisualStyleBackColor = true;
      this.btAsioDeviceSettings.Click += new System.EventHandler(this.btAsioDeviceSettings_Click);
      // 
      // tabPageWASAPIPLayerSettings
      // 
      this.tabPageWASAPIPLayerSettings.Controls.Add(this.WasapiExclusiveModeCkBox);
      this.tabPageWASAPIPLayerSettings.Controls.Add(this.mpLabel5);
      this.tabPageWASAPIPLayerSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageWASAPIPLayerSettings.Name = "tabPageWASAPIPLayerSettings";
      this.tabPageWASAPIPLayerSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageWASAPIPLayerSettings.Size = new System.Drawing.Size(424, 280);
      this.tabPageWASAPIPLayerSettings.TabIndex = 2;
      this.tabPageWASAPIPLayerSettings.Text = "WasAPI";
      this.tabPageWASAPIPLayerSettings.UseVisualStyleBackColor = true;
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(18, 22);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(346, 13);
      this.mpLabel5.TabIndex = 0;
      this.mpLabel5.Text = "Playback via BASS using Windows Audio Session API (WasAPI) drivers";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.soundDeviceComboBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.audioPlayerComboBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 13);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 85);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "General settings";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(7, 54);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(78, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Sound Device:";
      // 
      // soundDeviceComboBox
      // 
      this.soundDeviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.soundDeviceComboBox.BorderColor = System.Drawing.Color.Empty;
      this.soundDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.soundDeviceComboBox.Location = new System.Drawing.Point(91, 51);
      this.soundDeviceComboBox.Name = "soundDeviceComboBox";
      this.soundDeviceComboBox.Size = new System.Drawing.Size(289, 21);
      this.soundDeviceComboBox.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(46, 27);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(39, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Player:";
      // 
      // audioPlayerComboBox
      // 
      this.audioPlayerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioPlayerComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioPlayerComboBox.Location = new System.Drawing.Point(91, 24);
      this.audioPlayerComboBox.Name = "audioPlayerComboBox";
      this.audioPlayerComboBox.Size = new System.Drawing.Size(289, 21);
      this.audioPlayerComboBox.TabIndex = 1;
      this.audioPlayerComboBox.SelectedIndexChanged += new System.EventHandler(this.audioPlayerComboBox_SelectedIndexChanged);
      // 
      // PlaySettingsTabPg
      // 
      this.PlaySettingsTabPg.Controls.Add(this.groupBox3);
      this.PlaySettingsTabPg.Controls.Add(this.grpSelectOptions);
      this.PlaySettingsTabPg.Location = new System.Drawing.Point(4, 22);
      this.PlaySettingsTabPg.Name = "PlaySettingsTabPg";
      this.PlaySettingsTabPg.Size = new System.Drawing.Size(464, 421);
      this.PlaySettingsTabPg.TabIndex = 3;
      this.PlaySettingsTabPg.Text = "Play Settings";
      this.PlaySettingsTabPg.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.PlayNowJumpToCmbBox);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.Location = new System.Drawing.Point(14, 128);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(432, 74);
      this.groupBox3.TabIndex = 7;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Jump To Behaviour";
      // 
      // PlayNowJumpToCmbBox
      // 
      this.PlayNowJumpToCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.PlayNowJumpToCmbBox.FormattingEnabled = true;
      this.PlayNowJumpToCmbBox.Location = new System.Drawing.Point(124, 27);
      this.PlayNowJumpToCmbBox.Name = "PlayNowJumpToCmbBox";
      this.PlayNowJumpToCmbBox.Size = new System.Drawing.Size(293, 21);
      this.PlayNowJumpToCmbBox.TabIndex = 7;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(16, 31);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(106, 13);
      this.label8.TabIndex = 6;
      this.label8.Text = "Jump on \"Play now\":";
      this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // grpSelectOptions
      // 
      this.grpSelectOptions.Controls.Add(this.cmbSelectOption);
      this.grpSelectOptions.Controls.Add(this.chkAddAllTracks);
      this.grpSelectOptions.Location = new System.Drawing.Point(14, 17);
      this.grpSelectOptions.Name = "grpSelectOptions";
      this.grpSelectOptions.Size = new System.Drawing.Size(432, 74);
      this.grpSelectOptions.TabIndex = 6;
      this.grpSelectOptions.TabStop = false;
      this.grpSelectOptions.Text = "OK / Enter / Select Button";
      // 
      // cmbSelectOption
      // 
      this.cmbSelectOption.FormattingEnabled = true;
      this.cmbSelectOption.Items.AddRange(new object[] {
            "Play",
            "Queue"});
      this.cmbSelectOption.Location = new System.Drawing.Point(15, 31);
      this.cmbSelectOption.Name = "cmbSelectOption";
      this.cmbSelectOption.Size = new System.Drawing.Size(211, 21);
      this.cmbSelectOption.TabIndex = 3;
      // 
      // chkAddAllTracks
      // 
      this.chkAddAllTracks.AutoSize = true;
      this.chkAddAllTracks.Location = new System.Drawing.Point(259, 33);
      this.chkAddAllTracks.Name = "chkAddAllTracks";
      this.chkAddAllTracks.Size = new System.Drawing.Size(95, 17);
      this.chkAddAllTracks.TabIndex = 4;
      this.chkAddAllTracks.Text = "Add All Tracks";
      this.chkAddAllTracks.UseVisualStyleBackColor = true;
      // 
      // tabPageNowPlaying
      // 
      this.tabPageNowPlaying.Controls.Add(this.groupBoxVUMeter);
      this.tabPageNowPlaying.Controls.Add(this.groupBoxDynamicContent);
      this.tabPageNowPlaying.Controls.Add(this.groupBoxVizOptions);
      this.tabPageNowPlaying.Location = new System.Drawing.Point(4, 22);
      this.tabPageNowPlaying.Name = "tabPageNowPlaying";
      this.tabPageNowPlaying.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageNowPlaying.Size = new System.Drawing.Size(464, 421);
      this.tabPageNowPlaying.TabIndex = 5;
      this.tabPageNowPlaying.Text = "Now playing";
      this.tabPageNowPlaying.UseVisualStyleBackColor = true;
      // 
      // groupBoxVUMeter
      // 
      this.groupBoxVUMeter.Controls.Add(this.radioButtonVULed);
      this.groupBoxVUMeter.Controls.Add(this.radioButtonVUAnalog);
      this.groupBoxVUMeter.Controls.Add(this.radioButtonVUNone);
      this.groupBoxVUMeter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxVUMeter.Location = new System.Drawing.Point(16, 285);
      this.groupBoxVUMeter.Name = "groupBoxVUMeter";
      this.groupBoxVUMeter.Size = new System.Drawing.Size(432, 64);
      this.groupBoxVUMeter.TabIndex = 5;
      this.groupBoxVUMeter.TabStop = false;
      this.groupBoxVUMeter.Text = "VUMeter (BASS player only)";
      // 
      // radioButtonVULed
      // 
      this.radioButtonVULed.AutoSize = true;
      this.radioButtonVULed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonVULed.Location = new System.Drawing.Point(234, 33);
      this.radioButtonVULed.Name = "radioButtonVULed";
      this.radioButtonVULed.Size = new System.Drawing.Size(45, 17);
      this.radioButtonVULed.TabIndex = 2;
      this.radioButtonVULed.Text = "LED";
      this.radioButtonVULed.UseVisualStyleBackColor = true;
      // 
      // radioButtonVUAnalog
      // 
      this.radioButtonVUAnalog.AutoSize = true;
      this.radioButtonVUAnalog.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonVUAnalog.Location = new System.Drawing.Point(119, 33);
      this.radioButtonVUAnalog.Name = "radioButtonVUAnalog";
      this.radioButtonVUAnalog.Size = new System.Drawing.Size(57, 17);
      this.radioButtonVUAnalog.TabIndex = 1;
      this.radioButtonVUAnalog.Text = "Analog";
      this.radioButtonVUAnalog.UseVisualStyleBackColor = true;
      // 
      // radioButtonVUNone
      // 
      this.radioButtonVUNone.AutoSize = true;
      this.radioButtonVUNone.Checked = true;
      this.radioButtonVUNone.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonVUNone.Location = new System.Drawing.Point(11, 33);
      this.radioButtonVUNone.Name = "radioButtonVUNone";
      this.radioButtonVUNone.Size = new System.Drawing.Size(50, 17);
      this.radioButtonVUNone.TabIndex = 0;
      this.radioButtonVUNone.TabStop = true;
      this.radioButtonVUNone.Text = "None";
      this.radioButtonVUNone.UseVisualStyleBackColor = true;
      // 
      // groupBoxDynamicContent
      // 
      this.groupBoxDynamicContent.Controls.Add(this.checkBoxSwitchArtistOnLastFMSubmit);
      this.groupBoxDynamicContent.Controls.Add(this.checkBoxDisableTagLookups);
      this.groupBoxDynamicContent.Controls.Add(this.checkBoxDisableAlbumLookups);
      this.groupBoxDynamicContent.Controls.Add(this.checkBoxDisableCoverLookups);
      this.groupBoxDynamicContent.Location = new System.Drawing.Point(16, 16);
      this.groupBoxDynamicContent.Name = "groupBoxDynamicContent";
      this.groupBoxDynamicContent.Size = new System.Drawing.Size(432, 194);
      this.groupBoxDynamicContent.TabIndex = 4;
      this.groupBoxDynamicContent.TabStop = false;
      this.groupBoxDynamicContent.Text = "Dynamic content";
      // 
      // checkBoxSwitchArtistOnLastFMSubmit
      // 
      this.checkBoxSwitchArtistOnLastFMSubmit.AutoSize = true;
      this.checkBoxSwitchArtistOnLastFMSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxSwitchArtistOnLastFMSubmit.Location = new System.Drawing.Point(11, 88);
      this.checkBoxSwitchArtistOnLastFMSubmit.Name = "checkBoxSwitchArtistOnLastFMSubmit";
      this.checkBoxSwitchArtistOnLastFMSubmit.Size = new System.Drawing.Size(404, 17);
      this.checkBoxSwitchArtistOnLastFMSubmit.TabIndex = 13;
      this.checkBoxSwitchArtistOnLastFMSubmit.Text = "Switch artist on internet lookup. i.e. LastName, Firstname -> FirstName LastName";
      this.checkBoxSwitchArtistOnLastFMSubmit.UseVisualStyleBackColor = true;
      // 
      // checkBoxDisableTagLookups
      // 
      this.checkBoxDisableTagLookups.AutoSize = true;
      this.checkBoxDisableTagLookups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDisableTagLookups.Location = new System.Drawing.Point(11, 65);
      this.checkBoxDisableTagLookups.Name = "checkBoxDisableTagLookups";
      this.checkBoxDisableTagLookups.Size = new System.Drawing.Size(238, 17);
      this.checkBoxDisableTagLookups.TabIndex = 10;
      this.checkBoxDisableTagLookups.Text = "Disable internet lookups for track suggestions";
      this.checkBoxDisableTagLookups.UseVisualStyleBackColor = true;
      // 
      // checkBoxDisableAlbumLookups
      // 
      this.checkBoxDisableAlbumLookups.AutoSize = true;
      this.checkBoxDisableAlbumLookups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDisableAlbumLookups.Location = new System.Drawing.Point(11, 42);
      this.checkBoxDisableAlbumLookups.Name = "checkBoxDisableAlbumLookups";
      this.checkBoxDisableAlbumLookups.Size = new System.Drawing.Size(238, 17);
      this.checkBoxDisableAlbumLookups.TabIndex = 9;
      this.checkBoxDisableAlbumLookups.Text = "Disable internet lookups for best album tracks";
      this.checkBoxDisableAlbumLookups.UseVisualStyleBackColor = true;
      // 
      // checkBoxDisableCoverLookups
      // 
      this.checkBoxDisableCoverLookups.AutoSize = true;
      this.checkBoxDisableCoverLookups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDisableCoverLookups.Location = new System.Drawing.Point(11, 19);
      this.checkBoxDisableCoverLookups.Name = "checkBoxDisableCoverLookups";
      this.checkBoxDisableCoverLookups.Size = new System.Drawing.Size(197, 17);
      this.checkBoxDisableCoverLookups.TabIndex = 8;
      this.checkBoxDisableCoverLookups.Text = "Disable internet lookups for cover art";
      this.checkBoxDisableCoverLookups.UseVisualStyleBackColor = true;
      // 
      // groupBoxVizOptions
      // 
      this.groupBoxVizOptions.Controls.Add(this.ShowVizInNowPlayingChkBox);
      this.groupBoxVizOptions.Controls.Add(this.ShowLyricsCmbBox);
      this.groupBoxVizOptions.Controls.Add(this.label9);
      this.groupBoxVizOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxVizOptions.Location = new System.Drawing.Point(16, 216);
      this.groupBoxVizOptions.Name = "groupBoxVizOptions";
      this.groupBoxVizOptions.Size = new System.Drawing.Size(432, 54);
      this.groupBoxVizOptions.TabIndex = 3;
      this.groupBoxVizOptions.TabStop = false;
      this.groupBoxVizOptions.Text = "Visualization options";
      // 
      // ShowVizInNowPlayingChkBox
      // 
      this.ShowVizInNowPlayingChkBox.AutoSize = true;
      this.ShowVizInNowPlayingChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ShowVizInNowPlayingChkBox.Location = new System.Drawing.Point(91, 23);
      this.ShowVizInNowPlayingChkBox.Name = "ShowVizInNowPlayingChkBox";
      this.ShowVizInNowPlayingChkBox.Size = new System.Drawing.Size(201, 17);
      this.ShowVizInNowPlayingChkBox.TabIndex = 4;
      this.ShowVizInNowPlayingChkBox.Text = "Show visualization (BASS player only)";
      this.ShowVizInNowPlayingChkBox.UseVisualStyleBackColor = true;
      // 
      // ShowLyricsCmbBox
      // 
      this.ShowLyricsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ShowLyricsCmbBox.Enabled = false;
      this.ShowLyricsCmbBox.FormattingEnabled = true;
      this.ShowLyricsCmbBox.Location = new System.Drawing.Point(121, 32);
      this.ShowLyricsCmbBox.Name = "ShowLyricsCmbBox";
      this.ShowLyricsCmbBox.Size = new System.Drawing.Size(293, 21);
      this.ShowLyricsCmbBox.TabIndex = 1;
      this.ShowLyricsCmbBox.Visible = false;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Enabled = false;
      this.label9.Location = new System.Drawing.Point(22, 35);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(63, 13);
      this.label9.TabIndex = 0;
      this.label9.Text = "Show lyrics:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.label9.Visible = false;
      // 
      // PlaylistTabPg
      // 
      this.PlaylistTabPg.Controls.Add(this.groupBox1);
      this.PlaylistTabPg.Location = new System.Drawing.Point(4, 22);
      this.PlaylistTabPg.Name = "PlaylistTabPg";
      this.PlaylistTabPg.Size = new System.Drawing.Size(464, 421);
      this.PlaylistTabPg.TabIndex = 2;
      this.PlaylistTabPg.Text = "Playlist settings";
      this.PlaylistTabPg.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.PlaylistCurrentCheckBox);
      this.groupBox1.Controls.Add(this.autoShuffleCheckBox);
      this.groupBox1.Controls.Add(this.ResumePlaylistChkBox);
      this.groupBox1.Controls.Add(this.SavePlaylistOnExitChkBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.playlistButton);
      this.groupBox1.Controls.Add(this.playlistFolderTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 222);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Playlist settings";
      // 
      // PlaylistCurrentCheckBox
      // 
      this.PlaylistCurrentCheckBox.AutoSize = true;
      this.PlaylistCurrentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.PlaylistCurrentCheckBox.Location = new System.Drawing.Point(91, 153);
      this.PlaylistCurrentCheckBox.Name = "PlaylistCurrentCheckBox";
      this.PlaylistCurrentCheckBox.Size = new System.Drawing.Size(194, 17);
      this.PlaylistCurrentCheckBox.TabIndex = 7;
      this.PlaylistCurrentCheckBox.Text = "Playlist screen shows current playlist";
      this.PlaylistCurrentCheckBox.UseVisualStyleBackColor = true;
      // 
      // autoShuffleCheckBox
      // 
      this.autoShuffleCheckBox.AutoSize = true;
      this.autoShuffleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoShuffleCheckBox.Location = new System.Drawing.Point(91, 84);
      this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
      this.autoShuffleCheckBox.Size = new System.Drawing.Size(180, 17);
      this.autoShuffleCheckBox.TabIndex = 4;
      this.autoShuffleCheckBox.Text = "Auto shuffle playlists after loading";
      this.autoShuffleCheckBox.UseVisualStyleBackColor = true;
      // 
      // ResumePlaylistChkBox
      // 
      this.ResumePlaylistChkBox.AutoSize = true;
      this.ResumePlaylistChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ResumePlaylistChkBox.Location = new System.Drawing.Point(91, 130);
      this.ResumePlaylistChkBox.Name = "ResumePlaylistChkBox";
      this.ResumePlaylistChkBox.Size = new System.Drawing.Size(229, 17);
      this.ResumePlaylistChkBox.TabIndex = 5;
      this.ResumePlaylistChkBox.Text = "Load default playlist on MediaPortal startup ";
      this.ResumePlaylistChkBox.UseVisualStyleBackColor = true;
      // 
      // SavePlaylistOnExitChkBox
      // 
      this.SavePlaylistOnExitChkBox.AutoSize = true;
      this.SavePlaylistOnExitChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.SavePlaylistOnExitChkBox.Location = new System.Drawing.Point(91, 107);
      this.SavePlaylistOnExitChkBox.Name = "SavePlaylistOnExitChkBox";
      this.SavePlaylistOnExitChkBox.Size = new System.Drawing.Size(293, 17);
      this.SavePlaylistOnExitChkBox.TabIndex = 5;
      this.SavePlaylistOnExitChkBox.Text = "Save current playlist as default when leaving MediaPortal";
      this.SavePlaylistOnExitChkBox.UseVisualStyleBackColor = true;
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.AutoSize = true;
      this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(91, 61);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(219, 17);
      this.repeatPlaylistCheckBox.TabIndex = 3;
      this.repeatPlaylistCheckBox.Text = "Repeat/loop music playlists (m3u, b4, pls)";
      this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
      // 
      // playlistButton
      // 
      this.playlistButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistButton.Location = new System.Drawing.Point(365, 22);
      this.playlistButton.Name = "playlistButton";
      this.playlistButton.Size = new System.Drawing.Size(61, 22);
      this.playlistButton.TabIndex = 2;
      this.playlistButton.Text = "Browse";
      this.playlistButton.UseVisualStyleBackColor = true;
      this.playlistButton.Click += new System.EventHandler(this.playlistButton_Click);
      // 
      // playlistFolderTextBox
      // 
      this.playlistFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistFolderTextBox.BorderColor = System.Drawing.Color.Empty;
      this.playlistFolderTextBox.Location = new System.Drawing.Point(91, 24);
      this.playlistFolderTextBox.Name = "playlistFolderTextBox";
      this.playlistFolderTextBox.Size = new System.Drawing.Size(268, 20);
      this.playlistFolderTextBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(14, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(71, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Playlist folder:";
      // 
      // VisualizationsTabPg
      // 
      this.VisualizationsTabPg.Controls.Add(this.mpGroupBox3);
      this.VisualizationsTabPg.Location = new System.Drawing.Point(4, 22);
      this.VisualizationsTabPg.Name = "VisualizationsTabPg";
      this.VisualizationsTabPg.Size = new System.Drawing.Size(464, 421);
      this.VisualizationsTabPg.TabIndex = 4;
      this.VisualizationsTabPg.Text = "Visualizations";
      this.VisualizationsTabPg.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.groupBoxWinampVis);
      this.mpGroupBox3.Controls.Add(this.EnableStatusOverlaysChkBox);
      this.mpGroupBox3.Controls.Add(this.ShowTrackInfoChkBox);
      this.mpGroupBox3.Controls.Add(this.label11);
      this.mpGroupBox3.Controls.Add(this.label10);
      this.mpGroupBox3.Controls.Add(this.VizPresetsCmbBox);
      this.mpGroupBox3.Controls.Add(this.VisualizationsCmbBox);
      this.mpGroupBox3.Controls.Add(this.label7);
      this.mpGroupBox3.Controls.Add(this.label5);
      this.mpGroupBox3.Controls.Add(this.VisualizationFpsNud);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(432, 343);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Visualization settings";
      // 
      // groupBoxWinampVis
      // 
      this.groupBoxWinampVis.Controls.Add(this.btWinampConfig);
      this.groupBoxWinampVis.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxWinampVis.Location = new System.Drawing.Point(91, 86);
      this.groupBoxWinampVis.Name = "groupBoxWinampVis";
      this.groupBoxWinampVis.Size = new System.Drawing.Size(289, 56);
      this.groupBoxWinampVis.TabIndex = 11;
      this.groupBoxWinampVis.TabStop = false;
      this.groupBoxWinampVis.Text = "Winamp Vis.";
      // 
      // btWinampConfig
      // 
      this.btWinampConfig.Location = new System.Drawing.Point(6, 24);
      this.btWinampConfig.Name = "btWinampConfig";
      this.btWinampConfig.Size = new System.Drawing.Size(75, 23);
      this.btWinampConfig.TabIndex = 4;
      this.btWinampConfig.Text = "Config";
      this.btWinampConfig.UseVisualStyleBackColor = true;
      this.btWinampConfig.Click += new System.EventHandler(this.btWinampConfig_Click);
      // 
      // EnableStatusOverlaysChkBox
      // 
      this.EnableStatusOverlaysChkBox.AutoSize = true;
      this.EnableStatusOverlaysChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.EnableStatusOverlaysChkBox.Location = new System.Drawing.Point(91, 246);
      this.EnableStatusOverlaysChkBox.Name = "EnableStatusOverlaysChkBox";
      this.EnableStatusOverlaysChkBox.Size = new System.Drawing.Size(299, 17);
      this.EnableStatusOverlaysChkBox.TabIndex = 9;
      this.EnableStatusOverlaysChkBox.Text = "Enable status display in fullscreen mode (fast systems only)";
      this.EnableStatusOverlaysChkBox.UseVisualStyleBackColor = true;
      this.EnableStatusOverlaysChkBox.CheckedChanged += new System.EventHandler(this.EnableStatusOverlaysChkBox_CheckedChanged);
      // 
      // ShowTrackInfoChkBox
      // 
      this.ShowTrackInfoChkBox.AutoSize = true;
      this.ShowTrackInfoChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ShowTrackInfoChkBox.Location = new System.Drawing.Point(110, 267);
      this.ShowTrackInfoChkBox.Name = "ShowTrackInfoChkBox";
      this.ShowTrackInfoChkBox.Size = new System.Drawing.Size(178, 17);
      this.ShowTrackInfoChkBox.TabIndex = 10;
      this.ShowTrackInfoChkBox.Text = "Show song info on track change";
      this.ShowTrackInfoChkBox.UseVisualStyleBackColor = true;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(45, 54);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(40, 13);
      this.label11.TabIndex = 2;
      this.label11.Text = "Preset:";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(17, 27);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(68, 13);
      this.label10.TabIndex = 0;
      this.label10.Text = "Visualization:";
      // 
      // VizPresetsCmbBox
      // 
      this.VizPresetsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.VizPresetsCmbBox.FormattingEnabled = true;
      this.VizPresetsCmbBox.Location = new System.Drawing.Point(91, 51);
      this.VizPresetsCmbBox.Name = "VizPresetsCmbBox";
      this.VizPresetsCmbBox.Size = new System.Drawing.Size(289, 21);
      this.VizPresetsCmbBox.TabIndex = 3;
      this.VizPresetsCmbBox.SelectedIndexChanged += new System.EventHandler(this.VizPresetsCmbBox_SelectedIndexChanged);
      // 
      // VisualizationsCmbBox
      // 
      this.VisualizationsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.VisualizationsCmbBox.FormattingEnabled = true;
      this.VisualizationsCmbBox.Location = new System.Drawing.Point(91, 24);
      this.VisualizationsCmbBox.Name = "VisualizationsCmbBox";
      this.VisualizationsCmbBox.Size = new System.Drawing.Size(289, 21);
      this.VisualizationsCmbBox.TabIndex = 1;
      this.VisualizationsCmbBox.SelectedIndexChanged += new System.EventHandler(this.VisualizationsCmbBox_SelectedIndexChanged);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(149, 220);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(166, 13);
      this.label7.TabIndex = 8;
      this.label7.Text = "(use lower value for slow systems)";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(21, 220);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 6;
      this.label5.Text = "Target FPS:";
      // 
      // VisualizationFpsNud
      // 
      this.VisualizationFpsNud.Location = new System.Drawing.Point(91, 218);
      this.VisualizationFpsNud.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.VisualizationFpsNud.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.VisualizationFpsNud.Name = "VisualizationFpsNud";
      this.VisualizationFpsNud.Size = new System.Drawing.Size(52, 20);
      this.VisualizationFpsNud.TabIndex = 7;
      this.VisualizationFpsNud.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.VisualizationFpsNud.ValueChanged += new System.EventHandler(this.VisualizationFpsNud_ValueChanged);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 23);
      this.label4.TabIndex = 0;
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Location = new System.Drawing.Point(259, 21);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(95, 17);
      this.checkBox2.TabIndex = 6;
      this.checkBox2.Text = "Add All Tracks";
      this.checkBox2.UseVisualStyleBackColor = true;
      // 
      // WasapiExclusiveModeCkBox
      // 
      this.WasapiExclusiveModeCkBox.AutoSize = true;
      this.WasapiExclusiveModeCkBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.WasapiExclusiveModeCkBox.Location = new System.Drawing.Point(21, 53);
      this.WasapiExclusiveModeCkBox.Name = "WasapiExclusiveModeCkBox";
      this.WasapiExclusiveModeCkBox.Size = new System.Drawing.Size(161, 17);
      this.WasapiExclusiveModeCkBox.TabIndex = 3;
      this.WasapiExclusiveModeCkBox.Text = "Use WasApi Exclusive Mode";
      this.WasapiExclusiveModeCkBox.UseVisualStyleBackColor = true;
      // 
      // Music
      // 
      this.Controls.Add(this.MusicSettingsTabCtl);
      this.Name = "Music";
      this.Size = new System.Drawing.Size(472, 455);
      this.MusicSettingsTabCtl.ResumeLayout(false);
      this.PlayerTabPg.ResumeLayout(false);
      this.tabControlPlayerSettings.ResumeLayout(false);
      this.tabPageBassPlayerSettings.ResumeLayout(false);
      this.tabPageBassPlayerSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.StreamOutputLevelNud)).EndInit();
      this.tabPageASIOPlayerSettings.ResumeLayout(false);
      this.tabPageASIOPlayerSettings.PerformLayout();
      this.tabPageWASAPIPLayerSettings.ResumeLayout(false);
      this.tabPageWASAPIPLayerSettings.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.PlaySettingsTabPg.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.grpSelectOptions.ResumeLayout(false);
      this.grpSelectOptions.PerformLayout();
      this.tabPageNowPlaying.ResumeLayout(false);
      this.groupBoxVUMeter.ResumeLayout(false);
      this.groupBoxVUMeter.PerformLayout();
      this.groupBoxDynamicContent.ResumeLayout(false);
      this.groupBoxDynamicContent.PerformLayout();
      this.groupBoxVizOptions.ResumeLayout(false);
      this.groupBoxVizOptions.PerformLayout();
      this.PlaylistTabPg.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.VisualizationsTabPg.ResumeLayout(false);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.groupBoxWinampVis.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.VisualizationFpsNud)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private FolderBrowserDialog folderBrowserDialog;
    private MPLabel label4;
    private MPTabControl MusicSettingsTabCtl;
    private TabPage PlayerTabPg;
    private Label BufferingSecondsLbl;
    private Label CrossFadeSecondsLbl;
    private NumericUpDown StreamOutputLevelNud;
    private CheckBox FadeOnStartStopChkbox;
    private Label label12;
    private MPLabel mpLabel1;
    private MPLabel CrossFadingLbl;
    private MPGroupBox mpGroupBox1;
    private MPLabel label2;
    private MPComboBox audioPlayerComboBox;
    private TabPage VisualizationsTabPg;
    private MPGroupBox mpGroupBox3;
    private MPCheckBox EnableStatusOverlaysChkBox;
    private MPCheckBox ShowTrackInfoChkBox;
    private Label label11;
    private Label label10;
    private ComboBox VizPresetsCmbBox;
    private ComboBox VisualizationsCmbBox;
    private Label label7;
    private Label label5;
    private NumericUpDown VisualizationFpsNud;
    private TabPage PlaylistTabPg;
    private MPGroupBox groupBox1;
    private MPCheckBox autoShuffleCheckBox;
    private MPCheckBox ResumePlaylistChkBox;
    private MPCheckBox SavePlaylistOnExitChkBox;
    private MPCheckBox repeatPlaylistCheckBox;
    private MPButton playlistButton;
    private MPTextBox playlistFolderTextBox;
    private MPLabel label1;
    private TabPage PlaySettingsTabPg;
    private CheckBox GaplessPlaybackChkBox;
    private HScrollBar hScrollBarCrossFade;
    private HScrollBar hScrollBarBuffering;
    private TabPage tabPageNowPlaying;
    private GroupBox groupBoxDynamicContent;
    private MPCheckBox checkBoxDisableTagLookups;
    private MPCheckBox checkBoxDisableAlbumLookups;
    private MPCheckBox checkBoxDisableCoverLookups;
    private MPGroupBox groupBoxVizOptions;
    private MPCheckBox ShowVizInNowPlayingChkBox;
    private ComboBox ShowLyricsCmbBox;
    private Label label9;
    private MPLabel mpLabel2;
    private MPComboBox soundDeviceComboBox;
    private MPGroupBox groupBoxWinampVis;
    private MPButton btWinampConfig;
    private MPGroupBox groupBoxVUMeter;
    private MPRadioButton radioButtonVUAnalog;
    private MPRadioButton radioButtonVUNone;
    private MPRadioButton radioButtonVULed;
    private CheckBox chkAddAllTracks;
    private ComboBox cmbSelectOption;
    private GroupBox grpSelectOptions;
    private GroupBox groupBox3;
    private ComboBox PlayNowJumpToCmbBox;
    private Label label8;
    private CheckBox checkBox2;
    private MPCheckBox PlaylistCurrentCheckBox;
    private MPTabControl tabControlPlayerSettings;
    private TabPage tabPageBassPlayerSettings;
    private TabPage tabPageASIOPlayerSettings;
    private TabPage tabPageWASAPIPLayerSettings;
    private MPCheckBox checkBoxSwitchArtistOnLastFMSubmit;
    private MPButton btAsioDeviceSettings;
    private MPLabel mpLabel3;
    private MPLabel mpLabel4;
    private MPLabel mpLabel5;
    private MPLabel lbBalance;
    private HScrollBar hScrollBarBalance;
    private MPLabel mpLabel6;
    private MPLabel mpLabel7;
    private CheckBox WasapiExclusiveModeCkBox;
  }
}
