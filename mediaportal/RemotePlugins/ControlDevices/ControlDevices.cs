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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.ControlDevices;
using System.Collections;
using MediaPortal.InputDevices;
using MediaPortal.Util;
using System.Reflection;
using MediaPortal.Configuration;

namespace MediaPortal.ControlDevices
{
  public static class ControlDevices
  {
    #region vars
    private static System.Collections.Generic.List<IControlPlugin> _plugins;
    private static System.Collections.Generic.List<IControlInput> _input;
    private static System.Collections.Generic.List<IControlOutput> _output;

    private static bool _initialized;
    #endregion

    #region Methods

    /// <summary>
    /// Load the uninitialized controlplugins, load the settings and get the
    /// enabled plugins with there input/output interfaces.
    /// </summary>
    static ControlDevices()
    {
      _initialized = false;
      BuildControlStructure();
    }

    /// <summary>
    /// Initialize the linked lists and load the settings. This
    /// don't initialize the plugins, so no hardware resources
    /// are locked.
    /// </summary>
    private static void BuildControlStructure()
    {
      // Get all plugins
      System.Collections.Generic.List<IControlPlugin> pluginInstances = PluginInstances();

      // Initialize the arrays
      _plugins = new System.Collections.Generic.List<IControlPlugin>();
      _input = new System.Collections.Generic.List<IControlInput>();
      _output = new System.Collections.Generic.List<IControlOutput>();

      // Filter the ones that are enabled
      System.Collections.Generic.IEnumerator<IControlPlugin> pluginIterator = pluginInstances.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
        IControlSettings settings = plugin.Settings;
        if (null == settings)
        {
          Log.Error("ControlDevices: Error getting IControlSettings of {0} in {1}", ((Type)plugin).FullName, plugin.LibraryName);
          continue;
        }
        // Load the settings
        settings.Load();

        if (settings.Enabled)
        {
          if (settings.EnableInput)
          {
            IControlInput input = plugin.InputInterface;
            if (null == input)
            {
              Log.Error("ControlDevices: Error getting IControlInput Interface of {0} in {1}", ((Type)plugin).FullName, plugin.LibraryName);
              continue;
            }
            _input.Add(input);
          }
          if (settings.EnableOutput)
          {
            IControlOutput output = plugin.OutputInterface;
            if (null == output)
            {
              Log.Error("ControlDevices: Error getting IControlOutput Interface of {0} in {1}", ((Type)plugin).FullName, plugin.LibraryName);
              continue;
            }
            _output.Add(output);
          }
        }
      }
    }

    /// <summary>
    /// Initialize all the enabled controlplugins.
    /// </summary>
    public static void Initialize()
    {
      if (_initialized)
      {
        Log.Error("ControlDevices: Init was called before more then once - restarting devices now");
        DeInitialize();
      }

      System.Collections.Generic.IEnumerator<IControlPlugin> pluginIterator = _plugins.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
      }

      _initialized = true;
    }

    /// <summary>
    /// Stop the enabled controlplugins and free all the resources.
    /// </summary>
    public static void DeInitialize()
    {
      DeInitialize(false);
    }

    /// <summary>
    /// Stop the enabled controlplugins and free all the resources.
    /// </summary>
    /// <param name="silent">supress error messages</param>
    private static void DeInitialize(bool silent)
    {
      if (!_initialized)
      {
        if (!silent)
        {
          Log.Error("ControlDevices: Stop was called without Initialize - exiting");
        }
        return;
      }

      System.Collections.Generic.IEnumerator<IControlPlugin> pluginIterator = _plugins.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
      }

      _initialized = false;
    }

    public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      System.Collections.Generic.IEnumerator<IControlInput> inputIterator = _input.GetEnumerator();
      
      action = null;
      key = (char)0;
      keyCode = Keys.Escape;

      while (inputIterator.MoveNext())
      {
        // Get the settings interface
        IControlInput input = inputIterator.Current;
        if (input.UseWndProc)
        {
          if (input.WndProc(ref msg, out action, out  key, out keyCode))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Get an list of uninitialized controlplugins instances.
    /// </summary>
    /// <returns>list of uninitialized controlplugins instances</returns>
    public static System.Collections.Generic.List<IControlPlugin> PluginInstances()
    {
      ArrayList cioPlugins = new ArrayList();
      cioPlugins.Add(Config.GetFile(Config.Dir.Base, "RemotePlugins.dll"));

      System.Collections.Generic.List<IControlPlugin> pluginInstances = new System.Collections.Generic.List<IControlPlugin>();

      foreach (string plugin in cioPlugins)
      {
        string pluginFileName = plugin.Substring(plugin.LastIndexOf(@"\") + 1);
        Assembly assembly = null;
        try
        {
          assembly = Assembly.LoadFrom(plugin);
          if (null != assembly)
          {
            Type[] types = assembly.GetExportedTypes();

            // Enumerate each type and see if it's a plugin. One assemly can
            // have multiple controlplugins.
            foreach (Type type in types)
            {
              try
              {
                // an abstract class cannot be instanciated
                if (type.IsAbstract)
                  continue;

                // Try to locate the interface we're interested in
                if (null != type.GetInterface("MediaPortal.ControlDevices.IControlPlugin"))
                {
                  // Create instance of the current type
                  object instance = null;
                  instance = Activator.CreateInstance(type);
                  if (null == instance)
                  {
                    Log.Error("ControlDevices: Error creating instance of {0} in control plugin {1}", type.FullName, pluginFileName);
                    continue;
                  }
                  IControlPlugin controlPluginInterface = instance as IControlPlugin;
                  if (null == controlPluginInterface)
                  {
                    Log.Error("ControlDevices: Error getting IControlPlugin of {0} in {1}", type.FullName, pluginFileName);
                    continue;
                  }

                  Log.Debug("ControlDevices: Found controlplugin {0} in {1}", type.FullName, pluginFileName);
                  pluginInstances.Add(controlPluginInterface);
                }
              }
              catch (Exception ex) 
              {
                string message = String.Format("Plugin {0} is {1} incompatible with the current MediaPortal version!",type.FullName, pluginFileName);
                MessageBox.Show(string.Format("An error occured while loading a plugin.\n\n{0}", message, "Control Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error));
                Log.Error("Remote: {0}", message);
                Log.Error(ex);
                continue;
              }
            }
          }

        }
        catch (Exception ex)
        {
          string message = String.Format("Plugin file {0} broken or incompatible with the current MediaPortal version!", pluginFileName);
          MessageBox.Show(string.Format("An error occured while loading a plugin.\n\n{0}", message, "Control Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error));
          Log.Error("Remote: {0}", message);
          Log.Error(ex);
        }

        //;
      }
      return pluginInstances;
    }


    #endregion Methods
  }
}
