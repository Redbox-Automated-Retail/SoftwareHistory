using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IInventoryServices
{
	void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback);

	void BackwardsDiscReturn(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, RemoteServiceCallback callback);

	void Snapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback);

	void SnapshotImmediate(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback);

	void GetRentalCount(string storeNumber, RemoteServiceCallback completeCallback);

	bool CanSwitch();

	IInventoryItem CreateInventoryItem(string barcode, int? deck, int? slot, InventoryItemStatus status);

	void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback, long? transactionId = null, long? discId = null, DateTime? returnDate = null);

	void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> barcode_list, RemoteServiceCallback completeCallback);
}
