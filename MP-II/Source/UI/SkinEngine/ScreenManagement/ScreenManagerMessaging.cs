#region Copyright (C) 2007-2008 Team MediaPortal

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

using MediaPortal.Core;
using MediaPortal.Core.Messaging;
using MediaPortal.Presentation.Screens;

namespace MediaPortal.SkinEngine.ScreenManagement
{
  /// <summary>
  /// This class provides an interface for the messages sent by the screen manager.
  /// </summary>
  public class ScreenManagerMessaging
  {
    // Message channel name
    public const string CHANNEL = "ScreenManager";

    /// <summary>
    /// Messages of this type are sent by the <see cref="ScreenManager"/>.
    /// </summary>
    public enum MessageType
    {
      /// <summary>
      /// Internal message to show a screen asynchronously. The screen to be shown will be given in the
      /// parameter <see cref="SCREEN"/>. A bool indicating if open dialogs should be closed will be given in the
      /// parameter <see cref="CLOSE_DIALOGS"/>.
      /// </summary>
      ShowScreen,

      /// <summary>
      /// Internal message to show a dialog asynchronously. The dialog to be shown will be given in the
      /// parameter <see cref="SCREEN"/>.
      /// </summary>
      ShowDialog,

      /// <summary>
      /// Internal message to close a dialog asynchronously. The name of the dialog to close is given in the
      /// parameter <see cref="DIALOG_NAME"/>.
      /// </summary>
      CloseDialog,

      /// <summary>
      /// Internal message to reload the screen and all open dialogs.
      /// </summary>
      ReloadScreens,
    }

    // Message data
    public const string SCREEN = "Screen"; // Type Screen
    public const string CLOSE_DIALOGS = "CloseDialogs"; // Type bool
    public const string DIALOG_NAME = "DialogName"; // Type string
    public const string DIALOG_CLOSE_CALLBACK = "DialogCloseCallback"; // Type DialogCloseCallbackDlgt

    internal static void SendMessageShowScreen(Screen screen, bool closeDialogs)
    {
      QueueMessage msg = new QueueMessage(MessageType.ShowScreen);
      msg.MessageData[SCREEN] = screen;
      msg.MessageData[CLOSE_DIALOGS] = closeDialogs;
      ServiceScope.Get<IMessageBroker>().Send(CHANNEL, msg);
    }

    internal static void SendMessageShowDialog(Screen dialog, DialogCloseCallbackDlgt dialogCloseCallback)
    {
      QueueMessage msg = new QueueMessage(MessageType.ShowDialog);
      msg.MessageData[SCREEN] = dialog;
      msg.MessageData[DIALOG_CLOSE_CALLBACK] = dialogCloseCallback;
      ServiceScope.Get<IMessageBroker>().Send(CHANNEL, msg);
    }

    internal static void SendMessageCloseDialog(string dialogName)
    {
      QueueMessage msg = new QueueMessage(MessageType.CloseDialog);
      msg.MessageData[DIALOG_NAME] = dialogName;
      ServiceScope.Get<IMessageBroker>().Send(CHANNEL, msg);
    }

    internal static void SendMessageReloadScreens()
    {
      QueueMessage msg = new QueueMessage(MessageType.ReloadScreens);
      ServiceScope.Get<IMessageBroker>().Send(CHANNEL, msg);
    }
  }
}
