using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.Configuration.Sections
{
	public class Wizard_AnalogTV : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ComboBox cbCities;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbCountries;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnManualTV;
		private System.Windows.Forms.Button btnManualRadio;
		XmlDocument docSetup;
		public Wizard_AnalogTV() : this("Analog TV")
		{
		}

		public Wizard_AnalogTV(string name) : base(name)
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
			this.label3 = new System.Windows.Forms.Label();
			this.labelStatus = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.cbCities = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCountries = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnManualTV = new System.Windows.Forms.Button();
			this.btnManualRadio = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.btnManualRadio);
			this.groupBox1.Controls.Add(this.btnManualTV);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.labelStatus);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.cbCities);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.cbCountries);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 360);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Setup Analog tv and radio channels";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.label3.Location = new System.Drawing.Point(32, 232);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(432, 56);
			this.label3.TabIndex = 11;
			this.label3.Text = "NOTE: If your country/city is not listed then we dont have information which tele" +
				"vision and radio channels you can receive. You will need to add the tv/radio cha" +
				"nnels manually in the next screen by pressing the Manual Tune button";
			// 
			// labelStatus
			// 
			this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelStatus.Location = new System.Drawing.Point(32, 176);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(400, 23);
			this.labelStatus.TabIndex = 10;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(32, 136);
			this.button1.Name = "button1";
			this.button1.TabIndex = 9;
			this.button1.Text = "Download";
			this.button1.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// cbCities
			// 
			this.cbCities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCities.Location = new System.Drawing.Point(96, 104);
			this.cbCities.Name = "cbCities";
			this.cbCities.Size = new System.Drawing.Size(344, 21);
			this.cbCities.Sorted = true;
			this.cbCities.TabIndex = 8;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 104);
			this.label2.Name = "label2";
			this.label2.TabIndex = 7;
			this.label2.Text = "City:";
			// 
			// cbCountries
			// 
			this.cbCountries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCountries.Location = new System.Drawing.Point(96, 72);
			this.cbCountries.Name = "cbCountries";
			this.cbCountries.Size = new System.Drawing.Size(344, 21);
			this.cbCountries.Sorted = true;
			this.cbCountries.TabIndex = 6;
			this.cbCountries.SelectedIndexChanged += new System.EventHandler(this.cbCountries_SelectedIndexChanged_1);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 72);
			this.label1.Name = "label1";
			this.label1.TabIndex = 5;
			this.label1.Text = "Country:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(432, 40);
			this.label4.TabIndex = 12;
			this.label4.Text = "Mediaportal has detected one or more analog Tv cards. Select your country/city  a" +
				"nd press Import to download all the tv and radio channels";
			// 
			// btnManualTV
			// 
			this.btnManualTV.Location = new System.Drawing.Point(32, 296);
			this.btnManualTV.Name = "btnManualTV";
			this.btnManualTV.Size = new System.Drawing.Size(152, 23);
			this.btnManualTV.TabIndex = 13;
			this.btnManualTV.Text = "Manual scan TV channels";
			this.btnManualTV.Click += new System.EventHandler(this.btnManualTV_Click);
			// 
			// btnManualRadio
			// 
			this.btnManualRadio.Location = new System.Drawing.Point(200, 296);
			this.btnManualRadio.Name = "btnManualRadio";
			this.btnManualRadio.Size = new System.Drawing.Size(152, 23);
			this.btnManualRadio.TabIndex = 14;
			this.btnManualRadio.Text = "Manual scan Radio channels";
			this.btnManualRadio.Click += new System.EventHandler(this.btnManualRadio_Click);
			// 
			// Wizard_AnalogTV
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Wizard_AnalogTV";
			this.Size = new System.Drawing.Size(496, 384);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion




		public override void OnSectionActivated()
		{
			base.OnSectionActivated ();
			labelStatus.Text="";
			cbCountries.Items.Clear();
			cbCities.Items.Clear();
			docSetup = new XmlDocument();
			docSetup.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList listCountries = docSetup.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				cbCountries.Items.Add(nodeCountryName.Value);
			}
			
			if (cbCountries.Items.Count> 0 && cbCountries.SelectedIndex<0)
				cbCountries.SelectedIndex=0;
			FillInCities();
		}

		void FillInCities()
		{
			string country=(string)cbCountries.SelectedItem;
			foreach (TunerCountry analogCountry in TunerCountries.Countries)
			{
				if (country.ToLower().IndexOf(analogCountry.Country.ToLower())>=0)
				{
					using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						xmlwriter.SetValue("capture", "countryname", analogCountry.Country);
						xmlwriter.SetValue("capture", "country", analogCountry.Id);
						break;
					}
				}
			}
			cbCities.Items.Clear();
			XmlNodeList listCountries = docSetup.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				if (nodeCountryName.Value==country)
				{
					XmlNodeList listCities = nodeCountry.SelectNodes("city");
					foreach (XmlNode nodeCity in listCities)
					{
						XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
						cbCities.Items.Add(listCitiesName.Value);
					}
					break;
				}
			}
			if (cbCities.Items.Count> 0 && cbCities.SelectedIndex<0)
				cbCities.SelectedIndex=0;
		}

		private void button1_Click_1(object sender, System.EventArgs e)
		{
			string country=(string)cbCountries.SelectedItem;
			string city=(string)cbCities.SelectedItem;
			XmlDocument doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList listCountries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				if (nodeCountryName.Value==country)
				{
					XmlNodeList listCities = nodeCountry.SelectNodes("city");
					foreach (XmlNode nodeCity in listCities)
					{
						XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
						if (listCitiesName.Value==city)
						{
							XmlNode nodeAnalog = nodeCity.SelectSingleNode("analog");
							int tvChannels;
							int radioChannels;
							ImportAnalogChannels(nodeAnalog.InnerText, out tvChannels, out radioChannels);

							labelStatus.Text=String.Format("Imported {0} tv channels, {1} radio channels",tvChannels, radioChannels);
							return;
						}
					}
				}
			}
		}

		void ImportAnalogChannels(string xmlFile,out int tvChannels, out int radioChannels)
		{
			tvChannels=0;
			radioChannels=0;
			XmlDocument doc = new XmlDocument();
			UriBuilder builder = new UriBuilder("http","mediaportal.sourceforge.net",80,"tvsetup/analog/"+xmlFile);
			doc.Load(builder.Uri.AbsoluteUri);
			XmlNodeList listTvChannels = doc.DocumentElement.SelectNodes("/mediaportal/tv/channel");
			foreach (XmlNode nodeChannel in listTvChannels)
			{
				XmlNode name					 = nodeChannel.Attributes.GetNamedItem("name");
				XmlNode number				 = nodeChannel.Attributes.GetNamedItem("number");
				XmlNode frequency			 = nodeChannel.Attributes.GetNamedItem("frequency");
				TVChannel chan =new TVChannel();
				chan.Name=name.Value;
				try
				{
					chan.Number=Int32.Parse(number.Value);
				}
				catch(Exception){}
				chan.Frequency=ConvertToTvFrequency(frequency.Value, ref chan);
				TVDatabase.AddChannel(chan);
				tvChannels++;
			}
			XmlNodeList listRadioChannels = doc.DocumentElement.SelectNodes("/mediaportal/radio/channel");
			foreach (XmlNode nodeChannel in listRadioChannels)
			{
				XmlNode name					 = nodeChannel.Attributes.GetNamedItem("name");
				XmlNode frequency			 = nodeChannel.Attributes.GetNamedItem("frequency");
				MediaPortal.Radio.Database.RadioStation chan =new MediaPortal.Radio.Database.RadioStation();
				chan.Name=name.Value;
				chan.Frequency=ConvertToFrequency(frequency.Value);
				RadioDatabase.AddStation(ref chan);
				radioChannels++;
			}
		}
		long ConvertToFrequency(string frequency)
		{
			if (frequency.Trim()==String.Empty) return 0;
			float testValue=189.24f;
			string usage=testValue.ToString("f2");
			if (usage.IndexOf(".")>=0) frequency=frequency.Replace(",",".");
			if (usage.IndexOf(",")>=0) frequency=frequency.Replace(".",",");
			double freqValue=Convert.ToDouble(frequency);
			freqValue*=1000000;
			return (long)(freqValue);
		}
		

		long ConvertToTvFrequency(string frequency, ref TVChannel chan)
		{
			if (frequency.Trim()==String.Empty) return 0;
			chan.Number=TVDatabase.FindFreeTvChannelNumber(chan.Number);
			frequency=frequency.ToUpper();
			for (int i=0; i < TVChannel.SpecialChannels.Length;++i)
			{
				if (frequency.Equals(TVChannel.SpecialChannels[i].Name))
				{
					return TVChannel.SpecialChannels[i].Frequency;
				}
			}

			float testValue=189.24f;
			string usage=testValue.ToString("f2");
			if (usage.IndexOf(".")>=0) frequency=frequency.Replace(",",".");
			if (usage.IndexOf(",")>=0) frequency=frequency.Replace(".",",");
			double freqValue=Convert.ToDouble(frequency);
			freqValue*=1000000;
			return (long)(freqValue);
		}
		private void cbCountries_SelectedIndexChanged_1(object sender, System.EventArgs e)
		{
			FillInCities();
		}


		TVCaptureDevice CaptureCard
		{
			get
			{
				TVCaptureCards cards = new TVCaptureCards();
				cards.LoadCaptureCards();
				foreach (TVCaptureDevice dev in cards.captureCards)
				{
					if (dev.Network==NetworkType.Analog)
					{
						return dev;
					}
				}
				return null;
			}
		}
		private void btnManualTV_Click(object sender, System.EventArgs e)
		{
			TVCaptureDevice dev=CaptureCard;
			if (dev==null) return;
			AnalogTVTuningForm dialog = new AnalogTVTuningForm();
			if ( dev.CreateGraph())
			{
				ITuning tuning=GraphFactory.CreateTuning(CaptureCard);
				if (tuning!=null)
				{
					dialog.Tuning=tuning;
					dialog.Card=CaptureCard;
					dialog.ShowDialog(this);		
					MapTvToOtherCards(dev.ID);
				}
				dev.DeleteGraph();
			}
		}

		private void btnManualRadio_Click(object sender, System.EventArgs e)
		{
			TVCaptureDevice dev=CaptureCard;
			if (dev==null) return;
			if (dev.CreateGraph())
			{
				RadioAutoTuningForm dialog = new RadioAutoTuningForm(dev);
				dialog.ShowDialog(this);		
				MapRadioToOtherCards(dev.ID);
				dev.DeleteGraph();
			}
		}
		void MapTvToOtherCards(int id)
		{
			ArrayList tvchannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref tvchannels,id);
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.Analog && dev.ID != id)
				{
					foreach (TVChannel chan in tvchannels)
					{
						TVDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
		void MapRadioToOtherCards(int id)
		{
			ArrayList radioChans = new ArrayList();
			RadioDatabase.GetStationsForCard(ref radioChans,id);
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.Analog && dev.ID != id)
				{
					foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
					{
						RadioDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}

	}
}

