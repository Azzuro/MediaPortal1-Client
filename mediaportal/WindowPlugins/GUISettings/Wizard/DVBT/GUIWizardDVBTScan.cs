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
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;

namespace WindowPlugins.GUISettings.Wizard.DVBT
{
  /// <summary>
  /// Summary description for GUIWizardDVBTCountry.
  /// </summary>
  public class GUIWizardDVBTScan : GUIWizardScanBase
  {
    public GUIWizardDVBTScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_dvbt_scan.xml");
    }

    protected override ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      string country = GUIPropertyManager.GetProperty("#WizardCountry");

      ITuning tuning = new DVBTTuning();
      String[] parameters = new String[1];
      parameters[0] = country;
      tuning.AutoTuneTV(captureCard, this, parameters);
      return tuning;
    }
    protected override void OnScanDone()
    {
      GUIPropertyManager.SetProperty("#Wizard.DVBT.Done", "yes");
    }
    protected override NetworkType Network()
    {
      return NetworkType.DVBT;
    }

  }
}
