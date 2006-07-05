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
using System.IO;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using System.Xml;
using DirectShowLib;
using DirectShowLib.BDA;
using MediaPortal.Utils.Services;

namespace MediaPortal.TV.Scanning
{

  /// <summary>
  /// Summary description for DVBCTuning.
  /// </summary>
  public class DVBCTuning : ITuning
  {
    struct DVBCList
    {
      public int frequency;		 // frequency
      public int modulation;	 // modulation
      public string modstr;      // modulation string for display purpose
      public int symbolrate;	 // symbol rate
    }

    TVCaptureDevice _captureCard;
    AutoTuneCallback _callback = null;
    int _currentIndex = -1;
    DVBCList[] _dvbcChannels = new DVBCList[1000];
    int _channelCount = 0;

    int newChannels, updatedChannels;
    int newRadioChannels, updatedRadioChannels;
    protected ILog _log;

    public DVBCTuning()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    #region ITuning Members
   
    public void Start()
    {
      newChannels = 0;
      updatedChannels = 0;
      newRadioChannels = 0;
      updatedRadioChannels = 0;

      _currentIndex = 0;
      _callback.OnProgress(0);
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] parameters)
    {
        String fileName = null;
        if ((parameters == null) || (parameters.Length == 0))
        {
            return;
        }
        else
        {
            fileName = parameters[0];
        }

      newRadioChannels = 0;
      updatedRadioChannels = 0;
      newChannels = 0;
      updatedChannels = 0;
      _captureCard = card;
      _callback = statusCallback;
      _currentIndex = -1;

      _channelCount = 0;
      string line;
      string[] tpdata;
      _log.Info("dvbc-scan:Opening {0}", fileName);
      // load _dvbcChannelsList list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);

      int LineNr = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          LineNr++;
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {
                _dvbcChannels[_channelCount].frequency = Int32.Parse(tpdata[0]);
                string mod = tpdata[1].ToUpper();
                _dvbcChannels[_channelCount].modstr = mod;
                switch (mod)
                {
                  case "1024QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_1024QAM;
                    break;
                  case "112QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_112QAM;
                    break;
                  case "128QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_128QAM;
                    break;
                  case "160QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_160QAM;
                    break;
                  case "16QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_16QAM;
                    break;
                  case "16VSB":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_16VSB;
                    break;
                  case "192QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_192QAM;
                    break;
                  case "224QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_224QAM;
                    break;
                  case "256QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_256QAM;
                    break;
                  case "320QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_320QAM;
                    break;
                  case "384QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_384QAM;
                    break;
                  case "448QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_448QAM;
                    break;
                  case "512QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_512QAM;
                    break;
                  case "640QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_640QAM;
                    break;
                  case "64QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_64QAM;
                    break;
                  case "768QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_768QAM;
                    break;
                  case "80QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_80QAM;
                    break;
                  case "896QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_896QAM;
                    break;
                  case "8VSB":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_8VSB;
                    break;
                  case "96QAM":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_96QAM;
                    break;
                  case "AMPLITUDE":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE;
                    break;
                  case "FREQUENCY":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY;
                    break;
                  case "BPSK":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_BPSK;
                    break;
                  case "OQPSK":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_OQPSK;
                    break;
                  case "QPSK":
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_QPSK;
                    break;
                  default:
                    _dvbcChannels[_channelCount].modulation = (int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
                    break;
                }
                _dvbcChannels[_channelCount].symbolrate = Int32.Parse(tpdata[2]);
                _channelCount += 1;
              }
              catch
              {
                _log.Info("dvbc-scan:Error in line:{0}", LineNr);
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();


      _log.Info("dvbc-scan:loaded:{0} dvbc transponders", _channelCount);
      _currentIndex = 0;
      return;
    }

    public void Next()
    {
      if (_currentIndex  >= _channelCount) return;

      UpdateStatus();
      Tune();
      Scan();
      _currentIndex++;
    }


    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback _callback)
    {
      // TODO:  Add DVBCTuning.AutoTuneRadio implementation
    }


    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBCTuning.MapToChannel implementation
      return 0;
    }
    void UpdateStatus()
    {
      int index = _currentIndex;
      if (index < 0) index = 0;
      float percent = ((float)index) / ((float)_channelCount);
      percent *= 100.0f;
      _callback.OnProgress((int)percent);
    }

    public bool IsFinished()
    {
      if (_currentIndex >= _channelCount)
      {
        return true;
      }
      return false;
    }

    void DetectAvailableStreams()
    {
      DVBCList dvbcChan = _dvbcChannels[_currentIndex];
      string chanDesc = String.Format(" {0} MHz, Modulation:{1} SymbolRate:{2}",
            ((float)_dvbcChannels[_currentIndex].frequency) / 1000.0,
            _dvbcChannels[_currentIndex].modstr,
            _dvbcChannels[_currentIndex].symbolrate / 1000);
      string description = String.Format("Transponder:{0} Getting info...", chanDesc);
      _callback.OnStatus(description);
      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);

      _captureCard.StoreTunedChannels(false, true, ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
      _callback.OnStatus2(String.Format("new tv:{0} new radio:{1} updated tv:{2} updated radio:{3}", newChannels, newRadioChannels, updatedChannels, updatedRadioChannels));
      _callback.UpdateList();
      return;
    }

    void Scan()
    {
      _captureCard.Process();
      if (_captureCard.SignalPresent())
      {
        DetectAvailableStreams();
      }
    }

    void Tune()
    {
      if (_currentIndex < 0 || _currentIndex >= _channelCount)
      {
        return;
      }
      string chanDesc = String.Format(" {0} MHz, Modulation:{1} SymbolRate:{2}",
            ((float)_dvbcChannels[_currentIndex].frequency)/1000.0, 
            _dvbcChannels[_currentIndex].modstr, 
            _dvbcChannels[_currentIndex].symbolrate/1000);
      string description = String.Format("Transponder:{0} locking...", chanDesc);
      _log.Info("dvbc-scan:tune dvbcChannel:{0}/{1} {2}", _currentIndex, _channelCount, chanDesc);
      _callback.OnStatus(description);

      DVBChannel newchan = new DVBChannel();
      newchan.NetworkID = -1;
      newchan.TransportStreamID = -1;
      newchan.ProgramNumber = -1;

      newchan.Modulation = _dvbcChannels[_currentIndex].modulation;
      newchan.Symbolrate = (_dvbcChannels[_currentIndex].symbolrate) / 1000;
      newchan.FEC = (int)FECMethod.MethodNotSet;
      newchan.Frequency = _dvbcChannels[_currentIndex].frequency;
      _captureCard.Tune(newchan, 0);

      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      _log.Info("dvbc-scan:signal quality:{0} signal strength:{1} signal present:{2}",
                  _captureCard.SignalQuality, _captureCard.SignalStrength, _captureCard.SignalPresent());
    }
    #endregion
  }
}
