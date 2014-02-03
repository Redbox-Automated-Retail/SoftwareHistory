using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.DirectExchange;

internal class SyncResponseProcessorWithEvent : IResponseProcessor, IDisposable
{
	private Message _message;

	private bool _isDispatched = false;

	private ManualResetEvent _isReceivedEvent;

	public Message Response;

	public Exception DispatchedException;

	public ManualResetEvent IsReceivedEvent
	{
		get
		{
			lock (this)
			{
				return _isReceivedEvent;
			}
		}
		set
		{
			lock (this)
			{
				_isReceivedEvent = value;
			}
		}
	}

	public HostInformation Remote => _message.Sender;

	public bool IsShortInProcessing => false;

	public Message Message => _message;

	public SyncResponseProcessorWithEvent(Message message)
	{
		_message = message;
		IsReceivedEvent = EventPool.ObtainEvent();
	}

	public void Dispose()
	{
		lock (this)
		{
			EventPool.RecycleEvent(IsReceivedEvent);
			IsReceivedEvent = EventPool.GlobalStubEvent;
		}
	}

	public void ProcessRespose(Message message)
	{
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
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			DispatchedException = (Exception)binaryFormatter.Deserialize(message.Stream);
		}
		else
		{
			Response = message;
		}
		IsReceivedEvent.Set();
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
		DispatchedException = exceptionAsObject as Exception;
		IsReceivedEvent.Set();
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
