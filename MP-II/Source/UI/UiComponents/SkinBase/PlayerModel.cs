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
using System.Timers;
using MediaPortal.Control.InputManager;
using MediaPortal.Core;
using MediaPortal.Core.Messaging;
using MediaPortal.Presentation.Actions;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Presentation.Models;
using MediaPortal.Presentation.Players;
using MediaPortal.Presentation.Screens;
using MediaPortal.Presentation.Workflow;
using Timer=System.Timers.Timer;

namespace UiComponents.SkinBase
{
  /// <summary>
  /// This model provides skin data for the current primary player/playing state.
  /// </summary>
  public class PlayerModel : IDisposable, IWorkflowModel
  {
    public const string PLAYER_MODEL_ID_STR = "A2F24149-B44C-498b-AE93-288213B87A1A";

    public const string CURRENTLY_PLAYING_STATE_ID_STR = "5764A810-F298-4a20-BF84-F03D16F775B1";

    public const string FULLSCREENVIDEO_SCREEN_NAME = "FullScreenVideo";
    public const string FULLSCREENAUDIO_SCREEN_NAME = "FullScreenAudio";
    public const string FULLSCREENPICTURE_SCREEN_NAME = "FullScreenPicture";

    public static Guid PLAYER_MODEL_ID = new Guid(PLAYER_MODEL_ID_STR);
    public static Guid CURRENTLY_PLAYING_STATE_ID = new Guid(CURRENTLY_PLAYING_STATE_ID_STR);

    public const int VOLUME_CHANGE = 10;

    protected Timer _timer;

    protected Property _isPausedProperty;
    protected Property _isRunningProperty;
    protected Property _isMutedProperty;
    protected Property _isPlayerActiveProperty;
    protected Property _isPlayControlsVisibleProperty;
    protected Property _isPipProperty;
    protected Property _isVideoInfoVisibleProperty;
    protected Property _currentPlayerSlotProperty;
    protected Property _isCurrentAudioProperty;
    protected Property _showCurrentPlayerIndicatorProperty;

    protected string _currentlyPlayingScreen = null;
    protected ICollection<Key> _registeredKeyBindings;

    public PlayerModel()
    {
      _isPausedProperty = new Property(typeof(bool), false);
      _isRunningProperty = new Property(typeof(bool), false);
      _isPlayerActiveProperty = new Property(typeof(bool), false);
      _isMutedProperty = new Property(typeof(bool), false);
      _isPlayControlsVisibleProperty = new Property(typeof(bool), false);
      _isPipProperty = new Property(typeof(bool), false);
      _isVideoInfoVisibleProperty = new Property(typeof(bool), false);
      _currentPlayerSlotProperty = new Property(typeof(int), -1);
      _isCurrentAudioProperty = new Property(typeof(bool), false);
      _showCurrentPlayerIndicatorProperty = new Property(typeof(bool), false);

      _currentPlayerSlotProperty.Attach(OnCurrentPlayerSlotChanged);

      _registeredKeyBindings = new List<Key>();

      SubscribeToMessages();
    }

    public void Dispose()
    {
      _timer.Elapsed -= OnTimerElapsed;
      _timer.Enabled = false;
      UnsubscribeFromMessages();
      UnregisterKeyBindings();
    }

    protected void StartListening()
    {
      // Setup timer to update the properties
      _timer = new Timer(500);
      _timer.Elapsed += OnTimerElapsed;
      _timer.Enabled = true;

      CheckCurrentPlayerSlot();
      UpdatePlayControls();
      CheckIsCurrentAudio();
      UpdateKeyBindings();
    }

    protected void OnSystemMessageReceived(QueueMessage message)
    {
      SystemMessaging.MessageType messageType =
          (SystemMessaging.MessageType) message.MessageData[SystemMessaging.MESSAGE_TYPE];
      if (messageType == SystemMessaging.MessageType.SystemStarted)
      {
        StartListening();
        IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
        broker.GetOrCreate(SystemMessaging.QUEUE).MessageReceived -= OnSystemMessageReceived;
      }
    }

