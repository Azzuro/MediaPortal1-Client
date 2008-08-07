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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Log;

using Gentle.Common;
using Gentle.Framework;
using SetupControls;
namespace SetupTv.Sections
{
  public partial class TestService : SectionSettings
  {
    IList _cards = null;
    //Player _player;
    public TestService()
      : this("Manual Control")
    {
    }

    public TestService(string name)
      : base(name)
    {
      InitializeComponent();
      mpComboBoxChannels.ImageList = imageList1;
      this.DoubleBuffered = true;
    }


    public override void OnSectionActivated()
    {
      _cards = Card.ListAll();
      base.OnSectionActivated();
      mpGroupBox1.Visible = false;
      RemoteControl.Instance.EpgGrabberEnabled = true;

      comboBoxGroups.Items.Clear();
      IList groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
        comboBoxGroups.Items.Add(new ComboBoxExItem(group.GroupName,-1,group.IdGroup));
      if (comboBoxGroups.Items.Count==0)
        comboBoxGroups.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      comboBoxGroups.SelectedIndex = 0;

      timer1.Enabled = true;

      mpListView1.Items.Clear();

      buttonRestart.Visible = false;
      mpButtonRec.Enabled = false;
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      timer1.Enabled = false;
      if (RemoteControl.IsConnected)
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
      }
    }

    private void TestService_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonTimeShift_Click(object sender, EventArgs e)
    {

      if (ServiceHelper.IsStopped) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;

      TvServer server = new TvServer();
      VirtualCard card = GetCardTimeShiftingChannel(id);
      if (card != null)
      {
        card.StopTimeShifting();
        mpButtonRec.Enabled = false;
      }
      else
      {
        User user = new User();
        user.Name = "setuptv";
        user.IsAdmin = true;          //fixing mantis bug 1513: recordings you start in manual control can not be stopped  
        //user.Name = "setuptv" + id.ToString();
        TvResult result = server.StartTimeShifting(ref user, id, out card);
        if (result != TvResult.Succeeded)
        {
          switch (result)
          {
            case TvResult.CardIsDisabled:
              MessageBox.Show(this, "Card is not enabled");
              break;
            case TvResult.AllCardsBusy:
              MessageBox.Show(this, "All cards are busy");
              break;
            case TvResult.ChannelIsScrambled:
              MessageBox.Show(this, "Channel is scrambled");
              break;
            case TvResult.NoVideoAudioDetected:
              MessageBox.Show(this, "No Video/Audio detected");
              break;
            case TvResult.UnableToStartGraph:
              MessageBox.Show(this, "Unable to create/start graph");
              break;
            case TvResult.ChannelNotMappedToAnyCard:
              MessageBox.Show(this, "Channel is not mapped to any card");
              break;
            case TvResult.NoTuningDetails:
              MessageBox.Show(this, "No tuning information available for this channel");
              break;
            case TvResult.UnknownChannel:
              MessageBox.Show(this, "Unknown channel");
              break;
            case TvResult.UnknownError:
              MessageBox.Show(this, "Unknown error occured");
              break;
            case TvResult.ConnectionToSlaveFailed:
              MessageBox.Show(this, "Cannot connect to slave server");
              break;
            case TvResult.NotTheOwner:
              MessageBox.Show(this, "Failed since card is in use and we are not the owner");
              break;
          }
        }
        else
        {
          mpButtonRec.Enabled = true;
        }
      }
    }

