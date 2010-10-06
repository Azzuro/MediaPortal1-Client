/*
 *  Copyright (C) 2005 Team MediaPortal
 *  http://www.team-mediaportal.com
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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <sbe.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "pmtparser.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_TIME  86400000L
byte MPEG1AudioFormat[] =
{
  0x50, 0x00,       //wFormatTag
  0x02, 0x00,       //nChannels
  0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
  0x00, 0x7D, 0x00, 0x00, //nAvgBytesPerSec
  0x00, 0x03,       //nBlockAlign
  0x00, 0x00,       //wBitsPerSample
  0x16, 0x00,       //cbSize
  0x02, 0x00,       //wValidBitsPerSample
  0x00, 0xE8,       //wSamplesPerBlock
  0x03, 0x00,       //wReserved
  0x01, 0x00, 0x01,0x00,  //dwChannelMask
  0x01, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00

};
extern void LogDebug(const char *fmt, ...) ;

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
  CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
  m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"),pUnk,phr,section),
  m_section(section)
{
  m_bConnected=false;
  m_rtStart=0;
  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    //AM_SEEKING_CanGetCurrentPos |
    AM_SEEKING_Source;
  m_bSubtitleCompensationSet=false;
}

CAudioPin::~CAudioPin()
{
  LogDebug("pin:dtor()");
}
STDMETHODIMP CAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  if (riid == IID_IMediaPosition)
  {
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CAudioPin::GetMediaType(CMediaType *pmt)
{
  //LogDebug("aud:GetMediaType()");
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

  int audioIndex = 0;
  demux.GetAudioStream(audioIndex);

  //demux.GetAudioStreamType(demux.GetAudioStream(), *pmt);
  demux.GetAudioStreamType(audioIndex, *pmt);
  return S_OK;
}

void CAudioPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

HRESULT CAudioPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("aud:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
  HRESULT hr;
  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  if (pRequest->cBuffers == 0)
  {
    pRequest->cBuffers = 30;
  }

  pRequest->cbBuffer = 8192;

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

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
  LogDebug("aud:CompleteConnect()");
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr))
  {
    LogDebug("aud:CompleteConnect() done");
    m_bConnected=true;
  }
  else
  {
    LogDebug("aud:CompleteConnect() failed:%x",hr);
  }

  if (m_pTsReaderFilter->IsTimeShifting())
  {
    //m_rtDuration=CRefTime(MAX_TIME);
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  //LogDebug("aud:CompleteConnect() ok");
  return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected=false;
  //LogDebug("aud:BreakConnect()");
  return CSourceStream::BreakConnect();
}

extern int ShowBuffer;

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer=NULL;

    do
    {
      //get file-duration and set m_rtDuration
      GetDuration(NULL);

      //if the filter is currently seeking to a new position
      //or this pin is currently seeking to a new position then
      //we dont try to read any packets, but simply return...
      if (m_pTsReaderFilter->IsSeeking() || m_pTsReaderFilter->IsStopping())// /*|| m_bSeeking*/ || m_pTsReaderFilter->IsSeekingToEof())
      {
        //if (ShowBuffer) LogDebug("aud:isseeking");
        Sleep(20);
        pSample->SetTime(NULL,NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(FALSE);
        pSample->SetDiscontinuity(TRUE);
        return NOERROR;
      }

      buffer=demux.GetAudio();

      //did we reach the end of the file
      if (demux.EndOfFile()) // || ((GetTickCount()-m_LastTickCount > 3000) && !m_pTsReaderFilter->IsTimeShifting()))
      {
        LogDebug("aud:set eof");
        pSample->SetTime(NULL,NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(FALSE);
        pSample->SetDiscontinuity(TRUE);
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (buffer==NULL)
      {
        Sleep(10);
      }
      else
      {
        int cntA,cntV ;
        CRefTime firstAudio, lastAudio;
        CRefTime firstVideo, lastVideo;
        cntA = demux.GetAudioBufferPts(firstAudio, lastAudio) + 1; // this one...
        cntV = demux.GetVideoBufferPts(firstVideo, lastVideo);
        #define PRESENT_DELAY 0000000
        // Ambass : Coming here means we've a minimum of audio(>=1)/video buffers waiting...
        if (!m_pTsReaderFilter->m_bStreamCompensated)
        {
          // Ambass : Find out the best compensation to apply in order to have fast Audio/Video delivery
          CRefTime BestCompensation ;
          CRefTime AddVideoCompensation ;
          LogDebug("Audio Samples : %d, First : %03.3f, Last : %03.3f",cntA, (float)firstAudio.Millisecs()/1000.0f,(float)lastAudio.Millisecs()/1000.0f);
          LogDebug("Video Samples : %d, First : %03.3f, Last : %03.3f",cntV, (float)firstVideo.Millisecs()/1000.0f,(float)lastVideo.Millisecs()/1000.0f);
          if (m_pTsReaderFilter->GetVideoPin()->IsConnected())
          {
/*
            if( !m_EnableSlowMotionOnZapping &&
               ( lastAudio.Millisecs() < demux.m_IframeSample.Millisecs() ) &&
               demux.GetVideoServiceType() != SERVICE_TYPE_VIDEO_UNKNOWN )
            {
              // Drop audio sample, do not allow slow motion video on channel changes
              delete buffer;
              buffer=NULL ;
              return NOERROR;
            }
*/            
            if (firstAudio.Millisecs() < firstVideo.Millisecs())
            {
              if (lastAudio.Millisecs() - 200 < firstVideo.Millisecs())
              {
                BestCompensation  = lastAudio - 2000000 - m_pTsReaderFilter->m_RandomCompensation - m_rtStart ; //demux.m_IframeSample /*firstVideo*/ -m_rtStart ;
                AddVideoCompensation = ( firstVideo - lastAudio + 2000000 ) ;
                LogDebug("Compensation : ( Rnd : %d mS ) Audio pts greatly ahead Video pts . Add %03.3f sec of extra video comp to start now !...( real time TV )",(DWORD)m_pTsReaderFilter->m_RandomCompensation/10000,(float)AddVideoCompensation.Millisecs()/1000.0f) ;
              }
              else
              {
                BestCompensation  = firstVideo - m_pTsReaderFilter->m_RandomCompensation - m_rtStart ; //demux.m_IframeSample /*firstVideo*/ -m_rtStart ;
                AddVideoCompensation = 0 ; // ( demux.m_IframeSample-firstAudio ) ;
                LogDebug("Compensation : ( Rnd : %d mS ) Audio pts ahead Video Pts ( Recover skipping Audio ) ....",m_pTsReaderFilter->m_RandomCompensation/10000) ;
              }
            }
            else
            {
//              if (lastVideo.Millisecs() < firstAudio.Millisecs())
//              {
//                BestCompensation = lastVideo-m_rtStart ;
//                AddVideoCompensation = 0 ;
//                LogDebug("Compensation : Suspicious case, Audio pts greatly behind Videop pts ...") ;
//              }
//              else
//              {
              BestCompensation = firstAudio-m_rtStart ;
              AddVideoCompensation = 0 ;
              LogDebug("Compensation : Audio pts behind Video Pts ( Recover skipping Video ) ....") ;
//              }
            }
//						if (lastVideo.Millisecs()==firstVideo.Millisecs())		// temporary hack for channels containing full GOP into single PES.
//						{
//							BestCompensation -= (REFERENCE_TIME)5000000 ;
//							AddVideoCompensation += (REFERENCE_TIME)5000000 ;
//						}
            m_pTsReaderFilter->m_RandomCompensation += 500000 ;   // Stupid feature required to have FFRW working with DVXA ( at least ATI.. ) to avoid frozen picture. ( it moves just moves the sample time a bit !! )
            m_pTsReaderFilter->m_RandomCompensation = m_pTsReaderFilter->m_RandomCompensation % 1000000 ;
          }
          else
          {
            BestCompensation = firstAudio-m_rtStart - 4000000 ;   // Need add delay before playing...
            AddVideoCompensation = 0 ;
          }

          // Here, clock filter should be running, but : EVR  is generally running and need to be compensated accordingly
          //                                             with current elapsed time from "RUN" to avoid beeing late.
          //                                             VMR9 is generally waiting I-frame + 1 or 2 frames to start clock and "RUN"
          // Apply a margin of 200 mS seems safe to avoid being late.
          if (m_pTsReaderFilter->State() == State_Running)
          {
            REFERENCE_TIME RefClock = 0;
            m_pTsReaderFilter->GetMediaPosition(&RefClock) ;

            m_pTsReaderFilter->m_ClockOnStart = RefClock - m_rtStart.m_time ;
            if (m_pTsReaderFilter->m_bLiveTv)
            {
              LogDebug("Elapsed time from pause to Audio/Video ( total zapping time ) : %d mS",GetTickCount()-m_pTsReaderFilter->m_lastPause);
            }
          }
          else
          {
            m_pTsReaderFilter->m_ClockOnStart=0 ;
          }

          // set the current compensation
          m_pTsReaderFilter->Compensation.m_time=(BestCompensation.m_time - m_pTsReaderFilter->m_ClockOnStart.m_time) - PRESENT_DELAY ;
          m_pTsReaderFilter->AddVideoComp=AddVideoCompensation ;

          LogDebug("aud:Compensation:%03.3f, Clock on start %03.3f m_rtStart:%d ",(float)m_pTsReaderFilter->Compensation.Millisecs()/1000.0f, m_pTsReaderFilter->m_ClockOnStart.Millisecs()/1000.0f, m_rtStart.Millisecs());

          //set flag to false so we dont keep compensating
          m_pTsReaderFilter->m_bStreamCompensated = true;
          m_bSubtitleCompensationSet = false;
        }

        // Subtitle filter is "found" only after Run() has been completed
        if(!m_bSubtitleCompensationSet)
        {
          IDVBSubtitle* pDVBSubtitleFilter(m_pTsReaderFilter->GetSubtitleFilter());
          if(pDVBSubtitleFilter)
          {
            LogDebug("aud:pDVBSubtitleFilter->SetTimeCompensation");
            pDVBSubtitleFilter->SetTimeCompensation(m_pTsReaderFilter->Compensation);
            m_bSubtitleCompensationSet=true;
          }
        }

        CRefTime RefTime,cRefTime ;
        bool HasTimestamp ;
        //check if it has a timestamp
        if ((HasTimestamp=buffer->MediaTime(RefTime)))
        {
          cRefTime = RefTime ;
					cRefTime -= m_rtStart ;
          //adjust the timestamp with the compensation
          cRefTime-= m_pTsReaderFilter->Compensation ;

          if (cRefTime.m_time >= m_pTsReaderFilter->m_ClockOnStart) // m_rtStart.m_time+m_pTsReaderFilter->Compensation.m_time) // + PRESENT_DELAY)
          {
            m_bPresentSample = true ;
            Sleep(5) ;
          }
          else
          {
            // Sample is too late.
            m_bPresentSample = false ;
          }
        }

        if (m_bPresentSample && m_dRateSeeking == 1.0)
        {
          //do we need to set the discontinuity flag?
          if (m_bDiscontinuity || buffer->GetDiscontinuity())
          {
            //ifso, set it
            LogDebug("aud:set discontinuity");
            pSample->SetDiscontinuity(TRUE);
            m_bDiscontinuity=FALSE;
          }

          if (HasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
            refTime /= m_dRateSeeking;

            pSample->SetSyncPoint(TRUE);

            pSample->SetTime(&refTime,&refTime);
            if (m_dRateSeeking == 1.0)
            {
              REFERENCE_TIME RefClock = 0;
              m_pTsReaderFilter->GetMediaPosition(&RefClock) ;
              float clock = (double)(RefClock-m_rtStart.m_time)/10000000.0 ;
              float fTime=(float)cRefTime.Millisecs()/1000.0f - clock ;

              if (ShowBuffer || fTime < 0.030)
              {
                LogDebug("Aud/Ref : %03.3f, Late              Compensated = %03.3f ( %0.3f A/V buffers=%02d/%02d), Clk : %f, State %d", (float)RefTime.Millisecs()/1000.0f, (float)cRefTime.Millisecs()/1000.0f, fTime,cntA,cntV, clock, m_pTsReaderFilter->State());
              }
              if (ShowBuffer) ShowBuffer--;
            }
          }
          else
          {
            //buffer has no timestamp
            pSample->SetTime(NULL,NULL);
            pSample->SetSyncPoint(FALSE);
          }

          //copy buffer in sample
          BYTE* pSampleBuffer;
          pSample->SetActualDataLength(buffer->Length());
          pSample->GetPointer(&pSampleBuffer);
          memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
          //delete the buffer and return
          delete buffer;
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          buffer=NULL ;
        }
      }
    } while (buffer==NULL);
    return NOERROR;
  }

  // Should we return something else than NOERROR when hitting an exception?
  catch(int e)
  {
    LogDebug("aud:fillbuffer exception %d", e);
  }
  catch(...)
  {
    LogDebug("aud:fillbuffer exception ...");
  }
  return NOERROR;
}

