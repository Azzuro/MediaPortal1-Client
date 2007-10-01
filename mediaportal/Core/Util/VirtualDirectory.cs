#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Player;
using MediaPortal.Configuration;
using System.Management;
using System.IO;
using EnterpriseDT.Net.Ftp;
using Core.Util;


namespace MediaPortal.Util
{
  /// <summary>
  /// This class is used to present a virtual directory to MediaPortal
  /// A virtual directory holds one or more shares (see share.cs)
  /// 
  /// </summary>

  public class VirtualDirectory
  {
    const int Removable = 2;
    const int LocalDisk = 3;
    const int Network = 4;
    const int CD = 5;
    const int MaximumShares = 20;

    List<Share> m_shares = new List<Share>();
    List<string> m_extensions = null;
    string m_strPreviousDir = String.Empty;
    string currentShare = String.Empty;
    string previousShare = String.Empty;
    string m_strLocalFolder = String.Empty;
    Share defaultshare = null;
    bool showFilesWithoutExtension = false;
    /// <summary>
    /// constructor
    /// </summary>
    public VirtualDirectory()
    {
    }

    public void LoadSettings(string section)
    {
      Clear();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strDefault = xmlreader.GetValueAsString(section, "default", String.Empty);
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString(section, strShareName, String.Empty);
          share.Path = xmlreader.GetValueAsString(section, strSharePath, String.Empty);
          string pinCode = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString(section, strPincode, string.Empty));
          if (pinCode != string.Empty)
            share.Pincode = Convert.ToInt32(pinCode);
          else
            share.Pincode = -1;

