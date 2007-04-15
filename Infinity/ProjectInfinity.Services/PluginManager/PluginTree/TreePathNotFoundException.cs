﻿#region Copyright (C) 2005-2007 Team MediaPortal

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
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion
using System;
using System.Runtime.Serialization;

namespace ProjectInfinity.Plugins
{
	/// <summary>
	/// Is thrown when the AddInTree could not find the requested path.
	/// </summary>
	[Serializable()]
	public class TreePathNotFoundException : PluginException
	{
		/// <summary>
		/// Constructs a new <see cref="TreePathNotFoundException"/>
		/// </summary>
		public TreePathNotFoundException(string path) : base("Treepath not found: " + path)
		{
		}
		
		// Required for Serialization
		public TreePathNotFoundException() : base()
		{
		}
		
		public TreePathNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		protected TreePathNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
