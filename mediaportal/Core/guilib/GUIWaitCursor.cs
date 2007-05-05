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
using System.Drawing;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Media.Animation;

using MediaPortal.Drawing;

namespace MediaPortal.GUI.Library
{
	public sealed class GUIWaitCursor : GUIControl
	{
		#region Constructors

		private GUIWaitCursor()
		{
		}

		#endregion Constructors

		#region Methods

		public static void Dispose()
		{
			if(_animation != null)
				_animation.FreeResources();
			
			_animation = null;
		}

		public static void Hide()
		{
			Interlocked.Decrement(ref _showCount);
		}

		public static void Init()
		{
			if (_animation != null)
				return;
			_animation = new GUIAnimation();

			foreach(string filename in Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", "common.waiting.*.png"))
				_animation.Filenames.Add(Path.GetFileName(filename));

			_animation.HorizontalAlignment = HorizontalAlignment.Center;
			_animation.VerticalAlignment = VerticalAlignment.Center;
			_animation.SetPosition(GUIGraphicsContext.Width / 2, GUIGraphicsContext.Height / 2);
			_animation.AllocResources();
			_animation.Duration = new Duration(800);
			_animation.RepeatBehavior = RepeatBehavior.Forever;
		}

		public override void Render(float timePassed)
		{
		}

		public static void Render()
		{
			if(_showCount <= 0)
				return;

      GUIGraphicsContext.SetScalingResolution(0, 0, false);
			_animation.Render(GUIGraphicsContext.TimePassed);
		}

		public static void Show()
		{
			if(Interlocked.Increment(ref _showCount) == 1)
				_animation.Begin();
		}

		#endregion Methods

		#region Fields

		static GUIAnimation				_animation;
		static int						_showCount;

		#endregion Fields
	}
}
