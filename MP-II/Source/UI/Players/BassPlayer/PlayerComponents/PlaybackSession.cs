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

using Ui.Players.BassPlayer.Interfaces;

namespace Ui.Players.BassPlayer.PlayerComponents
{
  /// <summary>
  /// Represents a single playbacksession. 
  /// A playback session is a sequence of sources that have the same number of channels and the same samplerate.
  /// Within a playback session, we can perform crossfading and gapless switching.
  /// </summary>
  internal class PlaybackSession
  {
    #region Static members

    /// <summary>
    /// Creates and initializes a new instance.
    /// </summary>
    /// <param name="player">Reference to containing IPlayer object.</param>
    /// <param name="channels">Number of channels in this session.</param>
    /// <param name="sampleRate">Samplerate of this session.</param>
    /// <param name="isPassThrough">Set to <c>true</c> if the session to create is in AC3/DTS passthrough mode.</param>
    /// <returns>The new instance.</returns>
    public static PlaybackSession Create(BassPlayer player, int channels, int sampleRate, bool isPassThrough)
    {
      PlaybackSession playbackSession = new PlaybackSession(player, channels, sampleRate, isPassThrough);
      playbackSession.Initialize();
      return playbackSession;
    }

    #endregion

    #region Fields

    private readonly BassPlayer _Player;
    private readonly int _Channels;
    private readonly int _SampleRate;
    private readonly bool _IsPassThrough;

    #endregion

    #region Public members

    /// <summary>
    /// Gets the number of channels for the session.
    /// </summary>
    public int Channels
    {
      get { return _Channels; }
    }

    /// <summary>
    /// Gets the samplerate for the session.
    /// </summary>
    public int SampleRate
    {
      get { return _SampleRate; }
    }

    /// <summary>
    /// Gets whether the session is in AC3/DTS passthrough mode.
    /// </summary>
    public bool IsPassThrough
    {
      get { return _IsPassThrough; }
    }

    /// <summary>
    /// Determines whether a given inputsource fits in this session or not.
    /// </summary>
    /// <param name="inputSource">The inputsource to check.</param>
    /// <returns><c>true</c>, if the given <paramref name="inputSource"/> matches to this playback session,
    /// else <c>false</c>.</returns>
    public bool MatchesInputSource(IInputSource inputSource)
    {
      return inputSource.OutputStream.Channels == Channels &&
          inputSource.OutputStream.SampleRate == SampleRate &&
          inputSource.OutputStream.IsPassThrough == IsPassThrough;
    }

    /// <summary>
    /// Ends and discards the playback session.
    /// </summary>
    public void End()
    {
      _Player.OutputDeviceManager.StopDevice();
        
      _Player.OutputDeviceManager.ResetInputStream();
      _Player.PlaybackBuffer.ResetInputStream();
      _Player.WinAmpDSPProcessor.ResetInputStream();
      _Player.VSTProcessor.ResetInputStream();
      _Player.UpDownMixer.ResetInputStream();
      _Player.InputSourceSwitcher.Reset();
    }

    #endregion

    #region Private members

    private PlaybackSession(BassPlayer player, int channels, int sampleRate, bool isPassThrough)
    {
      _Player = player;
      _Channels = channels;
      _SampleRate = sampleRate;
      _IsPassThrough = isPassThrough;
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    private void Initialize()
    {
      // In case we are starting a webstream, do a fade-in.
      IInputSource inputSource = _Player.InputSourceQueue.Peek();
      bool fadeIn = (inputSource.MediaItemType == MediaItemType.WebStream);

      _Player.InputSourceSwitcher.InitToInputSource();

      _Player.UpDownMixer.SetInputStream(_Player.InputSourceSwitcher.OutputStream);
      _Player.VSTProcessor.SetInputStream(_Player.UpDownMixer.OutputStream);
      _Player.WinAmpDSPProcessor.SetInputStream(_Player.VSTProcessor.OutputStream);
      _Player.PlaybackBuffer.SetInputStream(_Player.WinAmpDSPProcessor.OutputStream);
      _Player.OutputDeviceManager.SetInputStream(_Player.PlaybackBuffer.OutputStream);
        
      _Player.OutputDeviceManager.StartDevice(fadeIn);
    }

    #endregion
  }
}
