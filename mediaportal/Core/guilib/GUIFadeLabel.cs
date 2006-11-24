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
using System.Collections;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying fading labels.
  /// </summary>
  public class GUIFadeLabel : GUIControl
  {
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("align")] Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("label")] protected string _label = "";

    ArrayList _listLabels = new ArrayList();
    int _currentLabelIndex = 0;
    int _scrollPosition = 0;
    double _scrollOffset = 0.0f;
    int _scrollPosititionX = 0;
    bool _fadeIn = false;
    int _currentFrame = 0;

    double timeElapsed = 0.0f;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal)*0.01f); }
    }

    bool _allowScrolling = true;
    bool _isScrolling = false;
    bool _containsProperty = false;

    string _previousText = "";
    GUILabelControl _labelControl = null;
    GUIFont _font = null;

    public GUIFadeLabel(int dwParentID) : base(dwParentID)
    {
    }

    /// <summary>
    /// The constructor of the GUIFadeLabel class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strFont">The indication of the font of this control.</param>
    /// <param name="dwTextColor">The color of this control.</param>
    /// <param name="dwTextAlign">The alignment of this control.</param>
    public GUIFadeLabel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strFont, long dwTextColor, Alignment dwTextAlign)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _fontName = strFont;
      _textColor = dwTextColor;
      _textAlignment = dwTextAlign;
      FinalizeConstruction();
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
      GUILocalizeStrings.LocalizeLabel(ref _label);
      _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName, _label, _textColor, _textAlignment, false);
      _labelControl.CacheFont = false;
      _labelControl.ParentControl = this;
      if (_fontName != "" && _fontName != "-")
        _font = GUIFontManager.GetFont(_fontName);
      if (_label.IndexOf("#") >= 0) _containsProperty = true;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }
      _isScrolling = false;
      if (_label != null && _label.Length > 0)
      {
        string strText = _label;
        if (_containsProperty) strText = GUIPropertyManager.Parse(_label);

        if (_previousText != strText)
        {
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _currentFrame = 0;
          timeElapsed = 0.0f;
          _fadeIn = true;
          _listLabels.Clear();
          _previousText = strText;
          strText = strText.Replace("\\r", "\r");
          int ipos = 0;
          do
          {
            ipos = strText.IndexOf("\r");
            int ipos2 = strText.IndexOf("\n");
            if (ipos >= 0 && ipos2 >= 0 && ipos2 < ipos) ipos = ipos2;
            if (ipos < 0 && ipos2 >= 0) ipos = ipos2;

            if (ipos >= 0)
            {
              string strLine = strText.Substring(0, ipos);
              if (strLine.Length > 1)
                _listLabels.Add(strLine);
              if (ipos + 1 >= strText.Length) break;
              strText = strText.Substring(ipos + 1);
            }
            else _listLabels.Add(strText);
          } while (ipos >= 0 && strText.Length > 0);
        }
      }

      // if there are no labels do not render
      if (_listLabels.Count == 0)
      {
        base.Render(timePassed);
        return;
      }

      // reset the current label is index is out of bounds
      if (_currentLabelIndex < 0 || _currentLabelIndex >= _listLabels.Count) _currentLabelIndex = 0;

      // get the current label
      string strLabel = (string) _listLabels[_currentLabelIndex];
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.Label = strLabel;
      _labelControl.SetPosition(_positionX, _positionY);
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextColor = _textColor;
      if (_labelControl.TextWidth < _width) _labelControl.CacheFont = true;
      else _labelControl.CacheFont = false;
      if (GUIGraphicsContext.graphics != null)
      {
        _labelControl.Render(timePassed);
				base.Render(timePassed);
        return;
      }

      // if there is only one label just draw the text
      if (_listLabels.Count == 1)
      {
        if (_labelControl.TextWidth < _width)
        {
          _labelControl.Render(timePassed);
					base.Render(timePassed);
          return;
        }
      }

      timeElapsed += timePassed;
      _currentFrame = (int) (timeElapsed / TimeSlice);

      // More than one label
      _isScrolling = true;


      // Make the label fade in
      if (_fadeIn && _allowScrolling)
      {
        long dwAlpha = (0xff/12)*_currentFrame;
        dwAlpha <<= 24;
        dwAlpha += (_textColor & 0x00ffffff);
        _labelControl.TextColor = dwAlpha;
        float fwt = 0;
        _labelControl.Label = GetShortenedText(strLabel, _width, ref fwt);
        if (_textAlignment == Alignment.ALIGN_RIGHT)
          _labelControl.Width = (int)(fwt);
        _labelControl.Render(timePassed);
        if (_currentFrame >= 12)
        {
          _fadeIn = false;
        }
      }
      // no fading
      else
      {
				long color = _textColor;
				if (Dimmed) color &= DimColor;
				if (!_allowScrolling)
        {
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _currentFrame = 0;
        }
        // render the text
        bool bDone = RenderText(timePassed, (float) _positionX, (float) _positionY, (float) _width, color, strLabel);
        if (bDone)
        {
          _currentLabelIndex++;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _fadeIn = true;
          _currentFrame = 0;
          timeElapsed = 0.0f;
          _currentFrame = 0;
        }
      }
      base.Render(timePassed);
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
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          _previousText = "";
          _listLabels.Clear();
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _fadeIn = true;
          _currentFrame = 0;
          timeElapsed = 0.0f;
          if (message.Label != null)
          {
            string strLabel = message.Label;
            if (strLabel.Length > 0)
            {
              _listLabels.Add(strLabel);
            }
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          if (message.Label != null)
          {
            string strLabel = message.Label;
            if (strLabel.Length > 0)
            {
              _listLabels.Add(strLabel);
            }
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          _previousText = "";
          _listLabels.Clear();
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _fadeIn = true;
          _currentFrame = 0;
          timeElapsed = 0.0f;
        }
      }
      return base.OnMessage(message);
    }


    /// <summary>
    /// Renders the text.
    /// </summary>
    /// <param name="fPosX">The X position of the text.</param>
    /// <param name="fPosY">The Y position of the text.</param>
    /// <param name="fMaxWidth">The maximum render width.</param>
    /// <param name="dwTextColor">The color of the text.</param>
    /// <param name="wszText">The actual text.</param>
    /// <returns>true if the render was successful</returns>
    bool RenderText(float timePassed, float fPosX, float fPosY, float fMaxWidth, long dwTextColor, string wszText)
    {
      bool bResult = false;
      float fTextHeight = 0, fTextWidth = 0;

      if (_font == null) return true;
			//Get the text width.
      _font.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);

			long color = _textColor;
			if (Dimmed) color &= DimColor;

      float fPosCX = fPosX;
      float fPosCY = fPosY;
      // Apply the screen calibration
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height)
        fPosCY = (float) GUIGraphicsContext.Height;


      float fWidth = 0;
      float fHeight = 60;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return true;

      if (_textAlignment == Alignment.ALIGN_RIGHT) fPosCX -= fMaxWidth;

      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.SetClip(new Rectangle((int) fPosCX, (int) fPosCY, (int) (fMaxWidth), (int) (fHeight)));
      }
      else
      {
        Viewport newviewport;
        newviewport = new Viewport();
        if (fMaxWidth < 1) return true;
        if (fHeight < 1) return true;

        newviewport.X = (int) fPosCX;
        newviewport.Y = (int) fPosCY;
        newviewport.Width = (int) (fMaxWidth);
        newviewport.Height = (int) (fHeight);
        newviewport.MinZ = 0.0f;
        newviewport.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = newviewport;
      }
      // scroll
      string wszOrgText = wszText;

      if (_textAlignment != Alignment.ALIGN_RIGHT)
      {
        do
        {
          _font.GetTextExtent(wszOrgText, ref fTextWidth, ref fTextHeight);
          wszOrgText += " ";
        } while (fTextWidth >= 0 && fTextWidth < fMaxWidth);
      }
      fMaxWidth += 50.0f; 
      string szText = "";

      if (_currentFrame > 12 + 25)
      {
        // doscroll (after having waited some frames)
        string wTmp = "";
        _scrollPosititionX = _currentFrame - (12 + 25);
          if (_scrollPosition >= wszOrgText.Length)
            wTmp = " ";
          else
            wTmp = wszOrgText.Substring(_scrollPosition, 1);
          _font.GetTextExtent(wTmp, ref fWidth, ref fHeight);
        if (_scrollPosititionX - _scrollOffset >= fWidth)
        {
          ++_scrollPosition;
          if (_scrollPosition > wszText.Length)
          {
            _scrollPosition = 0;
            bResult = true;
            if (GUIGraphicsContext.graphics != null)
            {
              GUIGraphicsContext.graphics.SetClip(new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height));
            }
            else
            {
              GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }

            return true;
          }
          // now we need to correct _scrollPosititionX
          // with the sum-length of all cut-off characters
          _scrollOffset += fWidth; 
        }
        int ipos = 0;
        for (int i = 0; i < wszOrgText.Length; i++)
        {
          if (i + _scrollPosition < wszOrgText.Length)
            szText += wszOrgText[i + _scrollPosition];
          else
          {
            szText += ' ';
            ipos++;
          }
        }
        if (fPosY >= 0.0)
        {
          if (Alignment.ALIGN_RIGHT == _textAlignment)
          {
            // right alignment => calculate xpos differently
            float fwt = 0;
            //            string strLabel = GetShortenedText(wszOrgText, _width, ref fwt);
            GetShortenedText(wszOrgText, _width, ref fwt); 
            int xpos = (int)(fPosX - fwt - _scrollPosititionX + _scrollOffset);
            _labelControl.Label = szText;
            _labelControl.Width = (int) (fMaxWidth - 50 + _scrollPosititionX - _scrollOffset); 
            _labelControl.TextColor = color;
            _labelControl.SetPosition(xpos, (int) fPosY);
            _labelControl.TextAlignment = Alignment.ALIGN_LEFT;
            _labelControl.Render(timePassed);
          }
          else
          {
            // left or centered alignment
            _labelControl.Label = szText;
            // 1) reduce maxwidth to ensure faded right edge is drawn
            // 2) compensate the Width to ensure the faded right edge does not move
            _labelControl.Width = (int) (fMaxWidth - 50 + _scrollPosititionX - _scrollOffset); 
            _labelControl.TextColor = color;
            int xpos = (int) (fPosX - _scrollPosititionX + _scrollOffset);
            //            _log.Info("fPosX, _scrollPosititionX, _scrollOffset, xpos: {0} {1} {2} {3}", fPosX, _scrollPosititionX, _scrollOffset, xpos);
            //            _log.Info("szText {0}", szText);
            _labelControl.SetPosition(xpos, (int) fPosY);
            _labelControl.Render(timePassed);
          }
        }
      }
      else
      {
        // wait some frames before scrolling
        if (fPosY >= 0.0)
        {
          float fwt = 0, fht = 0;
          _labelControl.Label = GetShortenedText(wszText, (int) fMaxWidth - 50, ref fwt);
          _font.GetTextExtent(_labelControl.Label, ref fwt, ref fht);
          if (_textAlignment == Alignment.ALIGN_RIGHT)
          {
            _labelControl.Width = (int)(fwt); 
          }
          else
          {
            _labelControl.Width = (int) fMaxWidth - 50;
          }
          _labelControl.TextColor = color;
          _labelControl.SetPosition((int) fPosX, (int) fPosY);
          _labelControl.Render(timePassed);
        }
      }

      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.SetClip(new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height));
      }
      else
      {
        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
      }
      return bResult;
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null) return;
        _fontName = value;
        _font = GUIFontManager.GetFont(_fontName);
      }
    }

    /// <summary>
    /// Get/set the color of the text.
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the alignment of the text.
    /// </summary>
    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      _font = GUIFontManager.GetFont(_fontName);
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      _previousText = "";
      _listLabels.Clear();
      _font = null;
    }

    /// <summary>
    /// Clears the control.
    /// </summary>
    public void Clear()
    {
      _currentLabelIndex = 0;
      _previousText = "";
      _listLabels.Clear();
      _currentFrame = 0;
      _scrollPosition = 0;
      _scrollPosititionX = 0;
      _scrollOffset = 0.0f;
      timeElapsed = 0.0f;
    }

    /// <summary>
    /// Add a label to the control.
    /// </summary>
    /// <param name="strLabel"></param>
    public void Add(string strLabel)
    {
      if (strLabel == null) return;
      _listLabels.Add(strLabel);
    }

    /// <summary>
    /// Get/set the scrolling property of the control.
    /// </summary>
    public bool AllowScrolling
    {
      get { return _allowScrolling; }
      set { _allowScrolling = value; }
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      if (_isScrolling && _allowScrolling) return true;
      return false;
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null) return;
        _label = value;
        if (_label.IndexOf("#") >= 0) _containsProperty = true;
        else _containsProperty = false;
      }
    }

    string GetShortenedText(string strLabel, int iMaxWidth, ref float fw)
    {
      if (strLabel == null) return string.Empty;
      if (strLabel.Length == 0) return string.Empty;
      if (_font == null) return strLabel;
      if (_textAlignment == Alignment.ALIGN_RIGHT)
      {
        if (strLabel.Length > 0)
        {
          bool bTooLong = false;
          float fh = 0;
          do
          {
            bTooLong = false;
            _font.GetTextExtent(strLabel, ref fw, ref fh);
            if (fw >= iMaxWidth)
            {
              strLabel = strLabel.Substring(0, strLabel.Length - 1);
              bTooLong = true;
            }
          } while (bTooLong && strLabel.Length > 1);
        }
      }
      return strLabel;
    }
  }
}