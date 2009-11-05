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
using MpeCore.Classes.SectionPanel;
using MpeInstaller.Controls;
using MpeInstaller.Dialogs;

namespace MpeInstaller
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            extensionListControl.UnInstallExtension += extensionListControl_UnInstallExtension;
        }

        void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
        {
            UnInstall dlg = new UnInstall();
            dlg.Execute(packageClass, false);
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MpeCore.MpeInstaller.Init();
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "Mpe package file(*.mpe2)|*.mpe2|All files|*.*"
                                        };
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                MpeCore.MpeInstaller.Init();
                PackageClass pak = new PackageClass();
                pak = pak.ZipProvider.Load(dialog.FileName);
                PackageClass installedPak = MpeCore.MpeInstaller.InstalledExtensions.Get(pak.GeneralInfo.Id);
                if (pak.CheckDependency(false))
                {
                    if (installedPak != null)
                    {
                        if (MessageBox.Show("This extension already have a installed version. \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo) != DialogResult.Yes)
                            return;
                        UnInstall dlg = new UnInstall();
                        dlg.Execute(installedPak, true);
                    }
                    pak.StartInstallWizard();
                    extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
                }
            }
        }
    }
}
