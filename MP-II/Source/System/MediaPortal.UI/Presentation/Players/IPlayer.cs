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
using MediaPortal.Core.MediaManagement;

namespace MediaPortal.UI.Presentation.Players
{
  public enum PlayerState
  {
    /// <summary>
    /// In active state, a player is either playing or seeking or paused or whatever.
    /// To determine the actual state, check the <see cref="IMediaPlaybackControl.PlaybackRate"/> property of the player.
    /// </summary>
    Active,

    /// <summary>
    /// The player was stopped from outside.
    /// </summary>
    Stopped,

    /// <summary>
    /// The media content has ended.
    /// </summary>
    Ended
  }

  /// <summary>
  /// Generic interface for all kinds of players.
  /// Instances, which are passed via this interface, are already prepared to play a media resource.
  /// To get a player for another resource, the player manager has to be called.
  /// Typically, players support sub interfaces of this interface like <see cref="IVideoPlayer"/>.
  /// Players typically will also implement additional additive interfaces as well, like <see cref="ISubtitlePlayer"/>,
  /// <see cref="IMediaPlaybackControl"/>, etc.
  /// </summary>
  /// <remarks>
  /// Different kinds of players have very different kinds of methods and properties.
  /// The player interfaces hierarchy reflects this fact.
  /// Methods and properties, which are common to all players, are introduced by this interface.
  /// </remarks>
  public interface IPlayer
  {
    /// <summary>
    /// Gets the name of the player. This should be something like "Video" for a video player,
    /// "DVD" for a dvd player, "Audio" for an audio player and so on.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Returns the unique id of this player.
    /// </summary>
    Guid PlayerId { get; }

    /// <summary>
    /// Gets the (external) playback state of this player. If the player also supports playback control, finer-grained
    /// playback state can be accessed via the interface <see cref="IMediaPlaybackControl"/>.
    /// </summary>
    PlayerState State { get; }

    /// <summary>
    /// Returns the title of the currently playing media item, or <c>null</c> if the player doesn't know the title.
    /// In this case, the current item's title of the player's playlist can be used.
    /// </summary>
    /// <remarks>
    /// This property is implemented here because the player might tell another name for the media item than the
    /// name/title of the <see cref="MediaItem"/> instance used to instantiate the player. Especially when the player
    /// is a streaming player, only the player knows the current item's title.
    /// </remarks>
    string MediaItemTitle { get; }

    /// <summary>
    /// Notifies this player about the current media item's title, as it is known in the system.
    /// </summary>
    /// <remarks>
    /// The player might or might not use this hint to build its <see cref="MediaItemTitle"/> property.
    /// For example a video player, which doesn't know anything about the title of its current media item,
    /// will use that hint. A TV- or radio-player will typically use extended streaming information about the
    /// media content currently played for the <see cref="MediaItemTitle"/> property and thus will ignore the
    /// given <paramref name="title"/>.
    /// </remarks>
    /// <param name="title">Title of the to-be-played media item as it is known to the system.</param>
    void SetMediaItemTitleHint(string title);

    /// <summary>
    /// Stops playback. The player will set its <see cref="State"/> to <see cref="PlayerState.Stopped"/>.
    /// </summary>
    void Stop();

    // Other playback methods can be found in interface IMediaPlaybackControl
  }
}
