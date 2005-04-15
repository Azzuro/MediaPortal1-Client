using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.GUI.Library;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Weather
{
	public class GUIWindowWeather : GUIWindow, ISetupForm
	{
		class Location
		{
			public string		m_strCity;
			public string		m_strCode;
			public string		m_strURLSattelite;
			public string		m_strURLTemperature;
			public string		m_strURLUVIndex;
			public string		m_strURLWinds;
			public string		m_strURLHumid;
			public string		m_strURLPrecip;
		}

		struct day_forcast
		{
			public string		m_szIconLow;
			public string		m_szIconHigh;
			public string		m_szOverview;
			public string		m_szDay;
			public string		m_szHigh;
			public string		m_szLow;
			public string		m_szSunRise;
			public string		m_szSunSet;
			public string		m_szPrecipitation;
			public string		m_szHumidity;
			public string		m_szWind;
		};

		enum Controls
		{
			  CONTROL_BTNSWITCH			= 2
			, CONTROL_BTNREFRESH		= 3
			, CONTROL_BTNVIEW			= 4
			, CONTROL_LOCATIONSELECT	= 5
			, CONTROL_LABELLOCATION		= 10
			, CONTROL_LABELUPDATED		= 11
			, CONTROL_IMAGELOGO			= 101
			, CONTROL_IMAGENOWICON		= 21
    		, CONTROL_LABELNOWCOND		= 22
			, CONTROL_LABELNOWTEMP		= 23
			, CONTROL_LABELNOWFEEL		= 24
			, CONTROL_LABELNOWUVID		= 25
			, CONTROL_LABELNOWWIND		= 26
			, CONTROL_LABELNOWDEWP		= 27
			, CONTORL_LABELNOWHUMI		= 28
			, CONTROL_STATICTEMP		= 223
			, CONTROL_STATICFEEL		= 224
			, CONTROL_STATICUVID		= 225
			, CONTROL_STATICWIND		= 226
			, CONTROL_STATICDEWP		= 227
			, CONTROL_STATICHUMI		= 228
			, CONTROL_LABELD0DAY		= 31
			, CONTROL_LABELD0HI			= 32
			, CONTROL_LABELD0LOW		= 33
			, CONTROL_LABELD0GEN		= 34
			, CONTROL_IMAGED0IMG		= 35
			, CONTROL_LABELSUNR			= 70
			, CONTROL_STATICSUNR		= 71
			, CONTROL_LABELSUNS			= 72
			, CONTROL_STATICSUNS		= 73
			, CONTROL_IMAGE_SAT			= 1000
			, CONTROL_IMAGE_SAT_END		= 1100
		}

		const int    NUM_DAYS			= 4;
		const char	 DEGREE_CHARACTER	= (char)176;				//the degree 'o' character
		const string PARTNER_ID			= "1004124588";			//weather.com partner id
		const string PARTNER_KEY		= "079f24145f208494";		//weather.com partner key

		string			m_strLocation	= "UKXX0085";
		ArrayList		m_locations		= new ArrayList();
		string			m_strWeatherFTemp = "C";
		string			m_strWeatherFSpeed = "K";
		int             m_iWeatherRefresh = 30;
		string			m_szLocation	= "";
		string			m_szUpdated		= "";
		string			m_szNowIcon		= @"weather\128x128\na.png";
		string			m_szNowCond		= "";
		string			m_szNowTemp		= "";
		string			m_szNowFeel		= "";
		string			m_szNowUVId		= "";
		string			m_szNowWind		= "";
		string			m_szNowDewp		= "";
		string			m_szNowHumd		= "";
		string			m_szForcastUpdated = "";

		day_forcast[]	m_dfForcast		= new 	day_forcast[NUM_DAYS];
		GUIImage		m_pNowImage		= null;
		string          m_strSatelliteURL = "";
		string          m_strTemperatureURL = "";
		string          m_strUVIndexURL	= "";
		string          m_strWindsURL	= "";
		string          m_strHumidityURL= "";
		string          m_strPrecipitationURL = "";
		string			m_strViewImageURL= "";
		DateTime        m_lRefreshTime	= DateTime.Now.AddHours(-1);		//for autorefresh
		int				m_iDayNum	= -2;
		string			m_strSelectedDayName = "All";

		enum Mode
		{
			Weather,
			Satellite
		}
		Mode           m_Mode=Mode.Weather;

		enum ImageView
		{
			Satellite,
			Temperature,
			UVIndex,
			Winds,
			Humidity,
			Precipitation
		}
		ImageView		m_ImageView=ImageView.Satellite; 

		public  GUIWindowWeather()
		{	
			//loop here as well
			for(int i=0; i<NUM_DAYS; i++)
			{
				m_dfForcast[i].m_szIconLow= @"weather\64x64\na.png";
				m_dfForcast[i].m_szIconHigh= @"weather\128x128\na.png";
				m_dfForcast[i].m_szOverview= "";
				m_dfForcast[i].m_szDay= "";
				m_dfForcast[i].m_szHigh= "";
				m_dfForcast[i].m_szLow= "";
			}
			GetID=(int)GUIWindow.Window.WINDOW_WEATHER;

		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myweather.xml");
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
					m_Mode=Mode.Weather;
					m_strSelectedDayName="All";
					m_iDayNum=-2;
					base.OnMessage(message);
					LoadSettings();
	
					//do image id to control stuff so we can use them later
					//do image id to control stuff so we can use them later
					m_pNowImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGENOWICON);	
					UpdateButtons();

					int i=0;
					int iSelected=0;
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT);
					foreach (Location loc in m_locations)
					{
						string strCity=loc.m_strCity;
						int pos=strCity.IndexOf(",");
						if (pos>0) strCity=strCity.Substring(0,pos);
							GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT,strCity);
						if (m_strLocation==loc.m_strCode )
						{
							m_szLocation=loc.m_strCity;
							m_strSatelliteURL=loc.m_strURLSattelite;
							m_strTemperatureURL=loc.m_strURLTemperature;
							m_strUVIndexURL=loc.m_strURLUVIndex;
							m_strWindsURL=loc.m_strURLWinds;
							m_strHumidityURL=loc.m_strURLHumid;
							m_strPrecipitationURL=loc.m_strURLPrecip;
							iSelected=i;
						}
						i++;
					}
					GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT,iSelected);

					TimeSpan ts=DateTime.Now-m_lRefreshTime;
					if( ts.TotalMinutes >= m_iWeatherRefresh && m_strLocation!="" )
					{
						RefreshMe(false);	//do an autoUpdate refresh
					}
					return true;
				}
        
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					SaveSettings();
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
					if (iControl == (int)Controls.CONTROL_BTNREFRESH)
					{
						m_iDayNum = -2;
						m_strSelectedDayName="All";
						RefreshMe(false);	//refresh clicked so do a complete update (not an autoUpdate)
					}
					if (iControl == (int)Controls.CONTROL_BTNVIEW)
					{
						if (m_Mode == Mode.Satellite)
						{
							switch (m_ImageView)
							{
								case ImageView.Satellite:
								{
									m_ImageView = ImageView.Temperature;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
								case ImageView.Temperature:
								{
									m_ImageView = ImageView.UVIndex;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
								case ImageView.UVIndex:
								{
									m_ImageView = ImageView.Winds;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
								case ImageView.Winds:
								{
									m_ImageView = ImageView.Humidity;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
								case ImageView.Humidity:
								{
									m_ImageView = ImageView.Precipitation;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
								case ImageView.Precipitation:
								{
									m_ImageView = ImageView.Satellite;
									UpdateButtons();
									RefreshMe(true);
									break;
								}
							}
						}
						else
						{
							switch (m_iDayNum)
							{
								case -2:
								{
									m_strSelectedDayName = m_dfForcast[0].m_szDay;
									m_iDayNum =0;
									UpdateButtons();
									m_iDayNum =1;
									break;
								}
								case -1:
								{
									m_strSelectedDayName = "All";
									UpdateButtons();
									m_iDayNum =0;
									break;
								}
								case 0:
								{
									m_strSelectedDayName = m_dfForcast[0].m_szDay;
									UpdateButtons();
									m_iDayNum =1;
									break;
								}
								case 1:
								{
									m_strSelectedDayName = m_dfForcast[1].m_szDay;
									UpdateButtons();
									m_iDayNum =2;
									break;
								}
								case 2:
								{
									m_strSelectedDayName = m_dfForcast[2].m_szDay;
									UpdateButtons();
									m_iDayNum =3;
									break;
								}
								case 3:
								{
									m_strSelectedDayName = m_dfForcast[3].m_szDay;
									UpdateButtons();
									m_iDayNum =-1;
									break;
								}
							}
						}
					}
					if (iControl == (int)Controls.CONTROL_LOCATIONSELECT)
					{
						foreach(Location loc in m_locations)
						{
							if (loc.m_strCity.StartsWith(message.Label))
							{
								m_strLocation=loc.m_strCode;
								m_szLocation=loc.m_strCity;
								m_strSatelliteURL=loc.m_strURLSattelite;
								m_strTemperatureURL=loc.m_strURLTemperature;
								m_strUVIndexURL=loc.m_strURLUVIndex;
								m_strWindsURL=loc.m_strURLWinds;
								m_strHumidityURL=loc.m_strURLHumid;
								m_strPrecipitationURL=loc.m_strURLPrecip;
								GUIImage img=GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
								if (img!=null)
								{
									//img.Filtering=true;
									//img.Centered=true;
									//img.KeepAspectRatio=true;
									switch (m_ImageView)
									{
										case ImageView.Satellite:
										{
											m_strViewImageURL=m_strSatelliteURL;
											break;
										}
										case ImageView.Temperature:
										{
											m_strViewImageURL=m_strTemperatureURL;
											break;
										}
										case ImageView.UVIndex:
										{
											m_strViewImageURL=m_strUVIndexURL;
											break;
										}
										case ImageView.Winds:
										{
											m_strViewImageURL=m_strWindsURL;
											break;
										}
										case ImageView.Humidity:
										{
											m_strViewImageURL=m_strHumidityURL;
											break;
										}
										case ImageView.Precipitation:
										{
											m_strViewImageURL=m_strPrecipitationURL;
											break;
										}
									}
									img.SetFileName(m_strViewImageURL);
									//reallocate & load then new image
									img.FreeResources();
									img.AllocResources();
								}
								break;
							}
						}
						m_iDayNum = -2;
						m_strSelectedDayName="All";
						RefreshMe(false);	//refresh clicked so do a complete update (not an autoUpdate)
					}
					if (iControl==(int)Controls.CONTROL_BTNSWITCH)
					{
						if (m_Mode==Mode.Weather) m_Mode=Mode.Satellite;
						else m_Mode=Mode.Weather;
						GUIImage img=GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
						if (img!=null)
						{
							//img.Filtering=true;
							//img.Centered=true;
							//img.KeepAspectRatio=true;
							switch (m_ImageView)
							{
								case ImageView.Satellite:
								{
									m_strViewImageURL=m_strSatelliteURL;
									break;
								}
								case ImageView.Temperature:
								{
									m_strViewImageURL=m_strTemperatureURL;
									break;
								}
								case ImageView.UVIndex:
								{
									m_strViewImageURL=m_strUVIndexURL;
									break;
								}
								case ImageView.Winds:
								{
									m_strViewImageURL=m_strWindsURL;
									break;
								}
								case ImageView.Humidity:
								{
									m_strViewImageURL=m_strHumidityURL;
									break;
								}
								case ImageView.Precipitation:
								{
									m_strViewImageURL=m_strPrecipitationURL;
									break;
								}
							}
							img.SetFileName(m_strViewImageURL);
							//reallocate & load then new image
							img.FreeResources();
							img.AllocResources();
						}
						if (m_Mode==Mode.Weather)
						{
							m_iDayNum = -2;
							m_strSelectedDayName="All";
						}
						UpdateButtons();
					}
				}
					break;
			}
			return base.OnMessage(message);
		}

    
		#region Serialisation
		void LoadSettings()
		{
			m_locations.Clear();
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_strLocation=xmlreader.GetValueAsString("weather","location","");
				m_strWeatherFTemp=xmlreader.GetValueAsString("weather","temperature","C");
				m_strWeatherFSpeed=xmlreader.GetValueAsString("weather","speed","K");
				m_iWeatherRefresh=xmlreader.GetValueAsInt("weather","refresh",30);

				bool bFound=false;
				for (int i=0; i < 20; i++)
				{
					string strCityTag=String.Format("city{0}",i);
					string strCodeTag=String.Format("code{0}",i);
					string strSatUrlTag=String.Format("sat{0}",i);
					string strTempUrlTag=String.Format("temp{0}",i);
					string strUVUrlTag=String.Format("uv{0}",i);
					string strWindsUrlTag=String.Format("winds{0}",i);
					string strHumidUrlTag=String.Format("humid{0}",i);
					string strPrecipUrlTag=String.Format("precip{0}",i);
					string strCity=xmlreader.GetValueAsString("weather",strCityTag,"");
					string strCode=xmlreader.GetValueAsString("weather",strCodeTag,"");
					string strSatURL=xmlreader.GetValueAsString("weather",strSatUrlTag,"");
					string strTempURL=xmlreader.GetValueAsString("weather",strTempUrlTag,"");
					string strUVURL=xmlreader.GetValueAsString("weather",strUVUrlTag,"");
					string strWindsURL=xmlreader.GetValueAsString("weather",strWindsUrlTag,"");
					string strHumidURL=xmlreader.GetValueAsString("weather",strHumidUrlTag,"");
					string strPrecipURL=xmlreader.GetValueAsString("weather",strPrecipUrlTag,"");
					if (strCity.Length>0 && strCode.Length>0)
					{
						if (strSatURL.Length==0)
							strSatURL="http://www.zdf.de/ZDFde/wetter/showpicture/0,2236,161,00.gif";
						Location loc= new Location();
						loc.m_strCity=strCity;
						loc.m_strCode=strCode;
						loc.m_strURLSattelite=strSatURL;
						loc.m_strURLTemperature=strTempURL;
						loc.m_strURLUVIndex=strUVURL;
						loc.m_strURLWinds=strWindsURL;
						loc.m_strURLHumid=strHumidURL;
						loc.m_strURLPrecip=strPrecipURL;
						m_locations.Add(loc);
						if (String.Compare(m_strLocation,strCode,true)==0)
						{
							bFound=true;
						}
					}
				}
				if (!bFound)
				{
					if (m_locations.Count>0)
					{
						m_strLocation=((Location)m_locations[0]).m_strCode;
					}
				}
			}
		}

		void SaveSettings()
		{
			using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("weather","location",m_strLocation);
				xmlwriter.SetValue("weather","temperature",m_strWeatherFTemp);      
				xmlwriter.SetValue("weather","speed",m_strWeatherFSpeed);
			}
			
		}
		#endregion

		int ConvertSpeed(int curSpeed)
		{
			//we might not need to convert at all
			if((m_strWeatherFTemp[0] == 'C' && m_strWeatherFSpeed[0] == 'K') ||
				(m_strWeatherFTemp[0] == 'F' && m_strWeatherFSpeed[0] == 'M'))
				return curSpeed;

			//got through that so if temp is C, speed must be M or S
			if(m_strWeatherFTemp[0] == 'C')
			{
				if (m_strWeatherFSpeed[0] == 'S')
					return (int)(curSpeed * (1000.0/3600.0) + 0.5);		//mps
				else
					return (int)(curSpeed / (8.0/5.0));		//mph
			}
			else
			{
				if (m_strWeatherFSpeed[0] == 'S')
					return (int)(curSpeed * (8.0/5.0) * (1000.0/3600.0) + 0.5);		//mps
				else
					return (int)(curSpeed * (8.0/5.0));		//kph
			}
		}

		void UpdateButtons()
		{
			if (m_Mode==Mode.Weather) 
			{
				for (int i=10; i < 900;++i)
					GUIControl.ShowControl(GetID,i);

				for (int i= (int)Controls.CONTROL_IMAGE_SAT; i < (int)Controls.CONTROL_IMAGE_SAT_END;++i)
					GUIControl.HideControl(GetID, i);

				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(750));			
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNREFRESH, GUILocalizeStrings.Get(184));			//Refresh
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELLOCATION, m_szLocation);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELUPDATED, m_szUpdated);

				//urgh, remove, create then add image each refresh to update nicely
				//Remove(m_pNowImage.GetID);
				int posX = m_pNowImage.XPosition;
				int posY = m_pNowImage.YPosition;
				//m_pNowImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGENOWICON, posX, posY, 128, 128, m_szNowIcon, 0);
				//Add(ref cntl);
				m_pNowImage.SetPosition(posX,posY);
				m_pNowImage.ColourDiffuse=0xffffffff;
				m_pNowImage.SetFileName(m_szNowIcon);

				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWCOND, m_szNowCond);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWTEMP, m_szNowTemp);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWFEEL, m_szNowFeel);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWUVID, m_szNowUVId);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWWIND, m_szNowWind);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWDEWP, m_szNowDewp);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTORL_LABELNOWHUMI, m_szNowHumd);

				//static labels
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(401));		//Temperature
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(402));		//Feels Like
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICUVID, GUILocalizeStrings.Get(403));		//UV Index
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICWIND, GUILocalizeStrings.Get(404));		//Wind
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(405));		//Dew Point
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICHUMI, GUILocalizeStrings.Get(406));		//Humidity

				if (m_iDayNum==-1 || m_iDayNum==-2)
				{
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_STATICSUNR);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_STATICSUNS);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELSUNR);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELSUNS);
					for(int i=0; i<NUM_DAYS; i++)
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0DAY+(i*10), m_dfForcast[i].m_szDay);
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0HI+(i*10), m_dfForcast[i].m_szHigh);
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0LOW+(i*10), m_dfForcast[i].m_szLow);
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0GEN+(i*10), m_dfForcast[i].m_szOverview);

						//Seems a bit messy, but works. Remove, Create and then Add the image to update nicely
						//Remove(m_dfForcast[i].m_pImage.GetID);
						GUIImage image=(GUIImage )GetControl((int)Controls.CONTROL_IMAGED0IMG+(i*10));
						image.ColourDiffuse=0xffffffff;
						image.SetFileName(m_dfForcast[i].m_szIconLow);
						//				m_dfForcast[i].m_pImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGED0IMG+(i*10), posX, posY, 64, 64, m_dfForcast[i].m_szIconLow, 0);
						//			cntl=(GUIControl)m_dfForcast[i].m_pImage;
						//		Add(ref cntl);
					}
				}
				else
				{
					for(int i=0; i<NUM_DAYS; i++)
					{
						GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELD0DAY+(i*10));
						GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELD0HI+(i*10));
						GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELD0LOW+(i*10));
						GUIControl.HideControl(GetID,(int)Controls.CONTROL_LABELD0GEN+(i*10));
						GUIControl.HideControl(GetID,(int)Controls.CONTROL_IMAGED0IMG+(i*10));
					}
					int iCurrentDayNum = m_iDayNum;

					GUIControl.HideControl(GetID, (int)Controls.CONTROL_STATICUVID);
					GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELNOWUVID);

					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICSUNR, GUILocalizeStrings.Get(744));
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICSUNS, GUILocalizeStrings.Get(745));
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(746));		//High
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(747));		//Low
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(748));

					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELSUNR, m_dfForcast[m_iDayNum].m_szSunRise);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELSUNS, m_dfForcast[m_iDayNum].m_szSunSet);	
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTORL_LABELNOWHUMI, m_dfForcast[m_iDayNum].m_szHumidity);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWTEMP, m_dfForcast[m_iDayNum].m_szHigh);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWFEEL, m_dfForcast[m_iDayNum].m_szLow);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWCOND, m_dfForcast[m_iDayNum].m_szOverview);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWDEWP, m_dfForcast[m_iDayNum].m_szPrecipitation);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWWIND, m_dfForcast[m_iDayNum].m_szWind);
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELUPDATED, m_szForcastUpdated);
					//					m_pNowImage.SetFileName(m_dfForcast[iCurrentDayNum].m_szIconLow);
					m_pNowImage.SetFileName(m_dfForcast[iCurrentDayNum].m_szIconHigh);
				}
			}
			else
			{
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(717));			
		
				for (int i=10; i < 900;++i)
					GUIControl.HideControl(GetID,i);

				for (int i= (int)Controls.CONTROL_IMAGE_SAT; i < (int)Controls.CONTROL_IMAGE_SAT_END;++i)
					GUIControl.ShowControl(GetID, i);

			}
			if (m_Mode==Mode.Satellite)
			{
				switch (m_ImageView)
				{
					case ImageView.Satellite:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(737));
						break;
					}
					case ImageView.Temperature:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(738));
						break;
					}
					case ImageView.UVIndex:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(739));
						break;
					}
					case ImageView.Winds:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(740));
						break;
					}
					case ImageView.Humidity:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(741));
						break;
					}
					case ImageView.Precipitation:
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(742));
						break;
					}
				}
			}
			else
			{
				switch (m_strSelectedDayName)
				{
					case "All":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(743));
						break;
					}
					case "Monday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(11));
						break;
					}
					case "Tuesday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(12));
						break;
					}
					case "Wednesday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(13));
						break;
					}
					case "Thursday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(14));
						break;
					}
					case "Friday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(15));
						break;
					}
					case "Saturday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(16));
						break;
					}
					case "Sunday":
					{
						GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(17));
						break;
					}
				}
			}
		}
    		
		public override void Render(float timePassed)
		{
      
			base.Render(timePassed);
		}


		bool Download(string strWeatherFile)
		{
			string			strURL;

			if (!Util.Win32API.IsConnectedToInternet())
			{
				if (System.IO.File.Exists(strWeatherFile)) return true;
				return false;
			}

			char c_units = m_strWeatherFTemp[0];	//convert from temp units to metric/standard
			if(c_units == 'F')	//we'll convert the speed later depending on what thats set to
				c_units = 's';
			else
				c_units = 'm';

			strURL=String.Format("http://xoap.weather.com/weather/local/{0}?cc=*&unit={1}&dayf=4&prod=xoap&par={2}&key={3}",
				m_strLocation, c_units.ToString(), PARTNER_ID, PARTNER_KEY);
			
			using (WebClient client = new WebClient())
			{
				try
				{
					client.DownloadFile(strURL, strWeatherFile);
					return true;
				} 
				catch(Exception ex)
				{
					Log.Write("Failed to download weather:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
				}
			}
			return false;
		}

		//convert weather.com day strings into localized string id's
		string LocalizeDay(string szDay)
		{
			string strLocDay="";

			if(szDay== "Monday" )			//monday is localized string 11
				strLocDay = GUILocalizeStrings.Get(11);
			else if(szDay== "Tuesday" )
				strLocDay = GUILocalizeStrings.Get(12);
			else if(szDay== "Wednesday" )
				strLocDay = GUILocalizeStrings.Get(13);
			else if(szDay== "Thursday" )
				strLocDay = GUILocalizeStrings.Get(14);
			else if(szDay== "Friday" )
				strLocDay = GUILocalizeStrings.Get(15);
			else if(szDay== "Saturday" )
				strLocDay = GUILocalizeStrings.Get(16);
			else if(szDay== "Sunday" )
				strLocDay = GUILocalizeStrings.Get(17);
			else
				strLocDay = "";

			return strLocDay ;
		}

		
		string LocalizeOverview(string szToken)
		{
			string strLocStr="";

			foreach (string szTokenSplit in szToken.Split(' '))
			{
				string strLocWord="";
			
				if(String.Compare(szTokenSplit, "T-Storms",true) == 0)
					strLocWord = GUILocalizeStrings.Get(370);
				else if(String.Compare(szTokenSplit, "Partly",true) == 0)
					strLocWord = GUILocalizeStrings.Get(371);
				else if(String.Compare(szTokenSplit, "Mostly",true) == 0)
					strLocWord = GUILocalizeStrings.Get(372);
				else if(String.Compare(szTokenSplit, "Sunny",true) == 0)
					strLocWord = GUILocalizeStrings.Get(373);
				else if(String.Compare(szTokenSplit, "Cloudy",true) == 0)
					strLocWord = GUILocalizeStrings.Get(374);
				else if(String.Compare(szTokenSplit, "Snow",true) == 0)
					strLocWord = GUILocalizeStrings.Get(375);
				else if(String.Compare(szTokenSplit, "Rain",true) == 0)
					strLocWord = GUILocalizeStrings.Get(376);
				else if(String.Compare(szTokenSplit, "Light",true) == 0)
					strLocWord = GUILocalizeStrings.Get(377);
				else if(String.Compare(szTokenSplit, "AM",true) == 0)
					strLocWord = GUILocalizeStrings.Get(378);
				else if(String.Compare(szTokenSplit, "PM",true) == 0)
					strLocWord = GUILocalizeStrings.Get(379);
				else if(String.Compare(szTokenSplit, "Showers",true) == 0)
					strLocWord = GUILocalizeStrings.Get(380);
				else if(String.Compare(szTokenSplit, "Few",true) == 0)
					strLocWord = GUILocalizeStrings.Get(381);
				else if(String.Compare(szTokenSplit, "Scattered",true) == 0)
					strLocWord = GUILocalizeStrings.Get(382);
				else if(String.Compare(szTokenSplit, "Wind",true) == 0)
					strLocWord = GUILocalizeStrings.Get(383);
				else if(String.Compare(szTokenSplit, "Strong",true) == 0)
					strLocWord = GUILocalizeStrings.Get(384);
				else if(String.Compare(szTokenSplit, "Fair",true) == 0)
					strLocWord = GUILocalizeStrings.Get(385);
				else if(String.Compare(szTokenSplit, "Clear",true) == 0)
					strLocWord = GUILocalizeStrings.Get(386);
				else if(String.Compare(szTokenSplit, "Early",true) == 0)
					strLocWord = GUILocalizeStrings.Get(387);
				else if(String.Compare(szTokenSplit, "and",true) == 0)
					strLocWord = GUILocalizeStrings.Get(388);
				else if(String.Compare(szTokenSplit, "Fog",true) == 0)
					strLocWord = GUILocalizeStrings.Get(389);
				else if(String.Compare(szTokenSplit, "Haze",true) == 0)
					strLocWord = GUILocalizeStrings.Get(390);
				else if(String.Compare(szTokenSplit, "Windy",true) == 0)
					strLocWord = GUILocalizeStrings.Get(391);
				else if(String.Compare(szTokenSplit, "Drizzle",true) == 0)
					strLocWord = GUILocalizeStrings.Get(392);
				else if(String.Compare(szTokenSplit, "Freezing",true) == 0)
					strLocWord = GUILocalizeStrings.Get(393);
				else if(String.Compare(szTokenSplit, "N/A",true) == 0)
					strLocWord = GUILocalizeStrings.Get(394);

				if(strLocWord == "")
					strLocWord = szTokenSplit;	//if not found, let fallback
					
				strLocStr = strLocStr + strLocWord;
				strLocStr += " ";
			}

			return strLocStr;

		}

		//splitStart + End are the chars to search between for a space to replace with a \n
		void SplitLongString(ref string szString, int splitStart, int splitEnd)
		{
			//search chars 10 to 15 for a space
			//if we find one, replace it with a newline
			for(int i=splitStart; i<splitEnd && i<(int)szString.Length; i++)
			{
				if(szString[i] == ' ')
				{
					szString=szString.Substring(0,i)+"\n"+szString.Substring(i+1);
					return;
				}
			}
		}

		//Do a complete download, parse and update
		void RefreshMe(bool autoUpdate)
		{
			//message strings for refresh of images
			string strWeatherFile = @"weather\curWeather.xml";

			GUIDialogProgress	pDlgProgress	= (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			GUIDialogOK 			pDlgOK				= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			bool dlRes = false, ldRes = false;

			//progress dialog for download
			if(pDlgProgress!=null && !autoUpdate) //dont display progress dialog on autoupdate or it crashes! :|
			{
				pDlgProgress.SetHeading(410);						//"Accessing Weather.com"
				pDlgProgress.SetLine(1, 411);						//"Getting Weather For:"
				pDlgProgress.SetLine(2, m_strLocation);	//Area code
				if(m_szLocation.Length > 1)							//got the location string yet?
					pDlgProgress.SetLine(2, m_szLocation);
				//else
				//	pDlgProgress.SetLine(3, "");
				pDlgProgress.StartModal(GetID);
				pDlgProgress.Progress();
			}	

			//Do The Download
			dlRes = Download(strWeatherFile);		

			if(null!=pDlgProgress && !autoUpdate)	//close progress dialog
				pDlgProgress.Close();

			if(dlRes)	//dont load if download failed
				ldRes = LoadWeather(strWeatherFile);	//parse

			//if the download or load failed, display an error message
			if((!dlRes || !ldRes) && null!=pDlgOK && !autoUpdate) //this will probably crash on an autoupdate as well, but not tested
			{
				// show failed dialog...
				pDlgOK.SetHeading(412);	//"Unable to get weather data"
				pDlgOK.SetLine(1, m_szLocation);
				pDlgOK.SetLine(2, "");
				pDlgOK.SetLine(3, "");
				pDlgOK.DoModal(GetID);
			} 
			else if(dlRes && ldRes)	//download and load went ok so update
			{
				UpdateButtons();

				//Send the refresh messages
				//				OnMessage(msgDe);
				//			OnMessage(msgRe);
			}

			// Update sattelite image
			GUIImage img=GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
			if (img!=null)
			{
				switch (m_ImageView)
				{
					case ImageView.Satellite:
					{
						m_strViewImageURL=m_strSatelliteURL;
						break;
					}
					case ImageView.Temperature:
					{
						m_strViewImageURL=m_strTemperatureURL;
						break;
					}
					case ImageView.UVIndex:
					{
						m_strViewImageURL=m_strUVIndexURL;
						break;
					}
					case ImageView.Winds:
					{
						m_strViewImageURL=m_strWindsURL;
						break;
					}
					case ImageView.Humidity:
					{
						m_strViewImageURL=m_strHumidityURL;
						break;
					}
					case ImageView.Precipitation:
					{
						m_strViewImageURL=m_strPrecipitationURL;
						break;
					}
				}
				img.SetFileName(m_strViewImageURL);
				//reallocate & load then new image
				img.FreeResources();
				img.AllocResources();
			}
			m_lRefreshTime = DateTime.Now;
			m_iDayNum = -2;
		}


		bool LoadWeather(string strWeatherFile)
		{
			int			iTmpInt=0;
			string	iTmpStr="";
			string	szUnitTemp="";
			string	szUnitSpeed="";
			DateTime time=DateTime.Now;
			
			// load the xml file
			XmlDocument doc= new XmlDocument();
			doc.Load(strWeatherFile);
			if (doc.DocumentElement==null) return false;
			string strRoot=doc.DocumentElement.Name;
			XmlNode pRootElement=doc.DocumentElement;
			if (strRoot=="error")
			{
				string szCheckError;
				GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);

				GetString(pRootElement, "err", out szCheckError, "Unknown Error");	//grab the error string

				// show error dialog...
				pDlgOK.SetHeading(412);	//"Unable to get weather data"
				pDlgOK.SetLine(1, szCheckError);
				pDlgOK.SetLine(2, m_szLocation);
				pDlgOK.SetLine(3, "");
				pDlgOK.DoModal(GetID);
				return true;	//we got a message so do display a second in refreshme()
			}

			// units (C or F and mph or km/h or m/s) 
			szUnitTemp= m_strWeatherFTemp;

			if(m_strWeatherFSpeed[0] == 'M')
				szUnitSpeed= "mph";
			else if(m_strWeatherFSpeed[0] == 'K')
				szUnitSpeed= "km/h";
			else
				szUnitSpeed= "m/s";

			// location
			XmlNode pElement = pRootElement.SelectSingleNode("loc");
			if(null!=pElement)
			{
				GetString(pElement, "dnam",out m_szLocation, "");
			}

			//current weather
			pElement = pRootElement.SelectSingleNode("cc");
			if(null!=pElement)
			{
				GetString(pElement, "lsup", out m_szUpdated, "");

				GetInteger(pElement, "icon",out iTmpInt);
				m_szNowIcon=String.Format(@"weather\128x128\{0}.png", iTmpInt);

				GetString(pElement, "t",out m_szNowCond, "");			//current condition
				m_szNowCond=LocalizeOverview(m_szNowCond);
				SplitLongString(ref m_szNowCond, 8, 15);				//split to 2 lines if needed

				GetInteger(pElement, "tmp",out iTmpInt);				//current temp
				m_szNowTemp=String.Format( "{0}{1}{2}", iTmpInt, DEGREE_CHARACTER, szUnitTemp);	
				GetInteger(pElement, "flik",out iTmpInt);				//current 'Feels Like'
				m_szNowFeel=String.Format( "{0}{1}{2}", iTmpInt, DEGREE_CHARACTER, szUnitTemp);
				
				XmlNode pNestElement = pElement.SelectSingleNode("wind");	//current wind
				if(null!=pNestElement)
				{
					GetInteger(pNestElement, "s",out iTmpInt);			//current wind strength
					iTmpInt = ConvertSpeed(iTmpInt);				//convert speed if needed
					GetString(pNestElement, "t",out  iTmpStr, "N");		//current wind direction

					//From <dir eg NW> at <speed> km/h		 GUILocalizeStrings.Get(407)
					//This is a bit untidy, but i'm fed up with localization and string formats :)
					string szWindFrom = GUILocalizeStrings.Get(407);
					string szWindAt = GUILocalizeStrings.Get(408);

					m_szNowWind=String.Format("{0} {1} {2} {3} {4}", 
						szWindFrom, iTmpStr, 
						szWindAt, iTmpInt, szUnitSpeed);
				}

				GetInteger(pElement, "hmid",out iTmpInt);				//current humidity
				m_szNowHumd=String.Format( "{0}%", iTmpInt);

				pNestElement = pElement.SelectSingleNode("uv");	//current UV index
				if(null!=pNestElement)
				{
					GetInteger(pNestElement, "i",out iTmpInt);	
					GetString(pNestElement, "t",out  iTmpStr, "");
					m_szNowUVId=String.Format( "{0} {1}", iTmpInt, iTmpStr);
				}

				GetInteger(pElement, "dewp",out iTmpInt);				//current dew point
				m_szNowDewp=String.Format( "{0}{1}{2}", iTmpInt, DEGREE_CHARACTER, szUnitTemp);

			}
			//future forcast
			pElement = pRootElement.SelectSingleNode("dayf");
			GetString(pElement, "lsup", out m_szForcastUpdated, "");
			if(null!=pElement)
			{
				XmlNode pOneDayElement = pElement.SelectSingleNode("day");;
				for(int i=0; i<NUM_DAYS; i++)
				{
					if(null!=pOneDayElement)
					{
						m_dfForcast[i].m_szDay= pOneDayElement.Attributes.GetNamedItem("t").InnerText;
						m_dfForcast[i].m_szDay=LocalizeDay(m_dfForcast[i].m_szDay);

						GetString(pOneDayElement, "hi",out  iTmpStr, "");	//string cause i've seen it return N/A
						if(iTmpStr== "N/A")
							m_dfForcast[i].m_szHigh= "";
						else
							m_dfForcast[i].m_szHigh=String.Format( "{0}{1}{2}", iTmpStr, DEGREE_CHARACTER, szUnitTemp);	

						GetString(pOneDayElement, "low",out  iTmpStr, "");
						if(iTmpStr== "N/A")
							m_dfForcast[i].m_szHigh= "";
						else
							m_dfForcast[i].m_szLow=String.Format( "{0}{1}{2}", iTmpStr, DEGREE_CHARACTER, szUnitTemp);

						GetString(pOneDayElement, "sunr", out  iTmpStr, "");
						if(iTmpStr== "N/A")
							m_dfForcast[i].m_szSunRise= "";
						else
							m_dfForcast[i].m_szSunRise=String.Format( "{0}", iTmpStr);
						
						GetString(pOneDayElement, "suns", out  iTmpStr, "");
						if(iTmpStr== "N/A")
							m_dfForcast[i].m_szSunSet= "";
						else
							m_dfForcast[i].m_szSunSet=String.Format( "{0}", iTmpStr);
						XmlNode pDayTimeElement = pOneDayElement.SelectSingleNode("part");	//grab the first day/night part (should be day)
						if(i == 0 && (time.Hour < 7 || time.Hour >= 19))	//weather.com works on a 7am to 7pm basis so grab night if its late in the day
							pDayTimeElement = pDayTimeElement.NextSibling;//.NextSiblingElement("part");

						if(null!=pDayTimeElement)
						{
							GetInteger(pDayTimeElement, "icon",out iTmpInt);
							m_dfForcast[i].m_szIconLow=String.Format( "weather\\64x64\\{0}.png", iTmpInt);
							m_dfForcast[i].m_szIconHigh=String.Format( "weather\\128x128\\{0}.png", iTmpInt);
							GetString(pDayTimeElement, "t",out  m_dfForcast[i].m_szOverview, "");
							m_dfForcast[i].m_szOverview=LocalizeOverview(m_dfForcast[i].m_szOverview);
							SplitLongString(ref m_dfForcast[i].m_szOverview, 6, 15);
							GetInteger(pDayTimeElement, "hmid",out iTmpInt);
							m_dfForcast[i].m_szHumidity=String.Format( "{0}%", iTmpInt);
							GetInteger(pDayTimeElement, "ppcp",out iTmpInt);
							m_dfForcast[i].m_szPrecipitation=String.Format( "{0}%", iTmpInt);
						}
						XmlNode pWindElement = pDayTimeElement.SelectSingleNode("wind");	//current wind
						if(null!=pWindElement)
						{
							GetInteger(pWindElement, "s",out iTmpInt);			//current wind strength
							iTmpInt = ConvertSpeed(iTmpInt);				//convert speed if needed
							GetString(pWindElement, "t",out  iTmpStr, "N");		//current wind direction

							//From <dir eg NW> at <speed> km/h		 GUILocalizeStrings.Get(407)
							//This is a bit untidy, but i'm fed up with localization and string formats :)
							string szWindFrom = GUILocalizeStrings.Get(407);
							string szWindAt = GUILocalizeStrings.Get(408);

							m_dfForcast[i].m_szWind=String.Format("{0} {1} {2} {3} {4}", 
								szWindFrom, iTmpStr, 
								szWindAt, iTmpInt, szUnitSpeed);
						}
					}
					pOneDayElement = pOneDayElement.NextSibling;//Element("day");
				}
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

		public override void Process()
		{
			TimeSpan ts=DateTime.Now-m_lRefreshTime;
			if( ts.TotalMinutes >= m_iWeatherRefresh && m_strLocation!="" )
			{
				m_strSelectedDayName="All";
				m_iDayNum=-2;
				RefreshMe(true);	//refresh clicked so do a complete update (not an autoUpdate)
			}
			base.Process ();
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
		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string PluginName()
		{
			return "My Weather";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public bool HasSetup()
		{
			return false;
		}
		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(8);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Plugin to show the current weather";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIWindowWeather.ShowPlugin implementation
		}

		#endregion
	}

}

