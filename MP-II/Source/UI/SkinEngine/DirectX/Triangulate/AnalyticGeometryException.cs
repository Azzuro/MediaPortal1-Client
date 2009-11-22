#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;

namespace MediaPortal.UI.SkinEngine.DirectX.Triangulate
{
	/// <summary>
	/// Summary description for NoValidReturnException.
	/// </summary>
	public class NonValidReturnException: ApplicationException
	{
		public NonValidReturnException():base()
		{
		
		}
		public NonValidReturnException(string msg)
			:base(msg)
		{
			string errMsg="\nThere is no valid return value available!";
			throw new NonValidReturnException(errMsg);
		}
		public NonValidReturnException(string msg,
			Exception inner): base(msg, inner)
		{
		
		}
	}

	public class InvalidInputGeometryDataException: ApplicationException
	{
		public InvalidInputGeometryDataException():base()
		{
		
		}
		public InvalidInputGeometryDataException(string msg)
			:base(msg)
		{

		}
		public InvalidInputGeometryDataException(string msg,
			Exception inner): base(msg, inner)
		{
		
		}
	}
}
