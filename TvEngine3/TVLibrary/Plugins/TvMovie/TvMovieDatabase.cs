#region Copyright (C) 2006-2007 Team MediaPortal

/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.Win32;

using TvDatabase;
using TvLibrary.Log;

using Gentle.Common;
using Gentle.Framework;

namespace TvEngine
{
  class TvMovieDatabase
  {
    #region Members
    private OleDbConnection _databaseConnection = null;
    private bool _canceled = false;
    private ArrayList _tvmEpgChannels = null;
    private ArrayList _channelList = null;
    private int _programsCounter = 0;
    private bool _useShortProgramDesc = false;
    private bool _extendDescription = false;
    private bool _showRatings = false;
    private bool _showAudioFormat = false;
    private bool _slowImport = false;
    private int _actorCount = 5;

    static string _xmlFile;
    #endregion

    #region Events
    public delegate void ProgramsChanged(int value, int maximum, string text);
    public event ProgramsChanged OnProgramsChanged;
    public delegate void StationsChanged(int value, int maximum, string text);
    public event StationsChanged OnStationsChanged;
    #endregion

    private struct Mapping
    {
      private string _mpChannel;
      private string _tvmEpgChannel;
      private TimeSpan _start;
      private TimeSpan _end;

      public Mapping(string mpChannel, string tvmChannel, string start, string end)
      {
        _mpChannel = mpChannel;
        _tvmEpgChannel = tvmChannel;
        _start = CleanInput(start);
        _end = CleanInput(end);
      }

      #region struct getter & setters
      public string Channel
      {
        get { return _mpChannel; }
      }

      public string TvmEpgChannel
      {
        get { return _tvmEpgChannel; }
      }

      public TimeSpan Start
      {
        get { return _start; }
      }

      public TimeSpan End
      {
        get { return _end; }
      }

      private static TimeSpan CleanInput(string input)
      {
        int hours = 0;
        int minutes = 0;
        input = input.Trim();
        int index = input.IndexOf(':');
        if (index > 0)
          hours = Convert.ToInt16(input.Substring(0, index));
        if (index + 1 < input.Length)
          minutes = Convert.ToInt16(input.Substring(index + 1));

        if (hours > 23)
          hours = 0;

        if (minutes > 59)
          minutes = 0;

        return new TimeSpan(hours, minutes, 0);
      }
      #endregion
    }

    #region class get && set functions
    public ArrayList Stations
    {
      get { return _tvmEpgChannels; }
    }

    public bool Canceled
    {
      get { return _canceled; }
      set { _canceled = value; }
    }

    public int Programs
    {
      get { return _programsCounter; }
    }

    public static string TVMovieProgramPath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("ProgrammPath"));

        TvBusinessLayer layer = new TvBusinessLayer();
        mpPath = layer.GetSetting("TvMovieInstallPath", path).Value;

        if (File.Exists(mpPath))
          return mpPath;

