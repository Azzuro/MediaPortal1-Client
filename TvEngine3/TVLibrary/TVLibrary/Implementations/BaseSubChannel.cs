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
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Microsoft.Win32;
using DirectShowLib;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Log;
using TvLibrary.Teletext;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.Helper;
using TvLibrary.Helper;
using TvLibrary.ChannelLinkage;
using TvLibrary.Implementations.Analog;


namespace TvLibrary.Implementations
{
  /// <summary>
  /// Base class for a sub channel of a tv card
  /// </summary>
  public abstract class BaseSubChannel : ITvSubChannel
  {
    #region Audio/Video Observer event and method
    /// <summary>
    /// Delegate for the audio video oberserver events
    /// </summary>
    /// <param name="pidType">Type of the pid</param>
    public delegate void AudioVideoObserverEvent(PidType pidType);

    /// <summary>
    /// Audio observer event
    /// </summary>
    public event AudioVideoObserverEvent AudioVideoEvent;

    /// <summary>
    /// Handles the audio video observer event
    /// </summary>
    /// <param name="pidType">Type of the pid</param>
    protected void OnAudioVideoEvent(PidType pidType)
    {
      if (AudioVideoEvent != null)
      {
        AudioVideoEvent(pidType);
      }
    }
    #endregion

    #region variables
    /// <summary>
    /// Indicates, if the channel has teletext
    /// </summary>
    protected bool _hasTeletext;
    /// <summary>
    /// Indicates, if teletext grabbing is activated
    /// </summary>
    protected bool _grabTeletext;
    /// <summary>
    /// Instance of the teletext decoder
    /// </summary>
    protected DVBTeletext _teletextDecoder;
    /// <summary>
    /// Struct of a ts header
    /// </summary>
    protected TSHelperTools.TSHeader _packetHeader;
    /// <summary>
    /// Instance of ts helper class
    /// </summary>
    protected TSHelperTools _tsHelper;
    /// <summary>
    /// Instance of the current channel
    /// </summary>
    protected IChannel _currentChannel;
    /// <summary>
    /// Name of the timeshift file
    /// </summary>
    protected string _timeshiftFileName;
    /// <summary>
    /// Name of the recording file
    /// </summary>
    protected string _recordingFileName;
    /// <summary>
    /// Date and time when timeshifting started
    /// </summary>
    protected DateTime _dateTimeShiftStarted;
    /// <summary>
    /// Date  and time when recording started
    /// </summary>
    protected DateTime _dateRecordingStarted;
    /// <summary>
    /// ID of this subchannel
    /// </summary>
    protected int _subChannelId;
    /// <summary>
    /// Current state of the graph
    /// </summary>
    protected GraphState _graphState;
    /// <summary>
    /// Indicates, if the graph is running
    /// </summary>
    protected bool _graphRunning;
    /// <summary>
    /// Scanning parameters
    /// </summary>
    protected ScanParameters _parameters;
    /// <summary>
    /// Indicates, if this subchannel is recording in transport stream format
    /// </summary>
    protected bool _isRecordingsTransportStream;
    /// <summary>
    /// Indicates, if timeshifting is started
    /// </summary>
    protected bool _startTimeShifting = false;
    /// <summary>
    /// Indicates, if recording is started
    /// </summary>
    protected bool _startRecording = false;
    /// <summary>
    /// Indicates, if this subchannel should record in transport stream format
    /// </summary>
    protected bool _recordTransportStream = false;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSubChannel"/> class.
    /// </summary>
    public BaseSubChannel()
    {
      _teletextDecoder = new DVBTeletext();
      _timeshiftFileName = String.Empty;
      _recordingFileName = String.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      _dateTimeShiftStarted = DateTime.MinValue;
      _graphRunning = false;
      _graphState = GraphState.Created;
      _tsHelper = new TSHelperTools();
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the sub channel id.
    /// </summary>
    /// <value>The sub channel id.</value>
    public int SubChannelId
    {
      get
      {
        return _subChannelId;
      }
    }

    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return _timeshiftFileName;
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get
      {
        return _dateTimeShiftStarted;
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        return _dateRecordingStarted;
      }
    }

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    public string RecordingFileName
    {
      get
      {
        return _recordingFileName;
      }
    }

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get
      {
        return (_recordingFileName.Length > 0);
      }
    }

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    public bool IsTimeShifting
    {
      get
      {
        return (_timeshiftFileName.Length > 0);
      }
    }

