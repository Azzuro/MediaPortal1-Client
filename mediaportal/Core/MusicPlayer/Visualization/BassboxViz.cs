#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.TagReader;
using MediaPortal.Util;
using BassVis_Api;


namespace MediaPortal.Visualization
{
  public class BassboxViz : VisualizationBase, IDisposable
  {
    #region Variables

    [DllImport("User32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    private BASSVIS_INFO _mediaInfo = null;

    private bool RenderStarted = false;
    private bool firstRun = true;

    private MusicTag trackTag = null;
    private string _songTitle = "   "; // Title of the song played
    private BASSVIS_EXEC visExec;

    #endregion

    #region Constructors/Destructors

    public BassboxViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl)
    {
    }

    #endregion

    #region Public Methods

    public override bool Initialize()
    {
      Bass.PlaybackStateChanged += new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);

      _mediaInfo = new BASSVIS_INFO("", "");

      try
      {
        Log.Info("Visualization Manager: Initializing {0} visualization...", VizPluginInfo.Name);

        if (VizPluginInfo == null)
        {
          Log.Error("Visualization Manager: {0} visualization engine initialization failed! Reason:{1}",
                    VizPluginInfo.Name, "Missing or invalid VisualizationInfo object.");

          return false;
        }

        firstRun = true;
        RenderStarted = false;
        bool result = SetOutputContext(VisualizationWindow.OutputContextType);
        _Initialized = result && _visParam.VisHandle != 0;
      }

      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Bassbox visualization engine initialization failed with the following exception {0}",
          ex);
        return false;
      }

