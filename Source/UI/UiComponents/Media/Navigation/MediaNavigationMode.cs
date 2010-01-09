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

namespace UiComponents.Media.Navigation
{
  /// <summary>
  /// Represents different modes, the media model can look for the user. Basically, each of those modes determines
  /// which screen to show, for example to show media items of a special type or to show a choice of values for a
  /// special filter criterion.
  /// </summary>
  public enum MediaNavigationMode
  {
    LocalMedia,
    MusicShowItems,
    MusicFilterByArtist,
    MusicFilterByAlbum,
    MusicFilterByGenre,
    MusicFilterByDecade,
    MusicSimpleSearch,
    MusicExtendedSearch,
    MoviesShowItems,
    MoviesFilterByActor,
    MoviesFilterByGenre,
    MoviesFilterByYear,
    MoviesSimpleSearch,
    MoviesExtendedSearch,
    PicturesShowItems,
    PicturesFilterByYear,
    PicturesFilterBySize,
    PicturesSimpleSearch,
    PicturesExtendedSearch,
  }
}