using System;
using System.Drawing;
using System.Net;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Music.Database;
using System.Text;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIMusicArtistInfo : GUIWindow
  { 
		[SkinControlAttribute(20)]		protected GUILabelControl lblArtist=null;
		[SkinControlAttribute(21)]		protected GUILabelControl lblArtistName=null;
		[SkinControlAttribute(22)]		protected GUILabelControl lblBorn=null;
		[SkinControlAttribute(23)]		protected GUILabelControl lblYearsActive=null;
		[SkinControlAttribute(24)]		protected GUILabelControl lblGenre=null;
		[SkinControlAttribute(25)]		protected GUIFadeLabel		lblTones=null;
		[SkinControlAttribute(26)]		protected GUIFadeLabel		lblStyles=null;
		[SkinControlAttribute(27)]		protected GUILabelControl lblInstruments=null;
		[SkinControlAttribute(3)]			protected GUIImage				imgCoverArt=null;
		[SkinControlAttribute(4)]			protected GUITextControl	tbReview=null;
		[SkinControlAttribute(5)]			protected GUIButtonControl  btnBio=null;
		[SkinControlAttribute(6)]			protected GUIButtonControl  btnRefresh=null;


    #region Base Dialog Variables
    bool m_bRunning=false;
    bool m_bRefresh=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;

    #endregion

    Texture coverArtTexture=null;
    bool    viewBio=false;
    MusicArtistInfo artistInfo=null;
    int coverArtTextureWidth=0;
    int coverArtTextureHeight=0;
    bool m_bOverlay=false;

    public GUIMusicArtistInfo()
    {
      GetID=(int)GUIWindow.Window.WINDOW_ARTIST_INFO;
    }
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\DialogArtistInfo.xml");
    }
    public override void PreInit()
    {
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }
    #region Base Dialog Members
    public void RenderDlg(float timePassed)
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render(timePassed);

			GUIFontManager.Present();
      // render this dialog box
      base.Render(timePassed);
    }

    void Close()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow=null;
      m_bRunning=false;
    }

    public void DoModal(int dwParentId)
    {
      m_bRefresh=false;
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      m_bRunning=true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion
	
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			artistInfo=null;
			if (coverArtTexture!=null)
			{
				coverArtTexture.Dispose();
				coverArtTexture=null;
			}
			GUIGraphicsContext.Overlay=m_bOverlay;
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			m_bOverlay=GUIGraphicsContext.Overlay;
			coverArtTexture=null;
			viewBio=true;
			Refresh();
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnRefresh)
			{
				string coverArtUrl=artistInfo.ImageURL;
				string coverArtFileName=GUIMusicFiles.GetArtistCoverArtName(artistInfo.Artist);
				if (coverArtFileName!=String.Empty) Utils.FileDelete(coverArtFileName);
				m_bRefresh=true;
				Close();
				return ;
			}

			if (control==btnBio)
			{
				viewBio=!viewBio;
				Update();
			}
		}

    public MusicArtistInfo Artist
    {
      set {artistInfo=value; }
    }

    void Update()
    {
      if (null==artistInfo) return;
      string tmpLine;
      string nameAKA = artistInfo.Artist;
      if(artistInfo.Aka != null && artistInfo.Aka.Length > 0)
        nameAKA += "(" + artistInfo.Aka + ")";
      lblArtist.Label= artistInfo.Artist ;
      lblArtistName.Label= nameAKA ;
      lblBorn.Label= artistInfo.Born ;
      lblYearsActive.Label= artistInfo.YearsActive ;
      lblGenre.Label= artistInfo.Genres ;
      lblInstruments.Label= artistInfo.Instruments ;

      // scroll Tones
			lblTones.Clear();
			lblTones.Add(artistInfo.Tones.Trim());

      // scroll Styles
			lblStyles.Clear();
			lblStyles.Add(artistInfo.Styles.Trim());

      if (viewBio)
      {
				tbReview.Label=artistInfo.AMGBiography;
				btnBio.Label=GUILocalizeStrings.Get(132);
      }
      else
      {
        // translate the diff. discographys
        string textAlbums = GUILocalizeStrings.Get(690);
        string textCompilations = GUILocalizeStrings.Get(691);
        string textSingles = GUILocalizeStrings.Get(700);
        string textMisc = GUILocalizeStrings.Get(701);

        
        StringBuilder strLine = new StringBuilder(2048);
        ArrayList list = null;
        string discography = null;

        // get the Discography Album
        list = artistInfo.DiscographyAlbums;
        strLine.Append('\t');
        strLine.Append(textAlbums);
        strLine.Append('\n');

        discography = artistInfo.Albums;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          };
          strLine.Append('\n');
          artistInfo.Albums = strLine2.ToString();
        }

        // get the Discography Compilations
        list = artistInfo.DiscographyCompilations;
        strLine.Append('\t');
        strLine.Append(textCompilations);
        strLine.Append('\n');
        discography = artistInfo.Compilations;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          };
          strLine.Append('\n');
          artistInfo.Compilations = strLine2.ToString();
        }

        // get the Discography Singles
        list = artistInfo.DiscographySingles;
        strLine.Append('\t');
        strLine.Append(textSingles);
        strLine.Append('\n');
        discography = artistInfo.Singles;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          };
          strLine.Append('\n');
          artistInfo.Singles = strLine2.ToString();
        }

        // get the Discography Misc
        list = artistInfo.DiscographyMisc;
        strLine.Append('\t');
        strLine.Append(textMisc);
        strLine.Append('\n');
        discography = artistInfo.Misc;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          };
          strLine.Append('\n');
          artistInfo.Misc = strLine2.ToString();
        }

				tbReview.Label=strLine.ToString();
        btnBio.Label=GUILocalizeStrings.Get(689);
      }
    }
    public override void Render(float timePassed)
    {
      RenderDlg(timePassed);

      if (null==coverArtTexture) return;

      if (null!=imgCoverArt)
      {
        float x=(float)imgCoverArt.XPosition;
        float y=(float)imgCoverArt.YPosition;
        int width;
        int height;
        GUIGraphicsContext.Correct(ref x,ref y);

        int maxWidth=imgCoverArt.Width;
        int maxHeight=imgCoverArt.Height;
        GUIGraphicsContext.GetOutputRect(coverArtTextureWidth, coverArtTextureHeight,maxWidth,maxHeight, out width,out height);

				GUIFontManager.Present();
        MediaPortal.Util.Picture.RenderImage(ref coverArtTexture,(int)x,(int)y,width,height,coverArtTextureWidth,coverArtTextureHeight,0,0,true);
      }
    }


    void Refresh()
    {
      if (coverArtTexture!=null)
      {
        coverArtTexture.Dispose();
        coverArtTexture=null;
      }

      string coverArtFileName;
      string coverArtUrl=artistInfo.ImageURL;
      coverArtFileName=GUIMusicFiles.GetArtistCoverArtName(artistInfo.Artist);
      if (coverArtFileName!=String.Empty )
      {
        //	Download image and save as 
        //	permanent thumb
        Utils.DownLoadImage(coverArtUrl,coverArtFileName);
      }

      if (System.IO.File.Exists(coverArtFileName) )
      {
        coverArtTexture=MediaPortal.Util.Picture.Load(coverArtFileName,0,128,128,true,false,out coverArtTextureWidth,out coverArtTextureHeight);
      }
      Update();
    }

    
    public bool NeedsRefresh
    {
      get {return m_bRefresh;}
    }
  }
}
