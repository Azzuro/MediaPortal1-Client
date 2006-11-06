#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal - Author: mPod
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
using System.Threading;
using NUnit.Framework;
using MediaPortal.Utils;
using MediaPortal.Utils.Time;

namespace MediaPortal.Tests.Utils.Time
{
    [TestFixture]
    [Category("WorldTimeZone")]
    public class WorldTimeZoneTest
    {

        [Test]
        public void ToLocalTime()
        {
            WorldTimeZone tz = new WorldTimeZone("Greenwich Standard Time");

            DateTime nowDT = DateTime.Now;
            DateTime utcDT = nowDT.ToUniversalTime();
            DateTime localDT = tz.ToLocalTime(utcDT);
            Assert.IsTrue(nowDT == localDT);
        }

        [Test]
        public void ToUTCTime()
        {
            WorldTimeZone tz = new WorldTimeZone(TimeZone.CurrentTimeZone.StandardName);

            DateTime nowDT = DateTime.Now;
            DateTime utcDT = nowDT.ToUniversalTime();
            DateTime tzUTCDT = tz.ToUniversalTime(nowDT);
            Assert.IsTrue(utcDT == tzUTCDT);
        }
    }
}
