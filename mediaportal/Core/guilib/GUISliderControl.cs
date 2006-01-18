/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Collections;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implementation of a slider control.
  /// </summary>
  public class GUISliderControl : GUIControl
  {
    public enum SpinSelect
    {
      SPIN_BUTTON_DOWN,
      SPIN_BUTTON_UP
    };



    [XMLSkinElement("textureSliderBar")]
    protected string _backgroundTextureName;
    [XMLSkinElement("textureSliderNib")]
    protected string _sliderTextureName;
    [XMLSkinElement("textureSliderNibFocus")]
    protected string _sliderFocusTextureName;

    protected int _percentage = 0;
    protected int _intStartValue = 0;
    protected int _intEndValue = 100;
    protected float _floatStartValue = 0.0f;
    protected float _floatEndValue = 1.0f;
    protected int _intValue = 0;
    protected float _floatValue = 0.0f;
    [XMLSkinElement("subtype")]
    protected GUISpinControl.SpinType _subType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;
    protected bool _reverse = false;
    protected float _floatInterval = 0.1f;
    protected ArrayList _listLabels = new ArrayList();
    protected ArrayList _listValues = new ArrayList();
    protected GUIImage _imageBackGround = null;
    protected GUIImage _imageMid = null;
    protected GUIImage _imageMidFocus = null;



    protected bool m_bShowRange = false;

    public GUISliderControl(int dwParentID)
      : base(dwParentID)
    {
    }
    /// <summary>
    /// The constructor of the GUISliderControl.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strBackGroundTexture">The background texture of the </param>
    /// <param name="strMidTexture">The unfocused texture.</param>
    /// <param name="strMidTextureFocus">The focused texture</param>
    /// <param name="iType">The type of control.</param>
    public GUISliderControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strBackGroundTexture, string strMidTexture, string strMidTextureFocus, GUISpinControl.SpinType iType)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _backgroundTextureName = strBackGroundTexture;
      _sliderTextureName = strMidTexture;
      _sliderFocusTextureName = strMidTextureFocus;
      _subType = iType;
      FinalizeConstruction();
    }
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageBackGround = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _backgroundTextureName, 0);
      _imageBackGround.ParentControl = this;

      _imageMid = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _sliderTextureName, 0);
      _imageMid.ParentControl = this;

      _imageMidFocus = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _sliderFocusTextureName, 0);
      _imageMidFocus.ParentControl = this;
    }


    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }
      string strValue = "";
      float fRange = 0.0f;
      float fPos = 0.0f;
      float fPercent = 0.0f;
      GUIFont _font13 = GUIFontManager.GetFont("font13");
      switch (_subType)
      {
        // Float based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
          strValue = String.Format("{0}", _floatValue);
          if (null != _font13)
          {
            _font13.DrawShadowText((float)_positionX, (float)_positionY, 0xffffffff,
                                      strValue,
                                      GUIControl.Alignment.ALIGN_LEFT,
                                      2,
                                      2,
                                      0xFF020202);
          }
          _imageBackGround.SetPosition(_positionX + 60, _positionY);

          fRange = (float)(_floatEndValue - _floatStartValue);
          fPos = (float)(_floatValue - _floatStartValue);
          fPercent = (fPos / fRange) * 100.0f;
          _percentage = (int)fPercent;
          break;

        // Integer based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
          strValue = String.Format("{0}/{1}", _intValue, _intEndValue);
          if (null != _font13)
          {
            _font13.DrawShadowText((float)_positionX, (float)_positionY, 0xffffffff,
                                            strValue,
                                            GUIControl.Alignment.ALIGN_LEFT,
                                            2,
                                            2,
                                            0xFF020202);
          }
          _imageBackGround.SetPosition(_positionX + 60, _positionY);

          fRange = (float)(_intEndValue - _intStartValue);
          fPos = (float)(_intValue - _intStartValue);
          _percentage = (int)((fPos / fRange) * 100.0f);
          break;
      }

      //int iHeight=25;
      _imageBackGround.Render(timePassed);
      //_imageBackGround.SetHeight(iHeight);
      _height = _imageBackGround.Height;
      _width = _imageBackGround.Width + 60;

      float fWidth = (float)(_imageBackGround.TextureWidth - _imageMid.Width); //-20.0f;

      fPos = (float)_percentage;
      fPos /= 100.0f;
      fPos *= fWidth;
      fPos += (float)_imageBackGround.XPosition;
      //fPos += 10.0f;
      if ((int)fWidth > 1)
      {
        if (IsFocused)
        {
          _imageMidFocus.SetPosition((int)fPos, _imageBackGround.YPosition);
          _imageMidFocus.Render(timePassed);
        }
        else
        {
          _imageMid.SetPosition((int)fPos, _imageBackGround.YPosition);
          _imageMid.Render(timePassed);
        }
      }
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
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_CLICK:
          float x = (float)action.fAmount1 - _imageBackGround.XPosition;
          if (x < 0) x = 0;
          if (x > _imageBackGround.RenderWidth) x = _imageBackGround.RenderWidth;
          x /= (float)_imageBackGround.RenderWidth;
          float total, pos;
          switch (_subType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              total = _floatEndValue - _floatStartValue;
              pos = (x * total);
              _floatValue = _floatStartValue + pos;
              _floatValue = (float)Math.Round(_floatValue, 1);
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              float start = _intStartValue;
              float end = _intEndValue;
              total = end - start;
              pos = (x * total);
              _intValue = _intStartValue + (int)pos;
              break;

            default:
              _percentage = (int)(100f * x);
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;

        // decrease the slider value
        case Action.ActionType.ACTION_MOVE_LEFT:
          switch (_subType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (_floatValue > _floatStartValue) _floatValue -= _floatInterval;
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              if (_intValue > _intStartValue) _intValue--;
              break;

            default:
              if (_percentage > 0) _percentage--;
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;

        // increase the slider value
        case Action.ActionType.ACTION_MOVE_RIGHT:
          switch (_subType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (_floatValue < _floatEndValue) _floatValue += _floatInterval;
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              if (_intValue < _intEndValue) _intValue++;
              break;

            default:
              if (_percentage < 100) _percentage++;
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;

        default:
          base.OnAction(action);
          break;
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
      if (message.TargetControlId == GetID)
      {
        switch (message.Message)
        {
          // Move the slider to a certain position
          case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
            Percentage = message.Param1;
            return true;

          // Reset the slider
          case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
            {
              Percentage = 0;
              return true;
            }
        }
      }

      return base.OnMessage(message);
    }

    /// <summary>
    /// Get/set the percentage the slider indicates.
    /// </summary>
    public int Percentage
    {
      get { return _percentage; }
      set
      {
        if (value >= 0 && value <= 100) _percentage = value;
      }
    }

    /// <summary>
    /// Get/set the integer value of the slider.
    /// </summary>
    public int IntValue
    {
      get { return _intValue; }
      set
      {
        if (value >= _intStartValue && value <= _intEndValue)
        {
          _intValue = value;
        }
      }
    }

    /// <summary>
    /// Get/set the float value of the slider.
    /// </summary>
    public float FloatValue
    {
      get { return _floatInterval; }
      set
      {
        if (value >= _floatStartValue && value <= _floatEndValue)
        {
          _floatInterval = value;
        }
      }
    }

    /// <summary>
    /// Get/Set the spintype of the control.
    /// </summary>
    public GUISpinControl.SpinType SpinType
    {
      get { return _subType; }
      set { _subType = value; }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageBackGround.PreAllocResources();
      _imageMid.PreAllocResources();
      _imageMidFocus.PreAllocResources();

    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageBackGround.AllocResources();
      _imageMid.AllocResources();
      _imageMidFocus.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageBackGround.FreeResources();
      _imageMid.FreeResources();
      _imageMidFocus.FreeResources();
    }

    /// <summary>
    /// Sets the integer range of the slider.
    /// </summary>
    /// <param name="iStart">Start point</param>
    /// <param name="iEnd">End point</param>
    public void SetRange(int iStart, int iEnd)
    {
      if (iEnd > iStart && iStart >= 0)
      {
        _intStartValue = iStart;
        _intEndValue = iEnd;
      }
    }

    /// <summary>
    /// Sets the float range of the slider.
    /// </summary>
    /// <param name="fStart">Start point</param>
    /// <param name="fEnd">End point</param>
    public void SetFloatRange(float fStart, float fEnd)
    {
      if (fEnd > _floatStartValue && _floatStartValue >= 0)
      {
        _floatStartValue = fStart;
        _floatEndValue = fEnd;
      }
    }

    /// <summary>
    /// Get/set the interval for the float. 
    /// </summary>
    public float FloatInterval
    {
      get { return _floatInterval; }
      set { _floatInterval = value; }
    }

    /// <summary>
    /// Get the name of the background texture.
    /// </summary>
    public string BackGroundTextureName
    {
      get { return _imageBackGround.FileName; }
    }

    /// <summary>
    /// Get the name of the middle texture.
    /// </summary>
    public string BackTextureMidName
    {
      get { return _imageMid.FileName; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>		
    protected override void Update()
    {
      _imageBackGround.SetPosition(XPosition, YPosition);
    }
  }
}
