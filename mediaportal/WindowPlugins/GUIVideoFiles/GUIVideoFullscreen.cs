using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
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

		class FullScreenState
		{
			public int	 SeekStep=1;
			public int	 Speed=1;
			public bool	 OsdVisible=false;
			public bool  Paused=false;
			public bool  MsnVisible=false;
			public bool  ContextMenuVisible=false;
			public bool  ShowStatusLine=false;
			public bool  ShowTime=false;
			public bool  wasVMRBitmapVisible=false;
			public bool  NotifyDialogVisible=false;
		}

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

    bool isOsdVisible=false;
    
    bool m_bShowStep=false;
    bool m_bShowStatus=false;
    bool m_bShowTime=false;
    
    DateTime    m_dwTimeCodeTimeout;
    string      m_strTimeStamp="";
    int         m_iTimeCodePosition=0;
    long        m_dwTimeStatusShowTime=0;
    DateTime    m_dwOSDTimeOut;
    long        m_iMaxTimeOSDOnscreen;    
		bool        m_bMSNChatVisible=false;
    //FormOSD     m_form=null;
    DateTime    m_UpdateTimer=DateTime.Now;  
		bool				m_bDialogVisible=false;
		bool				m_bMSNChatPopup=false;
		bool				needToClearScreen=false;
		GUIDialogMenu		dlg;
		GUIVideoOSD			m_osdWindow=null;
		GUIVideoMSNOSD	m_msnWindow=null;
		GUIDialogNotify dialogNotify=null;
		bool				NotifyDialogVisible=false;

		VMR9OSD				m_vmr9OSD=new VMR9OSD();

		FullScreenState screenState=new FullScreenState();

    public GUIVideoFullscreen()
		{
			GetID=(int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
			
    }

    public override bool Init()
    {
      bool bResult=Load(GUIGraphicsContext.Skin+@"\videoFullScreen.xml");
      GetID=(int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
      return bResult;
    }
		#region settings serialisation
    void LoadSettings()
    {
			string key="movieplayer";
			if (g_Player.IsDVD)
				key="dvdplayer";

      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
				m_bMSNChatPopup = (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1);
        m_iMaxTimeOSDOnscreen=1000*xmlreader.GetValueAsInt("movieplayer","osdtimeout",5);
        string strValue=xmlreader.GetValueAsString(key,"defaultar","normal");
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
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
		#endregion

		void OnOsdAction(Action action)
		{
			if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI)) && !m_osdWindow.SubMenuVisible) // hide the OSD
			{
				lock(this)
				{ 
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,GetID,0,null);
					m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
					isOsdVisible=false;
					
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
						
						return;
					}
					else
					{
						if ( m_osdWindow.InWindow(x,y))
						{
							m_osdWindow.OnAction(action);	// route keys to OSD window
							
							return;
						}
						else
						{
							if (!m_osdWindow.SubMenuVisible)
							{
								GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,GetID,0,null);
								m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
								isOsdVisible=false;
								
							}
						}
					}
				}
				Action newAction=new Action();
				if (action.wID != Action.ActionType.ACTION_KEY_PRESSED && ActionTranslator.GetAction((int)GUIWindow.Window.WINDOW_OSD,action.m_key,ref newAction))
				{
					m_osdWindow.OnAction(newAction);	// route keys to OSD window
					
				}
				else
				{
					// route unhandled actions to OSD window
					if (!m_osdWindow.SubMenuVisible)
					{
						m_osdWindow.OnAction(action);	
						
					}
				}
			}
			return;		
		}
		void OnMsnAction(Action action)
		{
			if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI))) // hide the OSD
			{
				lock(this)
				{ 
					GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,GetID,0,null);
					m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
					m_bMSNChatVisible=false;
					
				}
				return;
			}
			if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
			{
				m_msnWindow.OnAction(action);
				
				return;
			}		
		}

    public override void OnAction(Action action)
    {
      //switch back to menu on right-click
      if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK && action.MouseButton == MouseButtons.Right)
      {
        isOsdVisible=false;
        GUIGraphicsContext.IsFullScreenVideo=false;
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
			if (action.wID==Action.ActionType.ACTION_SHOW_VOLUME)
			{
				if(m_vmr9OSD!=null)
					m_vmr9OSD.RenderVolumeOSD();
			}
			if (isOsdVisible)
			{
				OnOsdAction(action); 
				return;
			}
			else if (m_bMSNChatVisible)
			{
				OnMsnAction(action);
				return;
			}
			else if (action.wID==Action.ActionType.ACTION_MOUSE_MOVE && GUIGraphicsContext.MouseSupport )
			{
				int y =(int)action.fAmount2;
				if (y > GUIGraphicsContext.Height-100)
				{
					m_dwOSDTimeOut=DateTime.Now;
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,m_osdWindow.GetID,0,0,GetID,0,null);
					m_osdWindow.OnMessage(msg);	// Send an init msg to the OSD
					isOsdVisible=true;
					
				}
			}

			if (g_Player.IsDVD)
			{
				Action newAction=new Action();
				if (ActionTranslator.GetAction((int)GUIWindow.Window.WINDOW_DVD,action.m_key,ref newAction))
				{
					if ( g_Player.OnAction(newAction)) 
					{
						if (m_osdWindow.NeedRefresh())
						{
							needToClearScreen=true;
						}
						return;
					}
				}

				// route all unhandled actions to the dvd player
				g_Player.OnAction(action);

			}			
      
      switch (action.wID)
      {
				case Action.ActionType.ACTION_SHOW_MSN_OSD:
					if (m_bMSNChatPopup)
					{
						Log.Write("MSN CHAT:ON");     
						
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
          isOsdVisible=false;
          GUIGraphicsContext.IsFullScreenVideo=false;
          GUIWindowManager.ShowPreviousWindow();
					if(m_vmr9OSD!=null)
						m_vmr9OSD.HideBitmap();

          return;
        }

        case Action.ActionType.ACTION_ASPECT_RATIO:
        {
          m_bShowStatus=true;
					m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
					string strStatus="";
          switch (GUIGraphicsContext.ARType)
          {
            case MediaPortal.GUI.Library.Geometry.Type.Zoom:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
							strStatus="Stretch";
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Stretch:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
							strStatus="Normal";
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Normal:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
							strStatus="Original";
              break;

            case MediaPortal.GUI.Library.Geometry.Type.Original:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
							strStatus="Letterbox 4:3";
              break;

            case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
							strStatus="PanScan 4:3";
              break;

            case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
              GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
							strStatus="Zoom";
              break;
          }
          SaveSettings();
          
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
					msg.Label=strStatus; 
					OnMessage(msg);
        }
          break;
    		
        case Action.ActionType.ACTION_STEP_BACK:
        {
          if (g_Player.CanSeek)
          {
            m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
            m_bShowStep=true;
						g_Player.SeekStep(false);
						string strStatus=g_Player.GetStepDescription();
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
						msg.Label=strStatus; 
						OnMessage(msg);
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
						string strStatus=g_Player.GetStepDescription();
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
						msg.Label=strStatus; 
						OnMessage(msg);

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
          
          GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,m_osdWindow.GetID,0,0,GetID,0,null);
          m_osdWindow.OnMessage(msg);	// Send an init msg to the OSD
          isOsdVisible=true;
          
          
        }
          break;
    			
        case Action.ActionType.ACTION_SHOW_SUBTITLES:
        {	
			g_Player.EnableSubtitle = !g_Player.EnableSubtitle;
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
          GUIWindowManager.ShowPreviousWindow();
        }
          break;

          // PAUSE action is handled globally in the Application class
        case Action.ActionType.ACTION_PAUSE:
          g_Player.Pause();
          
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
            
          }
          break;

          case Action.ActionType.ACTION_FORWARD:
          {
            g_Player.Speed=Utils.GetNextForwardSpeed(g_Player.Speed);
            
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
          
        }
          break;

				case Action.ActionType.ACTION_CONTEXT_MENU:
					ShowContextMenu();
					break;
      }

      base.OnAction(action);
      
    }

		bool OnOsdMessage(GUIMessage message)
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
    public override bool OnMessage(GUIMessage message)
    {
			if (message.Message==GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
			{
				dialogNotify=(GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
				TVProgram notify=message.Object as TVProgram;
				if (notify==null) return true;
				dialogNotify.SetHeading(1016);
				dialogNotify.SetText(String.Format("{0}\n{1}",notify.Title,notify.Description));
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,notify.Channel);
				dialogNotify.SetImage( strLogo);
				dialogNotify.TimeOut=10;
				NotifyDialogVisible=true;
				dialogNotify.DoModal(GetID);
				NotifyDialogVisible=false;
			}
      
      if (isOsdVisible)
      {
				return OnOsdMessage(message);
      }

      switch (message.Message)
      {
				case GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION:
					if (m_bMSNChatVisible)
					{
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,GetID,0,null);
						m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
					}
					m_bMSNChatVisible=false;
					break;

				case GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE:
				case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
					if (isOsdVisible && m_bMSNChatPopup)
					{
						GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,GetID,0,null);
						m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						isOsdVisible=false;
						
					}

					if (!m_bMSNChatVisible && m_bMSNChatPopup && (m_msnWindow != null))
					{
						Log.Write("MSN CHAT:ON");     
						m_bMSNChatVisible=true;											
						m_msnWindow.DoModal( GetID, message );
						m_bMSNChatVisible=false;
						
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
          
          isOsdVisible=false;
          
          m_bShowStep=false;
          m_bShowStatus=false;
          m_bShowTime=false;
          m_strTimeStamp="";
          m_iTimeCodePosition=0;
          m_dwTimeStatusShowTime=0;   
					NotifyDialogVisible=false;
          
          m_UpdateTimer=DateTime.Now;
          LoadSettings(); 
          
          if (!GUIGraphicsContext.Vmr9Active)
          {
//            m_form = new FormOSD();
//            m_form.Owner = GUIGraphicsContext.form;
//            m_form.Show();            
//            GUIGraphicsContext.form.Focus();
          }
          GUIGraphicsContext.IsFullScreenVideo=true;
					ScreenStateChanged();
					UpdateGUI();
					needToClearScreen=false;
          
          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          lock(this)
          {
            if (isOsdVisible)
            {
              GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,GetID,0,null);
              m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            }
						isOsdVisible=false;

						if (m_bMSNChatVisible)
						{
							GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_msnWindow.GetID,0,0,GetID,0,null);
							m_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
						}
						m_bMSNChatVisible=false;
            base.OnMessage(message);
            
//            if (m_form!=null) 
//            {
//              m_form.Close();
//              m_form.Dispose();
//            }
//            m_form=null;
          }
          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          if (isOsdVisible) return true;
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
			
			dlg.DoModal( GetID);
			m_bDialogVisible=false;
			
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
					isOsdVisible=false;
					m_bMSNChatVisible=false;
					GUIGraphicsContext.IsFullScreenVideo=false;
					GUIWindowManager.ShowPreviousWindow();
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
			
			dlg.DoModal( GetID);
			m_bDialogVisible=false;
			
			if (dlg.SelectedId==-1) return;
			m_dwTimeStatusShowTime=(DateTime.Now.Ticks/10000);
			string strStatus="";
			switch (dlg.SelectedId)
			{
				case 942: // Stretch
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					strStatus="Stretch";
					SaveSettings();
					break;

				case 943: // Normal
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					strStatus="Normal";
					SaveSettings();
					break;

				case 944: // Original
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					strStatus="Original";
					SaveSettings();
					break;

				case 945: // Letterbox
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					strStatus="Letterbox 4:3";
					SaveSettings();
					break;

				case 946: // Pan and scan
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
					strStatus="PanScan 4:3";
					SaveSettings();
					break;
      
				case 947: // Zoom
					GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					strStatus="Zoom";
					SaveSettings();
					break;
			}
			GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
			msg.Label=strStatus; 
			OnMessage(msg);
		}


		public bool ScreenStateChanged()
		{
			bool updateGUI=false;
			if (NotifyDialogVisible != screenState.NotifyDialogVisible)
			{
				screenState.NotifyDialogVisible=NotifyDialogVisible;
				updateGUI=true;
			}

			if (g_Player.Speed != screenState.Speed)
			{
				screenState.Speed=g_Player.Speed;
				updateGUI=true;
			}
			if (g_Player.Paused != screenState.Paused)
			{
				screenState.Paused=g_Player.Paused;
				updateGUI=true;
			}
			if (isOsdVisible != screenState.OsdVisible)
			{
				screenState.OsdVisible=isOsdVisible;
				updateGUI=true;
			}
			if (m_bMSNChatVisible != screenState.MsnVisible)
			{
				screenState.MsnVisible=m_bMSNChatVisible;
				updateGUI=true;
			}
			if (m_bDialogVisible!=screenState.ContextMenuVisible)
			{
				screenState.ContextMenuVisible=m_bDialogVisible;
				updateGUI=true;
			}

			bool bStart, bEnd;
			int step=g_Player.GetSeekStep(out bStart, out bEnd);
			if (step!=screenState.SeekStep)
			{
				if (step!=0) m_bShowStep=true;
				else m_bShowStep=false;
				screenState.SeekStep=step;
				updateGUI=true;
			}
			if (m_bShowStatus!=screenState.ShowStatusLine)
			{
				screenState.ShowStatusLine=m_bShowStatus;
				updateGUI=true;
			}
			if (m_bShowTime!=screenState.ShowTime)
			{
				screenState.ShowTime=m_bShowTime;
				updateGUI=true;
			}
			if (updateGUI)
			{
				needToClearScreen=true;
			}
			return updateGUI;
		}

		void UpdateGUI()
		{
			if ( (m_bShowStep||(!isOsdVisible&& g_Player.Speed!=1) || (!isOsdVisible&& g_Player.Paused)) )
			{
				if (!isOsdVisible)
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
			HideControl(GetID,(int)Control.LABEL_ROW1);
			HideControl(GetID,(int)Control.LABEL_ROW2);
			HideControl(GetID,(int)Control.LABEL_ROW3);
			HideControl(GetID,(int)Control.BLUE_BAR);
			if (screenState.SeekStep!=0)
			{
				ShowControl(GetID,(int)Control.BLUE_BAR);
				ShowControl(GetID,(int)Control.LABEL_ROW1);
			}
			if (m_bShowStatus)
			{
				ShowControl(GetID,(int)Control.BLUE_BAR);
				ShowControl(GetID,(int)Control.LABEL_ROW1);
			}
			if (m_bShowTime)
			{
				ShowControl(GetID,(int)Control.BLUE_BAR);
				ShowControl(GetID,(int)Control.LABEL_ROW1);
			}
		}

		
		void CheckTimeOuts()
		{
			if(m_vmr9OSD!=null)
				m_vmr9OSD.CheckTimeOuts();

			if (m_bShowStatus||m_bShowStep)
			{
				long lTimeSpan=( (DateTime.Now.Ticks/10000) - m_dwTimeStatusShowTime);
				if ( lTimeSpan >=2000)
				{
					m_bShowStep=false;
					m_bShowStatus=false;
				}
			}
			if (m_bShowTime)
			{
				TimeSpan lTimeSpan=DateTime.Now - m_dwTimeCodeTimeout;
				if ( lTimeSpan.TotalMilliseconds >=2500)
				{
					m_bShowTime=false;
					m_iTimeCodePosition = 0;
					m_strTimeStamp="";
					return;
				}
			}



			// OSD Timeout?
			if (isOsdVisible && m_iMaxTimeOSDOnscreen>0)
			{
				TimeSpan ts =DateTime.Now - m_dwOSDTimeOut;
				if ( ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
				{
					//yes, then remove osd offscreen
					GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_osdWindow.GetID,0,0,GetID,0,null);
					m_osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
					isOsdVisible=false;
				}
			}
		}

		public override void Process()
		{
			CheckTimeOuts();

			if (ScreenStateChanged())
			{
				UpdateGUI();
			}
			if (!g_Player.Playing)
			{
				if (PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC  ||
					PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP)
				{
					return;
				}
				isOsdVisible=false;
				GUIWindowManager.ShowPreviousWindow();
				return;
			}
		}

		public override void Render(float timePassed)
    {
			if (GUIGraphicsContext.Vmr9Active || GUIWindowManager.IsRouted)
			{
				base.Render(timePassed); 
				if (isOsdVisible)
				{
					m_osdWindow.Render(timePassed);
				}
			}
			else
			{
				if (screenState.MsnVisible ||
					screenState.ContextMenuVisible ||
					screenState.OsdVisible ||
					screenState.Paused ||
					screenState.ShowStatusLine ||
					screenState.ShowTime || needToClearScreen || 
					g_Player.Speed!=1)
				{
					if (VMR7Util.g_vmr7!=null)
					{
						using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width,GUIGraphicsContext.Height))
						{
							using (Graphics g = Graphics.FromImage(bmp))
							{
								GUIGraphicsContext.graphics=g;
								base.Render(timePassed);
								RenderForm(timePassed);
								GUIGraphicsContext.graphics=null;
								screenState.wasVMRBitmapVisible=true;
								VMR7Util.g_vmr7.SaveBitmap(bmp,true,true,0.8f);
							}
						}
					}
				}
				else
				{
					if (screenState.wasVMRBitmapVisible)
					{
						screenState.wasVMRBitmapVisible=false;
						if (VMR7Util.g_vmr7!=null)
						{
							VMR7Util.g_vmr7.SaveBitmap(null,false,false,0.8f);
						}
					}
				}
			}
    }

    bool OSDVisible() 
    {
      return isOsdVisible;
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
				GUIMessage msg= new GUIMessage  (GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Control.LABEL_ROW1,0,0,null); 
				msg.Label=m_strTimeStamp; 
				OnMessage(msg);
	    }
    }
    
    public void RenderForm(float timePassed)
    {
      if (!g_Player.Playing) return;
			else
			{
				if (needToClearScreen)
				{
					needToClearScreen=false;
					GUIGraphicsContext.graphics.Clear(Color.Black);
				}
				base.Render(timePassed); 
				if (isOsdVisible)
				{
					m_osdWindow.Render(timePassed);
				}
			}		
		}

		#region helper functions
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
      if (isOsdVisible) 
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
      if (isOsdVisible) 
      {
        return m_osdWindow.GetControl(iControlId);
      }
			if (m_bMSNChatVisible)
			{
				return m_msnWindow.GetControl(iControlId);
			}

      return base.GetControl(iControlId);
    }
		#endregion
  }
}