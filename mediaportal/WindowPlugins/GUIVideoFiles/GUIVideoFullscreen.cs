using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIVideoFullscreen: GUIWindow
  {
    enum Control 
    {
        BLUE_BAR    =0
        , OSD_VIDEOPROGRESS=1
        , LABEL_ROW1 =10
        , LABEL_ROW2 =11
        , LABEL_ROW3 =12
        , IMG_PAUSE     =16
        , IMG_2X	      =17
        , IMG_4X	      =18
        , IMG_8X		    =19
        , IMG_16X       =20
        , IMG_32X       =21

        , IMG_MIN2X	      =23
        , IMG_MIN4X	      =24
        , IMG_MIN8X		    =25
        , IMG_MIN16X       =26
        , IMG_MIN32X       =27
        , LABEL_CURRENT_TIME =22
        , OSD_TIMEINFO =100
        , PANEL1=101
        , PANEL2=120
    };

    bool m_bOSDVisible=false;
    bool m_bShowInfo=false;
    bool m_bShowStep=false;
    bool m_bShowStatus=false;
    bool m_bShowTime=false;
    
    DateTime    m_dwTimeCodeTimeout;
    string      m_strTimeStamp="";
    int         m_iTimeCodePosition=0;
    float       m_fFrameCounter=0;
    long        m_dwFPSTime=0;
    float       m_fFPS=0;
    long        m_dwTimeStatusShowTime=0;
    DateTime    m_dwOSDTimeOut;
    long        m_iMaxTimeOSDOnscreen;    
    GUIVideoOSD m_osdWindow=null;
		GUIVideoMSNOSD m_msnWindow=null;
		bool				m_bLastMSNChatVisible=false;
		bool        m_bMSNChatVisible=false;
    FormOSD     m_form=null;
    bool        m_bUpdate=false;
    bool        m_bLastStatus=false;
    bool        m_bLastStatusOSD=false;
    bool        m_bLastStatusFullScreen=true;
    DateTime    m_UpdateTimer=DateTime.Now;  
		bool				m_bDialogVisible=false;
		bool				m_bLastDialogVisible=false;
		bool				m_bMSNChatPopup=false;
		GUIDialogMenu dlg;
    
    public GUIVideoFullscreen()
    {
			
    }

    public override bool Init()
    {
      bool bResult=Load(GUIGraphicsContext.Skin+@"\videoFullScreen.xml");
      GetID=(int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
      return bResult;
    }

    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
				m_bMSNChatPopup = (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1);
        m_iMaxTimeOSDOnscreen=1000*xmlreader.GetValueAsInt("movieplayer","osdtimeout",5);
        string strValue=xmlreader.GetValueAsString("movieplayer","defaultar","normal");
        if (g_Player.IsDVD)
          strValue=xmlreader.GetValueAsString("dvdplayer","defaultar","normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
      }
    }

    void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strKey="movieplayer";
        if (g_Player.IsDVD)
          strKey="dvdplayer";
        switch (GUIGraphicsContext.ARType)
        {
          case MediaPortal.GUI.Library.Geometry.Type.Zoom:
            xmlwriter.SetValue(strKey,"defaultar","zoom");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Stretch:
            xmlwriter.SetValue(strKey,"defaultar","stretch");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Normal:
            xmlwriter.SetValue(strKey,"defaultar","normal");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Original:
            xmlwriter.SetValue(strKey,"defaultar","original");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
            xmlwriter.SetValue(strKey,"defaultar","letterbox");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
            xmlwriter.SetValue(strKey,"defaultar","panscan");
            break;
        }
      }
    }

    public override void OnAction(Action action)
    {
      //switch back to menu on right-click
      if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK && action.MouseButton == MouseButtons.Right)
      {
        m_bOSDVisible=false;
        GUIGraphicsContext.IsFullScreenVideo=false;
        GUIWindowManager.PreviousWindow();
        return;
      }

			if (m_bOSDVisible)
			{
				if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI)) && !m_osdWindow.SubMenuVisible) // hide the OSD
				{
					lock(this)
					{ 
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
						m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						m_bOSDVisible=false;
						m_bUpdate=true;
					}
				}
				else
				{
					m_dwOSDTimeOut=DateTime.Now;
					if (action.wID==Action.ActionType.ACTION_MOUSE_MOVE || action.wID==Action.ActionType.ACTION_MOUSE_CLICK)
					{
						int x=(int)action.fAmount1;
						int y=(int)action.fAmount2;
						if (!GUIGraphicsContext.MouseSupport)
						{
							m_osdWindow.OnAction(action);	// route keys to OSD window
							m_bUpdate=true;
							return;
						}
						else
						{
							if ( m_osdWindow.InWindow(x,y))
							{
								m_osdWindow.OnAction(action);	// route keys to OSD window
								m_bUpdate=true;
								return;
							}
							else
							{
								if (!m_osdWindow.SubMenuVisible)
								{
									GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
									m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
									m_bOSDVisible=false;
									m_bUpdate=true;
								}
							}
						}
					}
					Action newAction=new Action();
					if (ActionTranslator.GetAction((int)GUIWindow.Window.WINDOW_OSD,action.m_key,ref newAction))
					{
						m_osdWindow.OnAction(newAction);	// route keys to OSD window
						m_bUpdate=true;
					}
					else
					{
						// route unhandled actions to OSD window
						if (!m_osdWindow.SubMenuVisible)
						{
							m_osdWindow.OnAction(action);	
							m_bUpdate=true;
						}
					}
				}
				return;
			}
			else if (m_bMSNChatVisible)
			{
				if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI))) // hide the OSD
				{
					lock(this)
					{ 
						GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,0,0,null);
						m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						m_bMSNChatVisible=false;
						m_bUpdate=true;
					}
					return;
				}
				if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
				{
					m_msnWindow.OnAction(action);
					m_bUpdate=true;
					return;
				}		
			}
			else if (g_Player.IsDVD)
			{

				Action newAction=new Action();
				if (ActionTranslator.GetAction((int)GUIWindow.Window.WINDOW_DVD,action.m_key,ref newAction))
				{
					if ( g_Player.OnAction(newAction)) return;
				}
			}
			else if (action.wID==Action.ActionType.ACTION_MOUSE_MOVE && GUIGraphicsContext.MouseSupport )
			{
				int y =(int)action.fAmount2;
				if (y > GUIGraphicsContext.Height-100)
				{
					m_dwOSDTimeOut=DateTime.Now;
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,m_osdWindow.GetID,0,0,0,0,null);
					m_osdWindow.OnMessage(msg);	// Send an init msg to the OSD
					m_bOSDVisible=true;
					m_bUpdate=true;
				}
			}
      
      switch (action.wID)
      {
				case Action.ActionType.ACTION_SHOW_MSN_OSD:
					if (m_bMSNChatPopup)
					{
						Log.Write("MSN CHAT:ON");     
						m_bUpdate=true;  
						m_bMSNChatVisible=true;
						m_msnWindow.DoModal( GetID, null );
						m_bMSNChatVisible=false;
					}
					break;
        
          // previous : play previous song from playlist
        case Action.ActionType.ACTION_PREV_ITEM:
        {
          //g_playlistPlayer.PlayPrevious();
        }
          break;

          // next : play next song from playlist
        case Action.ActionType.ACTION_NEXT_ITEM:
        {
          //g_playlistPlayer.PlayNext();
        }
          break;

        case Action.ActionType.ACTION_SHOW_GUI:
        {
          // switch back to the menu
          m_bOSDVisible=false;
          GUIGraphicsContext.IsFullScreenVideo=false;
          GUIWindowManager.PreviousWindow();
          return;
        }

        case Action.ActionType.ACTION_ASPECT_RATIO:
        {
          m_bShowStatus=true;
          m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
          switch (GUIGraphicsContext.ARType)
          {
            case MediaPortal.GUI.Library.Geometry.Type.Zoom:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Stretch:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Normal:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Original:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
              break;

            case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
              break;

            case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
              break;
          }
          SaveSettings();
          m_bUpdate=true;
        }
          break;
    		
        case Action.ActionType.ACTION_STEP_BACK:
        {
          if (g_Player.CanSeek)
          {
            m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
            m_bShowStep=true;
            g_Player.SeekStep(false);
          }
        }
          break;

        case Action.ActionType.ACTION_STEP_FORWARD:
        {    
          if (g_Player.CanSeek)
          {
            m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
            m_bShowStep=true;
            g_Player.SeekStep(true);
          }
          } 
          break;

        case Action.ActionType.ACTION_BIG_STEP_BACK:
        { 
          PlayListPlayer.PlayPrevious();
          return;
        }
          //break;

        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
        {
            
          PlayListPlayer.PlayNext(true);
          return;
        }
          //break;

        case Action.ActionType.ACTION_SHOW_MPLAYER_OSD:
          //g_application.m_pPlayer.ToggleOSD();
          break;

        case Action.ActionType.ACTION_SHOW_OSD:	// Show the OSD
        {	
          m_dwOSDTimeOut=DateTime.Now;
          
          GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,m_osdWindow.GetID,0,0,0,0,null);
          m_osdWindow.OnMessage(msg);	// Send an init msg to the OSD
          m_bOSDVisible=true;
          m_bUpdate=true;
          
        }
          break;
    			
        case Action.ActionType.ACTION_SHOW_SUBTITLES:
        {	
          //g_application.m_pPlayer.ToggleSubtitles();
        }
          break;


        case Action.ActionType.ACTION_NEXT_SUBTITLE:
        {
          //g_application.m_pPlayer.SwitchToNextLanguage();
        }
          break;

        case Action.ActionType.ACTION_STOP:
				{
					Log.Write("GUIVideoFullscreen:stop");
          g_Player.Stop();
          GUIWindowManager.PreviousWindow();
        }
          break;

          // PAUSE action is handled globally in the Application class
        case Action.ActionType.ACTION_PAUSE:
          g_Player.Pause();
          m_bUpdate=true;
          break;

        case Action.ActionType.ACTION_SUBTITLE_DELAY_MIN:
          //g_application.m_pPlayer.SubtitleOffset(false);
          break;
        case Action.ActionType.ACTION_SUBTITLE_DELAY_PLUS:
          //g_application.m_pPlayer.SubtitleOffset(true);
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_MIN:
          //g_application.m_pPlayer.AudioOffset(false);
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_PLUS:
          //g_application.m_pPlayer.AudioOffset(true);
          break;
        case Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE:
          //g_application.m_pPlayer.AudioOffset(false);
          break;

          case Action.ActionType.ACTION_REWIND:
          {
            g_Player.Speed=Utils.GetNextRewindSpeed(g_Player.Speed);
            m_bUpdate=true;
          }
          break;

          case Action.ActionType.ACTION_FORWARD:
          {
            g_Player.Speed=Utils.GetNextForwardSpeed(g_Player.Speed);
            m_bUpdate=true;
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
					if ((action.m_key!=null) && (!m_bMSNChatVisible))
            ChangetheTimeCode((char)action.m_key.KeyChar);
          break;

        case Action.ActionType.ACTION_SMALL_STEP_BACK:
        {
            
          if (g_Player.CanSeek)
          {
            // seek back 5 sec
            double dPos=g_Player.CurrentPosition;
            if (dPos>5)
            {
              g_Player.SeekAbsolute(dPos-5.0d);
            }
          }
        }
        break;

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
        {
          g_Player.StepNow();
          g_Player.Speed=1;
          if (g_Player.Paused) g_Player.Pause();
          m_bUpdate=true;
        }
          break;

				case Action.ActionType.ACTION_CONTEXT_MENU:
					ShowContextMenu();
					break;
      }

      base.OnAction(action);
      m_bUpdate=true;
    }

    public override bool OnMessage(GUIMessage message)
    {
      m_bUpdate=true;
      if (m_bOSDVisible)
      { 

        switch (message.Message)
        {
          case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
            goto case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT;
          
          case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
            goto case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT;

          case GUIMessage.MessageType.GUI_MSG_CLICKED:
            goto case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT;

          case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
            goto case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT;

          case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
            m_dwOSDTimeOut=DateTime.Now;
          break;
        }
        return m_osdWindow.OnMessage(message);	// route messages to OSD window
        
      }

      switch (message.Message)
      {
				case GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION:
					if (m_bMSNChatVisible)
					{
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,0,0,null);
						m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
					}
					m_bMSNChatVisible=false;
					break;

				case GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE:
				case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
					if (m_bOSDVisible && m_bMSNChatPopup)
					{
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
						m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						m_bOSDVisible=false;
						m_bUpdate=true;
					}

					if (!m_bMSNChatVisible && m_bMSNChatPopup && (m_msnWindow != null))
					{
						Log.Write("MSN CHAT:ON");     
						m_bMSNChatVisible=true;											
						m_msnWindow.DoModal( GetID, message );
						m_bMSNChatVisible=false;
						m_bUpdate=true;         
					}
					break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          m_osdWindow=(GUIVideoOSD)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_OSD);
					m_msnWindow=(GUIVideoMSNOSD)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MSNOSD);
          
					HideControl(GetID,(int)Control.LABEL_ROW1);
					HideControl(GetID,(int)Control.LABEL_ROW2);
					HideControl(GetID,(int)Control.LABEL_ROW3);
					HideControl(GetID,(int)Control.BLUE_BAR);
					HideControl(GetID,(int)Control.LABEL_CURRENT_TIME);
          
          m_bOSDVisible=false;
          m_bShowInfo=false;
          m_bShowStep=false;
          m_bShowStatus=false;
          m_bShowTime=false;
          m_strTimeStamp="";
          m_iTimeCodePosition=0;
          m_fFrameCounter=0;
          m_dwFPSTime=0;
          m_fFPS=0;
          m_dwTimeStatusShowTime=0;   
          m_bUpdate=false;
          m_bLastStatus=false;
          m_bLastStatusOSD=false;
          m_bLastStatusFullScreen=true;
          m_UpdateTimer=DateTime.Now;
          LoadSettings();
          /*
          CUtil::SetBrightnessContrastGammaPercent(g_settings.m_iBrightness,g_settings.m_iContrast,g_settings.m_iGamma,true);
          GUIGraphicsContext.SetFullScreenVideo(false);//turn off to prevent calibration to OSD position
          */
          /*
          GUIGraphicsContext.Lock();
          GUIGraphicsContext.DX9Device.Clear( 0L, null, D3DCLEAR_TARGET|D3DCLEAR_ZBUFFER|D3DCLEAR_STENCIL, 0x00010001, 1.0f, 0L );
          GUIGraphicsContext.SetFullScreenVideo( true );
          GUIGraphicsContext.DX9Device.Present( );
          GUIGraphicsContext.Unlock();
          if (g_application.m_pPlayer)
            g_application.m_pPlayer.Update();
          */
          HideOSD();

          if (!GUIGraphicsContext.Vmr9Active)
          {
            m_form = new FormOSD();
            m_form.Owner = GUIGraphicsContext.form;
            m_form.Show();            
            GUIGraphicsContext.form.Focus();
          }
          GUIGraphicsContext.IsFullScreenVideo=true;
          
          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          lock(this)
          {
            if (m_bOSDVisible)
            {
              GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
              m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            }
						m_bOSDVisible=false;

						if (m_bMSNChatVisible)
						{
							GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,0,0,null);
							m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						}
						m_bMSNChatVisible=false;
            base.OnMessage(message);
            /*
            CUtil::RestoreBrightnessContrastGamma();
            GUIGraphicsContext.Lock();
            GUIGraphicsContext.SetFullScreenVideo( false );
            GUIGraphicsContext.Unlock();
            if (g_application.m_pPlayer)
              g_application.m_pPlayer.Update(true);	
            // Pause so that we make sure that our fullscreen renderer has finished...
            Sleep(100);
            */
            HideOSD();
            if (m_form!=null) 
            {
              m_form.Close();
              m_form.Dispose();
            }
            m_form=null;
          }
          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          if (m_bOSDVisible) return true;
          if (message.SenderControlId != (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO) return true;
          break;
      }

			if (m_bMSNChatVisible)
			{
				m_msnWindow.OnMessage(message);	// route messages to MSNChat window
			}

      return base.OnMessage(message);
    }

		void ShowContextMenu()
		{
			if (dlg==null)
				dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu

			dlg.AddLocalizedString(941); // Change aspect ratio
			if (PluginManager.IsPluginNameEnabled("MSN Messenger"))
			{
				dlg.AddLocalizedString(12902); // MSN Messenger
				dlg.AddLocalizedString(902); // MSN Online contacts
			}
			dlg.AddLocalizedString(970); // Previous window
			if (g_Player.IsDVD)
			{
				dlg.AddLocalizedString(974); // Root menu
				dlg.AddLocalizedString(975); // Previous chapter
				dlg.AddLocalizedString(976); // Next chapter
			}

			m_bDialogVisible=true;
			m_bUpdate=true;
			dlg.DoModal( GetID);
			m_bDialogVisible=false;
			m_bUpdate=true;
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 974: // DVD root menu
					Action actionMenu = new Action(Action.ActionType.ACTION_DVD_MENU,0,0);
					GUIGraphicsContext.OnAction(actionMenu);
					break;
				case 975: // DVD previous chapter
					Action actionPrevChapter = new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
					GUIGraphicsContext.OnAction(actionPrevChapter);
					break;
				case 976: // DVD next chapter
					Action actionNextChapter = new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
					GUIGraphicsContext.OnAction(actionNextChapter);
					break;
				case 941: // Change aspect ratio
					ShowAspectRatioMenu();
					break;
					
				case 12902: // MSN Messenger
					Log.Write("MSN CHAT:ON");     
					m_bMSNChatVisible=true;
					m_msnWindow.DoModal( GetID, null );
					m_bMSNChatVisible=false;
					break;

				case 902: // Online contacts
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
					break;

				case 970:
					// switch back to MyMovies window
					m_bOSDVisible=false;
					m_bMSNChatVisible=false;
					GUIGraphicsContext.IsFullScreenVideo=false;
					GUIWindowManager.PreviousWindow();
					break;
			}
		}
    
		void ShowAspectRatioMenu()
		{
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(941); // Change aspect ratio

			dlg.AddLocalizedString(942); // Stretch
			dlg.AddLocalizedString(943); // Normal
			dlg.AddLocalizedString(944); // Original
			dlg.AddLocalizedString(945); // Letterbox
			dlg.AddLocalizedString(946); // Pan and scan
			dlg.AddLocalizedString(947); // Zoom

			m_bDialogVisible=true;
			m_bUpdate=true;
			dlg.DoModal( GetID);
			m_bDialogVisible=false;
			m_bUpdate=true;
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 942: // Stretch
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					m_bUpdate=true;
					SaveSettings();
					break;

				case 943: // Normal
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					m_bUpdate=true;
					SaveSettings();
					break;

				case 944: // Original
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					m_bUpdate=true;
					SaveSettings();
					break;

				case 945: // Letterbox
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					m_bUpdate=true;
					SaveSettings();
					break;

				case 946: // Pan and scan
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
					m_bUpdate=true;
					SaveSettings();
					break;
      
				case 947: // Zoom
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					m_bUpdate=true;
					SaveSettings();
					break;
			}
		}

    public bool NeedUpdate()
    {
      if (m_bLastStatus && !m_bOSDVisible)
      {
        m_bUpdate=true;
      }
      m_bLastStatus=m_bOSDVisible;
			if (m_bOSDVisible)
			{
				if (m_iMaxTimeOSDOnscreen>0)
				{
					TimeSpan ts =DateTime.Now - m_dwOSDTimeOut;
					if ( ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
					{
						m_bUpdate=true;
					}
				}
				TimeSpan tsUpDate = DateTime.Now-m_UpdateTimer;
				if (tsUpDate.TotalSeconds>=1)
				{
					m_UpdateTimer=DateTime.Now;
					m_bUpdate=true;
				}
			}
			else if (m_bShowInfo||m_bShowStep)
			{
				TimeSpan tsUpDate = DateTime.Now-m_UpdateTimer;
				if (tsUpDate.TotalSeconds>=1)
				{
					m_UpdateTimer=DateTime.Now;
					m_bUpdate=true;
				}
			}

			if (m_bMSNChatVisible)
			{
				if (m_msnWindow.NeedRefresh()) m_bUpdate=true;
			}

			if (m_bDialogVisible)
			{
				if (dlg.NeedRefresh()) m_bUpdate=true;
			}
			
			if ( m_bUpdate)
      {
        m_bUpdate=false;
        return true;
      }

      if (NeedFullScreenRender()) // added for progressbar update
      {
        return true;
      }

      if (!NeedFullScreenRender() && m_bLastStatusFullScreen) 
      {
        return true;
      }
      return false;
    }


    public bool NeedFullScreenRender()
    {
      lock (this)
      {
        if (g_Player.Paused )return true;
        bool bStart, bEnd;
        if (g_Player.Speed != 1 )
        {
          m_bShowInfo=true;
          m_dwTimeCodeTimeout=DateTime.Now;
          return true;
        }
        if (g_Player.GetSeekStep(out bStart, out bEnd) !=0) 
        {
          m_bShowStep=true;
          m_dwTimeCodeTimeout=DateTime.Now;
          return true;
        }
        if (m_bShowTime) return true;
        if (m_bShowStatus) return true;
        if (m_bShowInfo) return true;
        if (m_bShowStep) return true;
				if (m_bDialogVisible) return true;
				if (m_bMSNChatVisible) return true;
        
        return false;
      }
    }
    public void SetFFRWLogos()
    {

      if (GUIGraphicsContext.Vmr9Active && (m_bShowStep||m_bShowInfo || (!m_bOSDVisible&& g_Player.Speed!=1) || (!m_bOSDVisible&& g_Player.Paused)) )
      {
        for (int i=(int)Control.PANEL1; i < (int)Control.PANEL2;++i)
          ShowControl(GetID,i);
        ShowControl(GetID,(int)Control.OSD_TIMEINFO);  
        ShowControl(GetID,(int)Control.OSD_VIDEOPROGRESS);  
      }
      else
      {
        for (int i=(int)Control.PANEL1; i < (int)Control.PANEL2;++i)
          HideControl(GetID,i);
        HideControl(GetID,(int)Control.OSD_TIMEINFO); 
        HideControl(GetID,(int)Control.OSD_VIDEOPROGRESS);  
      }
      if (g_Player.Paused )
      {
        ShowControl(GetID,(int)Control.IMG_PAUSE);  
      }
      else
      {
        HideControl(GetID,(int)Control.IMG_PAUSE);  
      }

      int iSpeed=g_Player.Speed;
      HideControl(GetID,(int)Control.IMG_2X);
      HideControl(GetID,(int)Control.IMG_4X);
      HideControl(GetID,(int)Control.IMG_8X);
      HideControl(GetID,(int)Control.IMG_16X);
      HideControl(GetID,(int)Control.IMG_32X);
      HideControl(GetID,(int)Control.IMG_MIN2X);
      HideControl(GetID,(int)Control.IMG_MIN4X);
      HideControl(GetID,(int)Control.IMG_MIN8X);
      HideControl(GetID,(int)Control.IMG_MIN16X);
      HideControl(GetID,(int)Control.IMG_MIN32X);

      if(iSpeed!=1)
      {
        if(iSpeed == 2)
        {
          ShowControl(GetID,(int)Control.IMG_2X);
        }
        else if(iSpeed == 4)
        {
          ShowControl(GetID,(int)Control.IMG_4X);
        }
        else if(iSpeed == 8)
        {
          ShowControl(GetID,(int)Control.IMG_8X);
        }
        else if(iSpeed == 16)
        {
          ShowControl(GetID,(int)Control.IMG_16X);
        }
        else if(iSpeed == 32)
        {
          ShowControl(GetID,(int)Control.IMG_32X);
        }

        if(iSpeed == -2)
        {
          ShowControl(GetID,(int)Control.IMG_MIN2X);
        }
        else if(iSpeed == -4)
        {
          ShowControl(GetID,(int)Control.IMG_MIN4X);
        }
        else if(iSpeed == -8)
        {
          ShowControl(GetID,(int)Control.IMG_MIN8X);
        }
        else if(iSpeed == -16)
        {
          ShowControl(GetID,(int)Control.IMG_MIN16X);
        }
        else if(iSpeed == -32)
        {
          ShowControl(GetID,(int)Control.IMG_MIN32X);
        }

      }
    }


    public override void Render(float timePassed)
    {
      if (!g_Player.Playing)
      {
        if (PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC  ||
           PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP)
        {
          return;
        }
        m_bOSDVisible=false;
        GUIWindowManager.PreviousWindow();
        return;
      }
			long lTimeSpan=( (DateTime.Now.Ticks/10000) - m_dwTimeStatusShowTime);
			if ( lTimeSpan >=2000)
			{
        m_bShowStep=false;
				m_bShowInfo=false;
				m_bShowStatus=false;
			}
			SetFFRWLogos();

      if (GUIGraphicsContext.Vmr9Active)
      {
        RenderFullScreen(timePassed);  
        // do we need 2 render the OSD?
				if (m_bOSDVisible)
				{
					m_osdWindow.Render(timePassed);
				}
        //base.Render(timePassed);
      }

			// OSD Timeout?
			if (m_iMaxTimeOSDOnscreen>0)
			{
				TimeSpan ts =DateTime.Now - m_dwOSDTimeOut;
				if ( ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
				{
					//yes, then remove osd offscreen
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
					m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD

					m_bOSDVisible=false;
				}
			}
    }

    void RenderFullScreen(float timePassed)
    {

      if (g_Player.Speed != 1) 
      { 
        m_dwTimeCodeTimeout=DateTime.Now;
      }

      

      //m_bLastRender=true;
	    m_fFrameCounter+=1.0f;
	    float fTimeSpan=(float)( (DateTime.Now.Ticks/10000)-m_dwFPSTime);
	    if (fTimeSpan >=1000.0f)
	    {
		    fTimeSpan/=1000.0f;
		    m_fFPS=(m_fFrameCounter/fTimeSpan);
		    m_dwFPSTime=(DateTime.Now.Ticks/10000);
		    m_fFrameCounter=0;
	    }
	    
      bool bRenderGUI=false;
	    if (g_Player.Paused )
      {
         ShowControl(GetID,(int)Control.IMG_PAUSE);  
        bRenderGUI=true;
      }
      else
      {
        HideControl(GetID,(int)Control.IMG_PAUSE);  
      }
     
	    if (m_bShowStatus||m_bShowInfo||m_bShowStep)
	    {
		    long lTimeSpan=( (DateTime.Now.Ticks/10000) - m_dwTimeStatusShowTime);
		    if ( lTimeSpan >=2000)
		    {
          m_bShowStep=false;
          m_bShowInfo=false;
			    m_bShowStatus=false;
		    }
        bool bStart,bEnd;
        int iTimeStep=g_Player.GetSeekStep(out bStart,out bEnd);
        bRenderGUI=true;
        string strStatus="";
        string strStatus2="";
        switch (GUIGraphicsContext.ARType)
        {
          case MediaPortal.GUI.Library.Geometry.Type.Zoom:
              strStatus="Zoom";
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Stretch:
              strStatus="Stretch";
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Normal:
              strStatus="Normal";
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Original:
            strStatus="Original";
            break;

          case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
            strStatus="Letterbox 4:3";
            break;

          case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
            strStatus="Pan&Scan 4:3";
            break;
        }

        string strRects=String.Format(" | ({0},{1})-({2},{3})  ({4},{5})-({6},{7})", 
                          g_Player.SourceWindow.Left,g_Player.SourceWindow.Top,
                          g_Player.SourceWindow.Right,g_Player.SourceWindow.Bottom, 
											    g_Player.VideoWindow.Left,g_Player.VideoWindow.Top,
											    g_Player.VideoWindow.Right,g_Player.VideoWindow.Bottom);

        if (m_bShowStep)
        {
          strStatus=g_Player.GetStepDescription();
        }
		    
	        
		    {
			    GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
			    msg.Label=strStatus; 
			    OnMessage(msg);
		    }
		    {
			    GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW2,0,0,null); 
			    msg.Label=strStatus2; 
			    OnMessage(msg);
		    }
		    {
			    GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW3,0,0,null); 
			    msg.Label=""; 
			    OnMessage(msg);
		    }

	    }

      
	    if (m_bShowTime && m_iTimeCodePosition != 0)
	    {
        TimeSpan lTimeSpan=DateTime.Now - m_dwTimeCodeTimeout;
		    if ( lTimeSpan.TotalMilliseconds >=2500)
		    {
			    m_bShowTime=false;
			    m_iTimeCodePosition = 0;
          m_strTimeStamp="";
			    return;
		    }
	      bRenderGUI=true;
        //string displaytime="";
        string strTmp=m_strTimeStamp;
        if (m_iTimeCodePosition==0) strTmp="??:??";
        if (m_iTimeCodePosition==1) strTmp+="?:??";
        if (m_iTimeCodePosition==3) strTmp+="??";
        if (m_iTimeCodePosition==4) strTmp+="?";
		    GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, (int)Control.LABEL_ROW1,0,0,null); 
		    /*
        int ihour=0;
        int imin=0; 
        int isec=0; 

		    double tmpvar = g_Player.Duration;
		    if(tmpvar != 0)
		    {
          ihour = (int)(tmpvar / 3600) % 100;
          imin = (int)((tmpvar / 60) % 60);
          isec = (int)((tmpvar /  1) % 60);
			    displaytime=String.Format("{0} / {1}:{2:00}:{3:00}", strTmp,ihour,imin,isec);
		    }
        else 
        {
          displaytime=String.Format("{0} / 0:00:00", strTmp);
        }
        double iCurrentTime=g_Player.CurrentPosition;
        if(iCurrentTime != 0)
		    {
          ihour = (int)(iCurrentTime / 3600) % 100;
          imin = (int)((iCurrentTime / 60) % 60);
          isec = (int)((iCurrentTime /  1) % 60);
          strTmp=String.Format(" [{0}:{1:00}:{2:00}]", ihour,imin,isec);
        }
        else
        {
          strTmp=" [??:??:??]";
        }
		    msg.Label=displaytime+strTmp; 
        OnMessage(msg);*/
				msg.Label=strTmp; 
				OnMessage(msg);
      }	
      
	    SetFFRWLogos();

      int iSpeed=g_Player.Speed;
      if ((iSpeed!=1) || m_bDialogVisible || m_bMSNChatVisible) bRenderGUI=true;

      if ( bRenderGUI)
      {
	      if (g_Player.Paused || iSpeed != 1)
	      {
          m_bShowInfo=true;
	        HideControl(GetID,(int)Control.LABEL_ROW1);
          HideControl(GetID,(int)Control.LABEL_ROW2);
          HideControl(GetID,(int)Control.LABEL_ROW3);
          HideControl(GetID,(int)Control.BLUE_BAR);
	      }
        else if (m_bShowStatus)
        {
           ShowControl(GetID,(int)Control.LABEL_ROW1);
           ShowControl(GetID,(int)Control.LABEL_ROW2);
           ShowControl(GetID,(int)Control.LABEL_ROW3);
           ShowControl(GetID,(int)Control.BLUE_BAR);
        }
        else if (m_bShowTime||m_bShowInfo||m_bShowStep)
	      {
		       ShowControl(GetID,(int)Control.LABEL_ROW1);
		      HideControl(GetID,(int)Control.LABEL_ROW2);
		      HideControl(GetID,(int)Control.LABEL_ROW3);
		       ShowControl(GetID,(int)Control.BLUE_BAR);
        }
        else
        {
          HideControl(GetID,(int)Control.LABEL_ROW1);
          HideControl(GetID,(int)Control.LABEL_ROW2);
          HideControl(GetID,(int)Control.LABEL_ROW3);
          HideControl(GetID,(int)Control.BLUE_BAR);
        }
        if (m_bShowInfo||m_bShowStep)
        {

          ShowControl(GetID,(int)Control.OSD_TIMEINFO);  
          ShowControl(GetID,(int)Control.OSD_VIDEOPROGRESS);            
          for (int i=(int)Control.PANEL1; i < (int)Control.PANEL2;++i)
            ShowControl(GetID,i);
        }
        else
        {
          
          HideControl(GetID,(int)Control.OSD_TIMEINFO);  
          HideControl(GetID,(int)Control.OSD_VIDEOPROGRESS);  
          for (int i=(int)Control.PANEL1; i < (int)Control.PANEL2;++i)
            HideControl(GetID,i);
        }
	      base.Render(timePassed);

				if (GUIGraphicsContext.graphics!=null)
				{
					if (m_bMSNChatVisible)
						m_msnWindow.Render(timePassed);
					if (m_bDialogVisible)
						dlg.Render(timePassed);

				}
      }
    }

    void HideOSD()
    {
      lock (this)
      {
        
         HideControl(GetID,(int)Control.LABEL_ROW1);
         HideControl(GetID,(int)Control.LABEL_ROW2);
         HideControl(GetID,(int)Control.LABEL_ROW3);
         HideControl(GetID,(int)Control.BLUE_BAR);
      }
    }

    bool OSDVisible() 
    {
      return m_bOSDVisible;
    }

    void ChangetheTimeCode(char chKey)
    {
	    if(chKey>='0'&& chKey <='9') //Make sure it's only for the remote
	    {
		    m_bShowTime = true;
		    m_dwTimeCodeTimeout=DateTime.Now;
		    if(m_iTimeCodePosition <= 4)
		    {
          //00:12
			    m_strTimeStamp+= chKey;
          m_iTimeCodePosition++;
          if(m_iTimeCodePosition == 2)
          {
            m_strTimeStamp+=":";
            m_iTimeCodePosition++;
          }
		    }
        if(m_iTimeCodePosition > 4)
		    {
			    int itotal,ih,im,lis=0;                 
			    ih =  (m_strTimeStamp[0]-(char)'0')*10;
			    ih += (m_strTimeStamp[1]-(char)'0');   
			    im =  (m_strTimeStamp[3]-(char)'0')*10;   
			    im += (m_strTimeStamp[4]-(char)'0');   
			    im*=60;
			    ih*=3600; 
			    itotal = ih+im+lis;
          if(itotal < g_Player.Duration)
          {
            g_Player.SeekAbsolute((double)itotal);
          }
          m_strTimeStamp="";
			    m_iTimeCodePosition = 0;
          m_bShowTime=false;
		    }
	    }
    }
    
    public void RenderForm(float timePassed)
    {
      if (!g_Player.Playing) return;

			bool bClear=false;

			if (m_bDialogVisible)
			{
				if (!m_bLastDialogVisible)
				{
					m_bLastDialogVisible=true;
					bClear=true;			
				}
			}
			else
			{
				if (m_bLastDialogVisible)
				{
					m_bLastDialogVisible=false;
					bClear=true;
				}
			}

			if (m_bLastMSNChatVisible)
			{
				if (!m_bMSNChatVisible)
				{
					bClear=true;			
					m_bLastMSNChatVisible=false;
				}
			}

			if (m_bMSNChatVisible)
			{
				m_bLastMSNChatVisible = true;
			}

      // if last time fullscreen window was visible
      if (m_bLastStatusFullScreen )
      {
        // and now its gone
        if (!NeedFullScreenRender())
        {
          // then clear screen
          bClear=true;
        }
      }
      // if last time OSD was visible
      if (m_bLastStatusOSD)
      {
        // and now its gone
        if (!m_bOSDVisible)
        {
          // then clear screen
          bClear=true;
        }
        else 
        {
          // osd still onscreen, check if it needs a refresh
          if (m_osdWindow.NeedRefresh())
          {
            // yes, then clear screen
            bClear=true;
          }
        }
      }

      
      if (bClear)
      {
        GUIGraphicsContext.graphics.Clear(Color.Black);
        Trace.WriteLine("osd:Clear window");
      }

      // do we need 2 render the GUIVideoFullScreen window?
      if (NeedFullScreenRender())
      {
        // yes
        m_bLastStatusFullScreen=true;
        RenderFullScreen(timePassed);
      }
      else m_bLastStatusFullScreen=false;

      // do we need 2 render the OSD?
      if (m_bOSDVisible)
      {
        //yes
        m_bLastStatusOSD=true;
        m_osdWindow.Render(timePassed);
        
        //times up?
        if (m_iMaxTimeOSDOnscreen>0)
        {
          TimeSpan ts =DateTime.Now - m_dwOSDTimeOut;
          if ( ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
          {
            //yes, then remove osd offscreen
            GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,0,0,null);
            m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD

            m_bOSDVisible=false;
          }
        }
      }
      else
      {
        m_bLastStatusOSD=false;
        /*
        if (!g_Player.IsDVD)
        {
          if (g_Player.HasSubs)
          {
            g_Player.RenderSubtitles();
          }
        }*/
      }
		}

    void HideControl (int dwSenderId, int dwControlID) 
    {
      GUIControl cntl=base.GetControl(dwControlID);
      if (cntl!=null)
      {
        cntl.IsVisible=false;
      }
    }
    void ShowControl (int dwSenderId, int dwControlID) 
    {
      GUIControl cntl=base.GetControl(dwControlID);
      if (cntl!=null)
      {
        cntl.IsVisible=true;
      }
    }

    public override int GetFocusControlId()
    {
      if (m_bOSDVisible) 
      {
        return m_osdWindow.GetFocusControlId();
      }
			if (m_bMSNChatVisible)
			{
				return m_msnWindow.GetFocusControlId();
			}

      return base.GetFocusControlId();
    }

    public override GUIControl	GetControl(int iControlId) 
    {
      if (m_bOSDVisible) 
      {
        return m_osdWindow.GetControl(iControlId);
      }
			if (m_bMSNChatVisible)
			{
				return m_msnWindow.GetControl(iControlId);
			}

      return base.GetControl(iControlId);
    }
  }
}