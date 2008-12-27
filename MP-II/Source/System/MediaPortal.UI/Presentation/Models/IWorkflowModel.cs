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
using MediaPortal.Presentation.Workflow;

namespace MediaPortal.Presentation.Models
{
  /// <summary>
  /// A workflow model is a special GUI model which is able to attend some states of a GUI workflow.
  /// It provides methods to track the current workflow state and to enrich the state with
  /// special state content like special menu- and context-menu-actions.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The workflow manager manages workflow contexts on a stack. Each workflow step/state/context
  /// is put onto the stack. The current workflow state composite consists of all the workflow states
  /// from the root of the stack to its top. This workflow state composite can be thought of as the
  /// menu line showing all navigation steps so far.
  /// </para>
  /// <para>
  /// Example for such a workflow state composite:
  /// <example>
  /// Home > Media > Artist 1 > Album 5 > Track
  /// </example>
  /// During the navigation, the workflow state composite is built up by selecting the appropriate buttons
  /// or other controls in the GUI to trigger the state transitions.<br/>
  /// Each of the workflow states can be attended by a workflow model. In the example, the <i>Home</i>
  /// state doesn't have a workflow model, and all following states are attended by the <i>Media</i> model.
  /// </para>
  /// <para>
  /// A more complex example is:
  /// <example>
  /// <b>(1) Home > (2) Settings > (3) Shares > (4) New share name > (5) Choose share provider > (6) ...</b>
  /// </example>
  /// In this example, the workflow states 2 and 3 are attended by the <i>Configuration</i> model, while
  /// the states 4 et seq. are managed by the <i>Media</i> model.<br/>
  /// To give the workflow models the chance to handle this state flow, there are several methods which
  /// will be called.
  /// In the example above, state 1 doesn't have an attending workflow model. So switching to state 1 doesn't
  /// trigger any workflow model methods.
  /// State 2 is attended by the <i>Configuration</i> workflow model, so here, method
  /// <see cref="EnterModelContext"/> will be called with the <i>Home</i> context as <c>oldContext</c> and
  /// the <i>Settings</i> context as <c>newContext</c>.
  /// State 3 is also attended by the <i>Configuration</i> workflow model, so switching to this state will
  /// trigger a call of method <see cref="ChangeModelContext"/> with the <i>Settings</i> and <i>Shares</i>
  /// contexts as arguments.
  /// State 4 is attended by the <i>Media</i> model. So when switching to this state, two method calls
  /// will take place: First, the configuration model will temporary be deactivated by a call to its
  /// <see cref="Deactivate"/> method with the <i>Shares</i> and <i>New share name</i> states as arguments.
  /// Second, the <i>Media</i> model will be invoked by calling its <see cref="EnterModelContext"/> method
  /// (same arguments).
  /// Switching to state 5 is similar as switching from state 2 to 3: Method <see cref="ChangeModelContext"/>
  /// will be called at the <i>Media</i> model.
  /// </para>
  /// <para>
  /// In short, the forward navigation produces this method call sequence:
  /// State 1 -> 2:
  /// <c>configurationModel.EnterModelContext([Home], [Settings]);</c>
  /// State 2 -> 3:
  /// <c>configurationModel.ChangeModelContext([Settings], [Shares]);</c>
  /// State 3 -> 4:
  /// <c>configurationModel.Deactivate([Shares], [New share name]);</c>
  /// <c>mediaModel.EnterModelContext([Shares], [New share name]);</c>
  /// State 4 -> 5:
  /// <c>mediaModel.ChangeModelContext([New share name], [Choose share provider]);</c>
  /// ...
  /// </para>
  /// <para>
  /// When navigating back, the inverse methods are called in the inverse sequence:
  /// ...
  /// State 5 -> 4:
  /// <c>mediaModel.ChangeModelContext([Choose share provider], [New share name]);</c>
  /// State 4 -> 3:
  /// <c>mediaModel.ExitModelContext([New share name], [Shares]);</c>
  /// <c>configurationModel.ReActivate([New share name], [Shares]);</c>
  /// State 3 -> 2:
  /// <c>configurationModel.ChangeModelContext([Shares], [Settings]);</c>
  /// State 2 -> 1:
  /// <c>configurationModel.ExitModelContext([Settings], [Home]);</c>
  /// </para>
  /// </remarks>
  public interface IWorkflowModel
  {
    /// <summary>
    /// Returns the id of this model. The returned id has to be the same as the id used to register
    /// this model in the plugin descriptor.
    /// </summary>
    Guid ModelId { get; }

