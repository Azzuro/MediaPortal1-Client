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
using MediaPortal.Core.Commands;
using MediaPortal.Core.Messaging;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Presentation.Localization;
using MediaPortal.Presentation.Models;
using MediaPortal.Presentation.Players;
using MediaPortal.Presentation.Workflow;

namespace UiComponents.SkinBase.Models
{
  /// <summary>
  /// This model attends the dialogs "DialogPlayerConfiguration" and "DialogChooseAudioStream".
  /// </summary>
  public class PlayerConfigurationDialogModel : BaseMessageControlledUIModel, IWorkflowModel
  {
    public const string PLAYER_CONFIGURATION_DIALOG_MODEL_ID_STR = "58A7F9E3-1514-47af-8E83-2AD60BA8A037";
    public const string PLAYER_CONFIGURATION_DIALOG_STATE_ID_STR = "D0B79345-69DF-4870-B80E-39050434C8B3";
    public const string CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID_STR = "A3F53310-4D93-4f93-8B09-D53EE8ACD829";

    public static Guid PLAYER_CONFIGURATION_DIALOG_MODEL_ID = new Guid(PLAYER_CONFIGURATION_DIALOG_MODEL_ID_STR);
    public static Guid PLAYER_CONFIGURATION_DIALOG_STATE_ID = new Guid(PLAYER_CONFIGURATION_DIALOG_STATE_ID_STR);
    public static Guid CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID = new Guid(CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID_STR);

    protected const string KEY_NAME = "Name";

    protected const string PLAYER_OF_TYPE_RESOURCE = "[Players.PlayerOfType]";
    protected const string SLOT_NO_RESOURCE = "[Players.SlotNo]";
    protected const string FOCUS_PLAYER_RESOURCE = "[Players.FocusPlayer]";
    protected const string SWITCH_PIP_PLAYERS_RESOURCE = "[Players.SwitchPipPlayers]";
    protected const string CHOOSE_AUDIO_STREAM_RESOURCE = "[Players.ChooseAudioStream]";
    protected const string MUTE_RESOURCE = "[Players.Mute]";
    protected const string MUTE_OFF_RESOURCE = "[Players.MuteOff]";
    protected const string CLOSE_PLAYER_CONTEXT_RESOURCE = "[Players.ClosePlayerContext]";

    protected ItemsList _playerConfigurationMenu = new ItemsList();
    protected ItemsList _audioStreamsMenu = new ItemsList();
    protected object _syncObj = new object();
    protected bool _inPlayerConfigurationDialog = false;
    protected bool _inChooseAudioStreamDialog = false;

    public PlayerConfigurationDialogModel()
    {
      SubscribeToMessages();
    }

    void SubscribeToMessages()
    {
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
      broker.GetOrCreate(PlayerManagerMessaging.QUEUE).MessageReceived_Async += OnPlayerManagerMessageReceived;
      broker.GetOrCreate(PlayerContextManagerMessaging.QUEUE).MessageReceived_Async += OnPlayerContextManagerMessageReceived;
    }

    protected override void UnsubscribeFromMessages()
    {
      base.UnsubscribeFromMessages();
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
      broker.GetOrCreate(PlayerManagerMessaging.QUEUE).MessageReceived_Async -= OnPlayerManagerMessageReceived;
      broker.GetOrCreate(PlayerContextManagerMessaging.QUEUE).MessageReceived_Async -= OnPlayerContextManagerMessageReceived;
    }

    protected void OnPlayerManagerMessageReceived(QueueMessage message)
    {
      PlayerManagerMessaging.MessageType messageType =
          (PlayerManagerMessaging.MessageType) message.MessageData[PlayerManagerMessaging.MESSAGE_TYPE];
      switch (messageType)
      {
        case PlayerManagerMessaging.MessageType.PlayerSlotActivated:
        case PlayerManagerMessaging.MessageType.PlayerSlotDeactivated:
        case PlayerManagerMessaging.MessageType.PlayerStarted:
        case PlayerManagerMessaging.MessageType.PlayerStopped:
        case PlayerManagerMessaging.MessageType.PlayersMuted:
        case PlayerManagerMessaging.MessageType.PlayersResetMute:
          CheckUpdatePlayerConfigurationData();
          break;
      }
    }

    protected void OnPlayerContextManagerMessageReceived(QueueMessage message)
    {
      PlayerContextManagerMessaging.MessageType messageType =
          (PlayerContextManagerMessaging.MessageType) message.MessageData[PlayerContextManagerMessaging.MESSAGE_TYPE];
      switch (messageType)
      {
        case PlayerContextManagerMessaging.MessageType.CurrentPlayerChanged:
          CheckUpdatePlayerConfigurationData();
          break;
      }
    }

