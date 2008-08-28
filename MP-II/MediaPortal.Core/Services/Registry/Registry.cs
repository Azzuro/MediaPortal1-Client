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
using System.Collections.Generic;
using MediaPortal.Core.Registry;

namespace MediaPortal.Core.Services.Registry
{
  /// <summary>
  /// Non-persistent application registry implementation.
  /// </summary>
  public class Registry: IRegistry
  {
    #region Protected fields

    protected RegistryNode _rootNode;

    #endregion

    #region Ctor

    public Registry()
    {
      _rootNode = new RegistryNode(null, string.Empty);
    }

    #endregion

    #region IRegistry implementation

    public IRegistryNode RootNode
    {
      get { return _rootNode; }
    }

    public IRegistryNode GetRegistryNode(string path, bool createOnNotExist)
    {
      CheckAbsolute(path);
      return _rootNode.GetSubNodeByPath(path.Substring(1), createOnNotExist);
    }

    public IRegistryNode GetRegistryNode(string path)
    {
      CheckAbsolute(path);
      return _rootNode.GetSubNodeByPath(path.Substring(1));
    }

    public bool RegistryNodeExists(string path)
    {
      CheckAbsolute(path);
      return _rootNode.SubNodeExists(path.Substring(1));
    }

    #endregion

    public static bool IsAbsolutePath(string path)
    {
      return path.StartsWith("/");
    }

    protected static void CheckAbsolute(string path)
    {
      if (!IsAbsolutePath(path))
        throw new ArgumentException("Registry path expression has to be an absolute path (should start with a '/' character)");
    }

    #region IStatus implementation

    public IList<string> GetStatus()
    {
      List<string> result = new List<string>();
      result.Add("=== Registry");
      foreach (string line in _rootNode.GetStatus())
        result.Add("  " + line);
      return result;
    }

    #endregion
  }
}
