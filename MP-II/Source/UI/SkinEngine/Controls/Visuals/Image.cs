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

using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using SlimDX;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public enum StretchDirection { UpOnly, DownOnly, Both };

  public enum Stretch
  {
    // The content preserves its original size.
    None,

    // The content is resized to fill the destination dimensions. The aspect ratio is not preserved.
    Fill,

    // The content is resized to fit in the destination dimensions while it preserves its
    // native aspect ratio. If the aspect ratio of the destination rectangle differs from
    // the source, the content won't fill the whole destionation area.
    Uniform,

    // The content is resized to fill the destination dimensions while it preserves its
    // native aspect ratio. 
    // If the aspect ratio of the destination rectangle differs from the source, the source content is 
    // clipped to fit in the destination dimensions completely.
    UniformToFill
  };

  public class Image : Control
  {
    #region Private fields

    AbstractProperty _fallbackSourceProperty;
    AbstractProperty _imageSourceProperty;
    AbstractProperty _stretchDirectionProperty;
    AbstractProperty _thumbnailProperty;
    private VertextBufferAsset _image;
    private VertextBufferAsset _fallbackImage;
    AbstractProperty _stretchProperty;
    TextureRender _renderImage;
    TextureRender _renderFallback;

    float _u, _v, _uoff, _voff, _w, _h;
    Vector2 _pos;
    bool _performImageLayout;

    #endregion

    #region Ctor

    public Image()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _imageSourceProperty = new SProperty(typeof(string), null);
      _fallbackSourceProperty = new SProperty(typeof(string), null);
      _stretchDirectionProperty = new SProperty(typeof(StretchDirection), StretchDirection.Both);
      _thumbnailProperty = new SProperty(typeof(bool), false);
      _stretchProperty = new SProperty(typeof(Stretch), Stretch.None);
    }

    void Attach()
    {
      _imageSourceProperty.Attach(OnImageChanged);
      _stretchDirectionProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _imageSourceProperty.Detach(OnImageChanged);
      _stretchDirectionProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Image i = (Image) source;
      Source = copyManager.GetCopy(i.Source);
      FallbackSource = copyManager.GetCopy(i.FallbackSource);
      StretchDirection = copyManager.GetCopy(i.StretchDirection);
      Stretch = copyManager.GetCopy(i.Stretch);
      Thumbnail = copyManager.GetCopy(i.Thumbnail);
      Attach();
    }

    #endregion

    /// <summary>
    /// Called when a property changed. 
    /// Simply sets a variable to indicate a layout needs to be performed
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value of the property.</param>
    void OnPropertyChanged(AbstractProperty property, object oldValue)
    {
      _performImageLayout = true;
      if (Screen != null) Screen.Invalidate(this);
    }

    /// <summary>
    /// Called when the imagesource has been changed
    /// Simply invalidates the image, the renderer will automaticly create a new one
    /// with the new image source
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value of the property.</param>
    void OnImageChanged(AbstractProperty property, object oldValue)
    {
      if (_image != null)
      {
        _image.Free(true);
        ContentManager.Remove(_image);
        _image = null;

        if (_renderImage != null)
        {
          _renderImage.Free();
          _renderImage = null;
        }
      }
      _performImageLayout = true;
      if (Screen != null) Screen.Invalidate(this);
    }

    public AbstractProperty StretchProperty
    {
      get { return _stretchProperty; }
    }

    public Stretch Stretch
    {
      get { return (Stretch)_stretchProperty.GetValue(); }
      set { _stretchProperty.SetValue(value); }
    }

    public AbstractProperty ThumbnailProperty
    {
      get { return _thumbnailProperty; }
    }

    public bool Thumbnail
    {
      get { return (bool)_thumbnailProperty.GetValue(); }
      set { _thumbnailProperty.SetValue(value); }
    }

    public AbstractProperty SourceProperty
    {
      get { return _imageSourceProperty; }
      set { _imageSourceProperty = value; }
    }

    public string Source
    {
      get { return (string)_imageSourceProperty.GetValue(); }
      set { _imageSourceProperty.SetValue(value); }
    }

    public AbstractProperty FallbackSourceProperty
    {
      get { return _fallbackSourceProperty; }
    }

    public string FallbackSource
    {
      get { return (string)_fallbackSourceProperty.GetValue(); }
      set { _fallbackSourceProperty.SetValue(value); }
    }

    public AbstractProperty StretchDirectionProperty
    {
      get { return _stretchDirectionProperty; }
    }

    public StretchDirection StretchDirection
    {
      get { return (StretchDirection)_stretchDirectionProperty.GetValue(); }
      set { _stretchDirectionProperty.SetValue(value); }
    }

    protected static void CalculateScalingFactors(SizeF availableSize, SizeF imageSize, Stretch stretch, out float scaleX, out float scaleY)
    {
      float sourceFrameRatio = imageSize.Width / imageSize.Height;
      
      // Adaptions when available size is not specified in any direction(s)
      if (double.IsNaN(availableSize.Width) && double.IsNaN(availableSize.Height))
        availableSize = imageSize;
      else if (double.IsNaN(availableSize.Height))
        availableSize.Height = availableSize.Width / sourceFrameRatio;
      else if (double.IsNaN(availableSize.Width))
        availableSize.Width = availableSize.Height * sourceFrameRatio;

      float outputFrameRatio = availableSize.Width / availableSize.Height;
      scaleX = 1;
      scaleY = 1;
      if (stretch == Stretch.Uniform || stretch == Stretch.UniformToFill)
      {
        // Uniform and UniformToFill both use the same scaling factors for X and Y dimensions,
        // they only differ in the choice of master dimension
        if ((stretch == Stretch.Uniform) == (sourceFrameRatio > outputFrameRatio))
        { // Source width/height is bigger than target width/height, so fill width and adapt height
          scaleX = availableSize.Width/imageSize.Width;
          scaleY = scaleX;
        }
        else
        { // Else fill height and adapt width
          scaleY = availableSize.Height/imageSize.Height;
          scaleX = scaleY;
        }
      }
      else if (stretch == Stretch.Fill)
      {
        scaleX = availableSize.Width/imageSize.Width;
        scaleY = availableSize.Height/imageSize.Height;
      }
    }

    protected override SizeF CalculateDesiredSize(SizeF totalSize)
    {
      Deallocate();
      if (_image == null && !string.IsNullOrEmpty(Source))
      {
        _image = ContentManager.Load(Source, Thumbnail);
        _image.Texture.UseThumbnail = Thumbnail;
        _image.Texture.Allocate();

        if (SkinContext.UseBatching)
          _renderImage = new TextureRender(_image.Texture);
      }

      if (_fallbackImage == null && !string.IsNullOrEmpty(FallbackSource))
      {
        _fallbackImage = ContentManager.Load(FallbackSource, Thumbnail);
        _fallbackImage.Texture.UseThumbnail = Thumbnail;
        _fallbackImage.Texture.Allocate();

        if (SkinContext.UseBatching)
          _renderFallback = new TextureRender(_fallbackImage.Texture);
      }

      SizeF imageSize;
      if (_image != null)
        imageSize = new SizeF(_image.Texture.Width * SkinContext.Zoom.Width, _image.Texture.Height * SkinContext.Zoom.Height);
      else if (_fallbackImage != null)
        imageSize = new SizeF(_fallbackImage.Texture.Width * SkinContext.Zoom.Width, _fallbackImage.Texture.Height * SkinContext.Zoom.Height);
      else
        imageSize = new SizeF();
      float scaleX;
      float scaleY;
      CalculateScalingFactors(totalSize, imageSize, Stretch, out scaleX, out scaleY);

      return new SizeF(imageSize.Width * scaleX, imageSize.Height * scaleY);
    }

    protected override void ArrangeOverride(RectangleF finalRect)
    {
      base.ArrangeOverride(finalRect);
      if (_image != null)
        PerformLayout(_image);

      if (_fallbackImage != null)
        PerformLayout(_fallbackImage);

      if (Screen != null)
        Screen.Invalidate(this);
    }

    public override void DoBuildRenderTree()
    {
      if (_hidden) 
        return;

      if (!IsVisible) 
        return;
      if (!IsEnabled && Opacity == 0.0) 
        return;

      SkinContext.AddOpacity(Opacity);
      float opacity = (float)SkinContext.Opacity;
      float posx = _pos.X + ActualPosition.X;
      float posy = _pos.Y + ActualPosition.Y;
      if (_renderImage != null)
      {
        _renderImage.Draw(posx, posy, ActualPosition.Z, _w, _h, _uoff, _voff, _u, _v, opacity, opacity, opacity, opacity);
        if (_renderImage.Texture.IsAllocated)
        {
          SkinContext.RemoveOpacity();
          return;
        }
      }
      if (_renderFallback != null)
        _renderFallback.Draw(posx, posy, ActualPosition.Z, _w, _h, _uoff, _voff, _u, _v, opacity, opacity, opacity, opacity);
      SkinContext.RemoveOpacity();
    }

    public override void DestroyRenderTree()
    {
      if (_renderImage != null)
      {
        _renderImage.Free();
        _renderImage = null;
      }
      if (_renderFallback != null)
      {
        _renderFallback.Free();
        _renderFallback = null;
      }
    }

    public override void DoRender()
    {
      if (!IsEnabled && Opacity == 0.0)
        return;

      base.DoRender();
      SkinContext.AddOpacity(Opacity);
      ExtendedMatrix m = new ExtendedMatrix
        {
            Matrix = Matrix.Translation(new Vector3(ActualPosition.X, ActualPosition.Y, ActualPosition.Z))
        };
      SkinContext.AddTransform(m);
      //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;
      float opacity = (float)SkinContext.Opacity;
      if (_image != null)
      {
        _image.Draw(_pos.X, _pos.Y, ActualPosition.Z, _w, _h, _uoff, _voff, _u, _v, opacity, opacity, opacity, opacity);
        if (_image.Texture.IsAllocated)
        {
          SkinContext.RemoveTransform();
          SkinContext.RemoveOpacity();
          return;
        }
      }
      if (_fallbackImage != null)
        _fallbackImage.Draw(_pos.X, _pos.Y, ActualPosition.Z, _w, _h, _uoff, _voff, _u, _v, opacity, opacity, opacity, opacity);
      SkinContext.RemoveTransform();
      SkinContext.RemoveOpacity();
    }

    void PerformLayout(VertextBufferAsset asset)
    {
      if (asset != null && asset.Texture.IsAllocated)
      {
        float sourceImageHeight = asset.Texture.Height;
        float sourceImageWidth = asset.Texture.Width;
        // Calculate scaling factor for both dimensions
        float scaleX;
        float scaleY;
        CalculateScalingFactors(new SizeF((float) ActualWidth, (float) ActualHeight), new SizeF(sourceImageWidth, sourceImageHeight), Stretch, out scaleX, out scaleY);

        float scaledImageWidth = sourceImageWidth*scaleX;
        float scaledImageHeight = sourceImageHeight*scaleY;

        _pos = new Vector2(0, 0);
        // Calculate offsets
        if (scaledImageWidth > ActualWidth)
        { // The drawn image is bigger than the destination rectangle -> clip
          _w = (float) ActualWidth;
          _uoff = (float) (scaledImageWidth - ActualWidth)/(2*sourceImageWidth*scaleX);
          _u = 1 - _uoff;
        }
        else
        { // The drawn image fits into destination rectangle -> center
          _pos.X += ((float) ActualWidth - scaledImageWidth) / 2.0f;
          _w = scaledImageWidth;
          _uoff = 0;
          _u = 1;
        }
        if (scaledImageHeight > ActualHeight)
        { // The drawn image is bigger than the destination rectangle -> clip
          _h = (float) ActualHeight;
          _voff = (float) (scaledImageHeight - ActualHeight)/(2*sourceImageHeight*scaleY);
          _v = 1 - _voff;
        }
        else
        { // The drawn image fits into destination rectangle -> center
          _pos.Y += ((float) ActualHeight - scaledImageHeight) / 2.0f;
          _h = scaledImageHeight;
          _voff = 0;
          _v = 1;
        }
      }
      //else
      //  Trace.WriteLine("Image: Texture not allocated");
    }

    public override void Deallocate()
    {
      if (_image != null)
      {
        _image.Free(true);
        ContentManager.Remove(_image);
        _image = null;
      }

      if (_fallbackImage != null)
      {
        _fallbackImage.Free(true);
        ContentManager.Remove(_fallbackImage);
        _fallbackImage = null;
      }

      if (_renderImage != null)
      {
        _renderImage.Free();
        _renderImage = null;
      }
      if (_renderFallback != null)
      {
        _renderFallback.Free();
        _renderFallback = null;
      }
      base.Deallocate();
    }

    public override void BecomesHidden()
    {
      if (_renderImage != null)
        _renderImage.Free();
      if (_renderFallback != null)
        _renderFallback.Free();
    }

    public override void BecomesVisible()
    {

      if (_renderImage != null)
        _renderImage.Alloc();
      if (_renderFallback != null)
        _renderFallback.Alloc();
    }

    public override void Update()
    {
      base.Update();
      if (_hidden == false)
      {
        if (_performImageLayout)
        {
          DoBuildRenderTree();
          _performImageLayout = false;
        }
      }
    }
  }
}
