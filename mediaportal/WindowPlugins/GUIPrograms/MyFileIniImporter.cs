using System;
using System.IO;
using System.Collections;
using SQLite.NET;

using MediaPortal.GUI.Library;		
using WindowPlugins.GUIPrograms;
using Programs.Utils;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for MyFileIniImporter.
	/// </summary>
	public class MyFileIniImporter
	{
		private AppItem m_App = null;
		private SQLiteClient m_db = null;
		private FileItem curFile = null;
		private bool m_bOverviewReading = false;

		// event: read new file
		public delegate void MyEventHandler (string strLine);
		public event MyEventHandler OnReadNewFile = null;

		public void Start()
		{
			MyFileIniReader objReader = new MyFileIniReader(m_App.Source);
			objReader.OnReadNewSection += new MyFileIniReader.IniEventHandler(ReadNewSection);
			objReader.OnReadNewEntry += new MyFileIniReader.IniEventHandler(ReadNewEntry);
			objReader.OnReadAdditionalLine += new MyFileIniReader.IniEventHandler(ReadAddLine);
			objReader.OnReadEndSection += new MyFileIniReader.IniEventHandler(ReadEndOfSection);
			try
			{
				objReader.Start();
			}
			finally
			{
				objReader.OnReadEndSection -= new MyFileIniReader.IniEventHandler(ReadEndOfSection);
				objReader.OnReadAdditionalLine -= new MyFileIniReader.IniEventHandler(ReadAddLine);
				objReader.OnReadNewEntry -= new MyFileIniReader.IniEventHandler(ReadNewEntry);
				objReader.OnReadNewSection -= new MyFileIniReader.IniEventHandler(ReadNewSection);
			}
		}

		private void ReadNewSection(string strLine)
		{
			string strTemp;
			// Log.Write("MyFileINI: ReadNewSection: {0}", strLine);
			if (curFile == null)
			{
				curFile = new FileItem(this.m_db);
			}
			curFile.Clear();
			curFile.AppID = this.m_App.AppID;
			strTemp = strLine.Trim();
			strTemp = strTemp.TrimStart('[');
			strTemp = strTemp.TrimEnd(']');
			curFile.ExtFileID = ProgramUtils.StrToIntDef(strTemp, -1);
			// set properties that will not be imported to default values
			curFile.ManualFilename = "";
			curFile.LastTimeLaunched = DateTime.MinValue;
			curFile.LaunchCount = 0;
		}

		private void ReadNewEntry(string strLine)
		{
			strLine = strLine.Trim();
			string strLowerLine = strLine.ToLower();
			// some stupid check by check code....
			string strTemp = "";

			// constants: important: ONLY USE LOWERCASE CHARS!
			const string cTITLE = "title=";
			const string cCATEGORY = "category=";
			const string cSYSTEM = "category=\"system\"";
			const string cSYSTEMnoQ = "category=system";
			const string cGENRE = "category=\"genre\"";
			const string cGENREnoQ = "category=genre";
			const string cCOUNTRY = "category=\"country\"";
			const string cCOUNTRYnoQ = "category=country";
			const string cMANUFACTURER = "category=\"company\"";
			const string cMANUFACTURERnoQ = "category=company";
			const string cYEAR = "category=\"year\"";
			const string cYEARnoQ = "category=year";

			const string cRATING = "review=";
			const string cOVERVIEW = "overview=";
			//const string cFILENAME = "chain=\"game\"";
			const string cFILENAME = "chain=";
			const string cIMAGEFILE = "images=";
			if (strLowerLine.StartsWith(cTITLE))
			{
				strTemp = strLine.Remove(0, cTITLE.Length);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Title = strTemp;
			}
			else if (strLowerLine.StartsWith(cSYSTEM))
			{
				strTemp = strLine.Remove(0, cSYSTEM.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.System_ = strTemp;
			}
			else if (strLowerLine.StartsWith(cSYSTEMnoQ))
			{
				strTemp = strLine.Remove(0, cSYSTEMnoQ.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.System_ = strTemp;
			}
			else if (strLowerLine.StartsWith(cGENRE))
			{
				strTemp = strLine.Remove(0, cGENRE.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Genre = strTemp;
			}
			else if (strLowerLine.StartsWith(cGENREnoQ))
			{
				strTemp = strLine.Remove(0, cGENREnoQ.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Genre = strTemp;
			}
			else if (strLowerLine.StartsWith(cCOUNTRY))
			{
				strTemp = strLine.Remove(0, cCOUNTRY.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Country = strTemp;
			}
			else if (strLowerLine.StartsWith(cCOUNTRYnoQ))
			{
				strTemp = strLine.Remove(0, cCOUNTRYnoQ.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Country = strTemp;
			}
			else if (strLowerLine.StartsWith(cMANUFACTURER))
			{
				strTemp = strLine.Remove(0, cMANUFACTURER.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Manufacturer = strTemp;
			}
			else if (strLowerLine.StartsWith(cMANUFACTURERnoQ))
			{
				strTemp = strLine.Remove(0, cMANUFACTURER.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Manufacturer = strTemp;
			}
			else if (strLowerLine.StartsWith(cYEAR))
			{
				strTemp = strLine.Remove(0, cYEAR.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Year = ProgramUtils.StrToIntDef(strTemp, -1);
			}
			else if (strLowerLine.StartsWith(cYEARnoQ))
			{
				strTemp = strLine.Remove(0, cYEARnoQ.Length+1);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Year = ProgramUtils.StrToIntDef(strTemp, -1);
			}
			else if (strLowerLine.StartsWith(cRATING))
			{
				strTemp = strLine.Remove(0, cRATING.Length);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Rating = ProgramUtils.StrToIntDef(strTemp, 5);
			}
			else if (strLowerLine.StartsWith(cOVERVIEW))
			{
				strTemp = strLine.Remove(0, cOVERVIEW.Length);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Overview = strTemp;
				m_bOverviewReading = true;
			}
			else if (strLowerLine.StartsWith(cFILENAME))
			{
				m_bOverviewReading = false;
				string[] strParts = strLine.Split( ',' );
				if ((strParts.Length >= 2) && (curFile.Filename == ""))
				{
					// try to extract filename
					strTemp = strParts[1].Trim();
					if (strTemp.StartsWith("\"") && strTemp.EndsWith("\"") && (strTemp.Length >= 2))
					{
						// remove enclosing quotes, but only ONE char!
						strTemp = strTemp.Remove(0, 1); 
						strTemp = strTemp.Remove(strTemp.Length-1, 1);
					}
					if (strTemp != "")
					{
						curFile.Filename = strTemp;
					}

				}
			}
			else if (strLowerLine.StartsWith(cIMAGEFILE))
			{
				strTemp = strLine.Remove(0, cIMAGEFILE.Length);
				strTemp = strTemp.TrimStart('"');
				strTemp = strTemp.TrimEnd('"');
				curFile.Imagefile = strTemp;
			}
			else 
			{
				if (!strLowerLine.StartsWith(cCATEGORY))
				{
					// other line: add it to TagData
					// don't reformat 
					if (curFile.TagData != "")
					{
						curFile.TagData = curFile.TagData + "\r\n";
					}
					curFile.TagData = curFile.TagData + strLine;
				}
			}

			if (strLowerLine.StartsWith(cCATEGORY))
			{
				// remove "category=" part 
				string strName = "";
				string strValue = "";
				strTemp = strLine.Remove(0, cCATEGORY.Length);
				ArrayList mItems = new ArrayList( strTemp.Split( ',' ) );
				if (mItems.Count >= 2)
				{
					bool bNameComplete = false;
					bool bFirst = true;
					string strSep = "";
					foreach(string strItem in mItems)
					{
						if (bNameComplete)
						{
							strValue = strValue + strSep + strItem;
							strSep = ",";
						}
						else
						{
							strName = strName + strSep + strItem;
							bNameComplete = (strItem.EndsWith("\"") || ((bFirst)&&(!strItem.StartsWith("\""))));
							if (!bNameComplete)
							{
								strSep = ",";
							}
						}
						bFirst = false;
					}
					strName = strName.TrimStart('\"');
					strName = strName.TrimEnd('\"');
					string strEntry = String.Format("\"{0}\"={1}", strName, strValue);
					if (curFile.CategoryData != "")
					{
						curFile.CategoryData = curFile.CategoryData + "\r\n";
					}
					curFile.CategoryData = curFile.CategoryData + strEntry;
				}
			}

		}

		private void ReadAddLine(string strLine)
		{
			if (m_bOverviewReading) 
			{
				curFile.Overview = curFile.Overview + "\r\n" + strLine;
			}
									
		}

		private void ReadEndOfSection(string strDummy)
		{
			// check if item is complete for importing
			bool bOk = (curFile.Title != ""); // 1) title must not be empty
//			if (bOk)
//			{
//				bOk = (curFile.Filename != ""); // 2) Filename must not be empty
//			}
			if (bOk && m_App.ImportValidImagesOnly)
			{
				// if "only import valid images" is activated, do some more checks
				bOk = (curFile.Imagefile != "");  
				if (bOk )
				{
					bOk = (System.IO.File.Exists(curFile.Imagefile));
				}
			}
			if (bOk)
			{
				curFile.Write();
				this.OnReadNewFile(curFile.Title); // send event to whom it may concern....
			}
		}



		public MyFileIniImporter(AppItem objApp, SQLiteClient objDB)
		{
			m_App = objApp;
			m_db = objDB;
		}
	}
}
