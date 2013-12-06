#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DShowNET.Helper;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.Visualization
{
  /// <summary>
  /// Provides information about a WMP Visualition.  
  /// Note that the rendering should not be performed via this class as 
  /// we have some COM interop issues.  Rendering will be performed natively
  /// via the MediaPortal's mpviz.dll
  /// </summary>
  public class WMPVisualizationPlugin : IWMPEffects, IDisposable
  {
    private IWMPEffects iWmpEffects = null;
    private IWMPEffects2 iWmpEffects2 = null;

    internal WMPVisualizationPlugin(string sClsid)
    {
      Guid g;
      Object oCom = null;
      bool isValidVizObject = false;

      try
      {
        g = new Guid(sClsid);

        Type comObjType = Type.GetTypeFromCLSID(g);
        oCom = Activator.CreateInstance(comObjType);

        // Cast of IWMPEffects
        iWmpEffects = oCom as IWMPEffects;

        if (iWmpEffects != null)
        {
          // supports the interface IWMPEffects > IWmpEffects2 ?
          iWmpEffects2 = oCom as IWMPEffects2;

          if (iWmpEffects2 != null)
          {              
            // the interface IWmpEffects2 is supported, need
            // we the interface IWmpEffects no longer.  
            iWmpEffects = null;
            isValidVizObject = true;
          }
        }
      }

      catch (FormatException)
      {
        isValidVizObject = false;
      }

      catch (COMException)
      {
        isValidVizObject = false;
      }

      catch (NullReferenceException)
      {
        isValidVizObject = false;
      }

      if (!isValidVizObject || oCom == null)
      {
        if (oCom != null)
        {
          DirectShowUtil.ReleaseComObject(oCom);
        }
        iWmpEffects = null;
        iWmpEffects2 = null;
        throw new Exception("Object is not a IWMPEffects or IWMPEffects2 interface!");
      }
    }

    ~WMPVisualizationPlugin()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (iWmpEffects != null)
      {
        DirectShowUtil.ReleaseComObject(iWmpEffects);
        iWmpEffects = null;
      }

      if (iWmpEffects2 != null)
      {
        DirectShowUtil.ReleaseComObject(iWmpEffects2);
        iWmpEffects2 = null;
      }
    }

    #region IWMPEffects Members

    public int Render(IntPtr timedLevels, IntPtr hdc, IntPtr rect)
    {
      throw new Exception("This method should not be called. Use the Render method in the mpviz.dll instead!");
    }

    public int MediaInfo(int channelCount, int sampleRate, string title)
    {
      if (iWmpEffects2 != null)
      {
        return iWmpEffects2.MediaInfo(channelCount, sampleRate, title);
      }
      else
      {
        if (iWmpEffects != null)
        {
          return iWmpEffects.MediaInfo(channelCount, sampleRate, title);
        }
        else
        {
          return -1;
        }
      }
    }

    public int GetCapabilities(ref int caps)
    {
      int val = 0;

      if (iWmpEffects2 != null)
      {
        int result = iWmpEffects2.GetCapabilities(ref val);
        caps = val;
        return result;
      }
      else
      {
        if (iWmpEffects != null)
        {
          int result = iWmpEffects.GetCapabilities(ref val);
          caps = val;
          return result;
        }
        else
        {
          return -1;
        }
      }           
    }

    public int GetTitle(out string title)
    {
      title = string.Empty;

      if (iWmpEffects2 != null)
      {
        string val = string.Empty;
        int result = iWmpEffects2.GetTitle(out val);
        title = val;
        return result;
      }
      else
      {
        if (iWmpEffects != null)
        {
          string val = string.Empty;
          int result = iWmpEffects.GetTitle(out val);
          title = val;
          return result;
        }
        else
        {
          return -1;
        }
      }
           
    }

    public int GetPresetTitle(int preset, out string title)
    {
      title = string.Empty;

      if (iWmpEffects2 != null)
      {
        string val = string.Empty;
        int result = iWmpEffects2.GetPresetTitle(preset, out val);
        title = val;
        return result;
      }
      else
      {
        if (iWmpEffects != null)
        {
          string val = string.Empty;
          int result = iWmpEffects.GetPresetTitle(preset, out val);
          title = val;
          return result;
        }
        else
        {
          return -1;
        }
      }      
    }

    public int GetPresetCount(ref int count)
    {
      int val = 0;
      if (iWmpEffects2 != null)
      {
        int result = iWmpEffects2.GetPresetCount(ref val);
        count = val;
        return result;
      }
      else
      {
        if (iWmpEffects != null)
        {
          int result = iWmpEffects.GetPresetCount(ref val);
          count = val;
          return result;
        }
        else
        {
          return -1;
        }
      }
    }

    public int SetCurrentPreset(int preset)
    {
      if (iWmpEffects2 != null)
      {
        return iWmpEffects2.SetCurrentPreset(preset);
      }
      else
      {
        if (iWmpEffects != null)
        {
          return iWmpEffects.SetCurrentPreset(preset);
        }
        else
        {
          return -1;
        }
      }
    }

    public int GetCurrentPreset(ref int preset)
    {
      int val = 0;

      if (iWmpEffects2 != null)
      {
        int result = iWmpEffects2.GetCurrentPreset(ref val);
        preset = val;
        return result;
      }
      else
      {
        if (iWmpEffects != null)
        {
          int result = iWmpEffects.GetCurrentPreset(ref val);
          preset = val;
          return result;
        }
        else
        {
          return -1;
        }
      }
    }

    public int DisplayPropertyPage(IntPtr hwndOwner)
    {

      if (iWmpEffects2 != null)
      {
        int val = 0;
        int caps = 0;

        iWmpEffects2.GetCapabilities(ref val);
        caps = val;

        if ((val & 2) > 0)
        {
          return iWmpEffects2.DisplayPropertyPage(hwndOwner);
        }
      }
      else
      {
        if (iWmpEffects != null)
        {
          int val = 0;
          int caps = 0;

          iWmpEffects.GetCapabilities(ref val);
          caps = val;

          if ((val & 2) > 0)
          {
            return iWmpEffects.DisplayPropertyPage(hwndOwner);
          }
        }
      }
      return -1;
    }

    public int GoFullScreen(bool startFullScreen)
    {
      // Purposely not implemented!
      return -1;
    }

    public int RenderFullScreen(IntPtr timedLevels)
    {
      // Purposely not implemented!
      return -1;
    }

    #endregion
  }

  /// <summary>
  /// Wraps the IWMPEffects object to simplify accessing values
  /// </summary>
  public class WMPVisualizationInfo : IDisposable
  {
    private WMPVisualizationPlugin WMPPlugin = null;

    private string _Title = string.Empty;
    private int _PresetCount = 0;
    private int _Capabilities = 0;
    private List<string> _Presets = new List<string>();

    public string Title
    {
      get { return _Title; }
    }

    public int PresetCount
    {
      get { return _PresetCount; }
    }

    public List<string> Presets
    {
      get { return _Presets; }
    }

    public bool CanGoFullScreen
    {
      get { return (_Capabilities & (int)EffectsCapability.EFFECT_CANGOFULLSCREEN) > 0; }
    }

    public bool HasPropertyPage
    {
      get { return (_Capabilities & (int)EffectsCapability.EFFECT_HASPROPERTYPAGE) > 0; }
    }

    public bool WindowedOnly
    {
      get { return (_Capabilities & (int)EffectsCapability.EFFECT_WINDOWEDONLY) > 0; }
    }

    public bool FullScreenExclusiveMode
    {
      get { return (_Capabilities & (int)EffectsCapability.EFFECT2_FULLSCREENEXCLUSIVE) > 0; }
    }

    public WMPVisualizationInfo(string sClsid)
    {
      WMPPlugin = new WMPVisualizationPlugin(sClsid);

      if (WMPPlugin == null)
      {
        return;
      }

      _Title = GetTitle();
      _PresetCount = GetPresetCount();
      _Capabilities = GetCapabilities();

      for (int i = 0; i < _PresetCount; i++)
      {
        string presetTitle = GetPresetTitle(i);

        if (presetTitle.Length == 0)
        {
          continue;
        }

        _Presets.Add(presetTitle);
      }
    }

    ~WMPVisualizationInfo()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (WMPPlugin != null)
      {
        WMPPlugin.SafeDispose();
        WMPPlugin = null;
      }
    }


    private string GetTitle()
    {
      if (WMPPlugin == null)
      {
        return string.Empty;
      }

      string val = string.Empty;
      WMPPlugin.GetTitle(out val);
      return val;
    }

    private int GetPresetCount()
    {
      if (WMPPlugin == null)
      {
        return 0;
      }

      int val = 0;
      WMPPlugin.GetPresetCount(ref val);
      return val;
    }

    private string GetPresetTitle(int presetIndex)
    {
      if (WMPPlugin == null)
      {
        return string.Empty;
      }

      string val = string.Empty;
      WMPPlugin.GetPresetTitle(presetIndex, out val);
      return val;
    }

    private int GetCapabilities()
    {
      if (WMPPlugin == null)
      {
        return 0;
      }

      int val = 0;
      WMPPlugin.GetCapabilities(ref val);
      return val;
    }
  }
}