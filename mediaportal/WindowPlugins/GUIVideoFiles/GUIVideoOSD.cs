using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Video.Database;
using MediaPortal.Util;
using MediaPortal.Playlists;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// 
  /// </summary>
  /// 

  public class GUIVideoOSD: GUIWindow
  {
    enum Controls {
      OSD_VIDEOPROGRESS =1
      , OSD_SKIPBWD =210
      , OSD_REWIND =211
      , OSD_STOP =212
      , OSD_PLAY =213
      , OSD_FFWD =214
      , OSD_SKIPFWD =215
      , OSD_MUTE =216
      //, OSD_SYNC =217 - not used
      , OSD_SUBTITLES =218
      , OSD_BOOKMARKS =219
      , OSD_VIDEO =220
      , OSD_AUDIO =221
      , OSD_VOLUMESLIDER =400
      , OSD_AVDELAY =500
      , OSD_AVDELAY_LABEL =550
      , OSD_AUDIOSTREAM_LIST =501
      , OSD_CREATEBOOKMARK =600
      , OSD_BOOKMARKS_LIST =601
      , OSD_BOOKMARKS_LIST_LABEL =650
      , OSD_CLEARBOOKMARKS =602
      , OSD_VIDEOPOS =700
      , OSD_VIDEOPOS_LABEL =750
      , OSD_NONINTERLEAVED =701
      , OSD_NOCACHE =702
      , OSD_ADJFRAMERATE =703
      , OSD_BRIGHTNESS =704
      , OSD_BRIGHTNESSLABEL =752
      , OSD_CONTRAST =705
      , OSD_CONTRASTLABEL =753
      , OSD_GAMMA =706
      , OSD_GAMMALABEL =754
      , OSD_SUBTITLE_DELAY =800
      , OSD_SUBTITLE_DELAY_LABEL =850
      , OSD_SUBTITLE_ONOFF =801
      , OSD_SUBTITLE_LIST =802
      , OSD_TIMEINFO =100
      , OSD_SUBMENU_BG_VOL =300
      //, OSD_SUBMENU_BG_SYNC 301	- not used
      , OSD_SUBMENU_BG_SUBTITLES =302
      , OSD_SUBMENU_BG_BOOKMARKS =303
      , OSD_SUBMENU_BG_VIDEO =304
      , OSD_SUBMENU_BG_AUDIO =305
      , OSD_SUBMENU_NIB =350
    };
  bool m_bSubMenuOn = false;
  int m_iActiveMenu = 0;
  int m_iActiveMenuButtonID = 0;
  int m_iCurrentBookmark = 0;
  bool m_bNeedRefresh=false;

  public GUIVideoOSD()
  {
  }

  public override bool Init()
  {
    bool bResult=Load (GUIGraphicsContext.Skin+@"\videoOSD.xml");
    GetID=(int)GUIWindow.Window.WINDOW_OSD;
    return bResult;
  }

  public bool SubMenuVisible
  {
    get { return m_bSubMenuOn;}
  }
  public override bool SupportsDelayedLoad
  {
    get { return false;}
  }    

  public override void Render(long timePassed)
  {
    base.Render(timePassed);		// render our controls to the screen
  }
  void HideControl (int dwSenderId, int dwControlID) 
  {
    GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID, dwSenderId, dwControlID,0,0,null); 
    OnMessage(msg); 
  }
  void ShowControl (int dwSenderId, int dwControlID) 
  {
    GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID, dwSenderId, dwControlID,0,0,null); 
    OnMessage(msg); 
  }

  void FocusControl (int dwSenderId, int dwControlID, int dwParam) 
  {
    GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, dwSenderId, dwControlID, dwParam,0,null); 
    OnMessage(msg); 
  }


  public override void OnAction(Action action)
  {
    switch (action.wID)
    {
      case Action.ActionType.ACTION_OSD_SHOW_LEFT:
      break;
      case Action.ActionType.ACTION_OSD_SHOW_RIGHT:
      break;
      case Action.ActionType.ACTION_OSD_SHOW_UP:
      break;
      case Action.ActionType.ACTION_OSD_SHOW_DOWN:
      break;
      case Action.ActionType.ACTION_OSD_SHOW_SELECT:
      break;

      case Action.ActionType.ACTION_OSD_HIDESUBMENU:
      break;
      case Action.ActionType.ACTION_SHOW_OSD:
      {
        if (m_bSubMenuOn)						// is sub menu on?
        {
          FocusControl(GetID, m_iActiveMenuButtonID, 0);	// set focus to last menu button
          ToggleSubMenu(0, m_iActiveMenu);						// hide the currently active sub-menu
        }
        return;
      }
		    
      case Action.ActionType.ACTION_PAUSE:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_PLAY,(int)Controls.OSD_PLAY,0,0,null);
        OnMessage(msgSet);
        return;
      }
  		    
      case Action.ActionType.ACTION_PLAY:
      case Action.ActionType.ACTION_MUSIC_PLAY:
      {
        g_Player.Speed=1;		// drop back to single speed
        ToggleButton((int)Controls.OSD_REWIND, false);	// pop all the relevant
        ToggleButton((int)Controls.OSD_FFWD, false);		// buttons back to
        ToggleButton((int)Controls.OSD_PLAY, false);		// their up state

        if (g_Player.Paused)
        {
          g_Player.Pause();
          ToggleButton((int)Controls.OSD_PLAY, false);		// make sure play button is up (so it shows the play symbol)
        }          
        return;
      }
  		    

      case Action.ActionType.ACTION_STOP:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_STOP,(int)Controls.OSD_STOP,0,0,null);
        OnMessage(msgSet);
        return;
      }
  		    

      case Action.ActionType.ACTION_FORWARD:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_FFWD,(int)Controls.OSD_FFWD,0,0,null);
        OnMessage(msgSet);
        return;
      }
  		    

      case Action.ActionType.ACTION_REWIND:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_REWIND,(int)Controls.OSD_REWIND,0,0,null);
        OnMessage(msgSet);
        return;
      }
  		    

      case Action.ActionType.ACTION_OSD_SHOW_VALUE_PLUS:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_SKIPFWD,(int)Controls.OSD_SKIPFWD,0,0,null);
        OnMessage(msgSet);
        return;
      }
  		    
      case Action.ActionType.ACTION_OSD_SHOW_VALUE_MIN:
      {
        // push a message through to this window to handle the remote control button
        GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,GetID,(int)Controls.OSD_SKIPBWD,(int)Controls.OSD_SKIPBWD,0,0,null);
        OnMessage(msgSet);
        return;
      }
    }

    base.OnAction(action);
  }

  public override bool OnMessage(GUIMessage message)
  {
    switch ( message.Message )
    {
      case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:	// fired when OSD is hidden
      {
        //if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(true);
        // following line should stay. Problems with OSD not
        // appearing are already fixed elsewhere
        FreeResources();
        return true;
      }
		    

      case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:	// fired when OSD is shown
      {
        // following line should stay. Problems with OSD not
        // appearing are already fixed elsewhere
        AllocResources();
        // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
        ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
        m_bSubMenuOn=false;
        m_iActiveMenuButtonID=0;
        m_iActiveMenu=0;
        m_bNeedRefresh=false;
        Reset();
        FocusControl(GetID, (int)Controls.OSD_PLAY, 0);	// set focus to play button by default when window is shown

        return true;
      }
      		    
      case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
        goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

      case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
      {
        if (message.SenderControlId == 13) return true;
      }
      break;

      case GUIMessage.MessageType.GUI_MSG_CLICKED:
      {
        int iControl=message.SenderControlId;		// get the ID of the control sending us a message
        if (iControl >= (int)Controls.OSD_VOLUMESLIDER)		// one of the settings (sub menu) controls is sending us a message
        {
          Handle_ControlSetting(iControl, message.Param1);
        }

        if (iControl == (int)Controls.OSD_PLAY)
        {
            
          //TODO
          int iSpeed=g_Player.Speed;
          if ( iSpeed != 1)	// we're in ffwd or rewind mode
          {
            g_Player.Speed=1;		// drop back to single speed
            ToggleButton((int)Controls.OSD_REWIND, false);	// pop all the relevant
            ToggleButton((int)Controls.OSD_FFWD, false);		// buttons back to
            ToggleButton((int)Controls.OSD_PLAY, false);		// their up state
          }
          else
          {
            g_Player.Pause();	// Pause/Un-Pause playback
            if (g_Player.Paused)
              ToggleButton((int)Controls.OSD_PLAY, true);		// make sure play button is down (so it shows the pause symbol)
            else
              ToggleButton((int)Controls.OSD_PLAY, false);		// make sure play button is up (so it shows the play symbol)
          }
        }

        if (iControl == (int)Controls.OSD_STOP)
        {
          if (m_bSubMenuOn)	// sub menu currently active ?
          {
            FocusControl(GetID, m_iActiveMenuButtonID, 0);	// set focus to last menu button
            ToggleSubMenu(0, m_iActiveMenu);						// hide the currently active sub-menu
          }
					//g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
					Log.Write("GUIVideoOSD:stop");
          g_Player.Stop();						// close our media
          //GUIWindowManager.PreviousWindow();							// go back to the previous window
        }

        if (iControl == (int)Controls.OSD_REWIND)
        {
          if (g_Player.Paused)
            g_Player.Pause();	// Unpause playback
            
          g_Player.Speed=Utils.GetNextRewindSpeed(g_Player.Speed);
          if (g_Player.Speed < 1)	// are we not playing back at normal speed
          {
            ToggleButton((int)Controls.OSD_REWIND, true);		// make sure out button is in the down position
            ToggleButton((int)Controls.OSD_FFWD, false);		// pop the button back to it's up state
          }
          else
          {
            ToggleButton((int)Controls.OSD_REWIND, false);	// pop the button back to it's up state
            if (g_Player.Speed==1)
              ToggleButton((int)Controls.OSD_FFWD, false);		// pop the button back to it's up state
          }
        }

        if (iControl == (int)Controls.OSD_FFWD)
        {
          if (g_Player.Paused)
            g_Player.Pause();	// Unpause playback
            
          g_Player.Speed=Utils.GetNextForwardSpeed(g_Player.Speed);
          if (g_Player.Speed > 1)	// are we not playing back at normal speed
          {
            ToggleButton((int)Controls.OSD_FFWD, true);		// make sure out button is in the down position
            ToggleButton((int)Controls.OSD_REWIND, false);	// pop the button back to it's up state
          }
          else
          {
            ToggleButton((int)Controls.OSD_FFWD, false);		// pop the button back to it's up state
            if (g_Player.Speed==1)
              ToggleButton((int)Controls.OSD_REWIND, false);		// pop the button back to it's up state

          }
        }

        if (iControl == (int)Controls.OSD_SKIPBWD)
        {
            
          PlayListPlayer.PlayPrevious();
          return true;

        }

        if (iControl == (int)Controls.OSD_SKIPFWD)
        {   
          PlayListPlayer.PlayNext(true);
          return true;
        }

        if (iControl == (int)Controls.OSD_MUTE)
        {
  
            ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VOL);			// hide or show the sub-menu
            if (m_bSubMenuOn)										// is sub menu on?
            {
              ShowControl(GetID, (int)Controls.OSD_VOLUMESLIDER);		// show the volume control
              FocusControl(GetID, (int)Controls.OSD_VOLUMESLIDER, 0);	// set focus to it
            }
            else													// sub menu is off
            {
              FocusControl(GetID, (int)Controls.OSD_MUTE, 0);			// set focus to the mute button
            }
           
        }

        /* not used
        if (iControl == (int)Controls.OSD_SYNC)
        {
          ToggleSubMenu(iControl, Controls.OSD_SUBMENU_BG_SYNC);		// hide or show the sub-menu
        }
        */

        if (iControl == (int)Controls.OSD_SUBTITLES)
        {
  
            ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_SUBTITLES);	// hide or show the sub-menu
            if (m_bSubMenuOn)
            {
              // set the controls values
              //SetSliderValue(-10.0f, 10.0f, g_application.m_pPlayer.GetSubTitleDelay(), Controls.OSD_SUBTITLE_DELAY);
              SetCheckmarkValue( g_Player.EnableSubtitle, (int)Controls.OSD_SUBTITLE_ONOFF);
              // show the controls on this sub menu
              ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
              ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
              ShowControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
              ShowControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

              FocusControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY, 0);	// set focus to the first control in our group
              PopulateSubTitles();	// populate the list control with subtitles for this video
            }
        }

        if (iControl == (int)Controls.OSD_BOOKMARKS)
        {
  
            ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_BOOKMARKS);	// hide or show the sub-menu
            if (m_bSubMenuOn)
            {
              // show the controls on this sub menu
              ShowControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
              ShowControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
              ShowControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
              ShowControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);

              FocusControl(GetID, (int)Controls.OSD_CREATEBOOKMARK, 0);	// set focus to the first control in our group
              PopulateBookmarks();	// populate the list control with bookmarks for this video
            }
        }

        if (iControl == (int)Controls.OSD_VIDEO)
        {
  
            ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VIDEO);		// hide or show the sub-menu
            if (m_bSubMenuOn)						// is sub menu on?
            {
              // set the controls values
              float fPercent=(float)(100*(g_Player.CurrentPosition/g_Player.Duration));
              SetSliderValue(0.0f, 100.0f, (float) fPercent, (int)Controls.OSD_VIDEOPOS);
              

             // SetCheckmarkValue(g_stSettings.m_bNonInterleaved, Controls.OSD_NONINTERLEAVED);
              //SetCheckmarkValue(g_stSettings.m_bNoCache, Controls.OSD_NOCACHE);
              //SetCheckmarkValue(g_stSettings.m_bFrameRateConversions, Controls.OSD_ADJFRAMERATE);

							UpdateGammaContrastBrightness();
              // show the controls on this sub menu
              ShowControl(GetID, (int)Controls.OSD_VIDEOPOS);
              ShowControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
              ShowControl(GetID, (int)Controls.OSD_NOCACHE);
              ShowControl(GetID, (int)Controls.OSD_ADJFRAMERATE);
              ShowControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
              ShowControl(GetID, (int)Controls.OSD_BRIGHTNESS);
              ShowControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);
              ShowControl(GetID, (int)Controls.OSD_CONTRAST);
              ShowControl(GetID, (int)Controls.OSD_CONTRASTLABEL);
              ShowControl(GetID, (int)Controls.OSD_GAMMA);
              ShowControl(GetID, (int)Controls.OSD_GAMMALABEL);
              FocusControl(GetID, (int)Controls.OSD_VIDEOPOS, 0);	// set focus to the first control in our group
            }
        }

        if (iControl == (int)Controls.OSD_AUDIO)
        {
  
            ToggleSubMenu( iControl, (int)Controls.OSD_SUBMENU_BG_AUDIO);		// hide or show the sub-menu
            if (m_bSubMenuOn)						// is sub menu on?
            {
              // set the controls values
              //SetSliderValue(-10.0f, 10.0f, g_application.m_pPlayer.GetAVDelay(), Controls.OSD_AVDELAY);
    				
              // show the controls on this sub menu
              ShowControl(GetID, (int)Controls.OSD_AVDELAY);
              ShowControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);
              ShowControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);

              FocusControl(GetID, (int)Controls.OSD_AVDELAY, 0);	// set focus to the first control in our group
              PopulateAudioStreams();		// populate the list control with audio streams for this video
            }
        }

        return true;
      }
    }
    return base.OnMessage(message);
  }
		

		void UpdateGammaContrastBrightness()
		{
			float fBrightNess=(float)GUIGraphicsContext.Brightness;
			float fContrast=(float)GUIGraphicsContext.Contrast;
			float fGamma=(float)GUIGraphicsContext.Gamma;
			float fSaturation=(float)GUIGraphicsContext.Saturation;
			float fSharpness=(float)GUIGraphicsContext.Sharpness;
              
			SetSliderValue(0.0f, 100.0f, (float) fBrightNess, (int)Controls.OSD_BRIGHTNESS);
			SetSliderValue(0.0f, 100.0f, (float) fContrast, (int)Controls.OSD_CONTRAST);
			SetSliderValue(0.0f, 100.0f, (float) fGamma, (int)Controls.OSD_GAMMA);
		}


  void SetVideoProgress()
  {
    
    if (g_Player.Playing)
    {
      

      int iValue=g_Player.Volume;
      GUISliderControl pSlider = (GUISliderControl)GetControl((int)Controls.OSD_VOLUMESLIDER);
      if (null!=pSlider) pSlider.Percentage=iValue;			// Update our progress bar accordingly ...
    }
  }


  void ToggleButton(int iButtonID, bool bSelected)
  {
    GUIControl pControl = (GUIControl)GetControl(iButtonID);

    if (pControl!=null)
    {
      if (bSelected)	// do we want the button to appear down?
      {
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID,0, iButtonID,0,0,null);
        OnMessage(msg);
      }
      else			// or appear up?
      {
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID,0, iButtonID,0,0,null);
        OnMessage(msg);
      }
    }
  }

  void ToggleSubMenu(int iButtonID, int iBackID)
  {
    int iX, iY;

    GUIImage pImgNib = GetControl((int)Controls.OSD_SUBMENU_NIB) as GUIImage;	// pointer to the nib graphic
    GUIImage pImgBG = GetControl(iBackID) as GUIImage;			// pointer to the background graphic
    GUIToggleButtonControl pButton = GetControl(iButtonID) as GUIToggleButtonControl;	// pointer to the OSD menu button
    
    // check to see if we are currently showing a sub-menu and it's position is different
    if (m_bSubMenuOn && iBackID != m_iActiveMenu)
    {
      m_bNeedRefresh=true;
      m_bSubMenuOn = false;	// toggle it ready for the new menu requested
    }

    // Get button position
    if (pButton!=null)	
    {
      iX = (pButton.XPosition + (pButton.Width / 2));	// center of button
      iY = pButton.YPosition;
    }
    else
    {
      iX = 0;
      iY = 0;
    }

    // Set nib position
    if (pImgNib!=null && pImgBG!=null)
    {		
      pImgNib.SetPosition(iX - (pImgNib.TextureWidth / 2), iY - pImgNib.TextureHeight);

      if (!m_bSubMenuOn)	// sub menu not currently showing?
      {
        pImgNib.IsVisible=true;		// make it show
        pImgBG.IsVisible=true;		// make it show
      }
      else
      {
        pImgNib.IsVisible=false;		// hide it
        pImgBG.IsVisible=false;		// hide it
      }
    }

    m_bSubMenuOn = !m_bSubMenuOn;		// toggle sub menu visible status
    if (!m_bSubMenuOn) m_bNeedRefresh=true;
    // Set all sub menu controls to hidden
    HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
    HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
    HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
    HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
    HideControl(GetID, (int)Controls.OSD_AVDELAY);
    HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
    HideControl(GetID, (int)Controls.OSD_NOCACHE);
    HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);
    HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);

    HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
    HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

    HideControl(GetID, (int)Controls.OSD_GAMMA);
    HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

    HideControl(GetID, (int)Controls.OSD_CONTRAST);
    HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

    HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
    HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
    HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
    HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
    HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
    HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
    HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
    HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

    // Reset the other buttons back to up except the one that's active
    if (iButtonID != (int)Controls.OSD_MUTE) ToggleButton((int)Controls.OSD_MUTE, false);
    //if (iButtonID != (int)Controls.OSD_SYNC) ToggleButton((int)Controls.OSD_SYNC, false); - not used
    if (iButtonID != (int)Controls.OSD_SUBTITLES) ToggleButton((int)Controls.OSD_SUBTITLES, false);
    if (iButtonID != (int)Controls.OSD_BOOKMARKS) ToggleButton((int)Controls.OSD_BOOKMARKS, false);
    if (iButtonID != (int)Controls.OSD_VIDEO) ToggleButton((int)Controls.OSD_VIDEO, false);
    if (iButtonID != (int)Controls.OSD_AUDIO) ToggleButton((int)Controls.OSD_AUDIO, false);

    m_iActiveMenu = iBackID;
    m_iActiveMenuButtonID = iButtonID;
  }

  void SetSliderValue(float fMin, float fMax, float fValue, int iControlID)
  {
    GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);

    if (null!=pControl)
    {
      switch(pControl.SpinType)
      {
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
          pControl.SetFloatRange(fMin, fMax);
          pControl.FloatValue=fValue;
        break;

        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
        pControl.SetRange((int) fMin, (int) fMax);
        pControl.IntValue=(int) fValue;
        break;

        default:
        pControl.Percentage=(int) fValue;
        break;
      }
    }
  }

  void SetCheckmarkValue(bool bValue, int iControlID)
  {
    if (bValue)
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED,GetID,0,iControlID,0,0,null);
      OnMessage(msg);
    }
    else
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED,GetID,0,iControlID,0,0,null);
      OnMessage(msg);
    }
  }

  void Handle_ControlSetting(int iControlID, long wID)
  {
    
      string strMovie=g_Player.CurrentFile;
//      CVideoDatabase dbs;
  //    VECBOOKMARKS bookmarks;

      switch (iControlID)
      {
        case (int)Controls.OSD_VOLUMESLIDER:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (null!=pControl)
          {
            // no volume control yet so no code here at the moment
            if (g_Player.Playing)
            {
              int iPercentage=pControl.Percentage;
              g_Player.Volume=iPercentage;
            }
          }
        }
        break;

        case (int)Controls.OSD_VIDEOPOS:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (null!=pControl)
          {
            // Set mplayer's seek position to the percentage requested by the user
            g_Player.SeekAsolutePercentage(pControl.Percentage);
          }
        }
        break;
        
        case (int)Controls.OSD_BRIGHTNESS:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (null!=pControl)
          {
            // Set mplayer's seek position to the percentage requested by the user
            GUIGraphicsContext.Brightness=pControl.Percentage;
						UpdateGammaContrastBrightness();
          }
        }
        break;
        case (int)Controls.OSD_CONTRAST:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (null!=pControl)
          {
            // Set mplayer's seek position to the percentage requested by the user
            GUIGraphicsContext.Contrast=pControl.Percentage;
						UpdateGammaContrastBrightness();
          }
        }
        break;
        case (int)Controls.OSD_GAMMA:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (null!=pControl)
          {
            // Set mplayer's seek position to the percentage requested by the user
            GUIGraphicsContext.Gamma=pControl.Percentage;
						UpdateGammaContrastBrightness();
          }
        }
        break;
        case (int)Controls.OSD_AUDIOSTREAM_LIST:
        {
          if (wID!=0)	// check to see if list control has an action ID, remote can cause 0 based events
          {
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.OSD_AUDIOSTREAM_LIST,0,0,null);
            OnMessage(msg);
            // only change the audio stream if a different one has been asked for
            if (g_Player.CurrentAudioStream != msg.Param1)	
            {
              g_Player.CurrentAudioStream = msg.Param1;				// Set the audio stream to the one selected
              //ToggleSubMenu(0, m_iActiveMenu);						// hide the currently active sub-menu
              PopulateAudioStreams();
            }
          }
        }
        break;
