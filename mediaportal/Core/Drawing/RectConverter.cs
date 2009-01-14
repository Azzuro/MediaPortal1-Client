#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Drawing
{
  public class RectConverter : TypeConverter
  {
    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
    {
      if (t == typeof (string))
      {
        return true;
      }

      return base.CanConvertFrom(context, t);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string)
      {
        if (string.Compare("Empty", (string) value) == 0)
        {
          return Rect.Empty;
        }

        string[] param = ((string) value).Split(' ');

        return new Rect(Convert.ToDouble(param[0], CultureInfo.CurrentCulture),
                        Convert.ToDouble(param[1], CultureInfo.CurrentCulture),
                        Convert.ToDouble(param[2], CultureInfo.CurrentCulture),
                        Convert.ToDouble(param[3], CultureInfo.CurrentCulture));
      }

      return base.ConvertFrom(context, culture, value);
    }

    #endregion Methods
  }
}