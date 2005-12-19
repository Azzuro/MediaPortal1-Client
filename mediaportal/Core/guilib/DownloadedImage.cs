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
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;



namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for DownloadedImage.
	/// </summary>
	class DownloadedImage
	{
		string    _fileName;
		string    _url;
		DateTime  _dateDownloaded=DateTime.MinValue;
		int       _cacheMinutes = 60*30; //30minutes

		public DownloadedImage(string url)
		{
			URL=url;
			int pos=url.LastIndexOf("/");
        
			_fileName=GetTempFileName();
		}

		string GetTempFileName()
		{
			int x=0;
			while (true)
			{
				string tempFile=String.Format(@"thumbs\MPTemp{0}.gif",x);
				string tempFile2=String.Format(@"thumbs\MPTemp{0}.jpg",x);
				string tempFile3=String.Format(@"thumbs\MPTemp{0}.bmp",x);
				if (!System.IO.File.Exists(tempFile) && 
					!System.IO.File.Exists(tempFile2) &&
					!System.IO.File.Exists(tempFile3))
				{
					return tempFile;
				}
				++x;
			}
		}
      
      
		public string FileName
		{
			get {return _fileName;}
			set {_fileName=value;}
		}
      
		public string URL
		{
			get { return _url;}
			set {_url=value;}
		}

		public int CacheTime
		{
			get { return _cacheMinutes;}
			set { _cacheMinutes=value;}
		}

		public bool ShouldDownLoad
		{
			get 
			{
				TimeSpan ts=DateTime.Now - _dateDownloaded;
				if (ts.TotalSeconds > CacheTime)
				{
					return true;
				}
				return false;
			}
		}

		public bool Download()
		{
			using (WebClient client = new WebClient())
			{
				try
				{
					try
					{
						System.IO.File.Delete(FileName);
					}
					catch(Exception)
					{
						Log.Write("DownloadedImage:Download() Delete failed:{0}", FileName);
					}

					client.DownloadFile(URL, FileName);
					try
					{
						string extension="";
						string contentType=client.ResponseHeaders["Content-type"].ToLower();
						if (contentType.IndexOf("gif")>=0) extension=".gif";
						if (contentType.IndexOf("jpg")>=0) extension=".jpg";
						if (contentType.IndexOf("jpeg")>=0) extension=".jpg";
						if (contentType.IndexOf("bmp")>=0) extension=".bmp";
						if (extension.Length>0)
						{
							string newFile=System.IO.Path.ChangeExtension(FileName,extension);
							if (!newFile.ToLower().Equals(FileName.ToLower()))
							{
								try
								{
									System.IO.File.Delete(newFile);
								}
								catch(Exception)
								{
									Log.Write("DownloadedImage:Download() Delete failed:{0}", newFile);
								}
								System.IO.File.Move(FileName,newFile);
								FileName=newFile;
							}
						}
					}
					catch(Exception)
					{
						Log.Write("DownloadedImage:Download() DownloadFile failed:{0}->{1}", URL,FileName);

					}
					_dateDownloaded=DateTime.Now;
					return true;
				} 
				catch(Exception ex)
				{
					Log.Write("download failed:{0}", ex.Message);
				}
			}
			return false;
		}
	}
}
