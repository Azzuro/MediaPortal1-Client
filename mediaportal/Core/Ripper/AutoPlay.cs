using System;
using System.IO;  
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;

namespace MediaPortal.Ripper
{
	/// <summary>
	/// AutoPlay functionality.
	/// </summary>
	public class AutoPlay
	{
    #region base variables
    
    static ArrayList m_vecList   = null;
    static Ripper.CDDrive [] m_drives=null;
    static bool m_dvd=false;
    static bool m_audiocd=false;

    enum MediaType
    {
      UNKNOWN      = 0,
      DVD          = 1,
      AUDIO_CD     = 2,
      PHOTOS       = 3,
      VIDEOS       = 4,
      AUDIO        = 5
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
			m_vecList   = new ArrayList();
			m_dvd=false;
			m_audiocd=false;
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
      CleanupDriveList();
    }
    #region initialization + serialization
   
    static void AddDrive (string Drive)
    {
			if (Drive==null) return;
			if (Drive.Length<2) return;
      string DriveLetter=Drive.Substring(0,2).ToLower();
      foreach (string share in m_vecList)
      {
        string DriveLetterTmp = share.Substring(0,2).ToLower();
        if (DriveLetterTmp.Equals(DriveLetter)) return;
      }
      m_vecList.Add(Drive);
    }

