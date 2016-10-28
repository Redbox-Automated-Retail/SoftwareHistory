using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public class GenuineUniversalServerTransportSink : IServerChannelSink, IChannelSinkBase
{
	public ITransportContext ITransportContext;

	private BasicChannelWithSecurity _channel;

	private IServerChannelSink _nextChannelSink;

	public IServerChannelSink NextChannelSink => _nextChannelSink;

	public IDictionary Properties => null;

	static GenuineUniversalServerTransportSink()
	{
		BinaryServerFormatterSinkProvider binaryServerFormatterSinkProvider = new BinaryServerFormatterSinkProvider();
		Type type = binaryServerFormatterSinkProvider.GetType().Assembly.GetType("System.Runtime.Remoting.Channels.CoreChannel");
		Type type2 = binaryServerFormatterSinkProvider.GetType().Assembly.GetType("System.IO.ByteBufferAllocator");
		FieldInfo field = type.GetField("_bufferPool", BindingFlags.Static | BindingFlags.NonPublic);
		ConstructorInfo constructor = type2.GetConstructor(new Type[1] { typeof(int) });
		field.SetValue(null, constructor.Invoke(new object[1] { 4096 }));
	}

	public GenuineUniversalServerTransportSink(BasicChannelWithSecurity channel, IServerChannelSink nextChannelSink, ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
		_channel = channel;
		_nextChannelSink = nextChannelSink;
	}

	public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
	{
		Message message = (Message)state;
		BinaryLogWriter binaryLogWriter = message.ITransportContext.BinaryLogWriter;
		Message message2 = new Message(message, headers, stream);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			string text = msg.Properties["__Uri"] as string;
			string text2 = BinaryLogWriter.ParseInvocationMethod(msg.Properties["__MethodName"] as string, msg.Properties["__TypeName"] as string);
			binaryLogWriter.WriteMessageCreatedEvent("GenuineUniversalServerTransportSink.AsyncProcessResponse", LogMessageType.MessageCreated, null, message2, false, message2.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message2.Stream : null, text, text2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "The response message has been created.");
			message2.ITransportHeaders["__genit"] = text;
			message2.ITransportHeaders["__genmn"] = text2;
			binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.AsyncProcessResponse", LogMessageType.MessageRequestInvoked, null, message2, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The .NET Remoting invocation has been performed.");
		}
		message.ITransportContext.ConnectionManager.Send(message2);
	}

	public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
	{
		return null;
	}

	public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
	{
		throw new NotSupportedException();
	}

	public void HandleIncomingMessage(Message message)
	{
		BinaryLogWriter binaryLogWriter = message.ITransportContext.BinaryLogWriter;
		try
		{
			ServerChannelSinkStack serverChannelSinkStack = new ServerChannelSinkStack();
			serverChannelSinkStack.Push(this, message);
			string applicationName = RemotingConfiguration.ApplicationName;
			if (applicationName != null)
			{
				string text = (string)message.ITransportHeaders["__RequestUri"];
				if (text.Length > applicationName.Length && text.StartsWith(applicationName))
				{
					int startIndex = applicationName.Length + ((text[applicationName.Length] == '/') ? 1 : 0);
					text = text.Substring(startIndex);
					message.ITransportHeaders["__RequestUri"] = text;
				}
			}
			message.ITransportHeaders["__CustomErrorsEnabled"] = false;
			message.ITransportHeaders["__IPAddress"] = ((message.Sender.PhysicalAddress is IPEndPoint) ? ((IPEndPoint)message.Sender.PhysicalAddress).Address : message.Sender.PhysicalAddress);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageRequestInvoking, null, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, 0, null, null, null, null, "The .NET Remoting request is being invoked.");
			}
			IMessage responseMsg;
			ITransportHeaders responseHeaders;
			Stream responseStream;
			switch (_nextChannelSink.ProcessMessage(serverChannelSinkStack, null, message.ITransportHeaders, message.Stream, out responseMsg, out responseHeaders, out responseStream))
			{
			case ServerProcessing.Complete:
			{
				Message message2 = new Message(message, responseHeaders, responseStream);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					string text2 = responseMsg.Properties["__Uri"] as string;
					string text3 = BinaryLogWriter.ParseInvocationMethod(responseMsg.Properties["__MethodName"] as string, responseMsg.Properties["__TypeName"] as string);
					binaryLogWriter.WriteMessageCreatedEvent("GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageCreated, null, message2, false, message2.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message2.Stream : null, text2, text3, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "The response message has been created.");
					message2.ITransportHeaders["__genit"] = text2;
					message2.ITransportHeaders["__genmn"] = text3;
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageRequestInvoked, null, message2, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The .NET Remoting invocation has been performed.");
				}
				message.ITransportContext.ConnectionManager.Send(message2);
				break;
			}
			case ServerProcessing.Async:
				break;
			case ServerProcessing.OneWay:
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageRequestInvoked, null, null, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "One-way .NET Remoting invocation has been performed. No response is available.");
				}
				break;
			}
		}
		catch (Exception ex)
		{
			try
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageRequestInvoking, ex, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The .NET Remoting request resulted in exception. The exception is being sent back.");
				}
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
				binaryFormatter.Serialize(genuineChunkedStream, ex);
				Message message3 = new Message(message, new TransportHeaders(), genuineChunkedStream);
				message3.ContainsSerializedException = true;
				ITransportContext.ConnectionManager.Send(message3);
			}
			catch (Exception exception)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineUniversalServerTransportSink.HandleIncomingMessage", LogMessageType.MessageRequestInvoking, exception, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The source exception cannot be sent over the network. Both exceptions are ignored.");
				}
			}
		}
	}
}
