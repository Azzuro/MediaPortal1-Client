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
using System.Globalization;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;
using Gentle.Common;
using Gentle.Framework;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using similaritymetrics;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvChannels : SectionSettings
  {
    public class CardInfo
    {
      protected Card _card;

      public Card Card
      {
        get
        {
          return _card;
        }
      }

      public CardInfo(Card card)
      {
        _card = card;
      }

      public override string ToString()
      {
        return _card.Name.ToString();
      }
    }
    private MPListViewStringColumnSorter lvwColumnSorter;
    private MPListViewStringColumnSorter lvwColumnSorter2;
    private MPListViewStringColumnSorter lvwColumnSorter3;
    bool _redrawTab1 = false;
    public TvChannels()
      : this("TV Channels")
    {
    }

    public TvChannels(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter3 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.Descending;
      lvwColumnSorter2.OrderType = MPListViewStringColumnSorter.OrderTypes.AsValue;
      this.mpListView1.ListViewItemSorter = lvwColumnSorter;
    }

    public override void OnSectionDeActivated()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        if (ch.SortOrder != i + 1)
        {
          ch.SortOrder = i + 1;
        }
        ch.VisibleInGuide = mpListView1.Items[i].Checked;
        ch.Persist();
      }

      //DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      base.OnSectionDeActivated();
    }

    private void UpdateMenu()
    {
      while (tabControl1.TabPages.Count > 1)
      {
        tabControl1.TabPages.RemoveAt(1);
      }
      addToFavoritesToolStripMenuItem.DropDownItems.Clear();
      IList groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.GroupName);
        item.Tag = group;
        item.Click += new EventHandler(OnAddToFavoritesMenuItem_Click);
        addToFavoritesToolStripMenuItem.DropDownItems.Add(item);
        TabPage page = new TabPage(group.GroupName);
        page.Controls.Add(new ChannelsInGroupControl());
        page.Tag = group;
        tabControl1.TabPages.Add(page);
      }
      ToolStripMenuItem itemNew = new ToolStripMenuItem("New...");
      itemNew.Click += new EventHandler(OnAddToFavoritesMenuItem_Click);
      addToFavoritesToolStripMenuItem.DropDownItems.Add(itemNew);
    }

    public override void OnSectionActivated()
    {
      UpdateMenu();
      _redrawTab1 = false;

      base.OnSectionActivated();

      RefreshAllChannels();
    }

    private void RefreshAllChannels()
    {
      IList dbsCards = Card.ListAll();
      Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
      foreach (Card card in dbsCards)
      {
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }      

      mpListView1.BeginUpdate();
      mpListView1.Items.Clear();
      IList chs = Channel.ListAll();
      int channelCount = 0;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      IList allmaps = ChannelMap.ListAll();
      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        bool webstream = false;
        bool notmapped = true;
        if (ch.IsTv == false) continue;
        channelCount++;
        if (ch.IsWebstream())
        {
          webstream = true;
          notmapped = false;
        }
        if (notmapped)
        {
          IList maps = ch.ReferringChannelMap();
          foreach (ChannelMap map in maps)
          {
            if (cards.ContainsKey(map.IdCard))
            {
              CardType type = cards[map.IdCard];
              switch (type)
              {
                case CardType.Analog: analog = true; notmapped = false; break;
                case CardType.DvbC: dvbc = true; notmapped = false; break;
                case CardType.DvbT: dvbt = true; notmapped = false; break;
                case CardType.DvbS: dvbs = true; notmapped = false; break;
                case CardType.Atsc: atsc = true; notmapped = false; break;
              }
            }
          }
        }
        StringBuilder builder = new StringBuilder();

        if (analog)
        {
          builder.Append("Analog");
        }
        if (notmapped)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("Channel not mapped to a card");
        }
        if (dvbc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-C");
        }
        if (dvbt)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-T");
        }
        if (dvbs)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-S");
        }
        if (atsc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("ATSC");
        }
        if (webstream)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("Webstream");
        }
        int imageIndex = 1;
        if (ch.FreeToAir == false)
          imageIndex = 2;

        ListViewItem item = new ListViewItem(ch.DisplayName, imageIndex);
        item.SubItems.Add("-");
        item.Checked = ch.VisibleInGuide;
        item.Tag = ch;
        item.SubItems.Add(builder.ToString());
        string provider = "";
        foreach (TuningDetail detail in ch.ReferringTuningDetail())
        {
          provider += String.Format("{0},", detail.Provider);
          float frequency;
          switch (detail.ChannelType)
          {
            case 0://Analog
              if (detail.VideoSource == (int)AnalogChannel.VideoInputType.Tuner)
              {
                frequency = detail.Frequency;
                frequency /= 1000000.0f;
                item.SubItems.Add(String.Format("#{0} {1} MHz", detail.ChannelNumber, frequency.ToString("f2")));
              }
              else
              {
                item.SubItems.Add(detail.VideoSource.ToString());
              }
              break;

            case 1://ATSC
              item.SubItems.Add(String.Format("{0} {1}:{2}", detail.ChannelNumber, detail.MajorChannel, detail.MinorChannel));
              break;

            case 2:// DVB-C
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), detail.Symbolrate));
              break;

            case 3:// DVB-S
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz {1}", frequency.ToString("f2"), (((Polarisation)detail.Polarisation))));
              break;

            case 4:// DVB-T
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), detail.Bandwidth));
              break;

            case 5:// Webstream
              item.SubItems.Add(detail.Url);
              break;
          }
        }
        if (provider.Length > 1) provider = provider.Substring(0, provider.Length - 1);
        item.SubItems[1].Text = (provider);
        items.Add(item);
      }
      mpListView1.Items.AddRange(items.ToArray());
      mpListView1.EndUpdate();
      mpLabelChannelCount.Text = String.Format("Total channels: {0}", channelCount);
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void OnAddToFavoritesMenuItem_Click(object sender, EventArgs e)
    {
      ChannelGroup group;
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }
        group = new ChannelGroup(dlg.GroupName, 9999);
        group.Persist();
        UpdateMenu();
      }
      else
      {
        group = (ChannelGroup)menuItem.Tag;
      }

      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[indexes[i]];
        Channel channel = (Channel)item.Tag;
        layer.AddChannelToGroup(channel, group.GroupName);
      }
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Clearing all tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      IList channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        if (channel.IsTv)
        {
          Gentle.Framework.Broker.Execute("delete from TvMovieMapping WHERE idChannel=" + channel.IdChannel.ToString());
          channel.Delete();
        }
      }
      dlg.Close();
      /*Gentle.Framework.Broker.Execute("delete from history");
      Gentle.Framework.Broker.Execute("delete from tuningdetail");
      Gentle.Framework.Broker.Execute("delete from GroupMap");
      Gentle.Framework.Broker.Execute("delete from Channelmap");
      Gentle.Framework.Broker.Execute("delete from Recording");
      Gentle.Framework.Broker.Execute("delete from CanceledSchedule");
      Gentle.Framework.Broker.Execute("delete from Schedule");
      Gentle.Framework.Broker.Execute("delete from Program");
      Gentle.Framework.Broker.Execute("delete from Channel");
      mpListView1.BeginUpdate();*/
      /*
      IList details = TuningDetail.ListAll();
      foreach (TuningDetail detail in details) detail.Remove();

      IList groupmaps = GroupMap.ListAll();
      foreach (GroupMap groupmap in groupmaps) groupmap.Remove();

      IList channelMaps = ChannelMap.ListAll();
      foreach (ChannelMap channelMap in channelMaps) channelMap.Remove();

      IList recordings = Recording.ListAll();
      foreach (Recording recording in recordings) recording.Remove();

      IList canceledSchedules = CanceledSchedule.ListAll();
      foreach (CanceledSchedule canceledSchedule in canceledSchedules) canceledSchedule.Remove();

      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules) schedule.Remove();

      IList programs = Program.ListAll();
      foreach (Program program in programs) program.Remove();

      IList channels = Channel.ListAll();
      foreach (Channel channel in channels) channel.Remove();
      */

      //mpListView1.EndUpdate();
      OnSectionActivated();
    }

    private void TvChannels_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonClearEncrypted_Click(object sender, EventArgs e)
    {
      //@ TODO : does not work
      IList channels = Channel.ListAll();

      mpListView1.BeginUpdate();
      for (int i = 0; i < channels.Count; ++i)
      {
        Channel ch = (Channel)channels[i];
        if (ch.IsTv)
        {
          for (int x = 0; x < ch.ReferringTuningDetail().Count; x++)
          {
            TuningDetail detail = (TuningDetail)ch.ReferringTuningDetail()[x];
            if (detail.FreeToAir == false)
            {
              ch.Delete();
              break;
            }
          }
        }
      }
      mpListView1.EndUpdate();
      OnSectionActivated();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      IList schedules = Schedule.ListAll();
      TvServer server = new TvServer();

      //Since it takes a very long time to add channels, make sure the user really wants to delete them
      if (mpListView1.SelectedItems.Count > 0)
      {
          string holder = String.Format("Are you sure you want to delete these {0:d} channels?", mpListView1.SelectedItems.Count);

          if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
          {
              mpListView1.EndUpdate();
              return;
          }
      }

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;

        //also delete any still active schedules
        if (schedules != null)
        {
          for (int i = schedules.Count - 1; i > -1; i--)
          {
            Schedule schedule = (Schedule)schedules[i];
            if (schedule.IdChannel == channel.IdChannel)
            {
              server.StopRecordingSchedule(schedule.IdSchedule);
              schedule.Delete();
              schedules.RemoveAt(i);
            }
          }
        }

        channel.Delete();
        mpListView1.Items.Remove(item);
      }

      mpListView1.EndUpdate();
      ReOrder();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index - 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      if (mpListView1.Items.Count < 2) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListView1.Items[index];
        mpListView1.Items.RemoveAt(index);
        if (index + 1 < mpListView1.Items.Count)
          mpListView1.Items.Insert(index + 1, item);
        else
          mpListView1.Items.Add(item);
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        //mpListView1.Items[i].Text = (i + 1).ToString();

        Channel channel = (Channel)mpListView1.Items[i].Tag;
        if (channel.SortOrder != i)
        {
          channel.SortOrder = i;
          channel.Persist();
        }
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      try
      {
        int oldIndex = e.Item;
        ListViewItem item = mpListView1.Items[oldIndex];
        int newIndex = (Int32.Parse(e.Label) - 1);
        if (newIndex == oldIndex) return;

        mpListView1.Items.RemoveAt(oldIndex);
        mpListView1.Items.Insert(newIndex, item);
        ReOrder();
        e.CancelEdit = true;
      }
      catch (Exception)
      {
      }
    }

    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = channel;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        channel.Persist();
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          if (detail.Name != channel.Name)
          {
            detail.Name = channel.Name;
            detail.Persist();
          }
        }
        OnSectionActivated();
      }
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListView1.Sort();
      ReOrder();
    }

    private void AddAttribute(XmlNode node, string tagName, string tagValue)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(tagName);
      attr.InnerText = tagValue;
      node.Attributes.Append(attr);
    }

    private void AddAttribute(XmlNode node, string tagName, int tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }

    // store DateTime Values as strings. Improves readability
    private void AddAttribute(XmlNode node, string tagName, DateTime tagValue)
    {
      AddAttribute(node, tagName, String.Format("{0}-{1}-{2} {3}:{4}:{5}", tagValue.Year, tagValue.Month, tagValue.Day, tagValue.Hour, tagValue.Minute, tagValue.Second));
    }

    private void AddAttribute(XmlNode node, string tagName, bool tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }

    private void Export()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlNode rootElement = xmlDoc.CreateElement("tvserver");
      AddAttribute(rootElement, "version", "1.0");

      XmlNode nodeServers = xmlDoc.CreateElement("servers");
      IList servers = Server.ListAll();
      foreach (Server server in servers)
      {
        XmlNode nodeServer = xmlDoc.CreateElement("server");
        AddAttribute(nodeServer, "HostName", server.HostName);
        AddAttribute(nodeServer, "IdServer", server.IdServer);
        AddAttribute(nodeServer, "IsMaster", server.IsMaster);

        XmlNode nodeCards = xmlDoc.CreateElement("cards");
        IList cards = Card.ListAll();
        foreach (Card card in cards)
        {
          XmlNode nodeCard = xmlDoc.CreateElement("card");
          AddAttribute(nodeCard, "IdCard", card.IdCard);
          AddAttribute(nodeCard, "DevicePath", card.DevicePath);
          AddAttribute(nodeCard, "Enabled", card.Enabled);
          AddAttribute(nodeCard, "CamType", card.CamType);
          AddAttribute(nodeCard, "GrabEPG", card.GrabEPG);
          AddAttribute(nodeCard, "LastEpgGrab", card.LastEpgGrab);
          AddAttribute(nodeCard, "Name", card.Name);
          AddAttribute(nodeCard, "Priority", card.Priority);
          AddAttribute(nodeCard, "RecordingFolder", card.RecordingFolder);
          nodeCards.AppendChild(nodeCard);
        }
        nodeServer.AppendChild(nodeCards);
        nodeServers.AppendChild(nodeServer);
      }
      rootElement.AppendChild(nodeServers);

      XmlNode nodechannels = xmlDoc.CreateElement("channels");
      IList channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        XmlNode nodechannel = xmlDoc.CreateElement("channel");
        AddAttribute(nodechannel, "Name", channel.Name);
        AddAttribute(nodechannel, "GrabEpg", channel.GrabEpg);
        AddAttribute(nodechannel, "IdChannel", channel.IdChannel);
        AddAttribute(nodechannel, "IsRadio", channel.IsRadio);
        AddAttribute(nodechannel, "IsTv", channel.IsTv);
        AddAttribute(nodechannel, "LastGrabTime", channel.LastGrabTime);
        AddAttribute(nodechannel, "SortOrder", channel.SortOrder);
        AddAttribute(nodechannel, "TimesWatched", channel.TimesWatched);
        AddAttribute(nodechannel, "TotalTimeWatched", channel.TotalTimeWatched);
        AddAttribute(nodechannel, "VisibleInGuide", channel.VisibleInGuide);
        AddAttribute(nodechannel, "FreeToAir", channel.FreeToAir);
        AddAttribute(nodechannel, "DisplayName", channel.DisplayName);

        XmlNode nodeMaps = xmlDoc.CreateElement("mappings");
        foreach (ChannelMap map in channel.ReferringChannelMap())
        {
          XmlNode nodeMap = xmlDoc.CreateElement("map");
          AddAttribute(nodeMap, "IdCard", map.IdCard);
          AddAttribute(nodeMap, "IdChannel", map.IdChannel);
          AddAttribute(nodeMap, "IdChannelMap", map.IdChannelMap);
          nodeMaps.AppendChild(nodeMap);
        }
        nodechannel.AppendChild(nodeMaps);

        XmlNode nodeTuningDetails = xmlDoc.CreateElement("TuningDetails");
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          XmlNode nodeTune = xmlDoc.CreateElement("tune");
          AddAttribute(nodeTune, "IdChannel", detail.IdChannel);
          AddAttribute(nodeTune, "IdTuning", detail.IdTuning);
          AddAttribute(nodeTune, "AudioPid", detail.AudioPid);
          AddAttribute(nodeTune, "Bandwidth", detail.Bandwidth);
          AddAttribute(nodeTune, "ChannelNumber", detail.ChannelNumber);
          AddAttribute(nodeTune, "ChannelType", (int)detail.ChannelType);
          AddAttribute(nodeTune, "CountryId", (int)detail.CountryId);
          AddAttribute(nodeTune, "Diseqc", (int)detail.Diseqc);
          AddAttribute(nodeTune, "FreeToAir", detail.FreeToAir);
          AddAttribute(nodeTune, "Frequency", detail.Frequency);
          AddAttribute(nodeTune, "MajorChannel", detail.MajorChannel);
          AddAttribute(nodeTune, "MinorChannel", detail.MinorChannel);
          AddAttribute(nodeTune, "Modulation", detail.Modulation);
          AddAttribute(nodeTune, "Name", detail.Name);
          AddAttribute(nodeTune, "NetworkId", detail.NetworkId);
          AddAttribute(nodeTune, "PcrPid", detail.PcrPid);
          AddAttribute(nodeTune, "PmtPid", detail.PmtPid);
          AddAttribute(nodeTune, "Polarisation", (int)detail.Polarisation);
          AddAttribute(nodeTune, "Provider", detail.Provider);
          AddAttribute(nodeTune, "ServiceId", (int)detail.ServiceId);
          AddAttribute(nodeTune, "SwitchingFrequency", (int)detail.SwitchingFrequency);
          AddAttribute(nodeTune, "Symbolrate", (int)detail.Symbolrate);
          AddAttribute(nodeTune, "TransportId", (int)detail.TransportId);
          AddAttribute(nodeTune, "TuningSource", (int)detail.TuningSource);
          AddAttribute(nodeTune, "VideoPid", (int)detail.VideoPid);
          AddAttribute(nodeTune, "VideoSource", (int)detail.VideoSource);
          AddAttribute(nodeTune, "SatIndex", (int)detail.SatIndex);
          AddAttribute(nodeTune, "InnerFecRate", (int)detail.InnerFecRate);
          AddAttribute(nodeTune, "Band", (int)detail.Band);
          AddAttribute(nodeTune, "Pilot", (int)detail.Pilot);
          AddAttribute(nodeTune, "RollOff", (int)detail.RollOff);
          AddAttribute(nodeTune, "Url", detail.Url);
          AddAttribute(nodeTune, "Bitrate", detail.Bitrate);
          nodeTuningDetails.AppendChild(nodeTune);
        }
        nodechannel.AppendChild(nodeTuningDetails);

        nodechannels.AppendChild(nodechannel);
      }
      rootElement.AppendChild(nodechannels);
      // exporting the schedules
      XmlNode nodeSchedules = xmlDoc.CreateElement("schedules");
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        XmlNode nodeSchedule = xmlDoc.CreateElement("schedule");
        AddAttribute(nodeSchedule, "ChannelName", schedule.ReferencedChannel().Name);
        AddAttribute(nodeSchedule, "ProgramName", schedule.ProgramName);
        AddAttribute(nodeSchedule, "StartTime", schedule.StartTime);
        AddAttribute(nodeSchedule, "EndTime", schedule.EndTime);
        AddAttribute(nodeSchedule, "KeepDate", schedule.KeepDate);
        AddAttribute(nodeSchedule, "PreRecordInterval", schedule.PreRecordInterval);
        AddAttribute(nodeSchedule, "PostRecordInterval", schedule.PostRecordInterval);
        AddAttribute(nodeSchedule, "Priority", schedule.Priority);
        AddAttribute(nodeSchedule, "Quality", schedule.Quality);
        AddAttribute(nodeSchedule, "Directory", schedule.Directory);
        AddAttribute(nodeSchedule, "KeepMethod", schedule.KeepMethod);
        AddAttribute(nodeSchedule, "MaxAirings", schedule.MaxAirings);
        AddAttribute(nodeSchedule, "RecommendedCard", schedule.RecommendedCard);
        AddAttribute(nodeSchedule, "ScheduleType", schedule.ScheduleType);
        AddAttribute(nodeSchedule, "Series", schedule.Series);
        nodeSchedules.AppendChild(nodeSchedule);
      }
      rootElement.AppendChild(nodeSchedules);
      //exporting channel groups
      XmlNode nodeChannelGroups = xmlDoc.CreateElement("channelgroups");
      IList channelgroups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in channelgroups)
      {
        XmlNode nodeChannelGroup = xmlDoc.CreateElement("channelgroup");
        AddAttribute(nodeChannelGroup, "GroupName", group.GroupName);
        AddAttribute(nodeChannelGroup, "SortOrder", group.SortOrder.ToString());
        XmlNode nodeGroupMap = xmlDoc.CreateElement("mappings");
        IList maps = group.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          XmlNode nodeMap = xmlDoc.CreateElement("map");
          AddAttribute(nodeMap, "ChannelName", map.ReferencedChannel().Name);
          AddAttribute(nodeMap, "SortOrder", map.SortOrder.ToString());
          nodeGroupMap.AppendChild(nodeMap);
        }
        nodeChannelGroup.AppendChild(nodeGroupMap);
        nodeChannelGroups.AppendChild(nodeChannelGroup);
      }
      rootElement.AppendChild(nodeChannelGroups);
      xmlDoc.AppendChild(rootElement);
      xmlDoc.Save("export.xml");
      MessageBox.Show(this, "Channels, channel groups and schedules exported to 'export.xml'");
    }

    private void mpButtonExpert_Click(object sender, EventArgs e)
    {
      Export();
    }

    private string GetNodeAttribute(XmlNode node, string attribute, string defaultValue)
    {
      if (node.Attributes[attribute] == null)
        return defaultValue;
      else
        return node.Attributes[attribute].Value;
    }

    private void mpButtonImport_Click(object sender, EventArgs e)
    {
      openFileDialog1.CheckFileExists = true;
      openFileDialog1.DefaultExt = "xml";
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.Title = "Load channels, channel groups and schedules";
      openFileDialog1.FileName = "export.xml";
      openFileDialog1.AddExtension = true;
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog(this) != DialogResult.OK) return;
      NotifyForm dlg = new NotifyForm("Importing tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      CountryCollection collection = new CountryCollection();
      TvBusinessLayer layer = new TvBusinessLayer();
      int channelCount = 0;
      int scheduleCount = 0;
      int channelGroupCount = 0;
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(openFileDialog1.FileName);
        XmlNodeList serverList = doc.SelectNodes("/tvserver/servers/servers");
        XmlNodeList channelList = doc.SelectNodes("/tvserver/channels/channel");
        XmlNodeList scheduleList = doc.SelectNodes("/tvserver/schedules/schedule");
        XmlNodeList channelGroupList = doc.SelectNodes("/tvserver/channelgroups/channelgroup");
        XmlNodeList cardList = doc.SelectNodes("/tvserver/servers/servers/cards/card");
        foreach (XmlNode nodeChannel in channelList)
        {
          channelCount++;
          XmlNodeList tuningList = nodeChannel.SelectNodes("TuningDetails/tune");
          XmlNodeList mappingList = nodeChannel.SelectNodes("mappings/map");
          string name = nodeChannel.Attributes["Name"].Value;
          bool grabEpg = (GetNodeAttribute(nodeChannel, "GrabEpg", "True") == "True");
          bool isRadio = (GetNodeAttribute(nodeChannel, "IsRadio", "False") == "True");
          bool isTv = (GetNodeAttribute(nodeChannel, "IsTv", "True") == "True");
          DateTime lastGrabTime = DateTime.ParseExact(GetNodeAttribute(nodeChannel, "LastGrabTime", "01.01.1900"), "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          int sortOrder = Int32.Parse(GetNodeAttribute(nodeChannel, "SortOrder", "0"));
          int timesWatched = Int32.Parse(GetNodeAttribute(nodeChannel, "TimesWatched", "0"));
          DateTime totalTimeWatched = DateTime.ParseExact(GetNodeAttribute(nodeChannel, "TotalTimeWatched", "01.01.1900"), "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          bool visibileInGuide = (GetNodeAttribute(nodeChannel, "VisibleInGuide", "True") == "True");
          bool FreeToAir = (GetNodeAttribute(nodeChannel, "FreeToAir", "True") == "True");
          string displayName = GetNodeAttribute(nodeChannel, "DisplayName", name);

          Channel dbChannel = layer.AddChannel("", name);
          dbChannel.GrabEpg = grabEpg;
          dbChannel.IsRadio = isRadio;
          dbChannel.IsTv = isTv;
          dbChannel.LastGrabTime = lastGrabTime;
          dbChannel.SortOrder = sortOrder;
          dbChannel.TimesWatched = timesWatched;
          dbChannel.TotalTimeWatched = totalTimeWatched;
          dbChannel.VisibleInGuide = visibileInGuide;
          dbChannel.FreeToAir = FreeToAir;
          dbChannel.DisplayName = displayName;
          dbChannel.Persist();
          foreach (XmlNode nodeMap in mappingList)
          {
            int idCard = Int32.Parse(nodeMap.Attributes["IdCard"].Value);
            XmlNode nodeCard = doc.SelectSingleNode(String.Format("/tvserver/servers/server/cards/card[@IdCard={0}]", idCard));
            Card dbCard = layer.GetCardByDevicePath(nodeCard.Attributes["DevicePath"].Value);
            if (dbCard != null)
            {
              layer.MapChannelToCard(dbCard, dbChannel, false);
            }
          }
          foreach (XmlNode nodeTune in tuningList)
          {
            int audioPid = Int32.Parse(nodeTune.Attributes["AudioPid"].Value);
            int bandwidth = Int32.Parse(nodeTune.Attributes["Bandwidth"].Value);
            int channelNumber = Int32.Parse(nodeTune.Attributes["ChannelNumber"].Value);
            int channelType = Int32.Parse(nodeTune.Attributes["ChannelType"].Value);
            int countryId = Int32.Parse(nodeTune.Attributes["CountryId"].Value);
            int diseqc = Int32.Parse(nodeTune.Attributes["Diseqc"].Value);
            bool fta = (nodeTune.Attributes["FreeToAir"].Value == "True");
            int frequency = Int32.Parse(nodeTune.Attributes["Frequency"].Value);
            int majorChannel = Int32.Parse(nodeTune.Attributes["MajorChannel"].Value);
            int minorChannel = Int32.Parse(nodeTune.Attributes["MinorChannel"].Value);
            int modulation = Int32.Parse(nodeTune.Attributes["Modulation"].Value);
            name = nodeTune.Attributes["Name"].Value;
            int networkId = Int32.Parse(nodeTune.Attributes["NetworkId"].Value);
            int pcrPid = Int32.Parse(nodeTune.Attributes["PcrPid"].Value);
            int pmtPid = Int32.Parse(nodeTune.Attributes["PmtPid"].Value);
            int polarisation = Int32.Parse(nodeTune.Attributes["Polarisation"].Value);
            string provider = GetNodeAttribute(nodeTune, "Provider", "");
            int serviceId = Int32.Parse(nodeTune.Attributes["ServiceId"].Value);
            int switchingFrequency = Int32.Parse(nodeTune.Attributes["SwitchingFrequency"].Value);
            int symbolrate = Int32.Parse(nodeTune.Attributes["Symbolrate"].Value);
            int transportId = Int32.Parse(nodeTune.Attributes["TransportId"].Value);
            int tuningSource = Int32.Parse(GetNodeAttribute(nodeTune, "TuningSource", "0"));
            int videoPid = Int32.Parse(GetNodeAttribute(nodeTune, "VideoPid", "-1"));
            int videoSource = Int32.Parse(GetNodeAttribute(nodeTune, "VideoSource", "0"));
            int SatIndex = Int32.Parse(GetNodeAttribute(nodeTune, "SatIndex", "-1"));
            int InnerFecRate = Int32.Parse(GetNodeAttribute(nodeTune, "InnerFecRate", "-1"));
            int band = Int32.Parse(GetNodeAttribute(nodeTune, "Band", "0"));
            int pilot = Int32.Parse(GetNodeAttribute(nodeTune, "Pilot", "-1"));
            int rollOff = Int32.Parse(GetNodeAttribute(nodeTune, "RollOff", "-1"));
            string url = GetNodeAttribute(nodeTune, "Url", "");
            int bitrate = Int32.Parse(GetNodeAttribute(nodeTune, "Bitrate", "0"));

            switch (channelType)
            {
              case 0: //AnalogChannel
                AnalogChannel analogChannel = new AnalogChannel();
                analogChannel.ChannelNumber = channelNumber;
                analogChannel.Country = collection.Countries[countryId];
                analogChannel.Frequency = frequency;
                analogChannel.IsRadio = isRadio;
                analogChannel.IsTv = isTv;
                analogChannel.Name = name;
                analogChannel.TunerSource = (TunerInputType)tuningSource;
                analogChannel.VideoSource = (AnalogChannel.VideoInputType)videoSource;
                layer.AddTuningDetails(dbChannel, analogChannel);
                break;
              case 1: //ATSCChannel
                ATSCChannel atscChannel = new ATSCChannel();
                atscChannel.MajorChannel = majorChannel;
                atscChannel.MinorChannel = minorChannel;
                atscChannel.PhysicalChannel = channelNumber;
                atscChannel.FreeToAir = fta;
                atscChannel.Frequency = frequency;
                atscChannel.IsRadio = isRadio;
                atscChannel.IsTv = isTv;
                atscChannel.Name = name;
                atscChannel.NetworkId = networkId;
                atscChannel.PcrPid = pcrPid;
                atscChannel.PmtPid = pmtPid;
                atscChannel.Provider = provider;
                atscChannel.ServiceId = serviceId;
                atscChannel.SymbolRate = symbolrate;
                atscChannel.TransportId = transportId;
                atscChannel.AudioPid = audioPid;
                atscChannel.VideoPid = videoPid;
                atscChannel.ModulationType = (ModulationType)modulation;
                layer.AddTuningDetails(dbChannel, atscChannel);
                break;
              case 2: //DVBCChannel
                DVBCChannel dvbcChannel = new DVBCChannel();
                dvbcChannel.ModulationType = (ModulationType)modulation;
                dvbcChannel.FreeToAir = fta;
                dvbcChannel.Frequency = frequency;
                dvbcChannel.IsRadio = isRadio;
                dvbcChannel.IsTv = isTv;
                dvbcChannel.Name = name;
                dvbcChannel.NetworkId = networkId;
                dvbcChannel.PcrPid = pcrPid;
                dvbcChannel.PmtPid = pmtPid;
                dvbcChannel.Provider = provider;
                dvbcChannel.ServiceId = serviceId;
                dvbcChannel.SymbolRate = symbolrate;
                dvbcChannel.TransportId = transportId;
                layer.AddTuningDetails(dbChannel, dvbcChannel);
                break;
              case 3: //DVBSChannel
                DVBSChannel dvbsChannel = new DVBSChannel();
                dvbsChannel.DisEqc = (DisEqcType)diseqc;
                dvbsChannel.Polarisation = (Polarisation)polarisation;
                dvbsChannel.SwitchingFrequency = switchingFrequency;
                dvbsChannel.FreeToAir = fta;
                dvbsChannel.Frequency = frequency;
                dvbsChannel.IsRadio = isRadio;
                dvbsChannel.IsTv = isTv;
                dvbsChannel.Name = name;
                dvbsChannel.NetworkId = networkId;
                dvbsChannel.PcrPid = pcrPid;
                dvbsChannel.PmtPid = pmtPid;
                dvbsChannel.Provider = provider;
                dvbsChannel.ServiceId = serviceId;
                dvbsChannel.SymbolRate = symbolrate;
                dvbsChannel.TransportId = transportId;
                dvbsChannel.SatelliteIndex = SatIndex;
                dvbsChannel.ModulationType = (ModulationType)modulation;
                dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)InnerFecRate;
                dvbsChannel.BandType = (BandType)band;
                dvbsChannel.Pilot = (Pilot)pilot;
                dvbsChannel.Rolloff = (RollOff)rollOff;
                dvbsChannel.LogicalChannelNumber = channelNumber;
                dvbsChannel.VideoPid = videoPid;
                dvbsChannel.AudioPid = audioPid;
                layer.AddTuningDetails(dbChannel, dvbsChannel);
                break;
              case 4: //DVBTChannel
                DVBTChannel dvbtChannel = new DVBTChannel();
                dvbtChannel.BandWidth = bandwidth;
                dvbtChannel.FreeToAir = fta;
                dvbtChannel.Frequency = frequency;
                dvbtChannel.IsRadio = isRadio;
                dvbtChannel.IsTv = isTv;
                dvbtChannel.Name = name;
                dvbtChannel.NetworkId = networkId;
                dvbtChannel.PcrPid = pcrPid;
                dvbtChannel.PmtPid = pmtPid;
                dvbtChannel.Provider = provider;
                dvbtChannel.ServiceId = serviceId;
                dvbtChannel.TransportId = transportId;
                dvbtChannel.VideoPid = videoPid;
                dvbtChannel.AudioPid = audioPid;
                dvbtChannel.LogicalChannelNumber = channelNumber;
                layer.AddTuningDetails(dbChannel, dvbtChannel);
                break;
              case 5: //Webstream
                layer.AddWebStreamTuningDetails(dbChannel, url, bitrate);
                break;
            }
          }
        }
        // Import schedules
        foreach (XmlNode nodeSchedule in scheduleList)
        {
          scheduleCount++;
          string programName = nodeSchedule.Attributes["ProgramName"].Value;
          string channel = nodeSchedule.Attributes["ChannelName"].Value;
          int idChannel = layer.GetChannelByName(channel).IdChannel;

          DateTime startTime = DateTime.ParseExact(nodeSchedule.Attributes["StartTime"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          DateTime endTime = DateTime.ParseExact(nodeSchedule.Attributes["EndTime"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          int scheduleType = Int32.Parse(nodeSchedule.Attributes["ScheduleType"].Value);
          Schedule schedule = layer.AddSchedule(idChannel, programName, startTime, endTime, scheduleType);

          schedule.ScheduleType = scheduleType;
          schedule.KeepDate = DateTime.ParseExact(nodeSchedule.Attributes["KeepDate"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          schedule.PreRecordInterval = Int32.Parse(nodeSchedule.Attributes["PreRecordInterval"].Value);
          schedule.PostRecordInterval = Int32.Parse(nodeSchedule.Attributes["PostRecordInterval"].Value);
          schedule.Priority = Int32.Parse(nodeSchedule.Attributes["Priority"].Value);
          schedule.Quality = Int32.Parse(nodeSchedule.Attributes["Quality"].Value);
          schedule.Directory = nodeSchedule.Attributes["Directory"].Value;
          schedule.KeepMethod = Int32.Parse(nodeSchedule.Attributes["KeepMethod"].Value);
          schedule.MaxAirings = Int32.Parse(nodeSchedule.Attributes["MaxAirings"].Value);
          schedule.RecommendedCard = Int32.Parse(nodeSchedule.Attributes["RecommendedCard"].Value);
          schedule.ScheduleType = Int32.Parse(nodeSchedule.Attributes["ScheduleType"].Value);
          schedule.Series = (GetNodeAttribute(nodeSchedule, "Series", "False") == "True");
          schedule.Persist();
        }
        // Import channel groups
        foreach (XmlNode nodeChannelGroup in channelGroupList)
        {
          channelGroupCount++;
          string groupName = nodeChannelGroup.Attributes["GroupName"].Value;
          int groupSortOrder = Int32.Parse(nodeChannelGroup.Attributes["SortOrder"].Value);
          ChannelGroup group = new ChannelGroup(groupName, groupSortOrder);
          group.Persist();
          XmlNodeList mappingList = nodeChannelGroup.SelectNodes("mappings/map");
          foreach (XmlNode nodeMap in mappingList)
          {
            Channel channel = layer.GetChannelByName(nodeMap.Attributes["ChannelName"].Value);
            int sortOrder = Int32.Parse(GetNodeAttribute(nodeMap, "SortOrder", "9999"));
            if (channel != null)
            {
              GroupMap map = new GroupMap(group.IdGroup, channel.IdChannel, sortOrder);
              map.Persist();
            }
          }

        }
        dlg.Close();
        MessageBox.Show(String.Format("Imported {0} channels, {1} channel groups and {2} schedules", channelCount, channelGroupCount, scheduleCount));
      }
      catch (Exception ex)
      {
        dlg.Close();
        MessageBox.Show(this, "Error while importing:\n\n" + ex.ToString() + " " + ex.StackTrace);
      }
      OnSectionActivated();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {

    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedIndex == 0)
      {
        if (_redrawTab1)
        {
          OnSectionActivated();
        }
      }
      else
      {
        TabPage page = tabControl1.TabPages[tabControl1.SelectedIndex];
        foreach (Control control in page.Controls)
        {
          ChannelsInGroupControl groupCnt = control as ChannelsInGroupControl;
          if (groupCnt != null)
          {
            groupCnt.Group = (ChannelGroup)page.Tag;
            groupCnt.OnActivated();
          }
        }
      }
    }

    private void mpButtonPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormPreview previewWindow = new FormPreview();
      previewWindow.Channel = channel;
      previewWindow.ShowDialog(this);
    }

    private void mpListView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrder();
    }

    private void mpListView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      mpButtonEdit_Click(null, null);
    }

    private void deleteThisChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void editChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonEdit_Click(null, null);
    }

    private void mpButtonAdd_Click(object sender, EventArgs e)
    {
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = null;
      if (dlg.ShowDialog(this) == DialogResult.OK)
        OnSectionActivated();
    }

    private void mpButtonDeleteEncrypted_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Clearing all scrambled tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      List<ListViewItem> itemsToRemove = new List<ListViewItem>();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        if (channel.FreeToAir == false)
        {
          channel.Delete();
          itemsToRemove.Add(item);
        }
      }
      foreach (ListViewItem item in itemsToRemove)
        mpListView1.Items.Remove(item);
      dlg.Close();
      ReOrder();
      RemoteControl.Instance.OnNewSchedule();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void renameMarkedChannelsBySIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = ((TuningDetail)details[0]).ServiceId.ToString();
          channel.Persist();
          item.Tag = channel;
        }
      }
      OnSectionActivated();
    }

    private void addSIDInFrontOfNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = ((TuningDetail)details[0]).ServiceId.ToString() + " " + channel.DisplayName;
          channel.Persist();
          item.Tag = channel;
        }
      }
      OnSectionActivated();
    }
  }
}
