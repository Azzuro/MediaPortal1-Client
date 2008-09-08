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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DirectShowLib;
using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Analog;

namespace SetupTv.Sections
{
  public partial class CardAnalog : SectionSettings
  {
    int _cardNumber;
    bool _isScanning = false;
    bool _stopScanning = false;
    bool _qualityControlSupported = false;
    string _cardName;
    string _devicePath;
    Configuration _configuration;

    public CardAnalog()
      : this("Analog")
    {
    }
    public CardAnalog(string name)
      : base(name)
    {
    }

    public CardAnalog(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();

    }
    void Init()
    {
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        mpComboBoxCountry.Items.Add(countries.Countries[i]);
      }
      mpComboBoxCountry.SelectedIndex = 0;
      mpComboBoxSource.Items.Add(TunerInputType.Antenna);
      mpComboBoxSource.Items.Add(TunerInputType.Cable);
      mpComboBoxSource.SelectedIndex = 0;
    }

    void UpdateStatus()
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      User user = new User();
      user.CardId = _cardNumber;
      AnalogChannel channel = RemoteControl.Instance.CurrentChannel(ref user) as AnalogChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
      {
        if (channel.IsTv)
          mpLabelChannel.Text = String.Format("#{0} {1}", channel.ChannelNumber, channel.Name);
        else
        {
          float freq = channel.Frequency;
          freq /= 1000000f;
          mpLabelChannel.Text = String.Format("Radio {0} MHz", freq.ToString("f2"));
        }
      }
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxSensitivity.SelectedIndex = 1;
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber.ToString() + "Country", "0").Value);
      mpComboBoxSource.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber.ToString() + "Source", "0").Value);
      if (String.IsNullOrEmpty(_cardName) || String.IsNullOrEmpty(_devicePath))
      {
        _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
        customValue.Value = _configuration.CustomQualityValue;
        customValuePeak.Value = _configuration.CustomPeakQualityValue;
        SetBitRateModes();
        SetBitRate();
      }
    }

    private void SetBitRateModes()
    {
      switch (_configuration.PlaybackQualityMode)
      {
        case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
          cbrPlayback.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
          vbrPlayback.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
          vbrPeakPlayback.Select();
          break;
      }
      switch (_configuration.RecordQualityMode)
      {
        case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
          cbrRecord.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
          vbrRecord.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
          vbrPeakRecord.Select();
          break;
      }
    }

    private void SetBitRate()
    {
      switch (_configuration.PlaybackQualityType)
      {
        case QualityType.Default:
          defaultPlayback.Select();
          break;
        case QualityType.Custom:
          customPlayback.Select();
          break;
        case QualityType.Portable:
          portablePlayback.Select();
          break;
        case QualityType.Low:
          lowPlayback.Select();
          break;
        case QualityType.Medium:
          mediumPlayback.Select();
          break;
        case QualityType.High:
          highPlayback.Select();
          break;
      }
      switch (_configuration.RecordQualityType)
      {
        case QualityType.Default:
          defaultRecord.Select();
          break;
        case QualityType.Custom:
          customRecord.Select();
          break;
        case QualityType.Portable:
          portableRecord.Select();
          break;
        case QualityType.Low:
          lowRecord.Select();
          break;
        case QualityType.Medium:
          mediumRecord.Select();
          break;
        case QualityType.High:
          highRecord.Select();
          break;
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting;
      setting = layer.GetSetting("analog" + _cardNumber.ToString() + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("analog" + _cardNumber.ToString() + "Source", "0");
      setting.Value = mpComboBoxSource.SelectedIndex.ToString();
      setting.Persist();
      UpdateConfiguration();
      Configuration.writeConfiguration(_configuration);
      RemoteControl.Instance.ReloadQualityControlConfigration(_cardNumber);
    }

    private void UpdateConfiguration()
    {
      _configuration.CustomQualityValue = (int)customValue.Value;
      _configuration.CustomPeakQualityValue = (int)customValuePeak.Value;
      if (cbrPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      }
      else if (vbrPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
      }
      else if (vbrPeakPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
      }
      if (cbrRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      }
      else if (vbrRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
      }
      else if (vbrPeakRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
      }
      if (defaultPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Default;
      }
      else if (customPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Custom;
      }
      else if (portablePlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Portable;
      }
      else if (lowPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Low;
      }
      else if (mediumPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Medium;
      }
      else if (highPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.High;
      }
      if (defaultRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Default;
      }
      else if (customRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Custom;
      }
      else if (portableRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Portable;
      }
      else if (lowRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Low;
      }
      else if (mediumRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Medium;
      }
      else if (highRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.High;
      }
    }

    private void mpButtonScan_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        Thread scanThread = new Thread(new ThreadStart(DoTvScan));
        scanThread.Name = "Analog TV scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    void DoTvScan()
    {
      string buttonText = mpButtonScanTv.Text;
      checkButton.Enabled = false;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpButtonScanRadio.Enabled = false;
        //mpButtonScanTv.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        UpdateStatus();
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.Tune(ref user, new AnalogChannel(), -1);
        int minChannel = RemoteControl.Instance.MinChannel(_cardNumber);
        int maxChannel = RemoteControl.Instance.MaxChannel(_cardNumber);
        if (maxChannel < 0)
        {
          if (mpComboBoxSource.SelectedIndex == 0)
            maxChannel = 69;
          else
            maxChannel = 125;
        }
        if (minChannel < 0) minChannel = 1;
        Log.Info("Min channel = {0}. Max channel = {1}", minChannel, maxChannel);
        for (int channelNr = minChannel; channelNr <= maxChannel; channelNr++)
        {
          if (_stopScanning) return;
          float percent = ((float)((channelNr - minChannel)) / (maxChannel - minChannel));
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          if (percent < 0) percent = 0f;
          progressBar1.Value = (int)percent;
          AnalogChannel channel = new AnalogChannel();
          if (mpComboBoxSource.SelectedIndex == 0)
            channel.TunerSource = TunerInputType.Antenna;
          else
            channel.TunerSource = TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.ChannelNumber = channelNr;
          channel.IsTv = true;
          channel.IsRadio = false;
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, channel);
          UpdateStatus();
          if (channels == null) continue;
          if (channels.Length == 0) continue;
          channel = (AnalogChannel)channels[0];
          if (channel.Name == "") channel.Name = String.Format(channel.ChannelNumber.ToString());
          ListViewItem item = mpListView1.Items.Add(channel.ChannelNumber.ToString());
          item.SubItems.Add(channel.Name);
          mpListView1.EnsureVisible(mpListView1.Items.Count - 1);
          Channel dbChannel;
          if (checkBoxNoMerge.Checked)
            dbChannel = new Channel(channel.Name, false, false, 0, new DateTime(2000, 1, 1), false, new DateTime(2000, 1, 1), -1, true, "", true, channel.Name);
          else
            dbChannel = layer.AddChannel("", channel.Name);
          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          dbChannel.FreeToAir = true;
          dbChannel.Persist();
          layer.AddTuningDetails(dbChannel, channel);
          layer.MapChannelToCard(card, dbChannel, false);
          layer.AddChannelToGroup(dbChannel, "Analog");
        }
      }
      finally
      {
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpButtonScanTv.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        //        mpButtonScanTv.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        //DatabaseManager.Instance.SaveChanges();
        _isScanning = false;
        checkButton.Enabled = true;
      }
    }

    private void mpButtonScanRadio_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        AnalogChannel radioChannel = new AnalogChannel();
        radioChannel.Frequency = 96000000;
        radioChannel.IsRadio = true;
        if (!RemoteControl.Instance.CanTune(_cardNumber, radioChannel))
        {
          MessageBox.Show(this, "The Tv Card does not support radio");
          return;
        }
        Thread scanThread = new Thread(new ThreadStart(DoRadioScan));
        scanThread.Name = "Analog Radio scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }
    int SignalStrength(int sensitivity)
    {
      int i = 0;
      for (i = 0; i < sensitivity * 2; i++)
      {
        if (!RemoteControl.Instance.TunerLocked(_cardNumber))
        {
          break;
        }
        System.Threading.Thread.Sleep(50);
      }
      return ((i * 50) / sensitivity);
    }

    void DoRadioScan()
    {
      checkButton.Enabled = false;
      int sensitivity = 1;
      switch (mpComboBoxSensitivity.Text)
      {
        case "High":
          sensitivity = 10;
          break;
        case "Medium":
          sensitivity = 2;
          break;
        case "Low":
          sensitivity = 1;
          break;
      }
      string buttonText = mpButtonScanRadio.Text;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanRadio.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        //mpButtonScanRadio.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        mpButtonScanTv.Enabled = false;
        UpdateStatus();
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        for (int freq = 87500000; freq < 108000000; freq += 100000)
        {
          if (_stopScanning) return;
          float percent = ((float)(freq - 87500000)) / (108000000f - 87500000f);
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;
          AnalogChannel channel = new AnalogChannel();
          channel.IsRadio = true;
          if (mpComboBoxSource.SelectedIndex == 0)
            channel.TunerSource = TunerInputType.Antenna;
          else
            channel.TunerSource = TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.Frequency = freq;
          channel.IsTv = false;
          channel.IsRadio = true;
          User user = new User();
          user.CardId = _cardNumber;
          RemoteControl.Instance.Tune(ref user, channel, -1);
          UpdateStatus();
          System.Threading.Thread.Sleep(2000);
          if (SignalStrength(sensitivity) == 100)
          {
            ListViewItem item = mpListView1.Items.Add(channel.Frequency.ToString());
            mpListView1.EnsureVisible(mpListView1.Items.Count - 1);
            channel.Name = String.Format("{0}", freq);
            Channel dbChannel = layer.AddChannel("", channel.Name);
            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.FreeToAir = true;
            dbChannel.Persist();
            layer.AddChannelToGroup(dbChannel, "Analog channels");
            layer.AddTuningDetails(dbChannel, channel);
            layer.MapChannelToCard(card, dbChannel, false);
            freq += 300000;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        checkButton.Enabled = true;
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpButtonScanRadio.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        mpButtonScanTv.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        //DatabaseManager.Instance.SaveChanges();
        _isScanning = false;
      }
    }

    private void mpBeveledLine1_Load(object sender, EventArgs e)
    {
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      Channel dbChannel = layer.AddChannel("", "CVBS#1 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      AnalogChannel tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput1;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "CVBS#2 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput2;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "CVBS#3 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput3;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "SVHS#1 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput1;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "SVHS#2 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput2;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "SVHS#3 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput3;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "RGB#1 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput1;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "RGB#2 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput2;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      dbChannel = layer.AddChannel("", "RGB#3 on " + card.IdCard.ToString());
      dbChannel.IsTv = true;
      dbChannel.Persist();
      tuningDetail = new AnalogChannel();
      tuningDetail.IsTv = true;
      tuningDetail.Name = dbChannel.Name;
      tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput3;
      layer.AddTuningDetails(dbChannel, tuningDetail);
      layer.MapChannelToCard(card, dbChannel, false);
      MessageBox.Show(this, "Channels added.");
    }

    private void checkButton_Click(object sender, EventArgs e)
    {
      User user;
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before checking quality control");
          return;
        }
        else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before checking quality control");
          return;
        }
        // Check if the card is locked for scanning.
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Checking quality control not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.Tune(ref user, new AnalogChannel(), -1);
        if (RemoteControl.Instance.SupportsQualityControl(_cardNumber))
        {
          _qualityControlSupported = true;
          _cardName = RemoteControl.Instance.CardName(_cardNumber);
          _devicePath = RemoteControl.Instance.CardDevice(_cardNumber);
          if (RemoteControl.Instance.SupportsBitRateModes(_cardNumber))
          {
            bitRateModeGroup.Enabled = true;
          }
          else
          {
            bitRateModeGroup.Enabled = false;
          }
          if (RemoteControl.Instance.SupportsPeakBitRateMode(_cardNumber))
          {
            vbrPeakPlayback.Enabled = true;
            vbrPeakRecord.Enabled = true;
          }
          else
          {
            vbrPeakPlayback.Enabled = false;
            vbrPeakRecord.Enabled = false;
          }
          if (RemoteControl.Instance.SupportsBitRate(_cardNumber))
          {
            bitRate.Enabled = true;
            customSettingsGroup.Enabled = true;
            customValue.Enabled = true;
            customValuePeak.Enabled = true;
          }
          else
          {
            bitRate.Enabled = false;
            customSettingsGroup.Enabled = false;
            customValue.Enabled = false;
            customValuePeak.Enabled = false;
          }
          _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
          customValue.Value = _configuration.CustomQualityValue;
          customValuePeak.Value = _configuration.CustomPeakQualityValue;
          SetBitRateModes();
          SetBitRate();
        }
        else
        {
          Log.WriteFile("Card doesn't support quality control");
          MessageBox.Show("The used encoder doesn't support quality control.", "MediaPortal - TV Server management console", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      finally
      {
        user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
      }
    }
  }
}
