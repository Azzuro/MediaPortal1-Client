using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogFileStacking : GUIWindow
  {

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    int m_iSelectedFile=-1;
    int m_iFrames=-1;
    int m_iNumberOfFiles=0;
    bool m_bPrevOverlay;
    public GUIDialogFileStacking()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\dialogFileStacking.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return true;}
    }    
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members
    public void RenderDlg()
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render();

      GUIFontManager.Present();
      // render this dialog box
      base.Render();
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
	
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          GUIGraphicsContext.Overlay=m_bPrevOverlay;		
          FreeResources();
          DeInitControls();

          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          m_bPrevOverlay=GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          GUIGraphicsContext.Overlay=false;
          m_iSelectedFile=-1;
          m_iFrames=0;
			
          // enable the CD's
          for (int i=1;  i <= m_iNumberOfFiles; ++i)
          {
            EnableControl(GetID,i);
            ShowControl(GetID,i);
          }

          // disable CD's we dont use
          for (int i=m_iNumberOfFiles+1;  i <= 40; ++i)
          {
            HideControl(GetID, i);
            DisableControl(GetID, i);
          }
        }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          m_iSelectedFile=message.SenderControlId;
          Close();
        }
          break;
      }

      return base.OnMessage(message);
    }


    public override void Render()
    {
      if (m_iFrames <=25)
      {
        // slide in...
        int dwScreenWidth=GUIGraphicsContext.Width;
        for (int i=1;  i <= m_iNumberOfFiles; ++i)
        {
          GUIControl pControl=(GUIControl)GetControl(i);
          if (null!=pControl)
          {
            int dwEndPos     = dwScreenWidth - ((m_iNumberOfFiles-i)*32)-140;
            int dwStartPos   = dwScreenWidth;
            float fStep= (float)(dwStartPos - dwEndPos);
            fStep/=25.0f;
            fStep*=(float)m_iFrames;
            int dwPosX = (int) ( ((float)dwStartPos)-fStep );
            pControl.SetPosition( dwPosX, pControl.YPosition );
          }
        }
        m_iFrames++;
      }

      RenderDlg();
    }


    public int SelectedFile 
    {
                                           
      get { return m_iSelectedFile;}
    }
    public void SetNumberOfFiles(int iFiles)
		{
				LoadSkin();
			AllocResources();
			InitControls();

      m_iNumberOfFiles=iFiles;
    }
    
    void HideControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN,iWindowId,0, iControlId,0,0,null); 
      OnMessage(msg);
    }
    void ShowControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE,iWindowId,0, iControlId,0,0,null); 
      OnMessage(msg);
    }

    void DisableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, iWindowId,0, iControlId,0,0,null);
      OnMessage(msg);
    }
    void EnableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, iWindowId,0, iControlId,0,0,null);
      OnMessage(msg);
    }
  }
}

