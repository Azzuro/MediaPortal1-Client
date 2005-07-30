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

using System;

namespace MediaPortal.UserInterface.Controls
{
	/// <summary>
	/// Summary description for ListView.
	/// </summary>
	public class MPListView : System.Windows.Forms.ListView
	{
		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			const int WM_PAINT = 0xf ;

			switch(m.Msg)
			{
				case WM_PAINT:
					if(this.View == System.Windows.Forms.View.Details && this.Columns.Count > 0)
					{
						this.Columns[this.Columns.Count - 1].Width = -2 ;
					}
					break ;
			}

			base.WndProc (ref m);
		}
	}
}
