using System;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;



namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung f�r DVBGraphSS2.
	/// </summary>
	/// 
	public class DVBGraphSS2 : IGraph
	
	{

		#region Mpeg2-Arrays
		static byte[] Mpeg2ProgramVideo = 
				{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
		} ;
	static byte [] MPEG1AudioFormat = 
	  {
		  0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
		  0x02, 0x00,             // channels		  = 2
		  0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
		  0x00, 0xEE, 0x02, 0x00, // nAvgBytesPerSec  = 0x00007d00=192000
		  0x04, 0x00,             // nBlockAlign      = 4 (channels*(bitspersample/8))
		  0x10, 0x00,             // wBitsPerSample   = 0
		  0x00, 0x00,             // extra size       = 0x0000 = 0 bytes
		};
		#endregion

		#region Enums
		protected enum State
		{ 
			None,
			Created,
			TimeShifting,
			Recording,
			Viewing,
			Radio,
			EPGGrab
		};

		public enum TunerType
		{
			ttCable=0,
			ttSat,
			ttTerrestrical,
			ttATSC
		
		}
		#endregion

		#region Structs
		public struct TunerData
		{
			public int tt;
			public UInt32 Frequency;
			public UInt32 SymbolRate;
			public UInt16 LNB;           //LNB Frequency, e.g. 9750, 10600
			public UInt16 PMT;           //PMT Pid
			public UInt16 ECM_0; //= 0 if unencrypted
			public byte Reserved1;
			public byte AC3;           //= 1 if audio PID = AC3 private stream
			//= 0 otherwise
			public UInt16 FEC;           //1 = 1/2, 2 = 2/3, 3 = 3/4,
			//4 = 5/6, 5 = 7/8, 6 = Auto
			public UInt16 CAID_0;
			public UInt16 Polarity;      //0 = H, 1 = V
			//or Modulation or GuardUInterval
			public UInt16 ECM_1;
			public UInt16 LNBSelection;  //0 = none, 1 = 22 khz
			public UInt16 CAID_1;
			public UInt16 DiseqC;        //0 = none, 1 = A, 2 = B,
			//3 = A/A, 4 = B/A, 5 = A/B, 6 = B/B
			public UInt16 ECM_2;
			public UInt16 AudioPID;
			public UInt16 CAID_2;
			public UInt16 VideoPID;
			public UInt16 TransportStreamID; //from 2.0 R3 on (?), if scanned channel
			public UInt16 TelePID;
			public UInt16 NetworkID;         //from 2.0 R3 on (?), if scanned channel
			public UInt16 SID;               //Service ID
			public UInt16 PCRPID;

		} 
		#endregion

		#region Imports 
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool EventMsg(int eventType,[In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int SetAppHandle([In] IntPtr hnd/*,[In, MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback*/);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick([In] int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle([In] long menu);

		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin pin,int pid,IPin pin1,int pid1);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid, int tid, ref int secCount,int tabSec,int timeout);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin,int pid);		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin);
		
		// registry settings
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
		#endregion

		#region Definitions
		//
		public  static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid( 0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
		public  static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid( 0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
		//
		const int WS_CHILD = 0x40000000;
		const int WS_CLIPCHILDREN = 0x02000000;
		const int WS_CLIPSIBLINGS = 0x04000000;
		//
		// 

		protected bool			m_bOverlayVisible=false;
		protected DVBChannel	m_currentChannel=new DVBChannel();
		//
		//
		protected bool					m_firstTune=false;
		//
		protected IBaseFilter			m_sampleGrabber=null;
		protected ISampleGrabber		m_sampleInterface=null;

		protected IMpeg2Demultiplexer	m_demuxInterface=null;
		protected IBaseFilter			m_mpeg2Data=null; 
		protected IBasicVideo2			m_basicVideo=null;
		protected IVideoWindow			m_videoWindow=null;
		protected State                 m_graphState=State.None;
		protected IMediaControl			m_mediaControl=null;
		protected int                   m_cardID=-1;
		protected IBaseFilter			m_b2c2Adapter=null;
		protected IPin					m_videoPin=null;
		protected IPin					m_audioPin=null;
		protected IPin					m_demuxVideoPin=null;
		protected IPin					m_demuxAudioPin=null;
		protected IPin					m_demuxSectionsPin=null;
		protected IPin					m_data0=null;
		protected IPin					m_data1=null;
		protected IPin					m_data2=null;
		protected IPin					m_data3=null;
		// stream buffer sink filter
		protected IStreamBufferInitialize		m_streamBufferInit=null; 
		protected IStreamBufferConfigure		m_config=null;
		protected IStreamBufferSink				m_sinkInterface=null;
		protected IBaseFilter					m_sinkFilter=null;
		protected IBaseFilter					m_mpeg2Analyzer=null;
		protected IBaseFilter					m_sourceFilter=null;
		protected IBaseFilter					m_demux=null;
		// def. the interfaces
		protected DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3		m_dataCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2	m_tunerCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2		m_avCtrl=null;
        // player graph
		protected IGraphBuilder			m_graphBuilder=null;
		protected bool					m_timeShift=true;
		protected int					m_myCookie=0; // for the rot
		protected DateTime              m_StartTime=DateTime.Now;
		protected int					m_iChannelNr=-1;
		protected bool					m_channelFound=false;
		StreamBufferConfig				m_streamBufferConfig=null;
		protected VMR9Util				Vmr9=null; 
		protected string				m_filename="";
		protected DVBSections			m_sections=new DVBSections();
		protected bool					m_pluginsEnabled=false;
		int	[]							m_ecmPids=new int[3]{0,0,0};
		int[]							m_ecmIDs=new int[3]{0,0,0};
        DVBDemuxer m_streamDemuxer = new DVBDemuxer();
		string							m_cardType="";
		string							m_cardFilename="";
		DVBChannel						m_currentTuningObject;
		DirectShowHelperLib.StreamBufferRecorderClass m_recorder=null;
		int								m_selectedAudioPid=0;

		#endregion

		
		public DVBGraphSS2(int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
		{

			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_pluginsEnabled=xmlreader.GetValueAsBool("dvb_ts_cards","enablePlugins",false);
				m_cardType=xmlreader.GetValueAsString("DVBSS2","cardtype","");
				m_cardFilename=xmlreader.GetValueAsString("dvb_ts_cards","filename","");
			}

			// teletext settings
			GUIWindow win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
			if(win!=null)
				win.SetObject(m_streamDemuxer.Teletext);
			
			win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
			if(win!=null)
                win.SetObject(m_streamDemuxer.Teletext);

            m_streamDemuxer.OnAudioFormatChanged += new DVBDemuxer.OnAudioChanged(OnAudioFormatChanged);
			m_streamDemuxer.CardType=(int)DVBEPG.EPGCard.TechnisatStarCards;
			//m_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
			m_streamDemuxer.OnGotSection+=new MediaPortal.TV.Recording.DVBDemuxer.OnSectionReceived(m_streamDemuxer_OnGotSection);
			m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
			// reg. settings
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");

			}
			catch(Exception){}
		}

		bool OnAudioFormatChanged(DVBDemuxer.AudioHeader audioFormat)
		{
			// set demuxer
			// release memory
			Log.Write("DVBGraphSS2:Audio format changed");
			Log.Write("DVBGraphSS2:  Bitrate:{0}",audioFormat.Bitrate);
			Log.Write("DVBGraphSS2:  Layer:{0}",audioFormat.Layer);
			Log.Write("DVBGraphSS2:  SamplingFreq:{0}",audioFormat.SamplingFreq);
			Log.Write("DVBGraphSS2:  Channel:{0}",audioFormat.Channel);
			Log.Write("DVBGraphSS2:  Bound:{0}",audioFormat.Bound);
			Log.Write("DVBGraphSS2:  Copyright:{0}",audioFormat.Copyright);
			Log.Write("DVBGraphSS2:  Emphasis:{0}",audioFormat.Emphasis);
			Log.Write("DVBGraphSS2:  ID:{0}",audioFormat.ID);
			Log.Write("DVBGraphSS2:  Mode:{0}",audioFormat.Mode);
			Log.Write("DVBGraphSS2:  ModeExtension:{0}",audioFormat.ModeExtension);
			Log.Write("DVBGraphSS2:  Original:{0}",audioFormat.Original);
			Log.Write("DVBGraphSS2:  PaddingBit:{0}",audioFormat.PaddingBit);
			Log.Write("DVBGraphSS2:  PrivateBit:{0}",audioFormat.PrivateBit);
			Log.Write("DVBGraphSS2:  ProtectionBit:{0}",audioFormat.ProtectionBit);
			Log.Write("DVBGraphSS2:  TimeLength:{0}",audioFormat.TimeLength);

//				AMMediaType mpegAudioOut = new AMMediaType();
//				mpegAudioOut.majorType = MediaType.Audio;
//				mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
//				mpegAudioOut.sampleSize = 0;
//				mpegAudioOut.temporalCompression = false;
//				mpegAudioOut.fixedSizeSamples = true;
//				mpegAudioOut.unkPtr = IntPtr.Zero;
//				mpegAudioOut.formatType = FormatType.WaveEx;
//				mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
//				mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
//				System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
				
			return true;
		}
		~DVBGraphSS2()
		{
		}
		//
		public static void Message()
		{
		}
		//
		#region Plugin-Handling
		void ExecTuner()
		{
			TunerData tu=new TunerData();
			//tu.TunerType=1;
			tu.tt=(int)TunerType.ttSat;
			tu.Frequency=(UInt32)(m_currentChannel.Frequency);
			tu.SymbolRate=(UInt32)(m_currentChannel.Symbolrate);
			tu.AC3=0;
			tu.AudioPID=(UInt16)m_currentChannel.AudioPid;
			tu.DiseqC=(UInt16)m_currentChannel.DiSEqC;
			tu.PMT=(UInt16)m_currentChannel.PMTPid;
			tu.FEC=(UInt16)6;
			tu.LNB=(UInt16)m_currentChannel.LNBFrequency;
			tu.LNBSelection=(UInt16)m_currentChannel.LNBKHz;
			tu.NetworkID=(UInt16)m_currentChannel.NetworkID;
			tu.PCRPID=(UInt16)m_currentChannel.PCRPid;
			tu.Polarity=(UInt16)m_currentChannel.Polarity;
			tu.SID=(UInt16)m_currentChannel.ProgramNumber;
			tu.TelePID=(UInt16)m_currentChannel.TeletextPid;
			tu.TransportStreamID=(UInt16)m_currentChannel.TransportStreamID;
			tu.VideoPID=(UInt16)m_currentChannel.VideoPid;
			tu.Reserved1=0;
			tu.ECM_0=(UInt16)m_currentChannel.ECMPid;
			tu.ECM_1=(UInt16)m_ecmPids[1];
			tu.ECM_2=(UInt16)m_ecmPids[2];
			tu.CAID_0=(UInt16)m_currentChannel.Audio3;
			tu.CAID_1=(UInt16)m_ecmIDs[1];
			tu.CAID_2=(UInt16)m_ecmIDs[2];

			IntPtr data=Marshal.AllocHGlobal(50);
			
			Marshal.StructureToPtr(tu,data,true);

			bool flag=false;
			if(m_pluginsEnabled)
			{
				try
				{
					flag=EventMsg(999, data/*,out pids*/);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Capture,"Plugins-Exception: {0}",ex.Message);
				}

			}
			Marshal.FreeHGlobal(data);
		}
		#endregion
		//
		/// <summary>
		/// Callback from Card. Sets an information struct with video settings
		/// </summary>

		public bool CreateGraph(int Quality)
		{
			if (m_graphState != State.None) return false;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph()");
			// create graphs
			Vmr9 =new VMR9Util("mytv");

			m_graphBuilder=(IGraphBuilder)  Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) );
			
			int n=0;
			m_b2c2Adapter=null;
			// create filters & interfaces
			try
			{
				m_b2c2Adapter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_B2C2Adapter, false ) );
				m_sinkFilter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_StreamBufferSink, false ) );
				m_mpeg2Analyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_Mpeg2VideoStreamAnalyzer, true ) );
				m_sinkInterface=(IStreamBufferSink)m_sinkFilter;
				m_mpeg2Data=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_Mpeg2Data, true ) );
				m_demux=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.Mpeg2Demultiplexer, true ) );
				m_demuxInterface=(IMpeg2Demultiplexer) m_demux;
				m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
				m_sampleInterface=(ISampleGrabber) m_sampleGrabber;
			}
			
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				return false;
				//System.Windows.Forms.MessageBox.Show(ex.Message);
			}

			if(m_b2c2Adapter==null)
				return false;
			try
			{

				n=m_graphBuilder.AddFilter(m_b2c2Adapter,"B2C2-Source");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add B2C2-Adapter");
					return false;
				}
				n=m_graphBuilder.AddFilter(m_sampleGrabber,"GrabberFilter");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add SampleGrabber");
					return false;
				}
				
				n=m_graphBuilder.AddFilter(m_mpeg2Data,"SectionsFilter");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add SectionsFilter");
					return false;
				}

				n=m_graphBuilder.AddFilter(m_demux,"Demuxer");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add Demultiplexer");
					return false;
				}
				// get interfaces
				m_dataCtrl=(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3) m_b2c2Adapter;
				if(m_dataCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2DataCtrl3");
					return false;
				}
				m_tunerCtrl=(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2) m_b2c2Adapter;
				if(m_tunerCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2TunerCtrl2");
					return false;
				}
				m_avCtrl=(DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2) m_b2c2Adapter;
				if(m_avCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2AVCtrl2");
					return false;
				}
				// init for tuner
				n=m_tunerCtrl.Initialize();
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Tuner initialize failed");
					return false;
				}
				// call checklock once, the return value dont matter
	
				n=m_tunerCtrl.CheckLock();
				bool b=false;
				b=SetVideoAudioPins();
				if(b==false)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetVideoAudioPins() failed");
					return false;
				}

				if(m_mpeg2Data!=null && m_demuxInterface!=null)
				{
					
					int hr=0;
					IPin mpeg2DataIn=null;
					hr=DsUtils.GetPin(m_mpeg2Data,PinDirection.Input,0,out mpeg2DataIn);
					if(mpeg2DataIn==null)
						return false;

					AMMediaType mt=new AMMediaType();
					mt.majorType=MEDIATYPE_MPEG2_SECTIONS;
					mt.subType=MEDIASUBTYPE_MPEG2_DATA;

					hr=m_demuxInterface.CreateOutputPin(ref mt,"MPEG2Sections",out m_demuxSectionsPin);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to create mpeg2-sections pin on demuxer");
						return false;
					}
					hr=m_graphBuilder.Connect(m_demuxSectionsPin,mpeg2DataIn);
					if(hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"dvbgrapgss2: FAILED to connect demux<->mpeg2data");
						return false;
					}
					Log.WriteFile(Log.LogType.Capture,"dvbgraphss2: successfully connected demux<->mpeg2data");
				}

				if(m_sampleInterface!=null)
				{
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					//m_sampleInterface.SetOneShot(true);
					m_sampleInterface.SetCallback(m_streamDemuxer,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}
				else
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() SampleGrabber-Interface not found");
					

			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				System.Windows.Forms.MessageBox.Show(ex.Message);
				return false;
			}
			
			m_graphState=State.Created;
			return true;
		}

		//
		private bool Tune(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq,int ecmPID,int ttxtPID,int pmtPID,int pcrPID,string pidText,int dvbsubPID)
		{
			int hr=0; // the result

			// clear epg
				if(Frequency>13000)
					Frequency/=1000;

				if(m_tunerCtrl==null || m_dataCtrl==null || m_b2c2Adapter==null || m_avCtrl==null)
					return false;

				// skystar
				if(m_cardType=="" || m_cardType=="skystar")
				{
					hr = m_tunerCtrl.SetFrequency(Frequency);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetFrequency");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetSymbolRate(SymbolRate);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetSymbolRate");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetLnbFrequency(LNBFreq);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetLnbFrequency");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetFec(FEC);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetFec");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetPolarity(POL);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetPolarity");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetLnbKHz(LNBKhz);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetLnbKHz");
						return false;	// *** FUNCTION EXIT POINT
					}
					hr = m_tunerCtrl.SetDiseqc(Diseq);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetDiseqc");
						return false;	// *** FUNCTION EXIT POINT
					}
				}
				// cablestar
			
			
			
				// airstar

				// final
				hr = m_tunerCtrl.SetTunerStatus();
				if (hr!=0)	
				{
					// some more tries...
					int retryCount=0;
					while(1>0)
					{
						hr=m_tunerCtrl.SetTunerStatus();
						if(hr!=0)
							retryCount++;
						else
							break;

						if(retryCount>=20)
						{
							Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetTunerStatus (in loop)");
							return false;	// *** FUNCTION EXIT POINT
						}
					}
					//
				
				}
			

			if(AudioPID!=-1 && VideoPID!=-1)
			{
				if(m_pluginsEnabled==false)
				{
					DeleteAllPIDs(m_dataCtrl,0);
					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					SetPidToPin(m_dataCtrl,0,16);
					SetPidToPin(m_dataCtrl,0,17);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,ttxtPID);
					SetPidToPin(m_dataCtrl,0,AudioPID);
					SetPidToPin(m_dataCtrl,0,m_currentChannel.Audio1);
					SetPidToPin(m_dataCtrl,0,m_currentChannel.Audio2);
					SetPidToPin(m_dataCtrl,0,VideoPID);
					SetPidToPin(m_dataCtrl,0,pmtPID);
					SetPidToPin(m_dataCtrl,0,dvbsubPID);
					SetPidToPin(m_dataCtrl,0,0xD3);
					SetPidToPin(m_dataCtrl,0,0xD2);

					if(pcrPID!=VideoPID)
						SetPidToPin(m_dataCtrl,0,pcrPID);

				}
				else
				{
					int epid=0;

					int eid=0;
					DeleteAllPIDs(m_dataCtrl,0);

					int count=0;
					for(int t=1;t<11;t++)
					{
						epid=GetPidNumber(pidText,t);
						eid=GetPidID(pidText,t);
						if(epid>0)
						{
							if(count<3)
							{
								m_ecmPids[count]=epid;
								m_ecmIDs[count]=eid;
								count++;
							}
							SetPidToPin(m_dataCtrl,0,epid);
						}
					}

					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					SetPidToPin(m_dataCtrl,0,16);
					SetPidToPin(m_dataCtrl,0,17);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,ecmPID);
					SetPidToPin(m_dataCtrl,0,0xD3);
					SetPidToPin(m_dataCtrl,0,0xD2);

					SetPidToPin(m_dataCtrl,0,ttxtPID);
					SetPidToPin(m_dataCtrl,0,AudioPID);
					SetPidToPin(m_dataCtrl,0,m_currentChannel.Audio1);
					SetPidToPin(m_dataCtrl,0,m_currentChannel.Audio2);
					SetPidToPin(m_dataCtrl,0,VideoPID);
					SetPidToPin(m_dataCtrl,0,dvbsubPID);
					SetPidToPin(m_dataCtrl,0,pmtPID);
					if(pcrPID!=VideoPID)
						SetPidToPin(m_dataCtrl,0,pcrPID);
				}


			}
			return true;
		}
		//
		/// <summary>
		/// Overlay-Controlling
		/// </summary>

		public bool Overlay
		{
			get 
			{
				return m_bOverlayVisible;
			}
			set 
			{
				if (value==m_bOverlayVisible) return;
				m_bOverlayVisible=value;
				if (!m_bOverlayVisible)
				{
					if (m_videoWindow!=null)
						m_videoWindow.put_Visible( DsHlp.OAFALSE );

				}
				else
				{
					if (m_videoWindow!=null)
						m_videoWindow.put_Visible( DsHlp.OATRUE );

				}
			}
		}
		/// <summary>
		/// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
		/// </summary>

		private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			if (m_graphState!=State.Viewing && m_graphState!=State.TimeShifting) return;
			
			if (!Vmr9.UseVMR9inMYTV)
			{

				if (GUIGraphicsContext.Overlay==false)
				{
					if(m_graphState!=State.Viewing)
					{
						Overlay=false;
						return;
					}
				}
				else
				{
					Overlay=true;
				}
			}
			int iVideoWidth=0;
			int iVideoHeight=0;
			int aspectX=4, aspectY=3;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);	
				m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			if (Vmr9.IsVMR9Connected)
			{
				aspectX=iVideoWidth=Vmr9.VideoWidth;
				aspectY=iVideoHeight=Vmr9.VideoHeight;
			}
			
			if (GUIGraphicsContext.IsFullScreenVideo || GUIGraphicsContext.ShowBackground==false)
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
					Log.Write("overlay: video WxH  : {0}x{1}",iVideoWidth,iVideoHeight);
					Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
					Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
					Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
					Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
					Log.Write("overlay: src        : ({0},{1})-({2},{3})",
						rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
					Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
						rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

					if(m_basicVideo!=null)
					{
						m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
						m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
					}
					if(m_videoWindow!=null)
						m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
				}
			}
			else if (!Vmr9.IsVMR9Connected)
			{
				if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
						GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
				if (iVideoHeight<=0 || iVideoWidth<=0) return;
        
				if(m_basicVideo!=null)
				{
					m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
					m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				}
				if(m_videoWindow!=null)
					m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);

			}

		}

		/// <summary>
		/// Deletes the current DirectShow graph created with CreateGraph()
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public void DeleteGraph()
		{
			if (m_graphState < State.Created) return;
			DirectShowUtil.DebugWrite("DVBGraphSS2:DeleteGraph()");
			
			m_iChannelNr=-1;
			//m_fileWriter.Close();

			if (m_streamDemuxer != null)
			{
				m_streamDemuxer.SetChannelData(0, 0, 0, 0, "",0);
			}

			StopRecording();
			StopTimeShifting();
			StopViewing();

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}

			if (m_mediaControl != null)
			{
				m_mediaControl.Stop();
				m_mediaControl = null;
			}

			//DsROT.RemoveGraphFromRot(ref m_myCookie);
			
			m_myCookie=0;

			if(m_sampleGrabber!=null)
			{
				Marshal.ReleaseComObject(m_sampleGrabber);
				m_sampleGrabber=null;
			}	
			if(m_sampleInterface!=null)
			{
				Marshal.ReleaseComObject(m_sampleInterface);
				m_sampleInterface=null;
			}	
			if (m_videoWindow != null)
			{
				m_bOverlayVisible=false;
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
				m_videoWindow.put_Owner(IntPtr.Zero);
				m_videoWindow = null;
			}


			if (m_basicVideo != null)
				m_basicVideo = null;
      

			DsUtils.RemoveFilters(m_graphBuilder);
			//
			// release all interfaces and pins
			//
			if(m_demux!=null)
			{
				Marshal.ReleaseComObject(m_demux);
				m_demux=null;
			}			
			if(m_demuxInterface!=null)
			{
				Marshal.ReleaseComObject(m_demuxInterface);
				m_demuxInterface=null;
			}			
			if(m_mpeg2Data!=null)
			{
				Marshal.ReleaseComObject(m_mpeg2Data);
				m_mpeg2Data=null;
			}			
			if(m_streamBufferInit!=null)
			{
				Marshal.ReleaseComObject(m_streamBufferInit);
				m_streamBufferInit=null;
			}
			if(m_config!=null)
			{
				Marshal.ReleaseComObject(m_config);
				m_config=null;
			}
			if(m_sinkInterface!=null)
			{
				Marshal.ReleaseComObject(m_sinkInterface);
				m_sinkInterface=null;
			}
			if(m_videoPin!=null)
			{
				Marshal.ReleaseComObject(m_videoPin);
				m_videoPin=null;
			}
			if(m_data0!=null)
			{
				Marshal.ReleaseComObject(m_data0);
				m_data0=null;
			}
			if(m_data1!=null)
			{
				Marshal.ReleaseComObject(m_data1);
				m_data1=null;
			}
			if(m_data2!=null)
			{
				Marshal.ReleaseComObject(m_data2);
				m_data2=null;
			}
			if(m_data3!=null)
			{
				Marshal.ReleaseComObject(m_data3);
				m_data3=null;
			}
			if(m_audioPin!=null)
			{
				Marshal.ReleaseComObject(m_audioPin);
				m_audioPin=null;
			}
			if(m_demuxVideoPin!=null)
			{
				Marshal.ReleaseComObject(m_demuxVideoPin);
				m_demuxVideoPin=null;
			}
			if(m_demuxAudioPin!=null)
			{
				Marshal.ReleaseComObject(m_demuxAudioPin);
				m_demuxAudioPin=null;
			}
			if(m_demuxSectionsPin!=null)
			{
				Marshal.ReleaseComObject(m_demuxSectionsPin);
				m_demuxSectionsPin=null;
			}

			if(m_tunerCtrl!=null)
			{
				Marshal.ReleaseComObject(m_tunerCtrl);
				m_tunerCtrl=null;
			}
			if(m_sinkFilter!=null)
			{
				Marshal.ReleaseComObject(m_sinkFilter);
				m_sinkFilter=null;
			}
			if(m_mpeg2Analyzer!=null)
			{
				Marshal.ReleaseComObject(m_mpeg2Analyzer);
				m_mpeg2Analyzer=null;
			}
			if(m_sourceFilter!=null)
			{
				Marshal.ReleaseComObject(m_sourceFilter);
				m_sourceFilter=null;
			}
			if(m_avCtrl!=null)
			{
				Marshal.ReleaseComObject(m_avCtrl);
				m_avCtrl=null;
			}
			if(m_dataCtrl!=null)
			{
				Marshal.ReleaseComObject(m_dataCtrl);
				m_dataCtrl=null;
			}
			if(m_b2c2Adapter!=null)
			{
				Marshal.ReleaseComObject(m_b2c2Adapter);
				m_b2c2Adapter=null;
			}
			if (m_graphBuilder != null)
			{
				Marshal.ReleaseComObject(m_graphBuilder); 
				m_graphBuilder = null;
			}
			GUIGraphicsContext.form.Invalidate(true);
			GC.Collect();

			//add collected stuff into programs database

			m_graphState = State.None;
			return;		
		}
		//
		//

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
		}
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
		/// 
		private bool SetVideoAudioPins()
		{
			int hr=0;
			PinInfo pInfo=new PinInfo();

			// video pin
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,0,out m_videoPin);
			if(hr!=0)
				return false;

			m_videoPin.QueryPinInfo(out pInfo);
			// audio pin
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,1,out m_audioPin);
			if(hr!=0)
				return false;

			if(m_videoPin==null || m_audioPin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: pins not found on adapter");
				return false;
			}
			m_audioPin.QueryPinInfo(out pInfo);

			// data pins
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,2,out m_data0);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,3,out m_data1);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,4,out m_data2);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,5,out m_data3);
			if(hr!=0)
				return false;


			return true;
		}
		//
		private bool CreateSinkSource(string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			int			hr=0;
			IPin		pinObj0=null;
			IPin		pinObj1=null;
			IPin		outPin=null;


			hr=m_graphBuilder.AddFilter(m_sinkFilter,"StreamBufferSink");
			hr=m_graphBuilder.AddFilter(m_mpeg2Analyzer,"Stream-Analyzer");

			// setup sampleGrabber and demuxer
			IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
			IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
			hr=m_graphBuilder.Connect(m_data0,samplePin);
			if(hr!=0)
				return false;

			samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);	
			hr=m_graphBuilder.Connect(demuxInPin,samplePin);
			if(hr!=0)
				return false;

			SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
				
				
			if(m_demuxVideoPin==null || m_demuxAudioPin==null)
				return false;

			pinObj0=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Input,0);
			if(pinObj0!=null)
			{
				
				hr=m_graphBuilder.Connect(m_demuxVideoPin,pinObj0);
				if(hr==0)
				{
					// render all out pins
					pinObj1=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Output,0);	
					hr=m_graphBuilder.Render(pinObj1);
					if(hr!=0)
						return false;
					hr=m_graphBuilder.Render(m_demuxAudioPin);
					if(hr!=0)
						return false;
					
					if(demuxInPin!=null)
						Marshal.ReleaseComObject(demuxInPin);
					if(samplePin!=null)
						Marshal.ReleaseComObject(samplePin);
					if(pinObj1!=null)
						Marshal.ReleaseComObject(pinObj1);
					if(pinObj0!=null)
						Marshal.ReleaseComObject(pinObj0);

					demuxInPin=null;
					samplePin=null;
					pinObj1=null;
					pinObj0=null;
				}
			} // render of sink is ready

			int ipos=fileName.LastIndexOf(@"\");
			string strDir=fileName.Substring(0,ipos);

			m_streamBufferConfig=new StreamBufferConfig();
			m_config = (IStreamBufferConfigure) m_streamBufferConfig;
			// setting the timeshift behaviors
			IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_config;
			IntPtr subKey = IntPtr.Zero;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pTemp.SetHKEY(subKey);
			
			hr=m_config.SetDirectory(strDir);	
			if(hr!=0)
				return false;
			hr=m_config.SetBackingFileCount(6, 8);    //4-6 files
			if(hr!=0)
				return false;
			
			hr=m_config.SetBackingFileDuration( 300); // 60sec * 4 files= 4 mins
			if(hr!=0)
				return false;

			subKey = IntPtr.Zero;
			HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_sinkFilter;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pConfig.SetHKEY(subKey);
			// lock on the 'filename' file
			hr=m_sinkInterface.LockProfile(fileName);
			m_filename=fileName;
			if(hr!=0)
				return false;

			if(pinObj0!=null)
				Marshal.ReleaseComObject(pinObj0);
			if(pinObj1!=null)
				Marshal.ReleaseComObject(pinObj1);
			if(outPin!=null)
				Marshal.ReleaseComObject(outPin);

			return true;
		}
		//
		bool DeleteDataPids(int pin)
		{
			bool res=false;

			res=DeleteAllPIDs(m_dataCtrl,0);

			return res;

		}
		int AddDataPidsToPin(int pin,int pid)
		{
			int res=0;
			
			res=SetPidToPin(m_dataCtrl,pin,pid);
			
			return res;
		}
		//
		public bool StartTimeShifting(TVChannel channel,string fileName)
		{

			if(m_graphState!=State.Created)
				return false;
			int hr=0;

			TuneChannel(channel);

			if(m_channelFound==false)
				return false;
			
			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}

			if(CreateSinkSource(fileName)==true)
			{
				m_mediaControl=(IMediaControl)m_graphBuilder;
				hr=m_mediaControl.Run();
				m_graphState = State.TimeShifting;

			}
			else {m_graphState=State.Created;return false;}

			return true;
		}
    
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
			DirectShowUtil.DebugWrite("DVBGraphSS2:StopTimeShifting()");
			m_mediaControl.Stop();
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}


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
		public bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFilename, bool bContentRecording, DateTime timeProgStart)
		{		
			if (m_graphState != State.TimeShifting ) return false;

			if (m_sinkFilter==null) 
			{
				return false;
			}

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			uint iRecordingType=0;
			if (bContentRecording) iRecordingType=0;
			else iRecordingType=1;										
		 
			m_recorder = new DirectShowHelperLib.StreamBufferRecorderClass();
			m_recorder.Create(m_sinkInterface as DirectShowHelperLib.IBaseFilter,strFilename,iRecordingType);
			long lStartTime=0;

			// if we're making a reference recording
			// then record all content from the past as well
			if (!bContentRecording)
			{
				// so set the startttime...
				uint uiSecondsPerFile;
				uint uiMinFiles, uiMaxFiles;
				m_config.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
				m_config.GetBackingFileDuration(out uiSecondsPerFile);
				lStartTime = uiSecondsPerFile;
				lStartTime*= (long)uiMaxFiles;

				// if start of program is given, then use that as our starttime
				if (timeProgStart.Year>2000)
				{
					TimeSpan ts = DateTime.Now-timeProgStart;
					DirectShowUtil.DebugWrite("mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
						timeProgStart.Hour,timeProgStart.Minute,timeProgStart.Second,
						ts.TotalHours,ts.TotalMinutes,ts.TotalSeconds);
															
					lStartTime = (long)ts.TotalSeconds;
				}
				else DirectShowUtil.DebugWrite("mpeg2:record entire timeshift buffer");
      
				TimeSpan tsMaxTimeBack=DateTime.Now-m_StartTime;
				if (lStartTime > tsMaxTimeBack.TotalSeconds )
				{
					lStartTime =(long)tsMaxTimeBack.TotalSeconds;
				}
        

				lStartTime*=-10000000L;//in reference time 
			}
			foreach (MetadataItem item in attribtutes.Values)
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

			m_graphState=State.Recording;
			return true;
		}
    
    
		/// <summary>
		/// Stops recording 
		/// </summary>
		/// <remarks>
		/// Graph should be recording. When Recording is stopped the graph is still 
		/// timeshifting
		/// </remarks>
		public void StopRecording()
		{
			if (m_recorder==null || m_graphState!=State.Recording)
				return ;
			

			if (m_recorder!=null) 
			{
				m_recorder.Stop();
				m_recorder=null;
			}

			m_graphState=State.TimeShifting;
			return ;

		}

		//
		//

		public void TuneChannel(TVChannel channel)
		{
			if(m_graphState==State.Recording)
				return;

			int channelID=channel.ID;
			m_iChannelNr=channel.Number;
			if(channelID!=-1)
			{
				
				DVBChannel ch=new DVBChannel();
				if(TVDatabase.GetSatChannel(channelID,1,ref ch)==false)//only television
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Tune: channel not found in database (idChannel={0})",channelID);
					m_channelFound=false;
					return;
				}

				if(m_pluginsEnabled==false && ch.IsScrambled==true)
				{
					m_channelFound=false;
					return;
				}
				m_channelFound=true;
				m_currentChannel=ch;
				m_selectedAudioPid=ch.AudioPid;
				if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,ch.VideoPid,ch.LNBFrequency,ch.ECMPid,ch.TeletextPid,ch.PMTPid,ch.PCRPid,ch.AudioLanguage3,ch.Audio3)==false)
				{
					m_channelFound=false;
					return;
				}

                if (m_streamDemuxer != null)
                {
                    m_streamDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName,ch.PMTPid);
                }
				if(m_pluginsEnabled==true)
					ExecTuner();

				if(m_mediaControl!=null && m_demuxVideoPin!=null && m_demuxAudioPin!=null && m_demux!=null && m_demuxInterface!=null)
				{

                    int hr = SetupDemuxer(m_demuxVideoPin, ch.VideoPid,m_demuxAudioPin, ch.AudioPid);
					if(hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}",hr.ToString());
						return;
					}
                }

				//SetMediaType();
				//m_gotAudioFormat=false;
				m_StartTime=DateTime.Now;
				if(m_streamDemuxer!=null)
					m_streamDemuxer.GetEPGSchedule(0x50,ch.ProgramNumber);
			}
			
		}
		void SetDemux(int audioPid,int videoPid)
		{
			
			if(m_demuxInterface==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetDemux FAILED: no Demux-Interface");
				return;
			}
			int hr=0;

			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:SetDemux() audio pid:0x{0:X} video pid:0x{1:X}",audioPid,videoPid);
			AMMediaType mpegVideoOut = new AMMediaType();
			mpegVideoOut.majorType = MediaType.Video;
			mpegVideoOut.subType = MediaSubType.MPEG2_Video;

			Size FrameSize=new Size(100,100);
			mpegVideoOut.unkPtr = IntPtr.Zero;
			mpegVideoOut.sampleSize = 0;
			mpegVideoOut.temporalCompression = false;
			mpegVideoOut.fixedSizeSamples = true;

			//Mpeg2ProgramVideo=new byte[Mpeg2ProgramVideo.GetLength(0)];
			mpegVideoOut.formatType = FormatType.Mpeg2Video;
			mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0) ;
			mpegVideoOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem( mpegVideoOut.formatSize);
			System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo,0,mpegVideoOut.formatPtr,mpegVideoOut.formatSize) ;

            AMMediaType mpegAudioOut = new AMMediaType();
            mpegAudioOut.majorType = MediaType.Audio;
            mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
            mpegAudioOut.sampleSize = 0;
            mpegAudioOut.temporalCompression = false;
            mpegAudioOut.fixedSizeSamples = true;
            mpegAudioOut.unkPtr = IntPtr.Zero;
            mpegAudioOut.formatType = FormatType.WaveEx;
            mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
            mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
            System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
            ////IPin pinVideoOut,pinAudioOut;
 
            hr=m_demuxInterface.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_demuxVideoPin);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED to create video output pin on demuxer");
				return;
			}
            hr = m_demuxInterface.CreateOutputPin(ref mpegAudioOut, "audio", out m_demuxAudioPin);
            if (hr != 0)
            {
                Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED to create audio output pin on demuxer");
                return;
            }

			hr=SetupDemuxer(m_demuxVideoPin,videoPid,m_demuxAudioPin,audioPid);
			if(hr!=0)//ignore audio pin
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to config Demuxer");
				return;
			}
			


			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:SetDemux() done:{0}", hr);
			//int //=0;
		}
		/// <summary>
		/// Returns the current tv channel
		/// </summary>
		/// <returns>Current channel</returns>
		public int GetChannelNumber()
		{
			return m_iChannelNr;
		}

		/// <summary>
		/// Property indiciating if the graph supports timeshifting
		/// </summary>
		/// <returns>boolean indiciating if the graph supports timeshifting</returns>
		public bool SupportsTimeshifting()
		{
				return true;
		}


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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing()");
			TuneChannel(channel);
			int hr=0;
			bool setVisFlag=false;
			
			if(m_channelFound==false)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() channel not found");
				return false;
			}
			AddPreferredCodecs(true,true);
			
			if(Vmr9.UseVMR9inMYTV)
			{
				Vmr9.AddVMR9(m_graphBuilder);
			}


			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() Using plugins");
			IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
			IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
			
			hr=m_graphBuilder.Connect(m_data0,samplePin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
				return false;
			}


			if (samplePin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
				return false;
			}
			if (demuxInPin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
				return false;
			}

			samplePin=null;
			samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
			if(samplePin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find sampleGrabber output pin");
				return false;
			}
			hr=m_graphBuilder.Connect(samplePin,demuxInPin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: connect sample->demux");
				return false;
			}

			SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
			
			if(m_demuxVideoPin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demux video output pin");
				return false;
			}
			if (m_demuxAudioPin == null)
			{
				Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux audio output pin");
				return false;
			}

			hr=m_graphBuilder.Render(m_demuxVideoPin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot render demux video output pin");
				return false;
			}
			hr = m_graphBuilder.Render(m_demuxAudioPin);
			if (hr != 0)
			{
				Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot render demux audio output pin");
				return false;
			}
			//
			//DsROT.AddGraphToRot(m_graphBuilder,out m_myCookie);
			if(demuxInPin!=null)
				Marshal.ReleaseComObject(demuxInPin);
			if(samplePin!=null)
				Marshal.ReleaseComObject(samplePin);

			//

			
			if(Vmr9.IsVMR9Connected==false && Vmr9.UseVMR9inMYTV==true)// fallback
			{
				if(Vmr9.VMR9Filter!=null)
					m_graphBuilder.RemoveFilter(Vmr9.VMR9Filter);
				Vmr9.RemoveVMR9();
				Vmr9.UseVMR9inMYTV=false;
			}
			//
			//
			if(Vmr9.IsVMR9Connected==true && Vmr9.UseVMR9inMYTV==true)// fallback
			{
				//m_vmr9Running=true;
			}
			//
			//
			m_mediaControl = (IMediaControl)m_graphBuilder;
			if (!Vmr9.UseVMR9inMYTV )
			{

				m_videoWindow = (IVideoWindow) m_graphBuilder as IVideoWindow;
				if (m_videoWindow==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:FAILED:Unable to get IVideoWindow");
				}

				m_basicVideo = (IBasicVideo2)m_graphBuilder as IBasicVideo2;
				if (m_basicVideo==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:FAILED:Unable to get IBasicVideo2");
				}
				hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window:0x{0:X}",hr);
				}
				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window style:0x{0:X}",hr);
				}
				setVisFlag=true;

			}

			m_bOverlayVisible=true;
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();
			//
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() start graph");

			if (Vmr9!=null) Vmr9.SetDeinterlaceMode();
			m_mediaControl.Run();
				
			if(setVisFlag)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() show video window");
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:put_Visible:0x{0:X}",hr);
				}

			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() startviewing done");
			if(m_streamDemuxer!=null)
			{
				
				//m_streamDemuxer.GetEPGSchedule(0x50,m_currentChannel.ProgramNumber);
				//int a=0;
			}
			return true;

		}

		int GetPidNumber(string pidText,int number)
		{
			if(pidText=="")
				return 0;
			string[] pidSegments;

			pidSegments=pidText.Split(new char[]{';'});
			if(pidSegments.Length-1<number || pidSegments.Length==0)
				return -1;

			string[] pid=pidSegments[number-1].Split(new char[]{'/'});
			if(pid.Length!=2)
				return -1;

			try
			{
				return Convert.ToInt16(pid[0]);
			}
			catch
			{
				return -1;
			}
		}
		int GetPidID(string pidText,int number)
		{
			if(pidText=="")
				return 0;
			string[] pidSegments;

			pidSegments=pidText.Split(new char[]{';'});
			if(pidSegments.Length-1<number || pidSegments.Length==0)
				return 0;

			string[] pid=pidSegments[number-1].Split(new char[]{'/'});
			if(pid.Length!=2)
				return 0;

			try
			{
				return Convert.ToInt16(pid[1]);
			}
			catch
			{
				return 0;
			}
		}

		/// <summary>
		/// Stops viewing the TV channel 
		/// </summary>
		/// <returns>boolean indicating if succeed</returns>
		/// <remarks>
		/// Graph must be viewing first with StartViewing()
		/// </remarks>
		public bool StopViewing()
		{
			if (m_graphState != State.Viewing) 
				return false;
			
			GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			DirectShowUtil.DebugWrite("DVBGraphSS2:StopViewing()");
			if(m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
			m_bOverlayVisible=false;

			if (Vmr9!=null)
			{
				Vmr9.Enable(false);
			}
			m_mediaControl.Stop();
			m_mediaControl=null;
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}

		//
		public bool ShouldRebuildGraph(int iChannel)
		{
			return false;
		}

		/// <summary>
		/// This method returns whether a signal is present. Meaning that the
		/// TV tuner (or video input) is tuned to a channel
		/// </summary>
		/// <returns>true:  tvtuner is tuned to a channel (or video-in has a video signal)
		///          false: tvtuner is not tuned to a channel (or video-in has no video signal)
		/// </returns>
		public bool SignalPresent()
		{
				return true;
		}
		
		public int  SignalQuality()
		{
			return 100;
		}
		
		public int  SignalStrength()
		{
			return 100;
		}

		/// <summary>
		/// This method returns the frequency to which the tv tuner is currently tuned
		/// </summary>
		/// <returns>frequency in Hertz
		/// </returns>
		public long VideoFrequency() 
		{
			return 0;
		}
		
		public void Process()
		{
			//
			if(GUIGraphicsContext.Vmr9Active && Vmr9!=null)
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
				}
			}




		}
		
		public PropertyPageCollection PropertyPages()
		{
			return null;
		}
		
		public IBaseFilter AudiodeviceFilter()
		{
			return null;
		}

		public bool SupportsFrameSize(Size framesize)
		{	
			return false;
		}
		public NetworkType Network()
		{
				return NetworkType.DVBS;
		}
		//
		public void Tune(object tuningObject, int disecqNo)
		{
			
			DVBChannel ch=(DVBChannel)tuningObject;
			ch=LoadDiseqcSettings(ch,disecqNo);
			m_currentTuningObject=new DVBChannel();
			if(m_mpeg2Data==null)
				return;
			try
			{
				if(m_mediaControl==null)
				{
					m_graphBuilder.Render(m_data0);
					m_mediaControl=m_graphBuilder as IMediaControl;
					m_mediaControl.Run();
				}
			}
			catch{}
			if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,-1,-1,ch.LNBFrequency,0,0,0,0,"",0)==false)
			{
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: FAILED to tune channel");
				return;
			}
			else
				Log.WriteFile(Log.LogType.Capture,"called Tune(object)");
			m_currentTuningObject=ch;

		}
		//
		public void StoreChannels(int ID,bool radio, bool tv, ref int newChannels, ref int updatedChannels)
		{
			Log.WriteFile(Log.LogType.Capture,"called StoreChannels()");
			if (m_mpeg2Data==null) return;


			//get list of current tv channels present in the database
			ArrayList tvChannels = new ArrayList();
			TVDatabase.GetChannels(ref tvChannels);

			DeleteAllPIDs(m_dataCtrl,0);
			SetPidToPin(m_dataCtrl,0,0);
			SetPidToPin(m_dataCtrl,0,16);
			SetPidToPin(m_dataCtrl,0,17);
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: StoreChannels()");
			DVBSections sections = new DVBSections();
			sections.SetPidsForTechnisat=true;
			sections.DataControl=m_dataCtrl;
			sections.Timeout=5000;
			
			DVBSections.Transponder transp = sections.Scan(m_mpeg2Data);
			if (transp.channels==null)
			{
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: found no channels", transp.channels);
				return;
			}
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: found {0} channels", transp.channels.Count);
			for (int i=0; i < transp.channels.Count;++i)
			{
				System.Windows.Forms.Application.DoEvents();
				System.Windows.Forms.Application.DoEvents();


				int audioOptions=0;

				DVBSections.ChannelInfo info=(DVBSections.ChannelInfo)transp.channels[i];
				if (info.service_provider_name==null) info.service_provider_name="";
				if (info.service_name==null) info.service_name="";
				
				info.service_provider_name=info.service_provider_name.Trim();
				info.service_name=info.service_name.Trim();
				if (info.service_provider_name.Length==0 ) 
					info.service_provider_name="Unknown";
				if (info.service_name.Length==0)
					info.service_name=String.Format("NoName:{0}{1}{2}{3}",info.networkID,info.transportStreamID, info.serviceID,i );

				if (info.serviceID==0) 
				{
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel#{0} has no service id",i);
					continue;
				}
				bool hasAudio=false;
				bool hasVideo=false;
				info.freq=m_currentTuningObject.Frequency;
				DVBChannel newchannel   = new DVBChannel();

				//check if this channel has audio/video streams
				if (info.pid_list!=null)
				{
					audioOptions=0;
					for (int pids =0; pids < info.pid_list.Count;pids++)
					{
						DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
						if(data.isAudio && hasAudio==true && audioOptions<2)
						{
							switch(audioOptions)
							{
								case 0:
									newchannel.Audio1=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											newchannel.AudioLanguage1=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions=1;
									break;
								case 1:
									newchannel.Audio2=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											newchannel.AudioLanguage2=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions=2;
									break;

							}
						}
						if (data.isAC3Audio)
						{
							m_currentTuningObject.AC3Pid=data.elementary_PID;
						}
						if (data.isVideo)
						{
							m_currentTuningObject.VideoPid=data.elementary_PID;
							hasVideo=true;
						}
						if (data.isAudio && hasAudio==false)
						{
							m_currentTuningObject.AudioPid=data.elementary_PID;
							if(data.data!=null)
							{
								if(data.data.Length==3)
									newchannel.AudioLanguage=DVBSections.GetLanguageFromCode(data.data);
							}
							hasAudio=true;
						}
						if (data.isTeletext)
						{
							m_currentTuningObject.TeletextPid=data.elementary_PID;
						}
						if(data.isDVBSubtitle)
						{
							m_currentTuningObject.Audio3=data.elementary_PID;
						}
					}
				}
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:{9} videopid:{10} teletextpid:{11}", 
					info.service_provider_name,
					info.service_name,
					info.scrambled,
					info.freq,
					info.networkID,
					info.transportStreamID,
					info.serviceID,
					hasVideo, ((!hasVideo) && hasAudio),
					m_currentTuningObject.AudioPid,m_currentTuningObject.VideoPid,m_currentTuningObject.TeletextPid);
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
				newchannel.Polarity = m_currentTuningObject.Polarity;
				newchannel.Modulation = m_currentTuningObject.Modulation;
				newchannel.Symbolrate = m_currentTuningObject.Symbolrate;
				newchannel.ServiceType=info.serviceType;//tv
				newchannel.PCRPid=info.pcr_pid;
				newchannel.PMTPid=info.network_pmt_PID;
				newchannel.LNBFrequency=m_currentTuningObject.LNBFrequency;
				newchannel.LNBKHz=m_currentTuningObject.LNBKHz;
				newchannel.DiSEqC=m_currentTuningObject.DiSEqC;
				newchannel.AudioPid=m_currentTuningObject.AudioPid;
				newchannel.VideoPid=m_currentTuningObject.VideoPid;
				newchannel.TeletextPid=m_currentTuningObject.TeletextPid;
				newchannel.AC3Pid=m_currentTuningObject.AC3Pid;
				newchannel.HasEITSchedule=info.eitSchedule;
				newchannel.HasEITPresentFollow=info.eitPreFollow;
				newchannel.AudioLanguage3=info.pidCache;
			
				if (info.serviceType==1 && tv)
				{
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a tv channel",newchannel.ServiceName);
					//check if this channel already exists in the tv database
					bool isNewChannel=true;
					int iChannelNumber=0;
					int channelId=-1;
					foreach (TVChannel tvchan in tvChannels)
					{
						if (tvchan.Name.Equals(newchannel.ServiceName))
						{
							//yes already exists
							iChannelNumber=tvchan.Number;
							isNewChannel=false;
							channelId=tvchan.ID;
							break;
						}
					}

					//if the tv channel found is not yet in the tv database
					TVChannel tvChan = new TVChannel();
					tvChan.Name=newchannel.ServiceName;
					tvChan.Number=newchannel.ProgramNumber;
					tvChan.VisibleInGuide=true;
					tvChan.Scrambled=newchannel.IsScrambled;
					if (isNewChannel)
					{
						//then add a new channel to the database
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: create new tv channel for {0}",newchannel.ServiceName);
						iChannelNumber=tvChan.Number;
						int id=TVDatabase.AddChannel(tvChan);
						channelId=id;
						newChannels++;
					}
					else
					{
						tvChan.ID=channelId;
						TVDatabase.UpdateChannel(tvChan,tvChan.Sort);
						updatedChannels++;
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} already exists in tv database",newchannel.ServiceName);
					}
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
					newchannel.ID=channelId;
					TVDatabase.AddSatChannel(newchannel);
					//}
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
				else
				{
					if(info.serviceType==2)
					{
						//todo: radio channels
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a radio channel",newchannel.ServiceName);
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
							Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: create new radio channel for {0}",newchannel.ServiceName);
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

						if (Network() == NetworkType.DVBS)
						{
							Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
							newchannel.ID=channelId;

							int scrambled=0;
							if (newchannel.IsScrambled) scrambled=1;
							RadioDatabase.MapDVBSChannel(newchannel.ID,newchannel.Frequency,newchannel.Symbolrate,
								newchannel.FEC,newchannel.LNBKHz,newchannel.DiSEqC,newchannel.ProgramNumber,
								0,newchannel.ServiceProvider,newchannel.ServiceName,
								0,0,newchannel.AudioPid,0,newchannel.AC3Pid,
								0,0,0,0,scrambled,
								newchannel.Polarity,newchannel.LNBFrequency
								,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.PCRPid,
								newchannel.AudioLanguage,newchannel.AudioLanguage1,
								newchannel.AudioLanguage2,newchannel.AudioLanguage3,
								newchannel.ECMPid,newchannel.PMTPid);
						}
						RadioDatabase.MapChannelToCard(channelId,ID);
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a radio channel",newchannel.ServiceName);
					}
				}
			}//for (int i=0; i < transp.channels.Count;++i)
		}

		public IBaseFilter Mpeg2DataFilter()
		{
			return m_mpeg2Data;
		}

		DVBChannel LoadDiseqcSettings(DVBChannel ch,int disNo)
		{
			if(m_cardFilename=="")
				return ch;

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

			using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))
			{
				lnb0MHZ=xmlreader.GetValueAsInt("dvbs","LNB0",9750);
				lnb1MHZ=xmlreader.GetValueAsInt("dvbs","LNB1",10600);
				lnbswMHZ=xmlreader.GetValueAsInt("dvbs","Switch",11700);
				cbandMHZ=xmlreader.GetValueAsInt("dvbs","CBand",5150);
				circularMHZ=xmlreader.GetValueAsInt("dvbs","Circular",10750);
//				bool useLNB1=xmlreader.GetValueAsBool("dvbs","useLNB1",false);
//				bool useLNB2=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
//				bool useLNB3=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
//				bool useLNB4=xmlreader.GetValueAsBool("dvbs","useLNB4",false);
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
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}",ch.Frequency,ch.LNBKHz,ch.LNBFrequency,ch.DiSEqC); 
			return ch;

		}// LoadDiseqcSettings()

		public void TuneRadioChannel(RadioStation station)
		{
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:TuneChannel() get DVBS tuning details");
			DVBChannel ch=new DVBChannel();
			if(RadioDatabase.GetDVBSTuneRequest(station.ID,0,ref ch)==false)//only radio
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:database invalid tuning details for channel:{0}", station.Channel);
				return;
			}
			if(Tune(ch.Frequency,ch.Symbolrate,ch.FEC,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,0,ch.LNBFrequency,0,0,ch.PMTPid,ch.PCRPid,ch.AudioLanguage3,0)==true)
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Radio tune ok");
			else
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED cannot tune");
				return;
			}

			m_currentChannel=ch;
			
			if (m_streamDemuxer != null)
			{
				m_streamDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName,ch.PMTPid);
			}

			if(m_demuxVideoPin!=null && m_demuxAudioPin!=null)
				SetupDemuxer(m_demuxVideoPin,m_currentChannel.VideoPid,m_demuxAudioPin,m_currentChannel.AudioPid);

		}

		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio) 
			{
				if(m_graphState!=State.Created)
					return;

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: start radio");

				int hr=0;
				AddPreferredCodecs(true,false);
			
				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9=null;
				}


				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() Using plugins");
				IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	

				if (samplePin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
					return ;
				}
				if (demuxInPin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
					return ;
				}

				hr=m_graphBuilder.Connect(m_data0,samplePin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
					return ;
				}
				samplePin=null;
				samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
				if(samplePin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot find sampleGrabber output pin");
					return ;
				}
				hr=m_graphBuilder.Connect(samplePin,demuxInPin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: connect sample->demux");
					return ;
				}

				SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
			
				if(m_demuxAudioPin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot find demux audio output pin");
					return ;
				}

				hr=m_graphBuilder.Render(m_demuxAudioPin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot render demux audio output pin");
					return ;
				}
				//
				//DsROT.AddGraphToRot(m_graphBuilder,out m_myCookie);
				if(demuxInPin!=null)
					Marshal.ReleaseComObject(demuxInPin);
				if(samplePin!=null)
					Marshal.ReleaseComObject(samplePin);

				//

			
				//
				//
				m_mediaControl = (IMediaControl)m_graphBuilder;
				m_graphState = State.Radio;
				//
				m_mediaControl.Run();


			}

			// tune to the correct channel
			TuneRadioChannel(station);
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:Listening to radio..");
		}
		public void TuneRadioFrequency(int frequency)
		{
		}
		public bool HasTeletext()
		{
			if (m_graphState!= State.TimeShifting && m_graphState!=State.Recording && m_graphState!=State.Viewing) return false;
			if (m_currentChannel==null) return false;
			if (m_currentChannel.TeletextPid>0) return true;
			return false;
		}
		#region Stream-Audio handling
		public int GetAudioLanguage()
		{
			return m_selectedAudioPid;
		}
		public void SetAudioLanguage(int audioPid)
		{
			if(audioPid!=m_selectedAudioPid)
			{
				int hr=SetupDemuxer(m_demuxVideoPin,m_currentChannel.VideoPid,m_demuxAudioPin,audioPid);
                if (hr != 0)
                {
                    Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}", hr.ToString());
                    return;
                }
                else
                {
                    m_selectedAudioPid = audioPid;
                    if(m_streamDemuxer!=null)
                        m_streamDemuxer.SetChannelData(audioPid, m_currentChannel.VideoPid, m_currentChannel.TeletextPid, m_currentChannel.Audio3, m_currentChannel.ServiceName,m_currentChannel.PMTPid);

                }
			}
		}

		public ArrayList GetAudioLanguageList()
		{
				
			DVBSections.AudioLanguage al;
			ArrayList alList=new ArrayList();
			if(m_currentChannel==null) return alList;
			if(m_currentChannel.AudioPid!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.AudioPid;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage;
				alList.Add(al);
			}
			if(m_currentChannel.Audio1!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.Audio1;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage1;
				alList.Add(al);
			}
			if(m_currentChannel.Audio2!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.Audio2;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage2;
				alList.Add(al);
			}
			return alList;
		}
		#endregion

		private void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
		{
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
			if(pid==0x12 && (tableID>=0x50 && tableID<=0x6f))
			{
				int count=m_streamDemuxer.ProcessEPGData(tableList,m_currentChannel.ProgramNumber);
				Log.Write("added {0} events to database. grabbing ready",count);
			}
		}
	}// class
}// namespace
