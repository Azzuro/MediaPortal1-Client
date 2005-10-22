/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

#include <streams.h>
#include "MPTSFilter.h"
//#include "Mmsystem.h"
class CFilterOutPin;
extern void LogDebug(const char *fmt, ...) ;

CFilterOutPin::CFilterOutPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr) :
	CSourceStream(NAME("PinObject"), phr, pFilter, L"Out"),
	CSourceSeeking(NAME("MediaSeekingObject"), pUnk, phr, &m_cSharedState),
	m_pMPTSFilter(pFilter),
	m_pFileReader(pFileReader),
	m_pSections(pSections),m_bDiscontinuity(FALSE)
{
	LogDebug("pin:ctor()");
	CAutoLock cAutoLock(&m_cSharedState);
	__int64 size;
	m_pFileReader->GetFileSize(&size);
	m_rtDuration = m_rtStop = m_pSections->pids.Duration;
	m_lTSPacketDeliverySize = 188*100;
	m_pBuffers = new CBuffers(m_pFileReader, &m_pSections->pids,m_lTSPacketDeliverySize);
	m_dRateSeeking = 1.0;
	m_bAboutToStop=false;
	
}

CFilterOutPin::~CFilterOutPin()
{
	LogDebug("pin:dtor()");
	CAutoLock cAutoLock(&m_cSharedState);
	m_pBuffers->Clear();
	delete m_pBuffers;
}
STDMETHODIMP CFilterOutPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
    if (riid == IID_IMediaSeeking)
    {
        return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
    }
    return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CFilterOutPin::GetMediaType(CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pmt, E_POINTER);

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);

    return S_OK;
}

HRESULT CFilterOutPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
    HRESULT hr;

	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 2;
    }

	pRequest->cbBuffer = m_lTSPacketDeliverySize;
	

    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(pRequest, &Actual);
    if (FAILED(hr))
    {
        return hr;
    }

    if (Actual.cbBuffer < pRequest->cbBuffer)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CFilterOutPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		hr=m_pMPTSFilter->OnConnect();
		LogDebug("pin:CompleteConnect() done");
	}
	else
	{
		LogDebug("pin:CompleteConnect() failed:%x",hr);
	}
	return hr;
}

