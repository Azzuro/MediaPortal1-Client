﻿#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Windows.Forms;
using MediaPortal.LastFM;

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  public partial class LastFMAuthentication : Form
  {
    public LastFMAuthentication()
    {
      InitializeComponent();
    }

    private void btnSubmit_Click(object sender, EventArgs e)
    {
      var userName = txtUserName.Text;
      var password = txtPassword.Text;

      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
      {
        MessageBox.Show("Enter a last.fm Username and password", "Missing username / password", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (!LastFMLibrary.AuthGetMobileSession(userName, password))
      {
        MessageBox.Show("Error adding user.  Please check logs.", "Error adding user", MessageBoxButtons.OK, MessageBoxIcon.Error);
        this.Close();
        return;
      }

      this.Close();

    }
  }
}

