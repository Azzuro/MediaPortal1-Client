/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using TvDatabase;

using TvControl;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using System.Xml.Serialization;

namespace SetupTv.Sections
{
  public partial class CardDvbT : SectionSettings
  {
    readonly int _cardNumber;
    bool _isScanning;
    bool _stopScanning;
    
    FileFilters fileFilters;

    List<DVBTTuning> _dvbtChannels;

    CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    public CardDvbT()
      : this("Analog")
    {
    }
    public CardDvbT(string name)
      : base(name)
    {
    }

    public CardDvbT(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      //insert complete ci menu dialog to tab
      Card dbCard = Card.Retrieve(_cardNumber);
      if (dbCard.CAM == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardNumber);
        this.tabPageCIMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        this.tabPageCIMenu.Dispose();
      }
      base.Text = name;
      Init();
    }

    void Init()
    {
      mpComboBoxCountry.Items.Clear();
      try
      {
        fileFilters = new FileFilters("DVBT", ref mpComboBoxCountry, ref mpComboBoxRegion);
      }
      catch (Exception)
      {
        MessageBox.Show(@"Unable to open TuningParameters\dvbt\*.xml");
        return;
      }
      _dvbtChannels = new List<DVBTTuning>();
      SetButtonState();
    }
    /// <summary>
    /// Saves a new list with found transponders
    /// </summary>
    /// <param name="fileName">Path for output filename</param>
    void SaveList(string fileName)
    {
      try
      {
        System.IO.TextWriter parFileXML = System.IO.File.CreateText(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DVBTTuning>));
        xmlSerializer.Serialize(parFileXML, _dvbtChannels);
        parFileXML.Close();
      }
      catch (Exception ex)
      {
        Log.Error("Error saving tuningdetails: {0}", ex.ToString());
        MessageBox.Show("Transponder list could not be saved, check error.log for details.");
      }
    }

    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));


    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      int index = Int32.Parse(layer.GetSetting("dvbt" + _cardNumber + "Country", "0").Value);
      if (index < mpComboBoxCountry.Items.Count)
        mpComboBoxCountry.SelectedIndex = index;


      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbt" + _cardNumber + "creategroups", "false").Value == "true");

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }


    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbt" + _cardNumber + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbt" + _cardNumber + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
      
      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
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
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning another part of a hybrid card.");
          return;
        }

        CustomFileName tuningFile=(CustomFileName)mpComboBoxRegion.SelectedItem;
        _dvbtChannels= (List<DVBTTuning>)fileFilters.LoadList(tuningFile.FileName, typeof(List<DVBTTuning>));
        if (_dvbtChannels == null)
        {
          return;
        }
        StartScanThread();
      }
      else
      {
        _stopScanning = true;
      }
    }
    void DoScan()
    {
      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;
      //int frequencyOffset = 0;

      string buttonText = mpButtonScanTv.Text;
      User user = new User();
      user.CardId = _cardNumber;
      try
      {
        // First lock the card, because so that other parts of a hybrid card can't be used at the same time
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();

        if (_dvbtChannels.Count == 0)
          return;

        SetButtonState();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        //int index = -1;
        //Dictionary<int, int>.Enumerator enumerator = frequencies.GetEnumerator();
        //while (enumerator.MoveNext())
        //{
        for (int index = 0; index < _dvbtChannels.Count; ++index)
        {
          if (_stopScanning)
            return;

          float percent = ((float)(index)) / _dvbtChannels.Count;
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;

          DVBTTuning curTuning = _dvbtChannels[index];
          DVBTChannel tuneChannel = new DVBTChannel(curTuning);
          //tuneChannel.Frequency = values.Key;
          //tuneChannel.BandWidth = values.Value;

          string line = String.Format("{0}tp- {1} {2}MHz ", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == 0)
          {
            RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          if (channels == null || channels.Length == 0)
          {
            /// try frequency - offset
            tuneChannel.Frequency = curTuning.Frequency - curTuning.Offset;
            tuneChannel.BandWidth = curTuning.BandWidth;
            line = String.Format("{0}tp- {1} {2}MHz ", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
            item.Text = line;
            channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
            if (channels == null || channels.Length == 0)
            {
              /// try frequency + offset
              tuneChannel.Frequency = curTuning.Frequency + curTuning.Offset;
              tuneChannel.BandWidth = curTuning.BandWidth;
              line = String.Format("{0}tp- {1} {2}MHz ", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
              item.Text = line;
              channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
            }
          }

          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2}MHz :No Signal", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("{0}tp- {1} {2}MHz :Nothing found", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }

          int newChannels = 0;
          int updatedChannels = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBTChannel channel = (DVBTChannel)channels[i];
            //TuningDetail currentDetail = layer.GetChannel(channel);
            TuningDetail currentDetail = layer.GetChannel(channel.Provider, channel.Name, channel.ServiceId);
            bool exists;
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddChannel(channel.Provider, channel.Name);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
            }

            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.FreeToAir = channel.FreeToAir;
            dbChannel.Persist();

            if (dbChannel.IsTv)
            {
              layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);
              if (checkBoxCreateGroups.Checked)
              {
                layer.AddChannelToGroup(dbChannel, channel.Provider);
              }
            }
            if (dbChannel.IsRadio)
            {
              layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);
              if (checkBoxCreateGroups.Checked)
              {
                layer.AddChannelToRadioGroup(dbChannel, channel.Provider);
              }
            }

            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
              td.Persist();
            }

            if (channel.IsTv)
            {
              if (exists)
              {
                tvChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                tvChannelsNew++;
                newChannels++;
              }
            }
            if (channel.IsRadio)
            {
              if (exists)
              {
                radioChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                radioChannelsNew++;
                newChannels++;
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}tp- {1} {2}MHz :New:{3} Updated:{4}", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth, newChannels, updatedChannels);
            item.Text = line;
          }
        }
        //DatabaseManager.Instance.SaveChanges();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
        SetButtonState();
      }
      listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem.EnsureVisible();
    }

    private void CardDvbT_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonManualScan_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      if (card.Enabled == false)
      {
        MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
        return;
      }
      if (!RemoteControl.Instance.CardPresent(card.IdCard))
      {
        MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
        return;
      }
      RemoteControl.Instance.EpgGrabberEnabled = false;

      if (_dvbtChannels == null)
      {
        _dvbtChannels = new List<DVBTTuning>();
      }
      else
      {
        _dvbtChannels.Clear();
      }
      mpButtonSaveList.Enabled = false;
      DVBTChannel tuneChannel = GetManualTuning();

      listViewStatus.Items.Clear();
      string line = String.Format("Scan freq:{0} bandwidth:{1} ...", tuneChannel.Frequency, tuneChannel.BandWidth);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
      item.EnsureVisible();
      Application.DoEvents();
      IChannel[] channels = RemoteControl.Instance.ScanNIT(_cardNumber, tuneChannel);
      if (channels != null)
      {
        for (int i = 0; i < channels.Length; ++i)
        {
          DVBTChannel ch = (DVBTChannel)channels[i];
          _dvbtChannels.Add(ch.TuningInfo);
          item = listViewStatus.Items.Add(new ListViewItem(ch.TuningInfo.ToString()));
          item.EnsureVisible();
        }
      }

      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Scan done, found {0} transponders...", _dvbtChannels.Count)));
      lastItem.EnsureVisible();

      RemoteControl.Instance.EpgGrabberEnabled = true;
      if (_dvbtChannels.Count != 0)
      {
        if (DialogResult.Yes == MessageBox.Show(String.Format("Found {0} transponders. Would you like to scan those?", _dvbtChannels.Count), "Manual scan results", MessageBoxButtons.YesNo))
        {
          StartScanThread();
        }
      }
      SetButtonState();
    }

    /// <summary>
    /// Sets correct button state 
    /// </summary>
    private void SetButtonState()
    {
      mpButtonScanSingleTP.Enabled = !_isScanning;
      mpComboBoxCountry.Enabled = !_isScanning;
      mpComboBoxRegion.Enabled = !_isScanning;
      mpButtonScanNIT.Enabled = !_isScanning;
      mpButtonSaveList.Enabled = (_dvbtChannels.Count != 0) && !_isScanning;
    }

    private void StartScanThread()
    {
      Thread scanThread = new Thread(DoScan);
      scanThread.Name = "DVB-T scan thread";
      scanThread.Start();
    }

    private DVBTChannel GetManualTuning()
    {
      DVBTChannel tuneChannel = new DVBTChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.BandWidth = Int32.Parse(textBoxBandWidth.Text);
      return tuneChannel;
    }

    private void mpButtonScanSingleTP_Click(object sender, EventArgs e)
    {
      DVBTChannel tuneChannel = GetManualTuning();
      _dvbtChannels.Clear();
      _dvbtChannels.Add(tuneChannel.TuningInfo);
      StartScanThread();
    }

    private void mpButtonSaveList_Click(object sender, EventArgs e)
    {
      if (_dvbtChannels.Count != 0)
      {
        String filePath = String.Format(@"{0}\TuningParameters\dvbt\Manual_Scans.{1}.xml", Log.GetPathName(), DateTime.Now.ToString("yyyy-MM-dd"));
        SaveList(filePath);
        Init(); // refresh list
      }
    }
  }
}
