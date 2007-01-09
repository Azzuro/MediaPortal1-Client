#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  /// 

  public class TvZapOsd : GUIWindow
  {
    [SkinControlAttribute(35)]
    protected GUILabelControl lblCurrentChannel = null;
    [SkinControlAttribute(36)]
    protected GUITextControl lblOnTvNow = null;
    [SkinControlAttribute(37)]
    protected GUITextControl lblOnTvNext = null;
    [SkinControlAttribute(100)]
    protected GUILabelControl lblCurrentTime = null;
    [SkinControlAttribute(101)]
    protected GUILabelControl lblStartTime = null;
    [SkinControlAttribute(102)]
    protected GUILabelControl lblEndTime = null;
    [SkinControlAttribute(39)]
    protected GUIImage imgRecIcon = null;
    [SkinControlAttribute(10)]
    protected GUIImage imgTvChannelLogo = null;


    bool m_bNeedRefresh = false;
    DateTime m_dateTime = DateTime.Now;

    IList tvChannelList;

    public TvZapOsd()
    {

      GetID = (int)GUIWindow.Window.WINDOW_TVZAPOSD;
    }
    public override void OnAdded()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TVZAPOSD;
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TVZAPOSD, this);
      Restore();
      PreInit();
      ResetAllControls();
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\tvZAPOSD.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TVZAPOSD;
      return bResult;
    }


    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void Render(float timePassed)
    {
      UpdateProgressBar();
      Get_TimeInfo();							// show the time elapsed/total playing time
      base.Render(timePassed);		// render our controls to the screen
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_OSD:
          {
            return;
          }

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          {
            OnNextChannel();
            return;
          }

        case Action.ActionType.ACTION_PREV_CHANNEL:
          {
            OnPreviousChannel();
            return;
          }

        case Action.ActionType.ACTION_CONTEXT_MENU:
          {
            if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
            {
              TvFullScreen tvWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
              tvWindow.OnAction(new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0));
              tvWindow.OnAction(action);
            }
            return;
          }
      }

      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      Log.Debug("zaposd pagedestroy");
      FreeResources();
      base.OnPageDestroy(newWindowId);

      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + newWindowId));
    }
    protected override void OnPageLoad()
    {
      Log.Debug("zaposd pageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "istv", 1);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      tvChannelList = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      AllocResources();
      // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
      ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
      m_bNeedRefresh = false;
      m_dateTime = DateTime.Now;
      SetCurrentChannelLogo();
      base.OnPageLoad();

      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
    }


    void Get_TimeInfo()
    {
      string strChannel = GetChannelName();
      string strTime = strChannel;
      Program prog = TVHome.Navigator.GetChannel(strChannel).CurrentProgram;
      if (prog != null)
      {

        strTime = String.Format("{0}-{1}",
          prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
      }
      if (lblCurrentTime != null)
      {
        lblCurrentTime.Label = strTime;
      }
    }

    public override void ResetAllControls()
    {
      //reset all
      bool bOffScreen = false;
      int iCalibrationY = GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin = 0;

      foreach (CPosition pos in _listPositions)
      {
        pos.control.SetPosition((int)pos.XPos, (int)pos.YPos + iCalibrationY);
      }
      foreach (CPosition pos in _listPositions)
      {
        GUIControl pControl = pos.control;

        int dwPosY = pControl.YPosition;
        if (pControl.IsVisible)
        {
          if (dwPosY < iTop)
          {
            int iSize = iTop - dwPosY;
            if (iSize > iMin) iMin = iSize;
            bOffScreen = true;
          }
        }
      }
      if (bOffScreen)
      {

        foreach (CPosition pos in _listPositions)
        {
          GUIControl pControl = pos.control;
          int dwPosX = pControl.XPosition;
          int dwPosY = pControl.YPosition;
          if (dwPosY < (int)100)
          {
            dwPosY += Math.Abs(iMin);
            pControl.SetPosition(dwPosX, dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }


    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh)
      {
        m_bNeedRefresh = false;
        return true;
      }
      return false;
    }

    private void OnPreviousChannel()
    {
      Log.Debug("GUITV OSD: OnNextChannel");
      if (!TVHome.Card.IsTimeShifting) return;
      TVHome.Navigator.ZapToPreviousChannel(true);

      SetCurrentChannelLogo();
      m_dateTime = DateTime.Now;
    }

    private void OnNextChannel()
    {

      Log.Debug("GUITV ZAPOSD: OnNextChannel");
      if (!TVHome.Card.IsTimeShifting) return;
      TVHome.Navigator.ZapToNextChannel(true);
      SetCurrentChannelLogo();
      m_dateTime = DateTime.Now;
    }

    public void UpdateChannelInfo()
    {
      SetCurrentChannelLogo();
    }


    void SetCurrentChannelLogo()
    {
      string strChannel = GetChannelName();
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannel);
      if (System.IO.File.Exists(strLogo))
      {
        if (imgTvChannelLogo != null)
        {
          imgTvChannelLogo.SetFileName(strLogo);
          //img.SetPosition(GUIGraphicsContext.OverScanLeft, GUIGraphicsContext.OverScanTop);
          m_bNeedRefresh = true;
          imgTvChannelLogo.IsVisible = true;
        }
      }
      else
      {
        if (imgTvChannelLogo != null)
        {
          imgTvChannelLogo.IsVisible = false;
        }
      }
      ShowPrograms();
    }

    string GetChannelName()
    {
      return TVHome.Navigator.ZapChannel.Name;
    }
    void ShowPrograms()
    {
      if (lblOnTvNow != null)
      {
        lblOnTvNow.EnableUpDown = false;
        lblOnTvNow.Clear();
      }
      if (lblOnTvNext != null)
      {
        lblOnTvNext.EnableUpDown = false;
        lblOnTvNext.Clear();
      }

      // Set recorder status
      if (imgRecIcon != null)
      {
        VirtualCard card;
        TvServer server = new TvServer();
        imgRecIcon.IsVisible = server.IsRecording(GetChannelName(), out card);
      }

      if (lblCurrentChannel != null)
      {
        lblCurrentChannel.Label = GetChannelName();
      }

      Program prog = TVHome.Navigator.GetChannel(GetChannelName()).GetProgramAt(m_dateTime);
      if (prog != null)
      {
        string strTime = String.Format("{0}-{1}",
          prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        if (lblCurrentTime != null)
        {
          lblCurrentTime.Label = strTime;
        }

        if (lblOnTvNow != null)
        {
          lblOnTvNow.Label = prog.Title;
        }
        if (lblStartTime != null)
        {
          strTime = String.Format("{0}", prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          lblStartTime.Label = strTime;
        }
        if (lblEndTime != null)
        {
          strTime = String.Format("{0} ", prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          lblEndTime.Label = strTime;
        }

        // next program
        prog = TVHome.Navigator.GetChannel(GetChannelName()).GetProgramAt(prog.EndTime.AddMinutes(1));
        if (prog != null)
        {
          if (lblOnTvNext != null)
          {
            lblOnTvNext.Label = prog.Title;
          }
        }
      }
      else
      {
        if (lblStartTime != null)
        {
          lblStartTime.Label = String.Empty;
        }
        if (lblEndTime != null)
        {
          lblEndTime.Label = String.Empty;
        }
        if (lblCurrentTime != null)
        {
          lblCurrentTime.Label = String.Empty;
        }
      }
      UpdateProgressBar();
    }

    void UpdateProgressBar()
    {
      double fPercent;
      Program prog = TVHome.Navigator.GetChannel(GetChannelName()).CurrentProgram;
      if (prog == null)
      {
        GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
        return;
      }
      string strTime = String.Format("{0}-{1}",
        prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      TimeSpan ts = prog.EndTime - prog.StartTime;
      double iTotalSecs = ts.TotalSeconds;
      ts = DateTime.Now - prog.StartTime;
      double iCurSecs = ts.TotalSeconds;
      fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
      fPercent *= 100.0d;
      GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());
    }
  }
}