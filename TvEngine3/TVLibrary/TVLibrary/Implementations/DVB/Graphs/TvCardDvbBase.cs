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
using System.Text;
using System.Xml;
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
using TvLibrary.ChannelLinkage;
using TvLibrary.Helper;
using MediaPortal.TV.Epg;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for DVB cards
  /// </summary>
  public class TvCardDvbBase : TvCardBase, IDisposable
  {
    #region constants
    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    class MpTsAnalyzer { }

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    class CyberLinkMuxer { }

    [ComImport, Guid("7F2BBEAF-E11C-4D39-90E8-938FB5A86045")]
    class PowerDirectorMuxer { }

    [ComImport, Guid("3E8868CB-5FE8-402C-AA90-CB1AC6AE3240")]
    class CyberLinkDumpFilter { };
    #endregion

    #region variables
    /// <summary>
    /// DVB Graph variables
    /// </summary>
    protected IFilterGraph2 _graphBuilder = null;
    protected ICaptureGraphBuilder2 _capBuilder = null;
    protected DsROTEntry _rotEntry = null;
    protected IBaseFilter _filterNetworkProvider = null;
    protected IBaseFilter _filterMpeg2DemuxTif = null;
    protected IBaseFilter _infTeeMain = null;
    protected IBaseFilter _infTeeSecond = null;
    protected IBaseFilter _infTeeWinTV = null;
    protected IBaseFilter _filterTuner = null;
    protected IBaseFilter _filterCapture = null;
    protected IBaseFilter _filterTIF = null;
    protected IBaseFilter _filterWinTvUsb = null;    
    protected DsDevice _captureDevice = null;
    protected DsDevice _deviceWinTvUsb = null;
    protected BaseEpgGrabber _epgGrabberCallback = null;
    protected MDPlugs _mdplugs = null;
    protected List<IBDA_SignalStatistics> _tunerStatistics = new List<IBDA_SignalStatistics>();
    protected IBaseFilter _filterTsWriter;
    protected bool _graphRunning = false;
    protected int _managedThreadId = -1;
    protected bool _isATSC = false;
    protected ITsEpgScanner _interfaceEpgGrabber;
    protected ITsChannelScan _interfaceChannelScan;
    protected ITsChannelLinkageScanner _interfaceChannelLinkageScanner;
    protected Hauppauge _hauppauge;
    private TimeShiftingEPGGrabber _timeshiftingEPGGrabber;
    protected bool tunerOnly = false;
    WinTvCiModule winTvCiHandler = null;
    protected bool _cardUsesRealCAM = true;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbBase"/> class.
    /// </summary>
    public TvCardDvbBase(DsDevice device)
      : base(device)
    {            
      _lastSignalUpdate = DateTime.MinValue;
      _mapSubChannels = new Dictionary<int, BaseSubChannel>();
      _parameters = new ScanParameters();
      _timeshiftingEPGGrabber = new TimeShiftingEPGGrabber((ITVCard)this);
      _minChannel = -1;
      _maxChannel = -1;
      _supportsSubChannels = true;     
    }
    #endregion

    #region subchannel management
    /// <summary>
    /// Allocates a new instance of TvDvbChannel which handles the new subchannel
    /// </summary>
    /// <returns>handle for to the subchannel</returns>
    protected int GetNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("dvb:GetNewSubChannel:{0} #{1}", _mapSubChannels.Count, id);

      TvDvbChannel subChannel = new TvDvbChannel(_graphBuilder, ref _conditionalAccess, ref _mdplugs, _filterTIF, _filterTsWriter, id, channel);
      subChannel.Parameters = Parameters;
      subChannel.CurrentChannel = channel;
      _mapSubChannels[id] = subChannel;
      return id;
    }
    #endregion

    #region graph building

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
    }
    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected bool CheckThreadId()
    {
      return true;
    }

    /// <summary>
    /// submits a tune request to the card.
    /// throws an TvException if card cannot tune to the channel requested
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="tuneRequest">tune requests</param>
    /// <returns></returns>
    protected ITvSubChannel SubmitTuneRequest(int subChannelId, IChannel channel, ITuneRequest tuneRequest)
    {
      Log.Log.Info("dvb:Submiting tunerequest Channel:{0} subChannel:{1} ", channel.Name, subChannelId);
      if (_mapSubChannels.ContainsKey(subChannelId) == false)
      {
        Log.Log.Info("dvb:Getting new subchannel");
        subChannelId = GetNewSubChannel(channel);
      } else
      {
      }
      Log.Log.Info("dvb:Submit tunerequest size:{0} new:{1}", _mapSubChannels.Count, subChannelId);
      _mapSubChannels[subChannelId].CurrentChannel = channel;

      _mapSubChannels[subChannelId].OnBeforeTune();

      if (_interfaceEpgGrabber != null)
      {
        _interfaceEpgGrabber.Reset();
      }

      Log.Log.WriteFile("dvb:Submit tunerequest calling put_TuneRequest");
      int hr = 0;
      hr = (_filterNetworkProvider as ITuner).put_TuneRequest(tuneRequest);
      Log.Log.WriteFile("dvb:Submit tunerequest done calling put_TuneRequest");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:SubmitTuneRequest  returns:0x{0:X}", hr);
        throw new TvException("Unable to tune to channel");
      }
      _lastSignalUpdate = DateTime.MinValue;

      _mapSubChannels[subChannelId].OnAfterTune();
      return _mapSubChannels[subChannelId];
    }

    /// <summary>
    /// this method gets the signal statistics interfaces from the bda tuner device
    /// and stores them in _tunerStatistics
    /// </summary>
    protected void GetTunerSignalStatistics()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb: GetTunerSignalStatistics()");
      //no tuner filter? then return;
      _tunerStatistics = new List<IBDA_SignalStatistics>();
      if (_filterTuner == null)
      {
        Log.Log.Error("dvb: could not get IBDA_Topology since no tuner device");
        return;
      }
      //get the IBDA_Topology from the tuner device
      //Log.Log.WriteFile("dvb: get IBDA_Topology");
      IBDA_Topology topology = _filterTuner as IBDA_Topology;
      if (topology == null)
      {
        Log.Log.Error("dvb: could not get IBDA_Topology from tuner");
        return;
      }
      //get the NodeTypes from the topology
      //Log.Log.WriteFile("dvb: GetNodeTypes");
      int nodeTypeCount = 0;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Log.Error("dvb: FAILED could not get node types from tuner:0x{0:X}", hr);
        return;
      }
      if (nodeTypeCount == 0)
      {
        Log.Log.Error("dvb: FAILED could not get any node types");
      }
      Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
      //for each node type
      //Log.Log.WriteFile("dvb: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        int numberOfInterfaces = 32;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out numberOfInterfaces, 32, guidInterfaces);
        if (hr != 0)
        {
          Log.Log.Error("dvb: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}", i, hr);
        }
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          Log.Log.Error("dvb: FAILED could not GetControlNode for node:{0} 0x:{1:X}", i, hr);
          return;
        }

        //and get the final IBDA_SignalStatistics
        for (int iface = 0; iface < numberOfInterfaces; iface++)
        {
          if (guidInterfaces[iface] == GuidIBDA_SignalStatistic)
          {
            //Log.Write(" got IBDA_SignalStatistics on node:{0} interface:{1}", i, iface);
            _tunerStatistics.Add((IBDA_SignalStatistics)objectNode);
          }
        }

      }//for (int i=0; i < nodeTypeCount;++i)
      //hr=Release.ComObject(topology);
      if (_conditionalAccess != null)
      {
        if (_conditionalAccess.AllowedToStopGraph == false)
        {
          RunGraph(-1);
        }
      }
      return;
    }//IBDA_SignalStatistics GetTunerSignalStatistics()

    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    protected void RunGraph(int subChannel)
    {
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStart();
      }
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      if (state == FilterState.Running) return;

      Log.Log.Info("dvb:  RunGraph");
      int hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("dvb:  RunGraph returns: 0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }
      //GetTunerSignalStatistics();
      _epgGrabbing = false;
      _graphRunning = true;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        _mapSubChannels[subChannel].OnGraphStarted();
      }
    }

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    public override void StopGraph()
    {
      if (!CheckThreadId()) return;
      _epgGrabbing = false;
      _isScanning = false;
      FreeAllSubChannels();
      if (_graphBuilder == null) return;
      if (_conditionalAccess.AllowedToStopGraph)
      {
        FilterState state;
        (_graphBuilder as IMediaControl).GetState(10, out state);
        if (state == FilterState.Stopped) return;
        Log.Log.WriteFile("dvb:StopGraph");
        int hr = 0;
        //hr = (_graphBuilder as IMediaControl).StopWhenReady();
        hr = (_graphBuilder as IMediaControl).Stop();
        if (hr < 0 || hr > 1)
        {
          Log.Log.Error("dvb:StopGraph returns:0x{0:X}", hr);
          throw new TvException("Unable to stop graph");
        }
        _conditionalAccess.OnStopGraph();
        _graphRunning = false;
      }
      _graphState = GraphState.Created;
    }

    /// <summary>
    /// This method adds the bda network provider filter to the graph
    /// </summary>
    protected void AddNetworkProviderFilter(Guid networkProviderClsId)
    {
      _isATSC = false;
      _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      Log.Log.WriteFile("dvb:AddNetworkProviderFilter");
      Guid genProviderClsId = new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}");
      // First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
      if (FilterGraphTools.IsThisComObjectInstalled(genProviderClsId))
      {
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, genProviderClsId, "Generic Network Provider");
        Log.Log.WriteFile("dvb:Add Generic Network Provider");
        return;
      }
      // Get the network type of the requested Tuning Space
      if (networkProviderClsId == typeof(DVBTNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBTNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBT Network Provider");
      } else if (networkProviderClsId == typeof(DVBSNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBSNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBS Network Provider");
      } else if (networkProviderClsId == typeof(ATSCNetworkProvider).GUID)
      {
        _isATSC = true;
        Log.Log.WriteFile("dvb:Add ATSCNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "ATSC Network Provider");
      } else if (networkProviderClsId == typeof(DVBCNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBCNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBC Network Provider");
      } else
      {
        Log.Log.Error("dvb:This application doesn't support this Tuning Space");
        // Tuning Space can also describe Analog TV but this application don't support them
        throw new TvException("This application doesn't support this Tuning Space");
      }
    }

    /// <summary>
    /// Checks if the WinTV USB CI module is installed
    /// if so it adds it to the directshow graph
    /// in the following way:
    /// [Network Provider]->[Tuner Filter]->[Capture Filter]->[WinTvCI Filter]->[InfTee]
    /// alternaively like this:
    /// [Network Provider]->[Tuner Filter]->[InfTee]->[WinTvCI Filter]->[InfTee]
    /// </summary>
    /// <param name="captureFilter">The capture filter.</param>
    /// <returns>
    /// true if graph building succeeded, else false
    /// </returns>
    protected bool AddWinTvCIModule(IBaseFilter captureFilter)
    {
      //check if the hauppauge wintv usb CI module is installed
      DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      DsDevice usbWinTvDevice = null;
      int hr = 0;
      //Log.Log.WriteFile("AddWinTvCIModule: capDevices {0}", capDevices.Length);
      for (int capIndex = 0; capIndex < capDevices.Length; capIndex++)
      {
        if (capDevices[capIndex].Name != null)
        {
          //Log.Log.WriteFile("AddWinTvCIModule: {0}", capDevices[capIndex].Name.ToLower());
          if (capDevices[capIndex].Name.ToLower() == "wintvciusbbda source")
          {
            if (false == DevicesInUse.Instance.IsUsed(capDevices[capIndex]))
            {
              usbWinTvDevice = capDevices[capIndex];
              break;
            }
          }
        }
      }
      if (usbWinTvDevice == null)
      {
        Log.Log.Info("dvb:  WinTv CI module not detected.");
        if (tunerOnly == true)
          Log.Log.Info("dvb:  Render [tuner]->[inftee]");
        else
          Log.Log.Info("dvb:  Render [capture]->[inftee]");
        //no wintv ci usb module found. Render [Capture]->[InfTee]
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
        return (hr == 0);
      }
      //wintv ci usb module found
      Log.Log.Info("dvb:  WinTv CI module deteced");
      //add filter to graph
      IBaseFilter tmpCiFilter;
      try
      {
        hr = _graphBuilder.AddSourceFilterForMoniker(usbWinTvDevice.Mon, null, usbWinTvDevice.Name, out tmpCiFilter);
      } catch (Exception)
      {
        Log.Log.Info("dvb:  failed to add WinTv CI filter to graph");
        //cannot add filter to graph...
        //Render [Capture]->[InfTee] or [Tuner]->[InfTee]
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
        return (hr == 0);
      }
      if (hr != 0)
      {
        //cannot add filter to graph...
        Log.Log.Info("dvb:  failed to add WinTv CI filter to graph");
        if (tmpCiFilter != null)
        {
          winTvCiHandler.Shutdown();
          _graphBuilder.RemoveFilter(tmpCiFilter);
          Release.ComObject("WintvUsbCI module", tmpCiFilter);
        }
        //Render [Capture]->[InfTee] or [Tuner]->[InfTee]
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
        return (hr == 0);
      }
      //Check if WinTV CI is plugged in to USB port if not remove it from graph.
      winTvCiHandler = new WinTvCiModule(tmpCiFilter);
      int winTVCIStatus = winTvCiHandler.Init();
      //Log.Log.Info("WinTVCI: Init() returned: {0}", winTVCIStatus);
      if (winTVCIStatus != (int)HResult.Serverity.Success)
      {
        //remove WinTV CI filter & render graph without it.
        winTvCiHandler.Shutdown();
        _graphBuilder.RemoveFilter(tmpCiFilter);
        Release.ComObject("WintvUsbCI module", tmpCiFilter);
        Log.Log.Info("dvb:  WinTv CI not plugged in or driver not installed correctly!");
        //Render [Capture]->[InfTee] or [Tuner]->[InfTee]
        if (tunerOnly == true)
          Log.Log.Info("dvb:  Render [Tuner]->[InfTee]");
        else
          Log.Log.Info("dvb:  Render [Capture]->[InfTee]");
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
        return (hr == 0);
      }
      if (tunerOnly == true)
      {
        //now render [Tuner]->[InfTee]->[WinTv USB]
        //Add InfTee first otherwise the MPEG-2 Demux is used between Tuner & WinTV filter on some cards
        Log.Log.Info("dvb:  Add InfTee for WinTV CI filter");
        _infTeeWinTV = (IBaseFilter)new InfTee();
        hr = _graphBuilder.AddFilter(_infTeeWinTV, "InfTee WinTV");
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Add InfTee WinTV returns:0x{0:X}", hr);
          throw new TvException("Unable to add  _infTeeWinTV");
        }
        //Render Tuner to InfTee first
        Log.Log.Info("dvb:  Render [Tuner]->[InfTee]");
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeWinTV);
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Render [Tuner]->[InfTee] returns:0x{0:X}", hr);
          throw new TvException("Unable to Render Tuner->[InfTee]");
        }
        //Now render InfTee to WinTV CI
        Log.Log.Info("dvb:  Render [InfTee]->[WinTvUSB]");
        hr = _capBuilder.RenderStream(null, null, _infTeeWinTV, null, tmpCiFilter);
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Render [InfTee]->[WinTvUSB] failed");
          winTvCiHandler.Shutdown();
          hr = _graphBuilder.RemoveFilter(tmpCiFilter);
          Release.ComObject("WintvUsbCI module", tmpCiFilter);
          Log.Log.Info("dvb:  Render [Tuner]->[InfTee]");
          hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
          if (hr != 0)
          {
            Log.Log.Error("dvb:  Render [Tuner]->[InfTee] failed");
            return false;
          }
          return (hr == 0);
        }
      } else
      {
        Log.Log.Info("dvb:  Render [Capture]->[WinTvUSB]");
        //Added WinTv USB CI module to the graph
        //now render [Capture]->[WinTv USB]
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, tmpCiFilter);
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Render [Capture]->[WinTvUSB] failed");
          winTvCiHandler.Shutdown();
          hr = _graphBuilder.RemoveFilter(tmpCiFilter);
          Release.ComObject("WintvUsbCI module", tmpCiFilter);
          //Render [Capture]->[InfTee]
          Log.Log.Info("dvb:  Render [Capture]->[InfTee-Main]");
          hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
          if (hr != 0)
          {
            Log.Log.Error("dvb:  Render [Capture]->[InfTee-Main] failed");
            return false;
          }
          return (hr == 0);
        }
      }
      //Finally render [WinTvCi]->[InfTee]
      Log.Log.Info("dvb:  Render [WinTvUSB]->[InfTee-Main]");
      hr = _capBuilder.RenderStream(null, null, tmpCiFilter, null, _infTeeMain);
      if (hr != 0)
      {
        Log.Log.Error("dvb:  Render [WinTvUSB]->[InfTee-Main] failed");
        winTvCiHandler.Shutdown();
        hr = _graphBuilder.RemoveFilter(tmpCiFilter);
        Release.ComObject("WintvUsbCI module", tmpCiFilter);
        if (tunerOnly == true)
          Log.Log.Info("dvb:  Render [Tuner]->[InfTee]");
        else
          Log.Log.Info("dvb:  Render [Capture]->[InfTee]");
        hr = _capBuilder.RenderStream(null, null, captureFilter, null, _infTeeMain);
        if (hr != 0)
        {
          if (tunerOnly == true)
            Log.Log.Error("dvb:  Render [Tuner]->[InfTee] failed");
          else
            Log.Log.Error("dvb:  Render [Capture]->[InfTee] failed");
          return false;
        }
      }
      _filterWinTvUsb = tmpCiFilter;
      _deviceWinTvUsb = usbWinTvDevice;
      DevicesInUse.Instance.Add(usbWinTvDevice);
      return (hr == 0);
    }

    /// <summary>
    /// Finds the correct bda tuner/capture filters and adds them to the graph
    /// Creates a graph like
    /// [NetworkProvider]->[Tuner]->[Capture]->[Inftee]->[Demuxer]
    /// or if no capture filter is present:
    /// [NetworkProvider]->[Tuner]->[Inftee]->[Demuxer]
    /// When a wintv ci module is found the graph will look like:
    /// [NetworkProvider]->[Tuner]->[Capture]->[WinTvCiUSB]->[Inftee]->[Demuxer]
    /// or if no capture filter is present:
    /// [NetworkProvider]->[Tuner]->[InfTee]->[WinTvCiUSB]->[Inftee]->[Demuxer]
    /// </summary>
    /// <param name="device"></param>
    protected void AddAndConnectBDABoardFilters(DsDevice device)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:AddAndConnectBDABoardFilters");
      int hr = 0;
      DsDevice[] devices;
      _rotEntry = new DsROTEntry(_graphBuilder);
      Log.Log.WriteFile("dvb: find bda tuner");
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        Log.Log.WriteFile("dvb:  -{0}", devices[i].Name);
        if (device.DevicePath != devices[i].DevicePath) continue;
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda tuner", tmp);
          }
          continue;
        }
        //render [Network provider]->[Tuner]
        hr = _capBuilder.RenderStream(null, null, _filterNetworkProvider, null, tmp);
        if (hr == 0)
        {
          // Got it !
          _filterTuner = tmp;
          _tunerDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);
          Log.Log.WriteFile("dvb:  Render [Network provider]->[Tuner] OK");
          break;
        } else
        {
          // Try another...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("bda tuner", tmp);
        }
      }
      // Assume we found a tuner filter...
      if (_filterTuner == null)
      {
        Log.Log.Error("dvb:  No TVTuner installed");
        throw new TvException("No TVTuner installed");
      }
      bool skipCaptureFilter = false;
      IPin pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
      if (pinOut != null)
      {
        IEnumMediaTypes enumMedia;
        int fetched;
        pinOut.EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          AMMediaType[] mediaTypes = new AMMediaType[21];
          enumMedia.Next(20, mediaTypes, out fetched);
          if (fetched > 0)
          {
            for (int i = 0; i < fetched; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MediaSubType.Mpeg2Transport && mediaTypes[i].formatType != FormatType.None)
              {
                skipCaptureFilter = false;
              }
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MediaSubType.BdaMpeg2Transport && mediaTypes[i].formatType == FormatType.None)
              {
                skipCaptureFilter = true;
              }
            }
          }
        }
      }
      if (false == skipCaptureFilter)
      {
        Log.Log.WriteFile("dvb:  Find BDA receiver");
        // Then enumerate BDA Receiver Components category to found a filter connecting 
        // to the tuner and the MPEG2 Demux
        devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
        string guidBdaMPEFilter = @"\{8e60217d-a2ee-47f8-b0c5-0f44c55f66dc}";
        string guidBdaSlipDeframerFilter = @"\{03884cb6-e89a-4deb-b69e-8dc621686e6a}";
        for (int i = 0; i < devices.Length; i++)
        {
          if (devices[i].DevicePath.ToLower().IndexOf(guidBdaMPEFilter) >= 0) continue;
          if (devices[i].DevicePath.ToLower().IndexOf(guidBdaSlipDeframerFilter) >= 0) continue;
          IBaseFilter tmp;
          Log.Log.WriteFile("dvb:  -{0}", devices[i].Name);
          if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          } catch (Exception)
          {
            continue;
          }
          if (hr != 0)
          {
            if (tmp != null)
            {
              Log.Log.Error("dvb:  Failed to add bda receiver: {0}. Is it in use?", devices[i].Name);
              _graphBuilder.RemoveFilter(tmp);
              Release.ComObject("bda receiver", tmp);
            }
            continue;
          }
          //render [Tuner]->[Capture]
          hr = _capBuilder.RenderStream(null, null, _filterTuner, null, tmp);
          if (hr == 0)
          {
            Log.Log.WriteFile("dvb:  Render [Tuner]->[Capture] AOK");
            // render [Capture]->[Inf Tee]
            if (AddWinTvCIModule(tmp))
            {
              _filterCapture = tmp;
              _captureDevice = devices[i];
              DevicesInUse.Instance.Add(devices[i]);
              Log.Log.WriteFile("dvb:  OK");
              break;
            } else
            {
              Log.Log.Error("dvb:  Render->main inftee demux failed");
              hr = _graphBuilder.RemoveFilter(tmp);
              Release.ComObject("bda receiver", tmp);
            }
          } else
          {
            // Try another...
            Log.Log.WriteFile("dvb:  Looking for another bda receiver...");
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda receiver", tmp);
          }
        }
      }
      if (_filterCapture == null)
      {
        Log.Log.WriteFile("dvb:  Single BDA filter implementation...");
        // render [Tuner]->[Inf Tee]
        tunerOnly = true;
        if (AddWinTvCIModule(_filterTuner))
        {
          Log.Log.WriteFile("dvb:  OK");
        } else
        {
          Log.Log.Error("dvb:  Render->main inftee demux failed");
          hr = _graphBuilder.RemoveFilter(_filterTuner);
          Release.ComObject("bda receiver", _filterTuner);
          throw new TvException("No Tv Receiver filter found");
        }
        Log.Log.WriteFile("dvb:  using only tv tuner filter...");
        ConnectMpeg2DemuxToInfTee();
        AddTsWriterFilterToGraph();
        _conditionalAccess = new ConditionalAccess(_filterTuner, _filterTsWriter, _filterWinTvUsb, this,_cardUsesRealCAM);
        return;
      }
      ConnectMpeg2DemuxToInfTee();
      AddTsWriterFilterToGraph();
      _conditionalAccess = new ConditionalAccess(_filterTuner, _filterTsWriter, _filterWinTvUsb, this,_cardUsesRealCAM);
    }

    /// <summary>
    /// adds the mpeg-2 demultiplexer filter and inftee filter to the graph
    /// </summary>
    protected void AddMpeg2DemuxerToGraph()
    {
      if (!CheckThreadId()) return;
      if (_filterMpeg2DemuxTif != null) return;
      Log.Log.WriteFile("dvb:Add MPEG2 Demultiplexer filter");
      int hr = 0;
      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTif, "MPEG2-Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerTif returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for tif");
      }
      //multi demux

