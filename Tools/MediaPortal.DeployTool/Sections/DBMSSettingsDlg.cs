#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  public partial class DBMSSettingsDlg : DeployDialog, IDeployDialog
  {
    public DBMSSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.DBMSSettings;
      if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
        textBoxDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Microsoft SQL Server";
      else
        textBoxDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\MySQL\\MySQL Server 5.0";
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.Instance.GetString("DBMSSettings_labelHeading");
      labelInstDir.Text = Localizer.Instance.GetString("DBMSSettings_labelInstDir");
      buttonBrowse.Text = Localizer.Instance.GetString("DBMSSettings_buttonBrowse");
      checkBoxFirewall.Text = Localizer.Instance.GetString("DBMSSettings_checkBoxFirewall");
      labelPassword.Text = Localizer.Instance.GetString("DBMSSettings_labelPassword");
    }
    public override DeployDialog GetNextDialog()
    {
      if (InstallationProperties.Instance["InstallType"] == "singleseat")
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
      else
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvServerSettings);
    }
    public override bool SettingsValid()
    {
      if (!Utils.CheckTargetDir(textBoxDir.Text))
      {
        Utils.ErrorDlg(Localizer.Instance.GetString("DBMSSettings_errInvalidInstallationPath"));
        return false;
      }
      if (textBoxPassword.Text == "")
      {
        Utils.ErrorDlg(Localizer.Instance.GetString("DBMSSettings_errPasswordMissing"));
        return false;
      }
      return true;
    }
    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("DBMSDir", textBoxDir.Text);
      InstallationProperties.Instance.Set("DBMSPassword", textBoxPassword.Text);
      if (checkBoxFirewall.Checked)
        InstallationProperties.Instance.Set("ConfigureDBMSFirewall", "1");
      else
        InstallationProperties.Instance.Set("ConfigureDBMSFirewall", "0");
    }
    #endregion

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.Description = Localizer.Instance.GetString("DBMSSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
        textBoxDir.Text = dlg.SelectedPath;
    }
  }
}
