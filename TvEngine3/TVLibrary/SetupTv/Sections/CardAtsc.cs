/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using DirectShowLib;
using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class CardAtsc : SectionSettings
  {
    struct ATSCList
    {
      public int frequency;		 // frequency
    }

    int _cardNumber;
    ATSCList[] _atscChannels = new ATSCList[1000];
    int _channelCount = 0;
    bool _isScanning = false;
    bool _stopScanning = false;
    public CardAtsc()
      : this("DVBC")
    {
    }
    public CardAtsc(string name)
      : base(name)
    {
    }

    public CardAtsc(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }

    void LoadList(string fileName)
    {
      _channelCount = 0;
      string line;
      string[] tpdata;
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);
      int LineNr = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          LineNr++;
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 1)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 1)
            {
              try
              {
                _atscChannels[_channelCount].frequency = Int32.Parse(tpdata[0]);
                _channelCount += 1;
              }
              catch
              {
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();
    }

    void Init()
    {
      if (checkBoxQAM.Enabled != true)
      {
        mpComboBoxFrequencies.Enabled = false;
      }
      mpComboBoxFrequencies.Items.Clear();
      try
      {
        string[] files = System.IO.Directory.GetFiles("TuningParameters");
        for (int i = 0; i < files.Length; ++i)
        {
          string ext = System.IO.Path.GetExtension(files[i]).ToLower();
          if (ext != ".qam") continue;
          string fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
          mpComboBoxFrequencies.Items.Add(fileName);
        }
        mpComboBoxFrequencies.SelectedIndex = 0;
      }
      catch (Exception)
      {
        return;
      }
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      checkBoxQAM.Checked = (layer.GetSetting("atsc" + _cardNumber.ToString() + "supportsqam", "false").Value == "true");
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("atsc" + _cardNumber.ToString() + "supportsqam", "false");
      setting.Value = checkBoxQAM.Checked ? "true" : "false";
      setting.Persist();
    }

    void UpdateStatus()
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
      User user = new User();
      user.CardId = _cardNumber;
      ATSCChannel channel = RemoteControl.Instance.CurrentChannel(ref user) as ATSCChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("{0}  Frequency {1}", channel.PhysicalChannel, channel.Frequency);
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        LoadList(String.Format(@"Tuningparameters\{0}.qam", mpComboBoxFrequencies.SelectedItem));
        Thread scanThread = new Thread(new ThreadStart(DoScan));
        scanThread.Start();
        listViewStatus.Items.Clear();
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
      string buttonText = mpButtonScanTv.Text;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        if (_channelCount == 0) return;
        mpComboBoxFrequencies.Enabled = false;
        listViewStatus.Items.Clear();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        User user = new User();
        user.CardId = _cardNumber;
        int minchan = 2;
        int maxchan = 69;
        //Check if QAM if so then the number of channels varies
        if (checkBoxQAM.Checked)
        {
          minchan = 0;
          maxchan = _channelCount;
        }
        //Log.WriteFile("ATSC tune: using min channel {0} & max channel {1}",minchan, maxchan);
        for (int index = minchan; index < maxchan; ++index)
        {
          if (_stopScanning) return;
          float percent = ((float)(index)) / (maxchan - minchan);
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;
          ATSCChannel tuneChannel = new ATSCChannel();
          tuneChannel.NetworkId = -1;
          tuneChannel.TransportId = -1;
          tuneChannel.ServiceId = -1;
          tuneChannel.MinorChannel = -1;
          tuneChannel.MajorChannel = -1;
          tuneChannel.SymbolRate = -1;
          if (checkBoxQAM.Checked)
          {
            Log.WriteFile("ATSC tune: QAM checkbox selected... using Modulation 256Qam");
            //tuneChannel.PhysicalChannel = index +1;
            tuneChannel.PhysicalChannel = -1;
            tuneChannel.Frequency = _atscChannels[index].frequency;
            tuneChannel.ModulationType = ModulationType.Mod256Qam;
          }
          else
          {
            Log.WriteFile("ATSC tune: QAM checkbox not selected... using Modulation 8Vsb");
            tuneChannel.PhysicalChannel = index;
            tuneChannel.Frequency = -1;
            tuneChannel.ModulationType = ModulationType.Mod8Vsb;
          }
          Log.WriteFile("ATSC tune: PhysicalChannel: {0} Frequency: {1} Modulation: {2}", tuneChannel.PhysicalChannel, tuneChannel.Frequency, tuneChannel.ModulationType);
          string line = String.Format("physical channel:{0} frequency:{1} modulation:{2}", tuneChannel.PhysicalChannel, tuneChannel.Frequency, tuneChannel.ModulationType);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == minchan)
          {
            RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = line = String.Format("physical channel:{0} frequency:{1} modulation:{2}: No signal", tuneChannel.PhysicalChannel, tuneChannel.Frequency, tuneChannel.ModulationType);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = line = String.Format("physical channel:{0} frequency:{1} modulation:{2}: Nothing found", tuneChannel.PhysicalChannel, tuneChannel.Frequency, tuneChannel.ModulationType);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }
          int newChannels = 0;
          int updatedChannels = 0;
          bool exists;
          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            ATSCChannel channel = (ATSCChannel)channels[i];
            //TuningDetail currentDetail = layer.GetAtscChannel(channel);
            TuningDetail currentDetail = layer.GetChannel(channel);
            if (currentDetail != null)
              if (channel.Frequency != currentDetail.Frequency)
                currentDetail = null;
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddChannel(channel.Provider,channel.Name);
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
            if (dbChannel.IsRadio)
            {
              dbChannel.GrabEpg = false;
            }
            dbChannel.Persist();
            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
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
            layer.MapChannelToCard(card, dbChannel);
            line = line = String.Format("physical channel:{0} frequency:{1} modulation:{2} New:{3} Updated:{4}", tuneChannel.PhysicalChannel, tuneChannel.Frequency, tuneChannel.ModulationType, newChannels, updatedChannels);
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
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpComboBoxFrequencies.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }
  }
}