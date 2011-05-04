#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#endregion

using System;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for ATSC BDA cards
  /// </summary>
  public class ATSCScanning : DvbBaseScanning
  {
    /// <summary>
    /// ATSC service types - see A/53 part 1
    /// </summary>
    protected enum AtscServiceType
    {
      /// <summary>
      /// Analog Television (See A/65 [9])
      /// </summary>
      AnalogTelevision = 0x01,
      /// <summary>
      /// ATSC Digital Television (See A/53-3 [2])
      /// </summary>
      DigitalTelevision = 0x02,
      /// <summary>
      /// ATSC Audio (See A/53-3 [2])
      /// </summary>
      Audio = 0x03
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ATSCScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public ATSCScanning(TvCardDvbBase card)
      : base(card)
    {
      _enableWaitForVCT = true;
    }

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      ATSCChannel tuningChannel = (ATSCChannel)_card.CurrentChannel;
      ATSCChannel atscChannel = new ATSCChannel();
      atscChannel.Name = info.service_name;
      atscChannel.LogicalChannelNumber = info.LCN;
      atscChannel.Provider = info.service_provider_name;
      atscChannel.ModulationType = tuningChannel.ModulationType;
      atscChannel.Frequency = tuningChannel.Frequency;
      atscChannel.PhysicalChannel = tuningChannel.PhysicalChannel;
      atscChannel.MajorChannel = info.majorChannel;
      atscChannel.MinorChannel = info.minorChannel;
      atscChannel.IsTv = IsTvService(info.serviceType);
      atscChannel.IsRadio = IsRadioService(info.serviceType);
      atscChannel.NetworkId = info.networkID;
      atscChannel.ServiceId = info.serviceID;
      atscChannel.TransportId = info.transportStreamID;
      atscChannel.PmtPid = info.network_pmt_PID;
      atscChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("atsc:Found: {0}", atscChannel);
      return atscChannel;
    }

    protected override void SetNameForUnknownChannel(IChannel channel, ChannelInfo info)
    {
      if (((ATSCChannel)channel).Frequency > 0)
      {
        Log.Log.Info("DVBBaseScanning: service_name is null so now = Unknown {0}-{1}",
                     ((ATSCChannel)channel).Frequency, info.serviceID);
        info.service_name = String.Format("Unknown {0}-{1:X}", ((ATSCChannel)channel).Frequency,
                                          info.serviceID);
      }
      else
      {
        Log.Log.Info("DVBBaseScanning: service_name is null so now = Unknown {0}-{1}",
                     ((ATSCChannel)channel).PhysicalChannel, info.serviceID);
        info.service_name = String.Format("Unknown {0}-{1:X}", ((ATSCChannel)channel).PhysicalChannel,
                                          info.serviceID);
      }
    }

    protected override bool IsRadioService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.Audio;
    }

    protected override bool IsTvService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.AnalogTelevision ||
             serviceType == (int)AtscServiceType.DigitalTelevision;
    }
  }
}