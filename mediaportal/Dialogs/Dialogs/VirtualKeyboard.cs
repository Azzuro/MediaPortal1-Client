#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class VirtualKeyboard : GUIWindow, IRenderLayer
  {
    #region constants
    const int GAP_WIDTH = 0;
    const int GAP2_WIDTH = 4;
    int MODEKEY_WIDTH = 110;
    const int KEY_INSET = 1;

    const int MAX_KEYS_PER_ROW = 14;

    // Must be this far from center on 0.0 - 1.0 scale
    const float JOY_THRESHOLD = 0.25f;

    // How often (per second) the caret blinks
    const float fCARET_BLINK_RATE = 1.0f;

    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    const float fCARET_ON_RATIO = 0.75f;

    // Text colors for keys
    const long COLOR_SEARCHTEXT = 0xff000000;   // black (0xff10e010)
    const long COLOR_HIGHLIGHT = 0xff00ff00;   // green
    const long COLOR_PRESSED = 0xff808080;   // gray
    const long COLOR_NORMAL = 0xff000000;   // black
    const long COLOR_DISABLED = 0xffffffff;   // white
    const long COLOR_HELPTEXT = 0xffffffff;   // white
    const long COLOR_FONT_DISABLED = 0xff808080;   // gray
    const long COLOR_INVISIBLE = 0xff0000ff;   // blue
    const long COLOR_RED = 0xffff0000;   // red
    // Font sizes
    const int FONTSIZE_BUTTONCHARS = 24;
    const int FONTSIZE_BUTTONSTRINGS = 18;
    const int FONTSIZE_SEARCHTEXT = 20;

    // Controller repeat values
    const float fINITIAL_REPEAT = 0.333f; // 333 mS recommended for first repeat
    const float fSTD_REPEAT = 0.085f; // 85 mS recommended for repeat rate

    // Maximum number of characters in string
    const int MAX_CHARS = 64;

    // Width of text box
    float fTEXTBOX_WIDTH = 576.0f - 64.0f - 4.0f - 4.0f - 10.0f;
    float BUTTON_Y_POS = 411.0f;      // button text line
    float BUTTON_X_OFFSET = 40.0f;      // space between button and text

    const long BUTTONTEXT_COLOR = 0xffffffff;
    const float FIXED_JSL_SIZE = 3.0f;

    const int KEY_WIDTH = 34;   // width of std key in pixels

 
    #endregion

    #region enums
    public enum SearchKinds
    {
      SEARCH_STARTS_WITH = 0,
      SEARCH_CONTAINS,
      SEARCH_ENDS_WITH,
      SEARCH_IS
    }

    enum KeyboardTypes
    {
      TYPE_ALPHABET = 0,
      TYPE_SYMBOLS,
      TYPE_ACCENTS,

      TYPE_HIRAGANA,
      TYPE_KATAKANA,
      TYPE_ANS,

      TYPE_MAX
    };

    enum State
    {
      STATE_BACK,         // Main menu
      STATE_KEYBOARD,     // Keyboard display
      STATE_MAX
    };



    enum Event
    {
      EV_NULL,            // No events
      EV_A_BUTTON,        // A button
      EV_START_BUTTON,    // Start button
      EV_B_BUTTON,        // B button
      EV_BACK_BUTTON,     // Back button
      EV_X_BUTTON,        // X button
      EV_Y_BUTTON,        // Y button
      EV_WHITE_BUTTON,    // White button
      EV_BLACK_BUTTON,    // Black button
      EV_LEFT_BUTTON,     // Left trigger
      EV_RIGHT_BUTTON,    // Right trigger
      EV_UP,              // Up Dpad or left joy
      EV_DOWN,            // Down Dpad or left joy
      EV_LEFT,            // Left Dpad or left joy
      EV_RIGHT,           // Right Dpad or left joy

      EVENT_MAX
    };

    enum Xkey
    {
      XK_NULL = 0,

      XK_SPACE = ' ',
      XK_LBRACK = '[',
      XK_RBRACK = ']',
      XK_LBRACE = '{',
      XK_RBRACE = '}',
      XK_LPAREN = '(',
      XK_RPAREN = ')',
      XK_FSLASH = '/',
      XK_BSLASH = '\\',
      XK_LT = '<',
      XK_GT = '>',
      XK_AT = '@',
      XK_SEMI = ';',
      XK_COLON = ':',
      XK_QUOTE = '\'',
      XK_DQUOTE = '\"',
      XK_AMPER = '&',
      XK_STAR = '*',
      XK_QMARK = '?',
      XK_COMMA = ',',
      XK_PERIOD = '.',
      XK_DASH = '-',
      XK_UNDERS = '_',
      XK_PLUS = '+',
      XK_EQUAL = '=',
      XK_DOLLAR = '$',
      XK_PERCENT = '%',
      XK_CARET = '^',
      XK_TILDE = '~',
      XK_APOS = '`',
      XK_EXCL = '!',
      XK_VERT = '|',
      XK_NSIGN = '#',

      // Numbers
      XK_0 = '0',
      XK_1,
      XK_2,
      XK_3,
      XK_4,
      XK_5,
      XK_6,
      XK_7,
      XK_8,
      XK_9,

      // Letters
      XK_A = 'A',
      XK_B,
      XK_C,
      XK_D,
      XK_E,
      XK_F,
      XK_G,
      XK_H,
      XK_I,
      XK_J,
      XK_K,
      XK_L,
      XK_M,
      XK_N,
      XK_O,
      XK_P,
      XK_Q,
      XK_R,
      XK_S,
      XK_T,
      XK_U,
      XK_V,
      XK_W,
      XK_X,
      XK_Y,
      XK_Z,

      // Accented characters and other special characters

      XK_INVERTED_EXCL = 0xA1, // �
      XK_CENT_SIGN = 0xA2, // �
      XK_POUND_SIGN = 0xA3, // �
      XK_YEN_SIGN = 0xA5, // �
      XK_COPYRIGHT_SIGN = 0xA9, // �
      XK_LT_DBL_ANGLE_QUOTE = 0xAB, // <<
      XK_REGISTERED_SIGN = 0xAE, // �
      XK_SUPERSCRIPT_TWO = 0xB2, // �
      XK_SUPERSCRIPT_THREE = 0xB3, // �
      XK_ACUTE_ACCENT = 0xB4, // �
      XK_MICRO_SIGN = 0xB5, // �
      XK_SUPERSCRIPT_ONE = 0xB9, // �
      XK_RT_DBL_ANGLE_QUOTE = 0xBB, // >>
      XK_INVERTED_QMARK = 0xBF, // �
      XK_CAP_A_GRAVE = 0xC0, // �
      XK_CAP_A_ACUTE = 0xC1, // �
      XK_CAP_A_CIRCUMFLEX = 0xC2, // �
      XK_CAP_A_TILDE = 0xC3, // �
      XK_CAP_A_DIAERESIS = 0xC4, // �
      XK_CAP_A_RING = 0xC5, // �
      XK_CAP_AE = 0xC6, // �
      XK_CAP_C_CEDILLA = 0xC7, // �
      XK_CAP_E_GRAVE = 0xC8, // �
      XK_CAP_E_ACUTE = 0xC9, // �
      XK_CAP_E_CIRCUMFLEX = 0xCA, // �
      XK_CAP_E_DIAERESIS = 0xCB, // �
      XK_CAP_I_GRAVE = 0xCC, // �
      XK_CAP_I_ACUTE = 0xCD, // �
      XK_CAP_I_CIRCUMFLEX = 0xCE, // �
      XK_CAP_I_DIAERESIS = 0xCF, // �
      XK_CAP_N_TILDE = 0xD1, // �
      XK_CAP_O_GRAVE = 0xD2, // �
      XK_CAP_O_ACUTE = 0xD3, // �
      XK_CAP_O_CIRCUMFLEX = 0xD4, // �
      XK_CAP_O_TILDE = 0xD5, // �
      XK_CAP_O_DIAERESIS = 0xD6, // �
      XK_CAP_O_STROKE = 0xD8, // �
      XK_CAP_U_GRAVE = 0xD9, // �
      XK_CAP_U_ACUTE = 0xDA, // �
      XK_CAP_U_CIRCUMFLEX = 0xDB, // �
      XK_CAP_U_DIAERESIS = 0xDC, // �
      XK_CAP_Y_ACUTE = 0xDD, // �
      XK_SM_SHARP_S = 0xDF, // �
      XK_SM_A_GRAVE = 0xE0, // �
      XK_SM_A_ACUTE = 0xE1, // �
      XK_SM_A_CIRCUMFLEX = 0xE2, // �
      XK_SM_A_TILDE = 0xE3, // �
      XK_SM_A_DIAERESIS = 0xE4, // �
      XK_SM_A_RING = 0xE5, // �
      XK_SM_AE = 0xE6, // �
      XK_SM_C_CEDILLA = 0xE7, // �
      XK_SM_E_GRAVE = 0xE8, // �
      XK_SM_E_ACUTE = 0xE9, // �
      XK_SM_E_CIRCUMFLEX = 0xEA, // �
      XK_SM_E_DIAERESIS = 0xEB, // �
      XK_SM_I_GRAVE = 0xEC, // �
      XK_SM_I_ACUTE = 0xED, // �
      XK_SM_I_CIRCUMFLEX = 0xEE, // �
      XK_SM_I_DIAERESIS = 0xEF, // �
      XK_SM_N_TILDE = 0xF1, // �
      XK_SM_O_GRAVE = 0xF2, // �
      XK_SM_O_ACUTE = 0xF3, // �
      XK_SM_O_CIRCUMFLEX = 0xF4, // �
      XK_SM_O_TILDE = 0xF5, // �
      XK_SM_O_DIAERESIS = 0xF6, // �
      XK_SM_O_STROKE = 0xF8, // �
      XK_SM_U_GRAVE = 0xF9, // �
      XK_SM_U_ACUTE = 0xFA, // �
      XK_SM_U_CIRCUMFLEX = 0xFB, // �
      XK_SM_U_DIAERESIS = 0xFC, // �
      XK_SM_Y_ACUTE = 0xFD, // �
      XK_SM_Y_DIAERESIS = 0xFF, // �

      // Unicode
      XK_CAP_Y_DIAERESIS = 0x0178, // Y umlaut
      XK_EURO_SIGN = 0x20AC, // Euro symbol
      XK_ARROWLEFT = '<', // left arrow
      XK_ARROWRIGHT = '>', // right arrow

      // Special
      XK_BACKSPACE = 0x10000, // backspace
      XK_DELETE,              // delete           // !!!
      XK_SHIFT,               // shift
      XK_CAPSLOCK,            // caps lock
      XK_ALPHABET,            // alphabet
      XK_SYMBOLS,             // symbols
      XK_ACCENTS,             // accents
      XK_OK,                  // "done"
      XK_HIRAGANA,            // Hiragana
      XK_KATAKANA,            // Katakana
      XK_ANS,                 // Alphabet/numeral/symbol

      // Special Search-Keys
      XK_SEARCH_START_WITH = 0x11000, // to search music that starts with string
      XK_SEARCH_CONTAINS, // ...contains string
      XK_SEARCH_ENDS_WITH, // ...ends with string
      XK_SEARCH_IS, // is the search text
      XK_SEARCH_ALBUM, // search for album
      XK_SEARCH_TITLE, // search for title
      XK_SEARCH_ARTIST, // search for artist
      XK_SEARCH_GENERE // search for genere
    };

    enum StringID
    {
      STR_MENU_KEYBOARD_NAME,
      STR_MENU_CHOOSE_KEYBOARD,
      STR_MENU_ILLUSTRATIVE_GRAPHICS,
      STR_MENU_A_SELECT,
      STR_MENU_B_BACK,
      STR_MENU_Y_HELP,
      STR_KEY_SPACE,
      STR_KEY_BACKSPACE,
      STR_KEY_SHIFT,
      STR_KEY_CAPSLOCK,
      STR_KEY_ALPHABET,
      STR_KEY_SYMBOLS,
      STR_KEY_ACCENTS,
      STR_KEY_DONE,
      STR_HELP_SELECT,
      STR_HELP_CANCEL,
      STR_HELP_TOGGLE,
      STR_HELP_HELP,
      STR_HELP_BACKSPACE,
      STR_HELP_SPACE,
      STR_HELP_TRIGGER,

      STR_MAX,
    };
    #endregion


    class Key
    {
      public Xkey xKey;       // virtual key code
      public int dwWidth = KEY_WIDTH;    // width of the key
      public string name = "";    // name of key when vKey >= 0x10000
      public Key(Xkey key)
      {
        xKey = key;
      }
      public Key(Xkey key, int iwidth)
      {
        xKey = key;
        dwWidth = iwidth;

        // Special keys get their own names
        switch (xKey)
        {
          case Xkey.XK_SPACE:
            name = "SPACE";
            break;
          case Xkey.XK_BACKSPACE:
            name = "BKSP";
            break;
          case Xkey.XK_SHIFT:
            name = "SHIFT";
            break;
          case Xkey.XK_CAPSLOCK:
            name = "CAPS";
            break;
          case Xkey.XK_ALPHABET:
            name = "ALPHABET";
            break;
          case Xkey.XK_SYMBOLS:
            name = "SYMB";
            break;
          case Xkey.XK_ACCENTS:
            name = "ACCENTS";
            break;
          case Xkey.XK_OK:
            name = GUILocalizeStrings.Get(804);
            break;
          case Xkey.XK_SEARCH_CONTAINS:
            name = GUILocalizeStrings.Get(801);
            break;
          case Xkey.XK_SEARCH_ENDS_WITH:
            name = GUILocalizeStrings.Get(802);
            break;
          case Xkey.XK_SEARCH_START_WITH:
            name = GUILocalizeStrings.Get(800);
            break;
          case Xkey.XK_SEARCH_IS:
            name = GUILocalizeStrings.Get(803);
            break;
        }
      }
    };

    #region variables
    string _textEntered = "";
    bool _capsLockTurnedOn = false;
    bool _shiftTurnedOn = false;
    State _state;
    int _position;
    KeyboardTypes _currentKeyboard;
    int _currentRow;
    int _currentKey;
    int _lastColumn;
    //float         m_fRepeatDelay;
    CachedTexture.Frame _keyTexture = null;
    float _keyHeight;
    int _maxRows;
    bool _pressedEnter;
    GUIFont _font18 = null;
    GUIFont _font12 = null;
    GUIFont _fontButtons = null;
    GUIFont _fontSearchText = null;
    DateTime _caretTimer = DateTime.Now;
    bool _previousOverlayVisible = true;
    bool _password = false;
    GUIImage image;
    bool _useSearchLayout = false;

    // added by Agree
    int _searchKind; // 0=Starts with, 1=Contains, 2=Ends with
    //

    ArrayList _keyboardList = new ArrayList();         // list of rows = keyboard

    #endregion


    #region Base Dialog Variables
    bool _isVisible = false;
    int _parentWindowId = 0;
    GUIWindow _parentWindow = null;
    #endregion

    // lets do some event stuff
    public delegate void TextChangedEventHandler(int kindOfSearch, string evtData);
    public event TextChangedEventHandler TextChanged;
    //

    public VirtualKeyboard()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      _keyTexture = null;

      _keyHeight = 42.0f;
      _maxRows = 5;
      _pressedEnter = false;
      _caretTimer = DateTime.Now;
      // construct search def.
      _searchKind = (int)SearchKinds.SEARCH_CONTAINS; // default search Contains

      if (GUIGraphicsContext.DX9Device != null)
        InitBoard();
    }

    public override bool Init()
    {
      return true;
    }

    public bool IsConfirmed
    {
      get { return _pressedEnter; }
    }

    public bool IsSearchKeyboard
    {
      set { _useSearchLayout = value; }
    }

    void Initialize()
    {
      _font12 = GUIFontManager.GetFont("font12");
      _font18 = GUIFontManager.GetFont("font18");
      _fontButtons = GUIFontManager.GetFont("dingbats");
      _fontSearchText = GUIFontManager.GetFont("font14");

      int iTextureWidth, iTextureHeight;
      int iImages = GUITextureManager.Load("keyNF.bmp", 0, 0, 0);
      if (iImages == 1)
      {
        _keyTexture = GUITextureManager.GetTexture("keyNF.bmp", 0, out iTextureWidth, out iTextureHeight);
      }
      image = new GUIImage(this.GetID, 1, 0, 0, 10, 10, "white.bmp", 1);
      image.AllocResources();
    }

    void DeInitialize()
    {
      if (image != null) image.FreeResources();
      image = null;
    }

    public void Reset()
    {
      _password = false;
      _pressedEnter = false;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      _keyHeight = 42.0f;
      _maxRows = 5;
      _position = 0;
      _textEntered = "";
      _caretTimer = DateTime.Now;

      _searchKind = (int)SearchKinds.SEARCH_CONTAINS; // default search Contains

      int y = 411;
      int x = 40;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
      BUTTON_Y_POS = x;      // button text line
      BUTTON_X_OFFSET = y;      // space between button and text

      int width = 42;
      GUIGraphicsContext.ScaleHorizontal(ref width);
      _keyHeight = width;

      width = (int)(576.0f - 64.0f - 4.0f - 4.0f - 10.0f);
      GUIGraphicsContext.ScaleHorizontal(ref width);
      fTEXTBOX_WIDTH = width;

      InitBoard();
    }

    public bool Password
    {
      get { return _password; }
      set { _password = value; }
    }

    protected void PageLoad()
    {
      _previousOverlayVisible = GUIGraphicsContext.Overlay;
      _pressedEnter = false;
      GUIGraphicsContext.Overlay = false;
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD));
      Log.Debug("Window: {0} init", this.ToString());
      Initialize();
    }

    protected void PageDestroy()
    {
      GUIGraphicsContext.Overlay = _previousOverlayVisible;
      DeInitialize();

      Log.Debug("Window: {0} deinit", this.ToString());
      FreeResources();
    }

    public string Text
    {
      get { return _textEntered; }
      set { _textEntered = value; }
    }

    public int KindOfSearch
    {
      get { return _searchKind; }
      set
      {
        _searchKind = value;
        SetSearchKind();
      }
    }

    public void SelectActiveButton(float x, float y)
    {
      // Draw each row
      int y1 = 250, x1 = 64;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      float fY = y1;
      ArrayList keyBoard = (ArrayList)_keyboardList[(int)_currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += _keyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList)keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          Key key = (Key)keyRow[i];
          int width = key.dwWidth;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          if (x >= fX + fWidthSum && x <= fX + fWidthSum + key.dwWidth)
          {
            if (y >= fY && y < fY + _keyHeight)
            {
              _currentRow = row;
              _currentKey = dwIndex;
              return;
            }
          }
          fWidthSum += width;
          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
          {
            width = GAP2_WIDTH;
            GUIGraphicsContext.ScaleHorizontal(ref width);
            fWidthSum += width;
          }
          else
          {
            width = GAP_WIDTH;
            GUIGraphicsContext.ScaleHorizontal(ref width);
            fWidthSum += width;
          }
          ++dwIndex;

        }
      }

      // Default no key found no key highlighted
      if (_currentKey != -1) _lastColumn = _currentKey;
      _currentKey = -1;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }

      Event ev;
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_MOVE:
          SelectActiveButton(action.fAmount1, action.fAmount2);
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          ev = Event.EV_A_BUTTON;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_SELECT_ITEM:
          if (_currentKey == -1)
          {
            Close();
            _pressedEnter = true;
          }
          ev = Event.EV_A_BUTTON;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          ev = Event.EV_DOWN;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          ev = Event.EV_UP;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          ev = Event.EV_LEFT;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          ev = Event.EV_RIGHT;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          ev = Event.EV_BACK_BUTTON;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            if (action.m_key.KeyChar >= 32)
              Press((char)action.m_key.KeyChar);
            if (action.m_key.KeyChar == 8)
            {
              Press(Xkey.XK_BACKSPACE);
            }
          }
          break;
      }
    }
    void Close()
    {
      _isVisible = false;
    }

    public void DoModal(int dwParentId)
    {

      _parentWindowId = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowId);
      if (null == _parentWindow)
      {
        _parentWindowId = 0;
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;

      GUIWindowManager.RouteToWindow(GetID);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);

      // active this window... (with its own OnPageLoad)
      PageLoad();

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _isVisible = true;
      _position = _textEntered.Length;
      while (_isVisible && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        // deactive this window... (with its own OnPageDestroy)
        PageDestroy();

        GUIWindowManager.UnRoute();
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);
    }

    public override void Render(float timePassed)
    {

      lock (this)
      {

        // render the parent window
        RenderKeyboardLatin(timePassed);
      }
    }

    void InitBoard()
    {
      if (_useSearchLayout)
        MODEKEY_WIDTH = 130;  // Searchkeyboard

      // Restore keyboard to default state
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 1;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _textEntered = "";
      _position = 0;
      int height = 42;
      GUIGraphicsContext.ScaleVertical(ref height);
      _keyHeight = height;
      _maxRows = 5;

      // Destroy old keyboard
      _keyboardList.Clear();


      //-------------------------------------------------------------------------
      // Alpha keyboard
      //-------------------------------------------------------------------------

      ArrayList keyBoard = new ArrayList();

      // First row is Done, 1-0
      ArrayList keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_1));
      keyRow.Add(new Key(Xkey.XK_2));
      keyRow.Add(new Key(Xkey.XK_3));
      keyRow.Add(new Key(Xkey.XK_4));
      keyRow.Add(new Key(Xkey.XK_5));
      keyRow.Add(new Key(Xkey.XK_6));
      keyRow.Add(new Key(Xkey.XK_7));
      keyRow.Add(new Key(Xkey.XK_8));
      keyRow.Add(new Key(Xkey.XK_9));
      keyRow.Add(new Key(Xkey.XK_0));

      keyBoard.Add(keyRow);

      // Second row is Shift, A-J
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_A));
      keyRow.Add(new Key(Xkey.XK_B));
      keyRow.Add(new Key(Xkey.XK_C));
      keyRow.Add(new Key(Xkey.XK_D));
      keyRow.Add(new Key(Xkey.XK_E));
      keyRow.Add(new Key(Xkey.XK_F));
      keyRow.Add(new Key(Xkey.XK_G));
      keyRow.Add(new Key(Xkey.XK_H));
      keyRow.Add(new Key(Xkey.XK_I));
      keyRow.Add(new Key(Xkey.XK_J));
      keyBoard.Add(keyRow);

      // Third row is Caps Lock, K-T
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_K));
      keyRow.Add(new Key(Xkey.XK_L));
      keyRow.Add(new Key(Xkey.XK_M));
      keyRow.Add(new Key(Xkey.XK_N));
      keyRow.Add(new Key(Xkey.XK_O));
      keyRow.Add(new Key(Xkey.XK_P));
      keyRow.Add(new Key(Xkey.XK_Q));
      keyRow.Add(new Key(Xkey.XK_R));
      keyRow.Add(new Key(Xkey.XK_S));
      keyRow.Add(new Key(Xkey.XK_T));
      keyBoard.Add(keyRow);

      // Fourth row is Accents, U-Z, Backspace
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));   // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_U));
      keyRow.Add(new Key(Xkey.XK_V));
      keyRow.Add(new Key(Xkey.XK_W));
      keyRow.Add(new Key(Xkey.XK_X));
      keyRow.Add(new Key(Xkey.XK_Y));
      keyRow.Add(new Key(Xkey.XK_Z));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row is <empty>, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));   // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));
        

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the alpha keyboard to the list
      _keyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Symbol keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_LPAREN));
      keyRow.Add(new Key(Xkey.XK_RPAREN));
      keyRow.Add(new Key(Xkey.XK_AMPER));
      keyRow.Add(new Key(Xkey.XK_UNDERS));
      keyRow.Add(new Key(Xkey.XK_CARET));
      keyRow.Add(new Key(Xkey.XK_PERCENT));
      keyRow.Add(new Key(Xkey.XK_BSLASH));
      keyRow.Add(new Key(Xkey.XK_FSLASH));
      keyRow.Add(new Key(Xkey.XK_AT));
      keyRow.Add(new Key(Xkey.XK_NSIGN));

      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_LBRACK));
      keyRow.Add(new Key(Xkey.XK_RBRACK));
      keyRow.Add(new Key(Xkey.XK_DOLLAR));
      keyRow.Add(new Key(Xkey.XK_POUND_SIGN));
      keyRow.Add(new Key(Xkey.XK_YEN_SIGN));
      keyRow.Add(new Key(Xkey.XK_EURO_SIGN));
      keyRow.Add(new Key(Xkey.XK_SEMI));
      keyRow.Add(new Key(Xkey.XK_COLON));
      keyRow.Add(new Key(Xkey.XK_QUOTE));
      keyRow.Add(new Key(Xkey.XK_DQUOTE));
      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_LT));
      keyRow.Add(new Key(Xkey.XK_GT));
      keyRow.Add(new Key(Xkey.XK_QMARK));
      keyRow.Add(new Key(Xkey.XK_EXCL));
      keyRow.Add(new Key(Xkey.XK_INVERTED_QMARK));
      keyRow.Add(new Key(Xkey.XK_INVERTED_EXCL));
      keyRow.Add(new Key(Xkey.XK_DASH));
      keyRow.Add(new Key(Xkey.XK_STAR));
      keyRow.Add(new Key(Xkey.XK_PLUS));
      keyRow.Add(new Key(Xkey.XK_EQUAL));
      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH)); // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_LBRACE));
      keyRow.Add(new Key(Xkey.XK_RBRACE));
      keyRow.Add(new Key(Xkey.XK_LT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_RT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_COMMA));
      keyRow.Add(new Key(Xkey.XK_PERIOD));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row is Accents, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH)); // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the symbol keyboard to the list
      _keyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Accents keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
      // Swedish - Finnish
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_A_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_I_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_I_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_I_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_I_DIAERESIS));
      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));

      //Danish - Norwegian
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING));
      keyRow.Add(new Key(Xkey.XK_CAP_AE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_STROKE));
      keyRow.Add(new Key(Xkey.XK_CAP_C_CEDILLA));
      keyRow.Add(new Key(Xkey.XK_CAP_E_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_E_DIAERESIS));

      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));  // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));

      // German
      keyRow.Add(new Key(Xkey.XK_CAP_U_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_SM_SHARP_S));
      keyRow.Add(new Key(Xkey.XK_CAP_O_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_O_TILDE));

      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH)); // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_CAP_N_TILDE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_CIRCUMFLEX));

      keyRow.Add(new Key(Xkey.XK_CAP_Y_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_Y_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH)); // Searchkeyboard
      else
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the accents keyboard to the list
      _keyboardList.Add(keyBoard);

    }

    void UpdateState(Event ev)
    {
      switch (_state)
      {
        case State.STATE_KEYBOARD:
          switch (ev)
          {
            case Event.EV_A_BUTTON:           // Select current key
            case Event.EV_START_BUTTON:
              PressCurrent();
              break;

            case Event.EV_B_BUTTON:           // Shift mode
            case Event.EV_BACK_BUTTON:        // Back
              _state = State.STATE_BACK;
              Close();	//Added by JM to close automatically
              break;

            case Event.EV_X_BUTTON:           // Toggle keyboard
              Press(_currentKeyboard == KeyboardTypes.TYPE_SYMBOLS ? Xkey.XK_ALPHABET : Xkey.XK_SYMBOLS);
              break;
            case Event.EV_WHITE_BUTTON:       // Backspace
              Press(Xkey.XK_BACKSPACE);
              break;
            case Event.EV_BLACK_BUTTON:       // Space
              Press(Xkey.XK_SPACE);
              break;
            case Event.EV_LEFT_BUTTON:        // Left
              Press(Xkey.XK_ARROWLEFT);
              break;
            case Event.EV_RIGHT_BUTTON:       // Right
              Press(Xkey.XK_ARROWRIGHT);
              break;

            // Navigation
            case Event.EV_UP: MoveUp(); break;
            case Event.EV_DOWN: MoveDown(); break;
            case Event.EV_LEFT: MoveLeft(); break;
            case Event.EV_RIGHT: MoveRight(); break;
          }
          break;
        default:
          Close();
          break;
      }
    }

    void ChangeKey(int iBoard, int iRow, int iKey, Key newkey)
    {
      ArrayList board = (ArrayList)_keyboardList[iBoard];
      ArrayList row = (ArrayList)board[iRow];
      row[iKey] = newkey;
    }

    void PressCurrent()
    {
      if (_currentKey == -1) return;

      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];

      // Press it
      Press(key.xKey);
    }

    void Press(char k)
    {
      // Don't add more than the maximum characters, and don't allow 
      // text to exceed the width of the text entry field
      if (_textEntered.Length < MAX_CHARS)
      {
        float fWidth = 0, fHeight = 0;
        _font18.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

        if (fWidth < fTEXTBOX_WIDTH)
        {
          if (_position >= _textEntered.Length)
          {
            _textEntered += k.ToString();
            if (TextChanged != null) TextChanged(_searchKind, _textEntered);
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, k.ToString());
            if (TextChanged != null) TextChanged(_searchKind, _textEntered);
          }
          ++_position; // move the caret
        }
      }

      // Unstick the shift key
      _shiftTurnedOn = false;
    }

    void Press(Xkey xk)
    {

      if (xk == Xkey.XK_NULL) // happens in Japanese keyboard (keyboard type)
        xk = Xkey.XK_SPACE;

      // If the key represents a character, add it to the word
      if (((uint)xk) < 0x10000 && xk != Xkey.XK_ARROWLEFT && xk != Xkey.XK_ARROWRIGHT)
      {
        // Don't add more than the maximum characters, and don't allow 
        // text to exceed the width of the text entry field
        if (_textEntered.Length < MAX_CHARS)
        {
          float fWidth = 0, fHeight = 0;
          _font18.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

          if (fWidth < fTEXTBOX_WIDTH)
          {
            if (_position >= _textEntered.Length)
            {
              _textEntered += GetChar(xk).ToString();
              if (TextChanged != null) TextChanged(_searchKind, _textEntered);
            }
            else
            {
              _textEntered = _textEntered.Insert(_position, GetChar(xk).ToString());
              if (TextChanged != null) TextChanged(_searchKind, _textEntered);
            }
            ++_position; // move the caret
          }
        }

        // Unstick the shift key
        _shiftTurnedOn = false;
      }

        // Special cases
      else switch (xk)
        {
          case Xkey.XK_BACKSPACE:
            if (_position > 0)
            {
              --_position; // move the caret
              _textEntered = _textEntered.Remove(_position, 1);
              if (TextChanged != null) TextChanged(_searchKind, _textEntered);
            }
            break;
          case Xkey.XK_DELETE: // Used for Japanese only
            if (_textEntered.Length > 0)
            {
              _textEntered = _textEntered.Remove(_position, 1);
              if (TextChanged != null) TextChanged(_searchKind, _textEntered);
            }
            break;
          case Xkey.XK_SHIFT:
            _shiftTurnedOn = !_shiftTurnedOn;
            break;
          case Xkey.XK_CAPSLOCK:
            _capsLockTurnedOn = !_capsLockTurnedOn;
            break;
          case Xkey.XK_ALPHABET:
            _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
            break;
          case Xkey.XK_SYMBOLS:
            _currentKeyboard = KeyboardTypes.TYPE_SYMBOLS;
            break;
          case Xkey.XK_ACCENTS:
            _currentKeyboard = KeyboardTypes.TYPE_ACCENTS;
            break;
          case Xkey.XK_ARROWLEFT:
            if (_position > 0)
              --_position;
            break;
          case Xkey.XK_ARROWRIGHT:
            if (_position < _textEntered.Length)
              ++_position;
            break;
          case Xkey.XK_OK:
            Close();
            _pressedEnter = true;
            break;
          // added to the original code VirtualKeyboard.cs
          // by Agree
          // starts here...

          case Xkey.XK_SEARCH_IS:
            _searchKind = (int)SearchKinds.SEARCH_STARTS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_CONTAINS:
            _searchKind = (int)SearchKinds.SEARCH_ENDS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_ENDS_WITH:
            _searchKind = (int)SearchKinds.SEARCH_IS;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_START_WITH:
            _searchKind = (int)SearchKinds.SEARCH_CONTAINS;
            SetSearchKind();
            break;
          // code by Agree ends here
          //
        }
    }

    void SetSearchKind()
    {
      switch (_searchKind)
      {
        case (int)SearchKinds.SEARCH_STARTS_WITH:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_START_WITH, MODEKEY_WIDTH));
          break;

        case (int)SearchKinds.SEARCH_ENDS_WITH:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_ENDS_WITH, MODEKEY_WIDTH));
          break;

        case (int)SearchKinds.SEARCH_IS:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_IS, MODEKEY_WIDTH));
          break;

        case (int)SearchKinds.SEARCH_CONTAINS:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH));
          break;
      }
      if (TextChanged != null) TextChanged(_searchKind, _textEntered);
    }

    void MoveUp()
    {
      if (_currentKey == -1) _currentKey = _lastColumn;

      do
      {
        // Update key index for special cases
        switch (_currentRow)
        {
          case 0:
            if (1 < _currentKey && _currentKey < 7)      // 2 - 6
            {
              _lastColumn = _currentKey;             // remember column
              _currentKey = 1;                         // move to spacebar
            }
            else if (6 < _currentKey && _currentKey < 9) // 7 - 8
            {
              _lastColumn = _currentKey;             // remember column
              _currentKey = 2;                         // move to left arrow
            }
            else if (_currentKey > 8)                   // 9 - 0
            {
              _lastColumn = _currentKey;             // remember column
              _currentKey = 3;                         // move to right arrow
            }
            break;
          case 3:
            if (_currentKey == 7)                       // backspace
              _currentKey = Math.Max(7, _lastColumn);   // restore column
            break;
          case 4:
            if (_currentKey == 1)                       // spacebar
              _currentKey = Math.Min(6, _lastColumn);   // restore column
            else if (_currentKey > 1)                   // left and right
              _currentKey = 7;                         // backspace
            break;
        }

        // Update row
        _currentRow = (_currentRow == 0) ? _maxRows - 1 : _currentRow - 1;

      } while (IsKeyDisabled());
    }

    void MoveDown()
    {
      if (_currentKey == -1) _currentKey = _lastColumn;

      do
      {
        // Update key index for special cases
        switch (_currentRow)
        {
          case 2:
            if (_currentKey > 7)                    // q - t
            {
              _lastColumn = _currentKey;         // remember column
              _currentKey = 7;                     // move to backspace
            }
            break;
          case 3:
            if (0 < _currentKey && _currentKey < 7)  // u - z
            {
              _lastColumn = _currentKey;         // remember column
              _currentKey = 1;                     // move to spacebar
            }
            else if (_currentKey > 6)               // backspace
            {
              if (_lastColumn > 8)
                _currentKey = 3;                 // move to right arrow
              else
                _currentKey = 2;                 // move to left arrow
            }
            break;
          case 4:
            switch (_currentKey)
            {
              case 1:                             // spacebar
                _currentKey = Math.Min(6, _lastColumn);
                break;
              case 2:                             // left arrow
                _currentKey = Math.Max(Math.Min(8, _lastColumn), 7);
                break;
              case 3:                             // right arrow
                _currentKey = Math.Max(9, _lastColumn);
                break;
            }
            break;
        }

        // Update row
        _currentRow = (_currentRow == _maxRows - 1) ? 0 : _currentRow + 1;

      } while (IsKeyDisabled());
    }

    void MoveLeft()
    {
      if (_currentKey == -1) _currentKey = _lastColumn;

      do
      {
        if (_currentKey <= 0)
        {
          ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
          ArrayList row = (ArrayList)board[_currentRow];
          _currentKey = row.Count - 1;

        }
        else
          --_currentKey;

      } while (IsKeyDisabled());

      SetLastColumn();
    }

    void MoveRight()
    {
      if (_currentKey == -1) _currentKey = _lastColumn;

      do
      {
        ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
        ArrayList row = (ArrayList)board[_currentRow];

        if (_currentKey == row.Count - 1)
          _currentKey = 0;
        else
          ++_currentKey;

      } while (IsKeyDisabled());

      SetLastColumn();
    }

    void SetLastColumn()
    {
      if (_currentKey == -1) return;

      // If the new key is a single character, remember it for later
      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];
      if (key.name == "")
      {
        switch (key.xKey)
        {
          // Adjust the last column for the arrow keys to confine it
          // within the range of the key width
          case Xkey.XK_ARROWLEFT:
            _lastColumn = (_lastColumn <= 7) ? 7 : 8; break;
          case Xkey.XK_ARROWRIGHT:
            _lastColumn = (_lastColumn <= 9) ? 9 : 10; break;

          // Single char, non-arrow
          default:
            _lastColumn = _currentKey; break;
        }
      }
    }

    bool IsKeyDisabled()
    {
      if (_currentKey == -1) return true;

      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];

      // On the symbols keyboard, Shift and Caps Lock are disabled
      if (_currentKeyboard == KeyboardTypes.TYPE_SYMBOLS)
      {
        if (key.xKey == Xkey.XK_SHIFT || key.xKey == Xkey.XK_CAPSLOCK)
          return true;
      }
      return false;
    }

    char GetChar(Xkey xk)
    {
      // Handle case conversion
      char wc = (char)(((uint)xk) & 0xffff);

      if ((_capsLockTurnedOn && !_shiftTurnedOn) || (!_capsLockTurnedOn && _shiftTurnedOn))
        wc = Char.ToUpper(wc);
      else
        wc = Char.ToLower(wc);

      return wc;
    }

    void RenderKey(float fX, float fY, Key key, long keyColor, long textColor)
    {
      if (keyColor == COLOR_INVISIBLE || key.xKey == Xkey.XK_NULL) return;


      string strKey = GetChar(key.xKey).ToString();
      string name = (key.name.Length == 0) ? strKey : key.name;

      int width = key.dwWidth - KEY_INSET + 2;
      int height = (int)(KEY_INSET + 2);
      GUIGraphicsContext.ScaleHorizontal(ref width);
      GUIGraphicsContext.ScaleVertical(ref height);

      float x = fX + KEY_INSET;
      float y = fY + KEY_INSET;
      float z = fX + width;//z
      float w = fY + _keyHeight - height;//w

      float nw = width;
      float nh = _keyHeight - height;

      float uoffs = 0;
      float v = 1.0f;
      float u = 1.0f;

      _keyTexture.Draw(x, y, nw, nh, uoffs, 0.0f, u, v, (int)keyColor);

      // Draw the key text. If key name is, use a slightly smaller font.
      float textWidth = 0;
      float textHeight = 0;
      float positionX = (x + z) / 2.0f;
      float positionY = (y + w) / 2.0f;
      positionX -= GUIGraphicsContext.OffsetX;
      positionY -= GUIGraphicsContext.OffsetY;
      if (key.name.Length > 1 && Char.IsUpper(key.name[1]))
      {
        _font12.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth / 2);
        positionY -= (textHeight / 2);
        _font12.DrawText(positionX, positionY, textColor, name, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
      else
      {
        _font18.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth / 2);
        positionY -= (textHeight / 2);
        _font18.DrawText(positionX, positionY, textColor, name, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
    }

    void DrawTextBox(float timePassed, int x1, int y1, int x2, int y2)
    {
      //long lColor=0xaaffffff;

      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);

      x1 += GUIGraphicsContext.OffsetX;
      x2 += GUIGraphicsContext.OffsetX;
      y1 += GUIGraphicsContext.OffsetY;
      y2 += GUIGraphicsContext.OffsetY;
      /*
            Rectangle[] rect = new Rectangle[1];
            rect[0].X=x1;
            rect[0].Y=y1;
            rect[0].Width=x2-x1;
            rect[0].Height=y2-y1;
            GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target|ClearFlags.Target, (int)lColor, 1.0f, 0, rect );
      */
      //image.ColourDiffuse=lColor;
      image.SetPosition(x1, y1);
      image.Width = (x2 - x1);
      image.Height = (y2 - y1);
      image.Render(timePassed);


    }

    void DrawText(int x, int y)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
      x += GUIGraphicsContext.OffsetX;
      y += GUIGraphicsContext.OffsetY;
      string textLine = _textEntered;
      if (_password)
      {
        textLine = "";
        for (int i = 0; i < _textEntered.Length; ++i) textLine += "*";
      }

      _fontSearchText.DrawText((float)x, (float)y, COLOR_SEARCHTEXT, textLine, GUIControl.Alignment.ALIGN_LEFT, -1);


      // Draw blinking caret using line primitives.
      TimeSpan ts = DateTime.Now - _caretTimer;
      if ((ts.TotalSeconds % fCARET_BLINK_RATE) < fCARET_ON_RATIO)
      {
        string line = textLine.Substring(0, _position);

        float caretWidth = 0.0f;
        float caretHeight = 0.0f;
        _fontSearchText.GetTextExtent(line, ref caretWidth, ref caretHeight);
        x += (int)caretWidth;
        _fontSearchText.DrawText((float)x, (float)y, 0xff202020, "|", GUIControl.Alignment.ALIGN_LEFT, -1);

      }
    }

    void RenderKeyboardLatin(float timePassed)
    {
      // Show text and caret
      DrawTextBox(timePassed, 64, 208, 576, 248);
      DrawText(68, 208);


      int x1 = 64;
      int y1 = 250;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      x1 += GUIGraphicsContext.OffsetX;
      y1 += GUIGraphicsContext.OffsetY;
      // Draw each row
      float fY = y1;
      ArrayList keyBoard = (ArrayList)_keyboardList[(int)_currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += _keyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList)keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          // Determine key name
          Key key = (Key)keyRow[i];
          long selKeyColor = 0xffffffff;
          long selTextColor = COLOR_NORMAL;

          // Handle special key coloring
          switch (key.xKey)
          {
            case Xkey.XK_SHIFT:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_shiftTurnedOn)
                    selKeyColor = COLOR_PRESSED;
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
            case Xkey.XK_CAPSLOCK:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_capsLockTurnedOn)
                    selKeyColor = COLOR_PRESSED;
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
           /* case Xkey.XK_ACCENTS:
              selKeyColor = COLOR_INVISIBLE;
              selTextColor = COLOR_INVISIBLE;
              break;*/
          }

          // Highlight the current key
          if (row == _currentRow && dwIndex == _currentKey)
            selKeyColor = COLOR_HIGHLIGHT;

          RenderKey(fX + fWidthSum, fY, key, selKeyColor, selTextColor);

          int width = key.dwWidth;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
            width = GAP2_WIDTH;
          else
            width = GAP_WIDTH;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          ++dwIndex;
        }
      }
    }


    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {

      Render(timePassed);
    }
    #endregion

  }
}
