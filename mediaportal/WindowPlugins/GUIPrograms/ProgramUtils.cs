using System;
using System.Collections;
using System.Diagnostics;
using MediaPortal.GUI.Library;
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace Programs.Utils
{
  public enum myProgSourceType
  {
    //Directory (Browse-Mode)
    //Directory (DBCache-Mode)
    //my-Files (myGames my-File output / kino.de scraper output, etc.)
    //MyGames Meedio output
    //Mame direct input
    //File-Editor based launcher
    //Item-Grouper
    //Gamebase 
    UNKNOWN = 0, DIRBROWSE = 1, DIRCACHE = 2, MYFILEINI = 3, MYFILEMEEDIO = 4, MAMEDIRECT = 5, FILELAUNCHER = 6, GROUPER = 7, GAMEBASE = 8
  };

  public enum myProgScraperType
  {
    UNKNOWN = 0, ALLGAME = 1 
    // VGMUSEUM / AMAZON etc. etc. etc. :)
  } 

  public enum ScraperSaveType
  {
    Data = 0, Images = 1, DataAndImages = 2
  } 


  /// <summary>
  /// Summary description for ProgramUtils.
  /// </summary>
  public class ProgramUtils
  {
    public const int GetID = (int)GUIWindow.Window.WINDOW_FILES;
    public const int ProgramInfoID = 1206; // some magic number, sync with DialogAppInfo.xml
    public const string cBackLabel = "..";

    public const string cMYFILEMEEDIO = "MY_FILE_MEEDIO";
    public const string cMYFILEINI = "MY_FILE_INI";
    public const string cMAMEDIRECT = "MAME_DIRECT";
    public const string cDIRBROWSE = "DIR_BROWSE";
    public const string cDIRCACHE = "DIR_CACHE";
    public const string cFILELAUNCHER = "FILELAUNCHER";
    public const string cGROUPER = "GROUPER";
    public const string cGAMEBASE = "GAMEBASE";

    public const string cMIGRATIONKEY = "V1_V2MIGRATION";
    public const string cPLUGINTITLE = "PLUGINTITLE";
    public const string cCONTENT_PATCH = "CONTENTPATCH";
    public const string cGENRE_PATCH = "GENREPATCH";

    // singleton. Dont allow any instance of this class
    private ProgramUtils(){}

    static ProgramUtils(){}

    static public string Encode(string strValue)
    {
      return strValue.Replace("'", "''");
    }

    static public string Get(SQLiteResultSet results, int iRecord, string strColumn)
    {
      if (null == results)
        return "";
      if (results.Rows.Count < iRecord)
        return "";
      ArrayList arr = (ArrayList)results.Rows[iRecord];
      int iCol = 0;
      foreach (string columnName in results.ColumnNames)
      {
        if (strColumn == columnName)
        {
          if (arr[iCol] == null)
            return "";
          return ((string)arr[iCol]).Trim();
        }
        iCol++;
      }
      return "";
    }

    static public bool StrToBoolean(string val)
    {
      return (val == "T");
    }

    static public string BooleanToStr(bool val)
    {
      if (val)
      {
        return "T";
      }
      else
      {
        return "F";
      }
    }

    static public int StrToIntDef(string strVal, int nDefValue)
    {
      int nResult = nDefValue;
      try
      {
        // nResult = Int32.Parse(strVal);
        // avoid round errors and accept DOUBLEs
        nResult = (int)Math.Floor(0.5d + Double.Parse(strVal));
      }
      catch (System.FormatException)
      {
        nResult = nDefValue;
      }
      return nResult;
    }

    static public int GetIntDef(SQLiteResultSet results, int iRecord, string strColumn, int nDefValue)
    // do a safe conversion.....
    {
      int nResult = nDefValue;
      string strValue = Get(results, iRecord, strColumn).Trim();
      if (strValue != "")
      {
        nResult = StrToIntDef(strValue, nDefValue);
      }
      return nResult;
    }


    static public DateTime StrToDateDef(string strVal, DateTime dteDefValue)
    {
      DateTime dteResult = dteDefValue;
      try
      {
        dteResult = DateTime.Parse(strVal);
      }
      catch (System.FormatException)
      {
        dteResult = dteDefValue;
      }
      return dteResult;
    }


    static public DateTime GetDateDef(SQLiteResultSet results, int iRecord, string strColumn, DateTime dteDefValue)
    {
      DateTime dteResult = dteDefValue;
      string strValue = Get(results, iRecord, strColumn);
      if (strValue != "")
      {
        dteResult = StrToDateDef(strValue, dteDefValue);
      }
      return dteResult;
    }


    static public bool GetBool(SQLiteResultSet results, int iRecord, string strColumn)
    {
      return (Get(results, iRecord, strColumn) == "T");
    }

    static public ProcessWindowStyle GetProcessWindowStyle(SQLiteResultSet results, int iRecord, string strColumn)
    {
      return (StringToWindowStyle(Get(results, iRecord, strColumn)));
    }

    static public myProgSourceType GetSourceType(SQLiteResultSet results, int iRecord, string strColumn)
    {
      return (StringToSourceType(Get(results, iRecord, strColumn)));
    }

    static public string WindowStyleToStr(ProcessWindowStyle val)
    {
      string res = "";
      switch (val)
      {
        case ProcessWindowStyle.Hidden:
          res = "hidden";
          break;
        case ProcessWindowStyle.Maximized:
          res = "maximized";
          break;
        case ProcessWindowStyle.Minimized:
          res = "minimized";
          break;
        case ProcessWindowStyle.Normal:
          res = "normal";
          break;
      }
      return res;
    }


    static public ProcessWindowStyle StringToWindowStyle(string strValue)
    {
      if (strValue.ToLower().Trim() == "hidden")
      {
        return ProcessWindowStyle.Hidden;
      }
      else if (strValue.ToLower().Trim() == "maximized")
      {
        return ProcessWindowStyle.Maximized;
      }
      else if (strValue.ToLower().Trim() == "minimized")
      {
        return ProcessWindowStyle.Minimized;
      }
      else
        return ProcessWindowStyle.Normal;
    }

    static public string SourceTypeToStr(myProgSourceType val)
    {
      string res = "";
      switch (val)
      {
        case myProgSourceType.MYFILEMEEDIO:
          res = cMYFILEMEEDIO;
          break;
        case myProgSourceType.MYFILEINI:
          res = cMYFILEINI;
          break;
        case myProgSourceType.MAMEDIRECT:
          res = cMAMEDIRECT;
          break;
        case myProgSourceType.DIRBROWSE:
          res = cDIRBROWSE;
          break;
        case myProgSourceType.DIRCACHE:
          res = cDIRCACHE;
          break;
        case myProgSourceType.FILELAUNCHER:
          res = cFILELAUNCHER;
          break;
        case myProgSourceType.GROUPER:
          res = cGROUPER;
          break;
        case myProgSourceType.GAMEBASE:
          res = cGAMEBASE;
          break;
      }
      return res;
    }


    static public myProgSourceType StringToSourceType(string strValue)
    {
      if (strValue == cMYFILEMEEDIO)
      {
        return myProgSourceType.MYFILEMEEDIO;
      }
      else if (strValue == cMYFILEINI)
      {
        return myProgSourceType.MYFILEINI;
      }
      else if (strValue == cMAMEDIRECT)
      {
        return myProgSourceType.MAMEDIRECT;
      }
      else if (strValue == cDIRBROWSE)
      {
        return myProgSourceType.DIRBROWSE;
      }
      else if (strValue == cDIRCACHE)
      {
        return myProgSourceType.DIRCACHE;
      }
      else if (strValue == cFILELAUNCHER)
      {
        return myProgSourceType.FILELAUNCHER;
      }
      else if (strValue == cGROUPER)
      {
        return myProgSourceType.GROUPER;
      }
      else if (strValue == cGAMEBASE)
      {
        return myProgSourceType.GAMEBASE;
      }
      else
        return myProgSourceType.UNKNOWN;
    }

    static public void RemoveInvalidChars(ref string strTxt)
    {
      string strReturn = "";
      for (int i = 0; i < strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'')
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn == "")
        strReturn = Strings.Unknown;
      strTxt = strReturn.Trim();
    }

    static public void AddBackButton(GUIFacadeControl facadeView)
    {
      // add BACK-Button
      GUIListItem gliBack = new GUIListItem(ProgramUtils.cBackLabel);
      gliBack.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBackBig.png";
      gliBack.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultFolderBack.png";
      gliBack.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBack.png";
      gliBack.IsFolder = true;
      gliBack.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(gliBack_OnItemSelected);
      facadeView.Add(gliBack);
    }

    static private void gliBack_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
        return ;
      filmstrip.InfoImageFileName = ""; // clear filmstrip image if back button is selected
    }


    static public string GetAvailableExtensions(string curDirectory)
    {
      string Result = "";
      string Checker = "#";
      string sep = "";
      if (System.IO.Directory.Exists(curDirectory))
      {
        string[] fileEntries = System.IO.Directory.GetFiles(curDirectory);
        foreach (string fileName in fileEntries)
        {
          string curExtension = System.IO.Path.GetExtension(fileName).ToLower();
          if (curExtension.Trim() != "")
          {
            if (Checker.IndexOf("#" + curExtension + "#") ==  - 1)
            {
              Result = Result + sep + curExtension;
              Checker = Checker + curExtension + "#";
              sep = ",";
            }
          }
        }
      }
      return Result;
    }

    static public string NormalizedString(string strVal)
    {
      string strRes = strVal;
      // trim away trailing [..] (..) codes
      int iPos = strRes.IndexOf("[");
      if (iPos > 0)
      {
        strRes = strRes.Substring(0, iPos - 1);
      }
      iPos = strRes.IndexOf("(");
      if (iPos > 0)
      {
        strRes = strRes.Substring(0, iPos - 1);
      }
      return strRes;
    }


  }
}
