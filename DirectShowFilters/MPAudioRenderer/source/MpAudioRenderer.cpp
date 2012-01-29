// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// parts of the code are based on MPC-HC audio renderer source code

#include "stdafx.h"
#include <initguid.h>
#include "moreuuids.h"
#include <ks.h>
#include <ksmedia.h>
#include <propkey.h>
#include <FunctionDiscoveryKeys_devpkey.h>

#include "MpAudioRenderer.h"
#include "FilterApp.h"

#include "alloctracing.h"

CFilterApp theApp;

#define MAX_SAMPLE_TIME_ERROR 10000 // 1.0 ms

extern HRESULT CopyWaveFormatEx(WAVEFORMATEX** dst, const WAVEFORMATEX* src);

CUnknown* WINAPI CMPAudioRenderer::CreateInstance(LPUNKNOWN punk, HRESULT* phr)
{
  ASSERT(phr);
  CMPAudioRenderer *pNewObject = new CMPAudioRenderer(punk, phr);

  if (!pNewObject)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}

// for logging
extern void Log(const char* fmt, ...);
extern void LogWaveFormat(const WAVEFORMATEX* pwfx, const char* text);

CMPAudioRenderer::CMPAudioRenderer(LPUNKNOWN punk, HRESULT* phr)
  : CBaseRenderer(__uuidof(this), NAME("MediaPortal - Audio Renderer"), punk, phr),
  m_dRate(1.0),
  m_pReferenceClock(NULL),
  m_pWaveFileFormat(NULL),
  m_dBias(1.0),
  m_dAdjustment(1.0),
  m_dSampleCounter(0),
  m_pVolumeHandler(NULL),
  m_pWASAPIRenderer(NULL),
  m_pAC3Encoder(NULL),
  m_pBitDepthAdapter(NULL)
{
  Log("CMPAudioRenderer - instance 0x%x", this);

  m_pClock = new CSyncClock(static_cast<IBaseFilter*>(this), phr, this, m_Settings.m_bHWBasedRefClock);

  if (!m_pClock)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
    return;
  }

  m_pClock->SetAudioDelay(m_Settings.m_lAudioDelay * 10000); // setting in registry is in ms

  m_pVolumeHandler = new CVolumeHandler(punk);

  if (m_pVolumeHandler)
    m_pVolumeHandler->AddRef();
  else
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
    return;
  }

  // CBaseRenderer is using a lazy initialization for the CRendererPosPassThru - we need it always
  CBasePin *pPin = GetPin(0);
  HRESULT hr = E_OUTOFMEMORY;
  m_pPosition = new CRendererPosPassThru(NAME("Renderer CPosPassThru"), CBaseFilter::GetOwner(), &hr, pPin);
  if (!m_pPosition && FAILED(hr))
  {
    if (phr)
      *phr = hr;
    return;
  }
  
  SetupFilterPipeline();

  hr = m_pPipeline->Init();
  if (FAILED(hr))
  {
    if (phr)
      *phr = hr;
    return;
  }  

  m_pPipeline->Start(0);
}

CMPAudioRenderer::~CMPAudioRenderer()
{
  Log("MP Audio Renderer - destructor - instance 0x%x", this);
  
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  Stop();

  if (m_pVolumeHandler)
    m_pVolumeHandler->Release();

  delete m_pClock;

  if (m_pReferenceClock)
  {
    SetSyncSource(NULL);
    SAFE_RELEASE(m_pReferenceClock);
  }

  HRESULT hr = m_pPipeline->Cleanup();
  if (FAILED(hr))
    Log("Pipeline cleanup failed with: (0x%08x)");

  delete m_pWASAPIRenderer;
  delete m_pAC3Encoder;
  delete m_pBitDepthAdapter;

  SAFE_DELETE_WAVEFORMATEX(m_pWaveFileFormat);

  Log("MP Audio Renderer - destructor - instance 0x%x - end", this);
}

