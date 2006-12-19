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
using System.Web;
using System.Text;

using System.Collections;

namespace MediaPortal.Utils.Web
{
  public class MatchTag
  {
    #region Variables
    string _fullTag;
    string _tagName;
    int _index;
    int _lenght;
    bool _isClose;
    #endregion

    #region Constructors/Destructors

    public MatchTag(string source, int index, int length)
    {
      _index = index;
      _lenght = length;
      _fullTag = source.Substring(index, length);


      int pos = _fullTag.IndexOf(' ');
      if (pos == -1)
        pos = _fullTag.Length - 1;

      int start = 1;
      _isClose = false;
      if (_fullTag[start] == '/')
      {
        start++;
        _isClose = true;
      }
      _tagName = _fullTag.Substring(start, pos - start).ToLower();

    }

    #endregion

    #region Properties

    public string FullTag
    {
      get { return _fullTag; }
    }

    public string TagName
    {
      get { return _tagName; }
    }

    public int Index
    {
      get { return _index; }
    }

    public int Length
    {
      get { return _lenght; }
    }

    public bool IsClose
    {
      get { return _isClose; }
    }
    #endregion

    #region Public Methods
    public bool SameType(MatchTag value)
    {
      if (_tagName == value._tagName &&
        _isClose == value._isClose)
        return true;

      return false;
    }

    public override string ToString()
    {
      return _fullTag;
    }
    #endregion
  }
}
