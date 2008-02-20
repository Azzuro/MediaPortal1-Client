#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MediaPortal.Core;
using MediaPortal.Core.Properties;
using SlimDX;
using SlimDX.Direct3D;
using SlimDX.Direct3D9;
using SkinEngine.DirectX;
using MyXaml.Core;
namespace SkinEngine.Controls.Brushes
{
  public class BrushTexture : IAsset
  {
    Texture _texture;
    DateTime _lastTimeUsed;
    GradientStopCollection _stops;
    bool _opacityBrush;

    public BrushTexture(GradientStopCollection stops, bool opacityBrush)
    {
      _opacityBrush = opacityBrush;
      _stops = (GradientStopCollection)stops.Clone();
      Allocate();
      ContentManager.Add(this);
    }

    public void Allocate()
    {
      if (!IsAllocated)
      {
        _texture = new Texture(GraphicsDevice.Device, 256, 2, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
        CreateGradient();
        KeepAlive();
      }
    }

    public bool IsSame(GradientStopCollection stops)
    {
      if (stops.Count != _stops.Count)
        return false;
      for (int i = 0; i < _stops.Count; ++i)
      {
        if (_stops[i].Offset != stops[i].Offset)
          return false;
        if (_stops[i].Color != stops[i].Color)
          return false;
      }
      return true;
    }

    public void KeepAlive()
    {
      _lastTimeUsed = SkinContext.Now;
    }
    public bool OpacityBrush
    {
      get
      {
        return _opacityBrush;
      }
    }

    public Texture Texture
    {
      get
      {
        if (!IsAllocated)
        {
          Allocate();
          //Trace.WriteLine("Allocate brush");
        }
        KeepAlive();
        return _texture;
      }
    }
    void CreateGradient()
    {
      ///@optimize: use brush-cache
      LockedRect rect = _texture.LockRectangle(0, LockFlags.None);
      //int[,] buffer = (int[,])_texture.LockRectangle(typeof(int), 0, LockFlags.None, new int[] { (int)2, (int)256 });
      float width = 256.0f;
      byte[] data = new byte[4 * 512];
      int offY = 256 * 4;
      for (int i = 0; i < _stops.Count - 1; ++i)
      {
        GradientStop stopbegin = _stops[i];
        GradientStop stopend = _stops[i + 1];
        ColorValue colorStart = ColorConverter.FromColor(stopbegin.Color);
        ColorValue colorEnd = ColorConverter.FromColor(stopend.Color);
        int offsetStart = (int)(stopbegin.Offset * width);
        int offsetEnd = (int)(stopend.Offset * width);

        float distance = offsetEnd - offsetStart;
        for (int x = offsetStart; x < offsetEnd; ++x)
        {
          float step = (x - offsetStart) / distance;
          float r = step * (colorEnd.Red - colorStart.Red);
          r += colorStart.Red;

          float g = step * (colorEnd.Green - colorStart.Green);
          g += colorStart.Green;

          float b = step * (colorEnd.Blue - colorStart.Blue);
          b += colorStart.Blue;

          float a = step * (colorEnd.Alpha - colorStart.Alpha);
          a += colorStart.Alpha;

          if (OpacityBrush)
          {
            a *= 255;
            r = a;
            g = a;
            b = 255;
          }
          else
          {
            a *= 255;
            r *= 255;
            g *= 255;
            b *= 255;
          }

          int offx = x * 4;
          data[offx] = (byte)b;
          data[offx + 1] = (byte)g;
          data[offx + 2] = (byte)r;
          data[offx + 3] = (byte)a;

          data[offY + offx] = (byte)b;
          data[offY + offx + 1] = (byte)g;
          data[offY + offx + 2] = (byte)r;
          data[offY + offx + 3] = (byte)a;

        }
      }
      rect.Data.Write(data, 0, 4 * 512);
      _texture.UnlockRectangle(0);
      rect.Data.Dispose();

    }


    #region IAsset Members

    public bool IsAllocated
    {
      get
      {
        return (_texture != null);
      }
    }

    public bool CanBeDeleted
    {
      get
      {

        if (!IsAllocated)
        {
          return false;
        }
        TimeSpan ts = SkinContext.Now - _lastTimeUsed;
        if (ts.TotalSeconds >= 10)
        {
          return true;
        }

        return false;
      }
    }

    public bool Free(bool force)
    {
      if (_texture != null)
      {
        _texture.Dispose();
        _texture = null;
      }
      return false;
    }


    #endregion
  }
}