        return path;
      }
    }

    public static string DatabasePath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("DBDatei"));

        TvBusinessLayer layer = new TvBusinessLayer();
        mpPath = layer.GetSetting("TvMoviedatabasepath", path).Value;

        if (File.Exists(mpPath))
          return mpPath;

        return path;
      }
      set
      {
        string path = string.Empty;

        string newPath = value;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("DBDatei"));

        if (!File.Exists(newPath))
          newPath = path;

        string mpPath = string.Empty;
        TvBusinessLayer layer = new TvBusinessLayer();

        mpPath = layer.GetSetting("TvMoviedatabasepath", string.Empty).Value;
        Setting setting = layer.GetSetting("TvMovieEnabled");

        if (newPath == path)
          setting.Value = string.Empty;
        else
          setting.Value = newPath;

        setting.Persist();
      }
    }
    #endregion

    #region public functions
    public ArrayList GetChannels()
    {
      ArrayList tvChannels = new ArrayList();
      IList allChannels = Channel.ListAll();
      foreach (Channel channel in allChannels)
      {
        if (channel.IsTv && channel.VisibleInGuide)
          tvChannels.Add(channel);
      }
      return tvChannels;
    }

    public void Connect()
    {
      LoadMemberSettings();

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";
      
      if (DatabasePath != string.Empty)
        dataProviderString = string.Format(dataProviderString, DatabasePath);
      else
        return;

      _databaseConnection = new OleDbConnection(dataProviderString);

      string sqlSelect = "SELECT Sender.SenderKennung FROM Sender WHERE (((Sender.Favorit)=-1)) ORDER BY Sender.SenderKennung DESC;";

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);
      DataSet tvMovieTable = new DataSet();

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "Sender");
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        Log.Error("TVMovie: Error accessing TV Movie Clickfinder database while reading stations");
        Log.Error("TVMovie: Exception: {0}", ex);
        _canceled = true;
        return;
      }
      finally
      {
        _databaseConnection.Close();
      }

      _tvmEpgChannels = new ArrayList();
      foreach (DataRow sender in tvMovieTable.Tables["Sender"].Rows)
        _tvmEpgChannels.Add(sender["Senderkennung"]);

      _channelList = GetChannels();
    }

    public void Import()
    {
      if (_canceled)
        return;

      ArrayList mappingList = GetMappingList();

      if (mappingList == null)
      {
        Log.Error("TVMovie: Cannot import from TV Movie database");
        return;
      }

      DateTime ImportStartTime = DateTime.Now;
      Log.Debug("TVMovie: Importing database");

      TvBusinessLayer layer = new TvBusinessLayer();

      if (_canceled)
        return;

      int maximum = 0;

      //Log.Debug("TVMovie: Calculating stations");
      foreach (string tvmChan in _tvmEpgChannels)
        foreach (Mapping mapping in mappingList)
          if (mapping.TvmEpgChannel == tvmChan)
          {
            maximum++;
            break;
          }

      if (OnStationsChanged != null)
        OnStationsChanged(1, maximum, string.Empty);
      Log.Debug("TVMovie: Calculating stations done");

      //ArrayList channelList = new ArrayList();
      //foreach (Mapping mapping in mappingList)
      //  if (channelList.IndexOf(mapping.Channel) == -1)
      //  {
      //    channelList.Add(mapping.Channel);
      //    Log.Debug("TVMovie: adding channel {0} - ClearPrograms", mapping.Channel);
      //    ClearPrograms(mapping.Channel);
      //  }

      // setting update time of epg import to avoid that the background thread triggers another import
      // if the process lasts longer than the timer's update check interval
      Setting setting = layer.GetSetting("TvMovieLastUpdate");
      setting.Value = DateTime.Now.ToString();
      setting.Persist();

      Log.Debug("TVMovie: Mapped {0} stations for EPG import", Convert.ToString(maximum));

      int counter = 0;

      foreach (string station in _tvmEpgChannels)
      {
        if (_canceled)
          return;

        // get all tv movie channels
        List<Mapping> channelNames = new List<Mapping>();
        // get all tv channels
        IList allChannels = Channel.ListAll();

        foreach (Mapping mapping in mappingList)
          if (mapping.TvmEpgChannel == station)
            channelNames.Add(mapping);

        if (channelNames.Count > 0)
        {          
          try
          {
            string display = string.Empty;
            foreach (Mapping channelName in channelNames)
              display += string.Format("{0}  /  ", channelName.Channel);

            display = display.Substring(0, display.Length - 5);
            if (OnStationsChanged != null)
              OnStationsChanged(counter, maximum, display);
            counter++;
            Log.Info("TVMovie: Retrieving data for station [{0}/{1}] - {2}", Convert.ToString(counter), Convert.ToString(maximum), display);
            _programsCounter += ImportStation(station, channelNames, allChannels);
          }
          catch (Exception ex)
          {
            Log.Error("TVMovie: Error importing EPG - {0},{1}", ex.Message, ex.StackTrace);
          }
        }
      }
      if (OnStationsChanged != null)
        OnStationsChanged(maximum, maximum, "Import done");

      if (!_canceled)
      {
        try
        {
          setting = layer.GetSetting("TvMovieLastUpdate");
          setting.Value = DateTime.Now.ToString();
          setting.Persist();

          TimeSpan ImportDuration = (DateTime.Now - ImportStartTime);
          Log.Debug("TVMovie: Imported {0} database entries for {1} stations in {2} minutes", _programsCounter, counter, Convert.ToString(ImportDuration.Minutes));
        }
        catch (Exception)
        {
          Log.Error("TVMovie: Error updating the database with last import date");
        }                
      }
      GC.Collect(); GC.Collect(); GC.Collect(); GC.Collect();
    }

    public bool NeedsImport
    {
      get
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        
        try
        {
          TimeSpan restTime = new TimeSpan(Convert.ToInt32(layer.GetSetting("TvMovieRestPeriod", "24").Value), 0, 0);          
          DateTime lastUpdated = Convert.ToDateTime(layer.GetSetting("TvMovieLastUpdate", "0").Value);
          //        if (Convert.ToInt64(layer.GetSetting("TvMovieLastUpdate", "0").Value) == LastUpdate)
          if (lastUpdated >= (DateTime.Now - restTime))
          {            
            return false;
          }
          else
          {
            Log.Debug("TVMovie: Last update was at {0} - new import scheduled", Convert.ToString(lastUpdated));
            return true;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVMovie: An error occured checking the last import time {0}", ex.Message);
          Log.Write(ex);
          return true;
        }
      }
    }    
    #endregion

    #region private functions
    private void LoadMemberSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      _useShortProgramDesc = layer.GetSetting("TvMovieShortProgramDesc", "true").Value == "true";
      _extendDescription = layer.GetSetting("TvMovieExtendDescription", "false").Value == "true";
      _showRatings = layer.GetSetting("TvMovieShowRatings", "false").Value == "true";
      _showAudioFormat = layer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      _slowImport = layer.GetSetting("TvMovieSlowImport", "false").Value == "true";
      _actorCount = Convert.ToInt32(layer.GetSetting("TvMovieLimitActors", "5").Value);

      _xmlFile = String.Format(@"{0}\MediaPortal TV Server\TVMovieMapping.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    private int ImportStation(string stationName, List<Mapping> channelNames, IList allChannels)
    {      
      string sqlSelect = string.Empty;
      string audioFormat = String.Empty;
      StringBuilder sqlb = new StringBuilder();

      if (_databaseConnection == null)
        return 0;

      // UNUSED: F16zu9 , live , untertitel , Dauer , Wiederholung
      sqlb.Append("SELECT TVDaten.Beginn, TVDaten.Ende, TVDaten.Sendung, TVDaten.Genre, TVDaten.Kurzkritik, TVDaten.KurzBeschreibung, TVDaten.Beschreibung");

      if (_showAudioFormat)
        sqlb.Append(", TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton");

      if (_extendDescription)
        sqlb.Append(", TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Originaltitel, TVDaten.Regie, TVDaten.Darsteller");

      if (_showRatings)
        sqlb.Append(", TVDaten.Interessant, TVDaten.Bewertungen");

      sqlb.Append(" FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;");

      sqlSelect = string.Format(sqlb.ToString(), stationName);
 
      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);

      DataSet tvMovieTable = new DataSet();

      foreach (Mapping map in channelNames)
        if (map.TvmEpgChannel == stationName)
        {
          ClearPrograms(map.Channel);
          Log.Debug("TVMovie: Purged old programs for channel {0}", map.Channel);          
        }

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "TVDaten");
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        Log.Error("TVMovie: Error accessing TV Movie Clickfinder database - Current import canceled, waiting for next schedule");
        Log.Error("TVMovie: Exception: {0}", ex);
        return 0;
      }
      finally
      {
        _databaseConnection.Close();
      }      

      int programsCount = tvMovieTable.Tables["TVDaten"].Rows.Count;

      if (OnProgramsChanged != null)
        OnProgramsChanged(0, programsCount + 1, string.Empty);

      int counter = 0;

      foreach (DataRow guideEntry in tvMovieTable.Tables["TVDaten"].Rows)
      {
        if (_canceled)
          break;

        string channel = stationName;
        DateTime end = DateTime.MinValue;
        DateTime start = DateTime.MinValue;
        string classification = string.Empty;
        string date = string.Empty;
        string episode = string.Empty;
        int starRating = -1;
        string detailedRating = string.Empty;
        string director = string.Empty;
        string actors = string.Empty;
        try
        {
          end = DateTime.Parse(guideEntry["Ende"].ToString());     // iEndTime ==> Ende  (15.06.2006 22:45:00 ==> 20060615224500)
          start = DateTime.Parse(guideEntry["Beginn"].ToString()); // iStartTime ==> Beginn (15.06.2006 22:45:00 ==> 20060615224500)
        }
        catch (Exception ex2)
        {
          Log.Error("TVMovie: Error parsing EPG data - {0},{1}", ex2.Message, ex2.StackTrace);
        }

        string title = guideEntry["Sendung"].ToString();
        string shortDescription = guideEntry["KurzBeschreibung"].ToString();
        string description;
        if (_useShortProgramDesc)  
          description = shortDescription;
        else
        {
          description = guideEntry["Beschreibung"].ToString();
          if (description.Length < shortDescription.Length)
            description = shortDescription;
        }
        
        string genre = guideEntry["Genre"].ToString();
        string shortCritic = guideEntry["Kurzkritik"].ToString();

        if (_extendDescription)
        {
          classification = guideEntry["FSK"].ToString();
          date = guideEntry["Herstellungsjahr"].ToString();
          episode = guideEntry["Originaltitel"].ToString();
          director = guideEntry["Regie"].ToString();
          actors = guideEntry["Darsteller"].ToString();
          //int repeat = Convert.ToInt16(guideEntry["Wiederholung"]);         // strRepeat ==> Wiederholung "Repeat" / "unknown"      
        }

        if (_showRatings)
        {
          starRating = Convert.ToInt16(guideEntry["Interessant"]) - 1;
          detailedRating = guideEntry["Bewertungen"].ToString();
        }    

        if (_showAudioFormat)
        {
          bool audioDesc = Convert.ToBoolean(guideEntry["Audiodescription"]);
          bool dolbyDigital = Convert.ToBoolean(guideEntry["DolbyDigital"]);
          bool dolbySuround = Convert.ToBoolean(guideEntry["DolbySuround"]);
          bool dolby = Convert.ToBoolean(guideEntry["Dolby"]);
          bool stereo = Convert.ToBoolean(guideEntry["Stereo"]);
          bool dualAudio = Convert.ToBoolean(guideEntry["Zweikanalton"]);
          audioFormat = BuildAudioDescription(audioDesc, dolbyDigital, dolbySuround, dolby, stereo, dualAudio);
        }

        if (OnProgramsChanged != null)
          OnProgramsChanged(counter, programsCount + 1, title);

        counter++;

        foreach (Mapping channelName in channelNames)
        {
          DateTime newStartDate = start;
          DateTime newEndDate = end;

          if (!CheckEntry(ref newStartDate, ref newEndDate, channelName.Start, channelName.End))
          {
            Channel progChannel = null;

            foreach (Channel ch in allChannels)
            {
              if (ch.DisplayName == channelName.Channel)
              {
                progChannel = ch;
                break;
              }
            }
            DateTime OnAirDate = DateTime.MinValue;

            if (date.Length > 0 && date != @"-")
            {
              try
              {
                OnAirDate = DateTime.Parse(String.Format("01.01.{0} 00:00:00", date));
              }
              catch (Exception ex3)
              {
                Log.Info("TVMovie: Invalid year for OnAirDate - {0}", date);
              }
            }

            short EPGStarRating = -1;

            switch (starRating)
            {
              case 0:
                EPGStarRating = 2; break;
              case 1:
                EPGStarRating = 4; break;
              case 2:
                EPGStarRating = 6; break;
              case 3:
                EPGStarRating = 8; break;
              case 4:
                EPGStarRating = 10; break;
              case 5:
                EPGStarRating = 8; break;
              case 6:
                EPGStarRating = 10; break;
              default:
                EPGStarRating = -1; break;
            }

            Program prog = new Program(progChannel.IdChannel, newStartDate, newEndDate, title, description, genre, false, OnAirDate, string.Empty, string.Empty, EPGStarRating, classification,0);

            if (audioFormat == String.Empty)
              prog.Description = description.Replace("<br>", "\n");
            else
              prog.Description = "Ton: " + audioFormat + "\n" + description.Replace("<br>", "\n");

            if (_extendDescription)
            {
              StringBuilder sb = new StringBuilder();

              if (episode != String.Empty)
                sb.Append("Folge: " + episode + "\n");

              if (starRating != -1 && _showRatings)
              {
                //sb.Append("Wertung: " + string.Format("{0}/5", starRating) + "\n");
                sb.Append("Wertung: ");
                if (shortCritic.Length > 1)
                {
                  sb.Append(shortCritic + " - ");
                }
                sb.Append(BuildRatingDescription(starRating));
                if (detailedRating.Length > 0)
                  sb.Append(BuildDetailedRatingDescription(detailedRating));
              }

              sb.Append(prog.Description + "\n");

              if (director.Length > 0)
                sb.Append("Regie: " + director + "\n");
              if (actors.Length > 0)
                sb.Append(BuildActorsDescription(actors));
              if (classification != String.Empty && classification != "0")
                sb.Append("FSK: " + classification + "\n");
              if (date != String.Empty)
                sb.Append("Jahr: " + date + "\n");

              prog.Description = sb.ToString();
            }
            else
            {
              if (_showRatings)
                if (shortCritic.Length > 1)
                  prog.Description = shortCritic + "\n" + description;
            }
            
            prog.Persist();
            if (_slowImport)
              Thread.Sleep(50);
          }
        }
      }

      //Log.Debug("TVMovie: Importing data for station done");

      if (OnProgramsChanged != null)
        OnProgramsChanged(programsCount + 1, programsCount + 1, string.Empty);
      return counter;
    }

    private ArrayList GetMappingList()
    {
      IList mappingDb = TvMovieMapping.ListAll();
      ArrayList mappingList = new ArrayList();

      foreach (TvMovieMapping mapping in mappingDb)
      {
        string newStart = mapping.TimeSharingStart;
        string newEnd = mapping.TimeSharingEnd;
        string newChannel = Channel.Retrieve(mapping.IdChannel).DisplayName;
        string newStation = mapping.StationName;

        mappingList.Add(new TvMovieDatabase.Mapping(newChannel, newStation, newStart, newEnd));
      }

      return mappingList;
    }

    private bool CheckChannel(string channelName)
    {
      if (_channelList != null)
        foreach (Channel channel in _channelList)
          if (channel.DisplayName == channelName)
            return true;

      return false;
    }

    private bool CheckStation(string stationName)
    {
      if (_tvmEpgChannels != null)
        foreach (string station in _tvmEpgChannels)
          if (station == stationName)
            return true;

      return false;
    }

    private bool CheckEntry(ref DateTime progStart, ref DateTime progEnd, TimeSpan timeSharingStart, TimeSpan timeSharingEnd)
    {
      if (timeSharingStart == timeSharingEnd)
        return false;

      DateTime stationStart = progStart.Date + timeSharingStart;
      DateTime stationEnd = progStart.Date + timeSharingEnd;

      if (stationStart > progStart && progEnd <= stationStart)
        stationStart = stationStart.AddDays(-1);
      else if (timeSharingEnd < timeSharingStart)
        stationEnd = stationEnd.AddDays(1);

      if (progStart >= stationStart && progStart < stationEnd && progEnd > stationEnd)
        progEnd = stationEnd;

      if (progStart <= stationStart && progEnd > stationStart && progEnd < stationEnd)
        progStart = stationStart;

      if ((progEnd <= stationEnd) && (progStart >= stationStart))
        return false;

      return true;
    }

    /// <summary>
    /// passing the TV movie sound bool params this method returns the audio format as string
    /// </summary>
    /// <param name="audioDesc"></param>
    /// <param name="dolbyDigital"></param>
    /// <param name="dolbySuround"></param>
    /// <param name="dolby"></param>
    /// <param name="stereo"></param>
    /// <param name="dualAudio"></param>
    /// <returns>plain text audio format</returns>
    private string BuildAudioDescription(bool audioDesc, bool dolbyDigital, bool dolbySurround, bool dolby, bool stereo, bool dualAudio)
    {
      string audioFormat = String.Empty;

      if (dolbyDigital)
        audioFormat = "Dolby Digital";
      if (dolbySurround)
        audioFormat = "Dolby Surround";
      if (dolby)
        audioFormat = "Dolby";
      if (stereo)
        audioFormat = "Stereo";
      if (dualAudio)
        audioFormat = "Mehrkanal-Ton";

      return audioFormat;
    }

    private string BuildRatingDescription(int dbRating)
    {
      string TVMovieRating = String.Empty;

      switch (dbRating)
      {
        case 0:
          TVMovieRating = "uninteressant";
          break;
        case 1:
          TVMovieRating = "durchschnittlich";
          break;
        case 2:
          TVMovieRating = "empfehlenswert";
          break;
        case 3:
          TVMovieRating = "Tages-Tipp!";
          break;
        case 4:
          TVMovieRating = "Blockbuster!";
          break;
        case 5:
          TVMovieRating = "Genre-Tipp";
          break;
        case 6:
          TVMovieRating = "Genre-Highlight!";
          break;
        default:
          TVMovieRating = "---";
          break;
      }

      return TVMovieRating + "\n";
    }

    private string BuildDetailedRatingDescription(string dbDetailedRating)
    {
      // "Spa�=1;Action=3;Erotik=1;Spannung=3;Anspruch=0"
      int posidx = 0;
      string detailedRating = string.Empty;
      StringBuilder strb = new StringBuilder();

      if (dbDetailedRating != String.Empty)
      {
        posidx = dbDetailedRating.IndexOf(@"Spa�=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 5] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 6));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Action=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Erotik=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Spannung=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 9] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 10));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Anspruch=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 9] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 10));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Gef�hl=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        detailedRating = strb.ToString();
      }

      return detailedRating;
    }

    private string BuildActorsDescription(string dbActors)
    {
      StringBuilder strb = new StringBuilder();
      // Mit: Bernd Schramm (Buster der Hund);Sandra Schwarzhaupt (Gwendolyn die Katze);Joachim Kemmer (Tortellini der Hahn);Mario Adorf (Fred der Esel);Katharina Thalbach (die Erbin);Peer Augustinski (Dr. Gier);Klausj�rgen Wussow (Der Erz�hler);Hartmut Engler (Hund Buster);Bert Henry (Drehbuch);Georg Reichel (Drehbuch);Dagmar Kekule (Drehbuch);Peter Wolf (Musik);Dagmar Kekul� (Drehbuch)
      strb.Append("Mit: ");
      if (_actorCount < 1)
      {
        strb.Append(dbActors);
        strb.Append("\n");
      }
      else
      {
        string[] splitActors = dbActors.Split(';');
        if (splitActors != null && splitActors.Length > 0)
        {
          for (int i = 0 ; i < splitActors.Length ; i++)
          {
            if (i < _actorCount)
            {
              strb.Append(splitActors[i]);
              strb.Append("\n");
            }
            else
              break;
          }
        }
      }

      return strb.ToString();
    }

    private long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0;//(long)dt.Second;
        long iMin = (long)dt.Minute;
        long iHour = (long)dt.Hour;
        long iDay = (long)dt.Day;
        long iMonth = (long)dt.Month;
        long iYear = (long)dt.Year;

        long lRet = (iYear);
        lRet = lRet * 100L + iMonth;
        lRet = lRet * 100L + iDay;
        lRet = lRet * 100L + iHour;
        lRet = lRet * 100L + iMin;
        lRet = lRet * 100L + iSec;
        return lRet;
      }
      catch (Exception)
      {
      }
      return 0;
    }

    private void ClearPrograms(string channel)
    {
      Channel progChannel = null;
      IList allChannels = Channel.ListAll();
      foreach (Channel ch in allChannels)
      {
        if (ch.DisplayName == channel)
        {
          progChannel = ch;
          break;
        }
      }

      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Delete, typeof(Program));
      sb.AddConstraint(String.Format("idChannel = '{0}'", progChannel.IdChannel));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public void LaunchTVMUpdater()
    {
      string UpdaterPath = Path.Combine(TVMovieProgramPath, @"tvuptodate.exe");
      if (File.Exists(UpdaterPath))
      {
        Stopwatch BenchClock = new Stopwatch();

        try
        {
          BenchClock.Start();

          // check whether e.g. tv movie itself already started an update
          Process[] processes = Process.GetProcessesByName("tvuptodate");
          if (processes.Length > 0)
          {
            processes[0].WaitForExit(600000);
            BenchClock.Stop();
            Log.Info("TVMovie: tvuptodate was already running - waited {0} seconds for internet update to finish", Convert.ToString((BenchClock.ElapsedMilliseconds / 1000)));
            return;
          }

          ProcessStartInfo startInfo = new ProcessStartInfo("tvuptodate.exe");
          //startInfo.Arguments = "";
          startInfo.FileName = UpdaterPath;
          startInfo.WindowStyle = ProcessWindowStyle.Normal;
          startInfo.WorkingDirectory = Path.GetDirectoryName(UpdaterPath);

          Process UpdateProcess = Process.Start(startInfo);
          //UpdateProcess.PriorityBoostEnabled = true;

          UpdateProcess.WaitForExit(600000); // do not wait longer than 10 minutes for the internet update

          BenchClock.Stop();
          Log.Info("TVMovie: tvuptodate finished internet update in {0} seconds", Convert.ToString((BenchClock.ElapsedMilliseconds / 1000)));
        }
        catch (Exception ex)
        {
          BenchClock.Stop();
          Log.Error("TVMovie: LaunchTVMUpdater failed: {0}", ex.Message);
        }
      }
      else
        Log.Info("TVMovie: tvuptodate.exe not found in default location: {0}", UpdaterPath);
    }
    #endregion

  } // class
}