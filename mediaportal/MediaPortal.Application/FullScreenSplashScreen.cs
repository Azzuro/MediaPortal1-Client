#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal
{
  public partial class FullScreenSplashScreen : MPForm
  {
    public FullScreenSplashScreen()
    {
      InitializeComponent();
    }

    public void SetVersion(string version)
    {
      string[] strVersion = version.Split('-');
      lblVersion.Text = strVersion[0];
      Log.Info("Version: Application {0}", strVersion[0]);
      if (strVersion.Length > 1)
      {
        string day = strVersion[2].Substring(0, 2);
        string month = strVersion[2].Substring(3, 2);
        string year = strVersion[2].Substring(6, 4);
        string time = strVersion[3].Substring(0, 5);
        string build = strVersion[4].Substring(0, 13).Trim();
        lblCVS.Text = string.Format("{0} {1} ({2}-{3}-{4} / {5} CET)", strVersion[1], build, year, month, day, time);
        Log.Info("Version: {0}", lblCVS.Text);
      }
      else
      {
        lblCVS.Text = string.Empty;
      }
      Update();
    }

    public void SetInformation(string information)
    {
      lblMain.Text = information;
      Update();
    }

    public void FadeIn()
    {
      while (Opacity <= 0.9)
      {
        Opacity += 0.05;
        Thread.Sleep(15);
      }
      Opacity = 1;
    }

    /// <summary>
    /// Tries to load settings of the splashscreen (color, background image...)
    /// </summary>
    public void RetrieveSplashScreenInfo()
    {
      ReadSplashScreenXML();
      if (pbBackground.Image == null)
      {
        ReadReferenceXML();
      }
    }

    /// <summary>
    /// Tries to read the splashscreen.xml file of the currentskin
    /// </summary>
    private void ReadSplashScreenXML()
    {
      string SkinFilePath = string.Empty;
      SkinFilePath = GUIGraphicsContext.GetThemedSkinFile("\\splashscreen.xml");

      if (!File.Exists(SkinFilePath))
      {
        Log.Debug("FullScreenSplash: Splashscreen.xml not found!: {0}", SkinFilePath);
        return; // if not found... leave
      }

      Log.Debug("FullScreenSplash: Splashscreen.xml found: {0}", SkinFilePath);

      XmlDocument doc = new XmlDocument();
      doc.Load(SkinFilePath);
      XmlNodeList ControlsList = doc.DocumentElement.SelectNodes("/window/controls/control");

      foreach (XmlNode Control in ControlsList)
      {
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "image"
            && Control.SelectSingleNode("id/text()").Value == "1") // if the background image control is found
        {
          string BackgoundImageName = Control.SelectSingleNode("texture/text()").Value;
          string BackgroundImagePath = GUIGraphicsContext.GetThemedSkinFile("\\media\\" + BackgoundImageName);
          if (File.Exists(BackgroundImagePath))
          {
            Log.Debug("FullScreenSplash: Try to load background image value found: {0}", BackgroundImagePath);
            pbBackground.Image = new Bitmap(BackgroundImagePath); // load the image as background
            Log.Debug("FullScreenSplash: background image successfully loaded: {0}", BackgroundImagePath);
          }
          continue;
        }
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "label"
            && Control.SelectSingleNode("id/text()").Value == "2") // if the center label control is found
        {
          if (Control.SelectSingleNode("textsize") != null) // textsize info found?
          {
            float TextSize = float.Parse(Control.SelectSingleNode("textsize/text()").Value);
            Log.Debug("FullScreenSplash: Textsize value found: {0}", TextSize);
            lblMain.Font = new Font(lblMain.Font.FontFamily, TextSize, lblMain.Font.Style);
            Log.Debug("FullScreenSplash: Textsize successfully set: {0}", TextSize);
          }
          if (Control.SelectSingleNode("textcolor") != null) // textcolor info found?
          {
            Color TextColor = ColorTranslator.FromHtml(Control.SelectSingleNode("textcolor/text()").Value);
            Log.Debug("FullScreenSplash: TextColor value found: {0}", TextColor);
            lblMain.ForeColor = TextColor;
            lblVersion.ForeColor = TextColor;
            lblCVS.ForeColor = TextColor;
            Log.Debug("FullScreenSplash: TextColor successfully set: {0}", TextColor);
          }
        }
      }
    }

    /// <summary>
    /// Tries to find the background of the current skin by analysing the reference.xml
    /// </summary>
    private void ReadReferenceXML()
    {
      string SkinReferenceFilePath = string.Empty;
      SkinReferenceFilePath = GUIGraphicsContext.GetThemedSkinFile("\\references.xml");

      Log.Debug("FullScreenSplash: Try to use the reference.xml: {0}", SkinReferenceFilePath);

      XmlDocument doc = new XmlDocument();
      doc.Load(SkinReferenceFilePath);
      XmlNodeList ControlsList = doc.DocumentElement.SelectNodes("/controls/control");

      foreach (XmlNode Control in ControlsList)
      {
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "image")
        {
          string BackgoundImageName = Control.SelectSingleNode("texture/text()").Value;
          string BackgroundImagePath = GUIGraphicsContext.GetThemedSkinFile("\\media\\" + BackgoundImageName);
          if (File.Exists(BackgroundImagePath))
          {
            pbBackground.Image = new Bitmap(BackgroundImagePath); // load the image as background
            Log.Debug("FullScreenSplash: Background image value found: {0}", BackgroundImagePath);
          }
        }
      }
    }

    private void FullScreenSplashScreen_Activated(object sender, EventArgs e)
    {
      //Cursor.Position = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
    }

    private void FullScreenSplashScreen_Load(object sender, EventArgs e)
    {
      using (Settings xmlreader = new MPSettings())
      {
        if ((D3DApp.ScreenNumberOverride != -1) ||
            xmlreader.GetValueAsBool("screenselector", "usescreenselector", false))
          // lets see if the command line option "/screen=" was set or the screen selector is enabled in the config
        {
          int ScreenNumber = 0;
          if (D3DApp.ScreenNumberOverride != -1)
          {
            ScreenNumber = D3DApp.ScreenNumberOverride;
          }
          else
          {
            ScreenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0); // read the screen number
          }

          ScreenNumber++; // increase the number by 1 to respect the DisplayDevice naming convention
          foreach (Screen tmpscreen in Screen.AllScreens)
          {
            if (tmpscreen.DeviceName.Contains("DISPLAY" + ScreenNumber)) // if the selected Display is found
            {
              this.Location = new Point(tmpscreen.Bounds.X, tmpscreen.Bounds.Y);
              // set the form position into this screen
              this.Size = new Size(tmpscreen.Bounds.Width + 1, tmpscreen.Bounds.Height + 1);
            }
          }
        }
        else
        {
          this.Location = new Point(0, 0);
          this.Size = new Size(Screen.FromHandle(this.Handle).Bounds.Width + 1,
                               Screen.FromHandle(this.Handle).Bounds.Height + 1);
        }
      }
      //this.WindowState = FormWindowState.Maximized; // fill the screen
    }
  }
}