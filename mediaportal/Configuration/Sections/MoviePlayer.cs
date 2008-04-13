#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.GUI.Library;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class MoviePlayer : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton parametersButton;
    private MediaPortal.UserInterface.Controls.MPTextBox parametersTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton fileNameButton;
    private MediaPortal.UserInterface.Controls.MPTextBox fileNameTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPCheckBox externalPlayerCheckBox;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoDecoderSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox wmvGroupBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox wmvCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    bool _init = false;

    public MoviePlayer()
      : this("Movie Player")
    {
    }

    public MoviePlayer(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        // Fetch available audio and video renderers
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        // Populate video and audio codecs
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
        //Remove Cyberlink Muxer from the list to avoid newbie user confusion.
        while (availableVideoFilters.Contains("CyberLink MPEG Muxer")) availableVideoFilters.Remove("CyberLink MPEG Muxer");
        while (availableVideoFilters.Contains("Ulead MPEG Muxer")) availableVideoFilters.Remove("Ulead MPEG Muxer");
        while (availableVideoFilters.Contains("PDR MPEG Muxer")) availableVideoFilters.Remove("PDR MPEG Muxer");
        while (availableVideoFilters.Contains("Nero Mpeg2 Encoder")) availableVideoFilters.Remove("Nero Mpeg2 Encoder");
        availableVideoFilters.Sort();
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        while (availableAudioFilters.Contains("CyberLink MPEG Muxer")) availableAudioFilters.Remove("CyberLink MPEG Muxer");
        while (availableAudioFilters.Contains("Ulead MPEG Muxer")) availableAudioFilters.Remove("Ulead MPEG Muxer");
        while (availableAudioFilters.Contains("PDR MPEG Muxer")) availableAudioFilters.Remove("PDR MPEG Muxer");
        while (availableAudioFilters.Contains("Nero Mpeg2 Encoder")) availableAudioFilters.Remove("Nero Mpeg2 Encoder");
        availableAudioFilters.Sort();
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        LoadSettings();
      }
    }

    /// <summary>
    /// sets useability of select config depending on whether auot decoder stting option is enabled.
    /// </summary>
    public void UpdateDecoderSettings()
    {
      label5.Enabled = !autoDecoderSettings.Checked;
      label6.Enabled = !autoDecoderSettings.Checked;
      mpLabel1.Enabled = !autoDecoderSettings.Checked;
      videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      h264videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      audioCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      wmvCheckBox.Enabled = !autoDecoderSettings.Checked;
    }

    /// <summary>
    /// Loads the movie player settings
    /// </summary>
    public override void LoadSettings()
    {
      if (_init == false) return;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        autoDecoderSettings.Checked = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
        UpdateDecoderSettings();
        fileNameTextBox.Text = xmlreader.GetValueAsString("movieplayer", "path", "");
        parametersTextBox.Text = xmlreader.GetValueAsString("movieplayer", "arguments", "");
        externalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "internal", true);
        externalPlayerCheckBox.Checked = !externalPlayerCheckBox.Checked;
        audioRendererComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
        wmvCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
        // Set codecs
        string videoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
        string h264videoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
        string audioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = false;
            bool DScalerFilterFound = false;
            audioCodec = (string)availableAudioFilters[0];
            foreach (string filter in availableAudioFilters)
            {
              if (filter.Equals("MPA Decoder Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Audio Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) audioCodec = "MPA Decoder Filter";
            else if (DScalerFilterFound) audioCodec = "DScaler Audio Decoder";
          }
        }
        Log.Info("  - videoCodec =(" + videoCodec + ")");
        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = false;
          bool DScalerFilterFound = false;
          Log.Info(" - availableVideoFilters.Count = " + availableVideoFilters.Count.ToString());
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              Log.Info(" - filter = (" + filter + ")");
              if (filter.Equals("MPV Decoder Filter"))
              {
                Log.Info(" - MPV Decoder filter found");
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) videoCodec = "MPV Decoder Filter";
            else if (DScalerFilterFound) videoCodec = "DScaler Mpeg2 Video Decoder";
          }
        }
        if (h264videoCodec == string.Empty)
        {
          ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
          bool H264DecFilterFound = false;
          if (availableH264VideoFilters.Count > 0)
          {
            h264videoCodec = (string)availableH264VideoFilters[0];
            foreach (string filter in availableH264VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                H264DecFilterFound = true;
              }
            }
            if (H264DecFilterFound) h264videoCodec = "CoreAVC Video Decoder";
          }
        }
        audioCodecComboBox.Text = audioCodec;
        videoCodecComboBox.Text = videoCodec;
        h264videoCodecComboBox.Text = h264videoCodec;
      }
    }

    /// <summary>
    /// Saves movie player settings and codec info.
    /// </summary>
    public override void SaveSettings()
    {
      if (_init == false) return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("movieplayer", "autodecodersettings", autoDecoderSettings.Checked);
        xmlwriter.SetValue("movieplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("movieplayer", "arguments", parametersTextBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "internal", !externalPlayerCheckBox.Checked);
        xmlwriter.SetValue("movieplayer", "audiorenderer", audioRendererComboBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "wmvaudio", wmvCheckBox.Checked);
        // Set codecs
        xmlwriter.SetValue("movieplayer", "mpeg2audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "mpeg2videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "h264videocodec", h264videoCodecComboBox.Text);
      }
    }

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
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.externalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.fileNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.autoDecoderSettings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.wmvGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.wmvCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.wmvGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.externalPlayerCheckBox);
      this.groupBox1.Controls.Add(this.parametersButton);
      this.groupBox1.Controls.Add(this.parametersTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.fileNameButton);
      this.groupBox1.Controls.Add(this.fileNameTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 239);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 112);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "External player";
      // 
      // externalPlayerCheckBox
      // 
      this.externalPlayerCheckBox.AutoSize = true;
      this.externalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.externalPlayerCheckBox.Location = new System.Drawing.Point(19, 28);
      this.externalPlayerCheckBox.Name = "externalPlayerCheckBox";
      this.externalPlayerCheckBox.Size = new System.Drawing.Size(231, 17);
      this.externalPlayerCheckBox.TabIndex = 0;
      this.externalPlayerCheckBox.Text = "Use external player (replaces internal player)";
      this.externalPlayerCheckBox.UseVisualStyleBackColor = true;
      this.externalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.externalPlayerCheckBox_CheckedChanged);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Location = new System.Drawing.Point(384, 84);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 6;
      this.parametersButton.Text = "List";
      this.parametersButton.UseVisualStyleBackColor = true;
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.BorderColor = System.Drawing.Color.Empty;
      this.parametersTextBox.Location = new System.Drawing.Point(168, 84);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Parameters:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(384, 60);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 3;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.fileNameTextBox.Location = new System.Drawing.Point(168, 60);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.fileNameTextBox.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 64);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Path/Filename:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.autoDecoderSettings);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.h264videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.audioRendererComboBox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.Controls.Add(this.audioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.label5);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 165);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Codec Settings (internal player)";
      // 
      // autoDecoderSettings
      // 
      this.autoDecoderSettings.AutoSize = true;
      this.autoDecoderSettings.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.autoDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoDecoderSettings.Location = new System.Drawing.Point(19, 129);
      this.autoDecoderSettings.Name = "autoDecoderSettings";
      this.autoDecoderSettings.Size = new System.Drawing.Size(309, 30);
      this.autoDecoderSettings.TabIndex = 0;
      this.autoDecoderSettings.Text = "Automatic Decoder Settings \r\n(use with caution - knowledge of DirectShow merits r" +
          "equired)";
      this.autoDecoderSettings.UseVisualStyleBackColor = true;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(146, 16);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "H.264 video decoder:";
      // 
      // h264videoCodecComboBox
      // 
      this.h264videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.h264videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.h264videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.h264videoCodecComboBox.Location = new System.Drawing.Point(168, 48);
      this.h264videoCodecComboBox.Name = "h264videoCodecComboBox";
      this.h264videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.h264videoCodecComboBox.TabIndex = 9;
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 96);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioRendererComboBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 100);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(146, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "MPEG-2 video decoder:";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 72);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 24);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 76);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 16);
      this.label5.TabIndex = 2;
      this.label5.Text = "Audio decoder:";
      // 
      // wmvGroupBox
      // 
      this.wmvGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.wmvGroupBox.Controls.Add(this.mpLabel2);
      this.wmvGroupBox.Controls.Add(this.wmvCheckBox);
      this.wmvGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvGroupBox.Location = new System.Drawing.Point(0, 171);
      this.wmvGroupBox.Name = "wmvGroupBox";
      this.wmvGroupBox.Size = new System.Drawing.Size(472, 62);
      this.wmvGroupBox.TabIndex = 7;
      this.wmvGroupBox.TabStop = false;
      this.wmvGroupBox.Text = "WMV playback (internal player)";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(34, 39);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(326, 16);
      this.mpLabel2.TabIndex = 10;
      this.mpLabel2.Text = "Will not be applied if Automatic Decoder Settings enabled.";
      // 
      // wmvCheckBox
      // 
      this.wmvCheckBox.AutoSize = true;
      this.wmvCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvCheckBox.Location = new System.Drawing.Point(19, 19);
      this.wmvCheckBox.Name = "wmvCheckBox";
      this.wmvCheckBox.Size = new System.Drawing.Size(233, 17);
      this.wmvCheckBox.TabIndex = 0;
      this.wmvCheckBox.Text = "Use 5.1 audio playback for WMV movie files";
      this.wmvCheckBox.UseVisualStyleBackColor = true;
      // 
      // MoviePlayer
      // 
      this.Controls.Add(this.wmvGroupBox);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "MoviePlayer";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.wmvGroupBox.ResumeLayout(false);
      this.wmvGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void externalPlayerCheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
      fileNameTextBox.Enabled = fileNameButton.Enabled = parametersTextBox.Enabled = parametersButton.Enabled = externalPlayerCheckBox.Checked;
    }

    /// <summary>
    /// sets the external movies player source file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void fileNameButton_Click(object sender, System.EventArgs e)
    {
      using (openFileDialog = new OpenFileDialog())
      {
        openFileDialog.FileName = fileNameTextBox.Text;
        openFileDialog.CheckFileExists = true;
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Filter = "exe files (*.exe)|*.exe";
        openFileDialog.FilterIndex = 0;
        openFileDialog.Title = "Select movie player";
        DialogResult dialogResult = openFileDialog.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
          fileNameTextBox.Text = openFileDialog.FileName;
        }
      }
    }

    /// <summary>
    /// sets the external movies player parameters.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void parametersButton_Click(object sender, System.EventArgs e)
    {
      ParameterForm parameters = new ParameterForm();
      parameters.AddParameter("%filename%", "Will be replaced by currently selected media file");
      if (parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    /// <summary>
    /// updates the useable options if the auto decoder option is enabled.
    /// </summary>
    private void autoDecoderSettings_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
    }
  }
}
