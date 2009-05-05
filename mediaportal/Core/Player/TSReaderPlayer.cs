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

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.Teletext;
using MediaPortal.Profile;

namespace MediaPortal.Player
{
  internal class TSReaderPlayer : BaseTSReaderPlayer
  {
    #region structs

    private static byte[] Mpeg2ProgramVideo =
      {
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcSource.left
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcSource.top
        0xd0, 0x02, 0x00, 0x00, //  .hdr.rcSource.right
        0x40, 0x02, 0x00, 0x00, //  .hdr.rcSource.bottom
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.left
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.top
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.right
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.bottom
        0xc0, 0xe1, 0xe4, 0x00, //  .hdr.dwBitRate
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwBitErrorRate
        0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwInterlaceFlags
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwCopyProtectFlags
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwPictAspectRatioX
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwPictAspectRatioY
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwReserved1
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwReserved2
        0x28, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biSize
        0xd0, 0x02, 0x00, 0x00, //  .hdr.bmiHeader.biWidth
        0x40, 0x02, 0x00, 0x00, //  .hdr.bmiHeader.biHeight
        0x00, 0x00, //  .hdr.bmiHeader.biPlanes
        0x00, 0x00, //  .hdr.bmiHeader.biBitCount
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biCompression
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biSizeImage
        0xd0, 0x07, 0x00, 0x00, //  .hdr.bmiHeader.biXPelsPerMeter
        0x42, 0xd8, 0x00, 0x00, //  .hdr.bmiHeader.biYPelsPerMeter
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biClrUsed
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biClrImportant
        0x00, 0x00, 0x00, 0x00, //  .dwStartTimeCode
        0x4c, 0x00, 0x00, 0x00, //  .cbSequenceHeader
        0x00, 0x00, 0x00, 0x00, //  .dwProfile
        0x00, 0x00, 0x00, 0x00, //  .dwLevel
        0x00, 0x00, 0x00, 0x00, //  .Flags
        //  .dwSequenceHeader [1]
        0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33,
        0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12,
        0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14,
        0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15,
        0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16,
        0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17,
        0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19,
        0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b,
        0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e,
        0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };

    private static byte[] MPEG2AudioFormat =
      {
        0x50, 0x00, //wFormatTag
        0x02, 0x00, //nChannels
        0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
        0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
        0x01, 0x00, //nBlockAlign
        0x00, 0x00, //wBitsPerSample
        0x16, 0x00, //cbSize
        0x02, 0x00, //wValidBitsPerSample
        0x00, 0xe8, //wSamplesPerBlock
        0x03, 0x00, //wReserved
        0x01, 0x00, 0x01, 0x00, //dwChannelMask
        0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };

    #endregion

    #region variables

    private VMR9Util _vmr9 = null;
    private IPin _pinAudio = null;
    private IPin _pinVideo = null;
    protected IBaseFilter _audioSwitcherFilter = null;
    protected IAudioStream _audioStream = null;
    protected ISubtitleStream _subtitleStream = null;
    protected TeletextReceiver _ttxtReceiver = null;
    protected ITeletextSource _teletextSource = null;
    private bool enableDVBBitmapSubtitles = false;
    private bool enableDVBTtxtSubtitles = false;
    private bool enableMPAudioSwitcher = false;
    private int  relaxTsReader = 0; // Disable dropping of discontinued dvb packets

    #endregion

