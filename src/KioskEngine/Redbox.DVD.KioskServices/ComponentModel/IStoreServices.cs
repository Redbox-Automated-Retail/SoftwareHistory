using System;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IStoreServices
{
	string GetEnvironment();

	string GetApplication();

	void GetPendingStates(RemoteServiceCallback completeCallback);

	void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback);

	void GetPendingBanners(int stateId, RemoteServiceCallback completeCallback);

	DateTime? GetRebootTime();

	bool ShouldRunInitialSync();

	string GetInitialSyncType();

	void SetRunInitialSync(bool flag);

	bool CanSwitch();
}
