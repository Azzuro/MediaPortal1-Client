/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Windows.Forms;
using MediaPortal.GUI.Library;


namespace MediaPortal.HCWBlaster
{
	/// <summary>
	/// Summary description for HCWBlaster.
	/// </summary>
	public class HCWBlaster: IPlugin, ISetupForm
	{
		private static int    _MPWindowID  = 9090;
		private static string _Description = "Hauppauge IR Blaster Plugin";
		private static string _Author      = "unknown";
		private static string _PluginName  = "HCW IR Blaster";
		private static bool   _CanEnable   = true;
		private static bool   _DefEnabled  = false;
		private static bool   _HasSetup    = true;


		private const  string _version     = "0.1";
		private        bool   _ExLogging   = false;

		private HCWIRBlaster irblaster;
		
		#region MPInteraction

		public HCWBlaster()
		{
			//
			// TODO: Add constructor logic here
			//
			//irblaster = new HCWIRBlaster();
		}

		public void Start()
		{
			Log.Write("HCWBlaster: HCWBlaster {0} plugin starting.", _version);

			LoadSettings();
			
			if (_ExLogging == true) 
                Log.Write("HCWBlaster: Extended Logging is Enabled.");

			if (_ExLogging == false) 
				Log.Write("HCWBlaster: Extended Logging is NOT Enabled.");

			if (_ExLogging == true)
				Log.Write("HCWBlaster: Creating IRBlaster Object.");

			irblaster = new HCWIRBlaster();

			Log.Write("HCWBlaster: Adding message handler for HCWBlaster {0}.", _version);

			GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
			return;
		}

		public void Stop()
		{
			Log.Write("HCWBlaster: HCWBlaster {0} plugin stopping.", _version);
			return;
		}

		private void LoadSettings()
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				_ExLogging = xmlreader.GetValueAsBool("HCWBlaster", "ExtendedLogging", false);
			}
		}

		private void OnThreadMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL : 
					bool bIsInteger;
					double retNum;
					bIsInteger = Double.TryParse(message.Label, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
					this.ChangeTunerChannel( message.Label );
					break;
			}
		}

		public void ChangeTunerChannel(string channel_data) 
		{
			if (_ExLogging == true)
				Log.Write("HCWBlaster: Calling IR Blaster Code for: {0}", channel_data );
			irblaster.blast(channel_data, _ExLogging);
		}



		#endregion





		#region ISetupForm Members
		
		// Returns the name of the plugin which is shown in the plugin menu
		public string PluginName()
		{
			return _PluginName.ToString();
		}

		// Returns the description of the plugin is shown in the plugin menu
		public string Description()
		{
			return _Description.ToString();
		}

		// Returns the author of the plugin which is shown in the plugin menu
		public string Author()
		{
			return _Author.ToString();
		}

		// Indicates whether plugin can be enabled/disabled
		public bool CanEnable()
		{
			return _CanEnable;
		}

		// get ID of windowplugin belonging to this setup
		public int GetWindowId()
		{
			return _MPWindowID;
		}

		// Indicates if plugin is enabled by default;
		public bool DefaultEnabled()
		{
			return _DefEnabled;
		}

		// indicates if a plugin has its own setup screen
		public bool HasSetup()
		{
			return _HasSetup;
		}

		// show the setup dialog
		public void   ShowPlugin()
		{
			//MessageBox.Show("Nothing to configure, this is just an example");
			Form setup = new HCWBlasterSetupForm();
			setup.ShowDialog();
		}

		/// <summary>
		/// If the plugin should have its own button on the main menu of Media Portal then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool   GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText       = String.Empty;
			strButtonImage      = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage     = String.Empty;
			return false;
		}

		#endregion


	}
}
