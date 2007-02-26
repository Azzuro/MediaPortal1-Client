/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class TechnoTrend : IDisposable
  {
    ITechnoTrend _technoTrendInterface = null;
    IntPtr ptrPmt;
    IntPtr _ptrDataInstance;
    DVBSChannel _previousChannel = null;
    /// <summary>
    /// Initializes a new instance of the <see cref="TechnoTrend"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public TechnoTrend(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      _technoTrendInterface = analyzerFilter as ITechnoTrend;
      _technoTrendInterface.SetTunerFilter(tunerFilter);
      ptrPmt = Marshal.AllocCoTaskMem(1024);
      _ptrDataInstance = Marshal.AllocCoTaskMem(1024);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _technoTrendInterface = null;
      Marshal.FreeCoTaskMem(ptrPmt);
      Marshal.FreeCoTaskMem(_ptrDataInstance);
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      if (_technoTrendInterface == null) return false;
      bool yesNo = false;
      _technoTrendInterface.IsCamReady(ref yesNo);
      return yesNo;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is techno trend.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is techno trend; otherwise, <c>false</c>.
    /// </value>
    public bool IsTechnoTrend
    {
      get
      {
        if (_technoTrendInterface == null) return false;
        bool yesNo = false;
        _technoTrendInterface.IsTechnoTrend(ref yesNo);
        return yesNo;
      }
    }

    /// <summary>
    /// Sends the PMT.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <param name="PMTlength">The PM tlength.</param>
    /// <returns></returns>
    public bool SendPMT(byte[] pmt, int PMTlength)
    {
      if (_technoTrendInterface == null) return true;
      bool succeeded = false;
      for (int i = 0; i < PMTlength; ++i)
      {
        Marshal.WriteByte(ptrPmt, i, pmt[i]);
      }

      _technoTrendInterface.DescrambleService(ptrPmt, (short)PMTlength, ref succeeded);
      return succeeded;
    }


    /// <summary>
    /// Instructs the technotrend card to descramble all programs mentioned in subChannels.
    /// </summary>
    /// <param name="subChannels">The sub channels.</param>
    /// <returns></returns>
    public bool DescrambleMultiple(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      if (_technoTrendInterface == null) return true;
      List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();
      bool succeeded = true;
      Dictionary<int, ConditionalAccessContext>.Enumerator en = subChannels.GetEnumerator();
      while (en.MoveNext())
      {
        bool exists = false;
        ConditionalAccessContext context = en.Current.Value;
        foreach (ConditionalAccessContext c in filteredChannels)
        {
          if (c.Channel.Equals(context.Channel)) exists = true;
        }
        if (!exists)
        {
          filteredChannels.Add(context);
        }
      }


      for (int i = 0; i < filteredChannels.Count; ++i)
      {
        ConditionalAccessContext context = filteredChannels[i];
        Marshal.WriteInt16(ptrPmt, 2 * i, (short)context.ServiceId);
      }
      _technoTrendInterface.DescrambleMultiple(ptrPmt, (short)filteredChannels.Count, ref succeeded);
      return succeeded;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_technoTrendInterface == null) return;


      if (_previousChannel != null)
      {
        if (_previousChannel.Frequency == channel.Frequency &&
            _previousChannel.DisEqc == channel.DisEqc &&
            _previousChannel.Polarisation == channel.Polarisation)
        {
          Log.Log.WriteFile("Technotrend: already tuned to diseqc:{0}, frequency:{1}, polarisation:{2}",
              channel.DisEqc, channel.Frequency, channel.Polarisation);
          return;
        }
      }
      _previousChannel = channel;
      int antennaNr = 1;
      switch (channel.DisEqc)
      {
        case DisEqcType.None: // none
          return;
        case DisEqcType.SimpleA: // Simple A
          antennaNr = 1;
          break;
        case DisEqcType.SimpleB: // Simple B
          antennaNr = 2;
          break;
        case DisEqcType.Level1AA: // Level 1 A/A
          antennaNr = 1;
          break;
        case DisEqcType.Level1AB: // Level 1 A/B
          antennaNr = 2;
          break;
        case DisEqcType.Level1BA: // Level 1 B/A
          antennaNr = 3;
          break;
        case DisEqcType.Level1BB: // Level 1 B/B
          antennaNr = 4;
          break;
      }
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	
      Marshal.WriteByte(_ptrDataInstance, 0, 0xE0);//diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 1, 0x10);//diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 2, 0x38);//diseqc command 1. uCommand=0x38


      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      int lnbFrequency = 10600000;
      bool hiBand = true;
      if (parameters.UseDefaultLnbFrequencies)
      {
        switch (channel.BandType)
        {
          case BandType.Universal:
            if (channel.Frequency >= 11700000)
            {
              lnbFrequency = 10600000;
              hiBand = true;
            }
            else
            {
              lnbFrequency = 9750000;
              hiBand = false;
            }
            break;

          case BandType.Circular:
            hiBand = false;
            break;

          case BandType.Linear:
            hiBand = false;
            break;

          case BandType.CBand:
            hiBand = false;
            break;
        }
      }
      else
      {
        if (parameters.LnbSwitchFrequency != 0)
        {
          if (channel.Frequency >= parameters.LnbSwitchFrequency * 1000)
          {
            lnbFrequency = parameters.LnbHighFrequency * 1000;
            hiBand = true;
          }
          else
          {
            lnbFrequency = parameters.LnbLowFrequency * 1000;
            hiBand = false;
          }
        }
        else
        {
          hiBand = false;
          lnbFrequency = parameters.LnbLowFrequency * 1000;
        }
      }
      Log.Log.WriteFile("FireDTV SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, lnb frequency:{3}, polarisation:{4} hiband:{5}",
              channel.DisEqc, antennaNr, channel.Frequency, lnbFrequency, channel.Polarisation, hiBand);


      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((channel.Polarisation == Polarisation.LinearH) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      Marshal.WriteByte(_ptrDataInstance, 3, cmd);
      _technoTrendInterface.SetDisEqc(_ptrDataInstance, 4, 1, 0, (short)channel.Polarisation);
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      return true;
    }
  }
}
