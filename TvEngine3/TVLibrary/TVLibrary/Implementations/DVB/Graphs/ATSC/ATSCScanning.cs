/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using DirectShowLib;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for ATSC BDA cards
  /// </summary>
  public class ATSCScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardATSC _card;
    /// <summary>
    /// Initializes a new instance of the <see cref="T:ATSCScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public ATSCScanning(TvCardATSC card)
      : base(card)
    {
      _card = card;
    }

    /// <summary>
    /// returns the tv card used
    /// </summary>
    /// <value></value>
    public ITVCard TvCard
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets the analyzer.
    /// </summary>
    /// <returns></returns>
    protected override ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }
    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      ATSCChannel tuningChannel = (ATSCChannel)_card.Channel;
      ATSCChannel atscChannel = new ATSCChannel();
      atscChannel.Name = info.service_name;
      atscChannel.LogicalChannelNumber = info.LCN;
      atscChannel.Provider = info.service_provider_name;
      atscChannel.SymbolRate = tuningChannel.SymbolRate;
      atscChannel.ModulationType = tuningChannel.ModulationType;
      atscChannel.Frequency = tuningChannel.Frequency;
      atscChannel.PhysicalChannel = tuningChannel.PhysicalChannel;
      atscChannel.MajorChannel = info.majorChannel;
      atscChannel.MinorChannel = info.minorChannel;
      atscChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
      atscChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      atscChannel.TransportId = info.transportStreamID;
      atscChannel.PmtPid = info.network_pmt_PID;
      atscChannel.PcrPid = info.pcr_pid;
      atscChannel.FreeToAir = !info.scrambled;
      foreach (PidInfo pid in info.pids)
      {
        if (pid.isAC3Audio )
        {
          if (pid.pid > 0)
          {
            atscChannel.AudioPid = pid.pid;
          }
        }
        if (pid.isVideo)
          atscChannel.VideoPid = pid.pid;
      }
      Log.Log.Write("atsc:Found: {0}", atscChannel);
      return atscChannel;
    }
  }
}
