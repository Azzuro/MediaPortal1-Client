using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;

namespace DShowNET
{
	/// <summary>
	/// This class implements methods for vendor specific actions on tv capture cards.
	/// Currently we support
	/// - Hauppauge PVR cards
	/// - FireDTV digital cards
	/// </summary>
	public class VideoCaptureProperties
	{
		static readonly Guid KSPROPSETID_Firesat = new Guid( 0xab132414, 0xd060, 0x11d0,  0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba,0xf3  );
		
		
		struct FIRESAT_CA_DATA
		{								
			public Byte			uSlot;								
			public Byte			uTag;									
			public Byte			bMore;								
			public ushort		uLength;					
		}

		[StructLayout(LayoutKind.Sequential),  ComVisible(false)]
		struct KSPROPERTY
		{
			Guid    Set;
			int   Id;
			int   Flags;
		};
		[StructLayout(LayoutKind.Sequential), ComVisible(false)]
		struct KSPROPERTYByte
		{
			public Guid    Set;  //16		0-15
			public int   Id;		 //4		16-19
			public int   Flags;	 //4		20-23
			public int alignment;//4		24-27
			public byte byData;	 //     28-31
		};		

		[StructLayout(LayoutKind.Sequential), ComVisible(false)]
			struct KSPROPERTYInt
		{
			public Guid    Set;  //16		0-15
			public int   Id;		 //4		16-19
			public int   Flags;	 //4		20-23
			public int alignment;//4		24-27
			public int byData;	 //     28-31
		};		


    public enum eAudioInputType
    {
      LineIn              = 0x00,
      Tv                  = 0x01,
      Mute                = 0x02
    } ;
    public enum eAudioSampleRate
    {
      Rate_32       = 0x02,
      Rate_44       = 0x00,
      Rate_48       = 0x01,
      Rate_Unsupported = 0x10
    } ;
    public enum eAudioOutputMode
    {   
      Stereo     = 0x00,
      Joint      = 0x01,
      Dual       = 0x02,
      Mono       = 0x03
    } ;
    public enum eBitRateMode:int
    {
      Cbr    = 0x00,
      Vbr    = 0x01
    };
    
		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
    public struct videoBitRate
    {
      public eBitRateMode    bEncodingMode;  // Variable or Constant bit rate
      public ushort          wBitrate;       // Actual bitrate in 1/400 mbits/sec
      public uint          dwPeak;         // Peak/400
    } 



    //
    // bits [2..0] indicate the supported sampling frequency
    // bits [5..3] indicate the audio digitizer chip present
    // 
    enum eSupportedAudioFrequencies:byte
    {
      SampleRate_32      = 0x01,
      SampleRate_44      = 0x02,
      SampleRate_48      = 0x04,

      Digitizer_MSP3440             = 0x01,
      Digitizer_MSP3438             = 0x0A,
      Digitizer_CS5330              = 0x17, // Used for Rainbow
      Digitizer_CS53L32_MSP3438     = 0x1F,  // Used for Condor
      Digitizer_CS5330_MSP3438      = 0x27  // Used for Hauppauge 
    };

		[StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct versionInfo
    {
      public string DriverVersion; //xx.yy.zzz
      public string FWVersion; //xx.yy.zzz
    } 


    public enum eStreamOutput:int
    {
      PROGRAM       = 0,
      TRANSPORT     = 1,
      MPEG1         = 2,
      PES_AV        = 3,
      PES_Video     = 5,
      PES_Audio     = 7,
      DVD           = 10,
      VCD           = 11
    };

    public enum eVideoFormat:byte
    {
      NTSC=0,
      PAL=1
    };

    enum eVideoResolution: byte
    {
      Resolution_720x480        = 0,
      Resolution_720x576        = 0, // For PAL
      Resolution_480x480        = 1,
      Resolution_480x576        = 1, // For PAL
      Resolution_352x480        = 2,
      Resolution_352x576        = 2, // For PAL
      Resolution_352x240        = 2, // For NTSC MPEG1
      Resolution_352x288        = 2,  // For PAL MPEG1
      Resolution_320x240        = 3  // For NTSC MPEG1
    };


    IBaseFilter captureFilter;
		public VideoCaptureProperties(IBaseFilter capturefilter)
		{
      captureFilter=capturefilter;
		}


		/// <summary>
		/// Fuction to set/get the video bitrate for Hauppauge PVR cards
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public videoBitRate VideoBitRate 
    {
      get
      {
				videoBitRate bitrate=new videoBitRate();
        object obj =GetStructure(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_BITRATE, typeof(videoBitRate)) ;
        try
        { 
          bitrate = (videoBitRate)obj ;
        }
        catch (Exception){}
        return bitrate;
      }
			set
			{
				SetStructure(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_BITRATE, typeof(videoBitRate), (object)value) ;
			}
    }


		/// <summary>
		/// Fuction to get the driver & firmware version for Hauppauge PVR cards
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public versionInfo VersionInfo
    {
      get
      {
        string version=GetString(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_VERSION_INFO);
        versionInfo info = new versionInfo();
        if (version!=null && version.Length==20)
        {
          info.DriverVersion =version.Substring(0,10);
          info.FWVersion     =version.Substring(10,10);
        }
        return info;
      }
    }

		/// <summary>
		/// Fuction to get/set the GOPsize for Hauppauge PVR cards
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public byte GopSize
    {
      get 
      {
        return GetByteValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_GOP_SIZE);
      }
      set
      {
        SetByteValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_GOP_SIZE, value);
      }
    }
		
