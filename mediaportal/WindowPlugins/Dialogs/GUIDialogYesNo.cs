using System;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogYesNo: GUIWindow
	{
		enum Controls
		{
			ID_BUTTON_NO   =10
				, ID_BUTTON_YES  =11
		};

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
    
		bool m_bConfirmed = false;
    bool m_bPrevOverlay=true;
    bool m_DefaultYes=false;

		public GUIDialogYesNo()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_YES_NO;
		}

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\dialogYesNo.xml");
		}
    public override bool SupportsDelayedLoad
    {
      get { return true;}
    }
    
		public override void PreInit()
		{
		//	AllocResources();
		}


		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				Close();
        m_DefaultYes=false;
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
					m_bConfirmed = false;
					base.OnMessage(message);
					GUIGraphicsContext.Overlay=false;
          if (m_DefaultYes)
          {
            GUIControl.FocusControl(GetID,(int)Controls.ID_BUTTON_YES);
          }
				}
					return true;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
        
					if ( GetControl((int)Controls.ID_BUTTON_YES) == null)
					{
						m_bConfirmed=true;
            Close();
            m_DefaultYes=false;
						return true;
					}
					if (iControl==(int)Controls.ID_BUTTON_NO)
					{
						m_bConfirmed=false;
            Close();
            m_DefaultYes=false;
						return true;
					}
					if (iControl==(int)Controls.ID_BUTTON_YES)
					{
						m_bConfirmed=true;
            Close();
            m_DefaultYes=false;
						return true;
					}
				}
					break;
			}

			return base.OnMessage(message);
		}


		public bool IsConfirmed
		{
			get { return m_bConfirmed;}
		}

		public void  SetHeading( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, 1,0,0,null);
			msg.Label=strLine; 
			OnMessage(msg);
      SetLine(1, "");
      SetLine(2, "");
      SetLine(3, "");
		}

		public void SetHeading(int iString)
    {
      if (iString==0) SetHeading ("");
			else SetHeading (GUILocalizeStrings.Get(iString) );
		}

		public void SetLine(int iLine, string strLine)
    {
      if (iLine<=0) return;
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, 1+iLine,0,0,null);
			msg.Label=strLine; 
			OnMessage(msg);
		}

		public void SetLine(int iLine,int iString)
		{
      if (iLine<=0) return;
      if (iString==0) SetLine (iLine, "");
			else SetLine (iLine, GUILocalizeStrings.Get(iString) );
		}

    public void SetDefaultToYes(bool bYesNo)
    {
      m_DefaultYes=bYesNo;
    }

		public override void Render()
		{
			RenderDlg();
		}
	}
}
