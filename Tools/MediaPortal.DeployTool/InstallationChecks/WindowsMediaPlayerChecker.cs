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
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace MediaPortal.DeployTool
{
  class WindowsMediaPlayerChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "Windows Media Player 11";
    }

    public bool Download()
    {
      string prg = "WindowsMediaPlayer";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string exe = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("WindowsMediaPlayer", "FILE");
      Process setup = Process.Start(exe, "/q");
      try
      {
        setup.WaitForExit();
        return true;
      }
      catch
      {
        return false;
      }
    }
    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      string prg = "WindowsMediaPlayer";
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.LocalizeDownloadFile(Utils.GetDownloadString(prg, "FILE"), Utils.GetDownloadString(prg, "TYPE"), prg));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        if (result.needsDownload == false)
          result.state = CheckState.DOWNLOADED;
        else
          result.state = CheckState.NOT_DOWNLOADED;
        return result;
      }
      Version aParamVersion;
      if (Utils.CheckFileVersion(Environment.SystemDirectory + "\\wmp.dll", "11.0.0000.0000", out aParamVersion))
      {
        result.state = CheckState.INSTALLED;
      }
      else
      {
        result.state = CheckState.NOT_INSTALLED;
      }
      return result;
    }
  }
}

