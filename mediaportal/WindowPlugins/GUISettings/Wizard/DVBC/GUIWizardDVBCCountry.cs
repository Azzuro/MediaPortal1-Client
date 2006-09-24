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
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.DVBC
{
  /// <summary>
  /// Summary description for GUIWizardDVBCCountry.
  /// </summary>
  public class GUIWizardDVBCCountry : GUIWindow
  {
    [SkinControlAttribute(24)]
    protected GUIListControl listCountries = null;
    public GUIWizardDVBCCountry()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBC_COUNTRY;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_DVBC_country.xml");
    }
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadCountries();
    }

    void LoadCountries()
    {
      listCountries.Clear();
      XmlDocument doc = new XmlDocument();
      string[] files = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Base, "Tuningparameters"));
      foreach (string file in files)
      {
        if (file.ToLower().IndexOf(".dvbc") >= 0)
        {
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = System.IO.Path.GetFileNameWithoutExtension(file);
          item.Path = file;
          listCountries.Add(item);
        }
      }
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == listCountries)
      {
        GUIListItem item = listCountries.SelectedListItem;
        DoScan(item.Path);
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBC_SCAN);
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    void DoScan(string country)
    {
      GUIPropertyManager.SetProperty("#WizardCountry", country);
    }
  }
}
