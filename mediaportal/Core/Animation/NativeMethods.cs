using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Animations
{
	internal sealed class NativeMethods
	{
		#region Constructors

		private NativeMethods()
		{
		}

		#endregion Constructors

		#region Enums

		internal enum PeekMessageFlags
		{
			NoRemove,
			Remove,
			NoYeild,
		}

		internal enum Messages
		{
			Quit = 0x0012,
		}

		#endregion

		#region Helpers

		public static bool SetForegroundWindow(IntPtr window, bool force)
		{
			IntPtr windowForeground = GetForegroundWindow(); 

			if(window == windowForeground || SetForegroundWindow(window)) return true;
			if(force == false) return false;
			if(windowForeground == IntPtr.Zero) return false;
			if(!AttachThreadInput(AppDomain.GetCurrentThreadId(), GetWindowThreadProcessId(windowForeground, 0), true)) return false;

			SetForegroundWindow(window);
			BringWindowToTop(window);

			AttachThreadInput(AppDomain.GetCurrentThreadId(), GetWindowThreadProcessId(windowForeground, 0), false);

			return (GetForegroundWindow() == window);
		}

		public static long QueryPerformanceCounter()
		{
			long tick = 0;
			QueryPerformanceCounter(ref tick);
			return tick;
		}

		public static long QueryPerformanceFrequency()
		{
			long freq = 0;
			QueryPerformanceFrequency(ref freq);
			return freq;
		}
		
		#endregion Helpers

		#region Interop

		[DllImport("user32")]
		public static extern bool AttachThreadInput(int nThreadId, int nThreadIdTo, bool bAttach);
		
		[DllImport("user32")]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern int DispatchMessage(ref Message msg);

		[DllImport("user32")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, int unused);

		[DllImport("user32")]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32")]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("user32", CharSet=CharSet.Auto)]
		public static extern bool PeekMessage(ref Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, PeekMessageFlags flags);

		[DllImport("user32")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32")] 
		public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern int TranslateMessage(ref Message msg);
	
		[DllImport("kernel32.dll")]
		public extern static bool QueryPerformanceCounter(ref long x);

		[DllImport("kernel32.dll")]
		public extern static bool QueryPerformanceFrequency(ref long x);

		#endregion Interop

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		public struct Message
		{
			public IntPtr hWnd;
			public Messages msg;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public System.Drawing.Point p;
		}

		#endregion Structures
	}
}
