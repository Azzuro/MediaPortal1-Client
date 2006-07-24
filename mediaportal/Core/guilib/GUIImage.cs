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
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Xml;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Utils.Services;


namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying Images.
  /// </summary>
  public class GUIImage : GUIControl
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineRemoveTexture(int textureNo);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern int FontEngineAddTexture(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern int FontEngineAddSurface(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEnginePresentTextures();

    [XMLSkinElement("colorkey")]
    private long m_dwColorKey = 0;
    //private VertexBuffer						m_vbBuffer=null;
    [XMLSkinElement("texture")]
    private string _textureFileNameTag = "";
    /// <summary>The width of the current texture.</summary>
    private int _textureWidth = 0;
    private int _textureHeight = 0;
    /// <summary>The width of the image containing the textures.</summary>
    private int _imageWidth = 0;
    private int _imageHeight = 0;
    private int _selectedFrameNumber = 0;
    private int m_dwItems = 0;
    private int _currentAnimationLoop = 0;
    private int _currentFrameNumber = 0;
    [XMLSkinElement("keepaspectratio")]
    private bool _keepAspectRatio = false;
    [XMLSkinElement("zoom")]
    private bool _zoomIn = false;
    [XMLSkinElement("zoomfromtop")]
    private bool _zoomFromTop = false;
    [XMLSkinElement("fixedheight")]
    private bool _isFixedHeight = false;
		[XMLSkinElement("RepeatBehavior")]
		protected RepeatBehavior _repeatBehavior = RepeatBehavior.Forever;

    private CachedTexture.Frame[] _listTextures = null;

    //TODO GIF PALLETTE
    //private PaletteEntry						m_pPalette=null;
    /// <summary>The width of in which the texture will be rendered after scaling texture.</summary>
    private int m_iRenderWidth = 0;
    private int m_iRenderHeight = 0;
    //private System.Drawing.Image m_image = null;
    private Rectangle m_destRect;
    string _cachedTextureFileName = "";
    int g_nAnisotropy = 0;
    [XMLSkinElement("filtered")]
    bool _filterImage = true;
    [XMLSkinElement("centered")]
    bool _centerImage = false;

    DateTime _animationTimer = DateTime.MinValue;
    bool _containsProperty = false;
    bool _propertyChanged = false;
    //    StateBlock                      savedStateBlock;
    Rectangle sourceRect;
    Rectangle destinationRect;
    Vector3 pntPosition;
    float scaleX = 1;
    float scaleY = 1;
    float _fx, _fy, _nw, _nh;
    float _uoff, _voff, _umax, _vmax;

    float _texUoff, _texVoff, _texUmax, _texVmax;
    Texture _packedTexture = null;
    int _packedTextureNo = -1;
    static bool logtextures = false;
    System.Drawing.Image memoryImage = null;
    bool _isFullScreenImage = false;
    bool _reCalculate = false;
    bool _allocated = false;
    bool _registeredForEvent = false;

    private GUIImage()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public GUIImage(int dwParentID)
      : base(dwParentID)
    {
    }

    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTexture, Color color)
      : this(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTexture, color.ToArgb())
    {
    }

    /// <summary>
    /// The constructor of the GUIImage class.
    /// </summary>
    /// <param name="dwParentID">The parent of this GUIImage control.</param>
    /// <param name="dwControlId">The ID of this GUIImage control.</param>
    /// <param name="dwPosX">The X position of this GUIImage control.</param>
    /// <param name="dwPosY">The Y position of this GUIImage control.</param>
    /// <param name="dwWidth">The width of this GUIImage control.</param>
    /// <param name="dwHeight">The height of this GUIImage control.</param>
    /// <param name="strTexture">The filename of the texture of this GUIImage control.</param>
    /// <param name="dwColorKey">The color that indicates transparancy.</param>
    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTexture, long dwColorKey)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _diffuseColor = 0xFFFFFFFF;
      _textureFileNameTag = strTexture;
      _textureWidth = 0;
      _textureHeight = 0;
      m_dwColorKey = dwColorKey;
      _selectedFrameNumber = 0;

      _currentFrameNumber = 0;
      _keepAspectRatio = false;
      _zoomIn = false;
      _currentAnimationLoop = 0;
      _imageWidth = 0;
      _imageHeight = 0;
      FinalizeConstruction();

    }
    public System.Drawing.Image MemoryImage
    {
      get { return memoryImage; }
      set { memoryImage = value; }
    }
    /// <summary>
    /// Does any scaling on the inital size\position values to fit them to screen 
    /// resolution. 
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      if (_textureFileNameTag == null) _textureFileNameTag = String.Empty;
      if (_textureFileNameTag != "-" && _textureFileNameTag != "")
      {
        if (_width == 0 || _height == 0)
        {
          try
          {
            string strFileNameTemp = "";
            if (!System.IO.File.Exists(_textureFileNameTag))
            {
              if (_textureFileNameTag[1] != ':')
                strFileNameTemp = GUIGraphicsContext.Skin + @"\media\" + _textureFileNameTag;
            }

            if (strFileNameTemp.Length > 0 && strFileNameTemp.IndexOf(@"\#") != -1)
              return;

            using (Image img = Image.FromFile(strFileNameTemp))
            {
              if (0 == _width) _width = img.Width;
              if (0 == _height) _height = img.Height;
            }
          }
          catch (Exception)
          {
          }
        }
      }
      base.ScaleToScreenResolution();
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

      m_dwItems = 1;

      m_iRenderWidth = _width;
      m_iRenderHeight = _height;
      if (_textureFileNameTag.IndexOf("#") >= 0) _containsProperty = true;
    }

    /// <summary>
    /// Get/Set the TextureWidth
    /// </summary>
    public int TextureWidth
    {
      get { return _textureWidth; }
      set
      {
        if (value < 0 || value == _textureWidth) return;
        _textureWidth = value;
        _reCalculate = true;
      }
    }

    /// <summary>
    /// Get/Set the TextureHeight
    /// </summary>
    public int TextureHeight
    {
      get { return _textureHeight; }
      set
      {
        if (value < 0 || value == _textureHeight) return;
        _textureHeight = value;
        _reCalculate = true;
      }
    }

    /// <summary>
    /// Get the filename of the texture.
    /// </summary>
    public string FileName
    {
      get { return _textureFileNameTag; }
      set
      {

        SetFileName(value);
      }
    }

    /// <summary>
    /// Get the transparent color.
    /// </summary>
    public long ColorKey
    {
      get { return m_dwColorKey; }
      set
      {
        if (m_dwColorKey != value)
        {
          m_dwColorKey = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set if the aspectratio of the texture needs to be preserved during rendering.
    /// </summary>
    public bool KeepAspectRatio
    {
      get { return _keepAspectRatio; }
      set
      {
        if (_keepAspectRatio != value)
        {
          _keepAspectRatio = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get the width in which the control is rendered.
    /// </summary>
    public int RenderWidth
    {
      get { return m_iRenderWidth; }
    }

    /// <summary>
    /// Get the height in which the control is rendered.
    /// </summary>
    public int RenderHeight
    {
      get { return m_iRenderHeight; }
    }

    /// <summary>
    /// Returns if the control can have the focus.
    /// </summary>
    /// <returns>False</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// If the texture holds more then 1 frame (like an animated gif)
    /// then you can select the current frame with this method
    /// </summary>
    /// <param name="iBitmap"></param>
    public void Select(int frameNumber)
    {
      if (_selectedFrameNumber == frameNumber) return;
      _selectedFrameNumber = frameNumber;
      _reCalculate = true;
    }

    /// <summary>
    /// If the texture has more then 1 frame like an animated gif then
    /// you can specify the max# of frames to play with this method
    /// </summary>
    /// <param name="iItems"></param>
    public void SetItems(int iItems)
    {
      m_dwItems = iItems;
    }

		public void BeginAnimation()
		{
			_currentAnimationLoop = 0;
			_currentFrameNumber   = 0;
		}

    /// <summary>
    /// This function will do the animation (when texture is an animated gif)
    /// by switching from frame 1->frame2->frame 3->...
    /// </summary>
    protected void Animate()
    {
      if (_listTextures == null) return;
      // If the number of textures that correspond to this control is lower than or equal to 1 do not change the texture.
      if (_listTextures.Length <= 1)
      {
        _currentFrameNumber = 0;
        return;
      }

      if (_currentFrameNumber >= _listTextures.Length)
        _currentFrameNumber = 0;

      CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
      // Check the delay.
      int dwDelay = frame.Duration;
      //int iMaxLoops = 0;
      frame = null;

      // Default delay = 100;
      if (0 == dwDelay) dwDelay = 100;

      TimeSpan ts = DateTime.Now - _animationTimer;
      if (ts.TotalMilliseconds > dwDelay)
      {
        _animationTimer = DateTime.Now;

        // Reset the current image
        if (_currentFrameNumber + 1 >= _listTextures.Length)
        {
					// Check if another loop is required
					if (RepeatBehavior.IterationCount > 0)
          {
            // Go to the next loop
						if (_currentAnimationLoop + 1 < RepeatBehavior.IterationCount)
            {
              _currentAnimationLoop++;
              _currentFrameNumber = 0;
            }
          }
          else
          {
            // 0 == loop forever
            _currentFrameNumber = 0;
          }
        }
        // Switch to the next image.
        else
        {
          _currentFrameNumber++;
        }
      }
    }

    /// <summary>
    /// Allocate the DirectX resources needed for rendering this GUIImage.
    /// </summary>
    public override void AllocResources()
    {
      try
      {
        if (GUIGraphicsContext.DX9Device == null) return;
        if (GUIGraphicsContext.DX9Device.Disposed) return;
        if (_registeredForEvent == false)
        {
          GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
          _registeredForEvent = true;
        }
        _propertyChanged = false;

        g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;

        //reset animation
				BeginAnimation();

				_listTextures = null;
				string textureFiles = _textureFileNameTag;
				if (textureFiles.ToUpper().Contains(".XML"))
				{
					LoadAnimation(ref textureFiles);
				}
        foreach (string file in textureFiles.Split(';'))
				{
					//get the filename of the texture
					string fileName = file;
          if (_containsProperty)  fileName = GUIPropertyManager.Parse(file);
          if (fileName.Length == 0) continue;
          if (_textureFileNameTag.Length == 0) continue;
          if (_textureFileNameTag == "") continue;

          if (logtextures) _log.Info("GUIImage:AllocResources:{0}", fileName);
          if (GUITextureManager.GetPackedTexture(fileName, out _texUoff, out _texVoff, out _texUmax, out _texVmax, out _textureWidth, out _textureHeight, out _packedTexture, out _packedTextureNo))
          {
            _reCalculate = true;
            return;
          }

					//load the texture
					int frameCount = 0;
					if (fileName.StartsWith("["))
					{
						frameCount = GUITextureManager.LoadFromMemory(memoryImage, fileName, m_dwColorKey, m_iRenderWidth, _textureHeight);
						if (0 == frameCount)
						{
							continue;// unable to load texture
						}
					}
					else
					{
						frameCount = GUITextureManager.Load(fileName, m_dwColorKey, m_iRenderWidth, _textureHeight);
						if (0 == frameCount)
						{
							continue;// unable to load texture
						}
					}
					//get each frame of the texture
					int iStartCopy = 0;
					if (_listTextures == null)
					{
						_listTextures = new CachedTexture.Frame[frameCount];
					}
					else
					{
						int newLength = _listTextures.Length + frameCount;
						iStartCopy = _listTextures.Length;
						CachedTexture.Frame[] _newList = new CachedTexture.Frame[newLength];
						_listTextures.CopyTo(_newList, 0);
						_listTextures = new CachedTexture.Frame[newLength];
						_newList.CopyTo(_listTextures, 0);
					}
					for (int i = 0; i < frameCount; i++)
					{
						_listTextures[i + iStartCopy] = GUITextureManager.GetTexture(fileName, i, out _textureWidth, out _textureHeight);//,m_pPalette);
						_listTextures[i + iStartCopy].Disposed += new EventHandler(OnImageDisposedEvent);
					}
				}
        // Set state to render the image
        _reCalculate = true;
      }
      finally
      {
        _allocated = true;
      }
    }

    void OnImageDisposedEvent(object sender, EventArgs e)
    {
      if (_listTextures == null) return;
      if (sender == null) return;
      for (int i = 0; i < _listTextures.Length; ++i)
      {
        if (_listTextures[i] == sender)
        {
          _listTextures[i].Disposed -= new EventHandler(OnImageDisposedEvent);
          _listTextures[i] = null;
        }
      }
    }

    void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (!_containsProperty) return;
      if (_textureFileNameTag.IndexOf(tag) >= 0)
      {
        _propertyChanged = true;
      }
    }

    void FreeResourcesAndRegEvent()
    {
      FreeResources();
      if (_registeredForEvent == false)
      {
        GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        _registeredForEvent = true;
      }
    }
    /// <summary>
    /// Free the DirectX resources needed for rendering this GUIImage.
    /// </summary>
    public override void FreeResources()
    {
      _allocated = false;
      lock (this)
      {
        if (_registeredForEvent)
        {
          GUIPropertyManager.OnPropertyChanged -= new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
          _registeredForEvent = false;
        }
        string file = _textureFileNameTag;
        _packedTexture = null;
        if (_containsProperty)
        {
          file = GUIPropertyManager.Parse(_textureFileNameTag);
        }
        if (file != null && file != String.Empty)
        {
          if (logtextures) _log.Info("GUIImage:freeresources:{0}", file);
          if (GUITextureManager.IsTemporary(file))
          {
            GUITextureManager.ReleaseTexture(file);
          }
        }
        Cleanup();
      }
    }


    void Cleanup()
    {
      _cachedTextureFileName = "";
      //m_image = null;
      if (_listTextures != null)
      {
        for (int i = 0; i < _listTextures.Length; ++i)
        {
          if (_listTextures[i] != null)
          {
            _listTextures[i].Disposed -= new EventHandler(OnImageDisposedEvent);
          }
        }
      }
      _listTextures = null;
      _currentFrameNumber = 0;
      _currentAnimationLoop = 0;
      _imageWidth = 0;
      _imageHeight = 0;
      _textureWidth = 0;
      _textureHeight = 0;
      _allocated = false;
      _packedTexture = null;
    }

    /// <summary>
    /// Sets the state to render the image
    /// </summary>
    protected void Calculate()
    {
      _reCalculate = false;
      float x = (float)_positionX;
      float y = (float)_positionY;
      if (_packedTexture != null)
      {
        if (0 == _imageWidth || 0 == _imageHeight)
        {
          _imageWidth = _textureWidth;
          _imageHeight = _textureHeight;
        }
      }
      else
      {
        if (_listTextures == null) return;
        if (_listTextures.Length == 0) return;
        if (_currentFrameNumber < 0 || _currentFrameNumber >= _listTextures.Length) return;

        CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
        if (frame == null) return;
        Direct3D.Texture texture = frame.Image;
        frame = null;
        if (texture == null)
        {
          //no texture? then nothing todo
          return;
        }

        // if texture is disposed then free its resources and return
        if (texture.Disposed)
        {
          texture = null;
          FreeResourcesAndRegEvent();
          texture = null;
          return;
        }

        // on first run, get the image width/height of the texture
        if (0 == _imageWidth || 0 == _imageHeight)
        {
          Direct3D.SurfaceDescription desc;
          desc = texture.GetLevelDescription(0);
          _imageWidth = desc.Width;
          _imageHeight = desc.Height;
        }
        texture = null;
      }

      // Calculate the _textureWidth and _textureHeight 
      // based on the _imageWidth and _imageHeight
      if (0 == _textureWidth || 0 == _textureHeight)
      {
        _textureWidth = (int)Math.Round(((float)_imageWidth) / ((float)m_dwItems));
        _textureHeight = _imageHeight;

        if (_textureHeight > (int)GUIGraphicsContext.Height)
          _textureHeight = (int)GUIGraphicsContext.Height;

        if (_textureWidth > (int)GUIGraphicsContext.Width)
          _textureWidth = (int)GUIGraphicsContext.Width;
      }

      // If there are multiple frames in the GUIImage thne the e _textureWidth is equal to the _width
      if (_width > 0 && m_dwItems > 1)
      {
        _textureWidth = (int)_width;
      }

      // Initialize the with of the control based on the texture width
      if (_width == 0)
        _width = _textureWidth;

      // Initialize the height of the control based on the texture height
      if (_height == 0)
        _height = _textureHeight;


      float nw = (float)_width;
      float nh = (float)_height;

      //adjust image based on current aspect ratio setting
      float fSourceFrameRatio = 1;
      float fOutputFrameRatio = 1;
      if (!_zoomIn && !_zoomFromTop && _keepAspectRatio && _textureWidth != 0 && _textureHeight != 0)
      {
        // TODO: remove or complete HDTV_1080i code
        //int iResolution=g_stSettings.m_ScreenResolution;
        fSourceFrameRatio = ((float)_textureWidth) / ((float)_textureHeight);
        fOutputFrameRatio = fSourceFrameRatio / GUIGraphicsContext.PixelRatio;
        //if (iResolution == HDTV_1080i) fOutputFrameRatio *= 2;

        // maximize the thumbnails width
        float fNewWidth = (float)_width;
        float fNewHeight = fNewWidth / fOutputFrameRatio;

        // make sure the height is not larger than the maximum
        if (fNewHeight > _height)
        {
          fNewHeight = (float)_height;
          fNewWidth = fNewHeight * fOutputFrameRatio;
        }
        // this shouldnt happen, but just make sure that everything still fits onscreen
        if (fNewWidth > _width || fNewHeight > _height)
        {
          fNewWidth = (float)_width;
          fNewHeight = (float)_height;
        }
        nw = fNewWidth;
        nh = fNewHeight;
      }

      // set the width/height the image gets rendererd
      m_iRenderWidth = (int)Math.Round(nw);
      m_iRenderHeight = (int)Math.Round(nh);

      // reposition if calibration of the UI has been done
      if (CalibrationEnabled)
      {
        GUIGraphicsContext.Correct(ref x, ref y);
      }

      // if necessary then center the image 
      // in the controls rectangle
      if (_centerImage)
      {
        x += ((((float)_width) - nw) / 2.0f);
        y += ((((float)_height) - nh) / 2.0f);
      }


      // Calculate source Texture
      int iSourceX = 0;
      int iSourceY = 0;
      int iSourceWidth = _textureWidth;
      int iSourceHeight = _textureHeight;

      if ((_zoomIn || _zoomFromTop) && _keepAspectRatio)
      {
        fSourceFrameRatio = ((float)nw) / ((float)nh);
        fOutputFrameRatio = fSourceFrameRatio * GUIGraphicsContext.PixelRatio;

        if (((float)iSourceWidth / (nw * GUIGraphicsContext.PixelRatio)) < ((float)iSourceHeight / nh))
        {
          //Calc height
          iSourceHeight = (int)((float)iSourceWidth / fOutputFrameRatio);
          if (iSourceHeight > _textureHeight)
          {
            iSourceHeight = _textureHeight;
            iSourceWidth = (int)((float)iSourceHeight * fOutputFrameRatio);
          }
        }
        else
        {
          //Calc width
          iSourceWidth = (int)((float)iSourceHeight * fOutputFrameRatio);
          if (iSourceWidth > _textureWidth)
          {
            iSourceWidth = _textureWidth;
            iSourceHeight = (int)((float)iSourceWidth / fOutputFrameRatio);
          }
        }

        if (!_zoomFromTop) iSourceY = (_textureHeight - iSourceHeight) / 2;
        iSourceX = (_textureWidth - iSourceWidth) / 2;
      }

      if (_isFixedHeight)
      {
        y = (float)_positionY;
        nh = (float)_height;
      }

      // check and compensate image
      if (x < GUIGraphicsContext.OffsetX)
      {
        // calc percentage offset
        iSourceX -= (int)((float)_textureWidth * ((x - GUIGraphicsContext.OffsetX) / nw));
        iSourceWidth += (int)((float)_textureWidth * ((x - GUIGraphicsContext.OffsetX) / nw));

        nw += x;
        nw -= GUIGraphicsContext.OffsetX;
        x = GUIGraphicsContext.OffsetX;
      }
      if (y < GUIGraphicsContext.OffsetY)
      {
        iSourceY -= (int)((float)_textureHeight * ((y - GUIGraphicsContext.OffsetY) / nh));
        iSourceHeight += (int)((float)_textureHeight * ((y - GUIGraphicsContext.OffsetY) / nh));

        nh += y;
        nh -= GUIGraphicsContext.OffsetY;
        y = GUIGraphicsContext.OffsetY;
      }
      if (x > GUIGraphicsContext.Width) x = GUIGraphicsContext.Width;
      if (y > GUIGraphicsContext.Height) y = GUIGraphicsContext.Height;

      if (nw < 0) nw = 0;
      if (nh < 0) nh = 0;
      if (x + nw > GUIGraphicsContext.Width)
      {
        iSourceWidth = (int)((float)_textureWidth * (((float)GUIGraphicsContext.Width - x) / nw));
        nw = GUIGraphicsContext.Width - x;
      }
      if (y + nh > GUIGraphicsContext.Height)
      {
        iSourceHeight = (int)((float)_textureHeight * (((float)GUIGraphicsContext.Height - y) / nh));
        nh = GUIGraphicsContext.Height - y;
      }

      // copy all coordinates to the vertex buffer
      // x-offset in texture
      float uoffs = ((float)(_selectedFrameNumber * _width + iSourceX)) / ((float)_imageWidth);

      // y-offset in texture
      float voffs = ((float)iSourceY) / ((float)_imageHeight);

      // width copied from texture
      float u = ((float)iSourceWidth) / ((float)_imageWidth);

      // height copied from texture
      float v = ((float)iSourceHeight) / ((float)_imageHeight);


      if (uoffs < 0 || uoffs > 1) uoffs = 0;
      if (u < 0 || u > 1) u = 1;
      if (v < 0 || v > 1) v = 1;
      if (u + uoffs > 1)
      {
        uoffs = 0;
        u = 1;
      }

      _fx = x; _fy = y; _nw = nw; _nh = nh;

      _uoff = uoffs;
      _voff = voffs;
      _umax = u;
      _vmax = v;

      if (_packedTexture != null)
      {
        _uoff = _texUoff + (uoffs * _texUmax);
        _voff = _texVoff + (voffs * _texVmax);
        _umax = _umax * _texUmax;
        _vmax = _vmax * _texVmax;
      }

      pntPosition = new Vector3(x, y, 0);
      sourceRect = new Rectangle(_selectedFrameNumber * _width + iSourceX, iSourceY, iSourceWidth, iSourceHeight);
      destinationRect = new Rectangle(0, 0, (int)nw, (int)nh);
      m_destRect = new Rectangle((int)x, (int)y, (int)nw, (int)nh);

      scaleX = (float)destinationRect.Width / (float)iSourceWidth;
      scaleY = (float)destinationRect.Height / (float)iSourceHeight;
      pntPosition.X /= scaleX;
      pntPosition.Y /= scaleY;

      _isFullScreenImage = false;
      if (m_iRenderWidth == GUIGraphicsContext.Width && m_iRenderHeight == GUIGraphicsContext.Height)
        _isFullScreenImage = true;
    }

    /*
      /// <summary>
      /// Check 
      ///  -IsVisible
      ///  -Filename
      ///  -Filename changed cause it contains a property
      ///  -m_vecTextures
      ///  -m_vbBuffer
      ///  -GUIGraphicsContext.DX9Device
      /// </summary>
      /// <returns></returns>
      public bool PreRender()
      { 

        //check if we should use GDI to draw the image
        if (GUIGraphicsContext.graphics != null)
        {
          // yes, If the GDI Image is not loaded, load the Image
          if (m_image == null)
          {
            string strFileName = _textureFileNameTag;
            if (_containsProperty)
              strFileName = GUIPropertyManager.Parse(_textureFileNameTag);
            if (strFileName != "-")
            {
              if (!System.IO.File.Exists(strFileName))
              {
                if (strFileName[1] != ':')
                  strFileName = GUIGraphicsContext.Skin + @"\media\" + strFileName;
              }
              m_image = GUITextureManager.GetImage(strFileName);
              strFileName = null;
            }
          }

          // Draw the GDI image
          if (m_image != null)
          {
            GUIGraphicsContext.graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            GUIGraphicsContext.graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            GUIGraphicsContext.graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            GUIGraphicsContext.graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            try
            {
              GUIGraphicsContext.graphics.DrawImage(m_image, m_destRect);
            }
            catch (Exception)
            {
            }
            return false;
          }
        }

        // if image is an animation then present the next frame
        if (m_vecTextures == null) return false;
        if (m_vecTextures.Length != 1)
          Process();

        // if the current frame is invalid then return
        if (_currentFrameNumber < 0 || _currentFrameNumber >= m_vecTextures.Length) return false;

        //get the current frame
        CachedTexture.Frame frame = m_vecTextures[_currentFrameNumber];
        if (frame == null) return false; // no frame? then return

        //get the texture of the frame
        Direct3D.Texture texture = frame.Image;
        if (texture == null)
        {
          // no texture? then return
          m_image = null;

          m_vecTextures = null;
          _currentFrameNumber = 0;
          _currentAnimationLoop = 0;
          _imageWidth = 0;
          _imageHeight = 0;
          _textureWidth = 0;
          _textureHeight = 0;
          frame = null;
          AllocResources();
          return false;
        }
        // is texture still valid?
        if (texture.Disposed)
        {
          //no? then return
          m_image = null;

          m_vecTextures = null;
          _currentFrameNumber = 0;
          _currentAnimationLoop = 0;
          _imageWidth = 0;
          _imageHeight = 0;
          _textureWidth = 0;
          _textureHeight = 0;
          frame = null;
          texture = null;
          AllocResources();
          return false;
        }

        return true;
      }

      public void RenderToSprite(Sprite sprite)
      {
        if (sprite == null) return;
        if (sprite.Disposed) return;

        if (!PreRender()) return; // SLOW

        //get the current frame
        CachedTexture.Frame frame = m_vecTextures[_currentFrameNumber];
        if (frame == null) return; // no frame? then return

        //get the texture of the frame
        Direct3D.Texture texture = frame.Image;
        // Set the scaling transform
        sprite.Transform = Matrix.Scaling(scaleX, scaleY, 1.0f);
        sprite.Draw(texture, sourceRect, new Vector3(), pntPosition, unchecked((int)_diffuseColor));
      }
      */
    public void RenderRect(float timePassed, Rectangle rectSrc, Rectangle rectDst)
    {
      _fx = rectDst.Left;
      _fy = rectDst.Top;
      _nw = rectDst.Width;
      _nh = rectDst.Height;
      float uoffs = ((float)(rectSrc.Left)) / ((float)(_textureWidth));
      float voffs = ((float)(rectSrc.Top)) / ((float)(_textureHeight));
      _umax = ((float)(rectSrc.Width)) / ((float)(_textureWidth));
      _vmax = ((float)(rectSrc.Height)) / ((float)(_textureHeight));


      if (_packedTexture != null)
      {
        _uoff = _texUoff + (uoffs * _texUmax);
        _voff = _texVoff + (voffs * _texVmax);
        _umax = _umax * _texUmax;
        _vmax = _vmax * _texVmax;
      }
      Render(timePassed);
    }
    /// <summary>
    /// Renders the Image
    /// </summary>
    public override void Render(float timePassed)
    {
      if (!IsVisible) return;
      if (!GUIGraphicsContext.ShowBackground && _isFullScreenImage) return;
      if (_packedTextureNo >= 0 && _packedTexture != null)
      {
        if (_packedTexture.Disposed)
        {
          FreeResourcesAndRegEvent();
          AllocResources();
          _reCalculate = true;
          return;
        }
      }

      if (_containsProperty && _propertyChanged)
      {
        _propertyChanged = false;
        string fileName = GUIPropertyManager.Parse(_textureFileNameTag);

        // if value changed or if we dont got any textures yet
        if (_cachedTextureFileName != fileName || _listTextures == null || 0 == _listTextures.Length)
        {
          // then free our resources, and reload the (new) image
          if (logtextures) _log.Info("GUIImage:PreRender() image changed:{0}->{1}", _cachedTextureFileName, fileName);
          FreeResourcesAndRegEvent();
          _cachedTextureFileName = fileName;
          if (fileName.Length == 0)
          {
            // filename for new image is empty
            // no need to load it
            return;
          }
          IsVisible = true;
          AllocResources();
          _reCalculate = true;
        }
      }
      if (!_allocated) return;

      if (_reCalculate)
      {
        Calculate();
      }

      //get the current frame
      if (_packedTextureNo >= 0)
      {
        if (Dimmed)
          FontEngineDrawTexture(_packedTextureNo, _fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)(_diffuseColor & DimColor));
        else
          FontEngineDrawTexture(_packedTextureNo, _fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)_diffuseColor);
        return;
      }
      else if (_listTextures != null)
      {
        if (_listTextures.Length > 0)
        {
          Animate();
          CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
          if (frame == null)
          {
            Cleanup();
            AllocResources();
            return;
          }
          if (frame.Image == null)
          {
            Cleanup();
            AllocResources();
            return;
          }
          if (Dimmed)
            frame.Draw(_fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)(_diffuseColor & DimColor));
          else
            frame.Draw(_fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)_diffuseColor);
          frame = null;
        }
      }
    }

    /// <summary>
    /// Set the filename of the texture and re-allocates the DirectX resources for this GUIImage.
    /// </summary>
    /// <param name="strFileName"></param>
    public void SetFileName(string fileName)
    {
      if (fileName == null) return;
      if (_textureFileNameTag == fileName) return;// same file, no need to do anything

      if (logtextures) _log.Info("GUIImage:SetFileName() {0}", fileName);
      _textureFileNameTag = fileName;
      if (_textureFileNameTag.IndexOf("#") >= 0) _containsProperty = true;
      else _containsProperty = false;

      //reallocate & load then new image
      _allocated = false;
      Cleanup();

      AllocResources();
    }

    /// <summary>
    /// Gets the rectangle in which this GUIImage is rendered.
    /// </summary>
    public Rectangle rect
    {
      get { return m_destRect; }
    }

    /// <summary>
    /// Property to enable/disable filtering
    /// </summary>
    public bool Filtering
    {
      get { return _filterImage; }
      set
      {
        if (_filterImage != value)
        {
          _filterImage = value;/*CreateStateBlock();*/
          _reCalculate = true;
        }

      }
    }

    /// <summary>
    /// Property which indicates if the image should be centered in the
    /// given rectangle of the control
    /// </summary>
    public bool Centered
    {
      get { return _centerImage; }
      set
      {
        if (_centerImage != value)
        {
          _centerImage = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should be zoomed in the
    /// given rectangle of the control
    /// </summary>
    public bool Zoom
    {
      get { return _zoomIn; }
      set
      {
        if (_zoomIn != value)
        {
          _zoomIn = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should retain its height 
    /// after it has been zoomed or aspectratio adjusted
    /// </summary>
    public bool FixedHeight
    {
      get { return _isFixedHeight; }
      set
      {
        if (_isFixedHeight != value)
        {
          _isFixedHeight = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should be zoomed into the
    /// given rectangle of the control. Zoom with fixed top, center width
    /// </summary>
    public bool ZoomFromTop
    {
      get { return _zoomFromTop; }
      set
      {
        if (_zoomFromTop != value)
        {
          _zoomFromTop = value;
          _reCalculate = true;
        }
      }
    }

    // recalculate the image dimensions & position
    public void Refresh()
    {
      Calculate();
    }

    /// <summary>
    /// property which returns true when this instance has a valid image
    /// </summary>
    public bool Allocated
    {
      get
      {
        if (FileName.Length == 0) return false;
        if (FileName.Equals("-")) return false;
        return true;
      }
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

    public override long ColourDiffuse
    {
      get
      {
        return base.ColourDiffuse;
      }
      set
      {
        if (base.ColourDiffuse != value)
        {
          base.ColourDiffuse = value;
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
    public override void Animate(float timePassed, Animator animator)
    {
      base.Animate(timePassed, animator);
      _reCalculate = true;
    }

    protected override void Update()
    {
      _reCalculate = true;
    }

		public RepeatBehavior RepeatBehavior
		{
			get { return _repeatBehavior; }
			set { _repeatBehavior = value; }
		}

		protected void LoadAnimation(ref string textureFiles)
		{
			string fileName = GUIGraphicsContext.Skin + "\\" + textureFiles;
			if (!System.IO.File.Exists(fileName)) return;
			XmlTextReader reader = new XmlTextReader(fileName);
			reader.WhitespaceHandling = WhitespaceHandling.None;
			// Parse the file and display each of the nodes.
      while (reader.Read()) 
			{
			  if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "textures" : 
							{
								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement) break;
									if (reader.NodeType == XmlNodeType.Text) textureFiles = reader.Value;
								}
								break;
							}
						case "RepeatBehavior":
							{
								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement) break;
									if (reader.NodeType == XmlNodeType.Text)
									{
										if (reader.Value.CompareTo("Forever")==0) _repeatBehavior = RepeatBehavior.Forever;
										else
										{
											_repeatBehavior = new RepeatBehavior(double.Parse(reader.Value));
										}
									}
								}
								break;
							}
					}
				   
				}
			}
		}

  }
}
