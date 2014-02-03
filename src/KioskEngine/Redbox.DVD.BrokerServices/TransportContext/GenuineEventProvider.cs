using System;
using System.Collections;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.TransportContext;

internal class GenuineEventProvider : MarshalByRefObject, IGenuineEventProvider
{
	public ITransportContext ITransportContext;

	private ArrayList _handlers = new ArrayList();

	private ReaderWriterLock _handlersLock = new ReaderWriterLock();

	public GenuineEventProvider(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
	}

	public void Register(IGenuineEventHandler iGenuineEventHandler)
	{
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_handlersLock);
		try
		{
			if (!_handlers.Contains(iGenuineEventHandler))
			{
				_handlers.Add(iGenuineEventHandler);
			}
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
	}

	public void Unregister(IGenuineEventHandler iGenuineEventHandler)
	{
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_handlersLock);
		try
		{
			_handlers.Remove(iGenuineEventHandler);
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
	}

	public void Fire(GenuineEventArgs genuineEventArgs)
	{
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_handlersLock);
		object[] array;
		try
		{
			array = _handlers.ToArray();
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			IGenuineEventHandler genuineEventHandler = (IGenuineEventHandler)array2[i];
			try
			{
				genuineEventHandler.OnGenuineEvent(genuineEventArgs);
			}
			catch (Exception)
			{
				WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_handlersLock);
				try
				{
					_handlers.Remove(genuineEventHandler);
				}
				finally
				{
					writerAutoLocker.Dispose();
				}
			}
		}
		if (genuineEventArgs.EventType == GenuineEventType.GTcpConnectionAccepted)
		{
			FireGlobalEvent(genuineEventArgs);
		}
		else
		{
			GenuineThreadPool.QueueUserWorkItem(FireGlobalEvent, genuineEventArgs, longDuration: false);
		}
	}

	internal void FireGlobalEvent(object genuineEventArgsAsObject)
	{
		GenuineEventArgs e = (GenuineEventArgs)genuineEventArgsAsObject;
		GenuineGlobalEventProvider.FireGlobalEvent(ITransportContext, e);
	}
}
