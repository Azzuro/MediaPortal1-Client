using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using System.Threading;
using Microsoft.Win32;
using System.Text;

namespace ProcessPlugins.CallerId
{
  public class ISDNWatch
  {
    [StructLayout(LayoutKind.Sequential)]
      struct capiRequest
    {
      public short Length;
      public short ApplicationId;
      public byte Command;
      public byte SubCommand;
      public short MessageNumber;
      public int Controller;
      public int InfoMask;
      public int CIPMask1;
      public int CIPMask2;
      public byte CallingParty;
      public byte CallingPartySub;
    }

    [StructLayout(LayoutKind.Sequential)]
      struct capiMessageHeader
    {
      public ushort Length;
      public ushort ApplicationId;
      public byte Command;
      public byte SubCommand;
      public ushort MessageNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
      struct capiConnectInd 
    {
      public uint PLCI;
      public ushort CIP;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst=100)]
      public byte[] buffer;
    }

    [DllImport("CAPI2032.DLL")]
      static extern int CAPI_INSTALLED();

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_REGISTER(
      int MessageBufferSize,
      int MaxLogicalConnection,
      int MaxBDataBlocks,
      int MaxBDataLen,
      ref int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_RELEASE(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern void CAPI_WAIT_FOR_SIGNAL(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_PUT_MESSAGE(
      int ApplicationID,
      [MarshalAs(UnmanagedType.AsAny)] object CAPIMessage );

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_GET_MESSAGE(
      int ApplicationID,
      ref IntPtr CapiBufferPointer);

    [DllImport("kernel32")]
    static extern void RtlMoveMemory(
      ref capiMessageHeader Destination,
      IntPtr Source,
      int Length);

    [DllImport("kernel32")]
    static extern void RtlMoveMemory(
      ref capiConnectInd Destination,
      IntPtr Source,
      int Length);

    [DllImport("tapi32.dll")]
    static extern int tapiGetLocationInfoW(
      [MarshalAs(UnmanagedType.LPTStr)]
      StringBuilder CountryCode,
      [MarshalAs(UnmanagedType.LPTStr)]
      StringBuilder AereaCode);

    public delegate void EventHandler(string CallerId);
    static public event EventHandler CidReceiver = null;
    
    bool stopThread = false;
    int stripPrefix;
    const int HeaderLength = 8;
    const int CAPI_CONNECT = 0x02;
    const int CAPI_IND = 0x82;

    public class LocationInfo
    {
      public string CountryCode, AreaCode;

      public LocationInfo()
      {
        CountryCode = "";
        AreaCode = "";
      }
    }

    static public bool CapiInstalled
    {
      get
      {
        int result = -1;

        try
        {
          result = CAPI_INSTALLED();
        }
        catch (Exception)
        {
        }

        if (result == 0)
          return true;
        else
          return false;
      }
    }

    public void Start()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        stripPrefix = xmlreader.GetValueAsInt("isdn", "stripprefix", 0);
      }
      Thread watchThread = new Thread(new ThreadStart(WatchThread));
      watchThread.Name = "CAPI Monitoring";
      watchThread.Start();
    }

    public void Stop()
    {
      stopThread = true;
    }

    void WatchThread()
    {
      int applicationId = 0;

      // Registering with CAPI
      int capiResult = CAPI_REGISTER(3072, 2, 7, 2048, ref applicationId);
      if (capiResult != 0)
        Log.Write("ISDN: Application cannot register with CAPI");
      else
      {
        Log.Write("ISDN: Application registered with CAPI ({0})", applicationId);

        capiRequest capiRequest = new capiRequest();
        capiRequest.Length = 26;
        capiRequest.ApplicationId = (short)applicationId;
        capiRequest.Command = 0x0005;
        capiRequest.SubCommand = 0x0080;
        capiRequest.MessageNumber = 1;
        capiRequest.Controller = 1;
        capiRequest.InfoMask = 0x0000;
        capiRequest.CIPMask1 = 1;
        capiRequest.CIPMask2 = 0x0000;
        capiRequest.CallingParty = 0x0000;
        capiRequest.CallingPartySub = 0x0000;
        capiResult = CAPI_PUT_MESSAGE(applicationId, capiRequest);

        if (capiResult != 0)
          Log.Write("ISDN: CAPI signaling cannot be activated");
        else
        {
          Log.Write("ISDN: CAPI signaling activated");
          
          while (!stopThread) // Waiting for signal and signal-processing
          {
            string callerId = null;
            string calledId = null;
            string logBuffer = "";
            capiMessageHeader messageHeader = new capiMessageHeader();
            IntPtr capiBufferPointer = new IntPtr();

            //        CAPI_WAIT_FOR_SIGNAL(applicationId);
            if (CAPI_GET_MESSAGE(applicationId, ref capiBufferPointer) == 0)
            {
              RtlMoveMemory(ref messageHeader, capiBufferPointer, HeaderLength);
              if ((messageHeader.Command == CAPI_CONNECT) && (messageHeader.SubCommand == CAPI_IND))
              {
                capiConnectInd ConnectInd = new capiConnectInd();
                RtlMoveMemory (ref ConnectInd, (IntPtr)(capiBufferPointer.ToInt32() + HeaderLength), (messageHeader.Length - HeaderLength));
                
                for (int i = 99; i >= 0; i--)
                  if ((logBuffer.Length != 0) || (ConnectInd.buffer[i] !=0))
                  {
                    if ((ConnectInd.buffer[i] < 48) || (ConnectInd.buffer[i] > 57))
                      logBuffer = "(" + ConnectInd.buffer[i] + ")" + logBuffer;
                    else
                      logBuffer = (char)ConnectInd.buffer[i] + logBuffer;
                  }

                Log.Write("ISDN: Buffer: {0}", logBuffer);

                int lengthCalledId = ConnectInd.buffer[0];
                int lengthCallerId = ConnectInd.buffer[lengthCalledId + 1];

                for (int i = 2; i < (lengthCalledId + 1); i++)
                  calledId = calledId + (char)ConnectInd.buffer[i];
                for (int i = (lengthCalledId + 4); i < (lengthCallerId + lengthCalledId + 2); i++)
                  callerId = callerId + (char)ConnectInd.buffer[i];

                callerId = callerId.TrimStart('0');
                Log.Write("ISDN: stripped {0} leading zeros", lengthCallerId - callerId.Length - 2);

                if (ConnectInd.buffer[lengthCalledId+2] == 17)  // International call
                  callerId = "+" + callerId;

                Log.Write("ISDN: CalledID: {0}", calledId);
                Log.Write("ISDN: CallerID: {0}", callerId);

                CidReceiver(callerId);
              }
            }
            Thread.Sleep(200);
          }

          // Release CAPI
          if (CAPI_RELEASE(applicationId) == 0)
          {
            stopThread = false;
            Log.Write("ISDN: CAPI released ({0})", applicationId);
          }
          else
            Log.Write("ISDN: CAPI cannot be released");
        }
      }
    }

    public static LocationInfo GetLocationInfo()
    {
      StringBuilder countryCode = new StringBuilder(8);
      StringBuilder areaCode = new StringBuilder(8);
      LocationInfo locationInfo = new LocationInfo();
      if (tapiGetLocationInfoW(countryCode, areaCode) == 0)
      {
        locationInfo.CountryCode = countryCode.ToString();
        locationInfo.AreaCode = areaCode.ToString();
        if (locationInfo.AreaCode[0] == '0')
          locationInfo.AreaCode = locationInfo.AreaCode.Remove(0, 1);
      }
      else
        Log.Write("ISDN: Can't get TAPI location info!!!");

      return locationInfo;
    }
  }
}