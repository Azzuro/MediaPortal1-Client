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
using System.IO;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using Win32.Utils.Cd;

namespace MediaPortal.Ripper
{
  /// <summary>
  /// AutoPlay functionality.
  /// </summary>
  public class AutoPlay
  {
    #region base variables

    static DeviceVolumeMonitor _deviceMonitor;
    static string m_dvd = "No";
    static string m_audiocd = "No";

    enum MediaType
    {
      UNKNOWN = 0,
      DVD = 1,
      AUDIO_CD = 2,
      PHOTOS = 3,
      VIDEOS = 4,
      AUDIO = 5
    }

    #endregion

    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private AutoPlay()
    {
    }

    /// <summary>
    /// Static constructor of the autoplay class.
    /// </summary>
    static AutoPlay()
    {
      m_dvd = "No";
      m_audiocd = "No";

    }

    ~AutoPlay()
    {
      _deviceMonitor.Dispose();
      _deviceMonitor = null;
    }

    /// <summary>
    /// Starts listening for events on the optical drives.
    /// </summary>
    public static void StartListening()
    {
      LoadSettings();
      StartListeningForEvents();
    }

    /// <summary>
    /// Stops listening for events on the optical drives and cleans up.
    /// </summary>
    public static void StopListening()
    {
      StopListeningForEvents();

    }
    #region initialization + serialization


