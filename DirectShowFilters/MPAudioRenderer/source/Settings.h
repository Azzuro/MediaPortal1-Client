// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#pragma once

#include <dsound.h>
#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>

#define MAX_REG_LENGTH 256

enum AC3Encoding
{ 
  DISABLED = 0,
  AUTO,
  FORCED
};

class AudioRendererSettings
{
public:  
  AudioRendererSettings();
  ~AudioRendererSettings();

public:
  bool m_bLogSampleTimes;
  bool m_bHWBasedRefClock;
  bool m_bEnableSyncAdjustment;
  bool m_bUseWASAPI;
  bool m_bWASAPIUseEventMode;
  bool m_bUseTimeStretching;
  int  m_lAC3Encoding;
  
  bool m_bQuality_USE_QUICKSEEK;
  bool m_bQuality_USE_AA_FILTER;
  
  int m_lQuality_AA_FILTER_LENGTH;
  int m_lQuality_SEQUENCE_MS;
  int m_lQuality_SEEKWINDOW_MS;
  int m_lQuality_OVERLAP_MS;

  int m_AC3bitrate;
  double m_dMaxBias;
  double m_dMinBias;

  int m_lAudioDelay;

  int m_nResamplingQuality;

  int m_nForceSamplingRate;
  int m_nForceBitDepth;
  
  REFERENCE_TIME m_hnsPeriod;

  AUDCLNT_SHAREMODE m_WASAPIShareMode;
  
  WCHAR* m_wWASAPIPreferredDeviceId;

private:
   // For accessing the registry
  void LoadSettingsFromRegistry();
  void ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data);
  void WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data);

  bool AllowedValue(unsigned int allowedRates[], unsigned int size, int rate);
  LPCTSTR ResamplingQualityAsString(int setting);
};
