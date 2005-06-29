using System;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.TV.Recording;

namespace MediaPortal
{
  /// <summary>
  /// Remotecontrol-mapping class
  /// Expects an XML file with mappings on construction
  /// Maps button code numbers to conditions and actions
  /// </summary>
  public class HCWHandler
  {
    ArrayList remote;
    int currentLayer = 1;

    /// <summary>
    /// Get current Layer (Multi-Layer support)
    /// </summary>
    public int CurrentLayer { get { return currentLayer; } }


    /// <summary>
    /// Condition/action class
    /// </summary>
    class Mapping
    {
      string condition;
      string conProperty;
      int    layer;
      string command;
      string cmdProperty;
      string sound;

      public int    Layer       { get { return layer;       } }
      public string Condition   { get { return condition;   } }
      public string ConProperty { get { return conProperty; } }
      public string Command     { get { return command;     } }
      public string CmdProperty { get { return cmdProperty; } }
      public string Sound       { get { return sound;       } }

      public Mapping(int newLayer, string newCondition, string newConProperty, string newCommand, string newCmdProperty, string newSound)
      {
        layer       = newLayer;
        condition   = newCondition;
        conProperty = newConProperty;
        command     = newCommand;
        cmdProperty = newCmdProperty;
        sound       = newSound;
      }
    }


    /// <summary>
    /// Button/mapping class
    /// </summary>
    class RemoteMap
    {
      int       code;
      string    name;
      ArrayList mapping = new ArrayList();

      public int       Code    { get { return code;    } }
      public string    Name    { get { return name;    } }
      public ArrayList Mapping { get { return mapping; } }

      public RemoteMap(int newCode, string newName, ArrayList newMapping)
      {
        code    = newCode;
        name    = newName;
        mapping = newMapping;
      }
    }


    /// <summary>
    /// Constructor: Initializes mappings from XML file
    /// </summary>
    /// <param name="xmlFile">path for XML mapping file</param>
    public HCWHandler(string xmlFile, out bool result)
    {
      string path;

      if (File.Exists("InputDeviceMappings\\custom\\" + xmlFile))
        path = "InputDeviceMappings\\custom\\";
      else
        path = "InputDeviceMappings\\defaults\\";

      if (!File.Exists(path + xmlFile))
      {
        Log.Write("MAP: XML file \"{0}\" cannot be found", xmlFile);
        result = false;
        return;
      }
      try
      {
        remote = new ArrayList();
        XmlDocument doc = new XmlDocument();
        doc.Load(path + xmlFile);
        XmlNodeList listButtons=doc.DocumentElement.SelectNodes("/mappings/remote/button");
        foreach (XmlNode nodeButton in listButtons)
        {
          string name  = nodeButton.Attributes["name"].Value;
          int    value = Convert.ToInt32(nodeButton.Attributes["code"].Value);

          ArrayList mapping = new ArrayList();
          XmlNodeList listActions = nodeButton.SelectNodes("action");
          foreach (XmlNode nodeAction in listActions)
          {
            string condition   = nodeAction.Attributes["condition"].Value.ToUpper();
            string conProperty = nodeAction.Attributes["conproperty"].Value.ToUpper();
            string command     = nodeAction.Attributes["command"].Value.ToUpper();
            string cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpper();
            string sound       = nodeAction.Attributes["sound"].Value;
            int    layer       = Convert.ToInt32(nodeAction.Attributes["layer"].Value);
            Mapping conditionMap = new Mapping(layer, condition, conProperty, command, cmdProperty, sound);
            mapping.Add(conditionMap);
          }
          RemoteMap remoteMap = new RemoteMap(value, name, mapping);
          remote.Add(remoteMap);
        }
        result = true;
      }
      catch (Exception ex)
      {
        Log.Write("MAP: error while reading XML configuration file \"{0}\": {1}", xmlFile, ex.Message);
        result = false;
      }
    }


    /// <summary>
    /// Evaluates the button number, gets its mapping and executes the action
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// <returns>true = action successfully executed</returns>
    /// <returns>false = no action executed</returns>
    public bool MapAction(int btnCode)
    {
      if (remote == null) return false; // No mapping loaded
      Mapping map = GetMapping(btnCode);
      if (map == null) return false;  // No mapping found
      Action action;
      if (map.Sound != "")
        Utils.PlaySound(map.Sound, false, true);
      switch (map.Command)
      {
        case "ACTION":  // execute Action x
          action = new Action((Action.ActionType)Convert.ToInt32(map.CmdProperty), 0, 0);
          GUIGraphicsContext.OnAction(action);
          break;
        case "KEY": // send Key x
          SendKeys.SendWait(map.CmdProperty);
          break;
        case "WINDOW":  // activate Window x
          GUIWindowManager.ActivateWindow(Convert.ToInt32(map.CmdProperty));
          break;
        case "TOGGLE":  // toggle Layer 1/2
          if (currentLayer == 1)
            currentLayer = 2;
          else
            currentLayer = 1;
          break;
        case "POWER": // power down commands
        {
          if ((map.CmdProperty == "STANDBY") || (map.CmdProperty == "HIBERNATE"))
          {
            // Stop all media before suspending or hibernating
            Recorder.StopViewing();
            Recorder.Stop();
            g_Player.Stop();
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
            GUIWindowManager.Dispose();
            GUITextureManager.Dispose();
            GUIFontManager.Dispose();
          }
          switch (map.CmdProperty)
          {
            case "EXIT":
              action = new Action(Action.ActionType.ACTION_EXIT, 0, 0);
              GUIGraphicsContext.OnAction(action);
              break;
            case "REBOOT":
              action = new Action(Action.ActionType.ACTION_REBOOT, 0, 0);
              GUIGraphicsContext.OnAction(action);
              break;
            case "SHUTDOWN":
              action = new Action(Action.ActionType.ACTION_SHUTDOWN, 0, 0);
              GUIGraphicsContext.OnAction(action);
              break;
            case "STANDBY":
              WindowsController.ExitWindows(RestartOptions.Suspend, true);
              break;
            case "HIBERNATE":
              WindowsController.ExitWindows(RestartOptions.Hibernate, true);
              break;
          }
        }
          break;
      }
      return true;
    }


    /// <summary>
    /// Get mappings for a given button code based on the current conditions
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// <returns>Mapping</returns>
    Mapping GetMapping(int btnCode)
    {
      RemoteMap button = null;
      Mapping found = null;

      foreach (RemoteMap btn in remote)
        if (btnCode == btn.Code)
        {
          button = btn;
          break;
        }
      foreach (Mapping map in button.Mapping)
        if ((map.Layer == 0) || (map.Layer == currentLayer))
        {
          switch (map.Condition)
          {
            case "*": // wildcard, no further condition
              found = map;
              break;
            case "WINDOW":  // Window-ID = x
              if (GUIWindowManager.ActiveWindow == Convert.ToInt32(map.ConProperty))
                found = map;
              break;
            case "FULLSCREEN":  // Fullscreen = true/false
              if (GUIGraphicsContext.IsFullScreenVideo == Convert.ToBoolean(map.ConProperty))
                found = map;
              break;
            case "PLAYER":  // Playing TV/DVD/general
            switch (map.ConProperty)
            {
              case "TV":
                if (g_Player.IsTV)
                  found = map;
                break;
              case "DVD":
                if (g_Player.IsDVD)
                  found = map;
                break;
              case "MEDIA":
                if (g_Player.Playing)
                  found = map;
                break;
            }
              break;
          }
          if (found != null)
          {
            return found;
          }
        }
      Log.Write("MAP: No suitable mappings found for button {0}", btnCode);
      return null;
    }
  }
}