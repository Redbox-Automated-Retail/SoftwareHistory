using System;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.Connection;

internal abstract class PhysicalConnection
{
	public SecuritySession ConnectionLevelSecurity;

	protected object _accessToLocalMembers = new object();

	public object PhysicalConnectionStateLock = new object();

	public bool ConnectionAvailable = false;

	private bool _reestablish_IsBeingReestablished = false;

	private bool __disposed = false;

	internal Exception _disposeReason = null;

	internal ReaderWriterLock DisposeLock = new ReaderWriterLock();

	public bool Reestablish_IsBeingReestablished
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _reestablish_IsBeingReestablished;
			}
		}
	}

	internal bool IsDisposed
	{
		get
		{
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(DisposeLock);
			try
			{
				return __disposed;
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
		}
		set
		{
			WriterAutoLocker writerAutoLocker = new WriterAutoLocker(DisposeLock);
			try
			{
				__disposed = value;
			}
			finally
			{
				writerAutoLocker.Dispose();
			}
		}
	}

	public PhysicalConnection()
	{
	}

	public bool AcquireIfAvailable()
	{
		lock (PhysicalConnectionStateLock)
		{
			if (!ConnectionAvailable)
			{
				return false;
			}
			ConnectionAvailable = false;
			return true;
		}
	}

	public void MarkAsAvailable()
	{
		lock (PhysicalConnectionStateLock)
		{
			ConnectionAvailable = true;
		}
	}

	public bool Reestablish_ObtainStatus()
	{
		lock (_accessToLocalMembers)
		{
			if (_reestablish_IsBeingReestablished)
			{
				return false;
			}
			_reestablish_IsBeingReestablished = true;
			return true;
		}
	}

	public void Reestablish_ResetStatus()
	{
		lock (_accessToLocalMembers)
		{
			if (!_reestablish_IsBeingReestablished)
			{
				GenuineLoggingServices.BinaryLogWriter?.WriteImplementationWarningEvent("PhysicalConnection.Reestablish_ResetStatus", LogMessageType.Error, GenuineExceptions.Get_Debugging_GeneralWarning("The connection is not being reestablished!"), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The connection is not being reestablished!");
			}
			_reestablish_IsBeingReestablished = false;
		}
	}

	public void Dispose(Exception reason)
	{
		if (IsDisposed)
		{
			return;
		}
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(DisposeLock);
		try
		{
			if (IsDisposed)
			{
				return;
			}
			IsDisposed = true;
			_disposeReason = reason;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		InternalDispose(reason);
	}

	public abstract void InternalDispose(Exception reason);
}
