namespace MediaPortal.MPInstaller
{
  partial class QueueInstaller
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
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.progressBar2 = new System.Windows.Forms.ProgressBar();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.button1 = new System.Windows.Forms.Button();
      this.progressBar3 = new System.Windows.Forms.ProgressBar();
      this.listBox2 = new System.Windows.Forms.ListBox();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(12, 12);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(505, 16);
      this.progressBar1.TabIndex = 0;
      // 
      // progressBar2
      // 
      this.progressBar2.Location = new System.Drawing.Point(12, 135);
      this.progressBar2.Name = "progressBar2";
      this.progressBar2.Size = new System.Drawing.Size(505, 15);
      this.progressBar2.TabIndex = 1;
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(12, 34);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(505, 95);
      this.listBox1.TabIndex = 2;
      // 
      // button1
      // 
      this.button1.Enabled = false;
      this.button1.Location = new System.Drawing.Point(229, 275);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 3;
      this.button1.Text = "Close";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // progressBar3
      // 
      this.progressBar3.Location = new System.Drawing.Point(12, 156);
      this.progressBar3.Name = "progressBar3";
      this.progressBar3.Size = new System.Drawing.Size(505, 15);
      this.progressBar3.TabIndex = 4;
      // 
      // listBox2
      // 
      this.listBox2.FormattingEnabled = true;
      this.listBox2.Location = new System.Drawing.Point(12, 177);
      this.listBox2.Name = "listBox2";
      this.listBox2.Size = new System.Drawing.Size(505, 82);
      this.listBox2.TabIndex = 5;
      // 
      // QueueInstaller
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(529, 301);
      this.Controls.Add(this.listBox2);
      this.Controls.Add(this.progressBar3);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.progressBar2);
      this.Controls.Add(this.progressBar1);
      this.Name = "QueueInstaller";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "QueueInstaller";
      this.Shown += new System.EventHandler(this.QueueInstaller_Shown);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.ProgressBar progressBar2;
    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ProgressBar progressBar3;
    private System.Windows.Forms.ListBox listBox2;
  }
}