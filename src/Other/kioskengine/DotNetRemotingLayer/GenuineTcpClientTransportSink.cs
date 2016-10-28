using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public class GenuineTcpClientTransportSink : BaseChannelSinkWithProperties, IClientChannelSink, IChannelSinkBase
{
	public ITransportContext ITransportContext;

	private Hashtable _properties = new Hashtable();

	protected string _recipientUri;

	public override ICollection Keys => _properties.Keys;

	public override object this[object key]
	{
		get
		{
			return _properties[key];
		}
		set
		{
			_properties[key] = value;
		}
	}

	public IClientChannelSink NextChannelSink => null;

	public GenuineTcpClientTransportSink(string url, ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
		_recipientUri = GenuineUtility.Parse(url, out var _);
		_properties["GC_URI"] = _recipientUri;
		_properties["GC_TC"] = iTransportContext;
		_properties["GC_TS"] = this;
	}

	public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
	{
		ITransportContext transportContext = ITransportContext;
		if (!GenuineUtility.CheckUrlOnConnectivity(_recipientUri))
		{
			transportContext = UriStorage.GetTransportContext(_recipientUri);
			if (transportContext == null)
			{
				transportContext = ITransportContext;
			}
		}
		Message message = Message.CreateOutcomingMessage(transportContext, msg, headers, stream, isSynchronous: false);
		message.Recipient = transportContext.KnownHosts[_recipientUri];
		IMethodMessage methodMessage = (IMethodMessage)msg;
		message.IsOneWay = RemotingServices.IsOneWay(methodMessage.MethodBase);
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			binaryLogWriter.WriteMessageCreatedEvent("GenuineTcpClientTransportSink.AsyncProcessRequest", LogMessageType.MessageCreated, null, message, true, message.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message.Stream : null, msg.Properties["__Uri"] as string, BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "Asynchronous .NET Remoting invocaiton is being initiated.");
			message.ITransportHeaders["__genit"] = msg.Properties["__Uri"] as string;
			message.ITransportHeaders["__genmn"] = BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string);
			binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineTcpClientTransportSink.AsyncProcessRequest", LogMessageType.MessageRequestInvoking, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, 0, null, null, null, null, "The .NET Remoting asynchronous invocation is being initiated.");
		}
		AsyncSinkStackResponseProcessor asyncSinkStackResponseProcessor = null;
		if (!message.IsOneWay)
		{
			asyncSinkStackResponseProcessor = new AsyncSinkStackResponseProcessor(transportContext, message, sinkStack);
			transportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, asyncSinkStackResponseProcessor);
		}
		try
		{
			transportContext.ConnectionManager.Send(message);
		}
		catch (Exception exceptionAsObject)
		{
			asyncSinkStackResponseProcessor.DispatchException(exceptionAsObject);
			throw;
		}
	}

	public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
	{
		throw new NotSupportedException();
	}

	public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
	{
		return null;
	}

	public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ITransportContext transportContext = ITransportContext;
		if (!GenuineUtility.CheckUrlOnConnectivity(_recipientUri))
		{
			transportContext = UriStorage.GetTransportContext(_recipientUri);
			if (transportContext == null)
			{
				transportContext = ITransportContext;
			}
		}
		Message message = Message.CreateOutcomingMessage(transportContext, msg, requestHeaders, requestStream, isSynchronous: true);
		try
		{
			message.Recipient = transportContext.KnownHosts[_recipientUri];
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteMessageCreatedEvent("GenuineTcpClientTransportSink.ProcessMessage", LogMessageType.MessageCreated, null, message, true, message.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message.Stream : null, msg.Properties["__Uri"] as string, BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "Synchronous .NET Remoting invocaiton is being initiated.");
				message.ITransportHeaders["__genit"] = msg.Properties["__Uri"] as string;
				message.ITransportHeaders["__genmn"] = BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string);
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineTcpClientTransportSink.ProcessMessage", LogMessageType.MessageRequestInvoking, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, 0, null, null, null, null, "The .NET Remoting synchronous invocation has been made.");
			}
			SyncSinkStackResponseProcessor syncSinkStackResponseProcessor = new SyncSinkStackResponseProcessor(transportContext, message);
			transportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, syncSinkStackResponseProcessor);
			message.CancelSending = syncSinkStackResponseProcessor.IsReceivedEvent;
			try
			{
				transportContext.ConnectionManager.Send(message);
			}
			catch (Exception exceptionAsObject)
			{
				if (syncSinkStackResponseProcessor.DispatchedException != null)
				{
					throw OperationException.WrapException(syncSinkStackResponseProcessor.DispatchedException);
				}
				syncSinkStackResponseProcessor.DispatchException(exceptionAsObject);
				throw;
			}
			if (!GenuineUtility.WaitOne(syncSinkStackResponseProcessor.IsReceivedEvent, GenuineUtility.GetMillisecondsLeft(message.FinishTime)))
			{
				throw GenuineExceptions.Get_Send_ServerDidNotReply();
			}
			if (syncSinkStackResponseProcessor.DispatchedException != null)
			{
				throw OperationException.WrapException(syncSinkStackResponseProcessor.DispatchedException);
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineTcpClientTransportSink.ProcessMessage", LogMessageType.MessageDispatched, null, syncSinkStackResponseProcessor.Response, syncSinkStackResponseProcessor.Response.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The .NET Remoting synchronous invocation has been completed.");
			}
			responseHeaders = syncSinkStackResponseProcessor.Response.ITransportHeaders;
			responseStream = syncSinkStackResponseProcessor.Response.Stream;
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineTcpClientTransportSink.ProcessMessage", LogMessageType.MessageDispatched, exception, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The exception is dispatched to the caller context.");
			}
			throw;
		}
	}
}