#if MULTI_DEMUX
      _filterMpeg2DemuxAnalyzer = (IBaseFilter)new MPEG2Demultiplexer();

      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxAnalyzer, "Analyzer MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerDemux returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for analyzer");
      }

      _filterMpeg2DemuxTs = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTs, "Timeshift MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerDemux returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for analyzer");
      }
#endif
      Log.Log.WriteFile("dvb:add Inf Tee filter");
      _infTeeMain = (IBaseFilter)new InfTee();
      hr = _graphBuilder.AddFilter(_infTeeMain, "Inf Tee");
      if (hr != 0)
      {
        Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
    }

    /// <summary>
    /// Connects the mpeg2 demuxers to the inf tee filter.
    /// </summary>
    protected void ConnectMpeg2DemuxToInfTee()
    {
      //multi demux
      int hr;
      DsDevice dv = _captureDevice;
      if (dv == null)
        dv = _tunerDevice; 
      _mdplugs = MDPlugs.Create(dv);
      if (_mdplugs != null)
      {
        _cardUsesRealCAM = true;
        Log.Log.WriteFile("dvb:add 2nd Inf Tee filter");
        _infTeeSecond = (IBaseFilter)new InfTee();
        hr = _graphBuilder.AddFilter(_infTeeSecond, "Inf Tee 2");
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add 2nd InfTee returns:0x{0:X}", hr);
          throw new TvException("Unable to add  _infTeeSecond");
        }
        _mdplugs.Connectmdapifilter(_graphBuilder, ref _infTeeMain, ref _infTeeSecond, ref _filterMpeg2DemuxTif);
      } 
      else
      {
        //connect the [inftee main] -> [TIF MPEG2 Demultiplexer]
        Log.Log.WriteFile("dvb:  Render [inftee]->[demux]");
        IPin mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 0);
        IPin demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
        hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
        Release.ComObject("maintee pin0", mainTeeOut);
        Release.ComObject("tifdemux pinin", demuxPinIn);
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
          throw new TvException("Unable to add  mainInfTee");
        }
      }

