/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *  Author: tourettes
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
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <initguid.h>

//#include "MPDVBSub.h"
//#include "proppage.h"
#include "SubtitleInputPin.h"

// Subtitle decoding 
#include "dvbsubs\dvbsubs.h"

extern void Log( const char *fmt, ... );

CSubtitleInputPin::CSubtitleInputPin( CSubTransform *pDump,
										LPUNKNOWN pUnk,
										CBaseFilter *pFilter,
										CCritSec *pLock,
										CCritSec *pReceiveLock,
										CDVBSubDecoder* pSubDecoder,
										HRESULT *phr ) :

    CRenderedInputPin(NAME( "CSubtitleInputPin" ),
					pFilter,						  // Filter
					pLock,							  // Locking
					phr,							    // Return code
					L"Subtitle" ),				// Pin name
					m_pReceiveLock( pReceiveLock ),
          m_pDemuxerPin( NULL ),
					m_pDump( pDump ),
					m_tLast( 0 ),
					m_PESdata( NULL ),
					m_pSubDecoder( pSubDecoder ),
					m_SubtitlePid( -1 ),
					m_PESlenght( 0 )
{
	m_PESdata = (unsigned char*)malloc(32000); // size is just a guess...
	Reset();
	Log( "Subtitle: Input pin created" );
}

CSubtitleInputPin::~CSubtitleInputPin()
{
}
//
// CheckMediaType
//
HRESULT CSubtitleInputPin::CheckMediaType( const CMediaType *pmt )
{
  Log("Subtitle: CSubtitleInputPin::CheckMediaType()");
	if( pmt->majortype == GUID_NULL )
	{
		return S_OK;
	}
	return S_FALSE;
}

//
// BreakConnect
//
HRESULT CSubtitleInputPin::BreakConnect()
{
  return CRenderedInputPin::BreakConnect();
}

HRESULT CSubtitleInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pDemuxerPin = pPin;

  if( m_SubtitlePid == -1 )
    return hr;  // PID is mapped later when we have it 

  hr = MapPidToDemuxer( m_SubtitlePid, m_pDemuxerPin, MEDIA_TRANSPORT_PAYLOAD );

  return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CSubtitleInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Buffers received media samples if needed 
// (PES packet size > maximum sample size)
//
STDMETHODIMP CSubtitleInputPin::Receive( IMediaSample *pSample )
{
	if( m_SubtitlePid == -1 )
    return S_OK;  // Nothing to be done yet

  try
	{
		if ( m_bReset )
		{
			Log( "Subtitle: reset" );

			if( m_PESdata != NULL )
			{	
				delete m_PESdata;	
				m_PESdata = NULL;
			}

			m_tLast = 0;
			m_PESlenght = 0;
			m_bReset = false;
		}
		CheckPointer( pSample, E_POINTER );

		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData = NULL;
		
		REFERENCE_TIME tStart, tStop;
		pSample->GetTime( &tStart, &tStop);

		m_tLast = tStart;
		long lDataLen = 0;

		HRESULT hr = pSample->GetPointer( &pbData );
		if( FAILED( hr ) ) 
		{
			Log( "Subtitle: Receive() err" );
			return hr;
		}
		lDataLen = pSample->GetActualDataLength();

		if( lDataLen > 5 )
		{
			Log("Subtitle: Receive() -- first bytes data %d %d %d %d -- lDataLen = %d IsDiscontinuity = %d", pbData[0], pbData[1], pbData[2], pbData[3], lDataLen, pSample->IsDiscontinuity() );

			unsigned __int8 endByte = pbData[lDataLen - 1];

			// PES header
			if ( pbData[0] == 0x00 && pbData[1] == 0x00 && pbData[2] == 0x01 && pbData[3]==0xbd )
			{
				m_PESlenght = pbData[4] * 256 + pbData[5];

				Log("Subtitle: PES lenght %d", m_PESlenght);

				// PES header and PES end byte in the same sample, no need for buffering
				if( endByte == 0xFF )
				{
					Log("Subtitle: Receive() - all PES data in one sample");
					
					// Send the whole sample to decoder
					m_pSubDecoder->ProcessPES( pbData, m_PESlenght, m_SubtitlePid );
				}
				else // PES continues in the next packet
				{
					// Free previous buffer
					if( m_PESdata != NULL )
						delete m_PESdata;	
	
					m_PESdata = (unsigned char*)malloc(32000); // size is just a guess...

					memcpy( m_PESdata, pbData, lDataLen );
					m_Position = lDataLen;

					Log( "Subtitle: Receive() - PES data continues in the next sample" );
				}
			}
			else // no PES header found
			{
				// PES end byte in the same sample, no need for buffering
				if( endByte == 0xFF )
				{
					// beginning of PES is missing
					if( !m_PESdata )
					{
						Log("Subtitle: Receive() - beginning of PES missing - last byte 0xFF");
						return S_OK;
					}
					
					Log("Subtitle: Receive() - all PES data arrived - last byte 0xFF");
					
					memcpy( m_PESdata + m_Position, pbData, lDataLen );
					m_pSubDecoder->ProcessPES( m_PESdata, m_PESlenght, m_SubtitlePid );
					
					m_Position = 0;
				}
				else // Append to buffer
				{
					memcpy( m_PESdata + m_Position, pbData, lDataLen );
					m_Position += lDataLen;

					Log( "Subtitle: Receive() - PES data continues in the next sample" );
				}
			}
		}
	}

	catch(...)
	{
		Log( "Subtitle: --- UNHANDLED EXCEPTION --- CSubtitleInputPin::Receive()" );
	}
  
  return S_OK;
}

void CSubtitleInputPin::Reset()
{
	m_bReset = true;
}

void CSubtitleInputPin::SetSubtitlePid( LONG pPid )
{
	m_SubtitlePid = pPid;
  MapPidToDemuxer( m_SubtitlePid, m_pDemuxerPin, MEDIA_TRANSPORT_PAYLOAD );
}

//
// EndOfStream
//
STDMETHODIMP CSubtitleInputPin::EndOfStream( void )
{
    CAutoLock lock( m_pReceiveLock );
    return CRenderedInputPin::EndOfStream();

} // EndOfStream

STDMETHODIMP CSubtitleInputPin::BeginFlush(void)
{
//	Reset();
	return CRenderedInputPin::BeginFlush();
}
STDMETHODIMP CSubtitleInputPin::EndFlush(void)
{
//	Reset();
	return CRenderedInputPin::EndFlush();
}

//
// NewSegment
//
STDMETHODIMP CSubtitleInputPin::NewSegment( REFERENCE_TIME tStart,
											REFERENCE_TIME tStop,
											double dRate )
{
    m_tLast = 0;
    return S_OK;
} // NewSegment
