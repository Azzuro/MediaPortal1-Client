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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Text;
using System.Drawing;
using System.Reflection;
using DirectShowLib;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Teletext;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.Helper;
using TvLibrary.ChannelLinkage;

namespace TvLibrary.Implementations.RadioWebStream
{
  /// <summary>
  /// Dummy card for radio web streams
  /// Timeshifting is not supported, the stream is played back on the client
  /// Recording is supported
  /// </summary>
  public class RadioWebStreamCard : ITVCard
  {
    #region enums
    /// <summary>
    /// Different states of the card
    /// </summary>
    protected enum GraphState
    {
      /// <summary>
      /// Card is idle
      /// </summary>
      Idle,
      /// <summary>
      /// Card is idle, but graph is created
      /// </summary>
      Created,
      /// <summary>
      /// Card is timeshifting
      /// </summary>
      TimeShifting,
      /// <summary>
      /// Card is recording
      /// </summary>
      Recording
    }
    #endregion

    #region variables
    protected string _name;
    protected string _recordingFileName = "";
    protected int _managedThreadId;
    protected bool _isScanning = false;
    protected DateTime _dateRecordingStarted = DateTime.MinValue;
    protected object m_context = null;
    protected IChannel _currentChannel;
    protected ScanParameters _parameters;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioWebStreamCard"/> class.
    /// </summary>
    public RadioWebStreamCard()
    {
      _name = "RadioWebStream Card (builtin)";
      _parameters = new ScanParameters();
    }
    #endregion

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

    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected bool CheckThreadId()
    {
      return true;
      if (_managedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
      {
        Log.Log.WriteFile("RadioWebStream:Invalid thread id!!!");
        return false;
      }
      return true;
    }

    #region recording
    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    protected void StartRecord(bool transportStream, string fileName)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("RadioWebStream:StartRecord({0})", fileName);
      Log.Log.WriteFile("RadioWebStream:Recording currently not implemented");
      _dateRecordingStarted = DateTime.Now;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected void StopRecord()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("RadioWebStream:StopRecord()");
      _recordingFileName = "";
    }
    #endregion

    #region Channel linkage handling
    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
    }
    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
    }
    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get
      {
        return null;
      }
    }
    #endregion

    #region epg & scanning
    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        return null;
      }
    }
    /// <summary>
    /// returns the ITVEPG interface used for grabbing the epg
    /// </summary>
    public ITVEPG EpgInterface
    {
      get
      {
        return null;
      }
    }
    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public void AbortGrabbing()
    {
      return;
    }
    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<EpgChannel> Epg
    {
      get
      {
        return null;
      }
    }
    #endregion

    #region tuning & recording
    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if (!channel.IsRadio) return false;
      if ((channel as RadioWebStreamChannel) == null) return false;
      return true;
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("RadioWebStream:  Tune:{0}", channel);
      return null;
    }
    #endregion

    #region quality control
    /// <summary>
    /// Get/Set the quality
    /// </summary>
    public IQuality Quality
    {
      get
      {
        return null;
      }
      set
      {
      }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    public bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        return 0;
      }
    }
    /// <summary>
    /// Gets a value indicating whether card supports subchannels
    /// </summary>
    /// <value><c>true</c> if card supports sub channels; otherwise, <c>false</c>.</value>
    public bool SupportsSubChannels
    {
      get
      {
        return true;
      }
    }
    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannel(int id)
    {
    }
    /// <summary>
    /// Gets the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      return null;
    }
    /// <summary>
    /// Gets the sub channels.
    /// </summary>
    /// <value>The sub channels.</value>
    public ITvSubChannel[] SubChannels
    {
      get
      {
        ITvSubChannel[] channels = new ITvSubChannel[1];
        channels[0] = null;
        return channels;
      }
    }

    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
      }
    }
    /// <summary>
    /// Gets/sets the card cardType
    /// </summary>
    public int cardType
    {
      get
      {
        return 5; 
      }
      set
      {
      }
    }
    public void StopGraph()
    {
      if (!CheckThreadId()) return;
    }
    public bool IsHybrid
    {
      get
      {
        return false;
      }
      set
      {
      }
    }
    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    public bool IsTunerLocked
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// returns the signal quality
    /// </summary>
    public int SignalQuality
    {
      get
      {
        if (!CheckThreadId()) return 0;
        if (IsTunerLocked)
          return 100;
        else
          return 0;
      }
    }

    /// <summary>
    /// returns the signal level
    /// </summary>
    public int SignalLevel
    {
      get
      {
        if (!CheckThreadId()) return 0;
        if (IsTunerLocked)
          return 100;
        else
          return 0;
      }
    }
    #endregion

    #region IDisposable Members
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    virtual public void Dispose()
    {
      Log.Log.WriteFile("RadioWebStream:Dispose()");
      if (!CheckThreadId()) return;
    }
    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }
    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public string DevicePath
    {
      get
      {
        return "(builtin)";
      }
    }
    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return "";
      }
    }

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    public bool IsEpgGrabbing
    {
      get
      {
        return false;
      }
      set
      {
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
        return false;
      }
    }

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    public bool IsScanning
    {
      get
      {
        return false;
      }
      set
      {
      }
    }
    /// <summary>
    /// returns the max. channel numbers for RadioWebStreamCards
    /// </summary>
    public int MaxChannel
    {
      get
      {
        return 99999;
      }
    }
    /// <summary>
    /// returns the min. channel numbers for RadioWebStreamCards
    /// </summary>
    /// <value>The min channel.</value>
    public int MinChannel
    {
      get
      {
        return 1;
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
        return DateTime.MaxValue;
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

    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        List<IAudioStream> streams = new List<IAudioStream>();
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.Stereo;
        stream.Language = "Stereo";
        streams.Add(stream);
        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get
      {
        return AvailableAudioStreams[0];
      }
      set
      {
      }
    }
    #endregion

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public bool IsReceivingAudioVideo
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return CamType.Default;
      }
      set
      {
      }
    }

    public object Context
    {
      get
      {
        return m_context;
      }
      set
      {
        m_context = value;
      }
    }

    public void GrabEpg(BaseEpgGrabber callback)
    {
    }
    public void GrabEpg()
    {
    }
    /// <summary>
    /// Gets the interface for controlling the diseqc motor
    /// </summary>
    /// <value>Theinterface for controlling the diseqc motor.</value>
    public IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return null;
      }
    }
    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    public void ResetSignalUpdate()
    {
    }
  }
}
