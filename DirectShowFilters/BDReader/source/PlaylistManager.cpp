/*
 *  Copyright (C) 2005-2011 Team MediaPortal
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

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

CPlaylistManager::CPlaylistManager(void)
{
  LogDebug("Playlist Manager Created");
  m_rtPlaylistOffset = 0LL;
  firstVideo = firstAudio = true;
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
  bool ret;
  // remove old playlists
  //ivecPlaylists it = m_vecPlaylists.begin();
  //while (it!=m_vecPlaylists.end())
  //{
  //  CPlaylist * playlist=*it;
  //  if (playlist->RemoveRedundantClips())
  //  {
  //    it=m_vecPlaylists.erase(it);
  //    delete playlist;
  //  }
  //  else ++it;
  //}

  LogDebug("Playlist Manager new Playlist %d clip %d start %6.3f clipOffset %6.3f Audio %d duration %6.3f",nPlaylist, nClip, firstPacketTime/10000000.0, clipOffsetTime/10000000.0, audioPresent, duration/10000000.0);

  REFERENCE_TIME remainingClipTime = Incomplete();
  REFERENCE_TIME playedDuration = ClipPlayTime();
  ret = remainingClipTime>5000000LL;

  LogDebug("Playlist Manager::TimeStamp Correction changed to %I64d adding %I64d",m_rtPlaylistOffset + playedDuration, playedDuration);

  m_rtPlaylistOffset += playedDuration;

  if (m_vecPlaylists.size()==0)
  {
    //first playlist
    CPlaylist * firstPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    firstPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset);
    m_vecPlaylists.push_back(firstPlaylist);
    m_itCurrentAudioPlayBackPlaylist = m_itCurrentVideoPlayBackPlaylist = m_itCurrentAudioSubmissionPlaylist = m_itCurrentVideoSubmissionPlaylist = m_vecPlaylists.begin();
  }
  else if (m_vecPlaylists.back()->nPlaylist == nPlaylist)
  {
    //new clip in existing playlist
    CPlaylist * existingPlaylist = m_vecPlaylists.back();
    existingPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset);
  }
  else
  {
    //completely new playlist
    CPlaylist * existingPlaylist = m_vecPlaylists.back();
    vector<CClip*> audioLess = existingPlaylist->Superceed();
    if (audioLess.size())
    {
      ivecClip it = audioLess.begin();
      while (it!=audioLess.end())
      {
        CClip * clip=*it;
        m_vecNonFilledClips.push_back(clip);
        ++it;
      }
    }

    CPlaylist * newPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    newPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset);
    
    PushPlaylists();
    m_vecPlaylists.push_back(newPlaylist);
    PopPlaylists(0);

    //mark current playlist as filled
    (*m_itCurrentAudioSubmissionPlaylist)->SetFilledAudio();
    (*m_itCurrentVideoSubmissionPlaylist)->SetFilledVideo();

    //move to this playlist
    m_itCurrentAudioSubmissionPlaylist++;
    m_itCurrentVideoSubmissionPlaylist++;
  }
  return ret; // was current clip interrupted?
}

bool CPlaylistManager::SubmitAudioPacket(Packet * packet)
{
  CAutoLock lock(&m_sectionAudio);
  bool ret = false;
  if (m_vecPlaylists.size()==0) 
  {
    LogDebug("m_currentAudioSubmissionPlaylist is NULL!!!");
    return false;
  }
  if (m_vecNonFilledClips.size())
  {
    ivecClip it = m_vecNonFilledClips.begin();
    while (it!=m_vecNonFilledClips.end())
    {
      CClip * clip=*it;
      if (!((clip->nClip == packet->nClipNumber) && (clip->nPlaylist == packet->nPlaylist)))
      {
        clip->Superceed(SUPERCEEDED_AUDIO_FILL);
        it = m_vecNonFilledClips.erase(it);
      }
      else
      {
        ++it;
      }
    }
  }
  ret=(*m_itCurrentAudioSubmissionPlaylist)->AcceptAudioPacket(packet);
  if (ret) 
  {
#ifdef LOG_AUDIO_PACKETS
    LogDebug("Audio Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
  }
  return ret;
}

bool CPlaylistManager::SubmitVideoPacket(Packet * packet)
{
  CAutoLock lock (&m_sectionVideo);
  bool ret=false;
  if (m_vecPlaylists.size()==0)
  {
    LogDebug("m_currentVideoSubmissionPlaylist is NULL!!!");
    return false;
  }
  ret=(*m_itCurrentVideoSubmissionPlaylist)->AcceptVideoPacket(packet);
  if (ret) 
  {
#ifdef LOG_VIDEO_PACKETS
    LogDebug("Video Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
  }
  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket()
{
  CAutoLock lock (&m_sectionAudio);
  Packet* ret=(*m_itCurrentAudioPlayBackPlaylist)->ReturnNextAudioPacket();
  if (!ret)
  {
    if (m_itCurrentAudioPlayBackPlaylist++ == m_vecPlaylists.end()) m_itCurrentAudioPlayBackPlaylist--;
    else 
    {
      (*(m_itCurrentAudioPlayBackPlaylist--))->SetEmptiedAudio();
      ret = (*(m_itCurrentAudioPlayBackPlaylist++))->ReturnNextAudioPacket();
      LogDebug("playlistManager: setting audio playback playlist to %d",(*m_itCurrentAudioPlayBackPlaylist)->nPlaylist);
    }
  }
  if (firstAudio)
  {
    firstAudio = false;
    ret->bNewClip = false;
  }
  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket(int playlist, int clip)
{
  Packet* ret=NULL;
  if ((*m_itCurrentAudioPlayBackPlaylist)->nPlaylist==playlist)
  {
    ret=(*m_itCurrentAudioPlayBackPlaylist)->ReturnNextAudioPacket(clip);
  }
  return ret;
}


Packet* CPlaylistManager::GetNextVideoPacket()
{
  CAutoLock lock (&m_sectionVideo);
  Packet* ret=(*m_itCurrentVideoPlayBackPlaylist)->ReturnNextVideoPacket();
  if (!ret)
  {
    if (m_itCurrentVideoPlayBackPlaylist++ == m_vecPlaylists.end()) m_itCurrentVideoPlayBackPlaylist--;
    else 
    {
      (*(m_itCurrentVideoPlayBackPlaylist--))->SetEmptiedVideo();
      ret = (*(m_itCurrentVideoPlayBackPlaylist++))->ReturnNextVideoPacket();
      LogDebug("playlistManager: setting video playback playlist to %d",(*m_itCurrentVideoPlayBackPlaylist)->nPlaylist);
    }
  }
  if (firstVideo && ret->rtStart != Packet::INVALID_TIME)
  {
    firstVideo = false;
    ret->bNewClip = false;
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
  m_itCurrentAudioPlayBackPlaylist=m_itCurrentAudioSubmissionPlaylist=m_vecPlaylists.begin();

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
  m_itCurrentVideoPlayBackPlaylist=m_itCurrentVideoSubmissionPlaylist=m_vecPlaylists.begin();
}

bool CPlaylistManager::HasAudio()
{
  if (m_vecPlaylists.size()==0) return false;
  if ((*m_itCurrentAudioPlayBackPlaylist)->HasAudio()) return true;
  if (++m_itCurrentAudioPlayBackPlaylist==m_vecPlaylists.end()) m_itCurrentAudioPlayBackPlaylist--;
  else return (*m_itCurrentAudioPlayBackPlaylist)->HasAudio();
  return false;
}

bool CPlaylistManager::HasVideo()
{
  if (m_vecPlaylists.size()==0) return false;
  if ((*m_itCurrentVideoPlayBackPlaylist)->HasVideo()) return true;
  if (++m_itCurrentVideoPlayBackPlaylist==m_vecPlaylists.end()) m_itCurrentVideoPlayBackPlaylist--;
  else return (*m_itCurrentVideoPlayBackPlaylist)->HasVideo();
  return false;
}

void CPlaylistManager::ClearAllButCurrentClip()
{
  CAutoLock locka (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);

  if (m_vecPlaylists.size()==0) return;
  LogDebug("CPlaylistManager::ClearAllButCurrentClip");
  int deletedPl=0;
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
      deletedPl++;
      it=m_vecPlaylists.erase(it);
      delete playlist;
    }
  }
  if (m_vecPlaylists.size()>0)
  {
    m_itCurrentAudioPlayBackPlaylist = m_itCurrentVideoPlayBackPlaylist = m_itCurrentAudioSubmissionPlaylist = m_itCurrentVideoSubmissionPlaylist = m_vecPlaylists.begin() + (m_vecPlaylists.size()-1);
    m_rtPlaylistOffset += (*m_itCurrentVideoPlayBackPlaylist)->ClearAllButCurrentClip(m_rtPlaylistOffset);
  }
}

REFERENCE_TIME CPlaylistManager::Incomplete()
{
  REFERENCE_TIME ret = 0LL;
  if (!m_vecPlaylists.empty())
  {
    ret = m_vecPlaylists.back()->Incomplete();
  }
    
  return ret;
}

REFERENCE_TIME CPlaylistManager::ClipPlayTime()
{
  REFERENCE_TIME ret = 0LL;
  if (!m_vecPlaylists.empty())
  {
    ret = m_vecPlaylists.back()->PlayedDuration();
  }
    
  return ret;
}

void CPlaylistManager::SetVideoPMT(AM_MEDIA_TYPE *pmt, int nPlaylist, int nClip)
{
  if (pmt)
  {
    LogDebug("CPlaylistManager: Setting video PMT {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X} for (%d, %d)",
	  pmt->subtype.Data1, pmt->subtype.Data2, pmt->subtype.Data3,
      pmt->subtype.Data4[0], pmt->subtype.Data4[1], pmt->subtype.Data4[2],
      pmt->subtype.Data4[3], pmt->subtype.Data4[4], pmt->subtype.Data4[5], 
      pmt->subtype.Data4[6], pmt->subtype.Data4[7], nPlaylist, nClip);
    CPlaylist* pl=GetPlaylist(nPlaylist);
    if (pl)
    {
      pl->SetVideoPMT(pmt, nClip);
    }  
  }
}

void CPlaylistManager::PushPlaylists()
{
  m_itCurrentAudioPlayBackPlaylistPos = m_itCurrentAudioPlayBackPlaylist-m_vecPlaylists.begin();
  m_itCurrentVideoPlayBackPlaylistPos = m_itCurrentVideoPlayBackPlaylist-m_vecPlaylists.begin();
  m_itCurrentAudioSubmissionPlaylistPos = m_itCurrentAudioSubmissionPlaylist-m_vecPlaylists.begin();
  m_itCurrentVideoSubmissionPlaylistPos = m_itCurrentVideoSubmissionPlaylist-m_vecPlaylists.begin();
}

void CPlaylistManager::PopPlaylists(int difference)
{
  if (m_itCurrentAudioPlayBackPlaylistPos - difference <0) m_itCurrentAudioPlayBackPlaylistPos = difference;
  m_itCurrentAudioPlayBackPlaylist = m_vecPlaylists.begin() + (m_itCurrentAudioPlayBackPlaylistPos - difference);
  if (m_itCurrentVideoPlayBackPlaylistPos - difference <0) m_itCurrentVideoPlayBackPlaylistPos = difference;
  m_itCurrentVideoPlayBackPlaylist = m_vecPlaylists.begin() + (m_itCurrentVideoPlayBackPlaylistPos - difference);
  m_itCurrentAudioSubmissionPlaylist = m_vecPlaylists.begin() + (m_itCurrentAudioSubmissionPlaylistPos - difference);
  m_itCurrentVideoSubmissionPlaylist = m_vecPlaylists.begin() + (m_itCurrentVideoSubmissionPlaylistPos - difference);
}

