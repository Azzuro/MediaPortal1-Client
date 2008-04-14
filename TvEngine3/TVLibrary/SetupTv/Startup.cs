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
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Threading;
using TvControl;
using TvLibrary.Log;
using TvDatabase;

namespace SetupTv
{
  /// <summary>
  /// Summary description for Startup.
  /// </summary>
  public class Startup
  {
    enum StartupMode
    {
      Normal,
      Wizard,
      DbCleanup,
    }
    static StartupMode startupMode = StartupMode.Normal;

    string sectionsConfiguration = String.Empty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    public Startup(string[] arguments)
    {
      startupMode = StartupMode.Normal;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
      FileInfo mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);

      Form applicationForm = null;

      switch (startupMode)
      {
        case StartupMode.Normal:
          applicationForm = new SetupTvSettingsForm();
          break;

        case StartupMode.Wizard:
          applicationForm = new WizardForm(sectionsConfiguration);
          break;

        case StartupMode.DbCleanup:
          applicationForm = new SetupTvSettingsForm();
          break;
      }

      if (applicationForm != null)
      {
        System.Windows.Forms.Application.Run(applicationForm);
      }
    }

    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.Write("Exception in setuptv");
      Log.Write(e.ToString());
      Log.Write(e.Exception);
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
      Log.Info("---- start setuptv ----");
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

      foreach (string param in arguments)
      {
        if (param == "--delete-db" || param == "-d")
          startupMode = StartupMode.DbCleanup;
      }

      //test connection with database
      Log.Info("---- check connection with database ----");
      SetupDatabaseForm dlg = new SetupDatabaseForm();
      if (!dlg.TestConnection())
      {
        if (startupMode == StartupMode.DbCleanup)
        {
          Log.Info("---- could not delete database because the connection is not setup correctly ----");
          MessageBox.Show("Unable to find a previous tvlibrary schema. Please make sure there is no old database in place!", "DB cleanup failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else
        {
          Log.Info("---- ask user for connection details ----");
          dlg.ShowDialog();
        }
        return; // close the application without restart here.
      }

      if (startupMode == StartupMode.DbCleanup)
      {
        if (!dlg.ExecuteSQLScript("delete"))
          MessageBox.Show("Failed to drop the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      Log.Info("---- check if database needs to be updated/created ----");
      int currentSchemaVersion = dlg.GetCurrentShemaVersion();
      if (currentSchemaVersion == -1)
      {
        Log.Info("---- create database ----");
        if (!dlg.ExecuteSQLScript("create"))
        {
          MessageBox.Show("Failed to create the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        else
          Log.Info("- Database created.");
        currentSchemaVersion = dlg.GetCurrentShemaVersion();
      }

      Log.Info("---- upgrade database schema ----");
      if (!dlg.UpgradeDBSchema(currentSchemaVersion))
      {
        MessageBox.Show("Failed to upgrade the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      Log.Info("---- check if tvservice is running ----");
      if (!ServiceHelper.IsRunning)
      {
        Log.Info("---- tvservice is not running ----");
        DialogResult result = MessageBox.Show("The Tv service is not running.\rStart it now?", "Mediaportal TV service", MessageBoxButtons.YesNo);
        if (result != DialogResult.Yes) return;
        Log.Info("---- start tvservice----");
        ServiceHelper.Start();
      }

      int cards = 0;
      try
      {
        cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        Log.Info("---- restart tvservice----");
        ServiceHelper.Restart();
        try
        {
          RemoteControl.Clear();
          RemoteControl.HostName = "localhost";
          cards = RemoteControl.Instance.Cards;
        }
        catch (Exception ex)
        {
          Log.Info("---- Unable to restart tv service----");
          Log.Write(ex);
          MessageBox.Show("Failed to startup tvservice" + ex.ToString());
          return;
        }
      }

      try
      {
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.DoEvents();

        new Startup(arguments).Start();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        GC.Collect();
      }
    }

    private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
        return null;
      if (args.Name.Contains(".XmlSerializers"))
        return null;
      MessageBox.Show("Failed to locate assembly '" + args.Name + "'." + Environment.NewLine + "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      System.Windows.Forms.Application.Exit();
      return null;
    }
  }
}
