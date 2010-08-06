﻿#region Copyright (C) 2005-2010 Team MediaPortal

/* 
 *	Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Provides a "cover flow" list navigation control.
  /// </summary>
  public class GUICoverFlow : GUIControl
  {
    #region Skin variables

    [XMLSkinElement("selectedCard")] protected int _selectedCard = 0;
    [XMLSkinElement("cardWidth")] protected int _cardWidth = 64;
    [XMLSkinElement("cardHeight")] protected int _cardHeight = 64;
    [XMLSkinElement("angle")] protected float _angle = 30.0f;
    [XMLSkinElement("sideShift")] protected float _sideShift = 75.0f;
    [XMLSkinElement("sideGap")] protected float _sideGap = 20.0f;
    [XMLSkinElement("sideDepth")] protected float _sideDepth = 30.0f;
    [XMLSkinElement("offsetY")] protected float _offsetY = 0.0f;
    [XMLSkinElement("selectedOffsetY")] protected float _selectedYOffset = 0.0f;
    [XMLSkinElement("speed")] protected float _speed = 8.0f;
    [XMLSkinElement("backgroundHeight")] protected int _backgroundHeight;
    [XMLSkinElement("backgroundWidth")] protected int _backgroundWidth;
    [XMLSkinElement("backgroundX")] protected int _backgroundPositionX;
    [XMLSkinElement("backgroundY")] protected int _backgroundPositionY;
    [XMLSkinElement("backgroundDiffuse")] protected int _backgroundDiffuseColor;
    [XMLSkinElement("background")] protected string _backgroundTextureName;
    [XMLSkinElement("showBackground")] protected bool _showBackground = false;
    [XMLSkinElement("foregroundHeight")] protected int _foregroundHeight;
    [XMLSkinElement("foregroundWidth")] protected int _foregroundWidth;
    [XMLSkinElement("foregroundX")] protected int _foregroundPositionX;
    [XMLSkinElement("foregroundY")] protected int _foregroundPositionY;
    [XMLSkinElement("foregroundDiffuse")] protected int _foregroundDiffuseColor;
    [XMLSkinElement("foreground")] protected string _foregroundTextureName;
    [XMLSkinElement("showForeground")] protected bool _showForeground = false;
    [XMLSkinElement("showFrame")] protected bool _showFrame = true;
    [XMLSkinElement("frame")] protected string _frameName = "";
    [XMLSkinElement("frameFocus")] protected string _frameFocusName = "";
    [XMLSkinElement("frameWidth")] protected int _frameWidth = 64;
    [XMLSkinElement("frameHeight")] protected int _frameHeight = 64;
    [XMLSkinElement("spinSpeed")] protected float _spinSpeed = 8.0f;
    [XMLSkinElement("unfocusedAlpha")] protected int _unfocusedAlpha = 0xFF;
    [XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";
    [XMLSkinElement("font1")] protected string _fontName1 = "";
    [XMLSkinElement("font2")] protected string _fontName2 = "";
    [XMLSkinElement("label1")] protected string _labelText1 = "#title";
    [XMLSkinElement("label2")] protected string _labelText2 = "";
    [XMLSkinElement("textColor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("remoteColor")] protected long _remoteColor = 0xFFFF0000;
    [XMLSkinElement("playedColor")] protected long _playedColor = 0xFFA0D0FF;
    [XMLSkinElement("downloadColor")] protected long _downloadColor = 0xFF00FF00;
    [XMLSkinElement("selectedColor")] protected long _selectedColor = 0xFFFFFFFF;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("label1YOff")] protected int _label1OffsetY = 0;
    [XMLSkinElement("label2YOff")] protected int _label2OffsetY = 0;
    [XMLSkinElement("pageSize")] protected int _pageSize = 10;
    [XMLSkinElement("scrollbarBackground")] protected string _scrollbarBackgroundTextureName = "";
    [XMLSkinElement("scrollbarLeft")] protected string _scrollbarLeftTextureName = "";
    [XMLSkinElement("scrollbarRight")] protected string _scrollbarRightTextureName = "";
    [XMLSkinElement("scrollbarYOff")] protected int _scrollbarOffsetY = 0;
    [XMLSkinElement("scrollbarWidth")] protected int _scrollbarWidth = 400;
    [XMLSkinElement("scrollbarHeight")] protected int _scrollbarHeight = 15;
    [XMLSkinElement("showScrollbar")] protected bool _showScrollbar = true;
    [XMLSkin("cards", "flipY")] protected bool _flipY = false;
    [XMLSkin("cards", "diffuse")] protected string _diffuseFilename = "";
    [XMLSkin("cards", "mask")] protected string _maskFilename = "";

    #endregion

    #region Member variables

    // Defines the kind of search.
    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    };

    // Defines the directions in which the cover flow can move.
    private enum FlowDirection
    {
      LEFT,
      RIGHT
    };

    // Define the controls
    private List<GUIListItem> _listItems = new List<GUIListItem>();
    private List<GUIAnimation> _frame = new List<GUIAnimation>();
    private List<GUIAnimation> _frameFocus = new List<GUIAnimation>();
    private GUILabelControl _label1 = null;
    private GUILabelControl _label2 = null;
    private GUIFont _font1 = null;
    private GUIFont _font2 = null;
    private GUIAnimation _imageBackground;
    private GUIAnimation _imageForeground;
    private GUIHorizontalScrollbar _horizontalScrollbar = null;

    // Create a collection to maintain the controls for the back of the card.
    private System.Windows.UIElementCollection _cardBackControls = new System.Windows.UIElementCollection();

    // Cover flow status
    private float _position = 0.0f;
    private FlowDirection _direction = FlowDirection.RIGHT;

    // Card spinning
    float _selectedSpinAngle = 0.0f;
    float _spinAnglePosition = 0.0f;
    Action _queuedAction = null;

    // Search            
    private DateTime _timerKey = DateTime.Now;
    private char _currentKey = (char)0;
    private char _previousKey = (char)0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;

    #endregion

    #region Construction

    public GUICoverFlow(int dwParentID)
      : base(dwParentID) {}

    #endregion Construction

    #region Overrides

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      _font1 = GUIFontManager.GetFont(_fontName1);
      _font2 = GUIFontManager.GetFont(_fontName2);

      // Create the background.
      _imageBackground = LoadAnimationControl(_controlId, 0, _backgroundPositionX, _backgroundPositionY,
                                              _backgroundWidth, _backgroundHeight, _backgroundTextureName);
      _imageBackground.ParentControl = this;
      _imageBackground.DimColor = DimColor;

      // Create the foreground.
      _imageForeground = LoadAnimationControl(_controlId, 0, _foregroundPositionX, _foregroundPositionY,
                                              _foregroundWidth, _foregroundHeight, _foregroundTextureName);
      _imageForeground.ParentControl = this;
      _imageForeground.DimColor = DimColor;

      // Create the card labels.
      int y = _positionY + _label1OffsetY;
      _label1 = new GUILabelControl(_controlId, 0,
                                        0, y,
                                        Width, 0,
                                        _fontName1, "", 0x0,
                                        Alignment.ALIGN_CENTER, VAlignment.ALIGN_TOP,
                                        false,
                                        _shadowAngle, _shadowDistance, _shadowColor);

      y = _positionY + _label2OffsetY;
      _label2 = new GUILabelControl(_controlId, 0,
                                       0, y,
                                       Width, 0,
                                       _fontName2, "", 0x0,
                                       Alignment.ALIGN_CENTER, VAlignment.ALIGN_TOP,
                                       false,
                                       _shadowAngle, _shadowDistance, _shadowColor);

      // Create the horizontal scrollbar.
      int scrollbarWidth = _scrollbarWidth;
      int scrollbarHeight = _scrollbarHeight;
      GUIGraphicsContext.ScaleHorizontal(ref scrollbarWidth);
      GUIGraphicsContext.ScaleVertical(ref scrollbarHeight);
      int scrollbarPosX = _positionX + (_width / 2) - (_scrollbarWidth / 2);

      _horizontalScrollbar = new GUIHorizontalScrollbar(_controlId, 0,
                                                        scrollbarPosX, _positionY + _scrollbarOffsetY,
                                                        _scrollbarWidth, _scrollbarHeight,
                                                        _scrollbarBackgroundTextureName, _scrollbarLeftTextureName,
                                                        _scrollbarRightTextureName);
      _horizontalScrollbar.ParentControl = this;
      _horizontalScrollbar.DimColor = DimColor;

      // Create controls for the back of the selected card.  All of the controls are provided as a single subitem.
      Hashtable defines = new Hashtable();  // An empty set of defines.
      XmlDocument doc = new XmlDocument();

      doc.LoadXml((string)GetSubItem(0));
      XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/controls/*");

      foreach (XmlNode node in nodeList)
      {
        try
        {
          GUIControl newControl = GUIControlFactory.Create(_windowId, node, defines, null);
          _cardBackControls.Add(newControl);
        }
        catch (Exception ex)
        {
          Log.Error("GUICoverFlow: Unable to load control. exception:{0}", ex.ToString());
        }
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();

      if (_imageBackground != null)
      {
        _imageBackground.PreAllocResources();
      }

      if (_imageForeground != null)
      {
        _imageForeground.PreAllocResources();
      }

      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.PreAllocResources();
      }
    }

    public override void AllocResources()
    {
      base.AllocResources();

      if (_imageBackground != null)
      {
        _imageBackground.AllocResources();
      }

      if (_imageForeground != null)
      {
        _imageForeground.AllocResources();
      }

      if (_label1 != null)
      {
        _label1.AllocResources();
      }

      if (_label2 != null)
      {
        _label2.AllocResources();
      }

      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.AllocResources();
      }

      foreach (GUIControl control in _cardBackControls)
      {
        control.AllocResources();
      }
    }

    public override void Dispose()
    {
      foreach (GUIListItem item in _listItems)
      {
        item.FreeMemory();
      }
      _listItems.Clear();

      if (_imageBackground != null)
      {
        _imageBackground.FreeResources();
      }

      if (_imageForeground != null)
      {
        _imageForeground.FreeResources();
      }

      if (_label1 != null)
      {
        _label1.FreeResources();
      }

      if (_label2 != null)
      {
        _label2.FreeResources();
      }

      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.FreeResources();
      }

      foreach (GUIControl control in _cardBackControls)
      {
        control.FreeResources();
      }

      base.Dispose();
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _backgroundPositionX, ref _backgroundPositionY,
                                                     ref _backgroundWidth, ref _backgroundHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _foregroundPositionX, ref _foregroundPositionY,
                                                     ref _foregroundWidth, ref _foregroundHeight);
      GUIGraphicsContext.ScaleHorizontal(ref _cardWidth);
      GUIGraphicsContext.ScaleVertical(ref _cardHeight);
      GUIGraphicsContext.ScaleHorizontal(ref _frameWidth);
      GUIGraphicsContext.ScaleVertical(ref _frameHeight);
      GUIGraphicsContext.ScaleHorizontal(ref _sideGap);
      GUIGraphicsContext.ScaleHorizontal(ref _sideShift);
      GUIGraphicsContext.ScaleHorizontal(ref _sideDepth);
      GUIGraphicsContext.ScaleVertical(ref _offsetY);
      GUIGraphicsContext.ScaleVertical(ref _selectedYOffset);
      GUIGraphicsContext.ScaleVertical(ref _scrollbarOffsetY);
      GUIGraphicsContext.ScaleVertical(ref _label1OffsetY);
      GUIGraphicsContext.ScaleVertical(ref _label2OffsetY);
    }

    public override void Animate(float timePassed, Animator animator)
    {
      if (animator == null)
      {
        return;
      }

      if (_imageBackground != null)
      {
        _imageBackground.Animate(timePassed, animator);
      }

      if (_imageForeground != null)
      {
        _imageForeground.Animate(timePassed, animator);
      }

      base.Animate(timePassed, animator);
    }

    public override void OnAction(Action action)
    {
      // If the selected card is spinning then throw away the action.
      if (CardIsSpinning())
      {
        return;
      }

      // If the card is spun around then unspin the card before executing the action.  The action is queued to execute after
      // the card has been unspun.
      if (CardIsSpun())
      {
        // Don't queue invalid actions or actions that are designed to spin the card in the first place.
        if (action.wID != Action.ActionType.ACTION_SHOW_INFO &&
            action.wID != Action.ActionType.ACTION_INVALID)
        {
          QueueAction(action);
        }

        UnspinCard();
        return;
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          {
            int iItem = Math.Min(SelectedListItemIndex + _pageSize, _listItems.Count - 1);
            SelectCardIndex(iItem);
          }
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          {
            int iItem = Math.Max(SelectedListItemIndex - _pageSize, 0);
            SelectCardIndex(iItem);
          }
          break;

        case Action.ActionType.ACTION_HOME:
          {
            SelectCardIndexNow(0);
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            SelectCardIndexNow(_listItems.Count - 1);
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            ResetSearchString();
            base.OnAction(action);
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            ResetSearchString();
            base.OnAction(action);
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (SelectedListItemIndex == FirstCardIndex)
            {
              base.OnAction(action);
            }
            SelectCard(_selectedCard - 1);
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (SelectedListItemIndex == LastCardIndex)
            {
              base.OnAction(action);
            }
            SelectCard(_selectedCard + 1);
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if (action.m_key != null)
            {
              // Check key
              if (((action.m_key.KeyChar >= '0') && (action.m_key.KeyChar <= '9')) ||
                  action.m_key.KeyChar == '*' || action.m_key.KeyChar == '(' || action.m_key.KeyChar == '#' ||
                  action.m_key.KeyChar == '§')
              {
                Press((char)action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int)Keys.Back)
              {
                if (_searchString.Length > 0)
                {
                  _searchString = _searchString.Remove(_searchString.Length - 1, 1);
                }
                SearchItem(_searchString, SearchType.SEARCH_FIRST);
              }

              if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) ||
                  (action.m_key.KeyChar == (int)Keys.Space))
              {
                if (action.m_key.KeyChar == (int)Keys.Space && _searchString == string.Empty)
                {
                  return;
                }
                _searchString += (char)action.m_key.KeyChar;
                SearchItem(_searchString, SearchType.SEARCH_FIRST);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            int id;
            bool focus;
            if (_horizontalScrollbar != null)
            {
              if (_horizontalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
              {
                _horizontalScrollbar.OnAction(action);
                int index = (int)((_horizontalScrollbar.Percentage / 100.0f) * _listItems.Count);
                SelectCardIndex(index);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_SHOW_INFO:
          {
            NotifyCardToSpin();
          }
          break;

        default:
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                            (int)action.wID, 0, null);
            GUIGraphicsContext.SendMessage(msg);
            ResetSearchString();
          }
          break;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          message.Object = SelectedListItem;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in _listItems)
          {
            item.FreeMemory();
          }
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          message.Object = GetCard(iItem);
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          if (Disabled || !IsVisible || !CanFocus())
          {
            base.OnMessage(message);
            return true;
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem newItem = message.Object as GUIListItem;
          if (newItem != null)
          {
            Add(newItem);
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = _listItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = SelectedListItemIndex;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          int iItem = message.Param1;
          SelectCardIndex(iItem);
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase))
          {
            SelectCard(item);
            break;
          }
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.IsRemote)
          {
            if (message.Label == item.Path)
            {
              item.IsDownloading = true;
            }
          }
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.IsRemote)
          {
            if (message.Label == item.Path)
            {
              item.Path = message.Label2;
              item.IsRemote = false;
              item.IsDownloading = false;
            }
          }
        }
      }

      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="SearchKey"></param>
    /// <param name="iSearchMethode"></param>
    private void SearchItem(string SearchKey, SearchType iSearchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = SelectedListItemIndex;

      if (iSearchMethode == SearchType.SEARCH_FIRST)
      {
        iCurrentItem = 0;
      }

      int iItem = iCurrentItem;
      do
      {
        if (iSearchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= _listItems.Count)
          {
            iItem = 0;
          }
        }
        if (iSearchMethode == SearchType.SEARCH_PREV && _listItems.Count > 0)
        {
          iItem--;
          if (iItem < 0)
          {
            iItem = _listItems.Count - 1;
          }
        }

        GUIListItem pItem = _listItems[iItem];

        if (pItem.Label.ToUpper().StartsWith(SearchKey.ToUpper()) == true)
        {
          bItemFound = true;
          break;
        }

        if (iSearchMethode == SearchType.SEARCH_FIRST)
        {
          iItem++;
          if (iItem >= _listItems.Count)
          {
            iItem = 0;
          }
        }
      } while (iItem != iCurrentItem);

      _lastSearchItem = iItem;

      if ((bItemFound) && (iItem >= 0 && iItem < _listItems.Count))
      {
        SelectCardIndex(iItem);
      }
      UpdateProperties();
    }

    /// <summary>
    /// Handle keypress events for SMS style search (key '1'..'9')
    /// </summary>
    /// <param name="Key"></param>
    private void Press(char Key)
    {
      // Check key timeout
      CheckTimer();

      // Check different key pressed
      if ((Key != _previousKey) && (Key >= '1' && Key <= '9'))
      {
        _currentKey = (char)0;
      }

      if (Key == '*' || Key == '(')
      {
        // Backspace
        if (_searchString.Length > 0)
        {
          _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        }
        _previousKey = (char)0;
        _currentKey = (char)0;
        _timerKey = DateTime.Now;
      }
      if (Key == '#' || Key == '§')
      {
        _timerKey = DateTime.Now;
      }
      else if (Key == '1')
      {
        if (_currentKey == 0)
        {
          _currentKey = ' ';
        }
        else if (_currentKey == ' ')
        {
          _currentKey = '!';
        }
        else if (_currentKey == '!')
        {
          _currentKey = '?';
        }
        else if (_currentKey == '?')
        {
          _currentKey = '.';
        }
        else if (_currentKey == '.')
        {
          _currentKey = '0';
        }
        else if (_currentKey == '0')
        {
          _currentKey = '1';
        }
        else if (_currentKey == '1')
        {
          _currentKey = '-';
        }
        else if (_currentKey == '-')
        {
          _currentKey = '+';
        }
        else if (_currentKey == '+')
        {
          _currentKey = ' ';
        }
      }
      else if (Key == '2')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'a';
        }
        else if (_currentKey == 'a')
        {
          _currentKey = 'b';
        }
        else if (_currentKey == 'b')
        {
          _currentKey = 'c';
        }
        else if (_currentKey == 'c')
        {
          _currentKey = '2';
        }
        else if (_currentKey == '2')
        {
          _currentKey = 'a';
        }
      }
      else if (Key == '3')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'd';
        }
        else if (_currentKey == 'd')
        {
          _currentKey = 'e';
        }
        else if (_currentKey == 'e')
        {
          _currentKey = 'f';
        }
        else if (_currentKey == 'f')
        {
          _currentKey = '3';
        }
        else if (_currentKey == '3')
        {
          _currentKey = 'd';
        }
      }
      else if (Key == '4')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'g';
        }
        else if (_currentKey == 'g')
        {
          _currentKey = 'h';
        }
        else if (_currentKey == 'h')
        {
          _currentKey = 'i';
        }
        else if (_currentKey == 'i')
        {
          _currentKey = '4';
        }
        else if (_currentKey == '4')
        {
          _currentKey = 'g';
        }
      }
      else if (Key == '5')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'j';
        }
        else if (_currentKey == 'j')
        {
          _currentKey = 'k';
        }
        else if (_currentKey == 'k')
        {
          _currentKey = 'l';
        }
        else if (_currentKey == 'l')
        {
          _currentKey = '5';
        }
        else if (_currentKey == '5')
        {
          _currentKey = 'j';
        }
      }
      else if (Key == '6')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'm';
        }
        else if (_currentKey == 'm')
        {
          _currentKey = 'n';
        }
        else if (_currentKey == 'n')
        {
          _currentKey = 'o';
        }
        else if (_currentKey == 'o')
        {
          _currentKey = '6';
        }
        else if (_currentKey == '6')
        {
          _currentKey = 'm';
        }
      }
      else if (Key == '7')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'p';
        }
        else if (_currentKey == 'p')
        {
          _currentKey = 'q';
        }
        else if (_currentKey == 'q')
        {
          _currentKey = 'r';
        }
        else if (_currentKey == 'r')
        {
          _currentKey = 's';
        }
        else if (_currentKey == 's')
        {
          _currentKey = '7';
        }
        else if (_currentKey == '7')
        {
          _currentKey = 'p';
        }
      }
      else if (Key == '8')
      {
        if (_currentKey == 0)
        {
          _currentKey = 't';
        }
        else if (_currentKey == 't')
        {
          _currentKey = 'u';
        }
        else if (_currentKey == 'u')
        {
          _currentKey = 'v';
        }
        else if (_currentKey == 'v')
        {
          _currentKey = '8';
        }
        else if (_currentKey == '8')
        {
          _currentKey = 't';
        }
      }
      else if (Key == '9')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'w';
        }
        else if (_currentKey == 'w')
        {
          _currentKey = 'x';
        }
        else if (_currentKey == 'x')
        {
          _currentKey = 'y';
        }
        else if (_currentKey == 'y')
        {
          _currentKey = 'z';
        }
        else if (_currentKey == 'z')
        {
          _currentKey = '9';
        }
        else if (_currentKey == '9')
        {
          _currentKey = 'w';
        }
      }

      if (Key >= '1' && Key <= '9')
      {
        // Check different key pressed
        if (Key == _previousKey)
        {
          if (_searchString.Length > 0)
          {
            _searchString = _searchString.Remove(_searchString.Length - 1, 1);
          }
        }
        _previousKey = Key;
        _searchString += _currentKey;
      }
      SearchItem(_searchString, SearchType.SEARCH_FIRST);
      _timerKey = DateTime.Now;
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 1000)
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
      }
    }

    /// <summary>
    /// Draws a single card.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="index">The index of the card in the entire set.</param>
    /// <param name="shouldFocus">True if the card should be in focus.</param>
    private void RenderCard(float timePassed, int index, bool shouldFocus)
    {
      // The selected card may have its front or back shown.
      if (index == SelectedListItemIndex)
      {
        // If the card spin angle is not yet 90 degrees then render the front of the card.
        if (Math.Abs(_spinAnglePosition) <= 90)
        {
          RenderCardFront(timePassed, index, shouldFocus);
        }
        else
        {
          RenderCardBack(timePassed, index, shouldFocus);
        }
      }
      else
      {
        // All other cards only show their front.
        RenderCardFront(timePassed, index, shouldFocus);
      }
    }

    /// <summary>
    /// Draws the front of a single item of the cover flow.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="index">The index of the card in the entire set.</param>
    /// <param name="shouldFocus">True if the card should be in focus.</param>
    private void RenderCardFront(float timePassed, int index, bool shouldFocus)
    {
      GUIListItem pItem = _listItems[index];
      GUIImage pCard = null;
      bool itemFocused = (shouldFocus == true);
      uint currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;

      if (pItem.HasThumbnail)
      {
        pCard = pItem.Thumbnail;
        if (null == pCard && !IsAnimating)
        {
          pCard = new GUIImage(0, 0,
                               0, 0,
                               _cardWidth, _cardHeight,
                               pItem.ThumbnailImage, 0x0);

          pCard.ParentControl = null; // We want to be able to treat each card as a control.
          pCard.KeepAspectRatio = true;
          pCard.Zoom = !pItem.IsFolder;
          pCard.FlipY = _flipY;
          pCard.DiffuseFileName = _diffuseFilename;
          pCard.MaskFileName = _maskFilename;
          pCard.DimColor = DimColor;
          pCard.AllocResources();

          pItem.Thumbnail = pCard;
        }

        if (null != pCard)
        {
          if (itemFocused)
          {
            pCard.ColourDiffuse = 0xffffffff;
            pCard.Focus = true;
          }
          else
          {
            pCard.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            pCard.Focus = false;
          }

          // Ensure our card is setup as we expect because other views (filmstrip) may have changed these values.
          pCard.KeepAspectRatio = true;
          pCard.Zoom = !pItem.IsFolder;
          pCard.FlipY = _flipY;
          pCard.DiffuseFileName = _diffuseFilename;
          pCard.MaskFileName = _maskFilename;
          pCard.DimColor = DimColor;

          // Center the image horizontally in the card.
          pCard.SetPosition((pCard.Width - pCard.RenderWidth) / 2, 0);

          pCard.UpdateVisibility();
          pCard.Render(timePassed);
        }
      }
      else if (pItem.HasIconBig)
      {
        pCard = pItem.IconBig;
        if (null == pCard && !IsAnimating)
        {
          pCard = new GUIImage(0, 0,
                               0, 0,
                               _cardWidth, _cardHeight,
                               pItem.IconImageBig, 0x0);

          pCard.ParentControl = null; // We want to be able to treat each card as a control.
          pCard.KeepAspectRatio = true;
          pCard.Zoom = !pItem.IsFolder;
          pCard.FlipY = _flipY;
          pCard.DiffuseFileName = _diffuseFilename;
          pCard.MaskFileName = _maskFilename;
          pCard.DimColor = DimColor;
          pCard.AllocResources();

          pItem.IconBig = pCard;
        }
        if (null != pCard)
        {
          if (itemFocused)
          {
            pCard.ColourDiffuse = 0xffffffff;
            pCard.Focus = true;
          }
          else
          {
            pCard.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            pCard.Focus = false;
          }

          // Ensure our card is setup as we expect because other views (filmstrip) may have changed these values.
          pCard.KeepAspectRatio = true;
          pCard.Zoom = !pItem.IsFolder;
          pCard.FlipY = _flipY;
          pCard.DiffuseFileName = _diffuseFilename;
          pCard.MaskFileName = _maskFilename;
          pCard.DimColor = DimColor;

          // Center the image horizontally in the card.
          pCard.SetPosition((pCard.Width - pCard.RenderWidth) / 2, 0);

          pCard.UpdateVisibility();
          pCard.Render(timePassed);
        }
      }

      // Render the card frame.
      if (_showFrame)
      {
        GUIAnimation frame = (itemFocused ? _frameFocus[index] : _frame[index]);
        frame.Focus = itemFocused;
        frame.Width = _frameWidth;
        frame.Height = _frameHeight;
        frame.MaskFileName = _maskFilename;

        // Center the frame over the card.
        int x = Math.Abs(_frameWidth - _cardWidth) / 2;
        int y = Math.Abs(_frameHeight - _cardHeight) / 2;
        if (_frameWidth >= _cardWidth)
        {
          x = -x;
        }
        if (_frameHeight >= _cardHeight)
        {
          y = -y;
        }

        frame.SetPosition(x, y);
        frame.UpdateVisibility();
        frame.Render(timePassed);
      }
    }

    /// <summary>
    /// Draws the back of a single item of the cover flow.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="index">The index of the card in the entire set.</param>
    /// <param name="shouldFocus">True if the card should be in focus.</param>
    /// <param name="position">The index of the card from the edge of the control, 0 is the visible card farthest from the center card; position values increase toward the center card.</param>
    private void RenderCardBack(float timePassed, int index, bool itemFocused)
    {
      // The back of the card must be rendered 180 degrees from the front.
      GUIGraphicsContext.PushMatrix();
      GUIGraphicsContext.RotateY(180.0f, _cardWidth / 2, 0);

      // Render all of the child controls on the back of the card.
      foreach (GUIControl control in _cardBackControls)
      {
        control.UpdateVisibility();
        control.Render(timePassed);
      }

      // Render the card frame.
      if (_showFrame)
      {
        GUIAnimation frame = (itemFocused ? _frameFocus[index] : _frame[index]);
        frame.Focus = itemFocused;
        frame.Width = _frameWidth;
        frame.Height = _frameHeight;
        frame.MaskFileName = _maskFilename;

        // Center the frame over the card.
        int x = Math.Abs(_frameWidth - _cardWidth) / 2;
        int y = Math.Abs(_frameHeight - _cardHeight) / 2;
        if (_frameWidth >= _cardWidth)
        {
          x = -x;
        }
        if (_frameHeight >= _cardHeight)
        {
          y = -y;
        }

        frame.SetPosition(x, y);
        frame.UpdateVisibility();
        frame.Render(timePassed);
      }

      GUIGraphicsContext.PopMatrix();
    }

    private void RenderCardLabel1(float timePassed)
    {
      if (_font1 == null)
      {
        return;
      }

      GUIListItem pItem = SelectedListItem;

      if (_labelText1 == string.Empty)
      {
        return;
      }

      // Set the text color based on the state of the item.
      long dwColor = _textColor;
      if (pItem.Selected)
      {
        dwColor = _selectedColor;
      }

      if (pItem.IsPlayed)
      {
        dwColor = _playedColor;
      }

      dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();

      if (pItem.IsRemote)
      {
        dwColor = _remoteColor;
        if (pItem.IsDownloading)
        {
          dwColor = _downloadColor;
        }
      }

      if (pItem.IsFolder)
      {
        _label1.Label = pItem.Label;
      }
      else
      {
        _label1.Label = _labelText1;
      }

      _label1.TextColor = dwColor;
      _label1.UpdateVisibility();
      _label1.Render(timePassed);
    }

    private void RenderCardLabel2(float timePassed)
    {
      if (_font2 == null)
      {
        return;
      }

      GUIListItem pItem = SelectedListItem;

      if (pItem.IsFolder ||
          _labelText2 == string.Empty)
      {
        return;
      }

      // Set the text color based on the state of the item.
      long dwColor = _textColor;
      if (pItem.Selected)
      {
        dwColor = _selectedColor;
      }

      if (pItem.IsPlayed)
      {
        dwColor = _playedColor;
      }

      dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();

      if (pItem.IsRemote)
      {
        dwColor = _remoteColor;
        if (pItem.IsDownloading)
        {
          dwColor = _downloadColor;
        }
      }

      _label2.Label = _labelText2;
      _label2.TextColor = dwColor;
      _label2.UpdateVisibility();
      _label2.Render(timePassed);
    }

    private void RenderScrollbar(float timePassed)
    {
      if (_listItems.Count > 0 && _showScrollbar)
      {
        if (_horizontalScrollbar != null)
        {
          float fPercent = (float)SelectedListItemIndex / (float)(_listItems.Count - 1) * 100.0f;
          if ((int)fPercent != (int)_horizontalScrollbar.Percentage)
          {
            _horizontalScrollbar.Percentage = fPercent;
          }

          int w = _horizontalScrollbar.Width;
          int h = _horizontalScrollbar.Height;
          GUIGraphicsContext.ScaleHorizontal(ref w);
          GUIGraphicsContext.ScaleVertical(ref h);
          _horizontalScrollbar.Width = w;
          _horizontalScrollbar.Height = h;

          int x = _horizontalScrollbar.XPosition;
          int y = _horizontalScrollbar.YPosition;
          GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
          _horizontalScrollbar.XPosition = x;
          _horizontalScrollbar.YPosition = y;

          // The scrollbar is only rendered when the mouse support is enabled.  Temporarily bypass the global setting to allow
          // the skin to determine whether or not it should be displayed.
          bool ms = GUIGraphicsContext.MouseSupport;
          GUIGraphicsContext.MouseSupport = true;

          _horizontalScrollbar.Render(timePassed);

          GUIGraphicsContext.MouseSupport = ms;
        }
      }
    }

    private bool CardAllowedToSpin()
    {
      // Do not allow the card to spin if there is no back of the card defined.
      return (_cardBackControls.Count > 0) &&
             SelectedListItem.FileInfo != null;
    }

    private void NotifyCardToSpin()
    {
      // Set the card spin angle to 180 degrees.
      _selectedSpinAngle = 180.0f;
    }

    private void TrySpinCard(float timePassed)
    {
      // Don't spin the card if not allowed.
      if (!CardAllowedToSpin())
      {
        return;
      }

      // Calculate the distance between the current spin angle and the selected spin angle (desired angle).
      // The current angle is a continuous value, the selected angle is a discrete value.
      float distance = _selectedSpinAngle - _spinAnglePosition;

      // When the distance between the current angle and the selected angle is small, lock (snap) the current
      // angle into the selected angle.
      if (Math.Abs(distance) < 1.0)
      {
        _spinAnglePosition = _selectedSpinAngle;

        // The card may have just stopped spinning.  If there is a queued user action then execute the action and clear
        // the queued action.
        if (_queuedAction != null)
        {
          Action action = _queuedAction;
          _queuedAction = null;
          OnAction(action);
        }
      }
      else
      {
        // Compute the new spin angle.  The larger the distance the faster the spinning.  As the distance gets
        // smaller the card will spin more slowly as it settles onto the selected angle.
        _spinAnglePosition += distance * timePassed * _spinSpeed;
      }

      // Set the card spin angle for this render cycle.
      int angle = (int)Math.Floor(_spinAnglePosition);
      GUIGraphicsContext.RotateY(angle, 0, 0);
    }

    private bool CardIsSpinning()
    {
      // The card is spinning if its angle is not zero or 180 degrees.
      return (_spinAnglePosition != 0.0f && _spinAnglePosition != 180.0f);
    }

    private bool CardIsSpun()
    {
      // The card is spun around if its angle is 180 degrees.
      return _spinAnglePosition == 180.0f;
    }

    private void UnspinCard()
    {
      // Set the spin angle to unspin the card.
      _selectedSpinAngle = 0.0f;
    }

    private void QueueAction(Action action)
    {
      // Queue the user action.
      _queuedAction = action;
    }

    /// <summary>
    /// Renders cards to the right of the flow, does not render the selected card once animation has stopped.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="card"></param>
    /// <param name="fractional"></param>
    /// <param name="fract"></param>
    /// <param name="maxCount"></param>
    private void RenderCardsRightSide(float timePassed, int card, float fractional, float fract, int maxCount)
    {
      // Draw cards to the right of center.
      int s = card + 1;
      for (int l = s + maxCount, c = maxCount; c >= 0; l--, c--)
      {
        GUIGraphicsContext.PushMatrix();
        {
          // Cards are in motion if fractional and fract are changing.
          // Move all the cards to the left to make room for the new card.
          float shiftx = c * _sideGap + _sideShift - _sideGap * fractional;
          float shifty = _offsetY;
          float shiftz = _sideDepth;
          float angle = -_angle;
          if (l == card + 1)
          {
            angle *= fract;
            shiftx *= fract;
            shifty = (_offsetY * fract + _selectedYOffset * fractional);
            shiftz *= fract;
          }

          // Set the cards local coordinates on the cover flow.
          GUIGraphicsContext.Translate(shiftx, shifty, shiftz);
          GUIGraphicsContext.RotateY(angle, 0.0f, 0.0f);
          GUIGraphicsContext.Translate(-_cardWidth / 2, 0, 0); // Locate the card at it's top center (card origin is top left).
          RenderCard(timePassed, l, false);
        }
        GUIGraphicsContext.PopMatrix();
      }
    }

    /// <summary>
    /// Renders cards to the left of the flow, renders the selected card once animation has stopped.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="card"></param>
    /// <param name="fractional"></param>
    /// <param name="fract"></param>
    /// <param name="maxCount"></param>
    private void RenderCardsLeftSide(float timePassed, int card, float fractional, float fract, int maxCount)
    {
      // Draw cards to the left of center.
      int s = card - maxCount;
      for (int l = s, c = 0; l <= card; l++, c++)
      {
        GUIGraphicsContext.PushMatrix();
        {
          // Cards are in motion if fractional and fract are changing.
          // Move all the cards to the left to make room for the new card.
          float shiftx = (c - maxCount + 1) * _sideGap - _sideShift - _sideGap * fractional;
          float shifty = _offsetY;
          float shiftz = _sideDepth;
          float angle = _angle;
          bool shouldFocus = false;

          // If the image to render is the selected image then adjust the position and angle according to the
          // fractional position.
          if (l == card)
          {
            angle *= fractional;
            shiftx *= fractional;
            shifty = (_offsetY * fractional + _selectedYOffset * fract);
            shiftz *= fractional;
            shouldFocus = true; // The card coming to the front should be focused.
          }

          // Set the cards local coordinates on the cover flow.
          GUIGraphicsContext.Translate(shiftx, shifty, shiftz);
          GUIGraphicsContext.RotateY(angle, 0.0f, 0.0f);

          // Set the card spin (front to back) angle.
          if (l == card)
          {
            TrySpinCard(timePassed);
          }

          GUIGraphicsContext.Translate(-_cardWidth / 2, 0, 0); // Locate the card at it's top center (card origin is top left).
          RenderCard(timePassed, l, shouldFocus);
        }
        GUIGraphicsContext.PopMatrix();
      }
    }

    /// <summary>
    /// Renders cards to the right of the flow, renders the selected card once animation has stopped.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="card"></param>
    /// <param name="fractional"></param>
    /// <param name="fract"></param>
    /// <param name="maxCount"></param>
    private void RenderCardsRightSide2(float timePassed, int card, float fractional, float fract, int maxCount)
    {
      RenderCardsRightSide(timePassed, card, fractional, fract, maxCount);

      // When the cards are not animating, render the top card only.
      if (fractional == 0.0f)
      {
        GUIGraphicsContext.PushMatrix();
        {
          // Cards are in motion if fractional and fract are changing.
          // Move all the cards to the left to make room for the new card.
          float shiftx = 0.0f;
          float shifty = _offsetY;
          float shiftz = 0.0f;
          bool shouldFocus = true;

          // Set the cards local coordinates on the cover flow.
          GUIGraphicsContext.Translate(shiftx, shifty, shiftz);

          // Set the card spin (front to back) angle.
          TrySpinCard(timePassed);

          GUIGraphicsContext.Translate(-_cardWidth / 2, 0, 0); // Locate the card at it's top center (card origin is top left).
          RenderCard(timePassed, card, shouldFocus);
        }
        GUIGraphicsContext.PopMatrix();
      }
    }

    /// <summary>
    /// Renders cards to the left of the flow, does not render the selected card once animation has stopped.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="card"></param>
    /// <param name="fractional"></param>
    /// <param name="fract"></param>
    /// <param name="maxCount"></param>
    private void RenderCardsLeftSide2(float timePassed, int card, float fractional, float fract, int maxCount)
    {
      // Prevent the render loop below from drawing the selected card when animation has stopped (fractional = 0).
      int ca = card;
      if (fractional == 0.0f)
      {
        ca--;
      }

      // Draw cards to the left of center.
      int s = card - maxCount;
      //for (int l = s, c = 0; l <= card; l++, c++)
      for (int l = s, c = 0; l <= ca; l++, c++)
        {
        GUIGraphicsContext.PushMatrix();
        {
          // Cards are in motion if fractional and fract are changing.
          // Move all the cards to the left to make room for the new card.
          float shiftx = (c - maxCount + 1) * _sideGap - _sideShift - _sideGap * fractional;
          float shifty = _offsetY;
          float shiftz = _sideDepth;
          float angle = _angle;
          bool shouldFocus = false;

          // If the image to render is the selected image then adjust the position and angle according to the
          // fractional position.
          if (l == card)
          {
            angle *= fractional;
            shiftx *= fractional;
            shifty = (_offsetY * fractional + _selectedYOffset * fract);
            shiftz *= fractional;
            shouldFocus = true; // The card coming to the front should be focused.
          }

          // Set the cards local coordinates on the cover flow.
          GUIGraphicsContext.Translate(shiftx, shifty, shiftz);
          GUIGraphicsContext.RotateY(angle, 0.0f, 0.0f);
          GUIGraphicsContext.Translate(-_cardWidth / 2, 0, 0); // Locate the card at it's top center (card origin is top left).
          RenderCard(timePassed, l, shouldFocus);
        }
        GUIGraphicsContext.PopMatrix();
      }
    }

    public override void Render(float timePassed)
    {
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }

      // Render the background.
      if (_imageBackground != null && _showBackground)
      {
        _imageBackground.Render(timePassed);
      }

      // Calculate the distance between the current position on the flow and the selected card (desired position).
      // The current position is a continuous value, the selected card is a discrete value.
      float distance = ((float)_selectedCard) - _position;

      // When the distance between the current position and the selected card is small, lock (snap) the current
      // position into the selected card position.
      if (Math.Abs(distance) < 0.001)
      {
        _position = _selectedCard;
      }
      else
      {
        // Compute the new position on the flow.  The larger the distance the faster the flow will move.  As the distance gets
        // smaller the flow will move more slowly as it settles onto the selected card.
        _position += distance * timePassed * _speed;
      }

      // The value of _position is dependant on rendering intervals (timePassed).  Rendering intervals are not guaranteed to
      // be constant; even though typical render intervals should be approx. 50-60fps the actual value could easily be much
      // lower (e.g. 1fps).  When the render interval is large the calculated value of _position may be well outside the bounds
      // of the list (_listItems).  To ensure we never index outside of the list the following clamps the high and low values
      // of _position.
      if (_position > _listItems.Count - 1)
      {
        _position = _listItems.Count - 1;
      }
      else if (_position < 0)
      {
        _position = 0;
      }

      // Render the cover flow.
      GUIGraphicsContext.PushMatrix();
      {
        // Set the location for the origin of the cover flow to the horizontal center of the window.
        GUIGraphicsContext.LoadIdentity();
        GUIGraphicsContext.Translate(Width / 2, YPosition, 0);

        // Calculate the fractional position of the selected card as it animates from the left or right stack.
        // These fractional components are used to adjust the rendered position and angle of the card.
        int card = (int)Math.Floor(_position);
        float fractional = _position - card;
        float fract = 1.0f - fractional;

        // Compute the maximum number of cards that may be draw to the left and right of center.
        //
        // Compute the side gap based on the depth of the cards.  The algorithm computes the side gap based on geometry
        // with a maximum depth of twice the screen width.
        float calcSideGap = _sideGap * (1.0f - _sideDepth / (GUIGraphicsContext.SkinSize.Width * 2.0f));
        int maxcount = (int)((Width / 2) / calcSideGap);
        int size = _listItems.Count;

        // Plus one to ensure that a card emerging from the edge of the control is drawn.
        int maxcountleft = Math.Min(maxcount + 1, Math.Max(0, card));
        int maxcountright = Math.Min(maxcount + 1, size - 2 - card);

        // Choose the rendering method based on which way the flow is moving.  This allows the cards to be rendered properly
        // when the selected card overlaps the cards in the flow.
        if (_direction == FlowDirection.RIGHT)
        {
          RenderCardsRightSide(timePassed, card, fractional, fract, maxcountright);
          RenderCardsLeftSide(timePassed, card, fractional, fract, maxcountleft);
        }
        else
        {
          RenderCardsLeftSide2(timePassed, card, fractional, fract, maxcountleft);
          RenderCardsRightSide2(timePassed, card, fractional, fract, maxcountright);
        }
      }
      GUIGraphicsContext.PopMatrix();

      // Render the foreground.
      if (_imageForeground != null && _showForeground)
      {
        _imageForeground.Render(timePassed);
      }

      // Render the card title and information.
      RenderCardLabel1(timePassed);
      RenderCardLabel2(timePassed);

      // Render the horizontal scrollbar.
      RenderScrollbar(timePassed);

      base.Render(timePassed);
    }

    /// <summary>
    /// Method to store(save) the current control rectangle
    /// </summary>
    public override void StorePosition()
    {
      if (_imageBackground != null)
      {
        _imageBackground.StorePosition();
      }

      if (_imageForeground != null)
      {
        _imageForeground.StorePosition();
      }

      base.StorePosition();
    }

    /// <summary>
    /// Method to restore the saved-current control rectangle
    /// </summary>
    public override void ReStorePosition()
    {
      if (_imageBackground != null)
      {
        _imageBackground.ReStorePosition();
        _imageBackground.GetRect(out _backgroundPositionX, out _backgroundPositionY, out _backgroundWidth,
                                 out _backgroundHeight);
      }

      if (_imageForeground != null)
      {
        _imageForeground.ReStorePosition();
        _imageForeground.GetRect(out _foregroundPositionX, out _foregroundPositionY, out _foregroundWidth,
                                 out _foregroundHeight);
      }

      base.ReStorePosition();
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;

        if (_imageBackground != null)
        {
          _imageBackground.DimColor = value;
        }

        if (_imageForeground != null)
        {
          _imageForeground.DimColor = value;
        }

        foreach (GUIListItem item in _listItems)
        {
          item.DimColor = value;
        }
      }
    }

    #endregion Overrides

    #region Implementation

    public int AddCard(GUIListItem card, GUIListItem beforeCard)
    {
      if (card == null)
      {
        return _listItems.Count - 1;
      }
      if (beforeCard == null)
      {
        _listItems.Add(card);
        OnCardAdded();
        return _listItems.Count - 1;
      }

      for (int i = 0; i < _listItems.Count; i++)
      {
        if (_listItems[i] == beforeCard)
        {
          _listItems.Insert(i, card);
          OnCardAdded();
          return i;
        }
      }

      // If beforeCard is not found then add it to the back.
      _listItems.Add(card);
      OnCardAdded();
      return _listItems.Count - 1;
    }

    public int AddCard(GUIListItem card, int index)
    {
      if (card == null)
      {
        return -1;
      }
      _listItems.Insert(index, card);
      OnCardAdded();
      return index;
    }

    public void DeleteCard(GUIListItem card)
    {
      for (int i = 0; i < _listItems.Count; i++)
      {
        if (_listItems[i] == card)
        {
          _listItems[i].FreeMemory();
          _listItems.RemoveAt(i);
          OnCardDeleted();
          SelectCard(_selectedCard);
          return;
        }
      }
    }

    public void DeleteCard(int index)
    {
      _listItems[index].FreeMemory();
      _listItems.RemoveAt(index);
      OnCardDeleted();
      SelectCard(_selectedCard);
    }

    public void SelectCard(GUIListItem card)
    {
      for (int i = 0; i < _listItems.Count; i++)
      {
        if (_listItems[i] == card)
        {
          SelectCard(i);
          return;
        }
      }
    }

    public void SelectCard(int index)
    {
      if (index == _selectedCard)
      {
        return;
      }

      if (index >= _listItems.Count)
      {
        index = _listItems.Count - 1;
      }

      if (index < 0)
      {
        index = 0;
      }

      // Set the direction in which te flow will move.
      if (index > SelectedListItemIndex)
      {
        _direction = FlowDirection.LEFT;
      }
      else
      {
        _direction = FlowDirection.RIGHT;
      }

      _selectedCard = index;
      OnSelectionChanged();
    }

    private void SelectCardNow(GUIListItem card)
    {
      SelectCard(card);
      _position = _selectedCard;
    }

    private void SelectCardNow(int index)
    {
      SelectCard(index);
      _position = _selectedCard;
    }

    /// <summary>
    /// Called when a new card is added to the flow.
    /// </summary>
    private void OnCardAdded()
    {
      // Add a card frame for the new card.
      // Focus frame.
      GUIAnimation anim = null;
      anim = LoadAnimationControl(0, 0,
                                  0, 0,
                                  _frameWidth, _frameHeight,
                                  _frameName);
      anim.ParentControl = null;
      anim.DimColor = DimColor;
      anim.FlipY = _flipY;
      anim.DiffuseFileName = _diffuseFilename;
      anim.MaskFileName = _maskFilename;
      anim.AllocResources();
      _frame.Add(anim);

      // Add a card frame for the new card.
      // Non-focus frame.
      anim = LoadAnimationControl(0, 0,
                                  0, 0,
                                  _cardWidth, _cardHeight,
                                  _frameFocusName);
      anim.ParentControl = null;
      anim.DimColor = DimColor;
      anim.FlipY = _flipY;
      anim.DiffuseFileName = _diffuseFilename;
      anim.MaskFileName = _maskFilename;
      anim.AllocResources();
      _frameFocus.Add(anim);
    }

    /// <summary>
    /// Called when an existing card is deleted from the flow.
    /// </summary>
    private void OnCardDeleted()
    {
      // Remove the card frame.
      _frame[_frame.Count - 1].FreeResources();
      _frame.RemoveAt(_frame.Count - 1);

      // Remove the card focus frame.
      _frameFocus[_frameFocus.Count - 1].FreeResources();
      _frameFocus.RemoveAt(_frameFocus.Count - 1);
    }

    /// <summary>
    /// Method which is called if the user selected another item in the coverflow.
    /// </summary>
    protected void OnSelectionChanged()
    {
      GUIListItem listitem = SelectedListItem;
      if (listitem != null)
      {
        listitem.ItemSelected(this);
      }

      UpdateProperties();

      if (_lastSearchItem != SelectedListItemIndex)
      {
        ResetSearchString();
      }
    }

    /// <summary>
    /// This method will update the property manager with the properties of the newly selected item.
    /// </summary>
    private void UpdateProperties()
    {
      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      string strIndex = "";
      int item = GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb, ref strIndex);

      if (!GUIWindowManager.IsRouted)
      {
        GUIPropertyManager.SetProperty("#selecteditem", strSelected);
        GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
        GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        GUIPropertyManager.SetProperty("#selectedindex", strIndex);
      }

      if (_searchString.Length > 0)
      {
        GUIPropertyManager.SetProperty("#selecteditem", "{" + _searchString.ToLower() + "}");
      }
    }

    private void ResetSearchString()
    {
      _previousKey = (char)0;
      _currentKey = (char)0;
      _searchString = "";

      UpdateProperties();
    }

    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumbnail, ref string strIndex)
    {
      strLabel = "";
      strLabel2 = "";
      strThumbnail = "";
      strIndex = "";
      int iItem = GetSelectedCardIndex();
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        if (pItem != null)
        {
          strLabel = pItem.Label;
          strLabel2 = pItem.Label2;
          int index = iItem;

          if (_listItems[0].Label != "..")
            index++;
          if (pItem.Label == "..")
            strIndex = "";
          else
            strIndex = index.ToString();

          if (pItem.IsFolder)
          {
            strLabel = String.Format("{0}{1}{2}", _folderPrefix, pItem.Label, _folderSuffix);
          }
          strThumbnail = pItem.ThumbnailImage;
        }
      }
      return iItem;
    }

    public int GetSelectedCardIndex()
    {
      return _selectedCard;
    }

    public GUIListItem GetCard(int index)
    {
      if (index >= 0 && index < _listItems.Count - 1)
      {
        return _listItems[index];
      }

      return null;
    }

    private GUIListItem GetSelectedCard()
    {
      return GetCard(_selectedCard);
    }

    private void SelectCardIndex(int index)
    {
      SelectCard(index);
    }

    private void SelectCardIndexNow(int index)
    {
      SelectCardNow(index);
    }

    public bool ShowBackground
    {
      get { return _showBackground; }
      set
      {
        _showBackground = value;
      }
    }

    public bool ShowForeground
    {
      get { return _showForeground; }
      set
      {
        _showForeground = value;
      }
    }

    public float OffsetY
    {
      get { return _offsetY; }
      set
      {
        _offsetY = value;
      }
    }

    public float SelectedOffsetY
    {
      get { return _selectedYOffset; }
      set
      {
        _selectedYOffset = value;
      }
    }

    public float Angle
    {
      get { return _angle; }
      set
      {
        _angle = value;
      }
    }

    public float SideShift
    {
      get { return _sideShift; }
      set
      {
        _sideShift = value;
      }
    }

    public float SideGap
    {
      get { return _sideGap; }
      set
      {
        _sideGap = value;
      }
    }

    public float SideDepth
    {
      get { return _sideDepth; }
      set
      {
        _sideDepth = value;
      }
    }

    public int BackgroundX
    {
      get { return _backgroundPositionX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backgroundPositionX = value;
        if (_imageBackground != null)
        {
          _imageBackground.XPosition = value;
        }
      }
    }

    public int BackgroundY
    {
      get { return _backgroundPositionY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backgroundPositionY = value;
        if (_imageBackground != null)
        {
          _imageBackground.YPosition = value;
        }
      }
    }

    public int BackgroundWidth
    {
      get { return _backgroundWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backgroundWidth = value;
        if (_imageBackground != null)
        {
          _imageBackground.Width = value;
        }
      }
    }

    public int BackgroundHeight
    {
      get { return _backgroundHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backgroundHeight = value;
        if (_imageBackground != null)
        {
          _imageBackground.Height = value;
        }
      }
    }

    public long BackgroundDiffuse
    {
      get
      {
        if (_imageBackground != null)
        {
          return _imageBackground.ColourDiffuse;
        }
        return 0;
      }
      set
      {
        if (_imageBackground != null)
        {
          _imageBackground.ColourDiffuse = value;
        }
      }
    }

    public string BackgroundFileName
    {
      get
      {
        if (_imageBackground != null)
        {
          return _imageBackground.FileName;
        }
        return string.Empty;
      }
      set
      {
        if (value == null)
        {
          return;
        }
        if (_imageBackground != null)
        {
          _imageBackground.SetFileName(value);
        }
      }
    }

    public int ForegroundX
    {
      get { return _foregroundPositionX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _foregroundPositionX = value;
        if (_imageForeground != null)
        {
          _imageForeground.XPosition = value;
        }
      }
    }

    public int ForegroundY
    {
      get { return _foregroundPositionY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _foregroundPositionY = value;
        if (_imageForeground != null)
        {
          _imageForeground.YPosition = value;
        }
      }
    }

    public int ForegroundWidth
    {
      get { return _foregroundWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _foregroundWidth = value;
        if (_imageForeground != null)
        {
          _imageForeground.Width = value;
        }
      }
    }

    public int ForegroundHeight
    {
      get { return _foregroundHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _foregroundHeight = value;
        if (_imageForeground != null)
        {
          _imageForeground.Height = value;
        }
      }
    }

    public long ForegroundDiffuse
    {
      get
      {
        if (_imageForeground != null)
        {
          return _imageForeground.ColourDiffuse;
        }
        return 0;
      }
      set
      {
        if (_imageForeground != null)
        {
          _imageForeground.ColourDiffuse = value;
        }
      }
    }

    public string ForegroundFileName
    {
      get
      {
        if (_imageForeground != null)
        {
          return _imageForeground.FileName;
        }
        return string.Empty;
      }
      set
      {
        if (value == null)
        {
          return;
        }
        if (_imageForeground != null)
        {
          _imageForeground.SetFileName(value);
        }
      }
    }

    #endregion Implementation

    #region GUIFacadeView Interface

    public void Sort(IComparer<GUIListItem> comparer)
    {
      try
      {
        _listItems.Sort(comparer);
      }
      catch (Exception) {}
    }

    public void Add(GUIListItem item)
    {
      AddCard(item, null);
    }

    public void Insert(int index, GUIListItem card)
    {
      AddCard(card, index);
    }

    public void Clear()
    {
      _listItems.Clear();
      OnSelectionChanged();
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= _listItems.Count)
        {
          return null;
        }
        return _listItems[index];
      }
    }

    public int FirstCardIndex
    {
      get
      {
        return 0;
      }
    }

    public int LastCardIndex
    {
      get
      {
        return _listItems.Count - 1;
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = GetSelectedCardIndex();
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          GUIListItem pItem = _listItems[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int Count
    {
      get { return _listItems.Count; }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = GetSelectedCardIndex();
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          return iItem;
        }
        return -1;
      }
    }

    #endregion
  }
}