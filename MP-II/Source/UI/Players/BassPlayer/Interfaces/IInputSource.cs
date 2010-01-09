#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
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
using MediaPortal.UI.Media.MediaManagement;

namespace Media.Players.BassPlayer
{
  public partial class BassPlayer
  {
    /// <summary>
    /// Provides members to control and read an inputsource.
    /// </summary>
    interface IInputSource : IDisposable
    {
      /// <summary>
      /// Gets the mediaitem object that is processed by the inputsource.
      /// </summary>
      IMediaItem MediaItem { get; }

      /// <summary>
      /// Gets the mediaitem type for the inputsource.
      /// </summary>
      MediaItemType MediaItemType { get; }

      /// <summary>
      /// Gets the output Bass stream.
      /// </summary>
      BassStream OutputStream { get; }
    }
  }
}