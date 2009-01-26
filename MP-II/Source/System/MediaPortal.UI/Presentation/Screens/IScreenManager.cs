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

namespace MediaPortal.Presentation.Screens
{
  public delegate void DialogCloseCallbackDlgt(string dialogName);

  /// <summary>
  /// External interface for the screen manager. The screen manager is responsible
  /// for managing (logical) screens in the current skin and theme.
  /// This interface will be used by the MediaPortal application modules to communicate with
  /// the screen manager.
  /// </summary>
  public interface IScreenManager
  {
    /// <summary>
    /// Closes the top most main dialog (together with it's children).
    /// </summary>
    void CloseDialog();

    /// <summary>
    /// Shows the dialog screen with the specified name.
    /// </summary>
    /// <param name="dialogName">The logical screen name of the dialog to show.</param>
    void ShowDialog(string dialogName);

    /// <summary>
    /// Shows the dialog screen with the specified name and calls the specified notification
    /// callback method when the dialog is closed.
    /// </summary>
    /// <param name="dialogName">The logical screen name of the dialog to show.</param>
    /// <param name="dialogCloseCallback">Callback delegate method to be called when the dialog
    /// gets closed, or <c>null</c>.</param>
    void ShowDialog(string dialogName, DialogCloseCallbackDlgt dialogCloseCallback);

    /// <summary>
    /// Shows a child dialog with the specified name. The dialog will be marked as child and will 
    /// be closed when main dialog is closed.
    /// </summary>
    /// <param name="dialogName">The logical screen name of the dialog to show.</param>
    void ShowChildDialog(string dialogName);

    /// <summary>
    /// Prepares the specified screen by loading & initializing it.
    /// The screen won't be shown (yet).
    /// </summary>
    /// <param name="screenName">The name of the screen to be prepared.</param>
    /// <returns><c>true</c>, if the specified screen is available and could be loaded,
    /// <c>false</c> if the screen isn't available or if there was a error while loading it.</returns>
    bool PrepareScreen(string screenName);

    /// <summary>
    /// Shows the specified screen.
    /// If the screen was not prepared yet, this method will prepare the screen first before
    /// showing it.
    /// </summary>
    /// <param name="screenName">Name of the screen to be shown.</param>
    bool ShowScreen(string screenName);

    void Reset();

    /// <summary>
    /// Switches the GUI to the specified skin, using the default theme of the skin.
    /// </summary>
    /// <param name="newSkinName">Logical name of the skin.</param>
    void SwitchSkin(string newSkinName);

    /// <summary>
    /// Gets the logical name of the current skin used.
    /// </summary>
    string SkinName { get;}

    /// <summary>
    /// Switches the GUI to the specified theme.
    /// </summary>
    /// <param name="newThemeName">Logical name of the theme.</param>
    void SwitchTheme(string newThemeName);

    /// <summary>
    /// Gets the name of the current theme used.
    /// </summary>
    string ThemeName { get;}

    /// <summary>
    /// Gets the name of the currently shown screen (or dialog).
    /// </summary>
    string CurrentScreenName { get; }

    /// <summary>
    /// Returns <c>true</c> if a dialog is currently visible, else <c>false</c>.
    /// </summary>
    bool IsDialogVisible { get; }
  }
}
