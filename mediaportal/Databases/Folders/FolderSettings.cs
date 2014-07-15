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
using System.IO;
using System.Threading;
using System.Collections;
using Databases.Folders;

namespace MediaPortal.Database
{
  public class FolderSettings
  {
    private static IFolderSettings _database = DatabaseFactory.GetFolderDatabase();

    public static void ReOpen()
    {
      Dispose();
      _database = DatabaseFactory.GetFolderDatabase();
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }

      _database = null;
    }

    private static bool WaitForPath(string pathName)
    {
      // while waking up from hibernation it can take a while before a network drive is accessible.   
      int count = 0;

      if (pathName.Length == 0 || pathName == "root")
      {
        return true;
      }

      //we cant be sure if pathName is a file or a folder, so we look for both.      
      while ((!Directory.Exists(pathName) && !File.Exists(pathName)) && count < 10)
      {
        Thread.Sleep(250);
        count++;
      }

      return (count < 10);
    }

    public static void GetPath(string strPath, ref ArrayList strPathList, string strKey)
    {
      _database.GetPath(strPath, ref strPathList, strKey);
    }

    public static void DeleteFolderSetting(string path, string Key)
    {
      //bool res = WaitForPath(path);
      _database.DeleteFolderSetting(path, Key);
    }

    public static void AddFolderSetting(string path, string Key, Type type, object Value)
    {
      //bool res = WaitForPath(path);
      _database.AddFolderSetting(path, Key, type, Value);
    }

    public static void GetFolderSetting(string path, string Key, Type type, out object Value)
    {
      //bool res = WaitForPath(path);
      _database.GetFolderSetting(path, Key, type, out Value);
    }

    public static string DatabaseName
    {
      get
      {
        if (_database != null)
        {
          return _database.DatabaseName;
        }
        return "";
      }
    }
  }
}