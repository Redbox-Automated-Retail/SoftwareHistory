using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpInvocationConnection
{
	private class ConnectionInfo
	{
		public HttpWebRequest HttpWebRequest;

		public int Index;

		public Message Message;

		public ConnectionInfo(HttpWebRequest httpWebRequest, int index, Message message)
		{
			HttpWebRequest = httpWebRequest;
			Index = index;
			Message = message;
		}
	}

	private int _httpAsynchronousRequestTimeout;

	public byte[] HostId;

	public string HostIdAsString;

	public ITransportContext ITransportContext;

	public HostInformation Remote;

	private object _accessToLocalMembers = new object();

	private AsyncCallback _asyncCallback_onRequestCompleted;

	private int _hostRenewingSpan;

	private ArrayList _availableConnections = new ArrayList();

	internal bool _keepalive = false;

	private IWebProxy _iWebProxy;

	private string _userAgent;

	private bool _useWebAuthentication = false;

	private bool _useUnsafeConnectionSharing = false;

	private ICredentials _credentials;

	private bool _allowWriteStreamBuffering = false;

	public HttpInvocationConnection(ITransportContext iTransportContext, HostInformation remote)
	{
		HostId = iTransportContext.BinaryHostIdentifier;
		HostIdAsString = iTransportContext.HostIdentifier;
		_asyncCallback_onRequestCompleted = OnRequestCompleted;
		ITransportContext = iTransportContext;
		Remote = remote;
		_userAgent = ITransportContext.IParameterProvider[GenuineParameter.HttpWebUserAgent] as string;
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
		_hostRenewingSpan = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]) + GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
		if (_credentials != null || (text != null && text != string.Empty && text2 != null && text2 != string.Empty))
		{
			_useWebAuthentication = true;
			if (_credentials == null)
			{
				if ((bool)ITransportContext.IParameterProvider[GenuineParameter.HttpUseDefaultCredentials])
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
		_httpAsynchronousRequestTimeout = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.HttpAsynchronousRequestTimeout]);
	}

	public void SendMessage(Message message)
	{
		int index = 0;
		HttpWebRequest httpWebRequest = null;
		try
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
			BufferKeeper bufferKeeper = new BufferKeeper(0);
			try
			{
				MessageCoder.FillInLabelledStream(message, null, null, genuineChunkedStream, bufferKeeper.Buffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
			}
			finally
			{
				bufferKeeper.Dispose();
			}
			GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream2);
			HttpMessageCoder.WriteRequestHeader(binaryWriter, 3, GenuineConnectionType.Invocation, ITransportContext.BinaryHostIdentifier, HttpPacketType.Usual, message.MessageId, string.Empty, Remote.LocalHostUniqueIdentifier);
			if (genuineChunkedStream.CanSeek)
			{
				genuineChunkedStream2.WriteStream(genuineChunkedStream);
			}
			else
			{
				GenuineUtility.CopyStreamToStream(genuineChunkedStream, genuineChunkedStream2);
			}
			index = FindAvailableConnectionEntry();
			httpWebRequest = InitializeRequest("__GC_INVC_" + index, _keepalive);
			InitiateSending(new ConnectionInfo(httpWebRequest, index, message), genuineChunkedStream2, _httpAsynchronousRequestTimeout);
		}
		catch
		{
			try
			{
				httpWebRequest?.Abort();
			}
			catch
			{
			}
			ReleaseConnectionEntry(index);
			throw;
		}
	}

	private void InitiateSending(ConnectionInfo connectionInfo, Stream stream, int timeout)
	{
		Stream stream2 = null;
		try
		{
			connectionInfo.HttpWebRequest.ContentLength = stream.Length;
			stream2 = connectionInfo.HttpWebRequest.GetRequestStream();
			GenuineUtility.CopyStreamToStream(stream, stream2, (int)stream.Length);
			HttpWebRequestCop httpWebRequestCop = new HttpWebRequestCop(connectionInfo.HttpWebRequest, _asyncCallback_onRequestCompleted, connectionInfo, _httpAsynchronousRequestTimeout);
		}
		finally
		{
			stream2?.Close();
		}
	}

	private void OnRequestCompleted(IAsyncResult ar)
	{
		HttpWebResponse httpWebResponse = null;
		Stream stream = null;
		ConnectionInfo connectionInfo = null;
		try
		{
			connectionInfo = (ConnectionInfo)ar.AsyncState;
			HttpWebRequest httpWebRequest = connectionInfo.HttpWebRequest;
			httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(ar);
			Remote.Renew(_hostRenewingSpan, canMakeShorter: false);
			stream = httpWebResponse.GetResponseStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			HttpMessageCoder.ReadResponseHeader(binaryReader, out var _, out var _, out var httpPacketType, out var _);
			if (httpPacketType == HttpPacketType.ClosedManually || httpPacketType == HttpPacketType.Desynchronization || httpPacketType == HttpPacketType.SenderError)
			{
				throw GenuineExceptions.Get_Receive_ConnectionClosed();
			}
			if (binaryReader.ReadByte() != 0)
			{
				if (connectionInfo.Message.IsOneWay)
				{
				}
				return;
			}
			BufferKeeper bufferKeeper = new BufferKeeper(0);
			try
			{
				using LabelledStream inputStream = new LabelledStream(ITransportContext, stream, bufferKeeper.Buffer);
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
				GenuineUtility.CopyStreamToStream(inputStream, genuineChunkedStream);
				ITransportContext.IIncomingStreamHandler.HandleMessage(genuineChunkedStream, Remote, GenuineConnectionType.Invocation, string.Empty, -1, useThisThread: true, null, null, null);
			}
			finally
			{
				bufferKeeper.Dispose();
			}
		}
		catch (Exception exception)
		{
			if (connectionInfo != null)
			{
				ITransportContext.IIncomingStreamHandler.DispatchException(connectionInfo.Message, exception);
			}
		}
		finally
		{
			stream?.Close();
			httpWebResponse?.Close();
			if (connectionInfo != null)
			{
				ReleaseConnectionEntry(connectionInfo.Index);
			}
		}
	}

	private int FindAvailableConnectionEntry()
	{
		lock (_availableConnections)
		{
			for (int i = 0; i < _availableConnections.Count; i++)
			{
				if ((bool)_availableConnections[i])
				{
					_availableConnections[i] = false;
					return i;
				}
			}
			_availableConnections.Add(false);
			return _availableConnections.Count - 1;
		}
	}

	private void ReleaseConnectionEntry(int index)
	{
		lock (_availableConnections)
		{
			_availableConnections[index] = true;
		}
	}

	public HttpWebRequest InitializeRequest(string postfix, bool keepAlive)
	{
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
		httpWebRequest.ConnectionGroupName = HostIdAsString + postfix;
		httpWebRequest.PreAuthenticate = _useWebAuthentication;
		httpWebRequest.UnsafeAuthenticatedConnectionSharing = _useUnsafeConnectionSharing;
		httpWebRequest.KeepAlive = keepAlive;
		httpWebRequest.Method = "POST";
		httpWebRequest.Pipelined = false;
		httpWebRequest.SendChunked = false;
		httpWebRequest.Proxy = _iWebProxy;
		httpWebRequest.UserAgent = _userAgent;
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
}
