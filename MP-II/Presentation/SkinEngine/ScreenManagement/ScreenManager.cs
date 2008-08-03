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
using MediaPortal.Presentation.Screen;
using Presentation.SkinEngine.Controls.Visuals;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Presentation.Players;
using MediaPortal.Core.Settings;
using MediaPortal.Control.InputManager;
using Presentation.SkinEngine.SkinManagement;

namespace Presentation.SkinEngine
{
  public class ScreenManager : IScreenManager
  {
    public const string STARTUP_SCREEN = "home";

    #region Variables

    private readonly Dictionary<string, Screen> _windowCache = new Dictionary<string, Screen>();
    private readonly Stack<string> _history = new Stack<string>();
    private Screen _currentScreen = null;
    private Screen _currentDialog = null;
    private Skin _skin = null;
    private Theme _theme = null;
    private SkinManager _skinManager;
    public TimeUtils _utils = new TimeUtils();

    private string _dialogTitle;
    private string[] _dialogLines = new string[3];
    private bool _dialogResponse;  // Yes = true, No = false

    #endregion

    public ScreenManager()
    {
      ScreenSettings screenSettings = new ScreenSettings();
      ServiceScope.Get<ISettingsManager>().Load(screenSettings);
      _skinManager = new SkinManager();

      string skinName = screenSettings.Skin;
      string themeName = screenSettings.Theme;
      if (string.IsNullOrEmpty(skinName))
      {
        skinName = SkinManager.DEFAULT_SKIN;
        themeName = null;
      }

      // Prepare the skin and theme - the theme will be activated in method MainForm_Load
      ServiceScope.Get<ILogger>().Debug("ScreenManager: Loading skin '{0}', theme '{1}'", skinName, themeName);
      PrepareSkinAndTheme(skinName, themeName);

      // Update the settings with our current skin/theme values
      if (screenSettings.Skin != SkinName || screenSettings.Theme != ThemeName)
      {
        screenSettings.Skin = _skin.Name;
        screenSettings.Theme = _theme == null ? null : _theme.Name;
        ServiceScope.Get<ISettingsManager>().Save(screenSettings);
      }
      Fonts.FontManager.Load();
    }

    /// <summary>
    /// Prepares the skin and theme, this will load the skin and theme instances and
    /// set it as the current skin and theme in the <see cref="SkinContext"/>.
    /// After calling this method, the <see cref="SkinContext.SkinResources"/>
    /// contents can be requested.
    /// </summary>
    /// <param name="skinName">The name of the skin to be prepared.</param>
    /// <param name="themeName">The name of the theme for the specified skin to be prepared,
    /// or <c>null</c> for the default theme of the skin.</param>
    protected void PrepareSkinAndTheme(string skinName, string themeName)
    {
      // Release old resources
      _skinManager.ReleaseSkinResources();

      // Prepare new skin data
      Skin skin = _skinManager.Skins.ContainsKey(skinName) ? _skinManager.Skins[skinName] : null;
      if (skin == null)
        skin = _skinManager.DefaultSkin;
      if (skin == null)
        throw new Exception(string.Format("Skin '{0}' not found", skinName));
      Theme theme = themeName == null ? null :
          (skin.Themes.ContainsKey(themeName) ? skin.Themes[themeName] : null);
      if (theme == null)
        theme = skin.DefaultTheme;

      if (!skin.IsValid)
        throw new ArgumentException(string.Format("Skin '{0}' is invalid", skin.Name));
      if (theme != null)
        if (!theme.IsValid)
          throw new ArgumentException(string.Format("Theme '{0}' of skin '{1}' is invalid", theme.Name, skin.Name));
      // Initialize SkinContext with new values
      SkinContext.SkinResources = theme == null ? skin : (SkinResources) theme;
      SkinContext.SkinName = skin.Name;
      SkinContext.ThemeName = theme == null ? null : theme.Name;
      SkinContext.SkinHeight = skin.NativeHeight;
      SkinContext.SkinWidth = skin.NativeWidth;

      _skin = skin;
      _theme = theme;
    }

    protected void InternalCloseScreen()
    {
      if (_currentScreen == null)
        return;
      lock (_history)
      {
        _currentScreen.ScreenState = Screen.State.Closing;
        _currentScreen.HasFocus = false;
        _currentScreen.DetachInput();
        _currentScreen.Hide();
        _currentScreen = null;
      }
    }

    protected void InternalCloseCurrentScreens()
    {
      CloseDialog();
      InternalCloseScreen();
    }

    protected bool InternalShowScreen(Screen screen)
    {
      CloseDialog();
      lock (_history)
      {
        _currentScreen = screen;
        _currentScreen.HasFocus = true;
        _currentScreen.ScreenState = Screen.State.Running;
        _currentScreen.AttachInput();
        _currentScreen.Show();
      }
      return true;
    }

    public void ShowStartupScreen()
    {
      ShowScreen(STARTUP_SCREEN);
    }

    /// <summary>
    /// Renders the current window and dialog.
    /// </summary>
    public void Render()
    {
      TimeUtils.Update();
      SkinContext.Now = DateTime.Now;
      lock (_history)
      {
        lock (_windowCache)
        {
          if (_currentScreen != null)
            _currentScreen.Render();
          if (_currentDialog != null)
            _currentDialog.Render();
        }
      }
    }

