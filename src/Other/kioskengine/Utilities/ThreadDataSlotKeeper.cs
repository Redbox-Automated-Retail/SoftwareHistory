using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Belikov.GenuineChannels.Utilities;

[ComVisible(true)]
public struct ThreadDataSlotKeeper : IDisposable
{
	private LocalDataStoreSlot _localDataStoreSlot;

	public ThreadDataSlotKeeper(string slotName, object val)
	{
		_localDataStoreSlot = Thread.GetNamedDataSlot(slotName);
		Thread.SetData(_localDataStoreSlot, val);
	}

	public void Dispose()
	{
		Thread.SetData(_localDataStoreSlot, null);
	}
}