    [Guid("558D9EA6-B177-4c30-9ED5-BF2D714BCBCA"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioStream
    {
      void GetAudioStream(ref Int32 stream);
    }

    /// <summary>
    /// Interface to the TsReader filter wich provides information about the 
    /// subtitle streams and allows us to change the current subtitle stream
    /// </summary>
    /// 
    [Guid("43FED769-C5EE-46aa-912D-7EBDAE4EE93A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISubtitleStream
    {
      void SetSubtitleStream(Int32 stream);
      void GetSubtitleStreamType(Int32 stream, ref Int32 type);
      void GetSubtitleStreamCount(ref Int32 count);
      void GetCurrentSubtitleStream(ref Int32 stream);
      void GetSubtitleStreamLanguage(Int32 stream, ref SUBTITLE_LANGUAGE szLanguage);
      void SetSubtitleResetCallback(IntPtr callBack);
    }

    /// <summary>
    /// Structure to pass the subtitle language data from TsReader to this class
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SUBTITLE_LANGUAGE
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public string lang;
    }

    #region ctor

    public TSReaderPlayer()
      : base()
    {
    }

    public TSReaderPlayer(g_Player.MediaType type)
      : base(type)
    {
    }

    #endregion

    public override eAudioDualMonoMode GetAudioDualMonoMode()
    {
      if (_audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      IMPAudioSwitcherFilter mpSwitcher = _audioSwitcherFilter as IMPAudioSwitcherFilter;
      if (mpSwitcher == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      uint iMode = 0;
      int hr = mpSwitcher.GetAudioDualMonoMode(out iMode);
      if (hr != 0)
      {
        Log.Info("TsReaderPlayer: GetAudioDualMonoMode failed 0x{0:X}", hr);
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      eAudioDualMonoMode mode = (eAudioDualMonoMode) iMode;
      Log.Info("TsReaderPlayer: GetAudioDualMonoMode mode={0} succeeded", iMode.ToString(), hr);
      return mode;
    }

    public override bool SetAudioDualMonoMode(eAudioDualMonoMode mode)
    {
      if (_audioSwitcherFilter == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return false;
      }
      IMPAudioSwitcherFilter mpSwitcher = _audioSwitcherFilter as IMPAudioSwitcherFilter;
      if (mpSwitcher == null)
      {
        Log.Info("TsReaderPlayer: AudioDualMonoMode switching not available. Audioswitcher filter not loaded");
        return false;
      }
      int hr = mpSwitcher.SetAudioDualMonoMode((uint) mode);
      if (hr != 0)
      {
        Log.Info("TsReaderPlayer: SetAudioDualMonoMode mode={0} failed 0x{1:X}", mode.ToString(), hr);
      }
      else
      {
        Log.Info("TsReaderPlayer: SetAudioDualMonoMode mode={0} succeeded", mode.ToString(), hr);
      }
      return (hr == 0);
    }

    protected bool IsVideoFile(string filename)
    {
      bool isTimeShiftStream = false;
      if (filename.ToLower().StartsWith("rtsp://"))
      {
        string url = filename.Remove(0, filename.LastIndexOf('/') + 1);
        isTimeShiftStream = (url.Substring(0, 6) == "stream");
      }
      return (filename.ToLower().IndexOf(".tsbuffer") < 0 && filename.ToLower().IndexOf("radio.tsbuffer") < 0 &&
              !isTimeShiftStream && !IsTV);
    }

    protected void LoadMyTvFilterSettings(ref int intFilters, ref string strFilters, ref string strVideoCodec,
                                          ref string strAudioCodec, ref string strAACAudioCodec,
                                          ref string strH264VideoCodec, ref string strAudioRenderer,
                                          ref bool enableDVBBitmapSubtitles, ref bool enableDVBTtxtSubtitles, ref int relaxTsReader)
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // FlipGer: load infos for custom filters
        int intCount = 0;
        while (xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("mytv", "usefilter" + intCount.ToString(), false))
          {
            strFilters += xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") + ";";
            intFilters++;
          }
          intCount++;
        }
        strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        strAACAudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
        strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
        enableDVBBitmapSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        enableDVBTtxtSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        enableMPAudioSwitcher = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        relaxTsReader = (xmlreader.GetValueAsBool("mytv", "relaxTsReader", false) == false ? 0 : 1); // as int for passing through interface
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);
        _CodecSupportsFastSeeking = xmlreader.GetValueAsBool("debug", "CodecSupportsFastSeeking", true);
      }
    }

    protected void LoadMyVideosFilterSettings(ref int intFilters, ref string strFilters, ref string strVideoCodec,
                                              ref string strAudioCodec, ref string strAACAudioCodec,
                                              ref string strH264VideoCodec, ref string strAudioRenderer,
                                              ref bool enableDVBBitmapSubtitles, ref bool enableDVBTtxtSubtitles, ref int relaxTsReader)
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
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
        strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        strH264VideoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");

        strAudioRenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
        enableDVBBitmapSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        enableDVBTtxtSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        enableMPAudioSwitcher = xmlreader.GetValueAsBool("movieplayer", "audiodualmono", false);
        relaxTsReader = (xmlreader.GetValueAsBool("mytv", "relaxTsReader", false) == false ? 0 : 1); // as int for passing through interface
        _CodecSupportsFastSeeking = xmlreader.GetValueAsBool("debug", "CodecSupportsFastSeeking", true);
      }
    }

