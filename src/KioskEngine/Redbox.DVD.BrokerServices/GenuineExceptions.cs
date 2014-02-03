using System;
using System.Runtime.Serialization;

namespace Belikov.GenuineChannels;

internal class GenuineExceptions
{
	[Serializable]
	public class TooLargePacketSize : OperationException
	{
		public TooLargePacketSize(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new TooLargePacketSize(OperationErrorMessage);
		}

		public TooLargePacketSize(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TooLargePacketSize(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SendTransportProblem : OperationException
	{
		public SendTransportProblem(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SendTransportProblem(OperationErrorMessage);
		}

		public SendTransportProblem(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SendTransportProblem(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ServerDidNotReply : OperationException
	{
		public ServerDidNotReply(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ServerDidNotReply(OperationErrorMessage);
		}

		public ServerDidNotReply(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ServerDidNotReply(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SendTimeout : OperationException
	{
		public SendTimeout(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SendTimeout(OperationErrorMessage);
		}

		public SendTimeout(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SendTimeout(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SendNoSender : OperationException
	{
		public SendNoSender(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SendNoSender(OperationErrorMessage);
		}

		public SendNoSender(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SendNoSender(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class DestinationIsUnreachable : OperationException
	{
		public DestinationIsUnreachable(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new DestinationIsUnreachable(OperationErrorMessage);
		}

		public DestinationIsUnreachable(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public DestinationIsUnreachable(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NoNamedConnectionFound : OperationException
	{
		public NoNamedConnectionFound(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NoNamedConnectionFound(OperationErrorMessage);
		}

		public NoNamedConnectionFound(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NoNamedConnectionFound(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class QueueIsOverloaded : OperationException
	{
		public QueueIsOverloaded(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new QueueIsOverloaded(OperationErrorMessage);
		}

		public QueueIsOverloaded(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public QueueIsOverloaded(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class CanNotConnectToRemoteHost : OperationException
	{
		public CanNotConnectToRemoteHost(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new CanNotConnectToRemoteHost(OperationErrorMessage);
		}

		public CanNotConnectToRemoteHost(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public CanNotConnectToRemoteHost(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class CanNotResolveHostName : OperationException
	{
		public CanNotResolveHostName(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new CanNotResolveHostName(OperationErrorMessage);
		}

		public CanNotResolveHostName(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public CanNotResolveHostName(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class RemoteHostDoesNotRespond : OperationException
	{
		public RemoteHostDoesNotRespond(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new RemoteHostDoesNotRespond(OperationErrorMessage);
		}

		public RemoteHostDoesNotRespond(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public RemoteHostDoesNotRespond(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class CanNotAcceptIncomingConnection : OperationException
	{
		public CanNotAcceptIncomingConnection(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new CanNotAcceptIncomingConnection(OperationErrorMessage);
		}

		public CanNotAcceptIncomingConnection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public CanNotAcceptIncomingConnection(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConnectionReestablished : OperationException
	{
		public ConnectionReestablished(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConnectionReestablished(OperationErrorMessage);
		}

		public ConnectionReestablished(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConnectionReestablished(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class LifetimeCycleEnded : OperationException
	{
		public LifetimeCycleEnded(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new LifetimeCycleEnded(OperationErrorMessage);
		}

		public LifetimeCycleEnded(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public LifetimeCycleEnded(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class IncorrectHostUpdate : OperationException
	{
		public IncorrectHostUpdate(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new IncorrectHostUpdate(OperationErrorMessage);
		}

		public IncorrectHostUpdate(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public IncorrectHostUpdate(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class IncorrectData : OperationException
	{
		public IncorrectData(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new IncorrectData(OperationErrorMessage);
		}

		public IncorrectData(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public IncorrectData(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class TooLargePortionToReceive : OperationException
	{
		public TooLargePortionToReceive(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new TooLargePortionToReceive(OperationErrorMessage);
		}

		public TooLargePortionToReceive(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TooLargePortionToReceive(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConnectionClosed : OperationException
	{
		public ConnectionClosed(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConnectionClosed(OperationErrorMessage);
		}

		public ConnectionClosed(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConnectionClosed(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NoServices : OperationException
	{
		public NoServices(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NoServices(OperationErrorMessage);
		}

		public NoServices(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NoServices(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NoServerChannel : OperationException
	{
		public NoServerChannel(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NoServerChannel(OperationErrorMessage);
		}

		public NoServerChannel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NoServerChannel(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ServerHasBeenRestarted : OperationException
	{
		public ServerHasBeenRestarted(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ServerHasBeenRestarted(OperationErrorMessage);
		}

		public ServerHasBeenRestarted(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ServerHasBeenRestarted(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NewSessionDetected : OperationException
	{
		public NewSessionDetected(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NewSessionDetected(OperationErrorMessage);
		}

		public NewSessionDetected(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NewSessionDetected(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConflictOfConnections : OperationException
	{
		public ConflictOfConnections(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConflictOfConnections(OperationErrorMessage);
		}

		public ConflictOfConnections(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConflictOfConnections(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class LogicError : OperationException
	{
		public LogicError(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new LogicError(OperationErrorMessage);
		}

		public LogicError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public LogicError(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NoSessionAvailable : OperationException
	{
		public NoSessionAvailable(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NoSessionAvailable(OperationErrorMessage);
		}

		public NoSessionAvailable(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NoSessionAvailable(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ThreadPoolLimitExceeded : OperationException
	{
		public ThreadPoolLimitExceeded(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ThreadPoolLimitExceeded(OperationErrorMessage);
		}

		public ThreadPoolLimitExceeded(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ThreadPoolLimitExceeded(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class TransportConnectionFailed : OperationException
	{
		public TransportConnectionFailed(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new TransportConnectionFailed(OperationErrorMessage);
		}

		public TransportConnectionFailed(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TransportConnectionFailed(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class HostResourcesReleased : OperationException
	{
		public HostResourcesReleased(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new HostResourcesReleased(OperationErrorMessage);
		}

		public HostResourcesReleased(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public HostResourcesReleased(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class IncorrectUrl : OperationException
	{
		public IncorrectUrl(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new IncorrectUrl(OperationErrorMessage);
		}

		public IncorrectUrl(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public IncorrectUrl(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class EndPointListened : OperationException
	{
		public EndPointListened(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new EndPointListened(OperationErrorMessage);
		}

		public EndPointListened(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public EndPointListened(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ClientDidNotReconnectWithinTimeOut : OperationException
	{
		public ClientDidNotReconnectWithinTimeOut(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ClientDidNotReconnectWithinTimeOut(OperationErrorMessage);
		}

		public ClientDidNotReconnectWithinTimeOut(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ClientDidNotReconnectWithinTimeOut(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class CanNotCreateOrOpenNamedEvent : OperationException
	{
		public CanNotCreateOrOpenNamedEvent(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new CanNotCreateOrOpenNamedEvent(OperationErrorMessage);
		}

		public CanNotCreateOrOpenNamedEvent(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public CanNotCreateOrOpenNamedEvent(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class NamedEventError : OperationException
	{
		public NamedEventError(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new NamedEventError(OperationErrorMessage);
		}

		public NamedEventError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NamedEventError(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class CanNotCreateOrOpenSharedMemory : OperationException
	{
		public CanNotCreateOrOpenSharedMemory(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new CanNotCreateOrOpenSharedMemory(OperationErrorMessage);
		}

		public CanNotCreateOrOpenSharedMemory(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public CanNotCreateOrOpenSharedMemory(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SharedMemoryError : OperationException
	{
		public SharedMemoryError(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SharedMemoryError(OperationErrorMessage);
		}

		public SharedMemoryError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SharedMemoryError(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SspiError : OperationException
	{
		public SspiError(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SspiError(OperationErrorMessage);
		}

		public SspiError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SspiError(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class SspiDidNotProvideRequestedFeature : OperationException
	{
		public SspiDidNotProvideRequestedFeature(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new SspiDidNotProvideRequestedFeature(OperationErrorMessage);
		}

		public SspiDidNotProvideRequestedFeature(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public SspiDidNotProvideRequestedFeature(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class InvalidParameter : OperationException
	{
		public InvalidParameter(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new InvalidParameter(OperationErrorMessage);
		}

		public InvalidParameter(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public InvalidParameter(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ReconnectionFailed : OperationException
	{
		public ReconnectionFailed(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ReconnectionFailed(OperationErrorMessage);
		}

		public ReconnectionFailed(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ReconnectionFailed(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConnectionGracefullyClosed : OperationException
	{
		public ConnectionGracefullyClosed(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConnectionGracefullyClosed(OperationErrorMessage);
		}

		public ConnectionGracefullyClosed(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConnectionGracefullyClosed(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConnectionShutDown : OperationException
	{
		public ConnectionShutDown(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConnectionShutDown(OperationErrorMessage);
		}

		public ConnectionShutDown(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConnectionShutDown(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ConnectionClosedAfterTimeout : OperationException
	{
		public ConnectionClosedAfterTimeout(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ConnectionClosedAfterTimeout(OperationErrorMessage);
		}

		public ConnectionClosedAfterTimeout(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ConnectionClosedAfterTimeout(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ChannelClosed : OperationException
	{
		public ChannelClosed(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ChannelClosed(OperationErrorMessage);
		}

		public ChannelClosed(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ChannelClosed(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ChannelDesynchronization : OperationException
	{
		public ChannelDesynchronization(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ChannelDesynchronization(OperationErrorMessage);
		}

		public ChannelDesynchronization(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ChannelDesynchronization(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ContextNotFound : OperationException
	{
		public ContextNotFound(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ContextNotFound(OperationErrorMessage);
		}

		public ContextNotFound(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ContextNotFound(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class ContextWasNotEstablished : OperationException
	{
		public ContextWasNotEstablished(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ContextWasNotEstablished(OperationErrorMessage);
		}

		public ContextWasNotEstablished(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ContextWasNotEstablished(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class PasswordKnowledgeIsNotProved : OperationException
	{
		public PasswordKnowledgeIsNotProved(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new PasswordKnowledgeIsNotProved(OperationErrorMessage);
		}

		public PasswordKnowledgeIsNotProved(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public PasswordKnowledgeIsNotProved(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class WrongSignature : OperationException
	{
		public WrongSignature(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new WrongSignature(OperationErrorMessage);
		}

		public WrongSignature(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public WrongSignature(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
		}
	}

	[Serializable]
	public class DebuggingGeneralWarning : OperationException
	{
		private string _stackTrace = string.Empty;

		public override string StackTrace => _stackTrace;

		public DebuggingGeneralWarning(OperationErrorMessage operationErrorMessage)
			: base(operationErrorMessage)
		{
		}

		public override object Clone()
		{
			return new ReconnectionFailed(OperationErrorMessage);
		}

		public DebuggingGeneralWarning(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public DebuggingGeneralWarning(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
			: base(objToFetchTypeFrom, errorSubIdentifier, userFriendlyMessage)
		{
			_stackTrace = Environment.StackTrace;
		}
	}

	private static IGenuineExceptionLocalizer _iGenuineExceptionLocalizer;

	private static object _genuineExceptionLocalizerLock = new object();

	public static IGenuineExceptionLocalizer IGenuineExceptionLocalizer
	{
		get
		{
			lock (_genuineExceptionLocalizerLock)
			{
				return _iGenuineExceptionLocalizer;
			}
		}
		set
		{
			lock (_genuineExceptionLocalizerLock)
			{
				_iGenuineExceptionLocalizer = value;
			}
		}
	}

	public static Exception Get_Send_TooLargePacketSize(int packetSize, int maxContentSize)
	{
		string text = "GenuineChannels.Exception.Send.TooLargePacketSize";
		return new TooLargePacketSize(null, text, GetLocalizedErrorOrDefaultText(text, "Transport sink error: too large packet size to send. Packet size: {0}. MaxContentSize: {1}. You should increase the MaxContentSize channel parameter or use the chunk-by-chunk delivery algorithm to avoid this exception.", packetSize, maxContentSize));
	}

	public static Exception Get_Send_TransportProblem()
	{
		string text = "GenuineChannels.Exception.Send.TransportProblem";
		return new SendTransportProblem(null, text, GetLocalizedErrorOrDefaultText(text, "Transport level error. Can not send the packet through the transport."));
	}

	public static Exception Get_Send_ServerDidNotReply()
	{
		string text = "GenuineChannels.Exception.Send.ServerDidNotReply";
		return new ServerDidNotReply(null, text, GetLocalizedErrorOrDefaultText(text, "The response has not been received within the specified period of time."));
	}

	public static Exception Get_Send_Timeout()
	{
		string text = "GenuineChannels.Exception.Send.Timeout";
		return new SendTimeout(null, text, GetLocalizedErrorOrDefaultText(text, "The message has not been sent within the specified period of time."));
	}

	public static Exception Get_Send_NoSender(string destination)
	{
		string text = "GenuineChannels.Exception.Send.NoSender";
		return new SendNoSender(null, text, GetLocalizedErrorOrDefaultText(text, "Connection to the remote host {0} has been closed.", destination));
	}

	public static Exception Get_Send_DestinationIsUnreachable(string destination)
	{
		string text = "GenuineChannels.Exception.Send.DestinationIsUnreachable";
		return new DestinationIsUnreachable(null, text, GetLocalizedErrorOrDefaultText(text, "Can not establish a connection to the remote host \"{0}\".", destination));
	}

	public static Exception Get_Send_NoNamedConnectionFound(string connectionName)
	{
		string text = "GenuineChannels.Exception.Send.NoNamedConnectionFound";
		return new NoNamedConnectionFound(null, text, GetLocalizedErrorOrDefaultText(text, "The requested connection is not found. Probably, it has been closed. The name of the requested connection: {0}", connectionName));
	}

	public static Exception Get_Send_QueueIsOverloaded(int maxQueuedItems, int currentlyQueuedItems, int maxTotalSize, int currentTotalSize)
	{
		string text = "GenuineChannels.Exception.Send.QueueIsOverloaded";
		return new QueueIsOverloaded(null, text, GetLocalizedErrorOrDefaultText(text, "The message queue has been overrun. Transport connection(s) to this remote host will be closed. MaxQueuedItems: {0}. The current number of queued messages: {1}. MaxTotalSize: {2}. The current size of the queue: {3}.", maxQueuedItems, currentlyQueuedItems, maxTotalSize, currentTotalSize));
	}

	public static Exception Get_Connect_CanNotConnectToRemoteHost(string url, string systemReason)
	{
		string text = "GenuineChannels.Exception.CanNotConnectToRemoteHost";
		return new CanNotConnectToRemoteHost(null, text, GetLocalizedErrorOrDefaultText(text, "Can not connect to the remote host \"{0}\". System error message: {1}.", url, systemReason));
	}

	public static Exception Get_Connect_CanNotResolveHostName(string hostName)
	{
		string text = "GenuineChannels.Exception.Connect.CanNotResolveHostName";
		return new CanNotResolveHostName(null, text, GetLocalizedErrorOrDefaultText(text, "Can not resolve the specified host name to the valid IP address. The specified host name: \"{0}\".", hostName));
	}

	public static Exception Get_Connect_RemoteHostDoesNotRespond()
	{
		string text = "GenuineChannels.Exception.Connect.RemoteHostDoesNotRespond";
		return new RemoteHostDoesNotRespond(null, text, GetLocalizedErrorOrDefaultText(text, "The remote host does not respond."));
	}

	public static Exception Get_Connect_CanNotAcceptIncomingConnection(string reason)
	{
		string text = "GenuineChannels.Exception.CanNotAcceptIncomingConnection";
		return new CanNotAcceptIncomingConnection(null, text, GetLocalizedErrorOrDefaultText(text, "Can not accept an incoming connection: \"{0}\"", reason));
	}

	public static Exception Get_Connect_ConnectionReestablished()
	{
		string text = "GenuineChannels.Exception.Connect.ConnectionReestablished";
		return new ConnectionReestablished(null, text, GetLocalizedErrorOrDefaultText(text, "The remote host has opened another connection and enforced connection reestablishing. The current established connection is being shut down."));
	}

	public static Exception Get_Connection_LifetimeCycleEnded()
	{
		string text = "GenuineChannels.Exception.Connection.LifetimeCycleEnded";
		return new LifetimeCycleEnded(null, text, GetLocalizedErrorOrDefaultText(text, "The invocation or one-way connection has been shut down according to Finite Automaton Transition Diagram."));
	}

	public static Exception Get_Host_IncorrectHostUpdate()
	{
		string text = "GenuineChannels.Exception.Receive.IncorrectHostUpdate";
		return new IncorrectHostUpdate(null, text, GetLocalizedErrorOrDefaultText(text, "It is impossible to update the client's URI or the server's URL if the connection is established."));
	}

	public static Exception Get_Receive_IncorrectData()
	{
		string text = "GenuineChannels.Exception.Receive.IncorrectData";
		return new IncorrectData(null, text, GetLocalizedErrorOrDefaultText(text, "The incoming stream is incorrect. Probably, the remote host uses an incorrect channel or an unexpected Security Session."));
	}

	public static Exception Get_Receive_IncorrectData(string reason)
	{
		string text = "GenuineChannels.Exception.Receive.IncorrectData";
		return new IncorrectData(null, text, GetLocalizedErrorOrDefaultText(text, "The incoming stream is incorrect. Probably, the remote host uses an incorrect channel or an unexpected Security Session. The reason of the error: {0}.", reason));
	}

	public static Exception Get_Receive_TooLargePortionToReceive()
	{
		string text = "GenuineChannels.Exception.Receive.TooLargePortionToReceive";
		return new TooLargePortionToReceive(null, text, GetLocalizedErrorOrDefaultText(text, "The client has sent a portion of data, which is too large. The server has declined the request because it requires a larger memory buffer than it is allowed. The better way to do it is to provide Marshal-By-Ref object (for example, Stream) and take small portions from it."));
	}

	public static Exception Get_Receive_Portion()
	{
		string text = "GenuineChannels.Exception.Receive.IncorrectData";
		return new IncorrectData(null, text, GetLocalizedErrorOrDefaultText(text, "Incorrect incoming stream. Only part of the expected data has been received. Probably, the connection has been unexpectedly closed."));
	}

	public static Exception Get_Receive_ConnectionClosed()
	{
		string text = "GenuineChannels.Exception.Receive.ConnectionClosed";
		return new ConnectionClosed(null, text, GetLocalizedErrorOrDefaultText(text, "The connection has been forcibly closed."));
	}

	public static Exception Get_Receive_ConnectionClosed(string reason)
	{
		string text = "GenuineChannels.Exception.Receive.ConnectionClosed";
		return new ConnectionClosed(null, text, GetLocalizedErrorOrDefaultText(text, "The connection has been forcibly closed. Specified reason: {0}.", reason));
	}

	public static Exception Get_Receive_NoServices(string destination)
	{
		string text = "GenuineChannels.Exception.Receive.NoServices";
		return new NoServices(null, text, GetLocalizedErrorOrDefaultText(text, "There are no services associated with the requested service name (\"{0}\").", destination));
	}

	public static Exception Get_Receive_NoServerChannel()
	{
		string text = "GenuineChannels.Exception.Receive.NoServerChannel";
		return new NoServerChannel(null, text, GetLocalizedErrorOrDefaultText(text, "Can not find a channel with the name \"ghttp\". You must create Genuine HTTP Server channel in order to receive client requests."));
	}

	public static Exception Get_Receive_ServerHasBeenRestared()
	{
		string text = "GenuineChannels.Exception.Receive.ServerHasBeenRestarted";
		return new ServerHasBeenRestarted(null, text, GetLocalizedErrorOrDefaultText(text, "The server application has been restarted. It is highly recommended to refresh all data recevied from the server."));
	}

	public static Exception Get_Receive_NewSessionDetected()
	{
		string text = "GenuineChannels.Exception.Receive.NewSessionDetected";
		return new ServerHasBeenRestarted(null, text, GetLocalizedErrorOrDefaultText(text, "The remote host has lost all information about us. It is recommended to resubscribe to all remote host's events."));
	}

	public static Exception Get_Receive_ConflictOfConnections()
	{
		string text = "GenuineChannels.Exception.Receive.ConflictOfConnections";
		return new ConflictOfConnections(null, text, GetLocalizedErrorOrDefaultText(text, "The server has reported about several connections with the same type opened by the same client. This connection will be terminated."));
	}

	public static Exception Get_Processing_LogicError(string reason)
	{
		string text = "GenuineChannels.Exception.Processing.LogicError";
		return new LogicError(null, text, GetLocalizedErrorOrDefaultText(text, "The logic assertion has failed: \"{0}\".", reason));
	}

	public static Exception Get_Processing_NoSessionAvailable()
	{
		string text = "GenuineChannels.Exception.Processing.NoSessionAvailable";
		return new NoSessionAvailable(null, text, GetLocalizedErrorOrDefaultText(text, "The session context is not established. Either it was a one-way message or you are not processing a client request."));
	}

	public static Exception Get_Processing_ThreadPoolLimitExceeded()
	{
		string text = "GenuineChannels.Exception.Processing.ThreadPoolLimitExceeded";
		return new ThreadPoolLimitExceeded(null, text, GetLocalizedErrorOrDefaultText(text, "The specified limit of threads has been exceeded."));
	}

	public static Exception Get_Processing_TransportConnectionFailed()
	{
		string text = "GenuineChannels.Exception.Processing.TransportConnectionFailed";
		return new TransportConnectionFailed(null, text, GetLocalizedErrorOrDefaultText(text, "The connection to the remote host has been closed."));
	}

	public static Exception Get_Processing_HostResourcesReleased(string uri, string url)
	{
		string text = "GenuineChannels.Exception.Processing.HostResourcesReleased";
		return new HostResourcesReleased(null, text, GetLocalizedErrorOrDefaultText(text, "All resources associated with the remote host have been released. URI: {0}. URL: {1}.", uri, url));
	}

	public static Exception Get_Server_IncorrectAddressToListen(string url)
	{
		if (url == null)
		{
			url = string.Empty;
		}
		string text = "GenuineChannels.Exception.Server.IncorrectUrl";
		return new IncorrectUrl(null, text, GetLocalizedErrorOrDefaultText(text, "An incorrect URI to listen to: \"{0}\".", url));
	}

	public static Exception Get_Server_EndPointIsAlreadyBeingListenedTo(string url)
	{
		string text = "GenuineChannels.Exception.Server.EndPointListened";
		return new EndPointListened(null, text, GetLocalizedErrorOrDefaultText(text, "The specified end point is already in use or it has not finished shutting down yet: \"{0}\".", url));
	}

	public static Exception Get_Channel_ClientDidNotReconnectWithinTimeOut(string url)
	{
		string text = "GenuineChannels.Exception.Server.ClientDidNotReconnectWithinTimeOut";
		return new ClientDidNotReconnectWithinTimeOut(null, text, GetLocalizedErrorOrDefaultText(text, "The client application {0} has not reconnected within the specified timeout. The information associated with that remote host will be released.", url));
	}

	public static Exception Get_Windows_CanNotCreateOrOpenNamedEvent(int errorCode)
	{
		string text = "GenuineChannels.Exception.Windows.CanNotCreateOrOpenNamedEvent";
		return new CanNotCreateOrOpenNamedEvent(null, text, GetLocalizedErrorOrDefaultText(text, "Can not create or open a global named event. Error code: {0}. Use the Visual C++ Error Lookup utility to see the text of the error message.", errorCode));
	}

	public static Exception Get_Windows_NamedEventError(int errorCode)
	{
		string text = "GenuineChannels.Exception.Windows.NamedEventError";
		return new NamedEventError(null, text, GetLocalizedErrorOrDefaultText(text, "An error has occurred while using a named event. Error code: {0}. Use the Visual C++ Error Lookup utility to see the text of the error message.", errorCode));
	}

	public static Exception Get_Windows_CanNotCreateOrOpenSharedMemory(int errorCode)
	{
		string text = "GenuineChannels.Exception.Windows.CanNotCreateOrOpenSharedMemory";
		return new CanNotCreateOrOpenSharedMemory(null, text, GetLocalizedErrorOrDefaultText(text, "Can not create or open a memory file mapping. Error code: {0}. Use the Visual C++ Error Lookup utility to see the text of the error message.", errorCode));
	}

	public static Exception Get_Windows_SharedMemoryError(int errorCode)
	{
		string text = "GenuineChannels.Exception.Windows.SharedMemoryError";
		return new SharedMemoryError(null, text, GetLocalizedErrorOrDefaultText(text, "Error has occurred while using shared memory. Error code: {0}. Use the Visual C++ Error Lookup utility to see the text of the error message.", errorCode));
	}

	public static Exception Get_Windows_SspiError(int errorCode)
	{
		string text = "GenuineChannels.Exception.Windows.SspiError";
		return new SspiError(null, text, GetLocalizedErrorOrDefaultText(text, "The SSPI API has failed with 0x{0:X} error code. Please, look at WinError.h or use the Visual C++ Error Lookup utility to see the text of the error message.", errorCode));
	}

	public static Exception Get_Windows_SspiDidNotProvideRequestedFeature(string feature)
	{
		string text = "GenuineChannels.Exception.Windows.SspiDidNotProvideRequestedFeature";
		return new SspiDidNotProvideRequestedFeature(null, text, GetLocalizedErrorOrDefaultText(text, "The SSPI package has not provided the requested feature: \"{0}\".", feature));
	}

	public static Exception Get_Channel_InvalidParameter(string parameterName)
	{
		string text = "GenuineChannels.Exception.Channel.InvalidParameter";
		return new InvalidParameter(null, text, GetLocalizedErrorOrDefaultText(text, "Invalid value of the \"{0}\" parameter. Please, refer to the Genuine Channels documentation.", parameterName));
	}

	public static Exception Get_Channel_ReconnectionFailed()
	{
		string text = "GenuineChannels.Exception.Channel.ReconnectionFailed";
		return new ReconnectionFailed(null, text, GetLocalizedErrorOrDefaultText(text, "The client channel has not reconnected to the server for the specified time and number of attempts. The channel will try to connect to the server again only when you invoke the SAO or CAO object of the remote host."));
	}

	public static Exception Get_Channel_ConnectionGracefullyClosed()
	{
		string text = "GenuineChannels.Exception.Channel.ConnectionGracefullyClosed";
		return new ConnectionGracefullyClosed(null, text, GetLocalizedErrorOrDefaultText(text, "The connection to the remote host has been closed according to a mutual agreement."));
	}

	public static Exception Get_Channel_ConnectionShutDown(Exception reason)
	{
		string text = "GenuineChannels.Exception.Channel.ConnectionShutDown";
		return new ConnectionShutDown(null, text, GetLocalizedErrorOrDefaultText(text, "The connection is forcibly closed because the ConnectionManager.ReleaseConnections method has been invoked. The specified reason: {0}.", reason.Message));
	}

	public static Exception Get_Channel_ConnectionClosedAfterTimeout()
	{
		string text = "GenuineChannels.Exception.Channel.ConnectionClosedAfterTimeout";
		return new ConnectionClosedAfterTimeout(null, text, GetLocalizedErrorOrDefaultText(text, "No messages have been received from the remote host for the specified time period. The connection is considered to be needless or broken."));
	}

	public static Exception Get_Channel_Closed()
	{
		string text = "GenuineChannels.Exception.Channel.Closed";
		return new ChannelClosed(null, text, GetLocalizedErrorOrDefaultText(text, "The connection(s) has been closed manually."));
	}

	public static Exception Get_Channel_Desynchronization()
	{
		string text = "GenuineChannels.Exception.Channel.Desynchronization";
		return new ChannelDesynchronization(null, text, GetLocalizedErrorOrDefaultText(text, "The incoming packet does not contain the correct packet identifier. It means that the connection is out of order and must be closed."));
	}

	public static Exception Get_Broadcast_DestinationTypeMustBeAnInterface()
	{
		string text = "GenuineChannels.Exception.Broadcast.DestinationTypeMustBeAnInterface";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "You must specify an interface while constructing a broadcast dispatcher."));
	}

	public static Exception Get_Broadcast_ClientSinkIsUnknown()
	{
		string text = "GenuineChannels.Exception.Broadcast.ClientSinkIsUnknown";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "If you want to use the MBR object obtained via a native channel, you should override the client sink and return it by request. Alternatively, you can explicitly specify the URI and Transport Context of the remote host while adding the transparent proxy to the Dispatcher."));
	}

	public static Exception Get_Broadcast_RemoteEndPointDidNotReplyForTimeOut()
	{
		string text = "GenuineChannels.Exception.Broadcast.RemoteEndPointDidNotReplyForTimeOut";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "The remote host has not replied to the message for the specified time."));
	}

	public static Exception Get_Broadcast_CallHasAlreadyBeenMade()
	{
		string text = "GenuineChannels.Exception.Broadcast.CallHasAlreadyBeenMade";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "The invocation has already been made on this recipient."));
	}

	public static Exception Get_Broadcast_HandlerUninitialized()
	{
		string text = "GenuineChannels.Exception.Broadcast.HandlerUninitialized";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "It is impossible to activate the asynchronous mode while the Dispatcher.BroadcastCallFinishedHandler property is not initialized."));
	}

	public static Exception Get_Security_ContextNotFound(string keyName)
	{
		string text = "GenuineChannels.Exception.Security.ContextNotFound";
		return new ContextNotFound(null, text, GetLocalizedErrorOrDefaultText(text, "The key provider with the name \"{0}\" is not found.", keyName));
	}

	public static Exception Get_Security_ContextWasNotEstablished(string keyName)
	{
		string text = "GenuineChannels.Exception.Security.ContextWasNotEstablished";
		return new ContextWasNotEstablished(null, text, GetLocalizedErrorOrDefaultText(text, "The Security Session can not be established due to an undetermined reason. Please, check the log files to reveal the source of the problem. Security Provider Name: {0}.", keyName));
	}

	public static Exception Get_Security_UnexpectedContext(string keyName, string expectedKeyName)
	{
		string text = "GenuineChannels.Exception.Security.UnexpectedContext";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "Remote host has requested the \"{0}\" security context while the expected security context is \"{1}\".", keyName));
	}

	public static Exception Get_Security_InvalidTimeoutParameter(string val)
	{
		string text = "GenuineChannels.Exception.Security.InvalidTimeoutParameter";
		return new OperationException(null, text, GetLocalizedErrorOrDefaultText(text, "Can not parse the timeout parameter: \"{0}\". Timeout parameter must contain the time in milliseconds: \"Timeout=240000\".", val));
	}

	public static Exception Get_Security_PasswordKnowledgeIsNotProved()
	{
		string text = "GenuineChannels.Exception.Security.PasswordKnowledgeIsNotProved";
		return new PasswordKnowledgeIsNotProved(null, text, GetLocalizedErrorOrDefaultText(text, "The knowledge of the password is not proved."));
	}

	public static Exception Get_Security_WrongSignature()
	{
		string text = "GenuineChannels.Exception.Security.WrongSignature";
		return new WrongSignature(null, text, GetLocalizedErrorOrDefaultText(text, "The message integrity checking failure: the message has a wrong signature."));
	}

	public static Exception Get_Debugging_GeneralWarning(string text, params object[] parameters)
	{
		string text2 = "GenuineChannels.Exception.Debugging.GeneralWarning";
		return new DebuggingGeneralWarning(null, text2, GetLocalizedErrorOrDefaultText(text2, text, parameters));
	}

	public static string GetLocalizedErrorOrDefaultText(string errorIdentifier, string defaultErrorMessage, params object[] errorMessageParameters)
	{
		lock (_genuineExceptionLocalizerLock)
		{
			string text = null;
			IGenuineExceptionLocalizer iGenuineExceptionLocalizer = _iGenuineExceptionLocalizer;
			try
			{
				if (iGenuineExceptionLocalizer != null)
				{
					text = iGenuineExceptionLocalizer.Localize(errorIdentifier, errorMessageParameters);
				}
			}
			catch
			{
			}
			if (text == null)
			{
				text = string.Format(defaultErrorMessage, errorMessageParameters);
			}
			return text;
		}
	}
}
