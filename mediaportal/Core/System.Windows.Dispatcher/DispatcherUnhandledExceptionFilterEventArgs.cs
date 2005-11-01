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

namespace System.Windows.Dispatcher
{
	public sealed class DispatcherUnhandledExceptionFilterEventArgs : DispatcherEventArgs
	{
		#region Constructors

		public DispatcherUnhandledExceptionFilterEventArgs(Dispatcher dispatcher, Exception exception) : base(dispatcher)
		{
			_exception = exception;
		}

		#endregion Constructors

		#region Properties

		public Exception Exception
		{
			get { return _exception; }
		}

		public bool RequestCatch
		{
			get { return _isRequestCatch; }
			set { _isRequestCatch = value; }
		}

		#endregion Properties

		#region Fields

		Exception					_exception;
		bool						_isRequestCatch = true;

		#endregion Fields
	}
}
