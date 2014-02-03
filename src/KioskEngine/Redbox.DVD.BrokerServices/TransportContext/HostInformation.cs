using System;
using System.Collections;
using System.Threading;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.TransportContext;

internal class HostInformation : MarshalByRefObject, ISetSecuritySession, ISessionSupport, ITransportContextProvider
{
	private object _accessToLocalMembers = new object();

	private ITransportContext _iTransportContext;

	private string _uri;

	private object _uriLock = new object();

	private object _physicalAddress;

	private int _queueLength;

	private byte _protocolVersion = 3;

	private object _localPhysicalAddress;

	private string _url;

	private object _urlLock = new object();

	private int _expireTime = GenuineUtility.FurthestFuture;

	private bool _firstRenewing = true;

	private Hashtable _securitySessions = new Hashtable();

	private Hashtable _clientSession = Hashtable.Synchronized(new Hashtable());

	private GenuinePersistentConnectionState _genuinePersistentConnectionState = GenuinePersistentConnectionState.NotEstablished;

	internal object PersistentConnectionEstablishingLock = new object();

	private int _remoteHostUniqueIdentifier = -1;

	private int _localHostUniqueIdentifier = Interlocked.Increment(ref _CurrentUniqueIdentifier);

	private static int _CurrentUniqueIdentifier = 0;

	private bool _isDisposed = false;

	internal object DisposeLock = new object();

	private Exception _disposeReason;

	private SecuritySessionParameters _securitySessionParameters;

	private object _securitySessionParametersLock = new object();

	public ITransportContext ITransportContext => _iTransportContext;

	public string Uri
	{
		get
		{
			lock (_uriLock)
			{
				return _uri;
			}
		}
	}

