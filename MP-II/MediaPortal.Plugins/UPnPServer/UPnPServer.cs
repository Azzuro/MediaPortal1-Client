﻿#region Copyright (C) 2007-2008 Team MediaPortal

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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MediaPortal.Core;
using MediaPortal.Core.PluginManager;
using MediaPortal.Core.Logging;
using MediaPortal.Core.Database.Interfaces;
using MediaPortal.Core.MediaManager;
using MediaPortal.Core.MediaManager.Views;

using Intel.UPNP.AV.MediaServer.CP;
using Intel.UPNP.AV.MediaServer.DV;
using Intel.UPNP.AV.CdsMetadata;
using IMediaItem = MediaPortal.Core.MediaManager.IMediaItem;

namespace MediaPortal.UPnPServer
{
  public class UPnPServer : IPlugin, IAutoStart
  {
    #region IPlugin Members

    private MediaServerCore2 _mediaServerCore;
    private UPnPMediaServer2 _mediaServer;

    public void Initialize(string id)
    {
      //   Start();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
    }

    #endregion

    #region IAutoStart Members

    public void Startup()
    {
#if DEBUG
      return;
#endif
      Thread startupThread = new Thread(new ThreadStart(Start));
      startupThread.IsBackground = true;
      startupThread.Name = "UpnP server start";
      startupThread.Priority = ThreadPriority.BelowNormal;
      startupThread.Start();

    }

    #endregion

    void Start()
    {
      System.Threading.Thread.Sleep(10000);
      ServiceScope.Get<ILogger>().Info("UPNP server: starting");
      _mediaServerCore = new MediaServerCore2(System.Environment.MachineName + ": MP-II");
      _mediaServer = new UPnPMediaServer2();
      IRootContainer root = null;
      List<IAbstractMediaItem> itemsInView = ServiceScope.Get<IMediaManager>().GetView(root);

      foreach (IAbstractMediaItem item in itemsInView)
      {
        IRootContainer container = item as IRootContainer;
        if (container != null && container.IsLocal)
        {
          ServiceScope.Get<ILogger>().Info("UPNP server: serving {0}", container.Title);
          DvMediaContainer2 mediaContainer = _mediaServer.AddDirectory(null, container.Title);
          mediaContainer.Context = item;
          mediaContainer.OnAddChildren += new DvMediaContainer2.Delegate_AddChildren(mediaContainer_OnAddChildren);
        }
      }
      ServiceScope.Get<ILogger>().Info("UPNP server: running");
    }

