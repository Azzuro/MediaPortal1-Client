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
  public class CardTuner
  {
    ITvCardHandler _cardHandler;
    /// <summary>
    /// Initializes a new instance of the <see cref="CardTuner"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public CardTuner(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }


    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public TvResult Tune(ref User user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.Info("card: Tune {0} to {1}", _cardHandler.DataBaseCard.IdCard, channel.Name);
        lock (this)
        {
          if (_cardHandler.IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
              return RemoteControl.Instance.Tune(ref user, channel, idChannel);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at: {0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
              return TvResult.ConnectionToSlaveFailed;
            }
          }

          //@FIX this fails for back-2-back recordings
          //if (CurrentDbChannel(ref user) == idChannel && idChannel >= 0)
          //{
          //  return true;
          //}
          Log.Debug("card: user: {0}:{1}:{2} tune {3}", user.Name, user.CardId, user.SubChannel, channel.ToString());
          _cardHandler.Card.CamType = (CamType)_cardHandler.DataBaseCard.CamType;
          _cardHandler.SetParameters();

          //check if transponder differs
          TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
          if (_cardHandler.Card.SubChannels.Length > 0)
          {
            if (IsTunedToTransponder(channel) == false)
            {
              if (context.IsOwner(user) || user.IsAdmin)
              {
                Log.Debug("card: to different transponder");

                //remove all subchannels, except for this user...
                User[] users = context.Users;
                for (int i = 0; i < users.Length; ++i)
                {
                  if (users[i].Name != user.Name)
                  {
                    Log.Debug("  stop subchannel: {0} user: {1}", i, users[i].Name);

                    //fix for b2b mantis; http://mantis.team-mediaportal.com/view.php?id=1112
                    if (users[i].IsAdmin) // if we are stopping an on-going recording/schedule (=admin), we have to make sure that we remove the schedule also.
                    {
                      Log.Debug("user is scheduler: {0}", users[i].Name);
                      int recScheduleId = RemoteControl.Instance.GetRecordingSchedule(users[i].CardId, users[i].IdChannel);

                      if (recScheduleId > 0)
                      {
                        Schedule schedule = Schedule.Retrieve(recScheduleId);
                        Log.Info("removing schedule with id: {0}", schedule.IdSchedule);
                        RemoteControl.Instance.StopRecordingSchedule(schedule.IdSchedule);
                        schedule.Delete();
                      }
                    }
                    else
                    {
                      _cardHandler.Card.FreeSubChannel(users[i].SubChannel);
                      context.Remove(users[i]);
                    }
                  }
                }
              }
              else
              {
                Log.Debug("card: user: {0} is not the card owner. Cannot switch transponder", user.Name);
                return TvResult.NotTheOwner;
              }
            }
          }

          result = _cardHandler.Card.Tune(user.SubChannel, channel);

          bool isLocked = _cardHandler.Card.IsTunerLocked;
          Log.Debug("card: Tuner locked: {0}", isLocked);

          Log.Info("**************************************************");
          Log.Info("***** SIGNAL LEVEL: {0}, SIGNAL QUALITY: {1} *****", _cardHandler.Card.SignalLevel, _cardHandler.Card.SignalQuality);
          Log.Info("**************************************************");

          if (result != null)
          {
            Log.Debug("card: tuned user: {0} subchannel: {1}", user.Name, result.SubChannelId);
            user.SubChannel = result.SubChannelId;
            user.IdChannel = idChannel;
            context.Add(user);
          }
          else if (result == null)
          {
            _cardHandler.Card.FreeSubChannel(result.SubChannelId);
            return TvResult.AllCardsBusy;
          }
          
          //no need to recheck if signal is ok, this is done sooner now.
          /*if (!isLocked)
          {
            _cardHandler.Card.FreeSubChannel(result.SubChannelId);
            return TvResult.NoSignalDetected;
          } 
          */         

          if (result.IsTimeShifting || result.IsRecording)
          {
            context.OnZap(user);
          }
          return TvResult.Succeeded;
        }
      }
      catch (TvExceptionNoSignal tvex)
      {
        _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        return TvResult.NoSignalDetected;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        return TvResult.UnknownError;
      }
    }


    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(ref User user, IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
        
				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return TvResult.CardIsDisabled;
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return TvResult.UnknownError;
				}

        TvResult result;
        Log.WriteFile("card: CardTune {0} {1} {2}:{3}:{4}", _cardHandler.DataBaseCard.IdCard, channel.Name, user.Name, user.CardId, user.SubChannel);
        if (_cardHandler.IsScrambled(ref user))
        {
          result = Tune(ref user, channel, dbChannel.IdChannel);
          Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
          return result;
        }
        if (_cardHandler.CurrentDbChannel(ref user) == dbChannel.IdChannel && dbChannel.IdChannel >= 0)
        {
          return TvResult.Succeeded;
        }
        result = Tune(ref user, channel, dbChannel.IdChannel);
        Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
        return result;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Determines whether card is tuned to the transponder specified by transponder
    /// </summary>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTunedToTransponder(IChannel transponder)
    {
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          return RemoteControl.Instance.IsTunedToTransponder(_cardHandler.DataBaseCard.IdCard, transponder);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
      }
      ITvSubChannel[] subchannels = _cardHandler.Card.SubChannels;
      if (subchannels == null) return false;
      if (subchannels.Length == 0) return false;
      if (subchannels[0].CurrentChannel == null) return false;
      return (false == IsDifferentTransponder(subchannels[0].CurrentChannel, transponder));
    }



    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {

      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return false;

				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return false;

					if (_cardHandler.IsLocal == false)
					{
						return RemoteControl.Instance.CanTune(_cardHandler.DataBaseCard.IdCard, channel);
					}
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return false;
				}
        return _cardHandler.Card.CanTune(channel);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }
    /// <summary>
    /// Determines whether transponder 1 is the same as transponder2
    /// </summary>
    /// <param name="transponder1">The transponder1.</param>
    /// <param name="transponder2">The transponder2.</param>
    /// <returns>
    /// 	<c>true</c> if transponder 1 is not equal to transponder 2; otherwise, <c>false</c>.
    /// </returns>
    public bool IsDifferentTransponder(IChannel transponder1, IChannel transponder2)
    {
      DVBCChannel dvbcChannelNew = transponder2 as DVBCChannel;
      // Check the type of the channel. If they are different, than we are definitely 
      // on a different transponder. This could happen with hybrid card.
      if (!transponder1.GetType().Equals(transponder2.GetType())) return true;
      if (dvbcChannelNew != null)
      {
        DVBCChannel dvbcChannelCurrent = transponder1 as DVBCChannel;
        if (dvbcChannelNew.Frequency != dvbcChannelCurrent.Frequency) return true;
        return false;
      }

      DVBTChannel dvbtChannelNew = transponder2 as DVBTChannel;
      if (dvbtChannelNew != null)
      {
        DVBTChannel dvbtChannelCurrent = transponder1 as DVBTChannel;
        if (dvbtChannelNew.Frequency != dvbtChannelCurrent.Frequency) return true;
        return false;
      }

      DVBSChannel dvbsChannelNew = transponder2 as DVBSChannel;
      if (dvbsChannelNew != null)
      {
        DVBSChannel dvbsChannelCurrent = transponder1 as DVBSChannel;
        if (dvbsChannelNew.Frequency != dvbsChannelCurrent.Frequency) return true;
        if (dvbsChannelNew.Polarisation != dvbsChannelCurrent.Polarisation) return true;
        if (dvbsChannelNew.ModulationType != dvbsChannelCurrent.ModulationType) return true;
        if (dvbsChannelNew.SatelliteIndex != dvbsChannelCurrent.SatelliteIndex) return true;
        if (dvbsChannelNew.InnerFecRate != dvbsChannelCurrent.InnerFecRate) return true;
        if (dvbsChannelNew.Pilot != dvbsChannelCurrent.Pilot) return true;
        if (dvbsChannelNew.Rolloff != dvbsChannelCurrent.Rolloff) return true;
        if (dvbsChannelNew.DisEqc != dvbsChannelCurrent.DisEqc) return true;
        return false;
      }

      ATSCChannel atscChannelNew = transponder2 as ATSCChannel;
      if (atscChannelNew != null)
      {
        ATSCChannel atscChannelCurrent = transponder1 as ATSCChannel;
        if (atscChannelNew.MajorChannel != atscChannelCurrent.MajorChannel) return true;
        if (atscChannelNew.MinorChannel != atscChannelCurrent.MinorChannel) return true;
        if (atscChannelNew.PhysicalChannel != atscChannelCurrent.PhysicalChannel) return true;
        return false;
      }

      AnalogChannel analogChannelNew = transponder2 as AnalogChannel;
      if (analogChannelNew != null)
      {
        AnalogChannel analogChannelCurrent = transponder1 as AnalogChannel;
        if (analogChannelNew.IsTv != analogChannelCurrent.IsTv) return true;
        if (analogChannelNew.IsRadio != analogChannelCurrent.IsRadio) return true;
        if (analogChannelNew.Country.Id != analogChannelCurrent.Country.Id) return true;
        if (analogChannelNew.VideoSource != analogChannelCurrent.VideoSource) return true;
        if (analogChannelNew.TunerSource != analogChannelCurrent.TunerSource) return true;
        if (analogChannelNew.ChannelNumber != analogChannelCurrent.ChannelNumber) return true;
        if (analogChannelNew.Frequency != analogChannelCurrent.Frequency) return true;
        return false;
      }

      return false;
    }
  }

}
