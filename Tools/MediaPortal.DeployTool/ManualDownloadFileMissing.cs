using System;
using System.Windows.Forms;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class ManualDownloadFileMissing : Form
  {
    private string target_file;
    private string target_dir;

    private void UpdateUI()
    {
      Text = Localizer.GetBestTranslation("ManualDownload_Title");
      buttonBrowse.Text = Localizer.GetBestTranslation("MainWindow_browseButton");
    }

    public ManualDownloadFileMissing()
    {
      InitializeComponent();
      Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      UpdateUI();
    }

    public DialogResult ShowDialog(string targetDir, string targetFile)
    {
      target_file = targetFile;
      target_dir = targetDir;
      labelHeading.Text = String.Format(Localizer.GetBestTranslation("ManualDownload_errFileNotFound"), target_file);
      ShowDialog();
      return DialogResult.OK;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      openFileDialog.FileName = target_file;
      openFileDialog.InitialDirectory = target_dir;
      openFileDialog.ValidateNames = true;
      openFileDialog.ShowDialog();
      textBox1.Text = openFileDialog.FileName;
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      if (openFileDialog.FileName != null)
      {
        File.Move(openFileDialog.FileName, target_dir + "\\" + target_file);
        Close();
      }
    }
  }
}