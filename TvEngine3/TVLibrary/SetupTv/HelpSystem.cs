#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;
using TvLibrary.Log;

namespace SetupTv
{
  public static class HelpSystem
  {
    private static readonly string helpReferencesFile = String.Format(@"{0}\HelpReferences.xml", Log.GetPathName());
    private const string helpReferencesURL = @"http://install.team-mediaportal.com/MP1/HelpReferences_TVServer.xml";

    public static void ShowHelp(string sectionName)
    {
      if (!File.Exists(helpReferencesFile))
      {
        MessageBox.Show(
          "No help reference found.\r\nPlease update your help references by pressing 'Update Help' on Project Section.");
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(helpReferencesFile);

      XmlNode generalNode = doc.SelectSingleNode("/helpsystem/general");
      XmlNodeList sectionNodes = doc.SelectNodes("/helpsystem/sections/section");

      if (sectionNodes != null)
      {
        for (int i = 0; i < sectionNodes.Count; i++)
        {
          XmlNode sectionNode = sectionNodes[i];
          if (sectionNode.Attributes["name"].Value == sectionName)
          {
            System.Diagnostics.Process.Start(
              String.Format(@"{0}{1}",
                            generalNode.Attributes["baseurl"].Value,
                            sectionNode.Attributes["suburl"].Value));
            return;
          }
        }
      }

      Log.Error("No help reference found for section: {0}", sectionName);
      MessageBox.Show(
        String.Format(
          "No help reference found for section:\r\n       {0}\r\n\r\nPlease update your help references by pressing 'Update Help' on Project Section.",
          sectionName));
    }

    public static void UpdateHelpReferences()
    {
      string helpReferencesTemp = Path.GetTempFileName();

      Application.DoEvents();
      try
      {
        if (File.Exists(helpReferencesTemp))
          File.Delete(helpReferencesTemp);

        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(helpReferencesURL);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // wr.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        Application.DoEvents();

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream))
            {
              using (TextWriter tout = File.CreateText(helpReferencesTemp))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null)
                    break;
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(helpReferencesFile);
        File.Move(helpReferencesTemp, helpReferencesFile);

        MessageBox.Show("HelpReferences update succeeded.");
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in UpdateHelpReferences | {0}\r\n{1}", ex.Message, ex.Source);
        MessageBox.Show("HelpReferences update failed.");
      }
      Application.DoEvents();
    }
  }
}