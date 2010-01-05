#region Copyright (C) 2007-2009 Team MediaPortal

/*
    Copyright (C) 2007-2009 Team MediaPortal
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

namespace MediaPortal.Core.Services.MediaManagement
{
  public class ResourceLocator : IResourceLocator
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

    /// <summary>
    /// GUID string for the Tve3 media provider.
    /// </summary>
    protected const string TVE3_MEDIA_PROVIDER_ID_STR = "{DE191DC6-9E95-41b2-8459-36099E2C2774}";

    /// <summary>
    /// Tve3 media provider GUID.
    /// </summary>
    public static Guid TVE3_MEDIA_PROVIDER_ID = new Guid(TVE3_MEDIA_PROVIDER_ID_STR);

    #endregion

    #region Protected fields

    protected SystemName _nativeSystem;
    protected ResourcePath _nativeResourcePath;

    #endregion

    public ResourceLocator(SystemName system, ResourcePath nativeResourcePath)
    {
      _nativeSystem = system;
      _nativeResourcePath = nativeResourcePath;
    }

    public SystemName NativeSystem
    {
      get { return _nativeSystem; }
    }

    public ResourcePath NativeResourcePath
    {
      get { return _nativeResourcePath; }
    }

    // Implementation hint: This method is responsible for creating all temporary connections/mappings/resources to access
    // the media item specified by this instance. It is also responsible for providing an ITidyUpExecutor for cleaning up
    // the resources which have been set up.
    public IResourceAccessor CreateAccessor()
    {
      if (!_nativeSystem.IsLocalSystem())
        throw new NotImplementedException("ResourceLocator.CreateAccessor for remote media items is not implemented yet");
      _nativeResourcePath.CheckValidLocalPath();
      return _nativeResourcePath.CreateLocalMediaItemAccessor();
    }

    // Implementation hint: This method is responsible for creating all temporary connections/mappings/resources to access
    // the media item specified by this instance. It is also responsible for providing an ITidyUpExecutor for cleaning up
    // the resources which have been set up.
    public ILocalFsResourceAccessor CreateLocalFsAccessor()
    {
      if (!_nativeSystem.IsLocalSystem())
        // TODO: Resource accessor for remote systems: create temporary SMB connection or other mapping to a local file path
        // and return a local FS accessor to that mapped local file path
        throw new NotImplementedException("ResourceLocator.CreateLocalFsAccessor for remote media items is not implemented yet");
      
      if (_nativeResourcePath.PathSegments.Count != 1 ||
          (
          _nativeResourcePath.BasePathSegment.ProviderId != LOCAL_FS_MEDIA_PROVIDER_ID &&
          _nativeResourcePath.BasePathSegment.ProviderId != TVE3_MEDIA_PROVIDER_ID
          )
        )
        // TODO: Create mapping of the complex resource path to a local file and return a local FS accessor to that
        // mapped local file path
        throw new NotImplementedException("ResourceLocator.CreateLocalFsAccessor for complex paths to media items is not implemented yet");

      // Simple case: The media item is located in the local file system - we don't have to do anything
      return _nativeResourcePath.CreateLocalMediaItemAccessor() as ILocalFsResourceAccessor;
    }
  }
}