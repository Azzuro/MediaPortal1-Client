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
using System.Threading;
using MediaPortal.Core.Messaging;

namespace MediaPortal.Core.Services.Messaging
{
  /// <summary>
  /// <summary>
  /// Named message queue to send messages through the system.
  /// </summary>
  /// <remarks>
  /// This service is thread-safe.
  /// </remarks>
  public class Queue
  {
    #region Classes

    protected class AsyncMessageSender
    {
      protected Queue<QueueMessage> _asyncMessages = new Queue<QueueMessage>();
      protected volatile bool _terminated = false; // Once terminated, no more messages are sent
      protected Queue _queue;

      public AsyncMessageSender(Queue parent)
      {
        _queue = parent;
      }

      public bool MessagesAvailable
      {
        get
        {
          lock (_queue.SyncObj)
            return _asyncMessages.Count > 0;
        }
      }

      public void EnqueueAsyncMessage(QueueMessage message)
      {
        lock (_queue.SyncObj)
        {
          if (_terminated)
            return;
          _asyncMessages.Enqueue(message);
          Monitor.PulseAll(_queue.SyncObj);
        }
      }

      public QueueMessage Dequeue()
      {
        lock (_queue.SyncObj)
          if (_asyncMessages.Count > 0)
            return _asyncMessages.Dequeue();
          else
            return null;
      }

      /// <summary>
      /// Terminates this sender. Once the sender is terminated, no more messages are delivered.
      /// </summary>
      public void Terminate()
      {
        lock (_queue.SyncObj)
        {
          _terminated = true;
          Monitor.PulseAll(_queue.SyncObj);
        }
      }

      public bool IsTerminated
      {
        get
        {
          lock (_queue.SyncObj)
            return _terminated;
        }
      }

      public void WaitForAsyncExecutions()
      {
        lock (_queue.SyncObj)
        {
          while (true)
          {
            if (_terminated || !MessagesAvailable)
              return;
            Monitor.Wait(_queue.SyncObj);
          }
        }
      }

      public void DoWork()
      {
        while (true)
        {
          QueueMessage message;
          if ((message = Dequeue()) != null)
            _queue.DoSendAsync(message);
          lock (_queue.SyncObj)
          {
            if (_terminated)
              // We have to check this in the synchronized block, else we could miss the PulseAll event
              break;
            else if (!MessagesAvailable)
            // We need to check this here again in a synchronized block. If we wouldn't prevent other threads from
            // enqueuing data in this moment, we could miss the PulseAll event
            {
              Monitor.PulseAll(_queue.SyncObj); // Necessary to awake the waiting threads in method WaitForAsyncExecutions()
              Monitor.Wait(_queue.SyncObj);
            }
          }
        }
        lock (_queue.SyncObj)
          Monitor.PulseAll(_queue.SyncObj); // Necessary to awake the waiting threads in method WaitForAsyncExecutions()
      }
    }

    #endregion

    #region Protected fields

    protected object _syncObj = new object();
    protected Thread _asyncThread = null; // Lazy initialized
    protected string _queueName;
    protected AsyncMessageSender _asyncMessageSender;

    #endregion

    public Queue(string name)
    {
      _queueName = name;
      _asyncMessageSender = new AsyncMessageSender(this);
      InitializeAsyncMessaging();
    }

    protected void InitializeAsyncMessaging()
    {
      lock (_syncObj)
      {
        if (_asyncThread != null)
          return;
        _asyncThread = new Thread(_asyncMessageSender.DoWork);
        _asyncThread.Name = string.Format("Message queue '{0}': Async sender thread", _queueName);
        _asyncThread.Start();
      }
    }

    public object SyncObj
    {
      get { return _syncObj; }
    }

    public void WaitForAsyncExecutions()
    {
      _asyncMessageSender.WaitForAsyncExecutions();
    }

    protected void DoSendAsync(QueueMessage message)
    {
      MessageReceivedHandler asyncHandler = MessageReceived_Async;
      if (asyncHandler != null)
        asyncHandler(message);
    }

    #region IMessageQueue implementation

    /// <summary>
    /// Delivers all queue messages synchronously.
    /// </summary>
    /// <remarks>
    /// The sender might hold locks on its internal mutexes, so it absolutely necessary to not acquire any
    /// multithreading locks while executing this event. If the callee needs to lock any locks, it MUST do this
    /// asynchronous from this event.
    /// </remarks>
    public event MessageReceivedHandler MessageReceived_Sync;

    /// <summary>
    /// Delivers all queue messages asynchronously.
    /// </summary>
    /// <remarks>
    /// In contrast to <see cref="MessageReceived_Sync"/>, the callee can request any mutexes it needs in
    /// this event.
    /// </remarks>
    public event MessageReceivedHandler MessageReceived_Async;

    /// <summary>
    /// Returns the name of this message queue.
    /// </summary>
    public string Name
    {
      get { return _queueName; }
    }

    /// <summary>
    /// Returns the information if this queue is already shut down.
    /// </summary>
    public bool IsShutdown
    {
      get
      {
        lock (_syncObj)
          return _asyncThread == null;
      }
    }

    /// <summary>
    /// Shuts this queue down. No more messages will be delivered.
    /// </summary>
    public void Shutdown()
    {
      _asyncMessageSender.Terminate();
      Thread threadToJoin;
      lock (_syncObj)
        threadToJoin = _asyncThread;
      if (threadToJoin != null)
      {
        threadToJoin.Join(); // Holding the lock while waiting for the thread would cause a deadlock
        lock (_syncObj)
          _asyncThread = null;
      }
    }

    public void Send(QueueMessage message)
    {
      lock (_syncObj)
        if (IsShutdown)
          return;
      message.MessageQueue = _queueName;
      // Send message synchronously...
      MessageReceivedHandler syncHandler = MessageReceived_Sync;
      if (syncHandler != null)
        syncHandler(message);
      // ... and asynchronously
      if (_asyncMessageSender.IsTerminated)
        // If already shut down, discard the message
        return;
      _asyncMessageSender.EnqueueAsyncMessage(message);
    }

    #endregion
  }
}
