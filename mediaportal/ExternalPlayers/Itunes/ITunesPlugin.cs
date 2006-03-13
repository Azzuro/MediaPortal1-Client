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
using MediaPortal.GUI.Library;
using MediaPortal.Player;

using iTunesLib;

namespace MediaPortal.ITunesPlayer
{
  /// <summary>
  /// Summary description for ITunesPlugin.
  /// </summary>
  public class ITunesPlugin : IExternalPlayer
  {
    iTunesLib.IiTunes _iTunesApplication = null;
    bool _playerIsPaused;
    string _currentFile = String.Empty;
    bool _started;

    private string[] m_supportedExtensions = new string[0];
    public ITunesPlugin()
    {
    }

    public override string Description()
    {
      return "Apple iTunes media player - http://www.apple.com/itunes";
    }

    public override void ShowPlugin()
    {
      ConfigurationForm confForm = new ConfigurationForm();
      confForm.ShowDialog();
    }

    public override string PlayerName
    {
      get { return "iTunes"; }
    }

    /// <summary>
    /// This method returns the version number of the plugin
    /// </summary>
    public override string VersionNumber
    {
      get { return "1.1"; }
    }

    /// <summary>
    /// This method returns the author of the external player
    /// </summary>
    /// <returns></returns>
    public override string AuthorName
    {
      get { return "Frodo"; }
    }

    /// <summary>
    /// Returns all the extensions that the external player supports.  
    /// The return value is an array of extensions of the form: .wma, .mp3, etc...
    /// </summary>
    /// <returns>array of strings of extensions in the form: .wma, .mp3, etc..</returns>
    public override string[] GetAllSupportedExtensions()
    {
      readConfig();
      return m_supportedExtensions;
    }


    /// <summary>
    /// Returns true or false depending if the filename passed is supported or not.
    /// The filename could be just the filename or the complete path of a file.
    /// </summary>
    /// <param name="filename">a fully qualified path and filename or just the filename</param>
    /// <returns>true or false if the file is supported by the player</returns>
    public override bool SupportsFile(string filename)
    {
      readConfig();
      string ext = null;
      int dot = filename.LastIndexOf(".");    // couldn't find the dot to get the extension
      if (dot == -1) return false;

      ext = filename.Substring(dot).Trim();
      if (ext.Length == 0) return false;   // no extension so return false;

      ext = ext.ToLower();

      for (int i = 0; i < m_supportedExtensions.Length; i++)
      {
        if (m_supportedExtensions[i].Equals(ext))
          return true;
      }

      // could not match the extension, so return false;
      return false;
    }


    private void readConfig()
    {
      string strExt = null;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        strExt = xmlreader.GetValueAsString("itunesplugin", "enabledextensions", "");
      }
      if (strExt != null && strExt.Length > 0)
      {
        m_supportedExtensions = strExt.Split(new char[] { ':', ',' });
        for (int i = 0; i < m_supportedExtensions.Length; i++)
        {
          m_supportedExtensions[i] = m_supportedExtensions[i].Trim();
        }
      }
    }


    public override bool Play(string strFile)
    {
      try
      {
        if (_iTunesApplication == null)
        {
          _iTunesApplication = new iTunesLib.iTunesAppClass();
        }

        _started = false;
        _iTunesApplication.Stop();
        _iTunesApplication.PlayFile(strFile);

        _playerIsPaused = false;
        _currentFile = strFile;

        UpdateStatus();
        return true;
      }
      catch (Exception)
      {
        _iTunesApplication = null;
      }
      return false;
    }

    public override double Duration
    {
      get
      {
        if (_iTunesApplication == null) return 0.0d;
        UpdateStatus();
        if (_started==false) return 300;
        try
        {
          return _iTunesApplication.CurrentTrack.Duration;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0.0d;
        }
      }
    }

    public override double CurrentPosition
    {
      get
      {
        try
        {
          if (_iTunesApplication == null) return 0.0d;
          UpdateStatus();
          if (_started==false) return 0.0d;
          return (double)_iTunesApplication.PlayerPosition;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0.0d;
        }
      }
    }

