#region Copyright (C) 2014 Team MediaPortal

// Copyright (C) 2014 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// Abstract base class for SoundGraph iMON display.
    /// SoundGraph iMON VFD and LCD are implementing this abstractiong.
    /// </summary>
    public abstract class SoundGraphImon
    {
        public Settings iSettings;

        public SoundGraphImon()
        {
            LoadAdvancedSettings();
            Settings.OnSettingsChanged += AdvancedSettings_OnSettingsChanged;
            Line1 = string.Empty;
            Line2 = string.Empty;
        }

        protected string Line1 { get; set; }
        protected string Line2 { get; set; }
        
        //Set text for give line index
        public abstract void SetLine(int line, string message);
        //Display name is notably used during configuration for testing
        public abstract string Name();
        //Launch advanced settings dialog
        public abstract void Configure();

        //Here comes settings related stuff
        //Settings stuff
        private void LoadAdvancedSettings()
        {
            iSettings = Settings.Load();
        }

        private void AdvancedSettings_OnSettingsChanged()
        {
            SoundGraphDisplay.LogDebug("SoundGraphImon.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS");

            //CleanUp();
            //Thread.Sleep(100);
            LoadAdvancedSettings();
            //Initialize();
        }

 

        [Serializable]
        public class Settings
        {
            //Generic iMON settings
            [XmlAttribute]
            public bool DisableWhenInBackground { get; set; }

            [XmlAttribute]
            public bool DisableWhenIdle { get; set; }

            [XmlAttribute]
            public int DisableWhenIdleDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool ReenableAfter { get; set; }

            [XmlAttribute]
            public int ReenableAfterDelayInSeconds { get; set; }

            //LCD properties
            [XmlAttribute]
            public bool PreferFirstLineGeneral { get; set; }

            [XmlAttribute]
            public bool PreferFirstLinePlayback { get; set; }

            #region Delegates

            public delegate void OnSettingsChangedHandler();

            #endregion

            private static Settings m_Instance;
            public const string m_Filename = "MiniDisplay_SoundGraphImon.xml";

            public static Settings Instance
            {
                get
                {
                    if (m_Instance == null)
                    {
                        m_Instance = Load();
                    }
                    return m_Instance;
                }
                set { m_Instance = value; }
            }

            [XmlAttribute]
            public bool DelayEQ { get; set; }

            [XmlAttribute]
            public int DelayEqTime { get; set; }
 

            [XmlAttribute]
            public int EqMode { get; set; }

            [XmlAttribute]
            public int EqRate { get; set; }

            [XmlAttribute]
            public bool EqDisplay { get; set; }

            [XmlAttribute]
            public bool NormalEQ { get; set; }

            [XmlAttribute]
            public bool StereoEQ { get; set; }

            [XmlAttribute]
            public bool VUmeter { get; set; }

            [XmlAttribute]
            public bool VUmeter2 { get; set; }

            [XmlAttribute]
            public bool VUindicators { get; set; }

            [XmlAttribute]
            public bool RestrictEQ { get; set; }

            [XmlAttribute]
            public bool SmoothEQ { get; set; }


            public static event OnSettingsChangedHandler OnSettingsChanged;

            private static void Default(Settings _settings)
            {
                _settings.EqDisplay = false;
                _settings.NormalEQ = true;
                _settings.StereoEQ = false;
                _settings.VUmeter = false;
                _settings.VUmeter2 = false;
                _settings.VUindicators = false;
                _settings.EqMode = 0;
                _settings.RestrictEQ = false;
                _settings.EqRate = 10;
                _settings.DelayEQ = false;
                _settings.DelayEqTime = 10;
                _settings.SmoothEQ = false;

                //LCD properties
                _settings.PreferFirstLineGeneral = true;
                _settings.PreferFirstLinePlayback = true;

            }

            public static Settings Load()
            {
                Settings settings;
                SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): started");
                if (File.Exists(Config.GetFile(Config.Dir.Config, m_Filename)))
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loading settings from XML file");
                    var serializer = new XmlSerializer(typeof(Settings));
                    var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, m_Filename));
                    settings = (Settings)serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                else
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loading settings from defaults");
                    settings = new Settings();
                    Default(settings);
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loaded settings from defaults");
                }
                SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): completed");
                return settings;
            }

            public static void NotifyDriver()
            {
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }

            public static void Save()
            {
                Save(Instance);
            }

            public static void Save(Settings ToSave)
            {
                var serializer = new XmlSerializer(typeof(Settings));
                var writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, m_Filename),
                                               Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 2 };
                serializer.Serialize(writer, ToSave);
                writer.Close();
            }

            public static void SetDefaults()
            {
                Default(Instance);
            }
        }
    }

}