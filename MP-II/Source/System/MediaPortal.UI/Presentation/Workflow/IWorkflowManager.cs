#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
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
using System.Threading;

namespace MediaPortal.UI.Presentation.Workflow
{
  public class NavigationContextConfig
  {
    /// <summary>
    /// Label for the new navigation context to be displayed in the GUI (in the navigation history).
    /// </summary>
    public string NavigationContextDisplayLabel = null;

    /// <summary>
    /// Additional variables to be set at the <see cref="NavigationContext"/> before entering the new state.
    /// If set to <c>null</c>, no additional variables will be added to the navigation context.
    /// </summary>
    public IDictionary<string, object> AdditionalContextVariables = null;
  }

  /// <summary>
  /// Component for tracking application states, managing application workflows and GUI models.
  /// </summary>
  public interface IWorkflowManager
  {
    /// <summary>
    /// Lock object for the workflow manager's internal data like the <see cref="NavigationContextStack"/>
    /// and its internal model cache.
    /// </summary>
    ReaderWriterLockSlim Lock { get; }

    /// <summary>
    /// Returns all currently known workflow states. The dictionary maps state ids to states.
    /// </summary>
    /// <remarks>
    /// This collection will change when plugins are added or removed.
    /// </remarks>
    IDictionary<Guid, WorkflowState> States { get; }

    /// <summary>
    /// Returns all currently known menu state actions. The dictionary maps action ids to actions.
    /// </summary>
    /// <remarks>
    /// This collection maybe change when plugins are added or removed.
    /// </remarks>
    IDictionary<Guid, WorkflowAction> MenuStateActions { get; }

    /// <summary>
    /// Returns the navigation structure consisting of a stack of currently active navigation contexts.
    /// </summary>
    Stack<NavigationContext> NavigationContextStack { get; }

    /// <summary>
    /// Returns the current navigation context in the <see cref="NavigationContextStack"/>.
    /// </summary>
    /// <remarks>
    /// This is a convenience property for calling the <see cref="NavigationContextStack"/>'s <see cref="Stack{T}.Peek"/>
    /// method.
    /// Remember to lock the returned navigation context's <see cref="NavigationContext.SyncRoot"/> object when accessing
    /// the context.
    /// </remarks>
    NavigationContext CurrentNavigationContext { get; }

    /// <summary>
    /// Initializes the workflow manager.
    /// </summary>
    /// <remarks>
    /// This method has to be called after the skin resources are loaded.
    /// </remarks>
    void Initialize();

    /// <summary>
    /// Shuts the workflow manager down.
    /// </summary>
    /// <remarks>
    /// This will dispose all managed instances like models.
    /// </remarks>
    void Shutdown();

    /// <summary>
    /// Navigates to the specified non-transient state. This will push a new navigation context entry
    /// containing the specified state on top of the navigation context stack. This realizes a
    /// forward navigation.
    /// </summary>
    /// <remarks>
    /// This is a convenience method for calling <c>NavigatePush(stateId, null);</c>.
    /// </remarks>
    /// <param name="stateId">Id of the non-transient state to enter.</param>
    /// <param name="config">Configuration for the new state.</param>
    void NavigatePush(Guid stateId, NavigationContextConfig config);

    /// <summary>
    /// Navigates to the specified transient state. This will push a new navigation context entry
    /// containing the specified state on top of the navigation context stack. This realizes a
    /// forward navigation.
    /// </summary>
    /// <remarks>
    /// A transient workflow state is a state which is built by the application on-the-fly and not stored in
    /// the workflow manager. After popping the state away from the navigation context stack, it is not
    /// referenced by the workflow manager any more.
    /// </remarks>
    /// <param name="state">Id of the new transient state to add and enter.</param>
    /// <param name="config">Configuration for the new state.</param>
    void NavigatePushTransient(WorkflowState state, NavigationContextConfig config);

    /// <summary>
    /// Removes the <paramref name="count"/> youngest navigation context levels from the
    /// <see cref="NavigationContextStack"/>. This realizes a "back" navigation.
    /// </summary>
    /// <param name="count">Number of navigation levels to remove.</param>
    void NavigatePop(int count);

    /// <summary>
    /// Removes all youngest navigation context levels from the <see cref="NavigationContextStack"/>
    /// until the workflow state with the specified <paramref name="stateId"/> is on the top of
    /// the navigation stack. This realizes a "cancel" navigation which breaks the current workflow
    /// until the specified state.
    /// </summary>
    /// <param name="stateId">Id of the state until that the navigation stack should be cleaned.</param>
    /// <param name="inclusive">If set to <c>true</c>, the specified state will be popped too, else
    /// it will remain on top of the workflow navigation stack.</param>
    /// <returns><c>true</c>, if the given state was found on the workflow navigation stack and was removed, else
    /// <c>false</c>.</returns>
    bool NavigatePopToState(Guid stateId, bool inclusive);

    /// <summary>
    /// Avoids screen updates during a batch workflow navigation.
    /// </summary>
    /// <remarks>
    /// When this method is called, all screen updates are cached to avoid updates to the screen during a
    /// workflow navigation which spans multiple calls to the navigation methods, like "pop" navigations followed
    /// by "push" navigations. After the batch navigation, method <see cref="EndBatchUpdate"/> must be called which
    /// updates the screen for the new workflow state.
    /// </remarks>
    void StartBatchUpdate();

    /// <summary>
    /// Ends a batch workflow navigation and updates the screen for the new workflow state.
    /// </summary>
    void EndBatchUpdate();

    /// <summary>
    /// Returns the model with the requested <paramref name="modelId"/> and assigns it to be related
    /// to the current <see cref="NavigationContext"/>.
    /// </summary>
    /// <param name="modelId">Id of the model to return.</param>
    /// <returns>Instance of the model with the specified <paramref name="modelId"/>.</returns>
    object GetModel(Guid modelId);

    /// <summary>
    /// Returns the information if one of the active navigation contexts on the stack is the workflow state
    /// with the given <paramref name="workflowStateId"/>.
    /// </summary>
    /// <param name="workflowStateId">Id of the workflow state to search.</param>
    /// <returns><c>true</c>, if the specified workflow state is currently available on the workflow navigation stack,
    /// else <c>false</c>.</returns>
    bool IsStateContainedInNavigationStack(Guid workflowStateId);

    /// <summary>
    /// Returns the information if one of the active navigation contexts on the stack contains the model
    /// with the specified <paramref name="modelId"/>.
    /// </summary>
    /// <param name="modelId">Id of the model to search.</param>
    /// <returns><c>true</c>, if the specified model is currently used in any navigation context, else
    /// <c>false</c>.</returns>
    bool IsModelContainedInNavigationStack(Guid modelId);

    /// <summary>
    /// Clears the cache of GUI models, i.e. removes GUI models which aren't used in any of the navigation
    /// states.
    /// </summary>
    void FlushModelCache();
  }
}
