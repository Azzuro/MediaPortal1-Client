//#define DUMP
#if (UseCaptureCardDefinitions)
using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using DShowNET;
using DShowNET.Device;
using DShowNET.BDA;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using TVCapture;
using System.Xml;
using DirectX.Capture;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Implementation of IGraph for digital TV capture cards using the BDA driver architecture
	/// It handles any DVB-T, DVB-C, DVB-S TV Capture card with BDA drivers
	///
	/// A graphbuilder object supports one or more TVCapture cards and
	/// contains all the code/logic necessary for
	/// -tv viewing
	/// -tv recording
	/// -tv timeshifting
	/// -radio
	/// </summary>
	public class DVBGraphBDA : MediaPortal.TV.Recording.IGraph
	{

		#region imports
		[ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
			class VideoAnalyzer {};

		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
    
		[ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
		class StreamBufferSink {};

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
	    
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern bool ConvertStringSidToSid(string pStringSid, ref IntPtr pSID);

		[DllImport("kernel32", CharSet=CharSet.Auto)]
		private static extern IntPtr  LocalFree( IntPtr hMem);

		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool EventMsg(int eventType,[In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int SetAppHandle([In] IntPtr hnd/*,[In, MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback*/);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern void PidCallback([In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick([In] int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle([In] long menu);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetPidMap(DShowNET.IPin filter, ref uint pid, ref uint mediasampletype);
		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin pin,int pid,IPin pin1,int pid1);

		#endregion

		#region class member variables
		enum State
		{ 
			None, 
			Created, 
			TimeShifting,
			Recording, 
			Viewing,
			Radio
		};
		const int WS_CHILD				= 0x40000000;	
		const int WS_CLIPCHILDREN	= 0x02000000;
		const int WS_CLIPSIBLINGS	= 0x04000000;

		private static Guid CLSID_StreamBufferSink					= new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
		private static Guid CLSID_Mpeg2VideoStreamAnalyzer	= new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
		private static Guid CLSID_StreamBufferConfig				= new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);

		int                     m_cardID								= -1;
		int                     m_iCurrentChannel				= 28;
		int											m_rotCookie							= 0;			// Cookie into the Running Object Table
		int                     m_iPrevChannel					= -1;
		bool                    m_bIsUsingMPEG					= false;
		State                   m_graphState						= State.None;
		DateTime								m_StartTime							= DateTime.Now;


		IBaseFilter             m_NetworkProvider				= null;			// BDA Network Provider
		IBaseFilter             m_TunerDevice						= null;			// BDA Digital Tuner Device
		IBaseFilter							m_CaptureDevice					= null;			// BDA Digital Capture Device
		IBaseFilter							m_MPEG2Demultiplexer		= null;			// Mpeg2 Demultiplexer that connects to Preview pin on Smart Tee (must connect before capture)
		IBaseFilter							m_TIF										= null;			// Transport Information Filter
		IBaseFilter							m_SectionsTables				= null;
		VideoAnalyzer						m_mpeg2Analyzer					= null;
		StreamBufferSink				m_StreamBufferSink=null;
		IGraphBuilder           m_graphBuilder					= null;
		ICaptureGraphBuilder2   m_captureGraphBuilder		= null;
		IVideoWindow            m_videoWindow						= null;
		IBasicVideo2            m_basicVideo						= null;
		IMediaControl						m_mediaControl					= null;
		IBDA_SignalStatistics[] m_TunerStatistics       = null;
		NetworkType							m_NetworkType=NetworkType.Unknown;
		IBaseFilter							m_sampleGrabber=null;
		ISampleGrabber					m_sampleInterface=null;
		
		TVCaptureDevice					m_Card;
		
		//streambuffer interfaces
		IPin												m_DemuxVideoPin				= null;
		IPin												m_DemuxAudioPin				= null;
		IPin												m_pinStreamBufferIn0	= null;
		IPin												m_pinStreamBufferIn1	= null;
		IStreamBufferSink						m_IStreamBufferSink		= null;
		IStreamBufferConfigure			m_IStreamBufferConfig	= null;
		StreamBufferConfig					m_StreamBufferConfig	= null;
		VMR9Util									  Vmr9								  = null; 
		//GuideDataEvent							m_Event               = null;
		//GCHandle										myHandle;
		//int                         adviseCookie;
		bool												graphRunning=false;
		DVBChannel									currentTuningObject=null;
		TSHelperTools								transportHelper=new TSHelperTools();
		bool												refreshPmtTable=false;
		
		protected bool							m_pluginsEnabled=false;

		DateTime										timeResendPid=DateTime.Now;
		DVBDemuxer									m_streamDemuxer = new DVBDemuxer();
		VMR9OSD	m_osd=new VMR9OSD();
		bool m_useVMR9Zap=false;
		
		int m_iVideoWidth=1;
		int m_iVideoHeight=1;
		int m_aspectX=1;
		int m_aspectY=1;

		DirectShowHelperLib.StreamBufferRecorderClass m_recorder=null;
#if DUMP
		System.IO.FileStream fileout;
#endif
		#endregion

		#region constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pCard">instance of a TVCaptureDevice which contains all details about this card</param>
		public DVBGraphBDA(TVCaptureDevice pCard)	
		{
			m_Card								= pCard;
			m_cardID							= pCard.ID;
			m_bIsUsingMPEG				= true;
			m_graphState					= State.None;

			try
			{
				System.IO.Directory.CreateDirectory("database");
			}
			catch(Exception){}

			try
			{				
				System.IO.Directory.CreateDirectory(@"database\pmt");
			}
			catch(Exception){}
			//create registry keys needed by the streambuffer engine for timeshifting/recording
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");
			}
			catch(Exception){}
			
		}

		#endregion

		#region create/view/timeshift/record
		#region createGraph/DeleteGraph()
		/// <summary>
		/// Creates a new DirectShow graph for the TV capturecard.
		/// This graph can be a DVB-T, DVB-C or DVB-S graph
		/// </summary>
		/// <returns>bool indicating if graph is created or not</returns>
		public bool CreateGraph(int Quality)
		{
			try
			{
				//check if we didnt already create a graph
				if (m_graphState != State.None) 
					return false;

#if DUMP
				fileout = new System.IO.FileStream("audiodump.dat",System.IO.FileMode.OpenOrCreate,System.IO.FileAccess.Write,System.IO.FileShare.None);
#endif
				graphRunning=false;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph(). ");

				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					m_pluginsEnabled=xmlreader.GetValueAsBool("dvb_ts_cards","enablePlugins",false);
				 m_useVMR9Zap=xmlreader.GetValueAsBool("general","useVMR9ZapOSD",false);
				}

				//no card defined? then we cannot build a graph
				if (m_Card==null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card is not defined");
					return false;
				}

				//load card definition from CaptureCardDefinitions.xml
				if (!m_Card.LoadDefinitions())											// Load configuration for this card
				{
					DirectShowUtil.DebugWrite("DVBGraphBDA: Loading card definitions for card {0} failed", m_Card.CaptureName);
					return false;
				}
				
				//check if definition contains a tv filter graph
				if (m_Card.TvFilterDefinitions==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card does not contain filters?");
					return false;
				}

				//check if definition contains <connections> for the tv filter graph
				if (m_Card.TvConnectionDefinitions==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card does not contain connections for tv?");
					return false;
				}

				//create new instance of VMR9 helper utility
				Vmr9 =new VMR9Util("mytv");

				// Make a new filter graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:create new filter graph (IGraphBuilder)");
				m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

			
				// Get the Capture Graph Builder
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
				Guid clsid = Clsid.CaptureGraphBuilder2;
				Guid riid = typeof(ICaptureGraphBuilder2).GUID;
				m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
				int hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
				if (hr < 0) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:link FAILED:0x{0:X}",hr);
					return false;
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Add graph to ROT table");
				DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);


				m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
				m_sampleInterface=(ISampleGrabber) m_sampleGrabber;
				m_graphBuilder.AddFilter(m_sampleGrabber,"Sample Grabber");

				// Loop through configured filters for this card, bind them and add them to the graph
				// Note that while adding filters to a graph, some connections may already be created...
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured filters...");
				foreach (string catName in m_Card.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = m_Card.TvFilterDefinitions[catName] as FilterDefinition;
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					dsFilter.DSFilter         = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
					hr = m_graphBuilder.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
					if (hr == 0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					}
					else
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:  Error! Result code = {0}", hr);
					}

					// Support the "legacy" member variables. This could be done different using properties
					// through which the filters are accessable. More implementation independent...
					if (dsFilter.Category == "networkprovider") 
					{
						m_NetworkProvider       = dsFilter.DSFilter;
						// Initialise Tuning Space (using the setupTuningSpace function)
						if(!setupTuningSpace()) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED couldnt create tuning space");
							return false;
						}
					}
					if (dsFilter.Category == "tunerdevice") m_TunerDevice	 							= dsFilter.DSFilter;
					if (dsFilter.Category == "capture")			m_CaptureDevice							= dsFilter.DSFilter;
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured filters...DONE");

				//no network provider specified? then we cannot build the graph
				if(m_NetworkProvider == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED networkprovider filter not found");
					return false;
				}

				//no capture device specified? then we cannot build the graph
				if(m_CaptureDevice == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED capture filter not found");
				}



				FilterDefinition sourceFilter;
				FilterDefinition sinkFilter;
				IPin sourcePin;
				IPin sinkPin;

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

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured pin connections...");
				for (int i = 0; i < m_Card.TvConnectionDefinitions.Count; i++)
				{
					//get the source filter for the connection
					sourceFilter = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourceCategory] as FilterDefinition;
					if (sourceFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"Cannot find source filter for connection:{0}",i);
						continue;
					}

					//get the destination/sink filter for the connection
					sinkFilter   = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkCategory] as FilterDefinition;
					if (sinkFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"Cannot find sink filter for connection:{0}",i);
						continue;
					}

					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Connecting <{0}>:{1} with <{2}>:{3}", 
										sourceFilter.FriendlyName, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName,
										sinkFilter.FriendlyName, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
					
					//find the pin of the source filter
					sourcePin    = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName);
					if (sourcePin == null)
					{
						String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sourcePin = DirectShowUtil.FindPinNr(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
							if (sourcePin==null)
								Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sourcePin: <{0}>", strPinName);
							else
								Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
						}
					}
					else
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sourcePin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName);

					//find the pin of the sink filter
					sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
					if (sinkPin == null)
					{
						String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
							if (sinkPin==null)
								Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
							else
								Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
						}
					}
					else
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);

					//if we have both pins
					if (sourcePin!=null && sinkPin!=null)
					{
						// then connect them
						IPin conPin;
						hr      = sourcePin.ConnectedTo(out conPin);
						if (hr != 0)
							hr = m_graphBuilder.Connect(sourcePin, sinkPin);
						if (hr == 0)
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Pins connected...");

						// Give warning and release pin...
						if (conPin != null)
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   (Pin was already connected...)");
							Marshal.ReleaseComObject(conPin as Object);
							conPin = null;
							hr     = 0;
						}
					}

					
					
					//log if connection failed
					//if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
					//	hr=1;
					if (hr != 0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   unable to connect pins");
						if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   try other instances");
							m_graphBuilder.RemoveFilter(sinkFilter.DSFilter);
							Marshal.ReleaseComObject(sinkPin);
							Marshal.ReleaseComObject(sinkFilter.DSFilter);
							sinkPin=null;
							foreach (string key in AvailableFilters.Filters.Keys)
							{
								Filter    filter;
								ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
								filter    = (Filter)al[0];
								if (filter.Name.Equals(sinkFilter.FriendlyName))
								{
									Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   found {0} instances",al.Count);
									for (int filterInstance=0; filterInstance < al.Count;++filterInstance)
									{
										filter    = (Filter)al[filterInstance];
										Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   try:{0}",filter.MonikerString);
										sinkFilter.MonikerDisplayName=filter.MonikerString;
										sinkFilter.DSFilter  = Marshal.BindToMoniker(sinkFilter.MonikerDisplayName) as IBaseFilter;
										hr = m_graphBuilder.AddFilter(sinkFilter.DSFilter, sinkFilter.FriendlyName);
										//find the pin of the sink filter
										sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
										if (sinkPin == null)
										{
											String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName;
											if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
											{
												sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
												if (sinkPin==null)
													Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
												else
													Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
											}
										}
										if (sinkPin!=null)
										{
											hr = m_graphBuilder.Connect(sourcePin, sinkPin);
											if (hr == 0)
											{
												Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Pins connected...");
												break;
											}
											else
											{
												Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   cannot connect pins.");
												m_graphBuilder.RemoveFilter(sinkFilter.DSFilter);
												Marshal.ReleaseComObject(sinkPin);
												Marshal.ReleaseComObject(sinkFilter.DSFilter);
											}
										}
									}
								}
							}
						}
					}
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured pin connections...DONE");

				// Find out which filter & pin is used as the interface to the rest of the graph.
				// The configuration defines the filter, including the Video, Audio and Mpeg2 pins where applicable
				// We only use the filter, as the software will find the correct pin for now...
				// This should be changed in the future, to allow custom graph endings (mux/no mux) using the
				// video and audio pins to connect to the rest of the graph (SBE, overlay etc.)
				// This might be needed by the ATI AIW cards (waiting for ob2 to release...)
				FilterDefinition lastFilter = m_Card.TvFilterDefinitions[m_Card.TvInterfaceDefinition.FilterCategory] as FilterDefinition;

				// no interface defined or interface not found? then return
				if(lastFilter == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED interface filter not found");
					return false;
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() connect interface pin->sample grabber");
				if (!ConnectFilters(ref lastFilter.DSFilter,ref m_sampleGrabber))
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
					return false;
				}
				//=========================================================================================================
				// add the MPEG-2 Demultiplexer 
				//=========================================================================================================
				// Use CLSID_Mpeg2Demultiplexer to create the filter
				m_MPEG2Demultiplexer = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.Mpeg2Demultiplexer, true));
				if(m_MPEG2Demultiplexer== null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to create Mpeg2 Demultiplexer");
					return false;
				}

				// Add the Demux to the graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() add mpeg2 demuxer to graph");
				m_graphBuilder.AddFilter(m_MPEG2Demultiplexer, "MPEG-2 Demultiplexer");
				
				if(!ConnectFilters(ref m_sampleGrabber, ref m_MPEG2Demultiplexer)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
					return false;
				}
				for (int i=0; i < 10; ++i)
				{
					IPin pin;
					DsUtils.GetPin(m_MPEG2Demultiplexer,PinDirection.Output,i,out pin);
					if (pin!=null)
					{
						IEnumMediaTypes enumMedia;
						pin.EnumMediaTypes(out enumMedia);
						enumMedia.Reset();
						AMMediaTypeClass mt = new AMMediaTypeClass();
						uint fetched;
						while (enumMedia.Next(1,out mt, out fetched)==0)
						{
							if (fetched==1)
							{
								if (mt.majorType==MediaType.Video)
								{
									Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   pin:{0} contains video",i+1);
									m_Card.TvInterfaceDefinition.VideoPinName = String.Format("{0}",i+1);

								}
								if (mt.majorType==MediaType.Audio)
								{
									Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   pin:{0} contains audio",i+1);
									m_Card.TvInterfaceDefinition.AudioPinName = String.Format("{0}",i+1);
								}
							}
						}
					}
				}
				if (m_Card.TvInterfaceDefinition.AudioPinName=="3" &&
						m_Card.TvInterfaceDefinition.VideoPinName=="2")
				{
					m_Card.TvInterfaceDefinition.Mpeg2PinName="1";
					m_Card.TvInterfaceDefinition.SectionsAndTablesPinName="5";
				}
				
				if (m_Card.TvInterfaceDefinition.AudioPinName=="4" &&
						m_Card.TvInterfaceDefinition.VideoPinName=="3")
				{
					m_Card.TvInterfaceDefinition.Mpeg2PinName="1";
					m_Card.TvInterfaceDefinition.SectionsAndTablesPinName="2";
				}
				//=========================================================================================================
				// Add the BDA MPEG2 Transport Information Filter
				//=========================================================================================================
				object tmpObject;
				if(!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "BDA MPEG2 Transport Information Filter", out tmpObject)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED Failed to find BDA MPEG2 Transport Information Filter");
					return false;
				}
				m_TIF = (IBaseFilter) tmpObject;
				tmpObject = null;
				if(m_TIF == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED BDA MPEG2 Transport Information Filter is null");
					return false;
				}
				m_graphBuilder.AddFilter(m_TIF, "BDA MPEG2 Transport Information Filter");

				//connect mpeg2 demultiplexer->BDA MPEG2 Transport Information Filter
				if(!ConnectFilters(ref m_MPEG2Demultiplexer, ref m_TIF)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect MPEG-2 Demultiplexer->BDA MPEG2 Transport Information Filter");
					return false;
				}

				//=========================================================================================================
				// Add the MPEG-2 Sections and Tables filter
				//=========================================================================================================
				if(!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "MPEG-2 Sections and Tables", out tmpObject)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED Failed to find MPEG-2 Sections and Tables Filter");
					return false;
				}
				m_SectionsTables = (IBaseFilter) tmpObject;
				tmpObject = null;
				if(m_SectionsTables == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED MPEG-2 Sections and Tables Filter is null");
				}

				m_graphBuilder.AddFilter(m_SectionsTables, "MPEG-2 Sections & Tables");

				//connect the mpeg2 demultiplexer->MPEG-2 Sections & Tables
				int iPreferredOutputPin=0;
				try
				{
					//get the preferred mpeg2 demultiplexer pin
					iPreferredOutputPin=Convert.ToInt32(m_Card.TvInterfaceDefinition.SectionsAndTablesPinName);
				}
				catch(Exception){}

				//and connect
				if(!ConnectFilters(ref m_MPEG2Demultiplexer, ref m_SectionsTables, iPreferredOutputPin)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect MPEG-2 Demultiplexer to MPEG-2 Sections and Tables Filter");
					return false;
				}

				//get the video/audio output pins of the mpeg2 demultiplexer
				m_MPEG2Demultiplexer.FindPin(m_Card.TvInterfaceDefinition.VideoPinName, out m_DemuxVideoPin);
				m_MPEG2Demultiplexer.FindPin(m_Card.TvInterfaceDefinition.AudioPinName, out m_DemuxAudioPin);
				if (m_DemuxVideoPin==null)
				{
					//video pin not found
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to get pin '{0}' (video out) from MPEG-2 Demultiplexer",m_DemuxVideoPin);
					return false;
				}
				if (m_DemuxAudioPin==null)
				{
					//audio pin not found
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to get pin '{0}' (audio out)  from MPEG-2 Demultiplexer",m_DemuxAudioPin);
					return false;
				}

				//=========================================================================================================
				// Create the streambuffer engine and mpeg2 video analyzer components since we need them for
				// recording and timeshifting
				//=========================================================================================================
				m_StreamBufferSink  = new StreamBufferSink();
				m_mpeg2Analyzer     = new VideoAnalyzer();
				m_IStreamBufferSink = (IStreamBufferSink) m_StreamBufferSink;
				m_graphState=State.Created;

				m_TunerStatistics=GetTunerSignalStatistics();

				// teletext settings
				GUIWindow win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
				if(win!=null)
					win.SetObject(m_streamDemuxer.Teletext);
			
				win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
				if(win!=null)
					win.SetObject(m_streamDemuxer.Teletext);


				m_streamDemuxer.OnAudioFormatChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnAudioChanged(m_streamDemuxer_OnAudioFormatChanged);
				m_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
				m_streamDemuxer.SetCardType((int)DVBEPG.EPGCard.BDACards, Network());
				m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);

				if(m_sampleInterface!=null)
				{
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					m_sampleInterface.SetCallback(m_streamDemuxer,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}
				else
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:creategraph() SampleGrabber-Interface not found");

				m_osd.Mute=false;
				win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if(win!=null)
					win.SetObject(m_osd);
				win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
				if(win!=null)
					win.SetObject(m_osd);
			
				return true;
			}
			catch(Exception)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Unable to create graph");
				return false;
			}
		}//public bool CreateGraph()

		/// <summary>
		/// Deletes the current DirectShow graph created with CreateGraph()
		/// Frees any (unmanaged) resources
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public void DeleteGraph()
		{
			try
			{
				if (m_graphState < State.Created) 
					return;
				m_iPrevChannel = -1;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:DeleteGraph()");
				StopRecording();
				StopViewing();
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free tuner interfaces");
				
				// to clear buffers for epg and teletext
				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(0, 0, 0, 0, "",0);
				}

				if (m_TunerStatistics!=null)
				{
					for (int i = 0; i < m_TunerStatistics.Length; i++) 
					{
						if (m_TunerStatistics[i] != null)
						{
							Marshal.ReleaseComObject(m_TunerStatistics[i]); 
							m_TunerStatistics[i] = null;
						}
					}
					m_TunerStatistics=null;
				}

				if (Vmr9!=null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free vmr9");
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}

				//			UnAdviseProgramInfo();
				
				if (m_recorder!=null) 
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free recorder");
					m_recorder.Stop();
					m_recorder=null;
				}
				
				if (m_StreamBufferSink!=null) 
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free streambuffer");
					Marshal.ReleaseComObject(m_StreamBufferSink); m_StreamBufferSink=null;
				}

				if (m_mediaControl != null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: stop graph");
					m_mediaControl.Stop();
				}
				graphRunning=false;

				if (m_videoWindow != null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: hide video window");
					m_videoWindow.put_Visible(DsHlp.OAFALSE);
					//m_videoWindow.put_Owner(IntPtr.Zero);
					m_videoWindow = null;
				}

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free other interfaces");
				if (m_sampleGrabber != null) 
					Marshal.ReleaseComObject(m_sampleGrabber); m_sampleGrabber=null;
				m_sampleInterface=null;

				if (m_StreamBufferConfig != null) 
					Marshal.ReleaseComObject(m_StreamBufferConfig); m_StreamBufferConfig=null;

				if (m_IStreamBufferConfig != null) 
					Marshal.ReleaseComObject(m_IStreamBufferConfig); m_IStreamBufferConfig=null;

				if (m_pinStreamBufferIn1 != null) 
					Marshal.ReleaseComObject(m_pinStreamBufferIn1); m_pinStreamBufferIn1=null;

				if (m_pinStreamBufferIn0 != null) 
					Marshal.ReleaseComObject(m_pinStreamBufferIn0); m_pinStreamBufferIn0=null;

				if (m_IStreamBufferSink != null) 
					Marshal.ReleaseComObject(m_IStreamBufferSink); m_IStreamBufferSink=null;

				if (m_NetworkProvider != null)
					Marshal.ReleaseComObject(m_NetworkProvider); m_NetworkProvider = null;

				if (m_TunerDevice != null)
					Marshal.ReleaseComObject(m_TunerDevice); m_TunerDevice = null;

				if (m_CaptureDevice != null)
					Marshal.ReleaseComObject(m_CaptureDevice); m_CaptureDevice = null;
				
				if (m_MPEG2Demultiplexer != null)
					Marshal.ReleaseComObject(m_MPEG2Demultiplexer); m_MPEG2Demultiplexer = null;

				if (m_TIF != null)
					Marshal.ReleaseComObject(m_TIF); m_TIF = null;

				if (m_SectionsTables != null)
					Marshal.ReleaseComObject(m_SectionsTables); m_SectionsTables = null;

				m_basicVideo = null;
				m_mediaControl = null;
			      
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: remove filters");
				if (m_graphBuilder!=null)
					DsUtils.RemoveFilters(m_graphBuilder);

				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: remove graph from rot");
				if (m_rotCookie != 0)
					DsROT.RemoveGraphFromRot(ref m_rotCookie);
				m_rotCookie = 0;

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: remove graph");
				if (m_captureGraphBuilder != null)
					Marshal.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;

				if (m_graphBuilder != null)
					Marshal.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;


				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: clean filters");
				foreach (string strfName in m_Card.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = m_Card.TvFilterDefinitions[strfName] as FilterDefinition;
					if (dsFilter.DSFilter != null)
						Marshal.ReleaseComObject(dsFilter.DSFilter);
					((FilterDefinition)m_Card.TvFilterDefinitions[strfName]).DSFilter = null;
					dsFilter = null;
				}

