/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 2.1 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2006 Live Networks, Inc.  All rights reserved.
// A filter that passes through (unchanged) chunks that contain an integral number
// of MPEG-2 Transport Stream packets, but returning (in "fDurationInMicroseconds")
// an updated estimate of the time gap between chunks.
// C++ header

#ifndef _MPEG2_TRANSPORT_STREAM_FRAMER_HH
#define _MPEG2_TRANSPORT_STREAM_FRAMER_HH

#ifndef _FRAMED_FILTER_HH
#include "FramedFilter.hh"
#endif

#ifndef _HASH_TABLE_HH
#include "HashTable.hh"
#endif
class IOnDelete
{
public: 
  virtual void OnDelete()=0;
};

class MPEG2TransportStreamFramer: public FramedFilter {
public:
  static MPEG2TransportStreamFramer*
  createNew(UsageEnvironment& env, FramedSource* inputSource);

  unsigned long tsPacketCount() const { return fTSPacketCount; }
  void SetOnDelete(IOnDelete* onDelete);
protected:
  MPEG2TransportStreamFramer(UsageEnvironment& env, FramedSource* inputSource);
      // called only by createNew()
  virtual ~MPEG2TransportStreamFramer();

private:
  // Redefined virtual functions:
  virtual void doGetNextFrame();
  virtual void doStopGettingFrames();

private:
  static void afterGettingFrame(void* clientData, unsigned frameSize,
				unsigned numTruncatedBytes,
				struct timeval presentationTime,
				unsigned durationInMicroseconds);
  void afterGettingFrame1(unsigned frameSize,
			  struct timeval presentationTime);

  void updateTSPacketDurationEstimate(unsigned char* pkt, double timeNow);

private:
  unsigned long fTSPacketCount;
  double fTSPacketDurationEstimate;
  HashTable* fPIDStatusTable;
  IOnDelete* m_pOnDelete;
};

#endif
