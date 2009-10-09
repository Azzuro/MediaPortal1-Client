﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;
using MpeCore;
using MpeCore.Interfaces;
using MpeCore.Classes;
using MpeCore.Classes.InstallerType;
using MpeCore.Classes.PathProvider;
using MpeCore.Classes.SectionPanel;
using MpeCore.Classes.ZipProvider;
namespace MpeCore
{
    public static class MpeInstaller
    {
        static public Dictionary<string,IInstallerTypeProvider> InstallerTypeProviders { get; set; }
        static public Dictionary<string,IPathProvider> PathProviders { get; set; }
        static public Dictionary<string, ISectionPanel> SectionPanels { get; set; }
        static public ZipProviderClass ZipProvider { get; set; }
        public static ExtensionCollection InstalledExtensions { get; set; }
        public static ExtensionCollection KnownExtensions { get; set; }

        static public void Init()
        {
            InstallerTypeProviders = new Dictionary<string, IInstallerTypeProvider>();
            PathProviders = new Dictionary<string, IPathProvider>();
            SectionPanels = new Dictionary<string, ISectionPanel>();
            ZipProvider = new ZipProviderClass();

            InstallerTypeProviders.Add("CopyFile", new CopyFile());

            PathProviders.Add("MediaPortalPaths", new MediaPortalPaths());
            PathProviders.Add("WindowsPaths", new WindowsPaths());

            AddSection(new Welcome());
            AddSection(new LicenseAgreement());
            AddSection(new ImageRadioSelector());
            AddSection(new TreeViewSelector());
            AddSection(new InstallSection());

            InstalledExtensions =
                ExtensionCollection.Load(string.Format("{0}\\V2\\InstalledExtensions.xml",
                                                       Config.GetFolder(Config.Dir.Installer)));
            KnownExtensions =
                ExtensionCollection.Load(string.Format("{0}\\V2\\KnownExtensions.xml",
                                                       Config.GetFolder(Config.Dir.Installer)));

        }

        public static void AddSection(ISectionPanel sp)
        {
            SectionPanels.Add(sp.DisplayName, sp);
        }


        public static void Save()
        {
            InstalledExtensions.Save(string.Format("{0}\\V2\\InstalledExtensions.xml", Config.GetFolder(Config.Dir.Installer)));
            KnownExtensions.Save(string.Format("{0}\\V2\\KnownExtensions.xml", Config.GetFolder(Config.Dir.Installer)));

        }

        /// <summary>
        /// Transfor a real path in a template path, based on providers
        /// </summary>
        /// <param name="localFile">The location of file.</param>
        /// <returns></returns>
        static public string TransformInTemplatePath(string localFile)
        {
            foreach (var pathProvider in PathProviders)
            {
                localFile = pathProvider.Value.Colapse(localFile);
            }
            return localFile;
        }
    }
}
