/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying text.
  /// </summary>
  public class GUILabelControl : GUIControl
  {
    [XMLSkinElement("font")]
    protected string _fontName = "";
    [XMLSkinElement("label")]
    protected string _labelText = "";
    [XMLSkinElement("textcolor")]
    protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("align")]
    Alignment _textAlignment = Alignment.ALIGN_LEFT;

    string _cachedTextLabel = "";
    bool _containsProperty = false;
    int _textwidth = 0;
    int _textheight = 0;
    bool _useFontCache = false;

    GUIFont _font = null;
    bool _useViewPort = true;
    bool _propertyHasChanged = false;
    bool _reCalculate = false;
    /// <summary>
    /// The constructor of the GUILabelControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strFont">The indication of the font of this control.</param>
    /// <param name="strLabel">The text of this control.</param>
    /// <param name="dwTextColor">The color of this control.</param>
    /// <param name="dwTextAlign">The alignment of this control.</param>
    /// <param name="bHasPath">Indicates if the label is containing a path.</param>
    public GUILabelControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strFont, string strLabel, long dwTextColor, GUIControl.Alignment dwTextAlign, bool bHasPath)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _labelText = strLabel;
      _fontName = strFont;
      _textColor = dwTextColor;
      _textAlignment = dwTextAlign;

      FinalizeConstruction();
    }
    public GUILabelControl(int dwParentID)
      : base(dwParentID)
    {
    }
    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_fontName == null) _fontName = String.Empty;
      if (_fontName != "" && _fontName != "-")
        _font = GUIFontManager.GetFont(_fontName);
      GUILocalizeStrings.LocalizeLabel(ref _labelText);
      if (_labelText == null) _labelText = String.Empty;
      if (_labelText.IndexOf("#") >= 0) _containsProperty = true;
      _cachedTextLabel = _labelText;
      _propertyHasChanged = true;
    }

    /// <summary>
    /// Renders the text onscreen.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible
      if (!IsVisible) return;
      if (_containsProperty && _propertyHasChanged)
      {
        _propertyHasChanged = false;
        string newLabel = GUIPropertyManager.Parse(_labelText);
        if (_cachedTextLabel != newLabel)
        {
          if (newLabel == null) newLabel = "";
          _cachedTextLabel = newLabel;
          _textwidth = 0;
          _textheight = 0;
          _reCalculate = true;
        }
      }

      if (_reCalculate)
      {
        _reCalculate = false;
        ClearFontCache();
      }

      if (_cachedTextLabel == null) return;
      if (_cachedTextLabel.Length == 0) return;

      long color = _textColor;
      if (Dimmed)
        color &= (DimColor);

      if (null != _font)
      {
        if (GUIGraphicsContext.graphics != null)
        {
          if (_width > 0)
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            _font.DrawTextWidth(_positionX, _positionY, (int)c, _cachedTextLabel, _width, _textAlignment);
          }
          else
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            _font.DrawText(_positionX, _positionY, (int)c, _cachedTextLabel, _textAlignment, -1);
          }
          return;
        }

        if (_textwidth == 0 || _textheight == 0)
        {
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }

        if (_textAlignment == GUIControl.Alignment.ALIGN_CENTER)
        {
          int xoff = (int)((_width - _textwidth) / 2);
          int yoff = (int)((_height - _textheight) / 2);
          uint c = (uint)color;
          c = GUIGraphicsContext.MergeAlpha(c);

          _font.DrawText((float)_positionX + xoff, (float)_positionY + yoff, (int)c, _cachedTextLabel, GUIControl.Alignment.ALIGN_LEFT, _width);
        }
        else
        {

          if (_textAlignment == GUIControl.Alignment.ALIGN_RIGHT)
          {
            if (_width == 0 || _textwidth < _width)
            {
              
              uint c = (uint)color;
              c = GUIGraphicsContext.MergeAlpha(c);

              _font.DrawText((float)_positionX - _textwidth, (float)_positionY, (int)c, _cachedTextLabel, GUIControl.Alignment.ALIGN_LEFT, -1);
            }
            else
            {/*
              float fPosCX = (float)_positionX;
              float fPosCY = (float)_positionY;
              GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
              if (fPosCX < 0) fPosCX = 0.0f;
              if (fPosCY < 0) fPosCY = 0.0f;
              if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
              float heighteight = 60.0f;
              if (heighteight + fPosCY >= GUIGraphicsContext.Height)
                heighteight = GUIGraphicsContext.Height - fPosCY - 1;
              if (heighteight <= 0) return;

              float fwidth = _width - 5.0f;

              if (fPosCX <= 0) fPosCX = 0;
              if (fPosCY <= 0) fPosCY = 0;
              if (fwidth < 1) return;
              if (heighteight < 1) return;
              */
              if (_width < 6) return;
              uint c = (uint)color;
              c = GUIGraphicsContext.MergeAlpha(c);

              _font.DrawText((float)_positionX - _textwidth, (float)_positionY, (int)c, _cachedTextLabel, GUIControl.Alignment.ALIGN_LEFT, (int)_width - 5);
              //if (_useViewPort)
              //  GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }
            return;
          }

          if (_width == 0 || _textwidth < _width)
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);

            _font.DrawText((float)_positionX, (float)_positionY, (int)c, _cachedTextLabel, _textAlignment, (int)_width);
          }
          else
          {
            /*
            float fPosCX = (float)_positionX;
            float fPosCY = (float)_positionY;
            GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
            if (fPosCX < 0) fPosCX = 0.0f;
            if (fPosCY < 0) fPosCY = 0.0f;
            if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
            float heighteight = 60.0f;
            if (heighteight + fPosCY >= GUIGraphicsContext.Height)
              heighteight = GUIGraphicsContext.Height - fPosCY - 1;
            if (heighteight <= 0) return;

            float fwidth = _width - 5.0f;
            if (fwidth < 1) return;
            if (heighteight < 1) return;

            if (fPosCX <= 0) fPosCX = 0;
            if (fPosCY <= 0) fPosCY = 0;*/
            if (_width < 6) return;

            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            _font.DrawText((float)_positionX, (float)_positionY, (int)c, _cachedTextLabel, _textAlignment, (int)_width - 5);
          }
        }
      }
      base.Render(timePassed);
    }

    public bool UseViewPort
    {
      get { return _useViewPort; }
      set { _useViewPort = value; }
    }
    /// <summary>
    /// Checks if the control can focus.
    /// </summary>
    /// <returns>false</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// This method is called when a message was recieved by this control.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      // Check if the message was ment for this control.
      if (message.TargetControlId == GetID)
      {
        // Set the text of the label.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
            Label = message.Label;
          else
            Label = String.Empty;
          return true;
        }
      }
      return base.OnMessage(message);
    }

    public override int Width
    {
      get
      {
        return base.Width;
      }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;
        }
      }
    }

    public override int Height
    {
      get
      {
        return base.Height;
      }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;
        }
      }
    }
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (_positionX == dwPosX && _positionY == dwPosY) return;
      _positionX = dwPosX;
      _positionY = dwPosY;
      _reCalculate = true;
    }
    /// <summary>
    /// Get/set the color of the text
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set
      {
        if (_textColor != value)
        {
          _textColor = value;

          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the alignment of the text
    /// </summary>
    public GUIControl.Alignment TextAlignment
    {
      get { return _textAlignment; }
      set
      {
        if (_textAlignment != value)
        {
          _textAlignment = value;

          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get
      {
        return _fontName;
      }
      set
      {
        if (value == null) return;
        if (value == String.Empty) return;
        if (_font == null)
        {
          _font = GUIFontManager.GetFont(value);
          _fontName = value;
          _reCalculate = true;
        }
        else if (value != _font.FontName)
        {
          _font = GUIFontManager.GetFont(value);
          _fontName = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Label
    {
      get { return _labelText; }
      set
      {
        if (value == null) return;
        if (value.Equals(_labelText)) return;
        _labelText = value;
        _cachedTextLabel = _labelText;
        if (_labelText.IndexOf("#") >= 0) _containsProperty = true;
        else _containsProperty = false;
        _textwidth = 0;
        _textheight = 0;
        _reCalculate = true;
      }
    }


    /// <summary>
    /// Property which returns true if the label contains a property
    /// or false when it doenst
    /// </summary>
    public bool _containsPropertyKey
    {
      get { return _containsProperty; }
    }

    /// <summary>
    /// Allocate any direct3d sources
    /// </summary>
    public override void AllocResources()
    {
      _propertyHasChanged = true;
      GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      _font = GUIFontManager.GetFont(_fontName);
      Update();
      base.AllocResources();
    }

    void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (!_containsProperty) return;
      if (_labelText.IndexOf(tag) >= 0)
      {
        _propertyHasChanged = true;
      }
    }

    /// <summary>
    /// Free any direct3d resources
    /// </summary>
    public override void FreeResources()
    {
      _reCalculate = true;
      GUIPropertyManager.OnPropertyChanged -= new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      base.FreeResources();
    }

    /// <summary>
    /// Property to get/set the usage of the font cache
    /// if enabled the renderd text is cached
    /// if not it will be re-created on every render() call
    /// </summary>
    public bool CacheFont
    {
      get { return _useFontCache; }
      set { _useFontCache = false; }
    }

    /// <summary>
    /// updates the current label by deleting the fontcache 
    /// </summary>
    protected override void Update()
    {

    }

    /// <summary>
    /// Returns the width of the current text
    /// </summary>
    public int TextWidth
    {
      get
      {
        if (_textwidth == 0 || _textheight == 0)
        {
          if (_font == null) return 0;
          _cachedTextLabel = GUIPropertyManager.Parse(_labelText);
          if (_cachedTextLabel == null)
            _cachedTextLabel = "";
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }
        return _textwidth;
      }
    }

    /// <summary>
    /// Returns the height of the current text
    /// </summary>
    public int TextHeight
    {
      get
      {
        if (_textwidth == 0 || _textheight == 0)
        {
          if (_font == null) return 0;
          _cachedTextLabel = GUIPropertyManager.Parse(_labelText);
          if (_cachedTextLabel == null)
            _cachedTextLabel = "";
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }
        return _textheight;
      }
    }
    void ClearFontCache()
    {
      Update();
    }
    public override void Animate(float timePassed, Animator animator)
    {
      base.Animate(timePassed, animator);
      _reCalculate = true;
    }
  }
}
