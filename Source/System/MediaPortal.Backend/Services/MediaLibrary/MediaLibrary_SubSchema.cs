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
using MediaPortal.Core;
using MediaPortal.Core.General;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Core.PathManager;
using MediaPortal.Backend.Database;
using MediaPortal.Utilities;

namespace MediaPortal.Backend.Services.MediaLibrary
{
  /// <summary>
  /// Creates SQL commands for the communication with the MediaLibrary subschema.
  /// </summary>
  public class MediaLibrary_SubSchema
  {
    #region Consts

    public const string SUBSCHEMA_NAME = "MediaLibrary";

    public const int EXPECTED_SCHEMA_VERSION_MAJOR = 1;
    public const int EXPECTED_SCHEMA_VERSION_MINOR = 0;

    internal const string MEDIA_ITEMS_TABLE_NAME = "MEDIA_ITEMS";
    internal const string MEDIA_ITEMS_ITEM_ID_COL_NAME = "MEDIA_ITEM_ID";
    internal const string MEDIA_ITEM_ID_SEQUENCE_NAME = "MEDIA_ITEM_ID_GEN";

    #endregion

    public static string SubSchemaScriptDirectory
    {
      get
      {
        IPathManager pathManager = ServiceScope.Get<IPathManager>();
        return pathManager.GetPath(@"<APPLICATION_ROOT>\Scripts\");
      }
    }

    public static IDbCommand SelectAllMediaItemAspectMetadataCommand(ITransaction transaction,
        out int aspectIdIndex, out int serializationsIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT MIAM_ID, MIAM_SERIALIZATION FROM MIA_TYPES";

      aspectIdIndex = 0;
      serializationsIndex = 1;
      return result;
    }

    public static IDbCommand CreateMediaItemAspectMetadataCommand(ITransaction transaction, Guid id,
        string name, string serialization)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "INSERT INTO MIA_TYPES (MIAM_ID, NAME, MIAM_SERIALIZATION) VALUES (?, ?, ?)";

      IDbDataParameter param = result.CreateParameter();
      param.Value = id.ToString();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = name;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = serialization;
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand SelectMIANameAliasesCommand(ITransaction transaction,
        out int aspectIdIndex, out int identifierIndex, out int dbObjectNameIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT MIAM_ID, IDENTIFIER, DATABASE_OBJECT_NAME FROM MIA_NAME_ALIASES";

      aspectIdIndex = 0;
      identifierIndex = 1;
      dbObjectNameIndex = 2;
      return result;
    }

    public static IDbCommand CreateMIANameAliasCommand(ITransaction transaction, Guid aspectId,
        string identifier, string dbObjectName)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "INSERT INTO MIA_NAME_ALIASES (MIAM_ID, IDENTIFIER, DATABASE_OBJECT_NAME) VALUES (?, ?, ?)";

      IDbDataParameter param = result.CreateParameter();
      param.Value = aspectId.ToString();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = identifier;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = dbObjectName;
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand DeleteMediaItemAspectMetadataCommand(ITransaction transaction, Guid aspectId)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "DELETE FROM MIA_TYPES WHERE MIAM_ID=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = aspectId.ToString();
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand SelectShareIdCommand(ITransaction transaction,
        string systemId, ResourcePath baseResourcePath, out int shareIdIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT SHARE_ID FROM SHARES WHERE SYSTEM_ID=? AND BASE_RESOURCE_PATH=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = systemId;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = baseResourcePath.Serialize();
      result.Parameters.Add(param);

      shareIdIndex = 0;
      return result;
    }

    public static IDbCommand SelectSharesCommand(ITransaction transaction, out int shareIdIndex, out int systemIdIndex,
        out int baseResourcePathIndex, out int shareNameIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT SHARE_ID, SYSTEM_ID, BASE_RESOURCE_PATH, NAME FROM SHARES";

      shareIdIndex = 0;
      systemIdIndex = 1;
      baseResourcePathIndex = 2;
      shareNameIndex = 3;
      return result;
    }

    public static IDbCommand SelectShareByIdCommand(ITransaction transaction, Guid shareId, out int systemIdIndex,
        out int baseResourcePathIndex, out int shareNameIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT SYSTEM_ID, BASE_RESOURCE_PATH, NAME FROM SHARES WHERE SHARE_ID=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      systemIdIndex = 0;
      baseResourcePathIndex = 1;
      shareNameIndex = 2;
      return result;
    }

    public static IDbCommand SelectSharesBySystemCommand(ITransaction transaction, string systemId,
        out int shareIdIndex, out int systemIdIndex, out int baseResourcePathIndex, out int shareNameIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT SHARE_ID, SYSTEM_ID, BASE_RESOURCE_PATH, NAME FROM SHARES WHERE SYSTEM_ID=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = systemId;
      result.Parameters.Add(param);

      shareIdIndex = 0;
      systemIdIndex = 1;
      baseResourcePathIndex = 2;
      shareNameIndex = 3;
      return result;
    }

    public static IDbCommand InsertShareCommand(ITransaction transaction, Guid shareId, string systemId,
        ResourcePath baseResourcePath, string shareName)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "INSERT INTO SHARES (SHARE_ID, NAME, SYSTEM_ID, BASE_RESOURCE_PATH) VALUES (?, ?, ?, ?)";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = shareName;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = systemId;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = baseResourcePath.Serialize();
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand SelectShareCategoriesCommand(ITransaction transaction, Guid shareId, out int categoryIndex)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "SELECT CATEGORYNAME FROM SHARES_CATEGORIES WHERE SHARE_ID=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      categoryIndex = 0;
      return result;
    }

