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
#if (UseCaptureCardDefinitions)
/*
 * This is a modified version of the SinkGraph class.
 * It supports MPEG2 hardware cards using a definition file.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using System.Xml;
using TVCapture;
using System.Windows.Forms;
//using DirectX.Capture;
using MediaPortal.TV.Teletext;


namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Implementation of IGraph for all hardware MPEG2 encoder cards (pretentions... ;-) )
  /// As there are the Hauppauge PVR 150MCE, 250, 350 and the WinFast PVR 2000
  /// 
  /// The graphbuilder can build the graph for the following functions:
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// </summary>	
  public class SinkGraphEx : MediaPortal.TV.Recording.SinkGraph, ISampleGrabberCB
  {
    #region imports
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern bool AddTeeSinkToGraph(IGraphBuilder graph);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern bool AddTeeSinkNameToGraph(IGraphBuilder graph, string name);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void AddWstCodecToGraph(IGraphBuilder graph);
    #endregion

    #region variables
    IBaseFilter _filterSampleGrabber = null;
    ISampleGrabber _sampleGrabberInterface = null;
    #endregion

    #region Constructors

    /// <summary>
    /// Constructor for the graph  for given capture card.
    /// </summary>
    /// <param name="pCard"></param>
    public SinkGraphEx(TVCaptureDevice pCard)
      : base(pCard)
    {
    }
    #endregion

    #region Overrides

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public override bool CreateGraph(int Quality)
    {
      try
      {
        _lastError = "";
        _hasTeletext = false;
        _grabTeletext = false;
        _vmr9 = new VMR9Util();

        _log.Info("SinkGraphEx:CreateGraph() IN");
        if (_graphState != State.None)
        {
          _lastError = "Graph already created";
          return false;		// If doing something already, return...
        }
        if (_card == null)
        {
          _lastError = "No card specified";
          _log.Error("SinkGraphEx:card is not defined");
          return false;
        }

        if ((_card.Graph == null) || (_card.Graph.TvFilterDefinitions == null))
        {
          _lastError = "Card definition does not contain any filters";
          _log.Error("SinkGraphEx:card does not contain filters?");
          return false;
        }
        if ((_card.Graph == null) || (_card.Graph.TvConnectionDefinitions == null))
        {
          _lastError = "Card definition does not contain any tv filters";
          _log.Error("SinkGraphEx:card does not contain connections for tv?");
          return false;
        }

        GUIGraphicsContext.OnGammaContrastBrightnessChanged += new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);


        // Initialize settings. No channel tuned yet...
        _previousChannel = -1;

        int hr = 0;

        // Make a new filter graph
        _log.Info("SinkGraphEx: Create new filter graph (IGraphBuilder)");
        _graphBuilderInterface = (IGraphBuilder)new FilterGraph();

        // Get the Capture Graph Builder...
        _log.Info("SinkGraphEx: Get the Capture Graph Builder (ICaptureGraphBuilder2)");
        _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

        // ...and link the Capture Graph Builder to the Graph Builder
        _log.Info("SinkGraphEx: Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilderInterface);
        if (hr < 0)
        {
          _lastError = "Unable to create graph";
          _log.Info("SinkGraphEx: Error: link FAILED");
          _log.Info("SinkGraphEx:CreateGraph() OUT");
          return false;
        }
        // Add graph to Running Object Table (ROT), so we can connect to the graph using GraphEdit ;)
        _log.Info("SinkGraphEx: Add graph to ROT table");
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilderInterface);

        // Loop through configured filters for this card, bind them and add them to the graph
        // Note that while adding filters to a graph, some connections may already be created...
        _log.Info("SinkGraphEx: Adding configured filters...");
        foreach (FilterDefinition dsFilter in _card.Graph.TvFilterDefinitions)
        {
          string catName = dsFilter.Category;
          dsFilter.DSFilter = GetFilter(dsFilter.FriendlyName);
          if (dsFilter.DSFilter == null)
          {
            dsFilter.DSFilter = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
          }
          //FilterDefinition dsFilter = _card.TvFilterDefinitions[catName] as FilterDefinition;
          _log.Info("SinkGraphEx:  adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
          hr = _graphBuilderInterface.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
          if (hr == 0)
          {
            _log.Info("SinkGraphEx:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
          }
          else
          {

            _log.Info("SinkGraphEx:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
            _log.Info("SinkGraphEx:  Error! Result code = {0}", hr);
          }

          // Support the "legacy" member variables. This could be done different using properties
          // through which the filters are accessable. More implementation independent...
          if (dsFilter.Category == "tvtuner") _tvTunerInterface = dsFilter.DSFilter as IAMTVTuner;
          if (dsFilter.Category == "tvaudio") _tvAudioTunerInterface = dsFilter.DSFilter as IAMTVAudio;
          if (dsFilter.Category == "capture") _filterCapture = dsFilter.DSFilter;
        }
        _log.Info("SinkGraphEx: Adding configured filters...DONE");

        _analogVideoDecoderInterface = _filterCapture as IAMAnalogVideoDecoder;
        InitializeTuner();
        // All filters, up-to and including the encoder filter have been added using a configuration file.
        // The rest of the filters depends on the fact if we are just viewing TV, or Timeshifting or even
        // recording. This part is however card independent and controlled by software, although this part
        // could also be configured using a definition file. If used, this could lead to the possibility
        // of having building blocks enabling the support of about every card, or combination of cards, ie
        // even (again, pretentions...) the Sigma Designs XCard could be "coupled" to the capture card...

        // Set crossbar routing, default to Tv Tuner + Audio Tuner...
        //	DsUtils.FixCrossbarRouting(_graphBuilderInterface, _captureGraphBuilderInterface, _filterCapture, true, false, false, false,_cardName);

        FilterDefinition sourceFilter;
        FilterDefinition sinkFilter;
        IPin sourcePin = null;
        IPin sinkPin = null;

        // Create pin connections. These connections are also specified in the definitions file.
        // Note that some connections might fail due to the fact that the connection is already made,
        // probably during the addition of filters to the graph (checked with GraphEdit...)
        //
        // Pin connections can be defined in two ways:
        // 1. Using the name of the pin.
        //		This method does work, but might be language dependent, meaning the connection attempt
        //		will fail because the pin cannot be found...
        // 2.	Using the 0-based index number of the input or output pin.
        //		This method is save. It simply tells to connect output pin #0 to input pin #1 for example.
        //
        // The code assumes method 1 is used. If that fails, method 2 is tried...

        _log.Info("SinkGraphEx: Adding configured pin connections...");
        for (int i = 0; i < _card.Graph.TvConnectionDefinitions.Count; i++)
        {
          sourceFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourceCategory);
          sinkFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkCategory);
          if (sourceFilter == null)
          {
            _log.Error("SinkGraphEx: Cannot find source filter for connection:{0}", i);
            continue;
          }
          if (sinkFilter == null)
          {
            _log.Error("SinkGraphEx: Cannot find sink filter for connection:{0}", i);
            continue;
          }
          _log.Info("SinkGraphEx:  Connecting <{0}>:{1} with <{2}>:{3}",
            sourceFilter.FriendlyName, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName,
            sinkFilter.FriendlyName, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);


          //sourceFilter.DSFilter.FindPin(((ConnectionDefinition)_card.ConnectionDefinitions[i]).SourcePinName, out sourcePin);
          sourcePin = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName);
          if (sourcePin == null)
          {
            String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName;
            if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
            {
              sourcePin = DsFindPin.ByDirection(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
              if (sourcePin == null)
                _log.Info("SinkGraphEx:   Unable to find sourcePin: <{0}>", strPinName);
              else
                _log.Info("SinkGraphEx:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
            }
          }
          else
            _log.Info("SinkGraphEx:   Found sourcePin: <{0}> ", ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName);

          sinkPin = GetPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);
          /*
           * //sinkFilter.DSFilter.FindPin(((ConnectionDefinition)_card.ConnectionDefinitions[i]).SinkPinName, out sinkPin);
          sinkPin = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)_card.TvConnectionDefinitions[i]).SinkPinName);
          if (sinkPin == null)
          {
            String strPinName = ((ConnectionDefinition)_card.TvConnectionDefinitions[i]).SinkPinName;
            if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
            {
              sinkPin = DsFindPin.ByDirection(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
              if (sinkPin == null)
                _log.Info("SinkGraphEx:   Unable to find sinkPin: <{0}>", strPinName);
              else
                _log.Info("SinkGraphEx:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
            }
          }
          else
            _log.Info("SinkGraphEx:   Found sinkPin: <{0}> ", ((ConnectionDefinition)_card.TvConnectionDefinitions[i]).SinkPinName);
          */
          if (sourcePin != null && sinkPin != null)
          {
            IPin conPin;
            hr = sourcePin.ConnectedTo(out conPin);
            if (hr != 0)
              hr = _graphBuilderInterface.Connect(sourcePin, sinkPin);
            if (hr == 0)
              _log.Info("SinkGraphEx:   Pins connected...");

            // Give warning and release pin...
            if (conPin != null)
            {
              _log.Info("SinkGraphEx:   (Pin was already connected...)");
              Marshal.ReleaseComObject(conPin as Object);
              conPin = null;
              hr = 0;
            }
          }

          if (hr != 0)
          {
            _log.Info("SinkGraphEx:  Error: Unable to connect Pins 0x{0:X}", hr);
            if (hr == -2147220969)
            {
              _log.Info("SinkGraphEx:   -- Cannot connect: {0} or {1}", sourcePin.ToString(), sinkPin.ToString());
            }

            if (sourcePin != null) Marshal.ReleaseComObject(sourcePin); sourcePin = null;
            if (sinkPin != null) Marshal.ReleaseComObject(sinkPin); sinkPin = null;
            if (sourceFilter.DSFilter != null)
            {
              _log.Info("SinkGraphEx: remove {0}", sourceFilter.FriendlyName);
              _graphBuilderInterface.RemoveFilter(sourceFilter.DSFilter);
              Marshal.ReleaseComObject(sourceFilter.DSFilter); sourceFilter.DSFilter = null;
            }
            RetryOtherInstances(i);
            _log.Info("SinkGraphEx: RetryOtherInstances done");
          }//if (hr != 0)
        }//for (int i = 0; i < _card.TvConnectionDefinitions.Count; i++)

        if (sinkPin != null)
          Marshal.ReleaseComObject(sinkPin);
        sinkPin = null;
        if (sourcePin != null)
          Marshal.ReleaseComObject(sourcePin);
        sourcePin = null;
        _log.Info("SinkGraphEx: Adding configured pin connections...DONE");

        // Find out which filter & pin is used as the interface to the rest of the graph.
        // The configuration defines the filter, including the Video, Audio and Mpeg2 pins where applicable
        // We only use the filter, as the software will find the correct pin for now...
        // This should be changed in the future, to allow custom graph endings (mux/no mux) using the
        // video and audio pins to connect to the rest of the graph (SBE, overlay etc.)
        // This might be needed by the ATI AIW cards (waiting for ob2 to release...)
        FilterDefinition lastFilter = _card.GetTvFilterDefinition(_card.Graph.TvInterfaceDefinition.FilterCategory);




        // All filters and connections have been made.
        // Now fix the rest of the graph, add MUX etc.
        _videoCaptureHelper = new VideoCaptureDevice(_graphBuilderInterface, _captureGraphBuilderInterface, _filterCapture, lastFilter.DSFilter);

        _videoCaptureHelper.SetFrameRate(25.0d);
        if (!SetFrameSize(720, 576))
        {
          if (!SetFrameSize(720, 480))
          {
            if (!SetFrameSize(768, 576))
            {
              if (!SetFrameSize(640, 480))
              {
                if (!SetFrameSize(320, 240))
                {
                  //???
                }
              }
            }
          }
        }
        _sizeFrame = _videoCaptureHelper.GetFrameSize();


        _log.Info("SinkGraphEx: Capturing:{0}x{1}", _sizeFrame.Width, _sizeFrame.Height);
        _mpeg2DemuxHelper = null;

        // creates the last part of the graph. Depending on timeshifting etc.
        // it will eventually connect to the lastFilter, so this object only does the last part of the graph
        _mpeg2DemuxHelper = new MPEG2Demux(ref _graphBuilderInterface, _sizeFrame);

        // Connect video capture->mpeg2 demuxer
        ConnectVideoCaptureToMPEG2Demuxer();
        _mpeg2DemuxHelper.CreateMappings();

        _videoProcAmpHelper = new VideoProcAmp(_filterCapture as IAMVideoProcAmp);
        if (_videoProcAmpHelper != null)
        {
          IBaseFilter capture;
          _graphBuilderInterface.FindFilterByName("NVTV", out capture);
          if (capture != null)
          {
            _log.Info("SinkGraphEx: NVTV capture filter detected");
            _videoProcAmpHelper.Contrast = 70;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Contrast {0}", _videoProcAmpHelper.Contrast);
            _videoProcAmpHelper.Saturation = 68;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Saturation {0}", _videoProcAmpHelper.Saturation);
            _videoProcAmpHelper.Brightness = _videoProcAmpHelper.BrightnessDefault;
            _videoProcAmpHelper.Gamma = _videoProcAmpHelper.GammaDefault;
            _videoProcAmpHelper.Sharpness = _videoProcAmpHelper.SharpnessDefault;
          }
          if (capture == null)
          {
            _log.Info("SinkGraphEx: VideoProcAmp setting defaults");
            _videoProcAmpHelper.Contrast = _videoProcAmpHelper.ContrastDefault;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Contrast {0}", _videoProcAmpHelper.Contrast);
            _videoProcAmpHelper.Brightness = _videoProcAmpHelper.BrightnessDefault;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Brightness {0}", _videoProcAmpHelper.Brightness);
            _videoProcAmpHelper.Gamma = _videoProcAmpHelper.GammaDefault;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Gamma {0}", _videoProcAmpHelper.Gamma);
            _videoProcAmpHelper.Saturation = _videoProcAmpHelper.SaturationDefault;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Saturation {0}", _videoProcAmpHelper.Saturation);
            _videoProcAmpHelper.Sharpness = _videoProcAmpHelper.SharpnessDefault;
            //_log.Info("SinkGraphEx: _videoProcAmpHelper.Sharpness {0}", _videoProcAmpHelper.Sharpness);
          }
        }

        _graphState = State.Created;
        _log.Info("SinkGraphEx:CreateGraph() OUT");

        SetQuality(3);

        SetupTeletext();
        return true;
      }
      catch (Exception ex)
      {
        _lastError = String.Format("Unable to create graph");
        _log.Error(ex);
        return false;
      }
    }

    void SetupTeletext()
    {
      //
      //	[							 ]		 [  tee/sink	]			 [	wst			]			[ sample	]
      //	[	capture			 ]		 [		to			]----->[	codec		]---->[ grabber	]
      //	[						vbi]---->[	sink			]			 [					]			[					]
      //
      //if (GUIGraphicsContext.DX9Device == null) return;MOSQUISS


      AddTeeSinkToGraph(_graphBuilderInterface); //Tee/Sink-to-Sink Converter
      IBaseFilter teesink = DirectShowUtil.GetFilterByName(_graphBuilderInterface, "Kernel Tee");
      if (teesink == null)
      {
        _log.Info("SinkGraphEx.SetupTeletext(): Trying to find Kernel Tee...");
        DsDevice[] devices = DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.AMKSSplitter);
        bool found = false;
        foreach (DsDevice dev in devices)
        {
          Guid clsID;
          string dName;
          dev.Mon.GetClassID(out clsID);
          dev.Mon.GetDisplayName(null, null, out dName);
          _log.Info("SinkGraphEx.SetupTeletext(): trying filter with ClassID: {0} ; Display Name: {1} ; Name: {2}", clsID, dName, dev.Name);
          AddTeeSinkNameToGraph(_graphBuilderInterface, dev.Name);
          teesink = DirectShowUtil.GetFilterByName(_graphBuilderInterface, "Kernel Tee");
          if (teesink != null)
          {
            _log.Info("SinkGraphEx.SetupTeletext(): Found Kernel Tee: {0}", dev.Name);
            found = true;
            break;
          }
        }
        if (!found)
        {
          _log.Error("SinkGraphEx.SetupTeletext(): Failed to find Kernel Tee");
          return;
        }
      }

      AddWstCodecToGraph(_graphBuilderInterface);//WST Codec
      IBaseFilter wstCodec = DirectShowUtil.GetFilterByName(_graphBuilderInterface, "WST Codec");
      if (wstCodec == null)
      {
        _graphBuilderInterface.RemoveFilter(teesink);
        Marshal.ReleaseComObject(teesink);
        _log.Error("SinkGraphEx.SetupTeletext(): Failed to find WST Codec");
        return;
      }

      int hr = _captureGraphBuilderInterface.RenderStream(new DsGuid(ClassId.PinCategoryVBI), null, _filterCapture, teesink, wstCodec);
      if (hr != 0)
      {
        _graphBuilderInterface.RemoveFilter(teesink);
        _graphBuilderInterface.RemoveFilter(wstCodec);
        Marshal.ReleaseComObject(teesink);
        Marshal.ReleaseComObject(wstCodec);
        _log.Error("SinkGraphEx.SetupTeletext(): Failed to find VBI pin");
        _log.Info("Teletext not available on {0}", _cardName);
        return;
      }


      _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
      _sampleGrabberInterface = (ISampleGrabber)_filterSampleGrabber;
      _graphBuilderInterface.AddFilter(_filterSampleGrabber, "Sample Grabber");

      AMMediaType mt = new AMMediaType();
      mt.majorType = MediaType.VBI;
      mt.subType = MediaSubTypeEx.Teletext;
      _sampleGrabberInterface.SetCallback(this, 1);
      _sampleGrabberInterface.SetMediaType(mt);
      _sampleGrabberInterface.SetBufferSamples(false);
      hr = _captureGraphBuilderInterface.RenderStream(null, null, wstCodec, null, _filterSampleGrabber);
      if (hr != 0)
      {
        _graphBuilderInterface.RemoveFilter(teesink);
        _graphBuilderInterface.RemoveFilter(wstCodec);
        _graphBuilderInterface.RemoveFilter(_filterSampleGrabber);
        Marshal.ReleaseComObject(teesink);
        Marshal.ReleaseComObject(wstCodec);
        Marshal.ReleaseComObject(_filterSampleGrabber);
        _log.Error("SinkGraphEx.SetupTeletext(): Failed to find Teletext info");
        return;
      }
      Marshal.ReleaseComObject(teesink);
      Marshal.ReleaseComObject(wstCodec);
      _hasTeletext = true;
    }

    void RetryOtherInstances(int instance)
    {

      _log.Error("SinkGraphEx: RetryOtherInstances:{0}", instance);
      FilterDefinition sourceFilter;
      FilterDefinition sinkFilter;
      IPin sourcePin = null;
      IPin sinkPin = null;

      sourceFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SourceCategory);
      sinkFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SinkCategory);
      if (sourceFilter == null)
      {
        _log.Error("SinkGraphEx: Cannot find source filter for connection:{0}", instance);
        return;
      }
      if (sinkFilter == null)
      {
        _log.Error("SinkGraphEx: Cannot find sink filter for connection:{0}", instance);
        return;
      }

      _log.Error("SinkGraphEx: find instances of :{0} instances", sourceFilter.FriendlyName);
      foreach (string key in AvailableFilters.Filters.Keys)
      {
        int hr;
        Filter filter;
        List<Filter> al = AvailableFilters.Filters[key];
        filter = (Filter)al[0];
        if (filter.Name.Equals(sourceFilter.FriendlyName))
        {
          _log.Error("SinkGraphEx: found :{0} instances", al.Count);
          for (int filterInstance = 0; filterInstance < al.Count; ++filterInstance)
          {
            filter = (Filter)al[filterInstance];
            _log.Error("SinkGraphEx: try instance :{0} {1}", filterInstance, filter.MonikerString);
            sourceFilter.MonikerDisplayName = filter.MonikerString;
            sourceFilter.DSFilter = Marshal.BindToMoniker(sinkFilter.MonikerDisplayName) as IBaseFilter;
            hr = _graphBuilderInterface.AddFilter(sourceFilter.DSFilter, sourceFilter.FriendlyName);
            sourcePin = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SourcePinName);
            if (sourcePin == null)
            {
              String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SourcePinName;
              if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
              {
                sourcePin = DsFindPin.ByDirection(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
                if (sourcePin == null)
                  _log.Info("SinkGraphEx:   Unable to find sourcePin: <{0}>", strPinName);
                else
                  _log.Info("SinkGraphEx:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
              }
            }
            else
              _log.Info("SinkGraphEx:   Found sourcePin: <{0}> ", ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SourcePinName);

            sinkPin = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SinkPinName);
            if (sinkPin == null)
            {
              String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SinkPinName;
              if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
              {
                sinkPin = DsFindPin.ByDirection(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
                if (sinkPin == null)
                  _log.Info("SinkGraphEx:   Unable to find sinkPin: <{0}>", strPinName);
                else
                  _log.Info("SinkGraphEx:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
              }
            }
            else
              _log.Info("SinkGraphEx:   Found sinkPin: <{0}> ", ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[instance]).SinkPinName);

            if (sourcePin != null && sinkPin != null)
            {
              IPin conPin;
              hr = sourcePin.ConnectedTo(out conPin);
              if (hr != 0)
                hr = _graphBuilderInterface.Connect(sourcePin, sinkPin);
              if (hr == 0)
                _log.Info("SinkGraphEx:   Pins connected...");

              // Give warning and release pin...
              if (conPin != null)
              {
                _log.Info("SinkGraphEx:   (Pin was already connected...)");
                Marshal.ReleaseComObject(conPin as Object);
                conPin = null;
                hr = 0;
              }
            }

            if (hr != 0)
            {
              _log.Info("SinkGraphEx:  Error: Unable to connect Pins 0x{0:X}", hr);

              if (sourcePin != null) Marshal.ReleaseComObject(sourcePin); sourcePin = null;
              if (sinkPin != null) Marshal.ReleaseComObject(sinkPin); sinkPin = null;
              if (sourceFilter.DSFilter != null)
              {
                _graphBuilderInterface.RemoveFilter(sourceFilter.DSFilter);
                Marshal.ReleaseComObject(sourceFilter.DSFilter);
                sourceFilter.DSFilter = null;
              }
            }//if (hr != 0)
            else
            {
              if (sinkPin != null)
                Marshal.ReleaseComObject(sinkPin);
              sinkPin = null;

              if (sourcePin != null)
                Marshal.ReleaseComObject(sourcePin);
              sourcePin = null;
              return;
            }
          }//for (int filterInstance=0; filterInstance < al.Count;++filterInstance)
        }//if (filter.Name.Equals(sinkFilter.FriendlyName))
      }//foreach (string key in AvailableFilters.Filters.Keys)
    }//void RetryOtherInstances(int instance)

    public override void DeleteGraph()
    {
      if (_graphState < State.Created) return;
      int hr;


      _grabTeletext = false;
      _previousChannel = -1;
      _log.Info("SinkGraph:DeleteGraph()");
      StopRecording();
      StopTimeShifting();
      StopViewing();

      GUIGraphicsContext.OnGammaContrastBrightnessChanged -= new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);
      _vmr9 = null;

      _analogVideoDecoderInterface = null;
      if (_videoProcAmpHelper != null)
      {
        _videoProcAmpHelper.Dispose();
        _videoProcAmpHelper = null;
      }
      if (_mpeg2DemuxHelper != null)
      {
        _mpeg2DemuxHelper.Dispose();
        _mpeg2DemuxHelper = null;
      }

      if (_videoCaptureHelper != null)
      {
        _videoCaptureHelper.Dispose();
        _videoCaptureHelper = null;
      }

      //      if (_tvTunerInterface != null)
      //        Marshal.ReleaseComObject(_tvTunerInterface); _tvTunerInterface = null;

      _sampleGrabberInterface = null;
      if (_filterSampleGrabber != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterSampleGrabber)) > 0) ;
        _filterSampleGrabber = null;
      }


      if (_graphBuilderInterface != null)
      {
        DirectShowUtil.RemoveFilters(_graphBuilderInterface);
      }

      //if (_filterCapture != null)
      //{
      //  Marshal.ReleaseComObject(_filterCapture); _filterCapture = null;
      //}

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;

      if ((_card != null) && (_card.Graph != null) && (_card.Graph.TvFilterDefinitions != null))
      {
        foreach (FilterDefinition dsFilter in _card.Graph.TvFilterDefinitions)
        {
          if (dsFilter.DSFilter != null)
          {
            while ((hr = Marshal.ReleaseComObject(dsFilter.DSFilter)) > 0) ;
          }
          dsFilter.DSFilter = null;
        }
      }
      if (_captureGraphBuilderInterface != null)
      {
        while ((hr = Marshal.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
        _captureGraphBuilderInterface = null;
      }

      if (_graphBuilderInterface != null)
      {
        while ((hr = Marshal.ReleaseComObject(_graphBuilderInterface)) > 0) ;
        _graphBuilderInterface = null;
      }
      _hasTeletext = false;
      _graphState = State.None;
    }

    #region graph builder helpers
    IPin GetPin(IBaseFilter filter, PinDirection direction, string pinName)
    {
      if (direction == PinDirection.Input)
      {
        if (String.Compare(pinName, "%tvtuner%", true) == 0)
        {
          IPin pin = FindCrossBarPin(filter, PhysicalConnectorType.Video_Tuner);
          if (pin != null) return pin;
        }
        if (String.Compare(pinName, "%audiotuner%", true) == 0)
        {
          IPin pin = FindCrossBarPin(filter, PhysicalConnectorType.Audio_Tuner);
          if (pin != null) return pin;
        }
      }

      IPin sinkPin = DirectShowUtil.FindPin(filter, direction, pinName);
      if (sinkPin == null)
      {
        if ((pinName.Length == 1) && (Char.IsDigit(pinName, 0)))
        {
          sinkPin = DsFindPin.ByDirection(filter, PinDirection.Input, Convert.ToInt32(pinName));
          if (sinkPin == null)
            _log.Info("SinkGraphEx:   Unable to find pin: <{0}>", pinName);
          else
            _log.Info("SinkGraphEx:   Found pin: <{0}> <{1}>", pinName, sinkPin.ToString());
        }
      }
      else
        _log.Info("SinkGraphEx:   Found pin: <{0}> ", pinName);
      return sinkPin;
    }

    IPin FindCrossBarPin(IBaseFilter filter, PhysicalConnectorType inputPinType)
    {
      IAMCrossbar crossbar = filter as IAMCrossbar;
      if (crossbar == null) return null;
      int outputPins, inputPins;
      crossbar.get_PinCounts(out outputPins, out inputPins);
      for (int i = 0; i < inputPins; ++i)
      {
        int relatedPin;
        PhysicalConnectorType physicalTypeIn;			// type of input pin
        crossbar.get_CrossbarPinInfo(true, i, out relatedPin, out physicalTypeIn);
        if (physicalTypeIn == inputPinType)
        {
          IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, i);
          return pin;
        }
      }
      return null;
    }

    IBaseFilter GetFilter(string filterName)
    {
      if (String.Compare(filterName, "%soundcard%", true) == 0)
      {
        _log.Info("SinkGraphEx:  preferred filter %soundcard%");

        FilterCollection audioInputs = Filters.AudioInputDevices;
        if (audioInputs.Count > 0)
        {
          _log.Info("SinkGraphEx:  adding filter <{0}>", audioInputs[0].Name);
          try
          {
            IBaseFilter audioInputFilter = Marshal.BindToMoniker(audioInputs[0].MonikerString) as IBaseFilter;
            return audioInputFilter;
          }
          catch (Exception)
          {
          }
        }
      }

      if (String.Compare(filterName, "%audioencoder%", true) == 0)
      {
        _log.Info("SinkGraphEx:  preferred filter %audioencoder%");
        string[] audioEncoders = new string[] { "InterVideo Audio Encoder", "MainConcept MPEG Audio Encoder", "MainConcept Demo MPEG Audio Encoder", "CyberLink Audio Encoder" };

        FilterCollection audioCodecs = Filters.AudioCompressors;
        FilterCollection legacyFilters = Filters.LegacyFilters;
        _log.Info("SinkGraphEx:  audio codecs installed:{0} preferred:{1}", audioCodecs.Count, audioEncoders.Length);
        for (int i = 0; i < audioEncoders.Length; ++i)
        {
          _log.Info("SinkGraphEx:  try audio codec:'{0}'", audioEncoders[i]);
          //check audio compressors
          try
          {
            foreach (Filter audioCodec in audioCodecs)
            {
              if (String.Compare(audioCodec.Name, audioEncoders[i], true) == 0)
              {
                _log.Info("SinkGraphEx:  audio codec:'{0}' is installed", audioEncoders[i]);
                try
                {
                  _log.Info("SinkGraphEx:  adding filter <{0}>", audioCodec.Name);
                  IBaseFilter audioCodecFilter = Marshal.BindToMoniker(audioCodec.MonikerString) as IBaseFilter;
                  return audioCodecFilter;
                }
                catch (Exception)
                {
                  _log.Info("SinkGraphEx:  failed to add audio codec:'{0}'", audioEncoders[i]);
                }
              }
            }
          }
          catch (Exception ex)
          {
            if (GUIGraphicsContext.RenderGUI == null)
            {
              MessageBox.Show("No supported MPEG-1 Layer II encoder found. Please install supported encoders for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _log.Error(ex);
          }

          //check legacy filters
          try
          {
            foreach (Filter audioCodec in legacyFilters)
            {
              if (String.Compare(audioCodec.Name, audioEncoders[i], true) == 0)
              {
                _log.Info("SinkGraphEx:  audio codec:'{0}' is installed", audioEncoders[i]);
                try
                {
                  _log.Info("SinkGraphEx:  adding filter <{0}>", audioCodec.Name);
                  IBaseFilter audioCodecFilter = Marshal.BindToMoniker(audioCodec.MonikerString) as IBaseFilter;
                  return audioCodecFilter;
                }
                catch (Exception)
                {
                  _log.Info("SinkGraphEx:  failed to add audio codec:'{0}'", audioEncoders[i]);
                }
              }
            }
          }
          catch (Exception ex)
          {
            if (GUIGraphicsContext.RenderGUI == null)
            {
              MessageBox.Show("No supported MPEG-1 Layer II encoder found. Please install supported encoders for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _log.Error(ex);
          }
        }
      }

      if (String.Compare(filterName, "%videoencoder%", true) == 0)
      {
        _log.Info("SinkGraphEx:  preferred filter %videoencoder%");
        string[] videoEncoders = new string[] { "InterVideo Video Encoder", "MainConcept MPEG Video Encoder", "MainConcept Demo MPEG Video Encoder", "CyberLink MPEG Video Encoder" };

        FilterCollection legacyFilters = Filters.LegacyFilters;
        FilterCollection videoCodecs = Filters.VideoCompressors;
        _log.Info("SinkGraphEx:  video codecs installed:{0} preferred:{1}", videoCodecs.Count, videoEncoders.Length);

        for (int i = 0; i < videoEncoders.Length; ++i)
        {
          _log.Info("SinkGraphEx:  try video codec:'{0}'", videoEncoders[i]);
          //
          //check video compressors
          try
          {
            foreach (Filter videoCodec in videoCodecs)
            {
              if (String.Compare(videoCodec.Name, videoEncoders[i], true) == 0)
              {
                _log.Info("SinkGraphEx:  video codec:'{0}' is installed", videoEncoders[i]);
                try
                {
                  _log.Info("SinkGraphEx:  adding filter <{0}>", videoCodec.Name);
                  IBaseFilter videoCodecFilter = Marshal.BindToMoniker(videoCodec.MonikerString) as IBaseFilter;
                  return videoCodecFilter;
                }
                catch (Exception)
                {
                  _log.Info("SinkGraphEx:  failed to add video codec:'{0}'", videoEncoders[i]);
                }
              }
            }
          }
          catch (Exception ex)
          {
            if (GUIGraphicsContext.RenderGUI == null)
            {
              MessageBox.Show("No supported MPEG-2 video encoder found. Please install supported encoders for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _log.Error(ex);
          }

          //check legacy filters
          try
          {
            foreach (Filter videoCodec in legacyFilters)
            {
              if (String.Compare(videoCodec.Name, videoEncoders[i], true) == 0)
              {
                _log.Info("SinkGraphEx:  video codec:'{0}' is installed", videoEncoders[i]);
                try
                {
                  _log.Info("SinkGraphEx:  adding filter <{0}>", videoCodec.Name);
                  IBaseFilter videoCodecFilter = Marshal.BindToMoniker(videoCodec.MonikerString) as IBaseFilter;
                  return videoCodecFilter;
                }
                catch (Exception)
                {
                  _log.Info("SinkGraphEx:  failed to add video codec:'{0}'", videoEncoders[i]);
                }
              }
            }
          }
          catch (Exception ex)
          {
            if (GUIGraphicsContext.RenderGUI == null)
            {
              MessageBox.Show("No supported MPEG-2 video encoder found. Please install supported encoders for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _log.Error(ex);
          }
        }
      }

      if (String.Compare(filterName, "%mpegmux%", true) == 0)
      {
        _log.Info("SinkGraphEx:  preferred filter %mpegmux%");
        string[] multiplexers = new string[] { "InterVideo Multiplexer", "Mainconcept MPEG Multiplexer", "Mainconcept Demo MPEG Multiplexer", "CyberLink MPEG Muxer" };

        FilterCollection legacyFilters = Filters.LegacyFilters;
        _log.Info("SinkGraphEx:  legacy filters installed:{0} preferred:{1}", legacyFilters.Count, multiplexers.Length);
        for (int i = 0; i < multiplexers.Length; ++i)
        {
          _log.Info("SinkGraphEx:  try multiplexer:'{0}'", multiplexers[i]);
          try
          {
            foreach (Filter filter in legacyFilters)
            {
              if (String.Compare(filter.Name, multiplexers[i], true) == 0)
              {
                _log.Info("SinkGraphEx:  multiplexer:'{0}' is installed", multiplexers[i]);
                try
                {
                  _log.Info("SinkGraphEx:  adding filter <{0}>", filter.Name);
                  IBaseFilter multiplexerFilter = Marshal.BindToMoniker(filter.MonikerString) as IBaseFilter;
                  return multiplexerFilter;
                }
                catch (Exception)
                {
                  _log.Info("SinkGraphEx:  failed to add multiplexer:'{0}'", multiplexers[i]);
                }
              }
            }
          }
          catch (Exception ex)
          {
            if (GUIGraphicsContext.RenderGUI == null)
            {
              MessageBox.Show("No supported MPEG-2 multiplexer found. Please install supported multiplexer for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _log.Error(ex);
          }
        }
      }
      return null;
    }
    #endregion

    #endregion Overrides

    #region Obsolete But Probably Needed Again

    // #MW#
    // Not used anymore. Might be needed though to support these cards, or is a generic approach possible?
    // What if the connection order always assures that the TVTuner is connected first, meaning the
    // outputs of the crossbar are NOT connected to any other filter???????
    //
    // Extra Note:
    // Just found out that once you load some of the filters, some connectiona are already made, even
    // without explicit calls, so the crossbar might be connected already...
    void ConnectTVTunerOutputs()
    {
      // AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
      // the crossbar outputs are disconnected
      // same for the winfast pvr 2000
      _log.Info("SinkGraphEx:ConnectTVTunerOutputs()");

      //find crossbar
      int hr;
      DsGuid cat;
      Guid iid;
      object o = null;
      cat = new DsGuid(FindDirection.UpstreamOnly);
      iid = typeof(IAMCrossbar).GUID;
      hr = _captureGraphBuilderInterface.FindInterface(cat, null, _filterCapture, iid, out o);
      if (hr != 0 || o == null)
      {
        _log.Info("SinkGraphEx:no crossbar found");
        return; // no crossbar found?
      }

      IAMCrossbar crossbar = o as IAMCrossbar;
      if (crossbar == null)
      {
        _log.Info("SinkGraphEx:no crossbar found");
        return;
      }

      //disconnect the output pins of the crossbar->video capture filter
      //			Log.WriteFile(Log.LogType.Log,"SinkGraphEx:disconnect crossbar outputs");
      DirectShowUtil.DisconnectOutputPins(_graphBuilderInterface, (IBaseFilter)crossbar);

      //connect the output pins of the tvtuner
      //			Log.WriteFile(Log.LogType.Log,"SinkGraphEx:connect tvtuner outputs");
      bool bAllConnected = DirectShowUtil.RenderOutputPins(_graphBuilderInterface, (IBaseFilter)_tvTunerInterface);
      if (!bAllConnected)
        _log.Info("SinkGraphEx:FAILED, not all pins connected");

      //reconnect the output pins of the crossbar
      //			Log.WriteFile(Log.LogType.Log,"SinkGraphEx:reconnect crossbar output pins");

      bAllConnected = DirectShowUtil.RenderOutputPins(_graphBuilderInterface, (IBaseFilter)crossbar);
      if (!bAllConnected)
        _log.Info("SinkGraphEx:FAILED, not all pins connected");

      if (o != null)
      {
        crossbar = null;
        Marshal.ReleaseComObject(o);
        o = null;
      }
    }

    #endregion

    #region ISampleGrabberCB Members

    public int SampleCB(double SampleTime, IMediaSample pSample)
    {
      // TODO:  Add SinkGraphEx.SampleCB implementation
      return 0;
    }

    public int BufferCB(double SampleTime, System.IntPtr pBuffer, int BufferLen)
    {
      if (!_grabTeletext) return 0;
      if (pBuffer == IntPtr.Zero) return 0;
      if (BufferLen < 43) return 0;

      TeletextGrabber.SaveAnalogData(pBuffer, BufferLen);
      return 0;
    }
    bool SetFrameSize(int width, int height)
    {
      _videoCaptureHelper.SetFrameSize(new Size(width, height));
      Size size = _videoCaptureHelper.GetFrameSize();
      if (size.Width != width) return false;
      if (size.Height != height) return false;
      return true;

    }

    #endregion
  }
}
#endif