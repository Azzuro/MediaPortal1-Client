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
using System.Windows.Input;

namespace System.Windows.Controls
{
	public class Menu : MenuBase
	{
		#region Constructors

		static Menu()
		{
			IsMainMenuProperty = DependencyProperty.Register("IsMainMenu", typeof(bool), typeof(Menu));
		}

		public Menu()
		{
		}

		#endregion Constructors

		#region Methods

		protected override void HandleMouseButton(MouseButtonEventArgs e)
		{
		}
		
		protected override void OnInitialized(EventArgs e)
		{
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, Object item)
		{
		}

		#endregion Methods

		#region Properties (Dependency)

		public static readonly DependencyProperty IsMainMenuProperty;

		#endregion Properties (Dependency)

		#region Fields

		public bool IsMainMenu
		{
			get { return (bool)GetValue(IsMainMenuProperty); }
			set { SetValue(IsMainMenuProperty, value); }
		}

		#endregion Fields
	}
}
