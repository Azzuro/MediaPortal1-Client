using System;
using System.Collections;
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicGenres: GUIMusicBaseWindow
  { 
    #region Base variabeles

    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory=String.Empty;
    int               m_iItemSelected=-1;   
		VirtualDirectory  m_directory = new VirtualDirectory();
		int[]  views      = new int[50];
		bool[] sortasc    = new bool[50];
		int[] sortby      = new int[50];


		[SkinControlAttribute(9)]			protected GUIButtonControl btnSearch=null;
    #endregion

    public GUIMusicGenres()
    {
			for (int i=0; i < sortasc.Length;++i)
				sortasc[i]=true;
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
    }

		#region overrides
    public override bool Init()
    {
      m_strDirectory=String.Empty;
			handler.CurrentView="Artists";
      return Load (GUIGraphicsContext.Skin+@"\mymusicgenres.xml");
    }
		protected override string SerializeName
		{
			get
			{
				return "mymusic"+handler.CurrentView;
			}
		}

		protected override View CurrentView
		{
			get
			{
				return (View)views[handler.CurrentLevel];
			}
			set
			{
				views[handler.CurrentLevel] = (int)value;
			}
		}

		protected override bool CurrentSortAsc
		{
			get
			{
				return sortasc[handler.CurrentLevel];
			}
			set
			{
				sortasc[handler.CurrentLevel]=value;
			}
		}
		protected override MusicSort.SortMethod CurrentSortMethod
		{
			get
			{
				return (MusicSort.SortMethod)sortby[handler.CurrentLevel];
			}
			set
			{
				sortby[handler.CurrentLevel]=(int)value;
			}
		}
		protected override bool AllowView(View view)
		{
			return true;
		}

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];
        if (item!=null)
        {
          if (item.IsFolder && item.Label=="..")
          {
						handler.CurrentLevel--;
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      base.OnAction(action);
    }

		protected override void OnPageLoad()
		{
			string view=MusicState.View;
			if (view==String.Empty)
				view=((ViewDefinition)handler.Views[0]).Name;

			handler.CurrentView = view;
			base.OnPageLoad ();
			LoadDirectory(m_strDirectory);
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected=facadeView.SelectedListItemIndex;
			if (GUIMusicFiles.IsMusicWindow(newWindowId))
			{
				MusicState.StartWindow=newWindowId;
			}
			base.OnPageDestroy (newWindowId);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == btnSearch)
			{
				int activeWindow=(int)GUIWindowManager.ActiveWindow;
				VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
				keyBoard.Text = String.Empty;
				keyBoard.Reset();
				keyBoard.TextChanged+=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
				keyBoard.DoModal(activeWindow); // show it...
				keyBoard.TextChanged-=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
				System.GC.Collect(); // collect some garbage
			}

			base.OnClicked (controlId, control, actionType);
		}

		protected override void OnRetrieveCoverArt(GUIListItem item)
		{
			if (item.Label=="..") return;
			Utils.SetDefaultIcons(item);
			if (item.IsRemote) return;
			Song song = item.AlbumInfoTag as Song;
			if (song==null) return;
			if (song.genreId>=0 && song.albumId<0 && song.artistId<0 && song.songId<0)
			{
				string strThumb=Utils.GetCoverArt(Thumbs.MusicGenre,item.Label);
				if (System.IO.File.Exists(strThumb))
				{
					item.IconImage=strThumb;
					item.IconImageBig=strThumb;
					item.ThumbnailImage=strThumb;
				}
			}
			else if (song.artistId>=0 && song.albumId<0 && song.songId<0)
			{
				string strThumb=Utils.GetCoverArt(Thumbs.MusicArtists,item.Label);
				if (System.IO.File.Exists(strThumb))
				{
					item.IconImage=strThumb;
					item.IconImageBig=strThumb;
					item.ThumbnailImage=strThumb;
				}
			}
			else if (song.albumId>=0)
			{
				MusicTag tag = item.MusicTag as MusicTag;
				string strThumb=GUIMusicFiles.GetAlbumThumbName(tag.Artist,tag.Album);
				if (System.IO.File.Exists(strThumb))
				{
					item.IconImage=strThumb;
					item.IconImageBig=strThumb;
					item.ThumbnailImage=strThumb;
				}
			}
			else
			{
				base.OnRetrieveCoverArt(item);
			}
		}


		
		protected override void OnClick(int iItem)
		{
			GUIListItem item = facadeView.SelectedListItem;
			if (item==null) return;
			if (item.IsFolder)
			{
				m_iItemSelected=-1;
				if (item.Label=="..")
					handler.CurrentLevel--;
				else
					handler.Select(item.AlbumInfoTag as Song);
				LoadDirectory(item.Path);
			}
			else
			{
				// play item
				//play and add current directory to temporary playlist
				int nFolderCount=0;
				PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP ).Clear();
				PlayListPlayer.Reset();
				for ( int i = 0; i < (int) facadeView.Count; i++ ) 
				{
					GUIListItem pItem=facadeView[i];
					if ( pItem.IsFolder ) 
					{
						nFolderCount++;
						continue;
					}
					PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
					playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName=pItem.Path;
					playlistItem.Description=pItem.Label;
					int iDuration=0;
					MusicTag tag=(MusicTag)pItem.MusicTag;
					if (tag!=null) iDuration=tag.Duration;
					playlistItem.Duration=iDuration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}

				//	Save current window and directory to know where the selected item was
				MusicState.TempPlaylistWindow=GetID;
				MusicState.TempPlaylistDirectory=m_strDirectory;

				PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem-nFolderCount);
			}
		}
    


		protected override  void OnShowContextMenu()
		{
			GUIListItem item=facadeView.SelectedListItem;
			int itemNo=facadeView.SelectedListItemIndex;
			if (item==null) return;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu

			dlg.Add( GUILocalizeStrings.Get(208)); //play
			dlg.Add( GUILocalizeStrings.Get(926)); //Queue
			dlg.Add( GUILocalizeStrings.Get(136)); //PlayList
			if (!item.IsFolder && !item.IsRemote)
			{
				Song song = item.AlbumInfoTag as Song;
				if (song.songId>=0)
				{
					dlg.AddLocalizedString(930); //Add to favorites
					dlg.AddLocalizedString(931); //Rating
				}
			}

			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			switch (dlg.SelectedLabel)
			{
				case 0: // play
					OnClick(itemNo);	
					break;
					
				case 1: // add to playlist
					OnQueueItem(itemNo);	
					break;
					
				case 2: // show playlist
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
					break;
				case 3: // add to favorites
					AddSongToFavorites(item);
					break;
				case 4:// Rating
					OnSetRating(facadeView.SelectedListItemIndex);
					break;
			}
		}

		protected override  void OnQueueItem(int iItem)
		{
			// add item 2 playlist
			GUIListItem pItem=facadeView[iItem];
			if (handler.CurrentLevel < handler.MaxLevels-1)
			{
				//queue
				handler.Select(pItem.AlbumInfoTag as Song);
				ArrayList songs = handler.Execute();
				handler.CurrentLevel--;
				foreach (Song song in songs)
				{
					if (song.songId>0)
					{
						GUIListItem item = new GUIListItem();
						item.Path=song.FileName;
						item.Label=song.Title;
						item.Duration=song.Duration;
						item.IsFolder=false;
						AddItemToPlayList(item);
					}
				}
			}
			else
			{
				AddItemToPlayList(pItem);
			}
			
			//move to next item and start playing
			GUIControl.SelectItemControl(GetID, facadeView.GetID,iItem+1);
			if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count > 0 &&  !g_Player.Playing)
			{
				PlayListPlayer.Reset();
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.Play(0);
			}
		}
		#endregion

	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayGeneresList(kindOfSearch,data);
	  }
    
		
    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIListItem SelectedItem = facadeView.SelectedListItem;
      if (SelectedItem!=null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label!="..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      m_strDirectory=strNewDirectory;
			
      GUIControl.ClearControl(GetID,facadeView.GetID);
            
      string strObjects=String.Empty;

			ArrayList itemlist = new ArrayList();
			ArrayList songs=handler.Execute();
			if (handler.CurrentLevel>0)
			{
				GUIListItem pItem = new GUIListItem ("..");
				pItem.Path=String.Empty;
				pItem.IsFolder=true;
				Utils.SetDefaultIcons(pItem);
				itemlist.Add(pItem);
			}
			foreach (Song song in songs)
			{
					GUIListItem item=new GUIListItem();
					item.Label=song.Title;
					if (handler.CurrentLevel+1 < handler.MaxLevels)
						item.IsFolder=true;
					else
						item.IsFolder=false;
					item.Path=song.FileName;
					item.Duration=song.Duration;
					
					MusicTag tag=new MusicTag();
					tag.Title=song.Title;
					tag.Album=song.Album;
					tag.Artist=song.Artist;
					tag.Duration=song.Duration;
					tag.Genre=song.Genre;
					tag.Track=song.Track;
					tag.Year=song.Year;
					tag.Rating=song.Rating;
          item.Duration = tag.Duration;
          item.Rating = song.Rating;
          item.Year=song.Year
					item.AlbumInfoTag = song;
					item.MusicTag=tag;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
					itemlist.Add(item);
			}
     

      
      string strSelectedItem=m_history.Get(m_strDirectory);	
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
				facadeView.Add(item);
      }

      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			SetLabels();
      OnSort();
      for (int i=0; i< facadeView.Count;++i)
      {
        GUIListItem item =facadeView[i];
        if (item.Label==strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID,facadeView.GetID,iItem);
          break;
        }
        iItem++;
      }
      if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
      }
			SwitchView();
		}
	  
	  void DisplayGeneresList(int searchKind,string searchText)
	  {
		  GUIControl.ClearControl(GetID,facadeView.GetID);
            
		  string strObjects=String.Empty;

		
		  ArrayList itemlist=new ArrayList();
		  ArrayList genres=new ArrayList();
		  m_database.GetGenres(searchKind,searchText,ref genres);
			  foreach(string strGenre in genres)
			  {
				  GUIListItem item=new GUIListItem();
				  item.Label=strGenre;
				  item.Path=strGenre;
				  item.IsFolder=true;
				  string strThumb=Utils.GetCoverArt(Thumbs.MusicGenre,item.Label);
				  item.IconImage=strThumb;
				  item.IconImageBig=strThumb;
				  item.ThumbnailImage=strThumb;

				  Utils.SetDefaultIcons(item);
				  itemlist.Add(item);
			  }

		  //
		  m_history.Set(m_strDirectory, m_strDirectory); //save where we are
		  GUIListItem dirUp=new GUIListItem("..");
		  dirUp.Path=m_strDirectory; // to get where we are
		  dirUp.IsFolder=true;
		  dirUp.ThumbnailImage=String.Empty;
		  dirUp.IconImage="defaultFolderBack.png";
		  dirUp.IconImageBig="defaultFolderBackBig.png";
		  itemlist.Insert(0,dirUp);
		  //

		  foreach (GUIListItem item in itemlist)
		  {
				facadeView.Add(item);
		  }

		  int iTotalItems=itemlist.Count;
		  if (itemlist.Count>0)
		  {
			  GUIListItem rootItem=(GUIListItem)itemlist[0];
			  if (rootItem.Label=="..") iTotalItems--;
		  }
		  strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
		  GUIPropertyManager.SetProperty("#itemcount",strObjects);
		  SetLabels();
		  OnSort();

	  }

		protected override void SetLabels()
		{
			base.SetLabels ();
			for (int i=0; i < facadeView.Count;++i)
			{
				GUIListItem item=facadeView[i];
				handler.SetLabel(item.AlbumInfoTag as Song, ref item);
			}
		}

    void AddItemToPlayList(GUIListItem pItem) 
    {
      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..") return;
        string strDirectory=m_strDirectory;
        m_strDirectory=pItem.Path;
		    
        ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
      }
      else
      {
        //TODO
        if (Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          playlistItem.Duration=pItem.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
    }
  }
}
