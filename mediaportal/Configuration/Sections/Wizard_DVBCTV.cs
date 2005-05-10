using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.Configuration.Sections
{
	public class Wizard_DVBCTV : MediaPortal.Configuration.SectionSettings
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ComboBox cbCountry;
		
		struct DVBCList
		{
			public int frequency;		 // frequency
			public int modulation;	 // modulation
			public int symbolrate;	 // symbol rate
		}

		enum State
		{
			ScanStart,
			ScanFrequencies,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		int                                 currentIndex=-1;
		DVBCList[]													dvbcChannels=new DVBCList[1000];
		int																	count = 0;
		int																	retryCount=0;
		int																	newChannels, updatedChannels;


		public Wizard_DVBCTV() : this("DVBC TV")
		{
		}

		public Wizard_DVBCTV(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCountry = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.labelStatus);
			this.groupBox1.Controls.Add(this.progressBar1);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.cbCountry);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 360);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Setup digital tv (DVBC Cable)";
			// 
			// labelStatus
			// 
			this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelStatus.Location = new System.Drawing.Point(40, 169);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(400, 23);
			this.labelStatus.TabIndex = 11;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(32, 128);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(416, 16);
			this.progressBar1.TabIndex = 4;
			this.progressBar1.Visible = false;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(344, 72);
			this.button1.Name = "button1";
			this.button1.TabIndex = 3;
			this.button1.Text = "Scan...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 72);
			this.label2.Name = "label2";
			this.label2.TabIndex = 2;
			this.label2.Text = "Country/Region:";
			// 
			// cbCountry
			// 
			this.cbCountry.Location = new System.Drawing.Point(144, 72);
			this.cbCountry.Name = "cbCountry";
			this.cbCountry.Size = new System.Drawing.Size(168, 21);
			this.cbCountry.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(432, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mediaportal has detected one or more digital Tv cards. Select your country and pr" +
				"ess auto tune to scan for the tv and radio channels";
			// 
			// Wizard_DVBCTV
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Wizard_DVBCTV";
			this.Size = new System.Drawing.Size(496, 384);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion




		public override void OnSectionActivated()
		{
			base.OnSectionActivated ();
			labelStatus.Text="";
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory());
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".dvbc") >=0)
				{
					cbCountry.Items.Add(file);
				}
			}

		}

		void LoadFrequencies()
		{
			string countryName=(string)cbCountry.SelectedItem;
			if (countryName==String.Empty) return;
			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"Opening {0}",countryName);
			// load dvbcChannelsList list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(countryName);
			
			int LineNr=0;
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				{
					LineNr++;
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						tpdata = line.Split(new char[]{','});
						if(tpdata.Length!=3)
							tpdata = line.Split(new char[]{';'});
						if (tpdata.Length == 3)
						{
							try
							{
								dvbcChannels[count].frequency = Int32.Parse(tpdata[0]) ;
								string mod=tpdata[1].ToUpper();
								dvbcChannels[count].modulation=(int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
								if (mod=="1024QAM") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_1024QAM;
								if (mod=="112QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_112QAM;
								if (mod=="128QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_128QAM;
								if (mod=="160QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_160QAM;
								if (mod=="16QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16QAM;
								if (mod=="16VSB")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16VSB;
								if (mod=="192QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_192QAM;
								if (mod=="224QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_224QAM;
								if (mod=="256QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_256QAM;
								if (mod=="320QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_320QAM;
								if (mod=="384QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_384QAM;
								if (mod=="448QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_448QAM;
								if (mod=="512QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_512QAM;
								if (mod=="640QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_640QAM;
								if (mod=="64QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_64QAM;
								if (mod=="768QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_768QAM;
								if (mod=="80QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_80QAM;
								if (mod=="896QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_896QAM;
								if (mod=="8VSB")    dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_8VSB;
								if (mod=="96QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_96QAM;
								if (mod=="AMPLITUDE") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE;
								if (mod=="FREQUENCY") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY;
								if (mod=="BPSK")    dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_BPSK;
								if (mod=="OQPSK")		dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_OQPSK;
								if (mod=="QPSK")		dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_QPSK;
								
								dvbcChannels[count].symbolrate = Int32.Parse(tpdata[2]);
								count += 1;
							}
							catch
							{
								Log.WriteFile(Log.LogType.Capture,"Error in line:{0}", LineNr);
							}
						}
					}
				}
			} while (!(line == null));
			tin.Close();
		}
	

		private void button1_Click(object sender, System.EventArgs e)
		{
			LoadFrequencies();
			if (count<=0) return;
			progressBar1.Visible=true;
			newChannels=0; updatedChannels=0;
			DoScan();
			labelStatus.Text=String.Format("Imported {0} channels",newChannels);
		}

		private void DoScan()
		{
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBC)
				{
					captureCard = dev;
					captureCard.CreateGraph();
					break;
				}
			}

			while (currentIndex < count)
			{
				
				int index=currentIndex;
				if (index<0) index=0;
				float percent = ((float)index) / ((float)count);
				percent *= 100.0f;
				progressBar1.Value=(int)percent;
			
				if (retryCount==0)
				{
					ScanNextDVBCChannel();
					if (captureCard.SignalPresent())
					{
						ScanChannels();
					}
					retryCount=1;
				}
				else 
				{
					ScanDVBCChannel();
					if (captureCard.SignalPresent())
					{
						ScanChannels();
					}
					retryCount=0;
				}
			}
			captureCard.DeleteGraph();
			captureCard=null;
		}


		void ScanChannels()
		{
			DVBCList dvbcChan=dvbcChannels[currentIndex];
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2} retry:{3}",dvbcChan.frequency,dvbcChan.modulation.ToString(), dvbcChan.symbolrate, retryCount);
			string description=String.Format("Found signal for channel:{0} {1}, Scanning channels", currentIndex,chanDesc);
			labelStatus.Text=description;

			
			Application.DoEvents();
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels);
			
			
			
			return;
		}

		void ScanNextDVBCChannel()
		{
			currentIndex++;
			ScanDVBCChannel();
			Application.DoEvents();
		}

		void ScanDVBCChannel()
		{
			if (currentIndex<0 || currentIndex>=count)
			{
				
				progressBar1.Value=100;
				
				captureCard.DeleteGraph();
				return;
			}
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2} retry:{3}",
				dvbcChannels[currentIndex].frequency,dvbcChannels[currentIndex].modulation.ToString(), dvbcChannels[currentIndex].symbolrate, retryCount);
			string description=String.Format("Channel:{0}/{1} {2}", currentIndex,count,chanDesc);
			labelStatus.Text=description;

			Log.WriteFile(Log.LogType.Capture,"tune dvbcChannel:{0}/{1} {2}",currentIndex ,count,chanDesc);

			DVBChannel newchan = new DVBChannel();
			newchan.NetworkID=-1;
			newchan.TransportStreamID=-1;
			newchan.ProgramNumber=-1;

			newchan.Modulation=dvbcChannels[currentIndex].modulation;
			newchan.Symbolrate=(dvbcChannels[currentIndex].symbolrate)/1000;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.Frequency=dvbcChannels[currentIndex].frequency;
			captureCard.Tune(newchan,0);
			Application.DoEvents();
			System.Threading.Thread.Sleep(100);
			Application.DoEvents();
		}


	}
}