    /// <summary>
    /// Switches the active skin and theme. This method will set the skin with
    /// the specified <paramref name="newSkinName"/> and the theme belonging
    /// to this skin with the specified <paramref name="newThemeName"/>, or the
    /// default theme for this skin.
    /// </summary>
    public void SwitchSkinAndTheme(string newSkinName, string newThemeName)
    {
      if (newSkinName == _skin.Name &&
          newThemeName == (_theme == null ? null : _theme.Name)) return;

      lock (_history)
      {
        ServiceScope.Get<ILogger>().Info("ScreenManager: Switching to skin '{0}', theme '{1}'",
            newSkinName, newThemeName);

        string currentScreenName = _currentScreen == null ? null : _currentScreen.Name;
        bool currentScreenInHistory = _currentScreen == null ? false : _currentScreen.History;

        InternalCloseCurrentScreens();

        _windowCache.Clear();

        // FIXME Albert78: Find a better way to make the InputManager, PlayerCollection and
        // ContentManager observe the current skin
        ServiceScope.Get<IInputManager>().Reset();
        ServiceScope.Get<PlayerCollection>().Dispose();
        ContentManager.Clear();

        try
        {
          PrepareSkinAndTheme(newSkinName, newThemeName);
        }
        catch (Exception ex)
        {
          ServiceScope.Get<ILogger>().Error("ScreenManager: Error loading skin '{0}', theme '{1}'", ex, newSkinName, newThemeName);
          // Continue with old skin
          // TODO: Show error dialog
        }
        Fonts.FontManager.Load();

        // We will clear the history because we cannot guarantee that the screens in the
        // history will be compatible with the new skin.
        _history.Clear();
        _history.Push(STARTUP_SCREEN);
        if (currentScreenInHistory && _skin.GetSkinFile(currentScreenName) != null)
          _history.Push(currentScreenName);

        if (_skin.GetSkinFile(currentScreenName) != null)
          _currentScreen = GetScreen(currentScreenName);
        if (_currentScreen == null)
          _currentScreen = GetScreen(_history.Peek());
        if (_currentScreen == null)
        { // The new skin is broken, so reset to default skin
          if (_skin == _skinManager.DefaultSkin)
              // We're already loading the default skin, it seems to be broken
            throw new Exception("The default skin seems to be broken, we don't have a fallback anymore");
          // Try it again with the default skin
          SwitchSkinAndTheme(SkinManager.DEFAULT_SKIN, null);
          return;
        }

        InternalShowScreen(_currentScreen);
      }
      ScreenSettings settings = new ScreenSettings();
      ServiceScope.Get<ISettingsManager>().Load(settings);
      settings.Skin = SkinName;
      settings.Theme = ThemeName;
      ServiceScope.Get<ISettingsManager>().Save(settings);
    }

    /// <summary>
    /// Loads the specified screen from the current skin.
    /// </summary>
    /// <param name="screenName">The screen to load.</param>
    protected UIElement LoadSkinFile(string screenName)
    {
      return SkinContext.SkinResources.LoadSkinFile(screenName) as UIElement;
    }

    /// <summary>
    /// Gets the window displaying the screen with the specified name. If the window is
    /// already loaded, the cached window will be returned. Else, a new window instance
    /// will be created for the specified <paramref name="screenName"/> and loaded from
    /// a skin file. The window will not be shown yet.
    /// </summary>
    /// <param name="screenName">Name of the screen to return the window instance for.</param>
    /// <returns>screen or <c>null</c>, if an error occured loading the window.</returns>
    public Screen GetScreen(string screenName)
    {
      try
      {
        // show waitcursor while loading a new window
        if (_currentScreen != null)
        {
          // TODO: Wait cursor
          //_currentScreen.WaitCursorVisible = true;
        }

        if (_windowCache.ContainsKey(screenName))
          return _windowCache[screenName];

        Screen result = new Screen(screenName);
        try
        {
          UIElement root = LoadSkinFile(screenName);
          if (root == null) return null;
          result.Visual = root;
          _windowCache.Add(screenName, result);
        }
        catch (Exception ex)
        {
          ServiceScope.Get<ILogger>().Error("ScreenManager: Error loading skin file for window '{0}'", ex, screenName);
          // TODO Albert78: Show error dialog with skin loading message
          return null;
        }
        return result;
      }
      finally
      {
        // hide the waitcursor again
        if (_currentScreen != null)
        {
          // TODO: Wait cursor
          //_currentScreen.WaitCursorVisible = false;
        }
      }
    }

    public void SwitchTheme(string newThemeName)
    {
      SwitchSkinAndTheme(SkinContext.SkinName, newThemeName);
    }

    public void SwitchSkin(string newSkinName)
    {
      SwitchSkinAndTheme(newSkinName, null);
    }

    public string SkinName
    {
      get { return _skin.Name; }
    }

    public string ThemeName
    {
      get { return _theme == null ? null : _theme.Name; }
    }