    protected static string GetNameForPlayerContext(IPlayerContextManager playerContextManager, int playerSlot)
    {
      IPlayerContext pc = playerContextManager.GetPlayerContext(playerSlot);
      if (pc == null)
        return null;
      IPlayer player = pc.CurrentPlayer;
      if (player == null)
      {
        IResourceString playerOfType = LocalizationHelper.CreateResourceString(PLAYER_OF_TYPE_RESOURCE); // "{0} player"
        IResourceString slotNo = LocalizationHelper.CreateResourceString(SLOT_NO_RESOURCE); // "Slot #{0}"
        return playerOfType.Evaluate(pc.MediaType.ToString()) + " (" + slotNo.Evaluate(playerSlot.ToString()) + ")"; // "Video player (Slot #1)"
      }
      else
        return player.Name + ": " + player.MediaItemTitle;
    }

    protected void UpdatePlayerConfigurationMenu()
    {
      // Some updates could be avoided if we tracked a "dirty" flag and break execution if !dirty
      lock (_syncObj)
      {
        IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
        IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
        int numActiveSlots = playerManager.NumActiveSlots;
        // Build player configuration menu
        _playerConfigurationMenu.Clear();
        if (numActiveSlots > 1)
        {
          // Set player focus
          int newCurrentPlayer = 1 - playerContextManager.CurrentPlayerIndex;
          string name = GetNameForPlayerContext(playerContextManager, newCurrentPlayer);
          if (name != null)
          {
            ListItem item = new ListItem(KEY_NAME, LocalizationHelper.CreateResourceString(FOCUS_PLAYER_RESOURCE).Evaluate(name))
              {
                  Command = new MethodDelegateCommand(() => SetCurrentPlayer(newCurrentPlayer))
              };
            _playerConfigurationMenu.Add(item);
          }
        }
        if (numActiveSlots > 1 && playerContextManager.IsPipActive)
        {
          ListItem item = new ListItem(KEY_NAME, SWITCH_PIP_PLAYERS_RESOURCE)
            {
                Command = new MethodDelegateCommand(SwitchPrimarySecondaryPlayer)
            };
          _playerConfigurationMenu.Add(item);
        }
        ICollection<AudioStreamDescriptor> audioStreams = playerContextManager.GetAvailableAudioStreams();
        if (audioStreams.Count > 1)
        {
          ListItem item = new ListItem(KEY_NAME, CHOOSE_AUDIO_STREAM_RESOURCE)
            {
                Command = new MethodDelegateCommand(OpenChooseAudioStreamDialog)
            };
          _playerConfigurationMenu.Add(item);
        }
        if (numActiveSlots > 0)
        {
          ListItem item;
          if (playerManager.Muted)
            item = new ListItem(KEY_NAME, MUTE_OFF_RESOURCE)
              {
                  Command = new MethodDelegateCommand(PlayersResetMute)
              };
          else
            item = new ListItem(KEY_NAME, MUTE_RESOURCE)
              {
                  Command = new MethodDelegateCommand(PlayersMute)
              };
          _playerConfigurationMenu.Add(item);
        }
        // TODO: Handle subtitles same as audio streams
        for (int i = 0; i < numActiveSlots; i++)
        {
          string name = GetNameForPlayerContext(playerContextManager, i);
          if (name != null)
          {
            int indexClosureCopy = i;
            ListItem item = new ListItem(KEY_NAME, LocalizationHelper.CreateResourceString(CLOSE_PLAYER_CONTEXT_RESOURCE).Evaluate(name))
              {
                  Command = new MethodDelegateCommand(() => ClosePlayerContext(indexClosureCopy))
              };
            _playerConfigurationMenu.Add(item);
          }
        }
        _playerConfigurationMenu.FireChange();
      }
    }

    protected void UpdateAudioStreamsMenu()
    {
      // Some updates could be avoided if we tracked a "dirty" flag and break execution if !dirty
      lock (_syncObj)
      {
        IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
        ICollection<AudioStreamDescriptor> audioStreams = playerContextManager.GetAvailableAudioStreams();

        // Build audio streams menu
        _audioStreamsMenu.Clear();
        // Cluster by player
        IDictionary<IPlayerContext, ICollection<AudioStreamDescriptor>> streamsByPlayerContext =
            new Dictionary<IPlayerContext, ICollection<AudioStreamDescriptor>>();
        foreach (AudioStreamDescriptor asd in audioStreams)
        {
          IPlayerContext pc = asd.PlayerContext;
          ICollection<AudioStreamDescriptor> asds;
          if (!streamsByPlayerContext.TryGetValue(pc, out asds))
            streamsByPlayerContext[pc] = asds = new List<AudioStreamDescriptor>();
          asds.Add(asd);
        }
        foreach (KeyValuePair<IPlayerContext, ICollection<AudioStreamDescriptor>> pasds in streamsByPlayerContext)
        {
          IPlayerContext pc = pasds.Key;
          IPlayer player = pc.CurrentPlayer;
          foreach (AudioStreamDescriptor asd in pasds.Value)
          {
            string playedItem = player == null ? null : player.MediaItemTitle;
            if (playedItem == null)
              playedItem = pc.Name;
            string choiceItemName;
            if (pasds.Value.Count > 1)
                // Only display the audio stream name if the player has more than one audio stream
              choiceItemName = playedItem + ": " + asd.AudioStreamName;
            else
              choiceItemName = playedItem;
            AudioStreamDescriptor asdClosureCopy = asd;
            ListItem item = new ListItem(KEY_NAME, choiceItemName)
              {
                  Command = new MethodDelegateCommand(() => ChooseAudioStream(asdClosureCopy))
              };
            _audioStreamsMenu.Add(item);
          }
        }
        _audioStreamsMenu.FireChange();
      }
    }

