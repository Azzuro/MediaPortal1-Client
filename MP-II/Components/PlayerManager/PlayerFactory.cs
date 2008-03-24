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
using MediaPortal.Core.Logging;
using MediaPortal.Presentation.Players;
using MediaPortal.Media.MediaManager;

namespace Components.Services.PlayerManager
{
  public class PlayerFactory : IPlayerFactory, IPlugin
  {
    List<IPlayerBuilder> _builders;
    bool _pluginServicesLoaded;

    public PlayerFactory()
    {
      _builders = new List<IPlayerBuilder>();
      _pluginServicesLoaded = false;
    }

    #region IPlugin Members
    public void Initialize(string id)
    {
    }

    public void Dispose()
    {
    }
    #endregion

    /// <summary>
    /// returns a player for the specified file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public IPlayer GetPlayer(IMediaItem mediaItem)
    {
      if (!_pluginServicesLoaded)
      {
        List<PlayerBuilder> builders = ServiceScope.Get<IPluginManager>().GetAllPluginItems<PlayerBuilder>("/Media/Players");
        foreach (PlayerBuilder playerBuilder in builders)
        {
          Register((IPlayerBuilder) playerBuilder);
        }

        _pluginServicesLoaded = true;
      }

      foreach (IPlayerBuilder builder in _builders)
      {
        if (builder.CanPlay(mediaItem, mediaItem.ContentUri))
        {
          IPlayer player = builder.GetPlayer(mediaItem, mediaItem.ContentUri);
          if (player != null) return player;
        }
      }
      return null;
    }

    public void Register(IPlayerBuilder builder)
    {
      _builders.Add(builder);
    }
  }
}
