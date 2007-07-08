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
#ifndef __AudioPin_H
#define __AudioPin_H
#include "tsreader.h"
#include "mediaseeking.h"

class CAudioPin : public CSourceStream, public CSourceSeeking
{
public:
	CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section);
	~CAudioPin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
  HRESULT CheckConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
  HRESULT BreakConnect();
	

	// CSourceSeeking
	HRESULT ChangeStart();
	HRESULT ChangeStop();
	HRESULT ChangeRate();
  STDMETHODIMP SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags);
  STDMETHODIMP GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest );
  STDMETHODIMP GetDuration(LONGLONG *pDuration);
  STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);

	HRESULT OnThreadStartPlay();
	void SetStart(CRefTime rtStartTime);

  bool IsConnected();

protected:
  void UpdateFromSeek();
  bool m_bConnected;
	CRefTime	m_refStartTime;
	BOOL m_bDiscontinuity;
	CTsReaderFilter *	const m_pTsReaderFilter;
	CCritSec* m_section;
	CCritSec m_bufferLock;
	bool	m_bDropPackets;
  bool  m_bDropSeek;
  bool  m_bSeeking;
  DWORD m_seekTimer;
  CRefTime m_lastSeek;
};

#endif
