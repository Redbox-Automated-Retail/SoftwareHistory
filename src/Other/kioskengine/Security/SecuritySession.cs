using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public abstract class SecuritySession : MarshalByRefObject
{
	private object _accessToLocalMembers = new object();

	public readonly HostInformation Remote;

	private string _name;

	private int _securitySessionId = Interlocked.Increment(ref _securitySessionMaxId);

	private static int _securitySessionMaxId = 0;

	private ManualResetEvent _failed = new ManualResetEvent(initialState: false);

	private Exception _reasonOfFailure;

	protected ManualResetEvent _isEstablished = new ManualResetEvent(initialState: false);

	protected SecuritySessionParameters _establishingSecuritySessionParameters;

	private ArrayList _awaitingMessages = new ArrayList();

	private object _awaitingMessagesLock = new object();

	public string Name => _name;

	public int SecuritySessionId => _securitySessionId;

	public bool IsEstablished => _isEstablished.WaitOne(0, exitContext: false);

	public ManualResetEvent Failed => _failed;

	public Exception ReasonOfFailure
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _reasonOfFailure;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_reasonOfFailure = value;
			}
		}
	}

	public ManualResetEvent IsEstablishedEvent => _isEstablished;

	public SecuritySession(string name, HostInformation remote)
	{
		_name = name;
		Remote = remote;
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession.SecuritySession", LogMessageType.SecuritySessionCreated, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, name, -1, 0, 0, 0, GetType().Name, null, null, null, "Security Session has been created.");
		}
	}

	protected virtual void SessionEstablished()
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession.SessionEstablished", LogMessageType.SecuritySessionEstablished, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, Name, -1, 0, 0, 0, null, null, null, null, "Security Session has been established.");
		}
		_isEstablished.Set();
		SendAssociatedMessages();
	}

	public abstract GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel);

	public void InitiateEstablishingSecuritySession(SecuritySessionParameters securitySessionParameters)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession.InitiateEstablishingSecuritySession", LogMessageType.SecuritySessionInitiated, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, Name, -1, 0, 0, 0, GetType().Name, Name, null, null, "Security Session establishing is initiated.");
		}
		Failed.Reset();
		_establishingSecuritySessionParameters = securitySessionParameters;
		GenuineThreadPool.QueueUserWorkItem(Internal_InitiateEstablishingSecuritySession, securitySessionParameters, longDuration: false);
	}

	private void Internal_InitiateEstablishingSecuritySession(object ignored)
	{
		try
		{
			if (!IsEstablished)
			{
				GenuineChunkedStream genuineChunkedStream = EstablishSession(Stream.Null, connectionLevel: false);
				if (genuineChunkedStream != null)
				{
					SendMessage(genuineChunkedStream);
				}
			}
		}
		catch (Exception ex)
		{
			DispatchException(ex);
			Remote.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionFailed, ex, Remote, this));
		}
	}

	protected void SendMessage(object streamAsObject)
	{
		Message message = new Message(Remote.ITransportContext, Remote, 0, new TransportHeaders(), Stream.Null);
		message.SecuritySessionParameters = _establishingSecuritySessionParameters;
		message.IsSynchronous = (bool)Remote.ITransportContext.IParameterProvider[GenuineParameter.HoldThreadDuringSecuritySessionEstablishing];
		message.SerializedContent = (Stream)streamAsObject;
		Remote.ITransportContext.ConnectionManager.Send(message);
	}

	public abstract void Encrypt(Stream input, GenuineChunkedStream output);

	public abstract Stream Decrypt(Stream input);

	public virtual void Invoke(Message message, bool connectionLevel)
	{
		if (connectionLevel)
		{
			message.ITransportContext.IIncomingStreamHandler.HandleMessage_AfterCLSS(message);
		}
		else
		{
			message.ITransportContext.IIncomingStreamHandler.HandleMessage_Final(message);
		}
	}

	protected GenuineChunkedStream CreateOutputStream()
	{
		GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
		binaryWriter.Write(Name);
		return genuineChunkedStream;
	}

	public void PutMessageToAwaitingQueue(Message message)
	{
		lock (_awaitingMessagesLock)
		{
			_awaitingMessages.Add(message);
		}
		if (IsEstablished)
		{
			SendAssociatedMessages();
		}
	}

	public void SendAssociatedMessages()
	{
		ArrayList arrayList = null;
		lock (_awaitingMessagesLock)
		{
			arrayList = _awaitingMessages;
			_awaitingMessages = new ArrayList();
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			Message message = (Message)arrayList[i];
			message.ITransportContext.ConnectionManager.Send(message);
		}
	}

	internal void DispatchException(Exception exception)
	{
		ReasonOfFailure = exception;
		Failed.Set();
		ArrayList arrayList = null;
		lock (_awaitingMessagesLock)
		{
			arrayList = _awaitingMessages;
			_awaitingMessages = new ArrayList();
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			Message message = (Message)arrayList[i];
			if (message.ReplyToId > 0)
			{
				message.ITransportContext.IIncomingStreamHandler.DispatchException(message, exception);
			}
		}
	}
}
