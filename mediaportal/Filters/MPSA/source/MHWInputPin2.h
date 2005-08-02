#pragma once
#pragma warning(disable: 4511 4512 4995)
#include "Section.h"
#include "SplitterSetup.h"
#include "Tablegrabber.h"
#include "MHWParser.h"

class CMHWInputPin2 :
	public CRenderedInputPin
{

private:
    CStreamAnalyzer    * const m_pDump;           // Main renderer object
    CCritSec * const m_pReceiveLock;    // Sample critical section
    REFERENCE_TIME m_tLast;             // Last sample receive time

public:

    CMHWInputPin2(CStreamAnalyzer *pDump,
                  LPUNKNOWN pUnk,
                  CBaseFilter *pFilter,
                  CCritSec *pLock,
                  CCritSec *pReceiveLock,
                  HRESULT *phr);

    // Do something with this media sample
    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream(void);
    STDMETHODIMP ReceiveCanBlock();

    // Write detailed information about this sample to a file
//    HRESULT WriteStringInfo(IMediaSample *pSample);

    // Check if the pin can support this specific proposed type and format
    HRESULT CheckMediaType(const CMediaType *);
	HRESULT CompleteConnect(IPin *pPin);
    // Break connection
    HRESULT BreakConnect();

    // Track NewSegment
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
	void ResetPids();
private:

	TableGrabber m_tableGrabber90;
	TableGrabber m_tableGrabber91;
	TableGrabber m_tableGrabber92;
	CMHWParser   m_MHWParser;
	
};
