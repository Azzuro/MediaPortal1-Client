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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "timeshifting.h"
#include "tsheader.h"


extern void LogDebug(const char *fmt, ...) ;

CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{

	m_params.chunkSize=1024*1024*256;
	m_params.maxFiles=20;
	m_params.maxSize=1024*1024*256;
	m_params.minFiles=6;
  m_pids.clear();
  m_pmtPid=-1;
  m_timeShiftMode=ProgramStream;
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
}
CTimeShifting::~CTimeShifting(void)
{
}

void CTimeShifting::OnTsPacket(byte* tsPacket)
{
	CEnterCriticalSection enter(m_section);
	if (m_bTimeShifting)
	{
    if (m_timeShiftMode==ProgramStream)
    {
		  m_multiPlexer.OnTsPacket(tsPacket);
    }
    else
    {
      WriteTs(tsPacket);
    }
	}
}


STDMETHODIMP CTimeShifting::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pcr pid:%x",pcrPid);
    m_pids.push_back(pcrPid);
		m_multiPlexer.ClearStreams();
		m_multiPlexer.SetPcrPid(pcrPid);
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPcrPid exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetPmtPid(int pmtPid)
{
  CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pmt pid:%x",pmtPid);
    m_pmtPid=pmtPid;
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPmtPid exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetMode(int mode) 
{
  m_timeShiftMode=(TimeShiftingMode)mode;
  if (mode==ProgramStream)
			LogDebug("Timeshifter:program stream mode");
  else
      LogDebug("Timeshifter:transport stream mode");
	return S_OK;
}

STDMETHODIMP CTimeShifting::GetMode(int *mode) 
{
  *mode=(int)m_timeShiftMode;
	return S_OK;
}

STDMETHODIMP CTimeShifting::AddPesStream(int pid, bool isAudio, bool isVideo)
{
	CEnterCriticalSection enter(m_section);
	try
	{
    m_pids.push_back(pid);
		if (isAudio)
			LogDebug("Timeshifter:add audio pes stream pid:%x",pid);
		else if (isVideo)
			LogDebug("Timeshifter:add video pes stream pid:%x",pid);
		else 
			LogDebug("Timeshifter:add private pes stream pid:%x",pid);
		m_multiPlexer.AddPesStream(pid,isAudio,isVideo);
	}
	catch(...)
	{
		LogDebug("Timeshifter:AddPesStream exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::RemovePesStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:remove pes stream pid:%x",pid);
		m_multiPlexer.RemovePesStream(pid);
    ivecPids it = m_pids.begin();
    while (it!=m_pids.end())
    {
      if (*it == pid)
      {
        it=m_pids.erase(it);
      }
      else
      {
        ++it;
      }
    }
	}
	catch(...)
	{
		LogDebug("Timeshifter:RemovePesStream exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetTimeShiftingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_multiPlexer.Reset();
		strcpy(m_szFileName,pszFileName);
		strcat(m_szFileName,".tsbuffer");
		LogDebug("Timeshifter:set filename:%s",m_szFileName);
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetTimeShiftingFileName exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (strlen(m_szFileName)==0) return E_FAIL;
		::DeleteFile((LPCTSTR) m_szFileName);
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

		m_pTimeShiftFile = new MultiFileWriter(&m_params);
		if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
		{
			LogDebug("Timeshifter:failed to open filename:%s %d",m_szFileName,GetLastError());
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
			return E_FAIL;
		}

		LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=true;
	}
	catch(...)
	{
		LogDebug("Timeshifter:Start timeshifting exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::Reset()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Reset");
    m_pmtPid=-1;
    m_pids.clear();
		m_multiPlexer.Reset();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Reset timeshifting exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::Stop()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Stop timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=false;
		m_multiPlexer.Reset();
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Stop timeshifting exception");
	}
	return S_OK;
}


void CTimeShifting::Write(byte* buffer, int len)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (!m_bTimeShifting) return;
    if (buffer==NULL) return;
    if (len <=0) return;
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->Write(buffer,len);
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Write exception");
	}
}

STDMETHODIMP CTimeShifting::GetBufferSize(long *size)
{
	CheckPointer(size, E_POINTER);
	*size = 0;
	return S_OK;
}

STDMETHODIMP CTimeShifting::GetNumbFilesAdded(WORD *numbAdd)
{
    CheckPointer(numbAdd, E_POINTER);
	*numbAdd = (WORD)m_pTimeShiftFile->getNumbFilesAdded();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetNumbFilesRemoved(WORD *numbRem)
{
    CheckPointer(numbRem, E_POINTER);
	*numbRem = (WORD)m_pTimeShiftFile->getNumbFilesRemoved();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetCurrentFileId(WORD *fileID)
{
    CheckPointer(fileID, E_POINTER);
	*fileID = (WORD)m_pTimeShiftFile->getCurrentFileId();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMinTSFiles(WORD *minFiles)
{
    CheckPointer(minFiles, E_POINTER);
	*minFiles = (WORD) m_params.minFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMinTSFiles(WORD minFiles)
{
	m_params.minFiles=(long)minFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMaxTSFiles(WORD *maxFiles)
{
    CheckPointer(maxFiles, E_POINTER);
	*maxFiles = (WORD) m_params.maxFiles;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMaxTSFiles(WORD maxFiles)
{
	m_params.maxFiles=(long)maxFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMaxTSFileSize(__int64 *maxSize)
{
    CheckPointer(maxSize, E_POINTER);
	*maxSize = m_params.maxSize;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMaxTSFileSize(__int64 maxSize)
{
	m_params.maxSize=maxSize;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetChunkReserve(__int64 *chunkSize)
{
  CheckPointer(chunkSize, E_POINTER);
	*chunkSize = m_params.chunkSize;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetChunkReserve(__int64 chunkSize)
{
  m_params.chunkSize=chunkSize;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetFileBufferSize(__int64 *lpllsize)
{
    CheckPointer(lpllsize, E_POINTER);
	m_pTimeShiftFile->GetFileSize(lpllsize);
	return NOERROR;
}


void CTimeShifting::WriteTs(byte* tsPacket)
{
  if (m_pids.size()==0) return;
	CTsHeader header(tsPacket);
  bool write=false;
  if (header.Pid==0 || header.Pid==0x10 || header.Pid==m_pmtPid) 
  {
    write=true;
  }
  else
  {
    for (int i=0; i < m_pids.size();++i)
    {
      if (m_pids[i] ==header.Pid)
      {
        write=true;
        break;
      }
    }
  }
  if (write==false) return;
  Write(tsPacket,188);
}