using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

internal class SyncSinkStackResponseProcessor : IResponseProcessor, IDisposable
{
	private ITransportContext _iTransportContext;

	private Message _message;

	private bool _isDispatched = false;

	private ManualResetEvent _isReceivedEvent = new ManualResetEvent(initialState: false);

	private Message _response;

	private Exception _dispatchedException;

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

	public Message Response
	{
		get
		{
			lock (this)
			{
				return _response;
			}
		}
		set
		{
			lock (this)
			{
				_response = value;
			}
		}
	}

	public Exception DispatchedException
	{
		get
		{
			lock (this)
			{
				return _dispatchedException;
			}
		}
		set
		{
			lock (this)
			{
				_dispatchedException = value;
			}
		}
	}

	public HostInformation Remote => _message.Recipient;

	public bool IsShortInProcessing => true;

	public Message Message => _message;

	public SyncSinkStackResponseProcessor(ITransportContext iTransportContext, Message message)
	{
		_iTransportContext = iTransportContext;
		_message = message;
	}

	public void Dispose()
	{
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
			if (message.ContainsSerializedException)
			{
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					DispatchedException = (Exception)binaryFormatter.Deserialize(message.Stream);
				}
				catch (Exception dispatchedException)
				{
					DispatchedException = dispatchedException;
				}
			}
			else
			{
				Response = message;
			}
			IsReceivedEvent.Set();
		}
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
