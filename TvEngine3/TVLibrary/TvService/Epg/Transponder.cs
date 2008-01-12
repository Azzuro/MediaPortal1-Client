/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvService
{
  /// <summary>
  /// Class which holds all channels for a transponder
  public class Transponder
  {
    #region variables
    TuningDetail _detail;
    List<Channel> _channels;
    int _currentChannelIndex;
    bool _inUse;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="Transponder"/> class.
    /// </summary>
    /// <param name="detail">The detail.</param>
    public Transponder(TuningDetail detail)
    {
      _channels = new List<Channel>();
      _detail = detail;
      _currentChannelIndex = -1;
      _inUse = false;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets a value indicating whether the transponder is in use or not
    /// </summary>
    /// <value><c>true</c> if in use; otherwise, <c>false</c>.</value>
    public bool InUse
    {
      get
      {
        return _inUse;
      }
      set
      {
        _inUse = value;
      }
    }

    /// <summary>
    /// Gets or sets the current channel index.
    /// </summary>
    /// <value>The channel index.</value>
    public int Index
    {
      get
      {
        return _currentChannelIndex;
      }
      set
      {
        _currentChannelIndex = value;
      }
    }

    /// <summary>
    /// Gets or sets the channels for this transponder
    /// </summary>
    /// <value>The channels.</value>
    public List<Channel> Channels
    {
      get
      {
        return _channels;
      }
      set
      {
        _channels = value;
      }
    }

    /// <summary>
    /// Gets or sets the tuning details for this transponder.
    /// </summary>
    /// <value>The tuning detail.</value>
    public TuningDetail TuningDetail
    {
      get
      {
        return _detail;
      }
      set
      {
        _detail = value;
      }
    }

    /// <summary>
    /// Gets the tuning detail for the current channel.
    /// </summary>
    /// <value>The tuning detail.</value>
    public IChannel Tuning
    {
      get
      {
        if (Index < 0 || Index >= Channels.Count)
        {
          Log.Error("transponder index out of range:{0}/{1}", Index, Channels.Count);
          return null;
        }
        TvBusinessLayer layer = new TvBusinessLayer();
        return layer.GetTuningChannelByType(Channels[Index],TuningDetail.ChannelType);
      }
    }
    #endregion

      #region public members
      /// <summary>
      /// Called when epg times out, simply sets the lastgrabtime for the current channel
      /// </summary>
      public void OnTimeOut()
      {
        if (Index < 0 || Index >= Channels.Count) return;
        Channels[Index].LastGrabTime = DateTime.Now;
        Channels[Index].Persist();
        Log.Write("EPG: database updated for #{0} {1}", Index, Channels[Index].Name);

      }

      /// <summary>
      /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
      /// </summary>
      /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
      /// <returns>
      /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
      /// </returns>
      public override bool Equals(object obj)
      {
        Transponder other = (Transponder)obj;
        if (other.TuningDetail.ChannelType != TuningDetail.ChannelType) return false;
        if (other.TuningDetail.Frequency != TuningDetail.Frequency) return false;
        if (other.TuningDetail.Modulation != TuningDetail.Modulation) return false;
        if (other.TuningDetail.Symbolrate != TuningDetail.Symbolrate) return false;
        if (other.TuningDetail.Bandwidth != TuningDetail.Bandwidth) return false;
        if (other.TuningDetail.Polarisation != TuningDetail.Polarisation) return false;
        return true;
      }

      /// <summary>
      /// Logs the transponder info to the log file.
      /// </summary>
      public void Dump()
      {
        Log.Write("Transponder:{0} {1} {2} {3} {4} {5}", _currentChannelIndex, TuningDetail.ChannelType, TuningDetail.Frequency, TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
        foreach (Channel c in _channels)
        {
          Log.Write(" {0}", c.Name);
        }
      }

      /// <summary>
      /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
      /// </summary>
      /// <returns>
      /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
      /// </returns>
      public override string ToString()
      {
        return string.Format("type:{0} freq:{1} mod:{2} sr:{3} bw:{4} pol:{5}",
          TuningDetail.ChannelType, TuningDetail.Frequency,
          TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
      }
      #endregion

    }
  }
