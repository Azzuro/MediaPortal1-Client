﻿#region Copyright (C) 2005-2007 Team MediaPortal

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
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using ProjectInfinity;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Plugins
{
  /// <summary>
  ///  interface for the PluginTree.
  /// </summary>
  public interface IPluginTree
  {
    IList<Plugin> Plugins
    {
      get;
    }

    Dictionary<string, IBuilder> Builders
    {
      get;
    }
  }
}