HRESULT CMPAudioRenderer::SetupFilterPipeline()
{
  m_pWASAPIRenderer = new CWASAPIRenderFilter(&m_Settings);
  if (!m_pWASAPIRenderer)
    return E_OUTOFMEMORY;

  m_pRenderFilter = static_cast<IRenderFilter*>(m_pWASAPIRenderer);

  m_pAC3Encoder = new CAC3EncoderFilter();
  if (!m_pAC3Encoder)
    return E_OUTOFMEMORY;

  m_pBitDepthAdapter = new CBitDepthAdapter();
  if (!m_pBitDepthAdapter)
    return E_OUTOFMEMORY;

  m_pTimestretchFilter = new CTimeStretchFilter(&m_Settings);
  if (!m_pTimestretchFilter)
    return E_OUTOFMEMORY;

  // Just for testing the sample duplication issue on pause
  /*
  
  CTimeStretchFilter* pTimestretchFilter = new CTimeStretchFilter(&m_Settings);
  CTimeStretchFilter* pTimestretchFilter1 = new CTimeStretchFilter(&m_Settings);
  CTimeStretchFilter* pTimestretchFilter2 = new CTimeStretchFilter(&m_Settings);
  CTimeStretchFilter* pTimestretchFilter3 = new CTimeStretchFilter(&m_Settings);
  CTimeStretchFilter* pTimestretchFilter4 = new CTimeStretchFilter(&m_Settings);

  pTimestretchFilter->ConnectTo(pTimestretchFilter1);
  pTimestretchFilter1->ConnectTo(pTimestretchFilter2);
  pTimestretchFilter2->ConnectTo(pTimestretchFilter3);
  pTimestretchFilter3->ConnectTo(pTimestretchFilter4);
  pTimestretchFilter4->ConnectTo(m_pTimestretchFilter);
  */

  m_pTimestretchFilter->ConnectTo(m_pWASAPIRenderer);

  //n_pBitDepthAdapter->ConnectTo(m_pAC3Encoder);
  //m_pAC3Encoder->ConnectTo(m_pWASAPIRenderer);
  
  // Entry point for the audio filter pipeline
  m_pPipeline = m_pTimestretchFilter; 

  return S_OK;
}

WAVEFORMATEX* CMPAudioRenderer::CreateWaveFormatForAC3(int pSamplesPerSec)
{
  WAVEFORMATEX* pwfx = (WAVEFORMATEX*)new BYTE[sizeof(WAVEFORMATEX)];
  if (pwfx)
  {
    // SPDIF uses static 2 channels and 16 bit. 
    // AC3 header contains the real stream information
    pwfx->wFormatTag = WAVE_FORMAT_DOLBY_AC3_SPDIF;
    pwfx->wBitsPerSample = 16;
    pwfx->nBlockAlign = 4;
    pwfx->nChannels = 2;
    pwfx->nSamplesPerSec = pSamplesPerSec;
    pwfx->nAvgBytesPerSec = pwfx->nSamplesPerSec * pwfx->nBlockAlign;
    pwfx->cbSize = 0;
  }
  return pwfx;
}

HRESULT CMPAudioRenderer::CheckInputType(const CMediaType *pmt)
{
  return CheckMediaType(pmt);
}

