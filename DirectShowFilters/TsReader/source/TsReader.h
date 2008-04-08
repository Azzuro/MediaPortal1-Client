
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
#pragma once
#include "multifilereader.h"
#include "pcrdecoder.h"
#include "demultiplexer.h"
#include "TsDuration.h"
#include "TSThread.h"
#include "rtspclient.h"
#include "memorybuffer.h"
#include "..\..\DVBSubtitle2\Source\IDVBSub.h"
#include "ISubtitleStream.h"
#include "IAudioStream.h"
#include "ITeletextSource.h"
#include <map>
using namespace std;

class CSubtitlePin;
class CAudioPin;
class CVideoPin;
class CTsReader;
class CTsReaderFilter;

DEFINE_GUID(CLSID_TSReader, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f);
DEFINE_GUID(IID_ITSReader, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x4f);
//DEFINE_GUID(IID_ITSReaderAudioChange, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x5f);

DECLARE_INTERFACE_(ITSReaderCallback, IUnknown)
{
	STDMETHOD(OnMediaTypeChanged) (THIS_)PURE;	
};

DECLARE_INTERFACE_(ITSReaderAudioChange, IUnknown)
{	
	STDMETHOD(OnRequestAudioChange) (THIS_)PURE;
};

  MIDL_INTERFACE("b9559486-e1bb-45d3-a2a2-9a7afe49b24f")
  ITSReader : public IUnknown
  {
  public:
      virtual HRESULT STDMETHODCALLTYPE SetGraphCallback( 
		/* [in] */ ITSReaderCallback* pCallback
		) = 0;		        
	virtual HRESULT STDMETHODCALLTYPE SetRequestAudioChangeCallback( 
		ITSReaderAudioChange* pCallback
		) = 0;
  };

class CTsReaderFilter : public CSource, 
						public TSThread, 
						public IFileSourceFilter, 
            public IAMFilterMiscFlags, 
						public IAMStreamSelect, 
						public ISubtitleStream, 
						public ITeletextSource,						
						public IAudioStream,
						public ITSReader
{
public:
  DECLARE_IUNKNOWN
  static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
  CTsReaderFilter(IUnknown *pUnk, HRESULT *phr);
  ~CTsReaderFilter();
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

  // Pin enumeration
  CBasePin * GetPin(int n);
  int GetPinCount();

  // Open and close the file as necessary
public:
  STDMETHODIMP Run(REFERENCE_TIME tStart);
  STDMETHODIMP Pause();
  STDMETHODIMP Stop();
private:
	// IAMFilterMiscFlags
  virtual ULONG STDMETHODCALLTYPE		GetMiscFlags();

  //IAMStreamSelect
  STDMETHODIMP Count(DWORD* streamCount);
  STDMETHODIMP Enable(long index, DWORD flags);
  STDMETHODIMP Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk);	

	//IAudioStream
	//STDMETHODIMP SetAudioStream(__int32 stream);	
	STDMETHODIMP GetAudioStream(__int32 &stream);

  //ISubtitleStream
  STDMETHODIMP SetSubtitleStream(__int32 stream);
  STDMETHODIMP GetSubtitleStreamType(__int32 stream, int &type);
  STDMETHODIMP GetSubtitleStreamCount(__int32 &count);
  STDMETHODIMP GetCurrentSubtitleStream(__int32 &stream);
  STDMETHODIMP GetSubtitleStreamLanguage(__int32 stream,char* szLanguage);
	STDMETHODIMP SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int count, void* opts, int* select)); 

	//ITeletextSource
	STDMETHODIMP SetTeletextTSPacketCallBack ( int (CALLBACK *pPacketCallback)(byte*, int));
	STDMETHODIMP SetTeletextEventCallback (int (CALLBACK *EventCallback)(int,DWORD64) ); 
	STDMETHODIMP SetTeletextServiceInfoCallback (int (CALLBACK *pServiceInfoCallback)(int,byte,byte,byte,byte) ); 

public:
	// ITSReader
	STDMETHODIMP	  SetGraphCallback(ITSReaderCallback* pCallback);
	STDMETHODIMP	  SetRequestAudioChangeCallback(ITSReaderAudioChange* pCallback);
	// IFileSourceFilter
	STDMETHODIMP    Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP    GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP    GetDuration(REFERENCE_TIME *dur);
	double		      GetStartTime();
	bool            IsSeeking();
  bool            IsFilterRunning();
	CDeMultiplexer& GetDemultiplexer();
	void            Seek(CRefTime&  seekTime, bool seekInFile);
  void            SeekDone(CRefTime& refTime);
  void            SeekStart();
	double          UpdateDuration();
  CAudioPin*      GetAudioPin();
  CVideoPin*      GetVideoPin();
  CSubtitlePin*   GetSubtitlePin();
  IDVBSubtitle*   GetSubtitleFilter();
  bool            IsTimeShifting();
  CTsDuration&    GetDuration();
  FILTER_STATE    State() {return m_State;};
  CRefTime        Compensation;
  void				    OnMediaTypeChanged();
  void				    OnRequestAudioChange();
  bool			      IsStreaming();
protected:
  void ThreadProc();
private:
  void SetDuration();
  HRESULT AddGraphToRot(IUnknown *pUnkGraph) ;
  HRESULT FindSubtitleFilter();
  void    RemoveGraphFromRot();
	CAudioPin*	    m_pAudioPin;
	CVideoPin*	    m_pVideoPin;
	CSubtitlePin*	  m_pSubtitlePin;
	WCHAR           m_fileName[1024];
	CCritSec        m_section;
	CCritSec        m_CritSecDuration;
	FileReader*     m_fileReader;
	FileReader*     m_fileDuration;
  CTsDuration     m_duration;
  CBaseReferenceClock* m_referenceClock;
	CDeMultiplexer  m_demultiplexer;
  bool            m_bSeeking;
  DWORD           m_dwGraphRegister;

  CRTSPClient     m_rtspClient;
  CMemoryBuffer   m_buffer;
  DWORD           m_tickCount;
  CRefTime        m_seekTime;
  bool            m_bNeedSeeking;
  bool            m_bTimeShifting;
  IDVBSubtitle*   m_pDVBSubtitle;
  ITSReaderCallback* m_pCallback;
  ITSReaderAudioChange* m_pRequestAudioCallback;
};

