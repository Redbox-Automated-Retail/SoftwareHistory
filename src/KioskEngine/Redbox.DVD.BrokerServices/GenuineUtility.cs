using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels;

internal class GenuineUtility
{
	private static Regex _splitUrlToHostAndPort = new Regex("^g\\w+://(?<hostName>[^:]+):(?<port>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static Regex _splitHttpLinkToHostAndPort = new Regex("^g\\w+://(?<hostName>[^:/\\\\]+)(:(?<port>\\d+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static Regex _fetchMachineGuidFromUri = new Regex("^_g\\w+://(?<remoteGuid>[^/]+)", RegexOptions.None);

	private static Guid _defaultHostIdentifier = Guid.NewGuid();

	public static string CurrentRemoteUri => CurrentMessage.Sender.Uri;

	public static HostInformation CurrentRemoteHost => CurrentMessage.Sender;

	public static ISessionSupport CurrentSession
	{
		get
		{
			Message currentMessage = CurrentMessage;
			if (currentMessage == null)
			{
				throw GenuineExceptions.Get_Processing_NoSessionAvailable();
			}
			return currentMessage.Sender;
		}
	}

	public static Message CurrentMessage
	{
		get
		{
			LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__CurrentMessage");
			return Thread.GetData(namedDataSlot) as Message;
		}
	}

	public static Socket CurrentSocket
	{
		get
		{
			LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__EstablishingSocket");
			return Thread.GetData(namedDataSlot) as Socket;
		}
	}

	public static HttpContext CurrentHttpContext => CurrentMessage.HttpServerRequestResult.HttpContext;

	public static SecuritySessionParameters CurrentInvocationSecuritySessionParameters => CurrentMessage.SecuritySessionParameters;

	public static SecuritySession CurrentConnectionSecuritySession => CurrentMessage.ConnectionLevelSecuritySession;

	public static SecuritySession CurrentInvocationSecuritySession
	{
		get
		{
			try
			{
				return CurrentRemoteHost.GetSecuritySession(CurrentInvocationSecuritySessionParameters.Name, null);
			}
			catch
			{
				return null;
			}
		}
	}

	public static IPrincipal HttpPrincipal => CurrentMessage.HttpServerRequestResult.IPrincipal;

	public static int TickCount => Environment.TickCount;

	public static int FurthestFuture => 536870911 + Environment.TickCount - 1;

	public static Guid DefaultHostIdentifier => _defaultHostIdentifier;

	public static int CurrentThreadId => AppDomain.GetCurrentThreadId();

	public static string Parse(string url, out string objectURI)
	{
		objectURI = null;
		if (url == null || url.Length <= 0)
		{
			return null;
		}
		if (url.StartsWith("ghttp") || url.StartsWith("gxhttp"))
		{
			int num = url.LastIndexOf('/');
			if (num > 0)
			{
				objectURI = url.Substring(num + 1);
			}
			int startIndex = objectURI.LastIndexOf('.');
			return url.Substring(0, num + 1) + "fake" + objectURI.Substring(startIndex);
		}
		if (url[0] != 'g' && (url[0] != '_' || url[1] != 'g'))
		{
			return null;
		}
		int num2 = url.IndexOf("://", 3);
		if (num2 < 0)
		{
			return null;
		}
		int num3 = url.IndexOf('/', num2 + 3);
		if (num3 < 0)
		{
			return url;
		}
		objectURI = url.Substring(num3 + 1);
		return url.Substring(0, num3);
	}

	public static string ParseUrl(string url, out string objectURI)
	{
		objectURI = null;
		if (url == null || url.Length <= 0)
		{
			return null;
		}
		if (url[0] != 'g' && (url[0] != '_' || url[1] != 'g'))
		{
			return null;
		}
		int num = url.IndexOf("://", 3);
		if (num < 0)
		{
			return null;
		}
		int num2 = url.IndexOf('/', num + 3);
		if (num2 < 0)
		{
			return url;
		}
		objectURI = url.Substring(num2 + 1);
		return url.Substring(0, num2);
	}

	public static string FetchChannelPrefix(string url)
	{
		int num = url.IndexOf(':');
		if (num <= -1)
		{
			return string.Empty;
		}
		if (url.StartsWith("_g"))
		{
			return url.Substring(2, num - 2);
		}
		if (url[0] == 'g')
		{
			return url.Substring(1, num - 1);
		}
		return string.Empty;
	}

	public static string SplitToHostAndPort(string url, out int port)
	{
		port = 0;
		Match match = _splitUrlToHostAndPort.Match(url);
		if (!match.Success)
		{
			return null;
		}
		port = Convert.ToInt32(match.Groups["port"].Value);
		return match.Groups["hostName"].Value;
	}

	public static string SplitHttpLinkToHostAndPort(string url, out int port)
	{
		port = 80;
		Match match = _splitHttpLinkToHostAndPort.Match(url);
		if (!match.Success)
		{
			return null;
		}
		if (match.Groups["port"].Value != null && match.Groups["port"].Value != string.Empty)
		{
			port = Convert.ToInt32(match.Groups["port"].Value);
		}
		return match.Groups["hostName"].Value;
	}

	public static bool CheckUrlOnConnectivity(string url)
	{
		if (url.StartsWith("_g"))
		{
			return false;
		}
		return true;
	}

	public static int ConvertToMilliseconds(object timeSpanAsObject)
	{
		TimeSpan timeSpan = (TimeSpan)timeSpanAsObject;
		return (int)timeSpan.TotalMilliseconds;
	}

	public static int SafeConvertToInt32(object str, int errorValue)
	{
		if (str == null || str as string == string.Empty)
		{
			return errorValue;
		}
		try
		{
			return Convert.ToInt32(str);
		}
		catch (Exception)
		{
		}
		return errorValue;
	}

	public static bool SafeConvertToBool(object str, bool errorValue)
	{
		try
		{
			return Convert.ToBoolean(str);
		}
		catch (Exception)
		{
			return errorValue;
		}
	}

	public static object SafeConvertToInt32(object str)
	{
		if (str == null || str as string == string.Empty)
		{
			return null;
		}
		try
		{
			return Convert.ToInt32(str);
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static object SafeConvertToBool(object str)
	{
		try
		{
			return Convert.ToBoolean(str);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static object SafeConvertToTimeSpan(object str)
	{
		if (str == null || str as string == string.Empty)
		{
			return null;
		}
		try
		{
			return TimeSpan.FromMilliseconds(Convert.ToInt32(str));
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static string FetchCurrentRemoteUri()
	{
		return CurrentMessage.Sender.Uri;
	}

	public static void ReadDataFromStream(Stream inputStream, byte[] buffer, int offset, int sizeToRead)
	{
		if (sizeToRead <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = inputStream.Read(buffer, offset, sizeToRead); i < sizeToRead; i += num)
		{
			num = inputStream.Read(buffer, offset + i, sizeToRead - i);
			if (num <= 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
		}
	}

	public static int TryToReadFromStream(Stream inputStream, byte[] buffer, int offset, int sizeToRead)
	{
		int num = 0;
		int i;
		for (i = inputStream.Read(buffer, offset, sizeToRead); i < sizeToRead; i += num)
		{
			num = inputStream.Read(buffer, offset + i, sizeToRead - i);
			if (num <= 0)
			{
				return i;
			}
		}
		return i;
	}

	public static void CopyStreamToStream(Stream inputStream, Stream outputStream)
	{
		if (inputStream is GenuineChunkedStream genuineChunkedStream)
		{
			genuineChunkedStream.WriteTo(outputStream);
			return;
		}
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			int count = bufferKeeper.Buffer.Length;
			for (int num = inputStream.Read(bufferKeeper.Buffer, 0, count); num > 0; num = inputStream.Read(bufferKeeper.Buffer, 0, count))
			{
				outputStream.Write(bufferKeeper.Buffer, 0, num);
			}
		}
		finally
		{
			bufferKeeper.Dispose();
		}
	}

	public static void CopyStreamToStream(Stream inputStream, Stream outputStream, byte[] buffer)
	{
		if (inputStream is GenuineChunkedStream genuineChunkedStream)
		{
			genuineChunkedStream.WriteTo(outputStream);
			return;
		}
		int count = buffer.Length;
		for (int num = inputStream.Read(buffer, 0, count); num > 0; num = inputStream.Read(buffer, 0, count))
		{
			outputStream.Write(buffer, 0, num);
		}
	}

	public static void CopyStreamToStreamWithMinimumOperations(Stream inputStream, Stream outputStream, byte[] buffer)
	{
		if (inputStream is GenuineChunkedStream genuineChunkedStream)
		{
			genuineChunkedStream.WriteTo(outputStream);
			return;
		}
		int count = buffer.Length;
		for (int num = inputStream.Read(buffer, 0, count); num > 0; num = inputStream.Read(buffer, 0, count))
		{
			outputStream.Write(buffer, 0, num);
		}
	}

	public static int CopyStreamToStream(Stream inputStream, Stream outputStream, int size)
	{
		if (size <= 0)
		{
			return 0;
		}
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			int val = bufferKeeper.Buffer.Length;
			int num = inputStream.Read(bufferKeeper.Buffer, 0, Math.Min(val, size));
			while (num > 0 && size > 0)
			{
				outputStream.Write(bufferKeeper.Buffer, 0, num);
				size -= num;
				if (size <= 0)
				{
					return size;
				}
				num = inputStream.Read(bufferKeeper.Buffer, 0, Math.Min(val, size));
			}
		}
		finally
		{
			bufferKeeper.Dispose();
		}
		return size;
	}

	public static void CopyStreamToStreamPhysically(Stream inputStream, Stream outputStream, byte[] buffer)
	{
		int count = buffer.Length;
		for (int num = inputStream.Read(buffer, 0, count); num > 0; num = inputStream.Read(buffer, 0, count))
		{
			outputStream.Write(buffer, 0, num);
		}
	}

	public static byte[] CopyStreamToStreamExceptSign(Stream inputStream, Stream outputStream, int signSize)
	{
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			int num = bufferKeeper.Buffer.Length;
			int num2 = inputStream.Read(bufferKeeper.Buffer, 0, num);
			int num3 = 0;
			while (num2 > 0)
			{
				num3 += num2;
				if (num3 > signSize)
				{
					outputStream.Write(bufferKeeper.Buffer, 0, num3 - signSize);
					Buffer.BlockCopy(bufferKeeper.Buffer, num3 - signSize, bufferKeeper.Buffer, 0, signSize);
					num3 = signSize;
				}
				num2 = inputStream.Read(bufferKeeper.Buffer, num3, num - num3);
			}
			byte[] array = new byte[num3];
			Buffer.BlockCopy(bufferKeeper.Buffer, 0, array, 0, num3);
			return array;
		}
		finally
		{
			bufferKeeper.Dispose();
		}
	}

	public static string FetchDotNetUriFromMbr(MarshalByRefObject marshalByRefObject)
	{
		ObjRef objRef = RemotingServices.Marshal(marshalByRefObject);
		return objRef.URI;
	}

	public static void FetchChannelUriFromMbr(MarshalByRefObject marshalByRefObject, out string uri, out ITransportContext iTransportContext)
	{
		IDictionary channelSinkProperties = ChannelServices.GetChannelSinkProperties(marshalByRefObject);
		uri = channelSinkProperties["GC_URI"] as string;
		iTransportContext = channelSinkProperties["GC_TC"] as ITransportContext;
	}

	public static ITransportContext FetchTransportContextFromMbr(MarshalByRefObject marshalByRefObject)
	{
		IDictionary channelSinkProperties = ChannelServices.GetChannelSinkProperties(marshalByRefObject);
		return channelSinkProperties["GC_TC"] as ITransportContext;
	}

	public static HostInformation FetchHostInformationFromMbr(MarshalByRefObject marshalByRefObject)
	{
		FetchChannelUriFromMbr(marshalByRefObject, out var uri, out var iTransportContext);
		string objectURI;
		return iTransportContext.KnownHosts.Get(Parse(uri, out objectURI));
	}

	public static string FetchMachineGuidFromUri(string uri)
	{
		Match match = _fetchMachineGuidFromUri.Match(uri);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups["remoteGuid"].Value;
	}

	public static IPAddress ResolveIPAddress(string url)
	{
		IPAddress iPAddress = null;
		try
		{
			if (url.Length > 0 && url[0] >= '0' && url[0] <= '9')
			{
				iPAddress = IPAddress.Parse(url);
			}
		}
		catch
		{
		}
		if (iPAddress == null)
		{
			IPAddress[] addressList = Dns.Resolve(url).AddressList;
			if (addressList.Length <= 0)
			{
				throw GenuineExceptions.Get_Connect_CanNotResolveHostName(url);
			}
			iPAddress = addressList[0];
		}
		if (iPAddress == null)
		{
			throw GenuineExceptions.Get_Connect_CanNotResolveHostName(url);
		}
		return iPAddress;
	}

	public static IServerChannelSinkProvider GetDefaultServerSinkChain()
	{
		BinaryServerFormatterSinkProvider binaryServerFormatterSinkProvider = null;
		try
		{
			IDictionary dictionary = new Hashtable();
			dictionary["typeFilterLevel"] = "Full";
			return new BinaryServerFormatterSinkProvider(dictionary, null);
		}
		catch (Exception)
		{
			return new BinaryServerFormatterSinkProvider();
		}
	}

	public static bool AreBuffersEquals(byte[] first, byte[] second)
	{
		if (first.Length != second.Length)
		{
			return false;
		}
		int num = first.Length;
		for (int i = 0; i < num; i++)
		{
			if (first[i] != second[i])
			{
				return false;
			}
		}
		return true;
	}

	public static byte[] CutOutBuffer(byte[] source, int offset, int count)
	{
		byte[] array = new byte[count];
		Buffer.BlockCopy(source, offset, array, 0, count);
		return array;
	}

	public static bool WaitOne(WaitHandle waitHandle, int timeSpan)
	{
		if (timeSpan < 0)
		{
			timeSpan = 0;
		}
		return waitHandle.WaitOne(timeSpan, exitContext: false);
	}

	public static Exception ReadException(Stream stream)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(stream) as Exception;
	}

	public static int CompareTickCounts(int finish, int start)
	{
		int num = finish - start;
		if ((num & 0x40000000) != 0)
		{
			return num | int.MinValue;
		}
		return num & 0x7FFFFFFF;
	}

	public static bool IsTimeoutExpired(int timeout, int now)
	{
		return CompareTickCounts(timeout, now) <= 0;
	}

	public static bool IsTimeoutExpired(int timeout)
	{
		return CompareTickCounts(timeout, TickCount) < 0;
	}

	public static int GetTimeout(int timespan)
	{
		return TickCount + timespan;
	}

	public static int GetTimeout(TimeSpan timespan)
	{
		return TickCount + ConvertToMilliseconds(timespan);
	}

	public static int GetMillisecondsLeft(int timeout)
	{
		int num = CompareTickCounts(timeout, TickCount);
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}
}
