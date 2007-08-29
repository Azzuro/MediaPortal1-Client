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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

using MediaPortal.UserInterface.Controls;
namespace SetupTv.Sections
{
  public partial class RadioChannelMapping : SectionSettings
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

    private MPListViewStringColumnSorter lvwColumnSorter1;
    private MPListViewStringColumnSorter lvwColumnSorter2;

    public RadioChannelMapping()
      : this("Radio Mapping")
    {
    }

    public RadioChannelMapping(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter1 = new MPListViewStringColumnSorter();
      lvwColumnSorter1.Order = SortOrder.None;
      this.mpListViewChannels.ListViewItemSorter = lvwColumnSorter1;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.None;
      this.mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
      //mpListViewChannels.ListViewItemSorter = new MPListViewSortOnColumn(0);
      //mpListViewMapped.ListViewItemSorter = new MPListViewSortOnColumn(0);
    }


    public override void OnSectionActivated()
    {
      mpComboBoxCard.Items.Clear();
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        mpComboBoxCard.Items.Add(new CardInfo(card));
      }
      mpComboBoxCard.SelectedIndex = 0;
      mpComboBoxCard_SelectedIndexChanged_1(null, null);
    }


    private void mpButtonMap_Click_1(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Mapping selected channels to TV-Card...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        ChannelMap map = layer.MapChannelToCard(card, channel);
        mpListViewChannels.Items.Remove(item);
        int imageIndex = 3;
        if (channel.FreeToAir == false)
          imageIndex = 0;

        ListViewItem newItem = mpListViewMapped.Items.Add(channel.DisplayName, imageIndex);
        newItem.Tag = map;
      }
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();

    }


    private void mpButtonUnmap_Click_1(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unmapping selected channels from TV-Card...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        ChannelMap map = (ChannelMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        int imageIndex = 3;
        if (map.ReferencedChannel().FreeToAir == false)
          imageIndex = 0;
        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().DisplayName, imageIndex);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
      mpListViewChannels.Sort();
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();

    }


    private void mpComboBoxCard_SelectedIndexChanged_1(object sender, EventArgs e)
    {

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      mpListViewMapped.Items.Clear();
      mpListViewChannels.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      IList maps = card.ReferringChannelMap();

      List<ListViewItem> items = new List<ListViewItem>();
      foreach (ChannelMap map in maps)
      {
        Channel channel = map.ReferencedChannel();
        if (channel.IsRadio == false) continue;
        int imageIndex = 3;
        if (channel.FreeToAir == false)
          imageIndex = 0;
        ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
        item.Tag = map;
        items.Add(item);
        bool remove = false;
        foreach (Channel ch in channels)
        {
          if (ch.IdChannel == channel.IdChannel)
          {
            remove = true;
            break;
          }
        }
        if (remove)
        {
          channels.Remove(channel);
        }
      }
      mpListViewMapped.Items.AddRange(items.ToArray());

      items = new List<ListViewItem>();
      foreach (Channel channel in channels)
      {
        if (channel.IsRadio == false) continue;
        int imageIndex = 3;
        if (channel.FreeToAir == false)
          imageIndex = 0;
        ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
        item.Tag = channel;
        items.Add(item);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpListViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpListViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter1.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter1.Order == SortOrder.Ascending)
        {
          lvwColumnSorter1.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter1.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter1.SortColumn = e.Column;
        lvwColumnSorter1.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewChannels.Sort();
    }

    private void mpListViewMapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter2.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter2.Order == SortOrder.Ascending)
        {
          lvwColumnSorter2.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter2.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter2.SortColumn = e.Column;
        lvwColumnSorter2.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewMapped.Sort();
    }

  }
}