    /// <summary>
    /// Updates the menu items for the dialogs "DialogPlayerConfiguration" and "DialogChooseAudioStream"
    /// and closes the dialogs when their entries are not valid any more.
    /// </summary>
    protected void CheckUpdatePlayerConfigurationData()
    {
      IWorkflowManager workflowManager = ServiceScope.Get<IWorkflowManager>();

      lock (_syncObj)
      {
        if (_inPlayerConfigurationDialog)
        {
          UpdatePlayerConfigurationMenu();
          if (_playerConfigurationMenu.Count == 0)
          {
            // Automatically close player configuration dialog
            while (_inPlayerConfigurationDialog)
              workflowManager.NavigatePop(1);
          }
        }
        if (_inChooseAudioStreamDialog)
        {
          UpdateAudioStreamsMenu();
          if (_audioStreamsMenu.Count <= 1)
          {
            // Automatically close audio stream choice dialog
            while (_inChooseAudioStreamDialog)
              workflowManager.NavigatePop(1);
          }
        }
      }
    }

    /// <summary>
    /// Returns the player context for the current focused player. The current player governs which
    /// "currently playing" screen is shown.
    /// </summary>
    /// <returns>Player context for the current player or <c>null</c>, if there is no current player.</returns>
    protected static IPlayerContext GetCurrentPlayerContext()
    {
      IPlayerContextManager pcm = ServiceScope.Get<IPlayerContextManager>();
      int currentPlayerSlot = pcm.CurrentPlayerIndex;
      if (currentPlayerSlot == -1)
        currentPlayerSlot = PlayerManagerConsts.PRIMARY_SLOT;
      return pcm.GetPlayerContext(currentPlayerSlot);
    }

    public override Guid ModelId
    {
      get { return PLAYER_CONFIGURATION_DIALOG_MODEL_ID; }
    }

    #region Members to be accessed from the GUI

    public ItemsList PlayerConfigurationMenu
    {
      get { return _playerConfigurationMenu; }
    }

    public ItemsList AudioStreamsMenu
    {
      get { return _audioStreamsMenu; }
    }

    public void ExecuteMenuItem(ListItem item)
    {
      if (item == null)
        return;
      ICommand command = item.Command;
      if (command != null)
        command.Execute();
    }

    public void SetCurrentPlayer(int playerIndex)
    {
      IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
      playerContextManager.CurrentPlayerIndex = playerIndex;
    }

    public void ClosePlayerContext(int playerIndex)
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerManager.CloseSlot(playerIndex);
    }

    public void PlayersMute()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerManager.Muted = true;
    }

    public void PlayersResetMute()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerManager.Muted = false;
    }

    public void SwitchPrimarySecondaryPlayer()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerManager.SwitchSlots();
    }

    public void OpenChooseAudioStreamDialog()
    {
      IWorkflowManager workflowManager = ServiceScope.Get<IWorkflowManager>();
      workflowManager.NavigatePush(CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID);
    }

    public void ChooseAudioStream(AudioStreamDescriptor asd)
    {
      IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerContextManager.SetAudioStream(asd);
      playerManager.Muted = false;
    }

    #endregion

    #region IWorkflowModel implementation

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      if (newContext.WorkflowState.StateId == PLAYER_CONFIGURATION_DIALOG_STATE_ID)
      {
        UpdatePlayerConfigurationMenu();
        return _playerConfigurationMenu.Count > 0;
      }
      else if (newContext.WorkflowState.StateId == CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID)
      {
        UpdateAudioStreamsMenu();
        return _audioStreamsMenu.Count > 0;
      }
      else
        return false;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      if (newContext.WorkflowState.StateId == PLAYER_CONFIGURATION_DIALOG_STATE_ID)
      {
        UpdatePlayerConfigurationMenu();
        _inPlayerConfigurationDialog = true;
      }
      else if (newContext.WorkflowState.StateId == CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID)
      {
        UpdateAudioStreamsMenu();
        _inChooseAudioStreamDialog = true;
      }
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      if (oldContext.WorkflowState.StateId == PLAYER_CONFIGURATION_DIALOG_STATE_ID)
        _inPlayerConfigurationDialog = false;
      else if (oldContext.WorkflowState.StateId == CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID)
        _inChooseAudioStreamDialog = false;
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      if (!push)
      {
        if (oldContext.WorkflowState.StateId == PLAYER_CONFIGURATION_DIALOG_STATE_ID)
          _inPlayerConfigurationDialog = false;
        else if (oldContext.WorkflowState.StateId == CHOOSE_AUDIO_STREAM_DIALOG_STATE_ID)
          _inChooseAudioStreamDialog = false;
      }
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
    }

    public void ReActivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
    }

    public void UpdateMenuActions(NavigationContext context, ICollection<WorkflowAction> actions)
    {
      // Nothing to do here
    }

    #endregion
  }
}