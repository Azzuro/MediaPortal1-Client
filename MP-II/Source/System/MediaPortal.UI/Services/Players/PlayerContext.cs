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
using MediaPortal.Core;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Core.MediaManagement.DefaultItemAspects;
using MediaPortal.Presentation.Geometries;
using MediaPortal.Presentation.Players;
using MediaPortal.Utilities.Exceptions;

namespace MediaPortal.Services.Players
{
  public class PlayerContext : IPlayerContext, IDisposable
  {
    #region Protected fields

    protected bool _closeWhenFinished = false;

    protected IPlayerSlotController _slotController;
    protected PlayerContextManager _contextManager;
    protected IPlaylist _playlist;
    protected Guid _mediaModuleId;
    protected string _name;
    protected PlayerContextType _type;
    protected Guid _currentlyPlayingWorkflowStateId;
    protected Guid _fullscreenContentWorkflowStateId;

    #endregion

    #region Ctor

    internal PlayerContext(PlayerContextManager contextManager, IPlayerSlotController slotController,
        Guid mediaModuleId, string name, PlayerContextType type,
        Guid currentlyPlayingWorkflowStateId, Guid fullscreenContentWorkflowStateId)
    {
      _contextManager = contextManager;
      _slotController = slotController;
      _playlist = new Playlist();
      _mediaModuleId = mediaModuleId;
      _name = name;
      _type = type;
      _currentlyPlayingWorkflowStateId = currentlyPlayingWorkflowStateId;
      _fullscreenContentWorkflowStateId = fullscreenContentWorkflowStateId;
    }

    public void Dispose()
    {
      _slotController = null;
    }

    #endregion

    protected static object SyncObj
    {
      get
      {
        IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
        return playerManager.SyncObj;
      }
    }

    protected static bool GetItemData(MediaItem item, out IMediaItemLocator locator, out string mimeType,
        out string mediaItemTitle)
    {
      locator = null;
      mimeType = null;
      mediaItemTitle = null;
      if (item == null)
        return false;
      IMediaManager mediaManager = ServiceScope.Get<IMediaManager>();
      locator = mediaManager.GetMediaItemLocator(item);
      MediaItemAspect mediaAspect = item[MediaAspect.ASPECT_ID];
      mimeType = (string) mediaAspect[MediaAspect.ATTR_MIME_TYPE];
      mediaItemTitle = (string) mediaAspect[MediaAspect.ATTR_TITLE];
      return locator != null;
    }

    protected IPlayer GetCurrentPlayer()
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return null;
      lock (SyncObj)
        return psc.IsActive ? psc.CurrentPlayer : null;
    }

    #region IPlayerContext implementation

    public bool IsValid
    {
      get { return _slotController != null; }
    }

    public Guid MediaModuleId
    {
       get { return _mediaModuleId; }
    }

    public PlayerContextType MediaType
    {
      get { return _type; }
    }

    public IPlaylist Playlist
    {
      get { return _playlist; }
    }

    public bool CloseWhenFinished
    {
      get { return _closeWhenFinished; }
      set { _closeWhenFinished = value; }
    }

    public IPlayer CurrentPlayer
    {
      get { return GetCurrentPlayer(); }
    }

    public PlaybackState PlayerState
    {
      get
      {
        IPlayer player = CurrentPlayer;
        if (player == null)
          return PlaybackState.Stopped;
        switch (player.State)
        {
          case Presentation.Players.PlayerState.Active:
            IMediaPlaybackControl mpc = player as IMediaPlaybackControl;
            if (mpc == null)
              return PlaybackState.Playing;
            if (mpc.IsPaused)
              return PlaybackState.Paused;
            else
              return PlaybackState.Playing;
          case Presentation.Players.PlayerState.Ended:
            return PlaybackState.Ended;
          case Presentation.Players.PlayerState.Stopped:
            return PlaybackState.Stopped;
          default:
            throw new UnexpectedStateException("Handling code for {0}.{1} is not implemented",
                typeof(PlayerState).Name, player.State);
        }
      }
    }

