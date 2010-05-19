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

using System.Collections.Generic;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  // TODO Albert: Cleanup brush textures, when not used any more
  public class BrushCache
  {
    static BrushCache _instance;
    ICollection<GradientBrushTexture> _cache;

    static BrushCache()
    {
      _instance = new BrushCache();
    }

    public static BrushCache Instance
    {
      get
      {
        return _instance;
      }
    }
    public BrushCache()
    {
      _cache = new List<GradientBrushTexture>();
    }

    public GradientBrushTexture GetGradientBrush(GradientStopCollection stops)
    {
      foreach (GradientBrushTexture texture in _cache)
        if (texture.IsSame(stops))
          return texture;
      // Here we must do a copy of the gradient stops. If we don't, the cache will change
      // when the stops are changed outside.
      GradientBrushTexture gradientBrush = new GradientBrushTexture(new GradientStopCollection(stops, null));
      _cache.Add(gradientBrush);
      return gradientBrush;
    }

  }
}
