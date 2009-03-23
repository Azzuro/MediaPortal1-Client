/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;


namespace TvService
{
  public class UserManagement
  {
    readonly ITvCardHandler _cardHandler;

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
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
      }
      TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
      return context.IsOwner(user);
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
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return;
        }
      }
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return;
      if (!context.DoesExists(user))
        return;

      int subChannelId;
      context.GetUser(ref user, out subChannelId);
      if (subChannelId != -1)
      {
        Log.Info("card: remove user:{0} sub:{1}", user.Name, subChannelId);
        context.Remove(user);
        if (!context.ContainsUsersForSubchannel(subChannelId))
        {
          //only remove subchannel if it exists.
          if (_cardHandler.Card.GetSubChannel(subChannelId) != null)
          {
            // Before we remove the subchannel we have to stop it
            ITvSubChannel subChannel = _cardHandler.Card.GetSubChannel(subChannelId);
            if (subChannel.IsTimeShifting)
            {
              subChannel.StopTimeShifting();
            }
            else if (subChannel.IsRecording)
            {
              subChannel.StopRecording();
            }
            Log.Info("card: free subchannel sub:{0}", subChannelId);
            _cardHandler.Card.FreeSubChannel(subChannelId);
            CleanTimeShiftFiles(_cardHandler.DataBaseCard.TimeShiftFolder, String.Format("live{0}-{1}.ts", _cardHandler.DataBaseCard.IdCard, subChannelId));
          }
        }
      }
      if (_cardHandler.IsIdle)
      {
        _cardHandler.Card.StopGraph();
      }
    }

    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    static void CleanTimeShiftFiles(string folder, string fileName)
    {
      try
      {
        Log.Write(@"card: delete timeshift files {0}\{1}", folder, fileName);
        string[] files = Directory.GetFiles(folder);
        for (int i = 0; i < files.Length; ++i)
        {
          if (files[i].IndexOf(fileName) >= 0)
          {
            try
            {
              Log.Write("card:   delete {0}", files[i]);
              File.Delete(files[i]);
            } catch (Exception)
            {
              Log.Error("card: Error on delete in CleanTimeshiftFiles");
            }
          }
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public void HeartBeartUser(User user)
    {
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return;
      if (!context.DoesExists(user))
        return;

      context.HeartBeatUser(user);
    }

    public TvStoppedReason GetTvStoppedReason(User user)
    {
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return TvStoppedReason.UnknownReason;

      return context.GetTimeshiftStoppedReason(user);
    }

    public void SetTvStoppedReason(User user, TvStoppedReason reason)
    {
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return;

      context.SetTimeshiftStoppedReason(user, reason);
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
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return null;
        }
      }
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return null;
      return context.Users;
    }
  }
}
