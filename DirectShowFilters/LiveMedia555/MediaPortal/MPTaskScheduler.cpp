/* 
*	Copyright (C) 2006-2009 Team MediaPortal
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

#ifndef IMN_PIM
#include "MPTaskScheduler.h"

////////// BasicTaskScheduler //////////

MPTaskScheduler* MPTaskScheduler::createNew() {
	return new MPTaskScheduler();
}

MPTaskScheduler::MPTaskScheduler()
: BasicTaskScheduler() {
}

MPTaskScheduler::~MPTaskScheduler() {
}

void MPTaskScheduler::doEventLoop(char* watchVariable) {
	// Repeatedly loop, handling readble sockets and timed events:
	//for (int i=0; i < 10;++i)
	{
		//    if (watchVariable != NULL && *watchVariable != 0) break;
		SingleStep(1000000LL); //delay time is in micro seconds
	} 
}


#endif