HRESULT	CMPAudioRenderer::CheckMediaType(const CMediaType *pmt)
{
  HRESULT hr = S_OK;
  
  if (!pmt) 
    return E_INVALIDARG;
  
  Log("CheckMediaType");

  if ((pmt->majortype	!= MEDIATYPE_Audio) ||
      (pmt->formattype != FORMAT_WaveFormatEx))
  {
    Log("CheckMediaType Not supported");
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  if (!pwfx) 
    return VFW_E_TYPE_NOT_ACCEPTED;

  return m_pPipeline->NegotiateFormat(pwfx, INFINITE);

/*
  LogWaveFormat(pwfx, "CheckMediaType");

  if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
  {
    WAVEFORMATEXTENSIBLE* tmp = (WAVEFORMATEXTENSIBLE*)pwfx;
    
    DWORD channelMask5_1 = m_Settings.m_dwChannelMaskOverride_5_1;
    DWORD channelMask7_1 = m_Settings.m_dwChannelMaskOverride_7_1;

    if (tmp->Format.nChannels == 6 && channelMask5_1 > 0)
    {
      Log("CheckMediaType:: overriding 5.1 channel mask to %d", channelMask5_1);
      tmp->dwChannelMask = channelMask5_1;  
    }

    if (tmp->Format.nChannels == 8 && channelMask7_1 > 0)
    {
      Log("CheckMediaType:: overriding 7.1 channel mask to %d", channelMask7_1);
      tmp->dwChannelMask = channelMask7_1;  
    }
  }

  if (m_Settings.m_bUseTimeStretching)
  {
    hr = m_pSoundTouch->CheckFormat(pwfx);
    if (FAILED(hr))
      return hr;
  }
  else
  {
    if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
    {
      WAVEFORMATEXTENSIBLE* tmp = (WAVEFORMATEXTENSIBLE*)pwfx;
      if (tmp->SubFormat != KSDATAFORMAT_SUBTYPE_PCM && 
          tmp->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
      {
          return VFW_E_TYPE_NOT_ACCEPTED;
      }
    }
    else if (pwfx->wFormatTag != WAVE_FORMAT_PCM && 
             pwfx->wFormatTag != WAVE_FORMAT_IEEE_FLOAT)
    {
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
  }

  if (m_pRenderDevice)
  {
    if (m_Settings.m_bEnableAC3Encoding && pwfx->nChannels > 2)
    {
      WAVEFORMATEX *pRenderFormat = CreateWaveFormatForAC3(pwfx->nSamplesPerSec);
      hr = m_pRenderDevice->CheckFormat(pRenderFormat);
      SAFE_DELETE_WAVEFORMATEX(pRenderFormat);
    }
    else
    {
      hr = m_pRenderDevice->CheckFormat(pwfx);
    }

    if (SUCCEEDED(hr))
    {
      Log("CheckMediaType - request old samples to be flushed");
      m_bFlushSamples = true;
    }
  }

  return hr;*/
}

HRESULT CMPAudioRenderer::AudioClock(UINT64& pTimestamp, UINT64& pQpc)
{
  if (m_pRenderFilter)
    return m_pRenderFilter->AudioClock(pTimestamp, pQpc);
  else
    return S_FALSE;

  //TRACE(_T("AudioClock query pos: %I64d qpc: %I64d"), pTimestamp, pQpc);
}

void CMPAudioRenderer::OnReceiveFirstSample(IMediaSample *pMediaSample)
{

}

BOOL CMPAudioRenderer::ScheduleSample(IMediaSample *pMediaSample)
{
  if (!pMediaSample) return false;

  REFERENCE_TIME rtSampleTime = 0;
  REFERENCE_TIME rtSampleEndTime = 0;

  //WaitForSingleObject((HANDLE)m_RenderEvent, 0);
  //HRESULT hr = m_pClock->AdviseTime((REFERENCE_TIME)m_tStart, rtSampleTime, (HEVENT)(HANDLE)m_RenderEvent, &m_dwAdvise);

  HRESULT hr = GetSampleTimes(pMediaSample, &rtSampleTime, &rtSampleEndTime);
  if (FAILED(hr)) return false;

  m_pPipeline->PutSample(pMediaSample);

  return false;

  // END
  /*

  REFERENCE_TIME rtSampleTime = 0;
  REFERENCE_TIME rtSampleEndTime = 0;
  REFERENCE_TIME rtSampleDuration = 0;
  REFERENCE_TIME rtTime = 0;
  UINT nFrames = 0;
  bool discontinuityDetected = false;

  if (m_bFlushSamples)
  {
    FlushSamples();
  }

  if (m_dRate >= 2.0 || m_dRate <= -2.0)
  {
    // Do not render Micey Mouse(tm) audio
    m_dSampleCounter++;
    return false;
  }
  
  HRESULT hr = GetSampleTimes(pMediaSample, &rtSampleTime, &rtSampleEndTime);
  if (FAILED(hr)) return false;

  long sampleLength = pMediaSample->GetActualDataLength();

  nFrames = sampleLength / m_pWaveFileFormat->nBlockAlign;
  rtSampleDuration = nFrames * UNITS / m_pWaveFileFormat->nSamplesPerSec;

  // Get media time
  m_pClock->GetTime(&rtTime);
  rtTime = rtTime - m_tStart;
  rtSampleTime -= m_pRenderDevice->Latency() * 2;

  // Try to keep the A/V sync when data has been dropped
  if ((abs(rtSampleTime - m_rtNextSampleTime) > MAX_SAMPLE_TIME_ERROR) && m_dSampleCounter > 1)
  {
    discontinuityDetected = true;
    Log("  Dropped audio data detected: diff: %.3f ms MAX_SAMPLE_TIME_ERROR: %.3f ms", ((double)rtSampleTime - (double)m_rtNextSampleTime) / 10000.0, (double)MAX_SAMPLE_TIME_ERROR / 10000.0);
  }

  if (rtSampleTime - rtTime > 0)
  {
    if(m_Settings.m_bLogSampleTimes)
      Log("     sample rtTime: %.3f ms rtSampleTime: %.3f ms", rtTime / 10000.0, rtSampleTime / 10000.0);
    
    if (m_dSampleCounter == 0 || discontinuityDetected)
    {
      ASSERT(m_dwAdvise == 0);
      ASSERT(m_pClock);
      WaitForSingleObject((HANDLE)m_RenderEvent, 0);
      hr = m_pClock->AdviseTime((REFERENCE_TIME)m_tStart, rtSampleTime, (HEVENT)(HANDLE)m_RenderEvent, &m_dwAdvise);
    }
    else
    {
      DoRenderSample(pMediaSample);
      hr = S_FALSE;
    }
    m_dSampleCounter++;
  }
  else
  {
    Log("DROP sample rtTime: %.3f ms rtSampleTime: %.3f ms", rtTime / 10000.0, rtSampleTime / 10000.0);
    hr = S_FALSE;
  }

  m_rtNextSampleTime = rtSampleTime + rtSampleDuration;

  if (hr == S_OK) 
    return true;
  else
    return false;

  */
}

HRESULT	CMPAudioRenderer::DoRenderSample(IMediaSample *pMediaSample)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  //return m_pRenderDevice->DoRenderSample(pMediaSample, m_dSampleCounter);
  m_pPipeline->PutSample(pMediaSample);

  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
  if (riid == IID_IReferenceClock)
    return GetInterface(static_cast<IReferenceClock*>(m_pClock), ppv);

  if (riid == IID_IAVSyncClock) 
    return GetInterface(static_cast<IAVSyncClock*>(this), ppv);

  if (riid == IID_IMediaSeeking) 
    return GetInterface(static_cast<IMediaSeeking*>(this), ppv);

  if (riid == IID_IBasicAudio)
    return GetInterface(static_cast<IBasicAudio*>(m_pVolumeHandler), ppv);

	return CBaseRenderer::NonDelegatingQueryInterface (riid, ppv);
}

