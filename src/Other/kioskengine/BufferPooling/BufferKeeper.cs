using System;

namespace Belikov.GenuineChannels.BufferPooling;

internal struct BufferKeeper(int ignored) : IDisposable
{
	public byte[] Buffer = BufferPool.ObtainBuffer();

	public void Dispose()
	{
		BufferPool.RecycleBuffer(Buffer);
	}
}
