using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal class ResultCollector : IResponseProcessor, IDisposable, IClientChannelSink, IChannelSinkBase, IMessageSink
{
	private const string _uniqueReceiverName = "_";

	private Dispatcher _dispatcher;

	private IMessage _iMessage;

	private string _broadcastGuid = Guid.NewGuid().ToString("N");

	private GenuineChunkedStream _messageStream;

	private bool _secondStagePerformed = false;

	public Hashtable Successful;

	public Hashtable Failed;

	public ManualResetEvent AllMessagesReceived = new ManualResetEvent(initialState: false);

	public Hashtable UnrepliedReceivers = Hashtable.Synchronized(new Hashtable());

	private BroadcastCallFinishedHandler _broadcastCallFinishedHandler;

	private int _dbgResultCollectorId = Interlocked.Increment(ref _dbgTotalResultCollectors);

	private static int _dbgTotalResultCollectors = 0;

	public HostInformation Remote => null;

	public bool IsShortInProcessing => false;

	public Message Message => null;

	public IDictionary Properties => null;

	public IClientChannelSink NextChannelSink => null;

	public IMessageSink NextSink => null;

	public int DbgResultCollectorId => _dbgResultCollectorId;

	public ResultCollector(Dispatcher dispatcher, int capacity, IMessage iMessage)
	{
		_iMessage = iMessage;
		_dispatcher = dispatcher;
		Successful = new Hashtable(capacity);
		Failed = new Hashtable(capacity);
	}

	public void Dispose()
	{
	}

	public void ParseResult(string mbrUri, IMethodReturnMessage returnMessage, Exception ex)
	{
		if (AllMessagesReceived.WaitOne(0, exitContext: false))
		{
			return;
		}
		ReceiverInfo receiverInfo = _dispatcher.GetReceiverInfo(mbrUri);
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		Exception ex2 = null;
		if (ex != null || (returnMessage != null && returnMessage.Exception != null))
		{
			ex2 = ((ex != null) ? ex : returnMessage.Exception);
			HostInformation remote = receiverInfo?.DbgRemoteHost;
			string text = ((receiverInfo == null) ? string.Empty : receiverInfo.NumberOfFails.ToString());
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.ParseResult", LogMessageType.BroadcastResponseParsed, ex2, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, text, "false", "The exception is received in response to the broadcast. NumberOfFails: {0}.", text);
			}
		}
		if (ex2 is OperationException)
		{
			OperationException ex3 = (OperationException)ex2;
			if (ex3.OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Broadcast.CallHasAlreadyBeenMade")
			{
				return;
			}
		}
		lock (this)
		{
			UnrepliedReceivers.Remove(mbrUri);
			if (Successful.ContainsKey(mbrUri))
			{
				return;
			}
		}
		lock (this)
		{
			if (ex2 != null)
			{
				Failed[mbrUri] = ex2;
				if (receiverInfo != null)
				{
					lock (receiverInfo)
					{
						if (receiverInfo.GeneralBroadcastSender == null && _dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically != 0 && receiverInfo.NumberOfFails >= _dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically)
						{
							_dispatcher.Remove(receiverInfo.MbrObject);
						}
					}
				}
			}
			else
			{
				Successful[mbrUri] = returnMessage;
				Failed.Remove(mbrUri);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
				{
					HostInformation remote2 = receiverInfo?.DbgRemoteHost;
					binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.ParseResult", LogMessageType.BroadcastResponseParsed, null, null, remote2, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, null, "true", "The successful response is received.");
				}
				if (receiverInfo != null)
				{
					lock (receiverInfo)
					{
						receiverInfo.NumberOfFails = 0;
					}
				}
			}
			if (UnrepliedReceivers.Count == 0)
			{
				AllMessagesReceived.Set();
			}
		}
	}

	public bool StartReceiving()
	{
		bool flag = false;
		lock (_dispatcher)
		{
			flag = _dispatcher.CallIsAsync;
			_broadcastCallFinishedHandler = _dispatcher.BroadcastCallFinishedHandler;
		}
		if (!flag || _broadcastCallFinishedHandler == null)
		{
			AllMessagesReceived.WaitOne(_dispatcher.ReceiveResultsTimeOut, exitContext: false);
			WaitUntilReceiversReplyOrTimeOut(null, timeOut: false);
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.StartReceiving", LogMessageType.BroadcastInvocationCompleted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, null, null, null, "The broadcast invocation is completed.");
			}
			AllMessagesReceived.Set();
			return true;
		}
		ThreadPool.RegisterWaitForSingleObject(AllMessagesReceived, WaitUntilReceiversReplyOrTimeOut, null, _dispatcher.ReceiveResultsTimeOut, executeOnlyOnce: true);
		return false;
	}

	private void WaitUntilReceiversReplyOrTimeOut(object state, bool timeOut)
	{
		bool flag = false;
		lock (this)
		{
			if (Failed.Count > 0 && !_secondStagePerformed)
			{
				foreach (object item in Failed)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
					string text = (string)dictionaryEntry.Key;
					ReceiverInfo receiverInfo = _dispatcher.GetReceiverInfo(text);
					int num = 0;
					if (receiverInfo != null)
					{
						lock (receiverInfo)
						{
							num = ++receiverInfo.NumberOfFails;
						}
					}
					if (_dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically != 0 && num >= _dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically)
					{
						_dispatcher.Remove(text);
					}
					if (dictionaryEntry.Value is OperationException { OperationErrorMessage: not null } ex && ex.OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Broadcast.RemoteEndPointDidNotReplyForTimeOut")
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			SendMessageToFailedReceiversDirectly();
			StartReceiving();
			return;
		}
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
		{
			binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.WaitUntilReceiversReplyOrTimeOut", LogMessageType.BroadcastInvocationCompleted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, null, null, null, "The broadcast invocation is completed.");
		}
		AllMessagesReceived.Set();
		if (_broadcastCallFinishedHandler != null)
		{
			_broadcastCallFinishedHandler(_dispatcher, _iMessage, this);
		}
	}

	public void PerformBroadcasting(IMessage msg)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		string text = null;
		string text2 = null;
		try
		{
			ArrayList arrayList = new ArrayList();
			_iMessage = msg;
			text = BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string);
			BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
			_messageStream = new GenuineChunkedStream(releaseOnReadMode: false);
			binaryFormatter.Serialize(_messageStream, msg);
			string text3 = null;
			if (_dispatcher.IgnoreRecurrentCalls)
			{
				text3 = Guid.NewGuid().ToString("N");
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.PerformBroadcasting", LogMessageType.BroadcastInvocationInitiated, null, null, null, (binaryLogWriter[LogCategory.BroadcastEngine] > 1) ? ((Stream)_messageStream.Clone()) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, true, _dispatcher, this, false, null, text, null, "The broadcast invocation is initiated.");
			}
			UnrepliedReceivers["_"] = null;
			_dispatcher.GetListOfReceivers(out var arrayOfReceiverInfo, msg, this);
			for (int i = 0; i < arrayOfReceiverInfo.Length; i++)
			{
				if (!(arrayOfReceiverInfo[i] is ReceiverInfo { MbrUri: var mbrUri } receiverInfo))
				{
					continue;
				}
				text2 = mbrUri;
				try
				{
					lock (receiverInfo)
					{
						if (_dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically != 0 && receiverInfo.NumberOfFails >= _dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically)
						{
							arrayList.Add(mbrUri);
							continue;
						}
					}
					if (!receiverInfo.Local && receiverInfo.GeneralBroadcastSender == null)
					{
						lock (this)
						{
							Failed[mbrUri] = GenuineExceptions.Get_Broadcast_RemoteEndPointDidNotReplyForTimeOut();
						}
					}
					if (receiverInfo.Local)
					{
						if (!_dispatcher.IgnoreRecurrentCalls || !UniqueCallTracer.Instance.WasGuidRegistered(mbrUri + text3))
						{
							UnrepliedReceivers[mbrUri] = null;
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
							{
								binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.PerformBroadcasting", LogMessageType.BroadcastRecipientInvoked, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, null, null, "The local receiver is invoked via LocalPerformer.");
							}
							IMessage msg2 = (IMessage)binaryFormatter.Deserialize((Stream)_messageStream.Clone());
							LocalPerformer localPerformer = new LocalPerformer(msg2, this, receiverInfo.MbrObject);
							GenuineThreadPool.QueueUserWorkItem(localPerformer.Call, null, longDuration: false);
						}
						continue;
					}
					if (receiverInfo.GeneralBroadcastSender != null)
					{
						Stream stream = (Stream)_messageStream.Clone();
						msg.Properties["__Uri"] = string.Empty;
						Message message = Message.CreateOutcomingMessage(receiverInfo.GeneralBroadcastSender.ITransportContext, msg, new TransportHeaders(), stream, isSynchronous: false);
						message.DestinationMarshalByRef = receiverInfo.SerializedObjRef;
						message.GenuineMessageType = GenuineMessageType.TrueBroadcast;
						if (_dispatcher.IgnoreRecurrentCalls)
						{
							message.ITransportHeaders["_genbg"] = text3;
						}
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							message.ITransportHeaders["__genit"] = text2;
							message.ITransportHeaders["__genmn"] = text;
							binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.PerformBroadcasting", LogMessageType.BroadcastRecipientInvoked, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, null, null, "Mulsticast sender is being invoked.");
						}
						receiverInfo.GeneralBroadcastSender.ITransportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, this);
						receiverInfo.GeneralBroadcastSender.SendMessage(message, this);
						continue;
					}
					UnrepliedReceivers[mbrUri] = null;
					if (receiverInfo.NeedsBroadcastSimulation)
					{
						Stream stream2 = (Stream)_messageStream.Clone();
						TransportHeaders transportHeaders = new TransportHeaders();
						if (_dispatcher.IgnoreRecurrentCalls)
						{
							transportHeaders["_genbg"] = text3;
						}
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.PerformBroadcasting", LogMessageType.BroadcastRecipientInvoked, null, null, receiverInfo.DbgRemoteHost, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, null, null, "The broadcast recipient is being invoked directly.");
						}
						msg.Properties["__Uri"] = receiverInfo.MbrUri;
						transportHeaders["__genbdc"] = receiverInfo.SerializedObjRef;
						transportHeaders["_genu"] = receiverInfo.MbrUri;
						transportHeaders["__genmt"] = GenuineMessageType.BroadcastEngine;
						transportHeaders["__genit"] = text2;
						transportHeaders["__genmn"] = text;
						ClientChannelSinkStack clientChannelSinkStack = new ClientChannelSinkStack(this);
						clientChannelSinkStack.Push(this, null);
						receiverInfo.IClientChannelSink.AsyncProcessRequest(clientChannelSinkStack, _iMessage, transportHeaders, stream2);
					}
				}
				catch (Exception ex)
				{
					ParseResult(mbrUri, null, ex);
				}
			}
			UnrepliedReceivers.Remove("_");
			if (UnrepliedReceivers.Count <= 0)
			{
				AllMessagesReceived.Set();
			}
			StartReceiving();
			if (arrayList.Count <= 0)
			{
				return;
			}
			foreach (string item in arrayList)
			{
				_dispatcher.Remove(item);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
			{
				binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.PerformBroadcasting", LogMessageType.BroadcastInvocationInitiated, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, null, text2, text, "A critical failure occurred during broadcast.");
			}
			throw;
		}
	}

	public void SendMessageToFailedReceiversDirectly()
	{
		if (_secondStagePerformed)
		{
			return;
		}
		_secondStagePerformed = true;
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_dispatcher._readerWriterLock);
		try
		{
			UnrepliedReceivers["_"] = null;
			lock (this)
			{
				foreach (object item in Failed)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
					if (!(dictionaryEntry.Value is OperationException { OperationErrorMessage: not null } ex) || !(ex.OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Broadcast.RemoteEndPointDidNotReplyForTimeOut"))
					{
						continue;
					}
					string mbrUri = (string)dictionaryEntry.Key;
					ReceiverInfo receiverInfo = _dispatcher.GetReceiverInfo(mbrUri);
					if (receiverInfo == null || receiverInfo.NeedsBroadcastSimulation)
					{
						continue;
					}
					lock (receiverInfo)
					{
						receiverInfo.NumberOfMulticastFails++;
						if (_dispatcher.MaximumNumberOfConsecutiveFailsToEnableSimulationMode != 0 && receiverInfo.NumberOfMulticastFails >= _dispatcher.MaximumNumberOfConsecutiveFailsToEnableSimulationMode)
						{
							receiverInfo.NeedsBroadcastSimulation = true;
							receiverInfo.NumberOfMulticastFails = 0;
						}
					}
					Stream stream = (Stream)_messageStream.Clone();
					TransportHeaders transportHeaders = new TransportHeaders();
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						string @string = BinaryLogWriter.ParseInvocationMethod(_iMessage.Properties["__MethodName"] as string, _iMessage.Properties["__TypeName"] as string);
						string mbrUri2 = receiverInfo.MbrUri;
						binaryLogWriter.WriteBroadcastEngineEvent(LogCategory.BroadcastEngine, "ResultCollector.SendMessageToFailedReceiversDirectly", LogMessageType.BroadcastRecipientInvokedAfterTimeout, null, null, receiverInfo.DbgRemoteHost, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, false, _dispatcher, this, false, receiverInfo, mbrUri2, @string, "The broadcast invocation is being directed to the recipient, which did not respond during the first stage.");
					}
					_iMessage.Properties["__Uri"] = receiverInfo.MbrUri;
					transportHeaders["__genbdc"] = receiverInfo.SerializedObjRef;
					ClientChannelSinkStack clientChannelSinkStack = new ClientChannelSinkStack(this);
					clientChannelSinkStack.Push(this, null);
					receiverInfo.IClientChannelSink.AsyncProcessRequest(clientChannelSinkStack, _iMessage, transportHeaders, stream);
				}
			}
			UnrepliedReceivers.Remove("_");
			if (UnrepliedReceivers.Count <= 0)
			{
				AllMessagesReceived.Set();
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	public void ProcessRespose(Message message)
	{
		if (!(message.DestinationMarshalByRef is string mbrUri))
		{
			return;
		}
		lock (_dispatcher)
		{
			ReceiverInfo receiverInfo = _dispatcher.GetReceiverInfo(mbrUri);
			if (receiverInfo == null)
			{
				return;
			}
			receiverInfo.NeedsBroadcastSimulation = false;
			receiverInfo.NumberOfMulticastFails = 0;
		}
		if (AllMessagesReceived.WaitOne(0, exitContext: false))
		{
			return;
		}
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			object obj = binaryFormatter.Deserialize(message.Stream);
			ParseResult(mbrUri, obj as IMethodReturnMessage, obj as Exception);
		}
		catch (Exception ex)
		{
			ParseResult(mbrUri, null, ex);
		}
	}

	public void DispatchException(object exceptionAsObject)
	{
	}

	public bool IsExpired(int now)
	{
		return AllMessagesReceived.WaitOne(0, exitContext: false);
	}

	public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
	{
	}

	public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
	{
		IMethodReturnMessage methodReturnMessage = null;
		string text = null;
		if (AllMessagesReceived.WaitOne(0, exitContext: false))
		{
			return;
		}
		try
		{
			text = (string)headers["_genu"];
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			methodReturnMessage = (IMethodReturnMessage)binaryFormatter.Deserialize(stream);
			ParseResult(text, methodReturnMessage, null);
		}
		catch (Exception ex)
		{
			if (text != null)
			{
				ParseResult(text, null, ex);
			}
		}
	}

	public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
	{
		return null;
	}

	public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
	{
		responseHeaders = null;
		responseStream = null;
	}

	public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
	{
		return null;
	}

	public IMessage SyncProcessMessage(IMessage msg)
	{
		return null;
	}
}