#if DUMP
				if (fileout!=null)
				{
					fileout.Close();
					fileout=null;
				}
#endif
				m_graphState = State.None;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: delete graph done");
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: deletegraph() {0} {1} {2}",
											ex.Message,ex.Source,ex.StackTrace);
			}
		}//public void DeleteGraph()
		
		#endregion
		#region Start/Stop Recording
		/// <summary>
		/// Starts recording live TV to a file
		/// <param name="strFileName">filename for the new recording</param>
		/// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
		/// <param name="timeProgStart">Contains the starttime of the current tv program</param>
		/// </summary>
		/// <returns>boolean indicating if recorded is started or not</returns> 
		/// <remarks>
		/// Graph should be timeshifting. When Recording is started the graph is still 
		/// timeshifting
		/// 
		/// A content recording will start recording from the moment this method is called
		/// and ignores any data left/present in the timeshifting buffer files
		/// 
		/// A reference recording will start recording from the moment this method is called
		/// It will examine the timeshifting files and try to record as much data as is available
		/// from the timeProgStart till the moment recording is stopped again
		/// </remarks>
		public bool StartRecording(Hashtable attributes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
		{
			if (m_graphState != State.TimeShifting) 
				return false;
			
			if (m_StreamBufferSink == null) 
				return false;

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRecording()");
			uint iRecordingType=0;
			if (bContentRecording) 
				iRecordingType = 0;
			else 
				iRecordingType = 1;										

			try
			{
				m_recorder = new DirectShowHelperLib.StreamBufferRecorderClass();
				m_recorder.Create(m_IStreamBufferSink as DirectShowHelperLib.IBaseFilter,strFileName,iRecordingType);

				long lStartTime = 0;

				// if we're making a reference recording
				// then record all content from the past as well
				if (!bContentRecording)
				{
					// so set the startttime...
					uint uiSecondsPerFile;
					uint uiMinFiles, uiMaxFiles;
					m_IStreamBufferConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
					m_IStreamBufferConfig.GetBackingFileDuration(out uiSecondsPerFile);
					lStartTime = uiSecondsPerFile;
					lStartTime *= (long)uiMaxFiles;

					// if start of program is given, then use that as our starttime
					if (timeProgStart.Year > 2000)
					{
						TimeSpan ts = DateTime.Now - timeProgStart;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
							timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
							ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
																
						lStartTime = (long)ts.TotalSeconds;
					}
					else Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: record entire timeshift buffer");
	      
					TimeSpan tsMaxTimeBack = DateTime.Now - m_StartTime;
					if (lStartTime > tsMaxTimeBack.TotalSeconds)
					{
						lStartTime = (long)tsMaxTimeBack.TotalSeconds;
					}
	        

					lStartTime *= -10000000L;//in reference time 
				}//if (!bContentRecording)
		
				foreach (MetadataItem item in attributes.Values)
				{
					try
					{
						if (item.Type == MetadataItemType.String)
							m_recorder.SetAttributeString(item.Name,item.Value.ToString());
						if (item.Type == MetadataItemType.Dword)
							m_recorder.SetAttributeDWORD(item.Name,UInt32.Parse(item.Value.ToString()));
					}
					catch(Exception){}
				}
				m_recorder.Start((int)lStartTime);
			}
			finally
			{
			}
			m_graphState = State.Recording;
			return true;
		}//public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
	    
		/// <summary>
		/// Stops recording 
		/// </summary>
		/// <remarks>
		/// Graph should be recording. When Recording is stopped the graph is still 
		/// timeshifting
		/// </remarks>
		public void StopRecording()
		{
			if (m_graphState != State.Recording) return;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:stop recording...");

			if (m_recorder!=null) 
			{
				m_recorder.Stop();
				m_recorder=null;
			}


			m_graphState = State.TimeShifting;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:stopped recording...");
		}//public void StopRecording()

		#endregion		
		#region Start/Stop Viewing
		/// <summary>
		/// Starts viewing the TV channel 
		/// </summary>
		/// <param name="iChannelNr">TV channel to which card should be tuned</param>
		/// <returns>boolean indicating if succeed</returns>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public bool StartViewing(TVChannel channel)
		{
			if (m_graphState != State.Created) return false;
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartViewing()");

			// add VMR9 renderer to graph
			Vmr9.AddVMR9(m_graphBuilder);

			// add the preferred video/audio codecs
			AddPreferredCodecs(true,true);

			// render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
			if(m_graphBuilder.Render(m_DemuxVideoPin) != 0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render video out pin MPEG-2 Demultiplexer");
				return false;
			}

			if(m_graphBuilder.Render(m_DemuxAudioPin) != 0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
				return false;
			}

			//get the IMediaControl interface of the graph
			if(m_mediaControl == null)
				m_mediaControl = (IMediaControl) m_graphBuilder;

			int hr;
			//if are using the overlay video renderer
			if (!Vmr9.IsVMR9Connected)
			{
				//then get the overlay video renderer interfaces
				m_videoWindow = m_graphBuilder as IVideoWindow;
				if (m_videoWindow == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED:Unable to get IVideoWindow");
					return false;
				}

				m_basicVideo = m_graphBuilder as IBasicVideo2;
				if (m_basicVideo == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED:Unable to get IBasicVideo2");
					return false;
				}

				// and set it up
				hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: FAILED:set Video window:0x{0:X}",hr);

				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:set Video window style:0x{0:X}",hr);

				//show overlay window
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:put_Visible:0x{0:X}",hr);
			}
			else
			{
		
			}

			//start the graph
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
			hr=m_mediaControl.Run();
			if (hr<0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
			}

			graphRunning=true;
			
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();


			// tune to the correct channel
			if (channel.Number>=0)
				TuneChannel(channel);

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Viewing..");
			return true;
		}//public bool StartViewing(AnalogVideoStandard standard, int iChannel,int country)


		/// <summary>
		/// Stops viewing the TV channel 
		/// </summary>
		/// <returns>boolean indicating if succeed</returns>
		/// <remarks>
		/// Graph must be viewing first with StartViewing()
		/// </remarks>
		public bool StopViewing()
		{
			if (m_graphState != State.Viewing) return false;
	       
			GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StopViewing()");
			if (m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);

			if (Vmr9!=null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: stop vmr9");
				Vmr9.Enable(false);
			}

			if (m_mediaControl!=null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: stop graph {0}", GUIGraphicsContext.InVmr9Render);
				m_mediaControl.Stop();
			}
			graphRunning=false;
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}
				
		#endregion
		#region Start/Stop Timeshifting
		/// <summary>
		/// Starts timeshifting the TV channel and stores the timeshifting 
		/// files in the specified filename
		/// </summary>
		/// <param name="iChannelNr">TV channel to which card should be tuned</param>
		/// <param name="strFileName">Filename for the timeshifting buffers</param>
		/// <returns>boolean indicating if timeshifting is running or not</returns>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public bool StartTimeShifting(TVChannel channel, string strFileName)
		{
			if(m_graphState!=State.Created)
				return false;
			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartTimeShifting()");

			if(CreateSinkSource(strFileName))
			{
				if(m_mediaControl == null) 
				{
					m_mediaControl = (IMediaControl) m_graphBuilder;
				}
				//now start the graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
				int hr=m_mediaControl.Run();
				if (hr<0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
				}
				graphRunning=true;
				m_graphState = State.TimeShifting;
			}
			else 
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Unable to create sinksource()");
				return false;
			}
			TuneChannel(channel);

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:timeshifting started");			
			return true;
		}//public bool StartTimeShifting(int country,AnalogVideoStandard standard, int iChannel, string strFileName)
		
		/// <summary>
		/// Stops timeshifting and cleans up the timeshifting files
		/// </summary>
		/// <returns>boolean indicating if timeshifting is stopped or not</returns>
		/// <remarks>
		/// Graph should be timeshifting 
		/// </remarks>
		public bool StopTimeShifting()
		{
			if (m_graphState != State.TimeShifting) return false;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StopTimeShifting()");
			if (m_mediaControl!=null)
				m_mediaControl.Stop();
			graphRunning=false;
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}//public bool StopTimeShifting()
		
		#endregion
		#endregion

		private bool m_streamDemuxer_AudioHasChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
		{
			return false;
		}
		#region overrides
		/// <summary>
		/// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
		/// </summary>
		private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			if (m_graphState != State.Viewing) return;
			int iVideoWidth, iVideoHeight;
			int aspectX, aspectY;
			if (Vmr9.IsVMR9Connected)
			{
				aspectX=iVideoWidth=Vmr9.VideoWidth;
				aspectY=iVideoHeight=Vmr9.VideoHeight;
			}
			else
			{
				m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
				m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}

			m_iVideoWidth=iVideoWidth;
			m_iVideoHeight=iVideoHeight;
			m_aspectX=aspectX;
			m_aspectY=aspectY;

			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				float x = GUIGraphicsContext.OverScanLeft;
				float y = GUIGraphicsContext.OverScanTop;
				int nw = GUIGraphicsContext.OverScanWidth;
				int nh = GUIGraphicsContext.OverScanHeight;
				if (nw <= 0 || nh <= 0) return;


				System.Drawing.Rectangle rSource, rDest;
				MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();
				m_geometry.ImageWidth = iVideoWidth;
				m_geometry.ImageHeight = iVideoHeight;
				m_geometry.ScreenWidth = nw;
				m_geometry.ScreenHeight = nh;
				m_geometry.ARType = GUIGraphicsContext.ARType;
				m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
				m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);
				rDest.X += (int)x;
				rDest.Y += (int)y;

				if (!Vmr9.IsVMR9Connected)
				{					
					if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
					if (rDest.Left <0 || rDest.Top < 0 || rDest.Width<=0 || rDest.Height<=0) return;

					Log.Write("overlay: video WxH  : {0}x{1}",iVideoWidth,iVideoHeight);
					Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
					Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
					Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
					Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
					Log.Write("overlay: src        : ({0},{1})-({2},{3})",
						rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
					Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
						rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);


					m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
					m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
					m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
				}
			}
			else
			{
				if (!Vmr9.IsVMR9Connected)
				{
					if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
						GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
					if (iVideoHeight<=0 || iVideoWidth<=0) return;

					m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
					m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
					m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				}

			}
		}
		
		/// <summary>
		/// This method can be used to ask the graph if it should be rebuild when
		/// we want to tune to the new channel:ichannel
		/// </summary>
		/// <param name="iChannel">new channel to tune to</param>
		/// <returns>true : graph needs to be rebuild for this channel
		///          false: graph does not need to be rebuild for this channel
		/// </returns>
		public bool ShouldRebuildGraph(int iChannel)
		{
			return false;
		}

		#region Stream-Audio handling
		public int GetAudioLanguage()
		{
			return currentTuningObject.AudioPid;
		}
		public void SetAudioLanguage(int audioPid)
		{
			if(audioPid!=currentTuningObject.AudioPid)
			{
				int hr=SetupDemuxer(m_DemuxVideoPin,currentTuningObject.VideoPid,m_DemuxAudioPin,audioPid);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: SetupDemuxer FAILED: errorcode {0}",hr.ToString());
					return;
				}
				else
				{
					currentTuningObject.AudioPid=audioPid;
				}
			}
		}
		public ArrayList GetAudioLanguageList()
		{
			if (currentTuningObject==null) return new ArrayList();
			DVBSections.AudioLanguage al;
			ArrayList audioPidList = new ArrayList();
			if(currentTuningObject.AudioPid!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.AudioPid;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio1!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio1;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage1;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio2!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio2;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage2;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio3!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio3;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage3;
				audioPidList.Add(al);
			}
			return audioPidList;
		}
		#endregion

		public bool HasTeletext()
		{
			if (m_graphState!= State.TimeShifting && m_graphState!=State.Recording && m_graphState!=State.Viewing) return false;
			if (currentTuningObject==null) return false;
			if (currentTuningObject.TeletextPid>0) return true;
			return false;
		}
		/// <summary>
		/// Returns the current tv channel
		/// </summary>
		/// <returns>Current channel</returns>
		public int GetChannelNumber()
		{
			return m_iCurrentChannel;
		}

		/// <summary>
		/// Property indiciating if the graph supports timeshifting
		/// </summary>
		/// <returns>boolean indiciating if the graph supports timeshifting</returns>
		public bool SupportsTimeshifting()
		{
			return m_bIsUsingMPEG;
		}

		/// <summary>
		/// Add preferred mpeg video/audio codecs to the graph
		/// the user has can specify these codecs in the setup
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		/// <summary>
		/// returns true if tuner is locked to a frequency and signalstrength/quality is > 0
		/// </summary>
		/// <returns>
		/// true: tuner has a signal and is locked
		/// false: tuner is not locked
		/// </returns>
		/// <remarks>
		/// Graph should be created and GetTunerSignalStatistics() should be called
		/// </remarks>
		public bool SignalPresent()
		{
			//if we dont have an IBDA_SignalStatistics interface then return
			if (m_TunerStatistics==null) return false;
			bool isTunerLocked		= false;
			bool isSignalPresent	= false;
			long signalQuality=0;

			for (int i = 0; i < m_TunerStatistics.Length; i++) 
			{
				bool isLocked=false;
				bool isPresent=false;
				try
				{
					//is the tuner locked?
					m_TunerStatistics[i].get_SignalLocked(ref isLocked);
					isTunerLocked |= isLocked;
				}
				catch (COMException)
				{
				}
				try
				{
					//is a signal present?
					m_TunerStatistics[i].get_SignalPresent(ref isPresent);
					isSignalPresent |= isPresent;
				}
				catch (COMException)
				{
				}
				try
				{
					//is a signal quality ok?
					long quality=0;
					m_TunerStatistics[i].get_SignalQuality(ref quality); //1-100
					if (quality>0) signalQuality += quality;
				}
				catch (COMException)
				{
				}
			}

			//some devices give different results about signal status
			//on some signalpresent is only true when tuned to a channel
			//on others  signalpresent is true when tuned to a transponder
			//so we just look if any variables returns true
			//	Log.WriteFile(Log.LogType.Capture,"  locked:{0} present:{1} quality:{2}",isTunerLocked ,isSignalPresent ,signalQuality); 

			if (isTunerLocked || isSignalPresent || (signalQuality>0) )
			{
				return true;
			}
			return false;
		}//public bool SignalPresent()

		
		public int  SignalQuality()
		{
			if (m_TunerStatistics==null) return 1;
			try
			{
				int signalQuality=1;
				for (int i = 0; i < m_TunerStatistics.Length; i++) 
				{
					long quality=0;
					m_TunerStatistics[i].get_SignalQuality(ref quality); //1-100
					if (quality>signalQuality) signalQuality = (int)quality;
				}
				return signalQuality;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVBGraphBDA: ERROR: exception getting SignalQuality {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
			return 0;
		}
		public int  SignalStrength()
		{
			if (m_TunerStatistics==null) return 1;
			try
			{
				int signalStrength=1;
				for (int i = 0; i < m_TunerStatistics.Length; i++) 
				{
					long strength=0;
					m_TunerStatistics[i].get_SignalStrength(ref strength); //in decibels
					if (strength>signalStrength) signalStrength = (int)strength;
				}
				return signalStrength;
			}
			catch(Exception ex)
			{	
				Log.WriteFile(Log.LogType.Log,true,"DVBGraphBDA: ERROR: exception getting SignalStrength {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
			return 0;
		}
		/// <summary>
		/// not used
		/// </summary>
		/// <returns>-1</returns>
		public long VideoFrequency()
		{
			if (currentTuningObject!=null) return currentTuningObject.Frequency*1000;
			return -1;
		}
		
		public PropertyPageCollection PropertyPages()
		{
			return null;
		}
		
		//not used
		public IBaseFilter AudiodeviceFilter()
		{
			return null;
		}

		//not used
		public bool SupportsFrameSize(Size framesize)
		{	
			return false;
		}

		/// <summary>
		/// return the network type (DVB-T, DVB-C, DVB-S)
		/// </summary>
		/// <returns>network type</returns>
		public NetworkType Network()
		{
			if (m_NetworkType==NetworkType.Unknown)
			{
				if (m_Card.LoadDefinitions())
				{
					foreach (string catName in m_Card.TvFilterDefinitions.Keys)
					{
						FilterDefinition dsFilter = m_Card.TvFilterDefinitions[catName] as FilterDefinition;
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBC Network Provider") 
						{
							m_NetworkType=NetworkType.DVBC;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBT Network Provider") 
						{
							m_NetworkType=NetworkType.DVBT;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBS Network Provider") 
						{
							m_NetworkType=NetworkType.DVBS;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft ATSC Network Provider") 
						{
							m_NetworkType=NetworkType.ATSC;
							return m_NetworkType;
						}
					}
				}
			}
			return m_NetworkType;
		}
		
		/// <summary>
		/// Set the LNB settings for a DVB-S tune request
		/// </summary>
		/// <remarks>Only needed for DVB-S</remarks>
		/// <param name="tuneRequest">IDVBTuneRequest tunerequest for a DVB-S channel</param>
		public IBaseFilter Mpeg2DataFilter()
		{
			return m_SectionsTables;
		}

		#endregion

		#region graph building helper functions
		void AddPreferredCodecs(bool audio, bool video)
		{
			// add preferred video & audio codecs
			string strVideoCodec="";
			string strAudioCodec="";
			string strAudioRenderer="";
			bool   bAddFFDshow=false;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
				strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
				strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
				strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
			}
			if (video && strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strVideoCodec);
			if (audio && strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strAudioCodec);
			if (audio && strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(m_graphBuilder,strAudioRenderer,false);
			if (video && bAddFFDshow) DirectShowUtil.AddFilterToGraph(m_graphBuilder,"ffdshow raw video filter");
		}//void AddPreferredCodecs()

		/// <summary>
		/// This method gets the IBDA_SignalStatistics interface from the tuner
		/// with this interface we can see if the tuner is locked to a signal
		/// and see what the signal strentgh is
		/// </summary>
		/// <returns>
		/// array of IBDA_SignalStatistics or null
		/// </returns>
		/// <remarks>
		/// Graph should be created
		/// </remarks>
		IBDA_SignalStatistics[] GetTunerSignalStatistics()
		{
			//no tuner filter? then return;
			if (m_TunerDevice==null) 
				return null;
			
			//get the IBDA_Topology from the tuner device
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: get IBDA_Topology");
			IBDA_Topology topology = m_TunerDevice as IBDA_Topology;
			if (topology==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: could not get IBDA_Topology from tuner");
				return null;
			}

			//get the NodeTypes from the topology
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: GetNodeTypes");
			int nodeTypeCount=0;
			int[] nodeTypes = new int[33];
			Guid[] guidInterfaces = new Guid[33];
			
			int hr=topology.GetNodeTypes(ref nodeTypeCount, 32, nodeTypes);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not get node types from tuner");
				return null;
			}
			IBDA_SignalStatistics[] signal = new IBDA_SignalStatistics[nodeTypeCount];
			//for each node type
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: got {0} node types", nodeTypeCount);
			for (int i=0; i < nodeTypeCount;++i)
			{
				object objectNode;
				hr=topology.GetControlNode(0,1, nodeTypes[i], out objectNode);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not GetControlNode for node:{0}",hr);
					return null;
				}
				//and get the final IBDA_SignalStatistics
				try
				{
					signal[i] = (IBDA_SignalStatistics) objectNode;
				}
				catch 
				{
					Log.WriteFile(Log.LogType.Capture,"No interface on node {0}", i); 
				}
			}//for (int i=0; i < nodeTypeCount;++i)
			Marshal.ReleaseComObject(topology);
			return signal;
		}//IBDA_SignalStatistics GetTunerSignalStatistics()

		IBDA_LNBInfo[] GetBDALNBInfoInterface()
		{
			//no tuner filter? then return;
			if (m_TunerDevice==null) 
				return null;
			
			//get the IBDA_Topology from the tuner device
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: get IBDA_Topology");
			IBDA_Topology topology = m_TunerDevice as IBDA_Topology;
			if (topology==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: could not get IBDA_Topology from tuner");
				return null;
			}

			//get the NodeTypes from the topology
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: GetNodeTypes");
			int nodeTypeCount=0;
			int[] nodeTypes = new int[33];
			Guid[] guidInterfaces = new Guid[33];
			
			int hr=topology.GetNodeTypes(ref nodeTypeCount, 32, nodeTypes);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not get node types from tuner");
				return null;
			}
			IBDA_LNBInfo[] signal = new IBDA_LNBInfo[nodeTypeCount];
			//for each node type
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: got {0} node types", nodeTypeCount);
			for (int i=0; i < nodeTypeCount;++i)
			{
				object objectNode;
				hr=topology.GetControlNode(0,1, nodeTypes[i], out objectNode);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not GetControlNode for node:{0}",hr);
					return null;
				}
				//and get the final IBDA_LNBInfo
				try
				{
					signal[i] = (IBDA_LNBInfo)objectNode ;
				}
				catch 
				{
					Log.WriteFile(Log.LogType.Capture,"No interface on node {0}", i); 
				}
			}//for (int i=0; i < nodeTypeCount;++i)
			Marshal.ReleaseComObject(topology);
			return signal;
		}//IBDA_LNBInfo[] GetBDALNBInfoInterface()


		private bool CreateSinkSource(string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			
			int		hr				= 0;
			IPin	pinObj0		= null;
			IPin	pinObj1		= null;
			IPin	pinObj2		= null;
			IPin	pinObj3		= null;
			IPin	outPin		= null;

			try
			{
				int iTimeShiftBuffer=30;
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					iTimeShiftBuffer= xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
					if (iTimeShiftBuffer<5) iTimeShiftBuffer=5;
				}
				iTimeShiftBuffer*=60; //in seconds
				int iFileDuration = iTimeShiftBuffer/6;

				//create StreamBufferSink filter
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateSinkSource()");
				hr=m_graphBuilder.AddFilter((IBaseFilter)m_StreamBufferSink,"StreamBufferSink");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add StreamBufferSink");
					return false;
				}			
				//create MPEG2 Analyzer filter
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Add mpeg2 analyzer()");
				hr=m_graphBuilder.AddFilter((IBaseFilter)m_mpeg2Analyzer,"Mpeg2 Analyzer");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add mpeg2 analyzer to graph");
					return false;
				}

				//get input pin of MPEG2 Analyzer filter
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:find mpeg2 analyzer input pin()");
				pinObj0=DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer,PinDirection.Input,0);
				if(pinObj0 == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find mpeg2 analyzer input pin");
					return false;
				}
				
				//connect mpeg2 demuxer video out->mpeg2 analyzer input pin
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect demux video output->mpeg2 analyzer");
				hr=m_graphBuilder.Connect(m_DemuxVideoPin, pinObj0) ;
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect demux video output->mpeg2 analyzer");
					return false;
				}

				//get output pin #0 from MPEG2 analyzer Filter
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:mpeg2 analyzer output->streambuffersink in");
				pinObj1 = DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer, PinDirection.Output, 0);	
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find mpeg2 analyzer output pin");
					return false;
				}
				
				//get input pin #0 from StreamBufferSink Filter
				pinObj2 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 0);	
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find SBE input pin");
					return false;
				}

				//connect MPEG2 analyzer output pin->StreamBufferSink Filter input pin
				hr=m_graphBuilder.Connect(pinObj1, pinObj2) ;
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 analyzer->streambuffer sink");
					return false;
				}

				//Get StreamBufferSink InputPin #1
				pinObj3 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);	
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find SBE input pin#2");
					return false;
				}
				//connect MPEG2 demuxer audio output ->StreamBufferSink Input #1
				hr=m_graphBuilder.Connect(m_DemuxAudioPin, pinObj3) ;
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 demuxer audio out->streambuffer sink in#2");
					return false;
				}

				int ipos=fileName.LastIndexOf(@"\");
				string strDir=fileName.Substring(0,ipos);
				m_StreamBufferConfig	= new StreamBufferConfig();
				m_IStreamBufferConfig	= (IStreamBufferConfigure) m_StreamBufferConfig;
				
				// setting the StreamBufferEngine registry key
				IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_IStreamBufferConfig;
			IntPtr subKey = IntPtr.Zero;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pTemp.SetHKEY(subKey);
				
				//set timeshifting folder
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set timeshift folder to:{0}", strDir);
				hr = m_IStreamBufferConfig.SetDirectory(strDir);	
				if(hr != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshift folder to:{0}", strDir);
					return false;
				}

				//set number of timeshifting files
				hr = m_IStreamBufferConfig.SetBackingFileCount(6, 8);    //4-6 files
				if(hr != 0)
					return false;
				
				//set duration of each timeshift file
				hr = m_IStreamBufferConfig.SetBackingFileDuration((uint)iFileDuration); // 60sec * 4 files= 4 mins
				if(hr != 0)
					return false;

			subKey = IntPtr.Zero;
			HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_StreamBufferSink;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr = pConfig.SetHKEY(subKey);
				//set timeshifting filename
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set timeshift file to:{0}", fileName);
				
				// lock on the 'filename' file
				if(m_IStreamBufferSink.LockProfile(fileName) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshift file to:{0}", fileName);
					return false;
				}
			}
			finally
			{
				if(pinObj0 != null)
					Marshal.ReleaseComObject(pinObj0);
				if(pinObj1 != null)
					Marshal.ReleaseComObject(pinObj1);
				if(pinObj2 != null)
					Marshal.ReleaseComObject(pinObj2);
				if(pinObj3 != null)
					Marshal.ReleaseComObject(pinObj3);
				if(outPin != null)
					Marshal.ReleaseComObject(outPin);

				//if ( streamBufferInitialize !=null)
					//Marshal.ReleaseComObject(streamBufferInitialize );
				
			}
