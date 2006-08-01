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
#include <windows.h>
class CPidTable
{
public:

  CPidTable();
  virtual ~CPidTable();
  void Reset();
  
  CPidTable operator = (const CPidTable &pids);

	ULONG PcrPid;
	ULONG PmtPid;
	WORD VideoPid;
	WORD AudioPid1;
	BYTE Lang1_1;
	BYTE Lang1_2;
	BYTE Lang1_3;
	WORD AudioPid2;
	BYTE Lang2_1;
	BYTE Lang2_2;
	BYTE Lang2_3;
  WORD AudioPid3;
	BYTE Lang3_1;
	BYTE Lang3_2;
	BYTE Lang3_3;
	WORD AC3Pid;
	WORD TeletextPid;
	WORD SubtitlePid;

  int  ServiceId;
};
