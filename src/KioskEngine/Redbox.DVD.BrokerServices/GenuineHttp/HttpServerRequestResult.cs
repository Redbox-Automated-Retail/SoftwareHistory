using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpServerRequestResult : IAsyncResult
{
	public HttpContext HttpContext;

	public IPrincipal IPrincipal;

	public AsyncCallback AsyncCallback;

	private object _asyncState;

	private bool _completedSynchronously = false;

	private ManualResetEvent _asyncWaitHandle = new ManualResetEvent(initialState: false);

	public object AsyncState => _asyncState;

	public bool CompletedSynchronously => _completedSynchronously;

	public WaitHandle AsyncWaitHandle => _asyncWaitHandle;

	public bool IsCompleted => _asyncWaitHandle.WaitOne(0, exitContext: false);

	public HttpServerRequestResult(HttpContext httpContext, AsyncCallback asyncCallback, object asyncState)
	{
		HttpContext = httpContext;
		AsyncCallback = asyncCallback;
		_asyncState = asyncState;
	}

	public void Complete(bool completedSynchronously)
	{
		try
		{
			_completedSynchronously = completedSynchronously;
			_asyncWaitHandle.Set();
			if (AsyncCallback != null)
			{
				AsyncCallback.DynamicInvoke(this);
			}
		}
		catch (Exception exception)
		{
			GenuineLoggingServices.BinaryLogWriter?.WriteImplementationWarningEvent("HttpServerRequestResult.Complete", LogMessageType.CriticalError, exception, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Fatal logic error.");
		}
	}
}
