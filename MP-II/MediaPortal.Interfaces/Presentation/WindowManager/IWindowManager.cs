﻿#region Copyright (C) 2007-2008 Team MediaPortal

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

namespace MediaPortal.Presentation.WindowManager
{
  /// <summary>
  /// Interface for a generic window manager. This interface is used by the MediaPortal
  /// application to communicate with the window manager.
  /// </summary>
  public interface IWindowManager
  {
    /// <summary>
    /// Closes the current dialog.
    /// </summary>
    void CloseDialog();

    /// <summary>
    /// Shows the dialog.
    /// </summary>
    /// <param name="window">dialog window name.</param>
    void ShowDialog(string window);

    /// <summary>
    /// Prepares the window by loading & initializing the window 
    /// but doesn't show it (yet).
    /// </summary>
    /// <param name="window">The window.</param>
    void PrepareWindow(string window);

    /// <summary>
    /// Shows the window.
    /// If window is not yet prepared, then this method will prepare the window and then show it.
    /// </summary>
    /// <param name="windowName">Name of the window.</param>
    void ShowWindow(string windowName);

    /// <summary>
    /// Shows the previous window.
    /// </summary>
    void ShowPreviousWindow();

    /// <summary>
    /// Gets the current window.
    /// </summary>
    /// <value>The current window.</value>
    IWindow CurrentWindow { get; }

    /// <summary>
    /// Resets the currently opened windows.
    /// </summary>
    void Reset();

    /// <summary>
    /// Reloads the current window
    /// </summary>
    void Reload();

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
    /// Gets / Sets the Title of a Dialog
    /// </summary>
    string DialogTitle { get; set; }

    /// <summary>
    /// Gets / Sets Dialog Line 1
    /// </summary>
    string DialogLine1 { get; set; }

    /// <summary>
    /// Gets / Sets Dialog Line 2
    /// </summary>
    string DialogLine2 { get; set; }

    /// <summary>
    /// Gets / Sets Dialog Line 3
    /// </summary>
    string DialogLine3 { get; set; }

    /// <summary>
    /// Gets the Dialog Response (Yes/No)
    /// </summary>
    /// <returns></returns>
    bool GetDialogResponse();

    /// <summary>
    /// Sets the Dialog Response
    /// </summary>
    /// <param name="response"></param>
    void SetDialogResponse(string response);
  }
}
