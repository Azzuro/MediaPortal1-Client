#include <streams.h>
#include "TsMPEG2TransportFileServerMediaSubsession.h"
#include "SimpleRTPSink.hh"
#include "TsStreamFileSource.hh"
#include "MPEG2TransportStreamFramer.hh"
#include "TsFileDuration.h"
#include "TsStreamFileSource.hh" 

extern void Log(const char *fmt, ...) ;

TsMPEG2TransportFileServerMediaSubsession* TsMPEG2TransportFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource) 
{
  return new TsMPEG2TransportFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

TsMPEG2TransportFileServerMediaSubsession::TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,char const* fileName, Boolean reuseFirstSource)
  : FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
  strcpy(m_fileName,fFileName);
}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* TsMPEG2TransportFileServerMediaSubsession::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
  estBitrate = 15000; // kbps, estimate

  // Create the video source:
  unsigned const inputDataChunkSize= TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (fileSource == NULL) return NULL;
  fFileSize = fileSource->fileSize();
  strcpy(m_fileName,fFileName);

  // Create a framer for the Transport Stream:
  return MPEG2TransportStreamFramer::createNew(envir(), fileSource);
}

RTPSink* TsMPEG2TransportFileServerMediaSubsession::createNewRTPSink(Groupsock* rtpGroupsock,unsigned char /*rtpPayloadTypeIfDynamic*/,FramedSource* /*inputSource*/) 
{
  return SimpleRTPSink::createNew(envir(), rtpGroupsock,
				  33, 90000, "video", "mp2t",
				  1, True, False /*no 'M' bit*/);
}
void TsMPEG2TransportFileServerMediaSubsession::seekStreamSource(FramedSource* inputSource, float seekNPT)
{
  MPEG2TransportStreamFramer* framer=(MPEG2TransportStreamFramer*)inputSource;
  TsStreamFileSource* source=(TsStreamFileSource*)framer->inputSource();
  if (seekNPT==0.0f)
  {
    source->seekToByteAbsolute(0LL);
    return;
  }
  float fileDuration=duration();
  if (seekNPT<0) seekNPT=0;
  if (seekNPT>(fileDuration-0.5f)) seekNPT=(fileDuration-0.5f);
  if (seekNPT <0) seekNPT=0;
  float pos=seekNPT / fileDuration;
  __int64 fileSize=source->fileSize();
  pos*=fileSize;
  pos/=188;
  pos*=188;
  __int64 newPos=(__int64) pos;

  source->seekToByteAbsolute(newPos);
	Log("ts seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
}

float TsMPEG2TransportFileServerMediaSubsession::duration() const
{
  CTsFileDuration duration;
  duration.SetFileName((char*)m_fileName);
  duration.OpenFile();
  duration.UpdateDuration();
  duration.CloseFile();
  return duration.Duration();
}