using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using TvDatabase;

namespace MPTvClient
{
  public class ReceptionDetails
  {
    public int signalLevel;
    public int signalQuality;
  }
  public class StreamingStatus
  {
    public int cardId;
    public string cardName;
    public string cardType;
    public string status;
    public string channelName;
    public string userName;
  }
  public class ProgrammInfo
  {
    public string timeInfo;
    public string description;
  }
  public class EPGInfo
  {
    public DateTime startTime;
    public DateTime endTime;
    public string title;
    public string description;
  }
  public class ChannelInfo
  {
    public string channelID;
    public string name;
    public bool isWebStream;
    public ProgrammInfo epgNow;
    public ProgrammInfo epgNext;
  }
  public class RecordingInfo
  {
    public string recordingID;
    public string title;
    public string genre;
    public string description;
    public string timeInfo;
  }
  public class ScheduleInfo
  {
    public string scheduleID;
    public DateTime startTime;
    public DateTime endTime;
    public string channelName;
    public string description;
    public string type;
  }
  public class ClientSettings
  {
    public static string playerPath = "";
    public static string playerArgs = "{0}";
    public static string serverHostname = "";
    public static bool useOverride = false;
    public static string overrideURL = "";
    public static int frmLeft;
    public static int frmTop;
    public static int frmWidth;
    public static int frmHeight;

    public static bool IsValid()
    {
      return (System.IO.File.Exists(playerPath)) && (playerArgs != "") && (serverHostname != "");
    }
    public static void Load()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Team MediaPortal\\MPTvClient");
      if (key == null)
        key = Registry.CurrentUser.CreateSubKey("Software\\Team MediaPortal\\MPTvClient");
      playerPath = (string)key.GetValue("PlayerPath", "");
      playerArgs = (string)key.GetValue("PlayerParams", "{0}");
      serverHostname = (string)key.GetValue("ServerHostname", "");
      useOverride = ((string)key.GetValue("UseOverride", "0") == "1");
      overrideURL = (string)key.GetValue("OverrideURL", "");
      frmLeft = (int)key.GetValue("Left", 0);
      frmTop = (int)key.GetValue("Top", 0);
      frmWidth = (int)key.GetValue("Width", 0);
      frmHeight = (int)key.GetValue("Height", 0);
      key.Close();
    }
    public static void Save()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Team MediaPortal\\MPTvClient", true);
      if (key == null)
        key = Registry.CurrentUser.CreateSubKey("Software\\Team MediaPortal\\MPTvClient");
      key.SetValue("PlayerPath", playerPath);
      key.SetValue("PlayerParams", playerArgs);
      key.SetValue("ServerHostname", serverHostname);
      if (useOverride)
        key.SetValue("UseOverride", "1");
      else
        key.SetValue("UseOverride", "0");
      key.SetValue("OverrideURL", overrideURL);
      key.SetValue("Left", frmLeft);
      key.SetValue("Top", frmTop);
      key.SetValue("Width", frmWidth);
      key.SetValue("Height", frmHeight);
      key.Close();
    }
  }
}
