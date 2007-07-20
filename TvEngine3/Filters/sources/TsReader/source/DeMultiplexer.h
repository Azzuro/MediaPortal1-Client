/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma once
#include "buffer.h"
#include "MultiFileReader.h"
#include "tsDuration.h"
#include "pcrdecoder.h"
#include "packetSync.h"
#include "pidtable.h"
#include "tsheader.h"
#include "patparser.h"
#include "channelInfo.h"
#include "PidTable.h"
#include "TSThread.h"
#include "Pcr.h"
#include <vector>
#include <map>
using namespace std;
class CTsReaderFilter;
class CDeMultiplexer : public CPacketSync, public TSThread, public IPatParserCallback
{
public:
  CDeMultiplexer( CTsDuration& duration,CTsReaderFilter& filter);
	virtual ~CDeMultiplexer(void);

  void       Start();
  void       Flush();
  CBuffer*   GetVideo();
  CBuffer*   GetAudio();
  CBuffer*   GetSubtitle();
	void       OnTsPacket(byte* tsPacket);
	void       OnNewChannel(CChannelInfo& info);
  void       SetFileReader(FileReader* reader);
  void       FillSubtitle(CTsHeader& header, byte* tsPacket);
  void       FillAudio(CTsHeader& header, byte* tsPacket);
  void       FillVideo(CTsHeader& header, byte* tsPacket);
  CPidTable  GetPidTable();
  void       SetAudioStream(int stream);
  int        GetAudioStream();
  void       GetAudioStreamInfo(int stream,char* szName);
  void       GetAudioStreamType(int stream,CMediaType&  pmt);
  void       GetVideoStreamType(CMediaType& pmt);
  int        GetAudioStreamCount();
  bool       EndOfFile();
	bool			 HoldAudio();
	void			 SetHoldAudio(bool onOff);
	bool			 HoldVideo();
	void			 SetHoldVideo(bool onOff);
  void       ThreadProc();
  void       FlushVideo();
  void       FlushAudio();
  int        GetVideoServiceType();
private:
  struct stAudioStream
  {
    int pid;
    int audioType;
    char language[4];
  } ;
  vector<struct stAudioStream> m_audioStreams;
  void GetVideoMedia(CMediaType *pmt);
  void GetH264Media(CMediaType *pmt);
  void GetMpeg4Media(CMediaType *pmt);
  bool ReadFromFile(bool isAudio, bool isVideo);
  bool m_bEndOfFile;
  HRESULT RenderFilterPin(CBasePin* pin, bool isAudio, bool isVideo);
  HRESULT DoStart();
  HRESULT DoStop();
  HRESULT IsStopped();
  HRESULT IsPlaying();
  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;
  CCritSec m_sectionSubtitle;
  CCritSec m_sectionRead;
	FileReader* m_reader;
  CPatParser m_patParser;
  CPidTable m_pids;
  vector<CBuffer*> m_vecSubtitleBuffers;
  vector<CBuffer*> m_vecVideoBuffers;
  vector<CBuffer*> m_vecAudioBuffers;
  typedef vector<CBuffer*>::iterator ivecBuffers;

  CBuffer* m_pCurrentSubtitleBuffer;
  CBuffer* m_pCurrentVideoBuffer;
  CBuffer* m_pCurrentAudioBuffer;
  CPcr     m_streamPcr;
  CTsDuration& m_duration;
  CTsReaderFilter& m_filter;
  int m_iAudioStream;
  int m_audioPid;
  bool m_bScanning;
	bool m_bHoldAudio;
	bool m_bHoldVideo;
};
