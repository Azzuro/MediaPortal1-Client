﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
    public partial class TreeViewSelector : Form, ISectionPanel
    {
        private ShowModeEnum Mode = ShowModeEnum.Preview;
        private SectionItem Section = new SectionItem();
        private PackageClass Package;
        private SectionResponseEnum _resp = SectionResponseEnum.Cancel;

        private const string CONST_TEXT = "Description ";

        public TreeViewSelector()
        {
            InitializeComponent();
        }

        #region ISectionPanel Members


        public SectionParamCollection Params { get; set; }

        public bool Unique
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public SectionParamCollection Init()
        {
            throw new NotImplementedException();
        }

        public SectionParamCollection GetDefaultParams()
        {
            SectionParamCollection param = new SectionParamCollection();
            param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String,
                                       "Description of this operation"));
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Preview;
            Package = packageClass;
            Section = sectionItem;
            SetValues();
            ShowDialog();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Real;
            Package = packageClass;
            Section = sectionItem;
            SetValues();
            ShowDialog();
            return _resp;
        }

        #endregion

        private void button_back_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Back;
            this.Close();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Next;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Cancel;
            this.Close();
        }

        private TreeNode CreateNode(GroupItem item)
        {
            TreeNode node = new TreeNode(item.DisplayName);
            node.Name = item.DisplayName;
            node.Checked = Mode == ShowModeEnum.Preview ? item.DefaulChecked : item.Checked;
            node.Tag = item;
            return node;
        }

        private void SetValues()
        {
            label1.Text = Section.Params[CONST_TEXT].Value;
            treeView1.Nodes.Clear();
            foreach (string includedGroup in Section.IncludedGroups)
            {
                GroupItem groupItem = Package.Groups[includedGroup];
                if (string.IsNullOrEmpty(groupItem.ParentGroup))
                {
                    treeView1.Nodes.Add(CreateNode(groupItem));
                }
                else
                {
                    GroupItem parent = Package.Groups[groupItem.ParentGroup];
                    if (!treeView1.Nodes.ContainsKey(parent.DisplayName))
                        treeView1.Nodes.Add(CreateNode(parent));
                    treeView1.Nodes[parent.DisplayName].Nodes.Add(CreateNode(groupItem));
                }
            }
            treeView1.Sort();
            treeView1.ExpandAll();
        }

        private void TreeViewSelector_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (Mode == ShowModeEnum.Preview)
                return;
            GroupItem groupItem = e.Node.Tag as GroupItem;
            groupItem.Checked = e.Node.Checked;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            GroupItem groupItem = e.Node.Tag as GroupItem;
            lbl_description.Text = groupItem.Description;
        }

    }
}
