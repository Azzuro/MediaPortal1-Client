#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Windows.Forms;
using MediaPortal.EPG;
using MediaPortal.Services;
using MediaPortal.Webepg.TV.Database;
using MediaPortal.Util;
using MediaPortal.Utils.CommandLine;

namespace MediaPortal.EPG.WebEPGxmltv
{
  public class Program
  {
    /// <summary>
    /// The main entry point for the WebEPG application as external exe.
    /// </summary>
    [STAThread]
    static void Main(params string[] args)
    {
      // Parse Command Line options
      CommandLineOptions webepgArgs = new CommandLineOptions();
      ICommandLineOptions iwebepgArgs = (ICommandLineOptions)webepgArgs;

      try
      {
        CommandLine.Parse(args, ref iwebepgArgs);
      }
      catch (ArgumentException)
      {
        iwebepgArgs.DisplayOptions();
        return;
      }

      // setup logging service
      ILog _log = GlobalServiceProvider.Get<ILog>();
      _log.BackupLogFiles();
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG: Starting");

      // set process priority lower
      System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

      // try to catch all exceptions .. disabled in debug mode.
#if !DEBUG
      try
      {
#endif
        // Set location of directories and config file
        string webepgDirectory = Environment.CurrentDirectory;
        if (webepgArgs.IsOption(CommandLineOptions.Option.webepg))
          webepgDirectory = webepgArgs.GetOption(CommandLineOptions.Option.webepg);

        string xmltvDirectory;
        if (webepgArgs.IsOption(CommandLineOptions.Option.xmltv))
          xmltvDirectory = webepgArgs.GetOption(CommandLineOptions.Option.xmltv);
        else
          xmltvDirectory = webepgDirectory + "\\xmltv\\";

        string configFile;
        if (webepgArgs.IsOption(CommandLineOptions.Option.config))
          configFile = webepgArgs.GetOption(CommandLineOptions.Option.config);
        else
          configFile = webepgDirectory + "\\WebEPG\\WebEPG.xml";

        _log.Info(LogType.WebEPG, "WebEPG: Using directories");
        _log.Info(LogType.WebEPG, " WebEPG - {0}", webepgDirectory);
        _log.Info(LogType.WebEPG, " xmltv  - {0}", xmltvDirectory);

        // Create main class and import guide
        WebEPG epg = new WebEPG(configFile, xmltvDirectory, webepgDirectory);
        epg.Import();

        // If not in debug mode - Catch all Exceptions and log as Fatal errors
        // Program crashes cleanly without the MS message.
#if !DEBUG
      }
      // Catch and log all exceptions - fail cleanly
      catch (Exception ex)
      {
        _log.WriteFile(LogType.WebEPG, Level.Error, "WebEPG: Fatal Error");
        _log.WriteFile(LogType.WebEPG, Level.Error, "WebEPG: {0}", ex.Message);
      }
#endif

      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG: Finished");
    }
  }
}