    protected override void OnInitialized()
    {
      Log.Info("TSReaderPlayer: OnInitialized");
      if (_vmr9 != null)
      {
        _vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }
      if (!_updateNeeded)
      {
        return;
      }
      _updateNeeded = false;
      _isStarted = true;
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      //create the pins on the demux then connect the demux then mapp the pids.
      Speed = 1;
      Log.Info("TSReaderPlayer: GetInterfaces()");
      //Log.Info("TSReaderPlayer: build graph");
      try
      {
        Cleanup(); //before adding vmr9 - make sure that cleanup is done, otherwise MP could hang in (_vmr9.AddVMR9)
        _graphBuilder = (IGraphBuilder) new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph) _graphBuilder);

        #region add vmr9

        if (_isRadio == false)
        {
          Log.Info("TSReaderPlayer: add _vmr9");
          _vmr9 = new VMR9Util();
          _vmr9.AddVMR9(_graphBuilder);
          _vmr9.Enable(false);
        }

        #endregion

        #region add codecs

        Log.Info("TSReaderPlayer: add codecs");
        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAACAudioCodec = "";
        string strAudioRenderer = "";
        string strH264VideoCodec = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters

        if (IsVideoFile(filename))
        {
          LoadMyVideosFilterSettings(ref intFilters, ref strFilters, ref strVideoCodec, ref strAudioCodec,
                                     ref strAACAudioCodec, ref strH264VideoCodec, ref strAudioRenderer,
                                     ref enableDVBBitmapSubtitles, ref enableDVBTtxtSubtitles, ref relaxTsReader);
        }
        else
        {
          LoadMyTvFilterSettings(ref intFilters, ref strFilters, ref strVideoCodec, ref strAudioCodec,
                                 ref strAACAudioCodec, ref strH264VideoCodec, ref strAudioRenderer,
                                 ref enableDVBBitmapSubtitles, ref enableDVBTtxtSubtitles, ref relaxTsReader);
        }

