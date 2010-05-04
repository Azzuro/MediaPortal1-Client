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

using MediaPortal.Backend.ClientCommunication;
using MediaPortal.Backend.BackendServer;
using MediaPortal.Backend.Services.SystemResolver;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Backend.Database;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Backend.Services.Database;
using MediaPortal.Backend.Services.MediaManagement;
using MediaPortal.Core.SystemResolver;

namespace MediaPortal.Backend
{
  public class BackendExtension
  {
    public static void RegisterBackendServices()
    {
      ILogger logger = ServiceScope.Get<ILogger>();

      logger.Debug("BackendExtension: Registering ISystemResolver service");
      ServiceScope.Add<ISystemResolver>(new SystemResolver());

      logger.Debug("BackendExtension: Registering IDatabaseManager service");
      ServiceScope.Add<IDatabaseManager>(new DatabaseManager());

      logger.Debug("BackendExtension: Registering IMediaLibrary service");
      ServiceScope.Add<IMediaLibrary>(new Services.MediaLibrary.MediaLibrary());

      logger.Debug("BackendExtension: Registering IMediaItemAspectTypeRegistration service");
      ServiceScope.Add<IMediaItemAspectTypeRegistration>(new MediaItemAspectTypeRegistration());

      logger.Debug("BackendExtension: Registering IBackendServer service");
      ServiceScope.Add<IBackendServer>(new Services.BackendServer.BackendServer());

      logger.Debug("BackendExtension: Registering IClientManager service");
      ServiceScope.Add<IClientManager>(new Services.ClientCommunication.ClientManager());
    }

    /// <summary>
    /// To be called when the database service is present.
    /// </summary>
    public static void StartupBackendServices()
    {
      ServiceScope.Get<IDatabaseManager>().Startup();
      ServiceScope.Get<IMediaLibrary>().Startup();
      ServiceScope.Get<IClientManager>().Startup();
      ServiceScope.Get<IBackendServer>().Startup();
    }

    public static void ShutdownBackendServices()
    {
      ServiceScope.Get<IClientManager>().Shutdown();
      ServiceScope.Get<IMediaLibrary>().Shutdown();
      ServiceScope.Get<IBackendServer>().Shutdown();
    }

    public static void DisposeBackendServices()
    {
      ILogger logger = ServiceScope.Get<ILogger>();

      logger.Debug("BackendExtension: Removing IClientManager service");
      ServiceScope.RemoveAndDispose<IClientManager>();

      logger.Debug("BackendExtension: Removing IBackendServer service");
      ServiceScope.RemoveAndDispose<IBackendServer>();

      logger.Debug("BackendExtension: Removing IMediaItemAspectTypeRegistration service");
      ServiceScope.RemoveAndDispose<IMediaItemAspectTypeRegistration>();

      logger.Debug("BackendExtension: Removing IMediaLibrary service");
      ServiceScope.RemoveAndDispose<IMediaLibrary>();

      logger.Debug("BackendExtension: Removing IDatabaseManager service");
      ServiceScope.RemoveAndDispose<IDatabaseManager>();

      logger.Debug("BackendExtension: Removing ISystemResolver service");
      ServiceScope.RemoveAndDispose<ISystemResolver>();
    }
  }
}
