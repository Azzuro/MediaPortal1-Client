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

using MediaPortal.Core.SystemResolver;
using MediaPortal.UI.Builders;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Core.MediaManagement;
using MediaPortal.UI.FrontendServer;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.ServerCommunication;
using MediaPortal.UI.Services.SystemResolver;
using MediaPortal.UI.Shares;
using MediaPortal.UI.Thumbnails;
using MediaPortal.UI.UserManagement;
using MediaPortal.UI.Services.Players;
using MediaPortal.UI.Services.ServerCommunication;
using MediaPortal.UI.Services.Shares;
using MediaPortal.UI.Services.ThumbnailGenerator;
using MediaPortal.UI.Services.UserManagement;
using MediaPortal.UI.Services.Workflow;
using MediaPortal.UI.Services.MediaManagement;

namespace MediaPortal.UI
{
  public class UiExtension
  {
    public static void RegisterUiServices()
    {
      ILogger logger = ServiceScope.Get<ILogger>();

      logger.Debug("UiExtension: Registering ISystemResolver service");
      ServiceScope.Add<ISystemResolver>(new SystemResolver());

      logger.Debug("UiExtension: Registering IWorkflowManager service");
      ServiceScope.Add<IWorkflowManager>(new WorkflowManager());

      logger.Debug("UiExtension: Registering IPlayerManager service");
      ServiceScope.Add<IPlayerManager>(new PlayerManager());

      logger.Debug("UiExtension: Registering IPlayerContextManager service");
      ServiceScope.Add<IPlayerContextManager>(new PlayerContextManager());

      logger.Debug("UiExtension: Registering IUserService service");
      ServiceScope.Add<IUserService>(new UserService());

      logger.Debug("UiExtension: Registering IAsyncThumbnailGenerator service");
      ServiceScope.Add<IAsyncThumbnailGenerator>(new ThumbnailGenerator());

      logger.Debug("UiExtension: Registering ILocalSharesManagement service");
      ServiceScope.Add<ILocalSharesManagement>(new LocalSharesManagement());

      logger.Debug("UiExtension: Registering IServerConnectionManager service");
      ServiceScope.Add<IServerConnectionManager>(new ServerConnectionManager());

      logger.Debug("UiExtension: Registering IMediaItemAspectTypeRegistration service");
      ServiceScope.Add<IMediaItemAspectTypeRegistration>(new MediaItemAspectTypeRegistration());

      logger.Debug("UiExtension: Registering IFrontendServer service");
      ServiceScope.Add<IFrontendServer>(new Services.FrontendServer.FrontendServer());

      AdditionalUiBuilders.Register();
    }

    public static void StopAll()
    {
      IServerConnectionManager serverConnectionManager = ServiceScope.Get<IServerConnectionManager>();
      serverConnectionManager.Shutdown();

      IPlayerContextManager playerContextManager = ServiceScope.Get<IPlayerContextManager>();
      playerContextManager.Shutdown();

      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>();
      playerManager.CloseAllSlots();
    }

    public static void DisposeUiServices()
    {
      ILogger logger = ServiceScope.Get<ILogger>();

      // Reverse order than method RegisterUiServices()

      logger.Debug("UiExtension: Removing IFrontendServer service");
      ServiceScope.RemoveAndDispose<IFrontendServer>();

      logger.Debug("UiExtension: Removing IMediaItemAspectTypeRegistration service");
      ServiceScope.RemoveAndDispose<IMediaItemAspectTypeRegistration>();

      logger.Debug("UiExtension: Removing IServerConnectionManager service");
      ServiceScope.RemoveAndDispose<IServerConnectionManager>();

      logger.Debug("UiExtension: Removing ILocalSharesManagement service");
      ServiceScope.RemoveAndDispose<ILocalSharesManagement>();

      logger.Debug("UiExtension: Removing IAsyncThumbnailGenerator service");
      ServiceScope.RemoveAndDispose<IAsyncThumbnailGenerator>();

      logger.Debug("UiExtension: Removing IUserService service");
      ServiceScope.RemoveAndDispose<IUserService>();

      logger.Debug("UiExtension: Removing IPlayerContextManager service");
      ServiceScope.RemoveAndDispose<IPlayerContextManager>();

      logger.Debug("UiExtension: Removing IPlayerManager service");
      ServiceScope.RemoveAndDispose<IPlayerManager>();

      logger.Debug("UiExtension: Removing IWorkflowManager service");
      ServiceScope.RemoveAndDispose<IWorkflowManager>();

      logger.Debug("UiExtension: Removing ISystemResolver service");
      ServiceScope.RemoveAndDispose<ISystemResolver>();
    }

    /// <summary>
    /// Registers default command shortcuts at the input manager.
    /// </summary>
    protected static void RegisterDefaultCommandShortcuts()
    {
      //TODO: Shortcut to handle the "Power" key, further shortcuts
    }

    public static void Startup()
    {
      RegisterDefaultCommandShortcuts();
      ServiceScope.Get<IServerConnectionManager>().Startup();
      ServiceScope.Get<IFrontendServer>().Startup();
    }
  }
}