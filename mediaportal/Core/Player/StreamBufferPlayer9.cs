using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DShowNET;

namespace MediaPortal.Player 
{
  public class StreamBufferPlayer9 : BaseStreamBufferPlayer
  {
		VMR9Util Vmr9 = null;
		bool		 inFullscreenMode=false;
    public StreamBufferPlayer9()
    {
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
			
			Log.Write("StreamBufferPlayer9: GetInterfaces()");
			Vmr9= new VMR9Util("mytv");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
			Log.Write("StreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);
			inFullscreenMode=true;

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.Write("StreamBufferPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
        Vmr9.AddVMR9(graphBuilder);				

				// create SBE source
        Guid clsid = Clsid.StreamBufferSource;
        Guid riid = typeof(IStreamBufferSource).GUID;
        Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
        bufferSource = (IStreamBufferSource) comObj; comObj = null;
        if (bufferSource==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to create instance of SBE (do you have WinXp SP1?)");
          return false;
        }	

		
        IBaseFilter filter = (IBaseFilter) bufferSource;
        int hr=graphBuilder.AddFilter(filter, "SBE SOURCE");
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }	
		
        IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
        if (fileSource==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }	
        hr = fileSource.Load(filename, IntPtr.Zero);
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}",filename,hr);
          return false;
        }	


				// add preferred video & audio codecs
				string strVideoCodec="";
        string strAudioCodec="";
        bool   bAddFFDshow=false;
				using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
				}
				if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
        if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        m_mediaSeeking = bufferSource as IStreamBufferMediaSeeking ;
        if (m_mediaSeeking==null)
        {
          Log.Write("StreamBufferPlayer9:Unable to get IMediaSeeking interface#1");
          return false;
        }
        
//        Log.Write("StreamBufferPlayer9:SetARMode");
//        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);

        Log.Write("StreamBufferPlayer9: set Deinterlace");
        DirectShowUtil.EnableDeInterlace(graphBuilder);

				if ( !Vmr9.IsVMR9Connected )
				{
					//VMR9 is not supported, switch to overlay
					Log.Write("StreamBufferPlayer9: switch to overlay");
					Cleanup(false);
					return base.GetInterfaces(filename);
				}

        return true;
      }
      catch( Exception  ex)
      {
        Log.Write("StreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }




    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
		{
			if (Vmr9!=null)
			{
				if (!Vmr9.IsVMR9Connected)
				{
					Vmr9.RemoveVMR9();
					base.CloseInterfaces();
					return;
				}
			}
			Cleanup(true );
		}

		void Cleanup(bool switchToWindowedMode)
		{
      //lock(this)
      {
        int hr;
        Log.Write("StreamBufferPlayer9:cleanup DShow graph");
        try 
        {
          //          Log.Write("StreamBufferPlayer9:stop graph");
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
          }
          
//          Log.Write("StreamBufferPlayer9:stop notifies");
          if( mediaEvt != null )
          {
            hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
            mediaEvt = null;
          }

			//added from agree: check if Vmr9 already null
			if(Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9=null;
			}

          basicAudio	= null;
          m_mediaSeeking=null;
          mediaCtrl = null;
	
		      DsUtils.RemoveFilters(graphBuilder);

          if( rotCookie != 0 )
            DsROT.RemoveGraphFromRot( ref rotCookie );
          rotCookie=0;

          if( graphBuilder != null )
            Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;


          GUIGraphicsContext.form.Invalidate(true);
          //GUIGraphicsContext.form.Update();
          //GUIGraphicsContext.form.Validate();
          m_state = PlayState.Init;
          GC.Collect();
//          Log.Write("StreamBufferPlayer9:cleaned up");
        }
        catch( Exception ex)
        {
          Log.Write("StreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
        }

        //switch back to directx windowed mode
				if (switchToWindowedMode && inFullscreenMode)
				{
					Log.Write("StreamBufferPlayer9: switch to windowed mode");
					inFullscreenMode=false;
					GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
					GUIWindowManager.SendMessage(msg);
				}
      }
    }

    protected override void OnProcess()
    {
      if (Paused)
      {
        //repaint
        if (Vmr9!=null) Vmr9.Repaint();
      }
    }

  }
}
