#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Class which hold information about a key press
  /// </summary>
  public class Key
  {
    private int m_iChar = 0; // character 
    private int m_iCode = 0; // character code 

    /// <summary>
    /// empty constructor
    /// </summary>
    public Key()
    {
    }

    /// <summary>
    /// copy constructor
    /// </summary>
    /// <param name="key"></param>
    public Key(Key key)
    {
      m_iChar = key.KeyChar;
      m_iCode = key.KeyCode;
    }

    public Key(int iChar, int iCode)
    {
      m_iChar = iChar;
      m_iCode = iCode;
    }

    public int KeyChar
    {
      get { return m_iChar; }
    }

    public int KeyCode
    {
      get { return m_iCode; }
    }
  }
}