      return _Initialized;
    }

    #endregion

    #region Private Methods

    private void PlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                      BassAudioEngine.PlayState newState)
    {
      Log.Debug("BassboxViz: BassPlayer_PlaybackStateChanged from {0} to {1}", oldState.ToString(), newState.ToString());
      if (newState == BassAudioEngine.PlayState.Playing)
      {
        RenderStarted = false;
        trackTag = TagReader.TagReader.ReadTag(Bass.CurrentFile);
        if (trackTag != null)
        {
          _songTitle = String.Format("{0} - {1}", trackTag.Artist, trackTag.Title);
        }
        else
        {
          _songTitle = "   ";
        }

        _mediaInfo.SongTitle = _songTitle;
        _mediaInfo.SongFile = Bass.CurrentFile;

        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);
      }
      else if (newState == BassAudioEngine.PlayState.Paused)
      {
        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Pause);
      }
      else if (newState == BassAudioEngine.PlayState.Ended)
      {
        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Stop);
        RenderStarted = false;
      }
    }

    #endregion

    #region <Base class> Overloads

    public override bool InitializePreview()
    {
      base.InitializePreview();
      return Initialize();
    }

    public override void Dispose()
    {
      Bass.PlaybackStateChanged -= new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);
      base.Dispose();
      Close();
    }

    public override int RenderVisualization()
    {
      try
      {
        if (VisualizationWindow == null || !VisualizationWindow.Visible || _visParam.VisHandle == 0)
        {
          return 0;
        }

        // Set Song information, so that the plugin can display it
        // Do not will see it with Bassbox then deactivate _mediaInfo.SongTitle
        // all others will be used, do not remove
        if (trackTag != null && Bass != null)
        {
          _mediaInfo.SongTitle = _songTitle;
          _mediaInfo.Position = (int) (1000*Bass.CurrentPosition);
          _mediaInfo.Duration = (int) Bass.Duration;
        }
        else
        {
          _mediaInfo.Position = 0;
          _mediaInfo.Duration = 0;
        }
        if (IsPreviewVisualization)
        {
          _mediaInfo.SongTitle = "Mediaportal Preview";
        }
        BassVis.BASSVIS_SetInfo(_visParam, _mediaInfo);

        if (RenderStarted)
        {
          return 1;
        }

        int stream = 0;

        if (Bass != null)
        {
          stream = (int) Bass.GetCurrentVizStream();
        }
        // ckeck is playing
        int nReturn = BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.IsPlaying);
        if (nReturn == Convert.ToInt32(BASSVIS_PLAYSTATE.Play) && (_visParam.VisHandle != 0))
        {
          // Do not Render without playing
          if (stream != 0)
          {
            // Do not Render without playing
            if (MusicPlayer.BASS.Config.MusicPlayer == AudioPlayer.WasApi)
            {
              RenderStarted = BassVis.BASSVIS_RenderChannel(_visParam, stream, true);
            }
            else
            {
              RenderStarted = BassVis.BASSVIS_RenderChannel(_visParam, stream, false);
            }
          }
        }
      }

      catch (Exception)
      {
      }

      return 1;
    }

    public override bool Close()
    {
      if (base.Close())
      {
        return true;
      }
      return false;
    }

    public override bool Config()
    {

      return true;
    }

    public override bool IsBassboxVis()
    {
      return true;
    }

    public override bool WindowChanged(VisualizationWindow vizWindow)
    {
      base.WindowChanged(vizWindow);
      return true;
    }

    public override bool WindowSizeChanged(Size newSize)
    {
      // If width or height are 0 the call to CreateVis will fail.  
      // If width or height are 1 the window is in transition so we can ignore it.
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      // Do a move of the Bassbox Viz
      if (_visParam.VisHandle != 0)
      {
        // Hide the Viswindow, so that we don't see it, while moving
        Win32API.ShowWindow(VisualizationWindow.Handle, Win32API.ShowWindowFlags.Hide);
        BassVis.BASSVIS_Resize(_visParam, 0, 0, newSize.Width, newSize.Height);
      }
      return true;
    }

    public override bool SetOutputContext(OutputContextType outputType)
    {
      if (VisualizationWindow == null)
      {
        return false;
      }

      if (_Initialized && !firstRun)
      {
        return true;
      }

      // If width or height are 0 the call to CreateVis will fail.  
      // If width or height are 1 the window is in transition so we can ignore it.
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      if (VizPluginInfo == null || VizPluginInfo.FilePath.Length == 0 || !File.Exists(VizPluginInfo.FilePath))
      {
        return false;
      }

      if (_visParam.VisHandle != 0)
      {
        RenderStarted = false;

        int counter = 0;

        bool bFree = BassVis.BASSVIS_Free(_visParam);
        while ((!bFree) && (counter <= 10))
        {
          bFree = BassVis.BASSVIS_IsFree(_visParam);
          System.Windows.Forms.Application.DoEvents();
          counter++;
        }
        _visParam.VisHandle = 0;
      }


      try
      {
        string vizPath = VizPluginInfo.FilePath;
        // Create the Visualisation
        visExec = new BASSVIS_EXEC(vizPath);

        visExec.PluginFile = vizPath;
        visExec.BB_Flags = BASSVISFlags.BASSVIS_NOINIT;

        BassVis.BASSVIS_ExecutePlugin(visExec, _visParam);

        // BassVis create internal for BassBox a OpenGL Window if this handle more then 0
        // then create of GLWindow is succes. _visParam.VisHandle is then the Handle of this GLWindow
        // which set as Parent with BASSVIS_SetVisPort in my Container VisualizationWindow 
        int VisHandle = _visParam.VisHandle;

        if (VisHandle != 0)
        {
          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);

          visExec.PluginFile = vizPath;
          visExec.BB_Flags = BASSVISFlags.BASSVIS_DEFAULT;
          visExec.BB_ParentHandle = VisualizationWindow.Handle;
          visExec.BB_ShowFPS = true;
          visExec.BB_ShowPrgBar = true;
          visExec.Left = 0;
          visExec.Top = 0;
          visExec.Width = VisualizationWindow.Width;
          visExec.Height = VisualizationWindow.Height;

          BassVis.BASSVIS_ExecutePlugin(visExec, _visParam);


          IntPtr scrVisHandle = new IntPtr(VisHandle);
          BassVis.BASSVIS_SetVisPort(_visParam,
                                     scrVisHandle,
                                     VisualizationWindow.Handle,
                                     0,
                                     0,
                                     VisualizationWindow.Width,
                                     VisualizationWindow.Height);

          // The Bassbox Plugin has stolen focus on the MP window. Bring it back to froeground
          Win32API.SetForegroundWindow(GUIGraphicsContext.form.Handle);
        }
        firstRun = false;
      }
      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Bassbox visualization engine initialization failed with the following exception {0}",
          ex);
      }
      _Initialized = _visParam.VisHandle != 0;
      return _visParam.VisHandle != 0;
    }

    #endregion
  }
}