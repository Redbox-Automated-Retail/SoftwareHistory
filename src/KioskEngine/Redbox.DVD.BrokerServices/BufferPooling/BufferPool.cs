using System.Collections;

namespace Belikov.GenuineChannels.BufferPooling;

internal class BufferPool
{
	public const int GENERAL_BUFFER_SIZE = 3000;

	private const int _maxPooledBuffers = 100;

	private static object _lock = new object();

	private static Stack _stack = new Stack(100);

	public static byte[] ObtainBuffer()
	{
		lock (_lock)
		{
			if (_stack.Count > 0)
			{
				return (byte[])_stack.Pop();
			}
			return new byte[3000];
		}
	}

	public static void RecycleBuffer(byte[] buf)
	{
		lock (_lock)
		{
			if (_stack.Count < 100 && buf.Length == 3000)
			{
				_stack.Push(buf);
			}
		}
	}
}
