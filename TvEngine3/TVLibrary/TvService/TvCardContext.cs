/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  /// <summary>
  /// Class which holds the context for a specific card
  /// </summary>
  public class TvCardContext
  {
    #region variables
    List<User> _users;
    User _owner;
    System.Timers.Timer _timer = new System.Timers.Timer();
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardContext"/> class.
    /// </summary>
    public TvCardContext()
    {
      _users = new List<User>();
      _owner = null;
      _timer.Interval = 60000;
      _timer.Enabled = true;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
    }

    #endregion

    #region public methods
    /// <summary>
    /// Locks the card for the user specifies
    /// </summary>
    /// <param name="user">The user.</param>
    public void Lock(User newUser)
    {
      _owner = newUser;
    }

    /// <summary>
    /// Unlocks this card.
    /// </summary>
    public void Unlock()
    {
      _owner = null;
    }

    /// <summary>
    /// Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out User user)
    {
      user = _owner;
      return (user != null);
    }

    /// <summary>
    /// Determines whether the specified user is owner.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(User user)
    {
      if (_owner == null) return true;
      if (_owner.Name == user.Name) return true;

      //exception, always allow everyone to stop the epg grabber
      if (_owner.Name == "epg") return true;
      return false;
    }

    /// <summary>
    /// Sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public User Owner
    {
      get
      {
        return _owner;
      }
      set
      {
        _owner = value;
      }
    }


    /// <summary>
    /// Adds the specified user.
    /// </summary>
    /// <param name="user">The user.</param>
    public void Add(User user)
    {
      Log.Info("user:{0} add", user.Name);
      if (_owner == null) _owner = user;
      for (int i = 0; i < _users.Count; ++i)
      {
        if (_users[i].Name == user.Name)
        {
          _users[i] = (User)user.Clone();
          return;
        }
      }
      _users.Add(user);
    }

    /// <summary>
    /// Removes the specified user.
    /// </summary>
    /// <param name="user">The user.</param>
    public void Remove(User user)
    {
      Log.Info("user:{0} remove", user.Name);
      foreach (User existingUser in _users)
      {
        if (existingUser.Name == user.Name)
        {
          OnStopUser( existingUser);
          _users.Remove(existingUser);
          break;
        }
      }
      if (_owner == null) return;
      if (_owner.Name == user.Name)
        _owner = null;
      if (_users.Count > 0)
      {
        _owner = _users[0];
      }
    }
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <param name="user">The user.</param>
    public void GetUser(ref User user)
    {
      foreach (User existingUser in _users)
      {
        if (existingUser.Name == user.Name)
        {
          user = (User)existingUser.Clone();
          return;
        }
      }
    }
    /// <summary>
    /// Returns if the user exists or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public bool DoesExists(User user)
    {
      foreach (User existingUser in _users)
      {
        if (existingUser.Name == user.Name)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="user">The user.</param>
    public void GetUser(int subChannelId, out User user)
    {
      user = null;
      foreach (User existingUser in _users)
      {
        if (existingUser.SubChannel == subChannelId)
        {
          user = (User)existingUser.Clone();
          return;
        }
      }
    }

    /// <summary>
    /// Gets the users.
    /// </summary>
    /// <value>The users.</value>
    public User[] Users
    {
      get
      {
        return _users.ToArray();
      }
    }

    /// <summary>
    /// Determines whether one or more users exist for the given subchannel
    /// </summary>
    /// <param name="subchannelId">The subchannel id.</param>
    /// <returns>
    /// 	<c>true</c> if users exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsUsersForSubchannel(int subchannelId)
    {
      foreach (User existingUser in _users)
      {
        if (existingUser.SubChannel == subchannelId)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Removes all users
    /// </summary>
    public void Clear()
    {
      foreach (User user in _users)
      {
        OnStopUser( user);
      }
      _users.Clear();
      _owner = null;
    }


    public void OnStopUser( User user)
    {
      History history = user.History as History;
      if (history != null)
      {
        history.Save();
      }
      user.History = null;
    }

    public void OnZap(User user)
    {
      foreach (User existingUser in _users)
      {
        if (existingUser.Name == user.Name)
        {
          Channel channel = Channel.Retrieve(user.IdChannel);
          if (channel != null)
          {
            History history = existingUser.History as History;
            if (history != null)
            {
              history.Save();
            }
            existingUser.History = null;
            TvDatabase.Program p = channel.CurrentProgram;
            if (p != null)
            {
              existingUser.History = new History(channel.IdChannel, p.StartTime, p.EndTime, p.Title, p.Description, p.Genre, false, 0);
            }
          }
        }
      }
    }
    #endregion

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        foreach (User existingUser in _users)
        {
          History history = existingUser.History as History;
          if (history != null)
          {
            Channel channel = Channel.Retrieve(existingUser.IdChannel);
            if (channel != null)
            {
              TvDatabase.Program p = channel.CurrentProgram;
              if (p.StartTime != history.StartTime)
              {
                history.Save();
                existingUser.History = new History(channel.IdChannel, p.StartTime, p.EndTime, p.Title, p.Description, p.Genre, false, 0);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
  }
}
