#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Threading;
using SQLite.NET;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.Music.Database;


namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  /// 


  public class MusicDatabaseReorg
  {
    #region Enums
    //	return codes of ReorgDatabase
    //	numbers are strings from strings.xml
    enum Errors
    {
      ERROR_OK = 317,
      ERROR_CANCEL = 0,
      ERROR_DATABASE = 315,
      ERROR_REORG_SONGS = 319,
      ERROR_REORG_ARTIST = 321,
      ERROR_REORG_ALBUMARTIST = 322,
      ERROR_REORG_GENRE = 323,
      ERROR_REORG_PATH = 325,
      ERROR_REORG_ALBUM = 327,
      ERROR_WRITING_CHANGES = 329, 
      ERROR_COMPRESSING = 332
    }
    #endregion

    #region Variables
    MusicDatabase m_dbs = new MusicDatabase();
    ArrayList m_pathids = new ArrayList();
    ArrayList m_shares = new ArrayList();

    int _parentWindowID = 0;
    #endregion

    #region CTOR
    public MusicDatabaseReorg()
    {
    }

    public MusicDatabaseReorg(int Id)
    {
      _parentWindowID = Id;
    }
    #endregion

    #region Implementation
    void SetPercentDonebyEvent(object sender, DatabaseReorgEventArgs e)
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress) return;
      pDlgProgress.SetPercentage(e.progress);
      pDlgProgress.SetLine(1, e.phase);
      pDlgProgress.Progress();
    }

    bool IsCanceled()
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress) return false;

      pDlgProgress.ProgressKeys();
      if (pDlgProgress.IsCanceled)
      {
        try
        {
          MusicDatabase.Instance.Execute("rollback");
        }
        catch (Exception)
        {
        }
        return true;
      }
      return false;
    }

    public int DoReorg()
    {
      /// Todo: move this statement to the GUI.
      /// Database Reorg now fully in music.database
      /// 

      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress) return (int)Errors.ERROR_REORG_SONGS;

      pDlgProgress.SetHeading(313);
      pDlgProgress.SetLine(2, "");
      pDlgProgress.SetLine(3, "");
      pDlgProgress.SetPercentage(0);
      pDlgProgress.Progress();
      pDlgProgress.SetLine(1, 316);
      pDlgProgress.ShowProgressBar(true);

      ///TFRO71 4 june 2005
      ///Connect the event to a method that knows what to do with the event.
      MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent);
      ///Execute the reorganisation
      int appel = m_dbs.MusicDatabaseReorg(null);
      ///Tfro Disconnect the event from the method.
      MusicDatabase.DatabaseReorgChanged -= new MusicDBReorgEventHandler(SetPercentDonebyEvent);

      pDlgProgress.SetLine(2, "Klaar");

      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Thread, which runs the reorg in background
    /// </summary>
    public void ReorgAsync()
    {
      m_dbs.MusicDatabaseReorg(null);
      GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (null != dlgNotify)
      {
        dlgNotify.SetHeading(GUILocalizeStrings.Get(313));
        dlgNotify.SetText(GUILocalizeStrings.Get(317));
        dlgNotify.DoModal(_parentWindowID);
      }
    }

    public void DeleteAlbumInfo()
    {
      // CMusicDatabaseReorg is friend of CMusicDatabase

      // use the same databaseobject as CMusicDatabase
      // to rollback transactions even if CMusicDatabase
      // memberfunctions are called; create our working dataset

      SQLiteResultSet results;
      string strSQL;
      strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist order by album.strAlbum");
      results = MusicDatabase.Instance.Execute(strSQL);
      int iRowsFound = results.Rows.Count;
      if (iRowsFound == 0)
      {
        GUIDialogOK pDlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlg != null)
        {
          pDlg.SetHeading(313);
          pDlg.SetLine(1, 425);
          pDlg.SetLine(2, "");
          pDlg.SetLine(3, "");
          pDlg.DoModal(GUIWindowManager.ActiveWindow);
        }
        return;
      }
      ArrayList vecAlbums = new ArrayList();
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        MusicDatabase.AlbumInfoCache album = new MusicDatabase.AlbumInfoCache();
        album.idAlbum = Int32.Parse(DatabaseUtility.Get(results, i, "album.idAlbum"));
        album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
        album.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
        vecAlbums.Add(album);
      }

      //	Show a selectdialog that the user can select the albuminfo to delete 
      string szText = GUILocalizeStrings.Get(181);
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      if (pDlgSelect != null)
      {
        pDlgSelect.SetHeading(szText);
        pDlgSelect.Reset();
        foreach (MusicDatabase.AlbumInfoCache album in vecAlbums)
        {
          pDlgSelect.Add(album.Album + " - " + album.Artist);
        }
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        int iSelectedAlbum = pDlgSelect.SelectedLabel;
        if (iSelectedAlbum < 0)
        {
          vecAlbums.Clear();
          return;
        }

        MusicDatabase.AlbumInfoCache albumDel = (MusicDatabase.AlbumInfoCache)vecAlbums[iSelectedAlbum];
        strSQL = String.Format("delete from albuminfo where albuminfo.idAlbum={0}", albumDel.idAlbum);
        MusicDatabase.Instance.Execute(strSQL);


        vecAlbums.Clear();
      }
    }



    public void DeleteSingleAlbum()
    {
      // CMusicDatabaseReorg is friend of CMusicDatabase

      // use the same databaseobject as CMusicDatabase
      // to rollback transactions even if CMusicDatabase
      // memberfunctions are called; create our working dataset

      ArrayList m_songids = new ArrayList();
      ArrayList m_albumids = new ArrayList();
      ArrayList m_artistids = new ArrayList();
      ArrayList m_genreids = new ArrayList();
      ArrayList m_albumnames = new ArrayList();

      string strSQL;
      SQLiteResultSet results;
      strSQL = String.Format("select * from album,albumartist where album.idAlbumArtist=albumartist.idAlbumArtist order by album.strAlbum");
      results = MusicDatabase.Instance.Execute(strSQL);
      int iRowsFound = results.Rows.Count;
      if (iRowsFound == 0)
      {
        GUIDialogOK pDlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (null != pDlg)
        {
          pDlg.SetHeading(313);
          pDlg.SetLine(1, 425);
          pDlg.SetLine(2, "");
          pDlg.SetLine(3, "");
          pDlg.DoModal(GUIWindowManager.ActiveWindow);
        }
        return;
      }
      ArrayList vecAlbums = new ArrayList();
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        MusicDatabase.AlbumInfoCache album = new MusicDatabase.AlbumInfoCache();
        album.idAlbum = Int32.Parse(DatabaseUtility.Get(results, i, "album.idAlbum"));
        album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
        album.AlbumArtist = DatabaseUtility.Get(results, i, "albumartist.strAlbumArtist");
        vecAlbums.Add(album);
      }

      //	Show a selectdialog that the user can select the album to delete 
      string szText = GUILocalizeStrings.Get(181);
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      if (null != pDlgSelect)
      {
        pDlgSelect.SetHeading(szText);
        pDlgSelect.Reset();
        foreach (MusicDatabase.AlbumInfoCache album in vecAlbums)
        {
          pDlgSelect.Add(album.Album + " - " + album.AlbumArtist);
        }
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        int iSelectedAlbum = pDlgSelect.SelectedLabel;
        if (iSelectedAlbum < 0)
        {
          vecAlbums.Clear();
          return;
        }

        MusicDatabase.AlbumInfoCache albumDel = (MusicDatabase.AlbumInfoCache)vecAlbums[iSelectedAlbum];
        //	Delete album
        strSQL = String.Format("delete from album where idAlbum={0}", albumDel.idAlbum);
        MusicDatabase.Instance.Execute(strSQL);

        //	Delete album info
        strSQL = String.Format("delete from albuminfo where idAlbum={0}", albumDel.idAlbum);
        MusicDatabase.Instance.Execute(strSQL);

        //	Get the songs of the album
        strSQL = String.Format("select * from song where idAlbum={0}", albumDel.idAlbum);
        results = MusicDatabase.Instance.Execute(strSQL);
        iRowsFound = results.Rows.Count;
        if (iRowsFound != 0)
        {
          //	Get all artists of this album
          m_artistids.Clear();
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            m_artistids.Add(Int32.Parse(DatabaseUtility.Get(results, i, "idArtist")));
          }

          //	Do we have another song of this artist?
          foreach (int iID in m_artistids)
          {
            strSQL = String.Format("select * from song where idArtist={0} and idAlbum<>{1}", iID, albumDel.idAlbum);
            results = MusicDatabase.Instance.Execute(strSQL);
            iRowsFound = results.Rows.Count;
            if (iRowsFound == 0)
            {
              //	No, delete the artist
              strSQL = String.Format("delete from artist where idArtist={0}", iID);
              MusicDatabase.Instance.Execute(strSQL);
            }
          }
          m_artistids.Clear();
        }

        //	Delete the albums songs
        strSQL = String.Format("delete from song where idAlbum={0}", albumDel.idAlbum);
        MusicDatabase.Instance.Execute(strSQL);

        // Delete album thumb

      }
    }
    #endregion
  }
}
