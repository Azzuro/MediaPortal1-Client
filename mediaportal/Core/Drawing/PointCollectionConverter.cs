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
using System.Drawing;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Drawing
{
	public class PointCollectionConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			if(t == typeof(string))
				return true;

			return base.CanConvertFrom(context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value == null)
				throw base.GetConvertFromException(value);

			if(value is string)
			{
				PointCollection points = new PointCollection();

				// TODO: this should be improved upon
				foreach(string token in ((string)value).Split(' '))
				{
					if(token == string.Empty)
						continue;

					string[] coords = token.Split(',');

					points.Add(new Point(Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1])));
				}

				return points;
			}

			return base.ConvertFrom(context, culture, value);
		}

		#endregion Methods
	}
}
