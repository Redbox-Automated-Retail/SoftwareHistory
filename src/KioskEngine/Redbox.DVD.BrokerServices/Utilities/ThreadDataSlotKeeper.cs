using System;
using System.Threading;

namespace Belikov.GenuineChannels.Utilities;

internal struct ThreadDataSlotKeeper : IDisposable
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
