using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using DShowNET;
using Microsoft.Win32;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for CodecsForm.
	/// </summary>
  public class CodecsForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.LinkLabel LinkLabel1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.LinkLabel LinkLabel2;
    private System.Windows.Forms.Label labelMPEG2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.LinkLabel LinkLabel3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.LinkLabel LinkLabel4;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Button button1;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public CodecsForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.labelMPEG2 = new System.Windows.Forms.Label();
			this.LinkLabel1 = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.LinkLabel2 = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.LinkLabel3 = new System.Windows.Forms.LinkLabel();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.LinkLabel4 = new System.Windows.Forms.LinkLabel();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelMPEG2
			// 
			this.labelMPEG2.Location = new System.Drawing.Point(24, 24);
			this.labelMPEG2.Name = "labelMPEG2";
			this.labelMPEG2.Size = new System.Drawing.Size(512, 32);
			this.labelMPEG2.TabIndex = 0;
			this.labelMPEG2.Text = "No MPEG2 video/audio codecs are installed on your PC. To play MPEG2 files, DVD\'s " +
				"or watch TV you\'ll need to install an MPEG2 codec. The Mediaportal team recommen" +
				"ds WinDVD6 or PowerDVD6";
			// 
			// LinkLabel1
			// 
			this.LinkLabel1.Location = new System.Drawing.Point(112, 64);
			this.LinkLabel1.Name = "LinkLabel1";
			this.LinkLabel1.Size = new System.Drawing.Size(264, 16);
			this.LinkLabel1.TabIndex = 1;
			this.LinkLabel1.TabStop = true;
			this.LinkLabel1.Text = "http://www.intervideo.com/jsp/WinDVD_Profile.jsp";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "WinDVD6:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 3;
			this.label3.Text = "PowerDVD6:";
			// 
			// LinkLabel2
			// 
			this.LinkLabel2.Location = new System.Drawing.Point(112, 88);
			this.LinkLabel2.Name = "LinkLabel2";
			this.LinkLabel2.Size = new System.Drawing.Size(464, 16);
			this.LinkLabel2.TabIndex = 4;
			this.LinkLabel2.TabStop = true;
			this.LinkLabel2.Text = "http://www.gocyberlink.com/english/products/product_main.jsp?ProdId=28";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(488, 32);
			this.label1.TabIndex = 5;
			this.label1.Text = "FFDShow is not installed on your PC. We recommend to install FFDShow if you want " +
				"to play xvid, divx, mpeg1 and many more media formats";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 168);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "FFDShow:";
			// 
			// LinkLabel3
			// 
			this.LinkLabel3.Location = new System.Drawing.Point(112, 168);
			this.LinkLabel3.Name = "LinkLabel3";
			this.LinkLabel3.Size = new System.Drawing.Size(384, 16);
			this.LinkLabel3.TabIndex = 7;
			this.LinkLabel3.TabStop = true;
			this.LinkLabel3.Text = "http://www.free-codecs.com/download/FFDShow.htm";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 208);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(488, 32);
			this.label5.TabIndex = 8;
			this.label5.Text = "The VOBSUB codec is not installed on your PC. We recommend to use vobsub if you w" +
				"ant to have subtitles with your movies";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 248);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = "VobSUB:";
			// 
			// LinkLabel4
			// 
			this.LinkLabel4.Location = new System.Drawing.Point(112, 248);
			this.LinkLabel4.Name = "LinkLabel4";
			this.LinkLabel4.Size = new System.Drawing.Size(416, 23);
			this.LinkLabel4.TabIndex = 10;
			this.LinkLabel4.TabStop = true;
			this.LinkLabel4.Text = "http://www.free-codecs.com/download/VobSub.htm";
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(32, 280);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(240, 24);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.Text = "Don\'t show this message again";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(512, 280);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(48, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// CodecsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(608, 317);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.LinkLabel4);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.LinkLabel3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.LinkLabel2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.LinkLabel1);
			this.Controls.Add(this.labelMPEG2);
			this.Name = "CodecsForm";
			this.Text = "Missing codecs";
			this.Load += new System.EventHandler(this.CodecsForm_Load);
			this.ResumeLayout(false);

		}
    #endregion

    private void button1_Click(object sender, System.EventArgs e)
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.SetValueAsBool("general","checkcodecs",!checkBox1.Checked);
      }
      this.Close();
    }

    private void CodecsForm_Load(object sender, System.EventArgs e)
    {
    }
    void ShowMPEG2Warning(bool bOn)
    {
      labelMPEG2.Visible=bOn;
      label2.Visible=bOn;
      label3.Visible=bOn;
      LinkLabel1.Visible=bOn;
      LinkLabel2.Visible=bOn;
    }
      
    void ShowFFDShowWarning(bool bOn)
    {
      label1.Visible=bOn;
      label4.Visible=bOn;
      LinkLabel3.Visible=bOn;
    }
      
    void ShowVobSUBWarning(bool bOn)
    {
      label5.Visible=bOn;
      label6.Visible=bOn;
      LinkLabel4.Visible=bOn;
    }

    public bool AreCodecsInstalled()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        bool checkCodecs=xmlreader.GetValueAsBool("general","checkcodecs",true);
        if (!checkCodecs) return true;
      }
      bool MPEG2CodecsInstalled=false;
      bool FFDShowInstalled=false;
      bool VobSubInstalled=false;
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);

      if (availableVideoFilters.Count>0 && availableAudioFilters.Count>0)
        MPEG2CodecsInstalled=true;

      try
      {
        RegistryKey hklm = Registry.LocalMachine;
        RegistryKey subkey = hklm.OpenSubKey(@"SOFTWARE\Classes\CLSID\{007FC171-01AA-4B3A-B2DB-062DEE815A1E}\InProcServer32");
        if (subkey!=null)
        {
          FFDShowInstalled=true;
          subkey.Close();
				}
				hklm.Close();
		
				hklm = Registry.ClassesRoot;
        subkey = hklm.OpenSubKey(@"CLSID\{0180E49C-13BF-46DB-9AFD-9F52292E1C22}\InprocServer32");
        if (subkey!=null)
        {
          VobSubInstalled=true;
          subkey.Close();
        }
        hklm.Close();
      }
      catch(Exception)
      {
      }
      ShowMPEG2Warning(!MPEG2CodecsInstalled);
      ShowFFDShowWarning(!FFDShowInstalled);
      ShowVobSUBWarning(!VobSubInstalled);
      if (MPEG2CodecsInstalled&&FFDShowInstalled&&VobSubInstalled) return true;
      return false;
    }
  }
}