//			(m_graphBuilder as IMediaFilter).SetSyncSource(m_MPEG2Demultiplexer as IReferenceClock);
			return true;
		}//private bool CreateSinkSource(string fileName)

		/// <summary>
		/// Finds and connects pins
		/// </summary>
		/// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
		/// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
		/// <returns>true if succeeded, false if failed</returns>
		private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter) 
		{
			return ConnectFilters(ref UpstreamFilter, ref DownstreamFilter, 0);
		}//bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter) 

		/// <summary>
		/// Finds and connects pins
		/// </summary>
		/// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
		/// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
		/// <param name="preferredOutputPin">The one-based index of the preferred output pin to use on the Upstream filter.  This is tried first. Pin 1 = 1, Pin 2 = 2, etc</param>
		/// <returns>true if succeeded, false if failed</returns>
		private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin) 
		{
			if (UpstreamFilter == null || DownstreamFilter == null)
				return false;

			int ulFetched = 0;
			int hr = 0;
			IEnumPins pinEnum;

			hr = UpstreamFilter.EnumPins( out pinEnum );
			if((hr < 0) || (pinEnum == null))
				return false;

			#region Attempt to connect preferred output pin first
			if (preferredOutputPin > 0) 
			{
				IPin[] outPin = new IPin[1];
				int outputPinCounter = 0;
				while(pinEnum.Next(1, outPin, out ulFetched) == 0) 
				{    
					PinDirection pinDir;
					outPin[0].QueryDirection(out pinDir);

					if (pinDir == PinDirection.Output)
					{
						outputPinCounter++;
						if (outputPinCounter == preferredOutputPin) // Go and find the input pin.
						{
							IEnumPins downstreamPins;

							DownstreamFilter.EnumPins(out downstreamPins);

							IPin[] dsPin = new IPin[1];
							while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
							{
								PinDirection dsPinDir;
								dsPin[0].QueryDirection(out dsPinDir);
								if (dsPinDir == PinDirection.Input)
								{
									hr = m_graphBuilder.Connect(outPin[0], dsPin[0]);
									if(hr != 0) 
									{
										Marshal.ReleaseComObject(dsPin[0]);
										break;
									} 
									else 
									{
										return true;
									}
								}
							}
							Marshal.ReleaseComObject(downstreamPins);
						}
					}
					Marshal.ReleaseComObject(outPin[0]);
				}
				pinEnum.Reset();        // Move back to start of enumerator
			}
			#endregion

			IPin[] testPin = new IPin[1];
			while(pinEnum.Next(1, testPin, out ulFetched) == 0) 
			{    
				PinDirection pinDir;
				testPin[0].QueryDirection(out pinDir);

				if(pinDir == PinDirection.Output) // Go and find the input pin.
				{
					IEnumPins downstreamPins;

					DownstreamFilter.EnumPins(out downstreamPins);

					IPin[] dsPin = new IPin[1];
					while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
					{
						PinDirection dsPinDir;
						dsPin[0].QueryDirection(out dsPinDir);
						if (dsPinDir == PinDirection.Input)
						{
							hr = m_graphBuilder.Connect(testPin[0], dsPin[0]);
							if(hr != 0) 
							{
								Marshal.ReleaseComObject(dsPin[0]);
								continue;
							} 
							else 
							{
								return true;
							}
						}//if (dsPinDir == PinDirection.Input)
					}//while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
					Marshal.ReleaseComObject(downstreamPins);
				}//if(pinDir == PinDirection.Output) // Go and find the input pin.
				Marshal.ReleaseComObject(testPin[0]);
			}//while(pinEnum.Next(1, testPin, out ulFetched) == 0) 
			Marshal.ReleaseComObject(pinEnum);
			return false;
		}//private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin) 

		/// <summary>
		/// This is the function for setting up a local tuning space.
		/// </summary>
		/// <returns>true if succeeded, fale if failed</returns>
		private bool setupTuningSpace() 
		{
			//int hr = 0;

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: setupTuningSpace()");
			if(m_NetworkProvider == null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:network provider is null ");
				return false;
			}
			System.Guid classID;
			int hr=m_NetworkProvider.GetClassID(out classID);
			//			if (hr <=0)
			//			{
			//				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:cannot get classid of network provider");
			//				return false;
			//			}

			string strClassID = classID.ToString();
			strClassID = strClassID.ToLower();
			switch (strClassID) 
			{
				case "0dad2fdd-5fd7-11d3-8f50-00c04f7971e2":
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=ATSC");
					m_NetworkType = NetworkType.ATSC;
					break;
				case "dc0c0fe7-0485-4266-b93f-68fbf80ed834":
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-C");
					m_NetworkType = NetworkType.DVBC;
					break;
				case "fa4b375a-45b4-4d45-8440-263957b11623":
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-S");
					m_NetworkType = NetworkType.DVBS;
					break;
				case "216c62df-6d7f-4e9a-8571-05f14edb766a":
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-T");
					m_NetworkType = NetworkType.DVBT;
					break;
				default:
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:unknown network type:{0} ",classID);
					return false;
			}//switch (strClassID) 

			TunerLib.ITuningSpaceContainer TuningSpaceContainer = (TunerLib.ITuningSpaceContainer) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_SystemTuningSpaces, true));
			if(TuningSpaceContainer == null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Failed to get ITuningSpaceContainer");
				return false;
			}

			TunerLib.ITuningSpaces myTuningSpaces = null;
			string uniqueName="";
			switch (m_NetworkType) 
			{
				case NetworkType.ATSC:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_ATSCTuningSpace);
					//ATSCInputType = "Antenna"; // Need to change to allow cable
					uniqueName="Mediaportal ATSC";
				} break;
				case NetworkType.DVBC:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
					uniqueName="Mediaportal DVB-C";
				} break;
				case NetworkType.DVBS:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBSTuningSpace);
					uniqueName="Mediaportal DVB-S";
				} break;
				case NetworkType.DVBT:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
					uniqueName="Mediaportal DVB-T";
				} break;
			}//switch (m_NetworkType) 

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: check available tuningspaces");
			TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;

			int Count = 0;
			Count = myTuningSpaces.Count;
			if(Count > 0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found {0} tuning spaces", Count);
				TunerLib.IEnumTuningSpaces TuneEnum = myTuningSpaces.EnumTuningSpaces;
				if (TuneEnum !=null)
				{
					uint ulFetched = 0;
					TunerLib.TuningSpace tuningSpaceFound;
					int counter = 0;
					TuneEnum.Reset();
					for (counter=0; counter < Count; counter++)
					{
						TuneEnum.Next(1, out tuningSpaceFound, out ulFetched);
						if (ulFetched==1 )
						{
							if (tuningSpaceFound.UniqueName==uniqueName)
							{
								myTuner.TuningSpace = tuningSpaceFound;
								Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: used tuningspace:{0} {1} {2}", counter, tuningSpaceFound.UniqueName,tuningSpaceFound.FriendlyName);
								if (myTuningSpaces!=null)
									Marshal.ReleaseComObject(myTuningSpaces);
								if (TuningSpaceContainer!=null)
									Marshal.ReleaseComObject(TuningSpaceContainer);
								return true;
							}//if (tuningSpaceFound.UniqueName==uniqueName)
						}//if (ulFetched==1 )
					}//for (counter=0; counter < Count; counter++)
					if (myTuningSpaces!=null)
						Marshal.ReleaseComObject(myTuningSpaces);
				}//if (TuneEnum !=null)
			}//if(Count > 0)

			TunerLib.ITuningSpace TuningSpace ;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new tuningspace");
			switch (m_NetworkType) 
			{
				case NetworkType.ATSC: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_ATSCTuningSpace, true));
					TunerLib.IATSCTuningSpace myTuningSpace = (TunerLib.IATSCTuningSpace) TuningSpace;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_ATSCNetworkProvider);
					myTuningSpace.InputType = TunerLib.tagTunerInputType.TunerInputAntenna;
					myTuningSpace.MaxChannel			= 10000;
					myTuningSpace.MaxMinorChannel		= 1;
					myTuningSpace.MaxPhysicalChannel	= 10000;
					myTuningSpace.MinChannel			= 1;
					myTuningSpace.MinMinorChannel		= 0;
					myTuningSpace.MinPhysicalChannel	= 0;
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;

					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_ATSCLocator, true));
					TunerLib.IATSCLocator myLocator = (TunerLib.IATSCLocator) DefaultLocator;

					myLocator.CarrierFrequency	 = -1;
					myLocator.InnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation				= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OuterFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.PhysicalChannel		= -1;
					myLocator.SymbolRate				= -1;
					myLocator.TSID							= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.ATSC: 
				
				case NetworkType.DVBC: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBTuningSpace, true));
					TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Cable;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBCNetworkProvider);

					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;
					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBCLocator, true));
					TunerLib.IDVBCLocator myLocator = (TunerLib.IDVBCLocator) DefaultLocator;

					myLocator.CarrierFrequency	= -1;
					myLocator.InnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation				= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OuterFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.SymbolRate				= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.DVBC: 
				
				case NetworkType.DVBS: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBSTuningSpace, true));
					TunerLib.IDVBSTuningSpace myTuningSpace = (TunerLib.IDVBSTuningSpace) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Satellite;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBSNetworkProvider);
					myTuningSpace.LNBSwitch = -1;
					myTuningSpace.HighOscillator = -1;
					myTuningSpace.LowOscillator = 11250000;
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;
					
					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBSLocator, true));
					TunerLib.IDVBSLocator myLocator = (TunerLib.IDVBSLocator) DefaultLocator;
					
					myLocator.CarrierFrequency		= -1;
					myLocator.InnerFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.OuterFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation					= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.SymbolRate					= -1;
					myLocator.Azimuth							= -1;
					myLocator.Elevation						= -1;
					myLocator.OrbitalPosition			= -1;
					myLocator.SignalPolarisation	= (TunerLib.Polarisation) Polarisation.BDA_POLARISATION_NOT_SET;
					myLocator.WestPosition				= false;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.DVBS: 
				
				case NetworkType.DVBT: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBTuningSpace, true));
					TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Terrestrial;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBTNetworkProvider);
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;

					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBTLocator, true));
					TunerLib.IDVBTLocator myLocator = (TunerLib.IDVBTLocator) DefaultLocator;

					myLocator.CarrierFrequency		= -1;
					myLocator.Bandwidth						= -1;
					myLocator.Guard								= (TunerLib.GuardInterval) GuardInterval.BDA_GUARD_NOT_SET;
					myLocator.HAlpha							= (TunerLib.HierarchyAlpha) HierarchyAlpha.BDA_HALPHA_NOT_SET;
					myLocator.InnerFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.LPInnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.LPInnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Mode								= (TunerLib.TransmissionMode) TransmissionMode.BDA_XMIT_MODE_NOT_SET;
					myLocator.Modulation					= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OtherFrequencyInUse	= false;
					myLocator.OuterFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.SymbolRate					= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;

				} break;//case NetworkType.DVBT: 
			}//switch (m_NetworkType) 
			return true;
		}//private bool setupTuningSpace() 

		/// <summary>
		/// Used to find the Network Provider for addition to the graph.
		/// </summary>
		/// <param name="ClassID">The filter category to enumerate.</param>
		/// <param name="FriendlyName">An identifier based on the DevicePath, used to find the device.</param>
		/// <param name="device">The filter that has been found.</param>
		/// <returns>true of succeeded, false if failed</returns>
		private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 
		{
			int hr;
			ICreateDevEnum		sysDevEnum	= null;
			UCOMIEnumMoniker	enumMoniker	= null;
			
			sysDevEnum = (ICreateDevEnum) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum, true));
			// Enumerate the filter category
			hr = sysDevEnum.CreateClassEnumerator(ref ClassID, out enumMoniker, 0);
			if( hr != 0 )
				throw new NotSupportedException( "No devices in this category" );

			int ulFetched = 0;
			UCOMIMoniker[] deviceMoniker = new UCOMIMoniker[1];
			while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
			{
				object bagObj = null;
				Guid bagId = typeof( IPropertyBag ).GUID;
				deviceMoniker[0].BindToStorage(null, null, ref bagId, out bagObj);
				IPropertyBag propBag = (IPropertyBag) bagObj;
				object val = "";
				propBag.Read("FriendlyName", ref val, IntPtr.Zero); 
				string Name = val as string;
				val = "";
				if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
				{
					object filterObj = null;
					System.Guid filterID = typeof(IBaseFilter).GUID;
					deviceMoniker[0].BindToObject(null, null, ref filterID, out filterObj);
					device = filterObj;
					
					filterObj = null;
					if(device == null) 
					{
						continue;
					} 
					else 
					{
						return true;
					}
				}//if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
				Marshal.ReleaseComObject(deviceMoniker[0]);
			}//while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
			device = null;
			return false;
		}//private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 

		#endregion
		#region process helper functions
		// send PMT to firedtv device
		bool SendPMT()
		{
			VideoCaptureProperties props = new VideoCaptureProperties(m_TunerDevice);
			try
			{

				string pmtName=String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
					Utils.FilterFileName(currentTuningObject.ServiceName),
					currentTuningObject.NetworkID,
					currentTuningObject.TransportStreamID,
					currentTuningObject.ProgramNumber,
					(int)Network());
				if (!System.IO.File.Exists(pmtName))
				{
					return false;
				}
					
				System.IO.FileStream stream = new System.IO.FileStream(pmtName,System.IO.FileMode.Open,System.IO.FileAccess.Read,System.IO.FileShare.None);
				long len=stream.Length;
				if (len>6)
				{
					byte[] pmt = new byte[len];
					stream.Read(pmt,0,(int)len);
					stream.Close();

					int pmtVersion= ((pmt[5]>>1)&0x1F);

					//yes, then send the PMT table to the device
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Process() send PMT version {0} to fireDTV device",pmtVersion);	
					if (props.SendPMTToFireDTV(pmt, (int)len))
					{
						return true;
					}
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ERROR: exception while sending pmt {0} {1} {2}",
							ex.Message,ex.Source,ex.StackTrace);
			}
			return false;
		}//SendPMT()

		void LoadLNBSettings(ref DVBChannel ch, int disNo)
		{
			/*
			try
			{
				string filename=String.Format(@"database\card_{0}.xml",m_Card.FriendlyName);

				int lnbKhz=0;
				int lnbKhzVal=0;
				int diseqc=0;
				int lnbKind=0;
				// lnb config
				int lnb0MHZ=0;
				int lnb1MHZ=0;
				int lnbswMHZ=0;
				int cbandMHZ=0;
				int circularMHZ=0;

				using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(filename))
				{
					lnb0MHZ=xmlreader.GetValueAsInt("dvbs","LNB0",9750);
					lnb1MHZ=xmlreader.GetValueAsInt("dvbs","LNB1",10600);
					lnbswMHZ=xmlreader.GetValueAsInt("dvbs","Switch",11700);
					cbandMHZ=xmlreader.GetValueAsInt("dvbs","CBand",5150);
					circularMHZ=xmlreader.GetValueAsInt("dvbs","Circular",10750);
					switch(disNo)
					{
						case 1:
							// config a
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind",0);
							break;
						case 2:
							// config b
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb2",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc2",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind2",0);
							break;
						case 3:
							// config c
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb3",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc3",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind3",0);
							break;
							//
						case 4:
							// config d
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb4",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc4",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind4",0);
							//
							break;
					}// switch(disNo)
					switch (lnbKhz)
					{
						case 0: lnbKhzVal=0;break;
						case 22: lnbKhzVal=1;break;
						case 33: lnbKhzVal=2;break;
						case 44: lnbKhzVal=3;break;
					}


				}//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))

				// set values to dvbchannel-object
				ch.DiSEqC=diseqc;
				// set the lnb parameter 
				if(ch.Frequency>=lnbswMHZ*1000)
				{
					ch.LNBFrequency=lnb1MHZ;
					ch.LNBKHz=lnbKhzVal;
				}
				else
				{
					ch.LNBFrequency=lnb0MHZ;
					ch.LNBKHz=0;
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: LNB Settings: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}",ch.Frequency,ch.LNBKHz,ch.LNBFrequency,ch.DiSEqC); 
			}
			catch(Exception)
			{
			}*/
		} //void LoadLNBSettings(TunerLib.IDVBTuneRequest tuneRequest)
		
		void SetLNBSettings(TunerLib.IDVBTuneRequest tuneRequest)
		{
			/*
			try
			{
				if (tuneRequest==null) return;
				if (tuneRequest.TuningSpace==null) return;
				TunerLib.IDVBSTuningSpace space = tuneRequest.TuningSpace as TunerLib.IDVBSTuningSpace;
				if (space==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: cannot get IDVBSTuningSpace in SetLNBSettings()");
					return ;
				}
			//
			//LOWORD -> LOBYTE -> Bit0 for Position (0-A,1-B)
			//HIWORD -> LOBYTE -> Bit0 for Option   (0-A,1-B)
			//LOWORD -> HIBYTE -> Bit0 for 22Khz    (0-Off,1-On)
			//HIWORD -> HIBYTE -> Bit0 for Burst    (0-Off,1-On)
				long inputRange=0;
				switch (currentTuningObject.DiSEqC)
				{
					case 0: //none
						return; 
					case 1: //simple A
						return; 
					case 2: //simple B
						return;
					case 3: //Level 1 A/A
						inputRange=0;
					break;
					case 4: //Level 1 B/A
						inputRange=1;
					break;
					case 5: //Level 1 A/B
						inputRange=1<<16;
					break;
					case 6: //Level 1 B/B
						inputRange=(1<<16)+1;
					break;
				}
				// test with burst on
				//inputRange|=1<<24;

				if (currentTuningObject.LNBKHz==1) // 22khz 
					inputRange |= (1<<8);

				space.InputRange=inputRange.ToString();
			}
			catch(Exception)
			{
			}*/
		}

		void CheckVideoResolutionChanges()
		{
			if (m_graphState != State.Viewing) return ;
			if (m_videoWindow==null || m_basicVideo==null) return;
			int aspectX, aspectY;
			int videoWidth=1, videoHeight=1;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetVideoSize(out videoWidth, out videoHeight);
			}
			aspectX=videoWidth;
			aspectY=videoHeight;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			if (videoHeight!=m_iVideoHeight || videoWidth != m_iVideoWidth ||
				aspectX != m_aspectX || aspectY != m_aspectY)
			{
				GUIGraphicsContext_OnVideoWindowChanged();
			}
		}

		public void Process()
		{
			if (m_SectionsTables==null) return;
			if(!GUIGraphicsContext.Vmr9Active && !g_Player.Playing)
			{
				CheckVideoResolutionChanges();
			}
			if(GUIGraphicsContext.Vmr9Active && Vmr9!=null)
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
					TimeSpan ts = DateTime.Now-timeResendPid;
					if (ts.TotalSeconds>5)
					{
						refreshPmtTable=true;
						timeResendPid=DateTime.Now;
					}
				}
				else timeResendPid=DateTime.Now;
			}
			else timeResendPid=DateTime.Now;
			
			if (!refreshPmtTable) return;

			try
			{
				SetupDemuxer(m_DemuxVideoPin,currentTuningObject.VideoPid,m_DemuxAudioPin,currentTuningObject.AudioPid);
				if (!SendPMT())
				{
					return;
				}
			}
			catch(Exception)
			{
			}
			finally
			{
				refreshPmtTable=false;
			}
		}//public void Process()

