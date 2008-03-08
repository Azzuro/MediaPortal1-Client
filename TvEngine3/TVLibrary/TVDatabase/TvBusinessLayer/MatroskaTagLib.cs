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

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace TvDatabase
{
  public class MatroskaTagInfo
  {
    public string title;
    public string description;
    public string genre;
    public string channelName;
  }

  /// <summary>
  /// Contains basic read and write methods to handle Matroska tags for recordings
  /// </summary>
  public class MatroskaTagHandler
  {
    #region Private members
    private static XmlNode AddSimpleTag(string tagName, string value, XmlDocument doc)
    {
      XmlNode rootNode = doc.CreateElement("SimpleTag");
      XmlNode nameNode = doc.CreateElement("name");
      nameNode.InnerText = tagName;
      XmlNode valueNode = doc.CreateElement("value");
      valueNode.InnerText = value;
      rootNode.AppendChild(nameNode);
      rootNode.AppendChild(valueNode);
      return rootNode;
    }
    #endregion

    #region Public members

    /// <summary>
    /// Searches a given path and its subdirectories for XML files and loads them into corresponding Matroska tags
    /// </summary>
    /// <param name="aDirectory">The parent folder (of recordings)</param>
    /// <returns>A dictionary which Key is the full filename of the XML file</returns>
    public static Dictionary<string, MatroskaTagInfo> GetAllMatroskaTags(string aDirectory)
    {
      Dictionary<string, MatroskaTagInfo> fileRecordings = new Dictionary<string, MatroskaTagInfo>();
      try
      {
        string[] importFiles = Directory.GetFiles(aDirectory, "*.xml", SearchOption.AllDirectories);

        foreach (string recordingXml in importFiles)
        {
          try
          {
            MatroskaTagInfo importTag = MatroskaTagHandler.ReadTag(recordingXml);
            fileRecordings[recordingXml] = importTag;
          }
          catch (Exception)
          {
          }
        }
      }
      catch (Exception)
      {
      }
      return fileRecordings;
    }

    /// <summary>
    /// Reads Matroska tag files
    /// </summary>
    /// <param name="filename">Path to an XML file containing recording infos</param>
    /// <returns>The Matroska tag object</returns>
    public static MatroskaTagInfo ReadTag(string filename)
    {
      if (!File.Exists(filename))
        return null;
      MatroskaTagInfo info = new MatroskaTagInfo();
      XmlDocument doc = new XmlDocument();
      doc.Load(filename);
      XmlNodeList simpleTags = doc.SelectNodes("/tags/tag/SimpleTag");
      foreach (XmlNode simpleTag in simpleTags)
      {
        string tagName = simpleTag.ChildNodes[0].InnerText;
        switch (tagName)
        {
          case "TITLE":
            info.title = simpleTag.ChildNodes[1].InnerText;
            break;
          case "COMMENT":
            info.description = simpleTag.ChildNodes[1].InnerText;
            break;
          case "GENRE":
            info.genre = simpleTag.ChildNodes[1].InnerText;
            break;
          case "CHANNEL_NAME":
            info.channelName = simpleTag.ChildNodes[1].InnerText;
            break;
        }
      }
      return info;
    }

    /// <summary>
    /// Saves the given MatroskaTagInfo into an XML file
    /// </summary>
    /// <param name="filename">Filename of the XML file</param>
    /// <param name="taginfo">the recording information the xml file should contain</param>
    public static void WriteTag(string filename, MatroskaTagInfo taginfo)
    {
      if (!Directory.Exists(Path.GetDirectoryName(filename)))
        Directory.CreateDirectory(Path.GetDirectoryName(filename));
      XmlDocument doc = new XmlDocument();
      XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      XmlNode tagsNode = doc.CreateElement("tags");
      XmlNode tagNode = doc.CreateElement("tag");
      tagNode.AppendChild(AddSimpleTag("TITLE", taginfo.title, doc));
      tagNode.AppendChild(AddSimpleTag("COMMENT", taginfo.description, doc));
      tagNode.AppendChild(AddSimpleTag("GENRE", taginfo.genre, doc));
      tagNode.AppendChild(AddSimpleTag("CHANNEL_NAME", taginfo.channelName, doc));
      tagsNode.AppendChild(tagNode);
      doc.AppendChild(tagsNode);
      doc.InsertBefore(xmldecl, tagsNode);
      doc.Save(filename);
    }
    #endregion
  }
}
