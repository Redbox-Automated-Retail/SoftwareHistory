using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Web;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.GenuineHttp;

[ComVisible(true)]
public class HttpServerHandler : IHttpAsyncHandler, IHttpHandler
{
	public HttpContext HttpContext;

	public AsyncCallback AsyncCallback;

	private HttpServerConnectionManager _httpServerConnectionManager;

	private object _httpServerConnectionManagerLock = new object();

	public bool IsReusable => true;

	public void ProcessRequest(HttpContext context)
	{
		IAsyncResult asyncResult = BeginProcessRequest(context, null, null);
		asyncResult.AsyncWaitHandle.WaitOne();
		EndProcessRequest(asyncResult);
	}

	public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
	{
		try
		{
			HttpServerRequestResult httpServerRequestResult = new HttpServerRequestResult(context, cb, extraData);
			httpServerRequestResult.IPrincipal = Thread.CurrentPrincipal;
			lock (_httpServerConnectionManagerLock)
			{
				if (_httpServerConnectionManager == null)
				{
					if (!(ChannelServices.GetChannel("ghttp") is GenuineHttpServerChannel genuineHttpServerChannel))
					{
						throw GenuineExceptions.Get_Receive_NoServerChannel();
					}
					_httpServerConnectionManager = genuineHttpServerChannel.ITransportContext.ConnectionManager as HttpServerConnectionManager;
				}
			}
			_httpServerConnectionManager.HandleIncomingRequest(httpServerRequestResult);
			return httpServerRequestResult;
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "HttpServerHandler.BeginProcessRequest", LogMessageType.ConnectionAccepting, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Can't process an incoming connection.");
			}
			throw;
		}
	}

	public void EndProcessRequest(IAsyncResult result)
	{
		result.AsyncWaitHandle.WaitOne();
	}
}
