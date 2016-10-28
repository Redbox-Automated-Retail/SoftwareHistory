using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

[ComVisible(true)]
public class DirectExchangeManager : MarshalByRefObject
{
	public ITransportContext ITransportContext;

	private Hashtable _services = Hashtable.Synchronized(new Hashtable());

	private static Hashtable _globalServices = Hashtable.Synchronized(new Hashtable());

	public DirectExchangeManager(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
	}

	public IServerServiceEntry RegisterServerService(string entryName, IServerServiceEntry iServerServiceEntry)
	{
		IServerServiceEntry result = _services[entryName] as IServerServiceEntry;
		_services[entryName] = iServerServiceEntry;
		return result;
	}

	public IServerServiceEntry UnregisterServerService(string entryName)
	{
		IServerServiceEntry result = _services[entryName] as IServerServiceEntry;
		_services.Remove(entryName);
		return result;
	}

	public IServerServiceEntry GetServerService(string entryName)
	{
		return _services[entryName] as IServerServiceEntry;
	}

	public string[] GetListOfRegisteredServices()
	{
		lock (_services.SyncRoot)
		{
			string[] array = new string[_services.Count];
			_services.Keys.CopyTo(array, 0);
			return array;
		}
	}

	public static IServerServiceEntry RegisterGlobalServerService(string entryName, IServerServiceEntry iServerServiceEntry)
	{
		IServerServiceEntry result = _globalServices[entryName] as IServerServiceEntry;
		_globalServices[entryName] = iServerServiceEntry;
		return result;
	}

	public static IServerServiceEntry UnregisterGlobalServerService(string entryName)
	{
		IServerServiceEntry result = _globalServices[entryName] as IServerServiceEntry;
		_globalServices.Remove(entryName);
		return result;
	}

	public static string[] GetListOfGlobalRegisteredServices()
	{
		lock (_globalServices.SyncRoot)
		{
			string[] array = new string[_globalServices.Count];
			_globalServices.Keys.CopyTo(array, 0);
			return array;
		}
	}

	public Stream SendSync(HostInformation destination, string serviceName, Stream content)
	{
		Message message = new Message(ITransportContext, destination, 0, new TransportHeaders(), content);
		message.IsSynchronous = true;
		message.GenuineMessageType = GenuineMessageType.ExternalStreamConsumer;
		message.DestinationMarshalByRef = serviceName;
		SyncResponseProcessorWithEvent syncResponseProcessorWithEvent = new SyncResponseProcessorWithEvent(message);
		ITransportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, syncResponseProcessorWithEvent);
		ITransportContext.ConnectionManager.Send(message);
		int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(message.FinishTime);
		if (millisecondsLeft <= 0)
		{
			throw GenuineExceptions.Get_Send_ServerDidNotReply();
		}
		if (!syncResponseProcessorWithEvent.IsReceivedEvent.WaitOne(millisecondsLeft, exitContext: false))
		{
			throw GenuineExceptions.Get_Send_ServerDidNotReply();
		}
		if (syncResponseProcessorWithEvent.DispatchedException != null)
		{
			throw OperationException.WrapException(syncResponseProcessorWithEvent.DispatchedException);
		}
		return syncResponseProcessorWithEvent.Response.Stream;
	}

	public void SendAsync(HostInformation destination, string serviceName, Stream content, StreamResponseEventHandler streamResponseEventHandler)
	{
		SendAsync(destination, serviceName, content, streamResponseEventHandler, null);
	}

	public void SendAsync(HostInformation destination, string serviceName, Stream content, StreamResponseEventHandler streamResponseEventHandler, object tag)
	{
		Message message = new Message(ITransportContext, destination, 0, new TransportHeaders(), content);
		message.IsSynchronous = false;
		message.GenuineMessageType = GenuineMessageType.ExternalStreamConsumer;
		message.DestinationMarshalByRef = serviceName;
		message.Tag = tag;
		UniversalAsyncResponseProcessor iResponseProcessor = new UniversalAsyncResponseProcessor(message, streamResponseEventHandler, null);
		ITransportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, iResponseProcessor);
		ITransportContext.ConnectionManager.Send(message);
	}

	public void SendAsync(HostInformation destination, string serviceName, Stream content, IStreamResponseHandler iStreamResponseHandler)
	{
		SendAsync(destination, serviceName, content, iStreamResponseHandler, null);
	}

	public void SendAsync(HostInformation destination, string serviceName, Stream content, IStreamResponseHandler iStreamResponseHandler, object tag)
	{
		Message message = new Message(ITransportContext, destination, 0, new TransportHeaders(), content);
		message.IsSynchronous = false;
		message.GenuineMessageType = GenuineMessageType.ExternalStreamConsumer;
		message.DestinationMarshalByRef = serviceName;
		message.Tag = tag;
		UniversalAsyncResponseProcessor iResponseProcessor = new UniversalAsyncResponseProcessor(message, null, iStreamResponseHandler);
		ITransportContext.IIncomingStreamHandler.RegisterResponseProcessor(message.MessageId, iResponseProcessor);
		ITransportContext.ConnectionManager.Send(message);
	}

	public void SendOneWay(HostInformation destination, string serviceName, Stream content)
	{
		try
		{
			Message message = new Message(ITransportContext, destination, 0, new TransportHeaders(), content);
			message.IsOneWay = true;
			message.GenuineMessageType = GenuineMessageType.ExternalStreamConsumer;
			message.DestinationMarshalByRef = serviceName;
			ITransportContext.ConnectionManager.Send(message);
		}
		catch (Exception)
		{
		}
	}

	public Stream HandleRequest(Message message)
	{
		IServerServiceEntry serverServiceEntry = null;
		string text = message.DestinationMarshalByRef as string;
		if (text != null)
		{
			serverServiceEntry = _services[text] as IServerServiceEntry;
		}
		if (text != null && serverServiceEntry == null)
		{
			serverServiceEntry = _globalServices[text] as IServerServiceEntry;
		}
		if (serverServiceEntry == null)
		{
			message.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.UnknownServerService, null, message.Sender, text));
			throw GenuineExceptions.Get_Receive_NoServices(text);
		}
		return serverServiceEntry.HandleMessage(message.Stream, message.Sender);
	}
}
