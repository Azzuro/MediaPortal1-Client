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

using UPnP.Infrastructure.Dv.DeviceTree;

namespace MediaPortal.Backend.ClientCommunication
{
  public class MP2ServerDevice : DvDevice
  {
    public const string DEVICE_TYPE = "schemas-team-mediaportal-com:device:MP-II-Server";
    public const int DEVICE_TYPE_VERSION = 1;

    public MP2ServerDevice(string deviceUuid) : base(DEVICE_TYPE, DEVICE_TYPE_VERSION, deviceUuid,
        new LocalizedUPnPDeviceInformation())
    {
      AddService(new UPnPContentDirectoryService());
      // TODO: Client manager service with method to register the online state of a client - will communicate with the
      //       system's client manager.
      // TODO: Connection manager (is our notion of a connection manager compatible with that of the UPnP standard MediaServer?)
      // TODO: Recording service (dito)
    }
  }
}
