#region Copyright (C) 2006-2008 Team MediaPortal
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
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Management;
using TvLibrary.Log;

namespace SetupTv
{
  /// <summary>
  /// Offers basic control functions for services
  /// </summary>
  public class ServiceHelper
  {
    /// <summary>
    /// Does a given service exist
    /// </summary>
    /// <param name="serviceToFind"></param>
    /// <returns></returns>
    public static bool IsInstalled(string serviceToFind)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, serviceToFind, true) == 0)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Is the status of TvService == Running
    /// </summary>
    public static bool IsRunning
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            if (service.Status == ServiceControllerStatus.Running) return true;
            return false;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Is the status of TvService == Stopped
    /// </summary>
    public static bool IsStopped
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            if (service.Status == ServiceControllerStatus.Stopped) return true;
            return false;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Stop the TvService
    /// </summary>
    /// <returns></returns>
    public static bool Stop()
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, "TvService", true) == 0)
        {
          if (service.Status == ServiceControllerStatus.Running)
          {
            service.Stop();
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Starts the TvService
    /// </summary>
    /// <returns></returns>
    public static bool Start()
    {
      return Start("TvService");
    }

    public static bool Start(string aServiceName)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, aServiceName, true) == 0)
        {
          if (service.Status == ServiceControllerStatus.Stopped)
          {
            service.Start();
            return true;
          }
          else
            if (service.Status == ServiceControllerStatus.Running)
            {
              return true;
            }
        }
      }
      return false;
    }

    /// <summary>
    /// Start/Stop cycles TvService
    /// </summary>
    /// <returns>Always true</returns>
    public static bool Restart()
    {
      if (!IsInstalled(@"TvService")) return false;

      Stop();
      while (!IsStopped)      
        System.Threading.Thread.Sleep(100);
      
      System.Threading.Thread.Sleep(1000);

      Start();
      while (!IsRunning)
        System.Threading.Thread.Sleep(100);

      return true;
    }

    /// <summary>
    /// Looks up the database service name for tvengine 3
    /// </summary>
    /// <param name="partOfSvcNameToComplete">Supply a (possibly unique) search term to indentify the service</param>
    /// <returns>true when search was successfull - modifies the search pattern to return the correct full name</returns>
    public static bool GetDBServiceName(ref string partOfSvcNameToComplete)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (service.ServiceName.Contains(partOfSvcNameToComplete))
        {
          partOfSvcNameToComplete = service.ServiceName;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks via registry whether a given service is set to autostart on boot
    /// </summary>
    /// <param name="aServiceName">The short name of the service</param>
    /// <param name="aSetEnabled">Enable autostart if needed</param>
    /// <returns>true if the service will start at boot</returns>
    public static bool IsServiceEnabled(string aServiceName, bool aSetEnabled)
    {
      try
      {
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + aServiceName, true))
        {
          int startMode = 3; // manual
          if (rKey != null)
          {
            startMode = (int)rKey.GetValue("Start", (int)3);
            if (startMode == 2) // autostart
              return true;
            else
            {
              if (aSetEnabled)
              {
                rKey.SetValue("Start", (int)2, RegistryValueKind.DWord);
                return true;
              }
              return false;
            }
          }
          return false; // probably wrong service name
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Write dependency info for TvService.exe to registry
    /// </summary>
    /// <param name="dependsOnService">the database service that needs to be started</param>
    /// <returns>true if dependency was added successfully</returns>
    public static bool AddDependencyByName(string dependsOnService)
    {
      try
      {
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TVService", true))
        {
          if (rKey != null)
          {
            rKey.SetValue("DependOnService", new string[] { dependsOnService, "Netman" }, RegistryValueKind.MultiString);
            rKey.SetValue("Start", (int)2, RegistryValueKind.DWord); // Set TVService to autostart
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("ServiceHelper: Failed to access registry {0}", ex.Message);
        return false;
      }
    }

  }
}
