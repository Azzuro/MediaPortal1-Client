using System;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;

namespace DShowNET
{
	/// <summary>
	/// Summary description for Twinhan.
	/// </summary>
	public class Twinhan : IksPropertyUtils
	{
		readonly Guid THBDA_TUNER = new Guid( "E5644CC4-17A1-4eed-BD90-74FDA1D65423");
		readonly Guid GUID_THBDA_CMD = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
		readonly uint THBDA_IOCTL_CI_PARSER_PMT=0xaa00033c;
		readonly uint THBDA_IOCTL_CI_INFO=0xaa0001e4;


		public Twinhan(IBaseFilter filter)
				:base(filter)
		{
			//isTwinHan=GetCIInfo();
		}
		public bool IsTwinhan
		{
			get
			{
				return GetCIInfo();
			}
		}

		public bool GetCIInfo()
		{
			Log.Write("Twinhan get CI Info");
			bool success=false;
			IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);

			int thbdaLen=0x28;
			IntPtr thbdaBuf = Marshal.AllocCoTaskMem(thbdaLen);
			Marshal.WriteInt32(thbdaBuf,0,0x255e0082);
			Marshal.WriteInt16(thbdaBuf,4,0x2017);
			Marshal.WriteInt16(thbdaBuf,6,0x4b03);
			Marshal.WriteByte(thbdaBuf,8,0x90);
			Marshal.WriteByte(thbdaBuf,9,0xf8);
			Marshal.WriteByte(thbdaBuf,10,0x85);
			Marshal.WriteByte(thbdaBuf,11,0x6a);
			Marshal.WriteByte(thbdaBuf,12,0x62);
			Marshal.WriteByte(thbdaBuf,13,0xcb);
			Marshal.WriteByte(thbdaBuf,14,0x3d);
			Marshal.WriteByte(thbdaBuf,15,0x67);
			Marshal.WriteInt32(thbdaBuf,16,(int)THBDA_IOCTL_CI_INFO);
			Marshal.WriteInt32(thbdaBuf,20,(int)IntPtr.Zero);
			Marshal.WriteInt32(thbdaBuf,24,0);
			Marshal.WriteInt32(thbdaBuf,28,(int)IntPtr.Zero);
			Marshal.WriteInt32(thbdaBuf,32,0);
			Marshal.WriteInt32(thbdaBuf,36,(int)ptrDwBytesReturned);

			IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
			if (pin!=null) 
			{
				IKsPropertySet propertySet= pin as IKsPropertySet;
				if (propertySet!=null) 
				{
					Guid propertyGuid=THBDA_TUNER;
					int hr=propertySet.RemoteSet(ref propertyGuid,0,IntPtr.Zero,0, thbdaBuf,(uint)thbdaLen );
					if (hr==0)
					{
						success=true;
					}
					Marshal.ReleaseComObject(propertySet);
				}
				Marshal.ReleaseComObject(pin);
			}


			Marshal.FreeCoTaskMem(ptrDwBytesReturned);

			return success;
		}


		public void SendPMT(uint videoPid, uint audioPid,byte[] PMT, int pmtLen)
		{
			Log.Write("Twinhan send PMT len:{0} video:0x{1:X} audio:0x{2:X}", pmtLen,videoPid,audioPid);
			IntPtr ptrPMT = Marshal.AllocCoTaskMem(pmtLen+1);
			
			if(ptrPMT==IntPtr.Zero)
				return;
			Marshal.Copy(PMT,0,(IntPtr)(((int)ptrPMT)+1),pmtLen);
			Marshal.WriteByte(ptrPMT,0);
			pmtLen+=1;
			int lenParserPMTInfo=20;
			IntPtr ptrPARSERPMTINFO = Marshal.AllocCoTaskMem(lenParserPMTInfo);
			if(ptrPMT==IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ptrPMT);
				return;
			}
			Marshal.WriteInt32(ptrPARSERPMTINFO,0,(int)ptrPMT);
			Marshal.WriteInt32(ptrPARSERPMTINFO,4,pmtLen);
			Marshal.WriteInt32(ptrPARSERPMTINFO,8,(int)videoPid);
			Marshal.WriteInt32(ptrPARSERPMTINFO,12,(int)audioPid);
			Marshal.WriteInt32(ptrPARSERPMTINFO,16,0);//default cam

			IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);
			if(ptrDwBytesReturned==IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ptrPMT);
				Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
				return;
			}
			IntPtr ksBla=Marshal.AllocCoTaskMem(0x18);
			if(ksBla==IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ptrPMT);
				Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
				Marshal.FreeCoTaskMem(ptrDwBytesReturned);
				return;
			}
		
			int thbdaLen=0x28;
			IntPtr thbdaBuf = Marshal.AllocCoTaskMem(thbdaLen);
			if(thbdaBuf==IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ptrPMT);
				Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
				Marshal.FreeCoTaskMem(ptrDwBytesReturned);
				Marshal.FreeCoTaskMem(ksBla);
				return;
			}
		
			
			Marshal.WriteInt32(thbdaBuf,0,0x255e0082);
			Marshal.WriteInt16(thbdaBuf,4,0x2017);
			Marshal.WriteInt16(thbdaBuf,6,0x4b03);
			Marshal.WriteByte(thbdaBuf,8,0x90);
			Marshal.WriteByte(thbdaBuf,9,0xf8);
			Marshal.WriteByte(thbdaBuf,10,0x85);
			Marshal.WriteByte(thbdaBuf,11,0x6a);
			Marshal.WriteByte(thbdaBuf,12,0x62);
			Marshal.WriteByte(thbdaBuf,13,0xcb);
			Marshal.WriteByte(thbdaBuf,14,0x3d);
			Marshal.WriteByte(thbdaBuf,15,0x67);
			Marshal.WriteInt32(thbdaBuf,16,(int)THBDA_IOCTL_CI_PARSER_PMT);
			Marshal.WriteInt32(thbdaBuf,20,(int)ptrPARSERPMTINFO);
			Marshal.WriteInt32(thbdaBuf,24,20);
			Marshal.WriteInt32(thbdaBuf,28,0);
			Marshal.WriteInt32(thbdaBuf,32,0);
			Marshal.WriteInt32(thbdaBuf,36,(int)ptrDwBytesReturned);

			IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
			if (pin!=null) 
			{
				IKsPropertySet propertySet= pin as IKsPropertySet;
				if (propertySet!=null) 
				{
					Guid propertyGuid=THBDA_TUNER;
					int hr=propertySet.RemoteSet(ref propertyGuid,0,ksBla,0x18, thbdaBuf,(uint)thbdaLen );
					int back=Marshal.ReadInt32(ptrDwBytesReturned);
					int ksBlaVal=Marshal.ReadInt32(ksBla);

					if (hr!=0)
					{
						Log.Write("SetStructure() failed 0x{0:X}",hr);
					}
					else
						Log.Write("SetStructure() returned ok 0x{0:X}",hr);
					Marshal.ReleaseComObject(propertySet);

				}
				Marshal.ReleaseComObject(pin);
			}


			Marshal.FreeCoTaskMem(thbdaBuf);
			Marshal.FreeCoTaskMem(ptrDwBytesReturned);
			Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
			Marshal.FreeCoTaskMem(ptrPMT);
			Marshal.FreeCoTaskMem(ksBla);


		}

		protected override void SetStructure(Guid guidPropSet, uint propId, System.Type structureType, object structValue)
		{
			Guid propertyGuid=guidPropSet;
			IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
			if (pin==null) return ;
			IKsPropertySet propertySet= pin as IKsPropertySet;
			if (propertySet==null) 
			{
				Log.Write("SetStructure() properySet=null");
				return ;
			}

			int iSize=Marshal.SizeOf(structureType);
			Log.Write("size:{0}",iSize);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(iSize);
			Marshal.StructureToPtr(structValue,pDataReturned,true);
			int hr=propertySet.RemoteSet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,(uint)Marshal.SizeOf(structureType) );
			if (hr!=0)
			{
				Log.Write("SetStructure() failed 0x{0:X}",hr);
			}
			else
				Log.Write("SetStructure() returned ok 0x{0:X}",hr);
			Marshal.FreeCoTaskMem(pDataReturned);
		}

	}
}
