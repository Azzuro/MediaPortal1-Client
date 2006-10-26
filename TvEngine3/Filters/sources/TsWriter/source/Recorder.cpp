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

#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "recorder.h"
#include "tsheader.h"


extern void LogDebug(const char *fmt, ...) ;

//FILE* fpOut=NULL;
CRecorder::CRecorder(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsRecorder"), pUnk)
{
	strcpy(m_szFileName,"");
  m_timeShiftMode=ProgramStream;
	m_bRecording=false;
	m_pRecordFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
}
CRecorder::~CRecorder(void)
{
}

void CRecorder::OnTsPacket(byte* tsPacket)
{
	
	CEnterCriticalSection enter(m_section);
	if (m_bRecording)
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

STDMETHODIMP CRecorder::SetMode(int mode) 
{
  m_timeShiftMode=(TimeShiftingMode)mode;
  if (mode==ProgramStream)
			LogDebug("Recorder:program stream mode");
  else
      LogDebug("Recorder:transport stream mode");
	return S_OK;
}

STDMETHODIMP CRecorder::GetMode(int *mode) 
{
  *mode=(int)m_timeShiftMode;
	return S_OK;
}

STDMETHODIMP CRecorder::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:pcr pid:%x",pcrPid);
	m_multiPlexer.SetPcrPid(pcrPid);
	return S_OK;
}

STDMETHODIMP CRecorder::AddStream(int pid,bool isAudio,bool isVideo)
{
	CEnterCriticalSection enter(m_section);
	if (isAudio)
		LogDebug("Recorder:add audio stream pid:%x",pid);
	else if (isVideo)
		LogDebug("Recorder:add video stream pid:%x",pid);
	else 
		LogDebug("Recorder:add private stream pid:%x",pid);

  m_vecPids.push_back(pid);
	m_multiPlexer.AddPesStream(pid,isAudio,isVideo);
	return S_OK;
}

STDMETHODIMP CRecorder::RemoveStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:remove pes stream pid:%x",pid);
	m_multiPlexer.RemovePesStream(pid);
  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (*it==pid)
    {
      it=m_vecPids.erase(it);
    }
    else
    {
      ++it;
    }
  }
	return S_OK;
}

STDMETHODIMP CRecorder::SetRecordingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
  m_vecPids.clear();
	m_multiPlexer.Reset();
	strcpy(m_szFileName,pszFileName);
	return S_OK;
}
STDMETHODIMP CRecorder::StartRecord()
{
	CEnterCriticalSection enter(m_section);
	if (strlen(m_szFileName)==0) return E_FAIL;
	::DeleteFile((LPCTSTR) m_szFileName);
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

	m_pRecordFile = new FileWriter();
	m_pRecordFile->SetFileName( wstrFileName);
	if (FAILED(m_pRecordFile->OpenFile())) 
	{
		m_pRecordFile->CloseFile();
		delete m_pRecordFile;
		m_pRecordFile=NULL;
		return E_FAIL;
	}

	LogDebug("Recorder:Start Recording:'%s'",m_szFileName);
	m_bRecording=true;
	//::DeleteFile("out.ts");
	//fpOut =fopen("out.ts","wb+");
	return S_OK;
}
STDMETHODIMP CRecorder::StopRecord()
{
	CEnterCriticalSection enter(m_section);
  if (m_bRecording)
	  LogDebug("Recorder:Stop Recording:'%s'",m_szFileName);
	m_bRecording=false;
	m_multiPlexer.Reset();
	if (m_pRecordFile!=NULL)
	{
		m_pRecordFile->CloseFile();
		delete m_pRecordFile;
		m_pRecordFile=NULL;
	}
	return S_OK;
}


void CRecorder::Write(byte* buffer, int len)
{
	CEnterCriticalSection enter(m_section);
	if (!m_bRecording) return;
	if (m_pRecordFile!=NULL)
	{
		m_pRecordFile->Write(buffer,len);
	}
}

void CRecorder::WriteTs(byte* tsPacket)
{
	if (!m_bRecording) return;
	CTsHeader header(tsPacket);
	if (header.TransportError) return;
  if (header.Pid==0)
  {
    //PAT
    Write(tsPacket,188);
    return;
  }
  if (header.Pid==m_multiPlexer.GetPcrPid())
  {
    //PCR
    Write(tsPacket,188);
      return;
  }
  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (header.Pid==*it)
    {
      Write(tsPacket,188);
      return;
    }
    ++it;
  }
}