    protected void SubscribeToMessages()
    {
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
      broker.GetOrCreate(PlayerManagerMessaging.QUEUE).MessageReceived += OnPlayerManagerMessageReceived;
      broker.GetOrCreate(SystemMessaging.QUEUE).MessageReceived += OnSystemMessageReceived;
    }

    protected void UnsubscribeFromMessages()
    {
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>(false);
      if (broker == null)
        return;
      broker.GetOrCreate(PlayerManagerMessaging.QUEUE).MessageReceived -= OnPlayerManagerMessageReceived;
      // SytemMessaging queue is unregistered as soon as the system is started
    }

    protected void OnPlayerManagerMessageReceived(QueueMessage message)
    {
      PlayerManagerMessaging.MessageType messageType =
          (PlayerManagerMessaging.MessageType) message.MessageData[PlayerManagerMessaging.MESSAGE_TYPE];
      switch (messageType)
      {
        case PlayerManagerMessaging.MessageType.PlayerStopped:
          if (_currentlyPlayingScreen != null)
          {
            IWorkflowManager workflowManager = ServiceScope.Get<IWorkflowManager>();
            // Maybe we should do more handling in this case - show dialogs "do you want to delete"
            // etc.? At the moment we'll simply return to the last workflow state.
            workflowManager.NavigatePop(1);
            // _currentlyPlayingScreen will be reset by ExitModelContext
          }
          break;
        case PlayerManagerMessaging.MessageType.PlayerEnded:
          if (_currentlyPlayingScreen != null)
          {
            // TODO: Leave currently playing state if no playlist is running?
          }
          break;
        case PlayerManagerMessaging.MessageType.PlayerStarted:
          if (_currentlyPlayingScreen != null)
            // Automatically switch "currently playing" screen if another player is started. This will
            // ensure that the screen is correctly updated when the playlist progresses.
            UpdateCurrentlyPlayingScreen();
          break;
        case PlayerManagerMessaging.MessageType.PlayerSlotActivated:
        case PlayerManagerMessaging.MessageType.PlayerSlotDeactivated:
          CheckCurrentPlayerSlot();
          break;
        case PlayerManagerMessaging.MessageType.AudioSlotChanged:
          CheckIsCurrentAudio();
          break;
      }
      UpdatePlayControls();
    }

    protected void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      UpdatePlayControls();
    }

    protected void OnCurrentPlayerSlotChanged(Property prop, object oldValue)
    {
      CheckIsCurrentAudio();
      UpdateKeyBindings();
    }

