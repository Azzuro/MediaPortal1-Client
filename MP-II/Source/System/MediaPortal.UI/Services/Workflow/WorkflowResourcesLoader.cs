#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Presentation.Localization;
using MediaPortal.Presentation.SkinResources;
using MediaPortal.Presentation.Workflow;
using MediaPortal.Utilities;

namespace MediaPortal.Services.Workflow
{
  /// <summary>
  /// Class for loading MediaPortal-II workflow resources from the current skin context.
  /// </summary>
  public class WorkflowResourcesLoader
  {
    public const int WORKFLOW_RESOURCE_SPEC_VERSION_HIGH = 1;
    public const int MIN_WORKFLOW_RESOURCE_SPEC_VERSION_LOW = 0;

    protected IDictionary<Guid, WorkflowState> _states = new Dictionary<Guid, WorkflowState>();
    protected IDictionary<Guid, ICollection<Guid>> _additionalModels =
        new Dictionary<Guid, ICollection<Guid>>();
    protected IDictionary<Guid, WorkflowStateAction> _menuActions =
        new Dictionary<Guid, WorkflowStateAction>();
    protected IDictionary<Guid, WorkflowStateAction> _contextMenuActions =
        new Dictionary<Guid, WorkflowStateAction>();

    public IDictionary<Guid, WorkflowState> States
    {
      get { return _states; }
    }

    public IDictionary<Guid, WorkflowStateAction> MenuActions
    {
      get { return _menuActions; }
    }

    public IDictionary<Guid, WorkflowStateAction> ContextMenuActions
    {
      get { return _contextMenuActions; }
    }

    /// <summary>
    /// Loads workflow resources from files contained in the current skin context.
    /// </summary>
    /// be added.</param>
    public void Load()
    {
      _states.Clear();
      _additionalModels.Clear();
      _menuActions.Clear();
      _contextMenuActions.Clear();
      IDictionary<string, string> workflowResources = ServiceScope.Get<ISkinResourceManager>().
          SkinResourceContext.GetResourceFilePaths("workflow\\\\.*\\.xml");
      foreach (string workflowResourceFilePath in workflowResources.Values)
        LoadWorkflowResourceFile(workflowResourceFilePath);

      foreach (Guid stateId in new List<Guid>(_additionalModels.Keys))
        if (_states.ContainsKey(stateId))
        {
          CollectionUtils.AddAll(_states[stateId].AdditionalModels, _additionalModels[stateId]);
          // Not necessary to remove the entry here but this reflects the fact that the additional models
          // for this state have been moved to the state instance
          _additionalModels.Remove(stateId);
        }
        else
          ServiceScope.Get<ILogger>().Info(
              "There are additional model attachments for workflow state '{0}', but the state is not known. The current skin might cover more workflow states than available.", stateId);
    }

    protected void LoadWorkflowResourceFile(string filePath)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlElement descriptorElement = doc.DocumentElement;
        if (descriptorElement == null || descriptorElement.Name != "Workflow")
          throw new ArgumentException(
              "File is no workflow descriptor file (document element must be 'Workflow')");

        bool versionOk = false;
        foreach (XmlAttribute attr in descriptorElement.Attributes)
        {
          switch (attr.Name)
          {
            case "DescriptorVersion":
              StringUtils.CheckVersionEG(attr.Value, WORKFLOW_RESOURCE_SPEC_VERSION_HIGH, MIN_WORKFLOW_RESOURCE_SPEC_VERSION_LOW);
              //string specVersion = attr.Value; <- if needed
              versionOk = true;
              break;
            default:
              throw new ArgumentException("'Workflow' element doesn't support an attribute '" + attr.Name + "'");
          }
        }
        if (!versionOk)
          throw new ArgumentException("'DescriptorVersion' attribute expected");