HRESULT CMPAudioRenderer::SetMediaType(const CMediaType *pmt)
{
	if (!pmt) return E_POINTER;
  
  HRESULT hr = S_OK;
  Log("SetMediaType");

  WAVEFORMATEX* pwf = (WAVEFORMATEX*) pmt->Format();
  
  /*
  if (m_pRenderDevice)
  {
    if (m_Settings.m_bEnableAC3Encoding && pwf->nChannels > 2)
    {
      WAVEFORMATEX* pRenderFormat = CreateWaveFormatForAC3(pwf->nSamplesPerSec);
      m_pRenderDevice->SetMediaType(pRenderFormat);
      SAFE_DELETE_WAVEFORMATEX(pRenderFormat);
    }
    else
    {
      m_pRenderDevice->SetMediaType(pwf);
    }
  }*/

  m_pPipeline->NegotiateFormat(pwf, INFINITE);

  SAFE_DELETE_WAVEFORMATEX(m_pWaveFileFormat);
  
  if (pwf)
  {
    hr = CopyWaveFormatEx(&m_pWaveFileFormat, pwf);
    if (FAILED(hr))
      return hr;

    /*
    if (m_pSoundTouch)
    {
      //m_pSoundTouch->setChannels(pwf->nChannels);
      hr = m_pSoundTouch->SetFormat(pwf);
      if (FAILED(hr))
      {
        Log("CMPAudioRenderer::SetMediaType: Format rejected by CMultiSoundTouch (0x%08x)", hr);
        LogWaveFormat(pwf, "SetMediaType");
        return hr;
      }

      m_pSoundTouch->setSampleRate(pwf->nSamplesPerSec);
      m_pSoundTouch->setTempoChange(0);
      m_pSoundTouch->setPitchSemiTones(0);
      m_pSoundTouch->setSetting(SETTING_USE_QUICKSEEK, m_Settings.m_bQuality_USE_QUICKSEEK);
      m_pSoundTouch->setSetting(SETTING_USE_AA_FILTER, m_Settings.m_bQuality_USE_AA_FILTER);
      m_pSoundTouch->setSetting(SETTING_AA_FILTER_LENGTH, m_Settings.m_lQuality_AA_FILTER_LENGTH);
      m_pSoundTouch->setSetting(SETTING_SEQUENCE_MS, m_Settings.m_lQuality_SEQUENCE_MS); 
      m_pSoundTouch->setSetting(SETTING_SEEKWINDOW_MS, m_Settings.m_lQuality_SEEKWINDOW_MS);
      m_pSoundTouch->setSetting(SETTING_OVERLAP_MS, m_Settings.m_lQuality_SEQUENCE_MS);
    }*/
  }

  return CBaseRenderer::SetMediaType(pmt);
}

