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
using MediaPortal.Core.Localization;
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
    public const int WORKFLOW_RESOURCE_SPEC_VERSION_MAJOR = 1;
    public const int MIN_WORKFLOW_RESOURCE_SPEC_VERSION_MINOR = 0;

    public const string WORKFLOW_DIRECTORY = "workflow";

    protected IDictionary<Guid, WorkflowState> _states = new Dictionary<Guid, WorkflowState>();
    protected IDictionary<Guid, WorkflowAction> _menuActions = new Dictionary<Guid, WorkflowAction>();

    public IDictionary<Guid, WorkflowState> States
    {
      get { return _states; }
    }

    public IDictionary<Guid, WorkflowAction> MenuActions
    {
      get { return _menuActions; }
    }

    /// <summary>
    /// Loads workflow resources from files contained in the current skin context.
    /// </summary>
    /// be added.</param>
    public void Load()
    {
      _states.Clear();
      _menuActions.Clear();
      IDictionary<string, string> workflowResources = ServiceScope.Get<ISkinResourceManager>().
          SkinResourceContext.GetResourceFilePaths("^" + WORKFLOW_DIRECTORY + "\\\\.*\\.xml$");
      foreach (string workflowResourceFilePath in workflowResources.Values)
        LoadWorkflowResourceFile(workflowResourceFilePath);
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
              Versions.CheckVersionCompatible(attr.Value, WORKFLOW_RESOURCE_SPEC_VERSION_MAJOR, MIN_WORKFLOW_RESOURCE_SPEC_VERSION_MINOR);
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
              foreach (WorkflowAction action in LoadActions(childElement))
              {
                if (_menuActions.ContainsKey(action.ActionId))
                  throw new ArgumentException(string.Format(
                      "A menu action with id '{0}' was already registered with action name '{1}' (name of duplicate action is '{2}') -> Forgot to create a new GUID?",
                      action.ActionId, _menuActions[action.ActionId].Name, action.Name));
                _menuActions.Add(action.ActionId, action);
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
          case "WorkflowState":
            WorkflowState workflowState = LoadWorkflowState(childElement);
            if (_states.ContainsKey(workflowState.StateId))
              throw new ArgumentException(string.Format(
                  "A workflow or dialog state with id '{0}' was already declared with name '{1}' (name of duplicate state is '{2}') -> Forgot to create a new GUID?",
                  workflowState.StateId, _states[workflowState.StateId].Name, workflowState.Name));
            _states.Add(workflowState.StateId, workflowState);
            break;
          case "DialogState":
            WorkflowState dialogState = LoadDialogState(childElement);
            if (_states.ContainsKey(dialogState.StateId))
              throw new ArgumentException(string.Format(
                  "A workflow or dialog state with id '{0}' was already declared with name '{1}' (name of duplicate state is '{2}') -> Forgot to create a new GUID?",
                  dialogState.StateId, _states[dialogState.StateId].Name, dialogState.Name));
            _states.Add(dialogState.StateId, dialogState);
            break;
          default:
            throw new ArgumentException("'" + statesElement.Name + "' element doesn't support a child element '" + child.Name + "'");
        }
      }
    }

    protected static IEnumerable<WorkflowAction> LoadActions(XmlElement actionsElement)
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
          case "WorkflowContributorAction":
            yield return LoadWorkflowContributorAction(childElement);
            break;
          // TODO: More actions - show screen, show dialog, call model method, ...
          default:
            throw new ArgumentException("'" + actionsElement.Name + "' element doesn't support a child element '" + child.Name + "'");
        }
      }
    }

    protected static WorkflowState LoadDialogState(XmlElement stateElement)
    {
      string id = null;
      string name = null;
      string dialogScreen = null;
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
          case "DialogScreen":
            dialogScreen = attr.Value;
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
      if (string.IsNullOrEmpty(dialogScreen) || string.IsNullOrEmpty(workflowModelId))
        throw new ArgumentException(string.Format("{0} '{1}': Both 'WorkflowModel' and 'DialogScreen' atrributes must be specified", stateElement.Name, name));
      return new WorkflowState(new Guid(id), name, dialogScreen, false, false,
          new Guid(workflowModelId), WorkflowType.Dialog);
    }

    protected static WorkflowState LoadWorkflowState(XmlElement stateElement)
    {
      string id = null;
      string name = null;
      string mainScreen = null;
      bool inheritMenu = false;
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
      if (string.IsNullOrEmpty(mainScreen) && string.IsNullOrEmpty(workflowModelId))
        throw new ArgumentException(string.Format("{0} '{1}': Either 'WorkflowModel' or 'MainScreen' atrribute must be specified", stateElement.Name, name));
      return new WorkflowState(new Guid(id), name, mainScreen, inheritMenu, false,
          string.IsNullOrEmpty(workflowModelId) ? null : new Guid?(new Guid(workflowModelId)), WorkflowType.Workflow);
    }

    protected static WorkflowAction LoadPushNavigationTransition(XmlElement actionElement)
    {
      string id = null;
      string name = null;
      string displayCategory = null;
      string sortOrder = null;
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
          case "DisplayCategory":
            displayCategory = attr.Value;
            break;
          case "SortOrder":
            sortOrder = attr.Value;
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
      PushNavigationTransition result = new PushNavigationTransition(new Guid(id), name, sourceState == "*" ? new Guid?() : new Guid(sourceState),
          new Guid(targetState), LocalizationHelper.CreateResourceString(displayTitle));
      result.DisplayCategory = displayCategory;
      result.SortOrder = sortOrder;
      return result;
    }

    protected static WorkflowAction LoadPopNavigationTransition(XmlElement actionElement)
    {
      string id = null;
      string name = null;
      string displayCategory = null;
      string sortOrder = null;
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
          case "DisplayCategory":
            displayCategory = attr.Value;
            break;
          case "SortOrder":
            sortOrder = attr.Value;
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
      PopNavigationTransition result = new PopNavigationTransition(new Guid(id), name, sourceState == "*" ? new Guid?() : new Guid(sourceState),
          numPop, LocalizationHelper.CreateResourceString(displayTitle));
      result.DisplayCategory = displayCategory;
      result.SortOrder = sortOrder;
      return result;
    }

    protected static WorkflowAction LoadWorkflowContributorAction(XmlElement actionElement)
    {
      string id = null;
      string name = null;
      string displayCategory = null;
      string sortOrder = null;
      string sourceState = null;
      string contributorModel = null;
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
          case "DisplayCategory":
            displayCategory = attr.Value;
            break;
          case "SortOrder":
            sortOrder = attr.Value;
            break;
          case "SourceState":
            sourceState = attr.Value;
            break;
          case "ContributorModelId":
            contributorModel = attr.Value;
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
      if (string.IsNullOrEmpty(contributorModel))
        throw new ArgumentException(string.Format("{0} '{1}': 'ContributorModelId' attribute missing", actionElement.Name, name));
      WorkflowContributorAction result = new WorkflowContributorAction(new Guid(id), name, sourceState == "*" ? new Guid?() : new Guid(sourceState),
          new Guid(contributorModel), LocalizationHelper.CreateResourceString(displayTitle));
      result.DisplayCategory = displayCategory;
      result.SortOrder = sortOrder;
      return result;
    }
  }
}
