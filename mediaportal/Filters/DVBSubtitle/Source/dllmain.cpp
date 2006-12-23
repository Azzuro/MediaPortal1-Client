/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
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

#pragma warning( disable: 4995 4996 )

#include <shlobj.h>
#include "DVBSub.h"

static bool folderOk = false;

const AMOVIESETUP_FILTER FilterInfo =
{
    &CLSID_DVBSub,		      // Filter CLSID
    L"MediaPortal DVBSub",  // String name
    MERIT_DO_NOT_USE,			  // Filter merit
    0,										  // Number pins
    NULL									  // Pin details
};

CFactoryTemplate g_Templates[1] = 
{
    { 
      L"MediaPortal DVBSub",      // Name
      &CLSID_DVBSub,              // CLSID
      CDVBSub::CreateInstance,    // Method to create an instance of MyComponent
      NULL,                       // Initialization function
      &FilterInfo                 // Set-up information (for filters)
    }
};

int g_cTemplates = 1;

STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2( TRUE );
}

STDAPI DllUnregisterServer()
{
    return AMovieDllRegisterServer2( FALSE );
}

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

// Logging 
#ifdef DEBUG
char *logbuffer=NULL; 
void LogDebug(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

  TCHAR folder[MAX_PATH];
  TCHAR fileName[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(fileName,"%s\\MediaPortal\\log\\MPDVBSubs.Log",folder);
  FILE* fp = fopen(fileName,"a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
};

#else
void LogDebug(const char *fmt, ...) 
{
}
#endif