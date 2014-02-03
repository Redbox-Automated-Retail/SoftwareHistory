using System;
using System.Net;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpWebRequestCop : ITimerConsumer
{
	private HttpWebRequest _httpWebRequest;

	private AsyncCallback _asyncCallback;

	private object _state;

	private int _timeoutExpiresAt;

	private bool _isCompleted = false;

	private bool _isAborted = false;

	public HttpWebRequestCop(HttpWebRequest httpWebRequest, AsyncCallback asyncCallback, object state, int timeoutSpan)
	{
		_httpWebRequest = httpWebRequest;
		_asyncCallback = asyncCallback;
		_state = state;
		_timeoutExpiresAt = GenuineUtility.GetTimeout(timeoutSpan);
		_httpWebRequest.BeginGetResponse(HandleWebResponse, state);
		TimerProvider.Attach(this);
	}

	private void HandleWebResponse(IAsyncResult ar)
	{
		lock (this)
		{
			if (_isAborted)
			{
				try
				{
					_httpWebRequest.EndGetResponse(ar);
					return;
				}
				catch
				{
					return;
				}
			}
			_isCompleted = true;
		}
		_asyncCallback(ar);
	}

	public void TimerCallback()
	{
		lock (this)
		{
			if (!_isCompleted && !_isAborted && GenuineUtility.IsTimeoutExpired(_timeoutExpiresAt))
			{
				GenuineLoggingServices.BinaryLogWriter?.WriteEvent(LogCategory.Debugging, "HttpWebRequestCop.TimerCallback", LogMessageType.DebuggingWarning, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Stopping the web request.");
				_isAborted = true;
				_httpWebRequest.Abort();
			}
		}
	}
}
