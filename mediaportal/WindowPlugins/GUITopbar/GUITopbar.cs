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
using System.Xml;
using MediaPortal.GUI.Library;

namespace MediaPortal.Topbar
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITopbar: GUIOverlayWindow
	{
    const int HIDE_SPEED = 8;

    bool m_bFocused=false;
    bool m_bEnabled=false;
    bool m_bTopBarAutoHide=false;

    bool m_bTopBarEffect=false; 
    bool m_bTopBarHide=false;
    bool m_bTopBarHidden=false;
    bool m_bOverrideSkinAutoHide=false;
		static bool useTopBarSub=false;
    int m_iMoveUp=0;
    int m_iTopbarRegion=10;
    int m_iAutoHideTimeOut=15;   

		public GUITopbar()
		{
			// 
			// TODO: Add constructor logic here
			//
			GetID=(int)GUIWindow.Window.WINDOW_TOPBAR;
		}

		public bool UseTopBarSub	// Use top Bar in Submenu. 
		{
			get{ return useTopBarSub; }
			set{ useTopBarSub = value; }
		}

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\topbar.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TOPBAR;
      m_bEnabled=PluginManager.IsPluginNameEnabled("Topbar");    

      using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        m_iAutoHideTimeOut = xmlreader.GetValueAsInt("TopBar", "autohidetimeout", 15);

        m_bOverrideSkinAutoHide = false;
        if (xmlreader.GetValueAsInt("TopBar", "overrideskinautohide", 0) == 1) m_bOverrideSkinAutoHide = true;

				GUIGraphicsContext.DefaultTopBarHide = this.AutoHideTopbar; // default autohide option
        m_bTopBarAutoHide = this.AutoHideTopbar; // Get topbar skin setting
				m_bTopBarHidden = m_bTopBarAutoHide;
 
        if (m_bOverrideSkinAutoHide)
        {          
          m_bTopBarAutoHide = false;
          if (xmlreader.GetValueAsInt("TopBar", "autohide", 0) == 1) m_bTopBarAutoHide = true;
					GUIGraphicsContext.TopBarHidden = m_bTopBarAutoHide;
        }
      }

      // Topbar region
      foreach (CPosition pos in m_vecPositions)
      {
        if ((pos.YPos+pos.control.Height) > m_iTopbarRegion) m_iTopbarRegion=pos.YPos+pos.control.Height;
      }

      return bResult;
    }
    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    public override void PreInit()
		{
			base.PreInit();
      AllocResources();
    }
    public override void Render(float timePassed)
    {
    }

		protected override bool ShouldFocus(Action action)
		{
			return (action.wID==Action.ActionType.ACTION_MOVE_UP);
		}
    public void CheckFocus()
    {
      if (!m_bFocused)
      {
        foreach (GUIControl control in controlList)
        {
          control.Focus=false;
        }
      }
    }
    public override bool DoesPostRender()
    {
      if (!m_bEnabled) return false;
			if (GUIGraphicsContext.IsFullScreenVideo) return false;
			if (GUIWindowManager.ActiveWindow==(int)5500) return false;
      if (GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_MOVIE_CALIBRATION) return false;
      if (GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_UI_CALIBRATION) return false;
      if (GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_SLIDESHOW) return false;
			if (GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_HOME && useTopBarSub==true) return true;
			if (GUIWindowManager.ActiveWindow!=(int)GUIWindow.Window.WINDOW_HOME)
      {
        return true;
      }
      return false;
    }
    public override void PostRender(float timePassed,int iLayer)
    {
      if (!m_bEnabled) return ;
      if (iLayer !=1) return;
      CheckFocus();

      // Check auto hide topbar
			if (GUIGraphicsContext.TopBarHidden != m_bTopBarHidden)
			{
				// Rest to new settings
				m_bTopBarHidden = GUIGraphicsContext.TopBarHidden;
				m_bTopBarHide = GUIGraphicsContext.TopBarHidden;
				m_bTopBarEffect = false;

				m_iMoveUp = 0;
				if (m_bTopBarHidden) m_iMoveUp = m_iTopbarRegion;
				foreach (CPosition pos in m_vecPositions)
				{
					int y=(int)pos.YPos - m_iMoveUp;
					y+=GUIGraphicsContext.OverScanTop;
					pos.control.SetPosition((int)pos.XPos,y);         
				}
			}
			else if (m_bTopBarHidden != m_bTopBarHide)
			{
				m_bTopBarEffect = true;
			}
			
			if (GUIGraphicsContext.AutoHideTopBar)
      {
        // Check autohide timeout
        if (m_bFocused)
        {
          m_bTopBarHide = false;
          GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
        }

        TimeSpan ts=DateTime.Now-GUIGraphicsContext.TopBarTimeOut;
        if ((ts.TotalSeconds > m_iAutoHideTimeOut) && !m_bTopBarHide)
        {
          // Hide topbar with effect
          m_bTopBarHide = true;
          m_iMoveUp=0;
        }
        
        if (m_bTopBarEffect)
        {
          if (m_bTopBarHide)
          {
            m_iMoveUp+=HIDE_SPEED;
            if (m_iMoveUp >= m_iTopbarRegion) 
            {
              m_bTopBarHidden = true;
              GUIGraphicsContext.TopBarHidden = true;
              m_bTopBarEffect = false;
            }
          }
          else
          {
            m_bTopBarHidden = false;
            GUIGraphicsContext.TopBarHidden = false;
            m_iMoveUp = 0;            
          }

          foreach (CPosition pos in m_vecPositions)
					{
						int y=(int)pos.YPos - m_iMoveUp;
						y+=GUIGraphicsContext.OverScanTop;
						pos.control.SetPosition((int)pos.XPos,y);         
          }
        }
      }

      if (GUIGraphicsContext.TopBarHidden) return;           

			GUIFontManager.Present();
      base.Render(timePassed);
    }

    public override bool Focused
    {
      get { return m_bFocused;}
      set 
      {
        m_bFocused=value;
        if (m_bFocused==true)
        {          
          // reset autohide timer          
          if (GUIGraphicsContext.AutoHideTopBar) 
          {
            GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
            m_bTopBarHide = false;
          }

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, 0,defaultControlId,0,0,null);
          OnMessage(msg);
        }
        else
        {
          foreach (GUIControl control in controlList)
          {
            control.Focus=false;
          }
        }
      }
    }

    public override void OnAction(Action action)
    {
      CheckFocus();
      if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        // reset autohide timer       
        if (m_bTopBarHide && GUIGraphicsContext.AutoHideTopBar)
        {
          if (action.fAmount2 < m_iTopbarRegion)
          {
            GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
            m_bTopBarHide = false;
          }
        }

        foreach (GUIControl control in controlList)
        {
          bool bFocus=control.Focus;
          int id;
          if (control.HitTest((int)action.fAmount1,(int)action.fAmount2,out id, out bFocus))
          {	
            if (!bFocus)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID,0,id,0,0,null);
              OnMessage(msg);
              control.HitTest((int)action.fAmount1,(int)action.fAmount2,out id, out bFocus);
            }
            control.OnAction(action);
            m_bFocused=true;
            return ;
          }
        }
        
         Focused=false;
        return ;
      }

      base.OnAction (action);
      if (action.wID==Action.ActionType.ACTION_MOVE_DOWN)
      {
        // reset autohide timer
        if (GUIGraphicsContext.AutoHideTopBar) GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
        Focused=false;
      }
    }
  }
}
