using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public class AsyncSinkStackResponseProcessor : IResponseProcessor, IDisposable
{
	private ITransportContext _iTransportContext;

	private Message _message;

	private IClientChannelSinkStack _iClientChannelSinkStack;

	private bool _isDispatched = false;

	public HostInformation Remote => _message.Recipient;

	public bool IsShortInProcessing => false;

	public Message Message => _message;

	public AsyncSinkStackResponseProcessor(ITransportContext iTransportContext, Message message, IClientChannelSinkStack iClientChannelSinkStack)
	{
		_iTransportContext = iTransportContext;
		_message = message;
		_iClientChannelSinkStack = iClientChannelSinkStack;
	}

	public void Dispose()
	{
	}

	public void ProcessRespose(Message message)
	{
		BinaryLogWriter binaryLogWriter = _iTransportContext.BinaryLogWriter;
		lock (this)
		{
			if (_isDispatched)
			{
				return;
			}
			_isDispatched = true;
		}
		if (message.ContainsSerializedException)
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				Exception ex = (Exception)binaryFormatter.Deserialize(message.Stream);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "AsyncSinkStackResponseProcessor.ProcessRespose", LogMessageType.MessageDispatched, ex, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, _message.SeqNo, null, null, null, null, "The exception received as a response to the asynchronous message is dispatched back to the caller context.");
				}
				_iClientChannelSinkStack.DispatchException(ex);
				return;
			}
			catch (Exception ex2)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "AsyncSinkStackResponseProcessor.ProcessRespose", LogMessageType.MessageDispatched, ex2, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, _message.SeqNo, null, null, null, null, "The received exception cannot be deserialized. This exception is dispatched instead.");
				}
				_iClientChannelSinkStack.DispatchException(ex2);
				return;
			}
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "AsyncSinkStackResponseProcessor.ProcessRespose", LogMessageType.MessageDispatched, null, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, _message.SeqNo, null, null, null, null, "The response to the asynchronous message is dispatched back to the caller context.");
		}
		_iClientChannelSinkStack.AsyncProcessResponse(message.ITransportHeaders, message.Stream);
	}

	public void DispatchException(object exceptionAsObject)
	{
		lock (this)
		{
			if (_isDispatched)
			{
				return;
			}
			_isDispatched = true;
		}
		_iClientChannelSinkStack.DispatchException(exceptionAsObject as Exception);
	}

	public bool IsExpired(int now)
	{
		lock (this)
		{
			if (_isDispatched)
			{
				return true;
			}
		}
		if (GenuineUtility.IsTimeoutExpired(_message.FinishTime, now))
		{
			return true;
		}
		return false;
	}
}
