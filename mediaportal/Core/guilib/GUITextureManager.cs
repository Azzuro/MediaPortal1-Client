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
//#define DO_RESAMPLE
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITextureManager
  {
    const int MAX_THUMB_WIDTH = 512;
    const int MAX_THUMB_HEIGHT = 512;
    static List<CachedTexture> _cache = new List<CachedTexture>();
    static List<DownloadedImage> _cacheDownload = new List<DownloadedImage>();
    static TexturePacker _packer = new TexturePacker();

    // singleton. Dont allow any instance of this class
    private GUITextureManager()
    {
    }

    ~GUITextureManager()
    {
      dispose(false);
    }

    static public void Dispose()
    {
      dispose(true);
    }

    static void dispose(bool disposing)
    {
      Log.Write("texturemanager:dispose()");
      _packer.Dispose();
      if (disposing)
      {
        foreach (CachedTexture cached in _cache)
        {
          cached.Dispose();
        }
        _cache.Clear();
      }
      _cacheDownload.Clear();

      string[] files = System.IO.Directory.GetFiles("thumbs", "MPTemp*.*");
      if (files != null)
      {
        foreach (string file in files)
        {
          try
          {
            System.IO.File.Delete(file);
          }
          catch (Exception)
          {
          }
        }
      }
    }

    static public void StartPreLoad()
    {
      //TODO
    }
    static public void EndPreLoad()
    {
      //TODO
    }

    static public Image Resample(Image imgSrc, int iMaxWidth, int iMaxHeight)
    {
      int width = imgSrc.Width;
      int height = imgSrc.Height;
      while (width < iMaxWidth || height < iMaxHeight)
      {
        width *= 2;
        height *= 2;
      }
      float fAspect = ((float)width) / ((float)height);

      if (width > iMaxWidth)
      {
        width = iMaxWidth;
        height = (int)Math.Round(((float)width) / fAspect);
      }

      if (height > (int)iMaxHeight)
      {
        height = iMaxHeight;
        width = (int)Math.Round(fAspect * ((float)height));
      }

      Bitmap result = new Bitmap(width, height);
      using (Graphics g = Graphics.FromImage(result))
      {
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.DrawImage(imgSrc, new Rectangle(0, 0, width, height));
      }
      return result;
    }

    static string GetFileName(string fileName)
    {
      if (fileName.Length == 0) return "";
      if (fileName == "-") return "";
      string lowerFileName = fileName.ToLower().Trim();
      if (lowerFileName.IndexOf(@"http:") >= 0)
      {
        foreach (DownloadedImage image in _cacheDownload)
        {
          if (String.Compare(image.URL, fileName, true) == 0)
          {
            if (image.ShouldDownLoad)
            {
              image.Download();
            }
            return image.FileName;
          }
        }
        DownloadedImage newimage = new DownloadedImage(fileName);
        newimage.Download();
        _cacheDownload.Add(newimage);
        return newimage.FileName;
      }

      if (!System.IO.File.Exists(fileName))
      {
        if (fileName[1] != ':')
          return GUIGraphicsContext.Skin + @"\media\" + fileName;
      }
      return fileName;
    }

    static public int Load(string fileNameOrg, long lColorKey, int iMaxWidth, int iMaxHeight)
    {
      string fileName = GetFileName(fileNameOrg);
      if (fileName == "") return 0;

      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];

        if (String.Compare(cached.Name, fileName, true) == 0)
        {
          return cached.Frames;
        }
      }

      string extensionension = System.IO.Path.GetExtension(fileName).ToLower();
      if (extensionension == ".gif")
      {
        if (!System.IO.File.Exists(fileName))
        {
          Log.Write("texture:{0} does not exists", fileName);
          return 0;
        }

        Image theImage = null;
        try
        {
          theImage = Image.FromFile(fileName);
          if (theImage != null)
          {
            CachedTexture newCache = new CachedTexture();

            newCache.Name = fileName;
            FrameDimension oDimension = new FrameDimension(theImage.FrameDimensionsList[0]);
            newCache.Frames = theImage.GetFrameCount(oDimension);
            int[] frameDelay = new int[newCache.Frames];
            for (int num2 = 0; (num2 < newCache.Frames); ++num2) frameDelay[num2] = 0;

            int num1 = 20736;
            PropertyItem item1 = theImage.GetPropertyItem(num1);
            if (item1 != null)
            {
              byte[] buffer1 = item1.Value;
              for (int num2 = 0; (num2 < newCache.Frames); ++num2)
              {
                frameDelay[num2] = (((buffer1[(num2 * 4)] + (256 * buffer1[((num2 * 4) + 1)])) + (65536 * buffer1[((num2 * 4) + 2)])) + (16777216 * buffer1[((num2 * 4) + 3)]));
              }
            }
            for (int i = 0; i < newCache.Frames; ++i)
            {
              theImage.SelectActiveFrame(oDimension, i);


              //load gif into texture
              using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
              {
                theImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                ImageInformation info2 = new ImageInformation();
                stream.Flush();
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                Direct3D.Texture texture = TextureLoader.FromStream(
                                                                  GUIGraphicsContext.DX9Device,
                                                                  stream,
                                                                  0, 0,//width/height
                                                                  1,//mipslevels
                                                                  0,//Usage.Dynamic,
                                                                  Direct3D.Format.A8R8G8B8,
                                                                  Pool.Managed,
                                                                  Filter.None,
                                                                  Filter.None,
                                                                  (int)lColorKey,
                                                                  ref info2);
                newCache.Width = info2.Width;
                newCache.Height = info2.Height;
                newCache[i] = new CachedTexture.Frame(fileName, texture, (frameDelay[i] / 5) * 50);
              }
            }

            theImage.Dispose();
            theImage = null;
            _cache.Add(newCache);

            Log.Write("  texturemanager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
            return newCache.Frames;
          }
        }
        catch (Exception ex)
        {
          Log.Write("TextureManager:exception loading texture {0}", fileName);
          Log.Write(ex);
        }
        return 0;
      }

      if (System.IO.File.Exists(fileName))
      {
        int width, height;
        Direct3D.Texture dxtexture = LoadGraphic(fileName, lColorKey, iMaxWidth, iMaxHeight, out width, out height);
        if (dxtexture != null)
        {
          CachedTexture newCache = new CachedTexture();
          newCache.Name = fileName;
          newCache.Frames = 1;
          newCache.Width = width;
          newCache.Height = height;
          newCache.texture = new CachedTexture.Frame(fileName, dxtexture, 0);
          Log.Write("  texturemanager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
          _cache.Add(newCache);
          return 1;
        }
      }
      return 0;
    }
    static public int LoadFromMemory(System.Drawing.Image memoryImage, string name, long lColorKey, int iMaxWidth, int iMaxHeight)
    {
      string cacheName = name;
      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];

        if (String.Compare(cached.Name, cacheName, true) == 0)
        {
          return cached.Frames;
        }
      }
      if (memoryImage == null) return 0;
      if (memoryImage.FrameDimensionsList == null) return 0;
      if (memoryImage.FrameDimensionsList.Length == 0) return 0;

      try
      {
        CachedTexture newCache = new CachedTexture();

        newCache.Name = cacheName;
        FrameDimension oDimension = new FrameDimension(memoryImage.FrameDimensionsList[0]);
        newCache.Frames = memoryImage.GetFrameCount(oDimension);
        if (newCache.Frames != 1) return 0;
        //load gif into texture
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
          memoryImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
          ImageInformation info2 = new ImageInformation();
          stream.Flush();
          stream.Seek(0, System.IO.SeekOrigin.Begin);
          Direct3D.Texture texture = TextureLoader.FromStream(
            GUIGraphicsContext.DX9Device,
            stream,
            0, 0,//width/height
            1,//mipslevels
            0,//Usage.Dynamic,
            Direct3D.Format.A8R8G8B8,
            Pool.Managed,
            Filter.None,
            Filter.None,
            (int)lColorKey,
            ref info2);
          newCache.Width = info2.Width;
          newCache.Height = info2.Height;
          newCache.texture = new CachedTexture.Frame(cacheName, texture, 0);
        }
        memoryImage.Dispose();
        memoryImage = null;
        _cache.Add(newCache);

        Log.Write("  texturemanager:added: memoryImage  " + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
        return newCache.Frames;

      }
      catch (Exception ex)
      {
        Log.Write("TextureManager: exception loading texture memoryImage");
        Log.Write(ex);
      }
      return 0;
    }
    static Direct3D.Texture LoadGraphic(string fileName, long lColorKey, int iMaxWidth, int iMaxHeight, out int width, out int height)
    {
      width = 0;
      height = 0;
      Image imgSrc = null;
      Direct3D.Texture texture = null;
      try
      {
#if DO_RESAMPLE
        imgSrc=Image.FromFile(fileName);   
        if (imgSrc==null) return null;
				//Direct3D prefers textures which height/width are a power of 2
				//doing this will increases performance
				//So the following core resamples all textures to
				//make sure all are 2x2, 4x4, 8x8, 16x16, 32x32, 64x64, 128x128, 256x256, 512x512
				int w=-1,h=-1;
				if (imgSrc.Width >2   && imgSrc.Width < 4)  w=2;
				if (imgSrc.Width >4   && imgSrc.Width < 8)  w=4;
				if (imgSrc.Width >8   && imgSrc.Width < 16) w=8;
				if (imgSrc.Width >16  && imgSrc.Width < 32) w=16;
				if (imgSrc.Width >32  && imgSrc.Width < 64) w=32;
				if (imgSrc.Width >64  && imgSrc.Width <128) w=64;
				if (imgSrc.Width >128 && imgSrc.Width <256) w=128;
				if (imgSrc.Width >256 && imgSrc.Width <512) w=256;
				if (imgSrc.Width >512 && imgSrc.Width <1024) w=512;


				if (imgSrc.Height >2   && imgSrc.Height < 4)  h=2;
				if (imgSrc.Height >4   && imgSrc.Height < 8)  h=4;
				if (imgSrc.Height >8   && imgSrc.Height < 16) h=8;				
				if (imgSrc.Height >16  && imgSrc.Height < 32) h=16;
				if (imgSrc.Height >32  && imgSrc.Height < 64) h=32;
				if (imgSrc.Height >64  && imgSrc.Height <128) h=64;
				if (imgSrc.Height >128 && imgSrc.Height <256) h=128;
				if (imgSrc.Height >256 && imgSrc.Height <512) h=256;
				if (imgSrc.Height >512 && imgSrc.Height <1024) h=512;
				if (w>0 || h>0)
				{
					if (h > w) w=h;
					Log.Write("TextureManager: resample {0}x{1} -> {2}x{3} {4}",
												imgSrc.Width,imgSrc.Height, w,w,fileName);

					Image imgResampled=Resample(imgSrc,w, h);
					imgSrc.Dispose();
					imgSrc=imgResampled;
					imgResampled=null;
				}
#endif

        //Format fmt=Format.A8R8G8B8;
        if (IsTemporary(fileName))
        {
          //fmt=Format.Dxt3;
          iMaxWidth = MAX_THUMB_WIDTH;
          iMaxHeight = MAX_THUMB_HEIGHT;
          imgSrc = Image.FromFile(fileName);
          if (imgSrc == null) return null;
          if (imgSrc.Width >= iMaxWidth || imgSrc.Height >= iMaxHeight)
          {
            Image imgResampled = Resample(imgSrc, iMaxWidth, iMaxHeight);
            imgSrc.Dispose();
            imgSrc = imgResampled;
            imgResampled = null;
          }
          //load jpg or png into texture
          using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
          {
            imgSrc.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            ImageInformation info2 = new ImageInformation();
            stream.Flush();
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            texture = TextureLoader.FromStream(GUIGraphicsContext.DX9Device,
              stream,
              0, 0,//width/height
              1,//mipslevels
              0,//Usage.Dynamic,
              Direct3D.Format.A8R8G8B8,
              Pool.Managed,
              Filter.None,
              Filter.None,
              (int)lColorKey,
              ref info2);
            width = info2.Width;
            height = info2.Height;

            //Log.Write("Texturemanager loaded temporay:{0} {1}x{2} format:{3}", fileName, width, height, info2.Format);
          }
        }
        else
        {
          //fmt=GetCompression(fileName);
          //fmt=Direct3D.Format.Dxt3;
          ImageInformation info2 = new ImageInformation();
          texture = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                          fileName,
                                          0, 0,//width/height
                                          1,//mipslevels
                                          0,//Usage.Dynamic,
                                          Direct3D.Format.A8R8G8B8,
                                          Pool.Managed,
                                          Filter.None,
                                          Filter.None,
                                          (int)lColorKey,
                                          ref info2);
          width = info2.Width;
          height = info2.Height;
          /*
          if (width > (GUIGraphicsContext.Width/2) ||
            height> (GUIGraphicsContext.Height/2) )
          {
            texture.Dispose();
            fmt=Direct3D.Format.A8R8G8B8;
            texture=TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
              fileName,
              0,0,//width/height
              1,//mipslevels
              0,//Usage.Dynamic,
              fmt,
              Pool.Managed,
              Filter.None,
              Filter.None,
              (int)lColorKey,
              ref info2);
            width=info2.Width;
            height=info2.Height;
          }
          Log.Write("Texturemanager loaded:{0} {1}x{2} format:{3}",
                        fileName,width,height,info2.Format);*/

        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "TextureManage:LoadGraphic({0})", fileName);
        Log.Write(ex);
      }
      finally
      {
        if (imgSrc != null)
        {
          imgSrc.Dispose();
        }
      }
      return texture;
    }

    static public Image GetImage(string fileNameOrg)
    {
      string fileName = GetFileName(fileNameOrg);
      if (fileNameOrg.StartsWith("["))
        fileName = fileNameOrg;
      if (fileName == "") return null;

      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];
        if (String.Compare(cached.Name, fileName, true) == 0)
        {
          if (cached.image != null)
            return cached.image;
          else
          {

            try
            {
              cached.image = Image.FromFile(fileName);
            }
            catch (Exception ex)
            {
              Log.WriteFile(Log.LogType.Log, true, "TextureManage:GetImage({0}) ", fileName);
              Log.Write(ex);
              return null;
            }
            return cached.image;
          }
        }
      }

      if (!System.IO.File.Exists(fileName)) return null;
      Image img = null;
      try
      {
        img = Image.FromFile(fileName);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "TextureManage:GetImage({0})", fileName);
        Log.Write(ex);
        return null;
      }
      if (img != null)
      {
        CachedTexture newCache = new CachedTexture();
        newCache.Frames = 1;
        newCache.Name = fileName;
        newCache.Width = img.Width;
        newCache.Height = img.Height;
        newCache.image = img;
        Log.Write("  texturemanager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
        _cache.Add(newCache);
        return img;
      }
      return null;
    }

    static public CachedTexture.Frame GetTexture(string fileNameOrg, int iImage, out int iTextureWidth, out int iTextureHeight)
    {
      iTextureWidth = 0;
      iTextureHeight = 0;
      string fileName = "";
      if (!fileNameOrg.StartsWith("["))
      {
        fileName = GetFileName(fileNameOrg);
        if (fileName == "") return null;
      }
      else
        fileName = fileNameOrg;
      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];
        if (String.Compare(cached.Name, fileName, true) == 0)
        {
          iTextureWidth = cached.Width;
          iTextureHeight = cached.Height;
          return (CachedTexture.Frame)cached[iImage];
        }
      }
      return null;
    }

    static public void ReleaseTexture(string fileName)
    {
      if (fileName == String.Empty) return;

      //dont dispose radio/tv logo's since they are used by the overlay windows
      if (fileName.ToLower().IndexOf(@"thumbs\tv\logos") >= 0) return;
      if (fileName.ToLower().IndexOf(@"thumbs\radio") >= 0) return;
      try
      {
        bool continueRemoving = false;
        do
        {
          continueRemoving = false;
          foreach (CachedTexture cached in _cache)
          {
            if (String.Compare(cached.Name, fileName, true) == 0)
            {
              Log.Write("texturemanager:dispose:{0} frames:{1} total:{2} mem left:{3}", cached.Name, cached.Frames, _cache.Count, GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
              _cache.Remove(cached);
              cached.Dispose();
              continueRemoving = true;
              break;
            }
          }
        } while (continueRemoving);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "TextureManage:ReleaseTexture({0})", fileName);
        Log.Write(ex);
      }
    }

    static public void PreLoad(string fileName)
    {
      //TODO
    }

    static public void CleanupThumbs()
    {
      Log.Write("texturemanager:CleanupThumbs()");
      try
      {
        List<CachedTexture> newCache = new List<CachedTexture>();
        foreach (CachedTexture cached in _cache)
        {
          if (IsTemporary(cached.Name))
          {
            Log.Write("texturemanager:dispose:" + cached.Name + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
            cached.Dispose();
          }
          else
          {
            newCache.Add(cached);
          }
        }
        _cache.Clear();
        _cache = newCache;
      }
      catch (Exception ex)
      {
        Log.Write("TextureManage:CleanupThumbs() ");
        Log.Write(ex);
      }
    }

    static public bool IsTemporary(string fileName)
    {
      if (fileName.Length == 0) return false;
      if (fileName == "-") return false;

      if (fileName.ToLower().IndexOf(@"thumbs\tv\logos") >= 0) return false;
      if (fileName.ToLower().IndexOf(@"thumbs\radio") >= 0) return false;

      /* Temporary: (textures that are disposed)
       * - all not skin images
       * 
       * NOT Temporary: (textures that are kept in cache)
       * - all skin graphics
       * 
       */

      // Get fullpath and file name
      string fullFileName = fileName;
      if (!System.IO.File.Exists(fileName))
      {
        if (fileName[1] != ':')
          fullFileName = GUIGraphicsContext.Skin + @"\media\" + fileName;
      }

      // Check if skin file
      if (fullFileName.ToLower().IndexOf(@"skin\") >= 0)
      {
        if (fullFileName.ToLower().IndexOf(@"media\animations\") >= 0)
          return true;
        if (fullFileName.ToLower().IndexOf(@"media\tetris\") >= 0)
          return true;
        return false;
      }
      return true;
    }

    static public void Init()
    {
      _packer.PackSkinGraphics(GUIGraphicsContext.Skin);
    }

    static public bool GetPackedTexture(string fileName, out float uoff, out float voff, out float umax, out float vmax, out int textureWidth, out int textureHeight, out Texture tex, out int _packedTextureNo)
    {
      return _packer.Get(fileName, out uoff, out voff, out umax, out vmax, out textureWidth, out textureHeight, out tex, out _packedTextureNo);
    }

    static public void Clear()
    {
      _packer.Dispose();
      _packer = new TexturePacker();
      _packer.PackSkinGraphics(GUIGraphicsContext.Skin);

      _cache.Clear();
      _cacheDownload.Clear();
    }
  }
}
