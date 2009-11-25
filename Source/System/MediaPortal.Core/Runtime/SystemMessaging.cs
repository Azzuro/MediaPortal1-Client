#region Copyright (C) 2007-2009 Team MediaPortal

/*
    Copyright (C) 2007-2009 Team MediaPortal
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

using MediaPortal.Core.Messaging;

namespace MediaPortal.Core.Runtime
{
  /// <summary>
  /// This class provides an interface for the messages sent by the system.
  /// </summary>
  public class SystemMessaging
  {
    // Message channel name
    public const string CHANNEL = "System";

    // Message type
    public enum MessageType
    {
      /// <summary>
      /// The system state changed to the state given in the PARAM parameter.
      /// </summary>
      SystemStateChanged,

      // TODO: Further events like hibernate, suspend, ...
    }

    // Message data
    public const string PARAM = "Param"; // Parameter depends on the message type, see the docs in MessageType enum

    /// <summary>
    /// Sends a <see cref="MessageType.SystemStateChanged"/> message.
    /// </summary>
    /// <param name="newState">The state the system will switch to.</param>
    public static void SendSystemStateChangeMessage(SystemState newState)
    {
      QueueMessage msg = new QueueMessage(MessageType.SystemStateChanged);
      msg.MessageData[PARAM] = newState;
      IMessageBroker messageBroker = ServiceScope.Get<IMessageBroker>();
      if (messageBroker != null)
        messageBroker.Send(CHANNEL, msg);
    }
  }
}