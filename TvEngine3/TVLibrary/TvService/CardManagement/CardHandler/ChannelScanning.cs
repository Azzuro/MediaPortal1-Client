/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  public class ChannelScanning
  {
    ITvCardHandler _cardHandler;
    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public ChannelScanning(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning
    {
      get
      {
        try
        {
          if (_cardHandler.DataBaseCard.Enabled == false) return false;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return false;
          if (_cardHandler.IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
              return RemoteControl.Instance.IsScanning(_cardHandler.DataBaseCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _cardHandler.Card.IsScanning;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }
    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return new List<IChannel>().ToArray();
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return new List<IChannel>().ToArray();
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.Scan(_cardHandler.DataBaseCard.IdCard, channel);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return null;
          }
        }
        ITVScanning scanner = _cardHandler.Card.ScanningInterface;
        if (scanner == null) return null;
        scanner.Reset();
        List<IChannel> channelsFound = scanner.Scan(channel, settings);
        if (channelsFound == null) return null;
        return channelsFound.ToArray();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }
    public IChannel[] ScanNIT(IChannel channel, ScanParameters settings)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return new List<IChannel>().ToArray();
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return new List<IChannel>().ToArray();
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.ScanNIT(_cardHandler.DataBaseCard.IdCard, channel);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return null;
          }
        }
        ITVScanning scanner = _cardHandler.Card.ScanningInterface;
        if (scanner == null) return null;
        scanner.Reset();
        List<IChannel> channelsFound = scanner.ScanNIT(channel, settings);
        if (channelsFound == null) return null;
        return channelsFound.ToArray();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

  }
}