    /// <summary>
    /// returns the IChannel to which the card is currently tuned
    /// </summary>
    public IChannel CurrentChannel
    {
      get
      {
        return _currentChannel;
      }
      set
      {
        _currentChannel = value;
      }
    }

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsTimeshiftingTransportStream
    {
      get
      {
        if (IsTimeShifting) return true;
        return false;
      }
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsRecordingTransportStream
    {
      get
      {
        return _isRecordingsTransportStream;
      }
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public ScanParameters Parameters
    {
      get
      {
        return _parameters;
      }
      set
      {
        _parameters = value;
      }
    }

    #endregion

    #region teletext
    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    /// <value></value>
    public bool HasTeletext
    {
      get
      {
        return _hasTeletext;
      }
    }

    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    public bool GrabTeletext
    {
      get
      {
        return _grabTeletext;
      }
      set
      {
        _grabTeletext = value;
        OnGrabTeletext();
      }
    }

    /// <summary>
    /// returns the ITeletext interface used for retrieving the teletext pages
    /// </summary>
    public ITeletext TeletextDecoder
    {
      get
      {
        if (!_hasTeletext) return null;
        return _teletextDecoder;
      }
    }

    #endregion

    #region timeshifting and recording
    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns></returns>
    public bool StartTimeShifting(string fileName)
    {
      return OnStartTimeShifting(fileName);
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    public bool StopTimeShifting()
    {
      OnStopTimeShifting();
      _startTimeShifting = false;
      _dateTimeShiftStarted = DateTime.MinValue;
      _graphState = GraphState.Created;
      return true;
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    public bool StartRecording(bool transportStream, string fileName)
    {
      Log.Log.WriteFile("StartRecording to {0}", fileName);
      OnStartRecording(transportStream, fileName);
      _isRecordingsTransportStream = transportStream;
      _recordingFileName = fileName;
      Log.Log.WriteFile("Analog:Started recording");
      _graphState = GraphState.Recording;
      return true;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    public bool StopRecording()
    {
      OnStopRecording();
      _isRecordingsTransportStream = false;
      if (_timeshiftFileName != "")
      {
        _graphState = GraphState.TimeShifting;
      } else
      {
        _graphState = GraphState.Created;
      }
      _recordingFileName = "";
      _dateRecordingStarted = DateTime.MinValue;
      return true;
    }
    #endregion

    #region IAnalogTeletextCallBack and ITeletextCallBack Members

    /// <summary>
    /// callback from the TsWriter filter when it received a new teletext packets
    /// </summary>
    /// <param name="data">teletext data</param>
    /// <param name="packetCount">number of packets in data</param>
    /// <returns></returns>
    public int OnTeletextReceived(IntPtr data, short packetCount)
    {
      try
      {
        for (int i = 0; i < packetCount; ++i)
        {
          IntPtr packetPtr = new IntPtr(data.ToInt32() + i * 188);
          ProcessPacket(packetPtr);
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile(ex.ToString());
      }
      return 0;
    }

    /// <summary>
    /// processes a single transport packet
    /// Called from BufferCB
    /// </summary>
    /// <param name="ptr">pointer to the transport packet</param>
    public void ProcessPacket(IntPtr ptr)
    {
      if (ptr == IntPtr.Zero) return;

      _packetHeader = _tsHelper.GetHeader((IntPtr)ptr);
      if (_packetHeader.SyncByte != 0x47)
      {
        Log.Log.WriteFile("packet sync error");
        return;
      }
      if (_packetHeader.TransportError == true)
      {
        Log.Log.WriteFile("packet transport error");
        return;
      }
      // teletext
      //if (_grabTeletext)
      {
        if (_teletextDecoder != null)
        {
          _teletextDecoder.SaveData((IntPtr)ptr);
        }
      }
    }

    #endregion

    #region IAnalogVideoAudioObserver
    /// <summary>
    /// Called when tswriter.ax has seen the video / audio data for the first time
    /// </summary>
    /// <returns></returns>
    public int OnNotify(PidType pidType)
    {
      try
      {
        Log.Log.WriteFile("PID seen - type = {0}", pidType);
        OnAudioVideoEvent(pidType);
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return 0;
    }
    #endregion

    #region public helper
    /// <summary>
    /// Decomposes the sub channel
    /// </summary>
    public void Decompose()
    {
      Log.Log.Info("analog subch:{0} Decompose()", _subChannelId);
      if (IsRecording)
      {
        StopRecording();
      }
      if (IsTimeShifting)
      {
        StopTimeShifting();
      }
      _timeshiftFileName = "";
      _recordingFileName = "";
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
      _graphState = GraphState.Created;
      _graphRunning = false;
      OnDecompose();
    }
    #endregion

    #region public abstract methods
    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public abstract void OnBeforeTune();

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public abstract void OnAfterTune();

    /// <summary>
    /// Should be called when the graph is about to start
    /// Resets the state 
    /// If graph is already running, starts the pmt grabber to grab the
    /// pmt for the new channel
    /// </summary>
    public abstract void OnGraphStart();

    /// <summary>
    /// Should be called when the graph has been started
    /// sets up the pmt grabber to grab the pmt of the channel
    /// </summary>
    public abstract void OnGraphStarted();

    /// <summary>
    /// Should be called when graph is about to stop.
    /// stops any timeshifting/recording on this channel
    /// </summary>
    public abstract void OnGraphStop();

    /// <summary>
    /// should be called when graph has been stopped
    /// Resets the graph state
    /// </summary>
    public abstract void OnGraphStopped();

    #endregion

    #region protected abstract methods
    /// <summary>
    /// A derrived class should do it's specific cleanup here. It will be called from called from Decompose()
    /// </summary>
    protected abstract void OnDecompose();

    /// <summary>
    /// A derrived class should start here the timeshifting on the tv card. It will be called from StartTimeshifting()
    /// </summary>
    protected abstract bool OnStartTimeShifting(string fileName);

    /// <summary>
    /// A derrived class should stop here the timeshifting on the tv card. It will be called from StopTimeshifting()
    /// </summary>
    protected abstract void OnStopTimeShifting();

    /// <summary>
    /// A derrived class should start here the recording on the tv card. It will be called from StartRecording()
    /// </summary>
    protected abstract void OnStartRecording(bool transportStream, string fileName);

    /// <summary>
    /// A derrived class should stop here the recording on the tv card. It will be called from StopRecording()
    /// </summary>
    protected abstract void OnStopRecording();

    /// <summary>
    /// A derrived class should activate or deactivate the teletext grabbing on the tv card.
    /// </summary>
    protected abstract void OnGrabTeletext();
    #endregion

    #region abstract ITvSubChannel Member

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public abstract bool IsReceivingAudioVideo
    {
      get;
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public abstract int GetCurrentVideoStream
    {
      get;
    }

    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public abstract List<IAudioStream> AvailableAudioStreams
    {
      get;
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public abstract IAudioStream CurrentAudioStream
    {
      get;
      set;
    }

    #endregion
  }
}
