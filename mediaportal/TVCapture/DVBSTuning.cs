using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using System.Xml;


namespace MediaPortal.TV.Recording
{

	/// <summary>
	/// Summary description for DVBSTuning.
	/// </summary>
	public class DVBSTuning : ITuning
	{
		struct TPList
		{
			public int TPfreq; // frequency
			public int TPpol;  // polarisation 0=hori, 1=vert
			public int TPsymb; // symbol rate
		}
		enum State
		{
			ScanStart,
			ScanTransponders,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		int                                 currentIndex=0;
		private System.Windows.Forms.Timer  timer1;
		State                               currentState;
		TPList[]														transp=new TPList[200];
		int																	count = 0;

		public DVBSTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			captureCard=card;
			callback=statusCallback;

			currentState=State.ScanStart;
			currentIndex=0;

			OpenFileDialog ofd =new OpenFileDialog();
			ofd.Filter = "Transponder-Listings (*.tpl)|*.tpl";
			ofd.Title = "Choose Transponder-Listing Files";
			DialogResult res=ofd.ShowDialog();
			if(res!=DialogResult.OK) return;
			
			count = 0;
			string line;
			string[] tpdata;
			Log.Write("Opening {0}",ofd.FileName);
			// load transponder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(ofd.FileName);
			
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
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
			
								transp[count].TPfreq = Int32.Parse(tpdata[0]) *1000;
								switch (tpdata[1].ToLower())
								{
									case "v":
						
										transp[count].TPpol = 1;
										break;
									case "h":
						
										transp[count].TPpol = 0;
										break;
									default:
						
										transp[count].TPpol = 0;
										break;
								}
								transp[count].TPsymb = Int32.Parse(tpdata[2]);
								count += 1;
							}
							catch
							{}
						}
					}
			} while (!(line == null));
			tin.Close();
			

			Log.Write("loaded:{0} transponders", count);
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
			return;
		}

		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
		{
			// TODO:  Add DVBSTuning.AutoTuneRadio implementation
		}

		public void Continue()
		{
			// TODO:  Add DVBSTuning.Continue implementation
		}

		public int MapToChannel(string channel)
		{
			// TODO:  Add DVBSTuning.MapToChannel implementation
			return 0;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (currentIndex >= count)
				return;
			
			float percent = ((float)currentIndex) / ((float)count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			TPList transponder=transp[currentIndex];
			string chanDesc=String.Format("freq:{0} Khz, Pol:{1} SR:{2}",
						transponder.TPfreq, transponder.TPpol, transponder.TPsymb );
			string description=String.Format("Transponder:{0}/{1} {2}", currentIndex,count,chanDesc);

			if (currentState==State.ScanTransponders)
			{
				if (captureCard.SignalPresent())
				{
					Log.Write("Found signal for transponder:{0} {1}",currentIndex,chanDesc);
					currentState=State.ScanChannels;
				}
			}

			if (currentState==State.ScanTransponders || currentState==State.ScanStart)
			{
				currentState=State.ScanTransponders ;
				callback.OnStatus(description);
				ScanNextTransponder();
			}

			if (currentState==State.ScanChannels)
			{
				description=String.Format("Found signal for transponder:{0} {1}, Scanning channels", currentIndex,chanDesc);
				callback.OnStatus(description);
				ScanChannels();
			}
			
		}

		void ScanChannels()
		{
			captureCard.Process();

			timer1.Enabled=false;
			captureCard.StoreTunedChannels(false,true);
			callback.UpdateList();
			Log.Write("timeout, goto scanning transponders");
			currentState=State.ScanTransponders;
			ScanNextTransponder();
			timer1.Enabled=true;
			return;
		}

		void ScanNextTransponder()
		{
			currentIndex++;
			if (currentIndex>=count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}
			DVBGraphBDA.DVBChannel newchan = new DVBGraphBDA.DVBChannel();
			newchan.ONID=-1;
			newchan.TSID=-1;
			newchan.SID=-1;

			newchan.polarisation=transp[currentIndex].TPpol;
			newchan.symbolRate=transp[currentIndex].TPsymb;
			newchan.innerFec=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.carrierFrequency=transp[currentIndex].TPfreq;

			

			Log.Write("tune transponder:{0} freq:{1} KHz symbolrate:{2} polarisation:{3}",currentIndex,
									newchan.carrierFrequency,newchan.symbolRate,newchan.polarisation);
			captureCard.Tune(newchan);
		}

		#endregion
	}
}
