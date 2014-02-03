using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Threading;
using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.GenuineHttp;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Receiving;

internal class GenuineReceivingHandler : MarshalByRefObject, IIncomingStreamHandler, ITimerConsumer
{
	public const int PING_MESSAGE_REPLYID = -3;

	private WaitCallback _waitCallback_InternalExecuteMessagewaitCallback;

	public readonly ITransportContext ITransportContext;

	public readonly ITransportUser DefaultTransportUser;

	private static Hashtable _responseHandlers = Hashtable.Synchronized(new Hashtable());

	private Hashtable _transportUsers = Hashtable.Synchronized(new Hashtable());

	public GenuineReceivingHandler(ITransportContext iTransportContext, ITransportUser defaultTransportUser)
	{
		ITransportContext = iTransportContext;
		DefaultTransportUser = defaultTransportUser;
		_waitCallback_InternalExecuteMessagewaitCallback = InternalExecuteMessage;
		TimerProvider.Attach(this);
	}

	public bool HandleMessage(Stream stream, HostInformation remote, GenuineConnectionType genuineConnectionType, string connectionName, int dbgConnectionId, bool useThisThread, IMessageRegistrator iMessageRegistrator, SecuritySession connectionLevelSecuritySession, HttpServerRequestResult httpServerRequestResult)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		string text = binaryReader.ReadString();
		Message message = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		SecuritySession securitySession = remote.GetSecuritySession(text, ITransportContext.IKeyStore);
		if (securitySession == null)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "GenuineReceivingHandler.HandleMessage", LogMessageType.SecuritySessionApplied, GenuineExceptions.Get_Security_ContextNotFound(text), message, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text, dbgConnectionId, 0, 0, 0, null, null, null, null, "The requested Security Session can not be constructed or established. The name of Security Session: {0}.", text);
			}
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionWasNotFound, GenuineExceptions.Get_Security_ContextNotFound(text), remote, null));
			return true;
		}
		stream = securitySession.Decrypt(stream);
		if (stream == null)
		{
			return true;
		}
		message = MessageCoder.Deserialize(stream, text);
		message.ConnectionName = connectionName;
		message.SecuritySessionParameters._connectionName = connectionName;
		message.SecuritySessionParameters._genuineConnectionType = genuineConnectionType;
		message.Sender = remote;
		message.ITransportContext = ITransportContext;
		message.ConnectionLevelSecuritySession = connectionLevelSecuritySession;
		message.HttpServerRequestResult = httpServerRequestResult;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "GenuineReceivingHandler.HandleMessage", LogMessageType.SecuritySessionApplied, null, message, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text, dbgConnectionId, 0, 0, 0, null, null, null, null, "The Security Session has been used for decrypting the message.");
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			bool flag = ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1;
			if (flag)
			{
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream();
				GenuineUtility.CopyStreamToStream(message.Stream, genuineChunkedStream);
				message.Stream = genuineChunkedStream;
			}
			binaryLogWriter.WriteMessageCreatedEvent("GenuineReceivingHandler.HandleMessage", LogMessageType.MessageReceived, null, message, message.ReplyToId > 0, remote, flag ? message.Stream : null, message.ITransportHeaders["__genit"] as string, message.ITransportHeaders["__genmn"] as string, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, connectionName, dbgConnectionId, connectionLevelSecuritySession?.SecuritySessionId ?? (-1), connectionLevelSecuritySession?.Name, securitySession?.SecuritySessionId ?? (-1), securitySession.Name, "The message has been received.");
		}
		if (message.ReplyToId == -3)
		{
			return true;
		}
		if (iMessageRegistrator != null && iMessageRegistrator.WasRegistered(remote.Uri, message.MessageId, message.ReplyToId))
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.HandleMessage", LogMessageType.MessageDispatched, GenuineExceptions.Get_Debugging_GeneralWarning("The message has been already processed."), message, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, securitySession.Name, dbgConnectionId, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The message has been already processed. Therefore, this message is ignored.");
			}
			return true;
		}
		if (message.ReplyToId > 0)
		{
			message.IsOneWay = true;
			if (!(_responseHandlers[message.ReplyToId] is IResponseProcessor responseProcessor))
			{
				return true;
			}
			if (responseProcessor.Message != null)
			{
				responseProcessor.Message.HasBeenAsnwered = true;
			}
			if (responseProcessor.IsShortInProcessing)
			{
				responseProcessor.ProcessRespose(message);
				if (responseProcessor.IsExpired(GenuineUtility.TickCount))
				{
					_responseHandlers.Remove(message.ReplyToId);
				}
				return true;
			}
		}
		if (useThisThread)
		{
			InternalExecuteMessage(message);
		}
		else
		{
			GenuineThreadPool.QueueUserWorkItem(_waitCallback_InternalExecuteMessagewaitCallback, message, longDuration: false);
		}
		return message.IsOneWay;
	}

	private void InternalExecuteMessage(object messageAsObject)
	{
		Message message = null;
		try
		{
			message = (Message)messageAsObject;
			if (message.ConnectionLevelSecuritySession == null)
			{
				HandleMessage_AfterCLSS(message);
			}
			else
			{
				message.ConnectionLevelSecuritySession.Invoke(message, connectionLevel: true);
			}
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.InternalExecuteMessage", LogMessageType.SecuritySessionApplied, exception, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, message.ConnectionLevelSecuritySession, null, -1, 0, 0, 0, null, null, null, null, "An exception occurred while executing the request.");
			}
		}
	}

	public void HandleMessage_AfterCLSS(Message message)
	{
		SecuritySession securitySession = message.Sender.GetSecuritySession(message.SecuritySessionParameters.Name, ITransportContext.IKeyStore);
		if (securitySession == null)
		{
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.HandleMessage_AfterCLSS", LogMessageType.SecuritySessionApplied, GenuineExceptions.Get_Security_ContextNotFound(message.SecuritySessionParameters.Name), message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, message.SecuritySessionParameters.Name, -1, 0, 0, 0, null, null, null, null, "The message can not be processed due to the raised exception.");
			}
			return;
		}
		ThreadDataSlotKeeper threadDataSlotKeeper = new ThreadDataSlotKeeper("__GC__CurrentMessage", message);
		try
		{
			if (message.HttpServerRequestResult != null && (bool)message.ITransportContext.IParameterProvider[GenuineParameter.HttpAuthentication] && message.HttpServerRequestResult.IPrincipal != null && message.HttpServerRequestResult.IPrincipal.Identity is WindowsIdentity && ((WindowsIdentity)message.HttpServerRequestResult.IPrincipal.Identity).Token != IntPtr.Zero)
			{
				WindowsImpersonationContext windowsImpersonationContext = null;
				IPrincipal principal = null;
				principal = Thread.CurrentPrincipal;
				try
				{
					windowsImpersonationContext = ((WindowsIdentity)message.HttpServerRequestResult.IPrincipal.Identity).Impersonate();
					Thread.CurrentPrincipal = message.HttpServerRequestResult.IPrincipal;
					securitySession.Invoke(message, connectionLevel: false);
					return;
				}
				finally
				{
					Thread.CurrentPrincipal = principal;
					windowsImpersonationContext.Undo();
				}
			}
			securitySession.Invoke(message, connectionLevel: false);
		}
		finally
		{
			threadDataSlotKeeper.Dispose();
		}
	}

	public void HandleMessage_Final(Message message)
	{
		ITransportUser transportUser = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (message.SecuritySessionParameters.RemoteTransportUser.Length > 0)
		{
			transportUser = _transportUsers[message.SecuritySessionParameters.RemoteTransportUser] as ITransportUser;
		}
		if (transportUser == null)
		{
			transportUser = DefaultTransportUser;
		}
		if (message.ReplyToId > 0)
		{
			if (!(_responseHandlers[message.ReplyToId] is IResponseProcessor responseProcessor))
			{
				_responseHandlers.Remove(message.ReplyToId);
				return;
			}
			if (!responseProcessor.IsExpired(GenuineUtility.TickCount))
			{
				responseProcessor.ProcessRespose(message);
			}
			if (responseProcessor.IsExpired(GenuineUtility.TickCount))
			{
				_responseHandlers.Remove(message.ReplyToId);
			}
			return;
		}
		switch (message.GenuineMessageType)
		{
		case GenuineMessageType.Ordinary:
			transportUser.HandleMessage(message);
			break;
		case GenuineMessageType.TrueBroadcast:
		case GenuineMessageType.BroadcastEngine:
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			IMessage message2 = null;
			try
			{
				message2 = (IMessage)binaryFormatter.Deserialize(message.Stream);
				IMethodReturnMessage methodReturnMessage = null;
				if (message.DestinationMarshalByRef is string)
				{
					CourtCollection.Find((string)message.DestinationMarshalByRef, out var receiver, out var sender);
					if (receiver == null)
					{
						ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.BroadcastUnknownCourtReceived, null, message.Sender, message.DestinationMarshalByRef));
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, GenuineExceptions.Get_Debugging_GeneralWarning("No objects are associated with the \"{0}\" court.", (string)message.DestinationMarshalByRef), message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The message is ignored because no objects are associated with the \"{0}\" court.", (string)message.DestinationMarshalByRef);
						}
						break;
					}
					if (message.ITransportHeaders["_genbg"] is string && UniqueCallTracer.Instance.WasGuidRegistered(RemotingServices.GetObjectUri(receiver) + (string)message.ITransportHeaders["_genbg"]))
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, GenuineExceptions.Get_Broadcast_CallHasAlreadyBeenMade(), message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "This message is ignored because this call has been already processed.");
						}
						methodReturnMessage = new ReturnMessage(GenuineExceptions.Get_Broadcast_CallHasAlreadyBeenMade(), (IMethodCallMessage)message2);
					}
					else
					{
						try
						{
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
							{
								binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, null, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The multicast message is being invoked on court \"{0}\".", (string)message.DestinationMarshalByRef);
							}
							methodReturnMessage = RemotingServices.ExecuteMessage(receiver, (IMethodCallMessage)message2);
						}
						catch (Exception ex)
						{
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
							{
								binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, ex, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An exception occurred while the message was being processed. The exception is dispatched back.");
							}
							methodReturnMessage = new ReturnMessage(ex, (IMethodCallMessage)message2);
						}
					}
					ITransportContext iTransportContext = ITransportContext;
					if (sender != null)
					{
						GenuineUtility.FetchChannelUriFromMbr(sender, out var uri, out iTransportContext);
						if (uri == null || iTransportContext == null)
						{
							iTransportContext = ITransportContext;
						}
						else
						{
							message.Sender = iTransportContext.KnownHosts[uri];
						}
					}
					Message message3 = CreateResponseToBroadcastMessage(message, methodReturnMessage, binaryFormatter);
					message3.DestinationMarshalByRef = GenuineUtility.FetchDotNetUriFromMbr(receiver);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						PutDownRecordAboutResponseCreation(binaryLogWriter, message3, methodReturnMessage, "The response to the \"true\" broadcast message has been created.");
					}
					iTransportContext.ConnectionManager.Send(message3);
					break;
				}
				MarshalByRefObject marshalByRefObject = null;
				if (message.DestinationMarshalByRef is MarshalByRefObject)
				{
					marshalByRefObject = (MarshalByRefObject)message.DestinationMarshalByRef;
					if (message.ITransportHeaders["_genbg"] is string && UniqueCallTracer.Instance.WasGuidRegistered(RemotingServices.GetObjectUri(marshalByRefObject) + (string)message.ITransportHeaders["_genbg"]))
					{
						Exception broadcast_CallHasAlreadyBeenMade = GenuineExceptions.Get_Broadcast_CallHasAlreadyBeenMade();
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, broadcast_CallHasAlreadyBeenMade, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "This message is ignored because this call has been already processed.");
						}
						methodReturnMessage = new ReturnMessage(broadcast_CallHasAlreadyBeenMade, (IMethodCallMessage)message2);
					}
					else
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, null, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The usual broadcast message is being invoked.");
						}
						methodReturnMessage = RemotingServices.ExecuteMessage(marshalByRefObject, (IMethodCallMessage)message2);
					}
					Message message4 = CreateResponseToBroadcastMessage(message, methodReturnMessage, binaryFormatter);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						PutDownRecordAboutResponseCreation(binaryLogWriter, message4, methodReturnMessage, "The response to the usual broadcast invocation has been created.");
					}
					message4.ITransportHeaders["_genu"] = message.ITransportHeaders["_genu"];
					ITransportContext.ConnectionManager.Send(message4);
					break;
				}
				Exception ex2 = GenuineExceptions.Get_Debugging_GeneralWarning("Can't process the object with type {0}", (message.DestinationMarshalByRef == null) ? "<null!>" : message.DestinationMarshalByRef.GetType().ToString());
				binaryLogWriter?.WriteImplementationWarningEvent("GenuineReceivingHandler.HandleMessage_Final", LogMessageType.CriticalError, ex2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The response with the {0} type cannot be processed.", message.DestinationMarshalByRef.GetType());
				throw ex2;
			}
			catch (Exception ex3)
			{
				try
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, ex3, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An exception occurred while the usual broadcast message was being processed. The exception is dispatched back.");
					}
					ReturnMessage returnMessage = new ReturnMessage(ex3, (IMethodCallMessage)message2);
					Message message5 = CreateResponseToBroadcastMessage(message, returnMessage, binaryFormatter);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						PutDownRecordAboutResponseCreation(binaryLogWriter, message5, returnMessage, "The response to the usual broadcast invocation has been created.");
					}
					ITransportContext.ConnectionManager.Send(message5);
					break;
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, exception, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An exception occurred again while the previous exception was being serialized. This request is ignored.");
					}
					break;
				}
			}
		}
		case GenuineMessageType.ExternalStreamConsumer:
			if (message.ReplyToId > 0)
			{
				IResponseProcessor responseProcessor2 = _responseHandlers[message.ReplyToId] as IResponseProcessor;
				if (!responseProcessor2.IsExpired(GenuineUtility.TickCount))
				{
					responseProcessor2.ProcessRespose(message);
				}
				if (responseProcessor2.IsExpired(GenuineUtility.TickCount))
				{
					_responseHandlers.Remove(message.ReplyToId);
				}
			}
			else
			{
				if (!(message.DestinationMarshalByRef is string text))
				{
					break;
				}
				if (!message.IsOneWay)
				{
					Stream stream = null;
					try
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.DXM] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, null, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The DXM message is being dispatched to the \"{0}\" entry.", text);
						}
						stream = ITransportContext.DirectExchangeManager.HandleRequest(message);
						if (stream == null)
						{
							stream = Stream.Null;
						}
						Message message6 = new Message(message, new TransportHeaders(), stream);
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
						{
							if (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1)
							{
								GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
								GenuineUtility.CopyStreamToStream(message6.Stream, genuineChunkedStream);
								message6.Stream = genuineChunkedStream;
							}
							binaryLogWriter.WriteMessageCreatedEvent("AsyncSinkStackResponseProcessor.ProcessRespose", LogMessageType.MessageCreated, null, message6, false, message6.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message6.Stream : null, text, text, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "A response to DXM request has been created.");
							message.ITransportHeaders["__genit"] = text;
							message.ITransportHeaders["__genmn"] = text;
							binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "AsyncSinkStackResponseProcessor.ProcessRespose", LogMessageType.MessageRequestInvoked, null, message6, message6.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, GenuineUtility.TickCount, 0, message.SeqNo, null, null, null, null, "The DXM invocation has been completed.");
						}
						message.ITransportContext.ConnectionManager.Send(message6);
						break;
					}
					catch (Exception ex4)
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.DXM] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.BroadcastEngine, "GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageRequestInvoking, ex4, message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The invocation of the DXM message directed to the \"{0}\" entry has resulted in exception.", text);
						}
						BinaryFormatter binaryFormatter2 = new BinaryFormatter();
						GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
						binaryFormatter2.Serialize(genuineChunkedStream2, ex4);
						Message message6 = new Message(message, new TransportHeaders(), genuineChunkedStream2);
						message6.ContainsSerializedException = true;
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.BroadcastEngine] > 0)
						{
							string text2 = text;
							string text3 = null;
							object serverService = ITransportContext.DirectExchangeManager.GetServerService(text);
							if (serverService != null)
							{
								text3 = serverService.GetType().FullName;
							}
							binaryLogWriter.WriteMessageCreatedEvent("GenuineReceivingHanlder.HandleMessage_Final", LogMessageType.MessageCreated, null, message6, false, message6.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? message6.Stream : null, text2, text3, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, "The response to DXM message directed to the \"{0}\" court has been created.", text);
							message6.ITransportHeaders["__genit"] = text2;
							message6.ITransportHeaders["__genmn"] = text3;
						}
						ITransportContext.ConnectionManager.Send(message6);
						break;
					}
				}
				ITransportContext.DirectExchangeManager.HandleRequest(message);
			}
			break;
		default:
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.HandleMessage_Final", LogMessageType.MessageDispatched, GenuineExceptions.Get_Debugging_GeneralWarning("Unknown message type."), message, message.Sender, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, message.SecuritySessionParameters.Name, -1, 0, 0, 0, null, null, null, null, "A message has an unknown type and can not be processed. Type: {0}", (int)message.GenuineMessageType);
			}
			break;
		}
	}

	internal void PutDownRecordAboutResponseCreation(BinaryLogWriter binaryLogWriter, Message reply, IMessage ret, string message, params object[] parameters)
	{
		string text = ret.Properties["__Uri"] as string;
		string text2 = BinaryLogWriter.ParseInvocationMethod(ret.Properties["__MethodName"] as string, ret.Properties["__TypeName"] as string);
		binaryLogWriter.WriteMessageCreatedEvent("GenuineReceivingHanlder.PutDownRecordAboutResponseCreation", LogMessageType.MessageCreated, null, reply, isSent: false, reply.Recipient, (ITransportContext.BinaryLogWriter[LogCategory.MessageProcessing] > 1) ? reply.Stream : null, text, text2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, -1, -1, null, -1, null, message, parameters);
		reply.ITransportHeaders["__genit"] = text;
		reply.ITransportHeaders["__genmn"] = text2;
	}

	private Message CreateResponseToBroadcastMessage(Message message, object returnMessage, BinaryFormatter binaryFormatter)
	{
		GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		binaryFormatter.Serialize(genuineChunkedStream, returnMessage);
		return new Message(message, new TransportHeaders(), genuineChunkedStream);
	}

	public void RegisterResponseProcessor(int replyId, IResponseProcessor iResponseProcessor)
	{
		_responseHandlers[replyId] = iResponseProcessor;
	}

	public bool IsHandlerAlive(int replyId)
	{
		if (!(_responseHandlers[replyId] is IResponseProcessor responseProcessor))
		{
			return false;
		}
		return !responseProcessor.IsExpired(GenuineUtility.TickCount);
	}

	public void RegisterTransportUser(string name, ITransportUser transportUser)
	{
		_transportUsers[name] = transportUser;
	}

	public void DispatchException(HostInformation hostInformation, Exception exception)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.DispatchException", LogMessageType.ExceptionDispatched, exception, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The exception is being dispatched back to the caller context.");
		}
		Hashtable hashtable = new Hashtable();
		lock (_responseHandlers.SyncRoot)
		{
			foreach (object responseHandler in _responseHandlers)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)responseHandler;
				IResponseProcessor responseProcessor = dictionaryEntry.Value as IResponseProcessor;
				bool flag = responseProcessor.Remote == hostInformation;
				if (!flag && responseProcessor.Remote != null && hostInformation != null)
				{
					flag = (responseProcessor.Remote.Url == hostInformation.Url && hostInformation.Url != null) || (responseProcessor.Remote.Uri == hostInformation.Uri && hostInformation.Uri != null);
				}
				if (flag)
				{
					hashtable[dictionaryEntry.Key] = responseProcessor;
				}
			}
			foreach (object item in hashtable)
			{
				DictionaryEntry dictionaryEntry2 = (DictionaryEntry)item;
				IResponseProcessor responseProcessor2 = dictionaryEntry2.Value as IResponseProcessor;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.DispatchException", LogMessageType.ExceptionDispatched, exception, responseProcessor2.Message, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The exception is being dispatched back to the caller context.");
				}
				responseProcessor2.DispatchException(OperationException.WrapException(exception));
				_responseHandlers.Remove(dictionaryEntry2.Key);
			}
		}
	}

	public void DispatchException(Message sourceMessage, Exception exception)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "GenuineReceivingHandler.DispatchException2", LogMessageType.ExceptionDispatched, exception, sourceMessage, sourceMessage.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The exception is being dispatched back to the caller context.");
		}
		if (_responseHandlers[sourceMessage.MessageId] is IResponseProcessor responseProcessor)
		{
			responseProcessor.DispatchException(OperationException.WrapException(exception));
			if (responseProcessor.IsExpired(GenuineUtility.TickCount))
			{
				_responseHandlers.Remove(sourceMessage.ReplyToId);
			}
		}
	}

	public void TimerCallback()
	{
		ArrayList arrayList = new ArrayList();
		Exception send_ServerDidNotReply = GenuineExceptions.Get_Send_ServerDidNotReply();
		int tickCount = GenuineUtility.TickCount;
		lock (_responseHandlers.SyncRoot)
		{
			foreach (object responseHandler in _responseHandlers)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)responseHandler;
				IResponseProcessor responseProcessor = (IResponseProcessor)dictionaryEntry.Value;
				if (responseProcessor.IsExpired(tickCount))
				{
					arrayList.Add(dictionaryEntry);
				}
			}
		}
		foreach (object item in arrayList)
		{
			DictionaryEntry dictionaryEntry2 = (DictionaryEntry)item;
			_responseHandlers.Remove(dictionaryEntry2.Key);
		}
		foreach (object item2 in arrayList)
		{
			DictionaryEntry dictionaryEntry3 = (DictionaryEntry)item2;
			IResponseProcessor responseProcessor2 = (IResponseProcessor)dictionaryEntry3.Value;
			GenuineThreadPool.QueueUserWorkItem(responseProcessor2.DispatchException, send_ServerDidNotReply, longDuration: false);
		}
	}
}
