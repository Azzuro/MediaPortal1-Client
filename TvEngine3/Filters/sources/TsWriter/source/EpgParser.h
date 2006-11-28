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
#pragma once
#include "sectiondecoder.h"
#include "isectioncallback.h"
#include "epgdecoder.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "TsHeader.h"
#include <vector>
using namespace std;

#define PID_EPG 0x12

class CEpgParser :  public ISectionCallback
{
public:
  CEpgParser(void);
  virtual ~CEpgParser(void);
  void  Reset();
	void GrabEPG();
	bool isGrabbing();
	bool	IsEPGReady();
	ULONG	GetEPGChannelCount( );
	ULONG	GetEPGEventCount( ULONG channel);
	void	GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	void	GetEPGEvent( ULONG channel,  ULONG event,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    );
	void    GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    );

	void	OnTsPacket(CTsHeader& header,byte* tsPacket);
	void  OnNewSection(int pid, int tableId, CSection& section); 


private:
  vector<CSectionDecoder*> m_vecDecoders;
	CEpgDecoder m_epgDecoder;
	bool				m_bGrabbing;
	CCriticalSection m_section;
  CTsHeader             m_tsHeader;
};
