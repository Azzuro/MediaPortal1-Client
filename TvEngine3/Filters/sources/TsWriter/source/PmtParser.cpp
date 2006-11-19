/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "PmtParser.h"
#include "channelinfo.h"
#include "tsheader.h"

void LogDebug(const char *fmt, ...) ; 

CPmtParser::CPmtParser()
{
	m_pmtCallback=NULL;
	_isFound=false;
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::SetPmtCallBack(IPmtCallBack* callback)
{
	m_pmtCallback=callback;
}

bool CPmtParser::IsReady()
{
  return _isFound;
}
void CPmtParser::OnNewSection(CSection& sections)
{ 
  byte* section=(&sections.Data)[0];
  int sectionLen=sections.SectionLength;

  CTsHeader header(section);
  int start=header.PayLoadStart;
  int table_id = section[start+0];
	if (table_id!=2) return;
  int section_syntax_indicator = (section[start+1]>>7) & 1;
  int section_length = ((section[start+1]& 0xF)<<8) + section[start+2];
  int program_number = (section[start+3]<<8)+section[start+4];
  int version_number = ((section[start+5]>>1)&0x1F);
  int current_next_indicator = section[start+5] & 1;
  int section_number = section[start+6];
  int last_section_number = section[start+7];
  int pcr_pid=((section[start+8]& 0x1F)<<8)+section[start+9];
  int program_info_length = ((section[start+10] & 0xF)<<8)+section[start+11];
  int len2 = program_info_length;
  int pointer = 12;
  int len1 = section_length -( 9 + program_info_length +4);
  int x;
	if (!_isFound)
	{
		//LogDebug("got pmt:%x service id:%x", GetPid(), program_number);
		_isFound=true;	
		if (m_pmtCallback!=NULL)
		{
			m_pmtCallback->OnPmtReceived(GetPid());
		}
	}
  CPidTable pidInfo;
  // loop 1
  while (len2 > 0)
  {
	  int indicator=section[start+pointer];
	  int descriptorLen=section[start+pointer+1];
	  len2 -= (descriptorLen+2);
	  pointer += (descriptorLen+2);
  }
  // loop 2
  int stream_type=0;
  int elementary_PID=0;
  int ES_info_length=0;
  int audioToSet=0;


  pidInfo.Reset();
  pidInfo.PmtPid=GetPid();
  pidInfo.ServiceId=program_number;
  while (len1 > 0)
  {
	  //if (start+pointer+4>=sectionLen+9) return ;
	  stream_type = section[start+pointer];
	  elementary_PID = ((section[start+pointer+1]&0x1F)<<8)+section[start+pointer+2];
	  ES_info_length = ((section[start+pointer+3] & 0xF)<<8)+section[start+pointer+4];
   // LogDebug("pmt: pid:%x type:%x",elementary_PID, stream_type);
		if(stream_type==SERVICE_TYPE_VIDEO_MPEG1 || stream_type==SERVICE_TYPE_VIDEO_MPEG2)
	  {
			//mpeg2 video
		  if(pidInfo.VideoPid==0)
			{
				pidInfo.VideoPid=elementary_PID;
				pidInfo.videoServiceType=stream_type;
			}
	  }
		if(stream_type==SERVICE_TYPE_VIDEO_MPEG4 || stream_type==SERVICE_TYPE_VIDEO_H264)
	  {
			//h.264/mpeg4 video
		  if(pidInfo.VideoPid==0)
			{
			  pidInfo.VideoPid=elementary_PID;
				pidInfo.videoServiceType=stream_type;
			}
	  }
		if(stream_type==SERVICE_TYPE_AUDIO_MPEG1 || stream_type==SERVICE_TYPE_AUDIO_MPEG2)
	  {
			//mpeg 2 audio
		  audioToSet=0;
		  if(pidInfo.AudioPid1==0)
		  {
			  audioToSet=1;
			  pidInfo.AudioPid1=elementary_PID;
		  }
		  else
		  {
			  if(pidInfo.AudioPid2==0)
			  {
				  audioToSet=2;
				  pidInfo.AudioPid2=elementary_PID;
			  }
			  else
			  {
				  if(pidInfo.AudioPid3==0)
				  {
					  audioToSet=3;
					  pidInfo.AudioPid3=elementary_PID;
				  }
			  }
		  }
	  }
	  pidInfo.PcrPid=pcr_pid;

		if(stream_type==SERVICE_TYPE_AUDIO_AC3)
	  {
			//ac3 audio
		  if(pidInfo.AC3Pid==0)
			  pidInfo.AC3Pid=elementary_PID;
	  }
	  pointer += 5;
	  len1 -= 5;
	  len2 = ES_info_length;
	  while (len2 > 0)
	  {
		  if (pointer+1>=sectionLen) 
			{
				LogDebug("pmt parser check1");
				return ;
			}
		  x = 0;
		  int indicator=section[start+pointer];
		  x = section[start+pointer + 1] + 2;
		  if(indicator==DESCRIPTOR_DVB_AC3)
			{
			  pidInfo.AC3Pid=elementary_PID;
			}
		  if(indicator==DESCRIPTOR_MPEG_ISO639_Lang)
		  {	
			  if (pointer+4>=sectionLen) 
			{
				LogDebug("pmt parser check2");
				return ;
			}
			  BYTE d[3];
			  d[0]=section[start+pointer+2];
			  d[1]=section[start+pointer+3];
			  d[2]=section[start+pointer+4];
			  if(audioToSet==1)
			  {
				  pidInfo.Lang1_1=d[0];
				  pidInfo.Lang1_2=d[1];
				  pidInfo.Lang1_3=d[2];
			  }
			  if(audioToSet==2)
			  {
				  pidInfo.Lang2_1=d[0];
				  pidInfo.Lang2_2=d[1];
				  pidInfo.Lang2_3=d[2];
			  }
			  if(audioToSet==3)
			  {
				  pidInfo.Lang3_1=d[0];
				  pidInfo.Lang3_2=d[1];
				  pidInfo.Lang3_3=d[2];
			  }

		  }
		  if(indicator==DESCRIPTOR_DVB_TELETEXT && pidInfo.TeletextPid==0)
			  pidInfo.TeletextPid=elementary_PID;

			if(indicator==DESCRIPTOR_DVB_SUBTITLING && pidInfo.SubtitlePid==0)
			{
				if (stream_type==SERVICE_TYPE_DVB_SUBTITLES2)
				{
					BYTE d[3];
					d[0]=section[start+pointer+2];
					d[1]=section[start+pointer+3];
					d[2]=section[start+pointer+4];
					pidInfo.SubtitlePid=elementary_PID;
					pidInfo.SubLang1_1=d[0];
					pidInfo.SubLang1_2=d[1];
					pidInfo.SubLang1_3=d[2];
				}
			}
		  len2 -= x;
		  len1 -= x;
		  pointer += x;
	  }
	  //LogDebug("DecodePMT pid:0x%x pcrpid:0x%x videopid:0x%x audiopid:0x%x ac3pid:0x%x sid:%x",
		//  pidInfo.PmtPid, pidInfo.PcrPid,pidInfo.VideoPid,pidInfo.AudioPid1,pidInfo.AC3Pid,pidInfo.ServiceId);
    if (m_pmtCallback!=NULL)
    {
      m_pmtCallback->OnPidsReceived(pidInfo);
    }
  }
}
