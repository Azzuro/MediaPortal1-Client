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
using TvLibrary.ChannelLinkage;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;


namespace TvService
{
  public class UserManagement
  {
    ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public UserManagement(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    /// <summary>
    /// Locks the card to the user specified
    /// </summary>
    /// <param name="user">The user.</param>
    public void Lock(User user)
    {
      if (_cardHandler.Card != null)
      {
        TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
        context.Lock(user);
      }
    }

    /// <summary>
    /// Unlocks this card.
    /// </summary>
    /// <param name="user">The user.</param>
    /// 
    public void Unlock(User user)
    {
      if (_cardHandler.Card != null)
      {
        TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
        context.Remove(user);
      }
    }

    /// <summary>
    /// Determines whether the specified user is owner of this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(User user)
    {
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          return RemoteControl.Instance.IsOwner(_cardHandler.DataBaseCard.IdCard, user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
      }
      else
      {
        TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
        return context.IsOwner(user);
      }
    }

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    public void RemoveUser(User user)
    {
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          RemoteControl.Instance.RemoveUserFromOtherCards(_cardHandler.DataBaseCard.IdCard, user);
          return;
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return;
        }
      }
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null) return;
      if (!context.DoesExists(user)) return;

      Log.Info("card: remove user:{0} sub:{1}", user.Name, user.SubChannel);
      context.GetUser(ref user);
      context.Remove(user);
      if (context.ContainsUsersForSubchannel(user.SubChannel) == false)
      {
        Log.Info("card: free subchannel sub:{0}", user.SubChannel);
        _cardHandler.Card.FreeSubChannel(user.SubChannel);
      }

      if (_cardHandler.IsIdle)
      {
        _cardHandler.Card.StopGraph();
      }
    }

		public void HeartBeartUser(User user)
		{
			TvCardContext context = _cardHandler.Card.Context as TvCardContext;
			if (context == null) return;
			if (!context.DoesExists(user)) return;
			
			context.HeartBeatUser(user);
		}

    /// <summary>
    /// Determines whether the card is locked and ifso returns by which user
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified card is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out User user)
    {
      user = null;
      if (_cardHandler.Card != null)
      {
        TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
        context.IsLocked(out user);
        return (user != null);
      }
      return false;
    }

    /// <summary>
    /// Gets the users for this card.
    /// </summary>
    /// <returns></returns>
    public User[] GetUsers()
    {
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          return RemoteControl.Instance.GetUsersForCard(_cardHandler.DataBaseCard.IdCard);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return null;
        }
      }
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null) return null;
      return context.Users;
    }
  }
}
