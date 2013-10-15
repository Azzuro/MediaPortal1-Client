namespace MediaPortal.Visualization
{
  partial class VisualizationWindow
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
      // Needed for RunRenderThread to prevent errors while disposing
      VisualizationRunning = false;

      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // VisualizationWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
      this.BackColor = System.Drawing.Color.Black;
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Black;
      this.Name = "VisualizationWindow";
      this.Size = new System.Drawing.Size(148, 148);
      this.ResumeLayout(false);

    }

    #endregion


  }
}
