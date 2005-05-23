using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;


namespace MediaPortal.Database
{
	/// <summary>
	/// Summary description for DatabaseUtility.
	/// </summary>
	public class DatabaseUtility
	{
    

		/// <summary>
		/// Check if a table column exists
		/// </summary>
		/// <param name="table">table name</param>
		/// <param name="column">column name</param>
		/// <returns>true if table + column exists
		/// false if table does not exists or if table doesnt contain the specified column</returns>
		static public bool TableColumnExists(SQLiteClient m_db, string table, string column)
		{
			SQLiteResultSet results;
			if (m_db==null) return false;
			if (table==null) return false;
			if (table.Length==0) return false;
			results = m_db.Execute("SELECT * FROM '"+table+"'");
			if (results!=null)
			{
				for (int i=0; i < results.ColumnNames.Count;++i)
				{
					if ( (string)results.ColumnNames[i] == column) 
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check if a table exists
		/// </summary>
		/// <param name="table">name of table</param>
		/// <returns>true: table exists
		/// false: table does not exist</returns>
		static public bool TableExists(SQLiteClient m_db, string table)
		{
			SQLiteResultSet results;
			if (m_db==null) return false;
			if (table==null) return false;
			if (table.Length==0) return false;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+table+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				if (results.Rows.Count==1) 
				{
					ArrayList arr = (ArrayList)results.Rows[0];
					if (arr.Count==1)
					{
						if ( (string)arr[0] == table) 
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Helper function to create a new table in the database
		/// </summary>
		/// <param name="strTable">name of table</param>
		/// <param name="strSQL">SQL command to create the new table</param>
		/// <returns>true if table is created</returns>
		static public bool AddTable(SQLiteClient m_db,  string strTable, string strSQL)
		{
			lock (typeof(DatabaseUtility))
			{
				if (m_db==null) 
				{
					Log.Write("AddTable: database not opened");
					return false;
				}
				if (strSQL==null) 
				{
					Log.Write("AddTable: no sql?");
					return false;
				}
				if (strTable==null) 
				{
					Log.Write("AddTable: No table?");
					return false;
				}
				if (strTable.Length==0) 
				{
					Log.Write("AddTable: empty table?");
					return false;
				}
				if (strSQL.Length==0) 
				{
					Log.Write("AddTable: empty sql?");
					return false;
				}

				//Log.Write("check for  table:{0}", strTable);
				SQLiteResultSet results;
				results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
				if (results!=null)
				{
					//Log.Write("  results:{0}", results.Rows.Count);
					if (results.Rows.Count==1) 
					{
						//Log.Write(" check result:0");
						ArrayList arr = (ArrayList)results.Rows[0];
						if (arr.Count==1)
						{

							if ( (string)arr[0] == strTable) 
							{
								//Log.Write(" table exists");
								return false;
							}
							//Log.Write(" table has different name:{0}", (string)arr[0]);
						}
						//else Log.Write(" array contains:{0} items?", arr.Count);
					}
				}

				try 
				{
					//Log.Write("create table:{0}", strSQL);
					m_db.Execute(strSQL);
					//Log.Write("table created");
				}
				catch (SQLiteException ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return true;
			}
		}

		static public string Get(SQLiteResultSet results, int iRecord, string strColum)
		{
			if (null == results) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
			ArrayList arr = (ArrayList)results.Rows[iRecord];
			int iCol = 0;
			foreach (string columnName in results.ColumnNames)
			{
				if (strColum == columnName)
				{
					if(arr[iCol]==null)
						continue;
					string strLine = ((string)arr[iCol]).Trim();
					strLine = strLine.Replace("''","'");
					return strLine;
				}
				iCol++;
			}
			return String.Empty;
		}

		static public string Get(SQLiteResultSet results, int iRecord, int column)
		{
			if (null == results) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
			if (column<0 || column>=results.ColumnNames.Count ) return String.Empty;
			ArrayList arr = (ArrayList)results.Rows[iRecord];
			if (arr[column]==null) return String.Empty;
			string strLine = ((string)arr[column]).Trim();
			strLine = strLine.Replace("''","'");
			return strLine;;
		}

		static public void RemoveInvalidChars(ref string strTxt)
		{
			if (strTxt==null) 
			{
				strTxt="unknown";
				return;
			}
			if (strTxt.Length==0) 
			{
				strTxt="unknown";
				return;
			}
			string strReturn = String.Empty;
			for (int i = 0; i < (int)strTxt.Length; ++i)
			{
				char k = strTxt[i];
				if (k == '\'') 
				{
					strReturn += "'";
				}
				if((byte)k==0)// remove 0-bytes from the string
					k=(char)32;

				strReturn += k;
			}
			strReturn=strReturn.Trim();
			if (strReturn == String.Empty) 
				strReturn = "unknown";
			strTxt = strReturn;
		}

		static public void Split(string strFileNameAndPath, out string strPath, out string strFileName)
		{
			strFileNameAndPath = strFileNameAndPath.Trim();
			strFileName = "";
			strPath = "";
			if (strFileNameAndPath.Length == 0) return;
			int i = strFileNameAndPath.Length - 1;
			while (i > 0)
			{
				char ch = strFileNameAndPath[i];
				if (ch == ':' || ch == '/' || ch == '\\') break;
				else i--;
			}
			strPath = strFileNameAndPath.Substring(0, i).Trim();
			strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
		}

	}
}
