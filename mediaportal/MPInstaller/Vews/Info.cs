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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class Info : Form
  {
    private MPpackageStruct info_pk;

    public Info(MPpackageStruct pk)
    {
      InitializeComponent();
      info_pk = pk;
    }

    private void Info_Load(object sender, EventArgs e)
    {
      label1.Text = info_pk._intalerStruct.Name;
      label2.Text = info_pk._intalerStruct.Author;
      label3.Text = info_pk._intalerStruct.Version;
      pictureBox1.Image = info_pk._intalerStruct.Logo;
      if (!String.IsNullOrEmpty(info_pk._intalerStruct.Description.Trim()))
        textBox1.Text = info_pk._intalerStruct.Description.Trim();
      else
        textBox1.Visible = false;
      linkLabel1.Text = info_pk._intalerStruct.ProiectProperties.ForumURL;
      linkLabel2.Text = info_pk._intalerStruct.ProiectProperties.WebURL;
      label4.Text = info_pk._intalerStruct.ProiectProperties.CreationDate.ToLongDateString();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}