using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogSelect2: GUIWindow
  {
    enum Controls
    {
			CONTROL_BACKGROUND=1
      , CONTROL_LIST          =3
			, CONTROL_HEADING	      =4
			, CONTROL_BACKGROUNDDLG	=6
		};

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
    int  m_iSelected      = -1;

		string m_strSelected="";
    ArrayList m_vecList   = new ArrayList();
    bool    m_bPrevOverlay=false;

    public GUIDialogSelect2()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_SELECT2;
    }
 
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\DialogSelect2.xml");
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
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members
    public void RenderDlg(float timePassed)
    {
			
			lock (this)
			{
				if (GUIGraphicsContext.IsFullScreenVideo)
				{
					if (VMR7Util.g_vmr7!=null)
					{
						using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width,GUIGraphicsContext.Height))
						{
							using (Graphics g = Graphics.FromImage(bmp))
							{
								GUIGraphicsContext.graphics=g;

								// render the parent window
								if (null!=m_pParentWindow) 
									m_pParentWindow.Render(timePassed);

								GUIFontManager.Present();
								// render this dialog box
								base.Render(timePassed);

								GUIGraphicsContext.graphics=null;
								VMR7Util.g_vmr7.SaveBitmap(bmp,true,true,0.8f);
							}
						}
						return;
					}
				}
				// render the parent window
				if (null!=m_pParentWindow) 
					m_pParentWindow.Render(timePassed);

				GUIFontManager.Present();
				// render this dialog box
				base.Render(timePassed);
			}
    }

    void Close()
		{
			GUIWindowManager.IsSwitchingToNewWindow=true;
			lock (this)
			{
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow=null;
				m_bRunning=false;
			}
			GUIWindowManager.IsSwitchingToNewWindow=false;
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
			GUIWindowManager.IsSwitchingToNewWindow=true;

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

			GUIWindowManager.IsSwitchingToNewWindow=false;
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
						Close();
          }
        }
          break;
      }

      return base.OnMessage(message);
    }

    public void Reset()
    {
			LoadSkin();
			AllocResources();
			InitControls();

      m_iSelected=-1;
      m_vecList.Clear();
    }

    public void Add(string strLabel)
    {
      GUIListItem pItem = new GUIListItem(strLabel);
      m_vecList.Add(pItem);
    }
    public int SelectedLabel 
    {
      get { return m_iSelected;}
      set { m_iSelected=value;}
    }
    public string SelectedLabelText
    {
      get { return m_strSelected;}
    }

    public void  SetHeading( string strLine)
    {
			LoadSkin();
			AllocResources();
			InitControls();

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

    public override void Render(float timePassed)
    {
			RenderDlg(timePassed);
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
