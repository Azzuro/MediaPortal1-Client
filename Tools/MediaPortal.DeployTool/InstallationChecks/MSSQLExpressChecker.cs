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
using System.Runtime.InteropServices;

namespace MediaPortal.DeployTool
{
  class MSSQLExpressChecker: IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section,string key, string val, string filePath);

    private void PrepareTemplateINI(string iniFile)
    {
      WritePrivateProfileString("Options","USERNAME","MediaPortal",iniFile);
      WritePrivateProfileString("Options", "COMPANYNAME", "\"Team MediaPortal\"", iniFile);
      WritePrivateProfileString("Options", "INSTALLSQLDIR", "\"" + InstallationProperties.Instance["DBMSDir"] + "\"", iniFile);
      WritePrivateProfileString("Options", "INSTALLSQLDIR", "\"" + InstallationProperties.Instance["DBMSDir"] + "\"", iniFile);
      WritePrivateProfileString("Options", "ADDLOCAL", "ALL", iniFile);
      WritePrivateProfileString("Options", "INSTANCENAME", "SQLEXPRESS", iniFile);
      WritePrivateProfileString("Options", "SQLBROWSERAUTOSTART", "1", iniFile);
      WritePrivateProfileString("Options", "SQLAUTOSTART", "1", iniFile);
      WritePrivateProfileString("Options", "SECURITYMODE", "SQL", iniFile);
      WritePrivateProfileString("Options", "SAPWD", InstallationProperties.Instance["DBMSPassword"], iniFile);
      WritePrivateProfileString("Options", "DISABLENETWORKPROTOCOLS", "0", iniFile);
    }
    
    public string GetDisplayName()
    {
      return "MS SQL Express 2005";
    }

    public bool Download()
    {
        string prg = "MSSQLExpress";
        DialogResult result;
        result = Utils.DownloadFile(prg);
        FileInfo FileInfo = new FileInfo(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile(prg));

        for (int i = 0; i < 5; i++)
        {
            if (FileInfo.Length < 100.000)
                result = Utils.DownloadFile(prg);
            else
                break;
        }
        return (result == DialogResult.OK);
    }
    public bool Install()
    {

      string tmpPath=Path.GetTempPath()+"\\SQLEXPRESS";
      //Extract all files
      Process extract=Process.Start(Application.StartupPath + "\\deploy\\" + ReformatDownloadFile(Utils.GetDownloadFile("MSSQLExpress")),"/X:\""+tmpPath+"\" /Q");
      extract.WaitForExit();
      //Prepare the unattended ini file
      PrepareTemplateINI(tmpPath+"\\template.ini");
      //run the setup
      Process setup = Process.Start(tmpPath+"\\setup.exe","/wait /settings \""+tmpPath+"\\template.ini\" /qb");
      try
      {
          setup.WaitForExit();
          if (setup.ExitCode == 0)
          {
              Directory.Delete(tmpPath, true);
              return true;
          }
          else
              return false;
      }
      catch
      {
          return false;
      }
    }
    public bool UnInstall()
    {
      Process setup = Process.Start("msiexec", "/X {2AFFFDD7-ED85-4A90-8C52-5DA9EBDC9B8F}");
      setup.WaitForExit();
       return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + ReformatDownloadFile(Utils.GetDownloadFile("MSSQLExpress")));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
          if (result.needsDownload == false)
              result.state = CheckState.DOWNLOADED;
          else
              result.state = CheckState.NOT_DOWNLOADED;
          return result;
      }
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Microsoft SQL Server\\SQLEXPRESS\\MSSQLServer\\CurrentVersion");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        string version = (string)key.GetValue("CurrentVersion");
        key.Close();
        if (version.StartsWith("9.0"))
          result.state = CheckState.INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
    public string ReformatDownloadFile(string filename)
    {
        string LangCode = System.Globalization.CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
        string NewFileName = "";
        if (LangCode == "ENU")
            NewFileName = filename;
        else
            NewFileName = filename.Split('.')[0] + "_" + LangCode + ".exe";
        return NewFileName;
    }
  }
}