	public object PhysicalAddress
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _physicalAddress;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_physicalAddress = value;
			}
		}
	}

	public int QueueLength
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _queueLength;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_queueLength = value;
			}
		}
	}

	public byte ProtocolVersion
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _protocolVersion;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_protocolVersion = value;
			}
		}
	}

	public object LocalPhysicalAddress
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _localPhysicalAddress;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_localPhysicalAddress = value;
			}
		}
	}

	public string Url
	{
		get
		{
			lock (_urlLock)
			{
				return _url;
			}
		}
	}

	public string PrimaryUri => (GenuinePersistentConnectionState == GenuinePersistentConnectionState.Opened) ? Url : Uri;

	public int ExpireTime
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _expireTime;
			}
		}
	}

	public object this[object key]
	{
		get
		{
			return _clientSession[key];
		}
		set
		{
			if (value == null)
			{
				_clientSession.Remove(key);
			}
			else
			{
				_clientSession[key] = value;
			}
		}
	}

	public GenuinePersistentConnectionState GenuinePersistentConnectionState
	{
		get
		{
			return _genuinePersistentConnectionState;
		}
		set
		{
			_genuinePersistentConnectionState = value;
		}
	}

	public int RemoteHostUniqueIdentifier
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _remoteHostUniqueIdentifier;
			}
		}
	}

	public int LocalHostUniqueIdentifier
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _localHostUniqueIdentifier;
			}
		}
	}

	public bool IsDisposed
	{
		get
		{
			lock (DisposeLock)
			{
				return _isDisposed;
			}
		}
	}

	public Exception DisposeReason
	{
		get
		{
			lock (DisposeLock)
			{
				return _disposeReason;
			}
		}
		set
		{
			lock (DisposeLock)
			{
				_disposeReason = value;
			}
		}
	}

	public SecuritySessionParameters SecuritySessionParameters
	{
		get
		{
			lock (_securitySessionParametersLock)
			{
				return _securitySessionParameters;
			}
		}
		set
		{
			lock (_securitySessionParametersLock)
			{
				_securitySessionParameters = value;
			}
		}
	}

	internal HostInformation(string uriOrUrl, ITransportContext iTransportContext)
	{
		_iTransportContext = iTransportContext;
		if (uriOrUrl[0] == '_')
		{
			_uri = uriOrUrl;
			UriStorage.RegisterConnection(uriOrUrl, ITransportContext);
		}
		else
		{
			_url = uriOrUrl;
		}
	}

	public Exception UpdateUri(string uri, int remoteHostUniqueIdentifier)
	{
		return UpdateUri(uri, remoteHostUniqueIdentifier, checkHostInfoVersion: true);
	}

	public Exception UpdateUri(string uri, int remoteHostUniqueIdentifier, bool checkHostInfoVersion)
	{
		Exception ex = null;
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		lock (_uriLock)
		{
			if (_uri != uri)
			{
				if (_uri != null && _uri != uri)
				{
					ex = GenuineExceptions.Get_Receive_ServerHasBeenRestared();
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.HostInformation, "HostInformation.UpdateUri", LogMessageType.HostInformationUriUpdated, ex, null, this, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, _uri, uri, null, null, "The URI mismatch has been detected. The remote host has been restarted. Expected uri: \"{0}\". Provided uri: \"{1}\".", _uri, uri);
					}
					throw ex;
				}
				_uri = uri;
				UriStorage.RegisterConnection(uri, ITransportContext);
				ITransportContext.KnownHosts.UpdateHost(uri, this);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.HostInformation, "HostInformation.UpdateUri", LogMessageType.HostInformationUriUpdated, null, null, this, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, _uri, uri, null, null, "The URI of the HostInformation has been updated. Provided uri: \"{0}\".", _uri);
				}
			}
			if ((checkHostInfoVersion && RemoteHostUniqueIdentifier != remoteHostUniqueIdentifier) || (_uri != null && _uri != uri))
			{
				if (RemoteHostUniqueIdentifier != -1)
				{
					ex = GenuineExceptions.Get_Receive_NewSessionDetected();
					lock (_securitySessions)
					{
						foreach (object securitySession2 in _securitySessions)
						{
							DictionaryEntry dictionaryEntry = (DictionaryEntry)securitySession2;
							SecuritySession securitySession = (SecuritySession)dictionaryEntry.Value;
							securitySession.DispatchException(ex);
						}
						_securitySessions = new Hashtable();
					}
					ITransportContext.IIncomingStreamHandler.DispatchException(this, ex);
				}
				_remoteHostUniqueIdentifier = remoteHostUniqueIdentifier;
				ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralNewSessionDetected, null, this, null));
			}
		}
		return ex;
	}

	public void UpdateUrl(string url)
	{
		lock (_urlLock)
		{
			if (!(_url == url))
			{
				_url = url;
				ITransportContext.KnownHosts.UpdateHost(url, this);
			}
		}
	}

	public void Renew(int timeSpan, bool canMakeShorter)
	{
		if (_firstRenewing)
		{
			_firstRenewing = false;
			canMakeShorter = true;
		}
		lock (DisposeLock)
		{
			if (IsDisposed)
			{
				throw OperationException.WrapException(DisposeReason);
			}
			lock (_accessToLocalMembers)
			{
				int timeout = GenuineUtility.GetTimeout(timeSpan);
				if (canMakeShorter || GenuineUtility.IsTimeoutExpired(_expireTime, timeout))
				{
					_expireTime = timeout;
				}
			}
		}
	}

	public SecuritySession GetSecuritySession(string securitySessionName, IKeyStore keyStore)
	{
		lock (DisposeLock)
		{
			if (IsDisposed)
			{
				throw OperationException.WrapException(DisposeReason);
			}
			lock (_securitySessions)
			{
				SecuritySession securitySession = _securitySessions[securitySessionName] as SecuritySession;
				if (securitySession == null)
				{
					IKeyProvider key = keyStore.GetKey(securitySessionName);
					if (key == null)
					{
						throw GenuineExceptions.Get_Security_ContextNotFound(securitySessionName);
					}
					securitySession = key.CreateSecuritySession(securitySessionName, this);
					_securitySessions[securitySessionName] = securitySession;
				}
				return securitySession;
			}
		}
	}

	public void DestroySecuritySession(string securitySessionName)
	{
		lock (DisposeLock)
		{
			if (IsDisposed)
			{
				throw OperationException.WrapException(DisposeReason);
			}
			lock (_securitySessions)
			{
				if (_securitySessions[securitySessionName] is SecuritySession securitySession)
				{
					if (securitySession is IDisposable disposable)
					{
						disposable.Dispose();
					}
					BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Security, "HostInformation.DestroySecuritySession", LogMessageType.SecuritySessionDestroyed, null, null, securitySession.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, securitySession.Name, -1, 0, 0, 0, securitySession.GetType().Name, securitySession.Name, null, null, "Security Session has been destroyed manually.");
					}
					_securitySessions.Remove(securitySessionName);
				}
			}
		}
	}

	internal bool Dispose(Exception reason)
	{
		if (IsDisposed)
		{
			return false;
		}
		lock (DisposeLock)
		{
			if (IsDisposed)
			{
				return false;
			}
			_isDisposed = true;
			DisposeReason = reason;
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
			{
				string text = string.Empty;
				try
				{
					text = Environment.StackTrace;
				}
				catch
				{
				}
				binaryLogWriter.WriteEvent(LogCategory.HostInformation, "HostInformation.Dispose", LogMessageType.HostInformationReleased, reason, null, this, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Uri, Url, null, null, "The HostInformation is released. The caller: {0}.", text);
			}
			lock (_securitySessions)
			{
				foreach (object securitySession in _securitySessions)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)securitySession;
					if (dictionaryEntry.Value is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
				_securitySessions.Clear();
			}
		}
		return true;
	}

	public override string ToString()
	{
		return string.Format("HOST (URL: {0} URI: {1} LocalNo: {2} RemoteNo: {3})", (Url == null) ? "<none>" : Url, (Uri == null) ? "<none>" : Uri, LocalHostUniqueIdentifier, RemoteHostUniqueIdentifier);
	}
}
