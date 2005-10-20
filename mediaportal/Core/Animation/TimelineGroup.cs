#region Copyright (C) 2005 Media Portal

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

#endregion

using System;

namespace MediaPortal.Animation
{
	public abstract class TimelineGroup : Timeline
	{
		#region Constructors

		protected TimelineGroup()
		{
		}

		protected TimelineGroup(TimeSpan beginTime) : base(beginTime)
		{
		}

		protected TimelineGroup(TimeSpan beginTime, Duration duration) : base(beginTime, duration)
		{
		}

		protected internal TimelineGroup(TimelineGroup timeline, CloneType cloneType) : base(timeline, cloneType)
		{
		}

		protected TimelineGroup(TimeSpan beginTime, Duration duration, RepeatBehavior repeatBehavior) : base(beginTime, duration, repeatBehavior)
		{
		}

		#endregion Constructors

		#region Methods

		public new ClockGroup CreateClock()
		{
			return (ClockGroup)base.CreateClock();
		}

		protected internal override Clock AllocateClock()
		{
			return new ClockGroup(this);
		}

		public new TimelineGroup Copy()
		{
			return (TimelineGroup)base.Copy();
		}

//		protected override sealed Freezable CopyCore();
//		protected abstract Animatable CopyCore(CloneType cloneType);
				
		#endregion Methods
	}
}
