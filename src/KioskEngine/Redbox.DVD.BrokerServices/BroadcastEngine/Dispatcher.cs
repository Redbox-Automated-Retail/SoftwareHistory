using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal class Dispatcher : MarshalByRefObject
{
	private BroadcastProxy _broadcastProxy;

	private object _accessToPrivateVariables = new object();

	private Type _interfaceToSupport;

	private object _tag;

	private TimeSpan _receiveResultsTimeOut = TimeSpan.FromSeconds(120.0);

	private bool _callIsAsync = false;

	private BroadcastCallFinishedHandler _broadcastCallFinishedHandler;

	private int _maximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically = 4;

	private int _maximumNumberOfConsecutiveFailsToEnableSimulationMode = 4;

	private bool _ignoreRecurrentCalls = true;

	private ClientSponsor GlobalSponsor = new ClientSponsor();

	internal ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

	internal Hashtable _receivers = new Hashtable();

	internal object[] _cachedReceiversInfoArray;

	private object _transparentProxy;

	private IMulticastFilter _iMulticastFilter;

	private object _iMulticastFilterLock = new object();

	private int _dbgDispatcherId = Interlocked.Increment(ref _dbgTotalDispatchers);

	private static int _dbgTotalDispatchers = 0;

	private string _dbgDispatcherName;

	public Type SupportedInterface => _interfaceToSupport;

	public object Tag
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _tag;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_tag = value;
			}
		}
	}

	public TimeSpan ReceiveResultsTimeOut
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _receiveResultsTimeOut;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_receiveResultsTimeOut = value;
			}
		}
	}

	public bool CallIsAsync
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _callIsAsync && BroadcastCallFinishedHandler != null;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				if (value && BroadcastCallFinishedHandler == null)
				{
					throw GenuineExceptions.Get_Broadcast_HandlerUninitialized();
				}
				_callIsAsync = value;
			}
		}
	}

	public BroadcastCallFinishedHandler BroadcastCallFinishedHandler
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _broadcastCallFinishedHandler;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_broadcastCallFinishedHandler = value;
			}
		}
	}

	public int MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _maximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_maximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically = value;
			}
		}
	}

	public int MaximumNumberOfConsecutiveFailsToEnableSimulationMode
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _maximumNumberOfConsecutiveFailsToEnableSimulationMode;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_maximumNumberOfConsecutiveFailsToEnableSimulationMode = value;
			}
		}
	}

	public bool IgnoreRecurrentCalls
	{
		get
		{
			lock (_accessToPrivateVariables)
			{
				return _ignoreRecurrentCalls;
			}
		}
		set
		{
			lock (_accessToPrivateVariables)
			{
				_ignoreRecurrentCalls = value;
			}
		}
	}

	public int RecipientsSubscribed
	{
		get
		{
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_readerWriterLock);
			try
			{
				return _receivers.Count;
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
		}
	}

	public object TransparentProxy => _transparentProxy;

	public IMulticastFilter IMulticastFilter
	{
		get
		{
			lock (_iMulticastFilterLock)
			{
				return _iMulticastFilter;
			}
		}
		set
		{
			lock (_iMulticastFilterLock)
			{
				_iMulticastFilter = value;
			}
		}
	}

	public int DbgDispatcherId => _dbgDispatcherId;

	public string DbgDispatcherName
	{
		get
		{
			return _dbgDispatcherName;
		}
		set
		{
			_dbgDispatcherName = value;
		}
	}

	public event BroadcastReceiverHasBeenExcludedEventHandler BroadcastReceiverHasBeenExcluded;

	public Dispatcher(Type interfaceToSupport)
	{
		if (!interfaceToSupport.IsInterface)
		{
			throw GenuineExceptions.Get_Broadcast_DestinationTypeMustBeAnInterface();
		}
		_broadcastProxy = new BroadcastProxy(interfaceToSupport, this);
		_transparentProxy = _broadcastProxy.GetTransparentProxy();
		_interfaceToSupport = interfaceToSupport;
	}

	public bool Add(MarshalByRefObject obj)
	{
		return Add(obj, null);
	}

	public bool Add(MarshalByRefObject obj, object tag)
	{
		return Add(obj, tag, null, null);
	}

	public bool Add(MarshalByRefObject obj, object tag, string remoteGenuineUri, ITransportContext transportContext)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		string objectUri = RemotingServices.GetObjectUri(obj);
		if (objectUri == null && !RemotingServices.IsObjectOutOfAppDomain(obj))
		{
			RemotingServices.Marshal(obj);
			objectUri = RemotingServices.GetObjectUri(obj);
		}
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_readerWriterLock);
		try
		{
			if (_receivers.ContainsKey(objectUri))
			{
				return false;
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		ReceiverInfo receiverInfo = new ReceiverInfo();
		receiverInfo.MbrObject = obj;
		receiverInfo.MbrUri = objectUri;
		receiverInfo.Tag = tag;
		if (binaryLogWriter != null)
		{
			try
			{
				if (receiverInfo.MbrObject != null)
				{
					receiverInfo.DbgRemoteHost = GenuineUtility.FetchHostInformationFromMbr(receiverInfo.MbrObject);
				}
			}
			catch (Exception exception)
			{
				binaryLogWriter.WriteImplementationWarningEvent("Dispatcher.Add", LogMessageType.Error, exception, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Can't get HostInformation from MbrObject.");
			}
		}
		ObjRef objRef = receiverInfo.MbrObject.CreateObjRef(typeof(MarshalByRefObject));
		receiverInfo.Local = objRef.IsFromThisAppDomain();
		if (!receiverInfo.Local)
		{
			if (remoteGenuineUri != null)
			{
				receiverInfo.IClientChannelSink = new GenuineTcpClientTransportSink(remoteGenuineUri, transportContext);
			}
			else
			{
				receiverInfo.IClientChannelSink = ChannelServices.GetChannelSinkProperties(obj)["GC_TS"] as IClientChannelSink;
				if (receiverInfo.IClientChannelSink == null)
				{
					throw GenuineExceptions.Get_Broadcast_ClientSinkIsUnknown();
				}
			}
			receiverInfo.SerializedObjRef = objRef;
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
		{
			binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.Add", LogMessageType.BroadcastRecipientAdded, null, null, receiverInfo.DbgRemoteHost, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, null, true, receiverInfo, null, null, "The broadcast recipient is added.");
		}
		((ILease)RemotingServices.GetLifetimeService(obj))?.Register(GlobalSponsor);
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_readerWriterLock);
		try
		{
			_cachedReceiversInfoArray = null;
			_receivers[objectUri] = receiverInfo;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		return true;
	}

	public string Add(GeneralBroadcastSender generalBroadcastSender)
	{
		ReceiverInfo receiverInfo = new ReceiverInfo();
		receiverInfo.GeneralBroadcastSender = generalBroadcastSender;
		receiverInfo.Local = false;
		string text = (receiverInfo.MbrUri = Guid.NewGuid().ToString("N") + "/" + generalBroadcastSender.Court);
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_readerWriterLock);
		try
		{
			_cachedReceiversInfoArray = null;
			_receivers[text] = receiverInfo;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
		{
			binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.Add", LogMessageType.BroadcastRecipientAdded, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, null, true, receiverInfo, generalBroadcastSender.Court, generalBroadcastSender.ITransportContext.HostIdentifier, "The \"true\" multicast sender is added.");
		}
		return text;
	}

	public bool Remove(MarshalByRefObject obj)
	{
		string objectUri = RemotingServices.GetObjectUri(obj);
		return Remove(objectUri);
	}

	public bool Remove(string uri)
	{
		if (uri == null || uri.Length <= 0)
		{
			return false;
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_readerWriterLock);
		try
		{
			if (!_receivers.ContainsKey(uri))
			{
				return false;
			}
			_cachedReceiversInfoArray = null;
			ReceiverInfo receiverInfo = (ReceiverInfo)_receivers[uri];
			_receivers.Remove(uri);
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.Remove", LogMessageType.BroadcastRecipientRemoved, null, null, receiverInfo.DbgRemoteHost, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, null, true, receiverInfo, uri, null, "The broadcast recipient or \"true\" broadcast sender is removed from the list of recipients.");
			}
			GenuineThreadPool.QueueUserWorkItem(DeleteSponsorFromTheObjectAndFireEvent, receiverInfo, longDuration: false);
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		return true;
	}

	public void DeleteSponsorFromTheObjectAndFireEvent(object state)
	{
		ReceiverInfo receiverInfo = (ReceiverInfo)state;
		MarshalByRefObject mbrObject = receiverInfo.MbrObject;
		try
		{
			if (BroadcastReceiverHasBeenExcluded != null)
			{
				BroadcastReceiverHasBeenExcluded(mbrObject, receiverInfo);
			}
		}
		catch
		{
		}
		try
		{
			((ILease)RemotingServices.GetLifetimeService(mbrObject))?.Unregister(GlobalSponsor);
		}
		catch
		{
		}
	}

	public void Clear()
	{
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_readerWriterLock);
		try
		{
			_cachedReceiversInfoArray = null;
			object[] array = new object[_receivers.Count];
			_receivers.Values.CopyTo(array, 0);
			GenuineThreadPool.QueueUserWorkItem(UnregisterSponsor, array, longDuration: true);
			_receivers.Clear();
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
	}

	private void UnregisterSponsor(object receiverInfoItemsAsObject)
	{
		object[] array = (object[])receiverInfoItemsAsObject;
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ReceiverInfo receiverInfo = (ReceiverInfo)array2[i];
			try
			{
				if (receiverInfo.MbrObject != null)
				{
					BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.UnregisterSponsor", LogMessageType.BroadcastRecipientRemoved, null, null, receiverInfo.DbgRemoteHost, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, null, true, receiverInfo, null, null, "The broadcast recipient is removed after Dispatcher.Clear call.");
					}
					((ILease)RemotingServices.GetLifetimeService(receiverInfo.MbrObject))?.Unregister(GlobalSponsor);
				}
			}
			catch
			{
			}
		}
	}

	public MarshalByRefObject FindObjectByUri(string uri)
	{
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_readerWriterLock);
		try
		{
			if (!(_receivers[uri] is ReceiverInfo { MbrObject: var mbrObject }))
			{
				return null;
			}
			return mbrObject;
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	public ReceiverInfo GetReceiverInfo(string mbrUri)
	{
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_readerWriterLock);
		try
		{
			return _receivers[mbrUri] as ReceiverInfo;
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	public void AsyncEventStub(Dispatcher dispatcher, IMessage message, ResultCollector resultCollector)
	{
	}

	public void GetListOfReceivers(out object[] arrayOfReceiverInfo, IMessage iMessage, ResultCollector resultCollector)
	{
		arrayOfReceiverInfo = null;
		try
		{
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_readerWriterLock);
			try
			{
				if (_cachedReceiversInfoArray == null)
				{
					_cachedReceiversInfoArray = new object[_receivers.Values.Count];
					_receivers.Values.CopyTo(_cachedReceiversInfoArray, 0);
				}
				arrayOfReceiverInfo = _cachedReceiversInfoArray;
				IMulticastFilter multicastFilter = GetCurrentFilter();
				if (multicastFilter == null)
				{
					multicastFilter = IMulticastFilter;
				}
				if (multicastFilter != null)
				{
					BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.GetListOfReceivers", LogMessageType.BroadcastFilterCalled, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, resultCollector, false, null, multicastFilter.GetType().ToString(), null, "The broadcast filter is called. The type of the filter: {0}.", multicastFilter.GetType().ToString());
					}
					arrayOfReceiverInfo = multicastFilter.GetReceivers(arrayOfReceiverInfo, iMessage);
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter2 = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter2 != null && binaryLogWriter2[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter2.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "Dispatcher.GetListOfReceivers", LogMessageType.BroadcastFilterCalled, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, this, resultCollector, false, null, null, null, "The exception occurred while calling the broadcast filter.");
			}
		}
	}

	public static IMulticastFilter SetCurrentFilter(IMulticastFilter iMulticastFilter)
	{
		LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__DispatcherFilterUsed");
		object data = Thread.GetData(namedDataSlot);
		Thread.SetData(namedDataSlot, iMulticastFilter);
		return data as IMulticastFilter;
	}

	public static IMulticastFilter GetCurrentFilter()
	{
		LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__DispatcherFilterUsed");
		return Thread.GetData(namedDataSlot) as IMulticastFilter;
	}
}
