using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels;

internal class GCHandleKeeper : IDisposable
{
	public readonly GCHandle GCHandle;

	private bool wasFreed = false;

	public GCHandleKeeper(object val, GCHandleType type)
	{
		GCHandle = GCHandle.Alloc(val, type);
	}

	public void Dispose()
	{
		if (!wasFreed)
		{
			GCHandle.Free();
		}
		wasFreed = true;
	}
}
