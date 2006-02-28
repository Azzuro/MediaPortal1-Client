/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{
  public class StopRecordingCommand : CardCommand
  {
    public override void Execute(CommandProcessor handler)
    {
      Log.WriteFile(Log.LogType.Recorder, "Command:Stop recording");
      
      if (handler.TVCards.Count == 0)
      {
        ErrorMessage="No tuner cards installed";
        Succeeded = false;
        return;
      }
      //get the current selected card
      if (handler.CurrentCardIndex < 0 || handler.CurrentCardIndex >= handler.TVCards.Count) return;
      TVCaptureDevice dev = handler.TVCards[handler.CurrentCardIndex];

      //is it recording?
      if (dev.IsRecording == false)
      {
        Succeeded = false;
        ErrorMessage = "Tuner is not recording";
        return;
      }
      //yes. then cancel the recording
      Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", dev.CommercialName, dev.TVChannel);
      int ID = dev.CurrentTVRecording.ID;

      if (dev.CurrentTVRecording.RecType == TVRecording.RecordingType.Once)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel recording");
        dev.CurrentTVRecording.Canceled = Utils.datetolong(DateTime.Now);
      }
      else
      {
        long datetime = Utils.datetolong(DateTime.Now);
        TVProgram prog = dev.CurrentProgramRecording;
        Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel {0}", prog);

        if (prog != null)
        {
          datetime = Utils.datetolong(prog.StartTime);
          Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel serie {0} {1} {2}", prog.Title, prog.StartTime.ToLongDateString(), prog.StartTime.ToLongTimeString());
        }
        else
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel series");
        }
        dev.CurrentTVRecording.CanceledSeries.Add(datetime);
      }
      TVDatabase.UpdateRecording(dev.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);

      //and tell the card to stop the recording
      dev.StopRecording();

      CheckRecordingsCommand cmd = new CheckRecordingsCommand();
      handler.AddCommand(cmd);
      Succeeded = true;
    }
  }
}
