using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels;

[ComVisible(true)]
public class GCHandleKeeper : IDisposable
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
