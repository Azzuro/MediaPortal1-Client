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
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Epg;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-S BDA cards
  /// </summary>
  public class TvCardDVBS : TvCardDvbBase, IDisposable, ITVCard
  {

    #region variables
    /// <summary>
    /// holds the DVB-S tuning space
    /// </summary>
    protected IDVBSTuningSpace _tuningSpace = null;
    /// <summary>
    /// holds the current DVB-S tuning request
    /// </summary>
    protected IDVBTuneRequest _tuneRequest = null;

    /// <summary>
    /// Device of the card
    /// </summary>
    DsDevice _device;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:TvCardDVBS"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardDVBS(DsDevice device)
    {
      _device = device;
      _name = device.Name;
      _devicePath = device.DevicePath;

      try
      {
        //        BuildGraph();
        //RunGraph();
        //StopGraph();
      }
      catch (Exception)
      {
      }
    }
    #endregion

    #region graphbuilding
    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          throw new TvException("Graph already built");
        }
        Log.Log.WriteFile("BuildGraph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        // Method names should be self explanatory
        AddNetworkProviderFilter(typeof(DVBSNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerToGraph();
        AddAndConnectBDABoardFilters(_device);
        AddBdaTransportFiltersToGraph();
        GetTunerSignalStatistics();
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw ex;
      }
    }

    /// <summary>
    /// Creates the tuning space.
    /// </summary>
    protected void CreateTuningSpace()
    {
      Log.Log.WriteFile("CreateTuningSpace()");
      ITuner tuner = (ITuner)_filterNetworkProvider;
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      IEnumTuningSpaces enumTuning;
      ITuningSpace[] spaces = new ITuningSpace[2];
      int lowOsc = 9750;
      int hiOsc = 10600;
      int lnbSwitch = 11700;
      if (_parameters.UseDefaultLnbFrequencies)
      {
        lowOsc = 9750;
        hiOsc = 10600;
        lnbSwitch = 11700;
      }
      else
      {
        lowOsc = _parameters.LnbLowFrequency;
        hiOsc = _parameters.LnbHighFrequency;
        lnbSwitch = _parameters.LnbSwitchFrequency;
      }
      ITuneRequest request;
      int fetched;
      container.get_EnumTuningSpaces(out enumTuning);
      while (true)
      {
        enumTuning.Next(1, spaces, out fetched);
        if (fetched != 1) break;
        string name;
        spaces[0].get_UniqueName(out name);
        Log.Log.WriteFile("Found tuningspace {0}", name);
        if (name == "DVBS TuningSpace")
        {
          Log.Log.WriteFile("Found correct tuningspace {0}", name);
          _tuningSpace = (IDVBSTuningSpace)spaces[0];
          _tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
          _tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
          _tuningSpace.put_LowOscillator(lowOsc * 1000);
          _tuningSpace.put_HighOscillator(hiOsc * 1000);
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }
      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("Create new tuningspace");
      _tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
      _tuningSpace.put_UniqueName("DVBS TuningSpace");
      _tuningSpace.put_FriendlyName("DVBS TuningSpace");
      _tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
      _tuningSpace.put_SystemType(DVBSystemType.Satellite);
      _tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
      _tuningSpace.put_LowOscillator(lowOsc * 1000);
      _tuningSpace.put_HighOscillator(hiOsc * 1000);
      IDVBSLocator locator = (IDVBSLocator)new DVBSLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add((ITuningSpace)_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("TuningSpaceContainer", container);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;
    }
    #endregion


    #region tuning & recording
    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;

      if (dvbsChannel == null)
      {
        Log.Log.WriteFile("Channel is not a DVBS channel!!! {0}", channel.GetType().ToString());
        return null;
      }
      DVBSChannel oldChannel = CurrentChannel as DVBSChannel;
      if (CurrentChannel != null)
      {
        //@FIX this fails for back-2-back recordings
        //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
      }
      if (dvbsChannel.SwitchingFrequency < 10)
      {
        dvbsChannel.SwitchingFrequency = 11700000;
      }
      Log.Log.WriteFile("dvbs:  Tune:{0}", channel);
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      //DVB-S2 specific modulation class call here if DVB-S2 card detected
      if (_conditionalAccess != null)
      {
        Log.Log.WriteFile("Set DVB-S2 modulation...");
        _conditionalAccess.SetDVBS2Modulation(_parameters, dvbsChannel);
      }

      //_pmtPid = -1;
      ILocator locator;

      int lowOsc = 9750;
      int hiOsc = 10600;
      int lnbSwitch = 11700;
      BandTypeConverter.GetDefaultLnbSetup(Parameters, dvbsChannel.BandType, out lowOsc, out hiOsc, out lnbSwitch);
      Log.Log.Info("LNB low:{0} hi:{1} switch:{2}", lowOsc, hiOsc, lnbSwitch);
      if (lnbSwitch == 0)
        lnbSwitch = 18000;
      _tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
      _tuningSpace.put_LowOscillator(lowOsc * 1000);
      _tuningSpace.put_HighOscillator(hiOsc * 1000);

      ITuneRequest request;
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;

      _tuningSpace.get_DefaultLocator(out locator);
      IDVBSLocator dvbsLocator = (IDVBSLocator)locator;

      int hr = dvbsLocator.put_Modulation(dvbsChannel.ModulationType);
      Log.Log.WriteFile("Channel modulation is set to {0}", dvbsChannel.ModulationType);
      Log.Log.Info("Put Modulation returned:{0:X}", hr);

      hr = _tuneRequest.put_ONID(dvbsChannel.NetworkId);
      hr = _tuneRequest.put_SID(dvbsChannel.ServiceId);
      hr = _tuneRequest.put_TSID(dvbsChannel.TransportId);
      hr = locator.put_CarrierFrequency((int)dvbsChannel.Frequency);
      hr = dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
      hr = dvbsLocator.put_SignalPolarisation(dvbsChannel.Polarisation);

      hr = dvbsLocator.put_InnerFECRate(dvbsChannel.InnerFecRate);
      Log.Log.WriteFile("Channel FECRate is set to {0}", dvbsChannel.InnerFecRate);
      Log.Log.Info("Put InnerFECRate returned:{0:X}", hr);
      
      _tuneRequest.put_Locator(locator);

      if (_conditionalAccess != null)
      {
        _conditionalAccess.SendDiseqcCommand(_parameters, dvbsChannel);
      }

      ITvSubChannel ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest);

      //move diseqc motor to correct satellite
      if (_conditionalAccess != null)
      {
        if (dvbsChannel.SatelliteIndex > 0 && _conditionalAccess.DiSEqCMotor != null)
        {
          _conditionalAccess.DiSEqCMotor.GotoPosition((byte)dvbsChannel.SatelliteIndex);
        }
      }
      RunGraph(ch.SubChannelId);

      return ch;
    }
    #endregion

    #region quality control
    /// <summary>
    /// Get/Set the quality
    /// </summary>
    /// <value></value>
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
    /// <value></value>
    public bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }
    #endregion


    #region epg & scanning
    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId()) return null;
        return new DVBSScanning(this);
      }
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
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>
    /// true if card can tune to the channel otherwise false
    /// </returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as DVBSChannel) == null) return false;
      return true;
    }

  }
}
