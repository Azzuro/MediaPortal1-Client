
#include <streams.h>
#include "MemoryBuffer.h"

#define MAX_MEMORY_BUFFER_SIZE (500000)

extern void Log(const char *fmt, ...) ;

CMemoryBuffer::CMemoryBuffer(void)
:m_event(NULL,TRUE,FALSE,"memevent")
{
  m_BytesInBuffer=0;
  m_pcallback=NULL;
  m_bStopping=false;
}

CMemoryBuffer::~CMemoryBuffer()
{
  Clear();
}
void CMemoryBuffer::Clear()
{
	CAutoLock BufferLock(&m_BufferLock);
  m_bStopping=false;
	std::vector<BUFFERITEM *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
    BUFFERITEM *item = *it;
    delete[] item->data;
		delete item;
	}
	m_Array.clear();
  m_BytesInBuffer=0;
}
DWORD CMemoryBuffer::Size()
{
  return m_BytesInBuffer;
}
void CMemoryBuffer::Stop()
{
  Clear();
  m_bStopping=true;
}

DWORD CMemoryBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{	
	if (lDataLength<0) return 0;
  while (m_BytesInBuffer < lDataLength)
  {	
    if (m_bStopping) return 0;
    m_event.ResetEvent();
    m_event.Wait();
  }
		
	//Log("get..%d/%d",lDataLength,m_BytesInBuffer);
  long bytesWritten = 0;
	CAutoLock BufferLock(&m_BufferLock);
	while (bytesWritten < lDataLength)
	{
    if (m_bStopping) return 0;
		if(!m_Array.size() || m_Array.size() <= 0)
    {
			::OutputDebugStringA("read:empty buffer\n");
			return 0;
    }
		BUFFERITEM *item = m_Array.at(0);
    
		long copyLength = min(item->nDataLength - item->nOffset, lDataLength-bytesWritten);
		memcpy(&pbData[bytesWritten], &item->data[item->nOffset], copyLength);

		bytesWritten += copyLength;
		item->nOffset += copyLength;
    m_BytesInBuffer-=copyLength;

		if (item->nOffset >= item->nDataLength)
		{
			m_Array.erase(m_Array.begin());
      delete[] item->data;
			delete item;
		}
	}
	return bytesWritten;
}

HRESULT CMemoryBuffer::PutBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
  if (lDataLength<=0) return E_FAIL;
  if (lOffset<0) return E_FAIL;
  if (pbData==NULL) return E_FAIL;

  BUFFERITEM* item = new BUFFERITEM();
  item->nOffset=0;
  item->nDataLength=(lDataLength-lOffset);
  item->data = new byte[item->nDataLength];
  memcpy(item->data, &pbData[lOffset], item->nDataLength);

  {
	  CAutoLock BufferLock(&m_BufferLock);
    m_Array.push_back(item);
    m_BytesInBuffer+=item->nDataLength;

	  //Log("add..%d/%d",lDataLength,m_BytesInBuffer);
    while (m_BytesInBuffer > MAX_MEMORY_BUFFER_SIZE)
    {
		  Log("add: full buffer (%d)",m_BytesInBuffer);
		  BUFFERITEM *item = m_Array.at(0);
      int copyLength=item->nDataLength - item->nOffset;

      m_BytesInBuffer-=copyLength;
		  m_Array.erase(m_Array.begin());
      delete[] item->data;
		  delete item;
    }
    if (m_BytesInBuffer>0)
    {
      m_event.SetEvent();
    }
  }
  if (m_pcallback)
  {
    m_pcallback->OnRawDataReceived(&pbData[lOffset],lDataLength);
  }
	return S_OK;
}


void CMemoryBuffer::SetCallback(IMemoryCallback* callback)
{
  m_pcallback=callback;
}