﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;
using MpeMaker.Dialogs;
using MpeMaker.Sections;

namespace MpeMaker
{
    public partial class MainForm : Form
    {
        public PackageClass Package { get; set; }
        Dictionary<string, Control> panels = new Dictionary<string, Control>();
        public string ProjectFileName = "";
        
        public MainForm()
        {
            MpeInstaller.Init();
            InitializeComponent();
            splitContainer1.Panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Package = new PackageClass();

            treeView1.ExpandAll();
            panels.Add("Node0", new WelcomSection());
            panels.Add("Node2", new GeneralSection());
            panels.Add("Node3", new FilesGroupsSection());
            panels.Add("Node4", new InstallSections());
            panels.Add("Node5", new RequirementsSection());
            panels.Add("Node6", new BuildSection());

            NewProject();

        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (panels.ContainsKey(e.Node.Name))
            {
                splitContainer1.Panel2.Controls.Clear();
                panels[e.Node.Name].Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top |
                                              AnchorStyles.Left);
                panels[e.Node.Name].Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(panels[e.Node.Name]);
                ((ISectionControl)panels[e.Node.Name]).Set(Package);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnu_save_Click(object sender, EventArgs e)
        {
            if (File.Exists(ProjectFileName))
                Save(ProjectFileName);
            else
            {
                mnu_saveAs_Click(null, null);
            }
        }

        private void mnu_saveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "All files|*.*";
            saveFileDialog1.Title = "Save extension installer proiect file";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save(saveFileDialog1.FileName);
            }
        }

        private void Save( string file)
        {
            Package.GenerateRelativePath(Path.GetDirectoryName(file));
            Package.Save(file);
            ProjectFileName = file;
            
        }

        private void SetTitle()
        {
            this.Text = "MpeMaker - " + ProjectFileName;
        }

        private void mnu_open_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files|*.*";
            openFileDialog1.Title = "Open extension installer proiect file";
            openFileDialog1.FileName = ProjectFileName;
            openFileDialog1.Multiselect = false;
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                Package.Load(openFileDialog1.FileName);
                Package.GenerateAbsolutePath(Path.GetDirectoryName(openFileDialog1.FileName));
                ProjectFileName = openFileDialog1.FileName;
                treeView1.SelectedNode = treeView1.Nodes[0];
                SetTitle();
            }
        }

        private void mnu_new_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("All not saved changes will be lost, \n Do you want to continue ?", "New proiect", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                NewProject();
            }
        }

        private void NewProject()
        {
            Package = new PackageClass();
            ProjectFileName = "";
            Package.Groups.Items.Add(new GroupItem("Default"));
            treeView1.SelectedNode = treeView1.Nodes[0];
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
    }
}
