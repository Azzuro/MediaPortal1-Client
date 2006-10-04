#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Skin : SectionSettings
  {
    private string SkinDirectory;
    private string LanguageDirectory;

    private MPGroupBox groupBoxAppearance;
    private MPGroupBox groupBoxSkin;
    private ListView listViewAvailableSkins;
    private ColumnHeader colName;
    private ColumnHeader colVersion;
    private MPGroupBox mpGroupBox1;
    private CheckBox checkBoxlangRTL;
    private MPComboBox languageComboBox;
    private MPLabel label2;
    private CheckBox checkBoxUsePrefix;
    private Panel panelFitImage;
    private PictureBox previewPictureBox;
    private new IContainer components = null;

    public Skin()
      : this("Skin")
    {
    }

    public Skin(string name)
      : base(name)
    {
      SkinDirectory = Config.GetFolder(Config.Dir.Skin);
      LanguageDirectory = Config.GetFolder(Config.Dir.Language);
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      LoadLanguages();
      //
      // Load available skins
      //
      listViewAvailableSkins.Items.Clear();

      if (Directory.Exists(SkinDirectory))
      {
        string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

        foreach (string skinFolder in skinFolders)
        {
          bool isInvalidDirectory = false;
          string[] invalidDirectoryNames = new string[] {"cvs"};

          string directoryName = skinFolder.Substring(SkinDirectory.Length+1);

          if (directoryName != null && directoryName.Length > 0)
          {
            foreach (string invalidDirectory in invalidDirectoryNames)
            {
              if (invalidDirectory.Equals(directoryName.ToLower()))
              {
                isInvalidDirectory = true;
                break;
              }
            }

            if (isInvalidDirectory == false)
            {
              //
              // Check if we have a home.xml located in the directory, if so we consider it as a
              // valid skin directory
              //
              string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if (File.Exists(filename))
              {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNode node = doc.SelectSingleNode("/controls/skin/version");
                ListViewItem item = listViewAvailableSkins.Items.Add(directoryName);
                if (node != null && node.InnerText != null)
                {
                  item.SubItems.Add(node.InnerText);
                }
                else
                {
                  item.SubItems.Add("?");
                }
              }
            }
          }
        }
      }
    }

    private void LoadLanguages()
    {
      // Get system language
      string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
      int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
      string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

      bool bExactLanguageFound = false;
      if (Directory.Exists(LanguageDirectory))
      {
        string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");

        foreach (string folder in folders)
        {
          string fileName = folder.Substring(folder.LastIndexOf(@"\") + 1);

          //
          // Exclude cvs folder
          //
          if (fileName.ToLower() != "cvs")
          {
            if (fileName.Length > 0)
            {
              fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
              languageComboBox.Items.Add(fileName);

              // Check language file to user region language
              if (fileName.ToLower() == strLongLanguage.ToLower())
              {
                languageComboBox.Text = fileName;
                bExactLanguageFound = true;
              }
              else if (!bExactLanguageFound && (fileName.ToLower() == strShortLanguage.ToLower()))
              {
                languageComboBox.Text = fileName;
              }
            }
          }
        }
      }

      if (languageComboBox.Text == "")
      {
        languageComboBox.Text = "English";
      }
    }

    private void listViewAvailableSkins_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewAvailableSkins.SelectedItems.Count == 0)
      {
        previewPictureBox.Image = null;
        previewPictureBox.Visible = false;
        return;
      }
      string currentSkin = listViewAvailableSkins.SelectedItems[0].Text;
      string previewFile = Path.Combine(Path.Combine(SkinDirectory,currentSkin),@"media\preview.png");

      //
      // Clear image
      //
      previewPictureBox.Image = null;
      Image img;

      if (File.Exists(previewFile))
      {
        using(Stream s = new FileStream(previewFile,FileMode.Open,FileAccess.Read))
        {img = Image.FromStream(s);}
        previewPictureBox.Width = img.Width;
        previewPictureBox.Height = img.Height;
        previewPictureBox.Image = img;
        previewPictureBox.Visible = true;
      }
      else
      {
        string logoFile = "mplogo.gif";

        if (File.Exists(logoFile))
        {
          img = Image.FromFile(logoFile);
          previewPictureBox.Width = img.Width;
          previewPictureBox.Height = img.Height;
          previewPictureBox.Image = img;
          previewPictureBox.Visible = true;
        }
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxUsePrefix.Checked = xmlreader.GetValueAsBool("general", "myprefix", true);
        checkBoxlangRTL.Checked = xmlreader.GetValueAsBool("skin", "rtllang", false);
        languageComboBox.Text = xmlreader.GetValueAsString("skin", "language", languageComboBox.Text);
        string currentSkin = xmlreader.GetValueAsString("skin", "name", "BlueTwo");

        //
        // Make sure the skin actually exists before setting it as the current skin
        //
        foreach (ListViewItem item in listViewAvailableSkins.Items)
        {
          if (item.SubItems[0].Text.Equals(currentSkin))
          {
            item.Selected = true;
            break;
          }
        }
      }
    }

    public override void SaveSettings()
    {
      if (listViewAvailableSkins.SelectedItems.Count == 0)
      {
        return;
      }
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string prevSkin = xmlwriter.GetValueAsString("skin", "name", "BlueTwo");
        if (prevSkin != listViewAvailableSkins.SelectedItems[0].Text)
        {
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, listViewAvailableSkins.Text + @"\fonts"), "*");
        }
        xmlwriter.SetValue("skin", "name", listViewAvailableSkins.SelectedItems[0].Text);
        // Set language
        string prevLanguage = xmlwriter.GetValueAsString("skin", "language", "English");
        string skin = xmlwriter.GetValueAsString("skin", "name", "BlueTwo");
        if (prevLanguage != languageComboBox.Text)
        {
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, skin + @"\fonts"), "*");
        }

        xmlwriter.SetValue("skin", "language", languageComboBox.Text);
        xmlwriter.SetValueAsBool("skin", "rtllang", checkBoxlangRTL.Checked);
        xmlwriter.SetValueAsBool("general", "myprefix", checkBoxUsePrefix.Checked);
        xmlwriter.SetValue("general", "skinobsoletecount", 0);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxAppearance = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxUsePrefix = new System.Windows.Forms.CheckBox();
      this.checkBoxlangRTL = new System.Windows.Forms.CheckBox();
      this.languageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxSkin = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.groupBoxAppearance.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.groupBoxSkin.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.previewPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxAppearance
      // 
      this.groupBoxAppearance.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAppearance.Controls.Add(this.mpGroupBox1);
      this.groupBoxAppearance.Controls.Add(this.groupBoxSkin);
      this.groupBoxAppearance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAppearance.Location = new System.Drawing.Point(0, 0);
      this.groupBoxAppearance.Name = "groupBoxAppearance";
      this.groupBoxAppearance.Size = new System.Drawing.Size(472, 408);
      this.groupBoxAppearance.TabIndex = 0;
      this.groupBoxAppearance.TabStop = false;
      this.groupBoxAppearance.Text = "Appearance";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.checkBoxUsePrefix);
      this.mpGroupBox1.Controls.Add(this.checkBoxlangRTL);
      this.mpGroupBox1.Controls.Add(this.languageComboBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 289);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(460, 113);
      this.mpGroupBox1.TabIndex = 4;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Language Settings";
      // 
      // checkBoxUsePrefix
      // 
      this.checkBoxUsePrefix.AutoSize = true;
      this.checkBoxUsePrefix.Location = new System.Drawing.Point(19, 84);
      this.checkBoxUsePrefix.Name = "checkBoxUsePrefix";
      this.checkBoxUsePrefix.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUsePrefix.TabIndex = 3;
      this.checkBoxUsePrefix.Text = "Use string prefixes (e.g. TV = My TV)";
      this.checkBoxUsePrefix.UseVisualStyleBackColor = true;
      // 
      // checkBoxlangRTL
      // 
      this.checkBoxlangRTL.AutoSize = true;
      this.checkBoxlangRTL.Location = new System.Drawing.Point(19, 58);
      this.checkBoxlangRTL.Name = "checkBoxlangRTL";
      this.checkBoxlangRTL.Size = new System.Drawing.Size(241, 17);
      this.checkBoxlangRTL.TabIndex = 2;
      this.checkBoxlangRTL.Text = "Language contains right to left direction chars";
      this.checkBoxlangRTL.UseVisualStyleBackColor = true;
      // 
      // languageComboBox
      // 
      this.languageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.languageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.languageComboBox.Location = new System.Drawing.Point(118, 21);
      this.languageComboBox.Name = "languageComboBox";
      this.languageComboBox.Size = new System.Drawing.Size(325, 21);
      this.languageComboBox.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 16);
      this.label2.TabIndex = 0;
      this.label2.Text = "Display language:";
      // 
      // groupBoxSkin
      // 
      this.groupBoxSkin.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkin.Controls.Add(this.panelFitImage);
      this.groupBoxSkin.Controls.Add(this.listViewAvailableSkins);
      this.groupBoxSkin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSkin.Location = new System.Drawing.Point(6, 23);
      this.groupBoxSkin.Name = "groupBoxSkin";
      this.groupBoxSkin.Size = new System.Drawing.Size(460, 236);
      this.groupBoxSkin.TabIndex = 3;
      this.groupBoxSkin.TabStop = false;
      this.groupBoxSkin.Text = "Skin";
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                     {
                                                       this.colName,
                                                       this.colVersion
                                                     });
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(15, 22);
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(200, 200);
      this.listViewAvailableSkins.TabIndex = 3;
      this.listViewAvailableSkins.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged +=
        new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 140;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 56;
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(243, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(200, 200);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.MinimumSize = new System.Drawing.Size(200, 200);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(200, 200);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // Skin
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBoxAppearance);
      this.Name = "Skin";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxAppearance.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.groupBoxSkin.ResumeLayout(false);
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) (this.previewPictureBox)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion
  }
}