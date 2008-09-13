#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using CSScriptLibrary;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// supporting classes to fetch movie information out of different databases
  /// currently supported: IMDB http://us.imdb.com and additional database by using external csscripts
  /// </summary>
  public class IMDB : IEnumerable
  {
    public static string ScriptDirectory = Config.GetSubFolder(Config.Dir.Config, "scripts\\MovieInfo");
    public const int DEFAULT_SEARCH_LIMIT = 10;

    #region interfaces and classes

    public interface IProgress
    {
      void OnProgress(string line1, string line2, string line3, int percent);
      bool OnDisableCancel(IMDBFetcher fetcher);
      bool OnSearchStarting(IMDBFetcher fetcher);
      bool OnSearchStarted(IMDBFetcher fetcher);
      bool OnSearchEnd(IMDBFetcher fetcher);
      bool OnMovieNotFound(IMDBFetcher fetcher);
      bool OnDetailsStarting(IMDBFetcher fetcher);
      bool OnDetailsStarted(IMDBFetcher fetcher);
      bool OnDetailsEnd(IMDBFetcher fetcher);
      bool OnDetailsNotFound(IMDBFetcher fetcher);
      bool OnActorsStarting(IMDBFetcher fetcher);
      bool OnActorsStarted(IMDBFetcher fetcher);
      bool OnActorsEnd(IMDBFetcher fetcher);
      bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName);
      bool OnSelectMovie(IMDBFetcher fetcher, out int selected);
      bool OnScanStart(int total);
      bool OnScanEnd();
      bool OnScanIterating(int count);
      bool OnScanIterated(int count);
    }

    /// <summary>
    /// class that represents URL and Title of a search result
    /// </summary>
    public class IMDBUrl
    {
      string m_strURL = "";
      string m_strTitle = "";
      string m_strDatabase = "";
      string m_strIMDBURL = "";

      public IMDBUrl(string strURL, string strTitle, string strDB)
      {
        URL = strURL;
        Title = strTitle;
        Database = strDB;
      }

      public string URL
      {
        get { return m_strURL; }
        set { m_strURL = value; }
      }

      public string Title
      {
        get { return m_strTitle; }
        set { m_strTitle = value; }
      }

      public string Database
      {
        get { return m_strDatabase; }
        set { m_strDatabase = value; }
      }

      public string IMDBURL
      {
        get { return m_strIMDBURL; }
        set { m_strIMDBURL = value; }
      }
    }

    public class IMDBEnumerator : IEnumerator
    {
      private int position = -1;
      private IMDB t;

      public IMDBEnumerator(IMDB t)
      {
        this.t = t;
      }

      public bool MoveNext()
      {
        if (position < t.elements.Count - 1)
        {
          position++;
          return true;
        }
        else
        {
          return false;
        }
      }

      public void Reset()
      {
        position = -1;
      }

      public IMDB.IMDBUrl Current // non-IEnumerator version: type-safe
      {
        get
        {
          if (t.elements.Count == 0)
            return null;
          return (IMDB.IMDBUrl)t.elements[position];
        }
      }

      object IEnumerator.Current // IEnumerator version: returns object
      {
        get
        {
          if (t.elements.Count == 0)
            return null;
          return t.elements[position];
        }
      }
    }

    #endregion

    #region internal vars

    // list of the search results, containts objects of IMDBUrl
    ArrayList elements = new ArrayList();

    // Arrays for multiple database support
    int[] aLimits;		// contains the limit for searchresults
    string[] aDatabases;		// contains the name of the database, e.g. IMDB

    IProgress m_progress;

    #endregion

    #region ctor
    public IMDB()
      : this(null) { }

    public IMDB(IProgress progress)
    {
      m_progress = progress;
      // load the settings
      LoadSettings();
    }
    #endregion

    /// <summary>
    /// load settings from mediaportal.xml
    /// </summary>
    private void LoadSettings()
    {
      // getting available databases and limits
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        if (iNumber <= 0)
        {
          // no given databases in XML - setting to IMDB
          aLimits = new int[1];
          aDatabases = new string[1];
          aLimits[0] = 25;
          aDatabases[0] = "IMDB";
        }
        else
        {
          // initialise arrays
          aLimits = new int[iNumber];
          aDatabases = new string[iNumber];
          string strDatabase;
          int iLimit;
          bool bDouble = false;
          // get the databases
          for (int i = 0; i < iNumber; i++)
          {
            bDouble = false;
            iLimit = xmlreader.GetValueAsInt("moviedatabase", "limit" + i.ToString(), 25);
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "IMDB");
            // be aware of double entries!
            for (int j = 0; j < i; j++)
            {
              if (aDatabases[j] == strDatabase)
              {
                // double entry found, exit search
                bDouble = true;
                j = i;
              }
            }
            // valid entry?
            if (!bDouble)
            {
              // entry does not exist yet
              aLimits[i] = iLimit;
              aDatabases[i] = strDatabase;
            }
            else
            {
              // skip this entry
              aLimits[i] = 0;
              aDatabases[i] = "";
            }
          }
        }
      }
    }

    /// <summary>
    /// count the elements
    /// </summary>
    public int Count
    {
      get { return elements.Count; }
    }

    public IMDB.IMDBUrl this[int index]
    {
      get { return (IMDB.IMDBUrl)elements[index]; }
    }

    public IMDBEnumerator GetEnumerator() // non-IEnumerable version
    {
      return new IMDBEnumerator(this);
    }

    #region IEnumerable Member
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)new IMDBEnumerator(this);
    }
    #endregion

    #region helper methods to get infos

    /// <summary>
    /// trys to get a webpage from the specified url and returns the content as string
    /// </summary>
    private string GetPage(string strURL, string strEncode, out string absoluteUri)
    {
      string strBody = "";
      absoluteUri = string.Empty;
      Stream ReceiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strURL);

        result = req.GetResponse();
        ReceiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
        sr = new StreamReader(ReceiveStream, encode);
        strBody = sr.ReadToEnd();

        absoluteUri = result.ResponseUri.AbsoluteUri;
      }
      catch (Exception)
      {
        //Log.Error("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message, ex.StackTrace);
      }
      finally
      {
        if (sr != null)
        {
          try
          {
            sr.Close();
          }
          catch (Exception)
          {
          }
        }
        if (ReceiveStream != null)
        {
          try
          {
            ReceiveStream.Close();
          }
          catch (Exception)
          {
          }
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception)
          {
          }
        }
      }
      return strBody;
    }

    /// <summary>
    /// cuts end of sting after strWord
    /// </summary>
    void RemoveAllAfter(ref string strLine, string strWord)
    {
      int iPos = strLine.IndexOf(strWord);
      if (iPos > 0)
      {
        strLine = strLine.Substring(0, iPos);
      }
    }

    /// <summary>
    /// make a searchstring out of the filename
    /// </summary>
    string GetSearchString(string strMovie)
    {
      string strURL = strMovie;
      strURL = strURL.ToLower();
      strURL = strURL.Trim();

      // @ 23.09.2004 by FlipGer
      if (strURL.Length <= 7)
      {
        return strURL;
      }
      // END @ | i think it does not make much sense to parse such a short string
      // and i have no problems with x-men on OFDB ;-) and a better result on IMDB with x-men (try out "x men" :-)

      string strTmp = "";
      int ipos = 0;
      int iBracket = 0;
      //bool	bSkip = false;
      for (int i = 0; i < strURL.Length; ++i)
      {
        /* Why are numbers bigger than 999 skipped?
        for (int c=0;i+c < strURL.Length&&Char.IsDigit(strURL[i+c]);c++)
        {
          Log.Info("c: {0}",c);
          if (c==3)
          {
            i+=4;
            break;
          }
        }*/
        //if (i >=strURL.Length) break;
        char kar = strURL[i];
        if (kar == '[' || kar == '(')
          iBracket++;			//skip everthing between () and []
        else if (kar == ']' || kar == ')')
          iBracket--;
        else if (iBracket <= 0)
        {
          // change all non cahrs or digits into ' '
          if (!Char.IsLetterOrDigit(kar))
          {
            kar = ' ';

          }
          // skip whitespace at the beginning, only necessary if the "number skipping" is used
          //if ((kar==' ') && (ipos==0)) continue;

          // Mars Warrior @ 03-sep-2004.
          // Check for ' ' and '+' to avoid double or more ' ' and '+' which
          // mess up the search to the IMDB...
          if (strTmp.Length == 0)
          {
            strTmp += kar;
            ipos++;
          }
          else
          {
            if (
              Char.IsLetterOrDigit(kar) ||
              (kar == ' ' && strTmp[strTmp.Length - 1] != ' ')
              //|| (kar == '+' && strTmp[strTmp.Length -1] != '+')
              )
            {
              strTmp += kar;
              ipos++;
            }
          }
        }
      }

      strTmp = strTmp.Trim();

      // Mars Warrior @ 03-sep-2004.
      // The simple line "strTmp.ToLower()" does NOT work. As a result the wrong string
      // (still includes the " dvd" etc. strings) is send to the IMDB causing wrong lookups
      // By changing the line, everything is working MUCH better now ;-)

      RemoveAllAfter(ref strTmp, "divx");
      RemoveAllAfter(ref strTmp, "xvid");
      RemoveAllAfter(ref strTmp, "dvd");
      RemoveAllAfter(ref strTmp, " dvdrip");
      RemoveAllAfter(ref strTmp, "svcd");
      RemoveAllAfter(ref strTmp, "mvcd");
      RemoveAllAfter(ref strTmp, "vcd");
      RemoveAllAfter(ref strTmp, "cd");
      RemoveAllAfter(ref strTmp, "ac3");
      RemoveAllAfter(ref strTmp, "ogg");
      RemoveAllAfter(ref strTmp, "ogm");
      RemoveAllAfter(ref strTmp, "internal");
      RemoveAllAfter(ref strTmp, "fragment");
      RemoveAllAfter(ref strTmp, "proper");
      RemoveAllAfter(ref strTmp, "limited");
      RemoveAllAfter(ref strTmp, "rerip");

      RemoveAllAfter(ref strTmp, "+divx");
      RemoveAllAfter(ref strTmp, "+xvid");
      RemoveAllAfter(ref strTmp, "+dvd");
      RemoveAllAfter(ref strTmp, "+dvdrip");
      RemoveAllAfter(ref strTmp, "+svcd");
      RemoveAllAfter(ref strTmp, "+mvcd");
      RemoveAllAfter(ref strTmp, "+vcd");
      RemoveAllAfter(ref strTmp, "+cd");
      RemoveAllAfter(ref strTmp, "+ac3");
      RemoveAllAfter(ref strTmp, "+ogg");
      RemoveAllAfter(ref strTmp, "+ogm");
      RemoveAllAfter(ref strTmp, "+internal");
      RemoveAllAfter(ref strTmp, "+fragment");
      RemoveAllAfter(ref strTmp, "+proper");
      RemoveAllAfter(ref strTmp, "+limited");
      RemoveAllAfter(ref strTmp, "+rerip");

      // return the new formatted string
      return strTmp;
    }

    #endregion

    #region methods to get movie infos from different databases

    /// <summary>
    /// this method switches between the different databases to get the search results
    /// </summary>
    public void Find(string strMovie)
    {
      try
      {
        string strURL;
        // getting searchstring
        string strSearch = HttpUtility.UrlEncode(GetSearchString(strMovie));

        // be aware of german special chars ���� äöüß %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
        strSearch = strSearch.Replace("%c3%a4", "%E4");
        strSearch = strSearch.Replace("%c3%b6", "%F6");
        strSearch = strSearch.Replace("%c3%bc", "%FC");
        strSearch = strSearch.Replace("%c3%9f", "%DF");
        // be aware of spanish special chars ����������� %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
        strSearch = strSearch.Replace("%c3%b1", "%F1");
        strSearch = strSearch.Replace("%c3%a0", "%E0");
        strSearch = strSearch.Replace("%c3%a1", "%E1");
        strSearch = strSearch.Replace("%c3%a8", "%E8");
        strSearch = strSearch.Replace("%c3%a9", "%E9");
        strSearch = strSearch.Replace("%c3%ac", "%EC");
        strSearch = strSearch.Replace("%c3%ad", "%ED");
        strSearch = strSearch.Replace("%c3%b2", "%F2");
        strSearch = strSearch.Replace("%c3%b3", "%F3");
        strSearch = strSearch.Replace("%c3%b9", "%F9");
        strSearch = strSearch.Replace("%c3%ba", "%FA");
        elements.Clear();

        string line1, line2, line3;
        line1 = GUILocalizeStrings.Get(984);
        line2 = GetSearchString(strMovie).Replace("+", " ");
        line3 = "";
        int percent = 0;

        if (m_progress != null)
          m_progress.OnProgress(line1, line2, line3, percent);
        // search the desired databases
        for (int i = 0; i < aDatabases.Length; i++)
        {
          // only do a search if requested
          if (aLimits[i] > 0)
          {
            // Script support script.csscript
            string grabberFileName = ScriptDirectory + @"\" + aDatabases[i] + ".csscript";
            if (!File.Exists(grabberFileName))
            {
              Log.Error("Movie database lookup Find()- grabber script not found: {0}", grabberFileName);
              return;
            }

            line1 = GUILocalizeStrings.Get(984) + ": Script " + aDatabases[i];

            if (m_progress != null)
              m_progress.OnProgress(line1, line2, line3, percent);
            try
            {
              Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
              AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.Load(grabberFileName, null, false));
              IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");
              grabber.FindFilm(strSearch, aLimits[i], elements);
              percent += 100/aDatabases.Length;
              if (m_progress != null)
                m_progress.OnProgress(line1, line2, line3, percent);
            }
            catch (Exception ex)
            {
              Log.Info("Movie database lookup Find() - file: {0}, message : {1}", grabberFileName, ex.Message);
              return;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("Movie database lookup Find() - Exception: {0}", ex.Message);
      }
    }

    /// <summary>
    /// this method switches between the different databases to fetche the search result into movieDetails
    /// </summary>
    public bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
    {
      string grabberFileName = ScriptDirectory + @"\" + url.Database + ".csscript";

      try
      {
        /*
        // extract host from url, to find out which mezhod should be called
        int		iStart = url.URL.IndexOf(".")+1;
        int		iEnd = url.URL.IndexOf(".",iStart);
        if ((iStart<0) || (iEnd<0))
        {
          // could not extract hostname!
          Log.Info("Movie DB lookup GetDetails(): could not extract hostname from {0}",url.URL);
          return false;
        }
        string	strHost = url.URL.Substring(iStart,iEnd-iStart).ToUpper();*/

        // Script support script.csscript
        if (!File.Exists(grabberFileName))
        {
          Log.Error("Movie database lookup GetDetails()- grabber script not found: {0}", grabberFileName);
          return false;
        }

        try
        {
          AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.Load(grabberFileName, null, false));
          IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");
          grabber.GetDetails(url, ref movieDetails);
          return true;
        }
        catch (Exception ex)
        {
          Log.Info("Movie database lookup GetDetails() - file: {0}, message : {1}", grabberFileName, ex.Message);
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Movie database lookup GetDetails() - file: {0}, message : {1}", grabberFileName, ex.Message);
        return false;
      }
    }

    #endregion

    #region methods to get actor infos

    public void FindActor(string strActor)
    {
      string strURL;
      // getting searchstring
      string strSearch = HttpUtility.UrlEncode(GetSearchString(strActor));

      // be aware of german special chars ���� äöüß %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
      strSearch = strSearch.Replace("%c3%a4", "%E4");
      strSearch = strSearch.Replace("%c3%b6", "%F6");
      strSearch = strSearch.Replace("%c3%bc", "%FC");
      strSearch = strSearch.Replace("%c3%9f", "%DF");
      // be aware of spanish special chars ����������� %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
      strSearch = strSearch.Replace("%c3%b1", "%F1");
      strSearch = strSearch.Replace("%c3%a0", "%E0");
      strSearch = strSearch.Replace("%c3%a1", "%E1");
      strSearch = strSearch.Replace("%c3%a8", "%E8");
      strSearch = strSearch.Replace("%c3%a9", "%E9");
      strSearch = strSearch.Replace("%c3%ac", "%EC");
      strSearch = strSearch.Replace("%c3%ad", "%ED");
      strSearch = strSearch.Replace("%c3%b2", "%F2");
      strSearch = strSearch.Replace("%c3%b3", "%F3");
      strSearch = strSearch.Replace("%c3%b9", "%F9");
      strSearch = strSearch.Replace("%c3%ba", "%FA");

      elements.Clear();

      string line1, line2, line3;
      line1 = GUILocalizeStrings.Get(986);
      line2 = strActor;
      line3 = "";
      int percent = -1;
      if (m_progress != null)
        m_progress.OnProgress(line1, line2, line3, percent);
      strURL = String.Format("http://us.imdb.com/find?q={0};nm=on;mx=20", strSearch);
      FindIMDBActor(strURL, strActor);

    }

    private void FindIMDBActor(string strURL, string strActor)
    {
      try
      {
        HTMLUtil htmlUtil = new HTMLUtil();
        string absoluteUri;
        string strBody = GetPage(strURL, "utf-8", out absoluteUri);
        string value = string.Empty;
        HTMLParser parser = new HTMLParser(strBody);
        if ((parser.skipToEndOf("<title>")) &&
            (parser.extractTo("</title>", ref value)) && !value.Equals("IMDb Name  Search"))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = MediaPortal.Util.Utils.RemoveParenthesis(value).Trim();
          IMDBUrl oneUrl = new IMDBUrl(absoluteUri, value, "IMDB");
          elements.Add(oneUrl);
          return;
        }
        parser.resetPosition();
        while (parser.skipToEndOfNoCase("found the following results"))
        {
          string url = string.Empty;
          string name = string.Empty;
          //<a href="/name/nm0000246/" onclick="set_args('nm0000246', 1)">Bruce Willis</a>
          if (parser.skipToStartOf("href=\"/name/"))
          {
            parser.skipToEndOf("href=\"");
            parser.extractTo("\"", ref url);
            parser.skipToEndOf(">");
            parser.extractTo("</a>", ref name);
            name = new HTMLUtil().ConvertHTMLToAnsi(name);
            name = MediaPortal.Util.Utils.RemoveParenthesis(name).Trim();
            IMDBUrl newUrl = new IMDBUrl("http://us.imdb.com" + url, name, "IMDB");
            elements.Add(newUrl);
          }
          else
          {
            parser.skipToEndOfNoCase("</a>");
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message, ex.StackTrace);
      }
    }

    public bool GetActorDetails(IMDB.IMDBUrl url, out IMDBActor actor)
    {
      actor = new IMDBActor();
      try
      {
        //<a name="headshot" href="photogallery"><img border="0" src="http://ia.imdb.com/media/imdb/01/I/84/36/12m.jpg" width="100" height="140" alt="Bruce Willis (I)"></a>
        string absoluteUri;
        string strBody = GetPage(url.URL, "utf-8", out absoluteUri);
        if (strBody == null)
          return false;
        if (strBody.Length == 0)
          return false;
        HTMLParser parser = new HTMLParser(strBody);
        string strThumb = string.Empty;
        string value = string.Empty;
        string value2 = string.Empty;
        if ((parser.skipToEndOf("<title>")) &&
            (parser.extractTo("</title>", ref value)))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = MediaPortal.Util.Utils.RemoveParenthesis(value).Trim();
          //Log.Info("Actor Name:{0}", value);
          actor.Name = value;
        }
        if (actor.Name == string.Empty)
        {
          actor.Name = url.Title;
        }
        //get picture
        if ((parser.skipToEndOf("<a name=\"headshot")) &&
            (parser.skipToEndOf("<img")) &&
            (parser.skipToEndOf("src=\"")) &&
            (parser.extractTo("\"", ref strThumb)))
        {
          //Log.Info("Actor Thumb:{0}", strThumb);
          actor.ThumbnailUrl = strThumb;
        }
        if ((parser.skipToEndOf("/OnThisDay?")) &&
            (parser.skipToEndOf(">")) &&
            (parser.extractTo("<", ref value)) &&
            (parser.skipToEndOf("/BornInYear?")) &&
            (parser.extractTo("\"", ref value2)))
        {
          //Log.Info("Actor Birth:{0} {1}", value, value2);
          actor.DateOfBirth = value + " " + value2;
        }

        if ((parser.skipToEndOf("/BornWhere?")) &&
            (parser.skipToEndOf(">")) &&
            (parser.extractTo("<", ref value)))
        {
          //Log.Info("Actor Place:{0}", value);
          actor.PlaceOfBirth = value;
        }
        //find Mini Biography
        //<dt><div class="ch">Mini biography</div></dt>
        //<dd><a href="/name/nm0000193/">Demi Moore</a> was born 1962 in Roswell, New Mexico. Her father left her mother... <a href="bio">(show more)</a></dd>
        //</dl>
        if ((parser.skipToEndOf("Mini biography")) &&
            (parser.skipToEndOf("</h5>")) &&
            (parser.extractTo("<a", ref value)) &&
            (parser.skipToEndOf("href=\"")) &&
            (parser.extractTo("\"", ref value2)))
        {
          //Log.Info("Actor Mini:{0}", value);
          //Log.Info("Actor BIO URL:{0}", value2);
          actor.MiniBiography = MediaPortal.Util.Utils.stripHTMLtags(value).Trim();
          actor.MiniBiography = HttpUtility.HtmlDecode(actor.MiniBiography);  // Remove HTML entities like &#189;

          //get complete biography
          string bioURL = absoluteUri;
          int pos = bioURL.IndexOf("?");
          if (pos > 0)
            bioURL = bioURL.Substring(0, pos);
          if (!bioURL.EndsWith("/"))
          {
            bioURL += "/";
          }
          bioURL += value2;
          //Log.Info("Bio Url:{0}", bioURL);
          string strBioBody = GetPage(bioURL, "utf-8", out absoluteUri);
          if (strBioBody != null && strBioBody.Length > 0)
          {
            HTMLParser parser1 = new HTMLParser(strBioBody);
            if (parser1.skipToEndOf("<h5>Mini biography</h5>") &&
                parser1.extractTo("</p>", ref value))
            {
              //Log.Info("Actor Bio:{0}", value);
              actor.Biography = MediaPortal.Util.Utils.stripHTMLtags(value).Trim();
              actor.Biography = HttpUtility.HtmlDecode(actor.Biography);  // Remove HTML entities like &#189;
            }
          }
        }
        if (parser.skipToEndOf("<ol>"))
        {
          string movies = string.Empty;
          if (parser.extractTo("</ol>", ref movies))
          {
            //Log.Info("Actor Movies:{0}", movies);
            parser.Content = movies;
          }
          while (parser.skipToStartOf("<li>"))
          {
            string movie = string.Empty;
            if (parser.extractTo("</li>", ref movie))
            {
              movie += "</li>";
              int start = movie.IndexOf("<i>");
              int end = movie.IndexOf("</i>");
              if ((start >= 0) && (end >= 0))
              {
                movie = movie.Substring(0, start) + movie.Substring(end + 4);
              }
              //Log.Info("Actor Movie:{0}", movie);
              HTMLParser movieParser = new HTMLParser(movie);
              string title = string.Empty;
              string episode = string.Empty;
              string strYear = string.Empty;
              string role = string.Empty;
              movieParser.skipToEndOf("<a");
              movieParser.skipToEndOf(">");
              movieParser.extractTo("</a>", ref title);
              title = HttpUtility.HtmlDecode(title);  // Remove HTML entities like &#189;
              //Log.Info("Actor Movie title:{0}", title);
              bool isTvSeries = false;
              while (movieParser.skipToEndOf("- <a"))
              {
                isTvSeries = true;
                if (movieParser.skipToEndOf(">"))
                {
                  movieParser.extractTo("</a>", ref episode);
                  episode = HttpUtility.HtmlDecode(episode);  // Remove HTML entities like &#189;
                  //Log.Info("Actor Movie episode:{0}", episode);
                }
                if (movieParser.skipToStartOf("(20") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Episode year:{0}", strYear);
                }
                else if (movieParser.skipToStartOf("(19") &&
                         movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Episode year:{0}", strYear);
                }
                if (movieParser.skipToEndOf(".... "))
                {
                  movieParser.extractTo("<", ref role);
                  //Log.Info("Actor Episode role:{0}", role);
                  role = role.Trim();
                  role = HttpUtility.HtmlDecode(role);  // Remove HTML entities like &#189;
                }

                int year = 0;
                try
                {
                  year = Int32.Parse(strYear);
                }
                catch (Exception)
                {
                  year = 1900;
                }
                IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
                actorMovie.MovieTitle = title + "-" + episode;
                actorMovie.Role = role;
                actorMovie.Year = year;
                actor.Add(actorMovie);
                //Log.Info("Actor Movie {0} as {1},{2}", actorMovie.MovieTitle, actorMovie.Role, actorMovie.Year);
              }
              if (!isTvSeries)
              {
                if (movieParser.skipToStartOf("(20") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Movie year:{0}", strYear);
                }
                else if (movieParser.skipToStartOf("(19") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Movie year:{0}", strYear);
                }
                if (movieParser.skipToEndOf(".... "))
                {
                  movieParser.extractTo("<", ref role);
                  //Log.Info("Actor Movie role:{0}", role);
                  role = role.Trim();
                }

                int year = 0;
                try
                {
                  year = Int32.Parse(strYear);
                }
                catch (Exception)
                {
                  year = 1900;
                }
                IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
                actorMovie.MovieTitle = title;
                actorMovie.Role = role;
                actorMovie.Year = year;
                actor.Add(actorMovie);
              }
            }

          }
        }
        return true;
      }
      catch (Exception)
      {
        //Log.Info("IMDB.GetActorDetails({0} exception:{1} {2} {3}", url.URL,ex.Message,ex.Source,ex.StackTrace);
      }
      return false;
    }

    #endregion
  }


  /// <summary>
  /// Interface used for script support
  /// </summary>
  public interface IIMDBScriptGrabber
  {
    void FindFilm(string title, int limit, ArrayList elements);
    bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails);
    string GetName();
    string GetLanguage();
  }
}