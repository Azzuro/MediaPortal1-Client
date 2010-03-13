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
using MediaPortal.Core;
using MediaPortal.Core.Messaging;
using MediaPortal.Core.Localization;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Workflow;

namespace UiComponents.SkinBase.Actions
{
  public class FullscreenContentAction : IWorkflowContributor
  {
    #region Consts

    public const string FULLSCREEN_CONTENT_CONTRIBUTOR_MODEL_ID_STR = "08E19EDA-7BB3-4e74-8079-FFB0D52F3838";

    public static readonly Guid FULLSCREEN_CONTENT_CONTRIBUTOR_MODEL_ID = new Guid(FULLSCREEN_CONTENT_CONTRIBUTOR_MODEL_ID_STR);

    public const string FULLSCREEN_VIDEO_RESOURCE = "[Players.FullscreenVideo]";
    public const string AUDIO_VISUALIZATION_RESOURCE = "[Players.AudioVisualization]";

    #endregion

    #region Protected fields

    protected AsynchronousMessageQueue _messageQueue = null;
    protected bool _isVisible;
    protected IResourceString _displayTitle;

    #endregion

    void SubscribeToMessages()
    {
      _messageQueue = new AsynchronousMessageQueue(this, new string[]
        {
           PlayerManagerMessaging.CHANNEL
        });
      _messageQueue.MessageReceived += OnMessageReceived;
      _messageQueue.Start();
    }

    void UnsubscribeFromMessages()
    {
      if (_messageQueue == null)
        return;
      _messageQueue.Shutdown();
      _messageQueue = null;
    }

    void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == PlayerManagerMessaging.CHANNEL)
      {
        PlayerManagerMessaging.MessageType messageType =
            (PlayerManagerMessaging.MessageType) message.MessageType;
        switch (messageType)
        {
          case PlayerManagerMessaging.MessageType.PlayerStarted:
          case PlayerManagerMessaging.MessageType.PlayerStopped:
          case PlayerManagerMessaging.MessageType.PlayerEnded:
            Update();
            break;
        }
      }
    }

    protected void Update()
    {
      IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
      IPlayerContext pcPrimary = playerContextManager.GetPlayerContext(PlayerManagerConsts.PRIMARY_SLOT);
      IVideoPlayer vp = pcPrimary == null ? null : pcPrimary.CurrentPlayer as IVideoPlayer;
      IAudioPlayer ap = pcPrimary == null ? null : pcPrimary.CurrentPlayer as IAudioPlayer;
      _isVisible = vp != null || ap != null;
      if (vp == null)
        _displayTitle = ap == null ? null : LocalizationHelper.CreateStaticString(AUDIO_VISUALIZATION_RESOURCE);
      else
        _displayTitle = LocalizationHelper.CreateStaticString(
            LocalizationHelper.CreateResourceString(FULLSCREEN_VIDEO_RESOURCE).Evaluate(vp.Name));
      FireStateChanged();
    }

    protected void FireStateChanged()
    {
      ContributorStateChangeDelegate d = StateChanged;
      if (d != null) d();
    }

    #region IWorkflowContributor implementation

    public event ContributorStateChangeDelegate StateChanged;

    public bool IsActionVisible
    {
      get { return _isVisible; }
    }

    public bool IsActionEnabled
    {
      get { return true; }
    }

    public IResourceString DisplayTitle
    {
      get { return _displayTitle; }
    }

    public void Initialize()
    {
      SubscribeToMessages();
      Update();
    }

    public void Uninitialize()
    {
      UnsubscribeFromMessages();
    }

    public void Execute()
    {
      IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
      IPlayerContext pc = playerContextManager.GetPlayerContext(PlayerManagerConsts.PRIMARY_SLOT);
      if (pc == null)
        return;
      IWorkflowManager workflowManager = ServiceScope.Get<IWorkflowManager>();
      workflowManager.NavigatePush(pc.FullscreenContentWorkflowStateId, null);
    }

    #endregion
  }
}