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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Configuration object for the web server
  /// </summary>
  public class Configuration
  {
    #region variables
    private string _name;
    private string _devicePath;
    private int _customQualityValue;
    private int _customPeakQualityValue;
    private QualityType _playbackQualityType;
    private QualityType _recordQualityType;
    private VIDEOENCODER_BITRATE_MODE _playbackQualityMode;
    private VIDEOENCODER_BITRATE_MODE _recordQualityMode;
    private int _cardId;
    #endregion

    #region ctor
    /// <summary>
    /// Simple constructor
    /// </summary>
    private Configuration()
    {
      _playbackQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      _recordQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      _playbackQualityType = QualityType.Default;
      _recordQualityType = QualityType.Default;
      _customQualityValue = 50;
      _customPeakQualityValue = 75;
    }
    #endregion

    #region properties
    public int CustomQualityValue
    {
      get { return _customQualityValue; }
      set { _customQualityValue = value; }
    }

    public int CustomPeakQualityValue
    {
      get { return _customPeakQualityValue; }
      set { _customPeakQualityValue = value; }
    }

    public QualityType PlaybackQualityType
    {
      get { return _playbackQualityType; }
      set { _playbackQualityType = value; }
    }

    public QualityType RecordQualityType
    {
      get { return _recordQualityType; }
      set { _recordQualityType = value; }
    }

    public VIDEOENCODER_BITRATE_MODE PlaybackQualityMode
    {
      get { return _playbackQualityMode; }
      set { _playbackQualityMode = value; }
    }

    public VIDEOENCODER_BITRATE_MODE RecordQualityMode
    {
      get { return _recordQualityMode; }
      set { _recordQualityMode = value; }
    }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public string DevicePath
    {
      get { return _devicePath; }
      set { _devicePath = value; }
    }

    public int CardId
    {
      get { return _cardId; }
      set { _cardId = value; }
    }

    #endregion

    #region Read/Write methods
    /// <summary>
    /// Loads the configuration from a xml file
    /// </summary>
    /// <param name="name">Name of the card</param>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="devicePath">The device path of the card</param>
    /// <returns>Configuration object</returns>
    public static Configuration readConfiguration(int cardId, string name, string devicePath)
    {
      Configuration _configuration = new Configuration();
      String fileName = GetFileName(name, cardId);
      _configuration.Name = name;
      _configuration.DevicePath = devicePath;
      _configuration.CardId = cardId;
      if (File.Exists(fileName))
      {
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(fileName);
          XmlNode cardNode = doc.DocumentElement.SelectSingleNode("/configuration/card");
          _configuration.CardId = int.Parse(cardNode.Attributes["cardId"].Value);
          if (_configuration.CardId != cardId)
          {
            File.Delete(fileName);
            _configuration.CardId = cardId;
            return _configuration;
          }
          _configuration.Name = cardNode.Attributes["name"].Value;
          if (_configuration.Name != name)
          {
            File.Delete(fileName);
            _configuration.Name = name;
            return _configuration;
          }
          XmlNode node = cardNode.SelectSingleNode("device/path");
          _configuration.DevicePath = node.InnerText;
          if (!_configuration.DevicePath.Equals(devicePath))
          {
            File.Delete(fileName);
            _configuration.DevicePath = devicePath;
            return _configuration;
          }

          node = cardNode.SelectSingleNode("qualityControl");
          XmlNode tempNode = node.SelectSingleNode("customSettings");
          _configuration.CustomQualityValue = int.Parse(tempNode.Attributes["value"].Value);
          _configuration.CustomPeakQualityValue = int.Parse(tempNode.Attributes["peakValue"].Value);
          tempNode = node.SelectSingleNode("playback");
          _configuration.PlaybackQualityMode = (VIDEOENCODER_BITRATE_MODE)int.Parse(tempNode.Attributes["mode"].Value);
          _configuration.PlaybackQualityType = (QualityType)int.Parse(tempNode.Attributes["type"].Value);
          tempNode = node.SelectSingleNode("record");
          _configuration.RecordQualityMode = (VIDEOENCODER_BITRATE_MODE)int.Parse(tempNode.Attributes["mode"].Value);
          _configuration.RecordQualityType = (QualityType)int.Parse(tempNode.Attributes["type"].Value);
        } catch (Exception e)
        {
          Log.Log.WriteFile("Error while reading analog card configuration file");
          Log.Log.Write(e);
          _configuration = new Configuration();
          _configuration.Name = name;
          _configuration.DevicePath = devicePath;
        }
      }
      return _configuration;
    }
    /// <summary>
    /// Saves the configuration object in a xml file
    /// </summary>
    /// <param name="configuration">Configuration object to be saved</param>
    public static void writeConfiguration(Configuration configuration)
    {
      String fileName = GetFileName(configuration.Name, configuration.CardId);
      XmlTextWriter writer = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 1;
      writer.IndentChar = (char)9;
      writer.WriteStartDocument(true);
      writer.WriteStartElement("configuration"); //<configuration>
      writer.WriteAttributeString("version", "1");
      writer.WriteStartElement("card"); //<card>
      writer.WriteAttributeString("cardId", XmlConvert.ToString(configuration.CardId));
      writer.WriteAttributeString("name", configuration.Name);
      writer.WriteStartElement("device"); //<device>
      writer.WriteElementString("path", configuration.DevicePath);
      writer.WriteEndElement(); //</device>
      writer.WriteStartElement("qualityControl"); //<qualityControl>
      writer.WriteStartElement("customSettings"); //<customSettings>
      writer.WriteAttributeString("value", XmlConvert.ToString(configuration.CustomQualityValue));
      writer.WriteAttributeString("peakValue", XmlConvert.ToString(configuration.CustomPeakQualityValue));
      writer.WriteEndElement(); //</customSettings>
      writer.WriteStartElement("playback");  //<playback>
      writer.WriteAttributeString("mode", XmlConvert.ToString((int)configuration.PlaybackQualityMode));
      writer.WriteAttributeString("type", XmlConvert.ToString((int)configuration.PlaybackQualityType));
      writer.WriteEndElement(); //</playback>
      writer.WriteStartElement("record"); //<record>
      writer.WriteAttributeString("mode", XmlConvert.ToString((int)configuration.RecordQualityMode));
      writer.WriteAttributeString("type", XmlConvert.ToString((int)configuration.RecordQualityType));
      writer.WriteEndElement(); //</record>
      writer.WriteEndElement(); //</qualityControl>
      writer.WriteEndElement(); //</card>
      writer.WriteEndElement(); //</configuration>
      writer.WriteEndDocument();
      writer.Close();
    }
    #endregion

    #region private helper
    /// <summary>
    /// Generates the file and pathname of the configuration file
    /// </summary>
    /// <param name="name">Name of the card</param>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>Complete filename of the configuration file</returns>
    private static String GetFileName(string name, int cardId)
    {
      String pathName = Log.Log.GetPathName();
      String fileName = String.Format(@"{0}\AnalogCard\Configuration-{1}-{2}.xml", pathName, cardId, name);
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      return fileName;
    }
    #endregion
  }
}
