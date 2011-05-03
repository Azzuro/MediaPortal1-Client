﻿#region Copyright (C) 2005-2010 Team MediaPortal

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

namespace MediaPortal.CoreServices
{
  /// <summary>
  /// Interface for all logger implementations.
  /// </summary>
  public interface ILogger
  {
    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref="LogLevel"/> value that indicates the minimum level messages must have to be 
    /// written to the logger.</value>
    LogLevel Level { get; set; }

    /// <summary>
    /// Writes a debug message to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Debug(string format, params object[] args);

    /// <summary>
    /// Writes an informational message to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Info(string format, params object[] args);

    /// <summary>
    /// Writes a warning to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Warn(string format, params object[] args);

    /// <summary>
    /// Writes an error message to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Error(string format, params object[] args);

    /// <summary>
    /// Writes an error <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Error(Exception ex);

    /// <summary>
    /// Writes an error <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Error(string message, Exception ex);

    /// <summary>
    /// Writes an critical message to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Critical(string format, params object[] args);

    /// <summary>
    /// Writes an critical <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Critical(Exception ex);

    /// <summary>
    /// Writes an critical <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Critical(string message, Exception ex);

    /// <summary>
    /// Writes a epg message to the  Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Epg(string format, params object[] args);

  }
}

