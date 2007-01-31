/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
  public partial class CardDvbC : SectionSettings
  {
    struct DVBCList
    {
      public int frequency;		 // frequency
      public ModulationType modulation;	 // modulation
      public int symbolrate;	 // symbol rate
    }

    int _cardNumber;
    DVBCList[] _dvbcChannels = new DVBCList[1000];
    int _channelCount = 0;
    bool _isScanning = false;
    bool _stopScanning = false;

    public CardDvbC()
      : this("DVBC")
    {
    }
    public CardDvbC(string name)
      : base(name)
    {
    }

    public CardDvbC(string name, int cardNumber)
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
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {
                _dvbcChannels[_channelCount].frequency = Int32.Parse(tpdata[0]);
                string mod = tpdata[1].ToUpper();
                switch (mod)
                {
                  case "1024QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod1024Qam;
                    break;
                  case "112QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod112Qam;
                    break;
                  case "128QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod128Qam;
                    break;
                  case "160QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod160Qam;
                    break;
                  case "16QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod16Qam;
                    break;
                  case "16VSB":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod16Vsb;
                    break;
                  case "192QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod192Qam;
                    break;
                  case "224QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod224Qam;
                    break;
                  case "256QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod256Qam;
                    break;
                  case "320QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod320Qam;
                    break;
                  case "384QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod384Qam;
                    break;
                  case "448QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod448Qam;
                    break;
                  case "512QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod512Qam;
                    break;
                  case "640QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod640Qam;
                    break;
                  case "64QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod64Qam;
                    break;
                  case "768QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod768Qam;
                    break;
                  case "80QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod80Qam;
                    break;
                  case "896QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod896Qam;
                    break;
                  case "8VSB":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod8Vsb;
                    break;
                  case "96QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod96Qam;
                    break;
                  case "AMPLITUDE":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModAnalogAmplitude;
                    break;
                  case "FREQUENCY":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModAnalogFrequency;
                    break;
                  case "BPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModBpsk;
                    break;
                  case "OQPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModOqpsk;
                    break;
                  case "QPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModQpsk;
                    break;
                  default:
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModNotSet;
                    break;
                }
                _dvbcChannels[_channelCount].symbolrate = Int32.Parse(tpdata[2]) / 1000;
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
      mpComboBoxCountry.Items.Clear();
      try
      {
        string[] files = System.IO.Directory.GetFiles("TuningParameters");
        for (int i = 0; i < files.Length; ++i)
        {
          string ext = System.IO.Path.GetExtension(files[i]).ToLower();
          if (ext != ".dvbc") continue;
          string fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
          mpComboBoxCountry.Items.Add(fileName);
        }
        mpComboBoxCountry.SelectedIndex = 0;
      }
      catch (Exception)
      {
        return;
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
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("dvbc" + _cardNumber.ToString() + "Country", "0").Value);


      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      mpComboBox1Cam.SelectedIndex = card.CamType;
      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbc" + _cardNumber.ToString() + "creategroups", "false").Value == "true");


    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbc" + _cardNumber.ToString() + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber.ToString() + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
      //DatabaseManager.Instance.SaveChanges();
    }



    private void mpButtonScanTv_Click_1(object sender, EventArgs e)
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
        Thread scanThread = new Thread(new ThreadStart(DoScan));
        scanThread.Start();
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
      User user = new User();
      user.CardId = _cardNumber;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        LoadList(String.Format(@"Tuningparameters\{0}.dvbc", mpComboBoxCountry.SelectedItem));
        if (_channelCount == 0) return;

        mpComboBoxCountry.Enabled = false;
        listViewStatus.Items.Clear();

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _channelCount; ++index)
        {
          if (_stopScanning) return;
          float percent = ((float)(index)) / _channelCount;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;


          DVBCChannel tuneChannel = new DVBCChannel();
          tuneChannel.Frequency = _dvbcChannels[index].frequency;
          tuneChannel.ModulationType = _dvbcChannels[index].modulation;
          tuneChannel.SymbolRate = _dvbcChannels[index].symbolrate;
          string line = String.Format("{0}tp- {1} {2} {3}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          if (index == 0)
          {
            RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          }

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2} {3}:No signal", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = String.Format("{0}tp- {1} {2} {3}:Nothing found", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;
          for (int i = 0; i < channels.Length; ++i)
          {
            DVBCChannel channel = (DVBCChannel)channels[i];
            IList channelList = layer.GetChannelsByName(channel.Name);
            Channel dbChannel = null;
            TuningDetail currentDetail = null;
            bool exists = false;
            if (channelList == null)
            {
              //channel does not exists. Add it
              dbChannel = layer.AddChannel(channel.Provider, channel.Name);
            }
            else
            {
              //one or more channel with name exists, check if provider exists also

              foreach (Channel ch in channelList)
              {
                TuningDetail detail = ch.ContainsProvider(channel.Provider);
                if (detail != null)
                {
                  dbChannel = ch;
                  if (detail.ChannelType == 2)
                  {
                    //provider exists for this type of transmission
                    currentDetail = detail;
                    exists = true;
                  }
                }
              }
              if (currentDetail != null)
              {
                //update tuning information
              }
              else if (dbChannel != null)
              {
                //add tuning detail
              }
              else
              {
                //add new channel
                bool channelTypeExists = false;
                foreach (Channel ch in channelList)
                {
                  if (ch.ContainsChannelType(2))
                  {
                    channelTypeExists = true;
                  }
                }
                if (channelTypeExists)
                {
                  dbChannel = layer.AddChannel(channel.Provider, channel.Name);
                }
                else
                {
                  dbChannel = (Channel)channelList[0];
                }
              }
            }

            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.FreeToAir = channel.FreeToAir;
            if (dbChannel.IsRadio)
            {
              dbChannel.GrabEpg = false;
            }
            dbChannel.SortOrder = 10000;
            if (channel.LogicalChannelNumber >= 1)
            {
              dbChannel.SortOrder = channel.LogicalChannelNumber;
            }
            dbChannel.Persist();

            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToGroup(dbChannel, channel.Provider);
            }
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
            line = String.Format("{0}tp- {1} {2} {3}:New:{4} Updated:{5}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate, newChannels, updatedChannels);
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
        mpComboBoxCountry.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));

      lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }

    private void mpComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      card.CamType = mpComboBox1Cam.SelectedIndex;
      card.Persist();
    }

    private void mpComboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpBeveledLine1_Load(object sender, EventArgs e)
    {

    }

  }
}