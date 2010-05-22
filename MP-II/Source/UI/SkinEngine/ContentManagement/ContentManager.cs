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
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.Fonts;
using MediaPortal.Core.Messaging;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.ContentManagement
{
  // FIXME Albert: make a real signleton object
  public class ContentManager
  {
    static public int TextureReferences = 0;
    static public int VertexReferences = 0;

    #region Private variables

    private static Dictionary<string, IAsset> _assetsNormal = new Dictionary<string, IAsset>();
    private static Dictionary<string, IAsset> _assetsHigh = new Dictionary<string, IAsset>();
    private static List<IAsset> _vertexBuffers = new List<IAsset>();
    private static List<IAsset> _unnamedAssets = new List<IAsset>();
    private static DateTime _timer = SkinContext.FrameRenderingStartTime;
    private static AsynchronousMessageQueue _messageQueue;

    #endregion

    public static void Initialize()
    {
      _messageQueue = new AsynchronousMessageQueue(typeof(ContentManager).Name, new string[] {"contentmanager"});
      _messageQueue.MessageReceived += OnMessageReceived;
      _messageQueue.Start();
    }

    public static void Uninitialize()
    {
      _messageQueue.Dispose();
      _messageQueue = null;
    }

    static void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == "contentmanager")
      {
        if (message.MessageData.ContainsKey("action") && message.MessageData.ContainsKey("fullpath"))
        {
          string action = (string) message.MessageData["action"];
          if (action == "changed")
          {
            string fileName = (string) message.MessageData["fullpath"];
            lock (_assetsNormal)
              if (_assetsNormal.ContainsKey(fileName))
              {
                TextureAsset asset = (TextureAsset) _assetsNormal[fileName];
                asset.Free(true);
              }
            lock (_assetsHigh)
              if (_assetsHigh.ContainsKey(fileName))
              {
                TextureAsset asset = (TextureAsset) _assetsHigh[fileName];
                asset.Free(true);
              }
          }
        }
      }
    }

    /// <summary>
    /// Adds an asset to the un-named asset collection
    /// </summary>
    /// <param name="unknownAsset">The unknown asset.</param>
    public static void Add(IAsset unknownAsset)
    {
      lock (_unnamedAssets)
        _unnamedAssets.Add(unknownAsset);
    }

    /// <summary>
    /// Removes the specified  asset.
    /// </summary>
    /// <param name="unknownAsset">The unknown asset.</param>
    public static void Remove(IAsset unknownAsset)
    {
      lock (_unnamedAssets)
        if (_unnamedAssets.Remove(unknownAsset)) return;
      lock (_vertexBuffers)
        if (_vertexBuffers.Remove(unknownAsset)) return;
      lock (_assetsNormal)
      {
        Dictionary<string, IAsset>.Enumerator enumer = _assetsNormal.GetEnumerator();
        while (enumer.MoveNext())
          if (enumer.Current.Value == unknownAsset)
          {
            _assetsNormal.Remove(enumer.Current.Key);
            break;
          }
      }
      lock (_assetsHigh)
      {
        Dictionary<string, IAsset>.Enumerator enumer = _assetsHigh.GetEnumerator();
        while (enumer.MoveNext())
          if (enumer.Current.Value == unknownAsset)
          {
            _assetsHigh.Remove(enumer.Current.Key);
            break;
          }
      }
    }

    /// <summary>
    /// returns a fontbuffer asset for the specified font
    /// </summary>
    /// <param name="font">The font.</param>
    /// <returns></returns>
    public static FontBufferAsset GetFont(Font font)
    {
      lock (_vertexBuffers)
      {
        FontBufferAsset vertex = new FontBufferAsset(font);
        _vertexBuffers.Add(vertex);
        return vertex;
      }
    }

    /// <summary>
    /// returns a vertex buffer asset for the specified graphic file
    /// </summary>
    /// <param name="fileName">Name of the file (.jpg, .png).</param>
    /// <param name="thumb">If set to <c>true</c>, the image will be loaded as thumbnail.</param>
    /// <returns></returns>
    public static VertexBufferAsset Load(string fileName, bool thumb)
    {
      TextureAsset texture = GetTexture(fileName, thumb);
      lock (_vertexBuffers)
      {
        VertexBufferAsset vertex = new VertexBufferAsset(texture);
        _vertexBuffers.Add(vertex);
        return vertex;
      }
    }

    public static EffectAsset GetEffect(string effectName)
    {
      lock (_assetsNormal)
      {
        if (_assetsNormal.ContainsKey(effectName))
          return (EffectAsset)_assetsNormal[effectName];
        EffectAsset newEffect = new EffectAsset(effectName);
        _assetsNormal[effectName] = newEffect;
        return newEffect;
      }
    }

    /// <summary>
    /// returns a texture asset for the specified graphic file
    /// </summary>
    /// <param name="fileName">Name of the file (.jpg, .png).</param>
    /// <returns></returns>
    public static TextureAsset GetTexture(string fileName, bool thumb)
    {
      if (thumb)
      {
        lock (_assetsNormal)
        {
          if (_assetsNormal.ContainsKey(fileName))
            return (TextureAsset) _assetsNormal[fileName];
          TextureAsset newImage = new TextureAsset(fileName);
          _assetsNormal[fileName] = newImage;
          return newImage;
        }
      }

      lock (_assetsHigh)
      {
        if (_assetsHigh.ContainsKey(fileName))
          return (TextureAsset) _assetsHigh[fileName];
        TextureAsset newImage = new TextureAsset(fileName);
        _assetsHigh[fileName] = newImage;
        return newImage;
      }
    }

    /// <summary>
    /// Frees any un-used assets
    /// </summary>
    public static void Clean()
    {
      TimeSpan ts = SkinContext.FrameRenderingStartTime - _timer;
      if (ts.TotalSeconds < 1)
        return;
      _timer = SkinContext.FrameRenderingStartTime;

      Free(true, false);
    }

    protected static void Free(ICollection<IAsset> assets, bool checkIfCanBeDeleted, bool force)
    {
      lock (assets)
        foreach (IAsset asset in assets)
          if (asset.IsAllocated && (!checkIfCanBeDeleted || asset.CanBeDeleted))
            asset.Free(force);
    }

    protected static void Free(IDictionary<string, IAsset> assets, bool checkIfCanBeDeleted, bool force)
    {
      lock (assets)
        Free(assets.Values, checkIfCanBeDeleted, force);
    }

    public static void Free()
    {
      Free(false, true);
    }

    /// <summary>
    /// Frees all resources
    /// </summary>
    protected static void Free(bool checkIfCanBeDeleted, bool force)
    {
      Free(_assetsNormal, checkIfCanBeDeleted, force);
      Free(_assetsHigh, checkIfCanBeDeleted, force);
      Free(_unnamedAssets, checkIfCanBeDeleted, force);
      Free(_vertexBuffers, checkIfCanBeDeleted, force);
    }

    public static void Clear()
    {
      Free();

      _vertexBuffers.Clear();
      _unnamedAssets.Clear();
      _assetsHigh.Clear();
      _assetsNormal.Clear();
    }

  }
}
