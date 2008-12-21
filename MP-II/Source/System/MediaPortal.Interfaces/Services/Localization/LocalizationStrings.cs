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
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Utilities;
using MediaPortal.Utilities.Localization.StringsFile;

namespace MediaPortal.Services.Localization
{
  /// <summary>
  /// Management class for localization resources distributed among different directories.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The localization resources must be available in XML files of the name "strings_[culture name].xml", where
  /// the "culture name" can either only contain the language code (like "en") or the language code plus
  /// region code (like "en-US"), for example "strings_en.xml" or "strings_en-US.xml".<br/>
  /// For a list of valid culture names, see the Microsoft docs in MSDN for class <see cref="CultureInfo"/>.
  /// </para>
  /// <para>
  /// This class maintains a "current" culture state. All strings, whose localization is requested by calling
  /// the appropriate methods, are translated into the "current" language.
  /// </para>
  /// </remarks>
  public class LocalizationStrings
  {
    #region Variables

    readonly Dictionary<string, Dictionary<string, StringLocalised>> _languageStrings =
        new Dictionary<string, Dictionary<string, StringLocalised>>(
            StringComparer.Create(CultureInfo.InvariantCulture, true));
    readonly ICollection<CultureInfo> _availableLanguages =
        new List<CultureInfo>();
    readonly ICollection<string> _languageDirectories = new List<string>();
    CultureInfo _currentLanguage;
    
    #endregion

    #region Constructors/Destructors
    
    /// <summary>
    /// Initializes a new instance of <see cref="LocalizationStrings"/> with the specified culture used
    /// as current culture.
    /// </summary>
    /// <param name="cultureName">Sets the "current" culture to the culture provided in this parameter. The
    /// format of this parameter is either the language code or language code plus region code, for example
    /// "en" or "en-US".</param>
    public LocalizationStrings(string cultureName)
    {
      if (string.IsNullOrEmpty(cultureName))
        cultureName = "en";

      _currentLanguage = CultureInfo.GetCultureInfo(cultureName);
    }

