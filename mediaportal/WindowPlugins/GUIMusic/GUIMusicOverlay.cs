using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

using MediaPortal.Radio.Database;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.TV.Recording;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1. 
  /// </summary>
  public class GUIMusicOverlay: GUIWindow
  {
    string m_strFile="";
    int                 m_iFrames=0;
    int                 m_iPosOrgIcon=0;
    int                 m_iPosOrgPlay=0;
    int                 m_iPosOrgPause=0;
    int                 m_iPosOrgInfo=0;
    int                 m_iPosOrgBigPlayTime=0;
    int                 m_iPosOrgPlayTime=0;
    int                 m_iPosOrgRectangle=0;
    int                 m_iPosXRect=0;
    int                 m_iPosYRect=0;
    int                 m_iFrame=0;
		string							m_strThumb="";
    enum Controls
    {
      CONTROL_LOGO_RECT    =0
      , CONTROL_LOGO_PIC    =1
      , CONTROL_PLAYTIME	=	2
      , CONTROL_PLAY_LOGO  = 3
      , CONTROL_PAUSE_LOGO = 4
      , CONTROL_INFO			  =5
      , CONTROL_BIG_PLAYTIME= 6
      , CONTROL_FF_LOGO  =7
      , CONTROL_RW_LOGO  =8
    }

    public GUIMusicOverlay()
		{
			GetID=(int)GUIWindow.Window.WINDOW_MUSIC_OVERLAY;
    }

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\musicOverlay.xml");
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    public override void PreInit()
    {
      AllocResources();
      m_iPosOrgIcon=0;    
    }
    void SetPosition(int iControl, int iStep, int iSteps,int iOrgPos)
    {
      int iScreenHeight=10+GUIGraphicsContext.Height;
      float fDiff=(float)iScreenHeight-(float)iOrgPos;
      float fPos = fDiff / ((float)iSteps);
      fPos *= -((float)iStep);
      fPos += (float)iScreenHeight;
      GUIControl pControl=(GUIControl)GetControl(iControl);
      if (pControl!=null) pControl.SetPosition(pControl.XPosition, (int)fPos);
    }
    int GetControlYPosition(int iControl)
    {
      GUIControl pControl=(GUIControl)GetControl(iControl);
      if (null==pControl) return 0;
      return pControl.YPosition;
    }
    int GetControlXPosition(int iControl)
    {
      GUIControl pControl=(GUIControl)GetControl(iControl);
      if (null==pControl) return 0;
      return pControl.XPosition;
    }

    
    public override void Render(float timePassed)
    {
    }
    
    public override bool DoesPostRender()
    {
      if (!g_Player.Playing &&!Recorder.IsRadio()) 
      { 
        m_strFile=String.Empty;
        return false;
      }

      if (!g_Player.IsRadio && !g_Player.IsMusic &&!Recorder.IsRadio()) 
			{
				m_strFile=String.Empty;
        return false;
      }
      if (GUIGraphicsContext.IsFullScreenVideo) return false;
      if (!GUIGraphicsContext.Overlay) return false;
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer!=2) return;
      GUIFadeLabel fader =(GUIFadeLabel)GetControl((int)Controls.CONTROL_INFO);
      if (fader!=null)
      {
        fader.AllowScrolling=true;
      }
      if (GUIGraphicsContext.Overlay==false) 
      {

        return;
      }
			
			if (GUIPropertyManager.GetProperty("#thumb") != m_strThumb)
			{
				m_strFile=g_Player.CurrentFile ;
				SetCurrentFile(m_strFile);
			}
			if (g_Player.Playing && g_Player.CurrentFile != m_strFile)
      {
        m_iFrames=0;
        m_strFile=g_Player.CurrentFile ;
        SetCurrentFile(m_strFile);
      }
			if (Recorder.IsRadio() && Recorder.RadioStationName()!=m_strFile)
			{
				m_strFile=Recorder.RadioStationName() ;
				SetCurrentFile(m_strFile);
			}

      if (m_iPosOrgIcon==0)
      {
        m_iPosXRect=GetControlXPosition((int)Controls.CONTROL_LOGO_RECT);
        m_iPosYRect=GetControlYPosition((int)Controls.CONTROL_LOGO_RECT);
        m_iPosOrgRectangle=GetControlYPosition((int)Controls.CONTROL_LOGO_RECT);
        m_iPosOrgIcon=GetControlYPosition((int)Controls.CONTROL_LOGO_PIC);
        m_iPosOrgPlay=GetControlYPosition((int)Controls.CONTROL_PLAY_LOGO);
        m_iPosOrgPause=GetControlYPosition((int)Controls.CONTROL_PAUSE_LOGO);
        m_iPosOrgInfo=GetControlYPosition((int)Controls.CONTROL_INFO);
        m_iPosOrgPlayTime=GetControlYPosition((int)Controls.CONTROL_PLAYTIME);
        m_iPosOrgBigPlayTime=GetControlYPosition((int)Controls.CONTROL_BIG_PLAYTIME);
      }
      int iSteps=25;
      if ( GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_VISUALISATION)
      {
        SetPosition(0, 50,50,m_iPosOrgRectangle);
        SetPosition((int)Controls.CONTROL_LOGO_PIC, 50,50,m_iPosOrgIcon);
        SetPosition((int)Controls.CONTROL_PLAY_LOGO, 50,50,m_iPosOrgPlay);
        SetPosition((int)Controls.CONTROL_PAUSE_LOGO, 50,50,m_iPosOrgPause);
        SetPosition((int)Controls.CONTROL_FF_LOGO, 50,50,m_iPosOrgPause);
        SetPosition((int)Controls.CONTROL_RW_LOGO, 50,50,m_iPosOrgPause);
        SetPosition((int)Controls.CONTROL_INFO, 50,50,m_iPosOrgInfo);
        SetPosition((int)Controls.CONTROL_PLAYTIME, 50,50,m_iPosOrgPlayTime);
        SetPosition((int)Controls.CONTROL_BIG_PLAYTIME, 50,50,m_iPosOrgBigPlayTime);
        m_iFrames=0;
      }
      else
      {
        if (m_iFrames < iSteps)
        {
          // scroll up
          SetPosition(0, m_iFrames,iSteps,m_iPosOrgRectangle);
          SetPosition((int)Controls.CONTROL_LOGO_PIC, m_iFrames,iSteps,m_iPosOrgIcon);
          SetPosition((int)Controls.CONTROL_PLAY_LOGO, m_iFrames,iSteps,m_iPosOrgPlay);
          SetPosition((int)Controls.CONTROL_PAUSE_LOGO, m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_FF_LOGO, m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_RW_LOGO, m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_INFO, m_iFrames,iSteps,m_iPosOrgInfo);
          SetPosition((int)Controls.CONTROL_PLAYTIME, m_iFrames,iSteps,m_iPosOrgPlayTime);
          SetPosition((int)Controls.CONTROL_BIG_PLAYTIME, m_iFrames,iSteps,m_iPosOrgBigPlayTime);
          m_iFrames++;
        }
        else if (m_iFrames >=iSteps && m_iFrames<=5*iSteps+iSteps)
        {
          //show
          SetPosition(0, iSteps,iSteps,m_iPosOrgRectangle);
          SetPosition((int)Controls.CONTROL_LOGO_PIC, iSteps,iSteps,m_iPosOrgIcon);
          SetPosition((int)Controls.CONTROL_PLAY_LOGO, iSteps,iSteps,m_iPosOrgPlay);
          SetPosition((int)Controls.CONTROL_PAUSE_LOGO, iSteps,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_FF_LOGO, iSteps,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_RW_LOGO, iSteps,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_INFO, iSteps,iSteps,m_iPosOrgInfo);
          SetPosition((int)Controls.CONTROL_PLAYTIME, iSteps,iSteps,m_iPosOrgPlayTime);
          SetPosition((int)Controls.CONTROL_BIG_PLAYTIME, iSteps,iSteps,m_iPosOrgBigPlayTime);
          m_iFrames++;
        }
        else if (m_iFrames >=5*iSteps+iSteps )
        {
          if (m_iFrames > 5*iSteps+2*iSteps) 
          {
            m_iFrames=5*iSteps+2*iSteps;
          }
          //scroll down
          SetPosition(0,                    5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgRectangle);
          SetPosition((int)Controls.CONTROL_LOGO_PIC,     5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgIcon);
          SetPosition((int)Controls.CONTROL_PLAY_LOGO,    5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgPlay);
          SetPosition((int)Controls.CONTROL_PAUSE_LOGO,   5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_FF_LOGO,   5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_RW_LOGO,   5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgPause);
          SetPosition((int)Controls.CONTROL_INFO,         5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgInfo);
          SetPosition((int)Controls.CONTROL_PLAYTIME,     5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgPlayTime);
          SetPosition((int)Controls.CONTROL_BIG_PLAYTIME, 5*iSteps+2*iSteps-m_iFrames,iSteps,m_iPosOrgBigPlayTime);
          m_iFrames++;
        }
      }

      long lPTS1=(long)(g_Player.CurrentPosition);
      int hh = (int)(lPTS1 / 3600) % 100;
      int mm = (int)((lPTS1 /  60) % 60);
      int ss = (int)((lPTS1 /   1) % 60);

      int iSpeed=g_Player.Speed;
      if (hh==0 && mm==0 && ss<5)
      {
        if (iSpeed < 1)
        {
          iSpeed=1;
          g_Player.Speed=iSpeed;
          g_Player.SeekAbsolute(0.0d);
        }
      }

      //msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, (int)Controls.CONTROL_PLAYTIME,0,0,null); 
      //msg.Label=strTime; 
      //OnMessage(msg);


      HideControl((int)Controls.CONTROL_PLAY_LOGO); 
      HideControl((int)Controls.CONTROL_PAUSE_LOGO); 
      HideControl( (int)Controls.CONTROL_FF_LOGO); 
      HideControl( (int)Controls.CONTROL_RW_LOGO); 
      if (g_Player.Paused )
      {
        ShowControl( (int)Controls.CONTROL_PAUSE_LOGO); 
      }
      else
      {
        iSpeed = g_Player.Speed;
        if (iSpeed > 1)
        {
          ShowControl( (int)Controls.CONTROL_FF_LOGO); 
        }
        else if (iSpeed < 0)
        {
          ShowControl( (int)Controls.CONTROL_RW_LOGO); 
        }
        else
        {
          ShowControl( (int)Controls.CONTROL_PLAY_LOGO); 
        }
      }
      float fx;
      float fy;


      if (g_Player.IsRadio)
      {
        HideControl( (int)Controls.CONTROL_PLAYTIME);
        HideControl( (int)Controls.CONTROL_BIG_PLAYTIME);
      }
      else
      {
        ShowControl( (int)Controls.CONTROL_PLAYTIME);
        ShowControl( (int)Controls.CONTROL_BIG_PLAYTIME);
      }

      string strThumb=(string )GUIPropertyManager.GetProperty("#thumb");


      GUIImage AlbumArtPicture = (GUIImage)GetControl((int)Controls.CONTROL_LOGO_PIC); 
      GUIImage SurroundingRectangle = (GUIImage)GetControl((int)Controls.CONTROL_LOGO_RECT); 
      // do we have album art?
      if (strThumb.Length==0)
      {
        // no then hide the album art picture
        if (AlbumArtPicture!=null) AlbumArtPicture.IsVisible=false;

        if (SurroundingRectangle!=null) 
        {
          // if no video or visualisation, then hide the surrounding rectangle also
          if (!g_Player.HasVideo) SurroundingRectangle.IsVisible=false;
          else
          {
            //else make surrounding rectangle visible
            SurroundingRectangle.SetPosition(m_iPosXRect,m_iPosYRect);

            // and position the video/visualisation in middle of the rectangle
            fx=AlbumArtPicture.XPosition;
            fy=AlbumArtPicture.YPosition;
            GUIGraphicsContext.Correct(ref fx, ref fy);
            GUIGraphicsContext.VideoWindow = new Rectangle((int)fx,(int)fy,AlbumArtPicture.Width,AlbumArtPicture.Height);
          }
        }
      }
      else
      {
        // ok, we have an album art picture!
        SurroundingRectangle.IsVisible=true;
        AlbumArtPicture.IsVisible=false;
        if (g_Player.HasVideo)
        {
          // if we have a video or visualisation then move surrounding rectangle by 20,20 pixels
          int xoff=20;
          int yoff=20;
          GUIGraphicsContext.ScaleHorizontal(ref xoff);
          GUIGraphicsContext.ScaleHorizontal(ref yoff);

          SurroundingRectangle.SetPosition(GUIGraphicsContext.OffsetX+  m_iPosXRect+xoff,GUIGraphicsContext.OffsetY+m_iPosYRect+yoff);
        }
        else
        {
          SurroundingRectangle.SetPosition(GUIGraphicsContext.OffsetX+m_iPosXRect,GUIGraphicsContext.OffsetY+m_iPosYRect);
        }
      }
      // if we have an album art picture
      if (strThumb.Length!=0)
      {
        try
        {
          base.RestoreControlPosition((int)Controls.CONTROL_LOGO_PIC);
          //SurroundingRectangle.Visible=true;
          
          // make album art visible
          AlbumArtPicture.IsVisible=true;
          AlbumArtPicture.FixedHeight = AlbumArtPicture.KeepAspectRatio;
          float fXPos=(float)AlbumArtPicture.XPosition;
          float fYPos=(float)AlbumArtPicture.YPosition;
          int iWidth=AlbumArtPicture.Width;
          int iHeight=AlbumArtPicture.Height;
          GUIGraphicsContext.Correct(ref fXPos,ref fYPos);

          // if we also have video or visualsation
          if (g_Player.HasVideo)
          {
            AlbumArtPicture.YPosition=(int)fYPos;
            int iStep=iWidth /15;
            if (m_iFrame < 15)
            {
              //slide in album art from left->right
              //and slide out the visualisation
              AlbumArtPicture.XPosition=(int)fXPos-GUIGraphicsContext.OffsetX;
              AlbumArtPicture.Width = iStep*m_iFrame;
              if ((AlbumArtPicture.Width<=0) || (m_iFrame==0)) AlbumArtPicture.Width=1;
              int x=(int)AlbumArtPicture.Width+AlbumArtPicture.XPosition+GUIGraphicsContext.OffsetX;
              int w=(iWidth-(x-(int)fXPos));
              GUIGraphicsContext.VideoWindow = new Rectangle((int)x,(int)fYPos,
                                                             (int)w,
                                                             (int)iHeight);
              AlbumArtPicture.DoUpdate();
              m_iFrame++;
            }
            else if (m_iFrame < 15+100)
            {
              //show the album art for 100 frames
              //and hide the visualisation
              m_iFrame++;
              GUIGraphicsContext.VideoWindow= new Rectangle(0,0,0,0);
            }
            else if (m_iFrame < 15+100+15)
            {
              //slide in the visualisation
              //and slide out the album art picture
              fXPos-=GUIGraphicsContext.OffsetX;
              int frame=m_iFrame-(15+100);
              AlbumArtPicture.XPosition=iStep*frame + (int)fXPos;
              AlbumArtPicture.Width = iWidth-(AlbumArtPicture.XPosition-(int)fXPos);
              
              GUIGraphicsContext.VideoWindow = new Rectangle((int)fXPos+GUIGraphicsContext.OffsetX,(int)fYPos,
                                                             (int)(AlbumArtPicture.XPosition-fXPos),(int)iHeight);
              AlbumArtPicture.DoUpdate();
              m_iFrame++;
            }
            else if (m_iFrame < 15+100+15+150)
            {
              //show the visualisation for 150 frames
              m_iFrame++;
              GUIGraphicsContext.VideoWindow = new Rectangle((int)fXPos,(int)fYPos,(int)iWidth,(int)iHeight);
            }
            else m_iFrame=0;
          }
          else
          {
            //center logo in rect
            int iw = (SurroundingRectangle.Width - AlbumArtPicture.Width)/2;
            int ih = (SurroundingRectangle.Height - AlbumArtPicture.Height)/2;
            AlbumArtPicture.SetPosition(SurroundingRectangle.XPosition+iw, SurroundingRectangle.YPosition+ih);

          }
        }
        catch(Exception)
        {
          m_strFile="";
        }
      }
      base.Render(timePassed);
    }

    
  
    void ShowControl(int iControl)
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID,0, iControl,0,0,null); 
      OnMessage(msg); 
    }

    void HideControl(int iControl)
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID,0, iControl,0,0,null); 
      OnMessage(msg); 
    }

    void SetCurrentFile(string strFile)
    {
      GUIPropertyManager.RemovePlayerProperties();
      m_iFrames=0;
      string skin = GUIGraphicsContext.Skin;
      GUIPropertyManager.SetProperty("#thumb","");
      GUIPropertyManager.SetProperty("#thumbnext","");

      base.RestoreControlPosition((int)Controls.CONTROL_LOGO_PIC);

      //	Set image visible that is displayed if no thumb is available
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID,0, (int)Controls.CONTROL_LOGO_PIC,0,0,null);  
      OnMessage(msg);

      MusicTag tag=null;
      string thumb;
      tag=GetInfo(strFile, out thumb);

      GUIPropertyManager.SetProperty("#thumb",thumb);
      try
      {
        GUIPropertyManager.SetProperty("#file",System.IO.Path.GetFileName(strFile));
        GUIPropertyManager.SetProperty("#title",System.IO.Path.GetFileName(strFile));
      }
      catch(Exception){}

      if (tag!=null)
      {
        string strText=GUILocalizeStrings.Get(437);	//	"Duration"
        string strDuration=String.Format("{0}{1}",strText, Utils.SecondsToHMSString(tag.Duration) );
        if (tag.Duration<=0) strDuration=String.Empty;
        
        strText=GUILocalizeStrings.Get(435);	//	"Track"
        string strTrack=String.Format("{0}{1}",strText, tag.Track);
        if (tag.Track<=0) strTrack=String.Empty;
        
        strText=GUILocalizeStrings.Get(436);	//	"Year"
        string strYear=String.Format("{0}{1}",strText, tag.Year);
        if (tag.Year<=1900) strYear=String.Empty;

        GUIPropertyManager.SetProperty("#genre",tag.Genre);
        GUIPropertyManager.SetProperty("#comment",tag.Comment);
        GUIPropertyManager.SetProperty("#title",tag.Title);
        GUIPropertyManager.SetProperty("#artist",tag.Artist);
        GUIPropertyManager.SetProperty("#album",tag.Album);
        GUIPropertyManager.SetProperty("#track",strTrack);
        GUIPropertyManager.SetProperty("#year",strYear);
        GUIPropertyManager.SetProperty("#duration",strDuration);
      }
      else
      {
        GUIMessage msg1=new GUIMessage (GUIMessage .MessageType.GUI_MSG_LABEL_ADD, GetID, 0,(int)Controls.CONTROL_INFO,0,0,null); 
        msg1.Label= System.IO.Path.GetFileName(strFile );
        OnMessage(msg1);
        GUIPropertyManager.SetProperty("#title",msg1.Label);
      }

      //--------- next file ---------------------
      strFile=Playlists.PlayListPlayer.GetNext();
      if (strFile==String.Empty) 
      {
        return;
      }

      tag=null;
      thumb="";
      tag=GetInfo(strFile, out thumb);

      GUIPropertyManager.SetProperty("#thumbNext",thumb);
      try
      {
        GUIPropertyManager.SetProperty("#fileNext",System.IO.Path.GetFileName(strFile));
        GUIPropertyManager.SetProperty("#titleNext",System.IO.Path.GetFileName(strFile));
      }
      catch(Exception){}

      if (tag!=null)
      {
        string strText=GUILocalizeStrings.Get(437);	//	"Duration"
        string strDuration=String.Format("{0}{1}",strText, Utils.SecondsToHMSString(tag.Duration) );
        if (tag.Duration<=0) strDuration=String.Empty;
        
        strText=GUILocalizeStrings.Get(435);	//	"Track"
        string strTrack=String.Format("{0}{1}",strText, tag.Track);
        if (tag.Track<=0) strTrack=String.Empty;
        
        strText=GUILocalizeStrings.Get(436);	//	"Year"
        string strYear=String.Format("{0}{1}",strText, tag.Year);
        if (tag.Year<=1900) strYear=String.Empty;

        GUIPropertyManager.SetProperty("#genreNext",tag.Genre);
        GUIPropertyManager.SetProperty("#commentNext",tag.Comment);
        GUIPropertyManager.SetProperty("#titleNext",tag.Title);
        GUIPropertyManager.SetProperty("#artistNext",tag.Artist);
        GUIPropertyManager.SetProperty("#albumNext",tag.Album);
        GUIPropertyManager.SetProperty("#trackNext",strTrack);
        GUIPropertyManager.SetProperty("#yearNext",strYear);
        GUIPropertyManager.SetProperty("#durationNext",strDuration);
      }
      else
      {
        GUIMessage msg1=new GUIMessage (GUIMessage .MessageType.GUI_MSG_LABEL_ADD, GetID, 0,(int)Controls.CONTROL_INFO,0,0,null); 
        msg1.Label= System.IO.Path.GetFileName(strFile );
        OnMessage(msg1);
        GUIPropertyManager.SetProperty("#titleNext",msg1.Label);
      }
			m_strThumb=(string )GUIPropertyManager.GetProperty("#thumb");
    }

    MusicTag GetInfo(string strFile , out string thumb)
    {
      string skin = GUIGraphicsContext.Skin;
      thumb="";
      MusicTag tag=null;
			Song song=new Song();
			bool bFound=false;
			MusicDatabase dbs=new MusicDatabase();
			bFound=dbs.GetSongByFileName(strFile, ref song);
      
      bool UseID3=false;
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        UseID3 = xmlreader.GetValueAsBool("musicfiles","showid3",true);
        if (Utils.IsCDDA(strFile)) UseID3 =true;
      }
      if (!bFound )
      {
        // no id3tag in the music database, check if we should re-scan for id3 tags
        if (UseID3)
        {
          //yes, then try reading the tag from the file
          tag=TagReader.TagReader.ReadTag(strFile);
          if (tag==null)
          {
            //hrmmm file has no tag, maybe its a audio cd?
            if (Utils.IsCDDA(strFile))
            {
              //yep, then do we got CDDB info for it?
              if (GUIMusicFiles.MusicCD!=null)
              {
                //yep! find the tracknumber from the filename
                string strTrack="";
                string strFileName=System.IO.Path.GetFileNameWithoutExtension(strFile);
                int pos=strFile.IndexOf(".cda");
                if (pos >=0)
                {
                  pos--;
                  while (Char.IsDigit(strFile[pos]) && pos>0) 
                  {
                    strTrack=strFile[pos]+strTrack;
                    pos--;
                  }
                }
                // and get the correct CDDB info for the track
                try
                {
                  Freedb.CDTrackDetail track = GUIMusicFiles.MusicCD.getTrack(Convert.ToInt32(strTrack));
                  tag = new MusicTag();
                  tag.Album=GUIMusicFiles.MusicCD.Title;
                  tag.Artist=track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
                  tag.Genre=GUIMusicFiles.MusicCD.Genre;
                  tag.Duration=track.Duration;
                  tag.Title=track.Title;
                  tag.Track=track.TrackNumber;
                }
                catch(Exception)
                {
                }
              }
            }
          }
        }
        if (tag==null)
        {
            // if we're playing a radio
						if (Recorder.IsRadio())
						{
							tag = new MusicTag();
							string cover=Utils.GetCoverArt(@"Thumbs\Radio",Recorder.RadioStationName());
							if (cover!=String.Empty) thumb=cover;
							tag.Title=Recorder.RadioStationName();
						}
            if (g_Player.IsRadio)
            {
              // then check which radio station we're playing
              tag = new MusicTag();
              ArrayList stations = new ArrayList();
              RadioDatabase.GetStations(ref stations);
              string strFName=g_Player.CurrentFile;
              foreach (RadioStation station in stations)
              {
                string coverart;
                if (strFName.IndexOf(".radio")>0)
                {
                  string strChan=System.IO.Path.GetFileNameWithoutExtension(strFName);  
                  if (station.Frequency.ToString().Equals(strChan))
                  {
                    // got it, check if it has a thumbnail
                    tag.Title=station.Name;
                    coverart=Utils.GetCoverArt(@"Thumbs\Radio",station.Name);
                    if (coverart!=String.Empty) thumb=coverart;
                  }
                }
                else
                {
                  if (station.URL.Equals(strFName))
                  {
                    tag.Title=station.Name;
                    coverart=Utils.GetCoverArt(@"Thumbs\Radio",station.Name);
                    if (coverart!=String.Empty) thumb=coverart;
                  }
                }
              } //foreach (RadioStation station in stations)
            } //if (g_Player.IsRadio)
          } //if (tag==null)
        
			}// if (!bFound )
			else
			{
        // got the music tag
				tag = new MusicTag();
				tag.Album=song.Album;
				tag.Artist=song.Artist;
				tag.Duration=song.Duration;
				tag.Genre=song.Genre;
				tag.Title=song.Title;
				tag.Track=song.Track;
				tag.Year=song.Year;
			}
      if (tag!=null)
      {
         if (tag.Album.Length>0)
        {
          string strThumb=GUIMusicFiles.GetCoverArt(false,strFile,tag);
          if (strThumb!=String.Empty)
          {
            thumb=strThumb;
          }
        }
      }
      return tag;
    }
  }
}
