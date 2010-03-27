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
using System.IO;
using MediaPortal.Utilities.Exceptions;

namespace MediaPortal.Core.MediaManagement
{
  public interface ITidyUpExecutor
  {
    void Execute();
  }

  /// <summary>
  /// Temporary local accessor instance for a resource which might located anywhere in an MP-II system.
  /// </summary>
  /// <remarks>
  /// Via this instance, the resource, which potentially is located in a remote system, can be accessed
  /// via a local media provider chain specified by the <see cref="LocalResourcePath"/>.
  /// To get a resource accessor, get an <see cref="IResourceLocator"/> and use its
  /// <see cref="IResourceLocator.CreateAccessor"/> method.
  /// The temporary resource accessor must be disposed using its <see cref="IDisposable.Dispose"/> method
  /// when it is not needed any more. This will clean up resources which were allocated to access the resource.
  /// </remarks>
  public interface IResourceAccessor : IDisposable
  {
    /// <summary>
    /// Returns the media provider which provides this resource.
    /// </summary>
    IMediaProvider ParentProvider { get; }

    /// <summary>
    /// Returns the information if this resource is a file which can be opened to an input stream.
    /// </summary>
    /// <value><c>true</c>, if this resource denotes a file which can be opened, else <c>false</c>.</value>
    bool IsFile { get; }

    /// <summary>
    /// Returns a short, human readable name for this resource.
    /// </summary>
    /// <value>A human readable name of this resource. For a filesystem resource accessor,
    /// this could be the file name or directory name, for example.</value>
    string ResourceName { get; }

    /// <summary>
    /// Returns the full human readable path name for this resource.
    /// </summary>
    /// <value>A human readable name of this resource. For a filesystem resource accessor,
    /// this could be the file path, for example.</value>
    string ResourcePathName { get; }

    /// <summary>
    /// Returns the technical resource path which points to this resource.
    /// </summary>
    ResourcePath LocalResourcePath { get; }

    /// <summary>
    /// Gets the date and time when this resource was changed for the last time.
    /// </summary>
    DateTime LastChanged { get; }

    /// <summary>
    /// Returns the information if the resource at the given path exists in the media provider of this resource.
    /// </summary>
    /// <remarks>
    /// This method is defined in interface <see cref="IResourceAccessor"/> rather than in interface <see cref="IMediaProvider"/>
    /// because we would need two different signatures for <see cref="IBaseMediaProvider"/> and <see cref="IChainedMediaProvider"/>,
    /// which is not convenient.
    /// </remarks>
    /// <param name="path">Path to check for a resource.</param>
    /// <returns><c>true</c> if a resource at the given path exists in the <see cref="ParentProvider"/>, else <c>false</c>.</returns>
    bool Exists(string path);

    /// <summary>
    /// Adds a tidy up executor instance whose <see cref="ITidyUpExecutor.Execute"/> method will be called when this
    /// resource accessor is disposed.
    /// </summary>
    /// <param name="tidUpExecutor">Tidy up executor instance to add.</param>
    void AddTidyUpExecutor(ITidyUpExecutor tidUpExecutor);

    /// <summary>
    /// Opens a stream to read this resource.
    /// </summary>
    /// <returns>Stream opened for read operations.</returns>
    /// <exception cref="IllegalCallException">If this resource is not a file (see <see cref="IsFile"/>).</exception>
    Stream OpenRead();

    /// <summary>
    /// Opens a stream to write this resource.
    /// </summary>
    /// <returns>Stream opened for write operations.</returns>
    /// <exception cref="IllegalCallException">If this resource is not a file (see <see cref="IsFile"/>).</exception>
    Stream OpenWrite();
  }
}