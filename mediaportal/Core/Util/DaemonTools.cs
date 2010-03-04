#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Diagnostics;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.Util
{
  /// <summary>
  /// 
  /// </summary>
  public class DaemonTools
  {
    private static string _Path;
    private static string _Drive;
    private static bool _Enabled;
    private static int _DriveNo;
    private static string _MountedIsoFile = string.Empty;
    private static List<string> _supportedExtensions;

    static DaemonTools()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        _Enabled = xmlreader.GetValueAsBool("daemon", "enabled", false);
        _Path = xmlreader.GetValueAsString("daemon", "path", "");
        _Drive = xmlreader.GetValueAsString("daemon", "drive", "E:");
        _DriveNo = xmlreader.GetValueAsInt("daemon", "driveNo", 0);
        /*
         * DAEMON Tools supports the following image files:
         * cue/bin
         * iso
         * ccd (CloneCD)
         * bwt (Blindwrite)
         * mds (Media Descriptor File)
         * cdi (Discjuggler)
         * nrg (Nero)
         * pdi (Instant CD/DVD)
         * b5t (BlindWrite 5)
         */
        string[] extensions =
          xmlreader.GetValueAsString("daemon", "extensions", Utils.ImageExtensionsDefault).Split(',');
        _supportedExtensions = new List<string>();
        // Can't use an AddRange, as we need to trim the blanks  
        foreach (string ext in extensions)
          _supportedExtensions.Add(ext.Trim());
      }
    }

    public static bool IsEnabled
    {
      get { return _Enabled; }
    }

    public static bool IsMounted(string IsoFile)
    {
      if (IsoFile == null) return false;
      if (IsoFile == string.Empty) return false;
      IsoFile = Utils.RemoveTrailingSlash(IsoFile);
      if (_MountedIsoFile.Equals(IsoFile))
      {
        if (System.IO.Directory.Exists(_Drive + @"\"))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      return false;
    }

    public static bool Mount(string IsoFile, out string VirtualDrive)
    {
      VirtualDrive = string.Empty;
      if (IsoFile == null) return false;
      if (IsoFile == string.Empty) return false;
      if (!_Enabled) return false;
      if (!System.IO.File.Exists(_Path)) return false;
      DateTime startTime = DateTime.Now;
      System.IO.DriveInfo drive = new System.IO.DriveInfo(_Drive);
      UnMount();

      IsoFile = Utils.RemoveTrailingSlash(IsoFile);
      string strParams = String.Format("-mount {0},\"{1}\"", _DriveNo, IsoFile);
      Process p = Utils.StartProcess(_Path, strParams, true, true);
      int timeout = 0;
      while ((!p.HasExited || !drive.IsReady || !System.IO.Directory.Exists(_Drive + @"\")) && (timeout < 10000))
      {
        System.Threading.Thread.Sleep(100);
        timeout += 100;
      }
      if (timeout >= 10000)
      {
        Log.Error("Mounting failed (timed out). Recheck your settings.");
        return false;
      }
      VirtualDrive = _Drive;
      _MountedIsoFile = IsoFile;
      Log.Debug("Mount time: {0}s", String.Format("{0:N}", (DateTime.Now - startTime).TotalSeconds));
      return true;
    }

    public static void UnMount()
    {
      if (!_Enabled) return;
      if (!System.IO.File.Exists(_Path)) return;
      if (!System.IO.Directory.Exists(_Drive + @"\")) return;

      string strParams = String.Format("-unmount {0}", _DriveNo);
      Process p = Utils.StartProcess(_Path, strParams, true, true);
      int timeout = 0;
      while (!p.HasExited && (timeout < 10000))
      {
        System.Threading.Thread.Sleep(100);
        timeout += 100;
      }
      _MountedIsoFile = string.Empty;
    }

    public static string GetVirtualDrive()
    {
      if (_MountedIsoFile != string.Empty) return _Drive;
      return string.Empty;
    }

    /// <summary>
    /// This method check is the given extension is a image file
    /// </summary>
    /// <param name="extension">file extension</param>
    /// <returns>
    /// true: if file is an image file (.img, .nrg, .bin, .iso, ...)
    /// false: if the file is not an image file
    /// </returns>
    public static bool IsImageFile(string extension)
    {
      if (extension == null) return false;
      if (extension == string.Empty) return false;
      extension = extension.ToLower();
      foreach (string ext in _supportedExtensions)
        if (ext.Equals(extension))
          return true;
      return false;
    }
  }
}