    /// <summary>
    /// Disposes all resources which have been allocated by this instance.
    /// </summary>
    public void Dispose()
    {
      Clear();
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Returns the culture whose language is currently used for translating strings.
    /// </summary>
    public CultureInfo CurrentCulture
    {
      get { return _currentLanguage; }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds a directory containing localization resources to the pool of language directories.
    /// The strings for the current language will be automatically loaded.
    /// </summary>
    /// <param name="directory">Directory which potentially contains localization resources.</param>
    public void AddDirectory(string directory)
    {
      // Add directory to list, to enable reloading/changing language
      _languageDirectories.Add(directory);

      Load(directory);
    }

    /// <summary>
    /// Removes a directory of localization resources from the pool of language directories.
    /// </summary>
    /// <param name="directory">Directory of localization resources which maybe was added before by
    /// <see cref="AddDirectory"/>.</param>
    public void RemoveDirectory(string directory)
    {
      _languageDirectories.Remove(directory);
      ReloadAll();
    }

    /// <summary>
    /// Sets the language to that all strings should be translated to the language of specified
    /// <see cref="culture"/>.
    /// </summary>
    /// <remarks>
    /// This will reload all language files for the new language from all files contained in the pool of
    /// language directories.
    /// </remarks>
    /// <param name="culture">The new culture.</param>
    public void ChangeLanguage(CultureInfo culture)
    {
      if (!_availableLanguages.Contains(culture))
        throw new ArgumentException(string.Format("Language '{0}' is not available", culture.Name));

      _currentLanguage = culture;

      ReloadAll();
    }

    /// <summary>
    /// Returns the localized string specified by its <paramref name="section"/> and <paramref name="name"/>.
    /// </summary>
    /// <param name="section">The section of the string to translate.</param>
    /// <param name="name">The name of the string to translate.</param>
    /// <returns>Translated string or <c>null</c>, if the string isn't available.</returns>
    public string ToString(string section, string name)
    {
      if (_languageStrings.ContainsKey(section) && _languageStrings[section].ContainsKey(name))
        return _languageStrings[section][name].text;

      return null;
    }

    /// <summary>
    /// Returns a collection of cultures for that language resources are available in our language
    /// directories pool.
    /// </summary>
    public ICollection<CultureInfo> AvailableLanguages
    {
      get { return _availableLanguages; }
    }

    /// <summary>
    /// Searches the specified <paramref name="directory"/> for all available language resource files and
    /// collects the available languages.
    /// </summary>
    /// <param name="directory">Directory to look through. Only the given directory will be searched,
    /// the search won't lookup recursively into sub directories.</param>
    /// <returns>Collection of cultures for that language resources are available in the given
    /// <paramref name="directory"/>.</returns>
    public static ICollection<CultureInfo> FindAvailableLanguages(string directory)
    {
      ICollection<CultureInfo> result = new List<CultureInfo>();
      foreach (string filePath in Directory.GetFiles(directory, "strings_*.xml"))
      {
        int pos = filePath.IndexOf('_') + 1;
        string cultName = filePath.Substring(pos, filePath.Length - Path.GetExtension(filePath).Length - pos);

        result.Add(CultureInfo.GetCultureInfo(cultName));
      }
      return result;
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Rebuilds all internal cached information by re-reading all language directories again.
    /// </summary>
    protected void ReloadAll()
    {
      Clear();

      foreach (string directory in _languageDirectories)
        Load(directory);
    }

    /// <summary>
    /// Clears the internal caches of all language resources. After calling this method, this instance is in the
    /// same state as after its creation.
    /// </summary>
    protected void Clear()
    {
      _availableLanguages.Clear();
      if (_languageStrings != null)
        _languageStrings.Clear();
    }

    /// <summary>
    /// Extracts all needed information from the specified language <paramref name="directory"/> and adds
    /// it to the internal cache.
    /// </summary>
    /// <param name="directory">Directory containing language resources to check.</param>
    protected void Load(string directory)
    {
      // Remember all languages found
      CollectionUtils.AddAll(_availableLanguages, FindAvailableLanguages(directory));
      // Add language resouces for current language to internal dictionaries
      TryAddLanguageFile(directory, _currentLanguage);
    }

    /// <summary>
    /// Tries to load all language files for the <paramref name="culture2Load"/> in the specified
    /// <paramref name="directory"/>.
    /// </summary>
    /// <remarks>
    /// The language for a culture can be split up into more than one file: We search the language for
    /// the parent culture (if present), then the more specific region language.
    /// If a language string is already present in the internal dictionary, it will be overwritten by
    /// the new string.
    /// </remarks>
    /// <param name="directory">Directory to load from.</param>
    /// <param name="culture2Load">Culture for that the language resource file will be searched.</param>
    protected void TryAddLanguageFile(string directory, CultureInfo culture2Load)
    {
      if (culture2Load.Parent != CultureInfo.InvariantCulture)
        TryAddLanguageFile(directory, culture2Load.Parent);
      else
        if (culture2Load.Name != "en")
          TryAddLanguageFile(directory, CultureInfo.GetCultureInfo("en"));
      string fileName = string.Format("strings_{0}.xml", culture2Load.Name);
      string filePath = Path.Combine(directory, fileName);

      if (File.Exists(filePath))
      {
        StringFile strings;
        try
        {
          XmlSerializer s = new XmlSerializer(typeof(StringFile));
          Encoding encoding = Encoding.UTF8;
          TextReader r = new StreamReader(filePath, encoding);
          strings = (StringFile) s.Deserialize(r);
        }
        catch (Exception ex)
        {
          ServiceScope.Get<ILogger>().Error("Failed to load language resource file '{0}'", ex, filePath);
          return;
        }

        try
        {
          foreach (StringSection section in strings.sections)
          {
            Dictionary<string, StringLocalised> sectionContents = _languageStrings.ContainsKey(section.name) ?
                _languageStrings[section.name] : new Dictionary<string, StringLocalised>(
                    StringComparer.Create(CultureInfo.InvariantCulture, true));
            foreach (StringLocalised languageString in section.localisedStrings)
              sectionContents[languageString.name] = languageString;
            if (sectionContents.Count > 0)
              _languageStrings[section.name] = sectionContents;
          }
        }
        catch (Exception ex)
        {
          ServiceScope.Get<ILogger>().Error("Failed to load language resource file '{0}'", ex, filePath);
          return;
        }
      }
    }

    #endregion
  }
}
