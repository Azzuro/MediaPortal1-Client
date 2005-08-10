using System;
using System.ComponentModel;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace MediaPortal.Dispatcher
{
	public sealed class Job
	{
		#region Events

		public event				DoWorkEventHandler DoWork;
		public event				ProgressChangedEventHandler ProgressChanged;
		public event				RunWorkerCompletedEventHandler RunWorkerCompleted;

		#endregion Events

		#region Methods

		public void Cancel()
		{
			lock(this)
				_isCancelPending = true;
		}

		public void Dispatch()
		{
			Dispatch(DateTime.Now);
		}

		public void Dispatch(int ticks)
		{
			Dispatch(DateTime.Now + TimeSpan.FromMilliseconds(ticks));
		}

		public void Dispatch(TimeSpan timespan)
		{
			Dispatch(DateTime.Now + timespan);
		}

		public void Dispatch(DateTime dateTime)
		{
			if(DoWork == null)
				return;

			lock(this)
				_dateTime = dateTime;

			JobDispatcher.Dispatch(this);
		}

		void InvokeDelegate(Delegate handler, object[] e)
		{
			ISynchronizeInvoke synchronizer = (ISynchronizeInvoke)handler.Target;

			if(synchronizer == null)
			{
				handler.DynamicInvoke(e);
				return;
			}

			if(synchronizer.InvokeRequired == false)
			{
				handler.DynamicInvoke(e);
				return;
			}

			synchronizer.Invoke(handler, e);
		}

		void InvokeDelegate(Delegate[] handlers, object[] e)
		{
			foreach(Delegate handler in handlers)
				InvokeDelegate(handler, e);
		}
	
		void ReportCompletion(IAsyncResult asyncResult)
		{
			AsyncResult ar = (AsyncResult)asyncResult;

			DoWorkEventHandler handler = (DoWorkEventHandler)ar.AsyncDelegate;
			DoWorkEventArgs args = (DoWorkEventArgs)ar.AsyncState;

			object result = null;

			Exception error = null;

			try
			{
				handler.EndInvoke(asyncResult);
				result = args.Result;
			}
			catch(Exception exception)
			{
				error = exception;
			}

			if(RunWorkerCompleted != null)
				InvokeDelegate(RunWorkerCompleted.GetInvocationList(), new object[] { this, new RunWorkerCompletedEventArgs(result, error, args.Cancel) });
		}

		public void ReportProgress(int percent)
		{
			if(_isProgressReported == false)
				throw new InvalidOperationException("Job does not report its progress");

			object[] e = new object[] { this, new ProgressChangedEventArgs(percent) };

			foreach(Delegate handler in ProgressChanged.GetInvocationList())
				InvokeDelegate(handler, e);
		}

		internal void Run()
		{
			if(DoWork == null)
				return;
			
			_isCancelPending = false;

			DoWorkEventArgs args = new DoWorkEventArgs(_argument);
			DoWork.BeginInvoke(this, args, new AsyncCallback(ReportCompletion), args);
		}

		#endregion Methods

		#region Properties

		public bool IsCancelPending
		{
			get { lock(this) return _isCancelPending; }
		}

		public object Argument
		{
			get { lock(this) return _argument; }
			set { lock(this) _argument = value; }
		}

		public JobFlags Flags
		{
			get { lock(this) return _flags; }
			set { lock(this) _flags = value; }
		}

		public bool IsReady
		{
			get { lock(this) return _isCancelPending == false && DateTime.Compare(_dateTime, DateTime.Now) <= 0; }
		}

		public string Name
		{
			get { lock(this) return _name; }
			set { lock(this) _name = value; }
		}

		public DateTime Next
		{
			get { lock(this) return _dateTime; }
		}

		public JobPriority Priority
		{
			get { lock(this) return _priority; }
			set { lock(this) _priority = value; }
		}

		public bool JobSupportsCancellation
		{
			get { lock(this) return _isCancellationSupported; } 
			set { lock(this) _isCancellationSupported = value; } 
		}

		public bool JobReportsProgress
		{
			get { lock(this) return _isProgressReported; }
			set { lock(this) _isProgressReported = value; }
		}

		#endregion Properties

		#region Fields

		DateTime					_dateTime = DateTime.Now;
		JobFlags					_flags;
		bool						_isCancelPending = false;
		bool						_isProgressReported = false;
		bool						_isCancellationSupported = false;
		string						_name = string.Empty;
		JobPriority					_priority = JobPriority.Lowest;
		object						_argument;

		#endregion Fields
	}
}
