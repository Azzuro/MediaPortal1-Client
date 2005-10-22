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

using MediaPortal.Drawing;

namespace MediaPortal.Controls
{
	public class Button
	{
		#region Constructors

		public Button()
		{
		}

		#endregion Constructors

		#region Properties

		public Brush Background
		{
			get { return _background; }
			set { _background = value; }
		}

		#endregion Properties

		#region Fields

		Brush						_background;

		#endregion Fields

//		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

//		// Provide CLR accessors for the event
//		public event RoutedEventHandler Tap
//		{
//			add { AddHandler(TapEvent, value); } 
//			remove { RemoveHandler(TapEvent, value); }
//		}
	}
}
