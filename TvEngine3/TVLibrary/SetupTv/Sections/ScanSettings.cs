#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using DirectShowLib;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class ScanSettings : SectionSettings
  {
    private static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid LegacyAmFilterCategory = new Guid(0x083863F1, 0x70DE, 0x11d0, 0xBD, 0x40, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

    public ScanSettings()
      : this("General") {}

    public ScanSettings(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      TvBusinessLayer layer = new TvBusinessLayer();

      numericUpDownTune.Value = Convert.ToDecimal(layer.GetSetting("timeoutTune", "2").Value);
      numericUpDownPAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPAT", "5").Value);
      numericUpDownCAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutCAT", "5").Value);
      numericUpDownPMT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPMT", "10").Value);
      numericUpDownSDT.Value = Convert.ToDecimal(layer.GetSetting("timeoutSDT", "20").Value);
      numericUpDownAnalog.Value = Convert.ToDecimal(layer.GetSetting("timeoutAnalog", "20").Value);

      delayDetectUpDown.Value = Convert.ToDecimal(layer.GetSetting("delayCardDetect", "0").Value);

      checkBoxEnableLinkageScanner.Checked = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

      mpComboBoxPrio.Items.Clear();
      mpComboBoxPrio.Items.Add("Realtime");
      mpComboBoxPrio.Items.Add("High");
      mpComboBoxPrio.Items.Add("Above Normal");
      mpComboBoxPrio.Items.Add("Normal");
      mpComboBoxPrio.Items.Add("Below Normal");
      mpComboBoxPrio.Items.Add("Idle");

      try
      {
        mpComboBoxPrio.SelectedIndex = Convert.ToInt32(layer.GetSetting("processPriority", "3").Value);
        //default is normal=3       
      }
      catch (Exception)
      {
        mpComboBoxPrio.SelectedIndex = 3; //fall back to default which is normal=3
      }
      BuildLists(layer);
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("timeoutTune", "2");
      s.Value = numericUpDownTune.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutPAT", "5");
      s.Value = numericUpDownPAT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutCAT", "5");
      s.Value = numericUpDownCAT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutPMT", "10");
      s.Value = numericUpDownPMT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutSDT", "20");
      s.Value = numericUpDownSDT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutAnalog", "20");
      s.Value = numericUpDownAnalog.Value.ToString();
      s.Persist();

      s = layer.GetSetting("linkageScannerEnabled", "no");
      s.Value = checkBoxEnableLinkageScanner.Checked ? "yes" : "no";
      s.Persist();

      s = layer.GetSetting("processPriority", "3");
      s.Value = mpComboBoxPrio.SelectedIndex.ToString();
      s.Persist();

      s = layer.GetSetting("delayCardDetect", "0");
      s.Value = delayDetectUpDown.Value.ToString();
      s.Persist();
    }

    private void mpComboBoxPrio_SelectedIndexChanged(object sender, EventArgs e)
    {
      System.Diagnostics.Process process;
      try
      {
        process = System.Diagnostics.Process.GetProcessesByName("TVService")[0];
      }
      catch (Exception ex)
      {
        Log.Write("could not set priority on tvservice - the process might be terminated : " + ex.Message);
        return;
      }

      try
      {
        switch (mpComboBoxPrio.SelectedIndex)
        {
          case 0:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            break;
          case 1:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            break;
          case 2:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
            break;
          case 3:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
          case 4:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            break;
          case 5:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;
            break;
          default:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
        }
      }
      catch (Exception exp)
      {
        Log.Write(string.Format("Could not set priority on tvservice. Error on setting process.PriorityClass: {0}",
                                exp.Message));
        return;
      }
    }

    private void BuildLists(TvBusinessLayer layer) {
      mpListViewVideo.Items.Clear();

      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(AudioCompressorCategory);
      DsDevice[] devices3 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      bool found;
      IList<SoftwareEncoder> encoders = layer.GetSofwareEncodersVideo();
      foreach (SoftwareEncoder encoder in encoders) {
        found=false;
        ListViewItem item = mpListViewVideo.Items.Add("", 0);
        for (int i = 0; i < devices1.Length; i++) {
          if (devices1[i].Name == encoder.Name) {
            found = true;
            item.Text = "Yes";
            break;
          }
        }
        if (!found) {
          for (int i = 0; i < devices3.Length; i++) {
            if (devices3[i].Name == encoder.Name) {
              found = true;
              item.Text = "Yes";
              break;
            }
          }
        }
        if (!found) {
          item.Text = "No";
        }
        item.SubItems.Add(encoder.Priority.ToString());
        item.SubItems.Add(encoder.Name);
        item.Tag = encoder;
      }

      mpListViewAudio.Items.Clear();
      encoders = layer.GetSofwareEncodersAudio();
      foreach (SoftwareEncoder encoder in encoders) {
        found = false;
        ListViewItem item = mpListViewAudio.Items.Add("", 0);
        for (int i = 0; i < devices2.Length; i++) {
          if (devices2[i].Name == encoder.Name) {
            found = true;
            item.Text = "Yes";
            break;
          }
        }
        if (!found) {
          for (int i = 0; i < devices3.Length; i++) {
            if (devices3[i].Name == encoder.Name) {
              found = true;
              item.Text = "Yes";
              break;
            }
          }
        }
        if (!found) {
          item.Text = "No";
        }
        item.SubItems.Add(encoder.Priority.ToString());
        item.SubItems.Add(encoder.Name);
        item.Tag = encoder;
      }
    }

    private void button1_Click(object sender, EventArgs e) {
      mpListViewVideo.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewVideo.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i) {
        int index = indexes[i];
        if (index > 0) {
          ListViewItem item = mpListViewVideo.Items[index];
          mpListViewVideo.Items.RemoveAt(index);
          mpListViewVideo.Items.Insert(index - 1, item);
        }
      }
      ReOrder(mpListViewVideo);
      mpListViewVideo.EndUpdate();
    }

    private void button2_Click(object sender, EventArgs e) {
      mpListViewVideo.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewVideo.SelectedIndices;
      if (indexes.Count == 0) return;
      if (mpListViewVideo.Items.Count < 2) return;
      for (int i = indexes.Count - 1; i >= 0; i--) {
        int index = indexes[i];
        ListViewItem item = mpListViewVideo.Items[index];
        mpListViewVideo.Items.RemoveAt(index);
        if (index + 1 < mpListViewVideo.Items.Count)
          mpListViewVideo.Items.Insert(index + 1, item);
        else
          mpListViewVideo.Items.Add(item);
      }
      ReOrder(mpListViewVideo);
      mpListViewVideo.EndUpdate();
    }

    private void ReOrder(MediaPortal.UserInterface.Controls.MPListView mpListView) {
      for (int i = 0; i < mpListView.Items.Count; ++i) {
        mpListView.Items[i].SubItems[1].Text = (i + 1).ToString();
        SoftwareEncoder encoder = (SoftwareEncoder)mpListView.Items[i].Tag;
        encoder.Priority = i+1;
        encoder.Persist();
      }
    }

    private void button4_Click(object sender, EventArgs e) {
      mpListViewAudio.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewAudio.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i) {
        int index = indexes[i];
        if (index > 0) {
          ListViewItem item = mpListViewAudio.Items[index];
          mpListViewAudio.Items.RemoveAt(index);
          mpListViewAudio.Items.Insert(index - 1, item);
        }
      }
      ReOrder(mpListViewAudio);
      mpListViewAudio.EndUpdate();
    }

    private void button3_Click(object sender, EventArgs e) {
      mpListViewAudio.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewAudio.SelectedIndices;
      if (indexes.Count == 0) return;
      if (mpListViewAudio.Items.Count < 2) return;
      for (int i = indexes.Count - 1; i >= 0; i--) {
        int index = indexes[i];
        ListViewItem item = mpListViewAudio.Items[index];
        mpListViewAudio.Items.RemoveAt(index);
        if (index + 1 < mpListViewAudio.Items.Count)
          mpListViewAudio.Items.Insert(index + 1, item);
        else
          mpListViewAudio.Items.Add(item);
      }
      ReOrder(mpListViewAudio);
      mpListViewAudio.EndUpdate();
    }
  }
}