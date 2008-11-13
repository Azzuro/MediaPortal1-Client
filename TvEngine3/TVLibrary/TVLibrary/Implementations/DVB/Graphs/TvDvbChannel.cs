/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
//#define FORM


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.Helper;
using TvLibrary.Epg;
using TvLibrary.Teletext;
using TvLibrary.Log;
using TvLibrary.Helper;
using System.Threading;

namespace TvLibrary.Implementations.DVB
{
  public class TvDvbChannel : BaseSubChannel, ITeletextCallBack, IPMTCallback, ICACallback, ITvSubChannel, IVideoAudioObserver
  {
    #region variables
    #region local variables    
    protected int _pmtVersion;
    protected int _pmtPid = -1;
    protected ChannelInfo _channelInfo;

    protected ITsFilter _tsFilterInterface = null;
    protected int _subChannelIndex = -1;
    protected DVBAudioStream _currentAudioStream;
    protected MDPlugs _mdplugs = null;

    #region teletext
    private int _pmtLength;
    private byte[] _pmtData;
    #endregion
    #endregion

    #region events
    private ManualResetEvent _eventPMT; // gets signaled when PMT arrives
    private ManualResetEvent _eventCA; // gets signaled when CA arrives
    #endregion

    #region graph variables
    ConditionalAccess _conditionalAccess;
    IBaseFilter _filterTIF;
    IBaseFilter _filterTsWriter;
    IFilterGraph2 _graphBuilder;
    #endregion
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvDvbChannel"/> class.
    /// </summary>
    public TvDvbChannel()
    {
      _graphState = GraphState.Created;            
      _teletextDecoder = new DVBTeletext();
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();
      _pmtPid = -1;
      _parameters = new ScanParameters();
      _subChannelId = 0;
      _timeshiftFileName = "";
      _recordingFileName = "";
      _pmtData = null;
      _pmtLength = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvDvbChannel"/> class.
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="ca">The ca.</param>
    /// <param name="mdplugs">The mdplugs class.</param>
    /// <param name="tif">The tif filter.</param>
    /// <param name="tsWriter">The ts writer filter.</param>
    public TvDvbChannel(IFilterGraph2 graphBuilder, ref ConditionalAccess ca, ref MDPlugs mdplugs, IBaseFilter tif, IBaseFilter tsWriter, int subChannelId, IChannel channel)
    {
      _graphState = GraphState.Created;
      _graphBuilder = graphBuilder;
      _conditionalAccess = ca;
      _mdplugs = mdplugs;
      _filterTIF = tif;
      _filterTsWriter = tsWriter;      
      _teletextDecoder = new DVBTeletext();
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();
      _pmtPid = -1;
      _subChannelIndex = -1;
      _tsFilterInterface = (ITsFilter)_filterTsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelIndex);
      _parameters = new ScanParameters();
      _subChannelId = subChannelId;
      _conditionalAccess.AddSubChannel(_subChannelId, channel);
      _timeshiftFileName = "";
      _recordingFileName = "";
      _pmtData = null;
      _pmtLength = 0;
    }
    #endregion
        