        if (_isRadio == false)
        {
          try
          {
            if (strVideoCodec.Length > 0)
            {
              _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
            }
          }
          catch (Exception ex)
          {
            Log.Warn("Failed to add video decoder. Maybe it's missing?\nMessage: {0}", ex.Message);
          }
          try
          {
            if (strH264VideoCodec.Length > 0)
            {
              _h264videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strH264VideoCodec);
            }
          }
          catch (Exception ex)
          {
            Log.Warn("Failed to add h.264 video decoder. Maybe it's missing?\nMessage: {0}", ex.Message);
          }
        }
        if (strAudioCodec.Length > 0)
        {
          _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        }
        if (strAACAudioCodec.Length > 0)
        {
          _aacaudioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAACAudioCodec);
        }
        if (strAudioRenderer.Length > 0)
        {
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, true);
        }
        if (enableDVBBitmapSubtitles)
        {
          try
          {
            _subtitleFilter = SubtitleRenderer.GetInstance().AddSubtitleFilter(_graphBuilder);
          }
          catch (Exception e)
          {
            Log.Error(e);
          }
        }
        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
        }

        #endregion

        #region add AudioSwitcher

        if (enableMPAudioSwitcher)
        {
          Log.Info("Adding audio switcher to graph");
          _audioSwitcherFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal AudioSwitcher");
          if (_audioSwitcherFilter == null)
          {
            Log.Error("TSReaderPlayer: Failed to add AudioSwitcher to graph");
          }
        }

        #endregion

        #region add TsReader

        TsReader reader = new TsReader();
        _fileSource = (IBaseFilter) reader;
        ireader = (ITSReader) reader;
        ireader.SetRelaxedMode(relaxTsReader); // enable/disable continousity filtering
        ireader.SetTsReaderCallback(this);
        ireader.SetRequestAudioChangeCallback(this);
        Log.Info("TSReaderPlayer:add TsReader to graph");
        int hr = _graphBuilder.AddFilter((IBaseFilter) _fileSource, "TsReader");
        if (hr != 0)
        {
          Log.Error("TSReaderPlayer:Failed to add TsReader to graph");
          ireader = null;
          return false;
        }

        #endregion

        #region load file in TsReader

        IFileSourceFilter interfaceFile = (IFileSourceFilter) _fileSource;
        if (interfaceFile == null)
        {
          Log.Error("TSReaderPlayer:Failed to get IFileSourceFilter");
          return false;
        }
        Log.Info("TSReaderPlayer: open file: {0}", filename);
        hr = interfaceFile.Load(filename, null);
        if (hr != 0)
        {
          Log.Error("TSReaderPlayer:Failed to open file:{0} :0x{1:x}", filename, hr);
          return false;
        }

        #endregion

        #region render TsReader output pins

        if (_isRadio)
        {
          IEnumPins enumPins;
          _fileSource.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1)
            {
              break;
            }
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input)
            {
              continue;
            }
            IEnumMediaTypes enumMediaTypes;
            pins[0].EnumMediaTypes(out enumMediaTypes);
            AMMediaType[] mediaTypes = new AMMediaType[20];
            int fetchedTypes;
            enumMediaTypes.Next(20, mediaTypes, out fetchedTypes);
            for (int i = 0; i < fetchedTypes; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Audio)
              {
                _graphBuilder.Render(pins[0]);
                break;
              }
            }
          }
        }
        else
        {
          Log.Info("TSReaderPlayer:render TsReader outputs");
          IEnumPins enumPins;
          _fileSource.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1)
            {
              break;
            }
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input)
            {
              DirectShowUtil.ReleaseComObject(pins[0]);
              continue;
            }
            try
            {
              _graphBuilder.Render(pins[0]);
            }
            catch (Exception ex)
            {
              Log.Warn(
                "Failed to render an output pin of TsReader. Maybe it's a radio recording and we try to render the video pin.\nMessage: {0}",
                ex.Message);
            }

            DirectShowUtil.ReleaseComObject(pins[0]);
          }
          DirectShowUtil.ReleaseComObject(enumPins);
        }

        #endregion

        _mediaCtrl = (IMediaControl) _graphBuilder;
        _mediaEvt = (IMediaEventEx) _graphBuilder;
        _mediaSeeking = _graphBuilder as IMediaSeeking;
        if (_mediaSeeking == null)
        {
          Log.Error("Unable to get IMediaSeeking interface#1");
        }
        _audioStream = _fileSource as IAudioStream;
        if (_audioStream == null)
        {
          Log.Error("Unable to get IAudioStream interface");
        }
        _audioSelector = new AudioSelector(_audioStream);
        if (enableDVBTtxtSubtitles || enableDVBBitmapSubtitles)
        {
          try
          {
            SubtitleRenderer.GetInstance().SetPlayer(this);
            _dvbSubRenderer = SubtitleRenderer.GetInstance();
          }
          catch (Exception e)
          {
            Log.Error(e);
          }
        }
        if (enableDVBBitmapSubtitles)
        {
          _subtitleStream = _fileSource as ISubtitleStream;
          if (_subtitleStream == null)
          {
            Log.Error("Unable to get ISubtitleStream interface");
          }
        }
        if (enableDVBTtxtSubtitles)
        {
          //Log.Debug("TSReaderPlayer: Obtaining TeletextSource");
          _teletextSource = _fileSource as ITeletextSource;
          if (_teletextSource == null)
          {
            Log.Error("Unable to get ITeletextSource interface");
          }
          Log.Debug("TSReaderPlayer: Creating Teletext Receiver");
          TeletextSubtitleDecoder ttxtDecoder = new TeletextSubtitleDecoder(_dvbSubRenderer);
          _ttxtReceiver = new TeletextReceiver(_teletextSource, ttxtDecoder);
          // regardless of whether dvb subs are enabled, the following call is okay
          // if _subtitleStream is null the subtitle will just not setup for bitmap subs 
          _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, ttxtDecoder);
        }
        else if (enableDVBBitmapSubtitles)
        {
          // if only dvb subs are enabled, pass null for ttxtDecoder
          _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, null);
        }
        if (_audioRendererFilter != null)
        {
          //Log.Info("TSReaderPlayer:set reference clock");
          IMediaFilter mp = _graphBuilder as IMediaFilter;
          IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);
          //Log.Info("TSReaderPlayer:set reference clock:{0:X}", hr);
          _basicAudio = (IBasicAudio) _graphBuilder;
          //_mediaSeeking.SetPositions(new DsLong(0), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(0), AMSeekingSeekingFlags.NoPositioning);
        }
        if (_isRadio == false)
        {
          if (!_vmr9.IsVMR9Connected)
          {
            int count = 0;
            while (true)
            {
              Log.Debug("TSReaderPlayer: waiting for vmr9 connection");
              Application.DoEvents();
              if (count > 20)
              {
                Log.Debug(
                  "TSReaderPlayer: no vmr9 connection. Maybe we have a radio recording but expect video too so we suppose it's ok.");
                //gemx: we have to return TRUE here because otherwise we won't be able to play recoded radio from the RecordedTV screen
                return true;
                /*if (g_Player.IsRadio)
                {
                  return true;
                }
                else
                {
                  g_Player.Stop();
                  return false;
                }*/
                //Cleanup();
                //return false;
              }
              count++;
              Thread.Sleep(100);
            }
          }
          _vmr9.SetDeinterlaceMode();
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("TSReaderPlayer:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    protected override void ExclusiveMode(bool onOff)
    {
      GUIMessage msg = null;
      if (onOff)
      {
        Log.Info("TSReaderPlayer: Enabling DX9 exclusive mode");
        if (_isRadio == false)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        }
      }
      else
      {
        Log.Info("TSReaderPlayer: Disabling DX9 exclusive mode");
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      }
      GUIWindowManager.SendMessage(msg);
    }

    private void Cleanup()
    {
      int hr;
      Log.Info("TSReaderPlayer:cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        if (_graphBuilder != null)
        {
          _videoWin = _graphBuilder as IVideoWindow;
          if (_videoWin != null)
          {
            _videoWin.put_Visible(OABool.False);
          }
        }
        else
        {
          Log.Info("TSReaderPlayer:grapbuilder=null");
        }
        if (_vmr9 != null)
        {
          Log.Info("TSReaderPlayer: vmr9 disable");
          _vmr9.Enable(false);
        }
        if (_mediaCtrl != null)
        {
          int counter = 0;
          while (GUIGraphicsContext.InVmr9Render)
          {
            counter++;
            Thread.Sleep(100);
            if (counter > 100)
            {
              break;
            }
            break;
          }
          hr = _mediaCtrl.Stop();
          FilterState state;
          hr = _mediaCtrl.GetState(10, out state);
          Log.Info("state:{0} {1:X}", state.ToString(), hr);
          _mediaCtrl = null;
        }
        _mediaEvt = null;
        _mediaSeeking = null;
        _videoWin = null;
        _basicAudio = null;
        _basicVideo = null;
        if (_vmr9 != null)
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }
        if (_fileSource != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_fileSource)) > 0)
          {
            ;
          }
          _fileSource = null;
        }
        if (_pinAudio != null)
        {
          DirectShowUtil.ReleaseComObject(_pinAudio);
          _pinAudio = null;
        }
        if (_pinVideo != null)
        {
          DirectShowUtil.ReleaseComObject(_pinVideo);
          _pinVideo = null;
        }
        if (_videoCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_videoCodecFilter)) > 0)
          {
            ;
          }
          _videoCodecFilter = null;
        }
        if (_h264videoCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_h264videoCodecFilter)) > 0)
          {
            ;
          }
          _h264videoCodecFilter = null;
        }
        if (_audioCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioCodecFilter)) > 0)
          {
            ;
          }
          _audioCodecFilter = null;
        }
        if (_aacaudioCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_aacaudioCodecFilter)) > 0)
          {
            ;
          }
          _aacaudioCodecFilter = null;
        }
        if (_audioRendererFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioRendererFilter)) > 0)
          {
            ;
          }
          _audioRendererFilter = null;
        }
        if (_subtitleFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_subtitleFilter)) > 0)
          {
            ;
          }
          _subtitleFilter = null;
          if (this._dvbSubRenderer != null)
          {
            this._dvbSubRenderer.SetPlayer(null);
          }
          this._dvbSubRenderer = null;
        }
        // FlipGer: release custom filters
        if (customFilters != null)
        {
          for (int i = 0; i < customFilters.Length; i++)
          {
            if (customFilters[i] != null)
            {
              while ((hr = DirectShowUtil.ReleaseComObject(customFilters[i])) > 0)
              {
                ;
              }
            }
            customFilters[i] = null;
          }
        }
        if (_mpegDemux != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_mpegDemux)) > 0)
          {
            ;
          }
          _mpegDemux = null;
        }
        //	DsUtils.RemoveFilters(graphBuilder);
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (_graphBuilder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0)
          {
            ;
          }
          _graphBuilder = null;
        }
        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;
        GC.Collect();
      }
      catch (Exception ex)
      {
        Log.Error("TSReaderPlayer: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      Log.Info("TSReaderPlayer: Cleanup done");
    }

    protected override void OnProcess()
    {
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      if (_vmr9 != null)
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }

    public override void SeekAbsolute(double dTimeInSecs)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          lock (_mediaCtrl)
          {
            UpdateCurrentPosition();
            if (dTimeInSecs < 0)
            {
              dTimeInSecs = 0;
            }
            dTimeInSecs *= 10000000d;
            long lTime = (long) dTimeInSecs;
            long pStop = 0;
            long lContentStart, lContentEnd;
            _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
            lTime += lContentStart;
            Log.Info("TsReaderPlayer:seekabs:{0} start:{1} end:{2}", lTime, lContentStart, lContentEnd);
            /*if (lTime > lContentEnd)
            {
              System.Threading.Thread.Sleep(500);
              _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
              lTime = lContentEnd;
            }*/
            int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                                new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
            Log.Info("TsReaderPlayer seek done:{0:X}", hr);
            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
          }
        }

        UpdateCurrentPosition();
        if (_dvbSubRenderer != null)
        {
          _dvbSubRenderer.OnSeek(CurrentPosition);
        }
        _state = PlayState.Playing;
        Log.Info("TSReaderPlayer: current pos:{0} dur:{1}", CurrentPosition, Duration);
      }
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public override int SubtitleStreams
    {
      get
      {
        if (_subSelector != null)
        {
          return _subSelector.CountOptions();
        }
        else
        {
          return 0;
        }
      }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public override int CurrentSubtitleStream
    {
      get
      {
        if (_subSelector != null)
        {
          return _subSelector.GetCurrentOption();
        }
        else
        {
          return 0;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          _subSelector.SetOption(value);
        }
      }
    }

    /// <summary>
    /// Property to get the name for a subtitle stream
    /// </summary>
    public override string SubtitleLanguage(int iStream)
    {
      if (_subSelector != null)
      {
        return _subSelector.GetLanguage(iStream);
      }
      else
      {
        return Strings.Unknown;
      }
    }

    /// <summary>
    /// Property to enable/disable subtitles
    /// </summary>
    public override bool EnableSubtitle
    {
      get
      {
        if (_subSelector != null)
        {
          return _dvbSubRenderer.RenderSubtitles;
        }
        else
        {
          return false;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          _dvbSubRenderer.RenderSubtitles = value;
        }
      }
    }

    /// <summary>
    /// Property to specify channel change
    /// </summary>
    public override void OnZapping(int info)
    {
      if (ireader != null)
      {
        ireader.OnZapping(info);
      }
      Log.Info("TSReaderPlayer: OnZapping :{0}", info);
      
    }
    
  }
}