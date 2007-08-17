#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.MPInstaller;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : SectionSettings
  {
    private ArrayList loadedPlugins = new ArrayList();
    private ArrayList availablePlugins = new ArrayList();
    private bool isLoaded = false;

    public MPInstallHelper lst = new MPInstallHelper();
    public MPInstallHelper lst_online = new MPInstallHelper();
    private string InstalDir = Config.GetFolder(Config.Dir.Base) + @"\" + "Installer";


    private class ItemTag
    {
      public string DllName;
      public ISetupForm SetupForm;
      public string Type = string.Empty;
      public int WindowId = -1;
      public bool IsProcess = false;
      public bool IsWindow = false;
      public bool IsExternalPlayer = false;
      public bool IsHome = false;
      public bool IsEnabled = false;
      public bool IsPlugins = false;
      public bool ShowDefaultHome = false;
      private Image activeImage = null;
      private Image inactiveImage = null;

      public Image ActiveImage
      {
        get { return activeImage; }
        set { activeImage = value; }
      }

      public Image InactiveImage
      {
        get { return inactiveImage; }
        set { inactiveImage = value; }
      }
    }

    public PluginsNew()
      : this("Plugins")
    {
    }

    public PluginsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadAll();
    }

    private void LoadAll()
    {
      if (!isLoaded)
      {
        isLoaded = true;
        //
        // Enumerate available plugins
        //
        EnumeratePlugins();

        //
        // Load plugins
        //
        loadPlugins();

        //
        // Populate our list
        //
        populateListView();
        LoadSettings();
        LoadListFiles();
        LoadToListview("All");
      }
    }

    private void EnumeratePlugins()
    {
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "subtitle"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "tagreaders"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "externalplayers"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
    }

    private void EnumeratePluginDirectory(string directory)
    {
      if (Directory.Exists(directory))
      {
        //
        // Enumerate files
        //
        string[] files = Directory.GetFiles(directory, "*.dll");

        //
        // Add to list
        //
        foreach (string file in files)
        {
          availablePlugins.Add(file);
        }
      }
    }

    private void populateListView()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        ListViewItem item;
        if (tag.IsProcess)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupProcess"]);
        }
        else if (tag.IsWindow)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupWindow"]);
        }
        else if (tag.IsExternalPlayer)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupExternalPlayers"]);
        }
        else
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupOther"]);
        }
        item.Tag = tag;
        item.ToolTipText = string.Format("{0}", tag.SetupForm.Description());
        listViewPlugins.Items.Add(item);
        updateListViewItem(item);
      }
    }

    private void loadPlugins()
    {
      foreach (string pluginFile in availablePlugins)
      {
        Assembly pluginAssembly = null;
        try
        {
          Log.Debug("PluginsNew: loadPlugins {0}", pluginFile);
          pluginAssembly = Assembly.LoadFrom(pluginFile);
        }
        catch (BadImageFormatException)
        {
          Log.Warn("PluginsNew: {0} has a bad image format", pluginFile);
        }

        if (pluginAssembly != null)
        {
          try
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();

            foreach (Type type in exportedTypes)
            {
              // an abstract class cannot be instanciated
              if (type.IsAbstract)
              {
                continue;
              }
              //
              // Try to locate the interface we're interested in
              //
              if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
              {
                //
                // Create instance of the current type
                //
                object pluginObject;
                try
                {
                  pluginObject = Activator.CreateInstance(type);
                }
                catch (TargetInvocationException)
                {
                  MessageBox.Show(
                    string.Format(
                      "An error occured while loading the plugin {0}.\n\nIt's incompatible with the current MediaPortal version and won't be loaded.",
                      type.FullName), "Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  Log.Info(
                    "Plugin Manager: Plugin {0} is incompatible with the current MediaPortal version! (File: {1})",
                    type.FullName, pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
                  continue;
                }
                ISetupForm pluginForm = pluginObject as ISetupForm;
                IExternalPlayer extPlayer = pluginObject as IExternalPlayer;
                IShowPlugin showPlugin = pluginObject as IShowPlugin;

                if (pluginForm != null)
                {
                  ItemTag tag = new ItemTag();
                  tag.SetupForm = pluginForm;
                  tag.DllName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                  tag.WindowId = pluginForm.GetWindowId();
                  if (extPlayer != null)
                  {
                    tag.IsExternalPlayer = true;
                  }
                  else
                  {
                    tag.IsProcess = true;
                  }
                  if (showPlugin != null)
                  {
                    tag.ShowDefaultHome = showPlugin.ShowDefaultHome();
                  }
                  LoadPluginImages(type, tag);
                  loadedPlugins.Add(tag);
                }
              }
            }
            foreach (Type t in exportedTypes)
            {
              if ((t.IsClass) && (t.IsSubclassOf(typeof(GUIWindow))))
              {
                object newObj;
                
                // an abstract class cannot be instanciated
                if (t.IsAbstract)
                {
                  continue;
                }
                try
                {
                  newObj = Activator.CreateInstance(t);
                }
                catch (TargetInvocationException)
                {
                  MessageBox.Show(
                    string.Format(
                      "An error occured while loading the plugin {0}.\n\nIt's incompatible with the current MediaPortal version and won't be loaded.",
                      t.FullName), "Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  Log.Warn("PluginManager: {0} is incompatible with the current MediaPortal version! (File: {1})",
                           t.FullName, pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
                  continue;
                }
                GUIWindow win = (GUIWindow) newObj;

                foreach (ItemTag tag in loadedPlugins)
                {
                  if (tag.WindowId == win.GetID)
                  {
                    tag.Type = win.GetType().ToString();
                    tag.IsProcess = false;
                    tag.IsWindow = true;
                    break;
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show(
              string.Format(
                "An error occured while loading the plugin file {0}.\n\nIt's broken or incompatible with the current MediaPortal version and won't be loaded.",
                pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1)), "Plugin Manager", MessageBoxButtons.OK,
              MessageBoxIcon.Error);
            Log.Warn(
              "PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!",
              pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
            Log.Info("PluginManager: Exception: {0}", ex);
          }
        }
      }
    }

    /// <summary>
    /// Checks whether the a plugin has a <see cref="PluginIconsAttribute"/> defined.  If it has, the images that are indicated
    /// in the attribute are loaded
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to examine.</param>
    /// <param name="tag">The <see cref="ItemTag"/> to store the images in.</param>
    private static void LoadPluginImages(Type type, ItemTag tag)
    {
      PluginIconsAttribute[] icons = (PluginIconsAttribute[])type.GetCustomAttributes(typeof(PluginIconsAttribute), false);
      if (icons == null || icons.Length == 0)
      {
        //Log.Debug("PluginsNew: no icons");
        return;
      }
      string resourceName = icons[0].ActivatedResourceName;
      if (!string.IsNullOrEmpty(resourceName))
      {
        Log.Debug("PluginsNew: load active image from resource - {0}", resourceName);
        tag.ActiveImage = LoadImageFromResource(type, resourceName);
      }
      resourceName = icons[0].DeactivatedResourceName;
      if (!string.IsNullOrEmpty(resourceName))
      {
        Log.Debug("PluginsNew: load deactivated image from resource - {0}", resourceName);
        tag.InactiveImage = LoadImageFromResource(type, resourceName);
      }
    }

    private static Image LoadImageFromResource(Type type, string resourceName)
    {
      try
      {
        return Image.FromStream(type.Assembly.GetManifestResourceStream(resourceName));
      }
      catch(ArgumentException aex)
      {
        Log.Error("PluginsNew: Argument Exception loading the image - {0}, {1}", resourceName, aex.Message);
        //Thrown when the stream does not seem to contain a valid image
      }
      catch (FileLoadException lex)
      {
        Log.Error("PluginsNew: FileLoad Exception loading the image - {0}, {1}", resourceName, lex.Message);
        //Throw when the resource could not be loaded
      }
      catch (FileNotFoundException fex)
      {
        Log.Error("PluginsNew: FileNotFound Exception loading the image - {0}, {1}", resourceName, fex.Message);
        //Thrown when the resource could not be found
      }
      return null;
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        foreach (ListViewItem item in listViewPlugins.Items)
        {
          ItemTag itemTag = (ItemTag) item.Tag;

          if (itemTag.SetupForm != null)
          {
            if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
            {
              itemTag.IsEnabled =
                xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
            }
            else
            {
              itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
            }

            if (itemTag.IsWindow)
            {
              bool isHome;
              bool isPlugins;
              string dummy;

              if (itemTag.SetupForm.CanEnable() ||
                  itemTag.SetupForm.DefaultEnabled() &&
                  itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
              {
                isHome = itemTag.ShowDefaultHome;
                isPlugins = !isHome;
                itemTag.IsHome = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
                itemTag.IsPlugins = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
              }
            }
          }
          updateListViewItem(item);
        }
      }
    }

    public override void SaveSettings()
    {
      LoadAll();
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        foreach (ListViewItem item in listViewPlugins.Items)
        {
          ItemTag itemTag = (ItemTag) item.Tag;

          bool isEnabled = itemTag.IsEnabled;
          bool isHome = itemTag.IsHome;
          bool isPlugins = itemTag.IsPlugins;

          xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), isEnabled);
          xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DllName, isEnabled);

          if ((isEnabled) && (!isHome && !isPlugins))
          {
            isHome = true;
          }

          if (itemTag.IsWindow)
          {
            xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
            xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
            xmlwriter.SetValueAsBool("pluginswindows", itemTag.Type, isEnabled);
          }
        }
      }
    }

    private void listViewPlugins_DoubleClick(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
      {
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
      }

      updateListViewItem(listViewPlugins.FocusedItem);
    }

    private void updateListViewItem(ListViewItem item)
    {
      ItemTag tag = (ItemTag) item.Tag;
      if (tag.IsEnabled && tag.ActiveImage != null)
      {
        string enabledKey = item.Text+"_enabled";
        if (!item.ImageList.Images.ContainsKey(enabledKey))
        {
          item.ImageList.Images.Add(enabledKey,tag.ActiveImage);
        }
        item.ImageKey = enabledKey;
        return;
      }
      if (!tag.IsEnabled && tag.InactiveImage !=null)
      {
        string disabledKey = item.Text + "_disabled";
        if (!item.ImageList.Images.ContainsKey(disabledKey))
        {
          item.ImageList.Images.Add(disabledKey, tag.InactiveImage);
        }
        item.ImageKey = disabledKey;
        return;
      }
      if (tag.IsWindow)
      {
        if (tag.IsEnabled)
        {
          if (tag.IsHome)
          {
            item.ImageIndex = 8;
          }
          else if (tag.IsPlugins)
          {
            item.ImageIndex = 9;
          }
          else
          {
            item.ImageIndex = 2;
          }
        }
        else
        {
          item.ImageIndex = 3;
        }
      }
      else if (tag.IsProcess)
      {
        if (tag.IsEnabled)
        {
          item.ImageIndex = 4;
        }
        else
        {
          item.ImageIndex = 5;
        }
      }
      else if (tag.IsExternalPlayer)
      {
        if (tag.IsEnabled)
        {
          item.ImageIndex = 6;
        }
        else
        {
          item.ImageIndex = 7;
        }
      }
      else if (tag.IsEnabled)
      {
        item.ImageIndex = 0;
      }
      else
      {
        item.ImageIndex = 1;
      }
    }

    private void itemMyPlugins_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      itemTag.IsPlugins = !itemTag.IsPlugins;
      if (itemTag.IsPlugins)
      {
        itemTag.IsHome = false;
      }
      if (!itemTag.IsPlugins && !itemTag.IsHome)
      {
        itemTag.IsPlugins = true;
      }
      updateListViewItem(listViewPlugins.FocusedItem);
    }

    private void itemMyHome_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      itemTag.IsHome = !itemTag.IsHome;
      if (itemTag.IsHome)
      {
        itemTag.IsPlugins = false;
      }
      if (!itemTag.IsPlugins && !itemTag.IsHome)
      {
        itemTag.IsHome = true;
      }
      updateListViewItem(listViewPlugins.FocusedItem);
    }

    private void itemEnabled_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
      {
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
      }

      updateListViewItem(listViewPlugins.FocusedItem);
    }

    private void itemConfigure_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if ((itemTag.SetupForm != null) &&
          (itemTag.SetupForm.HasSetup()))
      {
        itemTag.SetupForm.ShowPlugin();
      }
    }

    private void listViewPlugins_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        contextMenuStrip.Show(MousePosition);
      }
    }

    private void addContextMenuItem(string name, string menuEntry, Image image, bool clickable)
    {
      ToolStripItem item;
      if (clickable)
      {
        item = contextMenuStrip.Items.Add(string.Empty);
      }
      else
      {
        item = new ToolStripLabel(string.Empty);
        contextMenuStrip.Items.Add(item);
      }
      item.Text = menuEntry;
      item.Image = image;
      item.Name = name;

      switch (item.Name)
      {
        case "Config":
          item.Click += new EventHandler(itemConfigure_Click);
          break;
        case "Enabled":
          item.Click += new EventHandler(itemEnabled_Click);
          break;
        case "My Home":
          item.Click += new EventHandler(itemMyHome_Click);
          break;
        case "My Plugins":
          item.Click += new EventHandler(itemMyPlugins_Click);
          break;
        case "Name":
          item.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
          break;
        case "Author":
          item.Font = new Font("Tahoma", 7.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
          break;
      }
    }

    private void addContextMenuSeparator()
    {
      int item = contextMenuStrip.Items.Add(new ToolStripSeparator());
      contextMenuStrip.Items[item].Name = "Separator";
    }

    private void listViewPlugins_Click(object sender, EventArgs e)
    {
      contextMenuStrip.Items.Clear();

      if (listViewPlugins.FocusedItem != null)
      {
        ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;

        addContextMenuItem("Name", itemTag.SetupForm.PluginName(), null, false);
        addContextMenuItem("Author", string.Format("Author: {0}", itemTag.SetupForm.Author()), null, false);
        addContextMenuSeparator();

        if (!itemTag.IsEnabled)
        {
          addContextMenuItem("Enabled", "Disabled", imageListContextMenu.Images[1], true);
        }
        else
        {
          addContextMenuItem("Enabled", "Enabled", imageListContextMenu.Images[0], true);
          string dummy;
          if (itemTag.SetupForm.CanEnable() && itemTag.IsWindow &&
              itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
          {
            if (!itemTag.IsHome)
            {
              addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[1], true);
            }
            else
            {
              addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[0], true);
            }

            if (!itemTag.IsPlugins)
            {
              addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[1], true);
            }
            else
            {
              addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[0], true);
            }
          }

          if (itemTag.SetupForm.HasSetup())
          {
            addContextMenuSeparator();
            addContextMenuItem("Config", "Configuration", null, true);
          }
        }
      }
    }
 #region MPInstaller stuff
    private void mpButtonInstall_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        this.Hide();
        install_Package(openFileDialog1.FileName);
        this.Show();
        LoadListFiles();
        LoadToListview("All");
      }
      
    }

    private void install_Package(string fil)
    {
      wizard_1 wiz = new wizard_1();
      wiz.package.LoadFromFile(fil);
      if (wiz.package.isValid)
      {
        wiz.starStep();
      }
      else
        MessageBox.Show("Invalid package !");
    }

    private void LoadListFiles()
    {
      lst.LoadFromFile();
      //for (int i = 0; i < lst.lst.Count; i++)
      //{
      //  ((MPpackageStruct)lst.lst[i]).isInstalled = true;
      //  ((MPpackageStruct)lst.lst[i]).isLocal = true;
      //}
      //string temp_file = InstalDir + @"\online.xml";
      //if (File.Exists(temp_file))
      //{
      //  lst_online.LoadFromFile(temp_file);
      //  lst_online.Compare(lst);
      //  lst.AddRange(lst_online);
      //}
    }
    
    public void LoadToListview(string strgroup)
    {
      LoadToListview(lst, mpListView1, strgroup);
    }

    public bool TestView(MPpackageStruct pk, int idx)
    {
      switch (idx)
      {
        case 0:
          return true;
        case 1:
          {
            if (!pk.isNew)
              return true;
            break;
          }
        case 2:
          {
            if (pk.isUpdated)
              return true;
            break;
          }
        case 3:
          {
            if (pk.isNew)
              return true;
            break;
          }
      }
      return false;
    }

    public void LoadToListview(MPInstallHelper mpih, ListView lv, string strgroup)
    {

      lv.Items.Clear();
      for (int i = 0; i < mpih.lst.Count; i++)
      {
        MPpackageStruct pk = (MPpackageStruct)mpih.lst[i];
        if ((pk._intalerStruct.Group == strgroup || strgroup == "All") /*&& TestView(pk, comboBox3.SelectedIndex)*/)
        {
          ListViewItem item1 = new ListViewItem(pk._intalerStruct.Name, mpListView1.Groups["listViewGroup"+pk._intalerStruct.Group]);//listViewGroup listViewPlugins.Groups["listViewGroupProcess"]
          item1.ImageIndex = 0;
          if (pk._intalerStruct.Logo != null)
          {
            imageListMPInstaller.Images.Add(pk._intalerStruct.Logo);
            item1.ImageIndex = imageListMPInstaller.Images.Count - 1;
          }
          if (pk.isNew) item1.ForeColor = Color.Red;
          if (pk.isUpdated) item1.ForeColor = Color.BlueViolet;
          item1.ToolTipText = pk._intalerStruct.Description;
          item1.SubItems.Add(pk._intalerStruct.Author);
          item1.SubItems.Add(pk._intalerStruct.Version);
          item1.SubItems.Add(Path.GetFileName(pk.FileName));
          item1.SubItems.Add(pk._intalerStruct.Group);
          lv.Items.AddRange(new ListViewItem[] { item1 });
        }
        //        InitGroups(lv);
        //        SetGroups(0, lv);
        //        SetButtonState();
      }
    }
    
    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonState();
    }

    private void SetButtonState()
    {
      //contextMenuStrip1.Enabled = false;
      if (mpListView1.SelectedItems.Count > 0)
      {
        MPpackageStruct pk = lst.Find(mpListView1.SelectedItems[0].Text);
        //contextMenuStrip1.Enabled = true;
        mpButtonReinstall.Enabled = true;
        mpButtonUninstall.Enabled = true;
      }
      else
      {
        mpButtonReinstall.Enabled = false;
        mpButtonUninstall.Enabled = false;
      }
    }

    private void mpButtonReinstall_Click(object sender, EventArgs e)
    {
      wizard_1 wiz = new wizard_1();
      MPpackageStruct pk = lst.Find(mpListView1.SelectedItems[0].Text);
      wiz.package.LoadFromFile(InstalDir + @"\" + pk.FileName);
      if (wiz.package.isValid)
      {
        if (wiz.package.containsPlugin)
        {
          MessageBox.Show("This package contain plugin file. \n Use MPInstaller to reistall it !");
        }
        else
        {

          wiz.starStep();
        }
      }
      else
        MessageBox.Show("Invalid package !");
    }

    private void mpButtonUninstall_Click(object sender, EventArgs e)
    {
      wizard_1 wiz = new wizard_1();
      MPpackageStruct pk = lst.Find(mpListView1.SelectedItems[0].Text);
      wiz.package.LoadFromFile(InstalDir + @"\" + pk.FileName);
      if (wiz.package.isValid)
      {
        if (wiz.package.containsPlugin)
        {
          MessageBox.Show("This package contain plugin file. \n Use MPInstaller to unistall it !");
        }
        else
        {

          wiz.uninstall(pk._intalerStruct.Name);
          mpListView1.Items.Clear();
          LoadListFiles();
          LoadToListview("All");
        }
      }
      else
        MessageBox.Show("Invalid package !");
    }

 #endregion

  }
}