#region Copyright (C) 2005-2011 Team MediaPortal

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
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Util;
using BassVis_Api;

namespace MediaPortal.Visualization
{
  public class WinampViz : VisualizationBase, IDisposable
  {
    #region Variables

    private BASSVIS_INFO _mediaInfo = null;

    private bool RenderStarted = false;
    private bool firstRun = true;

    private IntPtr hwndChild; // Handle to the Winamp Child Window.

    private MusicTag trackTag = null;
    private string _songTitle = "   "; // Title of the song played

    #endregion

    #region Constructors/Destructors

    public WinampViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl) {}

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
          "Visualization Manager: Winamp visualization engine initialization failed with the following exception {0}",
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
      Log.Debug("WinampViz: BassPlayer_PlaybackStateChanged from {0} to {1}", oldState.ToString(), newState.ToString());
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
        if (trackTag != null && Bass != null)
        {
          _mediaInfo.Position = (int)Bass.CurrentPosition;
          _mediaInfo.Duration = (int)Bass.Duration;
          _mediaInfo.PlaylistLen = 1;
          _mediaInfo.PlaylistPos = 1;
        }
        else
        {
          _mediaInfo.Position = 0;
          _mediaInfo.Duration = 0;
          _mediaInfo.PlaylistLen = 0;
          _mediaInfo.PlaylistPos = 0;
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
          stream = (int)Bass.GetCurrentVizStream();
        }

        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);
        RenderStarted = BassVis.BASSVIS_RenderChannel(_visParam, stream);
      }

      catch (Exception) {}

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
      // We need to stop the Vis first, otherwise some plugins don't allow the config to be called
      if (_visParam.VisHandle != 0)
      {
        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Stop);
        BassVis.BASSVIS_Free(_visParam, ref _baseVisParam);
        _visParam.VisHandle = 0;
      }

      int tmpVis = BassVis.BASSVIS_GetPluginHandle(BASSVISKind.BASSVISKIND_WINAMP, VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASSVIS_GetModulePresetCount(_visParam, VizPluginInfo.FilePath);
        BassVis.BASSVIS_Config(_visParam, 0);
      }

      return true;
    }

    public override bool IsWinampVis()
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

      // Do a move of the Winamp Viz
      if (_visParam.VisHandle != 0)
      {
        hwndChild = Win32API.GetWindow(VisualizationWindow.Handle, Win32API.ShowWindowFlags.Show);
        if (hwndChild != IntPtr.Zero)
        {
          Win32API.MoveWindow(hwndChild, 0, 0, newSize.Width, newSize.Height, true);
        }
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
        BassVis.BASSVIS_Free(_visParam, ref _baseVisParam);
        _visParam.VisHandle = 0;
        RenderStarted = false;
      }

      // Set Dummy Information for the plugin, before creating it
      _mediaInfo.SongTitle = "";
      _mediaInfo.SongFile = "";
      _mediaInfo.Position = 0;
      _mediaInfo.Duration = 0;
      _mediaInfo.PlaylistPos = 0;
      _mediaInfo.PlaylistLen = 0;
      BassVis.BASSVIS_SetInfo(_visParam, _mediaInfo);

      try
      {
        // Create the Visualisation
        BASSVIS_EXEC visExec = new BASSVIS_EXEC(VizPluginInfo.FilePath);
        visExec.AMP_ModuleIndex = VizPluginInfo.PresetIndex;
        visExec.AMP_UseOwnW1 = 1;
        visExec.AMP_UseOwnW2 = 1;
        BassVis.BASSVIS_ExecutePlugin(visExec, _visParam);
        if (_visParam.VisGenWinHandle != IntPtr.Zero)
        {
          hwndChild = Win32API.GetWindow(VisualizationWindow.Handle, Win32API.ShowWindowFlags.Show);
          if (hwndChild != IntPtr.Zero)
          {
            Win32API.MoveWindow(hwndChild, 0, 0, VisualizationWindow.Width, VisualizationWindow.Height, true);
          }

          BassVis.BASSVIS_SetVisPort(_visParam,
                                     _visParam.VisGenWinHandle,
                                     VisualizationWindow.Handle,
                                     0,
                                     0,
                                     VisualizationWindow.Width,
                                     VisualizationWindow.Height);

          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);
        }
        else
        {
          BassVis.BASSVIS_SetVisPort(_visParam,
                                     _visParam.VisGenWinHandle,
                                     IntPtr.Zero,
                                     0,
                                     0,
                                     0,
                                     0);
        }

        // The Winamp Plugin has stolen focus on the MP window. Bring it back to froeground
        Win32API.SetForegroundWindow(GUIGraphicsContext.form.Handle);

        firstRun = false;
      }
      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Winamp visualization engine initialization failed with the following exception {0}",
          ex);
      }
      _Initialized = _visParam.VisHandle != 0;
      return _visParam.VisHandle != 0;
    }

    #endregion
  }
}