#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms; // used for Keys definition
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIFilmstripControl : GUIControl
  {
    //TODO : add comments
    //TODO : use GUILabelControl for drawing text
    const int SLEEP_FRAME_COUNT = 2;
    const int THUMBNAIL_OVERSIZED_DIVIDER = 32;

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    } ;

    [XMLSkinElement("remoteColor")]
    protected long _remoteColor = 0xffff0000;
    [XMLSkinElement("downloadColor")]
    protected long _downloadColor = 0xff00ff00;
    [XMLSkinElement("thumbPosXBig")]
    protected int _positionXThumbBig = 0;
    [XMLSkinElement("thumbPosYBig")]
    protected int _positionYThumbBig = 0;
    [XMLSkinElement("thumbWidthBig")]
    protected int _widthThumbBig = 0;
    [XMLSkinElement("thumbHeightBig")]
    protected int _heightThumbBig = 0;

    [XMLSkinElement("imageFolder")]
    protected string _imageFolderName = "";
    [XMLSkinElement("imageFolderFocus")]
    protected string _imageFolderNameFocus = "";

    [XMLSkinElement("textureUp")]
    protected string _upTextureName = "";
    [XMLSkinElement("textureDown")]
    protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")]
    protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")]
    protected string _downTextureNameFocus = "";
    [XMLSkinElement("spinColor")]
    protected long _colorSpinColor;
    [XMLSkinElement("spinHeight")]
    protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")]
    protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")]
    protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")]
    protected int _spinControlPositionY;
    [XMLSkinElement("itemHeight")]
    protected int _itemHeight;
    [XMLSkinElement("itemWidth")]
    protected int _itemWidth;
    [XMLSkinElement("texturelowHeight")]
    protected int _textureLowHeight;
    [XMLSkinElement("textureLowWidth")]
    protected int _textureLowWidth;
    [XMLSkinElement("textureWidth")]
    protected int _textureWidth = 80;
    [XMLSkinElement("textureHeight")]
    protected int _textureHeight = 80;
    [XMLSkinElement("thumbPosX")]
    protected int _thumbNailPositionX = 8;
    [XMLSkinElement("thumbPosY")]
    protected int _thumbNailPositionY = 8;
    [XMLSkinElement("thumbWidth")]
    protected int _thumbNailWidth = 64;
    [XMLSkinElement("thumbHeight")]
    protected int _thumbNailHeight = 64;
    [XMLSkinElement("font")]
    protected string _fontName = "";
    [XMLSkinElement("textcolor")]
    protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")]
    protected long _selectedColor = 0xFFFFFFFF;

    [XMLSkinElement("scrollbarbg")]
    protected string _scrollbarBackgroundName = "";
    [XMLSkinElement("scrollbartop")]
    protected string _scrollbarTopName = "";
    [XMLSkinElement("scrollbarbottom")]
    protected string _scrollbarBottomName = "";

    [XMLSkinElement("folderPrefix")]
    protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")]
    protected string _folderSuffix = "]";

    [XMLSkinElement("backgroundheight")]
    protected int _backGroundHeight;
    [XMLSkinElement("backgroundwidth")]
    protected int _backGroundWidth;
    [XMLSkinElement("backgroundx")]
    protected int _backGroundPositionX;
    [XMLSkinElement("backgroundy")]
    protected int _backGroundPositionY;
    [XMLSkinElement("backgrounddiffuse")]
    protected int _backGroundDiffuseColor;
    [XMLSkinElement("background")]
    protected string _backgroundTextureName;

    [XMLSkinElement("InfoImageheight")]
    protected int _infoImageHeight;
    [XMLSkinElement("InfoImagewidth")]
    protected int _infoImageWidth;
    [XMLSkinElement("InfoImagex")]
    protected int _infoImagePositionX;
    [XMLSkinElement("InfoImagey")]
    protected int _infoImagePositionY;
    [XMLSkinElement("InfoImagediffuse")]
    protected int _infoImageDiffuseColor;
    [XMLSkinElement("InfoImage")]
    protected string _infoImageName;
    [XMLSkinElement("unfocusedAlpha")]
    protected int _unfocusedAlpha = 0xFF;

    int _itemLowHeight;
    int _itemLowWidth;
    int _itemBigHeight;
    int _itemBigWidth;
    int _textureBigHeight;
    int _textureBigWidth;
    bool _showTexture = true;
    int _offset = 0;
    GUIFont _font = null;
    GUISpinControl _upDownControl = null;
    GUIAnimation _imageFolder = null;
    GUIAnimation _imageFolderFocus = null;
    GUIListControl.ListType _listType = GUIListControl.ListType.CONTROL_LIST;
    int _cursorX = 0;
    int _columns;
    bool _scrollingRight = false;
    bool _scrollingLeft = false;
    int _scrollCounter = 0;
    int _sleeper = 0;
    string _suffix = "|";

    int _lowThumbNailPositionX = 0;
    int _lowThumbNailPositionY = 0;
    int _lowThumbNailPositionWidth = 0;
    int _lowThumbNailPositionHeight = 0;

    List<GUIListItem> _listItems = new List<GUIListItem>();
    int _scrollPosition = 0;
    int _scrollPosititionX = 0;
    int _lastItem = -1;
    int _frames = 0;
    bool _refresh = false;
    protected GUIverticalScrollbar _horizontalScrollbar = null;
    protected string _brackedText;
    protected string _scollText;
    GUIAnimation _imageBackground;
    GUIImage _imageInfo;
    DateTime _idleTimer = DateTime.Now;
    bool _infoChanged = false;
    string _newInfoImageName = "";

    protected double _scrollOffset = 0.0f;
    protected int _currentFrame = 0;
    protected double _timeElapsed = 0.0f;
    protected bool _scrollContinuosly = false;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal) * 0.01f); }
    }


    // Search            
    DateTime _timerKey = DateTime.Now;
    char _currentKey = (char)0;
    char _previousKey = (char)0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;

    public GUIFilmstripControl(int dwParentID)
      : base(dwParentID)
    {
    }

    /*public GUIFilmstripControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, 
      string strFontName, 
      string strImageIcon, 
      string strImageIconFocus, 
      int dwitemWidth, int dwitemHeight, 
      int dwSpinWidth, int dwSpinHeight, 
      string strUp, string strDown, 
      string strUpFocus, string strDownFocus, 
      long dwSpinColor, int dwSpinX, int dwSpinY, 
      string strFont, long dwTextColor, long dwSelectedColor, 
      string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
    }*/

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_positionY > _positionY && _spinControlPositionY < _positionY + _height) _spinControlPositionY = _positionY + _height;
      _imageFolder = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth, _itemHeight, _imageFolderName);
      _imageFolder.ParentControl = this;
      _imageFolder.DimColor = DimColor;

			_imageFolderFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth, _itemHeight, _imageFolderNameFocus);
      _imageFolderFocus.ParentControl = this;
      _imageFolderFocus.DimColor = DimColor;

      _upDownControl = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth, _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus, _downTextureNameFocus, _fontName, _colorSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_LEFT);
      _upDownControl.ParentControl = this;
      _upDownControl.DimColor = DimColor;

      _font = GUIFontManager.GetFont(_fontName);
      int xpos = 5 + _positionX + _width;
      if (xpos + 15 > GUIGraphicsContext.Width) xpos = GUIGraphicsContext.Width - 15;
      _horizontalScrollbar = new GUIverticalScrollbar(_controlId, 0, 5 + _positionX + _width, _positionY, 15, _height, _scrollbarBackgroundName, _scrollbarTopName, _scrollbarBottomName);
      _horizontalScrollbar.ParentControl = this;
      _horizontalScrollbar.SendNotifies = false;
      _horizontalScrollbar.DimColor = DimColor;

      _upDownControl.Orientation = GUISpinControl.eOrientation.Horizontal;
      _upDownControl.SetReverse(true);

			_imageBackground = LoadAnimationControl(0, 0, _backGroundPositionX, _backGroundPositionY, _backGroundWidth, _backGroundHeight, _backgroundTextureName);
      _imageBackground.ParentControl = this;
      _imageBackground.DimColor = DimColor;

			_imageInfo = new GUIImage(0, 0, _infoImagePositionX, _infoImagePositionY, _infoImageWidth, _infoImageHeight, _infoImageName, 0);
      _imageInfo.ParentControl = this;
      _imageInfo.Filtering = true;
      _imageInfo.KeepAspectRatio = true;
      _imageInfo.Centered = true;
			_imageInfo.HorizontalAlignment = MediaPortal.Drawing.HorizontalAlignment.Center;
			_imageInfo.VerticalAlignment = MediaPortal.Drawing.VerticalAlignment.Center;
      _imageInfo.DimColor = DimColor;

      SetThumbDimensionsLow(_thumbNailPositionX, _thumbNailPositionY, _thumbNailWidth, _thumbNailHeight);
      SetTextureDimensions(_textureWidth, _textureHeight);
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY, ref _spinControlWidth, ref _spinControlHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textureWidth, ref _textureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _thumbNailPositionX, ref _thumbNailPositionY, ref _thumbNailWidth, ref _thumbNailHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textureBigWidth, ref _textureBigHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _positionXThumbBig, ref _positionYThumbBig, ref _widthThumbBig, ref _heightThumbBig);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _itemBigWidth, ref _itemBigHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _itemWidth, ref _itemHeight);

      _infoImagePositionX += GUIGraphicsContext.OverScanLeft;
      _infoImagePositionY += GUIGraphicsContext.OverScanTop;

      _backGroundPositionX += GUIGraphicsContext.OverScanLeft;
      _backGroundPositionY += GUIGraphicsContext.OverScanTop;
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _infoImagePositionX, ref _infoImagePositionY, ref _infoImageWidth, ref _infoImageHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _backGroundPositionX, ref _backGroundPositionY, ref _backGroundWidth, ref _backGroundHeight);
    }

    /// <summary>
    /// Method which is called if the user selected another item in the filmstrip
    /// This method will update the property manager with the properties of the
    /// newly selected item
    /// </summary>
    protected void OnSelectionChanged()
    {
      if (!IsVisible) return;

      _scrollPosition = 0;
      _scrollPosititionX = 0;
      _lastItem = -1;

      _scrollOffset = 0.0f;
      _currentFrame = 0;
      _timeElapsed = 0.0f;

      // Reset searchstring
      if (_lastSearchItem != (_cursorX + _offset))
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
        _searchString = "";
      }

      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      int item = GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb);
      if (!GUIWindowManager.IsRouted)
      {
        GUIPropertyManager.SetProperty("#selecteditem", strSelected);
        GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
        GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
      }
      if (!IsVisible) return;
      if (item >= 0 && item < _listItems.Count)
      {
        GUIListItem listitem = _listItems[item];
        if (listitem != null) listitem.ItemSelected(this);
      }

      // ToDo: add searchstring property
      if (_searchString.Length > 0)
        GUIPropertyManager.SetProperty("#selecteditem", "{" + _searchString.ToLower() + "}");
    }


    /// <summary>
    /// Method to render a single item of the filmstrip
    /// </summary>
    /// <param name="bFocus">true if item shown be drawn focused, false for normal mode</param>
    /// <param name="dwPosX">x-coordinate of the item</param>
    /// <param name="dwPosY">y-coordinate of the item</param>
    /// <param name="pItem">item itself</param>
    void RenderItem(float timePassed, bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem)
    {
      if (_font == null) return;
      if (pItem == null) return;
      if (dwPosY < 0) return;
      if (_imageFolderFocus == null) return;
      if (_imageFolder == null) return;

      float fTextHeight = 0, fTextWidth = 0;
      _font.GetTextExtent("W", ref fTextWidth, ref fTextHeight);

      float fTextPosY = (float)dwPosY + (float)_textureHeight;

      long dwColor = _textColor;
      if (pItem.Selected) dwColor = _selectedColor;
      if (!bFocus && Focus) dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
      if (pItem.IsRemote)
      {
        dwColor = _remoteColor;
        if (pItem.IsDownloading) dwColor = _downloadColor;
      }
      if (!Focus) dwColor &= DimColor;

      if (bFocus == true && Focus && _listType == GUIListControl.ListType.CONTROL_LIST)
      {
        _imageFolderFocus.SetPosition(dwPosX, dwPosY);
        if (true == _showTexture) _imageFolderFocus.Render(timePassed);

        RenderText((float)dwPosX, fTextPosY, dwColor, pItem.Label, true);
      }
      else
      {
        _imageFolder.SetPosition(dwPosX, dwPosY);
        if (true == _showTexture) _imageFolder.Render(timePassed);

        RenderText((float)dwPosX, fTextPosY, dwColor, pItem.Label, false);

      }

      // Set oversized value
      int iOverSized = 0;
      if (bFocus && Focus)
      {
        iOverSized = (_thumbNailWidth + _thumbNailHeight) / THUMBNAIL_OVERSIZED_DIVIDER;
      }

      if (pItem.HasThumbnail)
      {
        GUIImage pImage = pItem.Thumbnail;
        if (null == pImage && _sleeper == 0 && !IsAnimating)
        {
          pImage = new GUIImage(0, 0, _thumbNailPositionX - iOverSized + dwPosX, _thumbNailPositionY - iOverSized + dwPosY, _thumbNailWidth + 2 * iOverSized, _thumbNailHeight + 2 * iOverSized, pItem.ThumbnailImage, 0x0);
          pImage.ParentControl = this;
          pImage.KeepAspectRatio = true;
          pImage.ZoomFromTop = !pItem.IsFolder;
          pImage.AllocResources();
          pItem.Thumbnail = pImage;
          int xOff = (_thumbNailWidth + 2 * iOverSized - pImage.RenderWidth) / 2;
          int yOff = (_thumbNailHeight + 2 * iOverSized - pImage.RenderHeight) / 2;
          pImage.SetPosition(_thumbNailPositionX - iOverSized + dwPosX + xOff, _thumbNailPositionY - iOverSized + dwPosY + yOff);
          pImage.DimColor = DimColor;
          if (bFocus || !Focus) pImage.ColourDiffuse = 0xffffffff;
          else pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          pImage.Render(timePassed);
          _sleeper += SLEEP_FRAME_COUNT;
        }
        if (null != pImage)
        {
          if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
          {
            pImage.FreeResources();
            pImage.AllocResources();
          }
          pImage.ZoomFromTop = !pItem.IsFolder;
          pImage.Width = _thumbNailWidth + 2 * iOverSized;
          pImage.Height = _thumbNailHeight + 2 * iOverSized;
          int xOff = (_thumbNailWidth + 2 * iOverSized - pImage.RenderWidth) / 2;
          int yOff = (_thumbNailHeight + 2 * iOverSized - pImage.RenderHeight) / 2;
          pImage.SetPosition(_thumbNailPositionX + dwPosX - iOverSized + xOff, _thumbNailPositionY - iOverSized + dwPosY + yOff);
          pImage.DimColor = DimColor;
          if (bFocus || !Focus) pImage.ColourDiffuse = 0xffffffff;
          else pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          pImage.Render(timePassed);
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          GUIImage pImage = pItem.IconBig;
          if (null == pImage && _sleeper == 0 && !IsAnimating)
          {
            pImage = new GUIImage(0, 0, _thumbNailPositionX - iOverSized + dwPosX, _thumbNailPositionY - iOverSized + dwPosY, _thumbNailWidth + 2 * iOverSized, _thumbNailHeight + 2 * iOverSized, pItem.IconImageBig, 0x0);
            pImage.ParentControl = this;
            pImage.KeepAspectRatio = true;
            pImage.ZoomFromTop = !pItem.IsFolder;
            pImage.AllocResources();
            pItem.IconBig = pImage;
            int xOff = (_thumbNailWidth + 2 * iOverSized - pImage.RenderWidth) / 2;
            int yOff = (_thumbNailHeight + 2 * iOverSized - pImage.RenderHeight) / 2;
            pImage.SetPosition(_thumbNailPositionX + dwPosX - iOverSized + xOff, _thumbNailPositionY - iOverSized + dwPosY + yOff);
            pImage.DimColor = DimColor;
            if (bFocus || !Focus) pImage.ColourDiffuse = 0xffffffff;
            else pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            pImage.Render(timePassed);
            _sleeper += SLEEP_FRAME_COUNT;
          }
          if (null != pImage)
          {
            pImage.ZoomFromTop = !pItem.IsFolder;
            pImage.Width = _thumbNailWidth + 2 * iOverSized;
            pImage.Height = _thumbNailHeight + 2 * iOverSized;
            int xOff = (_thumbNailWidth + 2 * iOverSized - pImage.RenderWidth) / 2;
            int yOff = (_thumbNailHeight + 2 * iOverSized - pImage.RenderHeight) / 2;
            pImage.SetPosition(_thumbNailPositionX - iOverSized + dwPosX + xOff, _thumbNailPositionY - iOverSized + dwPosY + yOff);
            pImage.DimColor = DimColor;
            if (bFocus || !Focus) pImage.ColourDiffuse = 0xffffffff;
            else pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            pImage.Render(timePassed);
          }
        }
      }
    }

    /// <summary>
    /// The render method. 
    /// This method will draw the entire filmstrip
    /// </summary>
    public override void Render(float timePassed)
    {
      _timeElapsed += timePassed;
      _currentFrame = (int)(_timeElapsed / TimeSlice);

      if (null == _font) return;
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = 0;
      if ((_cursorX > 0) && !ValidItem(_cursorX))
      {
        _cursorX = 0;
        OnSelectionChanged();
      }

      if (_sleeper > 0) _sleeper--;

      UpdateInfoImage();
      if (_imageBackground != null)
      {
        _imageBackground.Render(timePassed);
      }

      if (_horizontalScrollbar != null)
      {
        float fPercent = (float)_offset + _cursorX;
        fPercent /= (float)(_listItems.Count);
        fPercent *= 100.0f;
        if ((int)fPercent != (int)_horizontalScrollbar.Percentage)
        {
          _horizontalScrollbar.Percentage = fPercent;
        }
      }

      int _scrollPosititionXOffset = 0;
      if (true == _scrollingRight)
      {
        _scrollPosititionXOffset = -(_itemWidth - _scrollCounter);
      }
      if (true == _scrollingLeft)
      {
        _scrollPosititionXOffset = _itemWidth - _scrollCounter;
      }

      Viewport oldview = GUIGraphicsContext.DX9Device.Viewport;
      Viewport view = new Viewport();
      float fx = (float)_positionX;
      float fy = (float)_positionY;
      GUIGraphicsContext.Correct(ref fx, ref fy);

      if (fx <= 0) fx = 0;
      if (fy <= 0) fy = 0;
      view.X = (int)fx;
      view.Y = (int)fy;
      view.Width = _columns * _itemWidth;
      view.Height = _itemHeight;
      view.MinZ = 0.0f;
      view.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = view;

      int iStartItem = 30000;
      int iEndItem = -1;
      dwPosY = _positionY;
      int dwPosX;

      //are we scrolling towards the left?
      if (_scrollingLeft)
      {
        // yes, then render item on right
        dwPosX = _positionX - _itemWidth + _scrollPosititionXOffset;
        int iItem = _offset - 1;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = _listItems[iItem];
          RenderItem(timePassed, false, dwPosX, dwPosY, pItem);
        }
      }

      // render main panel
      for (int iCol = 0; iCol < _columns; iCol++)
      {
        dwPosX = _positionX + iCol * _itemWidth + _scrollPosititionXOffset;
        int iItem = iCol + _offset;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = _listItems[iItem];
          bool bFocus = (_cursorX == iCol);
          RenderItem(timePassed, bFocus, dwPosX, dwPosY, pItem);
        }
      }


      //are we scrolling towards the right?
      if (_scrollingRight)
      {
        // yes, then render the new left item
        dwPosX = _positionX + _columns * _itemWidth + _scrollPosititionXOffset;
        _offset++;
        int iItem = _columns + _offset - 1;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = _listItems[iItem];
          RenderItem(timePassed, false, dwPosX, dwPosY, pItem);
        }
        _offset--;
      }
      GUIGraphicsContext.DX9Device.Viewport = oldview;

      //
      _frames = 12;
      int iStep = _itemHeight / _frames;
      if (0 == iStep) iStep = 1;
      if (_scrollingLeft)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingLeft = false;
          _offset--;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0) iPage++;
          _upDownControl.Value = iPage + 1;
          _refresh = true;
          OnSelectionChanged();
        }
      }

      if (_scrollingRight)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingRight = false;
          _offset++;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0) iPage++;
          _upDownControl.Value = iPage + 1;
          _refresh = true;
          OnSelectionChanged();
        }
      }

      if (_imageInfo != null)
      {
        _imageInfo.Render(timePassed);
      }
      //free memory
      if (iStartItem < 30000)
      {
        for (int i = 0; i < iStartItem; ++i)
        {
          if (i >= 0 && i < _listItems.Count)
          {
            GUIListItem pItem = _listItems[i];
            if (null != pItem)
            {
              pItem.FreeMemory();
            }
          }
        }
      }

      for (int i = iEndItem + 1; i < _listItems.Count; ++i)
      {
        if (i >= 0 && i < _listItems.Count)
        {
          GUIListItem pItem = _listItems[i];
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      dwPosY = _positionY + (_itemHeight);

      if (_upDownControl != null) _upDownControl.Render(timePassed);
      if (_scrollingLeft || _scrollingRight)
      {
        _refresh = true;
      }
      int iItemsPerPage = _columns;
      if (_listItems.Count > iItemsPerPage && _horizontalScrollbar != null)
      {
        _horizontalScrollbar.Render(timePassed);
      }
      if (Focus)
        GUIPropertyManager.SetProperty("#highlightedbutton", String.Empty);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            _offset = 0;
            _cursorX = 0;
            _upDownControl.Value = 1;
            OnSelectionChanged();

            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            int iItem = _listItems.Count - 1;
            if (iItem >= 0)
            {
              int iPage = 1;
              _cursorX = 0;
              _offset = 0;
              while (iItem >= (_columns))
              {
                _offset += (_columns);
                iItem -= (_columns);
                iPage++;
              }
              if (_upDownControl != null) _upDownControl.Value = iPage;
              _cursorX = iItem;
              OnSelectionChanged();
            }
            _refresh = true;
          }
          break;


        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            _searchString = "";
            OnDown();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            _searchString = "";
            if (!OnUp())
            {
              base.OnAction(action);
              return;
            }
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            _searchString = "";
            OnLeft();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            _searchString = "";
            OnRight();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if (action.m_key != null)
            {
              // Check key
              if (((action.m_key.KeyChar >= '0') && (action.m_key.KeyChar <= '9')) ||
                action.m_key.KeyChar == '*' || action.m_key.KeyChar == '(' || action.m_key.KeyChar == '#' || action.m_key.KeyChar == '�')
              {
                Press((char)action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int)Keys.Back)
              {
                if (_searchString.Length > 0)
                  _searchString = _searchString.Remove(_searchString.Length - 1, 1);
                SearchItem(_searchString, SearchType.SEARCH_FIRST);
              }
              if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) || (action.m_key.KeyChar == (int)Keys.Space))
              {
                if (action.m_key.KeyChar == (int)Keys.Space && _searchString == String.Empty) return;
                _searchString += (char)action.m_key.KeyChar;
                SearchItem(_searchString, SearchType.SEARCH_FIRST);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int id;
            bool focus;
            if (_horizontalScrollbar != null)
            {
              if (_horizontalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
              {
                ///TODO: add horz. scrollbar to filmstrip
              }
              //          _drawFocus=true;
            }
          }
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            int id;
            bool focus;
            if (_horizontalScrollbar != null)
            {
              if (_horizontalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
              {
                ///TODO: add horz. scrollbar to filmstrip
                return;
              }
            }

            //_drawFocus=true;
            if (_listType == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int)Action.ActionType.ACTION_SELECT_ITEM, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
            else
            {
              if (_upDownControl != null) _upDownControl.OnAction(action);
            }
            _refresh = true;
          }
          break;

        default:
          {
            if (_listType == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int)action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
              _searchString = "";
              _refresh = true;
            }
            else
            {
              if (_upDownControl != null) _upDownControl.OnAction(action);
              _refresh = true;
            }
          }
          break;
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.SenderControlId == 0)
        {
          if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
          {
            _offset = (_upDownControl.Value - 1) * (_columns);
            _refresh = true;
            OnSelectionChanged();
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = _offset + _cursorX;
          if (iItem >= 0 && iItem < _listItems.Count)
          {
            message.Object = _listItems[iItem];
          }
          else
          {
            message.Object = null;
          }
          _refresh = true;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in _listItems)
          {
            item.FreeMemory();
          }
          _refresh = true;
          if (_imageInfo != null)
          {
            _imageInfo.FreeResources();
            _imageInfo.AllocResources();
            _imageInfo.DoUpdate();
          }
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < _listItems.Count)
          {
            message.Object = _listItems[iItem];
          }
          else
          {
            message.Object = null;
          }
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
          _listType = GUIListControl.ListType.CONTROL_LIST;
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem newItem = message.Object as GUIListItem;
          if (newItem != null)
          {
            _listItems.Add(newItem);
            int iItemsPerPage = _columns;
            int iPages = _listItems.Count / iItemsPerPage;
            if ((_listItems.Count % iItemsPerPage) != 0) iPages++;
            if (_upDownControl != null)
            {
              _upDownControl.SetRange(1, iPages);
              _upDownControl.Value = 1;
            }
            _refresh = true;
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
          message.Param1 = _offset + _cursorX;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < _listItems.Count)
          {
            int iPage = 1;
            _cursorX = 0;
            _offset = 0;
            while (iItem >= (_columns))
            {
              _offset += (_columns);
              iItem -= (_columns);
              iPage++;
            }
            if (_upDownControl != null) _upDownControl.Value = iPage;
            _cursorX = iItem;
            OnSelectionChanged();
          }
          _refresh = true;
        }
      }

			if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
			{
				foreach (GUIListItem item in _listItems)
				{
					item.Selected = false;
				}
				foreach (GUIListItem item in _listItems)
				{
					if (item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase))
					{
						item.Selected = true;
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

      if (base.OnMessage(message)) return true;

      return false;
    }

    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="SearchKey">SearchKey</param>
    void SearchItem(string SearchKey, SearchType iSearchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = _offset + _cursorX;
      if (iSearchMethode == SearchType.SEARCH_FIRST) iCurrentItem = 0;
      int iItem = iCurrentItem;
      do
      {
        if (iSearchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= _listItems.Count) iItem = 0;
        }
        if (iSearchMethode == SearchType.SEARCH_PREV && _listItems.Count > 0)
        {
          iItem--;
          if (iItem < 0) iItem = _listItems.Count - 1;
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
          if (iItem >= _listItems.Count) iItem = 0;
        }
      } while (iItem != iCurrentItem);

      if ((bItemFound) && (iItem >= 0 && iItem < _listItems.Count))
      {
        // update spin controls
        int iPage = 1;
        _cursorX = 0;
        _offset = 0;
        while (iItem >= (_columns))
        {
          _offset += (_columns);
          iItem -= (_columns);
          iPage++;
        }
        if (_upDownControl != null) _upDownControl.Value = iPage;
        _cursorX = iItem;
      }

      _lastSearchItem = _cursorX + _offset;
      OnSelectionChanged();
      _refresh = true;
    }


    /// <summary>
    /// Handle keypress events for SMS style search (key '1'..'9')
    /// </summary>
    /// <param name="Key"></param>
    void Press(char Key)
    {
      // Check key timeout
      CheckTimer();

      // Check different key pressed
      if ((Key != _previousKey) && (Key >= '1' && Key <= '9'))
        _currentKey = (char)0;

      if (Key == '*' || Key == '(')
      {
        // Backspace
        if (_searchString.Length > 0)
          _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        _previousKey = (char)0;
        _currentKey = (char)0;
        _timerKey = DateTime.Now;
      }
      if (Key == '#' || Key == '�')
      {
        _timerKey = DateTime.Now;
      }
      else if (Key == '1')
      {
        if (_currentKey == 0) _currentKey = ' ';
        else if (_currentKey == ' ') _currentKey = '!';
        else if (_currentKey == '!') _currentKey = '?';
        else if (_currentKey == '?') _currentKey = '.';
        else if (_currentKey == '.') _currentKey = '0';
        else if (_currentKey == '0') _currentKey = '1';
        else if (_currentKey == '1') _currentKey = '-';
        else if (_currentKey == '-') _currentKey = '+';
        else if (_currentKey == '+') _currentKey = ' ';
      }
      else if (Key == '2')
      {
        if (_currentKey == 0) _currentKey = 'a';
        else if (_currentKey == 'a') _currentKey = 'b';
        else if (_currentKey == 'b') _currentKey = 'c';
        else if (_currentKey == 'c') _currentKey = '2';
        else if (_currentKey == '2') _currentKey = 'a';
      }
      else if (Key == '3')
      {
        if (_currentKey == 0) _currentKey = 'd';
        else if (_currentKey == 'd') _currentKey = 'e';
        else if (_currentKey == 'e') _currentKey = 'f';
        else if (_currentKey == 'f') _currentKey = '3';
        else if (_currentKey == '3') _currentKey = 'd';
      }
      else if (Key == '4')
      {
        if (_currentKey == 0) _currentKey = 'g';
        else if (_currentKey == 'g') _currentKey = 'h';
        else if (_currentKey == 'h') _currentKey = 'i';
        else if (_currentKey == 'i') _currentKey = '4';
        else if (_currentKey == '4') _currentKey = 'g';
      }
      else if (Key == '5')
      {
        if (_currentKey == 0) _currentKey = 'j';
        else if (_currentKey == 'j') _currentKey = 'k';
        else if (_currentKey == 'k') _currentKey = 'l';
        else if (_currentKey == 'l') _currentKey = '5';
        else if (_currentKey == '5') _currentKey = 'j';
      }
      else if (Key == '6')
      {
        if (_currentKey == 0) _currentKey = 'm';
        else if (_currentKey == 'm') _currentKey = 'n';
        else if (_currentKey == 'n') _currentKey = 'o';
        else if (_currentKey == 'o') _currentKey = '6';
        else if (_currentKey == '6') _currentKey = 'm';
      }
      else if (Key == '7')
      {
        if (_currentKey == 0) _currentKey = 'p';
        else if (_currentKey == 'p') _currentKey = 'q';
        else if (_currentKey == 'q') _currentKey = 'r';
        else if (_currentKey == 'r') _currentKey = 's';
        else if (_currentKey == 's') _currentKey = '7';
        else if (_currentKey == '7') _currentKey = 'p';
      }
      else if (Key == '8')
      {
        if (_currentKey == 0) _currentKey = 't';
        else if (_currentKey == 't') _currentKey = 'u';
        else if (_currentKey == 'u') _currentKey = 'v';
        else if (_currentKey == 'v') _currentKey = '8';
        else if (_currentKey == '8') _currentKey = 't';
      }
      else if (Key == '9')
      {
        if (_currentKey == 0) _currentKey = 'w';
        else if (_currentKey == 'w') _currentKey = 'x';
        else if (_currentKey == 'x') _currentKey = 'y';
        else if (_currentKey == 'y') _currentKey = 'z';
        else if (_currentKey == 'z') _currentKey = '9';
        else if (_currentKey == '9') _currentKey = 'w';
      }

      if (Key >= '1' && Key <= '9')
      {
        // Check different key pressed
        if (Key == _previousKey)
        {
          if (_searchString.Length > 0)
            _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        }
        _previousKey = Key;
        _searchString += _currentKey;
      }
      SearchItem(_searchString, SearchType.SEARCH_FIRST);
      _timerKey = DateTime.Now;
    }

    void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 1000)
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
        //        _searchString = string.Empty;
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      if (_upDownControl != null) _upDownControl.PreAllocResources();
      if (_imageFolder != null) _imageFolder.PreAllocResources();
      if (_imageFolderFocus != null) _imageFolderFocus.PreAllocResources();
      if (_horizontalScrollbar != null) _horizontalScrollbar.PreAllocResources();
      if (_imageBackground != null) _imageBackground.PreAllocResources();
      if (_imageInfo != null) _imageInfo.PreAllocResources();
      _idleTimer = DateTime.Now;
      _infoChanged = false;
      _newInfoImageName = "";
      _offset = 0;
      _cursorX = 0;
    }


    void Calculate()
    {
      if (_imageFolder != null)
      {
        _imageFolder.Width = _textureWidth;
        _imageFolder.Height = _textureHeight;
      }

      if (_imageFolderFocus != null)
      {
        _imageFolderFocus.Width = _textureWidth;
        _imageFolderFocus.Height = _textureHeight;
      }

      float fWidth = 0, fHeight = 0;
      if (_font != null)
      {
        // height of 1 item = folder image height + text row height + space in between
        _font.GetTextExtent("y", ref fWidth, ref fHeight);
      }

      fWidth = (float)_itemWidth;
      fHeight = (float)_itemHeight;

      _columns = (int)(_width / fWidth);

      int iItemsPerPage = _columns;
      int iPages = _listItems.Count / iItemsPerPage;
      if ((_listItems.Count % iItemsPerPage) != 0) iPages++;
      if (_upDownControl != null)
      {
        _upDownControl.SetRange(1, iPages);
        _upDownControl.Value = 1;
      }
    }

    public override void AllocResources()
    {
      _sleeper = 0;

      base.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      if (_imageBackground != null) _imageBackground.AllocResources();
      if (_imageInfo != null) _imageInfo.AllocResources();
      if (_upDownControl != null) _upDownControl.AllocResources();
      if (_imageFolder != null) _imageFolder.AllocResources();
      if (_imageFolderFocus != null) _imageFolderFocus.AllocResources();
      if (_horizontalScrollbar != null) _horizontalScrollbar.AllocResources();
      Calculate();

      _upDownControl.ParentID = GetID;
    }

    public override void FreeResources()
    {
      foreach (GUIListItem item in _listItems)
      {
        item.FreeIcons();
      }
      _listItems.Clear();
      base.FreeResources();
      if (_imageBackground != null) _imageBackground.FreeResources();
      if (_imageInfo != null) _imageInfo.FreeResources();
      if (_upDownControl != null) _upDownControl.FreeResources();
      if (_imageFolder != null) _imageFolder.FreeResources();
      if (_imageFolderFocus != null) _imageFolderFocus.FreeResources();
      if (_horizontalScrollbar != null) _horizontalScrollbar.FreeResources();
    }


    bool ValidItem(int iX)
    {
      if (iX >= _columns) return false;
      if (_offset + iX < _listItems.Count) return true;
      return false;
    }


    void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_scrollingRight)
        {
          _scrollingRight = false;
          _offset++;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0) iPage++;
          if (_upDownControl != null) _upDownControl.Value = iPage + 1;
        }

        if (_cursorX + 1 == _columns)
        {
          _offset++;
          if (!ValidItem(_cursorX))
          {
            _offset--;
          }
          else
          {
            _offset--;
            _scrollCounter = _itemWidth;
            _scrollingRight = true;
          }

          OnSelectionChanged();
          return;
        }
        else
        {
          if (ValidItem(_cursorX + 1))
          {
            _cursorX++;
          }
          OnSelectionChanged();
        }
      }
      else if (_upDownControl != null)
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
        {
          _upDownControl.Focus = false;
          _listType = GUIListControl.ListType.CONTROL_LIST;
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
          if (!_upDownControl.Focus)
          {
            base.OnAction(action);
          }
        }
        OnSelectionChanged();
      }
    }

    void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_scrollingLeft)
        {
          _scrollCounter = 0;
          _scrollingLeft = false;
          _offset--;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0) iPage++;
          if (_upDownControl != null) _upDownControl.Value = iPage + 1;
        }

        if (_cursorX > 0)
        {
          _cursorX--;
        }
        else if (_cursorX == 0 && _offset != 0)
        {
          _scrollCounter = _itemWidth;
          _scrollingLeft = true;
        }
        else
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
      else if (_upDownControl != null)
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN)
        {
          _upDownControl.Focus = false;
          _listType = GUIListControl.ListType.CONTROL_LIST;
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        if (!_upDownControl.Focus)
        {
          _listType = GUIListControl.ListType.CONTROL_LIST;
        }
        OnSelectionChanged();
      }
    }

    bool OnUp()
    {
      OnSelectionChanged();
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        return false;
      }
      else if (_upDownControl != null)
      {
        _upDownControl.OnAction(action);
        if (!_upDownControl.Focus)
        {
          _listType = GUIListControl.ListType.CONTROL_LIST;
        }
      }
      return true;
    }

    void OnDown()
    {
      OnSelectionChanged();
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        _listType = GUIListControl.ListType.CONTROL_UPDOWN;
        if (_upDownControl != null) _upDownControl.Focus = true;
      }
      else if (_upDownControl != null)
      {
        _upDownControl.OnAction(action);
        if (!_upDownControl.Focus)
        {
          base.OnAction(action);
        }
      }
    }


    void RenderText(float fPosX, float fPosY, long dwTextColor, string wszText, bool bScroll)
    {
      if (_font == null) return;
      if (wszText == null) return;
      if (wszText == String.Empty) return;

      float fTextHeight = 0, fTextWidth = 0;
      float fwidth, fWidth = 0, fHeight = 0;
      _font.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);
      float fMaxWidth = _itemWidth - _itemWidth / 10.0f;
      float fPosCX = fPosX;
      float fPosCY = fPosY;
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height)
        fPosCY = (float)GUIGraphicsContext.Height;
      fHeight = 60.0f;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return;

      fwidth = fMaxWidth - 5.0f;
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;

      int minX = _positionX;
      if (fPosCX < minX) fPosCX = minX;
      int maxX = _positionX + _columns * _itemWidth;
      if (fwidth + fPosCX > maxX)
        fwidth = ((float)maxX) - fPosCX;
      Viewport newviewport = new Viewport();
      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int)fPosCX;
      newviewport.Y = (int)fPosCY;
      newviewport.Width = (int)(fwidth);
      newviewport.Height = (int)(fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;

      if (!bScroll || fTextWidth <= fMaxWidth)
      {
        _font.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int)fMaxWidth);

        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
        return;
      }
      else
      {
        // scroll
        _brackedText = wszText;
        _brackedText += (" " + _suffix + " ");
        _font.GetTextExtent(_brackedText, ref fTextWidth, ref fTextHeight);

        int iItem = _cursorX + _offset;
        if (fTextWidth > fMaxWidth)
        {
          fMaxWidth += 50.0f;
          _scollText = "";
          if (_lastItem != iItem)
          {
            _scrollPosition = 0;
            _lastItem = iItem;
            _scrollPosititionX = 0;
            _scrollOffset = 0.0f;
            _currentFrame = 0;
            _timeElapsed = 0.0f;
            _scrollContinuosly = false;
          }
          //          if (iStartFrame > 50)
          if ((_currentFrame > 25 + 12) || _scrollContinuosly)
          {
            if (_scrollContinuosly)
            {
              _scrollPosititionX = _currentFrame;
            }
            else
            {
              _scrollPosititionX = _currentFrame - (25 + 12);
            }
            char wTmp;
            if (_scrollPosition >= _brackedText.Length)
              wTmp = ' ';
            else
              wTmp = _brackedText[_scrollPosition];

            _font.GetTextExtent(wTmp.ToString(), ref fWidth, ref fHeight);
            if (_scrollPosititionX - _scrollOffset >= fWidth)
            {
              ++_scrollPosition;
              if (_scrollPosition > _brackedText.Length)
              {
                _scrollPosition = 0;
                _scrollPosititionX = 0;
                _scrollOffset = 0.0f;
                _currentFrame = 0;
                _timeElapsed = 0.0f;
                _scrollContinuosly = true;
              }
              else
              {
                _scrollOffset += fWidth;
              }
            }
            int ipos = 0;
            for (int i = 0; i < _brackedText.Length; i++)
            {
              if (i + _scrollPosition < _brackedText.Length)
                _scollText += _brackedText[i + _scrollPosition];
              else
              {
                if (ipos == 0) _scollText += ' ';
                else _scollText += _brackedText[ipos - 1];
                ipos++;
              }
            }
            if (fPosY >= 0.0)
            {
              //              _font.DrawText((int) (fPosX - _scrollPosititionX + _scrollOffset), fPosY, dwTextColor, _scollText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f));
              _font.DrawText((int)(fPosX - _scrollPosititionX + _scrollOffset), fPosY, dwTextColor, _scollText, GUIControl.Alignment.ALIGN_LEFT, (int)(fMaxWidth - 50f + _scrollPosititionX - _scrollOffset));
            }
          }
          else
          {
            if (fPosY >= 0.0)
              _font.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int)(fMaxWidth - 50f));
          }
        }

        GUIGraphicsContext.DX9Device.Viewport = oldviewport;


      }
    }

    public string ScrollySuffix
    {
      get { return _suffix; }
      set
      {
        if (value == null) return;
        _suffix = value;
      }
    }


    void OnPageUp()
    {
      if (_upDownControl == null) return;
      int iPage = _upDownControl.Value;
      if (iPage > 1)
      {
        iPage--;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1) * _columns;
        OnSelectionChanged();
      }
    }

    void OnPageDown()
    {
      if (_upDownControl == null) return;
      int iItemsPerPage = _columns;
      int iPages = _listItems.Count / iItemsPerPage;
      if ((_listItems.Count % iItemsPerPage) != 0) iPages++;

      int iPage = _upDownControl.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1) * iItemsPerPage;
      }
      while (_cursorX > 0 && _offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      OnSelectionChanged();

    }


    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      _textureWidth = iWidth;
      _textureHeight = iHeight;


      if (_imageFolder != null)
      {
        _imageFolder.Height = _textureHeight;
        _imageFolder.Width = _textureWidth;
        _imageFolder.Refresh();
      }

      if (_imageFolderFocus != null)
      {
        _imageFolderFocus.Width = _textureWidth;
        _imageFolderFocus.Height = _textureHeight;
        _imageFolderFocus.Refresh();
      }

    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      _thumbNailWidth = iWidth;
      _thumbNailHeight = iHeight;
      _thumbNailPositionX = iXpos;
      _thumbNailPositionY = iYpos;
    }

    public void GetThumbDimensions(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iWidth = _thumbNailWidth;
      iHeight = _thumbNailHeight;
      iXpos = _thumbNailPositionX;
      iYpos = _thumbNailPositionY;
    }


    public int ItemWidth
    {
      get { return _itemWidth; }
      set
      {
        if (value < 0) return;
        _itemWidth = value;
        if (_itemWidth < 1) _itemWidth = 1;
        FreeResources();
        AllocResources();
      }
    }

    public int ItemHeight
    {
      get { return _itemHeight; }
      set
      {
        if (value < 0) return;
        _itemHeight = value;
        if (_itemHeight < 1) _itemHeight = 1;
        FreeResources();
        AllocResources();
      }
    }

    public bool ShowTexture
    {
      get { return _showTexture; }
      set { _showTexture = value; }
    }


    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumbnail)
    {
      strLabel = "";
      strLabel2 = "";
      strThumbnail = "";
      int iItem = _offset + _cursorX;
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        if (pItem != null)
        {
          strLabel = pItem.Label;
          strLabel2 = pItem.Label2;
          if (pItem.IsFolder)
          {
            strLabel = String.Format("{0}{1}{2}", _folderPrefix, pItem.Label, _folderSuffix);
          }
          strThumbnail = pItem.ThumbnailImage;
        }
      }
      return iItem;
    }


    public void ShowBigIcons(bool bOnOff)
    {
      if (bOnOff)
      {
        _itemWidth = _itemBigWidth;
        _itemHeight = _itemBigHeight;
        _textureWidth = _textureBigWidth;
        _textureHeight = _textureBigHeight;
        SetThumbDimensions(_positionXThumbBig, _positionYThumbBig, _widthThumbBig, _heightThumbBig);
        SetTextureDimensions(_textureWidth, _textureHeight);
      }
      else
      {
        _itemWidth = _itemLowWidth;
        _itemHeight = _itemLowHeight;
        _textureWidth = _textureLowWidth;
        _textureHeight = _textureLowHeight;
        SetThumbDimensions(_lowThumbNailPositionX, _lowThumbNailPositionY, _lowThumbNailPositionWidth, _lowThumbNailPositionHeight);
        SetTextureDimensions(_textureWidth, _textureHeight);
      }
      Calculate();
      _refresh = true;
    }


    public void GetThumbDimensionsBig(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = _positionXThumbBig;
      iYpos = _positionYThumbBig;
      iWidth = _widthThumbBig;
      iHeight = _heightThumbBig;
    }

    public void GetThumbDimensionsLow(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = _lowThumbNailPositionX;
      iYpos = _lowThumbNailPositionY;
      iWidth = _lowThumbNailPositionWidth;
      iHeight = _lowThumbNailPositionHeight;
    }

    public long TextColor
    {
      get { return _textColor; }
    }

    public long SelectedColor
    {
      get { return _selectedColor; }
    }

    public string FontName
    {
      get { return _fontName; }
    }

    public int SpinWidth
    {
      get
      {
        if (_upDownControl == null) return 0;
        return _upDownControl.Width / 2;
      }
    }

    public int SpinHeight
    {
      get
      {
        if (_upDownControl == null) return 0;
        return _upDownControl.Height;
      }
    }

    public string TexutureUpName
    {
      get
      {
        if (_upDownControl == null) return String.Empty;
        return _upDownControl.TexutureUpName;
      }
    }

    public string TexutureDownName
    {
      get
      {
        if (_upDownControl == null) return String.Empty;
        return _upDownControl.TexutureDownName;
      }
    }

    public string TexutureUpFocusName
    {
      get
      {
        if (_upDownControl == null) return String.Empty;
        return _upDownControl.TexutureUpFocusName;
      }
    }

    public string TexutureDownFocusName
    {
      get
      {
        if (_upDownControl == null) return String.Empty;
        return _upDownControl.TexutureDownFocusName;
      }
    }

    public long SpinTextColor
    {
      get
      {
        if (_upDownControl == null) return 0;
        return _upDownControl.TextColor;
      }
    }

    public int SpinX
    {
      get
      {
        if (_upDownControl == null) return 0;
        return _upDownControl.XPosition;
      }
    }

    public int SpinY
    {
      get
      {
        if (_upDownControl == null) return 0;
        return _upDownControl.YPosition;
      }
    }

    public int TextureWidth
    {
      get { return _textureWidth; }
    }

    public int TextureHeight
    {
      get { return _textureHeight; }
    }

    public string FocusName
    {
      get
      {
        if (_imageFolderFocus == null) return String.Empty;
        return _imageFolderFocus.FileName;
      }
    }

    public string NoFocusName
    {
      get
      {
        if (_imageFolder == null) return String.Empty;
        return _imageFolder.FileName;
      }
    }


    public int TextureWidthBig
    {
      get { return _textureBigWidth; }
      set
      {
        if (value < 0) return;
        _textureBigWidth = value;
        if (_textureBigWidth < 0) _textureBigWidth = 0;
      }
    }

    public int TextureHeightBig
    {
      get { return _textureBigHeight; }
      set
      {
        if (value < 0) return;
        _textureBigHeight = value;
        if (_textureBigHeight < 0) _textureBigHeight = 0;
      }
    }

    public int ItemWidthBig
    {
      get { return _itemBigWidth; }
      set
      {
        if (value < 0) return;
        _itemBigWidth = value;
        if (_itemBigWidth < 0) _itemBigWidth = 0;
      }
    }

    public int ItemHeightBig
    {
      get { return _itemBigHeight; }
      set
      {
        if (value < 0) return;
        _itemBigHeight = value;
        if (_itemBigHeight < 0) _itemBigHeight = 0;
      }
    }

    public int TextureWidthLow
    {
      get { return _textureLowWidth; }
      set
      {
        if (value < 0) return;
        _textureLowWidth = value;
        if (_textureLowWidth < 0) _textureLowWidth = 0;
      }
    }

    public int TextureHeightLow
    {
      get { return _textureLowHeight; }
      set
      {
        if (value < 0) return;
        _textureLowHeight = value;
        if (_textureLowHeight < 0) _textureLowHeight = 0;
      }
    }

    public int ItemWidthLow
    {
      get { return _itemLowWidth; }
      set
      {
        if (value < 0) return;
        _itemLowWidth = value;
        if (_itemLowWidth < 0) _itemLowWidth = 0;
      }
    }

    public int ItemHeightLow
    {
      get { return _itemLowHeight; }
      set
      {
        if (value < 0) return;
        _itemLowHeight = value;
        if (_itemLowHeight < 0) _itemLowHeight = 0;
      }
    }

    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      _lowThumbNailPositionX = iXpos;
      _lowThumbNailPositionY = iYpos;
      _lowThumbNailPositionWidth = iWidth;
      _lowThumbNailPositionHeight = iHeight;
    }

    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      _positionXThumbBig = iXpos;
      _positionYThumbBig = iYpos;
      _widthThumbBig = iWidth;
      _heightThumbBig = iHeight;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (_horizontalScrollbar != null)
      {
        if (_horizontalScrollbar.HitTest(x, y, out id, out focus)) return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        if (_upDownControl != null)
        {
          if (!_upDownControl.HitTest(x, y, out id, out focus))
            return false;
        }
      }

      if (_upDownControl != null)
      {
        if (_upDownControl.HitTest(x, y, out id, out focus))
        {
          if (_upDownControl.GetMaximum() > 1)
          {
            _listType = GUIListControl.ListType.CONTROL_UPDOWN;
            _upDownControl.Focus = true;
            if (!_upDownControl.Focus)
            {
              _listType = GUIListControl.ListType.CONTROL_LIST;
            }
            return true;
          }
          return true;
        }
      }

      _listType = GUIListControl.ListType.CONTROL_LIST;
      y -= _positionY;
      x -= _positionX;
      _cursorX = (x / _itemWidth);

      while (_cursorX > 0 && _offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      OnSelectionChanged();

      return true;
    }

    public void Sort(System.Collections.Generic.IComparer<GUIListItem> comparer)
    {
      try
      {
        _listItems.Sort(comparer);
      }
      catch (Exception)
      {
      }
      _refresh = true;
    }

    public override bool NeedRefresh()
    {
      if (_refresh)
      {
        _refresh = false;
        return true;
      }
      if (_scrollingLeft) return true;
      if (_scrollingRight) return true;
      return false;
    }

    public GUIverticalScrollbar Scrollbar
    {
      get { return _horizontalScrollbar; }
    }


    public override bool Focus
    {
      get { return IsFocused; }
			set
			{
				if (IsFocused != value && value)
				{
					if (_showTexture == true) _imageFolderFocus.Begin();
				}
				base.Focus = value; 
			}
    }

    public void Add(GUIListItem item)
    {
      if (item == null) return;
      _listItems.Add(item);
      int iItemsPerPage = _columns;
      int iPages = _listItems.Count / iItemsPerPage;
      if ((_listItems.Count % iItemsPerPage) != 0) iPages++;
      if (_upDownControl != null)
      {
        _upDownControl.SetRange(1, iPages);
        _upDownControl.Value = 1;
      }
      _refresh = true;
    }

    public int BackgroundX
    {
      get { return _backGroundPositionX; }
      set
      {
        if (value < 0) return;
        _backGroundPositionX = value;
        if (_imageBackground != null) _imageBackground.XPosition = value;
      }
    }

    public int BackgroundY
    {
      get { return _backGroundPositionY; }
      set
      {
        if (value < 0) return;
        _backGroundPositionY = value;
        if (_imageBackground != null) _imageBackground.YPosition = value;
      }
    }

    public int BackgroundWidth
    {
      get { return _backGroundWidth; }
      set
      {
        if (value < 0) return;
        _backGroundWidth = value;
        if (_imageBackground != null) _imageBackground.Width = value;
      }
    }

    public int BackgroundHeight
    {
      get { return _backGroundHeight; }
      set
      {
        if (value < 0) return;
        _backGroundHeight = value;
        if (_imageBackground != null) _imageBackground.Height = value;
      }
    }

    public long BackgroundDiffuse
    {
      get
      {
        if (_imageBackground != null) return _imageBackground.ColourDiffuse;
        return 0;
      }
      set { if (_imageBackground != null) _imageBackground.ColourDiffuse = value; }
    }

    public string BackgroundFileName
    {
      get
      {
        if (_imageBackground != null) return _imageBackground.FileName;
        return String.Empty;
      }
      set
      {
        if (value == null) return;
        if (_imageBackground != null) _imageBackground.SetFileName(value);
      }
    }


    public int InfoImageX
    {
      get { return _infoImagePositionX; }
      set
      {
        if (value < 0) return;
        _infoImagePositionX = value;
        if (_imageInfo != null) _imageInfo.XPosition = value;
      }
    }

    public int InfoImageY
    {
      get { return _infoImagePositionY; }
      set
      {
        if (value < 0) return;
        _infoImagePositionY = value;
        if (_imageInfo != null) _imageInfo.YPosition = value;
      }
    }

    public int InfoImageWidth
    {
      get { return _infoImageWidth; }
      set
      {
        if (value < 0) return;
        _infoImageWidth = value;
        if (_imageInfo != null) _imageInfo.Width = value;
      }
    }

    public int InfoImageHeight
    {
      get { return _infoImageHeight; }
      set
      {
        if (value < 0) return;
        _infoImageHeight = value;
        if (_imageInfo != null) _imageInfo.Height = value;
      }
    }

    public long InfoImageDiffuse
    {
      get
      {
        if (_imageInfo != null)
          return _imageInfo.ColourDiffuse;
        return 0;
      }
      set { if (_imageInfo != null) _imageInfo.ColourDiffuse = value; }
    }

    public string InfoImageFileName
    {
      get
      {
        if (_imageInfo != null)
          return _imageInfo.FileName;
        return String.Empty;
      }
      set
      {
        if (value == null) return;
        _newInfoImageName = value;
        _infoChanged = true;
        _idleTimer = DateTime.Now;
      }
    }

    void UpdateInfoImage()
    {
      if (_imageInfo == null) return;
      if (_infoChanged)
      {
        TimeSpan ts = DateTime.Now - _idleTimer;
        if (ts.TotalMilliseconds > 500)
        {
          _imageInfo.SetFileName(_newInfoImageName);
          _newInfoImageName = "";
          _infoChanged = false;
        }
      }
    }


    /// <summary>
    /// Method to store(save) the current control rectangle
    /// </summary>
    public override void StorePosition()
    {
      if (_imageInfo != null) _imageInfo.StorePosition();
      if (_upDownControl != null) _upDownControl.StorePosition();
      if (_imageBackground != null) _imageBackground.StorePosition();
      if (_imageFolder != null) _imageFolder.StorePosition();
      if (_imageFolderFocus != null) _imageFolderFocus.StorePosition();
      if (_horizontalScrollbar != null) _horizontalScrollbar.StorePosition();

      base.StorePosition();
    }

    /// <summary>
    /// Method to restore the saved-current control rectangle
    /// </summary>
    public override void ReStorePosition()
    {
      if (_imageInfo != null) _imageInfo.ReStorePosition();
      if (_upDownControl != null) _upDownControl.ReStorePosition();
      if (_imageBackground != null) _imageBackground.ReStorePosition();
      if (_imageFolder != null) _imageFolder.ReStorePosition();
      if (_imageFolderFocus != null) _imageFolderFocus.ReStorePosition();
      if (_horizontalScrollbar != null) _horizontalScrollbar.ReStorePosition();

      if (_imageInfo != null) _imageInfo.GetRect(out _infoImagePositionX, out _infoImagePositionY, out _infoImageWidth, out _infoImageHeight);
      if (_imageBackground != null) _imageBackground.GetRect(out _backGroundPositionX, out _backGroundPositionY, out _backGroundWidth, out _backGroundHeight);
      if (_upDownControl != null) _upDownControl.GetRect(out _spinControlPositionX, out _spinControlPositionY, out _spinControlWidth, out _spinControlHeight);

      base.ReStorePosition();
    }

    /// <summary>
    /// Method to get animate the current control
    /// </summary>
    public override void Animate(float timePassed, Animator animator)
    {
      if (animator == null) return;
      if (_imageInfo != null) _imageInfo.Animate(timePassed, animator);
      if (_upDownControl != null) _upDownControl.Animate(timePassed, animator);
      if (_imageBackground != null) _imageBackground.Animate(timePassed, animator);
      base.Animate(timePassed, animator);
    }


    /// <summary>
    /// Gets the ID of the control.
    /// </summary>
    public override int GetID
    {
      get { return _controlId; }
      set
      {
        _controlId = value;
        if (_upDownControl != null) _upDownControl.ParentID = value;
      }
    }

    public int Count
    {
      get { return _listItems.Count; }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= _listItems.Count) return null;
        return _listItems[index];
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = _offset + _cursorX;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          GUIListItem pItem = _listItems[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = _offset + _cursorX;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          return iItem;
        }
        return -1;
      }
    }

    public void Clear()
    {
      _listItems.Clear();
      //GUITextureManager.CleanupThumbs();
      _upDownControl.SetRange(1, 1);
      _upDownControl.Value = 1;
      _cursorX = _offset = 0;
      _refresh = true;
      OnSelectionChanged();
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageBackground != null) _imageBackground.DimColor = value;
        if (_imageInfo != null) _imageInfo.DimColor = value;
        if (_upDownControl != null) _upDownControl.DimColor = value;
        if (_imageFolder != null) _imageFolder.DimColor = value;
        if (_imageFolderFocus != null) _imageFolderFocus.DimColor = value;
        if (_horizontalScrollbar != null) _horizontalScrollbar.DimColor = value;
        foreach (GUIListItem item in _listItems) item.DimColor = value;
      }
    }

  }
}