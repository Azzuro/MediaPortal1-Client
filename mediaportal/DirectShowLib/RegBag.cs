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

#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2006
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Runtime.InteropServices;
#pragma warning disable 618
namespace DirectShowLib
{
    #region Interfaces

    [Guid("8A674B48-1F63-11d3-B64C-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICreatePropBagOnRegKey
    {
        [PreserveSig]
        int Create(
            [In] IntPtr hkey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string subkey,
            [In] int ulOptions,
            [In] int samDesired,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppBag
            );
    }

    #endregion
}
