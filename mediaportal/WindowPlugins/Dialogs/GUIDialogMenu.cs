using System;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogMenu: GUIWindow
  {
    enum Controls
    {
			CONTROL_BACKGROUND=1
      , CONTROL_CLOSEBTN      =2
      , CONTROL_LIST          =3
			, CONTROL_HEADING	      =4
			, CONTROL_BACKGROUNDDLG	=6
		};

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
    int m_iSelected      = -1;
    int m_iItemId = -1;

		string m_strSelected="";
    ArrayList m_vecList   = new ArrayList();
    bool    m_bPrevOverlay=false;

    public GUIDialogMenu()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_MENU;
    }

    public override bool Init()
    { 
      return Load (GUIGraphicsContext.Skin+@"\DialogMenu.xml");
    }
    
    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }
    public override void PreInit()
    {
      AllocResources();
    }


    public override void OnAction(Action action)
    {
      int iSelection;
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }

      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        if (action.m_key!=null)
        {
          if (action.m_key.KeyChar >'0' && action.m_key.KeyChar <='9')         
          {
            // Get offset to item
            iSelection = action.m_key.KeyChar-'1';
            if (iSelection < m_vecList.Count)
            {
              // Select dialog item
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)Controls.CONTROL_LIST, 0, 0, null);
              OnMessage(msg);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0, (int)Controls.CONTROL_LIST, iSelection ,0 ,null);
              OnMessage(msg);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.CONTROL_LIST, 0, 0, 0, null);
              OnMessage(msg);
            }
            else
            {
              // Exit dialog box
              Close();
            }
            return;
          }
        }
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
      if (m_vecList.Count == 0)
        Close();

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
        System.Threading.Thread.Sleep(100);

      }
    }
    #endregion
	
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          base.OnMessage(message);
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
          ClearControl(GetID,(int)Controls.CONTROL_LIST);

          for (int i=0; i < m_vecList.Count; i++)
          {
            GUIListItem pItem=(GUIListItem)m_vecList[i];
            AddListItemControl(GetID,(int)Controls.CONTROL_LIST,pItem);
          }

          if (m_iSelected>=0)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,(int)Controls.CONTROL_LIST,m_iSelected,0,null);
            OnMessage(msg);
          }
          m_iSelected=-1;
          m_iItemId=-1;
          string wszText=String.Format("{0} {1}", m_vecList.Count,GUILocalizeStrings.Get(127) );

        }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
    			
          int iControl=message.SenderControlId;
          if ((int)Controls.CONTROL_LIST==iControl)
          {
						m_iSelected=GetSelectedItemNo();
						m_strSelected=GetSelectedItem().Label;
            m_iItemId=GetSelectedItem().ItemId;
						Close();
          }
          if ((int)Controls.CONTROL_CLOSEBTN==iControl)
          {
            Close();
          }
        }
          break;
      }

      return base.OnMessage(message);
    }

    public void Reset()
    {
      m_iSelected=-1;
      m_vecList.Clear();
    }

    public void Add(string strLabel)
		{
			int iItemIndex = m_vecList.Count+1;
      GUIListItem pItem = new GUIListItem(iItemIndex.ToString()+" "+strLabel);
			pItem.ItemId =iItemIndex;
      m_vecList.Add(pItem);
    }

    public void AddLocalizedString(int iLocalizedString)
    {
      int iItemIndex = m_vecList.Count+1;
      GUIListItem pItem = new GUIListItem(iItemIndex.ToString()+" "+GUILocalizeStrings.Get(iLocalizedString));     
      pItem.ItemId = iLocalizedString;
      m_vecList.Add(pItem);      
    }

    public int SelectedLabel 
    {
      get { return m_iSelected;}
      set { m_iSelected=value;}
    }
    
    public int SelectedId 
    {
      get { return m_iItemId;}
      set { m_iItemId=value;}
    }

    public string SelectedLabelText
    {
      get { return m_strSelected;}
    }

    public void  SetHeading( string strLine)
    {
      SetControlLabel(GetID,(int)Controls.CONTROL_HEADING,strLine);
    }


    public void SetHeading(int iString)
    {
      SetHeading (GUILocalizeStrings.Get(iString) );
    }


    GUIListItem GetSelectedItem()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, GetID, 0,(int)Controls.CONTROL_LIST,0,0,null);
      OnMessage(msg);
      return (GUIListItem)msg.Object;
    }

    GUIListItem GetItem(int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_ITEM, GetID, 0,(int)Controls.CONTROL_LIST,iItem,0,null);
      OnMessage(msg);
      return (GUIListItem)msg.Object;
    }

    int GetSelectedItemNo()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_LIST,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }

    int GetItemCount()
    { 
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEMS, GetID, 0,(int)Controls.CONTROL_LIST,0,0,null);
      OnMessage(msg);
      return msg.Param1;
    }

    public override void Render()
    {
			RenderDlg();
    }

    void ClearControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, iWindowId,0, iControlId,0,0,null);
      OnMessage(msg);
    }

    void AddListItemControl(int iWindowId, int iControlId,GUIListItem item)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0,iControlId,0,0,item);
      OnMessage(msg);
    }    
    void SetControlLabel(int iWindowId, int iControlId,string strText)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, iWindowId, 0,iControlId,0,0,null);
      msg.Label=strText; 
      OnMessage(msg);
    }
    void HideControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN,iWindowId, 0,iControlId,0,0,null); 
      OnMessage(msg);
    }
    void ShowControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE,iWindowId, 0,iControlId,0,0,null); 
      OnMessage(msg);
    }

    void DisableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, iWindowId, 0,iControlId,0,0,null);
      OnMessage(msg);
    }
    void EnableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, iWindowId, 0,iControlId,0,0,null);
      OnMessage(msg);
    }
	}
}