    public static IDbCommand InsertShareCategoryCommand(ITransaction transaction, Guid shareId, string mediaCategory)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "INSERT INTO SHARES_CATEGORIES (SHARE_ID, CATEGORYNAME) VALUES (?, ?)";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = mediaCategory;
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand DeleteShareCategoryCommand(ITransaction transaction, Guid shareId, string mediaCategory)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "DELETE FROM SHARES_CATEGORIES WHERE SHARE_ID=? AND CATEGORYNAME=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = mediaCategory;
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand UpdateShareCommand(ITransaction transaction, Guid shareId, ResourcePath baseResourcePath,
        string shareName)
    {
      IDbCommand result = transaction.CreateCommand();
      result.CommandText = "UPDATE SHARES set NAME=?, BASE_RESOURCE_PATH=? WHERE SHARE_ID=?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = shareName;
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = baseResourcePath.Serialize();
      result.Parameters.Add(param);

      param = result.CreateParameter();
      param.Value = shareId.ToString();
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand DeleteSharesCommand(ITransaction transaction, IEnumerable<Guid> shareIds)
    {
      IDbCommand result = transaction.CreateCommand();

      ICollection<string> placeholders = new List<string>();
      foreach (Guid shareId in shareIds)
      {
        IDbDataParameter param = result.CreateParameter();
        param.Value = shareId.ToString();
        result.Parameters.Add(param);

        placeholders.Add("?");
      }
      result.CommandText = "DELETE FROM SHARES WHERE SHARE_ID in (" + StringUtils.Join(",", placeholders) + ")";

      return result;
    }

    public static IDbCommand DeleteSharesOfSystemCommand(ITransaction transaction, string systemId)
    {
      IDbCommand result = transaction.CreateCommand();

      result.CommandText = "DELETE FROM SHARES WHERE SYSTEM_ID = ?";

      IDbDataParameter param = result.CreateParameter();
      param.Value = systemId;
      result.Parameters.Add(param);

      return result;
    }

    public static IDbCommand InsertMediaItemCommand(ISQLDatabase database, ITransaction transaction)
    {
      IDbCommand result = transaction.CreateCommand();

      result.CommandText = "INSERT INTO " + MEDIA_ITEMS_TABLE_NAME + " (" + MEDIA_ITEMS_ITEM_ID_COL_NAME + ") VALUES (" +
          database.GetSelectSequenceNextValStatement(MEDIA_ITEM_ID_SEQUENCE_NAME) + ")";

      return result;
    }

    public static IDbCommand GetLastGeneratedMediaItemIdCommand(ISQLDatabase database, ITransaction transaction)
    {
      IDbCommand result = transaction.CreateCommand();

      IDatabaseManager databaseManager = ServiceScope.Get<IDatabaseManager>();
      result.CommandText = "SELECT " + database.GetSelectSequenceCurrValStatement(MEDIA_ITEM_ID_SEQUENCE_NAME) + " FROM " + databaseManager.DummyTableName;

      return result;
    }
  }
}
