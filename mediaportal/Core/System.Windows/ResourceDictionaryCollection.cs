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
using System.Collections;
using System.Windows.Serialization;

namespace System.Windows
{
	public sealed class ResourceDictionaryCollection : CollectionBase
	{
		#region Constructors

		public ResourceDictionaryCollection()
		{
		}

		#endregion Constructors

		#region Methods

		public void Add(ResourceDictionary dictionary)
		{
			if(dictionary == null)
				throw new ArgumentNullException("dictionary");

			List.Add(dictionary);
		}

		public bool Contains(ResourceDictionary dictionary)
		{
			if(dictionary == null)
				throw new ArgumentNullException("dictionary");

			return List.Contains(dictionary);
		}

		public void CopyTo(ResourceDictionary[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public ResourceDictionaryCollection GetCurrentValue()
		{
			throw new NotImplementedException();
		}
			
		public int IndexOf(ResourceDictionary dictionary)
		{
			if(dictionary == null)
				throw new ArgumentNullException("dictionary");

			return List.IndexOf(dictionary);
		}

		public void Insert(int index, ResourceDictionary dictionary)
		{
			if(dictionary == null)
				throw new ArgumentNullException("dictionary");

			List.Insert(index, dictionary);
		}

		public bool Remove(ResourceDictionary dictionary)
		{
			if(dictionary == null)
				throw new ArgumentNullException("dictionary");
			
			if(List.Contains(dictionary) == false)
				return false;

			List.Remove(dictionary);

			return true;
		}

		#endregion Methods

		#region Properties

		public ResourceDictionary this[int index]
		{ 
			get { return (ResourceDictionary)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