        foreach (XmlNode child in descriptorElement.ChildNodes)
        {
          XmlElement childElement = child as XmlElement;
          if (childElement == null)
            continue;
          switch (childElement.Name)
          {
            case "States":
              LoadStates(childElement);
              break;
            case "MenuActions":
              foreach (WorkflowStateAction action in LoadActions(childElement))
              {
                if (_menuActions.ContainsKey(action.ActionId))
                  throw new ArgumentException(string.Format(
                      "A menu action with id '{0}' was already registered with action name '{1}' (name of duplicate action is '{2}') -> Forgot to create a new GUID?",
                      action.ActionId, _menuActions[action.ActionId].Name, action.Name));
                _menuActions.Add(action.ActionId, action);
              }
              break;
            case "ContextMenuActions":
              foreach (WorkflowStateAction action in LoadActions(childElement))
              {
                if (_contextMenuActions.ContainsKey(action.ActionId))
                  throw new ArgumentException(string.Format(
                      "A context menu action with id '{0}' was already registered with action name '{1}' (name of duplicate action is '{2}') -> Forgot to create a new GUID?",
                      action.ActionId, _contextMenuActions[action.ActionId].Name, action.Name));
                _contextMenuActions.Add(action.ActionId, action);
              }
              break;
            default:
              throw new ArgumentException("'Workflow' element doesn't support a child element '" + child.Name + "'");
          }
        }
      }
      catch (Exception e)
      {
        ServiceScope.Get<ILogger>().Error("Error loading workflow resource file '" + filePath + "'", e);
      }
    }

    protected void LoadStates(XmlElement statesElement)
    {
      foreach (XmlNode child in statesElement.ChildNodes)
      {
        XmlElement childElement = child as XmlElement;
        if (childElement == null)
          continue;
        switch (childElement.Name)
        {
          case "State":
            WorkflowState state = LoadState(childElement);
            if (_states.ContainsKey(state.StateId))
              throw new ArgumentException(string.Format(
                  "A workflow state with id '{0}' was already declared with name '{1}' (name of duplicate state is '{2}') -> Forgot to create a new GUID?",
                  state.StateId, _states[state.StateId].Name, state.Name));
            _states.Add(state.StateId, state);
            break;
          case "AdditionalModels":
            Guid stateId;
            ICollection<Guid> additionalModels = new List<Guid>();
            LoadAdditionalModels(childElement, out stateId, additionalModels);
            if (_additionalModels.ContainsKey(stateId))
              CollectionUtils.AddAll(_additionalModels[stateId], additionalModels);
            else
              _additionalModels[stateId] = additionalModels;
            break;
          default:
            throw new ArgumentException("'" + statesElement.Name + "' element doesn't support a child element '" + child.Name + "'");
        }
      }
    }

    protected static IEnumerable<WorkflowStateAction> LoadActions(XmlElement actionsElement)
    {
      foreach (XmlNode child in actionsElement.ChildNodes)
      {
        XmlElement childElement = child as XmlElement;
        if (childElement == null)
          continue;
        switch (childElement.Name)
        {
          case "PushNavigationTransition":
            yield return LoadPushNavigationTransition(childElement);
            break;
          case "PopNavigationTransition":
            yield return LoadPopNavigationTransition(childElement);
            break;
          // TODO: More actions - show screen, close dialog, call model method, ...
          default:
            throw new ArgumentException("'" + actionsElement.Name + "' element doesn't support a child element '" + child.Name + "'");
        }
      }
    }

    protected static WorkflowState LoadState(XmlElement stateElement)
    {
      string id = null;
      string name = null;
      string mainScreen = null;
      bool inheritMenu = false;
      bool inheritContextMenu = false;
      string workflowModelId = null;
      foreach (XmlAttribute attr in stateElement.Attributes)
      {
        switch (attr.Name)
        {
          case "Id":
            id = attr.Value;
            break;
          case "Name":
            name = attr.Value;
            break;
          case "MainScreen":
            mainScreen = attr.Value;
            break;
          case "InheritMenu":
            if (!bool.TryParse(attr.Value, out inheritMenu))
              throw new ArgumentException("'InheritMenu' attribute has to be of type bool");
            break;
          case "InheritContextMenu":
            if (!bool.TryParse(attr.Value, out inheritContextMenu))
              throw new ArgumentException("'InheritContextMenu' attribute has to be of type bool");
            break;
          case "WorkflowModel":
            workflowModelId = attr.Value;
            break;
          default:
            throw new ArgumentException("'" + stateElement.Name + "' element doesn't support an attribute '" + attr.Name + "'");
        }
      }
      if (string.IsNullOrEmpty(id))
        throw new ArgumentException(string.Format("{0} '{1}': State must be specified", stateElement.Name, name));
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(string.Format("{0} with id '{1}': 'Name' attribute missing", stateElement.Name, id));
      if (string.IsNullOrEmpty(mainScreen))
        throw new ArgumentException(string.Format("{0} '{1}': 'MainScreen' attribute missing", stateElement.Name, name));
      return new WorkflowState(new Guid(id), name, mainScreen, inheritMenu, inheritContextMenu, false,
          string.IsNullOrEmpty(workflowModelId) ? null : new Guid?(new Guid(workflowModelId)), new List<Guid>());
    }

    protected static void LoadAdditionalModels(XmlElement additionalModelsElement,
        out Guid stateId, ICollection<Guid> additionalModels)
    {
      string stateIdStr = null;
      foreach (XmlAttribute attr in additionalModelsElement.Attributes)
      {
        switch (attr.Name)
        {
          case "StateId":
            stateIdStr = attr.Value;
            break;
          default:
            throw new ArgumentException("'" + additionalModelsElement.Name + "' element doesn't support an attribute '" + attr.Name + "'");
        }
      }
      if (string.IsNullOrEmpty(stateIdStr))
        throw new ArgumentException(string.Format("'" + additionalModelsElement.Name + "' element: StateId attribute is missing"));
      stateId = new Guid(stateIdStr);
      foreach (XmlNode child in additionalModelsElement.ChildNodes)
      {
        XmlElement childElement = child as XmlElement;
        if (childElement == null)
          continue;
        switch (childElement.Name)
        {
          case "Model":
            string modelIdStr = null;
            foreach (XmlAttribute attr in childElement.Attributes)
            {
              switch (attr.Name)
              {
                case "Id":
                  modelIdStr = attr.Value;
                  break;
                default:
                  throw new ArgumentException("'" + childElement.Name + "' element doesn't support an attribute '" + attr.Name + "'");
              }
            }
            if (string.IsNullOrEmpty(modelIdStr))
              throw new ArgumentException(string.Format("'" + childElement.Name + "' element: Id attribute is missing"));
            additionalModels.Add(new Guid(modelIdStr));
            break;
          default:
            throw new ArgumentException("'" + additionalModelsElement.Name + "' element doesn't support a child element '" + child.Name + "'");
        }
      }
    }

    protected static WorkflowStateAction LoadPushNavigationTransition(XmlElement actionElement)
    {
      string id = null;
      string name = null;
      string sourceState = null;
      string targetState = null;
      string displayTitle = null;
      foreach (XmlAttribute attr in actionElement.Attributes)
      {
        switch (attr.Name)
        {
          case "Id":
            id = attr.Value;
            break;
          case "Name":
            name = attr.Value;
            break;
          case "SourceState":
            sourceState = attr.Value;
            break;
          case "TargetState":
            targetState = attr.Value;
            break;
          case "DisplayTitle":
            displayTitle = attr.Value;
            break;
          default:
            throw new ArgumentException("'" + actionElement.Name + "' element doesn't support an attribute '" + attr.Name + "'");
        }
      }
      if (string.IsNullOrEmpty(id))
        throw new ArgumentException(string.Format("{0} '{1}': Id attribute is missing", actionElement.Name, name));
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(string.Format("{0} with id '{1}': 'Name' attribute missing", actionElement.Name, id));
      if (string.IsNullOrEmpty(sourceState))
        throw new ArgumentException(string.Format("{0} '{1}': 'SourceState' attribute missing", actionElement.Name, name));
      if (string.IsNullOrEmpty(targetState))
        throw new ArgumentException(string.Format("{0} '{1}': 'TargetState' attribute missing", actionElement.Name, name));
      return new PushNavigationTransition(new Guid(id), name, new Guid(sourceState), new Guid(targetState),
          LocalizationHelper.CreateResourceString(displayTitle));
    }

    protected static WorkflowStateAction LoadPopNavigationTransition(XmlElement actionElement)
    {
      string id = null;
      string name = null;
      string sourceState = null;
      int numPop = -1;
      string displayTitle = null;
      foreach (XmlAttribute attr in actionElement.Attributes)
      {
        switch (attr.Name)
        {
          case "Id":
            id = attr.Value;
            break;
          case "Name":
            name = attr.Value;
            break;
          case "SourceState":
            sourceState = attr.Value;
            break;
          case "NumPop":
            if (!Int32.TryParse(attr.Value, out numPop))
              throw new ArgumentException("'NumPop' attribute value must be a positive integer");
            break;
          case "DisplayTitle":
            displayTitle = attr.Value;
            break;
          default:
            throw new ArgumentException("'" + actionElement.Name + "' element doesn't support an attribute '" + attr.Name + "'");
        }
      }
      if (string.IsNullOrEmpty(id))
        throw new ArgumentException(string.Format("{0} '{1}': Id attribute is missing", actionElement.Name, name));
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(string.Format("{0} with id '{1}': 'Name' attribute missing", actionElement.Name, id));
      if (string.IsNullOrEmpty(sourceState))
        throw new ArgumentException(string.Format("{0} '{1}': 'SourceState' attribute missing", actionElement.Name, name));
      if (numPop == -1)
        throw new ArgumentException(string.Format("{0} '{1}': 'NumPop' attribute missing", actionElement.Name, name));
      return new PopNavigationTransition(new Guid(id), name, new Guid(sourceState), numPop,
          LocalizationHelper.CreateResourceString(displayTitle));
    }
  }
}
