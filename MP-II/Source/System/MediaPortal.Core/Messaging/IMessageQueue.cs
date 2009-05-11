﻿#region Copyright (C) 2007-2008 Team MediaPortal

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

namespace MediaPortal.Core.Messaging
{
  public delegate void MessageReceivedHandler(QueueMessage message);

  /// <summary>
  /// Named message queue to send messages through the system.
  /// </summary>
  /// <remarks>
  /// This service is thread-safe.
  /// </remarks>
  public interface IMessageQueue
  {
    /// <summary>
    /// Delivers all queue messages synchronously.
    /// </summary>
    /// <remarks>
    /// The sender might hold locks on its internal mutexes, so it absolutely necessary to not acquire any
    /// multithreading locks while executing this event. If the callee needs to lock any locks, it MUST do this
    /// asynchronous from this event.
    /// </remarks>
    event MessageReceivedHandler MessageReceived_Sync;

    /// <summary>
    /// Delivers all queue messages asynchronously.
    /// </summary>
    /// <remarks>
    /// In contrast to <see cref="MessageReceived_Sync"/>, the callee can request any mutexes it needs in
    /// this event.
    /// </remarks>
    event MessageReceivedHandler MessageReceived_Async;

    /// <summary>
    /// Gets the information if this queue has subscribers.
    /// </summary>
    bool HasSubscribers { get;}

    /// <summary>
    /// Returns the name of this message queue.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the message filters.
    /// </summary>
    IList<IMessageFilter> Filters { get;}

    /// <summary>
    /// Sends the specified <paramref name="message"/> synchronous, i.e. the method returns after the message
    /// was delivered to all listeners.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void Send(QueueMessage message);

    /// <summary>
    /// Shuts this message queue down. No more messages can be delivered after this method was called.
    /// </summary>
    void Shutdown();
  }
}
