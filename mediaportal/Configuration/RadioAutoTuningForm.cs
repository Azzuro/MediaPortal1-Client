using System;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.Radio.Database;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for RadioAutoTuningForm.
	/// </summary>
	public class RadioAutoTuningForm : AutoTuningForm
	{
		int		currentChannel = 0;
		int   channelNo=1;
		TVCaptureDevice m_graph=null;

		public RadioAutoTuningForm(TVCaptureDevice graph)
		{
			this.m_graph = graph;

			//
			// Setup progress bar
			//
			SetStep(100000);
			SetInterval(87500000, 108000000);
		}

		public override void OnStartTuning(int startValue)
		{
			Log.Write("Radio:Start scan...");
			m_graph.TuneRadioFrequency(startValue);
			currentChannel=startValue;
			channelNo=1;
		}

		public override void OnStopTuning()
		{
      Text=String.Format("Radio tuning");
		}

		private new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).BeginInit();
			// 
			// cancelButton
			// 
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			// 
			// groupBox1
			// 
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			// 
			// itemsListBox
			// 
			this.itemsListBox.Name = "itemsListBox";
			// 
			// progressBar
			// 
			this.progressBar.Name = "progressBar";
			// 
			// startButton
			// 
			this.startButton.Name = "startButton";
			// 
			// stopButton
			// 
			this.stopButton.Name = "stopButton";
			// 
			// tunerTimer
			// 
			this.tunerTimer.Enabled = false;
			// 
			// RadioAutoTuningForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 222);
			this.Name = "RadioAutoTuningForm";
			this.Text = "Scan for radio channels";
			((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).EndInit();

		}

		public override void OnPerformTuning(int stepSize)
		{
      try
      {
        if(m_graph.SignalPresent() == true)
        {
          //
          // We have found a channel!
					//
					Log.Write("Radio:found signal on {0}",currentChannel);
					MediaPortal.Radio.Database.RadioStation newStation =new MediaPortal.Radio.Database.RadioStation();
          newStation.Name=String.Format("Station{0}", channelNo);
          newStation.Channel=channelNo;
					newStation.Frequency=currentChannel;
          int id=RadioDatabase.AddStation(ref newStation);
					RadioDatabase.MapChannelToCard(id,m_graph.ID);
					RadioStation stat = new RadioStation(channelNo,currentChannel);
					stat.Name=newStation.Name;
					stat.Type="Radio";
					AddItem(stat);
					channelNo++;
        }
      }
      catch(NotSupportedException)
      {
        // Unable to read signal strength, step to next frequency
      }

			//
			// Move forward
			//
      try
      {
				currentChannel+=stepSize;
        m_graph.TuneRadioFrequency(currentChannel );
        Text=String.Format("Radio tuning:{0:} Hz", (int)currentChannel);
      }
      catch(Exception)
      {
        MessageBox.Show("Failed to perform autotuning, the tuner card did not supply the data needed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
		}
	}
}
