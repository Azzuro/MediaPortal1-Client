using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace MediaPortal.TV.Recording
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
		[DllImport("Kernel32.dll", EntryPoint="SetWaitableTimer")] static extern private bool SetWaitableTimer(IntPtr hTimer, Int64 *pDue, Int32 lPeriod, IntPtr rNotify, IntPtr pArgs, bool bResume);

		/// <summary>
		/// Wrap the system function <i>CreateWaitableTimer</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="CreateWaitableTimer")] static extern private IntPtr CreateWaitableTimer(IntPtr pSec, bool bManual, string szName);

		/// <summary>
		/// Wrap the system function <i>CancelWaitableTimer</i>.
		/// </summary>
		[DllImport("Kernel32.dll", EntryPoint="CancelWaitableTimer")] static extern private bool CancelWaitableTimer(IntPtr hTimer);

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
			Handle = CreateWaitableTimer(IntPtr.Zero, false, null);

			// Test
			if ( Handle.Equals(IntPtr.Zero) ) throw new TimerException("Unable to create Waitable Timer");
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
					CancelWaitableTimer(Handle);
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
				if ( !SetWaitableTimer(Handle, &lInterval, 0, IntPtr.Zero, IntPtr.Zero, true) ) throw new TimerException("Could not start Timer");

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