    /// <summary>
    /// Informs this model about the entrance of its workflow attendance. This means the current workflow
    /// context will be changed from an unattended context to an attended one.
    /// </summary>
    /// <remarks>
    /// This method will be called before the call of <see cref="UpdateMenuActions"/> and
    /// <see cref="UpdateContextMenuActions"/>, and the workflow state might not be the top context
    /// onto the workflow context stack yet.
    /// </remarks>
    /// <param name="oldContext">The old navigation context which was active before the
    /// <paramref name="newContext"/>. This context was attended by another workflow model.</param>
    /// <param name="newContext">The workflow navigation context which was entered. This context is
    /// attended by this model.</param>
    void EnterModelContext(NavigationContext oldContext, NavigationContext newContext);

    /// <summary>
    /// Will exit the workflow attendance of this model, which means the last attended workflow context
    /// will be removed from the workflow context stack.
    /// </summary>
    /// <param name="oldContext">The old workflow navigation context which was active before the
    /// <paramref name="newContext"/>. This context was attended by this workflow model.</param>
    /// <param name="newContext">The workflow navigation context which will be entered. This context is
    /// attended by another workflow model.</param>
    void ExitModelContext(NavigationContext oldContext, NavigationContext newContext);

    /// <summary>
    /// Informs this model about the change of a the workflow navigation context during the
    /// workflow attendance of this model. This means an the current workflow context will be changed
    /// from one attended context to another attended context.
    /// </summary>
    /// <remarks>
    /// This method will be called before the call of <see cref="UpdateMenuActions"/> and
    /// <see cref="UpdateContextMenuActions"/>, and the workflow state might not be the top context
    /// onto the workflow context stack yet.
    /// </remarks>
    /// <param name="oldContext">The workflow navigation context which was active before the
    /// <paramref name="newContext"/> and which was attended by this model.</param>
    /// <param name="newContext">The workflow navigation context which was entered and which will be attended
    /// by this model now.</param>
    /// <param name="push">If set to <c>true</c>, the <paramref name="newContext"/> was pushed onto the
    /// workflow context stack, else the <paramref name="oldContext"/> was removed.</param>
    void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push);

    /// <summary>
    /// Temporary deactivates the workflow attendance of this workflow model. This means another model
    /// will temporary attend the workflow now.
    /// </summary>
    /// This method will be called before the call of <see cref="UpdateMenuActions"/> and
    /// <see cref="UpdateContextMenuActions"/>, and the workflow state might not be the top context
    /// onto the workflow context stack yet.
    /// </remarks>
    /// <param name="oldContext">The workflow navigation context which was active before the
    /// <paramref name="newContext"/> and which was attended by this model.</param>
    /// <param name="newContext">The workflow navigation context which will entered and which will
    /// attended by another workflow model.</param>
    void Deactivate(NavigationContext oldContext, NavigationContext newContext);

    /// <summary>
    /// Reactivates the workflow attendance of this workflow model. This means another model, which did
    /// temporary the attendance of the workflow, was exited.
    /// </summary>
    /// This method will be called before the call of <see cref="UpdateMenuActions"/> and
    /// <see cref="UpdateContextMenuActions"/>, and the workflow state might not be the top context
    /// onto the workflow context stack yet.
    /// </remarks>
    /// <param name="oldContext">The workflow navigation context which was active before the
    /// <paramref name="newContext"/> and which was attended by another workflow model.</param>
    /// <param name="newContext">The workflow navigation context which was re-activated and which will
    /// continue to be attended by this model now.</param>
    void ReActivate(NavigationContext oldContext, NavigationContext newContext);

    /// <summary>
    /// Adds additional menu actions which are created dynamically for the state of the specified
    /// navigation <paramref name="context"/>, or updates/removes existing actions.
    /// </summary>
    /// <remarks>
    /// The updated collection of actions should remain valid while the specified navigation
    /// <paramref name="context"/> is valid.
    /// </remarks>
    /// <param name="context">Current navigation context, which should be enriched with additional
    /// dynamic menu actions.</param>
    /// <param name="actions">Collection where this model can add additional menu actions valid for
    /// the specified navigation <paramref name="context"/>.</param>
    void UpdateMenuActions(NavigationContext context, ICollection<WorkflowStateAction> actions);

    /// <summary>
    /// Adds additional context menu actions which are created dynamically for the state of the specified
    /// navigation <paramref name="context"/>, or updates/removes existing actions.
    /// </summary>
    /// <remarks>
    /// The updated collection of actions should remain valid while the specified navigation
    /// <paramref name="context"/> is valid.
    /// </remarks>
    /// <param name="context">Current navigation context, which should be enriched with additional
    /// dynamic context menu actions.</param>
    /// <param name="actions">Collection where this model can add additional context menu actions valid for
    /// the specified navigation <paramref name="context"/>.</param>
    void UpdateContextMenuActions(NavigationContext context, ICollection<WorkflowStateAction> actions);
  }
}