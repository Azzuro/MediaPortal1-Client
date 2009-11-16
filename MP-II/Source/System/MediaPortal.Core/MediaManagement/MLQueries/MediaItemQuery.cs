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
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using MediaPortal.Utilities;

namespace MediaPortal.Core.MediaManagement.MLQueries
{
  public enum SortDirection
  {
    Ascending,
    Descending
  }

  public class SortInformation
  {
    protected MediaItemAspectMetadata.AttributeSpecification _attributeType;
    protected SortDirection _sortDirection;

    public SortInformation(MediaItemAspectMetadata.AttributeSpecification attributeType, SortDirection sortDirection)
    {
      _attributeType = attributeType;
      _sortDirection = sortDirection;
    }

    public MediaItemAspectMetadata.AttributeSpecification AttributeType
    {
      get { return _attributeType; }
      set { _attributeType = value; }
    }

    public SortDirection Direction
    {
      get { return _sortDirection; }
      set { _sortDirection = value; }
    }
  }

  /// <summary>
  /// Encapsulates a query for media items. Holds selected media item aspect types and a filter criterion.
  /// </summary>
  public class MediaItemQuery
  {
    protected IFilter _filter;
    protected HashSet<Guid> _necessaryRequestedMIATypeIDs;
    protected HashSet<Guid> _optionalRequestedMIATypeIDs = null;
    protected List<SortInformation> _sortInformation = null;

    public MediaItemQuery(IEnumerable<Guid> necessaryRequestedMIATypeIDs, IFilter filter)
    {
      _necessaryRequestedMIATypeIDs = new HashSet<Guid>(necessaryRequestedMIATypeIDs);
      _filter = filter;
    }

    public MediaItemQuery(IEnumerable<Guid> necessaryRequestedMIATypeIDs, IEnumerable<Guid> optionalRequestedMIATypeIDs,
        IFilter filter)
    {
      _necessaryRequestedMIATypeIDs = new HashSet<Guid>(necessaryRequestedMIATypeIDs);
      _optionalRequestedMIATypeIDs = new HashSet<Guid>(optionalRequestedMIATypeIDs);
      _filter = filter;
    }

    [XmlIgnore]
    public ICollection<Guid> NecessaryRequestedMIATypeIDs
    {
      get { return _necessaryRequestedMIATypeIDs; }
      set
      { _necessaryRequestedMIATypeIDs = new HashSet<Guid>(value); }
    }

    [XmlIgnore]
    public ICollection<Guid> OptionalRequestedMIATypeIDs
    {
      get { return _optionalRequestedMIATypeIDs; }
      set { _optionalRequestedMIATypeIDs = new HashSet<Guid>(value); }
    }

    [XmlIgnore]
    public IFilter Filter
    {
      get { return _filter; }
      set { _filter = value; }
    }

    [XmlIgnore]
    public ICollection<SortInformation> SortInformation
    {
      get { return _sortInformation; }
      set { _sortInformation = new List<SortInformation>(value); }
    }

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();
      result.Append("MediaItemQuery: NecessaryRequestedMIATypes: [");
      result.Append(StringUtils.Join(", ", _necessaryRequestedMIATypeIDs));
      result.Append("], OptionalRequestedMIATypes: [");
      result.Append(StringUtils.Join(", ", _optionalRequestedMIATypeIDs));
      result.Append("], Filter: [");
      result.Append(_filter);
      result.Append("], SortInformation: [");
      result.Append(StringUtils.Join(", ", _sortInformation));
      result.Append("]");
      return result.ToString();
    }

    #region Additional members for the XML serialization

    internal MediaItemQuery() { }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlArray("NecessaryMIATypes")]
    [XmlArrayItem("TypeId")]
    public HashSet<Guid> XML_NecessaryRequestedMIATypeIDs
    {
      get { return _necessaryRequestedMIATypeIDs; }
      set { _necessaryRequestedMIATypeIDs = value; }
    }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlArray("OptionalMIATypes")]
    [XmlArrayItem("TypeId")]
    public HashSet<Guid> XML_OptionalRequestedMIATypeIDs
    {
      get { return _optionalRequestedMIATypeIDs; }
      set { _optionalRequestedMIATypeIDs = value; }
    }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlElement("BetweenFilter", typeof(BetweenFilter))]
    [XmlElement("BooleanCombinationFilter", typeof(BooleanCombinationFilter))]
    [XmlElement("InFilter", typeof(InFilter))]
    [XmlElement("LikeFilter", typeof(LikeFilter))]
    [XmlElement("SimilarToFilter", typeof(SimilarToFilter))]
    [XmlElement("NotFilter", typeof(NotFilter))]
    [XmlElement("RelationalFilter", typeof(RelationalFilter))]
    public object XML_Filter
    {
      get { return _filter; }
      set { _filter = value as IFilter; }
    }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlArray("SortInformation")]
    [XmlArrayItem("Sort")]
    public List<SortInformation> XML_SortInformation
    {
      get { return _sortInformation; }
      set { _sortInformation = value; }
    }

    #endregion
  }
}
