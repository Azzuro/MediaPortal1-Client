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
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Epg;
using TvLibrary.Teletext;
using DirectShowLib.SBE;
namespace TvLibrary.Interfaces
{
  /// <summary>
  /// cam types
  /// </summary>
  public enum CamType:int
  {
    /// <summary>
    /// Default
    /// </summary>
    Default = 0,
    /// <summary>
    /// Viacess cam
    /// </summary>
    Viaccess=1,
    /// <summary>
    /// Aston cam
    /// </summary>
    Aston=2,
    /// <summary>
    /// Conax cam
    /// </summary>
    Conax=3,
    /// <summary>
    /// Cryptoworks cam
    /// </summary>
    CryptoWorks=4

  }
  /// <summary>
  /// interface for a tv card
  /// </summary>
  public interface ITVCard
  {
    #region properties
    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    string Name { get;set;}

    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    string DevicePath { get;}

    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    string TimeShiftFileName { get;}

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    DateTime StartOfTimeShift { get;}

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    DateTime RecordingStarted { get;}


    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    bool CanTune(IChannel channel);

    /// <summary>
    /// Stops the current graph
    /// </summary>
    /// <returns></returns>
    void StopGraph();

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    bool IsReceivingAudioVideo { get;}

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    string FileName { get;}

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    bool IsRecording { get;}

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    bool IsTimeShifting { get;}

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    bool IsEpgGrabbing { get;set;}

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    bool IsScanning { get;set;}

    /// <summary>
    /// returns the IChannel to which the card is currently tuned
    /// </summary>
    IChannel Channel { get;}

    /// <summary>
    /// returns the min. channel number for analog cards
    /// </summary>
    int MinChannel { get;}

    /// <summary>
    /// returns the max. channel number for analog cards
    /// </summary>
    /// <value>The max channel.</value>
    int MaxChannel { get;}

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    bool IsTimeshiftingTransportStream { get;}
      
    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    bool IsRecordingTransportStream { get;}

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    CamType CamType {get;set;}
    /// <summary>
    /// Gets/sets the card type
    /// </summary>
    int cardType { get;}

    /// <summary>
    /// Gets the interface for controlling the diseqc motor
    /// </summary>
    /// <value>Theinterface for controlling the diseqc motor.</value>
    IDiSEqCMotor DiSEqCMotor { get;}

    #endregion

    #region epg & scanning
    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="callback">The callback which gets called when epg is received or canceled.</param>
    void GrabEpg(BaseEpgGrabber callback);

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    List<EpgChannel> Epg { get;}

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    ITVScanning ScanningInterface { get;}


    #endregion

    #region teletext
    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    bool GrabTeletext { get;set;}

    /// <summary>
    /// returns the ITeletext interface used for retrieving the teletext pages
    /// </summary>
    ITeletext TeletextDecoder { get;}

    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    bool HasTeletext { get;}
    #endregion

    #region tuning & recording
    /// <summary>
    /// tune the card to the channel specified by IChannel
    /// </summary>
    /// <param name="channel">channel to tune</param>
    /// <returns>true if succeeded else false</returns>
    bool TuneScan(IChannel channel);

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    bool Tune(IChannel channel);

    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns>true if succeeded else false</returns>
    bool StartTimeShifting(string fileName);

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    bool StopTimeShifting();

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">unused</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">unused</param>
    /// <returns>true if succeeded else false</returns>
    bool StartRecording(RecordingType recordingType,  string fileName, long startTime);

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    bool StopRecording();
    #endregion

    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    List<IAudioStream> AvailableAudioStreams { get;}

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    IAudioStream CurrentAudioStream { get;set;}
    #endregion

    #region quality control
    /// <summary>
    /// Get/Set the quality
    /// </summary>
    IQuality Quality { get;set;}

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    bool SupportsQualityControl { get;}
    #endregion

    #region properties
    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    bool IsTunerLocked { get;}

    /// <summary>
    /// returns the signal quality
    /// </summary>
    int SignalQuality { get;}

    /// <summary>
    /// returns the signal level
    /// </summary>
    int SignalLevel { get;}

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    void ResetSignalUpdate();

    #endregion

    #region idisposable
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    void Dispose();
    #endregion
  }
}
