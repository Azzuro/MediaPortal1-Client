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
using MediaPortal.Core.PluginManager;
using MediaPortal.Database;
using MediaPortal.Media.MediaManager;
using MediaPortal.Media.MediaManager.Views;

using Components.Services.MediaManager.Views;

namespace Components.Services.MediaManager
{
  public class MediaManager : IPlugin, IMediaManager
  {
    #region Variables
    private bool _providerPluginsLoaded = false;
    private readonly List<IProvider> _providers;
    private readonly List<IRootContainer> _views;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaManager"/> class.
    /// </summary>
    public MediaManager()
    {
      _providers = new List<IProvider>();
      _views = new List<IRootContainer>();
    }
    #endregion

    #region IPlugin Implementation
    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(string id)
    {
      ViewLoader loader = new ViewLoader();
      string[] files = System.IO.Directory.GetFiles("Views");
      for (int i = 0; i < files.Length; ++i)
      {
        MediaContainer cont = loader.Load(files[i]);
        Register(cont);
      }
    }
    #endregion

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() { }

    #region IMediaManager Implementation
    #region Properties
    /// <summary>
    /// Gets the providers.
    /// </summary>
    /// <value>The providers.</value>
    public List<IProvider> Providers
    {
      get
      {
        LoadProviderPlugins();

        return _providers;
      }
    }

    /// <summary>
    /// Gets the views.
    /// </summary>
    /// <value>The views.</value>
    public List<IRootContainer> Views
    {
      get
      {
        return _views;
      }
    }
    #endregion

    /// <summary>
    /// Registers the specified provider.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public void Register(IProvider provider)
    {
      _providers.Add(provider);
    }

    /// <summary>
    /// Registers the specified view.
    /// </summary>
    /// <param name="view">The view.</param>
    public void Register(IRootContainer view)
    {
      if (_views.Contains(view))
        throw new ArgumentException("View already exists", "view");
      _views.Add(view);
    }


    /// <summary>
    /// Uns the register.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public void UnRegister(IProvider provider)
    {
      _providers.Remove(provider);
    }


    /// <summary>
    /// Returns  a list of all root containers.
    /// </summary>
    /// <value>The root containers.</value>
    public List<IRootContainer> RootContainers
    {
      get
      {
        List<IRootContainer> list = new List<IRootContainer>();

        LoadProviderPlugins();

        foreach (IProvider provider in _providers)
        {
          List<IRootContainer> subContainers = provider.RootContainers;
          if (subContainers != null)
          {
            list.AddRange(subContainers);
          }
        }
        return list;
      }
    }

    /// <summary>
    /// Gets the view by a path
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public List<IAbstractMediaItem> GetView(string path)
    {
      List<IAbstractMediaItem> result = new List<IAbstractMediaItem>();
      IRootContainer root = GetRoot();
      if (path == "/")
      {
        return root.Items;
      }
      string[] hierarchy = path.Split('/');
      int level = 0;
      IRootContainer view = root;
      while (level < hierarchy.Length)
      {
        string currentLevel = hierarchy[level];
        if (currentLevel.Length > 0)
        {
          IAbstractMediaItem item = FindItem(view, currentLevel);
          IRootContainer container = item as IRootContainer;
          if (container == null)
          {
            if (item != null)
              result.Add(item);
            return result;
          }
          view = container;
        }
        level++;
      }
      if (view != null)
      {
        return view.Items;
      }
      return null;
    }

    /// <summary>
    /// Gets the view.
    /// </summary>
    /// <param name="parentItem">The parent item.</param>
    /// <returns></returns>
    public List<IAbstractMediaItem> GetView(IRootContainer parentItem)
    {
      if (parentItem == null)
      {
        return GetRoot().Items;
      }
      return parentItem.Items;
    }
    #endregion

    #region Private methods

    /// <summary>
    /// Loads the provider plugins and registers them
    /// </summary>
    private void LoadProviderPlugins()
    {
      if (!_providerPluginsLoaded)
      {
        foreach (IProvider provider in ServiceScope.Get<IPluginManager>().GetAllPluginItems<IProvider>("/Media/Providers"))
        {
          this.Register(provider);
        }
        _providerPluginsLoaded = true;
      }
    }

    /// <summary>
    /// Finds the item.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    private static IAbstractMediaItem FindItem(IRootContainer parent, string name)
    {
      // When a Database has been deleted and we're re-entering the view, get get a null value from the mapping
      try
      {
        foreach (IAbstractMediaItem item in parent.Items)
        {
          if (item.Title == name)
            return item;
        }
      }
      catch (Exception)
      { }
      return null;
    }

    /// <summary>
    /// Gets the root.
    /// </summary>
    /// <returns></returns>
    private IRootContainer GetRoot()
    {
      IRootContainer root = new MediaContainer("", "");

      foreach (IRootContainer view in _views)
      {
        root.Items.Add(view);
      }

      LoadProviderPlugins();

      foreach (IProvider provider in _providers)
      {
        if (provider.RootContainers == null)
          continue;
        foreach (IRootContainer container in provider.RootContainers)
        {
          root.Items.Add(container);
          container.Parent = root;
        }
      }
      return root;
    }
    #endregion

    #region IStatus Implementation
    public List<string> GetStatus()
    {
      List<string> status = new List<string>();
      status.Add("=== MediaManager - Providers");
      foreach (IProvider provider in _providers)
      {
        status.Add(string.Format("     Provider = {0}", provider.Title));
      }
      status.Add("=== MediaManager - Views");
      foreach (IRootContainer _view in _views)
      {
        status.Add(string.Format("     View = {0}", _view.Title));
      }

      return status;
    }
    #endregion
  }
}
