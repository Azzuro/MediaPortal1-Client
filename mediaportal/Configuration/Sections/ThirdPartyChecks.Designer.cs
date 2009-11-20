﻿namespace MediaPortal.Configuration.Sections
{
  partial class ThirdPartyChecks
  {
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBoxMCS = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonMCS = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelStatusMCS = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelStatus1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxMCS.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBoxMCS
      // 
      this.mpGroupBoxMCS.Controls.Add(this.mpButtonMCS);
      this.mpGroupBoxMCS.Controls.Add(this.mpLabelStatusMCS);
      this.mpGroupBoxMCS.Controls.Add(this.mpLabelStatus1);
      this.mpGroupBoxMCS.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxMCS.Location = new System.Drawing.Point(12, 11);
      this.mpGroupBoxMCS.Name = "mpGroupBoxMCS";
      this.mpGroupBoxMCS.Size = new System.Drawing.Size(445, 112);
      this.mpGroupBoxMCS.TabIndex = 0;
      this.mpGroupBoxMCS.TabStop = false;
      this.mpGroupBoxMCS.Text = "Microsoft Media Center Services";
      // 
      // mpButtonMCS
      // 
      this.mpButtonMCS.Location = new System.Drawing.Point(102, 69);
      this.mpButtonMCS.Name = "mpButtonMCS";
      this.mpButtonMCS.Size = new System.Drawing.Size(258, 23);
      this.mpButtonMCS.TabIndex = 2;
      this.mpButtonMCS.Text = "Enable policy to prevent services startup";
      this.mpButtonMCS.UseVisualStyleBackColor = true;
      this.mpButtonMCS.Click += new System.EventHandler(this.mpButtonMCS_Click);
      // 
      // mpLabelStatusMCS
      // 
      this.mpLabelStatusMCS.AutoSize = true;
      this.mpLabelStatusMCS.ForeColor = System.Drawing.Color.Red;
      this.mpLabelStatusMCS.Location = new System.Drawing.Point(142, 32);
      this.mpLabelStatusMCS.Name = "mpLabelStatusMCS";
      this.mpLabelStatusMCS.Size = new System.Drawing.Size(45, 13);
      this.mpLabelStatusMCS.TabIndex = 1;
      this.mpLabelStatusMCS.Text = "stopped";
      // 
      // mpLabelStatus1
      // 
      this.mpLabelStatus1.AutoSize = true;
      this.mpLabelStatus1.Location = new System.Drawing.Point(75, 32);
      this.mpLabelStatus1.Name = "mpLabelStatus1";
      this.mpLabelStatus1.Size = new System.Drawing.Size(40, 13);
      this.mpLabelStatus1.TabIndex = 0;
      this.mpLabelStatus1.Text = "Status:";
      // 
      // ThirdPartyChecks
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBoxMCS);
      this.Name = "ThirdPartyChecks";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxMCS.ResumeLayout(false);
      this.mpGroupBoxMCS.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxMCS;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatus1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelStatusMCS;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMCS;
  }
}