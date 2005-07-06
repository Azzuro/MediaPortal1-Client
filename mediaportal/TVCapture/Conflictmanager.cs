using System;
using System.Threading;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
	public class ConflictManager
	{	
		static  int[] cards ;
		static  ArrayList recordings ;
		static  TVUtil		util =null;
		static  ArrayList conflictingRecordings=null;
		static  ConflictManager()
		{
			TVDatabase.OnRecordingsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnRecordingsChanged);
		}
		static bool AllocateCard(string ChannelName)
		{
			int cardNo=-1;
			int minRecs=Int32.MaxValue;
			for (int i=0; i < cards.Length;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if ( !dev.UseForRecording) continue;
				if ( cards.Length>1)
				{
					if(!TVDatabase.CanCardViewTVChannel(ChannelName,dev.ID)) continue;
				}
				if (cards[i]==0) 
				{
					cardNo=i;
					break;
				}
				if (cards[i] < minRecs)
				{
					minRecs=cards[i];
					cardNo=i;
				}
			}
			if (cardNo>=0)
			{
				cards[cardNo]++;
				//Log.Write("  card:{0} {1} {2}",cardNo,cards[cardNo], ChannelName);
				if (cards[cardNo]>1) return true;
			}
			return false;
		}

		static void FreeCards()
		{
			for (int i=0; i < cards.Length;++i)
				cards[i]=0;
		}

		static void Initialize()
		{
			util = new TVUtil(14);
			recordings = new ArrayList();
			conflictingRecordings=new ArrayList();
			TVDatabase.GetRecordings(ref recordings);
			Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));
			WorkerThread.Start();
		}

		static void WorkerThreadFunction()
		{
			System.Threading.Thread.CurrentThread.Priority=ThreadPriority.BelowNormal;
			DateTime dtStart=DateTime.Now;
			foreach (TVRecording rec in recordings)
			{
				DetermineIsConflict(rec);
			}
			TimeSpan ts=DateTime.Now-dtStart;
			Log.Write("Took:{0}:{1}", ts.Seconds,ts.Milliseconds);
		}

		static public bool IsConflict(TVRecording rec)
		{
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			foreach (TVRecording conflict in conflictingRecordings)
			{
				if (conflict.ID==rec.ID) return true;
			}
			return false;
		}
		static bool DetermineIsConflict(TVRecording rec)
		{
			if (Recorder.Count<=0) 
				return false;
			if (rec.Canceled>0 || rec.IsDone()) return false;
			
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			cards = new int[Recorder.Count];
			if (recordings.Count==0) return false;
			ArrayList episodes = util.GetRecordingTimes(rec);
			foreach (TVRecording episode in episodes)
			{
				if (episode.Canceled!=0) continue;
				FreeCards();
				AllocateCard(episode.Channel);
				foreach (TVRecording otherRecording in recordings)
				{
					ArrayList otherEpisodes = util.GetRecordingTimes(otherRecording);
					foreach ( TVRecording otherEpisode in otherEpisodes)
					{
						if (otherEpisode.Canceled!=0) continue;
						if (otherEpisode.ID==episode.ID && 
							otherEpisode.Start==episode.Start && 
							otherEpisode.End==episode.End) continue;
						// episode        s------------------------e
						// other    ---------s-----------------------------
						// other ------------------e
						if ( (otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
							   (otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End)     ||
							(otherEpisode.End > episode.Start && otherEpisode.End <= episode.End) )
						{
							if (AllocateCard(otherEpisode.Channel))
							{
								conflictingRecordings.Add(rec);
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		static public void GetConflictingSeries(TVRecording rec, ArrayList recSeries)
		{
			recSeries.Clear();
			if (Recorder.Count<=0) return ;
			
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			cards = new int[Recorder.Count];
			if (recordings.Count==0) return ;
			
			ArrayList episodes = util.GetRecordingTimes(rec);
			foreach (TVRecording episode in episodes)
			{
				if (episode.Canceled!=0) continue;

				bool epsiodeConflict=false;
				FreeCards();
				AllocateCard(episode.Channel);
				foreach (TVRecording otherRecording in recordings)
				{
					ArrayList otherEpisodes = util.GetRecordingTimes(otherRecording);
					foreach ( TVRecording otherEpisode in otherEpisodes)
					{
						if (otherEpisode.Canceled!=0) continue;
						if (otherEpisode.ID==episode.ID && 
							otherEpisode.Start==episode.Start && 
							otherEpisode.End==episode.End) continue;
						// episode        s------------------------e
						// other    ---------s-----------------------------
						// other ------------------e
						if ( (otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
							(otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End)     ||
							(otherEpisode.End > episode.Start && otherEpisode.End <= episode.End) )
						{
							if (AllocateCard(otherEpisode.Channel))
							{
								epsiodeConflict=true;
								break;
							}
						}
					}
					if (epsiodeConflict) break;
				}
				if (epsiodeConflict)
				{
					recSeries.Add(episode);
				}
			}
		}

		static public TVRecording[] GetConflictingRecordings(TVRecording episode)
		{
			if (Recorder.Count<=0) return null;
			
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			cards = new int[Recorder.Count];
			if (recordings.Count==0) return null;
			
			ArrayList conflicts = new ArrayList();
			if (episode.Canceled!=0) return null;
			
			foreach (TVRecording otherRecording in recordings)
			{
				ArrayList otherEpisodes = util.GetRecordingTimes(otherRecording);
				foreach ( TVRecording otherEpisode in otherEpisodes)
				{
					if (otherEpisode.Canceled!=0) continue;
					if (otherEpisode.ID==episode.ID && 
						otherEpisode.Start==episode.Start && 
						otherEpisode.End==episode.End) continue;
					// episode        s------------------------e
					// other    ---------s-----------------------------
					// other ------------------e
					if ( (otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
						(otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End)     ||
						(otherEpisode.End > episode.Start && otherEpisode.End <= episode.End) )
					{
						conflicts.Add(otherEpisode);
					}
				}
			}
			TVRecording[] conflictingRecordings = new TVRecording[conflicts.Count];
			for (int i=0; i < conflicts.Count;++i)
				conflictingRecordings[i] = (TVRecording)conflicts[i];
			return conflictingRecordings;
		}

		static public TVUtil Util
		{
			get { 
				if (util==null) 
					Initialize();
				return util;
				}
		}

		static private void TVDatabase_OnRecordingsChanged()
		{
			Initialize();
		}
	}
}