#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using MediaPortal.Utilities.Strings;

namespace MediaPortal.Utilities.Files
{
  /// <summary>
  /// Contains file and directory related utility methods.
  /// </summary>
  public class FileUtils
  {
    /// <summary>
    /// Combines two paths. This method differs from
    /// <see cref="System.IO.Path.Combine(string, string)"/> in that way that it
    /// removes relative path navigations to the parent path in the
    /// <paramref name="path2"/> path.
    /// </summary>
    /// <example>
    /// CombinePaths(@"C:\Program Files\", "MediaPortal") returns @"C:\Program Files\MediaPortal",
    /// CombinePaths(@"C:\Program Files\MediaPortal", "..\abc") returns @"C:\Program Files\abc",
    /// </example>
    /// <param name="path1">First path to combine with the second one. If this path is
    /// <c>null</c>, the second path will be returned.</param>
    /// <param name="path2">Second path to be combined with the first one. This path may contain
    /// relative path navigation elements like <c>@".."</c>. If this path is <c>null</c>,
    /// null will be returned.</param>
    /// <returns>
    /// Combined paths as string. If the <paramref name="path1"/> is null, <paramref name="path2"/>
    /// will be returned. If <paramref name="path2"/> is null, null will be returned. Otherwise,
    /// the combination of the paths will be returned, as described.
    /// </returns>
    /// <exception cref="ArgumentException">If the two paths cannot be combined (For example if
    /// the paths contain illegal characters or if the relative navigation of the second path
    /// cannot be applied to the first one.</exception>
    public static string CombinePaths(string path1, string path2)
    {
      if (path1 == null) return path2;
      if (path2 == null) return null;
      if (Path.IsPathRooted(path2))
        return path2;
      path1 = RemoveTrailingPathDelimiter(path1);
      while (path2.StartsWith(@"..\") || path2.StartsWith("../"))
      {
        path2 = path2.Substring(3);
        // Find the last / or \ character, then trim the base path
        int pos = Math.Max(path1.LastIndexOf(@"\"), path1.LastIndexOf(@"/"));
        if (pos > 0)
        {
          if (path1 == @"\\")
            throw new ArgumentException("Cannot combine paths '{0}' and '{1}'");
          path1 = path1.Remove(pos);
        }
      }
      if (path1.Length == 2 && path1[1] == ':')
        path1 += @"\";
      return Path.Combine(path1, path2);
    }

    /// <summary>
    /// Removes a trailing slash or backslash from the given path string.
    /// </summary>
    /// <param name="path">A path string with or without trailing path delimiter (@"\" or "/").</param>
    /// <returns><paramref name="path"/> with removed path delimiter(s), if there were any.</returns>
    public static string RemoveTrailingPathDelimiter(string path)
    {
      if (path == null) return string.Empty;
      if (path.Length == 0) return string.Empty;
      while (HasPathDelimiter(path))
      {
        path = path.Remove(path.Length - 1);
      }
      return path;
    }

    /// <summary>
    /// Returns the information if the given path has a path delimiter at its end.
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns>true, if the specified <paramref name="path"/> has a path delimiter at its end,
    /// otherwise false</returns>
    public static bool HasPathDelimiter(string path)
    {
      return path.Length > 0 &&
        (path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Returns all files in the specified directory. The method traverses recursively
    /// through the directory tree.
    /// </summary>
    /// <param name="dir">Root directory to start the search from.</param>
    /// <returns>List of all files under the specified root directory.</returns>
    public static IList<FileInfo> GetAllFilesRecursively(DirectoryInfo dir)
    {
      IList<FileInfo> result = new List<FileInfo>();
      foreach (FileInfo file in dir.GetFiles())
        result.Add(file);
      foreach (DirectoryInfo subDir in dir.GetDirectories())
        foreach (FileInfo file in GetAllFilesRecursively(subDir))
          result.Add(file);
      return result;
    }
  }
}