bool CAudioPin::IsConnected()
{
  return m_bConnected;
}

HRESULT CAudioPin::ChangeStart()
{
  UpdateFromSeek();
  return S_OK;
}

HRESULT CAudioPin::ChangeStop()
{
  UpdateFromSeek();
  return S_OK;
}

HRESULT CAudioPin::ChangeRate()
{
  /*if( m_dRateSeeking <= 0 )
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }*/
  LogDebug("aud: ChangeRate, m_dRateSeeking %f, Force seek done %d",(float)m_dRateSeeking, m_pTsReaderFilter->m_bSeekAfterRcDone);
  if (!m_pTsReaderFilter->m_bSeekAfterRcDone) //Don't force seek if another pin has already triggered it
  {
    m_pTsReaderFilter->m_bForceSeekAfterRateChange = true;
  }
  UpdateFromSeek();
  return S_OK;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
///
HRESULT CAudioPin::OnThreadStartPlay()
{
  //set flag to compensate any differences in the stream time & file time
  m_pTsReaderFilter->m_bStreamCompensated = false;
  m_pTsReaderFilter->GetDemultiplexer().m_bAudioVideoReady=false ;

  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity = TRUE;
  m_bPresentSample = false;

  LogDebug("aud:OnThreadStartPlay(%f) %02.2f", (float)m_rtStart.Millisecs()/1000.0f, m_dRateSeeking);

  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay( );
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime ;
}
STDMETHODIMP CAudioPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
void CAudioPin::UpdateFromSeek()
{
  m_pTsReaderFilter->SeekPreStart(m_rtStart);

//  LogDebug("aud: seek done %f/%f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_rtDuration.Millisecs()/1000.0f);
  return ;
}

//******************************************************
/// GetAvailable() returns
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
///
STDMETHODIMP CAudioPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
  //LogDebug("aud:GetAvailable");
  //if we are timeshifting, the earliest/latest timestamp can change
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    if (pEarliest)
    {
      //return the startpcr, which is the earliest pcr timestamp available in the timeshifting file
      double d2=duration.StartPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pEarliest= mediaTime;
    }
    if (pLatest)
    {
      //return the endpcr, which is the latest pcr timestamp available in the timeshifting file
      double d2=duration.EndPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pLatest= mediaTime;
    }
    return S_OK;
  }

  //not timeshifting, then leave it to the default sourceseeking class
  //which returns earliest=0, latest=m_rtDuration
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

//******************************************************
/// Returns the file duration in REFERENCE_TIME
/// For nomal .ts files it returns the current pcr - first pcr in the file
/// for timeshifting files it returns the current pcr - the first pcr ever read
/// So the duration keeps growing, even if timeshifting files are wrapped and being resued!
//
STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
  //LogDebug("aud:GetDuration");
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    CRefTime totalDuration=duration.TotalDuration();
    m_rtDuration=totalDuration;
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  if (pDuration!=NULL)
  {
    return CSourceSeeking::GetDuration(pDuration);
  }
  return S_OK;
}

//******************************************************
/// GetCurrentPosition() simply returns that this is not implemented by this pin
/// reason is that only the audio/video renderer now exactly the
/// current playing position and they do implement GetCurrentPosition()
///
STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("aud:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CAudioPin::Notify(IBaseFilter * pSender, Quality q)
{
  return E_NOTIMPL;
}