using System;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Interface definition for auto-tuning
	/// 
	/// MP now supports multiple TV capture cards like analog cable/antenne, DVB-S, DVB-T and DVB-C
	/// each type has its own specific auto-tuning to locate/find new TV Channels
	/// By implementing this ITuning interface for a specific card type the configuration.exe does not need to know
	/// all details about tuning. 
	/// The configuration.exe asks graphfactory for an ITuning interface for a specific card
	/// and when it gets it calls the AutoTune() method
	/// </summary>
	/// 
	public interface AutoTuneCallback
	{
		/// <summary>
		/// new channel found. tuning is paused and user can now map it to a tv channel or continue
		/// </summary>
		void OnNewChannel();

		/// <summary>
		/// Shows current status of the tuning progress
		/// </summary>
		/// <param name="description"></param>
		void OnStatus(string description);
		/// <summary>
		/// Shows current status of the tuning progress
		/// </summary>
		/// <param name="description"></param>
		void OnStatus2(string description);

		/// <summary>
		/// Shows how much percent of the tuning has been done
		/// </summary>
		/// <param name="percentDone"></param>
		void OnProgress(int percentDone);

		/// <summary>
		/// called when tuning has ended
		/// </summary>
		void OnEnded();

		
		/// <summary>
		/// called when the listview containing the tvchannels should be refreshed
		/// </summary>
		void UpdateList();
	}

	public interface ITuning
	{
		/// <summary>
		/// This method should do all auto-tuning for TV
		/// It should locate & find all tv channels for the card specified and store them in the database
		/// </summary>
		/// <param name="card">specifies the tvcapture card for which tuning should occur</param>
		/// <param name="callback">specifies a callback interface to indicate status updates</param>
		void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback callback);

		
		/// <summary>
		/// This method should do all auto-tuning for Radio
		/// It should locate & find all radio channels for the card specified and store them in the database
		/// </summary>
		/// <param name="card">specifies the tvcapture card for which tuning should occur</param>
		/// <param name="callback">specifies a callback interface to indicate status updates</param>
		void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback);


		/// <summary>
		/// This method continues the scanning progress
		/// </summary>
		void Continue();

		
		/// <summary>
		/// This method maps the current tv/radio channel found to a tv channel name
		/// The method should store all info for the current tuned channel in the database
		/// </summary>
		int  MapToChannel(string channel);

		void Stop();
	}
}
