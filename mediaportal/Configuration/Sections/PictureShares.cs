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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class PictureShares : MediaPortal.Configuration.Sections.Shares
  {
    private System.ComponentModel.IContainer components = null;

    public PictureShares()
      : this("Picture Folders")
    {
    }

    public PictureShares(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string defaultShare = xmlreader.GetValueAsString("pictures", "default", "");
        RememberLastFolder = xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false);

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
          string shareViewPath = String.Format("shareview{0}", index);

          string shareNameData = xmlreader.GetValueAsString("pictures", shareName, "");
          string sharePathData = xmlreader.GetValueAsString("pictures", sharePath, "");
          string sharePinData = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString("pictures", sharePin, ""));

          // provide one default share
          if (index == 0 && shareNameData == string.Empty)
          {
            string PicturesProfilePath = Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYPICTURES);
            shareNameData = Util.VirtualDirectory.GetShareNameDefault(PicturesProfilePath);
            sharePathData = PicturesProfilePath;
            sharePinData = string.Empty;
          }

          bool shareTypeData = xmlreader.GetValueAsBool("pictures", shareType, false);
          string shareServerData = xmlreader.GetValueAsString("pictures", shareServer, "");
          string shareLoginData = xmlreader.GetValueAsString("pictures", shareLogin, "");
          string sharePwdData = xmlreader.GetValueAsString("pictures", sharePwd, "");
          int sharePortData = xmlreader.GetValueAsInt("pictures", sharePort, 21);
          string shareRemotePathData = xmlreader.GetValueAsString("pictures", shareRemotePath, "/");
          int shareView = xmlreader.GetValueAsInt("pictures", shareViewPath, (int)ShareData.Views.List);

          if (shareNameData != null && shareNameData.Length > 0)
          {
            ShareData newShare = new ShareData(shareNameData, sharePathData, sharePinData);
            newShare.IsRemote = shareTypeData;
            newShare.Server = shareServerData;
            newShare.LoginName = shareLoginData;
            newShare.PassWord = sharePwdData;
            newShare.Port = sharePortData;
            newShare.RemoteFolder = shareRemotePathData;
            newShare.DefaultView = (ShareData.Views)shareView;

            AddShare(newShare, shareNameData.Equals(defaultShare));
          }
        }
      }

      //
      // Add static shares
      //
      AddStaticShares(DriveType.DVD, "DVD");
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string defaultShare = string.Empty;

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
          string shareViewPath = String.Format("shareview{0}", index);

          string shareNameData = string.Empty;
          string sharePathData = string.Empty;
          string sharePinData = string.Empty;

          bool shareTypeData = false;
          string shareServerData = string.Empty;
          string shareLoginData = string.Empty;
          string sharePwdData = string.Empty;
          int sharePortData = 21;
          string shareRemotePathData = string.Empty;
          int shareView = (int)ShareData.Views.List;

          if (CurrentShares != null && CurrentShares.Count > index)
          {
            ShareData shareData = CurrentShares[index].Tag as ShareData;

            if (shareData != null)
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData = shareData.PinCode;

              shareTypeData = shareData.IsRemote;
              shareServerData = shareData.Server;
              shareLoginData = shareData.LoginName;
              sharePwdData = shareData.PassWord;
              sharePortData = shareData.Port;
              shareRemotePathData = shareData.RemoteFolder;
              shareView = (int)shareData.DefaultView;

              if (CurrentShares[index] == DefaultShare)
                defaultShare = shareNameData;
            }
          }
          xmlwriter.SetValue("pictures", shareName, shareNameData);
          xmlwriter.SetValue("pictures", sharePath, sharePathData);
          xmlwriter.SetValue("pictures", sharePin, MediaPortal.Util.Utils.EncryptPin(sharePinData));

          xmlwriter.SetValueAsBool("pictures", shareType, shareTypeData);
          xmlwriter.SetValue("pictures", shareServer, shareServerData);
          xmlwriter.SetValue("pictures", shareLogin, shareLoginData);
          xmlwriter.SetValue("pictures", sharePwd, sharePwdData);
          xmlwriter.SetValue("pictures", sharePort, sharePortData.ToString());
          xmlwriter.SetValue("pictures", shareRemotePath, shareRemotePathData);
          xmlwriter.SetValue("pictures", shareViewPath, shareView);
        }
        xmlwriter.SetValue("pictures", "default", defaultShare);
        xmlwriter.SetValueAsBool("pictures", "rememberlastfolder", RememberLastFolder);
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
    }
    #endregion
  }
}

