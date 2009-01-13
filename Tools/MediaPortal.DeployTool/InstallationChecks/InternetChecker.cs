#region Copyright (C) 2005-2008 Team MediaPortal

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
using System.Net;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class InternetChecker
  {
    public static bool CheckConnection()
    {
      Uri Url = new Uri("http://install.team-mediaportal.com/");
      WebRequest WebReq = System.Net.WebRequest.Create(Url);
      WebResponse Resp;
      
      try
      {
        Resp = WebReq.GetResponse();
        Resp.Close();
        WebReq = null;
        return true;
      }
      catch
      {
        WebReq = null;
        return false;
      }
    }
  }
}