		/// <summary>
		/// Fuction to enable/disable GOP
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public bool ClosedGop
    {
      get 
      {
        return (GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_CLOSED_GOP) !=0);
      }
      set
      {
        int byValue=0;
        if (value) byValue=1;
        SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_CLOSED_GOP, byValue);
      }
    }
    
		/// <summary>
		/// Fuction to enable/disable Inverse Telecine
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public bool InverseTelecine
    {
      get 
      {
        return (GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_INVERSE_TELECINE) !=0);
      }
      set
      {
        byte byValue=0;
        if (value) byValue=1;
        SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_INVERSE_TELECINE, byValue);
      }
    }
    
		/// <summary>
		/// Fuction to get/set the current video format (pal,ntsc,..)
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public eVideoFormat VideoFormat
    {
      get 
      {
        return (eVideoFormat)GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_TV_ENCODE_FORMAT);
      }
      set
      {
        int byValue=(int)value;
        SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_TV_ENCODE_FORMAT, byValue);
      }
    }

		
		/// <summary>
		/// Fuction to get/set the current video resolution
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public Size VideoResolution
    {
      get 
      {
        int videoRes= (GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_VIDEO_RESOLUTION) );
        if (VideoFormat==eVideoFormat.NTSC)
        {
          switch (videoRes)
          {
            case (int)eVideoResolution.Resolution_720x480:
              return new Size(720,480);
            case (int)eVideoResolution.Resolution_480x480:
              return new Size(480,480);
            case (int)eVideoResolution.Resolution_352x480:
              return new Size(352,480);
            //case (int)eVideoResolution.Resolution_352x240:
            //  return new Size(352,240);
            case (int)eVideoResolution.Resolution_320x240:
              return new Size(320,240);
          }
        }
        else
        {
          switch (videoRes)
          {
            case (int)eVideoResolution.Resolution_720x576:
              return new Size(720,576);
            case (int)eVideoResolution.Resolution_480x576:
              return new Size(480,576);
            case (int)eVideoResolution.Resolution_352x288:
              return new Size(352,288);
          }
        }
        return new Size(0,0);
      }
      set
      {
        int byValue=0;
        if (value.Width==720 && value.Height==480) byValue=(int)eVideoResolution.Resolution_720x480;
        if (value.Width==480 && value.Height==480) byValue=(int)eVideoResolution.Resolution_480x480;
        if (value.Width==352 && value.Height==480) byValue=(int)eVideoResolution.Resolution_352x480;
        if (value.Width==352 && value.Height==240) byValue=(int)eVideoResolution.Resolution_352x240;
        if (value.Width==320 && value.Height==240) byValue=(int)eVideoResolution.Resolution_320x240;

        if (value.Width==720 && value.Height==576) byValue=(int)eVideoResolution.Resolution_720x576;
        if (value.Width==480 && value.Height==576) byValue=(int)eVideoResolution.Resolution_480x576;
        if (value.Width==352 && value.Height==576) byValue=(int)eVideoResolution.Resolution_352x576;
        if (value.Width==352 && value.Height==288) byValue=(int)eVideoResolution.Resolution_352x288;

        SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_VIDEO_RESOLUTION, byValue);
      }
    }
    
		
		/// <summary>
		/// Fuction to get/set the current stream output (vhs, svhs, dvd)
		/// </summary>
		/// <remarks>
		/// This is a vendor specific. It will only work for hauppage PVR cards
		/// </remarks>
    public eStreamOutput StreamOutput
    {
      get 
      {
        return (eStreamOutput)GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_OUTPUT_TYPE);
      }
      set
      {
        int iValue=(int)value;
        SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_OUTPUT_TYPE, iValue);
      }
    }

    byte GetByteValue(Guid guidPropSet, uint propId)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
      uint IsTypeSupported=0;
      uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetByteValue() properySet=null");
				return 0;
			}
      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetByteValue() property is not supported");
				return 0;
			}

			byte returnValue=0;
			KSPROPERTYByte propByte = new KSPROPERTYByte();
			KSPROPERTY prop         = new KSPROPERTY();
			int sizeProperty     = Marshal.SizeOf(prop);
			int sizeByteProperty = Marshal.SizeOf(propByte);

			KSPROPERTYByte newByteValue = new KSPROPERTYByte();
      IntPtr pDataReturned=Marshal.AllocCoTaskMem(100);
			Marshal.StructureToPtr(newByteValue,pDataReturned,true);

			int adress=pDataReturned.ToInt32()+sizeProperty;
			IntPtr ptrData = new IntPtr(adress);
      hr=propertySet.RemoteGet(ref propertyGuid,
															 propId,
															 ptrData,
															 (uint)(sizeByteProperty-sizeProperty), 
															 pDataReturned,
															 (uint)sizeByteProperty,
															 out uiSize);
      if (hr==0 && uiSize==1)
      {
						returnValue=Marshal.ReadByte(ptrData);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
			
			if (hr!=0)
			{
				Log.Write("GetByteValue() failed 0x{0:X}",hr);
			}
      return returnValue;
    }

    void SetByteValue(Guid guidPropSet, uint propId, byte byteValue)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
      if (propertySet==null) 
			{
				Log.Write("GetByteValue() properySet=null");
				return ;
			}
      uint IsTypeSupported=0;

      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SetByteValue() property is not supported");
				return ;
			}

			KSPROPERTYByte KsProperty  = new KSPROPERTYByte ();
			KsProperty.byData=byteValue;
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
	    Marshal.StructureToPtr(KsProperty, pDataReturned,false);
      hr=propertySet.RemoteSet(ref propertyGuid,
																propId,
																pDataReturned,
																1, 
																pDataReturned,
																(uint)Marshal.SizeOf(KsProperty) );
      Marshal.FreeCoTaskMem(pDataReturned);
			
			if (hr!=0)
			{
				Log.Write("SetByteValue() failed 0x{0:X}",hr);
			}
    }

    int GetIntValue(Guid guidPropSet, uint propId)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
      uint IsTypeSupported=0;
      uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetIntValue() properySet=null");
				return 0;
			}
      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetIntValue() property is not supported");
				return 0;
			}
      
			int returnValue=0;
			KSPROPERTYInt propInt = new KSPROPERTYInt();
			KSPROPERTY prop         = new KSPROPERTY();
			int sizeProperty     = Marshal.SizeOf(prop);
			int sizeIntProperty = Marshal.SizeOf(propInt);

			KSPROPERTYInt newIntValue = new KSPROPERTYInt();
			IntPtr pDataReturned=Marshal.AllocCoTaskMem(100);
			Marshal.StructureToPtr(newIntValue,pDataReturned,true);

			int adress=pDataReturned.ToInt32()+sizeProperty;
			IntPtr ptrData = new IntPtr(adress);
			hr=propertySet.RemoteGet(ref propertyGuid,
																propId,
																ptrData,
																(uint)(sizeIntProperty-sizeProperty), 
																pDataReturned,
																(uint)sizeIntProperty,
																out uiSize);


			if (hr==0 && uiSize==4)
			{
				returnValue=Marshal.ReadInt32(pDataReturned);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
      return returnValue;
    }

    void SetIntValue(Guid guidPropSet, uint propId, int intValue)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			if (propertySet==null) 
			{
				Log.Write("SetIntValue() properySet=null");
				return ;
			}
      uint IsTypeSupported=0;

      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SetIntValue() property is not supported");
				return ;
			}
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
      Marshal.WriteInt32(pDataReturned, intValue);
      hr=propertySet.RemoteSet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,1);
			if (hr!=0)
			{
				Log.Write("SetIntValue() failed 0x{0:X}",hr);
			}
      Marshal.FreeCoTaskMem(pDataReturned);
    }

    string GetString(Guid guidPropSet, uint propId)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
      uint IsTypeSupported=0;
      uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetString() properySet=null");
				return String.Empty;
			}
      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
      if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetString() property is not supported");
				return String.Empty;
			}

      IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
      string returnedText=String.Empty;
      hr=propertySet.RemoteGet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,100,out uiSize);
      if (hr==0)
      {
        returnedText=Marshal.PtrToStringAnsi(pDataReturned,(int)uiSize);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
      return returnedText;
    }


    object GetStructure(Guid guidPropSet, uint propId, System.Type structureType)
    {
      Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet= captureFilter as IKsPropertySet;
      uint IsTypeSupported=0;
      uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetStructure() properySet=null");
				return null;
			}
      int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetString() GetStructure is not supported");
				return null;
			}

      object objReturned=null;
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1000);
      hr=propertySet.RemoteGet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,1000,out uiSize);
			if (hr==0)
			{
				objReturned=Marshal.PtrToStructure(pDataReturned, structureType);
			}
			else
			{
					Log.Write("GetStructure() failed 0x{0:X}",hr);
			}
      Marshal.FreeCoTaskMem(pDataReturned);
      return objReturned;
    }

		void SetStructure(Guid guidPropSet, uint propId, System.Type structureType, object structValue)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			if (propertySet==null) 
			{
				Log.Write("SetStructure() properySet=null");
				return ;
			}

			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("GetString() GetStructure is not supported");
				return ;
			}

			int iSize=Marshal.SizeOf(structureType);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(iSize);
			Marshal.StructureToPtr(structValue,pDataReturned,true);
			hr=propertySet.RemoteSet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,(uint)Marshal.SizeOf(structureType) );
			if (hr!=0)
			{
				Log.Write("SetStructure() failed 0x{0:X}",hr);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
		}


		/// <summary>
		/// Checks if the card specified supports getting/setting properties using the IKsPropertySet interface
		/// </summary>
		/// <returns>
		/// true:		IKsPropertySet is supported
		/// false:	IKsPropertySet is not supported
		/// </returns>
    public bool SupportsProperties
    {
      get 
      {
        IKsPropertySet propertySet= captureFilter as IKsPropertySet;
        if (propertySet==null) return false;
        return true;
      }
    }
		
		public bool SupportsHauppaugePVRProperties
		{
			get 
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=IVac.IvacGuid;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, (uint)IVac.PropertyId.IVAC_VERSION_INFO, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
				{
					return false;
				}
				return true;
			}
		}

		public bool SupportsFireDTVProperties
		{
			get 
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=KSPROPSETID_Firesat;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, 22, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
				{
					return false;
				}
				return true;
			}
		}
		/// <summary>
		/// This function sends the PMT (Program Map Table) to the FireDTV DVB-T/DVB-C/DVB-S card
		/// This allows the integrated CI & CAM module inside the FireDTv device to decrypt the current TV channel
		/// (provided that offcourse a smartcard with the correct subscription and its inserted in the CAM)
		/// </summary>
		/// <param name="PMT">Program Map Table received from digital transport stream</param>
		/// <remarks>
		/// 1. first byte in PMT is 0x02=tableId for PMT
		/// 2. This function is vender specific. It will only work on the FireDTV devices
		/// </remarks>
		/// <preconditions>
		/// 1. FireDTV device should be tuned to a digital DVB-C/S/T TV channel 
		/// 2. PMT should have been received 
		/// </preconditions>
		public void SendPMTToFireDTV(byte[] PMT)
		{
			if (PMT==null) return;
			if (PMT.Length==0) return;

			Log.Write("SendPMTToFireDTV pmt:{0}", PMT.Length);
			Guid propertyGuid=KSPROPSETID_Firesat;
			int propId=22;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			if (propertySet==null) 
			{
				Log.Write("SendPMTToFireDTV() properySet=null");
				return ;
			}

			int hr=propertySet.QuerySupported( ref propertyGuid, (uint)propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SendPMTToFireDTV() GetStructure is not supported");
				return ;
			}

			int iSize=12+2+PMT.Length;
			IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
			int offs=0;

			byte[] byData = new byte[1036];
			uint uLength=(uint)(2+PMT.Length);
			byData[offs]=0; offs++;			//slot
			byData[offs]= 2; offs++;			//utag
			byData[offs]= 0; offs++;     //bmore
			/*
			byData[offs]= 0; offs++;
			byData[offs]= 0; offs++;
			byData[offs]= 0; offs++;
			byData[offs]= 0; offs++;
			byData[offs]= 0; offs++;
			*/
			byData[offs]= (byte)(uLength%256); offs++;		//ulength hi
			byData[offs]= (byte)(uLength/256); offs++;		//ulength lo
			//byData[offs]= 0; offs++;
			//byData[offs]= 0; offs++;
			byData[offs]= 3; offs++;// List Management = ONLY
			byData[offs]= 1; offs++;// pmt_cmd = OK DESCRAMBLING		
			for (int i=0; i < PMT.Length;++i)
			{
				byData[offs]=PMT[i];
				offs++;
			}
			string log="data:";
			for (int i=0; i < offs;++i)
			{
				Marshal.WriteByte(pDataInstance,byData[i]);
				Marshal.WriteByte(pDataReturned,byData[i]);
				log += String.Format("{0:X} ",byData[i]);
			}

			Log.Write(log);
			hr=propertySet.RemoteSet(ref propertyGuid,(uint)propId,pDataInstance,(uint)1036, pDataReturned,(uint)1036 );
			if (hr!=0)
			{
				Log.Write("SetStructure() failed 0x{0:X} offs:{1}",hr, offs);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
			Marshal.FreeCoTaskMem(pDataInstance);

		}//public void SendPMTToFireDTV(byte[] PMT)

	}//public class VideoCaptureProperties
}//namespace DShowNET
