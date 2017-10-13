using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IInventoryServices
{
	void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, RemoteServiceCallback callback);

	void BackwardsDiscReturn(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, RemoteServiceCallback callback);

	void Snapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback);

	void SnapshotImmediate(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback);

	void GetRentalCount(string storeNumber, RemoteServiceCallback completeCallback);

	bool CanSwitch();

	IInventoryItem CreateInventoryItem(string barcode, int? deck, int? slot, InventoryItemStatus status);

	void MarkAsUnplayable(string storeNumber, string barcode, string email, string returnTime, RemoteServiceCallback completeCallback);

	void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback);

	void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> barcode_list, RemoteServiceCallback completeCallback);
}
