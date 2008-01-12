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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Time shifting mode types
  /// </summary>
  public enum TimeShiftingMode : short
  {
    /// <summary>
    /// use mpeg-2 program stream for timeshifting files
    /// </summary>
    ProgramStream = 0,
    /// <summary>
    /// use mpeg-2 transport stream for timeshifting files
    /// </summary>
    TransportStream = 1
  }
  /// <summary>
  /// interface to the timeshift com object
  /// </summary>
  [ComVisible(true), ComImport,
Guid("89459BF6-D00E-4d28-928E-9DA8F76B6D3A"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTimeShift
  {
    /// <summary>
    /// Sets the PCR pid.
    /// </summary>
    /// <param name="pcrPid">The PCR pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPcrPid(short pcrPid);
    /// <summary>
    /// Adds a stream.
    /// </summary>
    /// <param name="pid">The pid.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="language">The language.</param>
    /// <returns></returns>
    [PreserveSig]
    int AddStream(short pid, short serviceType,[In, MarshalAs(UnmanagedType.LPStr)] string language);

    /// <summary>
    /// Adds a stream.
    /// </summary>
    /// <param name="pid">The pid.</param>
    /// <param name="data">Original descriptor data (will be re-used in fake PMT)</param>
    /// <returns></returns>
    [PreserveSig]
    int AddStreamWithDescriptor(short pid, IntPtr data);

    /// <summary>
    /// Removes a stream.
    /// </summary>
    /// <param name="pid">The pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int RemoveStream(short pid);
    /// <summary>
    /// Sets the name of the time shifting file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTimeShiftingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    /// <summary>
    /// Starts timeshifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Start();
    /// <summary>
    /// Stops timeshifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Stop();
    /// <summary>
    /// Resets the timeshifting .
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();
    /// <summary>
    /// Gets the size of the buffer.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetBufferSize(out uint size);
    /// <summary>
    /// Gets the number of timeshifting files added.
    /// </summary>
    /// <param name="numbAdd">The number of timeshifting files added.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNumbFilesAdded(out ushort numbAdd);
    /// <summary>
    /// Gets the number of timeshifting  removed.
    /// </summary>
    /// <param name="numbRem">The number of timeshifting  removed.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNumbFilesRemoved(out ushort numbRem);
    /// <summary>
    /// Gets the current file id.
    /// </summary>
    /// <param name="fileID">The file ID.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetCurrentFileId(out ushort fileID);
    /// <summary>
    /// Gets the mininium number of .TS files.
    /// </summary>
    /// <param name="minFiles">The mininium number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMinTSFiles(out ushort minFiles);
    /// <summary>
    /// Sets the mininium number of .TS files.
    /// </summary>
    /// <param name="minFiles">The mininium number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMinTSFiles(ushort minFiles);
    /// <summary>
    /// Gets the max number of .TS files.
    /// </summary>
    /// <param name="maxFiles">The max number of .TS files</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMaxTSFiles(out ushort maxFiles);
    /// <summary>
    /// Sets the max number of .TS files..
    /// </summary>
    /// <param name="maxFiles">the max number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMaxTSFiles(ushort maxFiles);
    /// <summary>
    /// Gets the maxium filesize for each .ts file
    /// </summary>
    /// <param name="maxSize">the maxium filesize for each .ts file.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMaxTSFileSize(out long maxSize);
    /// <summary>
    /// Sets the maxium filesize for each .ts file
    /// </summary>
    /// <param name="maxSize">the maxium filesize for each .ts file</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMaxTSFileSize(long maxSize);
    /// <summary>
    /// Gets the chunk reserve for each .ts file.
    /// </summary>
    /// <param name="chunkSize">Size of the chunk.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChunkReserve(out long chunkSize);
    /// <summary>
    /// Sets the chunk reserve for each .ts file.
    /// </summary>
    /// <param name="chunkSize">Size of the chunk.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetChunkReserve(long chunkSize);
    /// <summary>
    /// Gets the size of the file buffer.
    /// </summary>
    /// <param name="lpllsize">The lpllsize.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetFileBufferSize(out long lpllsize);
    /// <summary>
    /// Sets the timeshifting mode.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMode(TimeShiftingMode mode);
    /// <summary>
    /// Gets the timeshifting mode
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMode(out TimeShiftingMode mode);
    /// <summary>
    /// Sets the PMT pid.
    /// </summary>
    /// <param name="pmtPid">The PMT pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPmtPid(short pmtPid);
    /// <summary>
    /// pauses or continues writing to the timeshifting file.
    /// </summary>
    /// <param name="onoff">if true then pause, else run.</param>
    /// <returns></returns>
    [PreserveSig]
    int Pause(byte onoff);
  }
}
