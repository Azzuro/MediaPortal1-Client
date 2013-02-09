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

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  partial class LastFMConfig
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LastFMConfig));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkScrobble = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkAnnounce = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.pbLastFMUser = new System.Windows.Forms.PictureBox();
      this.btnWebAuthenticate = new System.Windows.Forms.Button();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.numRandomness = new System.Windows.Forms.NumericUpDown();
      this.chkAutoDJ = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbLastFMUser)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(13, 13);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(157, 48);
      this.pictureBox1.TabIndex = 4;
      this.pictureBox1.TabStop = false;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.chkScrobble);
      this.mpGroupBox3.Controls.Add(this.chkAnnounce);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(187, 111);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(406, 104);
      this.mpGroupBox3.TabIndex = 8;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Scrobbling";
      // 
      // chkScrobble
      // 
      this.chkScrobble.AutoSize = true;
      this.chkScrobble.Checked = true;
      this.chkScrobble.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkScrobble.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkScrobble.Location = new System.Drawing.Point(20, 58);
      this.chkScrobble.Name = "chkScrobble";
      this.chkScrobble.Size = new System.Drawing.Size(168, 17);
      this.chkScrobble.TabIndex = 1;
      this.chkScrobble.Text = "Scrobble Tracks to user profile";
      this.chkScrobble.UseVisualStyleBackColor = true;
      this.chkScrobble.CheckedChanged += new System.EventHandler(this.chkScrobble_CheckedChanged);
      // 
      // chkAnnounce
      // 
      this.chkAnnounce.AutoSize = true;
      this.chkAnnounce.Checked = true;
      this.chkAnnounce.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAnnounce.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkAnnounce.Location = new System.Drawing.Point(20, 31);
      this.chkAnnounce.Name = "chkAnnounce";
      this.chkAnnounce.Size = new System.Drawing.Size(163, 17);
      this.chkAnnounce.TabIndex = 0;
      this.chkAnnounce.Text = "Announce Tracks on website";
      this.chkAnnounce.UseVisualStyleBackColor = true;
      this.chkAnnounce.CheckedChanged += new System.EventHandler(this.chkAnnounce_CheckedChanged);
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.txtUserName);
      this.mpGroupBox2.Controls.Add(this.pbLastFMUser);
      this.mpGroupBox2.Controls.Add(this.btnWebAuthenticate);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(187, 14);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(407, 88);
      this.mpGroupBox2.TabIndex = 7;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Authentication";
      // 
      // txtUserName
      // 
      this.txtUserName.BorderColor = System.Drawing.Color.Empty;
      this.txtUserName.Location = new System.Drawing.Point(136, 33);
      this.txtUserName.Name = "txtUserName";
      this.txtUserName.Size = new System.Drawing.Size(100, 20);
      this.txtUserName.TabIndex = 7;
      // 
      // pbLastFMUser
      // 
      this.pbLastFMUser.Location = new System.Drawing.Point(20, 19);
      this.pbLastFMUser.Name = "pbLastFMUser";
      this.pbLastFMUser.Size = new System.Drawing.Size(100, 50);
      this.pbLastFMUser.TabIndex = 6;
      this.pbLastFMUser.TabStop = false;
      // 
      // btnWebAuthenticate
      // 
      this.btnWebAuthenticate.Location = new System.Drawing.Point(277, 33);
      this.btnWebAuthenticate.Name = "btnWebAuthenticate";
      this.btnWebAuthenticate.Size = new System.Drawing.Size(99, 23);
      this.btnWebAuthenticate.TabIndex = 5;
      this.btnWebAuthenticate.Text = "Authenticate";
      this.btnWebAuthenticate.UseVisualStyleBackColor = true;
      this.btnWebAuthenticate.Click += new System.EventHandler(this.btnWebAuthenticate_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.numRandomness);
      this.mpGroupBox1.Controls.Add(this.chkAutoDJ);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(13, 92);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(144, 113);
      this.mpGroupBox1.TabIndex = 6;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Auto DJ";
      // 
      // numRandomness
      // 
      this.numRandomness.Location = new System.Drawing.Point(12, 77);
      this.numRandomness.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.numRandomness.Name = "numRandomness";
      this.numRandomness.Size = new System.Drawing.Size(120, 20);
      this.numRandomness.TabIndex = 3;
      this.numRandomness.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.numRandomness.ValueChanged += new System.EventHandler(this.numRandomness_ValueChanged);
      // 
      // chkAutoDJ
      // 
      this.chkAutoDJ.AutoSize = true;
      this.chkAutoDJ.Checked = true;
      this.chkAutoDJ.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAutoDJ.Location = new System.Drawing.Point(12, 30);
      this.chkAutoDJ.Name = "chkAutoDJ";
      this.chkAutoDJ.Size = new System.Drawing.Size(94, 17);
      this.chkAutoDJ.TabIndex = 2;
      this.chkAutoDJ.Text = "Auto DJ Mode";
      this.chkAutoDJ.UseVisualStyleBackColor = true;
      this.chkAutoDJ.CheckedChanged += new System.EventHandler(this.chkAutoDJ_CheckedChanged);
      // 
      // LastFMConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(617, 339);
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.pictureBox1);
      this.Name = "LastFMConfig";
      this.Text = "Scrobbler Configuration";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbLastFMUser)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.CheckBox chkAutoDJ;
    private System.Windows.Forms.NumericUpDown numRandomness;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button btnWebAuthenticate;
    private UserInterface.Controls.MPGroupBox mpGroupBox1;
    private UserInterface.Controls.MPGroupBox mpGroupBox2;
    private UserInterface.Controls.MPTextBox txtUserName;
    private System.Windows.Forms.PictureBox pbLastFMUser;
    private UserInterface.Controls.MPGroupBox mpGroupBox3;
    private UserInterface.Controls.MPCheckBox chkScrobble;
    private UserInterface.Controls.MPCheckBox chkAnnounce;
  }
}