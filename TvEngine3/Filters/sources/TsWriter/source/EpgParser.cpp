#include <windows.h>
#include "EpgParser.h"

extern void LogDebug(const char *fmt, ...) ;
CEpgParser::CEpgParser(void)
{
	CEnterCriticalSection enter(m_section);
	m_bGrabbing=false;
  for (int i=0x4e; i <=0x6f;++i)
  {
    CSectionDecoder* pDecoder= new CSectionDecoder();
    pDecoder->SetPid(0x12);
    pDecoder->SetTableId(i);
    m_vecDecoders.push_back(pDecoder);
		pDecoder->SetCallBack(this);
  }
}

CEpgParser::~CEpgParser(void)
{
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		delete pDecoder;
	}
	m_vecDecoders.clear();
}

void CEpgParser::Reset()
{
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->Reset();
	}
	m_bGrabbing=false;
	m_epgDecoder.ResetEPG();
}
void CEpgParser::GrabEPG()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("epg:GrabEPG");
	Reset();
	m_bGrabbing=true;
	m_epgDecoder.GrabEPG();
}
bool CEpgParser::isGrabbing()
{
	CEnterCriticalSection enter(m_section);
	m_bGrabbing= m_epgDecoder.IsEPGGrabbing();
	return m_bGrabbing;
}
bool	CEpgParser::IsEPGReady()
{
	CEnterCriticalSection enter(m_section);
	bool result= m_epgDecoder.IsEPGReady();
	if (result)
	{
		m_bGrabbing=false;
	}
	return result;
}
ULONG	CEpgParser::GetEPGChannelCount( )
{
	CEnterCriticalSection enter(m_section);
	return m_epgDecoder.GetEPGChannelCount();
}
ULONG	CEpgParser::GetEPGEventCount( ULONG channel)
{
	CEnterCriticalSection enter(m_section);
	return m_epgDecoder.GetEPGEventCount(channel);
}
void	CEpgParser::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGChannel(  channel,  networkId,  transportid, service_id  );
}
void	CEpgParser::GetEPGEvent( ULONG channel,  ULONG levent,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGEvent(  channel, levent,language, dateMJD, timeUTC, duration, strgenre    );
}
void  CEpgParser::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGLanguage(channel, eventid,languageIndex,language, eventText, eventDescription    );
}


void CEpgParser::OnTsPacket(byte* tsPacket)
{
	if (m_bGrabbing==false) return;
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(tsPacket);
	}
}

void CEpgParser::OnNewSection(int pid, int tableId, CSection& sections)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("epg new section pid:%x tableid:%x onid:%x sid:%x len:%x",pid,tableId,sections.NetworkId,sections.TransportId,sections.SectionLength);
		byte* section=&(sections.Data[5]);
		int sectionLength=sections.SectionLength;
		if (sectionLength>0)
		{
			m_epgDecoder.DecodeEPG(section,	sectionLength);
		}
	}
	catch(...)
	{
		LogDebug("exception in CEpgParser::OnNewSection");
	}
}