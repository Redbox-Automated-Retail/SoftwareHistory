using System;
using System.Runtime.Serialization.Formatters.Binary;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

internal class UniversalAsyncResponseProcessor : IResponseProcessor, IDisposable
{
	private Message _message;

	private bool _isDispatched = false;

	private StreamResponseEventHandler _streamResponseEventHandler;

	private IStreamResponseHandler _iStreamResponseHandler;

	public HostInformation Remote => _message.Recipient;

	public bool IsShortInProcessing => false;

	public Message Message => _message;

	public UniversalAsyncResponseProcessor(Message message, StreamResponseEventHandler streamResponseEventHandler, IStreamResponseHandler iStreamResponseHandler)
	{
		_message = message;
		_streamResponseEventHandler = streamResponseEventHandler;
		_iStreamResponseHandler = iStreamResponseHandler;
	}

	public void Dispose()
	{
		_message = null;
		_streamResponseEventHandler = null;
		_iStreamResponseHandler = null;
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
			Exception ex = (Exception)binaryFormatter.Deserialize(message.Stream);
			if (_streamResponseEventHandler != null)
			{
				_streamResponseEventHandler(ex, _message.Recipient, _message.Tag);
			}
			if (_iStreamResponseHandler != null)
			{
				_iStreamResponseHandler.HandleException(ex, _message.Recipient, _message.Tag);
			}
		}
		else
		{
			if (_streamResponseEventHandler != null)
			{
				_streamResponseEventHandler(message.Stream, _message.Recipient, _message.Tag);
			}
			if (_iStreamResponseHandler != null)
			{
				_iStreamResponseHandler.HandleResponse(message.Stream, _message.Recipient, _message.Tag);
			}
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
		if (_streamResponseEventHandler != null)
		{
			_streamResponseEventHandler(exceptionAsObject, _message.Recipient, _message.Tag);
		}
		if (_iStreamResponseHandler != null)
		{
			_iStreamResponseHandler.HandleException(exceptionAsObject as Exception, _message.Recipient, _message.Tag);
		}
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
