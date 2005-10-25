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
using System.ComponentModel;

namespace System.Windows
{
	public class DependencyPropertyChangedEventArgs : PropertyChangedEventArgs
	{
		#region Constructors

		public DependencyPropertyChangedEventArgs(DependencyProperty property, object oldValue, object newValue) : base(string.Empty)
		{
			_property = property;
			_oldValue = oldValue;
			_newValue = newValue;
		}

		#endregion Constructors

		#region Properties

		public object NewValue
		{
			get { return _newValue; }
		}

		public object OldValue
		{
			get { return _oldValue; }
		}

		public DependencyProperty Property
		{
			get { return _property; }
		}

		public override string PropertyName
		{
			get { return _property.Name; }
		}

		#endregion Properties

		#region Fields

		object						_newValue;
		object						_oldValue;
		DependencyProperty			_property;

		#endregion Fields
	}
}
