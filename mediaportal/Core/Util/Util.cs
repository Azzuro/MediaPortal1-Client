using System;

using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Management;
using System.Diagnostics;
using MediaPortal.GUI.Library;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Win32;


namespace MediaPortal.Util
{

	/// <summary>
	/// 
	/// </summary>
	public class Utils
	{
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		extern static bool GetVolumeInformation(
			string RootPathName,
			StringBuilder VolumeNameBuffer,
			int VolumeNameSize,
			out uint VolumeSerialNumber,
			out uint MaximumComponentLength,
			out uint FileSystemFlags,
			StringBuilder FileSystemNameBuffer,
			int nFileSystemNameSize);

		[DllImport("kernel32.dll")]
		public static extern long GetDriveType(string driveLetter);

		[DllImport( "winmm.dll", EntryPoint="mciSendStringA", CharSet=CharSet.Ansi )]
		protected static extern int mciSendString( string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback );

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
			IntPtr lpInBuffer, uint nInBufferSize,
			IntPtr lpOutBuffer, uint nOutBufferSize,
			out uint lpBytesReturned, IntPtr lpOverlapped);

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr CreateFile(
			string filename,
			[MarshalAs(UnmanagedType.U4)] FileAccess fileaccess,
			[MarshalAs(UnmanagedType.U4)] FileShare fileshare,
			int securityattributes,
			[MarshalAs(UnmanagedType.U4)] FileMode creationdisposition,
			int flags, IntPtr template);


		[DllImport("kernel32.dll", SetLastError=true)]
		static extern bool CloseHandle(IntPtr hObject);


		static ArrayList m_AudioExtensions		=new ArrayList();
		static ArrayList m_VideoExtensions		=new ArrayList();
		static ArrayList m_PictureExtensions		=new ArrayList();
		static bool m_bHideExtensions=false;

