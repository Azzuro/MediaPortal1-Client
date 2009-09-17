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
using System.Data;
using System.Text;
using MediaPortal.Core;
using MediaPortal.Core.General;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Core.MediaManagement.DefaultItemAspects;
using MediaPortal.Core.MediaManagement.MLQueries;
using MediaPortal.Database;
using MediaPortal.Exceptions;
using MediaPortal.MediaLibrary;
using MediaPortal.Services.Database;
using MediaPortal.Utilities;
using MediaPortal.Utilities.Exceptions;
using MediaPortal.Utilities.FileSystem;

namespace MediaPortal.Services.MediaLibrary
{
  // TODO: Preparation of some SQL statements? We could use a lazy initialized DBCommand cache which prepares DBCommands
  // on the fly and holds up to N prepared commands.
  public class MediaLibrary : IMediaLibrary, IDisposable
  {
    internal const string MIAM_MEDIA_ITEM_ID_COL_NAME = "MEDIA_ITEM_ID";

    public void Dispose()
    {
    }

    protected static bool GetMediaItemAspectMetadata(Guid aspectId, out string name, out string serialization)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        int nameIndex;
        int serializationIndex;
        IDbCommand command = MediaLibrary_SubSchema.SelectMediaItemAspectMetadataByIdCommand(transaction, aspectId,
            out nameIndex, out serializationIndex);
        IDataReader reader = command.ExecuteReader();
        try
        {
          if (reader.Read())
          {
            name = reader.GetString(nameIndex);
            serialization = reader.GetString(serializationIndex);
            return true;
          }
          name = null;
          serialization = null;
          return false;
        }
        finally
        {
          reader.Close();
        }
      }
      finally
      {
        transaction.Dispose();
      }
    }

    protected static IDictionary<Guid, string> GetAllMediaItemAspectMetadata()
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        int miamIdIndex;
        int serializationsIndex;
        IDbCommand command = MediaLibrary_SubSchema.SelectAllMediaItemAspectMetadataCommand(transaction, out miamIdIndex, out serializationsIndex);
        IDataReader reader = command.ExecuteReader();
        try
        {
          IDictionary<Guid, string> result = new Dictionary<Guid, string>();
          while (reader.Read())
            result.Add(new Guid(reader.GetString(miamIdIndex)), reader.GetString(serializationsIndex));
          return result;
        }
        finally
        {
          reader.Close();
        }
      }
      finally
      {
        transaction.Dispose();
      }
    }

    protected static string GetMIAMTableName(MediaItemAspectMetadata miam)
    {
      return "MIAM_" + SqlUtils.ToSQLIdentifier(miam.AspectId.ToString());
    }

    protected static string GetMIAMAttributeColumnName(string attributeName)
    {
      return SqlUtils.ToSQLIdentifier(attributeName);
    }

    protected static string GetMIAMCollectionAttributeTableName(MediaItemAspectMetadata miam,
        MediaItemAspectMetadata.AttributeSpecification spec)
    {
      return GetMIAMTableName(miam) + "_" + SqlUtils.ToSQLIdentifier(spec.AttributeName);
    }

    public static int DeleteAllMediaItemsUnderPath(ITransaction transaction,
        SystemName nativeSystem, Guid mediaProviderId, string path)
    {
      MediaItemAspectMetadata providerAspectMetadata = ProviderResourceAspect.Metadata;
      string providerAspectTableName = GetMIAMTableName(providerAspectMetadata);
      string providerAspectSourceComputerColName = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_SOURCE_COMPUTER.AttributeName);
      string providerAspectProviderIdColName = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_PROVIDER_ID.AttributeName);
      string providerAspectPathColName = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_PATH.AttributeName);
      IDbCommand command = transaction.CreateCommand();
      command.CommandText = "DELETE FROM " + MediaLibrary_SubSchema.MEDIA_ITEMS_TABLE_NAME +
          " WHERE " + MediaLibrary_SubSchema.MEDIA_ITEMS_ITEM_ID_COL_NAME + " IN (" +
              // TODO: Replace this inner select statement by a select statement generated from an appropriate item query
              "SELECT " + MIAM_MEDIA_ITEM_ID_COL_NAME + " FROM " + providerAspectTableName +
              " WHERE " + providerAspectSourceComputerColName + " = ? AND " +
              providerAspectProviderIdColName + " = ? AND " +
              providerAspectPathColName + " LIKE ? ESCAPE '\\'";

      IDbDataParameter param = command.CreateParameter();
      param.Value = nativeSystem.HostName;
      command.Parameters.Add(param);

      param = command.CreateParameter();
      param.Value = mediaProviderId.ToString();
      command.Parameters.Add(param);

      param = command.CreateParameter();
      param.Value = SqlUtils.LikeEscape(StringUtils.CheckSuffix(path, "/"), '\\') + "%";
      command.Parameters.Add(param);

      return command.ExecuteNonQuery();
    }

    protected static int RelocateMediaItems(ITransaction transaction,
        SystemName originalNativeSystem, Guid originalProviderId, string originalBasePath,
        SystemName newNativeSystem, Guid newProviderId, string newBasePath)
    {
      string providerAspectTable = GetMIAMTableName(ProviderResourceAspect.Metadata);
      string sourceComputerAttribute = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_SOURCE_COMPUTER.AttributeName);
      string providerIdAttribute = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_PROVIDER_ID.AttributeName);
      string pathAttribute = GetMIAMAttributeColumnName(ProviderResourceAspect.ATTR_PATH.AttributeName);

      originalBasePath = FileUtils.RemoveTrailingPathDelimiter(originalBasePath) + "/";
      newBasePath = FileUtils.RemoveTrailingPathDelimiter(newBasePath) + "/";

      IList<string> setTerms = new List<string>();
      IList<object> parameters = new List<object>();
      if (originalNativeSystem != newNativeSystem)
      {
        setTerms.Add(sourceComputerAttribute + " = ?");
        parameters.Add(newNativeSystem);
      }
      if (originalProviderId != newProviderId)
      {
        setTerms.Add(providerIdAttribute + " = ?");
        parameters.Add(newProviderId.ToString());
      }
      if (originalBasePath != newBasePath)
      {
        setTerms.Add(pathAttribute + " = ? || SUBSTRING(" + pathAttribute + " FROM CHAR_LENGTH(?) + 1)");
        parameters.Add(newBasePath);
        parameters.Add(originalBasePath);
      }

      IDbCommand command = transaction.CreateCommand();
      command.CommandText = "UPDATE " + providerAspectTable + " SET " + StringUtils.Join(", ", setTerms) +
          " WHERE " + sourceComputerAttribute + " = ? AND " + providerIdAttribute + " = ? AND " + 
          "SUBSTRING(" + pathAttribute + " FROM 1 FOR CHAR_LENGTH(?)) = ?";
      parameters.Add(originalNativeSystem.HostName);
      parameters.Add(originalProviderId.ToString());
      parameters.Add(originalBasePath);
      parameters.Add(originalBasePath);

      foreach (object paramValue in parameters)
      {
        IDbDataParameter param = command.CreateParameter();
        param.Value = paramValue;
        command.Parameters.Add(param);
      }

      return command.ExecuteNonQuery();
    }

    protected ICollection<string> GetShareMediaCategories(ITransaction transaction, Guid shareId)
    {
      int mediaCategoryIndex;
      IDbCommand command = MediaLibrary_SubSchema.SelectShareCategoriesCommand(transaction, shareId, out mediaCategoryIndex);

      ICollection<string> result = new List<string>();
      IDataReader reader = command.ExecuteReader();
      try
      {
        while (reader.Read())
          result.Add(reader.GetString(mediaCategoryIndex));
      }
      finally
      {
        reader.Close();
      }
      return result;
    }

    protected void AddMediaCategoryToShare(ITransaction transaction, Guid shareId, string mediaCategory)
    {
      IDbCommand command = MediaLibrary_SubSchema.InsertShareCategoryCommand(transaction, shareId, mediaCategory);
      command.ExecuteNonQuery();
    }

    protected void RemoveMediaCategoryFromShare(ITransaction transaction, Guid shareId, string mediaCategory)
    {
      IDbCommand command = MediaLibrary_SubSchema.DeleteShareCategoryCommand(transaction, shareId, mediaCategory);
      command.ExecuteNonQuery();
    }

    protected ICollection<Guid> GetShareMetadataExtractors(ITransaction transaction, Guid shareId)
    {
      int metadataExtractorIndex;
      IDbCommand command = MediaLibrary_SubSchema.SelectShareMetadataExtractorsCommand(transaction, shareId, out metadataExtractorIndex);

      ICollection<Guid> result = new List<Guid>();
      IDataReader reader = command.ExecuteReader();
      try
      {
        while (reader.Read())
          result.Add(new Guid(reader.GetString(metadataExtractorIndex)));
      }
      finally
      {
        reader.Close();
      }
      return result;
    }

    protected void AddMetadataExtractorToShare(ITransaction transaction, Guid shareId, Guid metadataExtractorId)
    {
      IDbCommand command = MediaLibrary_SubSchema.InsertShareMetadataExtractorCommand(transaction, shareId, metadataExtractorId);
      command.ExecuteNonQuery();
    }

    protected void RemoveMetadataExtractorFromShare(ITransaction transaction, Guid shareId, Guid metadataExtractorId)
    {
      IDbCommand command = MediaLibrary_SubSchema.DeleteShareMetadataExtractorCommand(transaction, shareId, metadataExtractorId);
      command.ExecuteNonQuery();
    }

    #region IMediaLibrary implementation

    public void Startup()
    {
      DatabaseSubSchemaManager updater = new DatabaseSubSchemaManager(MediaLibrary_SubSchema.SUBSCHEMA_NAME);
      updater.AddDirectory(MediaLibrary_SubSchema.SubSchemaScriptDirectory);
      int curVersionMajor;
      int curVersionMinor;
      if (!updater.UpdateSubSchema(out curVersionMajor, out curVersionMinor) ||
          curVersionMajor != MediaLibrary_SubSchema.EXPECTED_SCHEMA_VERSION_MAJOR ||
          curVersionMinor != MediaLibrary_SubSchema.EXPECTED_SCHEMA_VERSION_MINOR)
        throw new IllegalCallException(string.Format(
            "Unable to update the MediaLibrary's subschema version to expected version {0}.{1}",
            MediaLibrary_SubSchema.EXPECTED_SCHEMA_VERSION_MAJOR, MediaLibrary_SubSchema.EXPECTED_SCHEMA_VERSION_MINOR));
    }

    public void Shutdown()
    {
    }

    public ICollection<MediaItem> Search(IQuery query)
    {
      TODO: When query language is ready
    }

    public ICollection<MediaItem> Browse(MediaItem parent)
    {
      TODO: When query language is ready
    }

    public bool MediaItemAspectStorageExists(Guid aspectId)
    {
      string name;
      string serialization;
      return GetMediaItemAspectMetadata(aspectId, out name, out serialization);
    }

    public MediaItemAspectMetadata GetMediaItemAspectMetadata(Guid aspectId)
    {
      string name;
      string serialization;
      if (!GetMediaItemAspectMetadata(aspectId, out name, out serialization))
        throw new InvalidDataException("The requested MediaItemAspectMetadata of id '{0}' is unknown", aspectId);
      return MediaItemAspectMetadata.Deserialize(serialization);
    }

    public void AddMediaItemAspectStorage(MediaItemAspectMetadata miam)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = transaction.CreateCommand();
        StringBuilder sb = new StringBuilder("CREATE TABLE " + GetMIAMTableName(miam) + " (" +
            MIAM_MEDIA_ITEM_ID_COL_NAME + " " + SqlUtils.GetSQLType(typeof(Int64)) + ",");
        IList<string> terms = new List<string>();
        foreach (MediaItemAspectMetadata.AttributeSpecification spec in miam.AttributeSpecifications)
        {
          string attrName = GetMIAMAttributeColumnName(spec.AttributeName);
          string sqlType = spec.AttributeType == typeof(string) ? SqlUtils.GetSQLStringType(spec.MaxNumChars) :
              SqlUtils.GetSQLType(spec.AttributeType);
          switch (spec.Cardinality)
          {
            case Cardinality.Inline:
              terms.Add(attrName + " " + sqlType);
              break;
            case Cardinality.OneToMany:
            case Cardinality.ManyToOne:
            case Cardinality.ManyToMany:
              command.CommandText = "CREATE TABLE " + GetMIAMCollectionAttributeTableName(miam, spec) + " (" +
                  MIAM_MEDIA_ITEM_ID_COL_NAME + " " + SqlUtils.GetSQLType(typeof(Int64)) +
                  "VALUE " + sqlType +
                  "CONSTRAINT " + GetMIAMCollectionAttributeTableName(miam, spec) + "_PK PRIMARY KEY (" + MIAM_MEDIA_ITEM_ID_COL_NAME + ")," +
                  "CONSTRAINT " + GetMIAMCollectionAttributeTableName(miam, spec) + "_MEDIA_ITEM_FK" +
                  " FOREIGN KEY (" + MIAM_MEDIA_ITEM_ID_COL_NAME + ")" +
                  " REFERENCES " + MediaLibrary_SubSchema.MEDIA_ITEMS_TABLE_NAME + " (" + MediaLibrary_SubSchema.MEDIA_ITEMS_ITEM_ID_COL_NAME + ") ON DELETE CASCADE" +
                  ")";
              command.ExecuteNonQuery();
              break;
            default:
              throw new NotImplementedException(string.Format("Cardinality '{0}' for attribute '{1}.{2}' is not implemented",
                  spec.Cardinality, miam.AspectId, spec.AttributeName));
          }
        }
        sb.Append(
            "CONSTRAINT " + GetMIAMTableName(miam) + "_PK PRIMARY KEY (" + MIAM_MEDIA_ITEM_ID_COL_NAME + ")," +
            "CONSTRAINT " + GetMIAMTableName(miam) + "_MEDIA_ITEMS_FK" +
            " FOREIGN KEY (" + MIAM_MEDIA_ITEM_ID_COL_NAME + ") REFERENCES " + MediaLibrary_SubSchema.MEDIA_ITEMS_TABLE_NAME + " (" + MediaLibrary_SubSchema.MEDIA_ITEMS_ITEM_ID_COL_NAME + ") ON DELETE CASCADE");
        sb.Append(StringUtils.Join(", ", terms));
        sb.Append(")");
        command.CommandText = sb.ToString();
        command.ExecuteNonQuery();

        // Register metadata
        command = MediaLibrary_SubSchema.CreateMediaItemAspectMetadataCommand(transaction, miam.AspectId, miam.Name, miam.Serialize());
        command.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public void RemoveMediaItemAspectStorage(Guid aspectId)
    {
      MediaItemAspectMetadata miam = GetMediaItemAspectMetadata(aspectId);
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = transaction.CreateCommand();
        command.CommandText = "DROP TABLE " + GetMIAMTableName(miam);
        command.ExecuteNonQuery();
        foreach (MediaItemAspectMetadata.AttributeSpecification spec in miam.AttributeSpecifications)
        {
          switch (spec.Cardinality)
          {
            case Cardinality.Inline:
              break;
            case Cardinality.OneToMany:
            case Cardinality.ManyToOne:
            case Cardinality.ManyToMany:
              command.CommandText = "DROP TABLE " + GetMIAMCollectionAttributeTableName(miam, spec);
              command.ExecuteNonQuery();
              break;
            default:
              throw new NotImplementedException(string.Format("Attribute '{0}.{1}': Cardinality '{2}' is not implemented",
                  aspectId, spec.AttributeName, spec.Cardinality));
          }
        }
        // Unregister metadata
        command = MediaLibrary_SubSchema.DeleteMediaItemAspectMetadataCommand(transaction, aspectId);
        command.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public ICollection<MediaItemAspectMetadata> GetManagedMediaItemAspectMetadata()
    {
      ICollection<string> miamSerializations = GetAllMediaItemAspectMetadata().Values;
      IList<MediaItemAspectMetadata> result = new List<MediaItemAspectMetadata>(miamSerializations.Count);
      foreach (string serialization in miamSerializations)
        result.Add(MediaItemAspectMetadata.Deserialize(serialization));
      return result;
    }

    public MediaItemAspectMetadata GetManagedMediaItemAspectMetadata(Guid aspectId)
    {
      string name;
      string serialization;
      if (GetMediaItemAspectMetadata(aspectId, out name, out serialization))
        return MediaItemAspectMetadata.Deserialize(serialization);
      else
        return null;
    }

    public Guid RegisterShare(SystemName nativeSystem, Guid providerId, string path, string shareName,
        IEnumerable<string> mediaCategories, IEnumerable<Guid> metadataExtractorIds)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        int shareIdIndex;
        IDbCommand command = MediaLibrary_SubSchema.SelectShareIdCommand(
            transaction, nativeSystem, providerId, path, out shareIdIndex);
        IDataReader reader = command.ExecuteReader();
        try
        {
          if (reader.Read())
            throw new ShareExistsException("A share with the given system '{0}', provider '{1}' and path '{2}' already exists",
                nativeSystem.HostName, providerId.ToString(), path);
        }
        finally
        {
          reader.Close();
        }
        Guid shareId = Guid.NewGuid();
        command = MediaLibrary_SubSchema.InsertShareCommand(transaction, shareId, nativeSystem, providerId, path, shareName, true);
        command.ExecuteNonQuery();

        foreach (string mediaCategory in mediaCategories)
          AddMediaCategoryToShare(transaction, shareId, mediaCategory);

        foreach (Guid metadataExtractorId in metadataExtractorIds)
          AddMetadataExtractorToShare(transaction, shareId, metadataExtractorId);

        transaction.Commit();
        return shareId;
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public void RemoveShare(Guid shareId)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = MediaLibrary_SubSchema.DeleteShareCommand(transaction, shareId);
        command.ExecuteNonQuery();

        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public int UpdateShare(Guid shareId, SystemName nativeSystem, Guid providerId, string path, string shareName,
        IEnumerable<string> mediaCategories, IEnumerable<Guid> metadataExtractorIds, RelocationMode relocationMode)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        Share originalShare = relocationMode == RelocationMode.Relocate ? GetShare(shareId) : null;

        IDbCommand command = MediaLibrary_SubSchema.UpdateShareCommand(transaction, shareId, nativeSystem, providerId, path, shareName);
        command.ExecuteNonQuery();

        // Update media categories
        ICollection<string> formerMediaCategories = GetShareMediaCategories(transaction, shareId);

        ICollection<string> newCategories = new List<string>();
        foreach (string mediaCategory in mediaCategories)
        {
          newCategories.Add(mediaCategory);
          if (!formerMediaCategories.Contains(mediaCategory))
            AddMediaCategoryToShare(transaction, shareId, mediaCategory);
        }

        foreach (string mediaCategory in formerMediaCategories)
          if (!newCategories.Contains(mediaCategory))
            RemoveMediaCategoryFromShare(transaction, shareId, mediaCategory);

        // Update metadata extractors
        ICollection<Guid> formerMetadataExtractors = GetShareMetadataExtractors(transaction, shareId);

        ICollection<Guid> newMetadataExtractorIds = new List<Guid>();
        foreach (Guid metadataExtractorId in metadataExtractorIds)
        {
          newMetadataExtractorIds.Add(metadataExtractorId);
          if (!formerMetadataExtractors.Contains(metadataExtractorId))
            AddMetadataExtractorToShare(transaction, shareId, metadataExtractorId);
        }

        foreach (Guid metadataExtractorId in formerMetadataExtractors)
          if (!newMetadataExtractorIds.Contains(metadataExtractorId))
            RemoveMetadataExtractorFromShare(transaction, shareId, metadataExtractorId);

        // Relocate media items
        int numAffected;
        switch (relocationMode)
        {
          case RelocationMode.Relocate:
            numAffected = RelocateMediaItems(transaction,
                originalShare.NativeSystem, originalShare.MediaProviderId, originalShare.Path,
                nativeSystem, providerId, path);
            break;
          case RelocationMode.Remove:
            numAffected = DeleteAllMediaItemsUnderPath(transaction, originalShare.NativeSystem, originalShare.MediaProviderId, originalShare.Path);
            break;
          default:
            throw new NotImplementedException(string.Format("RelocationMode {0} is not implemented", relocationMode));
        }
        transaction.Commit();
        return numAffected;
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public IDictionary<Guid, Share> GetShares(SystemName system, bool onlyConnectedShares)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        int shareIdIndex;
        int nativeSystemIndex;
        int providerIdIndex;
        int pathIndex;
        int shareNameIndex;
        int isOnlineIndex;
        IDbCommand command;
        if (system == null)
          command = MediaLibrary_SubSchema.SelectSharesCommand(transaction, out shareIdIndex,
              out nativeSystemIndex, out providerIdIndex, out pathIndex, out shareNameIndex, out isOnlineIndex);
        else
          command = MediaLibrary_SubSchema.SelectSharesByNativeSystemCommand(transaction, system, out shareIdIndex,
              out nativeSystemIndex, out providerIdIndex, out pathIndex, out shareNameIndex, out isOnlineIndex);
        IDataReader reader = command.ExecuteReader();
        IDictionary<Guid, Share> result = new Dictionary<Guid, Share>();
        try
        {
          while (reader.Read())
          {
            Guid shareId = new Guid(reader.GetString(shareIdIndex));
            ICollection<string> mediaCategories = GetShareMediaCategories(transaction, shareId);
            ICollection<Guid> metadataExtractors = GetShareMetadataExtractors(transaction, shareId);
            if (onlyConnectedShares && !reader.GetBoolean(isOnlineIndex))
              continue;
            result.Add(shareId, new Share(shareId, new SystemName(reader.GetString(nativeSystemIndex)),
                new Guid(reader.GetString(providerIdIndex)), reader.GetString(pathIndex), reader.GetString(shareNameIndex),
                mediaCategories, metadataExtractors));
          }
        }
        finally
        {
          reader.Close();
        }
        return result;
      }
      finally
      {
        transaction.Dispose();
      }
    }

    public Share GetShare(Guid shareId)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        int nativeSystemIndex;
        int providerIdIndex;
        int pathIndex;
        int shareNameIndex;
        IDbCommand command = MediaLibrary_SubSchema.SelectShareByIdCommand(transaction, shareId, out nativeSystemIndex,
            out providerIdIndex, out pathIndex, out shareNameIndex);
        IDataReader reader = command.ExecuteReader();
        try
        {
          if (!reader.Read())
            return null;
          ICollection<string> mediaCategories = GetShareMediaCategories(transaction, shareId);
          ICollection<Guid> metadataExtractors = GetShareMetadataExtractors(transaction, shareId);
          return new Share(shareId, new SystemName(reader.GetString(nativeSystemIndex)),
              new Guid(reader.GetString(providerIdIndex)), reader.GetString(pathIndex), reader.GetString(shareNameIndex),
              mediaCategories, metadataExtractors);
        }
        finally
        {
          reader.Close();
        }
      }
      finally
      {
        transaction.Dispose();
      }
    }

    public void ConnectShares(ICollection<Guid> shareIds)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = MediaLibrary_SubSchema.SetSharesConnectionStateCommand(transaction, shareIds, true);
        command.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public void DisconnectShares(ICollection<Guid> shareIds)
    {
      ISQLDatabase database = ServiceScope.Get<ISQLDatabase>();
      ITransaction transaction = database.BeginTransaction();
      try
      {
        IDbCommand command = MediaLibrary_SubSchema.SetSharesConnectionStateCommand(transaction, shareIds, false);
        command.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    #endregion
  }
}
