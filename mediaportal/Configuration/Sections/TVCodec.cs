#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.Profile;
using Microsoft.Win32;

namespace MediaPortal.Configuration.Sections
{
  public partial class TVCodec : SectionSettings
  {
    private bool _init = false;

    /// <summary>
    /// 
    /// </summary>
    public TVCodec()
      : this("TV Codecs") {}

    /// <summary>
    /// 
    /// </summary>
    public TVCodec(string name)
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
        //
        // Populate video and audio codecs
        //
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        //Remove Muxer's from the Video decoder list to avoid confusion.
        while (availableVideoFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableVideoFilters.Remove("CyberLink MPEG Muxer");
        }
        while (availableVideoFilters.Contains("Ulead MPEG Muxer"))
        {
          availableVideoFilters.Remove("Ulead MPEG Muxer");
        }
        while (availableVideoFilters.Contains("PDR MPEG Muxer"))
        {
          availableVideoFilters.Remove("PDR MPEG Muxer");
        }
        while (availableVideoFilters.Contains("Nero Mpeg2 Encoder"))
        {
          availableVideoFilters.Remove("Nero Mpeg2 Encoder");
        }
        availableVideoFilters.Sort();
        ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
        availableH264VideoFilters.Sort();
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        //Remove Muxer's from Audio decoder list to avoid confusion.
        while (availableAudioFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableAudioFilters.Remove("CyberLink MPEG Muxer");
        }
        while (availableAudioFilters.Contains("Ulead MPEG Muxer"))
        {
          availableAudioFilters.Remove("Ulead MPEG Muxer");
        }
        while (availableAudioFilters.Contains("PDR MPEG Muxer"))
        {
          availableAudioFilters.Remove("PDR MPEG Muxer");
        }
        while (availableAudioFilters.Contains("Nero Mpeg2 Encoder"))
        {
          availableAudioFilters.Remove("Nero Mpeg2 Encoder");
        }
        availableAudioFilters.Sort();
        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
        availableAACAudioFilters.Sort();
        ArrayList availableDDPLUSAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.DDPLUS);
        availableDDPLUSAudioFilters.Sort();
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        availableAudioRenderers.Sort();
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        aacAudioCodecComboBox.Items.AddRange(availableAACAudioFilters.ToArray());
        ddplusAudioCodecComboBox.Items.AddRange(availableDDPLUSAudioFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        LoadSettings();
      }
    }

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        //
        // Set codecs
        //
        string audioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        string videoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        string h264videoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        string aacaudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
        string ddplusaudioCodec = xmlreader.GetValueAsString("mytv", "ddplusaudiocodec", "");
        string audioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");

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
              if (filter.Equals("MPC - MPA Decoder Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Audio Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound)
            {
              audioCodec = "MPC - MPA Decoder Filter";
            }
            else if (DScalerFilterFound)
            {
              audioCodec = "DScaler Audio Decoder";
            }
          }
        }
        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = false;
          bool DScalerFilterFound = false;
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              if (filter.Equals("MPC - MPEG-2 Video Decoder (Gabest)"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound)
            {
              videoCodec = "MPC - MPEG-2 Video Decoder (Gabest)";
            }
            else if (DScalerFilterFound)
            {
              videoCodec = "DScaler Mpeg2 Video Decoder";
            }
          }
        }

        if (h264videoCodec == string.Empty)
        {
          ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
          bool h264DecFilterFound = false;
          if (availableH264VideoFilters.Count > 0)
          {
            h264videoCodec = (string)availableH264VideoFilters[0];
            foreach (string filter in availableH264VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                h264DecFilterFound = true;
              }
            }
            if (h264DecFilterFound)
            {
              h264videoCodec = "CoreAVC Video Decoder";
            }
          }
        }

        if (aacaudioCodec == string.Empty)
        {
          ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
          if (availableAACAudioFilters.Count > 0)
          {
            bool MonogramAACFound = false;
            aacaudioCodec = (string)availableAACAudioFilters[0];
            foreach (string filter in availableAACAudioFilters)
            {
              if (filter.Equals("MONOGRAM AAC Decoder"))
              {
                MonogramAACFound = true;
              }
            }
            if (MonogramAACFound)
            {
              aacaudioCodec = "MONOGRAM AAC Decoder";
            }
          }
        }

        if (ddplusaudioCodec == string.Empty)
        {
          ArrayList availableDDPLUSAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.DDPLUS);
          if (availableDDPLUSAudioFilters.Count > 0)
          {
            bool ffdshowFound = false;
            ddplusaudioCodec = (string)availableDDPLUSAudioFilters[0];
            foreach (string filter in availableDDPLUSAudioFilters)
            {
              if (filter.Equals("ffdshow Audio Decoder"))
              {
                ffdshowFound = true;
              }
            }
            if (ffdshowFound)
            {
              ddplusaudioCodec = "ffdshow Audio Decoder";
            }
          }
        }

        audioCodecComboBox.Text = audioCodec;
        videoCodecComboBox.Text = videoCodec;
        h264videoCodecComboBox.Text = h264videoCodec;
        audioRendererComboBox.Text = audioRenderer;
        aacAudioCodecComboBox.Text = aacaudioCodec;
        ddplusAudioCodecComboBox.Text = ddplusaudioCodec;
      }
    }

    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        //
        // Set codecs
        //
        xmlwriter.SetValue("mytv", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "h264videocodec", h264videoCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "audiorenderer", audioRendererComboBox.Text);
        xmlwriter.SetValue("mytv", "aacaudiocodec", aacAudioCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "ddplusaudiocodec", ddplusAudioCodecComboBox.Text);
      }
    }

    private void videoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      h264videoCodecComboBox.SelectedIndexChanged -= h264videoCodecComboBox_SelectedIndexChanged;
      if (videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        h264videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (h264videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < h264videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = h264videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            h264videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      h264videoCodecComboBox.SelectedIndexChanged += h264videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void h264videoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      videoCodecComboBox.SelectedIndexChanged -= videoCodecComboBox_SelectedIndexChanged;
      if (h264videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      videoCodecComboBox.SelectedIndexChanged += videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void RegMPtoConfig(string subkeysource)
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(subkeysource))
      {
        if (subkey != null)
        {
          RegistryUtilities.RenameSubKey(subkey, @"MediaPortal",
                                         @"Configuration");
        }
      }
    }

    private void RegConfigtoMP(string subkeysource)
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(subkeysource))
      {
        if (subkey != null)
        {
          RegistryUtilities.RenameSubKey(subkey, @"Configuration",
                                         @"MediaPortal");
        }
      }
    }

    private void ConfigCodecSection(object sender, EventArgs e, string selection)
    {
      foreach (DsDevice device in DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.LegacyAmFilterCategory))
      {
        try
        {
          if (device.Name != null)
          {
            {
              if (selection.Equals(device.Name))
              {
                if (selection.Contains("CyberLink"))
                {
                  // Rename MediaPortal subkey to Configuration for Cyberlink take setting
                  RegMPtoConfig(@"Software\Cyberlink\Common\clcvd");
                  RegMPtoConfig(@"Software\Cyberlink\Common\cl264dec");
                  RegMPtoConfig(@"Software\Cyberlink\Common\CLVSD");
                  RegMPtoConfig(@"Software\Cyberlink\Common\CLAud");

                  // Show Codec page Setting
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)device);
                  page.Show(this);

                  // Rename Configuration subkey to MediaPortal to apply Cyberlink setting
                  RegConfigtoMP(@"Software\Cyberlink\Common\clcvd");
                  RegConfigtoMP(@"Software\Cyberlink\Common\cl264dec");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLVSD");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLAud");
                }
                else
                {
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)device);
                  page.Show(this);
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    private void configMPEG_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, videoCodecComboBox.Text);
    }

    private void configH264_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, h264videoCodecComboBox.Text);
    }

    private void configMPEGAudio_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, audioCodecComboBox.Text);
    }

    private void configAACAudio_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, aacAudioCodecComboBox.Text);
    }

    private void configDDPlus_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, ddplusAudioCodecComboBox.Text);
    }

    private void configAudioRenderer_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, audioRendererComboBox.Text);
    }
  }
}