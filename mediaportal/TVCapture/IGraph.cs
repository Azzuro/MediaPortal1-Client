using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;

namespace MediaPortal.TV.Recording
{
	public enum NetworkType 
	{
		ATSC,	// analog TV tuning
		DVBC,	// DVB-cable
		DVBS, // DVB-Sattelite
		DVBT  // DVB-Terrestial
	}
	/// <summary>
	/// Interface definition for graphbuilder objects
	/// A graphbuilder object supports one or more TVCapture cards and
	/// contains all the code/logic necessary todo
	/// -tv viewing
	/// -tv recording
	/// -tv timeshifting
	/// 
	/// Currently there are 2 implementations of IGraph
  /// Sinkgraph.cs      : support for cards with an onboard MPEG2 encoder like the hauppauge PVR 250/350/USB2
  /// SWEncodingGraph.cs: support for cards without an onboard mpeg2 encoder like the pinnacle PCTV Pro, WinTV,...
  /// 
  /// NOTE!!! IGraph instances are created by the GraphFactory.cs  
  ///   
	/// Basicly the flow is
	/// 
	/// for viewing:
	///   CreateGraph()
	///   if SupportsTimeshifting() then 
	///     StartTimeshifting()
  ///     ....
  ///     StopTimeshifting()
  ///     DeleteGraph()
	///   else
	///     StartViewing
	///     ...
	///     StopViewing()
	///     DeleteGraph()
	///     
	/// for recording:    
  ///   CreateGraph()
  ///   if SupportsTimeshifting() then 
  ///     StartTimeshifting()
	///     StartRecording()
	///     .....
	///     StopRecording()
	///     DeleteGraph()
	///   else
  ///     StartRecording()
  ///     .....
  ///     StopRecording()
  ///     DeleteGraph()
  ///     
  ///     
	/// </summary>
  /// <seealso cref="MediaPortal.TV.Recording.GraphFactory"/>

	public interface IGraph
	{
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    bool CreateGraph(int Quality);

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    void DeleteGraph();

    /// <summary>
    /// Starts timeshifting the TV channel and stores the timeshifting 
    /// files in the specified filename
    /// </summary>
    /// <param name="channel">TV channel to which card should be tuned</param>
    /// <param name="strFileName">Filename for the timeshifting buffers</param>
    /// <returns>boolean indicating if timeshifting is running or not</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    bool StartTimeShifting(TVChannel channel, string strFileName);
    
    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    bool StopTimeShifting();


    /// <summary>
		/// Starts recording live TV to a file
		/// <param name="channel">TV channel to which card should be tuned</param>
    /// <param name="strFileName">filename for the new recording</param>
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// <param name="timeProgStart">Contains the starttime of the current tv program</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the timeProgStart till the moment recording is stopped again
    /// </remarks>
    bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart);
    
    
    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    void StopRecording();


    /// <summary>
    /// Switches / tunes to another TV channel
		/// </summary>
		/// <param name="channel">TV channel to which card should be tuned</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    void TuneChannel(TVChannel channel);

    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    int GetChannelNumber();

    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    bool SupportsTimeshifting();


    /// <summary>
    /// Starts viewing the TV channel 
		/// </summary>
		/// <param name="channel">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    bool StartViewing(TVChannel channel);


    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// </remarks>
    bool StopViewing();

    /// <summary>
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    bool ShouldRebuildGraph(int iChannel);

    /// <summary>
    /// This method returns whether a signal is present. Meaning that the
    /// TV tuner (or video input) is tuned to a channel
    /// </summary>
    /// <returns>true:  tvtuner is tuned to a channel (or video-in has a video signal)
    ///          false: tvtuner is not tuned to a channel (or video-in has no video signal)
    /// </returns>
    bool SignalPresent();

    /// <summary>
    /// This method returns the frequency to which the tv tuner is currently tuned
    /// </summary>
    /// <returns>frequency in Hertz
    /// </returns>
    long VideoFrequency();

		/// <summary>
		/// MP will call this function on a regular basis
		/// It allows the tv card todo any progressing like
		/// getting/updating the EPG, channel information etc (for DVB cards)
		/// </summary>
		void Process();

		/// <summary>
		/// returns a collection of property pages for tvcapture card
		/// </summary>
		/// <returns></returns>
		PropertyPageCollection PropertyPages();

		bool SupportsFrameSize(Size framesize);

		IBaseFilter AudiodeviceFilter();
		NetworkType Network();
		void Tune(object tuningObject, int disecqNo);
		void StoreChannels(int ID,bool radio, bool tv);
		IBaseFilter Mpeg2DataFilter();

		void StartRadio(RadioStation station);
		void TuneRadioChannel(RadioStation station);
		void TuneRadioFrequency(int frequency);
		bool HasTeletext();
	}
}
