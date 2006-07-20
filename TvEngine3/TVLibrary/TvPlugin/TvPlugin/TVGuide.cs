  /* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
#region usings
using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using TvDatabase;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

#endregion


namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TVGuide : TvGuideBase
  {
    [SkinControlAttribute(98)]
    protected GUIImage videoBackground;
    [SkinControlAttribute(99)]
    protected GUIVideoControl videoWindow;

    public TVGuide() : base()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TVGUIDE;
    }
    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\mytvguide.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TVGUIDE;
      Initialize();
      return result;
    }
    public override void OnAdded()
    {
      Log.Write("TVGuide:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TVGUIDE, this);
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      ///@
      /*
      CheckNewTVGuide();
      // check if there's a new TVguide.xml
      try
      {
        List<TVChannel> channels = new List<TVChannel>();
        TVDatabase.GetChannels(ref channels);
        if (channels.Count == 0)
        {
          StartImportXML();
        }
        channels = null;
      }
      catch (Exception)
      {
      }
      */

      ///@
      //TVDatabase.OnProgramsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
      //TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
      //@ConflictManager.OnConflictsUpdated += new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      ///@
      ///TVDatabase.OnProgramsChanged -= new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
      ///TVDatabase.OnNotifiesChanged -= new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
      ///ConflictManager.OnConflictsUpdated -= new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);

      base.OnPageDestroy(newWindowId);
      ///@
      /*
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }*/
    }
  }
}
