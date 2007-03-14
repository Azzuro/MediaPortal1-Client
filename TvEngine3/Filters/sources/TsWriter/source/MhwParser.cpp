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
#include <time.h>
#include "MhwParser.h"


extern void LogDebug(const char *fmt, ...) ;
CMhwParser::CMhwParser(void)
{
  LogDebug("mhw ctor");
	CEnterCriticalSection enter(m_section);
	m_bGrabbing=false;
	m_bDone=false;

  CSectionDecoder* pDecoder= new CSectionDecoder();
  pDecoder->SetPid(PID_MHW1);
  pDecoder->SetTableId(0x90);
	pDecoder->SetCallBack(this);
  pDecoder->EnableCrcCheck(false);
 // pDecoder->EnableLogging(true);
  m_vecDecoders.push_back(pDecoder);

  for (int i=0x70; i <=0x7f;++i)
  {
    pDecoder= new CSectionDecoder();
    pDecoder->SetPid(PID_MHW1);
    pDecoder->SetTableId(i);
		pDecoder->SetCallBack(this);
    pDecoder->EnableCrcCheck(false);
    m_vecDecoders.push_back(pDecoder);
  }
  for (int i=0x90; i <=0x92;++i)
  {
    pDecoder= new CSectionDecoder();
    pDecoder->SetPid(PID_MHW2);
    pDecoder->SetTableId(i);
		pDecoder->SetCallBack(this);
    pDecoder->EnableCrcCheck(false);
    m_vecDecoders.push_back(pDecoder);
  }
}

CMhwParser::~CMhwParser(void)
{
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		delete pDecoder;
	}
	m_vecDecoders.clear();
}

void CMhwParser::Reset()
{
  LogDebug("mhw reset");
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->Reset();
	}
	m_bGrabbing=false;
	m_bDone=false;
	m_mhwDecoder.Reset();
	m_TimeOutTimer=time(NULL);
}


void CMhwParser::OnTsPacket(CTsHeader& header,byte* tsPacket)
{
	if (m_bGrabbing==false) return;
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (pid!=PID_MHW1 && pid!=PID_MHW2) return;

	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(header,tsPacket);
	}
}

void CMhwParser::OnNewSection(int pid, int tableId, CSection& sections)
{
	try
	{
		CEnterCriticalSection enter(m_section);
//		LogDebug("mhw new section pid:%x tableid:%x %x %x len:%d",pid,tableId,sections.Data[4],sections.Data[5],sections.Data[6], sections.SectionLength);
		m_tsHeader.Decode(&sections.Data[0]);
		if (m_tsHeader.PayLoadStart< 0 || m_tsHeader.PayLoadStart>188) return;

		byte* section=&(sections.Data[m_tsHeader.PayLoadStart]);
		int table_id = section[0];

		int sectionLength=sections.SectionLength;
		if (pid==0xd2)
		{
			if (table_id==0x90 ||(table_id >=0x70 && table_id <=0x7f) )
			{
//				LogDebug("mhw ParseTitles %d",sectionLength);
				if ( m_mhwDecoder.ParseTitles(section,sectionLength))
				{
					m_TimeOutTimer=time(NULL);
				}
//				LogDebug("mhw ParseTitles done");
			}
		}
		if (pid==0xd3)
		{
			if (table_id==0x90)
			{
//				LogDebug("mhw ParseSummaries %d",sectionLength);
				if (m_mhwDecoder.ParseSummaries(section,sectionLength))
				{
					m_TimeOutTimer=time(NULL);
				}
//				LogDebug("mhw ParseSummaries done");
			}
			if (table_id==0x91)
			{
//				LogDebug("mhw ParseChannels %d",sectionLength);
				if (m_mhwDecoder.ParseChannels(section,sectionLength))
				{
					m_TimeOutTimer=time(NULL);
				}
//				LogDebug("mhw ParseChannels done");
			}
			if (table_id==0x92)
			{
//				LogDebug("mhw ParseThemes %d",sectionLength);
				if (m_mhwDecoder.ParseThemes(section,sectionLength))
				{
					m_TimeOutTimer=time(NULL);
				}
//				LogDebug("mhw ParseThemes done");
			}
		}
				
		int passed=(int)(time(NULL)-m_TimeOutTimer);
		if (passed>60)
		{
			LogDebug("mhw grabber ended");
			m_bDone=true;
			m_bGrabbing=false;
		}
	}
	catch(...)
	{
		LogDebug("mhw on new section exception pid:%x tableid:%x",pid,tableId);
	}
}
void CMhwParser::GrabEPG()
{
  LogDebug("mhw grab");
	CEnterCriticalSection enter(m_section);
	Reset();
	m_bGrabbing=true;
	m_bDone=false;
	m_mhwDecoder.Reset();
	m_TimeOutTimer=time(NULL);
  LogDebug("mhw grabber started decoders:%d", m_vecDecoders.size());
}
bool CMhwParser::isGrabbing()
{
	CEnterCriticalSection enter(m_section);
	return m_bGrabbing;
}
bool	CMhwParser::IsEPGReady()
{
	CEnterCriticalSection enter(m_section);
	int passed=(int)(time(NULL)-m_TimeOutTimer);
  if (passed>60)
  {
    LogDebug("mhw grabber ended");
	  m_bDone=true;
	  m_bGrabbing=false;
  }
	return m_bDone;
}


void CMhwParser::GetTitleCount(WORD* count)
{
	CEnterCriticalSection enter(m_section);
	*count=m_mhwDecoder.GetTitleCount();
	LogDebug("CMhwParser:GetTitleCount:%d", (*count));
}
void CMhwParser::GetTitle(WORD program, UINT* id, WORD* transportId, WORD* networkId, WORD* channelId, UINT* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{
	CEnterCriticalSection enter(m_section);
	m_mhwDecoder.GetTitle(program, id, transportId, networkId, channelId, programId, themeId, PPV, Summaries, duration, dateStart, timeStart,title,programName);
}
void CMhwParser::GetChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName)
{
	CEnterCriticalSection enter(m_section);
	m_mhwDecoder.GetChannel(channelNr, channelId,  networkId, transportId, channelName);
}
void CMhwParser::GetSummary(UINT programId, char** summary)
{
	CEnterCriticalSection enter(m_section);
	m_mhwDecoder.GetSummary(programId, summary);
}
void CMhwParser::GetTheme(WORD themeId, char** theme)
{
	CEnterCriticalSection enter(m_section);
	m_mhwDecoder.GetTheme(themeId, theme);
}