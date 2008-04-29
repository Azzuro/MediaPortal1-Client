#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsTv.
  /// </summary>
  public class GUISettingsTv : GUIWindow
  {
    [SkinControlAttribute(24)]
    protected GUIButtonControl btnVideoCodec = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnAudioCodec = null;
    [SkinControlAttribute(27)]
    protected GUIButtonControl btnDeinterlace = null;
    [SkinControlAttribute(28)]
    protected GUIButtonControl btnAspectRatio = null;
    [SkinControlAttribute(29)]
    protected GUIButtonControl btnTimeshiftBuffer = null;
    [SkinControlAttribute(30)]
    protected GUIButtonControl btnAutoTurnOnTv = null;
    [SkinControlAttribute(26)]
    protected GUIButtonControl btnAutoTurnOnTS = null;
    [SkinControlAttribute(31)]
    protected GUIButtonControl btnRecordingOptions = null;
    [SkinControlAttribute(33)]
    protected GUIButtonControl btnAudioRenderer = null;
    [SkinControlAttribute(34)]
    protected GUIButtonControl btnEpg = null;
    [SkinControlAttribute(35)]
    protected GUIButtonControl btnH264VideoCodec = null;
    [SkinControlAttribute(36)]
    protected GUIButtonControl btnAACAudioCodec = null;
    [SkinControlAttribute(37)]
    protected GUIButtonControl btnXMLEpg = null;
    
    public GUISettingsTv()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_TV;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tv.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        btnTimeshiftBuffer.Visible = false;
        btnAutoTurnOnTS.Visible = false;
        btnRecordingOptions.Visible = false;
        btnEpg.Visible = false;
        btnXMLEpg.Visible = false;
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnAudioRenderer)
        OnAudioRenderer();
      if (control == btnVideoCodec)
        OnVideoCodec();
      if (control == btnAudioCodec)
        OnAudioCodec();
      if (control == btnAspectRatio)
        OnAspectRatio();
      if (control == btnTimeshiftBuffer)
        OnTimeshiftBuffer();
      if (control == btnDeinterlace)
        OnDeinterlace();
      if (control == btnAutoTurnOnTv)
        OnAutoTurnOnTv();
      if (control == btnAutoTurnOnTS)
        OnAutoTurnOnTS();
      if (control == btnH264VideoCodec)
        OnH264VideoCodec();
      if (control == btnAACAudioCodec)
        OnAACAudioCodec();
      base.OnClicked(controlId, control, actionType);
    }

    void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
      }
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      //Remove Muxer's from the list to avoid confusion.
      while (availableVideoFilters.Contains("CyberLink MPEG Muxer")) availableVideoFilters.Remove("CyberLink MPEG Muxer");
      while (availableVideoFilters.Contains("Ulead MPEG Muxer")) availableVideoFilters.Remove("Ulead MPEG Muxer");
      while (availableVideoFilters.Contains("PDR MPEG Muxer")) availableVideoFilters.Remove("PDR MPEG Muxer");
      while (availableVideoFilters.Contains("Nero Mpeg2 Encoder")) availableVideoFilters.Remove("Nero Mpeg2 Encoder");
      availableVideoFilters.Sort();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableVideoFilters)
        {
          dlg.Add(codec);//delete
          if (codec == strVideoCodec)
            selected = count;
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
        return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytv", "videocodec", (string)availableVideoFilters[dlg.SelectedLabel]);
      }
    }

    void OnH264VideoCodec()
    {
      string strH264VideoCodec = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
      }
      ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableH264VideoFilters)
        {
          dlg.Add(codec);//delete
          if (codec == strH264VideoCodec)
            selected = count;
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
        return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytv", "h264videocodec", (string)availableH264VideoFilters[dlg.SelectedLabel]);
      }
    }

    void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
      }
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      //Remove Muxer's from the list to avoid confusion.
      while (availableAudioFilters.Contains("CyberLink MPEG Muxer")) availableAudioFilters.Remove("CyberLink MPEG Muxer");
      while (availableAudioFilters.Contains("Ulead MPEG Muxer")) availableAudioFilters.Remove("Ulead MPEG Muxer");
      while (availableAudioFilters.Contains("PDR MPEG Muxer")) availableAudioFilters.Remove("PDR MPEG Muxer");
      while (availableAudioFilters.Contains("Nero Mpeg2 Encoder")) availableAudioFilters.Remove("Nero Mpeg2 Encoder");
      availableAudioFilters.Sort();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAudioFilters)
        {
          dlg.Add(codec);//delete
          if (codec == strAudioCodec)
            selected = count;
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
        return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytv", "audiocodec", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    void OnAACAudioCodec()
    {
      string strAACAudioCodec = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAACAudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
      }
      ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAACAudioFilters)
        {
          dlg.Add(codec);//delete
          if (codec == strAACAudioCodec)
            selected = count;
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
        return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytv", "aacaudiocodec", (string)availableAACAudioFilters[dlg.SelectedLabel]);
      }
    }

    void OnAspectRatio()
    {
      MediaPortal.GUI.Library.Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string aspectRatioText = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        aspectRatio = Utils.GetAspectRatio(aspectRatioText);
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      dlg.AddLocalizedString(942); // Stretch
      dlg.AddLocalizedString(943); // Normal
      dlg.AddLocalizedString(944); // Original
      dlg.AddLocalizedString(945); // Letterbox
      dlg.AddLocalizedString(946); // Pan and scan
      dlg.AddLocalizedString(947); // Zoom
      dlg.AddLocalizedString(1190); // Zoom 14:9

      // set the focus to currently used mode
      dlg.SelectedLabel = (int)aspectRatio;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

      aspectRatio = Utils.GetAspectRatioByLangID(dlg.SelectedId);

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string aspectRatioText = Utils.GetAspectRatio(aspectRatio);
        xmlwriter.SetValue("mytv", "defaultar", aspectRatioText);
      }
    }

    void OnTimeshiftBuffer()
    {
      int buflen = 30;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        buflen = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        for (int i = 30; i <= 180; i += 30)
        {
          dlg.Add(String.Format("{0} min", i.ToString()));
          if (i == buflen)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
          return;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          buflen = (dlg.SelectedLabel * 30) + 30;
          xmlwriter.SetValue("capture", "timeshiftbuffer", buflen.ToString());
        }
      }
    }

    void OnDeinterlace()
    {
      string[] deinterlaceModes = { "None", "Bob", "Weave", "Best" };
      int deInterlaceMode = 1;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        deInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        for (int index = 0; index < deinterlaceModes.Length; index++)
        {
          dlg.Add(deinterlaceModes[index]);
        }
        dlg.SelectedLabel = deInterlaceMode;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
          return;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("mytv", "deinterlace", dlg.SelectedLabel);
        }
      }
    }

    void OnAutoTurnOnTv()
    {
      bool autoTurnOn = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        autoTurnOn = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        dlg.Add(GUILocalizeStrings.Get(775));       //Start TV in MyTV sections automatically
        dlg.Add(GUILocalizeStrings.Get(776));       //Do not start / switch to TV automatically
        dlg.SelectedLabel = autoTurnOn ? 0 : 1;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
          return;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValueAsBool("mytv", "autoturnontv", (dlg.SelectedLabel == 0));
        }
      }
    }

    void OnAutoTurnOnTS()
    {
      bool autoTurnOnTS = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        autoTurnOnTS = xmlreader.GetValueAsBool("mytv", "autoturnontimeshifting", true);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        dlg.Add(GUILocalizeStrings.Get(778));       //Start with timeshift automatically enabled
        dlg.Add(GUILocalizeStrings.Get(779));       //Timeshift must be enabled manually
        dlg.SelectedLabel = autoTurnOnTS ? 0 : 1;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
          return;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (dlg.SelectedLabel == 0)
            autoTurnOnTS = true;
          else
            autoTurnOnTS = false;
          xmlwriter.SetValueAsBool("mytv", "autoturnontimeshifting", autoTurnOnTS);
          //if ( autoTurnOnTS ) // as long as timeshift on causes this behaviour - needs to be fixed
          //  xmlwriter.SetValueAsBool("mytv", "autoturnontv", false);
        }
      }
    }

    void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
      }
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496));//Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAudioFilters)
        {
          dlg.Add(codec);//delete
          if (codec == strAudioRenderer)
            selected = count;
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
        return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytv", "audiorenderer", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }
  }
}