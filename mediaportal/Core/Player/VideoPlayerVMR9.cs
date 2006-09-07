#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;

namespace MediaPortal.Player
{


  public class VideoPlayerVMR9 : VideoPlayerVMR7
  {

    VMR9Util Vmr9 = null;
    public VideoPlayerVMR9()
    {
    }

    protected override void OnInitialized()
    {
      if (Vmr9 != null)
      {
        Vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }
    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces()
    {
      Vmr9 = new VMR9Util();

      // switch back to directx fullscreen mode
      Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      GUIWindowManager.SendMessage(msg);

      //Type comtype = null;
      //object comobj = null;

      DsRect rect = new DsRect();
      rect.top = 0;
      rect.bottom = GUIGraphicsContext.form.Height;
      rect.left = 0;
      rect.right = GUIGraphicsContext.form.Width;


      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();

        Vmr9.AddVMR9(graphBuilder);
        Vmr9.Enable(false);

        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudiorenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        {
          strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "");
          // FlipGer: load infos for custom filters
          int intCount = 0;
          while (xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
              if (xmlreader.GetValueAsBool("movieplayer", "usefilter" + intCount.ToString(), false))
              {
                  strFilters += xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") + ";";
                  intFilters++;
              }
              intCount++;
          }
        }
        string extension = System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
        if (extension.Equals(".dvr-ms") || extension.Equals(".mpg") || extension.Equals(".mpeg") || extension.Equals(".bin") || extension.Equals(".dat"))
        {
          if (strVideoCodec.Length > 0) videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
          if (strAudioCodec.Length > 0) audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
        }
        // doesn't help for Music Videos to start..
        //if (extension.Equals(".wmv"))
        //{
        //  videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, "WMVideo Decoder DMO");
        //  audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, "WMAudio Decoder DMO");
        //}

        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
            customFilters[i] = DirectShowUtil.AddFilterToGraph(graphBuilder, arrFilters[i]);
        }
        if (strAudiorenderer.Length > 0) audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);


        //int hr = graphBuilder.RenderFile(m_strCurrentFile, String.Empty);
        graphBuilder.RenderFile(m_strCurrentFile, String.Empty);
        /*if (hr != 0)
        {
          Error.SetError("Unable to play movie", "Unable to render file. Missing codecs?");
          _log.Error("VideoPlayer9:Failed to render file -> vmr9");
          return false;
        }*/

        mediaCtrl = (IMediaControl)graphBuilder;
        mediaEvt = (IMediaEventEx)graphBuilder;
        mediaSeek = (IMediaSeeking)graphBuilder;
        mediaPos = (IMediaPosition)graphBuilder;
        basicAudio = graphBuilder as IBasicAudio;
        //DirectShowUtil.SetARMode(graphBuilder,AspectRatioMode.Stretched);
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;

        ushort b;
        unchecked
        {
          b = (ushort)0xfffff845;
        }
        Guid classID = new Guid(0x9852a670, b, 0x491b, 0x9b, 0xe6, 0xeb, 0xd8, 0x41, 0xb8, 0xa6, 0x13);
        IBaseFilter filter;
        DirectShowUtil.FindFilterByClassID(graphBuilder, classID, out filter);
        vobSub = null;
        vobSub = filter as IDirectVobSub;
        if (vobSub != null)
        {
          string defaultLanguage;
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
          {
            string strTmp = "";
            string strFont = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
            int iFontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);
            bool bBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
            defaultLanguage = xmlreader.GetValueAsString("subtitles", "language", "English");

            strTmp = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
            long iColor = Convert.ToInt64(strTmp, 16);
            int iShadow = xmlreader.GetValueAsInt("subtitles", "shadow", 5);

            LOGFONT logFont = new LOGFONT();
            int txtcolor;
            bool fShadow, fOutLine, fAdvancedRenderer = false;
            int size = Marshal.SizeOf(typeof(LOGFONT));
            vobSub.get_TextSettings(logFont, size, out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);

            FontStyle fontStyle = FontStyle.Regular;
            if (bBold) fontStyle = FontStyle.Bold;
            System.Drawing.Font Subfont = new System.Drawing.Font(strFont, iFontSize, fontStyle, System.Drawing.GraphicsUnit.Point, 1);
            Subfont.ToLogFont(logFont);
            int R = (int)((iColor >> 16) & 0xff);
            int G = (int)((iColor >> 8) & 0xff);
            int B = (int)((iColor) & 0xff);
            txtcolor = (B << 16) + (G << 8) + R;
            if (iShadow > 0) fShadow = true;
            int res = vobSub.put_TextSettings(logFont, size, txtcolor, fShadow, fOutLine, fAdvancedRenderer);
          }

          for (int i = 0; i < SubtitleStreams; ++i)
          {
            string language = SubtitleLanguage(i);
            if (String.Compare(language, defaultLanguage, true) == 0)
            {
              CurrentSubtitleStream = i;
              break;
            }
          }
        }
        if (filter != null)
          Marshal.ReleaseComObject(filter); filter = null;


        if (!Vmr9.IsVMR9Connected)
        {
          //VMR9 is not supported, switch to overlay
          mediaCtrl = null;
          Cleanup();
          return base.GetInterfaces();
        }
        Vmr9.SetDeinterlaceMode();

        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("VideoPlayer9:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    protected override void OnProcess()
    {
      if (Vmr9 != null)
      {
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    void Cleanup()
    {
      if (graphBuilder == null) return;
      int hr;
      Log.Info("VideoPlayer9:cleanup DShow graph");
      try
      {
        videoWin = graphBuilder as IVideoWindow;
        if (videoWin != null)
          videoWin.put_Visible(OABool.False);
        if (Vmr9 != null)
        {
          Vmr9.Enable(false);
        }
        if (mediaCtrl != null)
        {

          int counter = 0;
          while (GUIGraphicsContext.InVmr9Render)
          {
            counter++;
            System.Threading.Thread.Sleep(1);
            if (counter > 200) break;
          }
          hr = mediaCtrl.Stop();
          FilterState state;
          hr = mediaCtrl.GetState(10, out state);
          Log.Info("state:{0} {1:X}", state.ToString(), hr);
          mediaCtrl = null;
        }
        mediaEvt = null;


        if (Vmr9 != null)
        {
          Vmr9.Dispose();
          Vmr9 = null;
        }

        mediaSeek = null;
        mediaPos = null;
        basicAudio = null;
        basicVideo = null;
        videoWin = null;

        if (videoCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(videoCodecFilter)>0); 
          videoCodecFilter = null;
        }
        if (audioCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(audioCodecFilter)>0); 
          audioCodecFilter = null;
        }
        if (audioRendererFilter != null)
        {
          while (Marshal.ReleaseComObject(audioRendererFilter)>0); 
          audioRendererFilter = null;
        }
        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
            if (customFilters[i] != null)
            {
                while (( hr =Marshal.ReleaseComObject(customFilters[i])) > 0);
            }
            customFilters[i] = null;
        }

        if (vobSub != null)
        {
          while ((hr = Marshal.ReleaseComObject(vobSub)) > 0) ;
          vobSub = null;
        }
        //	DsUtils.RemoveFilters(graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;

        if (graphBuilder != null)
        {
          while ((hr = Marshal.ReleaseComObject(graphBuilder)) > 0) ;
          graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;
        GC.Collect();
      }
      catch (Exception ex)
      {
        Log.Error("VideoPlayerVMR9: Exception while cleanuping DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        Log.Info("VideoPlayerVMR9: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }

    }
  }
}
