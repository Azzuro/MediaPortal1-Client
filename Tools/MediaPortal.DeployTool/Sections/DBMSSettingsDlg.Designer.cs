namespace MediaPortal.DeployTool
{
  partial class DBMSSettingsDlg
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.labelHeading = new System.Windows.Forms.Label();
      this.labelInstDir = new System.Windows.Forms.Label();
      this.textBoxDir = new System.Windows.Forms.TextBox();
      this.textBoxPassword = new System.Windows.Forms.TextBox();
      this.labelPassword = new System.Windows.Forms.Label();
      this.labelPasswordDesc = new System.Windows.Forms.Label();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.checkBoxFirewall = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Location = new System.Drawing.Point(4, 30);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(296, 13);
      this.labelHeading.TabIndex = 2;
      this.labelHeading.Text = "Please set the needed options for the SQL-Server installation:";
      // 
      // labelInstDir
      // 
      this.labelInstDir.AutoSize = true;
      this.labelInstDir.Location = new System.Drawing.Point(7, 57);
      this.labelInstDir.Name = "labelInstDir";
      this.labelInstDir.Size = new System.Drawing.Size(51, 13);
      this.labelInstDir.TabIndex = 3;
      this.labelInstDir.Text = "Install dir:";
      // 
      // textBoxDir
      // 
      this.textBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDir.Location = new System.Drawing.Point(10, 78);
      this.textBoxDir.Name = "textBoxDir";
      this.textBoxDir.Size = new System.Drawing.Size(463, 20);
      this.textBoxDir.TabIndex = 4;
      this.textBoxDir.Text = "C:\\Programme\\MSSQL";
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.Location = new System.Drawing.Point(10, 123);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(132, 20);
      this.textBoxPassword.TabIndex = 8;
      this.textBoxPassword.Text = "MediaPortal";
      // 
      // labelPassword
      // 
      this.labelPassword.AutoSize = true;
      this.labelPassword.Location = new System.Drawing.Point(7, 102);
      this.labelPassword.Name = "labelPassword";
      this.labelPassword.Size = new System.Drawing.Size(70, 13);
      this.labelPassword.TabIndex = 7;
      this.labelPassword.Text = "sa Password:";
      // 
      // labelPasswordDesc
      // 
      this.labelPasswordDesc.AutoSize = true;
      this.labelPasswordDesc.Location = new System.Drawing.Point(7, 153);
      this.labelPasswordDesc.Name = "labelPasswordDesc";
      this.labelPasswordDesc.Size = new System.Drawing.Size(374, 13);
      this.labelPasswordDesc.TabIndex = 9;
      this.labelPasswordDesc.Text = "(The password for the admin-user of the SQL-Server. Default is \"MediaPortal\")";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(488, 77);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(93, 23);
      this.buttonBrowse.TabIndex = 10;
      this.buttonBrowse.Text = "browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // checkBoxFirewall
      // 
      this.checkBoxFirewall.AutoSize = true;
      this.checkBoxFirewall.Checked = true;
      this.checkBoxFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxFirewall.Location = new System.Drawing.Point(10, 182);
      this.checkBoxFirewall.Name = "checkBoxFirewall";
      this.checkBoxFirewall.Size = new System.Drawing.Size(363, 17);
      this.checkBoxFirewall.TabIndex = 11;
      this.checkBoxFirewall.Text = "Configure Windows Firewall to allow external access to database server";
      this.checkBoxFirewall.UseVisualStyleBackColor = true;
      // 
      // DBMSSettingsDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.checkBoxFirewall);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.labelPasswordDesc);
      this.Controls.Add(this.textBoxPassword);
      this.Controls.Add(this.labelPassword);
      this.Controls.Add(this.textBoxDir);
      this.Controls.Add(this.labelInstDir);
      this.Controls.Add(this.labelHeading);
      this.Name = "DBMSSettingsDlg";
      this.Size = new System.Drawing.Size(620, 224);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelInstDir, 0);
      this.Controls.SetChildIndex(this.textBoxDir, 0);
      this.Controls.SetChildIndex(this.labelPassword, 0);
      this.Controls.SetChildIndex(this.textBoxPassword, 0);
      this.Controls.SetChildIndex(this.labelPasswordDesc, 0);
      this.Controls.SetChildIndex(this.buttonBrowse, 0);
      this.Controls.SetChildIndex(this.checkBoxFirewall, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label labelInstDir;
    private System.Windows.Forms.TextBox textBoxDir;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.Label labelPassword;
    private System.Windows.Forms.Label labelPasswordDesc;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.CheckBox checkBoxFirewall;
  }
}
