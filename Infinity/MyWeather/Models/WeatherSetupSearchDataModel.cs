#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel;

namespace MyWeather
{

    #region WeatherSetupDataModel
    /// <summary>
    /// Summary description for Weather.
    /// </summary>
    public class WeatherSetupSearchDataModel : WeatherSetupDataModel
    {
        public event PropertyChangedEventHandler SearchPropertyChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public WeatherSetupSearchDataModel()
        {
        }

        /// <summary>
        /// searches online for available cities
        /// with the given name and lists them up
        /// </summary>
        /// <param name="searchString">city name to search for</param>
        /// <returns></returns>
        public void SearchCity(string searchString)
        {
            _locations.Clear();
            try
            {
                string searchURI = String.Format("http://xoap.weather.com/search/search?where={0}", UrlEncode(searchString));

                //
                // Create the request and fetch the response
                //
                WebRequest request = WebRequest.Create(searchURI);
                WebResponse response = request.GetResponse();

                //
                // Read data from the response stream
                //
                Stream responseStream = response.GetResponseStream();
                Encoding iso8859 = System.Text.Encoding.GetEncoding("iso-8859-1");
                StreamReader streamReader = new StreamReader(responseStream, iso8859);

                //
                // Fetch information from our stream
                //
                string data = streamReader.ReadToEnd();

                XmlDocument document = new XmlDocument();
                document.LoadXml(data);

                XmlNodeList nodes = document.DocumentElement.SelectNodes("/search/loc");

                if (nodes != null)
                {
                    //
                    // Iterate through our results
                    //
                    foreach (XmlNode node in nodes)
                    {
                        string name = node.InnerText;
                        string id = node.Attributes["id"].Value;

                        _locations.Add(new City(name, id));
                    }
                    if (SearchPropertyChanged != null)
                    {
                        SearchPropertyChanged(this, new PropertyChangedEventArgs("Locations"));
                    }
                }
            }
            catch (Exception)
            {
                //
                // Failed to perform search
                //
                throw new ApplicationException("Failed to perform city search, make sure you are connected to the internet.");
            }

        }

        public string UrlEncode(string instring)
        {
            StringReader strRdr = new StringReader(instring);
            StringWriter strWtr = new StringWriter();
            int charValue = strRdr.Read();
            while (charValue != -1)
            {
                if (((charValue >= 48) && (charValue <= 57)) // 0-9
                  || ((charValue >= 65) && (charValue <= 90)) // A-Z
                  || ((charValue >= 97) && (charValue <= 122))) // a-z
                {
                    strWtr.Write((char)charValue);
                }
                else if (charValue == 32)  // Space
                {
                    strWtr.Write("+");
                }
                else
                {
                    strWtr.Write("%{0:x2}", charValue);
                }

                charValue = strRdr.Read();
            }

            return strWtr.ToString();
        }
    }
    #endregion
}
