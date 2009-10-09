﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Interfaces;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
    public partial class InstallSections : UserControl, ISectionControl
    {
        public PackageClass Package { get; set; }
        private SectionItem SelectedSection;

        public InstallSections()
        {
            InitializeComponent();
            foreach (var panels in MpeInstaller.SectionPanels)
            {

                ToolStripMenuItem testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                testToolStripMenuItem.Text = panels.Value.DisplayName;
                testToolStripMenuItem.Tag = panels.Value;
                testToolStripMenuItem.Click += new System.EventHandler(TestToolStripMenuItemClick);
                mnu_add.DropDownItems.Add(testToolStripMenuItem);
                cmb_sectiontype.Items.Add(panels.Value.DisplayName);
            }
        }
        
        private void InstallSections_Load(object sender, EventArgs e)
        {

        }

        #region ISectionControl Members

        public void Set(PackageClass pak)
        {
            listBox_sections.Items.Clear();
            Package = pak;
            PopulateList();
            if(listBox_sections.Items.Count>0)
            {
                listBox_sections.SelectedItem = listBox_sections.Items[0];
            }
            cmb_grupvisibility.Items.Clear();
            cmb_grupvisibility.Items.Add(string.Empty);
            foreach (GroupItem groupItem in Package.Groups.Items)
            {
                cmb_grupvisibility.Items.Add(groupItem.Name);
            }
        }

        private void PopulateList()
        {
            foreach (SectionItem item in Package.Sections.Items)
            {
                AddSection(item);
            }
            if (SelectedSection != null)
            {
                listBox_sections.SelectedItem = SelectedSection;
            }
        }

        public PackageClass Get()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void AddSection(SectionItem item)
        {
            listBox_sections.Items.Add(item);
           
        }

        private void TestToolStripMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            SectionItem item = new SectionItem();
            ISectionPanel panel = menu.Tag as ISectionPanel;
            if (panel == null)
                return;
            SelectedSection = null;
            item.Name = panel.DisplayName;
            item.PanelName = panel.DisplayName;
            item.Params = panel.GetDefaultParams();
            Package.Sections.Add(item);
            AddSection(item);
            cmb_sectiontype.SelectedItem = item;
        }

        private void listBox_sections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox_sections.SelectedItems.Count>0)
            {
                SelectedSection = null;
                var param = listBox_sections.SelectedItem as SectionItem;
                txt_guid.Text = param.Guid;
                txt_name.Text = param.Name;
                cmb_sectiontype.Text = param.PanelName;
                cmb_grupvisibility.Text = param.ConditionGroup;
                list_groups.Items.Clear();
                foreach (var s in this.Package.Groups.Items)
                {
                    if (param.IncludedGroups.Contains(s.Name))
                        list_groups.Items.Add(s.Name, true);
                    else
                        list_groups.Items.Add(s.Name, false);
                }
                SelectedSection = param;
            }
        }

        private void txt_name_TextChanged(object sender, EventArgs e)
        {
            if (SelectedSection == null)
                return;
            SelectedSection.Name = txt_name.Text;
            SelectedSection.PanelName = cmb_sectiontype.Text;
            SelectedSection.ConditionGroup = cmb_grupvisibility.Text;
        }

        private void list_groups_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (SelectedSection == null)
                return;
            if (e.NewValue == CheckState.Checked)
            {
                SelectedSection.IncludedGroups.Add((string)list_groups.Items[e.Index]);
                
            }
            else
            {
                SelectedSection.IncludedGroups.Remove((string)list_groups.Items[e.Index]);
            }
        }

        private void btn_params_Click(object sender, EventArgs e)
        {
            if (SelectedSection == null)
                return;
            ParamEdit dlg = new ParamEdit();
            dlg.Set(SelectedSection.Params);
            dlg.ShowDialog();
        }

        private void btn_preview_Click(object sender, EventArgs e)
        {
            if (MpeInstaller.SectionPanels.ContainsKey(cmb_sectiontype.Text))
                MpeInstaller.SectionPanels[cmb_sectiontype.Text].Preview(Package, SelectedSection);
        }

        private void btn_up_Click(object sender, EventArgs e)
        {
            if (SelectedSection == null)
                return;
            int idx = Package.Sections.Items.IndexOf(SelectedSection);
            if(idx<1)
                return;
            Package.Sections.Items.Remove(SelectedSection);
            Package.Sections.Items.Insert(idx - 1, SelectedSection);
            listBox_sections.Items.Clear();
            PopulateList();
        }

        private void btn_down_Click(object sender, EventArgs e)
        {
            if (SelectedSection == null)
                return;
            int idx = Package.Sections.Items.IndexOf(SelectedSection);
            if (idx > Package.Sections.Items.Count - 2)
                return;
            Package.Sections.Items.Remove(SelectedSection);
            Package.Sections.Items.Insert(idx+1, SelectedSection);
            listBox_sections.Items.Clear();
            PopulateList();
        }

        private void mnu_remove_Click(object sender, EventArgs e)
        {
            if (SelectedSection == null)
                return;
            if (MessageBox.Show("Do you want to Delete section " + SelectedSection.Name, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            Package.Sections.Items.Remove(SelectedSection);
            if (Package.Sections.Items.Count > 0)
                SelectedSection = Package.Sections.Items[0];
            else
                SelectedSection = null;
            listBox_sections.Items.Clear();
            PopulateList();
        }
    }
}
