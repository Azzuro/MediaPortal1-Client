#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MediaPortal.Core.General;
using MediaPortal.Core.Localization;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using Font = MediaPortal.UI.SkinEngine.Fonts.Font;
using FontRender = MediaPortal.UI.SkinEngine.ContentManagement.FontRender;
using FontBufferAsset = MediaPortal.UI.SkinEngine.ContentManagement.FontBufferAsset;
using FontManager = MediaPortal.UI.SkinEngine.Fonts.FontManager;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class Label : Control
  {
    #region Private & protected fields

    protected AbstractProperty _contentProperty;
    protected AbstractProperty _colorProperty;
    protected AbstractProperty _scrollProperty;
    protected AbstractProperty _wrapProperty;
    protected AbstractProperty _textAlignProperty;
    protected AbstractProperty _maxDesiredWidthProperty;
    protected FontBufferAsset _asset;
    protected FontRender _renderer;
    protected IResourceString _resourceString;
    private int _fontSizeCache;

    #endregion

    #region Ctor

    public Label()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _contentProperty = new SProperty(typeof(string), string.Empty);
      _colorProperty = new SProperty(typeof(Color), Color.White);
      _scrollProperty = new SProperty(typeof(bool), false);
      _wrapProperty = new SProperty(typeof(bool), false);
      _textAlignProperty = new SProperty(typeof(HorizontalAlignmentEnum), HorizontalAlignmentEnum.Left);
      _maxDesiredWidthProperty = new SProperty(typeof(double), double.NaN);

      HorizontalAlignment = HorizontalAlignmentEnum.Left;
      InitializeResourceString();
    }

    void Attach()
    {
      _contentProperty.Attach(OnContentChanged);
      _scrollProperty.Attach(OnRenderAttributeChanged);
      _wrapProperty.Attach(OnLayoutPropertyChanged);
      _colorProperty.Attach(OnRenderAttributeChanged);
      _textAlignProperty.Attach(OnRenderAttributeChanged);
      _maxDesiredWidthProperty.Attach(OnLayoutPropertyChanged);
    }

    void Detach()
    {
      _contentProperty.Detach(OnContentChanged);
      _scrollProperty.Detach(OnRenderAttributeChanged);
      _wrapProperty.Detach(OnLayoutPropertyChanged);
      _colorProperty.Detach(OnRenderAttributeChanged);
      _textAlignProperty.Detach(OnRenderAttributeChanged);
      _maxDesiredWidthProperty.Detach(OnLayoutPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Label l = (Label) source;
      Content = l.Content;
      Color = l.Color;
      Scroll = l.Scroll;
      Wrap = l.Wrap;
      MaxDesiredWidth = l.MaxDesiredWidth;

      InitializeResourceString();
      Attach();
    }

    #endregion

    void OnContentChanged(AbstractProperty prop, object oldValue)
    {
      InitializeResourceString();
      InvalidateLayout();
    }

    void OnRenderAttributeChanged(AbstractProperty prop, object oldValue)
    {
    }

    void OnLayoutPropertyChanged(AbstractProperty prop, object oldValue)
    {
      InvalidateLayout();
    }

    protected override void OnFontChanged(AbstractProperty prop, object oldValue)
    {
      if (_asset != null)
      {
        _asset.Free(true);
        ContentManager.Remove(_asset);
      }
      _asset = null;
    }

    protected void InitializeResourceString()
    {
      _resourceString = string.IsNullOrEmpty(Content) ? LocalizationHelper.CreateStaticString(string.Empty) :
          LocalizationHelper.CreateResourceString(Content);
    }

    public AbstractProperty ContentProperty
    {
      get { return _contentProperty; }
    }

    public string Content
    {
      get { return _contentProperty.GetValue() as string; }
      set { _contentProperty.SetValue(value); }
    }

    public AbstractProperty ColorProperty
    {
      get { return _colorProperty; }
    }

    public Color Color
    {
      get { return (Color) _colorProperty.GetValue(); }
      set { _colorProperty.SetValue(value); }
    }

    public AbstractProperty ScrollProperty
    {
      get { return _scrollProperty; }
    }

    public bool Scroll
    {
      get { return (bool) _scrollProperty.GetValue(); }
      set { _scrollProperty.SetValue(value); }
    }

    public AbstractProperty WrapProperty
    {
      get { return _wrapProperty; }
    }

    public bool Wrap
    {
      get { return (bool) _wrapProperty.GetValue(); }
      set { _wrapProperty.SetValue(value); }
    }

    public AbstractProperty MaxDesiredWidthProperty
    {
      get { return _maxDesiredWidthProperty; }
    }

    /// <summary>
    /// Will be evaluated if <see cref="Scroll"/> or <see cref="Wrap"/> is set to give a maximum width of this label.
    /// This property differs from the <see cref="FrameworkElement.Width"/> property, as this label doesn't always occupy
    /// the whole maximum width.
    /// </summary>
    public double MaxDesiredWidth
    {
      get { return (double) _maxDesiredWidthProperty.GetValue(); }
      set { _maxDesiredWidthProperty.SetValue(value); }
    }

    public AbstractProperty TextAlignProperty
    {
      get { return _textAlignProperty; }
    }

    public HorizontalAlignmentEnum TextAlign
    {
      get { return (HorizontalAlignmentEnum) _textAlignProperty.GetValue(); }
      set { _textAlignProperty.SetValue(value); }
    }

    void AllocFont()
    {
      if (_asset == null)
      {
        // We want to select the font based on the maximum zoom height (fullscreen)
        // This means that the font will be scaled down in windowed mode, but look
        // good in full screen. 
        Font font = FontManager.GetScript(GetFontFamilyOrInherited(), (int) (_fontSizeCache * SkinContext.MaxZoomHeight));
        if (font != null)
          _asset = ContentManager.GetFont(font);
      }
      if (_renderer == null && _asset != null && _asset.Font != null)
        _renderer = new FontRender(_asset.Font);
    }

    /// <summary>
    /// Wraps the text of this label to the specified <paramref name="maxWidth"/> and returns the wrapped
    /// text parts.
    /// </summary>
    /// <param name="maxWidth">Maximum available width until the text should be wrapped.</param>
    /// <param name="findWordBoundaries">If set to <c>true</c>, this method will wrap the text
    /// at word boundaries. Else, it will wrap at the last character index which fits into the specified
    /// <paramref name="maxWidth"/>.</param>
    protected string[] WrapText(float maxWidth, bool findWordBoundaries)
    {
      string text = _resourceString.Evaluate();
      if (string.IsNullOrEmpty(text))
        return new string[0];
      IList<string> result = new List<string>();
      foreach (string para in text.Replace("\r\n", "\n").Split('\n'))
      {
        string paragraph = para.Trim();
        for (int nextIndex = 0; nextIndex < paragraph.Length; )
        {
          while (char.IsWhiteSpace(paragraph[nextIndex]))
            nextIndex++;
          int startIndex = nextIndex;
          nextIndex = _asset.Font.CalculateMaxSubstring(paragraph, _fontSizeCache, startIndex, maxWidth);
          if (findWordBoundaries && nextIndex < paragraph.Length)
          {
            int lastFitWordBoundary = paragraph.LastIndexOf(' ', nextIndex);
            while (lastFitWordBoundary > startIndex && char.IsWhiteSpace(paragraph[lastFitWordBoundary - 1]))
              lastFitWordBoundary--;
            if (lastFitWordBoundary > startIndex)
              nextIndex = lastFitWordBoundary;
          }
          result.Add(paragraph.Substring(startIndex, nextIndex - startIndex));
        }
      }
      return result.ToArray();
    }

    protected override SizeF CalculateDesiredSize(SizeF totalSize)
    {
      _fontSizeCache = GetFontSizeOrInherited();
      AllocFont();
      if (_asset == null)
        return new SizeF();
      // Measure the text
      float height = _asset.Font.LineHeight(_fontSizeCache);
      float width;
      float totalWidth; // Attention: totalWidth is cleaned up by SkinContext.Zoom
      if (double.IsNaN(Width))
        if ((Scroll || Wrap) && !double.IsNaN(MaxDesiredWidth))
          // MaxDesiredWidth will only be evaluated if either Scroll or Wrap is set
          totalWidth = (float) MaxDesiredWidth;
        else
          // No size constraints
          totalWidth = totalSize.Width;
      else
        // Width: highest priority
        totalWidth = (float) Width;
      if (Wrap)
      { // If Width property set and Wrap property set, we need to calculate the number of necessary text lines
        string[] lines = WrapText(totalWidth, true);
        width = 0;
        foreach (string line in lines)
          width = Math.Max(width, _asset.Font.Width(line, _fontSizeCache));
        height *= lines.Length;
      }
      else if (float.IsNaN(totalWidth))
        width = _asset.Font.Width(_resourceString.Evaluate(), _fontSizeCache);
      else
        // Although we maybe can scroll, we will measure all the label's needed space
        width = Math.Min(_asset.Font.Width(_resourceString.Evaluate(), _fontSizeCache), totalWidth);

      return new SizeF(width, height);
    }

    public override void DoRender(RenderContext localRenderContext)
    {
      base.DoRender(localRenderContext);

      if (_asset == null)
        return;

      float lineHeight = _asset.Font.LineHeight(_fontSizeCache);

      float y = _innerRect.Y + 0.05f * lineHeight;
      float x = _innerRect.X;
      float w = _innerRect.Width;
      float h = _innerRect.Height;

      GraphicsDevice.TransformWorld = localRenderContext.Transform;

      RectangleF rect = new RectangleF(x, y, w, h);
      Font.Align align = Font.Align.Left;
      if (TextAlign == HorizontalAlignmentEnum.Right)
        align = Font.Align.Right;
      else if (TextAlign == HorizontalAlignmentEnum.Center)
        align = Font.Align.Center;

      Color4 color = ColorConverter.FromColor(Color);
      color.Alpha *= (float) localRenderContext.Opacity;

      bool scroll = Scroll && !Wrap;
      string[] lines = Wrap ? WrapText(_innerRect.Width, true) : new string[] { _resourceString.Evaluate() };

      foreach (string line in lines)
      {
        float totalWidth;
        _asset.Draw(line, rect, align, _fontSizeCache * 0.9f, color, scroll, out totalWidth, localRenderContext.Transform);
        rect.Y += lineHeight;
      }
    }

    public override void Deallocate()
    {
      base.Deallocate();
      if (_asset != null)
      {
        ContentManager.Remove(_asset);
        _asset.Free(true);
        _asset = null;
      }
      if (_renderer != null)
        _renderer.Free();
      _renderer = null;
    }
  }
}

