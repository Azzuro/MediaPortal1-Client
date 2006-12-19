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
using System.Threading;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace MediaPortal.PowerScheduler
{
	/// <summary>
	/// Implements a timer which the process can be waiting on. The 
	/// timer supports waking up the system from a hibernated state.
	/// </summary>
	public unsafe sealed class WaitableTimer : WaitHandle

	{
		/// <summary>
		/// Wrap the system function <i>SetWaitableTimer</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="SetWaitableTimer")] static extern private bool SetWaitableTimer(SafeWaitHandle hTimer, Int64 *pDue, Int32 lPeriod, IntPtr rNotify, IntPtr pArgs, bool bResume);

		/// <summary>
		/// Wrap the system function <i>CreateWaitableTimer</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="CreateWaitableTimer")] static extern private SafeWaitHandle CreateWaitableTimer(IntPtr pSec, bool bManual, string szName);

		/// <summary>
		/// Wrap the system function <i>CancelWaitableTimer</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="CancelWaitableTimer")] static extern private bool CancelWaitableTimer(SafeWaitHandle hTimer);

		/// <summary>
		/// Wrap the system function <i>CloseHandle</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="CloseHandle")] static extern private bool CloseHandle(IntPtr hObject);

		/// <summary>
		/// Event handler to be used when the timer expires.
		/// </summary>
		public delegate void TimerExpiredHandler();

		/// <summary>
		/// Clients can register for the expiration of this timer.
		/// </summary>
		public event TimerExpiredHandler OnTimerExpired;

		/// <summary>
		/// This <see cref="Thread"/> will be create by <see cref="SecondsToWait"/> and
		/// runs <see cref="WaitThread"/>.
		/// </summary>
		private Thread m_Waiting = null;

		/// <summary>
		/// <see cref="DateTime.ToFileTime"/> of the time when the timer should
		/// expire.
		/// </summary>
		private long m_Interval = 0;

		/// <summary>
		/// Create the timer. The caller should call <see cref="Close"/> as soon as
		/// the timer is no longer needed.
		/// </summary>
		/// <remarks>
		/// <see cref="WaitHandle.Handle"/> will be used to store the system API
		/// handle of the newly created timer.
		/// </remarks>
		/// <exception cref="TimerException">When the timer could not be created.</exception>
		public WaitableTimer()
		{
			// Create it
            SafeWaitHandle = CreateWaitableTimer(IntPtr.Zero, false, null);

			// Test
            if (SafeWaitHandle.Equals(IntPtr.Zero)) throw new TimerException("Unable to create Waitable Timer");
		}

		/// <summary>
		/// Make sure that <see cref="Close"/> is called.
		/// </summary>
		~WaitableTimer()
		{
			// Forward
			Close();
		}

		/// <summary>
		/// Stop <see cref="m_Waiting"/> if necessary. To do so <see cref="Thread.Abort"/>
		/// is used.
		/// <seealso cref="SecondsToWait"/>
		/// <seealso cref="Close"/>
		/// </summary>
		private void AbortWaiter()
		{
			// Kill thread
			if ( null == m_Waiting ) return;
			
			// Terminate it
			try { m_Waiting.Abort(); } catch (Exception) {}                           

			// Detach
			m_Waiting = null;
		}

		/// <summary>
		/// Activate the timer to stop after the indicated number of seconds.
		/// </summary>
		/// <remarks>
		/// This method will always call <see cref="AbortWaiter"/>. If the number
		/// of seconds is positive a new <see cref="m_Waiting"/> <see cref="Thread"/>
		/// will be created running <see cref="WaitThread"/>. Before calling
		/// <see cref="Thread.Start"/> the <see cref="m_Interval"/> is initialized
		/// with the correct value. If the number of seconds is zero or negative
		/// the timer is canceled.
		/// </remarks>
		public double SecondsToWait
		{
			set
			{
				// Done with thread
				AbortWaiter();

				// Check mode
				if ( value > 0 )
				{
					// Calculate
					m_Interval = DateTime.UtcNow.AddSeconds(value).ToFileTimeUtc();

					// Create thread
					m_Waiting = new Thread(new ThreadStart(WaitThread));

					// Run it
					m_Waiting.Start();
				}
				else
				{
					// No timer
                    CancelWaitableTimer(SafeWaitHandle);
				}
			}
		}

		/// <summary>
		/// Initializes the timer with <see cref="m_Interval"/> and waits for it
		/// to expire. If the timer expires <see cref="OnTimerExpired"/> is fired.
		/// </summary>
		/// <remarks>
		/// The <see cref="Thread"/> may be terminated with a call to <see cref="AbortWaiter"/>
		/// before the time expires.
		/// </remarks>
		private void WaitThread()
		{
			// Ignore aborts
			try
			{
				// Interval to use
				long lInterval = m_Interval;

				// Start timer
                if (!SetWaitableTimer(SafeWaitHandle, &lInterval, 0, IntPtr.Zero, IntPtr.Zero, true)) throw new TimerException("Could not start Timer");

				// Wait for the timer to expire
				WaitOne();

				// Forward
				if ( null != OnTimerExpired ) OnTimerExpired();
			}
			catch (ThreadAbortException)
			{
				// Ignore
			}
		}

		/// <summary>
		/// Calles <see cref="AbortWaiter"/> and forwards to the base <see cref="WaitHandle.Close"/>
		/// method.
		/// </summary>
		public override void Close()
		{
			// Kill thread
			AbortWaiter();

			// Forward
			base.Close();
		}
	}
}
