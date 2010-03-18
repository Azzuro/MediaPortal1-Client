#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
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
using MediaPortal.Core.Logging;
using MediaPortal.Core.MediaManagement;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.Utilities.Exceptions;

namespace MediaPortal.UI.Services.Players
{
  /// <summary>
  /// Controller for one player slot. This class manages a player slot state, a current player, the audio setting
  /// (audio slot, volume, muted state), context variables and a <see cref="PlayerBuilderRegistration"/> instance.
  /// </summary>
  internal class PlayerSlotController : IPlayerSlotController
  {
    protected PlayerManager _playerManager;
    protected int _slotIndex;
    protected bool _isAudioSlot = false;
    protected PlayerBuilderRegistration _builderRegistration = null;
    protected IPlayer _player = null;
    protected readonly IDictionary<string, object> _contextVariables = new Dictionary<string, object>();
    protected PlayerSlotState _slotState = PlayerSlotState.Inactive;
    protected int _volume = 100;
    protected bool _isMuted = false;

    internal PlayerSlotController(PlayerManager parent, int slotIndex)
    {
      _playerManager = parent;
      _slotIndex = slotIndex;
    }

    protected object SyncObj
    {
      get { return _playerManager.SyncObj; }
    }

    /// <summary>
    /// Creates a new player for the specified <paramref name="locator"/> and <paramref name="mimeType"/>.
    /// </summary>
    /// <param name="locator">Resource locator of the media item to be played.</param>
    /// <param name="mimeType">Mime type of the media item to be played. May be <c>null</c>.</param>
    /// <returns><c>true</c>, if the player could be created, else <c>false</c>.</returns>
    internal bool CreatePlayer(IResourceLocator locator, string mimeType)
    {
      lock (_playerManager.SyncObj)
      {
        ReleasePlayer();
        _playerManager.BuildPlayer(locator, mimeType, this);
        if (_player != null)
        {
          // Initialize new player
          CheckAudio();
          RegisterPlayerEvents();
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Releases the current player.
    /// </summary>
    internal void ReleasePlayer()
    {
      lock (_playerManager.SyncObj)
      {
        if (_player != null)
        {
          ResetPlayerEvents();
          SetSlotState(PlayerSlotState.Stopped);
          if (_player.State != PlayerState.Stopped)
            _player.Stop();
          IDisposable d = _player as IDisposable;
          if (d != null)
            try
            {
              d.Dispose();
            }
            catch (Exception e)
            {
              ServiceScope.Get<ILogger>().Warn("Error disposing player '{0}'", e, d);
            }
          _player = null;
        }
        _playerManager.RevokePlayer(this);
      }
    }

    /// <summary>
    /// Returns the builder registration of the current player.
    /// </summary>
    /// <remarks>
    /// Access to the returned object has to be synchronized via the <see cref="_playerManager"/>'s
    /// <see cref="PlayerManager.SyncObj"/>.
    /// </remarks>
    internal PlayerBuilderRegistration BuilderRegistration
    {
      get { return _builderRegistration; }
    }

    /// <summary>
    /// Assigns both the current player and the builder registration.
    /// </summary>
    /// <param name="player">The player to be assigned to the <see cref="CurrentPlayer"/> property.</param>
    /// <param name="builderRegistration">The builder registration to be assigned to the <see cref="BuilderRegistration"/>
    /// property.</param>
    internal void AssignPlayerAndBuilderRegistration(IPlayer player, PlayerBuilderRegistration builderRegistration)
    {
      lock (_playerManager.SyncObj)
      {
        _player = player;
        _builderRegistration = builderRegistration;
        _builderRegistration.UsingSlotControllers.Add(this);
      }
    }

    /// <summary>
    /// Releases both the current player and the builder registration.
    /// </summary>
    internal void ResetPlayerAndBuilderRegistration()
    {
      lock (_playerManager.SyncObj)
      {
        _player = null;
        if (_builderRegistration != null)
        {
          _builderRegistration.UsingSlotControllers.Remove(this);
          _builderRegistration = null;
        }
      }
    }

    protected void CheckAudio()
    {
      lock (SyncObj)
      {
        if (_player == null)
          return;
        bool mute = !_isAudioSlot || _isMuted;
        try
        {
          IVolumeControl vc = _player as IVolumeControl;
          if (vc != null && mute && !vc.Mute)
            // If we are switching the audio off, first disable the audio before setting the volume -
            // perhaps both properties were changed and we want to avoid a short volume change before the audio gets disabled
            vc.Mute = true;
          if (vc != null)
            vc.Volume = _volume;
          if (vc != null)
            vc.Mute = mute;
        }
        catch (Exception e)
        {
          ServiceScope.Get<ILogger>().Warn("Error checking the audio state in player '{0}'", e, _player);
        }
      }
    }

    protected void CheckActive()
    {
      lock (_playerManager.SyncObj)
        if (_slotState == PlayerSlotState.Inactive)
          throw new IllegalCallException("PlayerSlotController: PSC is not active");
    }

    protected void RegisterPlayerEvents()
    {
      lock (SyncObj)
      {
        IPlayerEvents pe = _player as IPlayerEvents;
        if (pe != null)
          try
          {
            pe.InitializePlayerEvents(OnPlayerStarted, OnPlayerStateReady, OnPlayerStopped, OnPlayerEnded,
                OnPlaybackStateChanged, OnPlaybackError);
          }
          catch (Exception e)
          {
            ServiceScope.Get<ILogger>().Warn("Error initializing player events in player '{0}'", e, pe);
          }
        IReusablePlayer rp = _player as IReusablePlayer;
        if (rp != null)
          try
          {
            rp.NextItemRequest += OnNextItemRequest;
          }
          catch (Exception e)
          {
            ServiceScope.Get<ILogger>().Warn("Error initializing player NextItemRequest event in player '{0}'", e, rp);
          }
      }
    }

    protected void ResetPlayerEvents()
    {
      lock (SyncObj)
      {
        IPlayerEvents pe = _player as IPlayerEvents;
        if (pe != null)
          try
          {
            pe.ResetPlayerEvents();
          }
          catch (Exception e)
          {
            ServiceScope.Get<ILogger>().Warn("Error resetting player events in player '{0}'", e, pe);
          }
        IReusablePlayer rp = _player as IReusablePlayer;
        if (rp != null)
          try
          {
            rp.NextItemRequest -= OnNextItemRequest;
          }
          catch (Exception e)
          {
            ServiceScope.Get<ILogger>().Warn("Error resetting player NextItemRequest event in player '{0}'", e, rp);
          }
      }
    }

    internal void OnPlayerStarted(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerStarted, this);
    }

    internal void OnPlayerStateReady(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerStateReady, this);
    }

    internal void OnPlayerStopped(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerStopped, this);
    }

