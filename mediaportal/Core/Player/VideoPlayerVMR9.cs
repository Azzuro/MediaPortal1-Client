using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;


using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{


	public class VideoPlayerVMR9 : VideoPlayerVMR7
	{

		VMR9Util Vmr9 = null;
		public VideoPlayerVMR9()
		{
		}

		/// <summary> create the used COM components and get the interfaces. </summary>
		protected override bool GetInterfaces()
		{
			Vmr9 = new VMR9Util("movieplayer");
      //switch back to directx fullscreen mode
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);

			Type comtype = null;
			object comobj = null;
			RGB color;
			color.red = 0;
			color.green = 0;
			color.blu = 0;

			DsRECT rect = new DsRECT();
			rect.Top = 0;
			rect.Bottom =GUIGraphicsContext.form.Height;
			rect.Left = 0;
			rect.Right = GUIGraphicsContext.form.Width;
				

			try 
			{
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
          Error.SetError("Unable to play movie","Directx9 is not installed");
					Log.WriteFile(Log.LogType.Log,true,"VideoPlayer9:DirectX 9 not installed");
					return false;
				}
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
				Vmr9.AddVMR9(graphBuilder);

        // add preferred video & audio codecs
        string strVideoCodec="";
        string strAudioCodec="";
        bool   bAddFFDshow=false;
        using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("movieplayer","ffdshow",false);
          strVideoCodec=xmlreader.GetValueAsString("movieplayer","mpeg2videocodec","");
          strAudioCodec=xmlreader.GetValueAsString("movieplayer","mpeg2audiocodec","");
        }
        string strExt=System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
        if (strExt.Equals(".mpg") ||strExt.Equals(".mpeg")||strExt.Equals(".bin")||strExt.Equals(".dat"))
        {
          //if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
          //if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
        }
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");


				int hr = DsUtils.RenderFileToVMR9(graphBuilder, m_strCurrentFile, Vmr9.VMR9Filter, false);
        if (hr!=0) 
        {
          Error.SetError("Unable to play movie","Unable to render file. Missing codecs?");
          Log.WriteFile(Log.LogType.Log,true,"VideoPlayer9:Failed to render file -> vmr9");
          return false;
        }
        
        mediaCtrl	= (IMediaControl)  graphBuilder;
				mediaEvt	= (IMediaEventEx)  graphBuilder;
				mediaSeek	= (IMediaSeeking)  graphBuilder;
				mediaPos	= (IMediaPosition) graphBuilder;
				basicAudio	= graphBuilder as IBasicAudio;
				//DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);
				DirectShowUtil.EnableDeInterlace(graphBuilder);
        m_iVideoWidth=Vmr9.VideoWidth;
        m_iVideoHeight=Vmr9.VideoHeight;

        ushort b;
        unchecked
        {
          b=(ushort)0xfffff845;
        }
        Guid classID=new Guid(0x9852a670,b,0x491b,0x9b,0xe6,0xeb,0xd8,0x41,0xb8,0xa6,0x13);
        IBaseFilter filter;
        DsUtils.FindFilterByClassID(graphBuilder,  classID, out filter);
        vobSub = null;
        vobSub = filter as IDirectVobSub;
        if (vobSub!=null)
        {
          using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
          {
            string strTmp="";
            string strFont=xmlreader.GetValueAsString("subtitles","fontface","Arial");
            int    iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
            bool   bBold=xmlreader.GetValueAsBool("subtitles","bold",true);
            
            strTmp=xmlreader.GetValueAsString("subtitles","color","ffffff");
            long iColor=Convert.ToInt64(strTmp,16);
            int  iShadow=xmlreader.GetValueAsInt("subtitles","shadow",5);
          
            LOGFONT logFont = new LOGFONT();
            int txtcolor;
            bool fShadow, fOutLine, fAdvancedRenderer = false;
            int size = Marshal.SizeOf(typeof(LOGFONT));
            vobSub.get_TextSettings(logFont, size,out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);

            FontStyle fontStyle=FontStyle.Regular;
            if (bBold) fontStyle=FontStyle.Bold;
						System.Drawing.Font Subfont = new System.Drawing.Font(strFont,iFontSize,fontStyle,System.Drawing.GraphicsUnit.Point, 1);
            Subfont.ToLogFont(logFont);
            int R=(int)((iColor>>16)&0xff);
            int G=(int)((iColor>>8)&0xff);
            int B=(int)((iColor)&0xff);
            txtcolor=(B<<16)+(G<<8)+R;
            if (iShadow>0) fShadow=true;
            int res = vobSub.put_TextSettings(logFont, size, txtcolor,  fShadow, fOutLine, fAdvancedRenderer);
          }
        }
        if( filter != null )
          Marshal.ReleaseComObject( filter ); filter = null;


				if ( !Vmr9.IsVMR9Connected )
				{
					//VMR9 is not supported, switch to overlay
					mediaCtrl=null;
					Cleanup();
					return base.GetInterfaces();
				}
        
				return true;
			}
			catch( Exception  ex)
			{
        Error.SetError("Unable to play movie","Unable build graph for VMR9");
				Log.WriteFile(Log.LogType.Log,true,"VideoPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
				return false;
			}
		}

		protected override void OnProcess()
		{
			if (Vmr9!=null)
			{
				m_iVideoWidth=Vmr9.VideoWidth;
				m_iVideoHeight=Vmr9.VideoHeight;
			}
			if(Paused  || (GUIGraphicsContext.Vmr9Active && Vmr9!=null))
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
				}
			}
		}

		/// <summary> do cleanup and release DirectShow. </summary>
		protected override void CloseInterfaces()
		{
			Cleanup();
		}

		void Cleanup()
		{
      if (graphBuilder==null) return;
      int hr;
      Log.Write("VideoPlayer9:cleanup DShow graph");
      try 
      {
        if( mediaCtrl != null )
        {
          hr = mediaCtrl.Stop();
          System.Threading.Thread.Sleep(500);
          System.Threading.Thread.Sleep(500);
        }
  	    if( mediaEvt != null )
        {
          hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
          mediaEvt = null;
        }



				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}

        mediaSeek	= null;
        mediaPos	= null;
        basicAudio	= null;
        mediaCtrl = null;

        if( vobSub != null )
					Marshal.ReleaseComObject( vobSub ); vobSub = null;
		
				DsUtils.RemoveFilters(graphBuilder);

        if( rotCookie != 0 )
          DsROT.RemoveGraphFromRot( ref rotCookie );
        rotCookie=0;

        if( graphBuilder != null )
          Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;


        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;
        GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"VideoPlayer9:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
      GUIWindowManager.SendMessage(msg);
		}
	}
}
