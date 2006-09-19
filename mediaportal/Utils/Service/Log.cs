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
using System.Threading;

namespace MediaPortal.Utils.Services
{
    public class Log : ILog
    {
        private Level _minLevel;
        private TextWriter _logStream;

        public enum Level
        {
            Error = 0,
            Warning = 1,
            Information = 2,
            Debug = 3
        }

        private string GetLevelName(Level logLevel)
        {
            switch (logLevel)
            {
                case Level.Error:
                    return "ERROR";

                case Level.Warning:
                    return "Warn.";

                case Level.Information:
                    return "Info.";

                case Level.Debug:
                    return "Debug";
            }

            return "Unknown";
        }

        public Log(TextWriter stream, Level minLevel)
        {
            _minLevel = minLevel;
            _logStream = stream;
        }

        public Log(string name, Level minLevel)
        {
            _minLevel = minLevel;
            LogFile file = new LogFile(name);
            _logStream = file.GetStream();
        }

        public void Info(string format, params object[] arg)
        {
            Write(Level.Information, format, arg);
        }

        public void Warn(string format, params object[] arg)
        {
            Write(Level.Warning, format, arg);
        }

        public void Error(string format, params object[] arg)
        {
            Write(Level.Error, format, arg);
        }

        public void Debug(string format, params object[] arg)
        {
            Write(Level.Debug, format, arg);
        }

        public void InfoThread(string format, params object[] arg)
        {
            WriteThread(Level.Information, format, arg);
        }

        public void WarnThread(string format, params object[] arg)
        {
            WriteThread(Level.Warning, format, arg);
        }

        public void ErrorThread(string format, params object[] arg)
        {
            WriteThread(Level.Error, format, arg);
        }

        public void DebugThread(string format, params object[] arg)
        {
            WriteThread(Level.Debug, format, arg);
        }

        public void Error(Exception ex)
        {
            Write(Level.Error, "Exception   :{0}", ex.ToString());
            Write(Level.Error, "Exception   :{0}", ex.Message);
            Write(Level.Error, "  site      :{0}", ex.TargetSite);
            Write(Level.Error, "  source    :{0}", ex.Source);
            Write(Level.Error, "  stacktrace:{0}", ex.StackTrace);
        }

        private void WriteThread(Level logLevel, string format, params object[] arg)
        {
            // uncomment the following four lines to help identify the calling method, this
            // is useful in situations where an unreported exception causes problems
            //		StackTrace stackTrace = new StackTrace();
            //		StackFrame stackFrame = stackTrace.GetFrame(1);
            //		MethodBase methodBase = stackFrame.GetMethod();
            //		Write(logLevel, "{0}", methodBase.Name);
            String log = String.Format("{0:X} {1}",
                                       Thread.CurrentThread.ManagedThreadId, String.Format(format, arg));
            Write(logLevel, log);
        }

        private void Write(Level logLevel, string format, params object[] arg)
        {
            if (logLevel <= _minLevel)
            {
                StringWriter message = new StringWriter();

                // Build the log message
                // Add time stamp
                message.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " +
                              String.Format("{0:000}", DateTime.Now.Millisecond));
                // Add LogLevel
                message.Write(" [" + GetLevelName(logLevel) + "] ");
                // Write Log Message
                message.Write(format, arg);

                // Write message to log stream
                _logStream.WriteLine(message.ToString());
            }
        }

        public void Dispose()
        {
            if (_logStream == null)
            {
                return;
            }
            _logStream.Close();
            _logStream.Dispose();
            _logStream = null;
        }
    }
}