#endregion


		#region Tuning
		/// <summary>
		/// Switches / tunes to another TV channel
		/// </summary>
		/// <param name="iChannel">New channel</param>
		/// <remarks>
		/// Graph should be viewing or timeshifting. 
		/// </remarks>
		public void TuneChannel(TVChannel channel)
		{
			if (m_NetworkProvider==null) return;

			try
			{

				m_iPrevChannel		= m_iCurrentChannel;
				m_iCurrentChannel = channel.Number;
				m_StartTime				= DateTime.Now;
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() tune to channel:{0}", channel.ID);

				//get the ITuner interface from the network provider filter
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner==null) return;


				TunerLib.IATSCTuningSpace myAtscTuningSpace =null;
				TunerLib.IDVBTuningSpace2 myTuningSpace =null;
				if (Network()==NetworkType.ATSC)
				{
					//get the IATSCTuningSpace from the tuner
					myAtscTuningSpace = myTuner.TuningSpace as TunerLib.IATSCTuningSpace;
					if (myAtscTuningSpace ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed TuneChannel() tuningspace=null");
						return;
					}

					//create a new tuning request
					newTuneRequest = myAtscTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed TuneChannel() could not create new tuningrequest");
						return;
					}
				}
				else
				{
					//get the IDVBTuningSpace2 from the tuner
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBTuningSpace2");
					myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. Invalid tuningspace");
						return ;
					}


					//create a new tuning request
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() create new tuningrequest");
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}


				TunerLib.IDVBTuneRequest myTuneRequest=null;
				TunerLib.IATSCChannelTuneRequest myATSCTuneRequest=null;
				if (m_NetworkType!=NetworkType.ATSC)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IDVBTuneRequest");
					myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IATSCChannelTuneRequest");
					myATSCTuneRequest = newTuneRequest as TunerLib.IATSCChannelTuneRequest;
					if (myATSCTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}

				
				int bandWidth=-1;
				int frequency=-1,ONID=-1,TSID=-1,SID=-1;
				int audioPid=-1, videoPid=-1, teletextPid=-1, pmtPid=-1;
				string providerName;
				int audio1,audio2,audio3,ac3Pid;
				string audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3;
				
				switch (m_NetworkType)
				{
					case NetworkType.ATSC: 
					{
						//get the ATSC tuning details from the tv database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get ATSC tuning details");
						int symbolrate=0,innerFec=0,modulation=0,physicalChannel=0;
						int minorChannel=0,majorChannel=0;
						TVDatabase.GetATSCTuneRequest(channel.ID,out physicalChannel,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out minorChannel,out majorChannel);
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz physicalChannel:{1} symbolrate:{2} innerFec:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8}", 
							frequency,physicalChannel,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);

						//get the IDVBCLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBCLocator interface");
						TunerLib.IATSCLocator myLocator = myATSCTuneRequest.Locator as TunerLib.IATSCLocator;	
						if (myLocator==null)
						{
							myLocator = myAtscTuningSpace.DefaultLocator as TunerLib.IATSCLocator;
						}
						
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get IATSCLocator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						
						
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.PhysicalChannel			= physicalChannel;
						myLocator.SymbolRate				  = symbolrate;
						myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
						myLocator.Modulation					= (TunerLib.ModulationType)modulation;
						myATSCTuneRequest.MinorChannel= minorChannel;
						myATSCTuneRequest.Channel		  = majorChannel;
						myATSCTuneRequest.Locator=(TunerLib.Locator)myLocator;
						currentTuningObject=new DVBChannel();
						currentTuningObject.PhysicalChannel=physicalChannel;
						currentTuningObject.MinorChannel=minorChannel;
						currentTuningObject.MajorChannel=majorChannel;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						Marshal.ReleaseComObject(myATSCTuneRequest);

					} break;
					
					case NetworkType.DVBC: 
					{
						//get the DVB-C tuning details from the tv database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBC tuning details");
						int symbolrate=0,innerFec=0,modulation=0;
						TVDatabase.GetDVBCTuneRequest(channel.ID,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							frequency,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);

						//get the IDVBCLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBCLocator interface");
						TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						
						
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.SymbolRate				  = symbolrate;
						myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
						myLocator.Modulation					= (TunerLib.ModulationType)modulation;
						myTuneRequest.ONID	= ONID;					//original network id
						myTuneRequest.TSID	= TSID;					//transport stream id
						myTuneRequest.SID		= SID;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						Marshal.ReleaseComObject(myTuneRequest);


					} break;

					case NetworkType.DVBS: 
					{					
						//get the DVB-S tuning details from the tv database
						//for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBS tuning details");
						DVBChannel ch=new DVBChannel();
						if(TVDatabase.GetSatChannel(channel.ID,1,ref ch)==false)//only television
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							ch.Frequency,ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID,ch.ProgramNumber,ch.ServiceProvider);

						//get the IDVBSLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBSLocator");
						TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBSLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= ch.Frequency;
						if (ch.Polarity==0) 
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
						else
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;

						myLocator.SymbolRate= ch.Symbolrate;
						myLocator.InnerFEC	= (TunerLib.FECMethod)ch.FEC;
						myTuneRequest.ONID	= ch.NetworkID;		//original network id
						myTuneRequest.TSID	= ch.TransportStreamID;		//transport stream id
						myTuneRequest.SID		= ch.ProgramNumber;		//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						

						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=ch.Frequency;
						currentTuningObject.Symbolrate=ch.Symbolrate;
						currentTuningObject.FEC=ch.FEC;
						currentTuningObject.Polarity=ch.Polarity;
						currentTuningObject.NetworkID=ch.NetworkID;
						currentTuningObject.TransportStreamID=ch.TransportStreamID;
						currentTuningObject.ProgramNumber=ch.ProgramNumber;
						currentTuningObject.AudioPid=ch.AudioPid;
						currentTuningObject.VideoPid=ch.VideoPid;
						currentTuningObject.TeletextPid=ch.TeletextPid;
						currentTuningObject.PMTPid=ch.PMTPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=ch.AudioLanguage;
						currentTuningObject.AudioLanguage1=ch.AudioLanguage1;
						currentTuningObject.AudioLanguage2=ch.AudioLanguage2;
						currentTuningObject.AudioLanguage3=ch.AudioLanguage3;
						currentTuningObject.AC3Pid=ch.AC3Pid;
						currentTuningObject.Audio1=ch.Audio1;
						currentTuningObject.Audio2=ch.Audio2;
						currentTuningObject.Audio3=ch.Audio3;
						currentTuningObject.DiSEqC=ch.DiSEqC;
						currentTuningObject.LNBFrequency=ch.LNBFrequency;
						currentTuningObject.LNBKHz=ch.LNBKHz;
						SetLNBSettings(myTuneRequest);
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						Marshal.ReleaseComObject(myTuneRequest);

					} break;

					case NetworkType.DVBT: 
					{
						//get the DVB-T tuning details from the tv database
						//for DVB-T this is the frequency, ONID , TSID and SID
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBT tuning details");
						TVDatabase.GetDVBTTuneRequest(channel.ID,out providerName,out frequency, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,"true,DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID,providerName);
						//get the IDVBTLocator interface from the new tuning request

						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBTLocator");
						TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot get locator", frequency,ONID,TSID,SID);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.Bandwidth=bandWidth;
						myTuneRequest.ONID	= ONID;					//original network id
						myTuneRequest.TSID	= TSID;					//transport stream id
						myTuneRequest.SID		= SID;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;

						currentTuningObject=new DVBChannel();
						currentTuningObject.Bandwidth=bandWidth;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						Marshal.ReleaseComObject(myTuneRequest);

					} break;
				}	//switch (m_NetworkType)
				//submit tune request to the tuner
			


				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(currentTuningObject.AudioPid, currentTuningObject.VideoPid, currentTuningObject.TeletextPid, currentTuningObject.Audio3, currentTuningObject.ServiceName,currentTuningObject.PMTPid);
					m_streamDemuxer.GetEPGSchedule(0x50,currentTuningObject.ProgramNumber);
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map pid {0} to audio, pid {1} to video",currentTuningObject.AudioPid, currentTuningObject.VideoPid);
				SetupDemuxer(m_DemuxVideoPin, currentTuningObject.VideoPid, m_DemuxAudioPin,currentTuningObject.AudioPid);
				DirectShowUtil.EnableDeInterlace(m_graphBuilder);
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() done");

				refreshPmtTable	= false;
				SendPMT();

				if(m_pluginsEnabled==true)
					ExecTuner();
			}
			finally
			{
			}
			SetZapOSDData(channel);
			timeResendPid=DateTime.Now;
		}//public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)
		// this sets the channel to render the osd
		void SetZapOSDData(TVChannel channel)
		{
			if(GUIWindowManager.ActiveWindow!=(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
				return;
			if(m_osd!=null && channel!=null && m_useVMR9Zap==true)
			{

				int level=SignalStrength();
				int quality=SignalQuality();
				m_osd.ShowBitmap(m_osd.RenderZapOSD(channel,quality),0.8f);
				
			}
		}

		public void TuneFrequency(int frequency)
		{
		}


		#region AutoTuning
		/// <summary>
		/// Tune to a specific channel
		/// </summary>
		/// <param name="tuningObject">
		/// DVBChannel object containing the tuning parameter.
		/// </param>
		/// <remarks>
		/// Graph should be created 
		/// </remarks>
		public void Tune(object tuningObject, int disecqNo)
		{
			
			try
			{

				//if no network provider then return;
				if (m_NetworkProvider==null) return;
				if (tuningObject		 ==null) return;

				//start viewing if we're not yet viewing
				if (!graphRunning)
				{
					TVChannel chan = new TVChannel();
					chan.Number=-1;
					chan.ID=0;
					StartViewing(chan);
				}
				//get the ITuner from the network provider
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner ==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() tuner=null");
					return;
				}


				TunerLib.IATSCTuningSpace myAtscTuningSpace =null;
				TunerLib.IDVBTuningSpace2 myTuningSpace =null;
				if (Network()==NetworkType.ATSC)
				{
					//get the IATSCTuningSpace from the tuner
					myAtscTuningSpace = myTuner.TuningSpace as TunerLib.IATSCTuningSpace;
					if (myAtscTuningSpace ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() tuningspace=null");
						return;
					}

					//create a new tuning request
					newTuneRequest = myAtscTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not create new tuningrequest");
						return;
					}
				}
				else
				{
					//get the IDVBTuningSpace2 from the tuner
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBTuningSpace2");
					myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. Invalid tuningspace");
						return ;
					}


					//create a new tuning request
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() create new tuningrequest");
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}


				TunerLib.IDVBTuneRequest myTuneRequest=null;
				TunerLib.IATSCChannelTuneRequest myATSCTuneRequest=null;
				if (m_NetworkType!=NetworkType.ATSC)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IDVBTuneRequest");
					myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IATSCChannelTuneRequest");
					myATSCTuneRequest = newTuneRequest as TunerLib.IATSCChannelTuneRequest;
					if (myATSCTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}

				// for DVB-T
				if (Network() == NetworkType.DVBT)
				{
					DVBChannel chan=(DVBChannel)tuningObject;

					
					//get the IDVBTLocator interface
					TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
					if (myLocator == null)
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
					if (myLocator ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not get IDVBTLocator");
						return;
					}
					//set the properties for the new tuning request. For DVB-T we only set the frequency

					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Tune() DVB-T freq:{0} bandwidth:{1}",chan.Frequency,chan.Bandwidth);
					myLocator.CarrierFrequency		= chan.Frequency;
					myLocator.Bandwidth=chan.Bandwidth;
					
					myTuneRequest.ONID						= -1;					//original network id
					myTuneRequest.TSID						= -1;					//transport stream id
					myTuneRequest.SID							= -1;					//service id
					myTuneRequest.Locator					= (TunerLib.Locator)myLocator;
					currentTuningObject = new DVBChannel();
					currentTuningObject.Frequency=chan.Frequency;
					currentTuningObject.NetworkID=-1;
					currentTuningObject.Bandwidth=chan.Bandwidth;
					currentTuningObject.TransportStreamID=-1;
					currentTuningObject.ProgramNumber=-1;
					//and submit the tune request
					myTuner.TuneRequest  = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);

				}//if (Network() == NetworkType.DVBT)
				else if (Network() == NetworkType.ATSC)
				{
					//get the IDVBCLocator interface
					TunerLib.IATSCLocator myLocator = myATSCTuneRequest.Locator as TunerLib.IATSCLocator;	
					if (myLocator == null)
						myLocator = myAtscTuningSpace.DefaultLocator as TunerLib.IATSCLocator;
					if (myLocator ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not get IATSCLocator");
						return;
					}

					//set the properties for the new tuning request. For ATSC we only set the frequency
					DVBChannel chan=(DVBChannel)tuningObject;
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Tune() ATSC freq:{0} channel:{1} fec:{2} mod:{3} sr:{4} ONID:{5}, TSID:{6} SID:{7}",
						chan.Frequency,chan.PhysicalChannel,chan.FEC,chan.Modulation,chan.Symbolrate,chan.NetworkID,chan.TransportStreamID,chan.ProgramNumber);

					myLocator.PhysicalChannel       = chan.PhysicalChannel;
					myATSCTuneRequest.Channel       = chan.MajorChannel;
					myATSCTuneRequest.MinorChannel  = chan.MinorChannel;
					myLocator.CarrierFrequency		  = chan.Frequency;
					myLocator.InnerFEC						  = (TunerLib.FECMethod)chan.FEC;
					myLocator.SymbolRate					  = chan.Symbolrate;
					myLocator.Modulation					  = (TunerLib.ModulationType)chan.Modulation;
					myATSCTuneRequest.Locator					= (TunerLib.Locator)myLocator;
					currentTuningObject = chan;
					//and submit the tune request
					myTuner.TuneRequest  = newTuneRequest;
					Marshal.ReleaseComObject(myATSCTuneRequest);

				}
				else if (Network() == NetworkType.DVBC)
				{
					//get the IDVBCLocator interface
					TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
					if (myLocator == null)
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
					if (myLocator ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not get IDVBCLocator");
						return;
					}

					//set the properties for the new tuning request. For DVB-C we only set the frequency
					DVBChannel chan=(DVBChannel)tuningObject;
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Tune() DVB-C freq:{0} fec:{1} mod:{2} sr:{3} ONID:{4}, TSID:{5} SID:{6}",
						chan.Frequency,chan.FEC,chan.Modulation,chan.Symbolrate,chan.NetworkID,chan.TransportStreamID,chan.ProgramNumber);

					myLocator.CarrierFrequency		= chan.Frequency;
					myLocator.InnerFEC						= (TunerLib.FECMethod)chan.FEC;
					myLocator.SymbolRate					= chan.Symbolrate;
					myLocator.Modulation					= (TunerLib.ModulationType)chan.Modulation;
					
					myTuneRequest.ONID						= chan.NetworkID;	//original network id
					myTuneRequest.TSID						= chan.TransportStreamID;	//transport stream id
					myTuneRequest.SID							= chan.ProgramNumber;		//service id
					
					myTuneRequest.Locator					= (TunerLib.Locator)myLocator;
					currentTuningObject = chan;
					//and submit the tune request
					myTuner.TuneRequest  = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);

				}
				else if (Network() == NetworkType.DVBS)
				{
					//get the IDVBSLocator interface
					TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
					if (myLocator == null)
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBSLocator;
					if (myLocator ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not get IDVBSLocator");
						return;
					}

					DVBChannel chan=(DVBChannel)tuningObject;
					
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Tune() DVB-S freq:{0} fec:{1} pol:{2} sr:{3} ONID:{4}, TSID:{5} SID:{6}",
						chan.Frequency,chan.FEC,chan.Polarity,chan.Symbolrate,chan.NetworkID,chan.TransportStreamID,chan.ProgramNumber);
					//set the properties for the new tuning request. 
					myLocator.CarrierFrequency		= chan.Frequency;
					myLocator.InnerFEC						= (TunerLib.FECMethod)chan.FEC;
					if (chan.Polarity==0) 
						myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
					else
						myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;
					myLocator.SymbolRate					= chan.Symbolrate;
					myTuneRequest.ONID						= chan.NetworkID;	//original network id
					myTuneRequest.TSID						= chan.TransportStreamID;	//transport stream id
					myTuneRequest.SID							= chan.ProgramNumber;		//service id
					myTuneRequest.Locator					= (TunerLib.Locator)myLocator;
					currentTuningObject = chan;
					LoadLNBSettings(ref chan,disecqNo);
					SetLNBSettings(myTuneRequest);
					//and submit the tune request
					myTuner.TuneRequest  = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);
				}

				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(-1, -1, -1, -1, "",-1);
					//m_streamDemuxer.GetEPGSchedule(0x50,currentTuningObject.ProgramNumber);
				}

			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Tune() exception {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}

		}//public void Tune(object tuningObject)
		
		/// <summary>
		/// Store any new tv and/or radio channels found in the tvdatabase
		/// </summary>
		/// <param name="radio">if true:Store radio channels found in the database</param>
		/// <param name="tv">if true:Store tv channels found in the database</param>
		public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels)
		{	
			if (m_SectionsTables==null) return;

			//get list of current tv channels present in the database
			ArrayList tvChannels = new ArrayList();
			TVDatabase.GetChannels(ref tvChannels);

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StoreChannels()");
			DVBSections.Transponder transp;
			if (Network() == NetworkType.ATSC)
			{
				ATSCSections atscSections = new ATSCSections(m_streamDemuxer);
				atscSections.Timeout=8000;
				transp = atscSections.Scan(m_SectionsTables);
			}
			else
			{
				using (DVBSections sections = new DVBSections())
				{
					sections.DemuxerObject=m_streamDemuxer;
					sections.Timeout=8000;
					transp = sections.Scan(m_SectionsTables);
				}
			}
			if (transp.channels==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found no channels", transp.channels);
				return;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found {0} channels", transp.channels.Count);
			for (int i=0; i < transp.channels.Count;++i)
			{
				DVBSections.ChannelInfo info=(DVBSections.ChannelInfo)transp.channels[i];
				if (info.service_provider_name==null) info.service_provider_name="";
				if (info.service_name==null) info.service_name="";
				
				info.service_provider_name=info.service_provider_name.Trim();
				info.service_name=info.service_name.Trim();
				if (info.service_provider_name.Length==0 ) 
					info.service_provider_name="Unknown";
				if (info.service_name.Length==0)
					info.service_name=String.Format("NoName:{0}{1}{2}{3}",info.networkID,info.transportStreamID, info.serviceID,i );


				bool hasAudio=false;
				bool hasVideo=false;
				info.freq=currentTuningObject.Frequency;
				DVBChannel newchannel   = new DVBChannel();

				//check if this channel has audio/video streams
				int audioOptions=0;
				if (info.pid_list!=null)
				{
					for (int pids =0; pids < info.pid_list.Count;pids++)
					{
						DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
						if (data.isVideo)
						{
							currentTuningObject.VideoPid=data.elementary_PID;
							hasVideo=true;
						}
						
						if (data.isAudio) 
						{
							switch(audioOptions)
							{
								case 0:
									currentTuningObject.Audio1=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage1=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 1:
									currentTuningObject.Audio2=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage2=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 2:
									currentTuningObject.Audio3=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage3=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;

							}

							if (hasAudio==false)
							{
								currentTuningObject.AudioPid=data.elementary_PID;
								if(data.data!=null)
								{
									if(data.data.Length==3)
										currentTuningObject.AudioLanguage=DVBSections.GetLanguageFromCode(data.data);
								}
								hasAudio=true;
							}
						}
						if (data.isAC3Audio)
						{
							currentTuningObject.AC3Pid=data.elementary_PID;
						}

						if (data.isTeletext)
						{
							currentTuningObject.TeletextPid=data.elementary_PID;
						}
					}
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:{9} videopid:{10} teletextpid:{11} program#:{12}", 
					info.service_provider_name,
					info.service_name,
					info.scrambled,
					info.freq,
					info.networkID,
					info.transportStreamID,
					info.serviceID,
					hasVideo, ((!hasVideo) && hasAudio),
					currentTuningObject.AudioPid,currentTuningObject.VideoPid,currentTuningObject.TeletextPid,
					info.program_number);

				if (info.serviceID==0) 
				{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel#{0} has no service id",i);
						continue;
				}
				bool IsRadio		  = ((!hasVideo) && hasAudio);
				bool IsTv   		  = (hasVideo);//some tv channels dont have an audio stream
		
				newchannel.Frequency = info.freq;
				newchannel.ServiceName  = info.service_name;
				newchannel.ServiceProvider  = info.service_provider_name;
				newchannel.IsScrambled  = info.scrambled;
				newchannel.NetworkID         = info.networkID;
				newchannel.TransportStreamID         = info.transportStreamID;
				newchannel.ProgramNumber          = info.serviceID;
				newchannel.FEC     = info.fec;
				newchannel.Polarity = currentTuningObject.Polarity;
				newchannel.Modulation = currentTuningObject.Modulation;
				newchannel.Symbolrate = currentTuningObject.Symbolrate;
				newchannel.ServiceType=1;//tv
				newchannel.AudioPid=currentTuningObject.AudioPid;
				newchannel.AudioLanguage=currentTuningObject.AudioLanguage;
				newchannel.VideoPid=currentTuningObject.VideoPid;
				newchannel.TeletextPid=currentTuningObject.TeletextPid;
				newchannel.Bandwidth=currentTuningObject.Bandwidth;
				newchannel.PMTPid=info.network_pmt_PID;
				newchannel.Audio1=currentTuningObject.Audio1;
				newchannel.Audio2=currentTuningObject.Audio2;
				newchannel.Audio3=currentTuningObject.Audio3;
				newchannel.AudioLanguage1=currentTuningObject.AudioLanguage1;
				newchannel.AudioLanguage2=currentTuningObject.AudioLanguage2;
				newchannel.AudioLanguage3=currentTuningObject.AudioLanguage3;
				newchannel.DiSEqC=currentTuningObject.DiSEqC;
				newchannel.LNBFrequency=currentTuningObject.LNBFrequency;
				newchannel.LNBKHz=currentTuningObject.LNBKHz;
				newchannel.PhysicalChannel=currentTuningObject.PhysicalChannel;

				
				if (info.serviceType==1)//tv
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} is a tv channel",newchannel.ServiceName);
					//check if this channel already exists in the tv database
					bool isNewChannel=true;
					int iChannelNumber=0;
					int channelId=-1;
					foreach (TVChannel tvchan in tvChannels)
					{
						if (tvchan.Name.Equals(newchannel.ServiceName))
						{
							if (TVDatabase.DoesChannelExist(tvchan.ID, newchannel.TransportStreamID, newchannel.NetworkID))
							{
								//yes already exists
								iChannelNumber=tvchan.Number;
								isNewChannel=false;
								channelId=tvchan.ID;
								break;
							}
						}
					}

					//if the tv channel found is not yet in the tv database
					TVChannel tvChan = new TVChannel();
					tvChan.Name=newchannel.ServiceName;
					tvChan.Number=newchannel.ProgramNumber;
					tvChan.VisibleInGuide=true;
					tvChan.Scrambled=newchannel.IsScrambled;
					iChannelNumber=tvChan.Number;
					if (isNewChannel)
					{
						tvChan.Number=TVDatabase.FindFreeTvChannelNumber(0);
						//then add a new channel to the database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new tv channel for {0}",newchannel.ServiceName);
						int id=TVDatabase.AddChannel(tvChan);
						channelId=id;
						newChannels++;
					}
					else
					{
						tvChan.ID=channelId;
						TVDatabase.UpdateChannel(tvChan,tvChan.Sort);
						updatedChannels++;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} already exists in tv database",newchannel.ServiceName);
					}
				
					if (Network() == NetworkType.DVBT)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapDVBTChannel(newchannel.ServiceName,
																			newchannel.ServiceProvider,
																			channelId, 
																			newchannel.Frequency, 
																			newchannel.NetworkID,
																			newchannel.TransportStreamID,
																			newchannel.ProgramNumber,
																			currentTuningObject.AudioPid,
																			currentTuningObject.VideoPid, 
																			currentTuningObject.TeletextPid,
																			newchannel.PMTPid,
																			newchannel.Bandwidth,
																			newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,
																			newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3);
					}
					if (Network() == NetworkType.DVBC)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapDVBCChannel(newchannel.ServiceName,
																			newchannel.ServiceProvider,
																			channelId, 
																			newchannel.Frequency, 
																			newchannel.Symbolrate,
																			newchannel.FEC,
																			newchannel.Modulation,
																			newchannel.NetworkID,
																			newchannel.TransportStreamID,
																			newchannel.ProgramNumber,
																			currentTuningObject.AudioPid,
																			currentTuningObject.VideoPid, 
																			currentTuningObject.TeletextPid,
																			newchannel.PMTPid,
																			newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,
																			newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3);
					}
					if (Network() == NetworkType.ATSC)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to ATSC card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapATSCChannel(newchannel.ServiceName,
							newchannel.PhysicalChannel, 
							newchannel.MinorChannel,  
							newchannel.MajorChannel, 
							newchannel.ServiceProvider,
							channelId, 
							newchannel.Frequency, 
							newchannel.Symbolrate,
							newchannel.FEC,
							newchannel.Modulation,
							newchannel.NetworkID,
							newchannel.TransportStreamID,
							newchannel.ProgramNumber,
							currentTuningObject.AudioPid,
							currentTuningObject.VideoPid, 
							currentTuningObject.TeletextPid,
							newchannel.PMTPid,
							newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3);
					}

					if (Network() == NetworkType.DVBS)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
						newchannel.ID=channelId;
						TVDatabase.AddSatChannel(newchannel);
					}
					TVDatabase.MapChannelToCard(channelId,ID);

					
					TVGroup group = new TVGroup();
					if (info.scrambled)
					{
						group.GroupName="Scrambled";
					}
					else
					{
						group.GroupName="Unscrambled";
					}
					int groupid=TVDatabase.AddGroup(group);
					group.ID=groupid;
					TVChannel tvTmp=new TVChannel();
					tvTmp.Name=newchannel.ServiceName;
					tvTmp.Number=iChannelNumber;
					tvTmp.ID=channelId;
					TVDatabase.MapChannelToGroup(group,tvTmp);

					//make group for service provider
					group = new TVGroup();
					group.GroupName=newchannel.ServiceProvider;
					groupid=TVDatabase.AddGroup(group);
					group.ID=groupid;
					tvTmp=new TVChannel();
					tvTmp.Name=newchannel.ServiceName;
					tvTmp.Number=iChannelNumber;
					tvTmp.ID=channelId;
					TVDatabase.MapChannelToGroup(group,tvTmp);

				}
				else if (info.serviceType==2) //radio
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} is a radio channel",newchannel.ServiceName);
					//check if this channel already exists in the radio database
					bool isNewChannel=true;
					int channelId=-1;
					ArrayList radioStations = new ArrayList();
					RadioDatabase.GetStations(ref radioStations);
					foreach (RadioStation station in radioStations)
					{
						if (station.Name.Equals(newchannel.ServiceName))
						{
							//yes already exists
							isNewChannel=false;
							channelId=station.ID;
							station.Scrambled=info.scrambled;
							RadioDatabase.UpdateStation(station);
							break;
						}
					}

					//if the tv channel found is not yet in the tv database
					if (isNewChannel)
					{
						//then add a new channel to the database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new radio channel for {0}",newchannel.ServiceName);
						RadioStation station = new RadioStation();
						station.Name=newchannel.ServiceName;
						station.Channel=newchannel.ProgramNumber;
						station.Frequency=newchannel.Frequency;
						station.Scrambled=info.scrambled;
						int id=RadioDatabase.AddStation(ref station);
						channelId=id;
						newChannels++;
					}
					else
					{
						updatedChannels++;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} already exists in tv database",newchannel.ServiceName);
					}

					if (Network() == NetworkType.DVBT)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapDVBTChannel(newchannel.ServiceName,newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid,newchannel.Bandwidth);
					}
					if (Network() == NetworkType.DVBC)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapDVBCChannel(newchannel.ServiceName,newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.Symbolrate,newchannel.FEC,newchannel.Modulation,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid);
					}
					if (Network() == NetworkType.ATSC)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapATSCChannel(newchannel.ServiceName,newchannel.PhysicalChannel, 
							newchannel.MinorChannel,  
							newchannel.MajorChannel, newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.Symbolrate,newchannel.FEC,newchannel.Modulation,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid);
					}
					if (Network() == NetworkType.DVBS)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
						newchannel.ID=channelId;

						int scrambled=0;
						if (newchannel.IsScrambled) scrambled=1;
						RadioDatabase.MapDVBSChannel(newchannel.ID,newchannel.Frequency,newchannel.Symbolrate,
							newchannel.FEC,newchannel.LNBKHz,0,newchannel.ProgramNumber,
							0,newchannel.ServiceProvider,newchannel.ServiceName,
							0,0,newchannel.AudioPid,newchannel.VideoPid,newchannel.AC3Pid,
							0,0,0,0,scrambled,
							newchannel.Polarity,newchannel.LNBFrequency
							,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.PCRPid,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,
							newchannel.AudioLanguage2,newchannel.AudioLanguage3,
							newchannel.ECMPid,newchannel.PMTPid);
					}
					RadioDatabase.MapChannelToCard(channelId,ID);
				}
			}//for (int i=0; i < transp.channels.Count;++i)
		}//public void StoreChannels(bool radio, bool tv)
		#endregion

		#endregion

		#region Radio
		public void TuneRadioChannel(RadioStation channel)
		{	
			if (m_NetworkProvider==null) return;

			try
			{	
				m_iPrevChannel		= m_iCurrentChannel;
				m_iCurrentChannel = channel.Channel;
				m_StartTime				= DateTime.Now;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() tune to radio station:{0}", channel.Name);

				//get the ITuner interface from the network provider filter
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner==null) return;

				TunerLib.IATSCTuningSpace myAtscTuningSpace =null;
				TunerLib.IDVBTuningSpace2 myTuningSpace =null;
				if (Network()==NetworkType.ATSC)
				{
					//get the IATSCTuningSpace from the tuner
					myAtscTuningSpace = myTuner.TuningSpace as TunerLib.IATSCTuningSpace;
					if (myAtscTuningSpace ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() tuningspace=null");
						return;
					}

					//create a new tuning request
					newTuneRequest = myAtscTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed Tune() could not create new tuningrequest");
						return;
					}
				}
				else
				{
					//get the IDVBTuningSpace2 from the tuner
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IDVBTuningSpace2");
					myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. Invalid tuningspace");
						return ;
					}


					//create a new tuning request
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() create new tuningrequest");
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}
	

				TunerLib.IDVBTuneRequest myTuneRequest=null;
				TunerLib.IATSCChannelTuneRequest myATSCTuneRequest=null;
				if (m_NetworkType!=NetworkType.ATSC)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IDVBTuneRequest");
					myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() cast new tuningrequest to IATSCChannelTuneRequest");
					myATSCTuneRequest = newTuneRequest as TunerLib.IATSCChannelTuneRequest;
					if (myATSCTuneRequest ==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
						return ;
					}
				}

				
				int frequency=-1,ONID=-1,TSID=-1,SID=-1,  pmtPid=-1;
				int audioPid=-1,bandwidth=8;
				string providerName;
				switch (m_NetworkType)
				{
					case NetworkType.ATSC: 
					{
						//get the ATSC tuning details from the tv database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get ATSC tuning details");
						int symbolrate=0,innerFec=0,modulation=0,physicalChannel=0;
						int minorChannel=0,majorChannel=0;
						RadioDatabase.GetATSCTuneRequest(channel.ID,out physicalChannel, out minorChannel,out majorChannel,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out pmtPid);

						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz physicalChannel:{1} symbolrate:{2} innerFec:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8}", 
							frequency,physicalChannel,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);

						//get the IATSCLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get IATSCLocator interface");
						TunerLib.IATSCLocator myLocator = myATSCTuneRequest.Locator as TunerLib.IATSCLocator;	
						if (myLocator==null)
						{
							myLocator = myAtscTuningSpace.DefaultLocator as TunerLib.IATSCLocator;
						}
						
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get IATSCLocator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						
						
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.PhysicalChannel			= physicalChannel;
						myLocator.SymbolRate				  = symbolrate;
						myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
						myLocator.Modulation					= (TunerLib.ModulationType)modulation;
						myATSCTuneRequest.MinorChannel= minorChannel;
						myATSCTuneRequest.Channel		  = majorChannel;
						myATSCTuneRequest.Locator=(TunerLib.Locator)myLocator;
						currentTuningObject=new DVBChannel();
						currentTuningObject.PhysicalChannel=physicalChannel;
						currentTuningObject.MinorChannel=minorChannel;
						currentTuningObject.MajorChannel=majorChannel;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						Marshal.ReleaseComObject(myATSCTuneRequest);
					} break;
					
					case NetworkType.DVBC: 
					{
						//get the DVB-C tuning details from the tv database
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBC tuning details");
						int symbolrate=0,innerFec=0,modulation=0;
						RadioDatabase.GetDVBCTuneRequest(channel.ID,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out pmtPid);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							frequency,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);

						//get the IDVBCLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get IDVBCLocator interface");
						TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						
						
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.SymbolRate				  = symbolrate;
						myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
						myLocator.Modulation					= (TunerLib.ModulationType)modulation;
						myTuneRequest.ONID	= ONID;					//original network id
						myTuneRequest.TSID	= TSID;					//transport stream id
						myTuneRequest.SID		= SID;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;


					} break;

					case NetworkType.DVBS: 
					{					
						//get the DVB-S tuning details from the tv database
						//for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBS tuning details");
						DVBChannel ch=new DVBChannel();
						if(RadioDatabase.GetDVBSTuneRequest(channel.ID,0,ref ch)==false)//only radio
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							ch.Frequency,ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID,ch.ProgramNumber,ch.ServiceProvider);

						//get the IDVBSLocator interface from the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get IDVBSLocator");
						TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBSLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", frequency);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= ch.Frequency;
						if (ch.Polarity==0) 
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
						else
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;

						myLocator.SymbolRate= ch.Symbolrate;
						myLocator.InnerFEC	= (TunerLib.FECMethod)ch.FEC;
						myTuneRequest.ONID	= ch.NetworkID;		//original network id
						myTuneRequest.TSID	= ch.TransportStreamID;		//transport stream id
						myTuneRequest.SID		= ch.ProgramNumber;		//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						

						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=ch.Frequency;
						currentTuningObject.Symbolrate=ch.Symbolrate;
						currentTuningObject.FEC=ch.FEC;
						currentTuningObject.Polarity=ch.Polarity;
						currentTuningObject.NetworkID=ch.NetworkID;
						currentTuningObject.TransportStreamID=ch.TransportStreamID;
						currentTuningObject.ProgramNumber=ch.ProgramNumber;
						currentTuningObject.AudioPid=ch.AudioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=ch.PMTPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.DiSEqC=ch.DiSEqC;
						currentTuningObject.LNBFrequency=ch.LNBFrequency;
						currentTuningObject.LNBKHz=ch.LNBKHz;
						SetLNBSettings(myTuneRequest);
					} break;

					case NetworkType.DVBT: 
					{
						//get the DVB-T tuning details from the tv database
						//for DVB-T this is the frequency, ONID , TSID and SID
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBT tuning details");
						RadioDatabase.GetDVBTTuneRequest(channel.ID,out providerName,out frequency, out ONID, out TSID, out SID, out audioPid,out pmtPid, out bandwidth);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID,providerName);
						//get the IDVBTLocator interface from the new tuning request

						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get IDVBTLocator");
						TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
						}
						
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot get locator", frequency,ONID,TSID,SID);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= frequency;
						myLocator.Bandwidth=bandwidth;
						myTuneRequest.ONID	= ONID;					//original network id
						myTuneRequest.TSID	= TSID;					//transport stream id
						myTuneRequest.SID		= SID;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;

						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.Bandwidth=bandwidth;
						currentTuningObject.ServiceName=channel.Name;
					} break;
				}	//switch (m_NetworkType)
				//submit tune request to the tuner
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() submit tuning request");
				myTuner.TuneRequest = newTuneRequest;
				Marshal.ReleaseComObject(myTuneRequest);
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() done");

				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(currentTuningObject.AudioPid, currentTuningObject.VideoPid, currentTuningObject.TeletextPid, currentTuningObject.Audio3, currentTuningObject.ServiceName,currentTuningObject.PMTPid);
					m_streamDemuxer.GetEPGSchedule(0x50,currentTuningObject.ProgramNumber);
				}

				SetupDemuxer(m_DemuxVideoPin,0,m_DemuxAudioPin,currentTuningObject.AudioPid);
				SendPMT();
				if(m_pluginsEnabled==true)
					ExecTuner();
			}
			finally
			{
				refreshPmtTable=false;
			}
		}//public void TuneRadioChannel(AnalogVideoStandard standard,int iChannel,int country)

		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio) 
			{
				if (m_graphState!=State.Created)  return;
				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9=null;
				}
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRadio()");

				// add the preferred video/audio codecs
				AddPreferredCodecs(true,false);


				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRadio() render demux output pin");
				if(m_graphBuilder.Render(m_DemuxAudioPin) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
					return;
				}

				TuneRadioChannel(station);
				//get the IMediaControl interface of the graph
				if(m_mediaControl == null)
					m_mediaControl = m_graphBuilder as IMediaControl;

				//start the graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
				if (m_mediaControl!=null)
				{
					int hr=m_mediaControl.Run();
					if (hr<0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED cannot get IMediaControl");
				}

				graphRunning=true;
				m_graphState = State.Radio;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Listening to radio..");
				return;
			}

			// tune to the correct channel

			TuneRadioChannel(station);
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Listening to radio..");
		}
		
		public void TuneRadioFrequency(int frequency)
		{
		}
		#endregion
		
		#region plugins
		void ExecTuner()
		{
			DVBGraphSS2.TunerData tu=new DVBGraphSS2.TunerData();
			if (Network()==NetworkType.DVBS) tu.tt = (int) DVBGraphSS2.TunerType.ttSat;
			if (Network()==NetworkType.DVBC) tu.tt = (int) DVBGraphSS2.TunerType.ttCable;
			if (Network()==NetworkType.DVBT) tu.tt = (int) DVBGraphSS2.TunerType.ttTerrestrical;

			tu.Frequency=(UInt32)(currentTuningObject.Frequency);
			tu.SymbolRate=(UInt32)(currentTuningObject.Symbolrate);
			tu.AC3=0;
			tu.AudioPID=(UInt16)currentTuningObject.AudioPid;
			tu.DiseqC=(UInt16)currentTuningObject.DiSEqC;
			tu.PMT=(UInt16)currentTuningObject.PMTPid;
			tu.ECM_0=(UInt16)currentTuningObject.ECMPid;
			tu.FEC=(UInt16)6;
			tu.LNB=(UInt16)currentTuningObject.LNBFrequency;
			tu.LNBSelection=(UInt16)currentTuningObject.LNBKHz;
			tu.NetworkID=(UInt16)currentTuningObject.NetworkID;
			tu.PCRPID=(UInt16)currentTuningObject.PCRPid;
			tu.Polarity=(UInt16)currentTuningObject.Polarity;
			tu.SID=(UInt16)currentTuningObject.ProgramNumber;
			tu.TelePID=(UInt16)currentTuningObject.TeletextPid;
			tu.TransportStreamID=(UInt16)currentTuningObject.TransportStreamID;
			tu.VideoPID=(UInt16)currentTuningObject.VideoPid;
			tu.Reserved1=0;

			IntPtr data=Marshal.AllocHGlobal(50);
			Marshal.StructureToPtr(tu,data,true);

			bool flag=false;
			if(m_pluginsEnabled)
			{
				try
				{
					flag=EventMsg(999, data);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Capture,"Plugins-Exception: {0}",ex.Message);
				}
			}
			Marshal.FreeHGlobal(data);
		}
		#endregion

		private bool m_streamDemuxer_OnAudioFormatChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
		{/*
			Log.Write("DVBGraphBDA:Audio format changed");
			Log.Write("DVBGraphBDA:  Bitrate:{0}",audioFormat.Bitrate);
			Log.Write("DVBGraphBDA:  Layer:{0}",audioFormat.Layer);
			Log.Write("DVBGraphBDA:  SamplingFreq:{0}",audioFormat.SamplingFreq);
			Log.Write("DVBGraphBDA:  Channel:{0}",audioFormat.Channel);
			Log.Write("DVBGraphBDA:  Bound:{0}",audioFormat.Bound);
			Log.Write("DVBGraphBDA:  Copyright:{0}",audioFormat.Copyright);
			Log.Write("DVBGraphBDA:  Emphasis:{0}",audioFormat.Emphasis);
			Log.Write("DVBGraphBDA:  ID:{0}",audioFormat.ID);
			Log.Write("DVBGraphBDA:  Mode:{0}",audioFormat.Mode);
			Log.Write("DVBGraphBDA:  ModeExtension:{0}",audioFormat.ModeExtension);
			Log.Write("DVBGraphBDA:  Original:{0}",audioFormat.Original);
			Log.Write("DVBGraphBDA:  PaddingBit:{0}",audioFormat.PaddingBit);
			Log.Write("DVBGraphBDA:  PrivateBit:{0}",audioFormat.PrivateBit);
			Log.Write("DVBGraphBDA:  ProtectionBit:{0}",audioFormat.ProtectionBit);
			Log.Write("DVBGraphBDA:  TimeLength:{0}",audioFormat.TimeLength);*/
			return false;
		}

		private void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
		{
			if (pmtTable==null) return;
			if (pmtTable.Length<6) return;
			if (currentTuningObject.NetworkID<0 ||
				  currentTuningObject.TransportStreamID<0 ||
					currentTuningObject.ProgramNumber<0) return;
			try
			{
				string pmtName=String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
					Utils.FilterFileName(currentTuningObject.ServiceName),
					currentTuningObject.NetworkID,
					currentTuningObject.TransportStreamID,
					currentTuningObject.ProgramNumber,
					(int)Network());
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: OnPMTIsChanged:{0}", pmtName);
				System.IO.FileStream stream = new System.IO.FileStream(pmtName,System.IO.FileMode.Create,System.IO.FileAccess.Write,System.IO.FileShare.None);
				stream.Write(pmtTable,0,pmtTable.Length);
				stream.Close();
				refreshPmtTable=true;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ERROR: exception while creating pmt {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}
		private void m_streamDemuxer_OnGotSection(int pid, int tableID, byte[] sectionData)
		{
		}

		private void m_streamDemuxer_OnGotTable(int pid, int tableID, ArrayList tableList)
		{
			if(tableList==null)
				return;
			if(tableList.Count<1)
				return;

		//	Log.Write("pid:{0:X} table:{1:X}", pid,tableID);
		}

	}//public class DVBGraphBDA 
}//namespace MediaPortal.TV.Recording
//end of file
#endif