    protected void CheckIsCurrentAudio()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      IsCurrentAudio = CurrentPlayerSlot != -1 && CurrentPlayerSlot == playerManager.AudioSlotIndex;
    }

    protected void UpdateKeyBindings()
    {
      UnregisterKeyBindings();
      RegisterKeyBindings();
    }

    protected void RegisterKeyBindings()
    {
      IPlayerSlotController currentPSC = GetCurrentPlayerSlotController();
      if (currentPSC == null || !currentPSC.IsActive)
        return;
      // TODO: Is there a ZoomMode/Change Aspect Ratio key in any input device (keyboard, IR, ...)? If yes,
      // we should register it here too
      AddKeyBinding(Key.Play, () =>
        {
          Play();
          return true;
        });
      AddKeyBinding(Key.Pause, () =>
        {
          Pause();
          return true;
        });
      AddKeyBinding(Key.PlayPause, () =>
        {
          TogglePause();
          return true;
        });
      AddKeyBinding(Key.Printable(' '), () =>
        {
          TogglePause();
          return true;
        });
      AddKeyBinding(Key.Stop, () =>
        {
          Stop();
          return true;
        });
      AddKeyBinding(Key.Rew, () =>
        {
          SeekBackward();
          return true;
        });
      AddKeyBinding(Key.Fwd, () =>
        {
          SeekForward();
          return true;
        });
      AddKeyBinding(Key.Previous, () =>
        {
          Previous();
          return true;
        });
      AddKeyBinding(Key.Next, () =>
        {
          Next();
          return true;
        });
      AddKeyBinding(Key.Mute, () =>
        {
          ToggleMuteAudioPlayer();
          return true;
        });
      AddKeyBinding(Key.VolumeUp, () =>
        {
          VolumeUp();
          return true;
        });
      AddKeyBinding(Key.VolumeDown, () =>
        {
          VolumeDown();
          return true;
        });
      // Register player specific key bindings
      // TODO: Register key bindings from current player
    }

    protected void AddKeyBinding(Key key, ActionDlgt action)
    {
      _registeredKeyBindings.Add(key);
      IInputManager inputManager = ServiceScope.Get<IInputManager>();
      inputManager.AddKeyBinding(key, action);
    }

    protected void UnregisterKeyBindings()
    {
      IInputManager inputManager = ServiceScope.Get<IInputManager>(false);
      if (inputManager == null)
        return;
      foreach (Key key in _registeredKeyBindings)
        inputManager.RemoveKeyBinding(key);
    }

    protected void CheckCurrentPlayerSlot()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      bool primaryPlayerActive = playerManager.GetSlot(PlayerManagerConsts.PRIMARY_SLOT).IsActive;
      bool secondaryPlayerActive = playerManager.GetSlot(PlayerManagerConsts.SECONDARY_SLOT).IsActive;
      int currentPlayerSlot = CurrentPlayerSlot;
      if (currentPlayerSlot == PlayerManagerConsts.PRIMARY_SLOT && !primaryPlayerActive)
        currentPlayerSlot = -1;
      else if (currentPlayerSlot == PlayerManagerConsts.SECONDARY_SLOT && !secondaryPlayerActive)
        currentPlayerSlot = -1;
      if (currentPlayerSlot == -1)
        if (secondaryPlayerActive)
          currentPlayerSlot = PlayerManagerConsts.SECONDARY_SLOT;
        else if (primaryPlayerActive)
          currentPlayerSlot = PlayerManagerConsts.PRIMARY_SLOT;
      CurrentPlayerSlot = currentPlayerSlot;
      ShowCurrentPlayerIndicator = playerManager.NumOpenSlots > 1;
    }

    protected void UpdatePlayControls()
    {
      IScreenControl screenControl = ServiceScope.Get<IScreenControl>();
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      IPlayer player = playerManager[0];
      if (player == null)
      {
        IsPaused = false;
        IsRunning = false;
        IsPlayerActive = false;
        IsMuted = false;
        IsPlayControlsVisible = false;
        IsVideoInfoVisible = false;

        IsPip = false;
      }
      else
      {
        player.UpdateTime();
        IsPaused = player.State == PlaybackState.Paused;
        IsRunning = player.State == PlaybackState.Playing;
        IsPlayerActive = true;
        IVolumeControl volumeControl = player as IVolumeControl;
        IsMuted = volumeControl != null && volumeControl.Mute;
        IsPlayControlsVisible = screenControl.IsMouseUsed;
        // TODO: Trigger video info overlay by a button
        IsVideoInfoVisible = screenControl.IsMouseUsed;

        // TODO: PIP configuration
        IsPip = false;
      }
    }

    /// <summary>
    /// Returns the player slot controller for the current player slot. The current player governs which
    /// "currently playing" screen is shown. If the primary player is a video player, the fullscreen video screen
    /// will be shown, for example.
    /// </summary>
    /// <returns>Player slot controller for the current player. It must be checked if the returned slot controller
    /// is active (<see cref="IPlayerSlotController.IsActive"/>) before it can be used.</returns>
    protected IPlayerSlotController GetCurrentPlayerSlotController()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      int currentPlayerSlot = CurrentPlayerSlot;
      if (currentPlayerSlot == -1)
        currentPlayerSlot = PlayerManagerConsts.PRIMARY_SLOT;
      return playerManager.GetSlot(currentPlayerSlot);
    }

    protected static bool CanHandlePlayer(IPlayer player)
    {
      return player is IVideoPlayer || player is IAudioPlayer || player is IPicturePlayer;
    }

    protected void UpdateCurrentlyPlayingScreen()
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      if (psc == null || !psc.IsActive)
        return;
      string targetScreen;
      IPlayer currentPlayer = psc.CurrentPlayer;
      if (!CanHandlePlayer(currentPlayer))
        return;
      if (currentPlayer is IVideoPlayer)
        targetScreen = FULLSCREENVIDEO_SCREEN_NAME;
      else if (currentPlayer is IAudioPlayer)
        targetScreen = FULLSCREENAUDIO_SCREEN_NAME;
      else if (currentPlayer is IPicturePlayer)
        targetScreen = FULLSCREENPICTURE_SCREEN_NAME;
      else
          // Error case: The current player isn't recognized - its none of our supported players
        targetScreen = FULLSCREENVIDEO_SCREEN_NAME;
      if (_currentlyPlayingScreen != targetScreen)
      {
        IScreenManager screenManager = ServiceScope.Get<IScreenManager>();
        screenManager.ShowScreen(targetScreen);
        _currentlyPlayingScreen = targetScreen;
      }
    }

    protected static void ChangeVolume(int relativeValue)
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      IPlayerSlotController psc = playerManager.GetSlot(playerManager.AudioSlotIndex);
      if (psc == null)
        return;
      IVolumeControl player = psc.CurrentPlayer as IVolumeControl;
      if (player != null)
        player.Volume = Math.Min(player.Volume + relativeValue, 100);
    }

    public Property IsPipProperty
    {
      get { return _isPipProperty; }
    }

    public bool IsPip
    {
      get { return (bool) _isPipProperty.GetValue(); }
      set { _isPipProperty.SetValue(value); }
    }

    public Property IsPausedProperty
    {
      get { return _isPausedProperty; }
    }

    public bool IsPaused
    {
      get { return (bool) _isPausedProperty.GetValue(); }
      set { _isPausedProperty.SetValue(value); }
    }

    public Property IsRunningProperty
    {
      get { return _isRunningProperty; }
    }

    public bool IsRunning
    {
      get { return (bool) _isRunningProperty.GetValue(); }
      set { _isRunningProperty.SetValue(value); }
    }

    public Property IsMutedProperty
    {
      get { return _isMutedProperty; }
    }

    public bool IsMuted
    {
      get { return (bool) _isMutedProperty.GetValue(); }
      set { _isMutedProperty.SetValue(value); }
    }

    public Property IsPlayControlsVisibleProperty
    {
      get { return _isPlayControlsVisibleProperty; }
    }

    public bool IsPlayControlsVisible
    {
      get { return (bool) _isPlayControlsVisibleProperty.GetValue(); }
      set { _isPlayControlsVisibleProperty.SetValue(value); }
    }

    public Property IsPlayerActiveProperty
    {
      get { return _isPlayerActiveProperty; }
    }

    public bool IsPlayerActive
    {
      get { return (bool) _isPlayerActiveProperty.GetValue(); }
      set { _isPlayerActiveProperty.SetValue(value); }
    }

    public Property IsVideoInfoVisibleProperty
    {
      get { return _isVideoInfoVisibleProperty; }
    }

    public bool IsVideoInfoVisible
    {
      get { return (bool) _isVideoInfoVisibleProperty.GetValue(); }
      set { _isVideoInfoVisibleProperty.SetValue(value); }
    }

    public Property CurrentPlayerSlotProperty
    {
      get { return _currentPlayerSlotProperty; }
    }

    public int CurrentPlayerSlot
    {
      get { return (int) _currentPlayerSlotProperty.GetValue(); }
      set { _currentPlayerSlotProperty.SetValue(value); }
    }

    public Property IsCurrentAudioProperty
    {
      get { return _isCurrentAudioProperty; }
    }

    public bool IsCurrentAudio
    {
      get { return (bool) _isCurrentAudioProperty.GetValue(); }
      set { _isCurrentAudioProperty.SetValue(value); }
    }

    public Property ShowCurrentPlayerIndicatorProperty
    {
      get { return _showCurrentPlayerIndicatorProperty; }
    }

    public bool ShowCurrentPlayerIndicator
    {
      get { return (bool) _showCurrentPlayerIndicatorProperty.GetValue(); }
      set { _showCurrentPlayerIndicatorProperty.SetValue(value); }
    }

    public void Play()
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      if (psc == null || !psc.IsActive)
        return;
      if (psc.PlayerSlotState == PlayerSlotState.Paused)
        psc.Pause();
      else
        psc.Restart();
    }

    public void Pause()
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      if (psc == null || !psc.IsActive)
        return;
      psc.Pause();
    }

    public void TogglePause()
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      if (psc == null || !psc.IsActive)
        return;
      switch (psc.PlayerSlotState) {
        case PlayerSlotState.Playing:
          psc.Pause();
          break;
        case PlayerSlotState.Paused:
          psc.Play();
          break;
        default:
          psc.Restart();
          break;
      }
    }

    public void Stop()
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      if (psc == null || !psc.IsActive)
        return;
      psc.Stop();
    }

    public void SeekBackward()
    {
      // TODO
      IDialogManager dialogManager = ServiceScope.Get<IDialogManager>();
      dialogManager.ShowDialog("Not implemented", "The BKWD command is not implemented yet", DialogType.OkDialog, false);
    }

    public void SeekForward()
    {
      // TODO
      IDialogManager dialogManager = ServiceScope.Get<IDialogManager>();
      dialogManager.ShowDialog("Not implemented", "The FWD command is not implemented yet", DialogType.OkDialog, false);
    }

    public void Previous()
    {
      // TODO
      IDialogManager dialogManager = ServiceScope.Get<IDialogManager>();
      dialogManager.ShowDialog("Not implemented", "The PREV command is not implemented yet", DialogType.OkDialog, false);
    }

    public void Next()
    {
      // TODO
      IDialogManager dialogManager = ServiceScope.Get<IDialogManager>();
      dialogManager.ShowDialog("Not implemented", "The NEXT command is not implemented yet", DialogType.OkDialog, false);
    }

    public void VolumeUp()
    {
      ChangeVolume(VOLUME_CHANGE);
    }

    public void VolumeDown()
    {
      ChangeVolume(-VOLUME_CHANGE);
    }

    public void ToggleMuteAudioPlayer()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      IPlayerSlotController psc = playerManager.GetSlot(playerManager.AudioSlotIndex);
      if (psc == null)
        return;
      IVolumeControl player = psc.CurrentPlayer as IVolumeControl;
      if (player != null)
        player.Mute = !player.Mute;
    }

    public void ShowCurrentlyPlaying()
    {
      IWorkflowManager workflowManager = ServiceScope.Get<IWorkflowManager>();
      workflowManager.NavigatePush(new Guid(CURRENTLY_PLAYING_STATE_ID_STR));
    }

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return PLAYER_MODEL_ID; }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      IPlayerSlotController psc = GetCurrentPlayerSlotController();
      return psc != null && psc.IsActive && CanHandlePlayer(psc.CurrentPlayer);
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      if (newContext.WorkflowState.StateId == CURRENTLY_PLAYING_STATE_ID)
        UpdateCurrentlyPlayingScreen();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      if (oldContext.WorkflowState.StateId == CURRENTLY_PLAYING_STATE_ID)
        _currentlyPlayingScreen = null;
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      // Not implemented
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Not implemented
    }

    public void ReActivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Not implemented
    }

    public void UpdateMenuActions(NavigationContext context, ICollection<WorkflowStateAction> actions)
    {
      // Not implemented yet
    }

    #endregion
  }
}