#if MULTI_DEMUX
      mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 1);
      demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxAnalyzer, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("analyzer demux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
      mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 2);
      demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTs, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("mpg demux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
#endif
    }

    /// <summary>
    /// Gets the video audio pins.
    /// </summary>
    protected void AddTsWriterFilterToGraph()
    {
      if (_filterTsWriter == null)
      {
        Log.Log.WriteFile("dvb:  Add Mediaportal TsWriter filter");
        _filterTsWriter = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsWriter, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        IBaseFilter tee = _infTeeMain;
        if (_infTeeSecond != null)
          tee = _infTeeSecond;
        IPin pinTee = DsFindPin.ByDirection(tee, PinDirection.Output, 1);
        if (pinTee == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("dvb:  Unable to find pin#2 on inftee filter");
            throw new TvException("unable to find pin#2 on inftee filter");
          }
        }
        IPin pin = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
        if (pin == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("dvb:  Unable to find pin on ts analyzer filter");
            throw new TvException("unable to find pin on ts analyzer filter");
          }
        }
        Log.Log.Info("dvb:  Render [InfTee]->[TsWriter]");
        hr = _graphBuilder.Connect(pinTee, pin);
        Release.ComObject("pinTsWriterIn", pin);
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Unable to connect inftee to analyzer filter :0x{0:X}", hr);
          throw new TvException("unable to connect inftee to analyzer filter");
        }
        _interfaceChannelScan = (ITsChannelScan)_filterTsWriter;
        _interfaceEpgGrabber = (ITsEpgScanner)_filterTsWriter;
        _interfaceChannelLinkageScanner = (ITsChannelLinkageScanner)_filterTsWriter;
      }
    }

    /// <summary>
    /// adds the BDA Transport Information Filter  and the
    /// MPEG-2 sections and tables filter to the graph 
    /// </summary>
    protected void AddBdaTransportFiltersToGraph()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:  AddTransportStreamFiltersToGraph");
      int hr = 0;
      DsDevice[] devices;
      // Add two filters needed in a BDA graph
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        if (String.Compare(devices[i].Name, "BDA MPEG2 Transport Information Filter", true) == 0)
        {
          Log.Log.Write("    add BDA MPEG2 Transport Information Filter filter");
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterTIF);
            if (hr != 0)
            {
              Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter:0x{0:X}", hr);
              return;
            }
          } catch (Exception)
          {
            Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter");
          }
          continue;
        }
      }
      if (_filterTIF == null)
      {
        Log.Log.Error("BDA MPEG2 Transport Information Filter not found");
        return;
      }
      IPin pinInTif = DsFindPin.ByDirection(_filterTIF, PinDirection.Input, 0);
      if (pinInTif == null)
      {
        Log.Log.Error("    unable to find input pin of TIF");
        return;
      }
      if (_filterMpeg2DemuxTif == null)
      {
        Log.Log.Error("   _filterMpeg2DemuxTif==null");
        return;
      }
      //IPin pinInSec = DsFindPin.ByDirection(_filterSectionsAndTables, PinDirection.Input, 0);
      Log.Log.WriteFile("    pinTif:{0}", FilterGraphTools.LogPinInfo(pinInTif));
      //Log.Log.WriteFile("    pinSec:{0}", FilterGraphTools.LogPinInfo(pinInSec));
      //connect tif
      Log.Log.WriteFile("    Connect tif and mpeg2 sections and tables");
      IEnumPins enumPins;
      _filterMpeg2DemuxTif.EnumPins(out enumPins);
      if (enumPins == null)
      {
        Log.Log.Error("   _filterMpeg2DemuxTif.enumpins returned null");
        return;
      }
      bool tifConnected = false;
      //bool mpeg2SectionsConnected = false;
      int pinNr = 0;
      while (true)
      {
        pinNr++;
        PinDirection pinDir;
        AMMediaType[] mediaTypes = new AMMediaType[2];
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1) break;
        if (pins[0] == null) break;
        pins[0].QueryDirection(out pinDir);
        if (pinDir == PinDirection.Input)
        {
          Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
          continue;
        }
        IEnumMediaTypes enumMedia;
        pins[0].EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          enumMedia.Next(1, mediaTypes, out fetched);
          Release.ComObject("IEnumMedia", enumMedia);
          if (fetched == 1 && mediaTypes[0] != null)
          {
            if (mediaTypes[0].majorType == MediaType.Audio || mediaTypes[0].majorType == MediaType.Video)
            {
              //skip audio/video pins
              DsUtils.FreeAMMediaType(mediaTypes[0]);
              Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
              continue;
            }
          }
          DsUtils.FreeAMMediaType(mediaTypes[0]);
        }
        if (tifConnected == false)
        {
          try
          {
            Log.Log.WriteFile("dvb:try tif:{0}", FilterGraphTools.LogPinInfo(pins[0]));
            hr = _graphBuilder.Connect(pins[0], pinInTif);
            if (hr == 0)
            {
              Log.Log.WriteFile("    tif connected");
              tifConnected = true;
              Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
              continue;
            } else
            {
              Log.Log.WriteFile("    tif not connected:0x{0:X}", hr);
            }
          } catch (Exception)
          {
          }
        }
        Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
      }
      Release.ComObject("IEnumMedia", enumPins);
      Release.ComObject("TIF pin in", pinInTif);
      // Release.ComObject("mpeg2 sections&tables pin in", pinInSec);
      if (tifConnected == false)
      {
        Log.Log.Error("    unable to connect transport information filter");
        //throw new TvException("unable to connect transport information filter");
      }
    }

    /// <summary>
    /// Sends the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public virtual void SendHwPids(ArrayList pids)
    {
      //if (System.IO.File.Exists("usehwpids.txt"))
      {
        if (_conditionalAccess != null)
        {
          //  _conditionalAccess.SendPids((DVBBaseChannel)_currentChannel, pids);
        }
        return;
      }
    }
    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      Decompose();
    }
    /// <summary>
    /// destroys the graph and cleans up any resources
    /// </summary>
    protected void Decompose()
    {
      if (_graphBuilder == null) return;
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:Decompose");
      int hr = 0;
      if (_epgGrabbing)
      {
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          Log.Log.Epg("dvb:cancel epg->decompose");
          _epgGrabberCallback.OnEpgCancelled();
        }
        _epgGrabbing = false;
      }

      FreeAllSubChannels();
      _graphRunning = false;
      Log.Log.WriteFile("  stop");
      // Decompose the graph
      hr = (_graphBuilder as IMediaControl).StopWhenReady();
      //hr = (_graphBuilder as IMediaControl).Stop();

      //In case MDPlugs exists then close and release them
      if (_mdplugs != null)
      {
          Log.Log.Info("  Closing MDAPI Plugins");
          _mdplugs.Close();
          _mdplugs = null;
      }

      Log.Log.WriteFile("  remove all filters");
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      Log.Log.WriteFile("  free...");
      _interfaceChannelScan = null;
      _interfaceEpgGrabber = null;
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("_filterMpeg2DemuxTif filter", _filterMpeg2DemuxTif);
        _filterMpeg2DemuxTif = null;
      }
      if (_filterNetworkProvider != null)
      {
        Release.ComObject("_filterNetworkProvider filter", _filterNetworkProvider);
        _filterNetworkProvider = null;
      }
      if (_infTeeMain != null)
      {
        Release.ComObject("main inftee filter", _infTeeMain);
        _infTeeMain = null;
      }
      if (_infTeeSecond != null)
      {
        Release.ComObject("_infTeeSecond filter", _infTeeSecond);
        _infTeeSecond = null;
      }
      if (_infTeeWinTV != null)
      {
        Release.ComObject("_infTeeWinTV filter", _infTeeWinTV);
        _infTeeWinTV = null;
      }
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("TIF MPEG2 demux filter", _filterMpeg2DemuxTif);
        _filterMpeg2DemuxTif = null;
      }
      if (_filterTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTuner) > 0) ;
        _filterTuner = null;
      }
      if (_filterCapture != null)
      {
        while (Marshal.ReleaseComObject(_filterCapture) > 0) ;
        _filterCapture = null;
      }
      if (_filterWinTvUsb != null)
      {
        while (Marshal.ReleaseComObject(_filterWinTvUsb) > 0) ;
        _filterWinTvUsb = null;
      }
      if (_filterTIF != null)
      {
        Release.ComObject("TIF filter", _filterTIF);
        _filterTIF = null;
      }
      //if (_filterSectionsAndTables != null)
      //{
      //  Release.ComObject("secions&tables filter", _filterSectionsAndTables); _filterSectionsAndTables = null;
      //}
      Log.Log.WriteFile("  free pins...");
      if (_filterTsWriter != null)
      {
        Release.ComObject("TSWriter filter", _filterTsWriter);
        _filterTsWriter = null;
      }
      Log.Log.WriteFile("  free graph...");
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
      Log.Log.WriteFile("  free devices...");
      if (_deviceWinTvUsb != null)
      {
        DevicesInUse.Instance.Remove(_deviceWinTvUsb);
        _tunerDevice = null;
      }
      if (_tunerDevice != null)
      {
        DevicesInUse.Instance.Remove(_tunerDevice);
        _tunerDevice = null;
      }
      if (_captureDevice != null)
      {
        DevicesInUse.Instance.Remove(_captureDevice);
        _captureDevice = null;
      }
      if (_tunerStatistics != null)
      {
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = (IBDA_SignalStatistics)_tunerStatistics[i];
          while (Marshal.ReleaseComObject(stat) > 0) ;
        }
        _tunerStatistics.Clear();
      }
      _conditionalAccess = null;
      Log.Log.WriteFile("  decompose done...");
      _graphState = GraphState.Idle;
    }
    #endregion
    #endregion

    #region signal quality, level etc

    /// <summary>
    /// updates the signal quality/level and tuner locked statusses
    /// </summary>
    protected override void UpdateSignalQuality()
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000) return;
      try
      {
        if (_graphRunning == false)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (CurrentChannel == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (_filterNetworkProvider == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (!CheckThreadId())
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        //Log.Log.WriteFile("dvb:UpdateSignalQuality");
        //if we dont have an IBDA_SignalStatistics interface then return
        if (_tunerStatistics == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        if (_tunerStatistics.Count == 0)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        bool isTunerLocked = false;
        bool isSignalPresent = false;
        long signalQuality = 0;
        long signalStrength = 0;

        //       Log.Log.Write("dvb:UpdateSignalQuality() count:{0}", _tunerStatistics.Count);
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = (IBDA_SignalStatistics)_tunerStatistics[i];
          bool isLocked = false;
          bool isPresent = false;
          int quality = 0;
          int strength = 0;
          //          Log.Log.Write("   dvb:  #{0} get locked",i );
          try
          {
            //is the tuner locked?
            stat.get_SignalLocked(out isLocked);
            isTunerLocked |= isLocked;
            //  Log.Log.Write("   dvb:  #{0} isTunerLocked:{1}", i,isLocked);
          } catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          } catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }

          //          Log.Log.Write("   dvb:  #{0} get signalpresent", i);
          try
          {
            //is a signal present?
            stat.get_SignalPresent(out isPresent);
            isSignalPresent |= isPresent;
            //  Log.Log.Write("   dvb:  #{0} isSignalPresent:{1}", i, isPresent);
          } catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalPresent() locked :{0}", ex);
          } catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalPresent() locked :{0}", ex);
          }
          //          Log.Log.Write("   dvb:  #{0} get signalquality", i);
          try
          {
            //is a signal quality ok?
            stat.get_SignalQuality(out quality); //1-100
            if (quality > 0) signalQuality += quality;
            //   Log.Log.Write("   dvb:  #{0} signalQuality:{1}", i, quality);
          } catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          } catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //          Log.Log.Write("   dvb:  #{0} get signalstrength", i);
          try
          {
            //is a signal strength ok?
            stat.get_SignalStrength(out strength); //1-100
            if (strength > 0) signalStrength += strength;
            //    Log.Log.Write("   dvb:  #{0} signalStrength:{1}", i, strength);
          } catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          } catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //Log.Log.WriteFile("  dvb:#{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);
        }
        if (_tunerStatistics.Count > 0)
        {
          _signalQuality = (int)signalQuality / _tunerStatistics.Count;
          _signalLevel = (int)signalStrength / _tunerStatistics.Count;
        }
        if (isTunerLocked)
          _tunerLocked = true;
        else
          _tunerLocked = false;

        if (isTunerLocked)
        {
          _signalPresent = true;
        } else
        {
          _signalPresent = false;
        }
      } finally
      {
        _lastSignalUpdate = DateTime.Now;
      }
    }//public bool SignalPresent()
    #endregion

    #region properties

    /// <summary>
    /// returns the ITsChannelScan interface for the graph
    /// </summary>
    public ITsChannelScan StreamAnalyzer
    {
      get
      {
        return _interfaceChannelScan;
      }
    }

    protected override void UpdateEpgGrabber(bool value)
    {
      if (_epgGrabbing && value == false) _interfaceEpgGrabber.Reset();
    }

    protected override void OnScanning()
    {
      _epgGrabbing = false;
      if (_epgGrabberCallback != null && _epgGrabbing)
      {
        Log.Log.Epg("dvb:cancel epg->scanning");
        _epgGrabberCallback.OnEpgCancelled();
      }
    }
    #endregion

    #region Channel linkage handling

    private bool SameAsPortalChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      return ((pChannel.NetworkId == lChannel.NetworkId) && (pChannel.TransportId == lChannel.NetworkId) && (pChannel.ServiceId == lChannel.ServiceId));
    }

    private bool IsNewLinkedChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      bool bRet = true;
      foreach (LinkedChannel lchan in pChannel.LinkedChannels)
      {
        if ((lchan.NetworkId == lChannel.NetworkId) && (lchan.TransportId == lChannel.TransportId) && (lchan.ServiceId == lChannel.ServiceId))
        {
          bRet = false;
          break;
        }
      }
      return bRet;
    }

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
      if (!CheckThreadId()) return;

      _interfaceChannelLinkageScanner.SetCallBack((IChannelLinkageCallback)callback);
      _interfaceChannelLinkageScanner.Start();
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
      _interfaceChannelLinkageScanner.Reset();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get
      {
        try
        {
          uint channelCount;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          _interfaceChannelLinkageScanner.GetChannelCount(out channelCount);
          if (channelCount == 0)
            return portalChannels;
          for (uint i = 0; i < channelCount; i++)
          {
            ushort network_id = 0; ;
            ushort transport_id = 0; ;
            ushort service_id = 0;
            _interfaceChannelLinkageScanner.GetChannel(i, ref network_id, ref transport_id, ref service_id);
            PortalChannel pChannel = new PortalChannel();
            pChannel.NetworkId = network_id;
            pChannel.TransportId = transport_id;
            pChannel.ServiceId = service_id;
            uint linkCount = 0;
            _interfaceChannelLinkageScanner.GetLinkedChannelsCount(i, out linkCount);
            if (linkCount > 0)
            {
              for (uint j = 0; j < linkCount; j++)
              {
                ushort nid = 0;
                ushort tid = 0;
                ushort sid = 0;
                IntPtr ptrName;
                _interfaceChannelLinkageScanner.GetLinkedChannel(i, j, ref nid, ref tid, ref sid, out ptrName);
                LinkedChannel lChannel = new LinkedChannel();
                lChannel.NetworkId = nid;
                lChannel.TransportId = tid;
                lChannel.ServiceId = sid;
                lChannel.Name = Marshal.PtrToStringAnsi(ptrName);
                if ((!SameAsPortalChannel(pChannel, lChannel)) && (IsNewLinkedChannel(pChannel, lChannel)))
                  pChannel.LinkedChannels.Add(lChannel);
              }
            }
            if (pChannel.LinkedChannels.Count > 0)
              portalChannels.Add(pChannel);
          }
          _interfaceChannelLinkageScanner.Reset();
          return portalChannels;
        } catch (Exception ex)
        {
          Log.Log.Write(ex);
          return new List<PortalChannel>();
        }
      }
    }
    #endregion

    #region epg & scanning

    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public void GrabEpg(BaseEpgGrabber callback)
    {
      if (!CheckThreadId()) return;
      _epgGrabberCallback = callback;
      Log.Log.Write("dvb:grab epg...");
      if (_interfaceEpgGrabber == null)
        return;
      _interfaceEpgGrabber.SetCallBack((IEpgCallback)callback);
      _interfaceEpgGrabber.GrabEPG();
      _interfaceEpgGrabber.GrabMHW();
      _epgGrabbing = true;
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg()
    {
      if (_timeshiftingEPGGrabber.StartGrab())
        GrabEpg(_timeshiftingEPGGrabber);
    }

    /// <summary>
    /// Gets the UTC.
    /// </summary>
    /// <param name="val">The val.</param>
    /// <returns></returns>
    int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public void AbortGrabbing()
    {
      Log.Log.Write("dvb:abort grabbing epg");
      _interfaceEpgGrabber.AbortGrabbing();
      _timeshiftingEPGGrabber.OnEpgCancelled();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<EpgChannel> Epg
    {
      get
      {
        //if (!CheckThreadId()) return null;
        try
        {
          bool dvbReady, mhwReady;
          _interfaceEpgGrabber.IsEPGReady(out dvbReady);
          _interfaceEpgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false) return null;
          uint titleCount;
          uint channelCount = 0;
          _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
          if (titleCount > 10)
            mhwReady = true;
          else
            mhwReady = false;
          _interfaceEpgGrabber.GetEPGChannelCount(out channelCount);
          if (channelCount > 0)
            dvbReady = true;
          else
            dvbReady = false;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          Log.Log.Epg("dvb:mhw ready MHW {0} titles found", titleCount);
          Log.Log.Epg("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              uint id = 0;
              UInt32 programid = 0;
              uint transportid = 0, networkid = 0, channelnr = 0, channelid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              uint tmp1 = 0, tmp2 = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              _interfaceEpgGrabber.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart, ref timestart, out ptrTitle, out ptrProgramName);
              _interfaceEpgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid, out ptrChannelName);
              _interfaceEpgGrabber.GetMHWSummary(programid, out ptrSummary);
              _interfaceEpgGrabber.GetMHWTheme(themeid, out ptrTheme);
              string channelName, title, programName, summary, theme;
              channelName = DvbTextConverter.Convert(ptrChannelName, "");
              title = DvbTextConverter.Convert(ptrTitle, "");
              programName = DvbTextConverter.Convert(ptrProgramName, "");
              summary = DvbTextConverter.Convert(ptrSummary, "");
              theme = DvbTextConverter.Convert(ptrTheme, "");
              if (channelName == null) channelName = "";
              if (title == null) title = "";
              if (programName == null) programName = "";
              if (summary == null) summary = "";
              if (theme == null) theme = "";
              channelName = channelName.Trim();
              title = title.Trim();
              programName = programName.Trim();
              summary = summary.Trim();
              theme = theme.Trim();
              EpgChannel epgChannel = null;
              foreach (EpgChannel chan in epgChannels)
              {
                DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid && dvbChan.ServiceId == channelid)
                {
                  epgChannel = chan;
                  break;
                }
              }
              if (epgChannel == null)
              {
                DVBBaseChannel dvbChan = new DVBBaseChannel();
                dvbChan.NetworkId = (int)networkid;
                dvbChan.TransportId = (int)transportid;
                dvbChan.ServiceId = (int)channelid;
                dvbChan.Name = channelName;
                epgChannel = new EpgChannel();
                epgChannel.Channel = dvbChan;
                epgChannels.Add(epgChannel);
              }
              uint d1 = datestart;
              uint m = timestart & 0xff;
              uint h1 = (timestart >> 16) & 0xff;
              DateTime programStartTime = System.DateTime.Now;
              DateTime dayStart = System.DateTime.Now;
              dayStart = dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
              int day = (int)dayStart.DayOfWeek;
              programStartTime = dayStart;
              int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
              if (minVal < 21600)
                minVal += 604800;
              programStartTime = programStartTime.AddSeconds(minVal);
              EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
              EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme, 0, "", -1);
              program.Text.Add(epgLang);
              epgChannel.Programs.Add(program);
            }
            for (int i = 0; i < epgChannels.Count; ++i)
            {
              epgChannels[i].Sort();
            }
            // free the epg infos in TsWriter so that the mem used gets released 
            _interfaceEpgGrabber.Reset();
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            for (uint x = 0; x < channelCount; ++x)
            {
              _interfaceEpgGrabber.GetEPGChannel((uint)x, ref networkid, ref transportid, ref serviceid);
              EpgChannel epgChannel = new EpgChannel();
              DVBBaseChannel chan = new DVBBaseChannel();
              chan.NetworkId = networkid;
              chan.TransportId = transportid;
              chan.ServiceId = serviceid;
              epgChannel.Channel = chan;
              uint eventCount = 0;
              _interfaceEpgGrabber.GetEPGEventCount((uint)x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD = 0, start_time_UTC = 0, duration = 0, languageId = 0, languageCount = 0;
                string title, description, genre, classification;
                IntPtr ptrTitle = IntPtr.Zero;
                IntPtr ptrDesc = IntPtr.Zero;
                IntPtr ptrGenre = IntPtr.Zero;
                int starRating;
                IntPtr ptrClassification = IntPtr.Zero;
                int parentalRating;

                _interfaceEpgGrabber.GetEPGEvent((uint)x, (uint)i, out languageCount, out start_time_MJD, out start_time_UTC, out duration, out ptrGenre, out starRating, out ptrClassification);
                genre = DvbTextConverter.Convert(ptrGenre, "");
                classification = DvbTextConverter.Convert(ptrClassification, "");

                if (starRating < 1 || starRating > 7)
                  starRating = 0;

                int duration_hh = getUTC((int)((duration >> 16)) & 255);
                int duration_mm = getUTC((int)((duration >> 8)) & 255);
                int duration_ss = 0;//getUTC((int) (duration )& 255);
                int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
                int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
                int starttime_ss = 0;//getUTC((int) (start_time_UTC )& 255);

                if (starttime_hh > 23) starttime_hh = 23;
                if (starttime_mm > 59) starttime_mm = 59;
                if (starttime_ss > 59) starttime_ss = 59;

                // DON'T ENABLE THIS. Some entries can be indeed >23 Hours !!!
                //if (duration_hh > 23) duration_hh = 23;
                if (duration_mm > 59) duration_mm = 59;
                if (duration_ss > 59) duration_ss = 59;

                // convert the julian date
                int year = (int)((start_time_MJD - 15078.2) / 365.25);
                int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
                int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
                int k = (month == 14 || month == 15) ? 1 : 0;
                year += 1900 + k; // start from year 1900, so add that here
                month = month - 1 - k * 12;
                int starttime_y = year;
                int starttime_m = month;
                int starttime_d = day;
                if (year < 2000) continue;

                try
                {
                  DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm, starttime_ss, 0);
                  DateTime dtStart = dtUTC.ToLocalTime();
                  if (dtStart < DateTime.Now.AddDays(-1) || dtStart > DateTime.Now.AddMonths(2))
                    continue;
                  DateTime dtEnd = dtStart.AddHours(duration_hh);
                  dtEnd = dtEnd.AddMinutes(duration_mm);
                  dtEnd = dtEnd.AddSeconds(duration_ss);
                  EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                  //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                  for (int z = 0; z < languageCount; ++z)
                  {
                    _interfaceEpgGrabber.GetEPGLanguage((uint)x, (uint)i, (uint)z, out languageId, out ptrTitle, out ptrDesc, out parentalRating);
                    //title = DvbTextConverter.Convert(ptrTitle,"");
                    //description = DvbTextConverter.Convert(ptrDesc,"");
                    string language = String.Empty;
                    language += (char)((languageId >> 16) & 0xff);
                    language += (char)((languageId >> 8) & 0xff);
                    language += (char)((languageId) & 0xff);
                    //allows czech epg
                    if (language.ToLower() == "cze" || language.ToLower() == "ces")
                    {
                      title = Iso6937ToUnicode.Convert(ptrTitle);
                      description = Iso6937ToUnicode.Convert(ptrDesc);
                    } else
                    {
                      title = DvbTextConverter.Convert(ptrTitle, "");
                      description = DvbTextConverter.Convert(ptrDesc, "");
                    }
                    if (title == null) title = "";
                    if (description == null) description = "";
                    if (language == null) language = "";
                    if (genre == null) genre = "";
                    if (classification == null) classification = "";
                    title = title.Trim();
                    description = description.Trim();
                    language = language.Trim();
                    genre = genre.Trim();
                    EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre, starRating, classification, parentalRating);
                    epgProgram.Text.Add(epgLangague);
                  }
                  epgChannel.Programs.Add(epgProgram);
                } catch (Exception ex)
                {
                  Log.Log.Write(ex);
                }
              }//for (uint i = 0; i < eventCount; ++i)
              if (epgChannel.Programs.Count > 0)
              {
                epgChannel.Sort();
                epgChannels.Add(epgChannel);
              }
            }//for (uint x = 0; x < channelCount; ++x)
          }
          // free the epg infos in TsWriter so that the mem used gets released 
          _interfaceEpgGrabber.Reset();
          return epgChannels;
        } catch (Exception ex)
        {
          Log.Log.Write(ex);
          return new List<EpgChannel>();
        }
      }
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

    /// <summary>
    /// Reloads the quality control configuration
    /// </summary>
    public void ReloadQualityControlConfiguration()
    {
    }
    #endregion

    Int32 OnWinTvCiCamInfo(IntPtr Context, byte appType, ushort appManuf, ushort manufCode, StringBuilder Info)
    {
      Log.Log.Info("OnWinTvCiCamInfo: appType={0} appManuf={1} manufCode={2} info={3}", appType, appManuf, manufCode, Info.ToString());
      return 0;
    }
  }
}
