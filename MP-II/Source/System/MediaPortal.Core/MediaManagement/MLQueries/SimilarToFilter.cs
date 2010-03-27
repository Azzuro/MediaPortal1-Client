#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace MediaPortal.Core.MediaManagement.MLQueries
{
  /// <summary>
  /// Specifies an expression which filters media items by a string attribute which is SIMILAR TO a given string.
  /// </summary>
  public class SimilarToFilter : AbstractExpressionFilter
  {
    public SimilarToFilter(MediaItemAspectMetadata.AttributeSpecification attributeType,
        string expression, char escapeChar) : base(attributeType, expression, escapeChar) { }

    #region Additional members for the XML serialization

    internal SimilarToFilter() { }

    #endregion
  }
}
