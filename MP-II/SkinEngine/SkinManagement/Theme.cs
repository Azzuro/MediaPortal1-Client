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

using System;
using System.IO;
using System.Xml;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Utilities;

namespace MediaPortal.SkinEngine.SkinManagement
{
  /// <summary>
  /// Holds resource files for a theme. Typically, a theme consists of
  /// meta information (like theme name, author, ...) and some style resource files,
  /// but a theme may override any file from its skin or from the default theme or
  /// skin.
  /// </summary>
  /// <remarks>
  /// The meta information will be read from a file <i>theme.xml</i> located in one of the
  /// theme resource directories.
  /// </remarks>
  public class Theme: SkinResources
  {
    public const string THEME_META_FILE = "theme.xml";

    public const int MIN_THEME_DESCRIPTOR_VERSION_HIGH = 1;
    public const int MIN_THEME_DESCRIPTOR_VERSION_LOW = 0;

    // Meta information of the theme
    protected bool _metadataInitialized = false;
    protected string _author = null;
    protected string _description = null;
    protected string _previewResourceKey = null;
    protected string _specVersion = null;
    protected string _themeVersion = null;
    protected string _skinEngineVersion = null;
    protected int _minColorDepth = -1;

    public Theme(string name, Skin parentSkin): base(name, parentSkin)
    { }

    /// <summary>
    /// Returns the information if the resources of this skin are complete
    /// (i.e. if the theme meta file could be read).
    /// </summary>
    public override bool IsValid
    {
      get
      {
        CheckMetadataInitialized();
        return _metadataInitialized;
      }
    }

    /// <summary>
    /// Returns the <see cref="Skin"/> this theme belongs to.
    /// </summary>
    public Skin ParentSkin
    {
      get { return InheritedSkinResources as Skin; }
    }

    /// <summary>
    /// Will trigger the lazy metadata initialization on request.
    /// </summary>
    protected void CheckMetadataInitialized()
    {
      if (_metadataInitialized)
        return;
      FileInfo metaFile = GetResourceFile(THEME_META_FILE);
      _metadataInitialized = LoadMetadata(metaFile);
    }

    protected bool LoadMetadata(FileInfo metaFile)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        using (FileStream fs = metaFile.OpenRead())
          doc.Load(fs);
        XmlElement themeElement = doc.DocumentElement;
        if (themeElement.Name != "Theme")
          throw new ArgumentException("File is no theme descriptor (needs to contain a 'Theme' element)");

        bool versionOk = false;
        foreach (XmlAttribute attr in themeElement.Attributes)
        {
          switch (attr.Name)
          {
            case "Version":
              StringUtils.CheckVersionEG(attr.Value, MIN_THEME_DESCRIPTOR_VERSION_HIGH, MIN_THEME_DESCRIPTOR_VERSION_LOW);
              _specVersion = attr.Value;
              versionOk = true;
              break;
            case "Name":
              if (_name != null && _name != attr.Value)
                throw new ArgumentException("Theme name '" + _name + "' doesn't correspond to specified name '" + attr.Value + "'");
              else
                _name = attr.Value;
              break;
            default:
              throw new ArgumentException("Attribute '" + attr.Name + "' is unknown");
          }
        }
        if (!versionOk)
          throw new ArgumentException("Attribute 'Version' expected");

        foreach (XmlNode child in themeElement.ChildNodes)
        {
          switch (child.Name)
          {
            case "ShortDescription":
              _description = child.InnerText;
              break;
            case "Preview":
              _previewResourceKey = child.InnerText;
              break;
            case "Author":
              _author = child.InnerText;
              break;
            case "ThemeVersion":
              _themeVersion = child.InnerText;
              break;
            case "SkinEngine":
              _skinEngineVersion = child.InnerText;
              break;
            case "MinColorDepth":
              _minColorDepth = Int32.Parse(child.InnerText);
              break;
            default:
              throw new ArgumentException("Child element '" + child.Name + "' is unknown");
          }
        }
      }
      catch (Exception e)
      {
        ServiceScope.Get<ILogger>().Error("Error parsing theme descriptor '" + metaFile.FullName + "'", e);
        return false;
      }
      return true;
    }
  }
}
