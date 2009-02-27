/**
*  TSBuffer.cpp
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include <streams.h>
#include "TSBuffer.h"
#include <crtdbg.h>
#include <math.h>
#include "global.h"
#include "entercriticalsection.h"

extern void Log(const char *fmt, ...) ;
CTSBuffer::CTSBuffer()
{
	m_pFileReader = NULL;
	m_lItemOffset = 0;
	m_lTSBufferItemSize = 8084 ; //128028;

	//round to nearest byte boundary.

	m_ParserLock = FALSE;
	m_loopCount = 20;
	debugcount = 0;
}

CTSBuffer::~CTSBuffer()
{
	Clear();
}

void CTSBuffer::SetFileReader(FileReader *pFileReader)
{
	if (!pFileReader)
		return;

  Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	m_pFileReader = pFileReader;
}

void CTSBuffer::Clear()
{
  Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	std::vector<BYTE *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete[] *it;
	}
	m_Array.clear();

	m_lItemOffset = 0;
	m_loopCount = 2;
}

long CTSBuffer::Count()
{
	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	long bytesAvailable = 0;
	long itemCount = m_Array.size();

	if (itemCount > 0)
	{
		bytesAvailable += m_lTSBufferItemSize - m_lItemOffset;
		bytesAvailable += m_lTSBufferItemSize * (itemCount - 1);
	}
	return bytesAvailable;
}

HRESULT CTSBuffer::Require(long nBytes, BOOL bIgnoreDelay)
{
	if (!m_pFileReader)
		return E_POINTER;

	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	long bytesAvailable = Count();
	if (nBytes <= bytesAvailable)
		return S_OK;

	while (nBytes > bytesAvailable)
	{
		BYTE *newItem = new BYTE[m_lTSBufferItemSize];
		ULONG ulBytesRead = 0;

		__int64 currPosition = m_pFileReader->GetFilePointer();
		HRESULT hr = m_pFileReader->Read(newItem, m_lTSBufferItemSize, &ulBytesRead);
		if (FAILED(hr)){

			delete[] newItem;
			return hr;
		}

		if (ulBytesRead < (ULONG)m_lTSBufferItemSize) 
		{
			WORD wReadOnly = 0;
			m_pFileReader->get_ReadOnly(&wReadOnly);
			if (wReadOnly && !bIgnoreDelay)
			{
				m_loopCount = max(2, m_loopCount);
				m_loopCount = min(20, m_loopCount);
//				int count = 220; // 2 second max delay
				while (ulBytesRead < (ULONG)m_lTSBufferItemSize && m_loopCount) 
				{
          //Log("TSBuffer::Require() Waiting for file to grow:%d",bDelay);
//					::OutputDebugString(TEXT("TSBuffer::Require() Waiting for file to grow.\n"));

					WORD bDelay = 0;
					m_pFileReader->get_DelayMode(&bDelay);
					m_loopCount--;

					if (bDelay > 0)
					{
						Sleep(2000);
						m_loopCount = 0;
					}
					else
					{
						if (!m_loopCount)
						{
							m_loopCount = 20;
							delete[] newItem;
							return hr;
						}
						Sleep(100);
					}

					ULONG ulNextBytesRead = 0;				
					m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
					HRESULT hr = m_pFileReader->Read(newItem, m_lTSBufferItemSize, &ulNextBytesRead);
					if (FAILED(hr) && !m_loopCount){

						m_loopCount = 20;
						delete[] newItem;
						return hr;
					}

					if (((ulNextBytesRead == 0) | (ulNextBytesRead == ulBytesRead)) && !m_loopCount){

						m_loopCount = 20;
						delete[] newItem;
						return E_FAIL;
					}

					ulBytesRead = ulNextBytesRead;
				}
			}
			else
			{
				delete[] newItem;
				return E_FAIL;
			}
		}

		m_loopCount = 20;
		if (newItem)
		{
			m_Array.push_back(newItem);
			bytesAvailable += m_lTSBufferItemSize;
		}

//		m_pFileReader->setBufferPointer();

	}
	return S_OK;
}

HRESULT CTSBuffer::DequeFromBuffer(BYTE *pbData, long lDataLength)
{
	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	HRESULT hr = Require(lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
			return E_FAIL;

		BYTE *item = m_Array.at(0);

		long copyLength = min(m_lTSBufferItemSize-m_lItemOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, item + m_lItemOffset, copyLength);

		bytesWritten += copyLength;
		m_lItemOffset += copyLength;

		if (m_lItemOffset >= m_lTSBufferItemSize)
		{
			m_Array.erase(m_Array.begin());
			delete[] item;
			m_lItemOffset -= m_lTSBufferItemSize;	//should result in zero
		}
	}
	return S_OK;
}

HRESULT CTSBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
	if (!m_pFileReader)
		return E_POINTER;

	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	HRESULT hr = Require(lOffset + lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	long itemIndex = 0;
	lOffset += m_lItemOffset;

	while (bytesWritten < lDataLength)
	{
		while (lOffset >= m_lTSBufferItemSize)
		{
			lOffset -= m_lTSBufferItemSize;

			itemIndex++;
			if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
				return E_FAIL;
		}

		if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
			return E_FAIL;

		BYTE *item = m_Array.at(itemIndex);

		long copyLength = min(m_lTSBufferItemSize-lOffset, lDataLength-bytesWritten);
		{
			memcpy(pbData + bytesWritten, item + lOffset, copyLength);

			bytesWritten += copyLength;
			lOffset += copyLength;
		}
	}

	return S_OK;
}