    #region tuning and graph methods
    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public override void OnBeforeTune()
    {
      Log.Log.WriteFile("subch:{0} OnBeforeTune", _subChannelId);
      if (IsTimeShifting)
      {
        if (_subChannelIndex >= 0)
        {
          _tsFilterInterface.TimeShiftPause(_subChannelIndex, 1);
        }
      }
      _startTimeShifting = false;
      _startRecording = false;
      _channelInfo = new ChannelInfo();      
      _hasTeletext = false;
      _currentAudioStream = null;
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      Log.Log.WriteFile("subch:{0} OnAfterTune", _subChannelId);
      ArrayList pids = new ArrayList();
      pids.Add((ushort)0x0);//pat
      pids.Add((ushort)0x11);//sdt
      pids.Add((ushort)0x1fff);//padding stream
      if (_currentChannel != null)
      {
        DVBBaseChannel ch = (DVBBaseChannel)_currentChannel;
        if (ch.PmtPid > 0)
        {
          pids.Add((ushort)ch.PmtPid);//sdt
        }
      }

      _conditionalAccess.SendPids(_subChannelId,(DVBBaseChannel)_currentChannel,pids);

      _pmtPid = -1;
      _pmtVersion = -1;      
    }
    private bool WaitForPMT()
    {
      bool foundPMT = false;
      _pmtPid = -1;
      _pmtVersion = -1;      

      DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
      if (channel != null)
      {        
        _eventPMT = new ManualResetEvent(false);
        _eventCA = new ManualResetEvent(false);
        if (SetupPmtGrabber(channel.PmtPid, channel.ServiceId))
        {                    
          DateTime dtNow = DateTime.Now;
          int timeoutPMT = _parameters.TimeOutPMT * 1000;
          Log.Log.Debug("WaitForPMT: Waiting for PMT.");          
          if (_eventPMT.WaitOne(timeoutPMT, true))
          {
            _eventPMT.Close();
            _eventPMT = null;
            TimeSpan ts = DateTime.Now - dtNow;
            Log.Log.Debug("WaitForPMT: Found PMT after {0} seconds.", ts.TotalSeconds);
            foundPMT = true;                                    
            DateTime dtNowPMT2CAM = DateTime.Now;
            bool sendPmtToCamDone = false;
            try
            {                          
              bool updatePids;
              int retries = 0;
              int waitInterval = 100; //ms              

              while (retries < 4 && !sendPmtToCamDone) //lets keep trying to send pmt2cam for max. 4 retries.
              {
                sendPmtToCamDone = SendPmtToCam(out updatePids, out waitInterval);
                if (sendPmtToCamDone)
                {
                  if (updatePids)
                  {
                    if (_channelInfo != null)
                    {
                      SetMpegPidMapping(_channelInfo);
                      if (_mdplugs != null)
                        _mdplugs.SetChannel(_subChannelId, _currentChannel, _channelInfo);
                    }
                    Log.Log.Info("subch:{0} stop tif", _subChannelId);
                    if (_filterTIF != null)
                      _filterTIF.Stop();
                  }
                }
                else
                {                  
                  retries++;
                  Thread.Sleep(waitInterval);                  
                }
              }
              
            }
            catch (Exception ex)
            {
              Log.Log.WriteFile("subch:{0}", ex.Message);
              Log.Log.WriteFile("subch:{0}", ex.Source);
              Log.Log.WriteFile("subch::{0}", ex.StackTrace);
            }
            TimeSpan tsPMT2CAM = DateTime.Now - dtNowPMT2CAM;
            if (!sendPmtToCamDone)
            {              
              Log.Log.Debug("WaitForPMT: Timed out sending PMT to CAM {0} seconds.", tsPMT2CAM.TotalSeconds);
            }
            else
            {
              Log.Log.Debug("WaitForPMT: sending PMT to CAM took {0} seconds.", tsPMT2CAM.TotalSeconds);
            }
          }
          else
          {
            TimeSpan ts = DateTime.Now - dtNow;            
            Log.Log.Debug("WaitForPMT: Timed out waiting for PMT after {0} seconds. Increase the PMT timeout value?", ts.TotalSeconds);
            foundPMT = false;
          }                              
        }
      }
      if (_eventPMT != null)
      {
        _eventPMT.Close();
        _eventPMT = null;
      }
      return foundPMT;  
    }

    //[MethodImpl(MethodImplOptions.Synchronized)]
    protected bool GraphRunning()
    {      
      bool graphRunning = false;

      if (_graphBuilder != null)
      {
        try
        {
          FilterState state;
          (_graphBuilder as IMediaControl).GetState(10, out state);
          graphRunning = (state == FilterState.Running);
        }
        catch (InvalidComObjectException e)
        {
          // RCW error
          // ignore this error as, the graphbuilder is being disposed of in another thread.         
          return false;
        }
        catch (Exception e)
        {          
          Log.Log.Error("GraphRunning error : {0}", e.Message);
        }
      }
      //Log.Log.WriteFile("subch:{0} GraphRunning: {1}", _subChannelId, graphRunning);
      return graphRunning;
    }

    /// <summary>
    /// Should be called when the graph is about to start
    /// Resets the state 
    /// If graph is already running, starts the pmt grabber to grab the
    /// pmt for the new channel
    /// </summary>
    public override void OnGraphStart()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStart", _subChannelId);      

