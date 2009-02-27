#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#region usings

using MediaPortal.GUI.Library;

#endregion

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TVGuideDialog : TvGuideBase
  {
    public TVGuideDialog()
      : base()
    {
      GetID = (int) Window.WINDOW_DIALOG_TVGUIDE;
    }

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int) Window.WINDOW_DIALOG_TVGUIDE, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    public override bool IsTv
    {
      get { return true; }
    }

    private bool _groupChanged;
    public bool GroupChanged 
    {
      get 
      { 
        return _groupChanged; 
      }
      set 
      { 
        _groupChanged = value; 
      }
    }
    public override bool Init()
    {
      Initialize();

      Load(GUIGraphicsContext.Skin + @"\dialogTvGuide.xml");
      GetID = (int) Window.WINDOW_DIALOG_TVGUIDE;
      GUIWindowManager.Replace((int) Window.WINDOW_DIALOG_TVGUIDE, this);
      Restore();
      PreInit();
      ResetAllControls();

      _groupChanged = false;
      return true;
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      if (_running)
      {
        PageDestroy();
      }
    }

    /// <summary>
    /// changes the current tv group and refreshes guide display
    /// </summary>
    /// <param name="Direction"></param>
    protected override void OnChangeTvGroup(int Direction)
    {
      base.OnChangeTvGroup(Direction);
      _groupChanged = true;
    }

    /// <summary>
    /// Shows channel group selection dialog
    /// </summary>
    protected override void OnSelectGroup()
    {
      base.OnSelectGroup();
      _groupChanged = true;
    }
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
        case Action.ActionType.ACTION_CLOSE_DIALOG:
        case Action.ActionType.ACTION_SHOW_FULLSCREEN:
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //case Action.ActionType.ACTION_SELECT_ITEM:
          _groupChanged = false; // reset so guide dialog can exit loop in tvhome
          PageDestroy();
          return;
      }
      base.OnAction(action);
    }
  }
}