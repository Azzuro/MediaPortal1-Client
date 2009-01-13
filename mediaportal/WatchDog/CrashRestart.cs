using System;
using System.Windows.Forms;

namespace WatchDog
{
  public partial class CrashRestartDlg : MPForm
  {
    private int ticks = 10;

    public CrashRestartDlg(int cancelDelay)
    {
      InitializeComponent();
      ticks = cancelDelay;
      lDelay.Text = ticks.ToString() + " second(s)";
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      ticks--;
      lDelay.Text = ticks.ToString() + " second(s)";
      if (ticks == 0)
      {
        timer1.Enabled = false;
        this.DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}