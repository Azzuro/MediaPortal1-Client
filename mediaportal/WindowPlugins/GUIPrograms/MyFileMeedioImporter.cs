using System;
using System.IO;
using SQLite.NET;

using MediaPortal.GUI.Library;		
using WindowPlugins.GUIPrograms;
using Programs.Utils;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for MyFileMeedioImporter.
	/// </summary>
	public class MyFileMeedioImporter
	{
		private AppItem m_App = null;
		private SQLiteClient m_db = null;

		// event: read new file
		public delegate void MlfEventHandler (string strLine);
		public event MlfEventHandler OnReadNewFile = null;
		
		
		private void DBImportMeedioItem(SQLiteResultSet results,int iRecord)
		{
			// MP           MEEDIO
			// ------------------------
			//title		    item_name
			//filename	    item_location
			//imagefile	    item_image
			//genre		    tag_1, tag_2, tag_3, tag_4, tag_5
			//country       tag_6
			//manufacturer	tag_7
			//year		    tag_8
			//rating		tag_9
			//overview	    tag_10
			//system		tag_11
			//external_id	tag_12
			FileItem curFile = new FileItem(m_db);
			string strFilename = ProgramUtils.Get(results,iRecord,"item_location").Trim();
			string strImagefile = ProgramUtils.Get(results,iRecord,"item_image").Trim();
			// check if item is complete for importing
			bool bOk = (strFilename != ""); // 1) filename must not be empty
			if (bOk && m_App.ImportValidImagesOnly)
			{
				// if "only import valid images" is activated, do some more checks
				bOk = (strImagefile != "");  
				if (bOk )
				{
					bOk = (System.IO.File.Exists(strImagefile));
				}
			}
			if (bOk)
			{
				curFile.FileID = -1; // to force an INSERT statement when writing the item
				curFile.AppID = m_App.AppID;
				curFile.Title = ProgramUtils.Get(results,iRecord,"item_name");
				curFile.Filename = ProgramUtils.Get(results,iRecord,"item_location");
				curFile.Imagefile = ProgramUtils.Get(results,iRecord,"item_image");
				string strGenre1 = ProgramUtils.Get(results,iRecord,"tag_1");
				string strGenre2 = ProgramUtils.Get(results,iRecord,"tag_2");
				string strGenre3 = ProgramUtils.Get(results,iRecord,"tag_3");
				string strGenre4 = ProgramUtils.Get(results,iRecord,"tag_4");
				string strGenre5 = ProgramUtils.Get(results,iRecord,"tag_5");
				curFile.Genre = String.Format("{0} {1} {2} {3} {4}", strGenre1, strGenre2, strGenre3, strGenre4, strGenre5);
				curFile.Country = ProgramUtils.Get(results,iRecord,"tag_6");
				curFile.Manufacturer = ProgramUtils.Get(results,iRecord,"tag_7");
				curFile.Year = ProgramUtils.GetIntDef(results, iRecord, "tag_8", -1);
				curFile.Rating = ProgramUtils.GetIntDef(results, iRecord, "tag_9", 5);
				curFile.Overview = ProgramUtils.Get(results,iRecord,"tag_10");
				curFile.System_ = ProgramUtils.Get(results,iRecord,"tag_11");
				curFile.ExtFileID = ProgramUtils.GetIntDef(results,iRecord, "tag_12", -1);
				// not imported properties => set default values
				curFile.ManualFilename = "";
				curFile.LastTimeLaunched = DateTime.MinValue;
				curFile.LaunchCount = 0;
				curFile.Write();
				this.OnReadNewFile(curFile.Title); // send event to whom it may concern....
			}
			return;
		}



		public void Start()
		{
			try
			{
				SQLiteClient dbMyFile = new SQLiteClient(m_App.Source);
				SQLiteResultSet resMyFile;

				resMyFile=dbMyFile.Execute("select item_name, item_location, item_image, tag_1 || tag_2 || tag_3 || tag_4 || tag_5, tag_7, tag_8, tag_9, tag_10, tag_11, tag_12 from items order by item_name");
				if (resMyFile.Rows.Count == 0)  return;
				for (int iRow=0; iRow < resMyFile.Rows.Count;iRow++)
				{
					DBImportMeedioItem(resMyFile,iRow);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}

		}

		public MyFileMeedioImporter(AppItem objApp, SQLiteClient objDB)
		{
			m_App = objApp;
			m_db = objDB;
		}
	}
}
