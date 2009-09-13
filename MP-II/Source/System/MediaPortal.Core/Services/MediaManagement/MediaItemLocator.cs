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
using MediaPortal.Core.General;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Core.MediaManagement.MediaProviders;

namespace MediaPortal.Core.Services.MediaManagement
{
  /// <summary>
  /// Encapsulates the data needed to locate a specific media item.
  /// </summary>
  /// <remarks>
  /// To locate a media item, we basically need its <see cref="SystemName"/>, the <see cref="Guid"/> of its
  /// <see cref="IMediaProvider"/> and its path into the media provider. This triple of data identifies a media item
  /// uniquely in an MP-II system.
  /// </remarks>
  public class MediaItemLocator : IMediaItemLocator
  {
    #region Public constants

    /// <summary>
    /// GUID string for the local filesystem media provider.
    /// </summary>
    public const string LOCAL_FS_MEDIA_PROVIDER_ID_STR = "{E88E64A8-0233-4fdf-BA27-0B44C6A39AE9}";

    /// <summary>
    /// Local filesystem media provider GUID.
    /// </summary>
    public static Guid LOCAL_FS_MEDIA_PROVIDER_ID = new Guid(LOCAL_FS_MEDIA_PROVIDER_ID_STR);

    #endregion

    #region Protected fields

    protected SystemName _system;
    protected Guid _mediaProviderId;
    protected string _path;

    #endregion

    public MediaItemLocator(SystemName system, Guid mediaProviderId, string path)
    {
      _system = system;
      _mediaProviderId = mediaProviderId;
      _path = path;
    }

    public SystemName SystemName
    {
      get { return _system; }
    }

    public Guid MediaProviderId
    {
      get { return _mediaProviderId; }
    }

    public string Path
    {
      get { return _path; }
    }

    public IMediaItemAccessor CreateAccessor()
    {
      if (_system.IsLocalSystem())
        return new MediaItemAccessor(this, _mediaProviderId, _path, null);
      else
          // TODO: Media item accessor for remote systems: return a media accessor with a provider and a path for it
          // pointing to the remote media
        throw new NotImplementedException("MediaItemLocator.CreateAccessor for remote media items is not implemented yet");
    }

    public IMediaItemLocalFsAccessor CreateLocalFsAccessor()
    {
      if (_system.IsLocalSystem() && MediaProviderId == LOCAL_FS_MEDIA_PROVIDER_ID)
          // Simple case: The media item is located in the local file system - we don't have to do anything
        return new MediaItemLocalFsAccessor(this, LocalFsMediaProviderBase.ToDosPath(_path), null);
      else
          // TODO: Media item accessor for remote systems: create temporary SMB connection and return an accessor to the
          // media provider for that SMB connection
        throw new NotImplementedException("MediaItemLocator.CreateAccessor for remote media items is not implemented yet");
    }
  }
}