    void mediaContainer_OnAddChildren(DvMediaContainer2 parent)
    {
      if (parent.ChildCount > 0) return;
      IRootContainer root = parent.Context as IRootContainer;
      if (root == null) return;

      ServiceScope.Get<ILogger>().Info("UPNP server: get {0}", root.Title);
      List<IAbstractMediaItem> itemsInView = ServiceScope.Get<IMediaManager>().GetView(root);
      if (itemsInView == null) return;
      //if (root.Title != "Music") return;
      foreach (IAbstractMediaItem abstractItem in itemsInView)
      {
        IRootContainer container = abstractItem as IRootContainer;
        if (container != null)
        {
          DvMediaContainer2 mediaContainer = _mediaServer.AddDirectory(parent, container.Title);
          mediaContainer.Context = abstractItem;
          mediaContainer.OnAddChildren += new DvMediaContainer2.Delegate_AddChildren(mediaContainer_OnAddChildren);
        }

        IMediaItem mediaItem = abstractItem as IMediaItem;
        if (mediaItem != null)
        {
          Uri uri = mediaItem.ContentUri;
          if (uri == null) continue;
          if (uri.IsFile)
          {
            string fileName = uri.LocalPath;
            string extension = System.IO.Path.GetExtension(fileName);
            if (IsMovie(mediaItem))
            {
              MediaBuilder.videoItem info = new MediaBuilder.videoItem(abstractItem.Title);
              DvMediaItem2 newMediaItem = DvMediaBuilder2.CreateItem(info);

              string str;
              string str2;
              MimeTypes.ExtensionToMimeType(extension, out str, out str2);
              string protocolInfo = new StringBuilder().AppendFormat("http-get:*:{0}:*", str).ToString();
              ResourceBuilder.AllResourceAttributes attribs = new ResourceBuilder.AllResourceAttributes();
              attribs.contentUri = MediaResource.AUTOMAPFILE + fileName;
              attribs.protocolInfo = new ProtocolInfoString(protocolInfo);
              AddAttributes(attribs, mediaItem);
              AddMetaData(newMediaItem, mediaItem);

              DvMediaResource addThis = DvResourceBuilder.CreateResource(attribs, true);
              addThis.Tag = abstractItem;
              newMediaItem.AddResource(addThis);

              parent.AddObject(newMediaItem, true);
            }

            if (IsPicture(mediaItem))
            {
              MediaBuilder.photo info = new MediaBuilder.photo(abstractItem.Title);
              DvMediaItem2 newMediaItem = DvMediaBuilder2.CreateItem(info);

              string str;
              string str2;
              MimeTypes.ExtensionToMimeType(extension, out str, out str2);
              string protocolInfo = new StringBuilder().AppendFormat("http-get:*:{0}:*", str).ToString();
              ResourceBuilder.AllResourceAttributes attribs = new ResourceBuilder.AllResourceAttributes();
              attribs.contentUri = MediaResource.AUTOMAPFILE + fileName;
              attribs.protocolInfo = new ProtocolInfoString(protocolInfo);
              AddAttributes(attribs, mediaItem);
              AddMetaData(newMediaItem, mediaItem);


              DvMediaResource addThis = DvResourceBuilder.CreateResource(attribs, true);
              addThis.Tag = abstractItem;
              newMediaItem.AddResource(addThis);

              parent.AddObject(newMediaItem, true);
            }
            if (IsMusic(mediaItem))
            {
              MediaBuilder.musicTrack info = new MediaBuilder.musicTrack(abstractItem.Title);
              info.creator = "";
              DvMediaItem2 newMediaItem = DvMediaBuilder2.CreateItem(info);

              string str;
              string str2;
              MimeTypes.ExtensionToMimeType(extension, out str, out str2);
              string protocolInfo = new StringBuilder().AppendFormat("http-get:*:{0}:*", str).ToString();
              ResourceBuilder.AllResourceAttributes attribs = new ResourceBuilder.AllResourceAttributes();
              attribs.contentUri = MediaResource.AUTOMAPFILE + fileName;
              attribs.protocolInfo = new ProtocolInfoString(protocolInfo);

              AddAttributes(attribs, mediaItem);
              AddMetaData(newMediaItem, mediaItem);


              DvMediaResource addThis = DvResourceBuilder.CreateResource(attribs, true);
              addThis.Tag = abstractItem;
              newMediaItem.AddResource(addThis);

              parent.AddObject(newMediaItem, true);
            }
          }
        }
      }
    }
    bool IsMovie(IMediaItem mediaItem)
    {
      string extension = mediaItem.ContentUri.AbsoluteUri.ToLower();
      if (extension.IndexOf(".mpg") >= 0) return true;
      if (extension.IndexOf(".ts") >= 0) return true;
      if (extension.IndexOf(".avi") >= 0) return true;
      if (extension.IndexOf(".wmv") >= 0) return true;
      if (extension.IndexOf(".mkv") >= 0) return true;
      return false;
    }
    bool IsPicture(IMediaItem mediaItem)
    {
      string extension = mediaItem.ContentUri.AbsoluteUri.ToLower();
      if (extension.IndexOf(".jpg") >= 0) return true;
      if (extension.IndexOf(".gif") >= 0) return true;
      if (extension.IndexOf(".png") >= 0) return true;
      if (extension.IndexOf(".bmp") >= 0) return true;
      return false;
    }
    bool IsMusic(IMediaItem mediaItem)
    {
      string extension = mediaItem.ContentUri.AbsoluteUri.ToLower();
      if (extension.IndexOf(".mp3") >= 0) return true;
      if (extension.IndexOf(".wma") >= 0) return true;
      return false;
    }

    void AddAttributes(ResourceBuilder.AllResourceAttributes attribs, IMediaItem mediaItem)
    {
      if (mediaItem.MetaData.ContainsKey("duration"))
      {
        attribs.duration = new _TimeSpan(new TimeSpan((int)mediaItem.MetaData["duration"]));
      }
      if (mediaItem.MetaData.ContainsKey("sampleFrequency"))
      {
        attribs.sampleFrequency = new _UInt((uint)mediaItem.MetaData["sampleFrequency"]);
      }
      if (mediaItem.MetaData.ContainsKey("nrAudioChannels"))
      {
        attribs.nrAudioChannels = new _UInt((uint)mediaItem.MetaData["nrAudioChannels"]);
      }
      if (mediaItem.MetaData.ContainsKey("bitrate"))
      {
        attribs.bitrate = new _UInt((uint)mediaItem.MetaData["bitrate"]);
      }
      if (mediaItem.MetaData.ContainsKey("ImgDimensions"))
      {
        string dimensions = (string)mediaItem.MetaData["ImgDimensions"];
        int pos = dimensions.IndexOf("x");
        if (pos > 0)
        {
          string width = dimensions.Substring(0, pos).Trim();
          string height = dimensions.Substring(pos + 1).Trim();
          attribs.resolution = new ImageDimensions(Int32.Parse(width), Int32.Parse(height));
        }
      }
    }
    void AddMetaData(DvMediaItem2 upnpItem, IMediaItem mediaItem)
    {
      Dictionary<string, object>.Enumerator enumer = mediaItem.MetaData.GetEnumerator();
      while (enumer.MoveNext())
      {
        object val = enumer.Current.Value;
        if (val == null) continue;
        if (val.GetType() == typeof(Int32))
          upnpItem.SetPropertyValue_Int(enumer.Current.Key, (int)val);
        else if (val.GetType() == typeof(String))
          upnpItem.SetPropertyValue_String(enumer.Current.Key, (string)val);
        else if (val.GetType() == typeof(long))
          upnpItem.SetPropertyValue_Long(enumer.Current.Key, (long)val);
      }
    }
  }

}
