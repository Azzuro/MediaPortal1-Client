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

// PreTestDialog.cs: Shows progress of pre-test actions.
// Copyright (C) 2005-2006  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 17-9-2005
 * Time: 11:47
 * 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace MPTestTool
{
	/// <summary>
	/// Shows the progress of the current action in a dialog.
	/// </summary>
	public class ProgressDialog : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label actionLabel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label descLabel;
		public ProgressDialog()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
      this.descLabel = new System.Windows.Forms.Label();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.okButton = new System.Windows.Forms.Button();
      this.actionLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // descLabel
      // 
      this.descLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.descLabel.Location = new System.Drawing.Point(16, 8);
      this.descLabel.Name = "descLabel";
      this.descLabel.Size = new System.Drawing.Size(88, 16);
      this.descLabel.TabIndex = 0;
      this.descLabel.Text = "Current action:";
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(16, 56);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(336, 16);
      this.progressBar.TabIndex = 2;
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Enabled = false;
      this.okButton.Location = new System.Drawing.Point(129, 101);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(88, 24);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // actionLabel
      // 
      this.actionLabel.Location = new System.Drawing.Point(16, 32);
      this.actionLabel.Name = "actionLabel";
      this.actionLabel.Size = new System.Drawing.Size(344, 16);
      this.actionLabel.TabIndex = 1;
      // 
      // ProgressDialog
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(368, 137);
      this.ControlBox = false;
      this.Controls.Add(this.descLabel);
      this.Controls.Add(this.actionLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.progressBar);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(378, 168);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(378, 168);
      this.Name = "ProgressDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Progress";
      this.TopMost = true;
      this.ResumeLayout(false);

		}
		#endregion
		
		public void setWindowTitle(string title)
		{
			this.Text = title;
		}
		public void setAction(string action)
		{
			this.actionLabel.Text = action;
		}
		public int getProgress()
		{
			return this.progressBar.Value;
		}
		public Form getForm()
		{
			return this;
		}
		public void setProgress(int value)
		{
			if ((value < 0) || (value > 100))
				return;
			if (value > this.progressBar.Value)
			{
				while (this.progressBar.Value < value)
				{
					this.progressBar.Value++;
				}
			} else if (value < this.progressBar.Value)
			{
				while (this.progressBar.Value > value)
				{
					this.progressBar.Value--;
				}
			}
		}
		public void Done()
		{
			this.progressBar.Value = 100;
			setAction("Done!");
			this.okButton.Enabled = true;
		}
		void OkButtonClick(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
