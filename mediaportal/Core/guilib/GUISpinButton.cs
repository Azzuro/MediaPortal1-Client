#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Diagnostics;
using System.IO;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a GUISpinButton.
  /// </summary>
  public class GUISpinButton : GUIControl
  {
    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = "";
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = "";
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("textureUp")] protected string _upTextureName;
    [XMLSkinElement("textureDown")] protected string _downTextureName;
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus;
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus;
    [XMLSkinElement("spinWidth")] protected int _spinWidth;
    [XMLSkinElement("spinHeight")] protected int _spinHeight;
    [XMLSkinElement("spinXOff")] protected int _spinOffsetX = 0;
    [XMLSkinElement("spinYOff")] protected int _spinOffsetY = 0;
    [XMLSkinElement("spinalign")] protected Alignment _spinAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spinvalign")] protected VAlignment _spinVAlignment = VAlignment.ALIGN_MIDDLE;
    [XMLSkinElement("spinPrefixText")] protected string _prefixText = "";
    [XMLSkinElement("spinSuffixText")] protected string _suffixText = "";
    [XMLSkinElement("spinTextXOff")] protected int _spinTextOffsetX = 0;
    [XMLSkinElement("spinTextYOff")] protected int _spinTextOffsetY = 0;
    [XMLSkinElement("showrange")] protected bool _showRange = true;
    [XMLSkinElement("digits")] protected int _digits = -1;
    [XMLSkinElement("reverse")] protected bool _reverse = false;
    [XMLSkinElement("cycleItems")] protected bool _cycleItems = false;

    [XMLSkinElement("spintype")] protected GUISpinControl.SpinType _spinType =
      GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;

    [XMLSkinElement("orientation")] protected eOrientation _orientation = eOrientation.Horizontal;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("textvalign")] protected VAlignment _textVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkin("textureFocus", "border")] protected string _strBorderTF = "";
    [XMLSkin("textureFocus", "position")] protected string _strBorderPositionTF = "outside";
    [XMLSkin("textureFocus", "textureRepeat")] protected bool _borderTextureRepeatTF = false;
    [XMLSkin("textureFocus", "textureRotate")] protected bool _borderTextureRotateTF = false;
    [XMLSkin("textureFocus", "texture")] protected string _borderTextureFileNameTF = "image_border.png";
    [XMLSkin("textureFocus", "colorKey")] protected long _borderColorKeyTF = 0xFFFFFFFF;
    [XMLSkin("textureNoFocus", "border")] protected string _strBorderTNF = "";
    [XMLSkin("textureNoFocus", "position")] protected string _strBorderPositionTNF = "outside";
    [XMLSkin("textureNoFocus", "textureRepeat")] protected bool _borderTextureRepeatTNF = false;
    [XMLSkin("textureNoFocus", "textureRotate")] protected bool _borderTextureRotateTNF = false;
    [XMLSkin("textureNoFocus", "texture")] protected string _borderTextureFileNameTNF = "image_border.png";
    [XMLSkin("textureNoFocus", "colorKey")] protected long _borderColorKeyTNF = 0xFFFFFFFF;

    protected int _frameCounter = 0;
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected GUILabelControl _labelControl = null;
    protected GUISpinControl _spinControl = null;

    public GUISpinButton(int dwParentID) : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUISpinButton class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strTextureFocus">The filename containing the texture of the button, when the button has the focus.</param>
    /// <param name="strTextureNoFocus">The filename containing the texture of the button, when the button does not have the focus.</param>
    /// <param name="strUpFocus">The filename containing the texture of the spin up button, when the button has the focus.</param>
    /// <param name="strUpNoFocus">The filename containing the texture of the spin up button, when the button does not have the focus.</param>
    /// <param name="strDownFocus">The filename containing the texture of the spin down button, when the button has the focus.</param>
    /// <param name="strDownNoFocus">The filename containing the texture of the spin down button, when the button does not have the focus.</param>
    /// <param name="dwSpinWidth">The width of the spin control.</param>
    /// <param name="dwSpinHeight">The height of the spin control.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degrees along x-axis.</param>
    /// <param name="dwShadowDistance">The height of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUISpinButton(int dwParentID, int dwControlId, int dwPosX,
                         int dwPosY, int dwWidth, int dwHeight,
                         string strTextureFocus, string strTextureNoFocus,
                         string strUpFocus, string strUpNoFocus, string strDownFocus, string strDownNoFocus,
                         int dwSpinWidth, int dwSpinHeight,
                         int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      _upTextureName = strUpNoFocus;
      _upTextureNameFocus = strUpFocus;
      _downTextureName = strDownNoFocus;
      _downTextureNameFocus = strDownFocus;
      _spinWidth = dwSpinWidth;
      _spinHeight = dwSpinHeight;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                           _focusedTextureName);
      _imageFocused.ParentControl = this;
      _imageFocused.Filtering = false;
      _imageFocused.DimColor = DimColor;
      _imageFocused.SetBorder(_strBorderTF, _strBorderPositionTF, _borderTextureRepeatTF, _borderTextureRotateTF,
                              _borderTextureFileNameTF, _borderColorKeyTF);

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _nonFocusedTextureName);
      _imageNonFocused.ParentControl = this;
      _imageNonFocused.Filtering = false;
      _imageNonFocused.DimColor = DimColor;
      _imageNonFocused.SetBorder(_strBorderTNF, _strBorderPositionTNF, _borderTextureRepeatTNF, _borderTextureRotateTNF,
                                 _borderTextureFileNameTNF, _borderColorKeyTNF);
      GUILocalizeStrings.LocalizeLabel(ref _label);

      _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                          _label, _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false,
                                          _shadowAngle, _shadowDistance, _shadowColor);
      _labelControl.ParentControl = this;
      _labelControl.DimColor = DimColor;
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextVAlignment = _textVAlignment;

      _spinControl = new GUISpinControl(0, 0, _positionX + _width - _spinWidth, _positionY, _spinWidth,
                                        _spinHeight, _upTextureName, _downTextureName,
                                        _upTextureNameFocus, _downTextureNameFocus,
                                        _fontName, _textColor, _spinType, _spinAlignment);
      _spinControl.ParentControl = this;
      _spinControl.DimColor = DimColor;
      _spinControl.ShowRange = _showRange;
      _spinControl.Digits = _digits;
      _spinControl.SetReverse(_reverse);
      _spinControl.Orientation = _orientation;
      _spinControl.CycleItems = _cycleItems;
      _spinControl.SetShadow(_shadowAngle, _shadowDistance, _shadowColor);
      _spinControl.PrefixText = _prefixText;
      _spinControl.SuffixText = _suffixText;
      _spinControl.TextOffsetX = _spinTextOffsetX;
      _spinControl.TextOffsetY = _spinTextOffsetY;

      // Pass all of the subitems to the spin control.
      for (int i = 0; i < SubItemCount; ++i)
      {
        _spinControl.AddSubItem(GetSubItem(i));
      }
      for (int i = 0; i < SubItemCount; ++i)
      {
        RemoveSubItem(i);
      }
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control to scale itself to the current screen resolution
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
    }

    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (value != IsFocused)
        {
          if (value == true)
          {
            if (_imageFocused != null)
            {
              _imageFocused.Begin();
            }
            GUIPropertyManager.SetProperty("#highlightedbutton", Label);
          }
          else
          {
            if (_imageNonFocused != null)
            {
              _imageNonFocused.Begin();
            }
          }
        }
        base.Focus = value;
        _spinControl.Focus = value;
      }
    }

    public int SpinOffsetX
    {
      get { return _spinOffsetX; }
      set { _spinOffsetX = value; }
    }

    public int SpinOffsetY
    {
      get { return _spinOffsetY; }
      set { _spinOffsetY = value; }
    }

    /// <summary>
    /// Renders the GUISpinButton.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      // The GUISpinButton has the focus
      if (Focus)
      {
        //render the focused image
        _imageFocused.Render(timePassed);
      }
      else
      {
        //render the non-focused image
        _imageNonFocused.Render(timePassed);
      }

      int labelWidth = _width - 2 * _textOffsetX;
      if (labelWidth <= 0)
      {
        base.Render(timePassed);
        return;
      }
      _labelControl.Width = labelWidth;

      // render the text on the button
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextVAlignment = _textVAlignment;
      _labelControl.Label = _label;
      _labelControl.TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;

      int x = 0;
      int y = 0;

      switch (_textAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _textOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _textOffsetX;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width / 2) - (labelWidth / 2));
          break;
      }

      switch (_textVAlignment)
      {
        case VAlignment.ALIGN_TOP:
          y = _textOffsetY + _positionY;
          break;

        case VAlignment.ALIGN_BOTTOM:
          y = _positionY + _height - _textOffsetY;
          break;

        case VAlignment.ALIGN_MIDDLE:
          y = _positionY + ((_height / 2) - (_labelControl.Height / 2));
          break;
      }

      _labelControl.SetPosition(x, y);
      _labelControl.Render(timePassed);

      x = 0;
      y = 0;

      switch (_spinAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _spinOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _spinOffsetX - _spinControl.Width;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width / 2) - (_spinControl.Width / 2));
          break;
      }

      switch (_spinVAlignment)
      {
        case VAlignment.ALIGN_TOP:
          y = _spinOffsetY + _positionY;
          break;

        case VAlignment.ALIGN_BOTTOM:
          y = _positionY + _height - _spinOffsetY - _spinControl.Height;
          break;

        case VAlignment.ALIGN_MIDDLE:
          y = _positionY + ((_height / 2) - (_spinControl.Height / 2));
          break;
      }

      _spinControl.SetPosition(x, y);
      _spinControl.Render(timePassed);
      base.Render(timePassed);
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      GUIMessage message;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // send a message to anyone interested 
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
        }
      }

      // Allow the spin control to handle actions first.
      _spinControl.OnAction(action);

      // If the spin control handled the action then avoid the base action handler.
      // In particular this avoids the move up,down,left,right actions from leaving this control too soon.
      if (!_spinControl.ActionHandled)
      {
        base.OnAction(action);
      }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a control can respond to the messages of its controls
    /// </summary>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      // Let the spin control handle messages first
      if (_spinControl.OnMessage(message))
      {
        return true;
      }

      // Handle the GUI_MSG_LABEL_SET message
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
          return true;
        }
      }
      // Let the base class handle the other messages
      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();
      _spinControl.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _frameCounter = 0;
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();
      _width = _imageFocused.Width;
      _height = _imageFocused.Height;
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.AllocResources();
      _spinControl.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageFocused.FreeResources();
      _imageNonFocused.FreeResources();
      _labelControl.FreeResources();
      _spinControl.FreeResources();
    }

    /// <summary>
    /// Sets the position of the control.
    /// </summary>
    /// <param name="dwPosX">The X position.</param>
    /// <param name="dwPosY">The Y position.</param>		
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);
      _imageFocused.SetPosition(dwPosX, dwPosY);
      _imageNonFocused.SetPosition(dwPosX, dwPosY);
    }

    /// <summary>
    /// Changes the alpha transparency component of the colordiffuse.
    /// </summary>
    /// <param name="dwAlpha">The new value of the colordiffuse.</param>
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      _imageFocused.SetAlpha(dwAlpha);
      _imageNonFocused.SetAlpha(dwAlpha);
    }

    /// <summary>
    /// Get/set the color of the text when the GUISpinButton is disabled.
    /// </summary>
    public long DisabledColor
    {
      get { return _disabledColor; }
      set { _disabledColor = value; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUISpinButton does not have the focus.
    /// </summary>
    public string TexutureNoFocusName
    {
      get { return _imageNonFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUISpinButton has the focus.
    /// </summary>
    public string TexutureFocusName
    {
      get { return _imageFocused.FileName; }
    }

    /// <summary>
    /// Set the color of the text on the GUISpinButton. 
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the name of the font of the text of the GUISpinButton.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName = value;
        _labelControl.FontName = _fontName;
      }
    }

    /// <summary>
    /// Set the text of the GUISpinButton. 
    /// </summary>
    /// <param name="strFontName">The font name.</param>
    /// <param name="strLabel">The text.</param>
    /// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName, string strLabel, long dwColor)
    {
      if (strFontName == null)
      {
        return;
      }
      if (strLabel == null)
      {
        return;
      }
      Label = strLabel;
      _textColor = dwColor;
      _fontName = strFontName;

      _labelControl.FontName = _fontName;
      _labelControl.TextColor = dwColor;
      _labelControl.Label = strLabel;
    }

    /// <summary>
    /// Get/set the text of the GUISpinButton.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null)
        {
          return;
        }

        _label = value;
        _labelControl.Label = _label;
      }
    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX
    {
      get { return _textOffsetX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetX = value;
      }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY
    {
      get { return _textOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetY = value;
      }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    public VAlignment TextVAlignment
    {
      get { return _textVAlignment; }
      set { _textVAlignment = value; }
    }

    public VAlignment SpinVAlignment
    {
      get { return _spinVAlignment; }
      set { _spinVAlignment = value; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected override void Update()
    {
      base.Update();

      _imageFocused.ColourDiffuse = ColourDiffuse;
      _imageFocused.Width = _width;
      _imageFocused.Height = _height;


      _imageNonFocused.ColourDiffuse = ColourDiffuse;
      _imageNonFocused.Width = _width;
      _imageNonFocused.Height = _height;

      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
    }

    public void Refresh()
    {
      Update();
    }

/*
    /// <summary>
    /// get/set the current selected item
    /// A button can have 1 or more subitems
    /// each subitem has its own text to render on the button
    /// When the user presses the button, the next item will be selected
    /// and shown on the button
    /// </summary>
    public override int SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (SubItemCount > 0)
        {
          _selectedItem = value;
          if (_selectedItem < 0 || _selectedItem >= SubItemCount)
          {
            _selectedItem = 0;
          }
          Label = (string) GetSubItem(_selectedItem);
        }
        else
        {
          _selectedItem = 0;
        }
      }
    }
*/

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageFocused != null)
        {
          _imageFocused.DimColor = value;
        }
        if (_imageNonFocused != null)
        {
          _imageNonFocused.DimColor = value;
        }
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
        if (_spinControl != null)
        {
          _spinControl.DimColor = value;
        }
      }
    }

    public GUISpinControl SpinControl
    {
      get { return _spinControl; }
      set { _spinControl = value; }
    }
  }
}