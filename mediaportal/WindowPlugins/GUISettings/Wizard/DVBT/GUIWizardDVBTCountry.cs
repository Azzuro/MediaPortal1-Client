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
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace WindowPlugins.GUISettings.Wizard.DVBT
{
	/// <summary>
	/// Summary description for GUIWizardDVBTCountry.
	/// </summary>
	public class GUIWizardDVBTCountry : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		public GUIWizardDVBTCountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBT_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbt_country.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadCountries();
		}

		void LoadCountries()
		{
			listCountries.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(Config.GetFile(Config.Dir.Base, @"Tuningparameters\dvbt.xml"));
            XPathNavigator nav = doc.CreateNavigator();

            // Ensure we are at the root node
            nav.MoveToRoot();
            XPathExpression expr = nav.Compile("/dvbt/country");
            // Add an XSLT based sort
            expr.AddSort("@name", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Text);
            IEnumerator enumerator = nav.Select(expr).GetEnumerator();
            while (enumerator.MoveNext())
            {
                XPathNavigator nodeCountry = (XPathNavigator)enumerator.Current;
                XPathNavigator nameNode = nodeCountry.SelectSingleNode("@name");
                string name = nameNode.Value;
                GUIListItem item = new GUIListItem();
                item.IsFolder = false;
                item.Label = name;
                listCountries.Add(item);
            }
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item.Label);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN);
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string country)
		{
			GUIPropertyManager.SetProperty("#WizardCountry",country);
		}
	}
}
