﻿#region Copyright (C) 2007-2008 Team MediaPortal

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
using MediaPortal.Core;
using MediaPortal.Media.MediaManager;
using MediaPortal.Media.MetaData;

using Components.Services.MediaManager.Views;

namespace Components.Services.MediaManager
{
  public class ViewContainer : IRootContainer
  {
    #region variables

    View _view;
    IRootContainer _root;
    IRootContainer _parent;
    ViewNavigator _navigator;
    IMediaItem _mediaItem;
    string _title;
    private Dictionary<string, object> _metaData;
    IMetaDataMappingCollection _mapping;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewContainer"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="root">The root.</param>
    /// <param name="parent">The parent.</param>
    public ViewContainer(View view, IRootContainer root, IRootContainer parent)
    {
      _root = root;
      _parent = parent;
      _view = view;
      _navigator = new ViewNavigator(view);
      _title = view.Title;
      _metaData = new Dictionary<string, object>();
      _metaData["title"] = Title;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewContainer"/> class.
    /// </summary>
    /// <param name="navigator">The navigator.</param>
    /// <param name="root">The root.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="item">The item.</param>
    public ViewContainer(ViewNavigator navigator, IRootContainer root, IRootContainer parent, IMediaItem item)
    {
      _root = root;
      _parent = parent;
      _view = navigator.View;
      _mediaItem = item;
      _navigator = navigator;
      _title = _mediaItem.Title;
      _metaData = new Dictionary<string, object>();
      _metaData["title"] = Title;
    }

    #region IRootContainer Members

    /// <summary>
    /// Gets or sets the mapping for the metadata.
    /// </summary>
    /// <value>The mapping for the metadata.</value>
    public IMetaDataMappingCollection Mapping
    {
      get
      {
        if (_mapping == null && _view != null)
        {
          _mapping = ServiceScope.Get<IMetadataMappingProvider>().Get(_view.MappingTable);
        }
        return _mapping;
      }
      set
      {
        _mapping = value;
      }
    }
    /// <summary>
    /// Gets a value indicating whether this item is located locally or remote
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this item is located locally; otherwise, <c>false</c>.
    /// </value>
    public bool IsLocal
    {
      get
      {
        return true;
      }
    }
    /// <summary>
    /// gets the media items
    /// </summary>
    /// <value></value>
    public IList<IAbstractMediaItem> Items
    {
      get
      {
        return _navigator.Get(_mediaItem, _root, this);
      }
    }

    /// <summary>
    /// Gets the root.
    /// </summary>
    /// <value>The root.</value>
    public IRootContainer Root
    {
      get
      {
        return _root;
      }
    }
    /// <summary>
    /// Gets the content URI for this item
    /// </summary>
    /// <value>The content URI.</value>
    public Uri ContentUri
    {
      get
      {
        //view container represents a virtual folder
        //for which we dont have an uri
        return null;
      }
    }
    #endregion

    #region IAbstractMediaItem Members


    /// <summary>
    /// Returns the title of the media item.
    /// </summary>
    /// <value></value>
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value;
      }
    }

    /// <summary>
    /// the media container in which this media item resides
    /// </summary>
    /// <value></value>
    public IRootContainer Parent
    {
      get
      {
        return _parent;
      }
      set
      {
        _parent = value;
      }
    }

    /// <summary>
    /// Gets or sets the full path.
    /// </summary>
    /// <value>The full path.</value>
    public string FullPath
    {
      get
      {
        if (Parent != null)
          return String.Format("{0}/{1}", Parent.FullPath, Title);
        return String.Format("{0}/{1}", Root.FullPath, Title);
      }
      set
      {
      }
    }

    /// <summary>
    /// Returns the metadata of the media item.
    /// </summary>
    /// <value></value>
    public IDictionary<string, object> MetaData
    {
      get { return _metaData; }
    }

    #endregion
  }
}
