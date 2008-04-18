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
using System.Resources;

namespace MediaPortal.DeployTool
{
  public partial class BaseInstallationTypeDlg : DeployDialog, IDeployDialog
  {
    bool rbOneClickChecked;

    public BaseInstallationTypeDlg()
    {
      InitializeComponent();
      type = DialogType.BASE_INSTALLATION_TYPE;
      labelSectionHeader.Text = "";
      imgOneClick.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbOneClickChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelOneClickCaption.Text = Localizer.Instance.GetString("BaseInstallation_labelOneClickCaption");
      labelOneClickDesc.Text = Localizer.Instance.GetString("BaseInstallation_labelOneClickDesc");
      rbOneClick.Text = Localizer.Instance.GetString("BaseInstallation_rbOneClick");
      labelAdvancedCaption.Text = Localizer.Instance.GetString("BaseInstallation_labelAdvancedCaption");
      labelAdvancedDesc.Text = Localizer.Instance.GetString("BaseInstallation_labelAdvancedDesc");
      rbAdvanced.Text = Localizer.Instance.GetString("BaseInstallation_rbAdvanced");

    }
    public override DeployDialog GetNextDialog()
    {
      if (rbOneClickChecked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
      else
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.CUSTOM_INSTALLATION_TYPE);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      if (rbOneClickChecked)
      {
        InstallationProperties.Instance.Set("InstallTypeHeader", Localizer.Instance.GetString("BaseInstallation_rbOneClick"));
        InstallationProperties.Instance.Set("InstallType", "singleseat");
        InstallationProperties.Instance.Set("DBMSType", "mssql2005");
        InstallationProperties.Instance.Set("DBMSDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Microsoft SQL Server");
        InstallationProperties.Instance.Set("DBMSPassword", "MediaPortal");
        InstallationProperties.Instance.Set("ConfigureTVServerFirewall", "1");
        InstallationProperties.Instance.Set("ConfigureDBMSFirewall", "1");
      }
    }
    #endregion

    private void imgOneClick_Click(object sender, EventArgs e)
    {
      imgOneClick.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      imgAdvanced.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      rbOneClickChecked = true;
    }
    private void rbOneClick_Click(object sender, EventArgs e)
    {
      imgOneClick_Click(sender, e);
    }

    private void imgAdvanced_Click(object sender, EventArgs e)
    {
      imgOneClick.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgAdvanced.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbOneClickChecked = false;
    }
    private void rbAdvanced_Click(object sender, EventArgs e)
    {
      imgAdvanced_Click(sender, e);
    }
  }
}