    internal void OnPlayerEnded(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerEnded, this);
    }

    internal void OnPlaybackStateChanged(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlaybackStateChanged, this);
    }

    internal void OnPlaybackError(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerError, this);
    }

    internal void OnNextItemRequest(IPlayer player)
    {
      PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.RequestNextItem, this);
    }

    protected void SetSlotState(PlayerSlotState slotState)
    {
      lock (SyncObj)
      {
        if (slotState == _slotState)
          return;
        PlayerSlotState oldSlotState = _slotState;
        _slotState = slotState;
        InvokeSlotStateChanged(slotState);
        if (oldSlotState == PlayerSlotState.Inactive && slotState != PlayerSlotState.Inactive)
          PlayerManagerMessaging.SendPlayerManagerPlayerMessage(PlayerManagerMessaging.MessageType.PlayerSlotActivated);
        if (oldSlotState != PlayerSlotState.Inactive || slotState != PlayerSlotState.Stopped)
          // Suppress "PlayerStopped" message if slot was activated
          switch (slotState)
          {
            case PlayerSlotState.Inactive:
              PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerSlotDeactivated, this);
              break;
            case PlayerSlotState.Playing:
              PlayerManagerMessaging.SendPlayerMessage(PlayerManagerMessaging.MessageType.PlayerSlotStarted, this);
              break;
            // Presentation.Players.PlayerSlotState.Stopped:
            // this is no extra message, as we sent the PlayerSlotActivated message above
          }
      }
    }

    protected void InvokeSlotStateChanged(PlayerSlotState slotState)
    {
      SlotStateChangedDlgt dlgt = SlotStateChanged;
      if (dlgt != null)
        dlgt(this, slotState);
    }

    #region IPlayerSlotController implementation

    public event SlotStateChangedDlgt SlotStateChanged;

    public int SlotIndex
    {
      get
      {
        lock (SyncObj)
          return _slotIndex;
      }
      internal set
      {
        lock (SyncObj)
          _slotIndex = value;
      }
    }

    public bool IsAudioSlot
    {
      get
      {
        lock (SyncObj)
          return _isAudioSlot;
      }
      internal set
      {
        lock (SyncObj)
        {
          bool wasChanged = value != _isAudioSlot;
          _isAudioSlot = value;
          CheckAudio();
          if (wasChanged)
            PlayerManagerMessaging.SendPlayerManagerPlayerMessage(PlayerManagerMessaging.MessageType.AudioSlotChanged, this);
        }
      }
    }

    public bool IsMuted
    {
      get
      {
        lock (SyncObj)
          return _isMuted;
      }
      internal set
      {
        lock (SyncObj)
        {
          _isMuted = value;
          CheckAudio();
        }
      }
    }

    public int Volume
    {
      get
      {
        lock (SyncObj)
          return _volume;
      }
      set
      {
        lock (SyncObj)
        {
          _volume = value;
          CheckAudio();
        }
      }
    }

    public bool IsActive
    {
      get
      {
        lock (SyncObj)
          return _slotState != PlayerSlotState.Inactive;
      }
      internal set
      {
        lock (SyncObj)
        {
          if (value == IsActive)
            return;
          if (value)
            SetSlotState(PlayerSlotState.Stopped);
          else
          {
            Reset();
            _isAudioSlot = false;
            SetSlotState(PlayerSlotState.Inactive);
          }
        }
      }
    }

    public PlayerSlotState PlayerSlotState
    {
      get
      {
        lock (SyncObj)
          return _slotState;
      }
    }

    public IPlayer CurrentPlayer
    {
      get
      {
        lock (SyncObj)
          return _player;
      }
    }

    public IDictionary<string, object> ContextVariables
    {
      get
      {
        lock (SyncObj)
        {
          CheckActive();
          return _contextVariables;
        }
      }
    }

    public bool Play(IResourceLocator locator, string mimeType, string mediaItemTitle, StartTime startTime)
    {
      bool result = false;
      lock (SyncObj)
        try
        {
          CheckActive();
          IReusablePlayer rp = _player as IReusablePlayer;
          if (rp != null)
            result = rp.NextItem(locator, mimeType, startTime);
          if (result)
            return true;
          if (CreatePlayer(locator, mimeType))
          {
            OnPlayerStarted(_player);
            IMediaPlaybackControl mpc = _player as IMediaPlaybackControl;
            if (mpc != null)
              mpc.Resume();
            return result = true;
          }
          return false;
        }
        catch (Exception e)
        {
          ServiceScope.Get<ILogger>().Warn("Error playing item '{0}'", e, locator);
          return result = false;
        }
        finally
        {
          if (result)
            SetSlotState(PlayerSlotState.Playing);
          if (_player != null)
            _player.SetMediaItemTitleHint(mediaItemTitle);
        }
    }

    public void Stop()
    {
      lock (SyncObj)
      {
        CheckActive();
        SetSlotState(PlayerSlotState.Stopped);
        // We need to simulate the PlayerStopped event, as the ReleasePlayer() method discards all further player events
        PlayerManagerMessaging.SendPlayerManagerPlayerMessage(PlayerManagerMessaging.MessageType.PlayerStopped, this);
        ReleasePlayer();
      }
    }

    public void Reset()
    {
      lock (SyncObj)
      {
        Stop();
        _contextVariables.Clear();
      }
    }

   #endregion
  }
}
