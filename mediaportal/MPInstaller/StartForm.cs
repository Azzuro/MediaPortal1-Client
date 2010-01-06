#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class start_form : MPInstallerForm
  {
    public start_form()
    {
      InitializeComponent();
    }

    //private void button1_Click(object sender, EventArgs e)
    //{
    //  EditForm create_dlg = new EditForm();
    //  this.Hide();
    //  create_dlg.ShowDialog();
    //  this.Show();
    //}

    private void button2_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        this.Hide();
        install_Package(openFileDialog1.FileName);
        this.Show();
      }
    }

    private void install_Package(string fil)
    {
      InstallWizard wiz = new InstallWizard();
      wiz.package.LoadFromFile(fil);
      if (wiz.package.isValid)
      {
        wiz.starStep();
      }
      else
      {
        MessageBox.Show("Invalid package !");
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      ControlPanel cnt = new ControlPanel();
      this.Hide();
      cnt.ShowDialog();
      this.Show();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void button4_Click_1(object sender, EventArgs e)
    {
      this.Hide();
      UpdateChecker checker = new UpdateChecker();
      checker.Check();
      this.Show();
    }

    private void label1_MouseEnter(object sender, EventArgs e)
    {
      ((Label)sender).Font = new Font("Verdana", 9.75F, ((FontStyle)((FontStyle.Bold | FontStyle.Underline))),
                                      GraphicsUnit.Point, ((byte)(0)));
      ((Label)sender).ForeColor = Color.FromArgb(0x879996);
    }

    private void label1_MouseLeave(object sender, EventArgs e)
    {
      ((Label)sender).Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
      ((Label)sender).ForeColor = Color.White;
    }
  }
}