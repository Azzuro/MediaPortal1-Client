using System;
using System.IO;
using System.Threading;

using MediaPortal.Configuration;

namespace MediaPortal.MPInstaller
{
  public partial class QueueInstaller : MPInstallerForm
  {
    #region local vars
    public QueueEnumerator queue = new QueueEnumerator();
    MPInstallHelper inst = new MPInstallHelper();
    int time_to_close = 10;
    #endregion

    public QueueInstaller()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      this.Close();
    }

    private void QueueInstaller_Shown(object sender, EventArgs e)
    {
      this.Refresh();
      Thread.Sleep(2000);
      inst.LoadFromFile();
      this.Refresh();
      progressBar1.Maximum = queue.Items.Count;
      foreach (QueueItem item in queue.Items)
      {
        this.Refresh();
        Thread.Sleep(1000);
        listBox2.Items.Clear();
        switch (item.Action)
        {
          case QueueAction.Install:
            {
              listBox1.Items.Add(string.Format("Installing : {0} - {1}", item.Name, item.Version));
              progressBar3.Visible = true;
              string temp_file = item.LocalFile;
              if (!File.Exists(item.LocalFile))
              {
                listBox1.Items.Add("Local file not found downloading from main site ...");
                temp_file = Path.GetTempFileName();
                DownloadForm dw1 = new DownloadForm(item.DownloadUrl, temp_file);
                dw1.Text = string.Format("Download {0} - {1}", item.Name, item.Version);
                dw1.ShowDialog();
              }
              if (File.Exists(temp_file))
              {
                MPpackageStruct package = new MPpackageStruct();
                package.LoadFromFile(temp_file);
                if (package.isValid)
                {
                  package.InstallerInfo.SetupGroups = item.SetupGroups;
                  package.InstallableSkinList.AddRange(package.SkinList);
                  progressBar2.Maximum = package.InstallerInfo.FileList.Count + 1;

                  package.InstallerScript.Install(progressBar3, progressBar2, listBox2);

                  inst.Add(package);
                  inst.SaveToFile();
                  if (package.InstallerInfo.ProjectProperties.ClearSkinCache)
                  {
                    Directory.Delete(Config.GetFolder(Config.Dir.Cache), true);
                  }
                }
                else
                {
                  listBox1.Items.Add("Invalid package ! Install aborted");
                }
              }
              else
              {
                listBox1.Items.Add("Extension package file not found ");
              }
              progressBar1.Value++;
            }
            break;
          case QueueAction.Uninstall:
            {
              listBox1.Items.Add(string.Format("Uninstalling : {0} - {1}", item.Name, item.Version));
              MPpackageStruct pk = inst.Find(item.Name);
              if (pk != null && pk.InstallerInfo.Uninstall.Count > 0)
              {
                progressBar3.Visible = false;
                progressBar2.Maximum = pk.InstallerInfo.Uninstall.Count;
                progressBar2.Value = 0;
                for (int i = 0; i < pk.InstallerInfo.Uninstall.Count; i++)
                {
                  UninstallInfo u = (UninstallInfo)pk.InstallerInfo.Uninstall[i];
                  progressBar2.Value++;
                  progressBar2.Update();
                  progressBar2.Refresh();
                  if (System.IO.File.Exists(u.Path))
                  {
                    if (System.IO.File.GetCreationTime(u.Path) == u.Date)
                    {
                      try
                      {
                        System.IO.File.Delete(u.Path);
                        listBox2.Items.Add(u.Path);
                        listBox2.Update();
                        listBox2.Refresh();
                        this.Refresh();
                        this.Update();
                      }
                      catch (Exception)
                      {
                      }
                    }
                    else
                    {
                      listBox2.Items.Add("File date changed :" + u.Path);
                    }
                  }
                  else
                  {
                    listBox2.Items.Add("File not found :" + u.Path);
                  }
                }
                if (pk != null)
                  inst.Items.Remove(pk);
              }
              else
              {
                listBox1.Items.Add("No uninstall information was found ....");
                if (pk != null)
                  inst.Items.Remove(pk);
              }
            }
            inst.SaveToFile();
            break;
          case QueueAction.Unknow:
            break;
          default:
            break;
        }
      }
      queue.Items.Clear();
      queue.Save(MpiFileList.QUEUE_LISTING);
      button_close.Enabled = true;
      timer1.Enabled = true;
    }

    void ClearSkinCashe()
    {
      try
      {
        Directory.Delete(Config.GetFolder(Config.Dir.Cache), true);
      }
      catch (Exception)
      {

      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      time_to_close--;
      button_close.Text = string.Format("Close ({0})", time_to_close.ToString()); 
      if (time_to_close == 0)
      {
        this.Close();
      }
    }

  }
}