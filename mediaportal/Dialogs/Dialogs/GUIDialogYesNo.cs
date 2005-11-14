/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections;
using System.Drawing;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogYesNo: GUIWindow
	{

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
    
		[SkinControlAttribute(10)]			protected GUIButtonControl btnNo=null;
		[SkinControlAttribute(11)]			protected GUIButtonControl btnYes=null;
		bool m_bConfirmed = false;
    bool m_bPrevOverlay=true;
    bool m_DefaultYes=false;
    int iYesKey=-1;
    int iNoKey = -1;
    bool needRefresh = false;
    DateTime vmr7UpdateTimer = DateTime.Now;

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
      needRefresh = true;
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				Close();
        m_DefaultYes=false;
				return;
			}

      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        if (action.m_key!=null)
        {
          // Yes or No key
          if (action.m_key.KeyChar==iYesKey)
          {
            m_bConfirmed=true;
            Close();
            m_DefaultYes=false;
            return;
          }

          if (action.m_key.KeyChar==iNoKey)
          {
            m_bConfirmed=false;
            Close();
            m_DefaultYes=false;
            return;
          }
        }
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
          if (VMR7Util.g_vmr7 != null)
          {
            TimeSpan ts = DateTime.Now - vmr7UpdateTimer;
            if (ts.TotalMilliseconds >= 5000 || needRefresh)
            {
              needRefresh = false;
              using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width, GUIGraphicsContext.Height))
              {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                  GUIGraphicsContext.graphics = g;

                  // render the parent window
                  if (null != m_pParentWindow)
                    m_pParentWindow.Render(timePassed);


                  // render this dialog box
                  base.Render(timePassed);

                  GUIGraphicsContext.graphics = null;
                  VMR7Util.g_vmr7.SaveBitmap(bmp, true, true, 1.0f);
                  g.Dispose();
                  bmp.Dispose();
                }
              }
              vmr7UpdateTimer = DateTime.Now;
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
      needRefresh = true;
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					m_pParentWindow=null;
					m_bRunning=false;
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
            GUIControl.FocusControl(GetID,btnYes.GetID);
          }
          
          iYesKey = (int)btnYes.Label.ToLower()[0];

          iNoKey = (int)btnNo.Label.ToLower()[0];
				}
					return true;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
        
					if ( btnYes == null)
					{
						m_bConfirmed=true;
            Close();
            m_DefaultYes=false;
						return true;
					}
					if (iControl==btnNo.GetID)
					{
						m_bConfirmed=false;
            Close();
            m_DefaultYes=false;
						return true;
					}
					if (iControl==btnYes.GetID)
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
      SetLine(1, String.Empty);
      SetLine(2, String.Empty);
      SetLine(3, String.Empty);
		}

		public void SetHeading(int iString)
    {
      if (iString==0) SetHeading (String.Empty);
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
      if (iString==0) SetLine (iLine, String.Empty);
			else SetLine (iLine, GUILocalizeStrings.Get(iString) );
		}

    public void SetDefaultToYes(bool bYesNo)
    {
      m_DefaultYes=bYesNo;
    }

		public override void Render(float timePassed)
		{
			RenderDlg(timePassed);
		}
	}
}