    public override void Pause()
    {
      if (_iTunesApplication == null) return;
      UpdateStatus();
      if (_started == false) return;
      try
      {
        if (Paused)
        {
          _iTunesApplication.Play();
          _playerIsPaused = false;
        }
        else
        {
          _iTunesApplication.Pause();
          _playerIsPaused = true;
        }
      }
      catch (Exception)
      {
        _iTunesApplication = null;
        return;
      }
    }

    public override bool Paused
    {
      get
      {
        try
        {
          UpdateStatus();
          if (_started == false) return false;
          if (_iTunesApplication == null) return false;
          return _playerIsPaused;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return false;
        }
      }
    }

    public override bool Playing
    {
      get
      {
        try
        {
          if (_iTunesApplication == null)
            return false;
          UpdateStatus();
          if (_started == false) return true;
          if (Paused) return true;
          return (_iTunesApplication.PlayerState != ITPlayerState.ITPlayerStateStopped);

        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return false;
        }
      }
    }

    public override bool Ended
    {
      get
      {
        if (_iTunesApplication == null)
          return true;
        try
        {
          UpdateStatus();
          if (_started == false) return false;
          if (Paused) return false;
          return (_iTunesApplication.PlayerState == ITPlayerState.ITPlayerStateStopped);

        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return true;
        }
      }
    }

    public override bool Stopped
    {
      get
      {
        try
        {
          if (_iTunesApplication == null)
            return true;
          UpdateStatus();
          if (_started == false) return false;
          if (Paused) return false;
          return (_iTunesApplication.PlayerState == ITPlayerState.ITPlayerStateStopped);

        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return true;
        }
      }
    }

    public override string CurrentFile
    {
      get
      {
        return _currentFile;
      }
    }

    public override void Stop()
    {
      if (_iTunesApplication == null) return;
      try
      {
        _iTunesApplication.Stop();
        _playerIsPaused = false;
        _started = false;
      }
      catch (Exception)
      {
        _iTunesApplication = null;
      }
    }

    public override int Volume
    {
      get
      {
        if (_iTunesApplication == null) return 0;
        try
        {
          return _iTunesApplication.SoundVolume;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0;
        }
      }
      set
      {
        if (_iTunesApplication == null || value < 0 || value > 100) return;
        _iTunesApplication.SoundVolume = value;
        try
        {

        }
        catch (Exception)
        {
          _iTunesApplication = null;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      double dCurTime = CurrentPosition;
      dTime = dCurTime + dTime;
      if (dTime < 0.0d) dTime = 0.0d;
      if (dTime < Duration)
      {
        SeekAbsolute(dTime);
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (dTime < 0.0d) dTime = 0.0d;
      if (dTime < Duration)
      {
        //m_winampController.Position = dTime;
        if (_iTunesApplication == null) return;
        try
        {
          _iTunesApplication.PlayerPosition = (int)dTime;
        }
        catch (Exception) { }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      double dCurrentPos = CurrentPosition;
      double dDuration = Duration;

      double fCurPercent = (dCurrentPos / Duration) * 100.0d;
      double fOnePercent = Duration / 100.0d;
      fCurPercent = fCurPercent + (double)iPercentage;
      fCurPercent *= fOnePercent;
      if (fCurPercent < 0.0d) fCurPercent = 0.0d;
      if (fCurPercent < Duration)
      {
        SeekAbsolute(fCurPercent);
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (iPercentage < 0) iPercentage = 0;
      if (iPercentage >= 100) iPercentage = 100;
      double fPercent = Duration / 100.0f;
      fPercent *= (double)iPercentage;
      SeekAbsolute(fPercent);
    }
    private void UpdateStatus()
    {
      if (_started) return;
      _started = (_iTunesApplication.PlayerState == ITPlayerState.ITPlayerStatePlaying);
    }

  }
}
