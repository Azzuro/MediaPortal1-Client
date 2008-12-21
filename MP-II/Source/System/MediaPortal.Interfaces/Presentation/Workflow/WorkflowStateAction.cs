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
using MediaPortal.Presentation.DataObjects;

namespace MediaPortal.Presentation.Workflow
{
  /// <summary>
  /// Stores the data for an action which can be triggered when a specified state
  /// is given.
  /// Typically, a workflow state action will provide the data for a menu item at the GUI.
  /// </summary>
  public abstract class WorkflowStateAction
  {
    #region Protected fields

    protected Guid _actionId;
    protected string _name;
    protected Guid _sourceState;
    protected IResourceString _displayTitle;

    #endregion

    protected WorkflowStateAction(Guid actionId, string name, Guid sourceState, IResourceString displayTitle)
    {
      _actionId = actionId;
      _name = name;
      _sourceState = sourceState;
      _displayTitle = displayTitle;
    }

    /// <summary>
    /// Returns the id of this action.
    /// </summary>
    public Guid ActionId
    {
      get { return _actionId; }
    }

    /// <summary>
    /// Returns a human-readable name for this action. This property is only a
    /// hint for developers and designers to identify the action.
    /// </summary>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// Returns the id of the workflow state where this action is available.
    /// </summary>
    public Guid SourceState
    {
      get { return _sourceState; }
    }

    /// <summary>
    /// Returns the localized string displayed at the GUI in the menu for this action.
    /// </summary>
    public IResourceString DisplayTitle
    {
      get { return _displayTitle; }
    }

    /// <summary>
    /// Executes this action. This method will be overridden in subclasses.
    /// </summary>
    public abstract void Execute();
  }
}
