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
  /// Registration for all system message queues.
  /// </summary>
  /// <remarks>
  /// This service is thread-safe.
  /// </remarks>
  public interface IMessageBroker
  {
    /// <summary>
    /// Gets the names of all queues registered in this message broker.
    /// </summary>
    /// <returns>List of queue names.</returns>
    ICollection<string> Queues { get;}

    /// <summary>
    /// Registers the specified synchronous message <paramref name="handler"/> at the queue with the specified
    /// <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">Name of the queue to register the handler.</param>
    /// <param name="handler">Message handler that will receive all messages from the specified queue.</param>
    void Register_Sync(string queueName, MessageReceivedHandler handler);

    /// <summary>
    /// Unregisters the specified synchronous message <paramref name="handler"/> at the queue with the specified
    /// <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">Name of the queue to register the handler.</param>
    /// <param name="handler">Message handler that will receive all messages from the specified queue.</param>
    void Unregister_Sync(string queueName, MessageReceivedHandler handler);

    /// <summary>
    /// Registers the specified asynchronous message <paramref name="handler"/> at the queue with the specified
    /// <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">Name of the queue to register the handler.</param>
    /// <param name="handler">Message handler that will receive all messages from the specified queue.</param>
    void Register_Async(string queueName, MessageReceivedHandler handler);

    /// <summary>
    /// Unregisters the specified asynchronous message <paramref name="handler"/> at the queue with the specified
    /// <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">Name of the queue to register the handler.</param>
    /// <param name="handler">Message handler that will receive all messages from the specified queue.</param>
    void Unregister_Async(string queueName, MessageReceivedHandler handler);

    /// <summary>
    /// Sends the specified message in the queue of the specified <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">Name of the queue to be used for sending the message.</param>
    /// <param name="msg">Message to send.</param>
    void Send(string queueName, QueueMessage msg);

    /// <summary>
    /// Shuts the message broker down. No more messages can be delivered after this method was called.
    /// </summary>
    void Shutdown();
  }
}
