using System;
using System.Drawing;
using Microsoft.Win32;
using DShowNET;
using MediaPortal.GUI.Library;

using System.Runtime.InteropServices;
namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// Summary description for Dvrms2XVID.
	/// </summary>
	public class Dvrms2XVID : ITranscode
	{
		protected int												rotCookie = 0;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected IFileSinkFilter2										fileWriterFilter = null;			// DShow Filter: file writer
		protected IMediaControl											mediaControl=null;
		protected IStreamBufferMediaSeeking											mediaSeeking=null;
		protected IMediaPosition										mediaPos=null;
		protected IBaseFilter												xvidCodec =null;
		protected IBaseFilter												mp3Codec =null;
		protected IBaseFilter												Mpeg2VideoCodec =null;
		protected IBaseFilter												Mpeg2AudioCodec =null;
		protected IBaseFilter												aviMuxer =null;
		protected IMediaEventEx											mediaEvt=null;
		protected int bitrate;
		protected int fps;
		protected Size screenSize;
		protected long m_dDuration;
		protected const int WS_CHILD			= 0x40000000;	// attributes for video window
		protected const int WS_CLIPCHILDREN	= 0x02000000;
		protected const int WS_CLIPSIBLINGS	= 0x04000000;
		public Dvrms2XVID()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region ITranscode Members

		public bool Supports(MediaPortal.Core.Transcoding.VideoFormat format)
		{
			if (format==VideoFormat.Xvid) return true;
			return false;
		}
		public void CreateProfile(Size videoSize, int bitRate, int FPS)
		{
			bitrate=bitRate;
			screenSize=videoSize;
			fps=FPS;

		}

		public bool Transcode(TranscodeInfo info, MediaPortal.Core.Transcoding.VideoFormat format, MediaPortal.Core.Transcoding.Quality quality)
		{
			if (!Supports(format)) return false;
			string ext=System.IO.Path.GetExtension(info.file);
			if (ext.ToLower() !=".dvr-ms" && ext.ToLower() !=".sbe" ) return false;

			//disable xvid status window while encoding
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				RegistryKey subkey = hkcu.OpenSubKey(@"Software\GNU\XviD",true);
				if (subkey != null)
				{
					Int32 uivalue=0;
					subkey.SetValue("display_status", (Int32)uivalue);
					subkey.SetValue("debug", (Int32)uivalue);
					subkey.SetValue("bitrate", (Int32)bitrate);

					uivalue=1;
					subkey.SetValue("interlacing", (Int32)uivalue);
					subkey.Close();
				}
				hkcu.Close();
			}
			catch(Exception)
			{
			}
			Type comtype = null;
			object comobj = null;
			try 
			{
				Log.Write("DVR2XVID: create graph");
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
					Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:DirectX 9 not installed");
					return false;
				}
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
				DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph

				Log.Write("DVR2XVID: add streambuffersource");
				Guid clsid = Clsid.StreamBufferSource;
				Guid riid = typeof(IStreamBufferSource).GUID;
				Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
				bufferSource = (IStreamBufferSource) comObj; comObj = null;

		
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "SBE SOURCE");		
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				Log.Write("DVR2XVID: load file:{0}",info.file);
				int hr = fileSource.Load(info.file, IntPtr.Zero);



				//add mpeg2 audio/video codecs
				string strVideoCodecMoniker=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{F50B3F13-19C4-11CF-AA9A-02608C9BABA2}";
				string strAudioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
				Log.Write("DVR2XVID: add elecard mpeg2 video codec");
				Mpeg2VideoCodec = Marshal.BindToMoniker( strVideoCodecMoniker ) as IBaseFilter;
				if (Mpeg2VideoCodec==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to add Elecard mpeg2 video decoder");
					Cleanup();
					return false;
				}
				hr = graphBuilder.AddFilter( Mpeg2VideoCodec , "Elecard mpeg2 video decoder" );
				if( hr != 0 ) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add Elecard mpeg2 video  to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				Log.Write("DVR2XVID: add mpeg2 audio codec:{0}", strAudioCodec);
				Mpeg2AudioCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (Mpeg2AudioCodec==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to add mpeg2 audio codec");
					Cleanup();
					return false;
				}

				//connect output #0 of streambuffer source->mpeg2 audio codec pin 1
				//connect output #1 of streambuffer source->mpeg2 video codec pin 1
				Log.Write("DVR2XVID: connect streambufer source->mpeg audio/video decoders");				
				IPin pinOut0, pinOut1;
				IPin pinIn0, pinIn1;
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,0,out pinOut0);//audio
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,1,out pinOut1);//video
				if (pinOut0==null || pinOut1==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to get pins of source");
					Cleanup();
					return false;
				}

				DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Input,0,out pinIn0);//video
				DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Input,0,out pinIn1);//audio
				if (pinIn0==null || pinIn1==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to get pins of mpeg2 video/audio codec");
					Cleanup();
					return false;
				}
				
				hr=graphBuilder.Connect(pinOut0,pinIn1);
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				
				hr=graphBuilder.Connect(pinOut1,pinIn0);
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				if (!AddCodecs(graphBuilder, info)) return false;

				Log.Write("DVR2XVID: start transcoding");
				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				if (hr!=0)
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);
				mediaControl= graphBuilder as IMediaControl;
				mediaSeeking= bufferSource as IStreamBufferMediaSeeking;
				mediaEvt    = graphBuilder as IMediaEventEx;
				mediaPos    = graphBuilder as IMediaPosition;

				//get file duration
				Log.Write("DVR2XVID: Get duration of movie");				
				long lTime=5*60*60;
				lTime*=10000000;
				long pStop=0;
				hr=mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				if (hr==0)
				{
					long lStreamPos;
					mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
					m_dDuration=lStreamPos;
					lTime=0;
					mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				}
				double duration=m_dDuration/10000000d;
				Log.Write("DVR2XVID: movie duration:{0}",Util.Utils.SecondsToHMSString((int)duration));				

				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				if (hr!=0)
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);
				hr=mediaControl.Run();
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				int maxCount=20;
				while (true)
				{
					long lCurrent;
					mediaSeeking.GetCurrentPosition(out lCurrent);
					double dpos=(double)lCurrent;
					dpos/=10000000d;
					System.Threading.Thread.Sleep(100);
					if (dpos >=2.0d) break;
					maxCount--;
					if (maxCount<=0) break;
				}

				mediaControl.Stop();
				FilterState state;
				mediaControl.GetState(500,out state);
				GC.Collect();GC.Collect();GC.Collect();GC.WaitForPendingFinalizers();
				graphBuilder.RemoveFilter(aviMuxer);
				graphBuilder.RemoveFilter(xvidCodec);
				graphBuilder.RemoveFilter(mp3Codec);
				graphBuilder.RemoveFilter((IBaseFilter)fileWriterFilter);
				if (!AddCodecs(graphBuilder, info)) return false;

				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				if (hr!=0)
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);

				hr=mediaControl.Run();
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:Unable create graph", ex.Message);
				Cleanup();
				return false;
			}
			return true;
		}

		public bool IsFinished()
		{
			if (mediaControl==null) return true;
			FilterState state;

			mediaControl.GetState(200, out state);
			if (state==FilterState.Stopped)
			{
				Cleanup();
				return true;
			}
			int p1, p2, hr = 0;
			DsEvCode code;
			hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
			hr = mediaEvt.FreeEventParams( code, p1, p2 );
			if( code == DsEvCode.Complete || code== DsEvCode.ErrorAbort)
			{
				Cleanup();
				return true;
			}
			return false;
		}

		public int Percentage()
		{
			if (mediaSeeking==null) return 100;
			long lCurrent;
			mediaSeeking.GetCurrentPosition(out lCurrent);
			float percent = ((float)lCurrent) / ((float)m_dDuration);
			percent*=50.0f;
			if (percent >100) percent=100;
			return (int)percent;
		}

		public bool IsTranscoding()
		{
			if (IsFinished()) return false;
			return true;
		}

		void Cleanup()
		{
			Log.Write("DVR2XVID: cleanup");				
			if( rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref rotCookie );

			if( mediaControl != null )
			{
				mediaControl.Stop();
				mediaControl = null;
			}
			mediaSeeking=null;
			mediaEvt=null;
			mediaPos=null;
			mediaControl=null;

			if ( xvidCodec != null )
				Marshal.ReleaseComObject( xvidCodec );
			xvidCodec=null;
			
			if ( mp3Codec != null )
				Marshal.ReleaseComObject( mp3Codec );
			mp3Codec=null;

			if ( aviMuxer != null )
				Marshal.ReleaseComObject( aviMuxer );
			aviMuxer=null;
			
			if ( Mpeg2AudioCodec != null )
				Marshal.ReleaseComObject( Mpeg2AudioCodec );
			Mpeg2AudioCodec=null;
			
			if ( Mpeg2VideoCodec != null )
				Marshal.ReleaseComObject( Mpeg2VideoCodec );
			Mpeg2VideoCodec=null;

			if ( fileWriterFilter != null )
				Marshal.ReleaseComObject( fileWriterFilter );
			fileWriterFilter=null;

			if ( bufferSource != null )
				Marshal.ReleaseComObject( bufferSource );
			bufferSource = null;

			DsUtils.RemoveFilters(graphBuilder);

			if( graphBuilder != null )
				Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
			GC.Collect();
			GC.Collect();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		#endregion
		bool AddCodecs(IGraphBuilder graphBuilder, TranscodeInfo info)
		{
			int hr;
			Log.Write("DVR2XVID: add XVID codec to graph");				
			string monikerXVID=@"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\xvid";
			xvidCodec = Marshal.BindToMoniker( monikerXVID ) as IBaseFilter;
			if (xvidCodec==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Unable to create XviD MPEG-4 Codec");
				Cleanup();
				return false;
			}

			hr = graphBuilder.AddFilter( xvidCodec, "XviD MPEG-4 Codec" );
			if( hr != 0 ) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add XviD MPEG-4 Codec to filtergraph :0x{0:X}",hr);
				Cleanup();
				return false;
			}


			Log.Write("DVR2XVID: add MPEG3 codec to graph");				
			string monikerMPEG3=@"@device:cm:{33D9A761-90C8-11D0-BD43-00A0C911CE86}\85MPEG Layer-3";
			mp3Codec = Marshal.BindToMoniker( monikerMPEG3 ) as IBaseFilter;
			if (mp3Codec==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Unable to create MPEG Layer-3 Codec");
				Cleanup();
				return false;
			}

			hr = graphBuilder.AddFilter( mp3Codec, "MPEG Layer-3" );
			if( hr != 0 ) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add MPEG Layer-3 to filtergraph :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			//add filewriter 
			Log.Write("DVR2XVID: add FileWriter to graph");				
			string monikerFileWrite=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}";
			IBaseFilter fileWriterbase = Marshal.BindToMoniker( monikerFileWrite ) as IBaseFilter;
			if (fileWriterbase==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Unable to create FileWriter");
				Cleanup();
				return false;
			}

				
			fileWriterFilter = fileWriterbase as IFileSinkFilter2;
			if (fileWriterFilter ==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add unable to get IFileSinkFilter for filewriter");
				Cleanup();
				return false;
			}

			hr = graphBuilder.AddFilter( fileWriterbase , "FileWriter" );
			if( hr != 0 ) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add FileWriter to filtergraph :0x{0:X}",hr);
				Cleanup();
				return false;
			}


			//set output filename
			AMMediaType mt = new AMMediaType();
			string outputFileName=System.IO.Path.ChangeExtension(info.file,".avi");
			Log.Write("DVR2XVID: set output file to :{0}",outputFileName);				
			hr=fileWriterFilter.SetFileName(outputFileName, IntPtr.Zero);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to set filename for filewriter :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			// add avi muxer
			Log.Write("DVR2XVID: add AVI Muxer to graph");				
			string monikerAviMuxer=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E2510970-F137-11CE-8B67-00AA00A3F1A6}";
			aviMuxer = Marshal.BindToMoniker( monikerAviMuxer ) as IBaseFilter;
			if (aviMuxer==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Unable to create AviMux");
				Cleanup();
				return false;
			}


			hr = graphBuilder.AddFilter( aviMuxer , "AviMux" );
			if( hr != 0 ) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add AviMux to filtergraph :0x{0:X}",hr);
				Cleanup();
				return false;
			}


			//connect output of mpeg2 codec to xvid codec
			Log.Write("DVR2XVID: connect mpeg2 video codec->xvid codec");				
			IPin pinOut, pinIn;
			hr=DsUtils.GetPin(xvidCodec,PinDirection.Input,0,out pinIn);
			if (hr!=0 || pinIn==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get input pin of xvid codec:0x{0:X}",hr);
				Cleanup();
				return false;
			}
			hr=DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Output,0,out pinOut);
			if (hr!=0 || pinOut==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get output pin of mpeg2 video codec :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			hr=graphBuilder.Connect(pinOut,pinIn);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect mpeg2 video codec->xvid:0x{0:X}",hr);
				Cleanup();
				return false;
			}

			//connect output of mpeg2 audio codec to mpeg3 codec
			Log.Write("DVR2XVID: connect mpeg2 audio codec->mp3 codec");				
			hr=DsUtils.GetPin(mp3Codec,PinDirection.Input,0,out pinIn);
			if (hr!=0 || pinIn==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
				Cleanup();
				return false;
			}
			hr=DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Output,0,out pinOut);
			if (hr!=0 || pinOut==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			hr=graphBuilder.Connect(pinOut,pinIn);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect mpeg2 audio codec->mpeg3:0x{0:X}",hr);
				Cleanup();
				return false;
			}



			//connect output of mpeg3 codec to pin#0 of avimux
			Log.Write("DVR2XVID: connect mp3 codec->avimux");				
			hr=DsUtils.GetPin(mp3Codec,PinDirection.Output,0,out pinOut);
			if (hr!=0 || pinOut==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
				Cleanup();
				return false;
			}
			hr=DsUtils.GetPin(aviMuxer,PinDirection.Input,0,out pinIn);
			if (hr!=0 || pinIn==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			hr=graphBuilder.Connect(pinOut,pinIn);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect mpeg3 codec->avimux:0x{0:X}",hr);
				Cleanup();
				return false;
			}

			//connect output of xvid codec to pin#1 of avimux
			Log.Write("DVR2XVID: connect xvid codec->avimux");				
			hr=DsUtils.GetPin(xvidCodec,PinDirection.Output,0,out pinOut);
			if (hr!=0 || pinOut==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
				Cleanup();
				return false;
			}
			hr=DsUtils.GetPin(aviMuxer,PinDirection.Input,1,out pinIn);
			if (hr!=0 || pinIn==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get output#1 pin of avimux :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			hr=graphBuilder.Connect(pinOut,pinIn);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:unable to connect xvid codec->avimux:0x{0:X}",hr);
				Cleanup();
				return false;
			}


			//connect avi mux out->filewriter in
			Log.Write("DVR2XVID: connect avimux->filewriter");				
			hr=DsUtils.GetPin(aviMuxer,PinDirection.Output,0,out pinOut);
			if (hr!=0 || pinOut==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get output pin of avimux:0x{0:X}",hr);
				Cleanup();
				return false;
			}

			hr=DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn);
			if (hr!=0 || pinIn==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:cannot get input pin of Filewriter :0x{0:X}",hr);
				Cleanup();
				return false;
			}
			hr=graphBuilder.Connect(pinOut,pinIn);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:connect muxer->filewriter :0x{0:X}",hr);
				Cleanup();
				return false;
			}
			return true;
		}

		public void Stop()
		{
			if (mediaControl!=null)
			{
				mediaControl.Stop();
			}
			Cleanup();
		}
	}
}
