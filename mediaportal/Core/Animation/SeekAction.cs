#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

namespace MediaPortal.Animation
{
	public class SeekAction : TimelineAction
	{
		#region Constructors

		public SeekAction()
		{
		}

		#endregion Constructors

		#region Properties

		public TimeSpan Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public TimeSeekOrigin Origin
		{
			get { return _origin; }
			set { _origin = value; }
		}

		#endregion Properties

		#region Fields

		TimeSpan					_offset;
		TimeSeekOrigin				_origin;

		#endregion Fields
	}
}