    public IPlayerSlotController PlayerSlotController
    {
      get { return _slotController; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Guid CurrentlyPlayingWorkflowStateId
    {
      get { return _currentlyPlayingWorkflowStateId; }
    }

    public Guid FullscreenContentWorkflowStateId
    {
      get { return _fullscreenContentWorkflowStateId; }
    }

    public bool DoPlay(MediaItem item)
    {
      IMediaItemLocator locator;
      string mimeType;
      string mediaItemTitle;
      if (!GetItemData(item, out locator, out mimeType, out mediaItemTitle))
        return false;
      return DoPlay(locator, mimeType, mediaItemTitle);
    }

    public bool DoPlay(IMediaItemLocator locator, string mimeType, string mediaItemTitle)
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return false;
      lock (SyncObj)
        if (!psc.IsActive)
          return false;
        else
          return psc.Play(locator, mimeType, mediaItemTitle);
    }

    public IEnumerable<AudioStreamDescriptor> GetAudioStreamDescriptors()
    {
      IVideoPlayer player = CurrentPlayer as IVideoPlayer;
      if (player == null)
        yield break;
      ICollection<string> audioStreamNames = player.AudioStreams;
      foreach (string streamName in audioStreamNames)
        yield return new AudioStreamDescriptor(this, player.Name, streamName);
    }

    public void OverrideGeometry(IGeometry geometry)
    {
      IPlayerSlotController slotController = _slotController;
      if (slotController == null)
        return;
      IVideoPlayer player = CurrentPlayer as IVideoPlayer;
      if (player == null)
        return;
      bool changed = player.GeometryOverride != geometry;
      player.GeometryOverride = geometry;
      if (changed)
        PlayerGeometryMessaging.SendGeometryChangedMessage(slotController.SlotIndex);
    }

    public void SetContextVariable(string key, object value)
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return;
      lock (SyncObj)
        psc.ContextVariables[key] = value;
    }

    public void ResetContextVariable(string key)
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return;
      lock (SyncObj)
        psc.ContextVariables.Remove(key);
    }

    public object GetContextVariable(string key)
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return null;
      lock (SyncObj)
      {
        object result;
        if (IsValid && _slotController.ContextVariables.TryGetValue(key, out result))
          return result;
      }
      return null;
    }

    public void Stop()
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return;
      lock (SyncObj)
        psc.Stop();
    }

    public void Pause()
    {
      IPlayer player = GetCurrentPlayer();
      if (player == null)
        return;
      IMediaPlaybackControl mpc = player as IMediaPlaybackControl;
      if (mpc != null)
        mpc.Pause();
    }

    public void Play()
    {
      IPlayer player = GetCurrentPlayer();
      if (player == null)
      {
        NextItem();
        return;
      }
      IMediaPlaybackControl mpc = player as IMediaPlaybackControl;
      if (mpc != null)
        mpc.Resume();
    }

    public void TogglePlayPause()
    {
      IPlayer player = GetCurrentPlayer();
      if (player == null)
      {
        NextItem();
        return;
      }
      IMediaPlaybackControl mpc = player as IMediaPlaybackControl;
      if (mpc == null)
        return;
      if (player.State == Presentation.Players.PlayerState.Active)
        if (mpc.IsPaused)
          mpc.Resume();
        else
          mpc.Pause();
      else
        mpc.Restart();
    }

    public void Restart()
    {
      IPlayer player = GetCurrentPlayer();
      if (player == null)
      {
        NextItem();
        return;
      }
      IMediaPlaybackControl mpc = player as IMediaPlaybackControl;
      if (mpc != null)
        mpc.Restart();
    }

    public bool PreviousItem()
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return false;
      // Locking not necessary here. If a lock should be placed in future, be aware that the DoPlay method
      // will lock the PM as well
      MediaItem item = _playlist.Previous();
      return item != null && DoPlay(item);
    }

    public bool NextItem()
    {
      IPlayerSlotController psc = _slotController;
      if (psc == null)
        return false;
      // Locking not necessary here. If a lock should be placed in future, be aware that the DoPlay method
      // will lock the PM as well
      MediaItem item = _playlist.Next();
      return item != null && DoPlay(item);
    }

    #endregion
  }
}
