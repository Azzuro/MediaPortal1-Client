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
using System.Data.SqlClient;
using MediaPortal.Database;
using MediaPortal.Database.Provider;

namespace Database.Sql
{
  public class SqlDatabaseConnection : IDatabaseConnection, IDisposable
  {
    private SqlConnection _connection;

    #region IDatabaseConnection Members

    /// <summary>
    /// Opens the database
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public void Open(string connectionString)
    {
      _connection = new SqlConnection(connectionString);
      _connection.Open();
    }

    /// <summary>
    /// Opens the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="systemDatabase">if set to <c>true</c> [system database].</param>
    public void Open(string connectionString, bool systemDatabase)
    {
      if (systemDatabase)
      {
        int pos = connectionString.IndexOf("Initial Catalog=");
        pos += "Initial Catalog=".Length;
        int pos2 = connectionString.IndexOf(";", pos);
        connectionString = connectionString.Substring(0, pos) + "master" + connectionString.Substring(pos2);
      }
      Open(connectionString);
    }

    /// <summary>
    /// Closes the database.
    /// </summary>
    public void Close()
    {
      if (_connection != null)
      {
        _connection.Dispose();
        _connection = null;
      }
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Dispose();
        _connection = null;
      }
    }

    /// <summary>
    /// Gets the underlying connection.
    /// </summary>
    /// <value>The underlying connection.</value>
    public object UnderlyingConnection
    {
      get { return _connection; }
    }

    #endregion
  }
}
