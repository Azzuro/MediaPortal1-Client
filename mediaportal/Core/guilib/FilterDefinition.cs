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
using System.Collections;
using System.Net;
using System.Xml.Serialization;

namespace MediaPortal.GUI.View
{
	/// <summary>
	/// Summary description for FilterDefinition.
	/// </summary>
	[Serializable]
	public class FilterDefinition
	{
		protected bool	 distinct=false;
		protected bool	 sortAscending=true;
		protected string restriction="";
		protected string whereClause="";
		protected string fromStatement="";
		protected string sqloperator="";
		protected string whereValue="*";
		protected string selectedValue="";
		protected string defaultView="List";
		protected int    limit=-1;

		public FilterDefinition()
		{
		}

    [XmlElement("distinct")]
    public bool Distinct
    {
      get { return distinct;}
      set { distinct=value;}
    }
    [XmlElement("SortAscending")]
    public bool SortAscending
    {
      get { return sortAscending;}
      set { sortAscending=value;}
    }
    [XmlElement("Restriction")]
		public string Restriction
		{
			get { return restriction;}
			set { restriction=value;}
		}
		[XmlElement("operator")]
		public string SqlOperator
		{
			get { return sqloperator;}
			set { sqloperator=value;}
		}
		

		[XmlElement("DefaultView")]
		public string DefaultView
		{
			get { return defaultView;}
			set { defaultView=value;}
		}

		[XmlElement("Where")]
		public string Where
		{
			get { return whereClause;}
			set { whereClause=value;}
		}

		[XmlElement("WhereValue")]
		public string WhereValue
		{
			get { return whereValue;}
			set { whereValue=value;}
		}

		[XmlElement("Limit")]
		public int Limit
		{
			get { return limit;}
			set { limit=value;}
		}

		public string SelectedValue
		{
			get { return selectedValue;}
			set { selectedValue=value;}
		}
	}
}
