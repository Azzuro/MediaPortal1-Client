﻿#region Copyright (C) 2007-2008 Team MediaPortal

/*
 *  Copyright (C) 2007-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This file is part of MediaPortal II
 *
 *  MediaPortal II is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MediaPortal II is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Interfaces.Core.PluginManager;
using MediaPortal.Services.PluginManager.Builders;
using MediaPortal.Services.PluginManager.PluginSpace;

namespace MediaPortal.Services.PluginManager.PluginDetails
{
  internal class PluginRuntime
  {
    #region Variables
    string _hintPath;
    string _assembly;
    Assembly _loadedAssembly;
    bool _isActive;
    bool _isAssemblyLoaded;
    IList<PluginDefinedBuilder> _pluginDefinedBuilders;
    #endregion

    #region Constructors/Destructors
    public PluginRuntime(string assembly, string hintPath)
    {
      this._assembly = assembly;
      this._hintPath = hintPath;
      _loadedAssembly = null;
      _pluginDefinedBuilders = new List<PluginDefinedBuilder>();
      _isActive = true;
      _isAssemblyLoaded = false;
    }
    #endregion

    #region Properties
    public bool IsActive
    {
      get
      {
        return _isActive;
      }
    }

    public string Assembly
    {
      get { return _assembly; }
    }

    public Assembly LoadedAssembly
    {
      get
      {
        if (!_isAssemblyLoaded)
        {
          LoadAssembly();
        }
        return _loadedAssembly;
      }
    }

    public IList<PluginDefinedBuilder> PluginDefinedBuilders
    {
      get { return _pluginDefinedBuilders; }
    }

    #endregion

    #region Public Methods
    public object CreateInstance(string instance)
    {
      if (IsActive)
      {
        Assembly asm = LoadedAssembly;
        if (asm == null)
          return null;
        return asm.CreateInstance(instance);
      }
      else
      {
        return null;
      }
    }
    #endregion

    #region internal static Methods
    internal static void ReadSection(XmlReader reader, PluginInfo plugin, string hintPath)
    {
      //Stack<ICondition> conditionStack = new Stack<ICondition>();
      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.EndElement:
            //if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition") {
            //  conditionStack.Pop();
            //} else 
            if (reader.LocalName == "Runtime")
            {
              return;
            }
            break;
          case XmlNodeType.Element:
            switch (reader.LocalName)
            {
              //case "Condition":
              //  conditionStack.Push(Condition.Read(reader));
              //  break;
              //case "ComplexCondition":
              //  conditionStack.Push(Condition.ReadComplexCondition(reader));
              //  break;
              case "Import":
                plugin.Runtimes.Add(PluginRuntime.Read(plugin, reader, hintPath)); //, conditionStack));
                break;
              //case "DisablePlugin":
              //  if (Condition.GetFailedAction(conditionStack, plugin) == ConditionFailedAction.Nothing) {
              //    // The DisableAddIn node not was not disabled by a condition
              //    plugin.CustomErrorMessage = reader.GetAttribute("message");
              //  }
              //  break;
              default:
                throw new PluginLoadException("Unknown node in runtime section :" + reader.LocalName);
            }
            break;
        }
      }
    }

    internal static PluginRuntime Read(PluginInfo plugin, XmlReader reader, string hintPath) //, Stack<ICondition> conditionStack)
    {
      if (reader.AttributeCount != 1)
      {
        throw new PluginLoadException("Import node requires ONE attribute.");
      }
      PluginRuntime runtime = new PluginRuntime(reader.GetAttribute(0), hintPath);
      //if (conditionStack.Count > 0) {
      //  runtime.conditions = conditionStack.ToArray();
      //}
      if (!reader.IsEmptyElement)
      {
        while (reader.Read())
        {
          switch (reader.NodeType)
          {
            case XmlNodeType.EndElement:
              if (reader.LocalName == "Import")
              {
                return runtime;
              }
              break;
            case XmlNodeType.Element:
              string nodeName = reader.LocalName;
              PluginProperties properties = PluginProperties.ReadFromAttributes(reader);
              switch (nodeName)
              {
                case "Builder":
                  if (!reader.IsEmptyElement)
                  {
                    throw new PluginLoadException("Builder nodes must be empty!");
                  }
                  runtime._pluginDefinedBuilders.Add(new PluginDefinedBuilder(plugin, properties));
                  break;
                //case "ConditionEvaluator":
                //  if (!reader.IsEmptyElement) {
                //    throw new AddInLoadException("ConditionEvaluator nodes must be empty!");
                //  }
                //  runtime.definedConditionEvaluators.Add(new LazyConditionEvaluator(addIn, properties));
                //  break;
                default:
                  throw new PluginLoadException("Unknown node in Import section:" + nodeName);
              }
              break;
          }
        }
      }
      //runtime.definedDoozers             = (runtime.definedDoozers as List<LazyLoadDoozer>).AsReadOnly();
      //runtime.definedConditionEvaluators = (runtime.definedConditionEvaluators as List<LazyConditionEvaluator>).AsReadOnly();
      return runtime;
    }
    #endregion

    #region Private Methods
    private void LoadAssembly()
    {
      if (!_isAssemblyLoaded)
      {
        ServiceScope.Get<ILogger>().Debug("Loading Plugin: " + _assembly);

        _isAssemblyLoaded = true;

        try
        {
          if (_assembly[0] == ':')
          {
            _loadedAssembly = System.Reflection.Assembly.Load(_assembly.Substring(1));
          }
          else if (_assembly[0] == '$')
          {
            int pos = _assembly.IndexOf('/');
            if (pos < 0)
              throw new ApplicationException("Expected '/' in path beginning with '$'!");
            string referencedPlugin = _assembly.Substring(1, pos - 1);
            foreach (PluginInfo plugin in ServiceScope.Get<IPluginTree>().Plugins)
            {
              if (plugin.State==PluginState.Enabled && plugin.Manifest.Identities.ContainsKey(referencedPlugin))
              {
                string assemblyFile = Path.Combine(Path.GetDirectoryName(plugin.FileName),
                                                   _assembly.Substring(pos + 1));
                _loadedAssembly = System.Reflection.Assembly.LoadFrom(assemblyFile);
                break;
              }
            }
            if (_loadedAssembly == null)
            {
              throw new FileNotFoundException("Could not find referenced Plugin" + referencedPlugin);
            }
          }
          else
          {
            _loadedAssembly = System.Reflection.Assembly.LoadFrom(Path.Combine(_hintPath, _assembly));
          }

          ServiceScope.Get<ILogger>().Debug("Assembly Version: " + _loadedAssembly.GetName().Version.ToString());
#if DEBUG
          // preload assembly to provoke FileLoadException if dependencies are missing
          _loadedAssembly.GetExportedTypes();
#endif
        }
        catch (FileNotFoundException ex)
        {
          ServiceScope.Get<ILogger>().Error("The Plugin'" + _assembly + "' could not be loaded:\n" + ex.ToString());
        }
        catch (FileLoadException ex)
        {
          ServiceScope.Get<ILogger>().Error("The Plugin '" + _assembly + "' could not be loaded:\n" + ex.ToString());
        }
      }
    }
    #endregion
  }
}
