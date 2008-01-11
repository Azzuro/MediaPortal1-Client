#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  public class GUISortButtonControl : GUIButtonControl
  {
    #region Constructors

    public GUISortButtonControl(int parentId)
      : base(parentId)
    {

    }

    #endregion Constructors

    #region Events

    public event SortEventHandler SortChanged;

    #endregion Events

    #region Methods

    public override void AllocResources()
    {
      base.AllocResources();

      _sortImages[0].AllocResources();
      _sortImages[1].AllocResources();
      _sortImages[2].AllocResources();
      _sortImages[3].AllocResources();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      int x = _positionX + _sortButtonOffsetX;
      int y = _positionY + _sortButtonOffsetY;
      int w = _sortButtonWidth;
      int h = _sortButtonHeight;

      _sortImages[0] = LoadAnimationControl(this.GetID, this.GetID + 25000, x, y, w, h, _ascendingTextureFilename); //, 0xFFFFFFFF);
      _sortImages[1] = LoadAnimationControl(this.GetID, this.GetID + 25001, x, y, w, h, _ascendingTextureFocusedFilename); //, 0xFFFFFFFF);
      _sortImages[2] = LoadAnimationControl(this.GetID, this.GetID + 25002, x, y, w, h, _descendingTextureFilename); //, 0xFFFFFFFF);
      _sortImages[3] = LoadAnimationControl(this.GetID, this.GetID + 25003, x, y, w, h, _descendingTextureFocusedFilename); //, 0xFFFFFFFF);
      _sortImages[0].ParentControl = this;
      _sortImages[1].ParentControl = this;
      _sortImages[2].ParentControl = this;
      _sortImages[3].ParentControl = this;
      _sortImages[0].DimColor = DimColor;
      _sortImages[1].DimColor = DimColor;
      _sortImages[2].DimColor = DimColor;
      _sortImages[3].DimColor = DimColor;
    }
    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _sortButtonWidth, ref _sortButtonHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _sortButtonOffsetX, ref _sortButtonOffsetY);
    }

    public override void FreeResources()
    {
      base.FreeResources();

      _sortImages[0].FreeResources();
      _sortImages[1].FreeResources();
      _sortImages[2].FreeResources();
      _sortImages[3].FreeResources();
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();

      _sortImages[0].PreAllocResources();
      _sortImages[1].PreAllocResources();
      _sortImages[2].PreAllocResources();
      _sortImages[3].PreAllocResources();
    }

    public override void Render(float timePassed)
    {
      bool isFocused = this.Focus;

      base.Render(timePassed);
      int x = _positionX + _sortButtonOffsetX;
      int y = _positionY + _sortButtonOffsetY;

      if (_sortImages[0].XPosition != x || _sortImages[0].YPosition != y)
      {
        _sortImages[0].SetPosition(x, y);
        _sortImages[0].Width = _sortButtonWidth;
        _sortImages[0].Height = _sortButtonHeight;
      }
      if (_sortImages[1].XPosition != x || _sortImages[1].YPosition != y)
      {
        _sortImages[1].SetPosition(x, y);
        _sortImages[1].Width = _sortButtonWidth;
        _sortImages[1].Height = _sortButtonHeight;
      }
      if (_sortImages[2].XPosition != x || _sortImages[2].YPosition != y)
      {
        _sortImages[2].SetPosition(x, y);
        _sortImages[2].Width = _sortButtonWidth;
        _sortImages[2].Height = _sortButtonHeight;
      }
      if (_sortImages[3].XPosition != x || _sortImages[3].YPosition != y)
      {
        _sortImages[3].SetPosition(x, y);
        _sortImages[3].Width = _sortButtonWidth;
        _sortImages[3].Height = _sortButtonHeight;
      }
      int sortImageIndex = _isAscending ? isFocused && _isSortImageHot ? 3 : 2 : isFocused && _isSortImageHot ? 1 : 0;

      _sortImages[sortImageIndex].Render(timePassed);
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      bool isHovering = base.HitTest(x, y, out controlID, out focused);

      _isSortImageHot = false;

      if (isHovering && x >= _sortImages[0].XPosition && x <= _sortImages[0].XPosition + _sortImages[0].Width &&
          y >= _sortImages[0].YPosition && y <= _sortImages[0].YPosition + _sortImages[0].Height)
      {
        _isSortImageHot = true;
      }

      return isHovering;
    }

    protected override void Update()
    {
      base.Update();

      int x = _positionX + _sortButtonOffsetX;
      int y = _positionY + _sortButtonOffsetY;

      _sortImages[0].SetPosition(x, y);
      _sortImages[1].SetPosition(x, y);
      _sortImages[2].SetPosition(x, y);
      _sortImages[3].SetPosition(x, y);
    }

    public override void OnAction(Action action)
    {
      if (_isSortImageHot && action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM && _isSortImageHot)
      {
        _isAscending = !_isAscending;

        if (SortChanged != null)
          SortChanged(this, new SortEventArgs(_isAscending ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.Descending));

        return;
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_LEFT && _isSortImageHot)
      {
        _isSortImageHot = false;
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
      {
        if (_isSortImageHot == false)
        {
          _isSortImageHot = true;
          return;
        }

        _isSortImageHot = false;
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS || message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS)
        _isSortImageHot = false;

      return base.OnMessage(message);
    }

    #endregion Methods

    #region Properties

    public bool IsAscending
    {
      get { return _isAscending; }
      set { _isAscending = value; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        _sortImages[0].DimColor = value;
        _sortImages[1].DimColor = value;
        _sortImages[2].DimColor = value;
        _sortImages[3].DimColor = value;
      }
    }

    #endregion Properties

    #region Fields

    bool _isAscending = true;
    bool _isSortImageHot = false;

    [XMLSkinElement("textureAscending")]              string _ascendingTextureFilename = "arrow_round_up_nofocus.png";
    [XMLSkinElement("textureAscendingFocused")]       string _ascendingTextureFocusedFilename = "arrow_round_up_focus.png";
    [XMLSkinElement("textureDescending")]             string _descendingTextureFilename = "arrow_round_down_nofocus.png";
    [XMLSkinElement("textureDescendingFocused")]      string _descendingTextureFocusedFilename = "arrow_round_down_focus.png";    
    [XMLSkinElement("offsetSortButtonX")]             int _sortButtonOffsetX = 0;
    [XMLSkinElement("offsetSortButtonY")]             int _sortButtonOffsetY = 0;
    [XMLSkinElement("offsetSortButtonHeight")]        int _sortButtonHeight = 16;
    [XMLSkinElement("offsetSortButtonWidth")]         int _sortButtonWidth = 16;

    GUIAnimation[] _sortImages = new GUIAnimation[4];

    #endregion Fields
  }
}