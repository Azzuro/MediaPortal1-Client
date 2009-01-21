#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace WindowPlugins.VideoEditor
{
  public partial class VideoEditorConfiguration : MPConfigForm
  {
    public VideoEditorConfiguration()
    {
      InitializeComponent();
      LoadSettings();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      OpenFileDialog diag = new OpenFileDialog();
      diag.Filter = "exe-File (*.exe)|*.exe";
      try
      {
        diag.InitialDirectory = Path.GetDirectoryName(mencoderPath.Text);
      }
      catch (Exception)
      {
      }

      if (diag.ShowDialog() == DialogResult.OK)
      {
        if (File.Exists(diag.FileName))
        {
          mencoderPath.Text = diag.FileName;
        }
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("VideoEditor", "mencoder", mencoderPath.Text);
      }
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        mencoderPath.Text = xmlreader.GetValueAsString("VideoEditor", "mencoder", String.Empty);
      }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      LoadSettings();
      DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void linkLblMencoderHint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://www.mplayerhq.hu");
      }
      catch (Exception)
      {
      }
    }
  }
}