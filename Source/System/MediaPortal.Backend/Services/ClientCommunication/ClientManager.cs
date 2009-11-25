#region Copyright (C) 2007-2009 Team MediaPortal

/*
    Copyright (C) 2007-2009 Team MediaPortal
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
using System.Data;
using MediaPortal.Backend.BackendServer;
using MediaPortal.Backend.ClientCommunication;
using MediaPortal.Backend.ClientCommunication.Settings;
using MediaPortal.Backend.Database;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Core;
using MediaPortal.Core.General;
using MediaPortal.Core.Logging;
using MediaPortal.Core.Settings;
using MediaPortal.Core.Threading;
using MediaPortal.Utilities.Exceptions;

namespace MediaPortal.Backend.Services.ClientCommunication
{
  public class ClientManager : IClientManager
  {
    protected UPnPServerControlPoint _controlPoint = null;
    protected object _syncObj = new object();

    public ClientManager()
    {
      ClientConnectionSettings settings = ServiceScope.Get<ISettingsManager>().Load<ClientConnectionSettings>();
      _controlPoint = new UPnPServerControlPoint(settings.AttachedClients);
      _controlPoint.ClientConnected += OnClientConnected;
      _controlPoint.ClientDisconnected += OnClientDisconnected;
    }

    void OnClientConnected(ClientDescriptor client)
    {
      SetClientConnectionState(client.MPFrontendServerUUID, client.System, true);
      ClientManagerMessaging.SendConnectionStateChangedMessage(ClientManagerMessaging.MessageType.ClientOnline, client);
      // This method is called as a result of our control point's attempt to connect to the (allegedly attached) client;
      // But maybe the client isn't attached any more to this server (it could have detached while the server wasn't online).
      // So we will validate if the client is still attached.
      ClientManagerMessaging.SendConnectionStateChangedMessage(
          ClientManagerMessaging.MessageType.ValidateAttachmentState, client);
      ServiceScope.Get<IThreadPool>().Add(() => ValidateAttachmentState(client));
    }

    void OnClientDisconnected(ClientDescriptor client)
    {
      SetClientConnectionState(client.MPFrontendServerUUID, client.System, false);
      ClientManagerMessaging.SendConnectionStateChangedMessage(ClientManagerMessaging.MessageType.ClientOffline, client);
    }

    protected void ValidateAttachmentState(ClientDescriptor client)
    {
      string clientSystemId = client.MPFrontendServerUUID;
      ClientConnection connection = _controlPoint.GetClientConnection(clientSystemId);
      if (connection != null)
      {
        string homeServer = connection.ClientControllerService.GetHomeServer();
        IBackendServer backendServer = ServiceScope.Get<IBackendServer>();
        if (homeServer != backendServer.BackendServerSystemId)
          DetachClientAndRemoveShares(clientSystemId);
      }
    }

    protected void SetClientConnectionState(string systemId, SystemName currentSytemName, bool isOnline)
    {
      IMediaLibrary mediaLibrary = ServiceScope.Get<IMediaLibrary>();
      if (isOnline)
        mediaLibrary.NotifySystemOnline(systemId, currentSytemName);
      else
        mediaLibrary.NotifySystemOffline(systemId);
    }

    #region IClientManager implementation

    public void Startup()
    {
      DatabaseSubSchemaManager updater = new DatabaseSubSchemaManager(ClientManager_SubSchema.SUBSCHEMA_NAME);
      updater.AddDirectory(ClientManager_SubSchema.SubSchemaScriptDirectory);
      int curVersionMajor;
      int curVersionMinor;
      if (!updater.UpdateSubSchema(out curVersionMajor, out curVersionMinor) ||
          curVersionMajor != ClientManager_SubSchema.EXPECTED_SCHEMA_VERSION_MAJOR ||
          curVersionMinor != ClientManager_SubSchema.EXPECTED_SCHEMA_VERSION_MINOR)
        throw new IllegalCallException(string.Format(
            "Unable to update the ClientManager's subschema version to expected version {0}.{1}",
            ClientManager_SubSchema.EXPECTED_SCHEMA_VERSION_MAJOR, ClientManager_SubSchema.EXPECTED_SCHEMA_VERSION_MINOR));
      _controlPoint.Start();
    }

    public void Shutdown()
    {
      _controlPoint.Stop();
    }

    public ICollection<ClientConnection> ConnectedClients
    {
      get { return _controlPoint.ClientConnections.Values; }
    }

    public ICollection<string> AttachedClientsSystemIds
    {
      get { return _controlPoint.AttachedClientSystemIds; }
    }

    public void AttachClient(string clientSystemId)
    {
      ISettingsManager settingsManager = ServiceScope.Get<ISettingsManager>();
      ClientConnectionSettings settings = settingsManager.Load<ClientConnectionSettings>();
      settings.AttachedClients.Add(clientSystemId);
      settingsManager.Save(settings);

      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = ClientManager_SubSchema.InsertAttachedClientCommand(transaction, clientSystemId);
        command.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception e)
      {
        ServiceScope.Get<ILogger>().Error("ClientManager: Error attaching client '{0}'", e, clientSystemId);
        transaction.Rollback();
        throw;
      }
      // Establish the UPnP connection to the client, if available in the network
      _controlPoint.AddAttachedClient(clientSystemId);
    }

    public void DetachClientAndRemoveShares(string clientSystemId)
    {
      ISettingsManager settingsManager = ServiceScope.Get<ISettingsManager>();
      ClientConnectionSettings settings = settingsManager.Load<ClientConnectionSettings>();
      settings.AttachedClients.Remove(clientSystemId);
      settingsManager.Save(settings);

      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = ClientManager_SubSchema.DeleteAttachedClientCommand(transaction, clientSystemId);
        command.ExecuteNonQuery();

        IMediaLibrary mediaLibrary = ServiceScope.Get<IMediaLibrary>();
        mediaLibrary.DeleteMediaItemOrPath(clientSystemId, null);
        mediaLibrary.RemoveSharesOfSystem(clientSystemId);

        transaction.Commit();
      }
      catch (Exception e)
      {
        ServiceScope.Get<ILogger>().Error("ClientManager: Error detaching client '{0}'", e, clientSystemId);
        transaction.Rollback();
        throw;
      }
      // Last action: Remove the client from the collection of attached clients and disconnect the client connection, if connected
      _controlPoint.RemoveAttachedClient(clientSystemId);
    }

    #endregion
  }
}
