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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
	public class MovieExtensions : MediaPortal.Configuration.Sections.FileExtensions
	{
		private System.ComponentModel.IContainer components = null;

		public MovieExtensions() : this("Movie Extensions")
		{
		}

		public MovieExtensions(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				Extensions = xmlreader.GetValueAsString("movies", "extensions", ".avi,.mpg,.ogm,.mpeg,.mkv,.wmv,.ifo,.qt,.rm,.mov,.sbe,.dvr-ms,.ts,.dat,.mp4,.divx");
			}
		}

		public override void SaveSettings()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				//
				// Set language
				//
				xmlwriter.SetValue("movies", "extensions", Extensions);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		public override object GetSetting(string name)
		{
			switch(name.ToLower())
			{
				case "extensions":
					return Extensions;
			}

			return null;
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

