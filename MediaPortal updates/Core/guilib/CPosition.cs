/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A datastructure for keeping track of the position of a control in a window.
	/// </summary>
	public class CPosition
	{
		private GUIControl _control=null;		// control to which this structure applies
		private int  _positionX=0;							  // x-coordinate of the control
		private int  _positionY=0;								// y-coordinate of the control

		/// <summary>
		/// The (empty) constructor of the CPosition class.
		/// </summary>
		public CPosition()
		{	
		}

		/// <summary>
		/// Constructs a CPosition class.
		/// </summary>
		/// <param name="control">The control of which the position is kept.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		public CPosition(ref GUIControl control, int x, int y)
		{	
			_control=control;
			_positionX=x;
			_positionY=y;
		}

		/// <summary>
		/// Gets the X coordintate.
		/// </summary>
		public int XPos
		{
			get {return _positionX;}
		}

		/// <summary>
		/// Gets the Y coordinate.
		/// </summary>
		public int YPos
		{
			get {return _positionY;}
    }

		/// <summary>
		/// Gets the control.
		/// </summary>
    public GUIControl control
    {
      get {return _control;}
    }
	}
}