HRESULT CMPAudioRenderer::CompleteConnect(IPin* pReceivePin)
{
  Log("CompleteConnect");

  HRESULT hr = S_OK;
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  
  hr = pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter == NULL) return E_FAIL;
  hr = pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();
  pinInfo.pFilter->Release();

  if (FAILED(hr)) 
    return E_FAIL;
  
  Log("CompleteConnect - audio decoder: %S", &filterInfo.achName);

  //if (!m_pRenderDevice) return E_FAIL;

  if (SUCCEEDED(hr)) hr = CBaseRenderer::CompleteConnect(pReceivePin);
  
  //if (SUCCEEDED(hr)) hr = m_pRenderDevice->CompleteConnect(pReceivePin);

  if (SUCCEEDED(hr)) Log("CompleteConnect Success");

  return hr;
}

STDMETHODIMP CMPAudioRenderer::Run(REFERENCE_TIME tStart)
{
  Log("Run");
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  HRESULT	hr = S_OK;

  if (m_State == State_Running) 
    return hr;

  if (m_pClock)
    m_pClock->Reset();

  hr = m_pPipeline->Run(tStart);
     
  if (FAILED(hr))
    return hr;

  return CBaseRenderer::Run(tStart);
}

STDMETHODIMP CMPAudioRenderer::Stop() 
{
  Log("Stop");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  m_pPipeline->BeginStop();
  m_pPipeline->EndStop();

  return CBaseRenderer::Stop(); 
};

STDMETHODIMP CMPAudioRenderer::Pause()
{
  Log("Pause");
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  m_dSampleCounter = 0;
  HRESULT hr = m_pPipeline->Pause();

  if (FAILED(hr))
    return hr;

  return CBaseRenderer::Pause(); 
};


HRESULT CMPAudioRenderer::GetReferenceClockInterface(REFIID riid, void **ppv)
{
  HRESULT hr = S_OK;

  if (m_pReferenceClock)
    return m_pReferenceClock->NonDelegatingQueryInterface(riid, ppv);

  m_pReferenceClock = new CBaseReferenceClock (NAME("MP Audio Clock"), NULL, &hr);
	
  if (!m_pReferenceClock)
    return E_OUTOFMEMORY;

  m_pReferenceClock->AddRef();

  hr = SetSyncSource(m_pReferenceClock);
  if (FAILED(hr)) 
  {
    SetSyncSource(NULL);
    return hr;
  }

  return GetReferenceClockInterface(riid, ppv);
}

HRESULT CMPAudioRenderer::EndOfStream()
{
  Log("EndOfStream");

  HRESULT hr = m_pPipeline->EndOfStream();
  if (FAILED(hr))
    return hr;

  return CBaseRenderer::EndOfStream();
}

HRESULT CMPAudioRenderer::BeginFlush()
{
  Log("BeginFlush");

  CAutoLock cInterfaceLock(&m_InterfaceLock);

  HRESULT hr = CBaseRenderer::BeginFlush(); 
  if (FAILED(hr))
    return hr;

  return m_pPipeline->BeginFlush();
}

HRESULT CMPAudioRenderer::EndFlush()
{
  Log("EndFlush");
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  m_dSampleCounter = 0;

  m_pPipeline->EndFlush();
  m_pClock->Reset();

  return CBaseRenderer::EndFlush(); 
}

