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
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;

namespace MediaPortal.Player
{
  public class RTSPPlayer : IPlayer
  {
    [ComImport, Guid("DF5ACC0A-5612-44ba-963B-C757298F4030")]
    protected class RtpSourceFilter { }
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }
    protected int m_iPositionX = 0;
    protected int m_iPositionY = 0;
    protected int m_iWidth = 200;
    protected int m_iHeight = 100;
    protected int m_iVideoWidth = 100;
    protected int m_iVideoHeight = 100;
    protected string m_strCurrentFile = "";
    protected bool _updateNeeded = false;
    protected MediaPortal.GUI.Library.Geometry.Type m_ar = MediaPortal.GUI.Library.Geometry.Type.Normal;
    protected bool m_bFullScreen = true;
    protected PlayState m_state = PlayState.Init;
    protected int m_iVolume = 100;
    protected IGraphBuilder graphBuilder;
    protected long m_speedRate = 10000;
    protected double m_dCurrentPos;
    protected double m_dDuration;
    protected int m_aspectX = 1;
    protected int m_aspectY = 1;

    IPin _pinPcr = null;
    IPin _pinSubtitle = null;
    IPin _pinPMT = null;
    bool enableDvbSubtitles = false;

    protected bool m_bStarted = false;
    protected DsROTEntry _rotEntry = null;

    /// <summary> control interface. </summary>
    protected IMediaControl mediaCtrl;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx mediaEvt;

    /// <summary> seek interface for positioning in stream. </summary>
    protected IMediaSeeking mediaSeek;
    /// <summary> seek interface to set position in stream. </summary>
    protected IMediaPosition mediaPos;
    /// <summary> video preview window interface. </summary>
    protected IVideoWindow videoWin;
    /// <summary> interface to get information and control video. </summary>
    protected IBasicVideo2 basicVideo;
    /// <summary> interface to single-step video. </summary>
    protected IBaseFilter videoCodecFilter = null;
    protected IBaseFilter audioCodecFilter = null;
    protected IBaseFilter audioRendererFilter = null;
    protected IBaseFilter _subtitleFilter = null;
    protected IBaseFilter[] customFilters; // FlipGer: array for custom directshow filters
    protected IBaseFilter _mpegDemux;
    protected IDirectVobSub vobSub;
    DateTime elapsedTimer = DateTime.Now;

    /// <summary> audio interface used to control volume. </summary>
    protected IBasicAudio basicAudio;
    protected const int WM_GRAPHNOTIFY = 0x00008001;	// message from graph

    protected const int WS_CHILD = 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected bool m_bVisible = false;
    protected DateTime updateTimer;
    protected g_Player.MediaType _mediaType;

    VMR9Util Vmr9 = null;

    public RTSPPlayer()
    {
      _mediaType = g_Player.MediaType.TV;
    }
    public RTSPPlayer(g_Player.MediaType mediaType)
    {
      _mediaType = mediaType;
    }