    public string CurrentScreenName
    {
      get { return _currentScreen.Name; }
    }

    public void Reset()
    {
      if (_currentDialog != null)
        _currentDialog.Reset();
      if (_currentScreen != null)
      _currentScreen.Reset();
    }

    /// <summary>
    /// Closes the opened dialog, if one is open.
    /// </summary>
    public void CloseDialog()
    {
      if (_currentDialog == null)
        return;
      lock (_history)
      {
        _currentDialog.ScreenState = Screen.State.Closing;
        _currentDialog.DetachInput();
        _currentDialog.Hide();
        _currentDialog = null;

        if (_currentScreen != null)
        {
          _currentScreen.AttachInput();
          _currentScreen.Show();
        }
      }
    }

    /// <summary>
    /// Shows the dialog with the specified name.
    /// </summary>
    /// <param name="dialogName">The dialog name.</param>
    public void ShowDialog(string dialogName)
    {
      ServiceScope.Get<ILogger>().Debug("ScreenManager: Show dialog: {0}", dialogName);
      CloseDialog();
      lock (_history)
      {
        _currentDialog = GetScreen(dialogName);
        if (_currentDialog == null)
        {
          return;
        }
        _currentScreen.DetachInput();

        _currentDialog.AttachInput();
        _currentDialog.Show();
        _currentDialog.ScreenState = Screen.State.Running;
      }

      while (_currentDialog != null)
      {
        System.Windows.Forms.Application.DoEvents();
        System.Threading.Thread.Sleep(10);
      }
    }

    /// <summary>
    /// Reloads the current window.
    /// </summary>
    public void Reload()
    {
      CloseDialog();
      InternalCloseScreen();

      Screen currentScreen;
      lock (_windowCache)
      {
        string name = _currentScreen.Name;
        if (_windowCache.ContainsKey(name))
          _windowCache.Remove(name);
        currentScreen = GetScreen(name);
      }
      if (currentScreen == null)
        // Error message was shown in GetScreen()
        return;
      InternalShowScreen(currentScreen);
    }

    public bool PrepareScreen(string windowName)
    {
      return GetScreen(windowName) != null;
    }

    /// <summary>
    /// Shows the window with the specified name.
    /// </summary>
    /// <param name="windowName">Name of the window.</param>
    public bool ShowScreen(string windowName)
    {
      ServiceScope.Get<ILogger>().Debug("ScreenManager: Show window: {0}", windowName);
      Screen newScreen = GetScreen(windowName);
      if (newScreen == null)
        // Error message was shown in GetScreen()
        return false;

      lock (_history)
      {
        if (newScreen.History)
        {
          _history.Push(newScreen.Name);
        }

        CloseDialog();
        InternalCloseScreen();
        return InternalShowScreen(newScreen);
      }
    }

    /// <summary>
    /// Shows the previous window from the window history.
    /// </summary>
    public void ShowPreviousScreen()
    {
      lock (_history)
      {
        if (_history.Count == 0)
        {
          return;
        }
        ServiceScope.Get<ILogger>().Debug("ScreenManager: Show previous window");
        if (_currentDialog != null)
        {
          CloseDialog();
          return;
        }

        if (_history.Count <= 1)
        {
          return;
        }

        if (_currentScreen.History)
          _history.Pop();
        InternalCloseScreen();

        Screen newScreen = GetScreen(_history.Peek());
        if (newScreen == null)
          // Error message was shown in GetScreen()
          return;
        InternalShowScreen(newScreen);
      }
    }

    // FIXME Albert78: Move this, if needed, to an own service in ServiceScope
    public TimeUtils TimeUtils
    {
      get
      {
        return _utils;
      }
      set
      {
        _utils = value;
      }
    }

    /// <summary>
    /// Sets a Dialog Response
    /// </summary>
    /// <param name="response"></param>
    public void SetDialogResponse(string response)
    {
      if (response.ToLower() == "yes")
        _dialogResponse = true;
      else
        _dialogResponse = false;

      CloseDialog();
    }

    /// <summary>
    /// Gets the Dialog Response
    /// </summary>
    /// <returns></returns>
    public bool GetDialogResponse()
    {
      return _dialogResponse;
    }

    /// <summary>
    /// Gets / Sets the Dialopg Title
    /// </summary>
    public string DialogTitle
    {
      get
      {
        return _dialogTitle;
      }
      set
      {
        _dialogTitle = value;
      }
    }

    /// <summary>
    /// Gets / Sets Dialog Line 1
    /// </summary>
    public string DialogLine1
    {
     get
      {
        return _dialogLines[0];
      }
      set
      {
        _dialogLines[0] = value;
      }
    }
    
    /// <summary>
    /// Gets / Sets Dialog Line 2
    /// </summary>
    public string DialogLine2
    {
     get
      {
        return _dialogLines[1];
      }
      set
      {
        _dialogLines[1] = value;
      }
    }

    /// <summary>
    /// Gets / Sets Dialog Line 3
    /// </summary>
    public string DialogLine3
    {
      get
      {
        return _dialogLines[2];
      }
      set
      {
        _dialogLines[2] = value;
      }
    }
  }
}
