#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Automation.Provider
{
	[ComVisibleAttribute(true)] 
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)] 
	[GuidAttribute("56d00bd0-c4f4-433c-a836-1a52a57e0892")] 
	public interface IToggleProvider
	{
		#region Methods
		
		void Toggle();
		
		#endregion Methods

		#region Properties
		
		ToggleState ToggleState
		{
			get;
		}

		#endregion Properties
	}
}