    private static void LoadSettings()
    {

      m_vecList=new ArrayList();
      try
      {
        using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          m_dvd=xmlreader.GetValueAsBool("dvdplayer","autoplay",true);
          m_audiocd=xmlreader.GetValueAsBool("audioplayer","autoplay",true);
          if (m_dvd==false && m_audiocd==false) return;

          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("music", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("music", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              AddDrive(sharepath);
            }
          }
          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("movies", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("movies", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              AddDrive(sharepath);
            }
          }
          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("pictures", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("pictures", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              AddDrive(sharepath);
            }
          }
          // read autoplay information
        }
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.LoadSettings() {0} {1} {2}",  
          ex.Message,ex.Source,ex.StackTrace);
      }
    }

    private static void StartListeningForEvents()
    {
      int nrOfDrives=m_vecList.Count;
      if (nrOfDrives<=0) return;
      try
      {
        m_drives=new Ripper.CDDrive[nrOfDrives];
        for (int i=0;i<nrOfDrives;i++)
        {
          m_drives[i]=new CDDrive();
          m_drives[i].Open(((string)m_vecList[i])[0]);
          m_drives[i].CDInserted+=new CDDrive.CDNotificationHandler(CDInserted);
          m_drives[i].CDRemoved+=new CDDrive.CDNotificationHandler(CDRemoved);
        }
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.StartListeningForEvents() {0} {1} {2}",  
                    ex.Message,ex.Source,ex.StackTrace);
      }
    }
        
    #endregion

    #region cleanup

    private static void StopListeningForEvents()
    {
      if (m_drives==null) return;
      try
      {
        for (int i=0;i<m_drives.Length;i++)
        {
          m_drives[i].Close();
          m_drives[i].Dispose();
        }
                
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.StopListeningForEvents() {0} {1} {2}",  
        ex.Message,ex.Source,ex.StackTrace);
      }
    }

    private static void CleanupDriveList()
    {
      m_drives=null;
      m_vecList=null;
    }
    #endregion

    #region capture events

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is removed from a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDRemoved(char DriveLetter)
    {
      Log.Write("media removed from drive {0}",DriveLetter);  
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
                                      (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
                                      GUIWindowManager.ActiveWindow,0,0,0,0);
      msg.Label=String.Format("{0}:", DriveLetter);
      msg.SendToTargetWindow=true;
      GUIWindowManager.SendThreadMessage(msg);

      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
        (int)GUIWindow.Window.WINDOW_VIDEOS,
        GUIWindowManager.ActiveWindow,0,0,0,0);
      msg.Label=String.Format("{0}:", DriveLetter);
      msg.SendToTargetWindow=true;
      GUIWindowManager.SendThreadMessage(msg);


    }

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is inserted into a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDInserted(char DriveLetter)
    {
      Log.Write("media inserted in drive {0}",DriveLetter);  
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_INSERTED,
                                            (int)0,
                                            GUIWindowManager.ActiveWindow,0,0,0,0);
      msg.Label=String.Format("{0}:", DriveLetter);
      GUIWindowManager.SendThreadMessage(msg);

    }

    static bool ShouldWeAutoPlay()
    {
      if (GUIWindowManager.IsRouted) return false;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,null);
      msg.Param1=713;
      msg.Param2=714;
      msg.Param3=0;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1!=0) return true;
      return false;
    }
    
    public static void ExamineCD(string strDrive)
    {
			if (strDrive==null) return;
			if (strDrive.Length==0) return;
      StopListening();
      GUIMessage msg;
      switch(DetectMediaType(strDrive))       
      {         
        case MediaType.DVD:
          Log.Write("DVD inserted into drive {0}",strDrive);
          if (m_dvd)
          {
            // dont interrupt if we're already playing
            if (g_Player.Playing && g_Player.IsDVD) return;
            if (ShouldWeAutoPlay()) 
            {
              g_Player.PlayDVD(strDrive+@"\VIDEO_TS\VIDEO_TS.IFO");
            }
          }
          break;                  
        case MediaType.AUDIO_CD:
          Log.Write("Audio CD inserted into drive {0}",strDrive);
          if (m_audiocd)
          {
            if (ShouldWeAutoPlay()) 
            {
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD,
                (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
                GUIWindowManager.ActiveWindow,0,0,0,0);
              msg.Label=strDrive;
              msg.SendToTargetWindow=true;
              GUIWindowManager.SendThreadMessage(msg);
            }
          }
          break;                 

        case MediaType.PHOTOS:
          if (ShouldWeAutoPlay()) 
          {
            Log.Write("CD/DVD with photo's inserted into drive {0}",strDrive);
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_PICTURES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_INSERTED,
              (int)GUIWindow.Window.WINDOW_PICTURES,
              GUIWindowManager.ActiveWindow,0,0,0,0);
            msg.Label=strDrive;
            msg.SendToTargetWindow=true;
            GUIWindowManager.SendThreadMessage(msg);
          }
        break;

        case MediaType.VIDEOS:
          Log.Write("CD/DVD with videos inserted into drive {0}",strDrive);
        break;

        case MediaType.AUDIO:
          Log.Write("CD/DVD with audio inserted into drive {0}",strDrive);
        break;

        default:
          Log.Write("Unknown media type inserted into drive {0}",strDrive);  
        break;      
      }
      StartListening();
    }
    
    private static void GetAllFiles(string strFolder, ref ArrayList allfiles)
    {
			if (strFolder==null) return;
			if (strFolder.Length==0) return;
			if (allfiles==null) return;
			try
			{
				string [] files=System.IO.Directory.GetFiles(strFolder);
				if (files != null && files.Length>0)
				{
					for (int i=0; i < files.Length; ++i) allfiles.Add( files[i] );
				}
				string [] folders = System.IO.Directory.GetDirectories(strFolder);
				if (folders != null && folders.Length>0)
				{
					for (int i=0; i < folders.Length; ++i) GetAllFiles(folders[i], ref allfiles );
				}
			}
			catch(Exception)
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
			if (strDrive==null) return MediaType.UNKNOWN;
			if (strDrive==String.Empty) return MediaType.UNKNOWN;
      try
			{
				if (Directory.Exists(strDrive+"\\VIDEO_TS"))
				{
					return MediaType.DVD;
				}

        string[]  files=Directory.GetFiles(strDrive+"\\","*.cda");
        if (files!=null && files.Length!=0)
        {
          return MediaType.AUDIO_CD;
        }

        ArrayList allfiles=new ArrayList();
        GetAllFiles(strDrive+"\\",ref allfiles);
        foreach (string FileName in allfiles)
        {
          string ext=System.IO.Path.GetExtension(FileName).ToLower();
          if (Utils.IsVideo(FileName)) return MediaType.VIDEOS;
        }

        foreach (string FileName in allfiles)
        {
          string ext=System.IO.Path.GetExtension(FileName).ToLower();
          if (Utils.IsAudio(FileName)) return MediaType.AUDIO;
        }

        foreach (string FileName in allfiles)
        {
          if (Utils.IsPicture(FileName)) return MediaType.PHOTOS;
        }
      }
      catch(Exception)
      {
      }


      return MediaType.UNKNOWN;
    }
    #endregion
	}
}