    private void mpButtonRec_Click(object sender, EventArgs e)
    {
      if (ServiceHelper.IsStopped) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;
      TvServer server = new TvServer();
      VirtualCard card = GetCardRecordingChannel(id);
      if (card != null)
      {
        card.StopRecording();
        mpButtonRec.Enabled = false;
        mpButtonTimeShift.Enabled = true;
      }
      else
      {
        card = GetCardTimeShiftingChannel(id);
        if (card != null)
        {
          string fileName;
          fileName = String.Format(@"{0}\{1}.mpg", card.RecordingFolder, Utils.MakeFileName(channel));
          card.StartRecording(ref fileName, true, 0);
          mpButtonTimeShift.Enabled = false;
        }
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsRunning)
      {
        buttonRestart.Text = "Start Service";
        mpButtonReGrabEpg.Enabled = false;
        return;
      }
      buttonRestart.Text = "Stop Service";
      if (!buttonRestart.Visible)
        buttonRestart.Visible = true;
      mpButtonReGrabEpg.Enabled = true;
      UpdateCardStatus();

      if (mpComboBoxChannels.SelectedItem != null)
      {
        TvServer server = new TvServer();
        int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;
        VirtualCard card = GetCardTimeShiftingChannel(id);
        if (card != null)
        {
          mpGroupBox1.Visible = true;
          mpGroupBox1.Text = String.Format("Status of card {0}", card.Name);
          if (card.IsTunerLocked)
            mpLabelTunerLocked.Text = "yes";
          else
            mpLabelTunerLocked.Text = "no";
          progressBarLevel.Value = Math.Min(100, card.SignalLevel);
          progressBarQuality.Value = Math.Min(100, card.SignalQuality);


          mpLabelRecording.Text = card.RecordingFileName;


          mpLabelTimeShift.Text = card.TimeShiftFileName;
          mpLabelChannel.Text = card.Channel.ToString();
          if (card.IsRecording)
            mpButtonRec.Text = "Stop Rec/TimeShift";
          else
            mpButtonRec.Text = "Record";

          if (card.IsTimeShifting)
            mpButtonTimeShift.Text = "Stop TimeShift";
          else
            mpButtonTimeShift.Text = "Start TimeShift";

          return;
        }
        else
        {
          mpGroupBox1.Visible = false;
        }
      }
      mpLabelTunerLocked.Text = "no";
      progressBarLevel.Value = 0;
      progressBarQuality.Value = 0;
      mpLabelRecording.Text = "";
      mpLabelTimeShift.Text = "";
      mpButtonRec.Text = "Record";
      mpButtonTimeShift.Text = "Start TimeShift";
    }

    private void mpCheckBoxTimeShift_CheckedChanged(object sender, EventArgs e)
    {

    }
    void UpdateCardStatus()
    {

      ColumnHeaderAutoResizeStyle oldheadsize = ColumnHeaderAutoResizeStyle.HeaderSize;
      if (ServiceHelper.IsStopped) return;
      if (_cards == null) return;
      if (_cards.Count == 0) return;
      try
      {
        TvServer server = new TvServer();
        ListViewItem item;
        int cardNo = 0;
        int off = 0;
        bool userFound = false;
        foreach (Card card in _cards)
        {
          cardNo++;
          User user = new User();
          user.CardId = card.IdCard;          
          if (off >= mpListView1.Items.Count)
          {
            item = mpListView1.Items.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
          }
          else
          {
            item = mpListView1.Items[off];
          }

          bool cardPresent = RemoteControl.Instance.CardPresent(card.IdCard);
          if (!cardPresent)
          {
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[1].Text = "n/a";
            item.SubItems[2].Text = "n/a";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }

          VirtualCard vcard = new VirtualCard(user);
          item.SubItems[0].Text = cardNo.ToString();
          item.SubItems[1].Text = vcard.Type.ToString();

          if (card.Enabled == false)
          {
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[1].Text = vcard.Type.ToString();
            item.SubItems[2].Text = "disabled";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }                    
          
          User[] usersForCard = RemoteControl.Instance.GetUsersForCard(card.IdCard);
          if (usersForCard == null)
          {
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            item.SubItems[2].Text = tmp;
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }
          if (usersForCard.Length == 0)
          {
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            item.SubItems[2].Text = tmp;
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }
          

          userFound = false;
          for (int i = 0; i < usersForCard.Length; ++i)
          {
            string tmp = "idle";
            // Check if the card id fits. Hybrid cards share the context and therefor have
            // the same users.
            if (usersForCard[i].CardId != card.IdCard)
            {
              continue;
            }
            userFound = true;
            vcard = new VirtualCard(usersForCard[i]);
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[1].Text = vcard.Type.ToString();
            if (vcard.IsTimeShifting) tmp = "Timeshifting";
            if (vcard.IsRecording && vcard.User.IsAdmin) tmp = "Recording";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            if (vcard.IsTimeShifting && vcard.IsGrabbingEpg) tmp = " Timeshifting (Grabbing EPG)";
            if (vcard.IsRecording && vcard.IsGrabbingEpg) tmp = " Recording (Grabbing EPG)";
            item.SubItems[2].Text = tmp;
            if (vcard.IsScrambled) tmp = "yes";
            else tmp = "no";
            item.SubItems[4].Text = tmp;
            item.SubItems[3].Text = vcard.ChannelName;
            item.SubItems[5].Text = usersForCard[i].Name;
            item.SubItems[6].Text = card.Name;
            off++;


            if (off >= mpListView1.Items.Count)
            {
              item = mpListView1.Items.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
            }
            else
            {
              item = mpListView1.Items[off];
            }
          }
          // If we haven't found a user that fits, than it is a hybrid card which is inactive
          // This means that the card is idle.
          if (!userFound)
          {
            item.SubItems[2].Text = "idle";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
          }
        }
        for (int i = off; i < mpListView1.Items.Count; ++i)
        {
          item = mpListView1.Items[i];
          item.SubItems[0].Text = "";
          item.SubItems[1].Text = "";
          item.SubItems[2].Text = "";
          item.SubItems[3].Text = "";
          item.SubItems[4].Text = "";
          item.SubItems[5].Text = "";
          item.SubItems[6].Text = "";
        }

        if (oldheadsize != ColumnHeaderAutoResizeStyle.HeaderSize)
          mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    private void buttonRestart_Click(object sender, EventArgs e)
    {
      try
      {
        buttonRestart.Enabled = false;
        timer1.Enabled = false;

        RemoteControl.Clear();
        if (ServiceHelper.IsStopped)
        {
          if (ServiceHelper.Start())
          {
          }
        }
        else if (ServiceHelper.IsRunning)
        {
          if (ServiceHelper.Stop())
          {
          }
        }
        RemoteControl.Clear();
        timer1.Enabled = true;
      }
      finally
      {
        buttonRestart.Enabled = true;
      }
    }

    private void mpButtonReGrabEpg_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value != "yes")
      {
        MessageBox.Show("EPG grabber is disabled. Please enable EPG grabbing while idle.");
        return;
      }
      RemoteControl.Instance.EpgGrabberEnabled = false;
      Gentle.Framework.Broker.Execute("delete from Program");
      IList channels = Channel.ListAll();
      foreach (Channel ch in channels)
      {
        ch.LastGrabTime = Schedule.MinSchedule;
        ch.Persist();
      }

      RemoteControl.Instance.EpgGrabberEnabled = true;
      MessageBox.Show("EPG grabber will restart in a few seconds..");
    }