HRESULT CFilterOutPin::GetData(byte* pData, int lDataLength)
{
	HRESULT hr;
	__int64 fileSize;
	do
	{
		if (m_bAboutToStop) return E_FAIL;
		int count=0;
		if (m_pMPTSFilter->m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
		{
			while (true)
			{	
				if (m_bAboutToStop) return E_FAIL;
				if ( m_pMPTSFilter->UpdatePids())
				{
					LogDebug("pin:pids changed");
				}
				if ( m_pFileReader->GetFilePointer() <= m_pSections->pids.fileStartPosition &&
					m_pFileReader->GetFilePointer() + lDataLength>=m_pSections->pids.fileStartPosition )
				{
					//LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
					count++;
					if (count >100) break;
					Sleep(50);
				}
				else break;
			}
			//if (count>=100)
				//LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
		}

		bool endOfFile=false;
		hr = m_pBuffers->Require(lDataLength,endOfFile);
		if (endOfFile)
		{
			if (m_pMPTSFilter->m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
			{
				LogDebug("output pin:EOF");
				m_pMPTSFilter->m_pFileReader->GetFileSize(&fileSize);
				count=0;
				while (true)
				{
					if (m_bAboutToStop) return E_FAIL;
					m_pMPTSFilter->UpdatePids();
					if (m_pSections->pids.fileStartPosition >= fileSize-(1024*1024) ||
						m_pSections->pids.fileStartPosition < lDataLength) 
					{
						LogDebug("waiteof pos:%x size:%x (%d)", m_pSections->pids.fileStartPosition,fileSize,count);
						count++;
						if (count >100) break;
						Sleep(50);
					}
					else break;
				}
				LogDebug("outputpin:end of file, writepos:%x slept:%i fsize:%x", m_pSections->pids.fileStartPosition,count,fileSize);
				m_bDiscontinuity=TRUE;
			}
		}
					
		if (m_bAboutToStop) return E_FAIL;
	} while (hr==S_OK && m_pBuffers->Count() < lDataLength);
		

	if (FAILED(hr))
	{
		if (m_pMPTSFilter->m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE)
		{
			LogDebug("outpin:end of file detected");
			return S_FALSE;//end of stream
		}
			
		//LogDebug("outpin: Require(%d) failed:0x%x",lDataLength,hr);
		//m_pMPTSFilter->Refresh();
		//return S_FALSE; // cant read = end of stream
	}

	m_pBuffers->DequeFromBuffer(pData, lDataLength);
	return S_OK;
}

void CFilterOutPin::SeekIFrame()
{
		m_pBuffers->Clear();
		// find first i-frame
		TsDemux tsDemuxer;
		__int64 startPointer=m_pFileReader->GetFilePointer();;
		__int64 filePointer=m_pFileReader->GetFilePointer();
		ULONGLONG pts;
		if (m_pSections->pids.VideoPid>0)
		{
			LogDebug("find iframe pos:%x",(DWORD)filePointer);
			BYTE pData[188];
			Sections::TSHeader header;
			bool iFrameFound=false;
			while (true)
			{
				HRESULT hr=GetData(pData,188);
				
				if (hr!=S_OK) 
				{
					LogDebug("FAILED : GetData() in seekiframe!");
					return ;
				}
				m_pSections->GetTSHeader(pData,&header);

				int pid;
				if(m_pSections->CurrentPTS(pData,&pts,&pid)==S_OK)
				{
					if (pts>0)
					{
						if (pts >= m_pSections->pids.StartPTS && pts <= m_pSections->pids.EndPTS)
						{
							//LogDebug("pts:%x pid:%x", (DWORD)pts, header.Pid);
							m_iPESPid=header.Pid;
							UpdatePositions(pts);
						}
					}
				}

				if (header.Pid==m_pSections->pids.VideoPid)
				{
					bool isStart;
					if ( tsDemuxer.ParsePacket(pData, isStart))
					{
						if (isStart)
							startPointer=filePointer;
						filePointer=0;
						m_pFileReader->SetFilePointer(startPointer,FILE_BEGIN);	
						LogDebug("iframe found at pos:%x",startPointer);
						iFrameFound=true;
						break;
					}
					if (isStart)
						startPointer=filePointer;
				}
				filePointer+=188;
			}
			if (false==iFrameFound)
			{
					LogDebug("FAILED : Iframe not found!");
			}
		}
	
}
HRESULT CFilterOutPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
	//LogDebug("FillBuffer()");
	  
	CAutoLock cAutoLock(&m_cSharedState);
	if (m_bAboutToStop) 
	{
		return E_FAIL;
	}
	
	CheckPointer(pSample, E_POINTER);
	PBYTE pData;
	LONG lDataLength;
	
	ULONGLONG pts=0;
	ULONGLONG ptsNow=0;
	HRESULT hr = pSample->GetPointer(&pData);
	if (FAILED(hr))
	{
		LogDebug("FAILED: GetPointer() failed:%x",hr);
	
		return hr;
	}
	lDataLength = pSample->GetActualDataLength();


	hr=GetData(pData,lDataLength);
	if (hr==E_FAIL) 
	{
		LogDebug("FAILED to get data from file");
		return E_FAIL;
	}

	pSample->SetActualDataLength(lDataLength);
	pts=0;
	ptsNow=0;
	Sections::TSHeader header;
	for(int i=0;i<lDataLength;i+=188)
	{
		if (m_bAboutToStop) return E_FAIL;
		int pid;
		m_pSections->GetTSHeader(&pData[i],&header);
		if(m_pSections->CurrentPTS(&pData[i],&pts,&pid)==S_OK)
		{
			if (pts>0)
			{
				//LogDebug("found  pts:%x %x-%x pid:%x", (DWORD)pts, (DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,header.Pid);
				if (m_iPESPid==0 && pts >= m_pSections->pids.StartPTS && pts <= m_pSections->pids.EndPTS)
				{
					LogDebug("found start pts:%x %x-%x pid:%x", (DWORD)pts, (DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,header.Pid);
					m_iPESPid=header.Pid;
				}
				if (m_iPESPid==header.Pid)
				{
					if (ptsNow==0) 
					{ 
						ptsNow=pts; 
						break;
					}
				}
			}
		}
	}
	
	if (ptsNow>0)
	{
		if (ptsNow < m_pSections->pids.StartPTS || ptsNow > (m_pSections->pids.EndPTS+ ((__int64)0x100000)) )
		{
			LogDebug("INVALID pts:%x %x-%x", (DWORD)ptsNow,(DWORD)m_pSections->pids.StartPTS ,(DWORD) m_pSections->pids.EndPTS);
		}
		CRefTime rtStart,rtNow;
		Sections::PTSTime ptsTimeNow,ptsTimeStart;
		m_pSections->PTSToPTSTime(ptsNow,&ptsTimeNow);
		m_pSections->PTSToPTSTime(m_pSections->pids.StartPTS,&ptsTimeStart);
		rtNow=((ULONGLONG)36000000000*ptsTimeNow.h)+((ULONGLONG)600000000*ptsTimeNow.m)+((ULONGLONG)10000000*ptsTimeNow.s)+((ULONGLONG)1000*ptsTimeNow.u);
		rtStart =((ULONGLONG)36000000000*ptsTimeStart.h)+((ULONGLONG)600000000*ptsTimeStart.m)+((ULONGLONG)10000000*ptsTimeStart.s)+((ULONGLONG)1000*ptsTimeStart.u);
		rtNow -= rtStart;
		UpdatePositions(ptsNow);	
		REFERENCE_TIME tStart=(REFERENCE_TIME)rtNow;
		REFERENCE_TIME tEnd=(REFERENCE_TIME)rtNow;
		pSample->SetTime(&tStart,&tEnd);
	}

	pSample->SetTime(NULL,NULL);
	if(m_bDiscontinuity) 
	{
		LogDebug("set discontinuity");
		pSample->SetDiscontinuity(TRUE);
		m_bDiscontinuity = FALSE;
	}	
  }
  catch(...)
  {
	LogDebug("FAILED: pin:FillBuffer() exception");
  }
  return NOERROR;
}

HRESULT CFilterOutPin::OnThreadCreate( )
{
	LogDebug("pin:OnThreadCreate()");
	m_bAboutToStop=false;
    CAutoLock cAutoLockShared(&m_cSharedState);
	if(m_pFileReader->IsFileInvalid()==TRUE)
	{
		m_pFileReader->OpenFile();
	}
	SeekIFrame();
	return CSourceStream::OnThreadCreate();
}

HRESULT CFilterOutPin::OnThreadStartPlay( )
{
	LogDebug("pin:OnThreadStartPlay() %x-%x pos:%x", (DWORD)m_rtStart, (DWORD)m_rtStop, (DWORD)m_pSections->m_pFileReader->GetFilePointer());
	m_bDiscontinuity=TRUE;
	m_pBuffers->Clear();
	HRESULT hr=DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}




HRESULT CFilterOutPin::ChangeStart()
{
	if (ThreadExists())
	{
		DeliverBeginFlush();
		Stop();
		DeliverEndFlush();
		m_pMPTSFilter->SetFilePosition(m_rtStart);
		SeekIFrame();
 		LogDebug("pin:ChangeStart() done %x",(DWORD)m_rtStart);
		Run();
	}
   return S_OK;
}

HRESULT CFilterOutPin::ChangeStop()
{
	LogDebug("pin:ChangeStop()");
   {
        CAutoLock lock(CSourceSeeking::m_pLock);
    }

    UpdateFromSeek();
    return S_OK;
}

HRESULT CFilterOutPin::ChangeRate()
{
    {   // Scope for critical section lock.
        CAutoLock cAutoLockSeeking(CSourceSeeking::m_pLock);
        if( m_dRateSeeking <= 0 ) {
            m_dRateSeeking = 1.0;  // Reset to a reasonable value.
            return E_FAIL;
        }
    }
    UpdateFromSeek();
	return S_OK;
}


void CFilterOutPin::UpdateFromSeek(void)
{
	LogDebug("pin:UpdateFromSeek()");
	if (ThreadExists())
	{
        DeliverBeginFlush();
        Stop();
		DeliverEndFlush();
		Run();
	}
	LogDebug("pin:UpdateFromSeek() done");
}

HRESULT CFilterOutPin::SetDuration(REFERENCE_TIME duration)
{
	LogDebug("pin:SetDuration()");
	CAutoLock lock(CSourceSeeking::m_pLock);
	m_rtDuration = duration;
	m_rtStop = m_rtDuration;
	m_rtStart=0;
    return S_OK;
}
void CFilterOutPin::ResetBuffers(__int64 newPosition)
{
	CAutoLock cAutoLock(&m_cSharedState);
	LogDebug("Reset buffers");
	m_pFileReader->SetFilePointer(newPosition,FILE_BEGIN);
/*   
	if (m_pBuffers==NULL) return;
	m_pBuffers->Clear();
	m_mapDiscontinuitySent.clear();
	m_pFileReader->SetFilePointer(newPosition,FILE_BEGIN);
   m_bDiscontinuity=TRUE;
   m_iPESPid=0;
m_rtCurrent=0;
   m_rtStop=0;
   m_rtDuration=0;
  */
}

void CFilterOutPin::UpdatePositions(ULONGLONG& ptsNow)
{
	if (ptsNow==0) 
		return;
	static int prevsec=0;
	CRefTime rtStart,rtStop,rtDuration;
	Sections::PTSTime time;
	if (m_pSections->pids.StartPTS < m_pSections->pids.EndPTS)
	{
		rtStart   =m_pSections->pids.StartPTS;
		rtDuration=m_pSections->pids.EndPTS-m_pSections->pids.StartPTS;

		ptsNow -=rtStart;

		m_pSections->PTSToPTSTime(ptsNow,&time);
		ptsNow=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);


		if (prevsec!=time.s)
		{
			prevsec=time.s;
			LogDebug("1)%02.2d:%02.2d:%02.2d", time.h,time.m,time.s);

			/*
			char buffer[200];
			prevsec=time.s;
			HRESULT hr;
			IMediaSeeking* seek;
			IFilterGraph* graph=m_pFilter->GetFilterGraph();
			graph->QueryInterface(IID_IMediaSeeking,(void**)&seek);
			LONGLONG current,current2,stop,earliest,latest;
			hr=seek->GetCurrentPosition(&current);
			hr=seek->GetPositions(&current2,&stop);
			hr=seek->GetAvailable(&earliest,&latest);
			seek->Release();
			double fc=((double)current)/10000000.0;
			double fc2=((double)current2)/10000000.0;
			double fst=((double)stop)/10000000.0;
			double ear=((double)earliest)/10000000.0;
			double lat=((double)latest)/10000000.0;
			sprintf(buffer,"%02.2d:%02.2d:%02.2d %03.3f %03.3f %03.3f %03.3f %03.3f \n ", time.h,time.m,time.s,fc,fc2,fst,ear,lat);
			OutputDebugString(buffer);
			//graph->Release();*/
		}

		m_pSections->PTSToPTSTime(rtDuration,&time);
		rtDuration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);
	}
	else
	{
		//STARTPTS -------------- MAX_PTS -------------------------------- ENDPTS
		rtStart    = m_pSections->pids.StartPTS;
		rtDuration = m_pSections->pids.EndPTS- (MAX_PTS-m_pSections->pids.StartPTS);

		if (ptsNow  > m_pSections->pids.StartPTS)
		{
			ptsNow =ptsNow-rtStart;
		}
		else
		{
			ptsNow += (MAX_PTS-m_pSections->pids.StartPTS);
		}

		m_pSections->PTSToPTSTime(ptsNow,&time);
		ptsNow=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

		if (prevsec!=time.s)
		{
			prevsec=time.s;
			LogDebug("2)%02.2d:%02.2d:%02.2d", time.h,time.m,time.s);
		}

		m_pSections->PTSToPTSTime(rtDuration,&time);
		rtDuration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);
	}


	//m_rtStart=0;
	m_rtStop=rtDuration;
	m_rtDuration=rtDuration;
}
void CFilterOutPin::AboutToStop()
{			
	LogDebug("pin: AboutToStop()");
	m_bAboutToStop=true;
}