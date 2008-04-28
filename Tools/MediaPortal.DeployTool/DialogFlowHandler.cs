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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum DialogType
  {
    Welcome,
    DownloadOnly,
    WatchTV,
    WatchHDTv,
    TvEngineType,
    BASE_INSTALLATION_TYPE,
    BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE,
    CUSTOM_INSTALLATION_TYPE,
    DBMSType,
    DBMSSettings,
    MPSettings,
    TvServerSettings,
    Installation,
    Finished
  }

  public sealed class DialogFlowHandler
  {
    #region Singleton implementation
    static readonly DialogFlowHandler _instance = new DialogFlowHandler();

    static DialogFlowHandler()
    {
    }
    DialogFlowHandler()
    {
      _dlgs = new List<DeployDialog>();
    }

    public static DialogFlowHandler Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion

    #region Variables
    private List<DeployDialog> _dlgs;
    private int _currentDlgIndex = -1;
    #endregion

    #region Private members
    private DeployDialog FindDialog(DialogType dlgType)
    {
      for (int i = 0; i < _dlgs.Count; i++)
      {
        if (_dlgs[i].type == dlgType)
        {
          _currentDlgIndex = i;
          return _dlgs[i];
        }
      }
      return null;
    }
    #endregion

    #region Public members
    public DeployDialog GetPreviousDlg(ref bool isFirstDlg)
    {
      if (_currentDlgIndex == 0)
        return null;
      _currentDlgIndex--;
      isFirstDlg = (_currentDlgIndex == 0);
      return _dlgs[_currentDlgIndex];
    }

    public DeployDialog GetDialogInstance(DialogType dlgType)
    {
      DeployDialog dlg = FindDialog(dlgType);
      if (dlg == null)
      {
        switch (dlgType)
        {
          case DialogType.Welcome:
            dlg = (DeployDialog)new WelcomeDlg();
            break;
          case DialogType.DownloadOnly:
            dlg = (DeployDialog)new DownloadOnlyDlg();
            break;
          case DialogType.WatchTV:
            dlg = (DeployDialog)new WatchTVDlg();
            break;
          case DialogType.WatchHDTv:
            dlg = (DeployDialog)new WatchHDTvDlg();
            break;
          case DialogType.TvEngineType:
            dlg = (DeployDialog)new TvEngineTypeDlg();
            break;
          case DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE:
            dlg = (DeployDialog)new BaseInstallationTypeWithoutTvEngineDlg();
            break;
          case DialogType.BASE_INSTALLATION_TYPE:
            dlg = (DeployDialog)new BaseInstallationTypeDlg();
            break;
          case DialogType.CUSTOM_INSTALLATION_TYPE:
            dlg = (DeployDialog)new CustomInstallationTypeDlg();
            break;
          case DialogType.DBMSType:
            dlg = (DeployDialog)new DBMSTypeDlg();
            break;
          case DialogType.DBMSSettings:
            dlg = (DeployDialog)new DBMSSettingsDlg();
            break;
          case DialogType.MPSettings:
            dlg = (DeployDialog)new MPSettingsDlg();
            break;
          case DialogType.TvServerSettings:
            dlg = (DeployDialog)new TvServerSettingsDlg();
            break;
          case DialogType.Installation:
            dlg = (DeployDialog)new InstallDlg();
            break;
          case DialogType.Finished:
            dlg = (DeployDialog)new FinishedDlg();
            break;
        }
        if (dlg != null)
        {
          _dlgs.Add(dlg);
          _currentDlgIndex = _dlgs.Count - 1;
        }
      }
      else
        dlg.UpdateUI();
      return dlg;
    }

    public void ResetHistory()
    {
      DeployDialog cachedDlg = _dlgs[0];
      _dlgs.Clear();
      _dlgs.Add(cachedDlg);
    }
    #endregion
  }
}
