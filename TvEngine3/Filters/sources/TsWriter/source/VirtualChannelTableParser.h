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
#include "sectiondecoder.h"
#include "channelinfo.h"
#include "pidtable.h"
#include <vector>
using namespace std;
class CVirtualChannelTableParser :
  public CSectionDecoder
{
public:
  CVirtualChannelTableParser(void);
  virtual ~CVirtualChannelTableParser(void);

  void  Reset();
	void  OnNewSection(CSection** sections, int maxSections);

  int   Count();
  bool  GetChannelInfo(int serviceId,CChannelInfo& info);

private:
  void DecodeServiceLocationDescriptor( byte* buf,int start,CChannelInfo& channelInfo);
  void DecodeExtendedChannelNameDescriptor( byte* buf,int start,CChannelInfo& channelInfo, int maxLen);
  char* DecodeMultipleStrings(byte* buf, int offset, int maxLen);
  char* DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes);
  vector<CChannelInfo> m_vecChannels;
  int m_iVctVersion;
};
