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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MyWeather
{
    public class Helper
    {
        /// <summary>
        /// dll Import to check Internet Connection
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="ReservedValue"></param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// check if we have an Internetconnection
        /// </summary>
        /// <param name="code"></param>
        /// <returns>true if Internetconnection is available</returns>
        public static bool IsConnectedToInternet(ref int code)
        {
            return InternetGetConnectedState(out code, 0);
        }

        /// <summary>
        /// this will take the CityProviderInfo Object and turn it to a
        /// City object by adding empty data
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static City CityInfoToCityObject(CitySetupInfo info)
        {
            return new City(info);
        }

        /// <summary>
        /// this will create a new list of cities, that
        /// already hold the providerinformation
        /// </summary>
        /// <param name="cpiList"></param>
        /// <returns></returns>
        public static List<City> CityInfoListToCityObjectList(List<CitySetupInfo> cpiList)
        {
            List<City> cList = new List<City>();
            foreach (CitySetupInfo cpi in cpiList)
            {
                cList.Add(new City(cpi));
            }
            return cList;
        }

        /// <summary>
        /// encode a given string to an URL
        /// </summary>
        /// <param name="instring"></param>
        /// <returns></returns>
        public static string UrlEncode(string instring)
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
}
