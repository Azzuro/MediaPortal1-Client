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

using System.Collections.Generic;
using MediaPortal.Core.MediaManagement.MediaProviders;

namespace MediaPortal.Core.MediaManagement
{
  /// <summary>
  /// A metadata extractor is responsible for extracting metadata from a physical media file.
  /// Each metadata extractor creates metadata reports for its defined <see cref="MediaItemAspect"/> instances.
  /// </summary>
  /// <remarks>
  /// The metadata extractor is partitioned in its metadata descriptor part (<see cref="Metadata"/>)
  /// and this worker class.
  /// </remarks>
  public interface IMetadataExtractor
  {
    /// <summary>
    /// Returns the metadata descriptor for this metadata extractor.
    /// </summary>
    MetadataExtractorMetadata Metadata { get; }

    /// <summary>
    /// Worker method to actually try a metadata extraction from the <paramref name="provider"/> and
    /// <paramref name="path"/>.
    /// If this method returns <c>true</c>, the extracted media item aspects were written to the
    /// <paramref name="extractedAspectData"/> collection.
    /// </summary>
    /// <param name="provider">The provider instance to query the physical media with the specified
    /// <paramref name="path"/>.</param>
    /// <param name="path">The path of the physical media file in the specified <paramref "provider"/>
    /// to process.</param>
    /// <param name="extractedAspectData">Collection where this metadata extractor will fill its media item aspects
    /// with the extracted metadata in.</param>
    /// <returns><c>true</c> if the metadata could be extracted from the specified media item, else <c>false</c>.
    /// If the return value is <c>true</c>, the extractedAspectData collection was filled by this metadata extractor.
    /// If the return value is <c>false</c>, the <paramref name="extractedAspectData"/> collection remains
    /// unchanged.</returns>
    bool TryExtractMetadata(IMediaProvider provider, string path,
        ICollection<MediaItemAspect> extractedAspectData);
  }
}