		// singleton. Dont allow any instance of this class
		private Utils()
		{
		}
		static Utils()
		{
			try
			{
				System.IO.Directory.CreateDirectory("thumbs");
			}
			catch(Exception){}
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_bHideExtensions=xmlreader.GetValueAsBool("general","hideextensions",true);

				string strTmp=xmlreader.GetValueAsString("music","extensions",".mp3,.wma,.ogg,.flac,.wav,.cda,.m4a");
				Tokens tok = new Tokens(strTmp, new char[] {','} );
				foreach (string strExt in tok)
				{
					m_AudioExtensions.Add(strExt.ToLower());
				}

				strTmp=xmlreader.GetValueAsString("movies","extensions",".avi,.mpg,.ogm,.mpeg,.mkv,.wmv,.ifo,.qt,.rm,.mov,.sbe,.dvr-ms");
				tok = new Tokens(strTmp, new char[] {','} );
				foreach (string strExt in tok)
				{
					m_VideoExtensions.Add(strExt.ToLower());
				}
        
				strTmp=xmlreader.GetValueAsString("pictures","extensions",".jpg,.jpeg,.gif,.bmp,.png");
				tok = new Tokens(strTmp, new char[] {','} );
				foreach (string strExt in tok)
				{
					m_PictureExtensions.Add(strExt.ToLower());
				}
			}
		}


		public static ArrayList VideoExtensions
		{
			get {return m_VideoExtensions;}
		}
    
		public static ArrayList AudioExtensions
		{
			get {return m_AudioExtensions;}
		}
		public static ArrayList PictureExtensions
		{
			get {return m_PictureExtensions;}
		}
		static public string GetDriveSerial(string drive)
		{
			if (drive==null) return String.Empty;
			//receives volume name of drive
			StringBuilder volname = new StringBuilder(256);
			//receives serial number of drive,not in case of network drive(win95/98)
			uint sn;
			uint maxcomplen ;//receives maximum component length
			uint sysflags ;//receives file system flags
			StringBuilder sysname = new StringBuilder(256);//receives the file system name
			bool retval;//return value

			retval = GetVolumeInformation(drive.Substring(0,2), volname, 256, out sn, out maxcomplen, out sysflags, sysname,256);
              
			if(retval)
			{ 
				return String.Format("{0:X}",sn);
			}
			else return "";
		}
		static public string GetDriveName(string drive)
		{
			if (drive==null) return String.Empty;
			//receives volume name of drive
			StringBuilder volname = new StringBuilder(256);
			//receives serial number of drive,not in case of network drive(win95/98)
			uint sn;
			uint maxcomplen ;//receives maximum component length
			uint sysflags ;//receives file system flags
			StringBuilder sysname = new StringBuilder(256);//receives the file system name
			bool retval;//return value

			retval = GetVolumeInformation(drive, volname, 256, out sn, out maxcomplen, out sysflags, sysname,256);
              
			if(retval)
			{ 
				return volname.ToString();
			}
			else return "";
		}
		static public int getDriveType(string drive)
		{
			if (drive==null) return 2;
			if((GetDriveType(drive) & 5)==5)return 5;//cd
			if((GetDriveType(drive) & 3)==3)return 3;//fixed
			if((GetDriveType(drive) & 2)==2)return 2;//removable
			if((GetDriveType(drive) & 4)==4)return 4;//remote disk
			if((GetDriveType(drive) & 6)==6)return 6;//ram disk
			return 0;
		}

		static public string GetSize(long dwFileSize)
		{
			if (dwFileSize<0) return "0";
			string szTemp;
			// file < 1 kbyte?
			if (dwFileSize < 1024)
			{
				//  substract the integer part of the float value
				float fRemainder=(((float)dwFileSize)/1024.0f)-(((float)dwFileSize)/1024.0f);
				float fToAdd=0.0f;
				if (fRemainder < 0.01f)
					fToAdd=0.1f;
				szTemp=String.Format("{0:f} KB", (((float)dwFileSize)/1024.0f)+fToAdd);
				return szTemp;
			}
			long iOneMeg=1024*1024;

			// file < 1 megabyte?
			if (dwFileSize < iOneMeg)
			{
				szTemp=String.Format("{0:f} KB", ((float)dwFileSize)/1024.0f);
				return szTemp;
			}

			// file < 1 GByte?
			long iOneGigabyte=iOneMeg;
			iOneGigabyte *= (long)1000;
			if (dwFileSize < iOneGigabyte)
			{
				szTemp=String.Format("{0:f} MB", ((float)dwFileSize)/((float)iOneMeg));
				return szTemp;
			}
			//file > 1 GByte
			int iGigs=0;
			while (dwFileSize >= iOneGigabyte)
			{
				dwFileSize -=iOneGigabyte;
				iGigs++;
			}
			float fMegs=((float)dwFileSize)/((float)iOneMeg);
			fMegs /=1000.0f;
			fMegs+=iGigs;
			szTemp=String.Format("{0:f} GB", fMegs);
			return szTemp;
		}

		static public bool IsLiveTv(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				if (strExtFile.ToLower().Equals(".tv") ) return true;
			}
			catch(Exception){}
			return false;

		}
		static public bool IsVideo(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				if (IsPlayList(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				if (strExtFile.ToLower().Equals(".tv") ) return true;
				if (strExtFile.ToLower().Equals(".sbe") ) return true;
				if (strExtFile.ToLower().Equals(".dvr-ms") ) return true;
				if (VirtualDirectory.IsImageFile(strExtFile.ToLower())) return true;
				foreach (string strExt in m_VideoExtensions)
				{
					if (strExt == strExtFile) return true;
				}
			}
			catch(Exception)
			{
			}
			return false;
		}

		static public bool IsAudio(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				if (IsPlayList(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				foreach (string strExt in m_AudioExtensions)
				{
					if (strExt == strExtFile) return true;
				}
			}
			catch(Exception)
			{
			}
			return false;
		}

		static public bool IsPicture(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				if (IsPlayList(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				foreach (string strExt in m_PictureExtensions)
				{
					if (strExt == strExtFile) return true;
				}
			}
			catch(Exception)
			{
			}
			return false;
		}

		static public bool IsPlayList(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				if (strExtFile==".m3u") return true;
				if (strExtFile==".pls") return true;
				if (strExtFile==".b4s") return true;
				if (strExtFile==".wpl") return true;
			}
			catch(Exception)
			{
			}
			return false;
		}
		static public bool IsProgram(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				if (strExtFile==".exe") return true;
			}
			catch(Exception)
			{
			}
			return false;
		}
		static public bool IsShortcut(string strPath)
		{
			if (strPath==null) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
				string strExtFile=System.IO.Path.GetExtension(strPath).ToLower();
				if (strExtFile==".lnk") return true;
			}
			catch(Exception)
			{
			}
			return false;
		}
    
		public static void SetDefaultIcons(GUIListItem item)
		{
			if (item==null) return;
			if (!item.IsFolder)
			{
				if (IsPlayList(item.Path))
				{
					item.IconImage="DefaultPlaylist.png";
					item.IconImageBig="DefaultPlaylistBig.png";
				}
				else if (IsVideo(item.Path))
				{
					item.IconImage="defaultVideo.png";
					item.IconImageBig="defaultVideoBig.png";
				}
				else if (IsCDDA(item.Path))
				{
					item.IconImage="defaultCdda.png";
					item.IconImageBig="defaultCddaBig.png";
				}
				else if (IsAudio(item.Path))
				{
					item.IconImage="defaultAudio.png";
					item.IconImageBig="defaultAudioBig.png";
				}
				else if (IsPicture(item.Path))
				{
					item.IconImage="defaultPicture.png";
					item.IconImageBig="defaultPictureBig.png";
				}
				else if (IsProgram(item.Path))
				{
					item.IconImage="DefaultProgram.png";
					item.IconImageBig="DefaultProgramBig.png";
				}
				else if (IsShortcut(item.Path))
				{
					item.IconImage="DefaultShortcut.png";
					item.IconImageBig="DefaultShortcutBig.png";
				}
			}
			else
			{
				if (item.Label=="..")
				{
					item.IconImage="defaultFolderBack.png";
					item.IconImageBig="defaultFolderBackBig.png";
				}
				else
				{
					if (item.Path.Length<=3)
					{
						if ( IsDVD(item.Path))
						{
							item.IconImage="defaultDVDRom.png";
							item.IconImageBig="defaultDVDRomBig.png";
						}
						else if ( IsHD(item.Path))
						{
							item.IconImage="defaultHardDisk.png";
							item.IconImageBig="defaultHardDiskBig.png";
						}
						else if ( IsNetwork(item.Path))
						{
							item.IconImage="defaultNetwork.png";
							item.IconImageBig="defaultNetworkBig.png";
						}
						else if ( IsRemovable(item.Path))
						{
							item.IconImage="defaultRemovable.png";
							item.IconImageBig="defaultRemovableBig.png";
						}
						else
						{
							item.IconImage="defaultFolder.png";
							item.IconImageBig="defaultFolderBig.png";
						}
					}
					else
					{
						item.IconImage="defaultFolder.png";
						item.IconImageBig="defaultFolderBig.png";
					}
				}
			}
		}

		public static void SetThumbnails(ref GUIListItem item)
		{
			if (item==null) return;
			try
			{
				if (!item.IsFolder
				|| ( item.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path).ToLower())))
				{
					if (IsPicture(item.Path)) return;

					// check for filename.tbn
					string strThumb=System.IO.Path.ChangeExtension(item.Path,".tbn");
					if (System.IO.File.Exists(strThumb))
					{
						// yep got it
						item.ThumbnailImage=strThumb;
						item.IconImage=strThumb;
						item.IconImageBig=strThumb;
						return;
					}
					strThumb=System.IO.Path.ChangeExtension(item.Path,".jpg");
					if (System.IO.File.Exists(strThumb))
					{
						// yep got it
						item.ThumbnailImage=strThumb;
						item.IconImage=strThumb;
						item.IconImageBig=strThumb;
						return;
					}
					strThumb=System.IO.Path.ChangeExtension(item.Path,".png");
					if (System.IO.File.Exists(strThumb))
					{
						// yep got it
						item.ThumbnailImage=strThumb;
						item.IconImage=strThumb;
						item.IconImageBig=strThumb;
						return;
					}
          
					// check for thumbs\filename.tbn
          
					strThumb=GetThumb(item.Path);
					if (System.IO.File.Exists(strThumb))
					{
						// yep got it
						item.ThumbnailImage=strThumb;
						item.IconImage=strThumb;
						item.IconImageBig=strThumb;
						return;
					}
				}
				else
				{
					if (item.Label!="..")
					{
						// check for folder.jpg
						string strThumb=item.Path+@"\folder.jpg";
						if (System.IO.File.Exists(strThumb))
						{
							// got it
							item.ThumbnailImage=strThumb;
						}
					}
				}
			}
			catch(Exception)
			{
			}
		}
    
		static public string SecondsToShortHMSString(int lSeconds)
		{
			if (lSeconds<0) return ("0:00");
			int hh = lSeconds / 3600;
			lSeconds = lSeconds%3600;
			int mm = lSeconds / 60;
			int ss = lSeconds % 60;

			string strHMS="";
			strHMS=String.Format("{0}:{1:00}",hh,mm);
			return strHMS;
		}
		static public string SecondsToHMSString(int lSeconds)
		{
			if (lSeconds<0) return ("0:00");
			int hh = lSeconds / 3600;
			lSeconds = lSeconds%3600;
			int mm = lSeconds / 60;
			int ss = lSeconds % 60;

			string strHMS="";
			if (hh>=1)
				strHMS=String.Format("{0}:{1:00}:{2:00}",hh,mm,ss);
			else
				strHMS=String.Format("{0}:{1:00}",mm,ss);
			return strHMS;
		}
		static public string GetShortDayString(DateTime dt)
		{
			string day;
			switch (dt.DayOfWeek)
			{
				case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(657);	break;
				case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(658);	break;
				case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(659);	break;
				case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(660);	break;
				case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(661);	break;
				case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(662);	break;
				default:	day = GUILocalizeStrings.Get(17);	break;
			}
			return String.Format("{0} {1}-{2}", day,dt.Day,dt.Month);
		}
		static public string SecondsToHMString(int lSeconds)
		{
			if (lSeconds<0) return "0:00";
			int hh = lSeconds / 3600;
			lSeconds = lSeconds%3600;
			int mm = lSeconds / 60;

			string strHM="";
			if (hh>=1)
				strHM=String.Format("{0:00}:{1:00}",hh,mm);
			else
				strHM=String.Format("0:{0:00}",mm);
			return strHM;
		}

		static public void GetQualifiedFilename(string strBasePath,ref string strFileName)
		{
			if (strFileName==null) return;
			if (strFileName.Length<=2) return;
			if (strFileName[1]==':') return;
			strBasePath=Utils.RemoveTrailingSlash(strBasePath);
			while (strFileName.StartsWith(@"..\") || strFileName.StartsWith("../"))
			{
				strFileName=strFileName.Substring(3);
				int pos=strBasePath.LastIndexOf(@"\");
				if (pos > 0)
				{
					strBasePath=strBasePath.Substring(0,pos);
				}
				else
				{
					pos=strBasePath.LastIndexOf(@"/");
					if (pos>0)
					{
						strBasePath=strBasePath.Substring(0,pos);
					}
				}
			}
			if (strBasePath.Length==2 && strBasePath[1]==':')
				strBasePath+=@"\";
			strFileName=System.IO.Path.Combine(strBasePath,strFileName);
		}

		static public string stripHTMLtags(string strHTML)
		{
			if (strHTML==null) return String.Empty;
			if (strHTML.Length==0) return String.Empty;
			string stripped = Regex.Replace(strHTML,@"<(.|\n)*?>",string.Empty);
			return stripped.Trim() ;
		}
		static public bool IsNetwork(string strPath)
		{
			if (strPath==null) return false;
			if (strPath.Length<2) return false;
			string strDrive=strPath.Substring(0,2);
			if (getDriveType(strDrive)==4) return true;
			return false;
		}
		
		static public bool IsHD(string strPath)
		{
			if (strPath==null) return false;
			if (strPath.Length<2) return false;
			string strDrive=strPath.Substring(0,2);
			if (getDriveType(strDrive)==3) return true;
			return false;
		}

		static public bool IsCDDA(string strFile)
		{
			if (strFile==null) return false;
			if (strFile.Length<=0) return false;
			if (strFile.IndexOf("cdda:")>=0) return true;
			if (strFile.IndexOf(".cda")>=0) return true;
			return false;
		}

		static public bool IsDVD(string strFile)
		{
			if (strFile==null) return false;
			if (strFile.Length<2) return false;
			string strDrive=strFile.Substring(0,2);
			if (getDriveType(strDrive)==5) return true;
			return false;
		}

		static public bool IsRemovable(string strFile)
		{
			if (strFile==null) return false;
			if (strFile.Length<2) return false;
			string strDrive=strFile.Substring(0,2);
			if (getDriveType(strDrive)==2) return true;
			return false;
		}

		static public bool GetDVDLabel(string strFile, out string strLabel)
		{
			strLabel="";
			if (strFile==null) return false;
			if (strFile.Length==0) return false;
			string strDrive=strFile.Substring(0,2);
			strLabel=GetDriveName(strDrive);
			return true;    
		}
		static public bool ShouldStack(string strFile1, string strFile2)
		{
			if (strFile1==null) return false;
			if (strFile2==null) return false;
			try
			{
				// Patterns that are used for matching
				// 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
				// 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
				//     ? is -_ or space (second ? is optional), ## is 1 or 2 digits
				string[] pattern = {"\\[[0-9]{1,2}-[0-9]{1,2}\\]",
														 "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"};

				// Strip the extensions and make everything lowercase
				string strFileName1=System.IO.Path.GetFileNameWithoutExtension(strFile1).ToLower();
				string strFileName2=System.IO.Path.GetFileNameWithoutExtension(strFile2).ToLower();

				// Check all the patterns
				for (int i=0; i<pattern.Length; i++)
				{
					// See if we can find the special patterns in both filenames
					if (Regex.IsMatch(strFileName1, pattern[i]) && Regex.IsMatch(strFileName2, pattern[i]))
					{
						// Both strings had the special pattern. Now see if the filenames are the same.
						// Do this by removing the special pattern and compare the remains.
						if (Regex.Replace(strFileName1, pattern[i], "")
							== Regex.Replace(strFileName2, pattern[i], "") )
						{
							// It was a match so stack it
							return true;
						}
					}
				}
			}
			catch(Exception)
			{
			}

			// No matches were found, so no stacking
			return false;
		}

		static public void RemoveStackEndings(ref string strFileName)
		{
      
			if (strFileName==null) return ;
			string[] pattern = {"\\[[0-9]{1,2}-[0-9]{1,2}\\]",
													 "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"};
			for (int i=0; i<pattern.Length; i++)
			{
				// See if we can find the special patterns in both filenames
				if (Regex.IsMatch(strFileName, pattern[i]) )
				{
					strFileName=Regex.Replace(strFileName, pattern[i], "");
				}
			}
		}

		static public string GetThumb(string strLine)
		{
			if (strLine==null) return "000";
			try
			{
				if (String.Compare("unknown",strLine,true)==0) return "";
				CRCTool crc=new CRCTool();
				crc.Init(CRCTool.CRCCode.CRC32);
				ulong dwcrc=crc.calc(strLine);
				string strRet=System.IO.Path.GetFullPath(String.Format("thumbs\\{0}.jpg",dwcrc));
				return strRet;
			}
			catch(Exception)
			{
			}
			return "000";
		}
		
		static public void Split( string strFileNameAndPath, out string strPath, out string strFileName)
		{
			strFileName="";
			strPath="";
			if (strFileNameAndPath==null) return;
			if (strFileNameAndPath.Length==0) return;
			try
			{
				strFileNameAndPath=strFileNameAndPath.Trim();
				if (strFileNameAndPath.Length==0) return;
				int i=strFileNameAndPath.Length-1;
				while (i > 0)
				{
					char ch=strFileNameAndPath[i];
					if (ch==':' || ch=='/' || ch=='\\') break;
					else i--;
				}
				strPath     = strFileNameAndPath.Substring(0,i).Trim();
				strFileName = strFileNameAndPath.Substring(i,strFileNameAndPath.Length - i).Trim();
			}
			catch(Exception)
			{
				strPath="";
				strFileName=strFileNameAndPath;
			}
		}

		static public string GetFolderThumb(string strFile)
		{
			if (strFile==null) return "";
			if (strFile.Length==0) return "";
			string strPath,strFileName;
			Utils.Split( strFile, out strPath, out strFileName);
			string strFolderJpg = String.Format(@"{0}\folder.jpg", strPath);
			return strFolderJpg ;
		}
    
		static public bool EjectCDROM(string strDrive)
		{
			bool result = false;
			strDrive = @"\\.\"+strDrive;

			try 
			{
				IntPtr fHandle = CreateFile(strDrive, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 0, System.IO.FileMode.Open, 0x80, IntPtr.Zero);
				if (fHandle.ToInt64() != -1) //INVALID_HANDLE_VALUE)
				{
					uint Result;
					if (DeviceIoControl(fHandle, 0x002d4808, IntPtr.Zero, 0, IntPtr.Zero, 0, out Result, IntPtr.Zero) == true)
					{
						result = true;
					}
					CloseHandle(fHandle);
				}
			}
			catch(Exception)
			{
			}

			return result;
		}

		static public void EjectCDROM()
		{
			mciSendString( "set cdaudio door open", null, 0, IntPtr.Zero );
		}
    
		static public Process StartProcess(string strProgram, string strParams, bool bWaitForExit, bool bMinimized)
		{
			if (strProgram==null) return null;
			if (strProgram.Length==0) return null;
			Log.Write("Start process {0} {1}", strProgram,strParams);
			Process dvdplayer = new Process();

			string strWorkingDir=System.IO.Path.GetFullPath(strProgram);
			string strFileName=System.IO.Path.GetFileName(strProgram);
			strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
			dvdplayer.StartInfo.FileName=strFileName;
			dvdplayer.StartInfo.WorkingDirectory=strWorkingDir;
			dvdplayer.StartInfo.Arguments=strParams;
			if (bMinimized)
			{
				dvdplayer.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
				dvdplayer.StartInfo.CreateNoWindow=true;
			}
			dvdplayer.Start();
			if (bWaitForExit) dvdplayer.WaitForExit();
			return dvdplayer;
		}
		static public bool PlayDVD()
		{
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string strPath=xmlreader.GetValueAsString("dvdplayer","path","");
				string strParams=xmlreader.GetValueAsString("dvdplayer","arguments","");
				bool bInternal=xmlreader.GetValueAsBool("dvdplayer","internal",true);
				if (bInternal) return false;

				if (strPath!="")
				{
					if (System.IO.File.Exists(strPath))
					{
						Process dvdplayer = new Process();

						string strWorkingDir=System.IO.Path.GetFullPath(strPath);
						string strFileName=System.IO.Path.GetFileName(strPath);
						strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
						dvdplayer.StartInfo.FileName=strFileName;
						dvdplayer.StartInfo.WorkingDirectory=strWorkingDir;

						if (strParams.Length>0)
						{
							dvdplayer.StartInfo.Arguments=strParams;
						}
						Log.Write("start process {0} {1}",strPath,dvdplayer.StartInfo.Arguments);

						dvdplayer.Start();
						dvdplayer.WaitForExit();
						Log.Write("{0} done",strPath);
					}
					else
					{
						Log.Write("file {0} does not exists", strPath);
					}
				}
			}
			return true;
		}

		static public bool PlayMovie(string strFile)
		{
			if (strFile==null) return false;
			if (strFile.Length==0) return false; 

			try
			{
				string strExt=System.IO.Path.GetExtension(strFile).ToLower();
				if (strExt.Equals(".tv")) return false;
				if (strExt.Equals(".sbe")) return false;
				if (strExt.Equals(".dvr-ms")) return false;
				if (strExt.Equals(".radio")) return false;
				if ( strFile.IndexOf("record0.")>0 || strFile.IndexOf("record1.")>0 || 
					strFile.IndexOf("record2.")>0 || strFile.IndexOf("record3.")>0 || 
					strFile.IndexOf("record4.")>0 || strFile.IndexOf("record5.")>0 ) return false;

				using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
				{
					bool bInternal=xmlreader.GetValueAsBool("movieplayer","internal",true);
					if (bInternal) return false;
					string strPath=xmlreader.GetValueAsString("movieplayer","path","");
					string strParams=xmlreader.GetValueAsString("movieplayer","arguments","");
					if (strExt.ToLower()==".ifo" || strExt.ToLower()==".vob")
					{
						strPath=xmlreader.GetValueAsString("dvdplayer","path","");
						strParams=xmlreader.GetValueAsString("dvdplayer","arguments","");
					}
					if (strPath!="")
					{
						if (System.IO.File.Exists(strPath))
						{
							if (strParams.IndexOf("%filename%")>=0)
								strParams=strParams.Replace("%filename%", "\""+strFile+"\"" );

							Process movieplayer = new Process();
							string strWorkingDir=System.IO.Path.GetFullPath(strPath);
							string strFileName=System.IO.Path.GetFileName(strPath);
							strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
							movieplayer.StartInfo.FileName=strFileName;
							movieplayer.StartInfo.WorkingDirectory=strWorkingDir;
							if (strParams.Length>0)
							{
								movieplayer.StartInfo.Arguments=strParams;
							}
							else
							{
								movieplayer.StartInfo.Arguments="\""+strFile+"\"";
							}
							Log.Write("start process {0} {1}",strPath,movieplayer.StartInfo.Arguments);
							movieplayer.Start();
							movieplayer.WaitForExit();
							Log.Write("{0} done",strPath);
							return true;
						}
						else
						{
							Log.Write("file {0} does not exists", strPath);
						}
					}
				}
			}
			catch(Exception)
			{
			}
			return false;
		}

		static public DateTime longtodate(long ldate)
		{
			if (ldate<0) return DateTime.MinValue;
			int year,month,day,hour,minute,sec;
			sec=(int)(ldate%100L); ldate /=100L;
			minute=(int)(ldate%100L); ldate /=100L;
			hour=(int)(ldate%100L); ldate /=100L;
			day=(int)(ldate%100L); ldate /=100L;
			month=(int)(ldate%100L); ldate /=100L;
			year=(int)ldate;
			DateTime dt=new DateTime(year,month,day,hour,minute,0,0);
			return dt;
		}

		static public long datetolong(DateTime dt)
		{
			long iSec=0;//(long)dt.Second;
			long iMin=(long)dt.Minute;
			long iHour=(long)dt.Hour;
			long iDay=(long)dt.Day;
			long iMonth=(long)dt.Month;
			long iYear=(long)dt.Year;

			long lRet=(iYear);
			lRet=lRet*100L + iMonth;
			lRet=lRet*100L + iDay;
			lRet=lRet*100L + iHour;
			lRet=lRet*100L + iMin;
			lRet=lRet*100L + iSec;
			return lRet;
		}
		static public string MakeFileName(string strText)
		{
			if (strText==null) return String.Empty;
			if (strText.Length==0) return String.Empty;
			string strFName=strText.Replace(':', '_');
			strFName=strFName.Replace('/', '_');
			strFName=strFName.Replace('\\', '_');
			strFName=strFName.Replace('*', '_');
			strFName=strFName.Replace('?', '_');
			strFName=strFName.Replace('\"', '_');
			strFName=strFName.Replace('<', '_');;
			strFName=strFName.Replace('>', '_');
			strFName=strFName.Replace('|', '_');
			return strFName;

		}
		static public bool FileDelete(string strFile)
		{
			if (strFile==null) return true;
			if (strFile.Length==0) return  true;
			try
			{
				if (!System.IO.File.Exists(strFile)) return true;
				System.IO.File.Delete(strFile);
				return true;
			}
			catch(Exception)
			{
			}
			return false;
		}
    static public void DirectoryDelete(string strDir)
    {
      if (strDir==null) return ;
      if (strDir.Length==0) return ;
      try
      {
        System.IO.Directory.Delete(strDir);
      }
      catch(Exception)
      {
      }
    }
		static public void DownLoadImage(string strURL, string strFile, System.Drawing.Imaging.ImageFormat imageFormat)
		{
			if (strURL==null) return ;
			if (strURL.Length==0) return ;
			if (strFile==null) return ;
			if (strFile.Length==0) return ;

			using (WebClient client = new WebClient())
			{
				try
				{
					string strExtURL =System.IO.Path.GetExtension(strURL);
					string strExtFile=System.IO.Path.GetExtension(strFile);
					if (strExtURL.Length>0 && strExtFile.Length>0)
					{
						strExtURL=strExtURL.ToLower();
						strExtFile=strExtFile.ToLower();
						string strLogo=System.IO.Path.ChangeExtension(strFile,strExtURL);
						client.DownloadFile(strURL, strLogo);
						if (strExtURL != strExtFile)
						{
							using (Image imgSrc = Image.FromFile(strLogo) )
							{
								imgSrc.Save(strFile,imageFormat);
							}
							Utils.FileDelete(strLogo);
						}
						GUITextureManager.CleanupThumbs();
					}
				} 
				catch(Exception ex)
				{
					Log.Write("download failed:{0}", ex.Message);
				}
			}
		}


		static public void DownLoadAndCacheImage(string strURL, string strFile)
		{
			if (strURL==null) return ;
			if (strURL.Length==0) return ;
			if (strFile==null) return ;
			if (strFile.Length==0) return ;
			string url=String.Format("cache{0}",EncryptLine(strURL));

			string file=GetCoverArt("thumbs", url);
			if (file!=String.Empty)
			{
				try
				{
					System.IO.File.Copy(file,strFile,true);
				}
				catch(Exception)
				{
				}
				return;
			}
			DownLoadImage(strURL, strFile);
			if (System.IO.File.Exists(strFile))
			{
				try
				{
					file=GetCoverArtName("thumbs", url);
					System.IO.File.Copy(strFile,file,true);
				}
				catch(Exception)
				{
				}
			}

		}
		static public void DownLoadImage(string strURL, string strFile)
		{
			if (strURL==null) return ;
			if (strURL.Length==0) return ;
			if (strFile==null) return ;
			if (strFile.Length==0) return ;
			try
			{

				HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(strURL);
				wr.Timeout=5000;
				HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();

				Stream str = ws.GetResponseStream();
				byte[] inBuf = new byte[900000];
				int bytesToRead = (int) inBuf.Length;
				int bytesRead = 0;

				DateTime dt=DateTime.Now;
				while (bytesToRead > 0) 
				{
					dt=DateTime.Now;
					int n = str.Read(inBuf, bytesRead,bytesToRead);
					if (n==0)
						break;
					bytesRead += n;
					bytesToRead -= n;
					TimeSpan ts=DateTime.Now-dt;
					if (ts.TotalSeconds>=5)
					{
						throw new Exception("timeout");
					}
				}
				FileStream fstr = new FileStream(strFile, FileMode.OpenOrCreate,FileAccess.Write);
				fstr.Write(inBuf, 0, bytesRead);
				str.Close();
				fstr.Close();

				GUITextureManager.CleanupThumbs();
			} 
			catch(Exception ex)
			{
				Log.Write("download failed:{0}", ex.Message);
			}
		}


		static public string RemoveTrailingSlash(string strLine)
		{
			if (strLine==null) return String.Empty;
			if (strLine.Length==0) return String.Empty;
			string strPath=strLine;
			while (strPath.Length>0)
			{
				if ( strPath[strPath.Length-1]=='\\' || strPath[strPath.Length-1]=='/')
				{
					strPath=strPath.Substring(0,strPath.Length-1);
				}
				else break;
			}
			return strPath;
		}
		static public void RGB2YUV(int R, int G, int B, out int Y, out int U, out int V)
		{
			Y = (int)(  ((float)R) *  0.257f + ((float)G) *  0.504f + ((float)B) *  0.098f + 16.0f);
			U = (int)(  ((float)R) * -0.148f + ((float)G) * -0.291f + ((float)B) *  0.439f + 128.0f);
			V = (int)(  ((float)R) *  0.439f + ((float)G) * -0.368f + ((float)B) * -0.071f + 128.0f);
			Y=Y&0xff;
			U=U&0xff;
			V=V&0xff;
		}
		static public void RGB2YUV(int iRGB, out int YUV)
		{
			int Y,U,V;
			RGB2YUV( (iRGB>>16)&0xff, (iRGB>>8)&0xff, (iRGB&0xff), out Y,out U,out V);
      
			Y<<=16;
			U<<=8;

			YUV=Y+U+V;
		}
		static public string GetFilename(string strPath)
		{
			if (strPath==null) return String.Empty;
			if (strPath.Length==0) return String.Empty;
			try
			{
				if ( m_bHideExtensions)
					return  System.IO.Path.GetFileNameWithoutExtension(strPath);
				else
					return  System.IO.Path.GetFileName(strPath);     
			}
			catch(Exception)
			{
			}
			return strPath;
		}


		///<summary>
		///Plays a sound from a byte array. 
		///Note: If distortion or corruption of 
		//     audio playback occurs, 
		///try using synchronous playback, or sa
		//     ve to a temp file and
		///use the file-based option.
		///</summary>
		public static int PlaySound(byte[] audio, bool bSynchronous, bool bIgnoreErrors) 
		{
			if (audio==null) return 0;
			return PlaySound(audio, bSynchronous, bIgnoreErrors, false, false, false);
		}
		///<summary>
		///Plays a sound from a byte array. 
		///Note: If distortion or corruption of 
		//     audio playback occurs, 
		///try using synchronous playback, or sa
		//     ve to a temp file and
		///use the file-based option.
		///</summary>
		public static int PlaySound(byte[] audio, bool bSynchronous, bool bIgnoreErrors,
			bool bNoDefault, bool bLoop, bool bNoStop) 
		{
			if (audio==null) return 0;
			const int SND_ASYNC = 1;
			const int SND_NODEFAULT = 2;
			const int SND_MEMORY = 4;
			const int SND_LOOP = 8;
			const int SND_NOSTOP = 16;
			int Snd_Options = SND_MEMORY;
			if (!bSynchronous) 
			{
				Snd_Options += SND_ASYNC;
			}
			if (bNoDefault) Snd_Options += SND_NODEFAULT;
			if (bLoop) Snd_Options += SND_LOOP;
			if (bNoStop) Snd_Options += SND_NOSTOP;
			try 
			{
				return PlaySound(audio, 0, Snd_Options);
			} 
			catch (Exception ex) 
			{
				if (!bIgnoreErrors) 
				{
					throw ex;
				} 
				else 
				{
					return 0;
				}
			}
		}
		public static int PlaySound(string sSoundFile, bool bSynchronous, bool bIgnoreErrors) 
		{
			if (sSoundFile==null) return 0;
			if (sSoundFile.Length==0) return 0;
			return PlaySound(sSoundFile, bSynchronous, bIgnoreErrors, false, false, false);
		}
		public static int PlaySound(string sSoundFile, bool bSynchronous, bool bIgnoreErrors,
			bool bNoDefault, bool bLoop, bool bNoStop) 
		{
			const int SND_ASYNC = 1;
			const int SND_NODEFAULT = 2;
			const int SND_LOOP = 8;
			const int SND_NOSTOP = 16;
      
			if (sSoundFile==null) return 0;
			if (sSoundFile.Length==0) return 0;
			if (!System.IO.File.Exists(sSoundFile)) 
			{
				string strSkin=GUIGraphicsContext.Skin;
				if (System.IO.File.Exists(strSkin + "\\sounds\\" + sSoundFile)) 
				{
					sSoundFile = strSkin + "\\sounds\\" + sSoundFile;
				} 
				else if (System.IO.File.Exists(strSkin + "\\" + sSoundFile + ".wav")) 
				{
					sSoundFile = strSkin + "\\" + sSoundFile + ".wav";
				} 
				else 
				{
					Log.Write(@"Cannot find sound:{0}\sounds\{1} ", strSkin, sSoundFile);
					return 0;
				}
			}
			int Snd_Options = 0;
			if (!bSynchronous) 
			{
				Snd_Options = SND_ASYNC;
			}
			if (bNoDefault) Snd_Options += SND_NODEFAULT;
			if (bLoop) Snd_Options += SND_LOOP;
			if (bNoStop) Snd_Options += SND_NOSTOP;
			try 
			{
				return sndPlaySoundA(sSoundFile, Snd_Options);
			} 
			catch (Exception ex) 
			{
				if (!bIgnoreErrors) 
				{
					throw ex;
				} 
				else 
				{
					return 0;
				}
			}
		}
		[DllImport("winmm.dll")]
		private static extern int sndPlaySoundA(string lpszSoundName, int uFlags);
		[DllImport("winmm.dll")]
		private static extern int PlaySound(byte[] pszSound, Int16 hMod, long fdwSound);
		static public int GetNextForwardSpeed(int iCurSpeed)
		{
			switch (iCurSpeed)
			{
				case -32:
					return -16;
				case -16:
					return -8;
				case -8:
					return -4;
				case -4:
					return -2;
				case -2:
					return 1;
				case 1:
					return 2;
				case 2:
					return 4;
				case 4:
					return 8;
				case 8:
					return 16;
				case 16:
					return 32;
			}

			return 1;
		}

		static public int GetNextRewindSpeed(int iCurSpeed)
		{
			switch (iCurSpeed)
			{
				case -16: 
					return -32;
				case -8: 
					return -16;
				case -4: 
					return -8;
				case -2: 
					return -4;
				case 1: 
					return -2;
				case 2:
					return 1;
				case 4:
					return 2;
				case 8:
					return 4;
				case 16:
					return 8;
				case 32:
					return 16;
			}
			return 1;
		}

		static public string FilterFileName(string strName)
		{
			if (strName==null) return String.Empty;
			if (strName.Length==0) return String.Empty;
			strName=strName.Replace(@"\","_");
			strName=strName.Replace("/","_");
			strName=strName.Replace(":","_");
			strName=strName.Replace("*","_");
			strName=strName.Replace("?","_");
			strName=strName.Replace("\"","_");
			strName=strName.Replace("<","_");
			strName=strName.Replace(">","_");
			strName=strName.Replace("|","_");
			return strName;
		}

		static public string EncryptLine(string strLine)
		{
			if (strLine==null) return String.Empty;
			if (strLine.Length==0) return String.Empty;
			if (String.Compare("unknown",strLine,true)==0) return String.Empty;
			CRCTool crc=new CRCTool();
			crc.Init(CRCTool.CRCCode.CRC32);
			ulong dwcrc=crc.calc(strLine);
			string strRet=String.Format("{0}",dwcrc);
			return strRet;
		}

		static public string GetCoverArt(string strFolder,string strFileName)
		{
			if (strFolder==null) return String.Empty;
			if (strFolder.Length==0) return String.Empty;
      
			if (strFileName==null) return String.Empty;
			if (strFileName.Length==0) return String.Empty;
			if (strFileName==String.Empty) return String.Empty;/*
			try
			{
				string tbnImage = System.IO.Path.ChangeExtension(strFileName,".tbn");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".png");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".gif");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".jpg");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
			}
			catch(Exception){}*/

			string strThumb=String.Format(@"{0}\{1}",strFolder,Utils.FilterFileName(strFileName));
			if (System.IO.File.Exists(strThumb+".jpg")) return strThumb+".jpg";
			else if (System.IO.File.Exists(strThumb+".png")) return strThumb+".png";
			else if (System.IO.File.Exists(strThumb+".gif")) return strThumb+".gif";
			else if (System.IO.File.Exists(strThumb+".tbn")) return strThumb+".tbn";
			return String.Empty;
		}

		static public string ConvertToLargeCoverArt(string smallArt)
		{
			if (smallArt==null) return String.Empty;
			if (smallArt.Length==0) return String.Empty;
			if (smallArt==String.Empty) return smallArt;
			return smallArt.Replace(".jpg", "L.jpg");
		}

		static public string GetCoverArtName(string strFolder,string strFileName)
		{
			if (strFolder==null) return String.Empty;
			if (strFolder.Length==0) return String.Empty;

			if (strFileName==null) return String.Empty;
			if (strFileName.Length==0) return String.Empty;

			string strThumb=Utils.GetCoverArt(strFolder,strFileName);
			if (strThumb==String.Empty)
			{
				strThumb=String.Format(@"{0}\{1}.jpg",strFolder,Utils.FilterFileName(strFileName));
			}
			return strThumb;
		}
		static public string GetLargeCoverArtName(string strFolder,string strFileName)
		{
			if (strFolder==null) return String.Empty;
			if (strFolder.Length==0) return String.Empty;

			if (strFileName==null) return String.Empty;
			if (strFileName.Length==0) return String.Empty;

			string strThumb=String.Format(@"{0}\{1}L.jpg",strFolder,Utils.FilterFileName(strFileName));
			if (strThumb==String.Empty)
			{
				strThumb=Utils.GetCoverArt(strFolder,strFileName);
			}
			return strThumb;
		}


		static public void DeleteFiles(string strDir, string strPattern)
		{
			if (strDir==null) return ;
			if (strDir.Length==0) return ;

			if (strPattern==null) return ;
			if (strPattern.Length==0) return ;

			string[] strFiles;
			try
			{
				strFiles=System.IO.Directory.GetFiles(strDir,strPattern);
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}

		}
		static public void KillExternalTVProcesses()
		{
	
			Process[] myProcesses;
        
			// kill ehtray.exe since that program catches the mce remote keys
			// and will start mce 2005
			myProcesses = Process.GetProcesses();
			foreach(Process myProcess in myProcesses)
			{
				if (myProcess.ProcessName.ToLower().Equals("ehrecvr.exe"))
				{
					try
					{
						myProcess.Kill();
					}
					catch(Exception){}
					return;
				}
			}
		}
		static public DateTime ParseDateTimeString(string dateTime)
		{
			try
			{
				if (dateTime==null) return DateTime.Now;
				if (dateTime.Length==0) return DateTime.Now;
				//format is d-m-y h:m:s
				dateTime=dateTime.Replace(":","-");
				string[] parts = dateTime.Split('-');
				if (parts.Length<6) return DateTime.Now;
				int hour,min,sec,year,day,month;
				day=Int32.Parse(parts[0]);
				month=Int32.Parse(parts[1]);
				year=Int32.Parse(parts[2]);
				
				hour=Int32.Parse(parts[3]);
				min=Int32.Parse(parts[4]);
				sec=Int32.Parse(parts[5]);
				return new DateTime(year,month,day,hour,min,sec,0);
			}
			catch(Exception)
			{
			}
			return DateTime.Now;
		}
	}

}

