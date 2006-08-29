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
#include <windows.h>
#include "PatParser.h"
#include "tsheader.h"

void LogDebug(const char *fmt, ...) ;
CPatParser::CPatParser(void)
{
  Reset();
  SetTableId(0);
  SetPid(0);
}

CPatParser::~CPatParser(void)
{
  CleanUp();
}

void  CPatParser::CleanUp()
{
  for (int i=0; i < m_pmtParsers.size();++i)
  {
    CPmtParser* parser=m_pmtParsers[i];
    delete parser;
  }
  m_pmtParsers.clear();
}

void  CPatParser::Reset()
{
//	Dump();
	LogDebug("PatParser:Reset()");
	CSectionDecoder::Reset();
  CleanUp();
  m_vctParser.Reset();
  m_sdtParser.Reset();
	m_nitDecoder.Reset();
}
 
int CPatParser::Count()
{
	if (m_nitDecoder.Ready()==false) return 0;
  return m_pmtParsers.size();
}

bool CPatParser::GetChannel(int index, CChannelInfo& info)
{
	static CChannelInfo unknownChannel;
  if (index < 0 || index >Count()) 
	{
		LogDebug("CPatParser::GetChannel(%d) invalid index", index);
		return false;
	}

  CPmtParser* parser=m_pmtParsers[index];
	if (false==parser->Ready()) 
	{
		return false;
	}

  CPidTable& table=parser->GetPidInfo();
  if ( m_vctParser.Count() > 0) 
  {
		if (m_vctParser.GetChannelInfo(table.ServiceId,info))
		{
			info.PidTable = table;
			info.LCN=m_nitDecoder.GetLogicialChannelNumber(info.NetworkId,info.TransportId,info.ServiceId);
			return true;
		}
  }
	if (m_sdtParser.Count()>0)
  {
    if (m_sdtParser.GetChannelInfo(table.ServiceId,info))
		{
			info.PidTable = table;
			info.LCN=m_nitDecoder.GetLogicialChannelNumber(info.NetworkId,info.TransportId,info.ServiceId);
			return true;
		}
  }
/*
	sprintf(unknownChannel.ProviderName,"unknown");
	sprintf(unknownChannel.ServiceName,"%x", table.ServiceId);
	unknownChannel.PidTable=table;
	unknownChannel.ServiceType=0;
	unknownChannel.NetworkId=0xfff;
	unknownChannel.TransportId=0xfff;
	unknownChannel.ServiceId=table.ServiceId;
	info=unknownChannel;*/
	return false;
}

void CPatParser::OnTsPacket(byte* tsPacket)
{
	m_nitDecoder.OnTsPacket(tsPacket);
  m_vctParser.OnTsPacket(tsPacket);
  m_sdtParser.OnTsPacket(tsPacket);
  for (int i=0; i < m_pmtParsers.size();++i)
  {
    CPmtParser* parser=m_pmtParsers[i];
    parser->OnTsPacket(tsPacket);
  }
  CSectionDecoder::OnTsPacket(tsPacket);
}

void CPatParser::OnNewSection(CSection& sections)
{
  byte* section=sections.Data;

  CTsHeader header(section);
  int start=header.PayLoadStart+1;
  int table_id = section[start];
  if (table_id!=0) return ;
  int section_syntax_indicator = (section[start+1]>>7) & 1;
  int section_length = ((section[start+1]& 0xF)<<8) + section[start+2];
  int transport_stream_id = (section[start+3]<<8)+section[start+4];
  int version_number = ((section[start+5]>>1)&0x1F);
  int current_next_indicator = section[start+5] & 1;
  int section_number = section[start+6];
  int last_section_number = section[start+7];

//	  LogDebug("DecodePat  %d section:%d lastsection:%d sectionlen:%d",
//						  version_number,section_number,last_section_number,section_length);

  int pmtcount=0;
  int loop =(section_length - 9) / 4;
  for(int i=0; i < loop; i++)
  {
	  int offset = (8 +(i * 4));
	  int pmtPid = ((section[start+offset+2] & 0x1F)<<8) + section[start+offset+3];
	  if (pmtPid < 0x10 || pmtPid >=0x1fff) 
	  {
      //invalid pmt pid
		  return ;
	  }

	  bool found=false;
	  for (int idx=0; idx < m_pmtParsers.size(); idx++)
	  {
		  CPmtParser* pmtParser = m_pmtParsers[idx];
		  if (pmtParser->GetPid() == pmtPid)
		  {
			  found=true;
			  break;
		  }
	  }
	  if (!found && pmtPid>0x12)
	  {
		  CPmtParser* pmtParser = new CPmtParser();
		  pmtParser->SetTableId(2);
		  pmtParser->SetPid(pmtPid);
		  m_pmtParsers.push_back( pmtParser );
			LogDebug("  add pmt# %d pid: %x",m_pmtParsers.size(), pmtPid);
	  }
  }
}

void CPatParser::Dump()
{
  for (int i=0; i < Count();++i)
  {
    CChannelInfo info;
    if (GetChannel( i, info))
    {
      LogDebug("%d) onid:%x tsid:%x sid:%x major:%d minor:%x freq:%x type:%d provider:%s service:%s",i,
            info.NetworkId,info.TransportId,info.ServiceId,info.MajorChannel,info.MinorChannel,info.Frequency,info.ServiceType,info.ProviderName,info.ServiceName);
      LogDebug("  pcr:%x pmt:%x video:%x audio1:%x audio2:%x audio3:%x ac3:%x ttx:%x sub:%x",
            info.PidTable.PcrPid,info.PidTable.PmtPid,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AudioPid2,info.PidTable.AudioPid3,info.PidTable.AC3Pid,info.PidTable.TeletextPid,info.PidTable.SubtitlePid);
    }
    else
    {
      LogDebug("%d) not found",i);
    }
  }
}