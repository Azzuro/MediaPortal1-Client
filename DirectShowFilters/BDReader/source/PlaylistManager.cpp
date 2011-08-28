/*
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include "PlaylistManager.h"

extern void LogDebug(const char *fmt, ...);

CPlaylistManager::CPlaylistManager(void)
{
  LogDebug("Playlist Manager Created");
  m_currentAudioPlayBackPlaylist=NULL;
  m_currentVideoPlayBackPlaylist=NULL;
  m_currentAudioSubmissionPlaylist=NULL;
  m_currentVideoSubmissionPlaylist=NULL;
  m_VideoPacketsUntilLatestplaylist=0;
  AudioPackets=0;
  VideoPackets=0;
  m_bInteruption=false;
}


CPlaylistManager::~CPlaylistManager(void)
{
  CAutoLock lock (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);
  LogDebug("Playlist Manager Closing");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    it=m_vecPlaylists.erase(it);
    delete playlist;
  }
}

bool CPlaylistManager::CreateNewPlaylistClip(int nPlaylist, int nClip, bool audioPresent, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffsetTime, REFERENCE_TIME duration, bool discontinuousClip)
{
  CAutoLock lock (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);
  
  LogDebug("Playlist Manager new Playlist %d clip %d start %6.3f Audio %d duration %6.3f",nPlaylist, nClip, firstPacketTime/10000000.0, audioPresent, duration/10000000.0);
  if (m_vecPlaylists.size()==0)
  {
    //first playlist
    CPlaylist * firstPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    firstPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, false);
    m_vecPlaylists.push_back(firstPlaylist);
    m_currentAudioPlayBackPlaylist=m_currentVideoPlayBackPlaylist=m_currentAudioSubmissionPlaylist=m_currentVideoSubmissionPlaylist=firstPlaylist;

  }
  else if (m_vecPlaylists.back()->nPlaylist == nPlaylist)
  {
    //new clip in existing playlist
    CPlaylist * existingPlaylist = m_vecPlaylists.back();
    existingPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, discontinuousClip);
  }
  else
  {
    //completely new playlist
    CPlaylist * newPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    newPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, false);
    m_vecPlaylists.push_back(newPlaylist);
  }
  
  return Incomplete(); // was current clip interrupted?
}

bool CPlaylistManager::SubmitAudioPacket(Packet * packet)
{
  CAutoLock lock (&m_sectionAudio);
  bool ret = false;
  if (m_currentAudioSubmissionPlaylist==NULL) 
  {
    LogDebug("m_currentAudioSubmissionPlaylist is NULL!!!");
    return false;
  }
  if (m_currentAudioSubmissionPlaylist->nPlaylist==packet->nPlaylist)
  {
    ret=m_currentAudioSubmissionPlaylist->AcceptAudioPacket(packet, true);
  }
  if (ret) 
  {
    AudioPackets++;
#ifdef LOG_AUDIO_PACKETS
    LogDebug("Audio Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
  }
  if (!ret)
  {
    CPlaylist* nextPlaylist = GetNextAudioSubmissionPlaylist(m_currentAudioSubmissionPlaylist);
    if (nextPlaylist==m_currentAudioSubmissionPlaylist)
    {
      ret=m_currentAudioSubmissionPlaylist->AcceptAudioPacket(packet, true);
      AudioPackets++;
//      LogDebug("Audio Packet %I64d Forced in %d %d", packet->rtStart, m_currentAudioSubmissionPlaylist->nPlaylist,m_currentAudioSubmissionPlaylist->CurrentAudioSubmissionClip());
    }
    else
    {
      m_currentAudioSubmissionPlaylist->SetFilledAudio();
      m_currentAudioSubmissionPlaylist = nextPlaylist;
      ret=SubmitAudioPacket(packet);
    }
  }

  return ret;
}

bool CPlaylistManager::SubmitVideoPacket(Packet * packet)
{
  CAutoLock lock (&m_sectionVideo);
  bool ret=false;
  if (m_currentVideoSubmissionPlaylist==NULL)
  {
    LogDebug("m_currentVideoSubmissionPlaylist is NULL!!!");
    return false;
  }
  if (m_currentVideoSubmissionPlaylist->nPlaylist==packet->nPlaylist)
  {
    ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,false,true);
  }
  if (ret) 
  {
    VideoPackets++;
#ifdef LOG_VIDEO_PACKETS
    AM_MEDIA_TYPE* pmt = packet->pmt;

    LogDebug("Video Packet %I64d Accepted in %d %d - format %d {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}", 
      packet->rtStart, packet->nPlaylist, packet->nClipNumber, 
      pmt->cbFormat, pmt->subtype.Data1, pmt->subtype.Data2, pmt->subtype.Data3,
      pmt->subtype.Data4[0], pmt->subtype.Data4[1], pmt->subtype.Data4[2],
      pmt->subtype.Data4[3], pmt->subtype.Data4[4], pmt->subtype.Data4[5], 
      pmt->subtype.Data4[6], pmt->subtype.Data4[7]);
#endif
  }
  if (!ret)
  {
#ifdef LOG_VIDEO_PACKETS
     LogDebug("Video Packet %I64d %d %d rejected from %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber, m_currentVideoSubmissionPlaylist->nPlaylist);
#endif

    CPlaylist* nextPlaylist = GetNextVideoSubmissionPlaylist(m_currentVideoSubmissionPlaylist);
    if (nextPlaylist == m_currentVideoSubmissionPlaylist)
    {
//      LogDebug("Failed to find video submission playlist");
      ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,false,true);
      VideoPackets++;
//      LogDebug("Video Packet %I64d Forced in %d %d", packet->rtStart, m_currentVideoSubmissionPlaylist->nPlaylist,m_currentVideoSubmissionPlaylist->CurrentVideoSubmissionClip());
    }
    else
    {
      m_currentVideoSubmissionPlaylist->SetFilledVideo();
      m_currentVideoSubmissionPlaylist = nextPlaylist;
      ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,true, false);
    }
    if (ret) VideoPackets++;
  }
  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket()
{
  CAutoLock lock (&m_sectionAudio);
  Packet* ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket();
  if (ret!=NULL && !m_currentAudioPlayBackPlaylist->IsFakingAudio()) AudioPackets--;
  if (ret==NULL)
  {
    CPlaylist* nextPlaylist = GetNextAudioPlaylist(m_currentAudioPlayBackPlaylist);
    if (m_currentAudioPlayBackPlaylist!=nextPlaylist)
    {
      m_currentAudioPlayBackPlaylist->SetEmptiedAudio();
      m_currentAudioPlayBackPlaylist = nextPlaylist;
      ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket();
      if (ret!=NULL && !m_currentAudioPlayBackPlaylist->IsFakingAudio()) AudioPackets--;
    }
  }
  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket(int playlist, int clip)
{
  Packet* ret=NULL;
//  CAutoLock lock (&m_sectionAudio);
  if (m_currentAudioPlayBackPlaylist->nPlaylist==playlist)
  {
    ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket(clip);
    if (ret!=NULL && !m_currentAudioPlayBackPlaylist->IsFakingAudio()) AudioPackets--;
  }
  return ret;
}


Packet* CPlaylistManager::GetNextVideoPacket()
{
  CAutoLock lock (&m_sectionVideo);
  Packet* ret=m_currentVideoPlayBackPlaylist->ReturnNextVideoPacket();
  if (ret!=NULL) VideoPackets--;
  if (ret==NULL)
  {
    CPlaylist* nextPlaylist = GetNextVideoPlaylist(m_currentVideoPlayBackPlaylist);

    if (m_currentVideoPlayBackPlaylist!=nextPlaylist)
    {
      m_currentVideoPlayBackPlaylist->SetEmptiedVideo();
      m_currentVideoPlayBackPlaylist=nextPlaylist;
      ret=m_currentVideoPlayBackPlaylist->ReturnNextVideoPacket();
      if (ret!=NULL) VideoPackets--;
    }
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextAudioPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.end();
  while (it!=m_vecPlaylists.begin())
  {
    --it;
    CPlaylist * playlist=*it;
    if (!playlist->IsEmptiedAudio() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
//      LogDebug("Next Audio Playlist %d HasAudio %d",playlist->nPlaylist,playlist->IsFakingAudio());
      return playlist;
    }
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetPlaylist(int nPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = NULL;
  ivecPlaylists it = m_vecPlaylists.end();
  while (it!=m_vecPlaylists.begin())
  {
    --it;
    CPlaylist * playlist=*it;
    if (playlist->nPlaylist == nPlaylist)
    {
//      LogDebug("Next Audio Playlist %d HasAudio %d",playlist->nPlaylist,playlist->IsFakingAudio());
      return playlist;
    }
  }
  LogDebug("Playlist %d not found!",nPlaylist);
  return ret;
}

CPlaylist * CPlaylistManager::GetNextVideoPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionVideo);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsEmptiedVideo() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
//      LogDebug("Next Video Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextAudioSubmissionPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsFilledAudio() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
      LogDebug("Playlist Manager New Audio Submission Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextVideoSubmissionPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionVideo);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsFilledVideo() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
      LogDebug("Playlist Manager New Video Submission Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

void CPlaylistManager::FlushAudio(void)
{
  CAutoLock lock (&m_sectionAudio);
  LogDebug("Playlist Manager Flush Audio");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    playlist->FlushAudio();
    ++it;
  }
  m_currentAudioPlayBackPlaylist=m_currentAudioSubmissionPlaylist=*m_vecPlaylists.begin();

}

void CPlaylistManager::FlushVideo(void)
{
  CAutoLock lock (&m_sectionVideo);
  LogDebug("Playlist Manager Flush Video");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    playlist->FlushVideo();
    ++it;
  }
}

bool CPlaylistManager::HasAudio()
{
  if (m_currentAudioPlayBackPlaylist==NULL) return false;
  if (m_currentAudioPlayBackPlaylist->HasAudio()) return true;
  CPlaylist * nextPlaylist = GetNextAudioPlaylist(m_currentAudioPlayBackPlaylist);
  if (nextPlaylist!=m_currentAudioPlayBackPlaylist)
  {
    return nextPlaylist->HasAudio();
  }
  return false;
}
bool CPlaylistManager::HasVideo()
{
  if (m_currentVideoPlayBackPlaylist==NULL) return false;
  if (m_currentVideoPlayBackPlaylist->HasVideo()) return true;
  CPlaylist * nextPlaylist = GetNextVideoPlaylist(m_currentVideoPlayBackPlaylist);
  if (nextPlaylist!=m_currentVideoPlayBackPlaylist)
  {
    return nextPlaylist->HasVideo();
  }
  return false;
}

void CPlaylistManager::ClearAllButCurrentClip(bool resetClip)
{
  CAutoLock locka (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);

  LogDebug("CPlaylistManager::ClearAllButCurrentClip");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (playlist==m_vecPlaylists.back())
    {
      ++it;
    }
    else
    {
      it=m_vecPlaylists.erase(it);
      delete playlist;
    }
  }
  if (m_vecPlaylists.size()>0)
  {
    m_currentAudioPlayBackPlaylist=m_currentVideoPlayBackPlaylist=m_currentAudioSubmissionPlaylist=m_currentVideoSubmissionPlaylist=m_vecPlaylists.back();
    m_currentAudioPlayBackPlaylist->ClearAllButCurrentClip(resetClip);
  }
  VideoPackets=VideoPacketCount();
  AudioPackets=AudioPacketCount();
}

int CPlaylistManager::AudioPacketCount()
{
  int nAudPackets=0;
  if (m_currentAudioPlayBackPlaylist==NULL) return false;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    nAudPackets+= playlist->VideoPacketCount();
    ++it;
  }

  return nAudPackets;
}

int CPlaylistManager::VideoPacketCount()
{
  int nVidPackets = 0;
  if (m_currentVideoPlayBackPlaylist==NULL) return false;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    nVidPackets+= playlist->VideoPacketCount();
    ++it;
  }
  return nVidPackets;
}

bool CPlaylistManager::Incomplete()
{
  bool ret = false;
  if (!m_vecPlaylists.empty() && m_currentAudioPlayBackPlaylist != m_vecPlaylists.back())
  {
    ret = m_currentAudioPlayBackPlaylist->Incomplete();
  }
    
  return ret;
}

void CPlaylistManager::SetPMT(AM_MEDIA_TYPE *pmt, int nPlaylist, int nClip)
{
  LogDebug("Setting PMT %d for (%d,%d)",pmt->cbFormat, nPlaylist, nClip);
  CPlaylist* pl=GetPlaylist(nPlaylist);
  if (pl!=NULL)
  {
    pl->SetPmt(pmt, nClip);
  }
}
