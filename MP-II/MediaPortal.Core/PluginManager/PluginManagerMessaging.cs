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

namespace MediaPortal.Core.PluginManager
{
  /// <summary>
  /// This class provides an interface for the messages sent by the <see cref="PluginManager"/>.
  /// This class is part of the plugin manager interface.
  /// </summary>
  public class PluginManagerMessaging
  {
    // Message Queue name
    public const string Queue = "Plugin";

    // Message data
    public const string Notification = "Notification"; // Notification stored as NotificationType

    public enum NotificationType
    {
      /// <summary>
      /// This message will be sent before the plugin manager performs its startup tasks.
      /// </summary>
      Startup,

      /// <summary>
      /// This message will be sent after all plugins were loaded, enabled and auto-started.
      /// </summary>
      PluginsInitialized,

      /// <summary>
      /// This message will be sent before the plugin manager shuts down.
      /// </summary>
      Shutdown
    }
  }
}