/*
        case Controls.OSD_AVDELAY:
        {
          GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
          if (pControl)
          {
            // Set the AV Delay
            g_application.m_pPlayer.SetAVDelay(pControl.GetFloatValue());
          }
        }
        break;

        case Controls.OSD_NONINTERLEAVED:
        {
          g_stSettings.m_bNonInterleaved=!g_stSettings.m_bNonInterleaved;
          m_bSubMenuOn = false;										// hide the sub menu
          m_bNeedRefresh=true;
          g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
          g_application.Restart(true);								// restart to make the new setting active
        }
        break;

        case Controls.OSD_NOCACHE:
        {
          g_stSettings.m_bNoCache=!g_stSettings.m_bNoCache;
          m_bSubMenuOn = false;										// hide the sub menu
          m_bNeedRefresh=true;
          g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
          g_application.Restart(true);								// restart to make the new setting active
        }
        break;

        case Controls.OSD_ADJFRAMERATE:
        {
          g_stSettings.m_bFrameRateConversions=!g_stSettings.m_bFrameRateConversions;
          m_bSubMenuOn = false;										// hide the sub menu
          m_bNeedRefresh=true;
          OutputDebugString("OSD:RESTART4\n");
          g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
          g_application.Restart(true);								// restart to make the new setting active
        }
        break;
*/
        case (int)Controls.OSD_CREATEBOOKMARK:
        {
          double dCurTime=g_Player.CurrentPosition;			// get the current playing time position

          VideoDatabase.AddBookMarkToMovie(g_Player.CurrentFile, (float)dCurTime);				// add the current timestamp
          PopulateBookmarks();										// refresh our list control
        }
        break;

        case (int)Controls.OSD_BOOKMARKS_LIST:
        {
          if (wID!=0)	// check to see if list control has an action ID, remote can cause 0 based events
          {
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.OSD_BOOKMARKS_LIST,0,0,null);
            OnMessage(msg);
            m_iCurrentBookmark = msg.Param1;					// index of bookmark user selected


            ArrayList bookmarks = new ArrayList();
            VideoDatabase.GetBookMarksForMovie(strMovie, ref bookmarks);			// load the stored bookmarks
            if (bookmarks.Count<=0) return;						// no bookmarks? leave if so ...

            g_Player.SeekAbsolute((double) bookmarks[m_iCurrentBookmark]);	// set mplayers play position
          }
        }
        break;

        case (int)Controls.OSD_CLEARBOOKMARKS:
        {
          VideoDatabase.ClearBookMarksOfMovie(g_Player.CurrentFile);					// empty the bookmarks table for this movie
          m_iCurrentBookmark=0;									// reset current bookmark
          PopulateBookmarks();									// refresh our list control
        }
        break;
