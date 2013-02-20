#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class MySQLChecker : IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private static readonly string _arch = Utils.Check64bit() ? "64" : "32";
    private static readonly string prg = "MySQL" + _arch;
    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private readonly string _dataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                       "\\MySQL\\MySQL Server 5.6";

    private void PrepareMyIni(string iniFile)
    {
      WritePrivateProfileString("client", "port", "3306", iniFile);
      WritePrivateProfileString("mysql", "default-character-set", "latin1", iniFile);
      WritePrivateProfileString("mysqld", "port", "3306", iniFile);
      WritePrivateProfileString("mysqld", "basedir",
                                "\"" + InstallationProperties.Instance["DBMSDir"].Replace('\\', '/') + "/\"", iniFile);
      WritePrivateProfileString("mysqld", "datadir", "\"" + _dataDir.Replace('\\', '/') + "/Data\"", iniFile);
      WritePrivateProfileString("mysqld", "default-storage-engine", "INNODB", iniFile);
      WritePrivateProfileString("mysqld", "sql-mode",
                                "\"STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION\"", iniFile);
      WritePrivateProfileString("mysqld", "max_connections", "100", iniFile);
      WritePrivateProfileString("mysqld", "query_cache_size", "32M", iniFile);
      WritePrivateProfileString("mysqld", "tmp_table_size", "18M", iniFile);
      WritePrivateProfileString("mysqld", "thread_cache_size", "4", iniFile);
      WritePrivateProfileString("mysqld", "thread_concurrency", "4", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_sort_file_size", "100M", iniFile);
      WritePrivateProfileString("mysqld", "myisam_sort_buffer_size", "64M", iniFile);
      WritePrivateProfileString("mysqld", "key_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "read_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "read_rnd_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "sort_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_additional_mem_pool_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_flush_log_at_trx_commit", "1", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_buffer_size", "1M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_buffer_pool_size", "96M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_file_size", "50M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_thread_concurrency", "8", iniFile);
    }

    public string GetDisplayName()
    {
      return "MySQL 5.6";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      string cmdLine = "/i \"" + _fileName + "\"";
      cmdLine += " INSTALLDIR=\"" + InstallationProperties.Instance["DBMSDir"] + "\"";
      cmdLine += " DATADIR=\"" + _dataDir + "\"";
      cmdLine += " /qn";
      cmdLine += " /L* \"" + Path.GetTempPath() + "\\mysqlinst.log\"";
      Process setup = Process.Start("msiexec.exe", cmdLine);
      try
      {
        if (setup != null)
        {
          setup.WaitForExit();
        }
      }
      catch
      {
        return false;
      }
      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\mysqlinst.log");
      bool installOk = false;
      while (!sr.EndOfStream)
      {
        string line = sr.ReadLine();
        if (line.Contains("Installation completed successfully"))
        {
          installOk = true;
          break;
        }
      }
      sr.Close();
      if (!installOk)
      {
        return false;
      }
      string inifile = InstallationProperties.Instance["DBMSDir"] + "\\my.ini";
      PrepareMyIni(inifile);
      const string ServiceName = "MySQL";
      string cmdExe = Environment.SystemDirectory + "\\sc.exe";
      string cmdParam = "create " + ServiceName + " start= auto DisplayName= " + ServiceName + " binPath= \"" +
                        InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld.exe --defaults-file=\\\"" + inifile +
                        "\\\" " + ServiceName + "\"";
#if DEBUG
      string ff = "c:\\mysql-srv.bat";
      StreamWriter a = new StreamWriter(ff);
      a.WriteLine("@echo off");
      a.WriteLine(cmdExe + " " + cmdParam);
      a.Close();
      Process svcInstaller = Process.Start(ff);
#else
      Process svcInstaller = Process.Start(cmdExe, cmdParam);
#endif
      if (svcInstaller != null)
      {
        svcInstaller.WaitForExit();
      }
      ServiceController ctrl = new ServiceController(ServiceName);
      try
      {
        ctrl.Start();
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - start service exception");
        return false;
      }

      ctrl.WaitForStatus(ServiceControllerStatus.Running);
      // Service is running, but on slow machines still take some time to answer network queries
      System.Threading.Thread.Sleep(5000);
      //
      // mysqladmin.exe is used to set MySQL password
      //
      cmdLine = "-u root password " + InstallationProperties.Instance["DBMSPassword"];

      try
      {
        Process mysqladmin = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
        if (mysqladmin != null)
        {
          mysqladmin.WaitForExit();
          if (mysqladmin.ExitCode != 0)
          {
            MessageBox.Show("MySQL - set password error: " + mysqladmin.ExitCode);
            return false;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - set password exception");
        return false;
      }
      System.Threading.Thread.Sleep(2000);
      //
      // mysql.exe is used to grant root access from all machines
      //
      cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] +
                " --execute=\"GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '" +
                InstallationProperties.Instance["DBMSPassword"] + "' WITH GRANT OPTION\" mysql";
      Process mysql = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe", cmdLine);
      try
      {
        if (mysql != null)
        {
          mysql.WaitForExit();
          if (mysql.ExitCode != 0)
          {
            MessageBox.Show("MySQL - set privileges error: " + mysql.ExitCode);
            return false;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - set privileges exception");
        return false;
      }
      return true;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{56DA0CB5-ABD2-4318-BEAB-62FDBC9B12CC}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      RegistryKey key = null;
      CheckResult result;
      result.needsDownload = true;
      FileInfo mySqlFile = new FileInfo(_fileName);

      if (mySqlFile.Exists && mySqlFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      key = Utils.registryKey32.OpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.6");
      if (key == null)
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.6");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        key.Close();
        result.state = CheckState.INSTALLED;
      }
      return result;
    }
  }
}