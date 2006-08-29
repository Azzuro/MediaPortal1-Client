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
#pragma once

class CPcrDecoder
{
public:
	CPcrDecoder(void);
	virtual ~CPcrDecoder(void);
	void	SetPcrPid(int pid);
	int		GetPcrPid();
	void	Reset();
	void	OnTsPacket(byte* tsPacket);

	__int64 PcrHigh();
	int     PcrLow();
	__int64 Pcr();
	bool    GetPtsDts(byte* pesHeader, __int64& pts, __int64& dts);
	void		ChangePtsDts(byte* header, __int64 startPcr);
private:
	int m_pcrPid;
	__int64 m_pcrHigh;
	int     m_pcrLow;
};
