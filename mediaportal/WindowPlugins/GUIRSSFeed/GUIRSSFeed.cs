/*
 * Created by SharpDevelop.
 * User: Josh
 * Date: 6/17/2004
 * Time: 7:42 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Net;
using System.Text;
using MediaPortal.GUI.Library;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using Rss;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.RSS
{
	/// <summary>
	/// A My News (RSS Feed) Plugin for MediaPortal
	/// </summary>
	public class GUIRSSFeed : GUIWindow
	{

		class Site
		{
			public string m_Name;
			public string m_URL;
			public string m_Image;
			public string m_Description;
		}

		struct feed_details
		{
			public string		m_site;
			public string		m_title;
			public string		m_description;
		};

		enum Controls
		{
			CONTROL_BACKGROUND 		= 1,
			CONTROL_BTNREFRESH		= 2,
			CONTROL_BTNCHANNEL    = 4,
			CONTROL_LABELUPDATED	= 11,
			CONTROL_LABELCHANNEL	= 12,
			CONTROL_IMAGELOGO		= 101,
			CONTROL_LIST			= 50,
			CONTROL_FEEDTITLE 		= 500,
			CONTROL_STORY1			= 501,
			CONTROL_STORY2			= 502,
			CONTROL_STORY3			= 503,
			CONTROL_STORY4			= 504,
			CONTROL_STORY5			= 505,
			CONTROL_STORYTEXT		= 506
		}

		public static int WINDOW_RSS = 2700;

		public static string DEFAULT_NEWS_ICON = "news.png";

		const int 		NUM_STORIES = 100;
		ArrayList		m_sites = new ArrayList();
		string			m_strSiteIcon=DEFAULT_NEWS_ICON;
		string			m_strSiteName="";
		string			m_strSiteURL="";
		string			m_strDescription="";
		DateTime        m_lRefreshTime=DateTime.Now.AddHours(-1);		//for autorefresh
		feed_details[]	m_feed_details		=  new 	feed_details[NUM_STORIES];
		int         m_iRSSRefresh=15;
		bool				m_bRSSAutoRefresh=true;

		GUIImage		m_pSiteImage=null;

		public  GUIRSSFeed()
		{
			for(int i=0; i<NUM_STORIES; i++)
			{
				m_feed_details[i].m_site= "";
				m_feed_details[i].m_title= "";
				m_feed_details[i].m_description = "";
			}
			GetID=(int)GUIRSSFeed.WINDOW_RSS;
		}

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myrss.xml");
		}

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
					GUIWindowManager.PreviousWindow();
					return;
				}
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					for(int j=0; j<NUM_STORIES; j++)
					{
						m_feed_details[j].m_site= "";
						m_feed_details[j].m_title= "";
						m_feed_details[j].m_description = "";
					}

					base.OnMessage(message);
					LoadSettings();
					m_pSiteImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGELOGO);

					UpdateNews(true);

					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					SaveSettings();
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
				{
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_LIST)
					{
						UpdateDetails();
					}
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
					if (iControl == (int)Controls.CONTROL_BTNREFRESH)
					{            
						UpdateNews(true);
					}

					if (iControl==(int)Controls.CONTROL_LIST)
					{
						UpdateDetails();
					}

					if (iControl==(int)Controls.CONTROL_BTNCHANNEL)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);

						int nSelected = (int)msg.Param1;

						m_strSiteURL = ((Site)m_sites[nSelected]).m_URL;
						m_strSiteName = ((Site)m_sites[nSelected]).m_Name;
						m_strSiteIcon = ((Site)m_sites[nSelected]).m_Image;
						m_strDescription = ((Site)m_sites[nSelected]).m_Description;

						if (m_strDescription == "")
						{
							m_strDescription = ((Site)m_sites[nSelected]).m_Name;
						}

						UpdateNews(true);

						return true;
					}

				}
					break;
			}
			return base.OnMessage(message);
		}

		public override void Process()
		{
			TimeSpan ts=DateTime.Now-m_lRefreshTime;
			if ((m_bRSSAutoRefresh) && (ts.TotalMinutes >= m_iRSSRefresh && m_strSiteURL!=""))
			{
				// Reset time
				m_lRefreshTime = DateTime.Now;

				// Check if we may refresh (only when not in the list or on the first item)
				if (GetFocusControlId() != (int)Controls.CONTROL_LIST)
				{
					// Try refresh without warnings
					UpdateNews(false);
				}
				else
				{
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_LIST,0,0,null);
					OnMessage(msg);
					if ((int)msg.Param1 < 1)
					{
						// Try refresh without warnings
						UpdateNews(false);
					}
				}
			}         
			base.Process ();
		}


		private void RefreshNews(object sender,System.EventArgs e)
		{
			System.GC.Collect();
		}

		void UpdateNews(bool bShowWarning)
		{
			try
			{
				if (m_strSiteURL.ToLower().StartsWith("http://") ==false && m_strSiteURL.ToLower().StartsWith("file://") == false)
				{
					m_strSiteURL="http://"+m_strSiteURL;
				}
				Uri newURL=new Uri(m_strSiteURL);
				Download(newURL);
				UpdateButtons();
			}
			catch(Exception)
			{
				if (bShowWarning)
				{
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
					msg.Param1=9;//my news
					msg.Param2=912;//Unable to download latest news
					msg.Param3=0;
					msg.Label3=m_strSiteURL;
					GUIWindowManager.SendMessage(msg);
				}
			}
		}

		#region Serialisation
		void LoadSettings()
		{
			String firstSite = "";
			m_sites.Clear();
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				for (int i=0; i < NUM_STORIES; i++)
				{
					string strNameTag=String.Format("siteName{0}",i);
					string strURLTag=String.Format("siteURL{0}",i);
					string strImageTag = String.Format("siteImage{0}", i);
					string strDescriptionTag = String.Format("siteDescription{0}", i);

					string strName=xmlreader.GetValueAsString("rss",strNameTag,"");
					string strURL=xmlreader.GetValueAsString("rss",strURLTag,"");
					string strImage = xmlreader.GetValueAsString("rss", strImageTag, "");
					string strDescription = xmlreader.GetValueAsString("rss", strDescriptionTag, "");

					if (strName.Length>0 && strURL.Length>0)
					{
						if (strImage.Length==0) strImage=DEFAULT_NEWS_ICON;

						if (firstSite == "")
						{
							m_strSiteName=strName;
							m_strSiteURL=strURL;
							m_strSiteIcon = strImage;
							m_strDescription = strDescription;
							firstSite = strURL;
						}

						Site loc = new Site();
						loc.m_Name=strName;
						loc.m_URL=strURL;
						loc.m_Image = strImage;
						loc.m_Description = strDescription;
						m_sites.Add(loc);
					}
				}

				// General settings
				m_iRSSRefresh=xmlreader.GetValueAsInt("rss","iRefreshTime",15);
				m_bRSSAutoRefresh=false;
				if (xmlreader.GetValueAsInt("rss","bAutoRefresh",0) != 0)
					m_bRSSAutoRefresh=true;
			}
		}

		void SaveSettings()
		{
		}
		#endregion

		public void refreshFeeds()
		{
			m_sites.Clear();
			for(int j=0; j<NUM_STORIES; j++)
			{
				m_feed_details[j].m_site= "";
				m_feed_details[j].m_title= "";
				m_feed_details[j].m_description = "";
			}

			LoadSettings();
			UpdateNews(true);
		}

		void UpdateButtons()
		{
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNREFRESH, GUILocalizeStrings.Get(184));			//Refresh
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELCHANNEL, m_strDescription);			//Channel name label
			GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(9) + @"/" + m_strSiteName);

			int posX = m_pSiteImage.XPosition;
			int posY = m_pSiteImage.YPosition;

			m_pSiteImage.SetPosition(posX,posY);
			m_pSiteImage.ColourDiffuse=0xffffffff;
			m_pSiteImage.SetFileName(m_strSiteIcon);

			m_pSiteImage.Width = m_pSiteImage.TextureWidth;
			m_pSiteImage.Height = m_pSiteImage.TextureHeight;

			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);

			int iTotalItems=0;
			foreach (feed_details feed in m_feed_details)
			{
				if (feed.m_title == "" && feed.m_description == "")
				{
					// Skip this empty item
					continue;
				}

				GUIListItem item = new GUIListItem();
				item.Label=feed.m_title;
				item.IsFolder=false;
				item.MusicTag = feed;
				item.ThumbnailImage="";
				item.IconImage="defaultMyNews.png";

				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
				iTotalItems++;
			}

			string strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);

			GUIControl.FocusControl(GetID,(int)Controls.CONTROL_LIST);
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STORYTEXT, m_feed_details[0].m_description);

			// Setup the selectionbutton for the channels
			int i=0;

			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_BTNCHANNEL);
			foreach (Site loc in m_sites)
			{
				GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_BTNCHANNEL, loc.m_Name);
				if (m_strSiteURL==loc.m_URL )
				{
					GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BTNCHANNEL, i);
				}
				i++;
			}
		}

		bool Download(Uri location)
		{
			int i=0;
			Uri uri = location;

			for(int j=0; j<NUM_STORIES; j++)
			{
				m_feed_details[j].m_site= "";
				m_feed_details[j].m_title= "";
				m_feed_details[j].m_description = "";
			}

			try
			{
				RssFeed feed = RssFeed.Read(location.ToString());
				Rss.RssChannel channel = (Rss.RssChannel)feed.Channels[0];

				// Download the image from the feed if available/needed
				if (channel.Image != null && m_strSiteIcon == DEFAULT_NEWS_ICON)
				{
					string strImage=channel.Image.Url.ToString();

					if (strImage.Length >0)
					{
						m_strSiteIcon=Utils.GetThumb(m_strSiteURL);
						if (!System.IO.File.Exists(m_strSiteIcon) )
						{
							string strExtension;
							strExtension=System.IO.Path.GetExtension(strImage);
							if ( strExtension.Length>0)
							{
								string strTemp="temp";
								strTemp+=strExtension;
								Utils.FileDelete(strTemp);

								Utils.DownLoadImage(strImage, strTemp);
								if (System.IO.File.Exists(strTemp) )
								{
									MediaPortal.Util.Picture.CreateThumbnail(strTemp,m_strSiteIcon,128,128,0);
								}

								Utils.FileDelete(strTemp);
							}//if ( strExtension.Length>0)
							else
							{
								Log.Write("image has no extension:{0}", strImage);
							}
						}
						m_strSiteIcon=Utils.GetThumb(m_strSiteURL);
					}
				}

				// Fill the items
				foreach(Rss.RssItem item in channel.Items)
				{
					// Check if there are HTML tags in the description, if so we need to "parse" the HTML
					// which is actually just stripping the tags.
					if (Regex.IsMatch(item.Description, @"<[^>]*>"))
					{
						// Strip \n's
						item.Description = Regex.Replace(item.Description, @"(\n|\r)", "", RegexOptions.Multiline);

						// Remove whitespace (double spaces)
						item.Description = Regex.Replace(item.Description, @"  +", "", RegexOptions.Multiline);

						// Replace <br/> with \n
						item.Description = Regex.Replace(item.Description, @"< *br */*>", "\n", RegexOptions.IgnoreCase & RegexOptions.Multiline);

						// Remove remaining HTML tags
						item.Description = Regex.Replace(item.Description, @"<[^>]*>", "", RegexOptions.Multiline);
					}

					// Strip newlines from titles because it looks funny to see multiple lines in the listbox
					item.Title = Regex.Replace(item.Title, @" *(\n|\r)+ *", " ", RegexOptions.Multiline);

					if (item.Description == "")
					{
						item.Description = item.Title;
					}
					else
					{
						item.Description = item.Title + ": \n\n" + item.Description + "\n";
					}

					//
					m_feed_details[i].m_site = channel.Title;
					m_feed_details[i].m_title = item.Title;
					m_feed_details[i].m_description = item.Description;

					// Make sure that everything is "decoded" like &amp; becomes &
					m_feed_details[i].m_title = HttpUtility.HtmlDecode(m_feed_details[i].m_title);
					m_feed_details[i].m_description = HttpUtility.HtmlDecode(m_feed_details[i].m_description);

					i++;
					if (i >= NUM_STORIES)
						break;
				}

				UpdateButtons();
				m_lRefreshTime = DateTime.Now;
			}
			catch (WebException ex)
			{
				m_lRefreshTime = DateTime.Now;
				throw ex;
			}

			return true;
		}

		void GetString(XmlNode pRootElement, string strTagName, out string szValue, string strDefaultValue)
		{
			szValue="";

			XmlNode node=pRootElement.SelectSingleNode(strTagName);
			if (node !=null)
			{
				if (node.InnerText!=null)
				{
					if (node.InnerText!="-")
						szValue=node.InnerText;
				}
			}
			if (szValue.Length==0)
			{
				szValue=strDefaultValue;
			}
		}

		void GetInteger(XmlNode pRootElement, string strTagName, out int iValue)
		{
			iValue=0;
			XmlNode node=pRootElement.SelectSingleNode(strTagName);
			if (node !=null)
			{
				if (node.InnerText!=null)
				{
					try
					{
						iValue=Int32.Parse(node.InnerText);
					}
					catch(Exception)
					{
					}
				}
			}
		}

		GUIListItem GetSelectedItem()
		{
			int iControl;

			iControl=(int)Controls.CONTROL_LIST;
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
			return item;
		}

		void UpdateDetails()
		{
			GUIListItem item = GetSelectedItem();
			if (item==null) return;

			feed_details feed = (feed_details)item.MusicTag;
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STORYTEXT, feed.m_description);
		}

	}
}