    protected void OnInitialized()
    {
      if (Vmr9 != null)
      {
        Vmr9.FrameCounter = 123;
        Vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }
    /// <summary> create the used COM components and get the interfaces. </summary>
    protected bool GetInterfaces()
    {
      Vmr9 = null;
      if (IsRadio == false)
      {
        Vmr9 = new VMR9Util();
      
        // switch back to directx fullscreen mode
        Log.Info("RTSPPlayer: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      //Type comtype = null;
      //object comobj = null;

      DsRect rect = new DsRect();
      rect.top = 0;
      rect.bottom = GUIGraphicsContext.form.Height;
      rect.left = 0;
      rect.right = GUIGraphicsContext.form.Width;


      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();

        Log.Info("RTSPPlayer: add source filter");
        if (IsRadio == false)
        {
          Vmr9.AddVMR9(graphBuilder);
          Vmr9.Enable(false);
        }

        _mpegDemux = (IBaseFilter)new MPEG2Demultiplexer();
        graphBuilder.AddFilter(_mpegDemux, "MPEG-2 Demultiplexer");

        IBaseFilter rtspSource = (IBaseFilter)new RtpSourceFilter();
        int hr = graphBuilder.AddFilter((IBaseFilter)rtspSource, "RTSP Source Filter");
        if (hr != 0)
        {
          Log.Error("RTSPPlayer:unable to add RTSP source filter:{0:X}", hr);
          return false;
        }

        // add preferred video & audio codecs
        Log.Info("RTSPPlayer: add video/audio codecs");
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudiorenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
          enableDvbSubtitles = xmlreader.GetValueAsBool("mytv", "dvbsubtitles", false);
          // FlipGer: load infos for custom filters
          int intCount = 0;
          while (xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
            if (xmlreader.GetValueAsBool("movieplayer", "usefilter" + intCount.ToString(), false))
            {
              strFilters += xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") + ";";
              intFilters++;
            }
            intCount++;
          }
        }
        string extension = System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
        if (IsRadio == false)
        {
          if (strVideoCodec.Length > 0) videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
        }
        if (strAudioCodec.Length > 0) audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);

        if (enableDvbSubtitles == true)
          _subtitleFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, "MediaPortal DVB subtitles transform");

        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(graphBuilder, arrFilters[i]);
        }
        if (strAudiorenderer.Length > 0) audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);


        Log.Info("RTSPPlayer: load:{0}", m_strCurrentFile);
        IFileSourceFilter interfaceFile = (IFileSourceFilter)rtspSource;
        if (interfaceFile == null)
        {
          Log.Error("RTSPPlayer:Failed to get IFileSourceFilter");
          return false;
        }


        //Log.Info("RTSPPlayer: open file:{0}",filename);
        hr = interfaceFile.Load(m_strCurrentFile, null);
        if (hr != 0)
        {
          Log.Error("RTSPPlayer:Failed to open file:{0} :0x{1:x}", m_strCurrentFile, hr);
          return false;
        }


        #region connect rtspsource->demux
        Log.Info("RTSPPlayer:connect rtspsource->mpeg2 demux");
        IPin pinTsOut = DsFindPin.ByDirection((IBaseFilter)rtspSource, PinDirection.Output, 0);
        if (pinTsOut == null)
        {
          Log.Info("RTSPPlayer:failed to find output pin of tsfilesource");
          return false;
        }
        IPin pinDemuxIn = DsFindPin.ByDirection(_mpegDemux, PinDirection.Input, 0);
        if (pinDemuxIn == null)
        {
          Log.Info("RTSPPlayer:failed to find output pin of tsfilesource");
          return false;
        }

        hr = graphBuilder.Connect(pinTsOut, pinDemuxIn);
        if (hr != 0)
        {
          Log.Info("RTSPPlayer:failed to connect rtspsource->mpeg2 demux:{0:X}", hr);
          return false;
        }
        Marshal.ReleaseComObject(pinTsOut);
        Marshal.ReleaseComObject(pinDemuxIn);

        #endregion

        #region render demux output pins
        if (IsRadio)
        {
          Log.Info("RTSPPlayer:render audio demux outputs");
          IEnumPins enumPins;
          _mpegDemux.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1) break;
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input) continue;
            IEnumMediaTypes enumMediaTypes;
            pins[0].EnumMediaTypes(out enumMediaTypes);
            AMMediaType[] mediaTypes = new AMMediaType[20];
            int fetchedTypes;
            enumMediaTypes.Next(20, mediaTypes, out fetchedTypes);
            for (int i = 0; i < fetchedTypes; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Audio)
              {
                graphBuilder.Render(pins[0]);
                break;
              }
            }
          }
        }
        else
        {
          Log.Info("RTSPPlayer:render audio/video demux outputs");
          IEnumPins enumPins;
          _mpegDemux.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1) break;
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input) continue;
            graphBuilder.Render(pins[0]);
          }
        }
        #endregion


        // Connect DVB subtitle filter pins in the graph
        if (_mpegDemux != null && enableDvbSubtitles == true)
        {
          IMpeg2Demultiplexer demuxer = _mpegDemux as IMpeg2Demultiplexer;
          hr = demuxer.CreateOutputPin(GetTSMedia(), "Pcr", out _pinPcr);

          if (hr == 0)
          {
            Log.Info("RTSPPlayer:_pinPcr OK");

            IPin pDemuxerPcr = DsFindPin.ByName(_mpegDemux, "Pcr");
            IPin pSubtitlePcr = DsFindPin.ByName(_subtitleFilter, "Audio");
            hr = graphBuilder.Connect(pDemuxerPcr, pSubtitlePcr);
          }
          else
          {
            Log.Info("RTSPPlayer:Failed to create _pinPcr in demuxer:{0:X}", hr);
          }

          hr = demuxer.CreateOutputPin(GetSubtitleMedia(), "Subtitle", out _pinSubtitle);
          if (hr == 0)
          {
            Log.Info("RTSPPlayer:_pinPcr OK");

            IPin pDemuxerSubtitle = DsFindPin.ByName(_mpegDemux, "Subtitle");
            IPin pSubtitle = DsFindPin.ByName(_subtitleFilter, "Subtitle");
            hr = graphBuilder.Connect(pDemuxerSubtitle, pSubtitle);
          }
          else
          {
            Log.Info("RTSPPlayer:Failed to create _pinSubtitle in demuxer:{0:X}", hr);
          }

          hr = demuxer.CreateOutputPin(GetTSMedia(), "PMT", out _pinPMT);
          if (hr == 0)
          {
            Log.Info("RTSPPlayer:_pinPMT OK");

            IPin pDemuxerSubtitle = DsFindPin.ByName(_mpegDemux, "PMT");
            IPin pSubtitle = DsFindPin.ByName(_subtitleFilter, "PMT");
            hr = graphBuilder.Connect(pDemuxerSubtitle, pSubtitle);
          }
          else
          {
            Log.Info("RTSPPlayer:Failed to create _pinPMT in demuxer:{0:X}", hr);
          }
        }


        if (IsRadio == false)
        {
          if (!Vmr9.IsVMR9Connected)
          {
            //VMR9 is not supported, switch to overlay
            Log.Info("RTSPPlayer: vmr9 not connected");
            mediaCtrl = null;
            Cleanup();
            return false;
          }
          Vmr9.SetDeinterlaceMode();
        }


        mediaCtrl = (IMediaControl)graphBuilder;
        mediaEvt = (IMediaEventEx)graphBuilder;
        mediaSeek = (IMediaSeeking)graphBuilder;
        mediaPos = (IMediaPosition)graphBuilder;
        basicAudio = graphBuilder as IBasicAudio;
        //DirectShowUtil.SetARMode(graphBuilder,AspectRatioMode.Stretched);
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        if (Vmr9 != null)
        {
          m_iVideoWidth = Vmr9.VideoWidth;
          m_iVideoHeight = Vmr9.VideoHeight;
        }
        if (audioRendererFilter != null)
        {
          Log.Info("TSStreamBufferPlayer9:set reference clock");
          IMediaFilter mp = graphBuilder as IMediaFilter;
          IReferenceClock clock = audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);
          Log.Info("TSStreamBufferPlayer9:set reference clock:{0:X}", hr);
        }
        Log.Info("RTSPPlayer: graph build successfull");
        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("RTSPPlayer:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    protected void OnProcess()
    {
      if (Vmr9 != null)
      {
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected void CloseInterfaces()
    {
      Cleanup();
    }

    void Cleanup()
    {
      if (graphBuilder == null) return;
      int hr;
      Log.Info("RTSPPlayer:cleanup DShow graph");
      try
      {
        videoWin = graphBuilder as IVideoWindow;
        if (videoWin != null)
          videoWin.put_Visible(OABool.False);
        if (Vmr9 != null)
        {
          Vmr9.Enable(false);
        }
        if (mediaCtrl != null)
        {

          int counter = 0;
          while (GUIGraphicsContext.InVmr9Render)
          {
            counter++;
            System.Threading.Thread.Sleep(1);
            if (counter > 200) break;
          }
          hr = mediaCtrl.Stop();
          FilterState state;
          hr = mediaCtrl.GetState(10, out state);
          Log.Info("state:{0} {1:X}", state.ToString(), hr);
          mediaCtrl = null;
        }
        mediaEvt = null;


        if (Vmr9 != null)
        {
          Vmr9.Dispose();
          Vmr9 = null;
        }

        mediaSeek = null;
        mediaPos = null;
        basicAudio = null;
        basicVideo = null;
        videoWin = null;

        if (_mpegDemux != null)
        {
          while ((hr = Marshal.ReleaseComObject(_mpegDemux)) > 0)
            ;
          _mpegDemux = null;
        }
        if (videoCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(videoCodecFilter) > 0) ;
          videoCodecFilter = null;
        }
        if (audioCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(audioCodecFilter) > 0) ;
          audioCodecFilter = null;
        }
        if (_subtitleFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_subtitleFilter)) > 0) ;
          _subtitleFilter = null;
        }
        if (audioRendererFilter != null)
        {
          while (Marshal.ReleaseComObject(audioRendererFilter) > 0) ;
          audioRendererFilter = null;
        }
        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
          if (customFilters[i] != null)
          {
            while ((hr = Marshal.ReleaseComObject(customFilters[i])) > 0) ;
          }
          customFilters[i] = null;
        }

        if (vobSub != null)
        {
          while ((hr = Marshal.ReleaseComObject(vobSub)) > 0) ;
          vobSub = null;
        }
        //	DsUtils.RemoveFilters(graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;

        if (graphBuilder != null)
        {
          while ((hr = Marshal.ReleaseComObject(graphBuilder)) > 0) ;
          graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;
        GC.Collect();
      }
      catch (Exception ex)
      {
        Log.Error("RTSPPlayer: Exception while cleanuping DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        Log.Info("RTSPPlayer: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }

    public override bool IsTV
    {
      get
      {
        return (_mediaType == g_Player.MediaType.TV);
      }
    }
    public override bool IsRadio
    {
      get
      {
        return (_mediaType == g_Player.MediaType.Radio);
      }
    }

    public override bool IsTimeShifting
    {
      get
      {
        return (_mediaType == g_Player.MediaType.TV || _mediaType == g_Player.MediaType.Radio);
      }
    }

    public override bool Play(string strFile)
    {
      updateTimer = DateTime.Now;
      m_speedRate = 10000;
      m_bVisible = false;
      m_iVolume = 100;
      m_state = PlayState.Init;
      m_strCurrentFile = strFile;
      m_bFullScreen = true;
      m_ar = GUIGraphicsContext.ARType;

      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      _updateNeeded = true;
      Log.Info("RTSPPlayer:play {0}", strFile);
      //lock ( typeof(VideoPlayerVMR7) )
      {
        GC.Collect();
        CloseInterfaces();
        GC.Collect();
        m_bStarted = false;
        if (!GetInterfaces())
        {
          m_strCurrentFile = "";
          return false;
        }
        int hr = mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Can not set notifications");
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }

        DirectShowUtil.SetARMode(graphBuilder, AspectRatioMode.Stretched);
        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);

        // DsUtils.DumpFilters(graphBuilder);
        hr = mediaCtrl.Run();
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Unable to start movie");
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
        msg.Label = strFile;
        GUIWindowManager.SendThreadMessage(msg);
        m_state = PlayState.Playing;
        //Brutus GUIGraphicsContext.IsFullScreenVideo=true;
        m_iPositionX = GUIGraphicsContext.VideoWindow.X;
        m_iPositionY = GUIGraphicsContext.VideoWindow.Y;
        m_iWidth = GUIGraphicsContext.VideoWindow.Width;
        m_iHeight = GUIGraphicsContext.VideoWindow.Height;
        m_ar = GUIGraphicsContext.ARType;
        _updateNeeded = true;
        SetVideoWindow();
        mediaPos.get_Duration(out m_dDuration);
        Log.Info("RTSPPlayer:Duration:{0}", m_dDuration);
        if (_mediaType == g_Player.MediaType.TV)
        {
          if (m_dDuration < 1) m_dDuration = 1;
          SeekAbsolute(m_dDuration - 1);
        }
        else
        {
          SeekAbsolute(0);
        }

        OnInitialized();
      }
      return true;
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.Vmr9Active)
      {
        _updateNeeded = false;
        m_bStarted = true;
        return;
      }

    }

    void MovieEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      m_strCurrentFile = "";
      if (!bManualStop)
      {
        CloseInterfaces();
        m_state = PlayState.Ended;
        GUIGraphicsContext.IsPlaying = false;
      }
    }


    public override void Process()
    {
      if (!Playing) return;
      if (!m_bStarted) return;
      if (GUIGraphicsContext.InVmr9Render) return;
      TimeSpan ts = DateTime.Now - updateTimer;
      if (ts.TotalMilliseconds >= 800 || m_speedRate != 1)
      {
        if (mediaPos != null)
        {
          mediaPos.get_Duration(out m_dDuration);
          mediaPos.get_CurrentPosition(out m_dCurrentPos);
          //Log.Info("rtsp: pos:{0} duration:{1}", m_dCurrentPos, m_dDuration);
        }

        if (GUIGraphicsContext.BlankScreen || (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
        {
          if (m_bVisible)
          {
            m_bVisible = false;
            if (videoWin != null) videoWin.put_Visible(OABool.False);
          }
        }
        else if (!m_bVisible)
        {
          m_bVisible = true;
          if (videoWin != null) videoWin.put_Visible(OABool.True);
        }
        CheckVideoResolutionChanges();
        updateTimer = DateTime.Now;
      }
      if (m_speedRate != 1)
      {
        DoFFRW();
      }
      OnProcess();
    }

    void CheckVideoResolutionChanges()
    {
      if (videoWin == null || basicVideo == null) return;
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (basicVideo != null)
      {
        basicVideo.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (basicVideo != null)
      {
        basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != m_iVideoHeight || videoWidth != m_iVideoWidth ||
        aspectX != m_aspectX || aspectY != m_aspectY)
      {
        _updateNeeded = true;
        SetVideoWindow();
      }
    }




    public override int PositionX
    {
      get { return m_iPositionX; }
      set
      {
        if (value != m_iPositionX)
        {
          m_iPositionX = value;
          _updateNeeded = true;
        }
      }
    }

    public override int PositionY
    {
      get { return m_iPositionY; }
      set
      {
        if (value != m_iPositionY)
        {
          m_iPositionY = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return m_iWidth; }
      set
      {
        if (value != m_iWidth)
        {
          m_iWidth = value;
          _updateNeeded = true;
        }
      }
    }
    public override int RenderHeight
    {
      get { return m_iHeight; }
      set
      {
        if (value != m_iHeight)
        {
          m_iHeight = value;
          _updateNeeded = true;
        }
      }
    }
    public override double Duration
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          return m_dDuration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          return m_dCurrentPos;
        }
        return 0.0d;
      }
    }

    public override bool FullScreen
    {
      get
      {
        return GUIGraphicsContext.IsFullScreenVideo;
      }
      set
      {
        if (value != m_bFullScreen)
        {
          m_bFullScreen = value;
          _updateNeeded = true;
        }
      }
    }
    public override int Width
    {
      get
      {
        return m_iVideoWidth;
      }
    }

    public override int Height
    {
      get
      {
        return m_iVideoHeight;
      }
    }

    public override void Pause()
    {
      if (m_state == PlayState.Paused)
      {
        m_speedRate = 10000;
        mediaCtrl.Run();
        m_state = PlayState.Playing;
      }
      else if (m_state == PlayState.Playing)
      {
        m_state = PlayState.Paused;
        mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get
      {
        return (m_state == PlayState.Paused);
      }
    }

    public override bool Playing
    {
      get
      {
        return (m_state == PlayState.Playing || m_state == PlayState.Paused);
      }
    }

    public override bool Stopped
    {
      get
      {
        return (m_state == PlayState.Init);
      }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile; }
    }

    public override void Stop()
    {
      if (m_state != PlayState.Init)
      {
        Log.Info("RTSPPlayer:ended {0}", m_strCurrentFile);
        m_strCurrentFile = "";
        CloseInterfaces();
        m_state = PlayState.Init;
        GUIGraphicsContext.IsPlaying = false;

      }
    }

    public override int Speed
    {
      get
      {
        if (m_state == PlayState.Init) return 1;
        if (mediaSeek == null) return 1;
        switch (m_speedRate)
        {
          case -10000:
            return -1;
          case -15000:
            return -2;
          case -30000:
            return -4;
          case -45000:
            return -8;
          case -60000:
            return -16;
          case -75000:
            return -32;

          case 10000:
            return 1;
          case 15000:
            return 2;
          case 30000:
            return 4;
          case 45000:
            return 8;
          case 60000:
            return 16;
          default:
            return 32;
        }
      }
      set
      {
        if (m_state != PlayState.Init)
        {
          if (mediaSeek != null)
          {
            switch ((int)value)
            {
              case -1: m_speedRate = -10000; break;
              case -2: m_speedRate = -15000; break;
              case -4: m_speedRate = -30000; break;
              case -8: m_speedRate = -45000; break;
              case -16: m_speedRate = -60000; break;
              case -32: m_speedRate = -75000; break;

              case 1:
                m_speedRate = 10000;
                mediaCtrl.Run();
                break;
              case 2: m_speedRate = 15000; break;
              case 4: m_speedRate = 30000; break;
              case 8: m_speedRate = 45000; break;
              case 16: m_speedRate = 60000; break;
              default: m_speedRate = 75000; break;
            }
          }
        }
        Log.Info("RTSPPlayer:SetRate to:{0}", m_speedRate);
      }
    }


    public override int Volume
    {
      get { return m_iVolume; }
      set
      {
        if (m_iVolume != value)
        {
          m_iVolume = value;
          if (m_state != PlayState.Init)
          {
            if (basicAudio != null)
            {
              // Divide by 100 to get equivalent decibel value. For example, �10,000 is �100 dB. 
              float fPercent = (float)m_iVolume / 100.0f;
              int iVolume = (int)((DirectShowVolume.VOLUME_MAX - DirectShowVolume.VOLUME_MIN) * fPercent);
              basicAudio.put_Volume((iVolume - DirectShowVolume.VOLUME_MIN));
            }
          }
        }
      }
    }

    public override MediaPortal.GUI.Library.Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType; }
      set
      {
        if (m_ar != value)
        {
          m_ar = value;
          _updateNeeded = true;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {

          double dCurTime;
          mediaPos.get_CurrentPosition(out dCurTime);

          dTime = dCurTime + dTime;
          if (dTime < 0.0d) dTime = 0.0d;
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          if (dTime < 0.0d) dTime = 0.0d;
          if (dTime < Duration)
          {
            Log.Info("seekabs:{0}", dTime);
            if (Vmr9 != null)
              Vmr9.FrameCounter = 123;
            mediaPos.put_CurrentPosition(dTime);
            Log.Info("seekabs:{0} done", dTime);
            if (Vmr9 != null)
              Vmr9.FrameCounter = 123;
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          double dCurrentPos;
          mediaPos.get_CurrentPosition(out dCurrentPos);
          double dDuration = Duration;

          double fCurPercent = (dCurrentPos / Duration) * 100.0d;
          double fOnePercent = Duration / 100.0d;
          fCurPercent = fCurPercent + (double)iPercentage;
          fCurPercent *= fOnePercent;
          if (fCurPercent < 0.0d) fCurPercent = 0.0d;
          if (fCurPercent < Duration)
          {
            mediaPos.put_CurrentPosition(fCurPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          if (iPercentage < 0) iPercentage = 0;
          if (iPercentage >= 100) iPercentage = 100;
          double fPercent = Duration / 100.0f;
          fPercent *= (double)iPercentage;
          mediaPos.put_CurrentPosition(fPercent);
        }
      }
    }


    public override bool HasVideo
    {
      get { return true; }
    }

    public override bool Ended
    {
      get { return m_state == PlayState.Ended; }
    }


    public override void WndProc(ref Message m)
    {
      if (m.Msg == WM_GRAPHNOTIFY)
      {
        if (mediaEvt != null)
          OnGraphNotify();
        return;
      }
      base.WndProc(ref m);
    }

    void OnGraphNotify()
    {
      if (mediaEvt == null) return;
      int p1, p2, hr = 0;
      EventCode code;
      do
      {
        hr = mediaEvt.GetEvent(out code, out p1, out p2, 0);
        if (hr < 0)
          break;
        hr = mediaEvt.FreeEventParams(code, p1, p2);
        if (code == EventCode.Complete || code == EventCode.ErrorAbort)
        {
          MovieEnded(false);
          return;
        }
      }
      while (hr == 0);
    }

    protected void DoFFRW()
    {

      if (!Playing)
        return;

      if ((m_speedRate == 10000) || (mediaSeek == null))
        return;

      TimeSpan ts = DateTime.Now - elapsedTimer;
      if (ts.TotalMilliseconds < 100) return;
      long earliest, latest, current, stop, rewind, pStop;

      mediaSeek.GetAvailable(out earliest, out latest);
      mediaSeek.GetPositions(out current, out stop);

      // Log.Info("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
      //         earliest/10000000,latest/10000000,current/10000000,stop/10000000,m_speedRate, (latest-earliest)/10000000);

      //earliest += + 30 * 10000000;

      // new time = current time + 2*timerinterval* (speed)
      long lTimerInterval = (long)ts.TotalMilliseconds;
      if (lTimerInterval > 300) lTimerInterval = 300;
      lTimerInterval = 300;
      rewind = (long)(current + (2 * (long)(lTimerInterval) * m_speedRate));

      int hr;
      pStop = 0;

      // if we end up before the first moment of time then just
      // start @ the beginning
      if ((rewind < earliest) && (m_speedRate < 0))
      {
        m_speedRate = 10000;
        rewind = earliest;
        //Log.Info(" seek back:{0}",rewind/10000000);
        hr = mediaSeek.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        mediaCtrl.Run();
        return;
      }
      // if we end up at the end of time then just
      // start @ the end-100msec
      if ((rewind > (latest - 100000)) && (m_speedRate > 0))
      {
        m_speedRate = 10000;
        rewind = latest - 100000;
        //Log.Info(" seek ff:{0}",rewind/10000000);
        hr = mediaSeek.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        mediaCtrl.Run();
        return;
      }

      //seek to new moment in time
      //Log.Info(" seek :{0}",rewind/10000000);
      hr = mediaSeek.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
      mediaCtrl.Pause();
    }

    #region IDisposable Members

    public override void Release()
    {
      CloseInterfaces();
    }
    #endregion

    AMMediaType GetTSMedia()
    {
      AMMediaType TS = new AMMediaType();
      TS.majorType = MediaType.Stream;
      TS.subType = MediaSubType.Mpeg2Transport;
      TS.formatType = FormatType.Null;
      TS.formatPtr = IntPtr.Zero;
      TS.sampleSize = 1;
      TS.temporalCompression = false;
      TS.fixedSizeSamples = true;
      TS.unkPtr = IntPtr.Zero;
      TS.formatType = FormatType.None;
      TS.formatSize = 0;
      TS.formatPtr = IntPtr.Zero;
      return TS;
    }

    AMMediaType GetSubtitleMedia()
    {
      AMMediaType mediaSubtitle = new AMMediaType();
      mediaSubtitle.majorType = MediaType.Null;
      mediaSubtitle.subType = MediaSubType.Null;
      mediaSubtitle.formatType = FormatType.Null;
      mediaSubtitle.formatPtr = IntPtr.Zero;
      mediaSubtitle.sampleSize = 1;
      mediaSubtitle.temporalCompression = false;
      mediaSubtitle.fixedSizeSamples = true;
      mediaSubtitle.unkPtr = IntPtr.Zero;
      mediaSubtitle.formatType = FormatType.None;
      mediaSubtitle.formatSize = 0;
      mediaSubtitle.formatPtr = IntPtr.Zero;
      return mediaSubtitle;
    }

  }
}
