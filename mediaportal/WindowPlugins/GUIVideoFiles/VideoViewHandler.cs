#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Video.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for VideoViewHandler.
  /// </summary>
  public class VideoViewHandler : ViewHandler
  {
    private readonly string defaultVideoViews = Path.Combine(DefaultsDirectory, "VideoViews.xml");
    private readonly string customVideoViews = Config.GetFile(Config.Dir.Config, "VideoViews.xml");

    public VideoViewHandler()
    {
      if (!File.Exists(customVideoViews))
      {
        File.Copy(defaultVideoViews, customVideoViews);
      }

      try
      {
        using (FileStream fileStream = new FileInfo(customVideoViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          ArrayList viewlist = (ArrayList)formatter.Deserialize(fileStream);
          foreach (ViewDefinition view in viewlist)
          {
            views.Add(view);
          }
          fileStream.Close();
        }
      }
      catch (Exception) {}
    }

    public void Select(IMDBMovie movie)
    {
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldIdValue(movie, definition.Where).ToString();
      if (currentLevel + 1 < currentView.Filters.Count)
      {
        currentLevel++;
      }
    }

    public ArrayList Execute()
    {
      //build the query
      ArrayList movies = new ArrayList();
      string whereClause = string.Empty;
      string orderClause = string.Empty;
      string fromClause = "movie,movieinfo,path";
      if (CurrentLevel > 0)
      {
        whereClause =
          "where movieinfo.idmovie=movie.idmovie and movie.idpath=path.idpath";
      }

      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause, ref fromClause);
      }
      BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

      //execute the query
      string sql;
      if (CurrentLevel == 0)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
        string table = GetTable(defRoot.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable);

        if (table == "actors")
        {
          sql = String.Format("select * from actors ");
          if (whereClause != string.Empty)
          {
            sql += "where " + whereClause;
          }
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, true, false, false);
        }
        else if (table == "genre")
        {
          sql = String.Format("select * from genre ");
          if (whereClause != string.Empty)
          {
            sql += "where " + whereClause;
          }
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, true);
        }
        else if (defRoot.Where == "year")
        {
          movies = new ArrayList();
          sql = String.Format("select distinct iYear from movieinfo ");
          
          SQLiteResultSet results = VideoDatabase.GetResults(sql);
          
          for (int i = 0; i < results.Rows.Count; i++)
          {
            IMDBMovie movie = new IMDBMovie();
            movie.Year = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "iYear")));
            movies.Add(movie);
          }
        }
        // Recently added
        else if (defRoot.Where == "recently added")
        {
          try
          {
            if (string.IsNullOrEmpty(defRoot.Restriction))
              defRoot.Restriction = "7";

            TimeSpan ts = new TimeSpan(Convert.ToInt32(defRoot.Restriction), 0, 0, 0);
            DateTime searchDate = DateTime.Today - ts;

            //whereClause = String.Format("where actors.idActor=movieinfo.idDirector and movieinfo.dateAdded >= '{0}'", 
            //                            searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            whereClause = String.Format("where movieinfo.dateAdded >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            sql = String.Format("select * from movieinfo {0} {1}", whereClause, orderClause);
            
            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false);
          }
          catch (Exception) { }
        }
        // Recently watched
        else if (defRoot.Where == "recently watched")
        {
          try
          {
            if (string.IsNullOrEmpty(defRoot.Restriction))
              defRoot.Restriction = "7";

            TimeSpan ts = new TimeSpan(Convert.ToInt32(defRoot.Restriction), 0, 0, 0);
            DateTime searchDate = DateTime.Today - ts;

            //whereClause = String.Format("where actors.idActor=movieinfo.idDirector and movieinfo.dateWatched >= '{0}'", 
            //                            searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            whereClause = String.Format("where movieinfo.dateWatched >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            
            sql = String.Format("select * from movieinfo {0} {1}", whereClause, orderClause);
            
            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false);
          }
          catch (Exception) { }
        }
        else
        {
          whereClause =
            "where movieinfo.idmovie=movie.idmovie and movie.idpath=path.idpath";
          
          BuildRestriction(defRoot, ref whereClause);
          
          sql = String.Format("select * from {0} {1} {2}",
                              fromClause, whereClause, orderClause);
          
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, true);
        }
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        
        string table = GetTable(defCurrent.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable);
        
        sql = String.Format("select distinct {0}.* {1} {2} {3}",
                            table, fromClause, whereClause, orderClause);
        
        VideoDatabase.GetMoviesByFilter(sql, out movies, useActorsTable, useMovieInfoTable, useGenreTable);
      }
      else
      {
        sql =
          String.Format(
            "select movieinfo.fRating,movieinfo.strCredits,movieinfo.strDirector,movieinfo.strTagLine,movieinfo.strPlotOutline,movieinfo.strPlot,movieinfo.strVotes,movieinfo.strCast,movieinfo.iYear,movieinfo.strGenre,movieinfo.strPictureURL,movieinfo.strTitle,path.strPath,movie.discid,movieinfo.IMDBID,movieinfo.idMovie,path.cdlabel,movieinfo.mpaa,movieinfo.runtime,movieinfo.iswatched, movieinfo.strUserReview, movieinfo.studios from {0} {1} {2}",
            fromClause, whereClause, orderClause);
        
        VideoDatabase.GetMoviesByFilter(sql, out movies, true, true, true);
      }
      return movies;
    }

    private void BuildSelect(FilterDefinition filter, ref string whereClause, ref string fromClause)
    {
      if (whereClause != "")
      {
        whereClause += " and ";
      }
      whereClause += String.Format(" {0}='{1}'", GetFieldId(filter.Where), filter.SelectedValue);

      bool useMovieInfoTable = false;
      bool useAlbumTable = false;
      bool useActorsTable = false;
      bool useGenreTable = false;
      string table = GetTable(filter.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                              ref useGenreTable);
      if (useGenreTable)
      {
        fromClause += String.Format(",genre,genrelinkmovie");
        whereClause += " and genre.idGenre=genrelinkMovie.idGenre and genrelinkMovie.idMovie=movieinfo.idMovie";
      }
      if (useActorsTable)
      {
        fromClause += String.Format(",actors castactors,actorlinkmovie");
        whereClause += " and castactors.idActor=actorlinkmovie.idActor and actorlinkmovie.idMovie=movieinfo.idMovie";
      }
    }

    private void BuildRestriction(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator != string.Empty && filter.Restriction != string.Empty)
      {
        if (whereClause != "")
        {
          whereClause += " and ";
        }
        string restriction = filter.Restriction;
        restriction = restriction.Replace("*", "%");
        DatabaseUtility.RemoveInvalidChars(ref restriction);
        if (filter.SqlOperator == "=")
        {
          bool isascii = false;
          for (int x = 0; x < restriction.Length; ++x)
          {
            if (!Char.IsDigit(restriction[x]))
            {
              isascii = true;
              break;
            }
          }
          if (isascii)
          {
            filter.SqlOperator = "like";
          }
        }
        whereClause += String.Format(" {0} {1} '{2}'", GetFieldName(filter.Where), filter.SqlOperator, restriction);
      }
    }

    private void BuildWhere(FilterDefinition filter, ref string whereClause)
    {
      if (filter.WhereValue != "*")
      {
        if (whereClause != "")
        {
          whereClause += " and ";
        }
        whereClause += String.Format(" {0}='{1}'", GetField(filter.Where), filter.WhereValue);
      }
    }

    private void BuildOrder(FilterDefinition filter, ref string orderClause)
    {
      orderClause = " order by " + GetField(filter.Where) + " ";
      if (!filter.SortAscending)
      {
        orderClause += "desc";
      }
      else
      {
        orderClause += "asc";
      }
      if (filter.Limit > 0)
      {
        orderClause += String.Format(" Limit {0}", filter.Limit);
      }
    }

    private string GetTable(string where, ref bool useMovieInfoTable, ref bool useAlbumTable, ref bool useActorsTable,
                            ref bool useGenreTable)
    {
      if (where == "actor")
      {
        useActorsTable = true;
        return "actors";
      }
      if (where == "title")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "genre")
      {
        useGenreTable = true;
        return "genre";
      }
      if (where == "year")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "rating")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "recently added")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "recently watched")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      return null;
    }

    private string GetField(string where)
    {
      if (where == "watched")
      {
        return "iswatched";
      }
      if (where == "actor")
      {
        return "strActor";
      }
      if (where == "title")
      {
        return "strTitle";
      }
      if (where == "genre")
      {
        return "strGenre";
      }
      if (where == "year")
      {
        return "iYear";
      }
      if (where == "rating")
      {
        return "fRating";
      }
      if (where == "recently added")
      {
        return "dateAdded";
      }
      if (where == "recently watched")
      {
        return "dateWatched";
      }
      return null;
    }

    private string GetFieldId(string where)
    {
      if (where == "watched")
      {
        return "movieinfo.idMovie";
      }
      if (where == "actor")
      {
        return "castactors.idActor";
      }
      if (where == "title")
      {
        return "movieinfo.idMovie";
      }
      if (where == "genre")
      {
        return "genre.idGenre";
      }
      if (where == "year")
      {
        return "movieinfo.iYear";
      }
      if (where == "rating")
      {
        return "movieinfo.fRating";
      }
      if (where == "recently added")
      {
        return "movieinfo.idMovie";
      }
      if (where == "recently watched")
      {
        return "movieinfo.idMovie";
      }
      return null;
    }

    private string GetFieldName(string where)
    {
      if (where == "watched")
      {
        return "movieinfo.iswatched";
      }
      if (where == "actor")
      {
        return "actor.strActor";
      }
      if (where == "title")
      {
        return "movieinfo.strTitle";
      }
      if (where == "genre")
      {
        return "genre.strGenre";
      }
      if (where == "year")
      {
        return "movieinfo.iYear";
      }
      if (where == "rating")
      {
        return "movieinfo.fRating";
      }
      if (where == "recently added")
      {
        return "movieinfo.dateAdded";
      }
      if (where == "recently watched")
      {
        return "movieinfo.dateWatched";
      }
      return null;
    }

    private int GetFieldIdValue(IMDBMovie movie, string where)
    {
      if (where == "watched")
      {
        return (int)movie.Watched;
      }
      if (where == "actor")
      {
        return movie.ActorID;
      }
      if (where == "title")
      {
        return movie.ID;
      }
      if (where == "genre")
      {
        return movie.GenreID;
      }
      if (where == "year")
      {
        return movie.Year;
      }
      if (where == "rating")
      {
        return (int)movie.Rating;
      }
      if (where == "recently added")
      {
        return movie.ID; ;
      }
      if (where == "recently watched")
      {
        return movie.ID; ;
      }
      return -1;
    }

    public void SetLabel(IMDBMovie movie, ref GUIListItem item)
    {
      if (movie == null)
      {
        return;
      }
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      if (definition.Where == "genre")
      {
        item.Label = movie.SingleGenre;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "actor")
      {
        item.Label = movie.Actor;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "year")
      {
        item.Label = movie.Year.ToString();
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "recently added")
      {
        item.Label = movie.Title;
        item.Label2 = Convert.ToDateTime(movie.DateAdded).ToShortDateString() + " " +
                      Convert.ToDateTime(movie.DateAdded).ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        //item.Label3 = string.Empty;          // Watched percentage is here
      }
      if (definition.Where == "recently watched")
      {
        item.Label = movie.Title;
        item.Label2 = Convert.ToDateTime(movie.DateWatched).ToShortDateString() + " " +
                      Convert.ToDateTime(movie.DateWatched).ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        //item.Label3 = string.Empty;          // Watched percentage is here
      }
    }

    protected override string GetLocalizedViewLevel(string lvlName)
    {
      string localizedLevelName = string.Empty;
      
      switch(lvlName)
      {          
        case "actor":
          localizedLevelName = GUILocalizeStrings.Get(344);
          break;
        case "genre":
          localizedLevelName = GUILocalizeStrings.Get(135);
          break;
        case "year":
          localizedLevelName = GUILocalizeStrings.Get(987);
          break;      
        case "watched":
        case "title":
        case "rating":
          localizedLevelName = GUILocalizeStrings.Get(342);
          break;
        default:
          localizedLevelName = lvlName;
          break;
      }
      
      return localizedLevelName;
    } 

  }
}