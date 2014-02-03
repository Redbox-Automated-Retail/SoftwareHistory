using System.Collections;
using System.Threading;

namespace Belikov.GenuineChannels.Utilities;

internal class EventPool
{
	private const int _maxPooledObjects = 100;

	private static ManualResetEvent _globalStubEvent = new ManualResetEvent(initialState: false);

	private static object _lock = new object();

	private static Stack _stack = new Stack(100);

	public static ManualResetEvent GlobalStubEvent => _globalStubEvent;

	public static ManualResetEvent ObtainEvent()
	{
		lock (_lock)
		{
			if (_stack.Count > 0)
			{
				return (ManualResetEvent)_stack.Pop();
			}
			return new ManualResetEvent(initialState: false);
		}
	}

	public static void RecycleEvent(ManualResetEvent manualResetEvent)
	{
		lock (_lock)
		{
			if (_stack.Count < 100)
			{
				manualResetEvent.Reset();
				_stack.Push(manualResetEvent);
			}
		}
	}
}
