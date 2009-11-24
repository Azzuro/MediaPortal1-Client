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
using MpeMaker.Wizards;

namespace MpeMaker.Dialogs
{
    public partial class NewFileSelector : Form
    {
        public PackageClass Package;
        public NewFileSelector(PackageClass packageClass)
        {
            Package = packageClass;
            InitializeComponent();
            listView1.Items[0].Selected = true;
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            switch (listView1.SelectedIndices[0])
            {
                case 0:
                    New();
                    break;
                case 1:
                    Package = NewSkin.Get(Package);
                    break;
                default:
                    break;


            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void New()
        {
            Package = new PackageClass();
            Package.Groups.Items.Add(new GroupItem("Default"));
            AddSection("Welcome Screen");
            Package.Sections.Items[0].WizardButtonsEnum = WizardButtonsEnum.NextCancel;
            AddSection("Install Section");
            var item = new ActionItem("InstallFiles")
            {
                Params =
                    new SectionParamCollection(
                    MpeInstaller.ActionProviders["InstallFiles"].GetDefaultParams())
            };
            Package.Sections.Items[1].Actions.Add(item);
            Package.Sections.Items[1].WizardButtonsEnum = WizardButtonsEnum.Next;
            AddSection("Setup Complete");
            Package.Sections.Items[2].WizardButtonsEnum = WizardButtonsEnum.Finish;   
        }


        private void AddSection(string name)
        {
            SectionItem item = new SectionItem();
            ISectionPanel panel = MpeInstaller.SectionPanels[name];
            if (panel == null)
                return;
            item.Name = panel.DisplayName;
            item.PanelName = panel.DisplayName;
            item.Params = panel.GetDefaultParams();
            Package.Sections.Add(item);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_ok.Enabled = listView1.SelectedItems.Count > 0;
        }
    }
}
