#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using DirectShowLib;
using MediaPortal.Profile;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for VideoRendererStatistics.
  /// </summary>
  public class VideoRendererStatistics
  {
    public enum State
    {
      NotUsed,
      NoSignal,			// no (or bad) signal found
      Signal,				// signal found, but no video detected
      Scrambled,		// video is scrambled
      VideoPresent  // video present
    }
    static State videoState = State.NotUsed;
    static int framesDrawn = 0, avgSyncOffset = 0, avgDevSyncOffset = 0, framesDropped = 0, jitter = 0;
    static float avgFrameRate = 0f;
    static int _noSignalTimeOut = -1;

    static public int NoSignalTimeOut
    {
      get
      {
        ServiceProvider services = GlobalServiceProvider.Instance;
        IConfig _config = services.Get<IConfig>();
        if (_noSignalTimeOut == -1)
          using (Settings xmlreader = new Settings(_config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
            _noSignalTimeOut = xmlreader.GetValueAsInt("debug", "nosignaltimeout", 5);

        return _noSignalTimeOut;
      }
    }

    static public bool IsVideoFound
    {
      get
      {
        return (videoState == State.NotUsed || videoState == State.VideoPresent);
      }
    }
    static public State VideoState
    {
      get { return videoState; }
      set { videoState = value; }
    }

    static public float AverageFrameRate
    {
      get { return avgFrameRate; }
      set
      {
        avgFrameRate = value;
      }
    }
    static public int AverageSyncOffset
    {
      get { return avgSyncOffset; }
      set
      {
        avgSyncOffset = value;
      }
    }
    static public int AverageDeviationSyncOffset
    {
      get { return avgDevSyncOffset; }
      set
      {
        avgDevSyncOffset = value;
      }
    }
    static public int FramesDrawn
    {
      get { return framesDrawn; }
      set
      {
        framesDrawn = value;
      }
    }
    static public int FramesDropped
    {
      get { return framesDropped; }
      set
      {
        framesDropped = value;
      }
    }
    static public int Jitter
    {
      get { return jitter; }
      set { jitter = value; }
    }


    static public void Update(IQualProp quality)
    {
      try
      {
        if (quality != null)
        {
          int framesDrawn = 0, avgFrameRate = 0, avgSyncOffset = 0, avgDevSyncOffset = 0, framesDropped = 0, jitter = 0;
          quality.get_AvgFrameRate(out avgFrameRate);
          quality.get_AvgSyncOffset(out avgSyncOffset);
          quality.get_DevSyncOffset(out avgDevSyncOffset);
          quality.get_FramesDrawn(out framesDrawn);
          quality.get_FramesDroppedInRenderer(out framesDropped);
          quality.get_Jitter(out jitter);
          VideoRendererStatistics.AverageFrameRate = ((float)avgFrameRate) / 100.0f;
          VideoRendererStatistics.AverageSyncOffset = avgSyncOffset;
          VideoRendererStatistics.AverageDeviationSyncOffset = avgDevSyncOffset;
          VideoRendererStatistics.FramesDrawn = framesDrawn;
          VideoRendererStatistics.FramesDropped = framesDropped;
          VideoRendererStatistics.Jitter = jitter;
        }
      }
      catch
      {
      }
    }
  }
}
