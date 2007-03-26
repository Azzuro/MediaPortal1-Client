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
  public partial class TvGroups : SectionSettings
  {
    public class ListViewColumnSorter : IComparer
    {
      public int SortColumn;
      public SortOrder Order;
      public int Compare(object x, object y)
      {
        int compareResult;
        ListViewItem listviewX, listviewY;
        // Cast the objects to be compared to ListViewItem objects
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;
        if (SortColumn == 0)
        {
          compareResult = String.Compare(listviewX.Text, listviewY.Text);
        }
        else
        {
          // Compare the two items
          compareResult = String.Compare(listviewX.SubItems[SortColumn].Text,
            listviewY.SubItems[SortColumn].Text);
        }
        // Calculate correct return value based on object comparison
        if (Order == SortOrder.Ascending)
        {
          // Ascending sort is selected,
          // return normal result of compare operation
          return compareResult;
        }
        else if (Order == SortOrder.Descending)
        {
          // Descending sort is selected,
          // return negative result of compare operation
          return (-compareResult);
        }
        else
        {
          // Return '0' to indicate they are equal
          return 0;
        }
      }
    }
    struct ComboGroup
    {
      public ChannelGroup Group;
      public override string ToString()
      {
        return Group.GroupName;
      }

    }
    private ListViewColumnSorter lvwColumnSorter;

    public TvGroups()
      : this("TV Groups")
    {
    }

    public TvGroups(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new ListViewColumnSorter();
      this.mpListViewGroups.ListViewItemSorter = lvwColumnSorter;
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      Init();
      InitMapping();
    }

    void Init()
    {
      mpListViewGroups.Items.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      foreach (ChannelGroup group in groups)
      {
        ListViewItem item = mpListViewGroups.Items.Add(group.GroupName);
        item.Tag = group;
      }
      mpListViewGroups.Sort();
    }

    private void mpButtonDeleteGroup_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListViewGroups.SelectedItems)
      {
        ChannelGroup group = (ChannelGroup)item.Tag;
        group.Delete();
        mpListViewGroups.Items.Remove(item);
      }
    }

    private void mpButtonAddGroup_Click(object sender, EventArgs e)
    {
      GroupNameForm dlg = new GroupNameForm();
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0) return;
      ChannelGroup newGroup = new ChannelGroup(dlg.GroupName,9999);
      newGroup.Persist();
      Init();
    }
    private void mpTabControl1_TabIndexChanged(object sender, EventArgs e)
    {
      if (mpTabControl1.TabIndex == 1)
      {
        InitMapping();
      }
    }

    void InitMapping()
    {
      mpComboBoxGroup.Items.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      foreach (ChannelGroup group in groups)
      {
        ComboGroup g = new ComboGroup();
        g.Group = group;
        mpComboBoxGroup.Items.Add(g);
      }

      if (mpComboBoxGroup.Items.Count > 0)
        mpComboBoxGroup.SelectedIndex = 0;
      //mpComboBoxGroup_SelectedIndexChanged(null,null);
    }

    private void mpComboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (mpComboBoxGroup.SelectedItem == null) return;
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;
      //g.Group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);


      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();

      mpListViewChannels.Items.Clear();
      mpListViewMapped.Items.Clear();
      
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "isTv", 1);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());


      Dictionary<int, bool> channelsMapped = new Dictionary<int, bool>();

      sb = new SqlBuilder(StatementType.Select, typeof(GroupMap));
      sb.AddConstraint(Operator.Equals, "idGroup", g.Group.IdGroup);
      sb.AddOrderByField(true, "sortOrder");
      stmt = sb.GetStatement(true);
      IList maps = ObjectFactory.GetCollection(typeof(GroupMap), stmt.Execute());

      foreach (GroupMap map in maps)
      {
        Channel channel=map.ReferencedChannel();
        if (channel.IsTv == false) continue;
        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem mappedItem = mpListViewMapped.Items.Add(channel.Name, imageIndex);
        mappedItem.Tag = map;
        channelsMapped[channel.IdChannel] = true;
      }

      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel channel in channels)
      {
        if (channelsMapped.ContainsKey(channel.IdChannel)) continue;
        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem newItem = new ListViewItem(channel.Name, imageIndex);
        newItem.Tag = channel;
        items.Add(newItem);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      mpListViewChannels.Sort();
      mpListViewMapped.Sort();
    }

    private void mpButtonMap_Click(object sender, EventArgs e)
    {
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        GroupMap newMap = new GroupMap(g.Group.IdGroup, channel.IdChannel, channel.SortOrder);
        newMap.Persist();

        mpListViewChannels.Items.Remove(item);

        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem newItem = mpListViewMapped.Items.Add(channel.Name, imageIndex);
        newItem.Tag = newMap;
      }
      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        GroupMap map = (GroupMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        int imageIndex = 1;
        if (map.ReferencedChannel().FreeToAir == false)
          imageIndex = 2;
        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().Name, imageIndex);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpTabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {

      InitMapping();
    }

    private void mpListViewGroups_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        // Real sorting is now done via the up/down buttons
      /*if (e.Column == lvwColumnSorter.SortColumn)
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
      this.mpListViewGroups.Sort();*/
    }

    private void mpListViewMapped_DragDrop(object sender, DragEventArgs e)
    {
      
    }

    private void mpListViewMapped_DragEnter(object sender, DragEventArgs e)
    {

    }

    private void mpListViewMapped_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrderMap();
    }
    void ReOrderMap()
    {
      for (int i = 0; i < mpListViewMapped.Items.Count; ++i)
      {
        GroupMap map =(GroupMap) mpListViewMapped.Items[i].Tag;
        map.SortOrder = i;
        map.Persist();
      }
    }
    void ReOrderGroups()
    {
        for (int i = 0; i < mpListViewGroups.Items.Count; ++i)
        {
            ChannelGroup group = (ChannelGroup)mpListViewGroups.Items[i].Tag;

            group.SortOrder = i;
            group.Persist();
        }
    }

      private void buttonUtp_Click(object sender, EventArgs e)
      {
        mpListViewGroups.BeginUpdate();
        ListView.SelectedIndexCollection indexes = mpListViewGroups.SelectedIndices;
        if (indexes.Count == 0) return;
        for (int i = 0; i < indexes.Count; ++i)
        {
            int index = indexes[i];
            if (index > 0)
            {
                ListViewItem item = mpListViewGroups.Items[index];
                mpListViewGroups.Items.RemoveAt(index);
                mpListViewGroups.Items.Insert(index - 1, item);
            }
        }
        ReOrderGroups();
        mpListViewGroups.EndUpdate();
      }

      private void buttonDown_Click(object sender, EventArgs e)
      {
        mpListViewGroups.BeginUpdate();
        ListView.SelectedIndexCollection indexes = mpListViewGroups.SelectedIndices;
        if (indexes.Count == 0) return;
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
          int index = indexes[i];
          if (index > 0 && index + 1 < mpListViewGroups.Items.Count)
          {
            ListViewItem item = mpListViewGroups.Items[index];
            mpListViewGroups.Items.RemoveAt(index);
            mpListViewGroups.Items.Insert(index + 1, item);
          }
        }
        ReOrderGroups();
        mpListViewGroups.EndUpdate();
      }
    private void RenameGroup()
    {
      ListView.SelectedListViewItemCollection items = mpListViewGroups.SelectedItems;
      if (items.Count != 1) return;
      ChannelGroup group = (ChannelGroup)items[0].Tag;
      GroupNameForm dlg = new GroupNameForm(group.GroupName);
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0) return;
      group.GroupName = dlg.GroupName;
      group.Persist();
      Init();
    }

    private void mpButtonRenameGroup_Click(object sender, EventArgs e)
    {
      RenameGroup();
    }

    private void mpListViewGroups_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      RenameGroup();
    }

  }
}
