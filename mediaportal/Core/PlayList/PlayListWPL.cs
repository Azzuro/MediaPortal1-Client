/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayListWPL : PlayList
	{

		public PlayListWPL()
		{
			// 
			// TODO: Add constructor logic here
			//
		}
		public override bool Load(string fileName)
		{
			Clear();

			try
			{
				string basePath=System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(fileName));
			
				XmlDocument doc= new XmlDocument();
				doc.Load(fileName);
				if (doc.DocumentElement==null) return false;
				XmlNode nodeRoot=doc.DocumentElement.SelectSingleNode("/smil/body/seq");
				if (nodeRoot==null) return false;
				XmlNodeList nodeEntries=nodeRoot.SelectNodes("media");
				foreach (XmlNode node in nodeEntries)
				{
					XmlNode srcNode=node.Attributes.GetNamedItem("src");					
					if (srcNode!=null)
					{
						if (srcNode.InnerText!=null)
						{
							if (srcNode.InnerText.Length>0)
							{
								fileName=srcNode.InnerText;
								Utils.GetQualifiedFilename(basePath,ref fileName);
								PlayList.PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
								newItem.Type = PlayListItem.PlayListItemType.Audio;
								string description;
								description=System.IO.Path.GetFileName(fileName);
								newItem.Description=description;
								Add(newItem);
							}
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Write("exception loading playlist {0} err:{1} stack:{2}", fileName, ex.Message,ex.StackTrace);
			}
			return false;

		}

		public override void 	Save(string fileName)  
		{
		}

	}
}