// TODO - implement TsReader side as well

/*
bool CMPAudioRenderer::CheckForLiveSouce()
{
  FILTER_INFO filterInfo;
  ZeroMemory(&filterInfo, sizeof(filterInfo));
  m_EVRFilter->QueryFilterInfo(&filterInfo); // This addref's the pGraph member

  CComPtr<IBaseFilter> pBaseFilter;

  HRESULT hr = filterInfo.pGraph->FindFilterByName(L"MediaPortal File Reader", &pBaseFilter);
  filterInfo.pGraph->Release();
}*/

// IAVSyncClock interface implementation

HRESULT CMPAudioRenderer::AdjustClock(DOUBLE pAdjustment)
{
  //CAutoLock cAutoLock(&m_csResampleLock);
  
  if (m_Settings.m_bUseTimeStretching && m_Settings.m_bEnableSyncAdjustment)
  {
    m_dAdjustment = pAdjustment;
    m_pClock->SetAdjustment(m_dAdjustment);
    //if (m_pSoundTouch)
      //m_pSoundTouch->setTempo(m_dBias, m_dAdjustment);

    // TODO notify pipeline

    return S_OK;
  }
  else
    return S_FALSE;
}

HRESULT CMPAudioRenderer::SetEVRPresentationDelay(DOUBLE pEVRDelay)
{
  //CAutoLock cAutoLock(&m_csResampleLock);

  bool ret = S_FALSE;

  if (m_Settings.m_bUseTimeStretching)
  {
    Log("SetPresentationDelay: %1.10f", pEVRDelay);

    m_pClock->SetEVRDelay(pEVRDelay * 10000); // Presenter sets delay in ms

    ret = S_OK;
  }
  else
  {
    Log("SetPresentationDelay: %1.10f - failed, time stretching is disabled", pEVRDelay);
    ret = S_FALSE;  
  }

  return ret;
}

HRESULT CMPAudioRenderer::SetBias(DOUBLE pBias)
{
  //CAutoLock cAutoLock(&m_csResampleLock);

  bool ret = S_FALSE;

  if (m_Settings.m_bUseTimeStretching)
  {
    Log("SetBias: %1.10f", pBias);

    if (pBias < m_Settings.m_dMinBias)
    {
      Log("   bias value too small - using 1.0");
      m_dBias = 1.0;
      ret = S_FALSE; 
    }
    else if(pBias > m_Settings.m_dMaxBias)
    {
      Log("   bias value too big - using 1.0");
      m_dBias = 1.0;
      ret = S_FALSE; 
    }
    else
    {
      m_dBias = pBias;
      ret = S_OK;  
    }
    
    m_pClock->SetBias(m_dBias);
//    if (m_pSoundTouch)
    {
      // TODO - provide to pipeline

     // m_pSoundTouch->setTempo(m_dBias, m_dAdjustment);
      Log("SetBias - updated SoundTouch tempo");
      // ret is not set since we want to be able to indicate the too big / small bias value	  
    }
    //else
    {
      Log("SetBias - no SoundTouch avaible!");
      ret = S_FALSE;
    }
  }
  else
  {
    Log("SetBias: %1.10f - failed, time stretching is disabled", pBias);
    ret = S_FALSE;  
  }

  return ret;
}

HRESULT CMPAudioRenderer::GetBias(DOUBLE* pBias)
{
  CheckPointer(pBias, E_POINTER);
  *pBias = m_pClock->Bias();

  return S_OK;
}

HRESULT CMPAudioRenderer::GetMaxBias(DOUBLE *pMaxBias)
{
  CheckPointer(pMaxBias, E_POINTER);
  *pMaxBias = m_Settings.m_dMaxBias;

  return S_OK;
}

HRESULT CMPAudioRenderer::GetMinBias(DOUBLE *pMinBias)
{
  CheckPointer(pMinBias, E_POINTER);
  *pMinBias = m_Settings.m_dMinBias;

  return S_OK;
}

HRESULT CMPAudioRenderer::GetClockData(CLOCKDATA *pClockData)
{
  CheckPointer(pClockData, E_POINTER);
  m_pClock->GetClockData(pClockData);

  return S_OK;
}

