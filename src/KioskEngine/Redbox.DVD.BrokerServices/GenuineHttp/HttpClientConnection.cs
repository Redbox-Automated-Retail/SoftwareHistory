using System;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpClientConnection
{
	internal class ReconnectionContainer
	{
		public object SyncLock = new object();

		private bool _requestFailed = false;

		private int _reconnectionStarted = 0;

		private bool _isReconnectionStarted = false;

		public bool RequestFailed
		{
			get
			{
				lock (SyncLock)
				{
					return _requestFailed;
				}
			}
			set
			{
				lock (SyncLock)
				{
					_requestFailed = value;
				}
			}
		}

		public int ReconnectionStartedAt
		{
			get
			{
				lock (SyncLock)
				{
					return _reconnectionStarted;
				}
			}
			set
			{
				lock (SyncLock)
				{
					_reconnectionStarted = value;
				}
			}
		}

		public bool IsReconnectionStarted
		{
			get
			{
				lock (SyncLock)
				{
					return _isReconnectionStarted;
				}
			}
			set
			{
				lock (SyncLock)
				{
					if (value)
					{
						_isReconnectionStarted = true;
						_reconnectionStarted = GenuineUtility.TickCount;
					}
					else
					{
						_isReconnectionStarted = false;
						_reconnectionStarted = 0;
					}
				}
			}
		}
	}

	public HttpClientConnectionManager HttpClientConnectionManager;

	public byte[] HostId;

	public string HostIdAsString;

	public string ConnectionName;

	public int DbgConnectionId = ConnectionManager.GetUniqueConnectionId();

	public MessageContainer MessageContainer;

	public ITransportContext ITransportContext;

	public HostInformation Remote;

	public GenuineConnectionType GenuineConnectionType;

	private object _accessToLocalMembers = new object();

	public SecuritySession Sender_ConnectionLevelSecurity;

	public SecuritySession Listener_ConnectionLevelSecurity;

	public object SendingLock = new object();

	public bool IsSent = false;

	public ArrayList MessagesBeingSent = new ArrayList();

	public HttpWebRequest Sender;

	public GenuineChunkedStream SentContent;

	public byte[] Sender_SendBuffer;

	public byte[] Sender_ReceiveBuffer;

	public AsyncCallback OnEndSending;

	public int SendSequenceNo = 0;

	public ManualResetEvent SenderClosed = new ManualResetEvent(initialState: true);

	public int ListenerSequenceNo = 0;

	public HttpWebRequest Listener;

	public AsyncCallback OnEndReceiving;

	public byte[] Listener_ReceiveBuffer;

	public ManualResetEvent ListenerClosed = new ManualResetEvent(initialState: true);

	public ReconnectionContainer ListenerReconnectionLock = new ReconnectionContainer();

	public ReconnectionContainer SenderReconnectionLock = new ReconnectionContainer();

	internal bool _keepalive = false;

	private IWebProxy _iWebProxy;

	private string _userAgent;

	private string _mimeMediaType;

	private bool _useWebAuthentication = false;

	private bool _useUnsafeConnectionSharing = false;

	private ICredentials _credentials;

	private bool _allowWriteStreamBuffering = false;

	private int _lastMessageWasReceviedAt = GenuineUtility.TickCount;

	private int _lastMessageWasSentAt = GenuineUtility.TickCount;

	private bool __disposed = false;

	private object __disposedLock = new object();

	internal Exception _disposeReason = null;

	internal ReaderWriterLock _disposeLock = new ReaderWriterLock();

	private ConnectionStateSignaller _connectionStateSignaller;

	private object _connectionStateSignallerLock = new object();

	public int LastMessageWasReceviedAt
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _lastMessageWasReceviedAt;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_lastMessageWasReceviedAt = value;
			}
		}
	}

	public int LastMessageWasSentAt
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _lastMessageWasSentAt;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_lastMessageWasSentAt = value;
			}
		}
	}

	internal bool _disposed
	{
		get
		{
			lock (__disposedLock)
			{
				return __disposed;
			}
		}
		set
		{
			lock (__disposedLock)
			{
				__disposed = value;
			}
		}
	}

	public HttpClientConnection(ITransportContext iTransportContext, string connectionName)
	{
		HostId = iTransportContext.BinaryHostIdentifier;
		HostIdAsString = iTransportContext.HostIdentifier;
		ConnectionName = connectionName;
		ITransportContext = iTransportContext;
		HttpClientConnectionManager = (HttpClientConnectionManager)ITransportContext.ConnectionManager;
		MessageContainer = new MessageContainer(iTransportContext);
		Sender_SendBuffer = new byte[35000];
		Sender_ReceiveBuffer = new byte[35000];
		Listener_ReceiveBuffer = new byte[35000];
		OnEndSending = Callback_OnEndSending;
		OnEndReceiving = Callback_OnEndReceiving;
		_userAgent = ITransportContext.IParameterProvider[GenuineParameter.HttpWebUserAgent] as string;
		_mimeMediaType = ITransportContext.IParameterProvider[GenuineParameter.HttpMimeMediaType] as string;
		if (_userAgent == null)
		{
			_userAgent = string.Concat("Mozilla/4.0+ (compatible; MSIE 6.0; Windows ", Environment.OSVersion.Version, "; Genuine HTTP Client Channel; MS .NET CLR ", Environment.Version.ToString(), ")");
		}
		_userAgent = Regex.Replace(_userAgent, "\r|\n", " ", RegexOptions.None);
		_useUnsafeConnectionSharing = (bool)ITransportContext.IParameterProvider[GenuineParameter.HttpUnsafeConnectionSharing];
		_allowWriteStreamBuffering = (bool)ITransportContext.IParameterProvider[GenuineParameter.HttpAllowWriteStreamBuffering];
		_keepalive = (bool)iTransportContext.IParameterProvider[GenuineParameter.HttpKeepAlive];
		_credentials = ITransportContext.IParameterProvider[GenuineParameter.HttpAuthCredential] as ICredentials;
		string text = ITransportContext.IParameterProvider[GenuineParameter.HttpAuthUserName] as string;
		string text2 = ITransportContext.IParameterProvider[GenuineParameter.HttpAuthPassword] as string;
		string text3 = ITransportContext.IParameterProvider[GenuineParameter.HttpAuthDomain] as string;
		bool flag = (bool)ITransportContext.IParameterProvider[GenuineParameter.HttpUseDefaultCredentials];
		if (_credentials != null || (text != null && text != string.Empty && text2 != null && text2 != string.Empty) || flag)
		{
			_useWebAuthentication = true;
			if (_credentials == null)
			{
				if (flag)
				{
					_credentials = CredentialCache.DefaultCredentials;
				}
				else if (text3 == null)
				{
					_credentials = new NetworkCredential(text, text2);
				}
				else
				{
					_credentials = new NetworkCredential(text, text2, text3);
				}
			}
		}
		if ((bool)ITransportContext.IParameterProvider[GenuineParameter.HttpUseGlobalProxy])
		{
			_iWebProxy = GlobalProxySelection.Select;
		}
		else if (!(ITransportContext.IParameterProvider[GenuineParameter.HttpProxyUri] is string))
		{
			_iWebProxy = GlobalProxySelection.GetEmptyWebProxy();
		}
		else
		{
			_iWebProxy = new WebProxy((string)ITransportContext.IParameterProvider[GenuineParameter.HttpProxyUri], (bool)ITransportContext.IParameterProvider[GenuineParameter.HttpBypassOnLocal]);
		}
	}

	private void Callback_OnEndSending(IAsyncResult ar)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (ar.AsyncState != Sender)
			{
				binaryLogWriter?.WriteImplementationWarningEvent("HttpClientConnection.Callback_OnEndSending", LogMessageType.Warning, GenuineExceptions.Get_Debugging_GeneralWarning("The provided sender does not match the expected Sender instance."), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The provided sender does not match the expected Sender instance.");
			}
			HttpWebRequest httpWebRequest = (HttpWebRequest)ar.AsyncState;
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(ar);
			if (_disposed)
			{
				httpWebResponse.GetResponseStream().Close();
				httpWebResponse.Close();
			}
			else
			{
				HttpClientConnectionManager.Pool_Sender_OnEndSending(this, httpWebResponse);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "HttpClientConnection.Callback_OnEndSending", LogMessageType.LowLevelTransport_AsyncSendingCompleted, exception, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, null, null, null, null, "Exception occurred while completing an asynchronous sending.");
			}
			try
			{
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)ar.AsyncState;
				httpWebRequest2.Abort();
			}
			catch
			{
			}
			if (!_disposed)
			{
				HttpClientConnectionManager.StartReestablishingIfNecessary(this, exception, sender: true);
			}
		}
	}

	private void Callback_OnEndReceiving(IAsyncResult ar)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)ar.AsyncState;
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(ar);
			if (_disposed)
			{
				httpWebResponse.GetResponseStream().Close();
				httpWebResponse.Close();
			}
			else
			{
				ITransportContext.ConnectionManager.IncreaseBytesReceived((int)httpWebResponse.ContentLength);
				HttpClientConnectionManager.Listener_OnEndReceiving(this, httpWebResponse);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "HttpClientConnection.Callback_OnEndReceiving", LogMessageType.LowLevelTransport_AsyncReceivingCompleted, exception, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, null, null, null, null, "Exception occurred while completing an asynchronous receiving.");
			}
			try
			{
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)ar.AsyncState;
				httpWebRequest2.Abort();
			}
			catch
			{
			}
			if (!_disposed)
			{
				HttpClientConnectionManager.StartReestablishingIfNecessary(this, exception, sender: false);
			}
		}
	}

	public HttpWebRequest InitializeRequest(bool sender, bool keepAlive)
	{
		string text = (sender ? "SENDER" : "LISTENER");
		int num = Remote.Url.IndexOf(':');
		if (num <= 0)
		{
			throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(Remote.Url, "Incorrect URI.");
		}
		HttpWebRequest httpWebRequest = null;
		httpWebRequest = ((Remote.Url[num - 1] != 's') ? ((HttpWebRequest)WebRequest.Create("http" + Remote.Url.Substring(num) + '?' + Guid.NewGuid().ToString("N"))) : ((HttpWebRequest)WebRequest.Create("https" + Remote.Url.Substring(num) + '?' + Guid.NewGuid().ToString("N"))));
		httpWebRequest.AllowAutoRedirect = false;
		httpWebRequest.AllowWriteStreamBuffering = _useWebAuthentication || _allowWriteStreamBuffering || _iWebProxy != null;
		httpWebRequest.ContentType = "application/octet-stream";
		httpWebRequest.ConnectionGroupName = HostIdAsString + text;
		httpWebRequest.PreAuthenticate = _useWebAuthentication;
		httpWebRequest.UnsafeAuthenticatedConnectionSharing = _useUnsafeConnectionSharing;
		httpWebRequest.KeepAlive = keepAlive;
		httpWebRequest.Method = "POST";
		httpWebRequest.Pipelined = false;
		httpWebRequest.SendChunked = false;
		httpWebRequest.Proxy = _iWebProxy;
		httpWebRequest.UserAgent = _userAgent;
		httpWebRequest.MediaType = _mimeMediaType;
		httpWebRequest.Accept = "*/*";
		httpWebRequest.Headers.Set("Cache-Control", "no-cache");
		httpWebRequest.Headers.Set("Pragma", "no-cache");
		httpWebRequest.Expect = null;
		if (_credentials != null)
		{
			httpWebRequest.Credentials = _credentials;
			httpWebRequest.AllowWriteStreamBuffering = true;
		}
		return httpWebRequest;
	}

	public void Dispose(Exception reason)
	{
		if (_disposed)
		{
			return;
		}
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			_disposeReason = reason;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		try
		{
			Sender.Abort();
		}
		catch (Exception)
		{
		}
		try
		{
			Sender = InitializeRequest(sender: true, keepAlive: false);
			Sender.ContentLength = 0L;
			Sender.GetRequestStream().Close();
			Sender.Timeout = 300;
			HttpWebResponse httpWebResponse = (HttpWebResponse)Sender.GetResponse();
			httpWebResponse.GetResponseStream().Close();
			httpWebResponse.Close();
		}
		catch (Exception)
		{
		}
		if (GenuineConnectionType != GenuineConnectionType.Persistent)
		{
			return;
		}
		try
		{
			Listener.Abort();
		}
		catch (Exception)
		{
		}
		try
		{
			Listener = InitializeRequest(sender: false, keepAlive: false);
			Listener.ContentLength = 0L;
			Listener.GetRequestStream().Close();
			Listener.Timeout = 300;
			HttpWebResponse httpWebResponse2 = (HttpWebResponse)Listener.GetResponse();
			httpWebResponse2.GetResponseStream().Close();
			httpWebResponse2.Close();
		}
		catch (Exception)
		{
		}
	}

	private void On_ShutdownCompleted(IAsyncResult ar)
	{
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)ar.AsyncState;
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(ar);
			httpWebResponse.GetResponseStream().Close();
			httpWebResponse.Close();
		}
		catch (Exception)
		{
		}
	}

	public void SignalState(GenuineEventType genuineEventType, Exception reason, object additionalInfo)
	{
		lock (_connectionStateSignallerLock)
		{
			if (_connectionStateSignaller == null)
			{
				_connectionStateSignaller = new ConnectionStateSignaller(Remote, ITransportContext.IGenuineEventProvider);
			}
			_connectionStateSignaller.SetState(genuineEventType, reason, additionalInfo);
		}
	}
}