    private static void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        m_dvd = xmlreader.GetValueAsString("dvdplayer", "autoplay", "Ask");
        m_audiocd = xmlreader.GetValueAsString("audioplayer", "autoplay", "No");
      }
    }

    private static void StartListeningForEvents()
    {
      _deviceMonitor = new DeviceVolumeMonitor(GUIGraphicsContext.form.Handle);
      _deviceMonitor.OnVolumeInserted += new DeviceVolumeAction(VolumeInserted);
      _deviceMonitor.OnVolumeRemoved += new DeviceVolumeAction(VolumeRemoved);
      _deviceMonitor.AsynchronousEvents = true;
      _deviceMonitor.Enabled = true;
    }

    #endregion

    #region cleanup

    private static void StopListeningForEvents()
    {
      if (_deviceMonitor != null)
        _deviceMonitor.Dispose();
      _deviceMonitor = null;
    }

    #endregion

    #region capture events

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is removed from a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDRemoved(string DriveLetter)
    {
      Log.Write("media removed from drive {0}", DriveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
                                      (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
                                      GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = String.Format("{0}:", DriveLetter);
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);

      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
        (int)GUIWindow.Window.WINDOW_VIDEOS,
        GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = DriveLetter;
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);


    }

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is inserted into a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDInserted(string DriveLetter)
    {
      Log.Write("media inserted in drive {0}", DriveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_INSERTED,
                                            (int)0,
                                            GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = DriveLetter;
      GUIWindowManager.SendThreadMessage(msg);

    }

    /// <summary>
    /// The event that gets triggered whenever a new volume is removed.
    /// </summary>	
    private static void VolumeRemoved(int bitMask)
    {
      string driveLetter = _deviceMonitor.MaskToLogicalPaths(bitMask);

      Log.Write("volume removed drive {0}", driveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED,
                                (int)0,
                                GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = driveLetter;
      GUIWindowManager.SendThreadMessage(msg);
      CDRemoved(driveLetter);
    }

    /// <summary>
    /// The event that gets triggered whenever a new volume is inserted.
    /// </summary>	
    private static void VolumeInserted(int bitMask)
    {
      string driveLetter = _deviceMonitor.MaskToLogicalPaths(bitMask);
      Log.Write("volume inserted drive {0}", driveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED,
        (int)0,
        GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = driveLetter;
      GUIWindowManager.SendThreadMessage(msg);
      CDInserted(driveLetter);
    }

    static bool ShouldWeAutoPlay(MediaType iMedia)
    {
      Log.Write("Check if we want to autoplay a {0}", iMedia);
      if (GUIWindowManager.IsRouted) return false;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, null);
      msg.Param1 = 713;
      switch (iMedia)
      {
        case MediaType.PHOTOS: // Photo
          msg.Param2 = 530;
          break;
        case MediaType.VIDEOS: // Movie
          msg.Param2 = 531;
          break;
        case MediaType.AUDIO: // Audio
          msg.Param2 = 532;
          break;
        case MediaType.AUDIO_CD: // Audio cd
          msg.Param2 = 532;
          break;
        default:
          msg.Param2 = 714;
          break;
      }
      msg.Param3 = 0;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1 != 0)
      {
        //stop tv...
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);

        //stop radio...
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
        return true;
      }
      return false;
    }

    public static void ExamineCD(string strDrive)
    {
      if (strDrive == null) return;
      if (strDrive.Length == 0) return;

      StopListening();

      GUIMessage msg;
      switch (DetectMediaType(strDrive))
      {
        //case MediaType.DVD:
        //  Log.Write("ExamineCD: DVD inserted into drive {0}", strDrive);

        //  bool PlayDVD = false;
        //  if (m_dvd == "Yes")
        //  {
        //    // Automaticaly play the CD
        //    PlayDVD = true;
        //    Log.Write("DVD Autoplay = auto");
        //  }
        //  else if (m_dvd == "Ask")
        //  {
        //    if (ShouldWeAutoPlay(MediaType.DVD))
        //    {
        //      PlayDVD = true;
        //      Log.Write("DVD Autoplay = ask,answer = yes");
        //    }
        //    else
        //    {
        //      Log.Write("DVD Autoplay = ask, answer = no");
        //    }
        //  }
        //  if (PlayDVD)
        //  {
        //    // dont interrupt if we're already playing
        //    if (g_Player.Playing && g_Player.IsDVD) return;

        //    // We are going to autoplay if..
        //    //Log.Write ("We seem to want to play the DVD");
        //    Log.Write("Autoplay:start DVD in {0}", strDrive);
        //    g_Player.PlayDVD(strDrive + @"\VIDEO_TS\VIDEO_TS.IFO");
        //  }
        //  break;

        case MediaType.AUDIO_CD:
          Log.Write("ExamineCD: Audio CD inserted into drive {0}", strDrive);
          //m_audiocd tells us if we want to autoplay or not.
          Log.Write("CD Autoplay = {0}", m_audiocd);
          bool PlayAudioCd = false;
          if (m_audiocd == "Yes")
          {
            // Automaticaly play the CD
            PlayAudioCd = true;
            Log.Write("CD Autoplay = auto");
          }
          else if ((m_audiocd == "Ask") && (ShouldWeAutoPlay(MediaType.AUDIO_CD)))
          {
            PlayAudioCd = true;
          }
          if (PlayAudioCd)
          {
            // The user wants to play the Audio CD
            if (g_Player.Playing) g_Player.Stop();

            // Send a message with the drive to the message handler. 
            // The message handler will play the CD
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD,
              (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
              GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            msg.Label2 = m_audiocd;
            Log.Write("Autoplay = {0}", m_audiocd);
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.PHOTOS:
          if (ShouldWeAutoPlay(MediaType.PHOTOS))
          {
            Log.Write("CD/DVD with photo's inserted into drive {0}", strDrive);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME,
              (int)GUIWindow.Window.WINDOW_PICTURES,
              GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.VIDEOS:
          Log.Write("CD/DVD with videos inserted into drive {0}", strDrive);
          break;

        case MediaType.AUDIO:
          Log.Write("CD/DVD with audio inserted into drive {0}", strDrive);
          break;

        default:
          Log.Write("ExamineCD: Unknown media type inserted into drive {0}", strDrive);
          break;
      }
      StartListening();
    }

    public static void ExamineVolume(string strDrive)
    {
      if (strDrive == null) return;
      if (strDrive.Length == 0) return;
      GUIMessage msg;
      switch (DetectMediaType(strDrive))
      {
        case MediaType.DVD:
          Log.Write("DVD volume inserted {0}", strDrive);
          if (m_dvd == "Yes")
          {
            // dont interrupt if we're already playing
            if (g_Player.Playing && g_Player.IsDVD) return;
            Log.Write("Autoplay: Yes, start DVD in {0}", strDrive);
            g_Player.PlayDVD(strDrive + @"\VIDEO_TS\VIDEO_TS.IFO");
          }
          if (m_dvd == "Ask")
          {
            if (ShouldWeAutoPlay(MediaType.DVD))
            {
              Log.Write("Autoplay: Answered yes, start DVD in {0}", strDrive);
              g_Player.PlayDVD(strDrive + @"\VIDEO_TS\VIDEO_TS.IFO");
            }
            else
            {
              Log.Write("Autoplay: Answered no, do not start DVD in {0}", strDrive);
            }
          }
          break;

        case MediaType.PHOTOS:
          Log.Write("Photo volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.PHOTOS))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
              (int)GUIWindow.Window.WINDOW_PICTURES,
              GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.VIDEOS:
          Log.Write("Video volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.VIDEOS))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEOS);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
              (int)GUIWindow.Window.WINDOW_VIDEOS,
              GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.AUDIO:
          Log.Write("Audio volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.AUDIO))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_FILES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
              (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
              GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        default:
          Log.Write("ExamineVolume: Unknown media type inserted into drive {0}", strDrive);
          break;
      }
    }

    private static bool isARedBookCD(string driveLetter)
    {
      try
      {
        if (driveLetter.Length < 1) return false;
        int cddaTracks = 0;
        CDDrive m_Drive = new CDDrive();

        if (m_Drive.IsOpened)
          m_Drive.Close();
        if (m_Drive.Open(driveLetter[0]) && m_Drive.IsCDReady() && m_Drive.Refresh())
        {
          cddaTracks = m_Drive.GetNumAudioTracks();
          m_Drive.Close();
        }
        if (cddaTracks > 0)
          return true;
        else
          return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static void GetAllFiles(string strFolder, ref ArrayList allfiles)
    {
      if (strFolder == null) return;
      if (strFolder.Length == 0) return;
      if (allfiles == null) return;
      try
      {
        string[] files = System.IO.Directory.GetFiles(strFolder);
        if (files != null && files.Length > 0)
        {
          for (int i = 0; i < files.Length; ++i) allfiles.Add(files[i]);
        }
        string[] folders = System.IO.Directory.GetDirectories(strFolder);
        if (folders != null && folders.Length > 0)
        {
          for (int i = 0; i < folders.Length; ++i) GetAllFiles(folders[i], ref allfiles);
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Detects the media type of the CD/DVD inserted into a drive.
    /// </summary>
    /// <param name="driveLetter">The drive that contains the data.</param>
    /// <returns>The media type of the drive.</returns>
    private static MediaType DetectMediaType(string strDrive)
    {
      if (strDrive == null)
        return MediaType.UNKNOWN;
      if (strDrive == String.Empty)
        return MediaType.UNKNOWN;
      try
      {
        if (Directory.Exists(strDrive + "\\VIDEO_TS"))
          return MediaType.DVD;

        // following has been replaced by isARedBookCD:
        //string[] files = Directory.GetFiles(strDrive + "\\", "*.cda");
        //if (files != null && files.Length != 0)
        //{
        //  return MediaType.AUDIO_CD;
        //}

        if (isARedBookCD(strDrive))
          return MediaType.AUDIO_CD;

        ArrayList allfiles = new ArrayList();
        GetAllFiles(strDrive + "\\", ref allfiles);
        foreach (string FileName in allfiles)
        {
          string ext = System.IO.Path.GetExtension(FileName).ToLower();
          if (Utils.IsVideo(FileName)) return MediaType.VIDEOS;
        }

        foreach (string FileName in allfiles)
        {
          string ext = System.IO.Path.GetExtension(FileName).ToLower();
          if (Utils.IsAudio(FileName)) return MediaType.AUDIO;
        }

        foreach (string FileName in allfiles)
        {
          if (Utils.IsPicture(FileName)) return MediaType.PHOTOS;
        }
      }
      catch (Exception)
      {
      }
      return MediaType.UNKNOWN;
    }
    #endregion
  }
}