          share.IsFtpShare = xmlreader.GetValueAsBool(section, shareType, false);
          share.FtpServer = xmlreader.GetValueAsString(section, shareServer, String.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString(section, shareLogin, String.Empty);
          share.FtpPassword = xmlreader.GetValueAsString(section, sharePwd, String.Empty);
          share.FtpPort = xmlreader.GetValueAsInt(section, sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString(section, remoteFolder, "/");
          share.DefaultView = (Share.Views)xmlreader.GetValueAsInt(section, shareViewPath, (int)Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (defaultshare == null)
              {
                defaultshare = share;
              }
            }
            Add(share);
          }
          else break;
        }
      }
    }

    public Share DefaultShare
    {
      get { return defaultshare; }
    }

    public string CurrentShare
    {
      get { return currentShare; }
      set
      {
        previousShare = currentShare;
        currentShare = value;
        //Log.Debug("VirtualDirectory: Setting current share: {0} - Previous: {1}", currentShare, previousShare);
      }
    }
    public string PreviousShare
    {
      get { return previousShare; }
    }

    public bool ShowFilesWithoutExtension
    {
      get { return showFilesWithoutExtension; }
      set { showFilesWithoutExtension = value; }
    }

    public void AddDrives()
    {
    }

    public void Reset()
    {
      currentShare = String.Empty;
      previousShare = String.Empty;
      m_strPreviousDir = String.Empty;
    }

    public bool RequestPin(string folder)
    {
      int iPincodeCorrect;
      if (IsProtectedShare(folder, out iPincodeCorrect))
      {
        bool retry = true;
        {
          while (retry)
          {
            //no, then ask user to enter the pincode
            GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
            GUIWindowManager.SendMessage(msgGetPassword);
            int iPincode = -1;
            try
            {
              iPincode = Int32.Parse(msgGetPassword.Label);
            }
            catch (Exception)
            {
            }
            if (iPincode != iPincodeCorrect)
            {
              GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgWrongPassword);

              if (!(bool)msgWrongPassword.Object)
              {
                return false;
              }
            }
            else
              retry = false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Method to set a list of all file extensions
    /// The virtual directory will only show folders and files which match one of those file extensions
    /// </summary>
    /// <param name="extensions">string arraylist of file extensions</param>
    public void SetExtensions(ArrayList extensions)
    {
      if (extensions == null) return;
      if (m_extensions == null)
        m_extensions = new List<string>();
      else
        m_extensions.Clear();

      foreach (string ext in extensions)
      {
        m_extensions.Add(ext);
      }
      for (int i = 0; i < m_extensions.Count; ++i)
      {
        m_extensions[i] = ((string)m_extensions[i]).ToLower();
      }
    }
    /// <summary>
    /// Method to add a new file extension to the file extensions list
    /// </summary>
    /// <param name="extension">string containg the new extension in the format .mp3</param>
    public void AddExtension(string extension)
    {
      if (m_extensions == null)
        m_extensions = new List<string>();
      m_extensions.Add(extension.ToLower());
    }

    /// <summary>
    /// Add a new share to this virtual directory
    /// </summary>
    /// <param name="share">new Share</param>
    public void Add(Share share)
    {
      if (share == null) return;
      m_shares.Add(share);
    }

    /// <summary>
    /// This method removes all shares from the virtual directory
    /// </summary>
    public void Clear()
    {
      m_shares.Clear();
    }

    /// <summary>
    /// This method will return the root folder
    /// which contains a list of all shares
    /// </summary>
    /// <returns>
    /// ArrayList containing a GUIListItem for each share
    /// </returns>
    public ArrayList GetRoot()
    {
      previousShare = String.Empty;

      ArrayList items = new ArrayList();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
        if (Utils.IsRemovable(item.Path) && Directory.Exists(item.Path))
        {
          string driveName = Utils.GetDriveName(item.Path);
          if (driveName == "") driveName = GUILocalizeStrings.Get(1061);
          item.Label = String.Format("({0}) {1}", item.Path, driveName);
        }
        if (Utils.IsDVD(item.Path))
        {
          item.DVDLabel = Utils.GetDriveName(item.Path);
          item.DVDLabel = item.DVDLabel.Replace('_', ' ');
        }

        if (item.DVDLabel != "")
        {
          item.Label = String.Format("({0}) {1}", item.Path, item.DVDLabel);
        }
        else
          item.Label = share.Name;
        item.IsFolder = true;

        if (share.IsFtpShare)
        {
          //item.Path = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
          //    share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
          item.Path = GetShareRemoteURL(share);
          item.IsRemote = true;
        }
        Utils.SetDefaultIcons(item);
        if (share.Pincode < 0)
        {
          string coverArt = Utils.GetCoverArtName(item.Path, "folder");
          string largeCoverArt = Utils.GetLargeCoverArtName(item.Path, "folder");
          if (System.IO.File.Exists(coverArt))
          {
            item.IconImage = coverArt;
          }
          if (System.IO.File.Exists(largeCoverArt))
          {
            item.IconImageBig = largeCoverArt;
          }
        }
        items.Add(item);
      }

      // add removable drives with media
      string[] drives = Environment.GetLogicalDrives();
      foreach (string drive in drives)
      {
        if (drive[0] > 'B' && Util.Utils.getDriveType(drive) == Removable)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);
          string driveLetter = drive.Substring(0, 1).ToUpper() + ":";
          if (driveName == "") driveName = GUILocalizeStrings.Get(1061);

          //
          // Check if the share already exists
          //
          foreach (Share share in m_shares)
          {
            if (share.Path == driveLetter)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            GUIListItem item = new GUIListItem();
            item.Path = driveLetter;
            item.Label = String.Format("({0}) {1}", item.Path, driveName);
            item.IsFolder = true;

            Utils.SetDefaultIcons(item);

            // dont add removable shares without media
            // virtual cd/dvd drive (daemontools) without mounted image
            if (!Directory.Exists(item.Path))
              break;

            items.Add(item);
          }
        }
      }

      return items;
    }

    /// <summary>
    /// Method which checks if the given folder is the root share folder or not
    /// </summary>
    /// <param name="strDir">folder name</param>
    /// <returns>
    /// true: this is the root folder. There's no parent folder
    /// false: this is not the root folder, there is a parent folder
    /// </returns>
    public bool IsRootShare(string strDir)
    {
      Share share = GetShare(strDir);
      if (share == null) return false;
      if (share.IsFtpShare)
      {
        //string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
        //  share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
        string remoteFolder = GetShareRemoteURL(share);
        if (strDir == remoteFolder)
        {
          return true;
        }
      }
      else
      {
        if (strDir == share.Path)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// This method checks if the specified folder is protected by a pincode
    /// </summary>
    /// <param name="strDir">folder to check</param>
    /// <param name="iPincode">on return: pincode to use or -1 when no pincode is needed</param>
    /// <returns>
    /// true : folder is protected by a pincode
    /// false: folder is not protected
    /// </returns>
    public bool IsProtectedShare(string strDir, out int iPincode)
    {
      iPincode = -1;
      Share share = GetShare(strDir);
      if (share == null) return false;
      iPincode = share.Pincode;
      if (share.Pincode >= 0)
      {
        if (share.IsFtpShare)
        {
          //string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
          //  share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
          string remoteFolder = GetShareRemoteURL(share);
          if (CurrentShare == remoteFolder)
          {
            return false;
          }
        }
        else
        {
          if (CurrentShare == share.Path)
          {
            return false;
          }
        }
        return true;
      }
      return false;
    }

    public void SetCurrentShare(string strDir)
    {
      //Setting current share;
      Share share = GetShare(strDir);
      if (share == null)
      {
        CurrentShare = "";
      }
      else if (share.IsFtpShare)
      {
        //string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
        //  share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
        string remoteFolder = GetShareRemoteURL(share);
        CurrentShare = remoteFolder;
      }
      else
      {
        CurrentShare = share.Path;
      }
    }

    public Share GetShare(string strDir)
    {
      if (strDir == null) return null;
      if (strDir.Length <= 0) return null;
      string strRoot = strDir;
      bool isRemote = IsRemote(strDir);
      if (!isRemote)
      {
        if (strDir != "cdda:")
          try
          {
            strRoot = System.IO.Path.GetFullPath(strDir);
          }
          catch (Exception)
          {
          }
      }
      Share foundShare = null;
      string foundFullPath = string.Empty;
      foreach (Share share in m_shares)
      {
        try
        {
          if (isRemote)
          {
            if (share.IsFtpShare)
            {
              //string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
              //  share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
              string remoteFolder = GetShareRemoteURL(share);
              if (strDir.ToLower() == remoteFolder.ToLower())
              {
                return share;
              }
              if (strDir.ToLower().StartsWith(remoteFolder.ToLower()))
              {
                if (foundShare == null)
                {
                  foundShare = share;
                  foundFullPath = foundShare.Path;
                }
                else
                {
                  //string foundRemoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                  //  foundShare.FtpServer, foundShare.FtpPort, foundShare.FtpLoginName, foundShare.FtpPassword, Utils.RemoveTrailingSlash(foundShare.FtpFolder));
                  string foundRemoteFolder = GetShareRemoteURL(foundShare);
                  if (foundRemoteFolder.Length < remoteFolder.Length)
                  {
                    foundShare = share;
                    foundFullPath = foundShare.Path;
                  }
                }
              }
            }
          }
          else
          {
            string strFullPath = share.Path;
            if (strRoot.ToLower().StartsWith(strFullPath.ToLower()))
            {
              if (strRoot.ToLower() == strFullPath.ToLower())
              {
                return share;
              }
              if (foundShare == null)
              {
                foundShare = share;
                foundFullPath = foundShare.Path;
              }
              else
              {
                if (foundFullPath.Length < strFullPath.Length)
                {
                  foundShare = share;
                  foundFullPath = foundShare.Path;
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
      return foundShare;
    }

    /// <summary>
    /// This method check is the given extension is a image file
    /// </summary>
    /// <param name="extension">file extension</param>
    /// <returns>
    /// true: if file is an image file (.img, .nrg, .bin, .iso, ...)
    /// false: if the file is not an image file
    /// </returns>
    static public bool IsImageFile(string extension)
    {
      return DaemonTools.IsImageFile(extension);
    }

    public bool IsRemote(string folder)
    {
      if (folder == null) return false;
      if (folder.IndexOf("remote:") == 0) return true;
      return false;
    }

    public string GetShareRemoteURL(Share shareName)
    {
      return String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                shareName.FtpServer,
                shareName.FtpPort,
                shareName.FtpLoginName,
                shareName.FtpPassword,
                MediaPortal.Util.Utils.RemoveTrailingSlash(shareName.FtpFolder));
    }

    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// If the folder is protected by a pincode then the user is asked to enter the pincode
    /// and the folder contents is only returned when the pincode is correct
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public ArrayList GetDirectory(string strDir)
    {
      bool res = WaitForPath(strDir);

      if ((strDir == null) || (strDir == "") || !res)
      {
        m_strPreviousDir = "";
        CurrentShare = "";
        return GetRoot();
      }

      //if we have a folder like D:\
      //then remove the \
      if (strDir.EndsWith(@"\"))
      {
        strDir = strDir.Substring(0, strDir.Length - 1);
      }

      ArrayList items = new ArrayList();

      //get the parent folder
      string strParent = "";
      if (IsRemote(strDir))
      {
        int ipos = strDir.LastIndexOf(@"/");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }
      else
      {
        int ipos = strDir.LastIndexOf(@"\");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }


      //is this directory protected
      int iPincodeCorrect;
      if (IsProtectedShare(strDir, out iPincodeCorrect))
      {
        bool retry = true;
        {
          while (retry)
          {
            //no, then ask user to enter the pincode
            GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
            GUIWindowManager.SendMessage(msgGetPassword);
            int iPincode = -1;
            try
            {
              iPincode = Int32.Parse(msgGetPassword.Label);
            }
            catch (Exception)
            {
            }
            if (iPincode != iPincodeCorrect)
            {
              GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgWrongPassword);

              if (!(bool)msgWrongPassword.Object)
              {
                GUIListItem itemTmp = new GUIListItem();
                itemTmp.IsFolder = true;
                itemTmp.Label = "..";
                itemTmp.Label2 = "";
                itemTmp.Path = m_strPreviousDir;
                Utils.SetDefaultIcons(itemTmp);
                items.Add(itemTmp);
                return items;
              }
            }
            else
              retry = false;
          }
        }
      }
      //Setting current share;
      SetCurrentShare(strDir);

      //check if this is an image file like .iso, .nrg,...
      //ifso then ask daemontools to automount it

      bool VirtualShare = false;
      if (!IsRemote(strDir))
      {
        if (DaemonTools.IsEnabled)
        {
          string extension = System.IO.Path.GetExtension(strDir);
          if (IsImageFile(extension))
          {
            if (!DaemonTools.IsMounted(strDir))
            {
              AutoPlay.StopListening();

              string virtualPath;
              if (DaemonTools.Mount(strDir, out virtualPath))
              {
                strDir = virtualPath;
                VirtualShare = true;
              }
              //Start listening to Volume Events.Wait 10 seconds in another thread and start listeneing again
              new System.Threading.Thread(new System.Threading.ThreadStart(this._startListening)).Start();
            }
            else
            {
              strDir = DaemonTools.GetVirtualDrive();
              VirtualShare = true;
            }

            if (VirtualShare/* && !g_Player.Playing*/) // dont interrupt if we're already playing
            {
              // If it looks like a DVD directory structure then return so
              // that the playing of the DVD is handled by the caller.
              if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
              {
                return items;
              }
            }
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        items.Add(item);
      }

      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Info("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              int pin;
              if (!IsProtectedShare(item.Path, out pin))
              {
                Utils.SetThumbnails(ref item);
              }
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(file.Name);
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      else
      {
        try
        {
          strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
          strFiles = System.IO.Directory.GetFiles(strDir + @"\");
        }
        catch (Exception)
        {
        }


        if (strDirs != null)
        {
          for (int i = 0; i < strDirs.Length; ++i)
          {
            string strPath = strDirs[i].Substring(strDir.Length + 1);

            // Skip hidden folders
            if ((!Directory.Exists(strDirs[i]) || (File.GetAttributes(strDirs[i]) & FileAttributes.Hidden) == FileAttributes.Hidden))
            {
              continue;
            }

            item = new GUIListItem();
            item.IsFolder = true;
            item.Label = strPath;
            item.Label2 = "";
            item.Path = strDirs[i];
            item.FileInfo = new FileInformation(strDirs[i], item.IsFolder);
            Utils.SetDefaultIcons(item);
            int pin;
            if (!IsProtectedShare(item.Path, out pin))
            {
              Utils.SetThumbnails(ref item);
            }
            items.Add(item);
          }
        }

        if (strFiles != null)
        {
          for (int i = 0; i < strFiles.Length; ++i)
          {
            string extension = System.IO.Path.GetExtension(strFiles[i]);
            if (IsImageFile(extension))
            {
              if (DaemonTools.IsEnabled)
              {

                item = new GUIListItem();
                item.IsFolder = true;
              	item.Label = System.IO.Path.GetFileName(strFiles[i]);  //Utils.GetFilename(strFiles[i]);
                item.Label2 = "";
                item.Path = strFiles[i];
                item.FileInfo = null;
                Utils.SetDefaultIcons(item);
                Utils.SetThumbnails(ref item);
                items.Add(item);
                continue;
              }
            }
            if (IsValidExtension(strFiles[i]))
            {
              // Skip hidden files
              if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              {
                continue;
              }

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];

              item.FileInfo = new FileInformation(strFiles[i], item.IsFolder);
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      m_strPreviousDir = strDir;
      return items;
    }


    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// This method does not check if the folder is protected by an pincode. it will
    /// always return all files/subfolders present
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public ArrayList GetDirectoryUnProtected(string strDir, bool useExtensions)
    {
      bool res = WaitForPath(strDir);

      if (strDir == null) return GetRoot();
      if (strDir == "") return GetRoot();
      if (!res) return GetRoot();

      //if we have a folder like D:\
      //then remove the \
      if (strDir.EndsWith(@"\"))
      {
        strDir = strDir.Substring(0, strDir.Length - 1);
      }
      ArrayList items = new ArrayList();

      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }


      bool VirtualShare = false;
      if (DaemonTools.IsEnabled)
      {
        string extension = System.IO.Path.GetExtension(strDir);
        if (IsImageFile(extension))
        {
          AutoPlay.StopListening();
          if (!DaemonTools.IsMounted(strDir))
          {
            string virtualPath;
            if (DaemonTools.Mount(strDir, out virtualPath))
            {
              strDir = virtualPath;
              VirtualShare = true;
            }
            //Start listening to Volume Events (Hack to start listening after 10 seconds)
            new System.Threading.Thread(new System.Threading.ThreadStart(this._startListening)).Start();
          }
          else
          {
            strDir = DaemonTools.GetVirtualDrive();
            VirtualShare = true;
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      try
      {
        strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
        strFiles = System.IO.Directory.GetFiles(strDir + @"\");
      }
      catch (Exception)
      {
      }


      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        items.Add(item);

      }
      if (strDirs != null)
      {
        for (int i = 0; i < strDirs.Length; ++i)
        {
          string strPath = strDirs[i].Substring(strDir.Length + 1);

          // Skip hidden folders
          if ((!Directory.Exists(strDirs[i]) || (File.GetAttributes(strDirs[i]) & FileAttributes.Hidden) == FileAttributes.Hidden))
          {
            continue;
          }

          item = new GUIListItem();
          item.IsFolder = true;
          item.Label = strPath;
          item.Label2 = "";
          item.Path = strDirs[i];
          item.FileInfo = new FileInformation(strDirs[i], item.IsFolder);
          Utils.SetDefaultIcons(item);
          int pin;
          if (!IsProtectedShare(item.Path, out pin))
          {
            Utils.SetThumbnails(ref item);
          }
          items.Add(item);
        }
      }

      if (strFiles != null)
      {
        for (int i = 0; i < strFiles.Length; ++i)
        {
          string extension = System.IO.Path.GetExtension(strFiles[i]);
          if (IsImageFile(extension))
          {
            if (DaemonTools.IsEnabled)
            {

              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
              continue;
            }
          }
          if (IsValidExtension(strFiles[i]) || (!useExtensions))
          {
            // Skip hidden files
            if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              if (useExtensions)
                continue;
            }

            item = new GUIListItem();
            item.IsFolder = false;
            item.Label = Utils.GetFilename(strFiles[i]);
            item.Label2 = "";
            item.Path = strFiles[i];
            item.FileInfo = new FileInformation(strFiles[i], item.IsFolder);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
          }
        }
      }
      return items;
    }

    /// <summary>
    /// This method checks if the extension of the specified file is valid for the current virtual folder
    /// The virtual directory will only show files with valid extensions
    /// </summary>
    /// <param name="strPath">filename</param>
    /// <returns>
    /// true : file has a valid extension
    /// false: file has an unknown extension
    /// </returns>
    public bool IsValidExtension(string strPath)
    {
      if (strPath == null) return false;
      if (strPath == String.Empty) return false;
      try
      {
        //				if (!System.IO.Path.HasExtension(strPath)) return false;
        // waeberd: allow searching for files without an extension
        if (!System.IO.Path.HasExtension(strPath)) return showFilesWithoutExtension;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if ((m_extensions[0] as string) == "*") return true;   // added for explorer modul by gucky
        for (int i = 0; i < m_extensions.Count; ++i)
        {
          if ((m_extensions[i] as string) == extensionFile) return true;
        }
      }
      catch (Exception) { }

      return false;
    }
    /// <summary>
    /// This method checks if the extension of the specified file is valid for the current virtual folder
    /// The virtual directory will only show files with valid extensions
    /// </summary>
    /// <param name="strPath">filename</param>
    /// <returns>
    /// true : file has a valid extension
    /// false: file has an unknown extension
    /// </returns>
    public static bool IsValidExtension(string strPath, ArrayList extensions, bool filesWithoutExtension)
    {
      if (strPath == null) return false;
      if (strPath == String.Empty) return false;
      try
      {
        //				if (!System.IO.Path.HasExtension(strPath)) return false;
        // waeberd: allow searching for files without an extension
        if (!System.IO.Path.HasExtension(strPath)) return filesWithoutExtension;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if ((extensions[0] as string) == "*") return true;   // added for explorer modul by gucky
        for (int i = 0; i < extensions.Count; ++i)
        {
          if ((extensions[i] as string) == extensionFile) return true;
        }
      }
      catch (Exception) { }

      return false;
    }

    /// <summary>
    /// Returns the path of the default share
    /// </summary>
    /// <returns>Returns the path of the default share
    /// or empty if no default path has been set
    /// </returns>
    public string GetDefaultPath()
    {
      foreach (Share share in m_shares)
      {
        if (!share.IsFtpShare && share.Default)
        {
          return share.Path;
        }
      }
      return String.Empty;
    }

    /// <summary>
    /// Function to split a string containg the remote file+path 
    /// and get the path and filename for a remote file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="remotefile">string containing the remote file</param>
    /// <param name="filename">on return contains the filename</param>
    /// <param name="path">on return contains the path</param>
    public void GetRemoteFileNameAndPath(string remotefile, out string filename, out string path)
    {
      filename = String.Empty;
      path = "/";
      if (!IsRemote(remotefile)) return;
      int slash = remotefile.LastIndexOf("/");

      if (slash > 0)
      {
        filename = remotefile.Substring(slash + 1);
        remotefile = remotefile.Substring(0, slash);
      }
      int questionMark = remotefile.LastIndexOf("?");
      if (questionMark > 0) path = remotefile.Substring(questionMark + 1);
    }

    /// <summary>
    /// Returns the local filename for a downloaded file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="remotefile">remote filename+path</param>
    /// <returns>local filename+path</returns>
    public string GetLocalFilename(string remotefile)
    {
      //get the default share
      if (!IsRemote(remotefile)) return remotefile;

      foreach (Share share in m_shares)
      {
        if (share.IsFtpShare)
        {
          string filename, path;
          GetRemoteFileNameAndPath(remotefile, out filename, out path);

          //string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
          //  share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
          string remoteFolder = GetShareRemoteURL(share);

          if (remotefile.IndexOf(remoteFolder) == 0)
          {
            string localFile = string.Format(@"{0}\{1}", Utils.RemoveTrailingSlash(share.Path), filename);
            return localFile;
          }
        }
      }
      return String.Empty;
    }

    /// <summary>
    /// Function to check if a remote file has been downloaded or not
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote filename + path</param>
    /// <returns>true: file is downloaded
    /// false: file is not downloaded or MP is busy downloading</returns>
    public bool IsRemoteFileDownloaded(string file, long size)
    {
      if (!IsRemote(file))
      {
        //Log.Debug("VirtualDirectory: File {0} is not remote", file);
        Log.Debug("VirtualDirectory: this file is not remote");
        return true;
      }

      string singleFilename = String.Empty;
      string singlePath = String.Empty;
      GetRemoteFileNameAndPath(file, out singleFilename, out singlePath);

      //check if we're still downloading
      if (FtpConnectionCache.IsDownloading(file))
      {
        Log.Debug("VirtualDirectory: Remote file {0} is downloading right now", singleFilename);
        return false;
      }

      //nop then check if local file exists
      string localFile = GetLocalFilename(file);
      if (localFile == String.Empty) return false;
      if (System.IO.File.Exists(localFile))
      {
        FileInfo info = new FileInfo(localFile);
        if (info.Length == size)
        {
          Log.Debug("VirtualDirectory: Remote file {0} is downloaded completely", singleFilename);
          //already downloaded
          return true;
        }

        Log.Debug("VirtualDirectory: Downloading remote file {0} already got {1} bytes", singleFilename, Convert.ToString(info.Length));
        // not completely downloaded yet
      }

      return false;
    }

    /// <summary>
    /// Function which checks if a remote file has been downloaded and if not
    /// asks the user whether it should download it
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download file
    /// false: do not download file</returns>
    public bool ShouldWeDownloadFile(string file)
    {

      GUIMessage msg;
      if (FtpConnectionCache.IsDownloading(file))
      {
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
        msg.Param1 = 916;
        msg.Param2 = 921;
        msg.Param3 = 0;
        msg.Param4 = 0;
        GUIWindowManager.SendMessage(msg);
        return false;
      }

      //file is remote, ask if user wants to download it
      //this file is on a remote share, ask if user wants to download it
      //to the default local share
      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
      msg.Param1 = 916;
      msg.Param2 = 917;
      msg.Param3 = 918;
      msg.Param4 = 919;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1 == 1) return true;
      return false;
    }

    /// <summary>
    /// Function to download a remote file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download started,
    /// false: failed to start download</returns>
    /// <remarks>file is downloaded to the local folder specified in the setup</remarks>
    public bool DownloadRemoteFile(string file, long size)
    {
      if (IsRemoteFileDownloaded(file, size)) return true;

      //start download...
      FTPClient client = GetFtpClient(file);
      if (client == null)
      {
        //Log.Debug("VirtualDirectory: DownloadRemoteFile {0} aborted due to previous errors", file);
        Log.Debug("VirtualDirectory: DownloadRemoteFile aborted due to previous errors");
        return false;
      }

      string remoteFilename;
      string remotePath;
      GetRemoteFileNameAndPath(file, out remoteFilename, out remotePath);
      try
      {
        client.ChDir(remotePath);
      }
      catch (Exception ex)
      {
        Log.Error("VirtualDirectory: Error on ftp command ChDir {0}", ex.Message);
        FtpConnectionCache.Remove(client);
        client = GetFtpClient(file);
        if (client == null) return false;
      }
      try
      {
        // client.ChDir(remotePath);
        string localFile = GetLocalFilename(file);
        //Log.Debug("VirtualDirectory: Trying to download file: {0} remote: {1} local: {2}", file, remoteFilename, localFile);
        Log.Debug("VirtualDirectory: Trying to download remote file: {0} local: {1}", remoteFilename, localFile);
        return FtpConnectionCache.Download(client, file, remoteFilename, localFile);
      }
      catch (Exception ex)
      {
        Log.Error("VirtualDirectory:Unable to start download:{0}", ex.Message, ex.StackTrace);
      }
      return false;
    }

    /// <summary>
    /// Function which gets an available ftp client 
    /// if none is available it creates a new one
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file/folder</param>
    /// <returns>FTP client or null</returns>
    FTPClient GetFtpClient(string file)
    {
      bool ActiveConnection = true;
      string folder = file.Substring("remote:".Length);
      string[] subitems = folder.Split(new char[] { '?' });
      if (subitems[4] == String.Empty) subitems[4] = "/";
      //if (subitems[5] == null) 
      ActiveConnection = true;
      //else
      //  ActiveConnection = Convert.ToBoolean(subitems[5]);      
      int port = 21;
      unchecked
      {
        port = Int32.Parse(subitems[1]);
      }
      FTPClient ftp;
      if (!FtpConnectionCache.InCache(subitems[0], subitems[2], subitems[3], port, ActiveConnection, out ftp))
      {
        ftp = FtpConnectionCache.MakeConnection(subitems[0], subitems[2], subitems[3], port, ActiveConnection);
      }
      if (ftp == null)
        Log.Info("VirtualDirectory:unable to connect to remote share");
      return ftp;
    }
    #region generics

    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// If the folder is protected by a pincode then the user is asked to enter the pincode
    /// and the folder contents is only returned when the pincode is correct
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public List<GUIListItem> GetDirectoryExt(string strDir)
    {
      bool res = WaitForPath(strDir);
      if ((strDir == null) || (strDir == "") || !res)
      {
        m_strPreviousDir = "";
        CurrentShare = "";
        return GetRootExt();
      }

      //if we have a folder like D:\
      //then remove the \
      if (strDir.EndsWith(@"\"))
      {
        strDir = strDir.Substring(0, strDir.Length - 1);
      }

      List<GUIListItem> items = new List<GUIListItem>();

      if (strDir.Length > 254)
      {
        Log.Warn("VirtualDirectory: GetDirectoryExt received a path which contains too many chars");
        return items;
      }

      //get the parent folder
      string strParent = "";
      if (IsRemote(strDir))
      {
        int ipos = strDir.LastIndexOf(@"/");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }
      else
      {
        int ipos = strDir.LastIndexOf(@"\");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }


      //is this directory protected
      int iPincodeCorrect;
      if (IsProtectedShare(strDir, out iPincodeCorrect))
      {
        bool retry = true;
        {
          while (retry)
          {
            //no, then ask user to enter the pincode
            GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
            GUIWindowManager.SendMessage(msgGetPassword);
            int iPincode = -1;
            try
            {
              iPincode = Int32.Parse(msgGetPassword.Label);
            }
            catch (Exception)
            {
            }
            if (iPincode != iPincodeCorrect)
            {
              GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgWrongPassword);

              if (!(bool)msgWrongPassword.Object)
              {
                GUIListItem itemTmp = new GUIListItem();
                itemTmp.IsFolder = true;
                itemTmp.Label = "..";
                itemTmp.Label2 = "";
                itemTmp.Path = m_strPreviousDir;
                Utils.SetDefaultIcons(itemTmp);
                items.Add(itemTmp);
                return items;
              }
            }
            else
              retry = false;
          }
        }
      }
      //Setting current share;
      SetCurrentShare(strDir);

      //check if this is an image file like .iso, .nrg,...
      //ifso then ask daemontools to automount it

      bool VirtualShare = false;
      if (!IsRemote(strDir))
      {
        if (DaemonTools.IsEnabled)
        {
          string extension = System.IO.Path.GetExtension(strDir);
          if (IsImageFile(extension))
          {
            if (!DaemonTools.IsMounted(strDir))
            {
              AutoPlay.StopListening();
              string virtualPath;
              if (DaemonTools.Mount(strDir, out virtualPath))
              {
                strDir = virtualPath;
                VirtualShare = true;
              }
              //Start listening to Volume Events (Hack to start listening after 10 seconds)
              new System.Threading.Thread(new System.Threading.ThreadStart(this._startListening)).Start();
            }
            else
            {
              strDir = DaemonTools.GetVirtualDrive();
              VirtualShare = true;
            }

            if (VirtualShare /*&& !g_Player.Playing*/) // dont interrupt if we're already playing
            {
              // If it looks like a DVD directory structure then return so
              // that the playing of the DVD is handled by the caller.
              if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
              {
                return items;
              }
            }
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        items.Add(item);
      }

      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Info("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              int pin;
              if (!IsProtectedShare(item.Path, out pin))
              {
                Utils.SetThumbnails(ref item);
              }
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(file.Name);
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      else
      {
        /* Here is the trick to play Enhanced CD and not only CD-DA: 
         * we force the creation of GUIListItem of Red Book tracks.
         * Track names are made up on-the-fly, using naming conventions
         * (Track%%.cda) and are not really retrieved from the drive
         * directory, as opposed to the old method.
         * If the data session happens to be the first to be recorded
         * then the old method can't read the Red Book structure.
         * Current audio tracks number is found using Windows API 
         * (dependances on the Ripper package).
         * These are not really limitations (because it was already
         * the way CD-DA playing was handled before) but worth noting:
         * -The drive structure is browsed twice, once in Autoplay and
         *    the other one here.
         * -CD-DA and Enhanced CD share the same methods.
         * -A CD Audio must be a Windows drive letter followed by a
         *    semi-colon (mounted images should work, Windows shared
         *    drives seem not to).
         * Works with Windows Media Player
         * Seems to work with Winamp (didn't make more than one experiment)
         * Wasn't tested with other players
         */

        bool doesContainRedBookData = false;

        if (item.IsFolder && strDir.Length == 2 && strDir[1] == ':')
        {
          try
          {
            CDDrive m_Drive = new CDDrive();

            if (m_Drive.IsOpened)
              m_Drive.Close();

            if (m_Drive.Open(strDir[0]) && m_Drive.IsCDReady() && m_Drive.Refresh())
            {
              int totalNumberOfTracks = m_Drive.GetNumTracks();
              int totalNumberOfRedBookTracks = 0;
              for (int i = 1; i <= totalNumberOfTracks; i++)
                if (m_Drive.IsAudioTrack(i))
                {
                  doesContainRedBookData = true;
                  totalNumberOfRedBookTracks++;
                }
              strFiles = new string[totalNumberOfRedBookTracks];
              for (int j = 1; j <= totalNumberOfRedBookTracks; j++)
                strFiles[j - 1] = string.Format("{0}\\Track{1:00}.cda", strDir, j);
            }
            m_Drive.Close();
          }
          catch (Exception)
          { }
        }

        if (!doesContainRedBookData)
          try
          {
            strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
            strFiles = System.IO.Directory.GetFiles(strDir + @"\");
          }
          catch (Exception)
          { }

        if (strDirs != null)
        {
          for (int i = 0; i < strDirs.Length; ++i)
          {
            string strPath = strDirs[i].Substring(strDir.Length + 1);

            // Skip hidden folders
            if ((!Directory.Exists(strDirs[i]) || (File.GetAttributes(strDirs[i]) & FileAttributes.Hidden) == FileAttributes.Hidden))
            {
              continue;
            }

            item = new GUIListItem();
            item.IsFolder = true;
            item.Label = strPath;
            item.Label2 = "";
            item.Path = strDirs[i];
            item.FileInfo = new FileInformation(strDirs[i], item.IsFolder);
            Utils.SetDefaultIcons(item);
            int pin;
            if (!IsProtectedShare(item.Path, out pin))
            {
              Utils.SetThumbnails(ref item);
            }
            items.Add(item);
          }
        }

        if (strFiles != null)
        {
          for (int i = 0; i < strFiles.Length; ++i)
          {
            //<OKAY_AWRIGHT-310506>
            string extension;
            if (!doesContainRedBookData)
              extension = System.IO.Path.GetExtension(strFiles[i]);
            else
              extension = ".cda";
            //</OKAY_AWRIGHT-310506>

            if (IsImageFile(extension))
            {
              if (DaemonTools.IsEnabled)
              {

                item = new GUIListItem();
                item.IsFolder = true;
                item.Label = Utils.GetFilename(strFiles[i]);
                item.Label2 = "";
                item.Path = strFiles[i];
                item.FileInfo = null;
                Utils.SetDefaultIcons(item);
                Utils.SetThumbnails(ref item);
                items.Add(item);
                continue;
              }
            }
            if (IsValidExtension(strFiles[i]))
            {
              // Skip too long path
              if (strFiles[i].Length > 254)
              {
                Log.Warn("VirtualDirectory: GetDirectoryExt - a path contains too many chars: {0}", strFiles[i]);
                continue;
              }

              // Skip hidden files
              //<OKAY_AWRIGHT-310506>
              if (!doesContainRedBookData && (File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              //</OKAY_AWRIGHT-310506>
              {
                continue;
              }

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];

              //<OKAY_AWRIGHT-310506>
              if (!doesContainRedBookData)
                item.FileInfo = new FileInformation(strFiles[i], item.IsFolder);
              else
              {
                item.FileInfo = new FileInformation();
                DateTime modified = System.DateTime.Now;
                item.FileInfo.CreationTime = modified;
                item.FileInfo.Length = 0;
              }

              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
              //</OKAY_AWRIGHT-310506>
            }
          }
        }
      }
      m_strPreviousDir = strDir;
      return items;
    }


    private bool WaitForPath(string pathName)
    {
      // while waking up from hibernation it can take a while before a network drive is accessible.
      // lets wait 10 sec      
      int count = 0;      

      if (pathName.Length == 0 || pathName == "root")
      {
        return true;
      }
      
      //we cant be sure if pathName is a file or a folder, so we look for both.      
      while ((!Directory.Exists(pathName) && !File.Exists(pathName)) && count < 100)
      {
        System.Threading.Thread.Sleep(100);
        count++;
      }
                  
      return (count < 100);
    }

    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// This method does not check if the folder is protected by an pincode. it will
    /// always return all files/subfolders present
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public List<GUIListItem> GetDirectoryUnProtectedExt(string strDir, bool useExtensions)
    {
      bool res = WaitForPath(strDir);

      if (strDir == null) return GetRootExt();
      if (strDir == "") return GetRootExt();
      if (!res) return GetRootExt();

      if (strDir.Substring(1) == @"\") strDir = strDir.Substring(0, strDir.Length - 1);
      List<GUIListItem> items = new List<GUIListItem>();

      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }

      GUIListItem item = null;
      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Info("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Info("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              int pin;
              if (!IsProtectedShare(item.Path, out pin))
              {
                Utils.SetThumbnails(ref item);
              }
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name) || (useExtensions == false))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(file.Name);
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }


      bool VirtualShare = false;
      if (DaemonTools.IsEnabled)
      {
        string extension = System.IO.Path.GetExtension(strDir);
        if (IsImageFile(extension))
        {
          if (!DaemonTools.IsMounted(strDir))
          {
            AutoPlay.StopListening();
            string virtualPath;
            if (DaemonTools.Mount(strDir, out virtualPath))
            {
              strDir = virtualPath;
              VirtualShare = true;
            }
            //Start listening to Volume Events.Wait 10 seconds in another thread and start listeneing again
            new System.Threading.Thread(new System.Threading.ThreadStart(this._startListening)).Start();
          }
          else
          {
            strDir = DaemonTools.GetVirtualDrive();
            VirtualShare = true;
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      try
      {
        strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
        strFiles = System.IO.Directory.GetFiles(strDir + @"\");
      }
      catch (Exception)
      {
      }


      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        items.Add(item);
      }
      if (strDirs != null)
      {
        for (int i = 0; i < strDirs.Length; ++i)
        {
          string strPath = strDirs[i].Substring(strDir.Length + 1);

          // Skip hidden folders
          if ((!Directory.Exists(strDirs[i]) || (File.GetAttributes(strDirs[i]) & FileAttributes.Hidden) == FileAttributes.Hidden))
          {
            continue;
          }

          item = new GUIListItem();
          item.IsFolder = true;
          item.Label = strPath;
          item.Label2 = "";
          item.Path = strDirs[i];
          item.FileInfo = new FileInformation(strDirs[i], item.IsFolder);
          Utils.SetDefaultIcons(item);
          int pin;
          if (!IsProtectedShare(item.Path, out pin))
          {
            Utils.SetThumbnails(ref item);
          }
          items.Add(item);
        }
      }

      if (strFiles != null)
      {
        for (int i = 0; i < strFiles.Length; ++i)
        {
          string extension = System.IO.Path.GetExtension(strFiles[i]);
          if (IsImageFile(extension))
          {
            if (DaemonTools.IsEnabled)
            {

              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
              continue;
            }
          }
          if (IsValidExtension(strFiles[i]) || (!useExtensions))
          {
            // Skip hidden files
            if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              if (useExtensions)
                continue;
            }

            item = new GUIListItem();
            item.IsFolder = false;
            item.Label = Utils.GetFilename(strFiles[i]);
            item.Label2 = "";
            item.Path = strFiles[i];
            item.FileInfo = new FileInformation(strFiles[i], item.IsFolder);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
          }
        }
      }
      return items;
    }
    /// <summary>
    /// This method will return the root folder
    /// which contains a list of all shares
    /// </summary>
    /// <returns>
    /// ArrayList containing a GUIListItem for each share
    /// </returns>
    public List<GUIListItem> GetRootExt()
    {
      previousShare = String.Empty;

      List<GUIListItem> items = new List<GUIListItem>();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
        if (Utils.IsRemovable(item.Path) && Directory.Exists(item.Path))
        {
          string driveName = Utils.GetDriveName(item.Path);
          if (driveName == "") driveName = GUILocalizeStrings.Get(1061);
          item.Label = String.Format("({0}) {1}", item.Path, driveName);
        }
        if (Utils.IsDVD(item.Path))
        {
          item.DVDLabel = Utils.GetDriveName(item.Path);
          item.DVDLabel = item.DVDLabel.Replace('_', ' ');
        }

        if (item.DVDLabel != "")
        {
          item.Label = String.Format("({0}) {1}", item.Path, item.DVDLabel);
        }
        else
          item.Label = share.Name;
        item.IsFolder = true;

        if (share.IsFtpShare)
        {
          //item.Path = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
          //      share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
          item.Path = GetShareRemoteURL(share);
          item.IsRemote = true;
        }
        Utils.SetDefaultIcons(item);
        if (share.Pincode < 0)
        {
          string coverArt = Utils.GetCoverArtName(item.Path, "folder");
          string largeCoverArt = Utils.GetLargeCoverArtName(item.Path, "folder");
          if (System.IO.File.Exists(coverArt))
          {
            item.IconImage = coverArt;
          }
          if (System.IO.File.Exists(largeCoverArt))
          {
            item.IconImageBig = largeCoverArt;
          }
        }
        items.Add(item);
      }

      // add removable drives with media
      string[] drives = Environment.GetLogicalDrives();
      foreach (string drive in drives)
      {
        if (drive[0] > 'B' && Util.Utils.getDriveType(drive) == Removable)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);
          string driveLetter = drive.Substring(0, 1).ToUpper() + ":";
          if (driveName == "") driveName = GUILocalizeStrings.Get(1061);

          //
          // Check if the share already exists
          //
          foreach (Share share in m_shares)
          {
            if (share.Path == driveLetter)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            GUIListItem item = new GUIListItem();
            item.Path = driveLetter;
            item.Label = String.Format("({0}) {1}", item.Path, driveName);
            item.IsFolder = true;

            Utils.SetDefaultIcons(item);

            // dont add removable shares without media
            // virtual cd/dvd drive (daemontools) without mounted image
            if (!Directory.Exists(item.Path))
              break;

            items.Add(item);
          }
        }
      }

      return items;
    }

    private void _startListening()
    {
      System.Threading.Thread.Sleep(5000);
      AutoPlay.StartListening();
      Log.Info("*****Start listening to drives");
    }

    static public void SetInitialDefaultShares(bool addOptical, bool addMusic, bool addPictures, bool addVideos)
    {
      ArrayList sharesVideos = new ArrayList();
      ArrayList sharesMusic = new ArrayList();
      ArrayList sharesPhotos = new ArrayList();

      // add optical drive letters
      if (addOptical)
      {
        string[] drives = Environment.GetLogicalDrives();
        foreach (string drive in drives)
        {
          int driveType = Util.Utils.getDriveType(drive);
          if (driveType == (int)DriveType.CDRom)
          {
            string driveName = String.Format("({0}:) CD/DVD", drive.Substring(0, 1).ToUpper());
            Share share = new Share(driveName, drive, -1);
            sharesMusic.Add(share);
            sharesPhotos.Add(share);
            sharesVideos.Add(share);
          }
        }
      }

      // add user profile dirs
      string MusicProfilePath = Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYMUSIC);
      if (addMusic)
      {        
        Share MusicShare = new Share(GetShareNameDefault(MusicProfilePath), MusicProfilePath, -1);
        sharesMusic.Add(MusicShare);        
      }

      string PicturesProfilePath = Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYPICTURES);
      if (addPictures)
      {        
        Share PicShare = new Share(GetShareNameDefault(PicturesProfilePath), PicturesProfilePath, -1);
        sharesPhotos.Add(PicShare);        
      }

      string VideoProfilePath = Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYVIDEO);
      if (addVideos)
      {        
        Share VidShare = new Share(GetShareNameDefault(VideoProfilePath), VideoProfilePath, -1);
        sharesVideos.Add(VidShare);
      }

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (addMusic)
        {
          xmlwriter.SetValue("music", "default", MusicProfilePath);
          SaveShare(sharesMusic, "music");
        }
        if (addPictures)
        {
          xmlwriter.SetValue("pictures", "default", PicturesProfilePath);
          SaveShare(sharesPhotos, "pictures");
        }
        if (addVideos)
        {
          xmlwriter.SetValue("movies", "default", VideoProfilePath);
          SaveShare(sharesVideos, "movies");
        }
      }
    }

    static public string GetShareNameDefault(string folder)
    {
      string name = folder;
      int pos = folder.LastIndexOf(@"\");
      if (pos > 0)
      {
        name = name.Substring(pos + 1);
      }
      return name;
    }

    static void SaveShare(ArrayList sharesList, string mediaType)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        for (int index = 0; index < MaximumShares; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);

          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);

          string shareNameData = String.Empty;
          string sharePathData = String.Empty;
          int sharePinData = -1;

          bool shareTypeData = false;
          string shareServerData = String.Empty;
          string shareLoginData = String.Empty;
          string sharePwdData = String.Empty;
          int sharePortData = 21;
          string shareRemotePathData = String.Empty;

          if (sharesList != null && sharesList.Count > index)
          {
            Share shareData = sharesList[index] as Share;

            if (shareData != null)
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Path;
              sharePinData = shareData.Pincode;

              shareTypeData = shareData.IsFtpShare;
              shareServerData = shareData.FtpServer;
              shareLoginData = shareData.FtpLoginName;
              sharePwdData = shareData.FtpPassword;
              sharePortData = shareData.FtpPort;
              shareRemotePathData = shareData.FtpFolder;
            }
          }

          xmlwriter.SetValue(mediaType, shareName, shareNameData);
          xmlwriter.SetValue(mediaType, sharePath, sharePathData);
          xmlwriter.SetValue(mediaType, sharePin, sharePinData);

          xmlwriter.SetValueAsBool(mediaType, shareType, shareTypeData);
          xmlwriter.SetValue(mediaType, shareServer, shareServerData);
          xmlwriter.SetValue(mediaType, shareLogin, shareLoginData);
          xmlwriter.SetValue(mediaType, sharePwd, sharePwdData);
          xmlwriter.SetValue(mediaType, sharePort, sharePortData.ToString());
          xmlwriter.SetValue(mediaType, shareRemotePath, shareRemotePathData);
        }

      }
    }
  }
    #endregion

  /// <summary>
  /// Singleton class that returns instances to the diffrent kind
  /// of virtual directories music, movies ..
  /// Loads virtual directory information on demand
  /// </summary>

  public class VirtualDirectories
  {
    internal static VirtualDirectories _Instance = null;

    VirtualDirectory _Music = null;
    VirtualDirectory _Movies = null;
    VirtualDirectory _Pictures = null;

    private VirtualDirectories()
    {
    }

    public static VirtualDirectories Instance
    {
      get
      {
        if (_Instance == null)
          _Instance = new VirtualDirectories();
        return _Instance;
      }
    }

    public VirtualDirectory Music
    {
      get
      {
        if (_Music == null)
        {
          _Music = new VirtualDirectory();
          _Music.LoadSettings("music");
        }
        return _Music;
      }
    }

    public VirtualDirectory Movies
    {
      get
      {
        if (_Movies == null)
        {
          _Movies = new VirtualDirectory();
          _Movies.LoadSettings("movies");
          _Movies.AddDrives();
          _Movies.SetExtensions(MediaPortal.Util.Utils.VideoExtensions);
          _Movies.AddExtension(".m3u");
        }
        return _Movies;
      }
    }

    public VirtualDirectory Pictures
    {
      get
      {
        if (_Pictures == null)
        {
          _Pictures = new VirtualDirectory();
          _Pictures.LoadSettings("pictures");
        }
        return _Pictures;
      }
    }

  }
}