      if (GraphRunning())
      {
        WaitForPMT();
      }
      else
      {
        Log.Log.WriteFile("subch:{0} Graph already running - skip WaitForPMT", _subChannelId);
        if (_teletextDecoder != null)
          _teletextDecoder.ClearBuffer();

        _pmtPid = -1;
        _pmtVersion = -1;        
      }    
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// when the graph hasn't been running previously
    /// </summary>
    public override void OnGraphStarted()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStarted", _subChannelId);

      if (!GraphRunning())
      {
        _pmtPid = -1;
        _pmtVersion = -1;

        Log.Log.WriteFile("subch:{0} Graph not started - skip WaitForPMT", _subChannelId);
        return;
      }      
          
      WaitForPMT();     
    }

    /// <summary>
    /// Should be called when graph is about to stop.
    /// stops any timeshifting/recording on this channel
    /// </summary>
    public override void OnGraphStop()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStop", _subChannelId);
      _pmtPid = -1;
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;      
      _startTimeShifting = false;
      _startRecording = false;
      _pmtVersion = -1;      
      _recordingFileName = "";
      _channelInfo = new ChannelInfo();
      _currentChannel = null;
      _recordTransportStream = false;

      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        _tsFilterInterface.TimeShiftStop(_subChannelIndex);
        _graphState = GraphState.Created;
      }
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
    }

    /// <summary>
    /// should be called when graph has been stopped
    /// Resets the graph state
    /// </summary>
    public override void OnGraphStopped()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStopped", _subChannelId);
      _graphState = GraphState.Created;      
    }

    #endregion

    #region Timeshifting - Recording methods

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">if set to <c>true</c> [transport stream].</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    protected override void OnStartRecording(bool transportStream, string fileName)
    {
      Log.Log.WriteFile("subch:{0} StartRecord({1})", _subChannelId, fileName);
      _recordTransportStream = transportStream;
      int hr;
      if (_tsFilterInterface != null)
      {
        if (transportStream)
        {
          _tsFilterInterface.RecordSetMode(_subChannelIndex, TimeShiftingMode.TransportStream);
          Log.Log.WriteFile("subch:{0} record transport stream mode", _subChannelId);
        } else
        {
          _tsFilterInterface.RecordSetMode(_subChannelIndex, TimeShiftingMode.ProgramStream);
          Log.Log.WriteFile("subch:{0} record program stream mode", _subChannelId);
        }

        hr = _tsFilterInterface.RecordSetRecordingFileName(_subChannelIndex, fileName);
        if (hr != 0)
        {
          Log.Log.Error("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        //if (_channelInfo.pids.Count == 0)
        if (!PMTreceived)
        {
          Log.Log.WriteFile("subch:{0} StartRecord no pmt received yet", _subChannelId);
          _startRecording = true;
        } else
        {
          Log.Log.WriteFile("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _subChannelIndex);
          SetRecorderPids();
          hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
          if (hr != 0)
          {
            Log.Log.Error("subch:{0} tswriter StartRecord failed:{1:X}", _subChannelId, hr);
          }
          _graphState = GraphState.Recording;
        }
      }
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      if (IsRecording)
      {        
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopRecording...", _subChannelId, _subChannelIndex);

        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.RecordStopRecord(_subChannelIndex);
          if (_timeshiftFileName != "")
            _graphState = GraphState.TimeShifting;
          else
            _graphState = GraphState.Created;
        }
      }
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override bool OnStartTimeShifting(string fileName)
    {
      //if (_channelInfo.pids.Count == 0)
      if (!PMTreceived)
      {
        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName no pmt received. Timeshifting failed", _subChannelId);
        return false;
      }

      _timeshiftFileName = fileName;
      Log.Log.WriteFile("subch:{0} SetTimeShiftFileName:{1}", _subChannelId, fileName);
      //int hr;
      if (_tsFilterInterface != null)
      {
        Log.Log.WriteFile("Set video / audio observer");
        _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);       
        _tsFilterInterface.TimeShiftSetParams(_subChannelIndex, _parameters.MinimumFiles, _parameters.MaximumFiles, _parameters.MaximumFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileName(_subChannelIndex, fileName);
        _tsFilterInterface.TimeShiftSetMode(_subChannelIndex, TimeShiftingMode.TransportStream);
        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        _startTimeShifting = false;
        SetTimeShiftPids();
        Log.Log.WriteFile("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _subChannelIndex);
        _tsFilterInterface.TimeShiftStart(_subChannelIndex);
        
        _graphState = GraphState.TimeShifting;
      }
      return (_channelInfo.pids.Count != 0);
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      if (_timeshiftFileName != "")
      {        
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _subChannelIndex);
        if (_tsFilterInterface != null)
        {          
          _tsFilterInterface.TimeShiftStop(_subChannelIndex);         
        }
        _graphState = GraphState.Created;
      }
      _timeshiftFileName = "";
    }
    #endregion

    #region audio streams methods
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public override List<IAudioStream> AvailableAudioStreams
    {
      get
      {

        List<IAudioStream> streams = new List<IAudioStream>();
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isAC3Audio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            stream.StreamType = AudioStreamType.AC3;
            streams.Add(stream);
          } else if (info.isAudio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            if (info.IsMpeg1Audio)
              stream.StreamType = AudioStreamType.Mpeg1;
            if (info.IsMpeg2Audio)
              stream.StreamType = AudioStreamType.Mpeg2;
            if (info.IsAACAudio)
              stream.StreamType = AudioStreamType.AAC;
            if (info.IsLATMAACAudio)
              stream.StreamType = AudioStreamType.LATMAAC;
            else
              stream.StreamType = AudioStreamType.Unknown;
            streams.Add(stream);
          }
        }
        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public override IAudioStream CurrentAudioStream
    {
      get
      {
        return _currentAudioStream;
      }
      set
      {

        List<IAudioStream> streams = AvailableAudioStreams;
        DVBAudioStream audioStream = (DVBAudioStream)value;
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.AnalyzerSetAudioPid(_subChannelIndex, audioStream.Pid);
        }
        _currentAudioStream = audioStream;
        _pmtVersion = -1;
        bool updatePids;
        int interval = 0;
        SendPmtToCam(out updatePids, out interval);
      }
    }
    #endregion

    #region video streams methods

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public override bool IsReceivingAudioVideo
    {
      get
      {        
        if (GraphRunning() == false) return false;
        if (_tsFilterInterface == null) return false;
        if (_currentChannel == null) return false;

        int audioEncrypted = 0;
        int videoEncrypted = 0;
        _tsFilterInterface.AnalyzerIsAudioEncrypted(_subChannelIndex, out audioEncrypted);
        if (_currentChannel.IsTv)
        {
          _tsFilterInterface.AnalyzerIsVideoEncrypted(_subChannelIndex, out videoEncrypted);
        }
        return ((audioEncrypted == 0) && (videoEncrypted == 0));
      }
    }

    public override int GetCurrentVideoStream
    {
      get
      {
        if (_channelInfo == null) return -1;
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isVideo)
          {
            return info.stream_type;
          }
        }
        return -1;
      }
    }

    #endregion

    #region teletext
    protected override void OnGrabTeletext()
    {
      if (_grabTeletext)
      {
        int teletextPid = -1;
        foreach (PidInfo pidInfo in _channelInfo.pids)
        {
          if (pidInfo.isTeletext)
          {
            teletextPid = pidInfo.pid;
            break;
          }
        }

        if (teletextPid == -1)
        {
          Log.Log.Info("subch: stop grabbing teletext");
          _tsFilterInterface.TTxStop(_subChannelIndex);
          _grabTeletext = false;
          return;
        }
        Log.Log.Info("subch: start grabbing teletext");
        _tsFilterInterface.TTxSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, teletextPid);
        _tsFilterInterface.TTxStart(_subChannelIndex);
      } else
      {
        Log.Log.Info("subch: stop grabbing teletext");
        _tsFilterInterface.TTxStop(_subChannelIndex);
      }
    }

    #endregion

    #region OnDecompose
    /// <summary>
    /// disposes this channel
    /// </summary>
    protected override void OnDecompose()
    {
      if (_tsFilterInterface != null && _subChannelIndex >= 0)
      {
        _tsFilterInterface.DeleteChannel(_subChannelIndex);
        _subChannelIndex = -1;
      }
      _conditionalAccess.FreeSubChannel(_subChannelId);
    }

    #endregion

    #region pidmapping

    /// <summary>
    /// Instructs the ts analyzer filter to start grabbing the PMT
    /// </summary>
    /// <param name="pmtPid">pid of the PMT</param>
    /// <param name="serviceId">The service id.</param>
    protected bool SetupPmtGrabber(int pmtPid, int serviceId)
    {
      Log.Log.Info("subch:{0} SetupPmtGrabber:pid {1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      if (pmtPid < 0) return false;
      if (pmtPid == _pmtPid) return false;
      _pmtVersion = -1;
      _pmtPid = pmtPid;

      if (_conditionalAccess != null)
        _conditionalAccess.OnRunGraph(serviceId);

      Log.Log.Write("subch:{0} set pmt grabber pmt:{1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      _tsFilterInterface.PmtSetCallBack(_subChannelIndex, this);
      _tsFilterInterface.PmtSetPmtPid(_subChannelIndex, pmtPid, serviceId);
      if (_mdplugs != null)
      {
        Log.Log.Write("subch:{0} set ca grabber ", _subChannelId);
        _tsFilterInterface.CaSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.CaReset(_subChannelIndex);

      }

      return true;
    }

    /// <summary>
    /// maps the correct pids to the TsFileSink filter and teletext pins
    /// </summary>
    /// <param name="info"></param>
    protected void SetMpegPidMapping(ChannelInfo info)
    {
      if (info == null) return;
      try
      {
        Log.Log.WriteFile("subch:{0} SetMpegPidMapping", _subChannelId);

        ArrayList hwPids = new ArrayList();
        hwPids.Add((ushort)0x0);//PAT
        hwPids.Add((ushort)0x1);//CAT
        hwPids.Add((ushort)0x10);//NIT
        hwPids.Add((ushort)0x11);//SDT

        Log.Log.WriteFile("subch:{0}  pid:{1:X} pcr", _subChannelId, info.pcr_pid);
        Log.Log.WriteFile("subch:{0}  pid:{1:X} pmt", _subChannelId, info.network_pmt_PID);

        if (info.pids != null)
        {
          foreach (PidInfo pidInfo in info.pids)
          {
            Log.Log.WriteFile("subch:{0}  {1}", _subChannelId, pidInfo.ToString());
            if (pidInfo.pid == 0 || pidInfo.pid > 0x1fff) continue;
            if (pidInfo.isTeletext)
            {
              Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
              if (GrabTeletext)
              {
                _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
              _hasTeletext = true;
            }
            if (pidInfo.isAC3Audio || pidInfo.isAudio)
            {
              if (_currentAudioStream == null || pidInfo.isAC3Audio)
              {
                _currentAudioStream = new DVBAudioStream();
                _currentAudioStream.Pid = pidInfo.pid;
                _currentAudioStream.Language = pidInfo.language;
                if (pidInfo.IsMpeg1Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg1;
                else if (pidInfo.IsMpeg2Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg2;
                if (pidInfo.isAC3Audio)
                  _currentAudioStream.StreamType = AudioStreamType.AC3;
                if (pidInfo.IsAACAudio)
                  _currentAudioStream.StreamType = AudioStreamType.AAC;
                if (pidInfo.IsLATMAACAudio)
                  _currentAudioStream.StreamType = AudioStreamType.LATMAAC;
              }

              if (_currentAudioStream.Pid == pidInfo.pid)
              {
                Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
                _tsFilterInterface.AnalyzerSetAudioPid(_subChannelIndex, pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
            }

            if (pidInfo.isVideo)
            {
              Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
              hwPids.Add((ushort)pidInfo.pid);
              _tsFilterInterface.AnalyzerSetVideoPid(_subChannelIndex, pidInfo.pid);
              if (info.pcr_pid > 0 && info.pcr_pid != pidInfo.pid)
              {
                hwPids.Add((ushort)info.pcr_pid);
              }
            }
          }
        }
        if (info.network_pmt_PID >= 0 && ((DVBBaseChannel)_currentChannel).ServiceId >= 0)
        {
          hwPids.Add((ushort)info.network_pmt_PID);
          _conditionalAccess.SendPids(_subChannelId, (DVBBaseChannel)_currentChannel, hwPids);
        }

        if (_startTimeShifting)
        {
          _startTimeShifting = false;
          _tsFilterInterface.TimeShiftReset(_subChannelIndex);
          SetTimeShiftPids();
          _tsFilterInterface.TimeShiftStart(_subChannelIndex);

          Log.Log.WriteFile("Set video / audio observer");
          _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);

          _graphState = GraphState.TimeShifting;
        }
        if (_startRecording)
        {
          _startRecording = false;
          SetRecorderPids();

          int hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
          if (hr != 0)
          {
            Log.Log.Error("subch:[0} StartRecord failed:{1:X}", _subChannelId, hr);
          }
          _graphState = GraphState.Recording;
        } else if (IsTimeShifting)
        {
          SetTimeShiftPids();
        }
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }    

    /// <summary>
    /// Sets the pids for the timeshifter
    /// </summary>
    void SetTimeShiftPids()
    {
      //Log.Log.WriteFile("SetTimeShiftPids new DLL");
      if (_channelInfo == null) return;
      if (_channelInfo.pids.Count == 0) return;
      if (_currentChannel == null) return;
      //if (_currentAudioStream == null) return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null) return;

      try
      {
        _tsFilterInterface.TimeShiftPause(_subChannelIndex, 1);
        _tsFilterInterface.TimeShiftSetPmtPid(_subChannelIndex, dvbChannel.PmtPid, dvbChannel.ServiceId, _pmtData, _pmtLength);

        //_linkageScannerEnabled = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

        _tsFilterInterface.TimeShiftPause(_subChannelIndex, 0);
        _dateTimeShiftStarted = DateTime.Now;
      }
      catch (Exception e)
      {
        Log.Log.Error("could not set TimeShiftSetPmtPid {0}", e.Message);
      }
    }

    /// <summary>
    /// Sets the pids for the recorder
    /// </summary>
    void SetRecorderPids()
    {
      Log.Log.WriteFile("SetRecorderPids");
      if (_channelInfo == null) return;
      if (_channelInfo.pids.Count == 0) return;
      if (_currentChannel == null) return;
      if (_currentAudioStream == null) return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null) return;

      if (_recordTransportStream)
      {
        if (dvbChannel.PmtPid > 0)
        {
          _tsFilterInterface.RecordSetPmtPid(_subChannelIndex, dvbChannel.PmtPid, dvbChannel.ServiceId, _pmtData, _pmtLength);
        }
      } else
      {
        if (dvbChannel.PmtPid > 0)
        {
          _tsFilterInterface.RecordSetPmtPid(_subChannelIndex, dvbChannel.PmtPid, dvbChannel.ServiceId, _pmtData, _pmtLength);
        }
      }
      _dateRecordingStarted = DateTime.Now;
    }

    /// <summary>
    /// Decodes the PMT and sends the PMT to cam.
    /// </summary>
    protected bool SendPmtToCam(out bool updatePids, out int waitInterval)
    {
      lock (this)
      {
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        updatePids = false;
        waitInterval = 100;
        bool foundCA = false;
        if (_mdplugs != null)
        {
          if (channel != null)
          {
            //HACK: Currently Premiere Direkt Feeds (nid=133) have the free_ca flag in SDT set to true (means not scrambled), so we have to override this
            if ((!channel.FreeToAir) || (channel.NetworkId == 133 && !channel.Provider.Equals("BetaDigital")))
            {
              DateTime dtNow = DateTime.Now;
              //_eventCA = new ManualResetEvent(false);
              if (!_eventCA.WaitOne(10000, true)) //wait 10 sec for CA to arrive.
              {
                _eventCA.Close();
                _eventCA = null;
                TimeSpan ts = DateTime.Now - dtNow;
                Log.Log.Info("subch:{0} SendPmt:no CA found after {1} seconds", _subChannelId, ts.TotalSeconds);
                return false;
              }
              else
              {
                TimeSpan ts = DateTime.Now - dtNow;
                Log.Log.Info("subch:{0} SendPmt:CA found after {1}seconds", _subChannelId, ts.TotalSeconds);
                foundCA = true;
                _eventCA.Close();
                _eventCA = null;
              }              
            }
          }
        }
        
        if (channel == null)
        {
          Log.Log.Info("subch:{0} SendPmt:no channel set", _subChannelId);
          return true;
        }
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
        IntPtr catMem = Marshal.AllocCoTaskMem(4096);// max. size for cat
        try
        {
          _pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelIndex, pmtMem);
          if (_pmtLength > 6)
          {
            _pmtData = new byte[_pmtLength];
            int version = -1;
            Marshal.Copy(pmtMem, _pmtData, 0, _pmtLength);
            version = ((_pmtData[5] >> 1) & 0x1F);
            int pmtProgramNumber = (_pmtData[3] << 8) + _pmtData[4];
            Log.Log.Info("subch:{0} SendPmt:{1:X} {2:X} {3:X} {4:X}", _subChannelId, pmtProgramNumber, channel.ServiceId, _pmtVersion, version);
            if (pmtProgramNumber == channel.ServiceId)
            {
              if (_pmtVersion != version)
              {
                _channelInfo = new ChannelInfo();
                _channelInfo.DecodePmt(_pmtData);
                _channelInfo.network_pmt_PID = channel.PmtPid;
                if (channel.PcrPid <= 0)
                {
                  channel.PcrPid = _channelInfo.pcr_pid;
                }
                _channelInfo.pcr_pid = channel.PcrPid;
                // update any service scrambled / unscambled changes
                if (_channelInfo.scrambled = channel.FreeToAir)
                {
                  channel.FreeToAir = !_channelInfo.scrambled;
                }
                if ((_mdplugs != null) && (foundCA))
                {
                  try
                  {
                    int catLength = _tsFilterInterface.CaGetCaData(_subChannelIndex, catMem);
                    if (catLength > 0)
                    {
                      byte[] cat = new byte[catLength];
                      Marshal.Copy(catMem, cat, 0, catLength);
                      _channelInfo.DecodeCat(cat, catLength);
                    }
                  } catch (Exception ex)
                  {
                    Log.Log.Write(ex); ;
                  }
                }

                updatePids = true;
                Log.Log.WriteFile("subch:{0} SendPMT version:{1} len:{2} {3}", _subChannelId, version, _pmtLength, _channelInfo.caPMT.ProgramNumber);
                if (_conditionalAccess != null)
                {
                  int audioPid = -1;
                  if (_currentAudioStream != null)
                  {
                    audioPid = _currentAudioStream.Pid;
                  }

                  if (_conditionalAccess.SendPMT(_subChannelId, (DVBBaseChannel)CurrentChannel, _pmtData, _pmtLength, audioPid))
                  {
                    _pmtVersion = version;
                    Log.Log.WriteFile("subch:{0} cam flags:{1}", _subChannelId, _conditionalAccess.IsCamReady());
                    return true;
                  } 
                  else
                  {
                    //cam is not ready yet
                    Log.Log.WriteFile("subch:{0} SendPmt failed cam flags:{1}", _subChannelId, _conditionalAccess.IsCamReady());
                    _pmtVersion = -1;                    
                    waitInterval = 3000;
                    return false;
                  }
                } else
                {
                  Log.Log.Info("subch:{0} No cam in use", _subChannelId);
                }                
                _pmtVersion = version;                
                return true;
              } 
              else
              {
                //already received this pmt                
                return true;
              }
            }
          }
        } catch (Exception ex)
        {
          Log.Log.Write(ex);
        } finally
        {
          Marshal.FreeCoTaskMem(pmtMem);
          Marshal.FreeCoTaskMem(catMem);
        }
      }
      return false;
    }
    #endregion

    #region properties

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public bool PMTreceived
    {
      get
      {
        return (_pmtVersion > -1 && _channelInfo.pids.Count > 0);
      }      
    }

    #endregion

    #region tswriter callback handlers

    #region ICaCallback Members

    /// <summary>
    /// Called when tswriter.ax has received a new ca section
    /// </summary>
    /// <returns></returns>
    public int OnCaReceived()
    {
      if (_eventCA != null)
      {
        Log.Log.WriteFile("subch:OnCaReceived()");
        _eventCA.Set();
      }      

      return 0;
    }

    #endregion
   
    #region IPMTCallback Members
    /// <summary>
    /// Called when tswriter.ax has received a new pmt
    /// </summary>
    /// <returns></returns>
    public int OnPMTReceived()
    {           
      if (_eventPMT != null)
      {
        Log.Log.WriteFile("subch:{0} OnPMTReceived() {1}", _subChannelId, GraphRunning());
        _eventPMT.Set();
      }
      return 0;
    }



    #endregion

    #endregion

  }
}
