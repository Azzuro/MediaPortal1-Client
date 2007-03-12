#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal - diehard2
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

#endregion

using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using DirectShowLib;
using MediaPortal.Util;

namespace DShowNET
{

	public class Hauppauge:IDisposable
	{
    private bool disposed = false;

    [DllImport("kernel32.dll")]
    internal static extern IntPtr LoadLibrary(String dllname);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, String procname);

    [DllImport("kernel32.dll")]
    internal static extern bool FreeLibrary(IntPtr hModule);

    IntPtr hauppaugelib = IntPtr.Zero;

    delegate int Init(IBaseFilter tuner);
    delegate int DeInit();
    delegate bool IsHauppauge();
    delegate int SetVidBitRate(int maxkbps, int minkbps, bool isVBR);
    delegate int GetVidBitRate(out int maxkbps, out int minkbps, out bool isVBR);
    delegate int SetAudBitRate(int bitrate);
    delegate int GetAudBitRate(out int bitrate);
    delegate int SetStreamType(int stream);
    delegate int GetStreamType(out int stream);
    delegate int SetDNRFilter(bool onoff);

    Init _Init = null;
    DeInit _DeInit = null;
    IsHauppauge _IsHauppauge = null;
    SetVidBitRate _SetVidBitRate = null;
    GetVidBitRate _GetVidBitRate = null;
    SetAudBitRate _SetAudBitRate = null;
    GetAudBitRate _GetAudBitRate = null;
    SetStreamType _SetStreamType = null;
    GetStreamType _GetStreamType = null;
    SetDNRFilter _SetDNRFilter = null;

    HResult hr; 
    
    //Initializes the Hauppauge interfaces

		public Hauppauge(IBaseFilter filter)
		{
      try
      {
        //Don't create the class is we don't have any filter;

        if (filter == null)
        {
          return;
        }

        //Load Library
        hauppaugelib = LoadLibrary("hauppauge.dll");
        
        //Get Proc addresses, and set the delegates
        IntPtr procaddr = GetProcAddress(hauppaugelib, "Init");
        _Init = (Init)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(Init));

        procaddr = GetProcAddress(hauppaugelib, "DeInit");
        _DeInit = (DeInit)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(DeInit));

        procaddr = GetProcAddress(hauppaugelib, "IsHauppauge");
        _IsHauppauge = (IsHauppauge)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(IsHauppauge));

        procaddr = GetProcAddress(hauppaugelib, "SetVidBitRate");
        _SetVidBitRate = (SetVidBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetVidBitRate));

        procaddr = GetProcAddress(hauppaugelib, "GetVidBitRate");
        _GetVidBitRate = (GetVidBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetVidBitRate));

        procaddr = GetProcAddress(hauppaugelib, "SetAudBitRate");
        _SetAudBitRate = (SetAudBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetAudBitRate));

        procaddr = GetProcAddress(hauppaugelib, "GetAudBitRate");
        _GetAudBitRate = (GetAudBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetAudBitRate));

        procaddr = GetProcAddress(hauppaugelib, "SetStreamType");
        _SetStreamType = (SetStreamType)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetStreamType));

        procaddr = GetProcAddress(hauppaugelib, "GetStreamType");
        _GetStreamType = (GetStreamType)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetStreamType));

        procaddr = GetProcAddress(hauppaugelib, "SetDNRFilter");
        _SetDNRFilter = (SetDNRFilter)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetDNRFilter));


        hr = new HResult(_Init(filter));
        Log.Info("Hauppauge Quality Control Initializing " + hr.ToDXString());
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Init failed "+ ex.Message);
      }
		}

    public bool SetDNR(bool onoff)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetDNRFilter(onoff);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge SetDNR failed " + ex.Message);
      }
      return false;

    }
   
		public bool GetVideoBitRate(out int minKbps, out int maxKbps,out bool isVBR)
		{
      maxKbps = minKbps = -1;
      isVBR = false;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetVidBitRate(out maxKbps, out minKbps, out isVBR);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Error GetBitrate " + ex.Message);
      }

			return true;
		}

		public bool SetVideoBitRate(int minKbps, int maxKbps,bool isVBR)
		{
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            hr.Set(_SetVidBitRate(maxKbps, minKbps, isVBR));
            Log.Info("Hauppauge Set Bit Rate " + hr.ToDXString());
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Set Vid Rate " + ex.Message);
      }
      return false;
		}

    public bool GetAudioBitRate(out int Kbps)
    {
      Kbps = -1;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetAudBitRate(out Kbps);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Get Audio Bitrate " + ex.Message);
      }
      return false;
    }

    public bool SetAudioBitRate(int Kbps)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetAudBitRate(Kbps);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Set Audio Bit Rate " + ex.Message);
      }
      return false;
    }

    public bool GetStream(out int stream)
    {
      stream = -1;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetStreamType(out stream);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Get Stream " + ex.Message);
      }
      return false;
    }

    public bool SetStream(int stream)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetStreamType(103);
            return true;
          }
        }
      }
      catch(Exception ex)
      {
        Log.Error("Hauppauge Set Stream Type " + ex.Message);
      }
      return false;
    }
   
    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          // Dispose managed resources if any
        }
        try
        {
          if (hauppaugelib != IntPtr.Zero)
          {
              if (_IsHauppauge())
                _DeInit();
          
             FreeLibrary(hauppaugelib);
             hauppaugelib = IntPtr.Zero;
          }
        }
        catch (Exception ex)
        {
          Log.Info("Hauppauge exception " + ex.Message);
          Log.Info("Hauppauge Disposed hcw.txt");
        }
      }
      disposed = true;
    }

    ~Hauppauge()      
   {
      Dispose(false);
   }
    #endregion
  }
}