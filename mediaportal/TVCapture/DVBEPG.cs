using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung f�r DVBEPG.
	/// </summary>
	public class DVBEPG
	{
		public DVBEPG()
		{
			m_cardType=(int)EPGCard.Invalid;
		}

		public DVBEPG(int card)
		{

			m_cardType=card;
			m_networkType=NetworkType.DVBS;

		}
		public DVBEPG(int card, NetworkType networkType)
		{
			//
			// TODO: F�gen Sie hier die Konstruktorlogik hinzu
			//
			m_cardType=card;
			m_networkType=networkType;
		}
		//public event SendCounter GiveFeedback;
		DVBSections		m_sections=new DVBSections();
		int				m_cardType=0;
		string			m_channelName="";
		string			m_languagesToGrab="";
		ArrayList		m_titleBuffer=new ArrayList();
		ArrayList		m_namesBuffer=new ArrayList();
		ArrayList		m_themeBuffer=new ArrayList();
		ArrayList		m_summaryBuffer=new ArrayList();
		NetworkType		m_networkType;
		ArrayList		m_streamBuffer=new ArrayList();
		int				m_addsToDatabase=0;
		int				m_mhwChannelsCount=0;
		bool			m_titlesParsing=false;
		bool			m_summaryParsing=false;
		bool			m_channelsParsing=false;
		int				m_channelGrabLen=0;
		System.Windows.Forms.Label m_mhwChannels;
		System.Windows.Forms.Label m_mhwTitles;
		bool			m_isLocked=false;
		
		// mhw
		public struct Programm 
		{
			public int		ID;
			public int		ChannelID;
			public int		ThemeID;
			public int		PPV;
			public DateTime	Time;
			public bool		Summaries;
			public int		Duration;
			public string	Title;
			public int		ProgrammID;
			public string	ProgrammName;
			public int		TransportStreamID;
			public int		NetworkID;
		};
		//
		public struct MHWTheme
		{
			public int ThemeIndex;
			public string ThemeText;
		}
		public struct MHWChannel
		{
			public int		NetworkID;
			public int		TransponderID;
			public int		ChannelID;
			public string	ChannelName;
		};
		public struct Summary
		{
			public int		ProgramID;// its the programm-id of epg, not an channel id
			public string	Description;
		}
		// mhw end
		public enum EPGCard
		{
			Invalid=0,
			TechnisatStarCards,
			BDACards,
			Unknown,
			ChannelName
		}
		//
		//
		public string Languages
		{
			get
			{
				return m_languagesToGrab;
			}
			set
			{
				m_languagesToGrab=value;
			}
		}
		//
		// commits epg-data to database
		//
		public bool Locked
		{
			get{return m_isLocked;}
		}
		public System.Windows.Forms.Label LabelChannels
		{
			set{m_mhwChannels=value;}
		}
		public System.Windows.Forms.Label LabelTitles
		{
			set{m_mhwTitles=value;}
		}
		public bool ChannelsReady
		{
			get{return m_mhwChannelsCount>0?true:false;}
		}
		public int ChannelsGrabLen
		{
			get{return m_channelGrabLen;}
			set{m_channelGrabLen=value;}
		}
		public bool SummaryParsing
		{
			get{return m_summaryParsing;}
			set{m_summaryParsing=value;}
		}
		public bool ChannelsParsing
		{
			get{return m_channelsParsing;}
			set
			{

				m_channelsParsing=value;
				if(m_namesBuffer.Count>0)
					m_channelsParsing=false;
			}
		}
		public bool TitlesParsing
		{
			get{return m_titlesParsing;}
		}
		public int GetAdditionsToDB
		{
			get{return m_addsToDatabase;}
		}

		public void GetMHWBuffer(ref ArrayList channels,ref ArrayList titles,ref ArrayList themes,ref ArrayList summaries)
		{
			lock(m_namesBuffer.SyncRoot)
			{
				channels=(ArrayList)m_namesBuffer.Clone();
			}
			lock(m_titleBuffer.SyncRoot)
			{
				titles=(ArrayList)m_titleBuffer.Clone();
			}
			lock(m_themeBuffer.SyncRoot)
			{
				themes=(ArrayList)m_themeBuffer.Clone();
			}
			lock(m_summaryBuffer.SyncRoot)
			{
				summaries=(ArrayList)m_summaryBuffer.Clone();
			}
		}
		public void SetMHWBuffer(ArrayList channels,ArrayList titles,ArrayList themes,ArrayList summaries)
		{
			lock(m_namesBuffer.SyncRoot)
			{
				lock(channels.SyncRoot)
				{
					m_namesBuffer=(ArrayList)channels.Clone();
				}
			}
			lock(m_titleBuffer.SyncRoot)
			{
				lock(titles.SyncRoot)
				{
					m_titleBuffer=(ArrayList)titles.Clone();
				}
			}
			lock(m_themeBuffer.SyncRoot)
			{
				lock(themes.SyncRoot)
				{
					m_themeBuffer=(ArrayList)themes.Clone();
				}
			}
			lock(m_summaryBuffer.SyncRoot)
			{
				lock(summaries.SyncRoot)
				{
					m_summaryBuffer=(ArrayList)summaries.Clone();
				}
			}
		}
		//
		//
		//
		public int ChannelCount
		{
			get{return m_namesBuffer.Count;}
		}

		public int ClearBuffer()
		{
			m_streamBuffer=new ArrayList();
			m_namesBuffer=new ArrayList();
			m_titleBuffer=new ArrayList();
			m_summaryBuffer=new ArrayList();
			m_themeBuffer=new ArrayList();
			m_mhwChannelsCount=0;
			m_titlesParsing=false;
			m_summaryParsing=false;
			m_channelsParsing=false;
			m_isLocked=false;
			return 0;
		}
		public int GetEPG(out DateTime reGrabTime)
		{

			int count=SubmittMHW(out reGrabTime);
			ClearBuffer();
			return count;
		}
		public int SetEITToDatabase(DVBSections.EITDescr data,string channelName,int eventKind, out DateTime dateProgramEnd)
		{
			try
			{
				dateProgramEnd=DateTime.MinValue;
				int retVal=0;
				//
				//
				if(data.extendedEventUseable==false && data.shortEventUseable==false)
				{
					//Log.Write("epg-grabbing: event IGNORED by language selection");
					return 0;
				}
				
				//
				TVProgram tv=new TVProgram();
				long chStart=0;
				long chEnd=0;

				if(data.isMHWEvent==false)
				{
					System.DateTime date;
					try
					{
						date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
					}
					catch
					{
						return 0;
					}
					date=date.ToLocalTime();
					System.DateTime dur=new DateTime(date.Ticks);
					dur=dur.AddSeconds((double)data.duration_ss);
					dur=dur.AddMinutes((double)data.duration_mm);
					dur=dur.AddHours((double)data.duration_hh);
					System.DateTime chStartDate=new DateTime((long)date.Ticks);
					chStartDate=chStartDate.AddMinutes(2);
					System.DateTime chEndDate=new DateTime((long)dur.Ticks-(4*60000));
					chStart=GetLongFromDate(chStartDate.Year,chStartDate.Month,chStartDate.Day,chStartDate.Hour,chStartDate.Minute,chStartDate.Second);
					chEnd=GetLongFromDate(chEndDate.Year,chEndDate.Month,chEndDate.Day,chEndDate.Hour,chEndDate.Minute,chEndDate.Second);
					//
					//
					tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
					tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
					dateProgramEnd=new DateTime(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				}
				else
				{
					DateTime date=data.mhwStartTime;
					System.DateTime dur=new DateTime(date.Ticks);
					dur=dur.AddMinutes((double)data.duration_mm);
					System.DateTime chStartDate=new DateTime((long)date.Ticks);
					chStartDate=chStartDate.AddMinutes(2);
					System.DateTime chEndDate=new DateTime((long)dur.Ticks-(4*60000));
					chStart=GetLongFromDate(chStartDate.Year,chStartDate.Month,chStartDate.Day,chStartDate.Hour,chStartDate.Minute,chStartDate.Second);
					chEnd=GetLongFromDate(chEndDate.Year,chEndDate.Month,chEndDate.Day,chEndDate.Hour,chEndDate.Minute,chEndDate.Second);
					//
					//
					tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
					tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
					dateProgramEnd=new DateTime(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				}
				tv.Channel=channelName;
				tv.Genre=data.genere_text;

				tv.Title=data.event_item;
				tv.Description=data.event_item_text;
				//
				if(tv.Title==null)
					tv.Title="";

				if(tv.Description==null)
					tv.Description="";

				if(tv.Description=="")
					tv.Description=data.event_text;

				if(tv.Title=="")
					tv.Title=data.event_name;

				//
				if(tv.Title=="" || tv.Title=="n.a.") 
				{
					//Log.Write("epg: entrie without title found");
					dateProgramEnd=DateTime.MinValue;
					return 0;
				}

				//
				// for check
				//
				ArrayList programsInDatabase = new ArrayList();
				TVDatabase.GetProgramsPerChannel(tv.Channel,chStart,chEnd,ref programsInDatabase);
				if(channelName=="")
				{
					//Log.Write("epg-grab: FAILED no channel-name: {0} : {1}",tv.Start,tv.End);
					dateProgramEnd=DateTime.MinValue;
					return 0;
				}
				if(programsInDatabase.Count==0)
				{
					int programID=TVDatabase.AddProgram(tv);
					//TVDatabase.RemoveOverlappingPrograms();
					if(programID!=-1)
					{
						retVal= 1;
					}

				}else
					retVal=0;

				return retVal;
			}
			catch(Exception ex)
			{
				Log.Write("epg-grab: FAILED to add to database. message:{0} stack:{1} source:{2}",ex.Message,ex.StackTrace,ex.Source);
				dateProgramEnd=DateTime.MinValue;
				return 0;
			}
		}
		public string ChannelName
		{
			get{return m_channelName;}
			set{m_channelName=value;}
		}
		//
		// returns long-value from sep. date
		//
		private long GetLongFromDate(int year,int mon,int day,int hour,int min,int sec)
		{
			
			string longStringA=String.Format("{0:0000}{1:00}{2:00}",year,mon,day);
			string longStringB=String.Format("{0:00}{1:00}{2:00}",hour,min,sec);
			//Log.Write("epg-grab: string-value={0}",longStringA+longStringB);
			return (long)Convert.ToUInt64(longStringA+longStringB);
		}
		//
		//
		//

		public int GetEPG(DShowNET.IBaseFilter filter,int serviceID)
		{
			// there must be an ts (card tuned) to get eit
			// if serviceID!=0 only those services are grabbed
			// else all epg for all services found on act. ts will go to database

			if(m_cardType==(int)EPGCard.Invalid || m_cardType==(int)EPGCard.Unknown)
				return 0;

			int			eventsCount=0;
			ArrayList	eitList=new ArrayList();
			ArrayList	tableList=new ArrayList();
			int			lastTab=0;
			int			dummyTab=0;

			if (m_cardType==(int)EPGCard.BDACards)
				m_sections.Timeout=500;
			else
				m_sections.Timeout=750;
			Log.Write("epg-grab: grabbing table {0}",80);
			eitList=m_sections.GetEITSchedule(0x50,filter,ref lastTab);
			tableList.Add(eitList);
			
			if(lastTab>0x5F)
				lastTab=0x50;

			if(lastTab>0x50)
			{
				for(int tab=0x51;tab<lastTab;tab++)
				{
					Log.Write("epg-grab: grabbing table {0}",tab);
					eitList.Clear();
					eitList=m_sections.GetEITSchedule(tab,filter,ref dummyTab);
					if(eitList.Count>0)
						tableList.Add(eitList);
				}
			}
			//
			int n=0;
			foreach(ArrayList eitData in tableList)
				foreach(DVBSections.EITDescr eit in eitData)
				{
					// the progName must be get from the database
					// to submitt to correct channel
					string progName="";
				
					switch(m_cardType)
					{
						case (int)EPGCard.TechnisatStarCards:
							progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id);
							Log.Write("epg-grab: counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss);
							break;

						case (int)EPGCard.BDACards:
						{
							ArrayList channels = new ArrayList();
							TVDatabase.GetChannels(ref channels);
							int freq, symbolrate,innerFec,modulation, ONID, TSID, SID;
							int audioPid, videoPid, teletextPid, pmtPid,bandWidth;
							int audio1, audio2, audio3, ac3Pid;
							string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;

							string provider="";
							foreach (TVChannel chan in channels)
							{
								switch (m_networkType)
								{
									case NetworkType.DVBC:
										TVDatabase.GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
										if (eit.program_number==SID && eit.ts_id==TSID)
										{
											progName=chan.Name;
											Log.Write("epg-grab: DVBC counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
										}
										break;
									case NetworkType.DVBS:
										progName=TVDatabase.GetSatChannelName(eit.program_number,eit.ts_id);
										break;
									case NetworkType.DVBT:
										TVDatabase.GetDVBTTuneRequest(chan.ID,out provider,out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
										if (eit.program_number==SID && eit.ts_id==TSID)
										{
											Log.Write("epg-grab: DVBT counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
											progName=chan.Name;
										}
										break;
								}
								if (progName!=String.Empty) break;
							}//foreach (TVChannel chan in channels)
						}
							break;

						case (int)EPGCard.ChannelName:
							progName=m_channelName;
							break;
					}
					if(progName==null)
					{
						Log.Write("epg-grab: FAILED name is NULL");
						continue;
					}
				
					if(progName=="")
					{
						Log.Write("epg-grab: FAILED empty name service-id:{0}",eit.program_number);
						continue;
					}
					DVBSections.EITDescr eit2DB=new MediaPortal.TV.Recording.DVBSections.EITDescr();
					eit2DB=eit;
					if(m_languagesToGrab!="")
					{
						eit2DB.extendedEventUseable=false;
						eit2DB.shortEventUseable=false;
					}
					else
					{
						eit2DB.extendedEventUseable=true;
						eit2DB.shortEventUseable=true;
					}

					if(m_languagesToGrab!="")
					{
						string[] langs=m_languagesToGrab.Split(new char[]{'/'});
						foreach(string lang in langs)
						{
							if(lang=="")
								continue;
							Log.Write("epg-grabbing: language selected={0}",lang);
							string codeEE="";
							string codeSE="";

							string eitItem=eit.event_item_text;
							if(eitItem==null)
								eitItem="";

							if(eit.eeLanguageCode!=null)
							{
								Log.Write("epg-grabbing: e-event-lang={0}",eit.eeLanguageCode);
								codeEE=eit.eeLanguageCode.ToLower();
								if(codeEE.Length==3)
								{
									if(lang.ToLower().Equals(codeEE))
									{
										eit2DB.extendedEventUseable=true;
										break;
									}
								}
							}

							if(eit.seLanguageCode!=null)
							{
								Log.Write("epg-grabbing: s-event-lang={0}",eit.seLanguageCode);
								codeSE=eit.seLanguageCode.ToLower();
								if(codeSE.Length==3)
								{
									if(lang.ToLower().Equals(codeSE))
									{
										eit2DB.shortEventUseable=true;
										break;
									}

								}

							}


						}
					}

					DateTime dateProgramEnd;
					if(serviceID!=0)
					{
						if(eit.program_number==serviceID)
							eventsCount+=SetEITToDatabase(eit2DB,progName,0x50, out dateProgramEnd);
					}
					else
						eventsCount+=SetEITToDatabase(eit2DB,progName,0x50, out dateProgramEnd);
					n++;
				}
		
			GC.Collect();
			return 	eventsCount;

		}//public int GetEPG(DShowNET.IBaseFilter filter,int serviceID)
		//
		public int GetEPG(ArrayList epgData,int serviceID, out DateTime reGrabTime)
		{
			reGrabTime=DateTime.MinValue;
			if(m_cardType==(int)EPGCard.Invalid || m_cardType==(int)EPGCard.Unknown)
				return 0;

			int			eventsCount=0;
			ArrayList	eitList=new ArrayList();
			ArrayList	tableList=new ArrayList();
			DVBSections tmpSections=new DVBSections();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);

			DateTime[] reGrabTimes = new DateTime[channels.Count];
			for (int i=0; i < reGrabTimes.Length;++i)
			{
				reGrabTimes[i] = new DateTime();
				reGrabTimes[i] = DateTime.MinValue;
			}
			eitList=tmpSections.GetEITSchedule(epgData);

			TVDatabase.RemoveOldPrograms();

			int n=0;
			if(eitList==null)
				return 0;

			foreach(DVBSections.EITDescr eit in eitList)
			{

				// the progName must be get from the database
				// to submitt to correct channel
				string progName="";
				
				switch(m_cardType)
				{
					case (int)EPGCard.TechnisatStarCards:
						progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id);
						//Log.Write("epg-grab: counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss);
						break;

					case (int)EPGCard.BDACards:
					{
						int freq, symbolrate,innerFec,modulation, ONID, TSID, SID;
						int audioPid, videoPid, teletextPid, pmtPid,bandWidth;
						int audio1, audio2, audio3, ac3Pid;
						string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;

						string provider="";
						foreach (TVChannel chan in channels)
						{
							switch (m_networkType)
							{
								case NetworkType.DVBC:
									TVDatabase.GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
									if (eit.program_number==SID && eit.ts_id==TSID)
									{
										progName=chan.Name;
										Log.Write("epg-grab: DVBC counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
									}
									break;
								case NetworkType.DVBS:
									progName=TVDatabase.GetSatChannelName(eit.program_number,eit.ts_id);
									break;
								case NetworkType.DVBT:
									TVDatabase.GetDVBTTuneRequest(chan.ID,out provider,out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
									if (eit.program_number==SID && eit.ts_id==TSID)
									{
										Log.Write("epg-grab: DVBT counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
										progName=chan.Name;
									}
									break;
							}
							if (progName!=String.Empty) break;
						}//foreach (TVChannel chan in channels)
					}
						break;

					case (int)EPGCard.ChannelName:
						progName=m_channelName;
						break;
				}
				if(progName==null)
				{
					//Log.Write("epg-grab: FAILED name is NULL");
					continue;
				}

				if(progName=="")
				{
					//Log.Write("epg-grab: FAILED empty name service-id:{0}",eit.program_number);
					continue;
				}
				DVBSections.EITDescr eit2DB=new MediaPortal.TV.Recording.DVBSections.EITDescr();
				eit2DB=eit;
				if(m_languagesToGrab!="")
				{
					eit2DB.extendedEventUseable=false;
					eit2DB.shortEventUseable=false;
				}
				else
				{
					eit2DB.extendedEventUseable=true;
					eit2DB.shortEventUseable=true;
				}

				if(m_languagesToGrab!="")
				{
					string[] langs=m_languagesToGrab.Split(new char[]{'/'});
					foreach(string lang in langs)
					{
						if(lang=="")
							continue;
						//Log.Write("epg-grabbing: language selected={0}",lang);
						string codeEE="";
						string codeSE="";

						string eitItem=eit.event_item_text;
						if(eitItem==null)
							eitItem="";

						if(eit.eeLanguageCode!=null)
						{
							//Log.Write("epg-grabbing: e-event-lang={0}",eit.eeLanguageCode);
							codeEE=eit.eeLanguageCode.ToLower();
							if(codeEE.Length==3)
							{
								if(lang.ToLower().Equals(codeEE))
								{
									eit2DB.extendedEventUseable=true;
									break;
								}
							}
						}

						if(eit.seLanguageCode!=null)
						{
							//Log.Write("epg-grabbing: s-event-lang={0}",eit.seLanguageCode);
							codeSE=eit.seLanguageCode.ToLower();
							if(codeSE.Length==3)
							{
								if(lang.ToLower().Equals(codeSE))
								{
									eit2DB.shortEventUseable=true;
									break;
								}

							}

						}


					}
				}

				DateTime dateProgramEnd=DateTime.MinValue;
				if(serviceID!=0)
				{
					if(eit.program_number==serviceID)
						eventsCount+=SetEITToDatabase(eit2DB,progName,0x50, out dateProgramEnd);
				}
				else
					eventsCount+=SetEITToDatabase(eit2DB,progName,0x50, out dateProgramEnd);

				if (dateProgramEnd!=DateTime.MinValue)
				{
					for (int i=0; i < channels.Count;++i)
					{
						TVChannel chan = (TVChannel)channels[i];
						if (chan.Name.Equals(progName))
						{
							if (dateProgramEnd > reGrabTimes[i])
							{
								reGrabTimes[i]=dateProgramEnd;
							}
							break;
						}
					}
				}
				n++;
			}
			
			GC.Collect();
			for (int i=0; i < reGrabTimes.Length;++i)
			{
				TVChannel ch=(TVChannel)channels[i];
				if (reGrabTimes[i]!=DateTime.MinValue)
				{
					if (reGrabTimes[i] < reGrabTime || reGrabTime==DateTime.MinValue)
						reGrabTime = reGrabTimes[i];
				}
			}
			return 	eventsCount;

		}//public int GetEPG(DShowNET.IBaseFilter filter,int serviceID)

		//
		//
		public void ParseChannels(byte[] data)
		{
			
			if(m_namesBuffer==null)
				return; // error
			if(m_namesBuffer.Count>0)
				return; // already got channles table

			int dataLen=data.Length;
			Log.Write("mhw-epg: start parse channels for mhw",m_namesBuffer.Count);
			lock(m_namesBuffer.SyncRoot)
			{
				for(int n=4;n<dataLen;n+=22)
				{
					if(m_namesBuffer.Count>=((dataLen-3)/22))
						break;
					MHWChannel ch=new MHWChannel();
					ch.NetworkID=(data[n]<<8)+data[n+1];
					ch.TransponderID=(data[n+2]<<8)+data[n+3];
					ch.ChannelID=(data[n+4]<<8)+data[n+5];
					ch.ChannelName=System.Text.Encoding.ASCII.GetString(data,n+6,16);
					ch.ChannelName=ch.ChannelName.Trim();
					m_namesBuffer.Add(ch);
					Log.Write("mhw-epg: added channel {0} to mhw channels table",ch.ChannelName);
				}// for(int n=0
				//Log.Write("mhw-epg: found {0} channels for mhw",m_namesBuffer.Count);
				m_mhwChannelsCount=m_namesBuffer.Count;
			}
		}

		public void ParseThemes(byte[] data)
		{
			
			if(m_themeBuffer==null)
				return; // error
			if(m_themeBuffer.Count>0)
				return; // already got channles table

			int dataLen=data.Length;
			lock(m_themeBuffer.SyncRoot)
			{
				try
				{
					int themesIndex = 3;
					int themesNames = 19;
					int theme=0;			
					int val=0;
					int count = (dataLen-19)/15;
					for (int i=0; i<count; i++)
					{
						if (data[themesIndex+theme] == i)	/* New theme */
						{
							val = (val+15) & 0xF0;
							theme++;
						}
						MHWTheme th=new MHWTheme();
						th.ThemeText=System.Text.Encoding.ASCII.GetString(data,themesNames,15);
						th.ThemeText=th.ThemeText.Trim();
						th.ThemeIndex=val;
						m_themeBuffer.Add(th);
						Log.Write("mhw-epg: theme '{0}' with id 0x{1:X} found",th.ThemeText,th.ThemeIndex);
						val++;
						themesNames+=15;
					}
				}
				catch{}
			}// lock
		}
		//
		//
		//

		public void ParseSummaries(byte[] data)
		{
			
			if(m_summaryBuffer==null)
				return;
		
			lock(m_summaryBuffer.SyncRoot)
			{
				int dataLen=((data[1]-0x70)<<8)+data[2];
				int n=0;
				Summary sum=new Summary();
				sum.ProgramID=(data[n+3]<<24)+(data[n+4]<<16)+(data[n+5]<<8)+data[n+6];
				sum.Description="";
				n+=11+(data[n+10]*7);
				sum.Description=System.Text.Encoding.ASCII.GetString(data,n,dataLen-n);
				if(SummaryExists(sum.ProgramID)==false && sum.ProgramID!=-1)
				{
					m_summaryBuffer.Add(sum);
				}//if(m_summaryBuffer.Contains(sum)==false)
			}
		}
		public void ParseTitles(byte[] data)
		{
			//foreach(byte[] data in m_progtabBuffer)

			lock(m_titleBuffer.SyncRoot)
			{

				Programm prg=new Programm();
				if(data[3]==0xff)
					return;
				prg.ChannelID=(data[3])-1;
				prg.ThemeID=data[4];
				int h=data[5] & 0x1F;
				int d=(data[5] & 0xE0)>>5;
				prg.Summaries=(data[6] & 0x80)==0?false:true;
				int m=data[6] >>2;
				prg.Duration=((data[9]<<8)+data[10]);// minutes
				prg.Title=System.Text.Encoding.ASCII.GetString(data,11,23);
				prg.Title=prg.Title.Trim();
				prg.PPV=(data[34]<<24)+(data[35]<<16)+(data[36]<<8)+data[37];
				prg.ID=(data[38]<<24)+(data[39]<<16)+(data[40]<<8)+data[41];
				// get time
				int d1=d;
				int h1=h;
				if (d1 == 7)
					d1 = 0;
				if (h1>15)
					h1 = h1-4;
				else if (h1>7)
					h1 = h1-2;
				else
					d1= (d1==6) ? 0 : d1+1;

				prg.Time=new DateTime(System.DateTime.Now.Ticks);
				DateTime dayStart=new DateTime(System.DateTime.Now.Ticks);
				dayStart=dayStart.Subtract(new TimeSpan(1,dayStart.Hour,dayStart.Minute,dayStart.Second,dayStart.Millisecond));
				int day=(int)dayStart.DayOfWeek;
				
				prg.Time=dayStart;
				int minVal=(d1-day)*86400+h1*3600+m*60;
				if(minVal<21600)
					minVal+=604800;

				prg.Time=prg.Time.AddSeconds(minVal);
				if(prg.Time.Hour==18 && prg.Time.Minute==25 && prg.Time.Day==20)
				{
					//					int a=0;
				}
				if(ProgramExists(prg.ID)==false)
				{
					m_titleBuffer.Add(prg);
				}
				
			}
		}

		int SubmittMHW(out DateTime reGrabTime)
		{
			reGrabTime=new DateTime();
			int count=0;
			if(m_namesBuffer==null)
				return 0;
			if(m_namesBuffer.Count<1)
				return 0;

			ArrayList channels1 = new ArrayList();
			TVDatabase.GetChannels(ref channels1);
			DateTime[] reGrabTimes = new DateTime[channels1.Count];
			for (int i=0; i < reGrabTimes.Length;++i)
			{
				reGrabTimes[i] = new DateTime();
				reGrabTimes[i] = DateTime.MinValue;
			}
			TVDatabase.RemoveOldPrograms();

			lock(m_titleBuffer.SyncRoot)
			{
				Log.Write("mhw-epg: count of programms={0}",m_titleBuffer.Count);
				Log.Write("mhw-epg: buffer contains {0} summaries now",m_summaryBuffer.Count);
				ArrayList list=new ArrayList();
				foreach(Programm prg in m_titleBuffer)
				{
					DVBSections.EITDescr eit=new MediaPortal.TV.Recording.DVBSections.EITDescr();
					string channelName=String.Empty;
					if(prg.ChannelID>=m_namesBuffer.Count || prg.ChannelID<0)
					{
						list.Add(prg);
						continue;
					}
					int programID=((MHWChannel)m_namesBuffer[prg.ChannelID]).ChannelID;
					int tsID=((MHWChannel)m_namesBuffer[prg.ChannelID]).NetworkID;
					switch(m_cardType)
					{
						case (int)EPGCard.TechnisatStarCards:
							channelName=TVDatabase.GetSatChannelName(programID,tsID);
							break;

						case (int)EPGCard.BDACards:
						{
							ArrayList channels = new ArrayList();
							TVDatabase.GetChannels(ref channels);
							int freq, symbolrate,innerFec,modulation, ONID, TSID, SID;
							int audioPid, videoPid, teletextPid, pmtPid,bandWidth;
							int audio1, audio2, audio3, ac3Pid;
							string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;

							string provider="";
							foreach (TVChannel chan in channels)
							{
								switch (m_networkType)
								{
									case NetworkType.DVBC:
										TVDatabase.GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
										if (programID==SID && tsID==TSID)
										{
											channelName=chan.Name;
										}
										break;
									case NetworkType.DVBS:
										channelName=TVDatabase.GetSatChannelName(programID,tsID);
										break;
									case NetworkType.DVBT:
										TVDatabase.GetDVBTTuneRequest(chan.ID,out provider,out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3);
										if (programID==SID && tsID==TSID)
										{
											channelName=chan.Name;
										}
										break;
								}
								if (channelName!=String.Empty) break;
							}//foreach (TVChannel chan in channels)
						}
							break;

						case (int)EPGCard.ChannelName:
							break;
					}
					if(channelName=="")
					{
						list.Add(prg);
						//m_titleBuffer.Remove(prg);// remove if it is in database
						continue;
					}
					eit.event_name=prg.Title;
					eit.program_number=programID;
					eit.event_text=GetSummaryByPrgID(prg.ID);
					if(eit.event_text=="")
						eit.event_text=String.Format("0x{0:X}",prg.ID);
					eit.genere_text=GetThemeText(prg.ThemeID);
					eit.duration_mm=prg.Duration;
					eit.isMHWEvent=true;
					eit.shortEventUseable=true;
					eit.mhwStartTime=prg.Time;

					DateTime dateProgramEnd;
					int result=SetEITToDatabase(eit,channelName,0, out dateProgramEnd);
					if(result==1)
					{
						count++;
						list.Add(prg);
						//m_titleBuffer.Remove(prg);// remove if it is in database
					}
					if(result==-2)
						list.Add(prg);
					
					if (dateProgramEnd!=DateTime.MinValue)
					{
						for (int i=0; i < channels1.Count;++i)
						{
							TVChannel chan = (TVChannel)channels1[i];
							if (chan.Name.Equals(channelName))
							{
								if (dateProgramEnd > reGrabTimes[i])
								{
									reGrabTimes[i]=dateProgramEnd;
								}
								break;
							}
						}
					}
				}

			
				foreach(Programm prg in list)
				{
					m_titleBuffer.Remove(prg);
				}
				m_titleBuffer.TrimToSize();
				
				if(count>0)
				{
					Log.Write("mhw-epg: added {0} entries to database",m_addsToDatabase);
					Log.Write("mhw-epg: titles buffer contains {0} objects",m_titleBuffer.Count);
					Log.Write("mhw-epg: summaries buffer contains {0} objects",m_summaryBuffer.Count);
				}
				//m_titleBuffer.Clear();
				for (int i=0; i < reGrabTimes.Length;++i)
				{
					TVChannel ch=(TVChannel)channels1[i];
					if (reGrabTimes[i]!=DateTime.MinValue)
					{
						if (reGrabTimes[i] < reGrabTime || reGrabTime==DateTime.MinValue)
							reGrabTime = reGrabTimes[i];
					}
				}

				return count;
			}

		}
		//
		string GetSummaryByPrgID(int id)
		{
			if(m_summaryBuffer==null)
				return "";
			if(m_summaryBuffer.Count<1)
				return "";
			lock(m_summaryBuffer.SyncRoot)
			{
				foreach(Summary sum in m_summaryBuffer)
				{
					if(sum.ProgramID==id)
						return sum.Description;
				}
			}
			return "";
		}
		string GetThemeText(int themeID)
		{
			if(m_themeBuffer==null)
				return "unknown";
			if(m_themeBuffer.Count<1)
				return "unknown";
			lock(m_themeBuffer.SyncRoot)
			{
				foreach(MHWTheme th in m_themeBuffer)
				{
					if(th.ThemeIndex==themeID)
						return th.ThemeText;
				}
				return "unknown";
			}
		}
		bool ProgramExists(int prgID)
		{
			lock(m_titleBuffer.SyncRoot)
			{
				foreach(Programm prg in m_titleBuffer)
				{
					if(prg.ID==prgID)
						return true;
				}
			}
			return false;
		}
		bool SummaryExists(int prgID)
		{
			lock(m_summaryBuffer.SyncRoot)
			{
				foreach(Summary sum in m_summaryBuffer)
				{
					if(sum.ProgramID==prgID)
						return true;
				}
			}
			return false;
		}

	}// class
}// namespace