/*
          case Controls.OSD_SUBTITLE_DELAY:
          {
            GUISliderControl pControl=(GUISliderControl)GetControl(iControlID);
            if (pControl)
            {
              // Set the subtitle delay
              g_application.m_pPlayer.SetSubTittleDelay(pControl.GetFloatValue());
            }
          }
          break;
*/
          case (int)Controls.OSD_SUBTITLE_ONOFF:
          {
            g_Player.EnableSubtitle=!g_Player.EnableSubtitle;
          }
          break;

          case (int)Controls.OSD_SUBTITLE_LIST:
          {
            if (wID!=0)	// check to see if list control has an action ID, remote can cause 0 based events
            {
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.OSD_SUBTITLE_LIST,0,0,null);
              OnMessage(msg);								// retrieve the selected list item
              g_Player.CurrentSubtitleStream=msg.Param1;		// set the current subtitle
              PopulateSubTitles();
            }
          }
          break;
      }
    }

    void PopulateBookmarks()
    {
      ArrayList bookmarks = new ArrayList();
      string strMovie=g_Player.CurrentFile;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl=(GUIListControl)GetControl((int)Controls.OSD_BOOKMARKS_LIST);
      if (null==pControl) pControl.SetPageControlVisible(false);

      VideoDatabase.AddMovieFile(strMovie);
      // open the d/b and retrieve the bookmarks for the current movie
      VideoDatabase.GetBookMarksForMovie(strMovie, ref bookmarks);

      // empty the list ready for population
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET,GetID,0,(int)Controls.OSD_BOOKMARKS_LIST,0,0,null);
      OnMessage(msg);

      // cycle through each stored bookmark and add it to our list control
      for (int i=0; i < (int)(bookmarks.Count); ++i)
      {
        string strItem;
        double fTime=(double)bookmarks[i];
        long lPTS1 = (long)(fTime);
        int hh = (int)(lPTS1 / 3600) % 100;
        int mm = (int)((lPTS1 / 60) % 60);
        int	ss = (int)((lPTS1 /  1) % 60);
        strItem=String.Format("{0:00}.   {1:00}:{2:00}:{3:00}",i+1,hh,mm,ss);

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label=strItem;

        // add it ...
        GUIMessage msg2=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD,GetID,0,(int)Controls.OSD_BOOKMARKS_LIST,0,0,pItem);
        OnMessage(msg2);    
      }

      // set the currently active bookmark as the selected item in the list control
      GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT,GetID,0,(int)Controls.OSD_BOOKMARKS_LIST,m_iCurrentBookmark,0,null);
      OnMessage(msgSet);
    }

    void PopulateAudioStreams()
    {
      
      // get the number of audio strams for the current movie
      int iValue=g_Player.AudioStreams;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl=(GUIListControl)GetControl((int)Controls.OSD_AUDIOSTREAM_LIST);
      if (null!=pControl) pControl.SetPageControlVisible(false);

      // empty the list ready for population
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET,GetID,0,(int)Controls.OSD_AUDIOSTREAM_LIST,0,0,null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(460);					// "Audio Stream"
      string strActiveLabel = GUILocalizeStrings.Get(461);		// "[active]"

      // cycle through each audio stream and add it to our list control
      for (int i=0; i < iValue; ++i)
      {
        string strItem;
        string strLang=g_Player.AudioLanguage(i);
        int ipos=strLang.IndexOf("(");
        if (ipos > 0) strLang=strLang.Substring(0,ipos);
        if (g_Player.CurrentAudioStream == i)
        {
          // formats to 'Audio Stream X [active]'
          strItem=String.Format(strLang + "  " + strActiveLabel);	// this audio stream is active, show as such
        }
        else
        {
          // formats to 'Audio Stream X'
          strItem=String.Format(strLang );
        }

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label=strItem;

        // add it ...
        GUIMessage msg2=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD,GetID,0,(int)Controls.OSD_AUDIOSTREAM_LIST,0,0,pItem);
        OnMessage(msg2);    
      }

      // set the current active audio stream as the selected item in the list control
      GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT,GetID,0,(int)Controls.OSD_AUDIOSTREAM_LIST,g_Player.CurrentAudioStream,0,null);
      OnMessage(msgSet);
    }

    void PopulateSubTitles()
    {

      // get the number of subtitles in the current movie
      int iValue=g_Player.SubtitleStreams;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl=(GUIListControl)GetControl((int)Controls.OSD_SUBTITLE_LIST);
      if (null!=pControl) pControl.SetPageControlVisible(false);

      // empty the list ready for population
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET,GetID,0,(int)Controls.OSD_SUBTITLE_LIST,0,0,null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(462);					// "Subtitle"
      string strActiveLabel = GUILocalizeStrings.Get(461);		// "[active]"

      // cycle through each subtitle and add it to our list control
      for (int i=0; i < iValue; ++i)
      {
        string strItem;
        string strLang=g_Player.SubtitleLanguage(i);
        int ipos=strLang.IndexOf("(");
        if (ipos > 0) strLang=strLang.Substring(0,ipos);
        if (g_Player.CurrentSubtitleStream == i)		// this subtitle is active, show as such
        {
          // formats to 'Subtitle X [active]'
          strItem=String.Format(strLang + " " + strActiveLabel);	// this audio stream is active, show as such
        }
        else
        {
          // formats to 'Subtitle X'
          strItem=String.Format(strLang );
        }

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label=strItem;

        // add it ...
        GUIMessage msg2=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD,GetID,0,(int)Controls.OSD_SUBTITLE_LIST,0,0,pItem);
        OnMessage(msg2);    
      }

      // set the current active subtitle as the selected item in the list control
      GUIMessage msgSet=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT,GetID,0,(int)Controls.OSD_SUBTITLE_LIST,g_Player.CurrentSubtitleStream,0,null);
      OnMessage(msgSet);
    }

    public override void	 ResetAllControls()
    {
      //reset all
      
      bool bOffScreen=false;
      int iCalibrationY=GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin=0;

      foreach (CPosition pos in m_vecPositions)
      {
        pos.control.SetPosition((int)pos.XPos,(int)pos.YPos+iCalibrationY);
      }
      foreach (CPosition pos in m_vecPositions)
      {
        GUIControl pControl= pos.control;

        int dwPosY=pControl.YPosition;
        if (pControl.IsVisible)
        {
          if ( dwPosY < iTop)
          {
            int iSize=iTop-dwPosY;
            if ( iSize > iMin) iMin=iSize;
            bOffScreen=true;
          }
        }
      }
      if (bOffScreen) 
      {

        foreach (CPosition pos in m_vecPositions)
        {
          GUIControl pControl= pos.control;
          int dwPosX=pControl.XPosition;
          int dwPosY=pControl.YPosition;
          if ( dwPosY < (int)100)
          {
            dwPosY+=Math.Abs(iMin);
            pControl.SetPosition(dwPosX,dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }

    void Reset()
    {
      // Set all sub menu controls to hidden

      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_AUDIO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VIDEO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_BOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_SUBTITLES);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VOL);


      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
      HideControl(GetID, (int)Controls.OSD_AVDELAY);
      HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
      HideControl(GetID, (int)Controls.OSD_NOCACHE);
      HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);
      HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);

      HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
      HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_GAMMA);
      HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

      HideControl(GetID, (int)Controls.OSD_CONTRAST);
      HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

      HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
      HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

      ToggleButton((int)Controls.OSD_MUTE, false);
      ToggleButton((int)Controls.OSD_SUBTITLES, false);
      ToggleButton((int)Controls.OSD_BOOKMARKS, false);
      ToggleButton((int)Controls.OSD_VIDEO, false);
      ToggleButton((int)Controls.OSD_AUDIO, false);

      ToggleButton((int)Controls.OSD_REWIND, false);	// pop all the relevant
      ToggleButton((int)Controls.OSD_FFWD, false);		// buttons back to
      ToggleButton((int)Controls.OSD_PLAY, false);		// their up state

      ToggleButton((int)Controls.OSD_SKIPBWD, false);	// pop all the relevant
      ToggleButton((int)Controls.OSD_STOP, false);		// buttons back to
      ToggleButton((int)Controls.OSD_SKIPFWD, false);		// their up state
      ToggleButton((int)Controls.OSD_MUTE, false);		// their up state

      ShowControl(GetID, (int)Controls.OSD_VIDEOPROGRESS);

    }
    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh) 
      {
        m_bNeedRefresh=false;
        return true;
      }
      return false;
    }
    public bool InWindow(int x,int y)
    {
      for (int i=0; i < m_vecControls.Count;++i)
      {
        GUIControl control =(GUIControl )m_vecControls[i];
        int controlID;
        if (control.IsVisible)
        {
          if (control.InControl(x, y, out controlID))
          {
            return true;
          }
        }
      }
      return false;
    }
	}
}
  