    /// <summary>
    /// returns the virtualcard which is timeshifting the channel specified
    /// </summary>
    /// <param name="channelId">Id of the channel</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardTimeShiftingChannel(int channelId)
    {
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        if (!TvControl.RemoteControl.Instance.CardPresent(card.IdCard)) continue;
        User[] usersForCard = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (usersForCard == null) continue;
        if (usersForCard.Length == 0) continue;
        for (int i = 0; i < usersForCard.Length; ++i)
        {
          if (usersForCard[i].IdChannel == channelId)
          {

            VirtualCard vcard = new VirtualCard(usersForCard[i], RemoteControl.HostName);
            if (vcard.IsTimeShifting)
            {
              vcard.RecordingFolder = card.RecordingFolder;
              return vcard;
            }
          }
        }
      }
      return null;
    }

    /// <summary>
    /// returns the virtualcard which is recording the channel specified
    /// </summary>
    /// <param name="channelId">Id of the channel</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardRecordingChannel(int channelId)
    {
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        if (!TvControl.RemoteControl.Instance.CardPresent(card.IdCard)) continue;
        User[] usersForCard = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (usersForCard == null) continue;
        if (usersForCard.Length == 0) continue;
        for (int i = 0; i < usersForCard.Length; ++i)
        {
          if (usersForCard[i].IdChannel == channelId)
          {
            VirtualCard vcard = new VirtualCard(usersForCard[i], RemoteControl.HostName);
            if (vcard.IsRecording)
            {
              vcard.RecordingFolder = card.RecordingFolder;
              return vcard;
            }
          }
        }
      }
      return null;
    }

    private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxExItem idItem = (ComboBoxExItem)comboBoxGroups.Items[comboBoxGroups.SelectedIndex];
      mpComboBoxChannels.Items.Clear();
      if (idItem.Id == -1)
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        foreach (Channel ch in channels)
        {
          if (ch.IsTv == false) continue;
          int imageIndex = 1;
          if (ch.FreeToAir == false)
            imageIndex = 2;
          ComboBoxExItem item = new ComboBoxExItem(ch.DisplayName, imageIndex, ch.IdChannel);

          mpComboBoxChannels.Items.Add(item);

        }
      }
      else
      {
        ChannelGroup group = ChannelGroup.Retrieve(idItem.Id);
        IList maps = group.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          Channel ch = map.ReferencedChannel();
          if (ch.IsTv == false) continue;
          int imageIndex = 1;
          if (ch.FreeToAir == false)
            imageIndex = 2;
          ComboBoxExItem item = new ComboBoxExItem(ch.DisplayName, imageIndex, ch.IdChannel);
          mpComboBoxChannels.Items.Add(item);
        }
      }
      if (mpComboBoxChannels.Items.Count > 0)
        mpComboBoxChannels.SelectedIndex = 0;
    }
  }
}
