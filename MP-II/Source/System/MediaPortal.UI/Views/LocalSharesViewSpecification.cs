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

using System;
using System.Collections.Generic;
using MediaPortal.Core;
using MediaPortal.Core.MediaManagement;
using MediaPortal.UI.Shares;

namespace MediaPortal.UI.Views
{
  /// <summary>
  /// View implementation which presents a list of of all local shares, one sub view for each share.
  /// </summary>
  public class LocalSharesViewSpecification : ViewSpecification
  {
    #region Ctor

    public LocalSharesViewSpecification(string viewDisplayName,
        IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds) :
        base(viewDisplayName, necessaryMIATypeIds, optionalMIATypeIds) { }

    #endregion

    #region Base overrides

    public override bool CanBeBuilt
    {
      get { return true; }
    }

    protected internal override IEnumerable<MediaItem> ReLoadItems()
    {
      yield break;
    }

    protected internal override IEnumerable<ViewSpecification> ReLoadSubViewSpecifications()
    {
      ILocalSharesManagement sharesManagement = ServiceScope.Get<ILocalSharesManagement>();
      foreach (Share share in sharesManagement.Shares.Values)
      {
        yield return new LocalDirectoryViewSpecification(share.Name, share.BaseResourcePath,
            _necessaryMIATypeIds, _optionalMIATypeIds);
      }
    }

    #endregion
  }
}
