using Redbox.KioskEngine.ComponentModel.FraudServices;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.FraudServices.Proxy.ComponentModel;

public interface IBarcodeTrackingService
{
	int TimerInterval { get; set; }

	int MaxBatchSize { get; set; }

	bool IsActive { get; set; }

	int BarcodeTrackingMessagePriority { get; set; }

	void AddBarcodeTrackingInfoToQueue(IBarcodeTrackingInfo barcodeTrackingInfo, RemoteServiceCallback completeCallback);

	void AddCachedBarcodeTrackingInfosToMessageQueue(RemoteServiceCallback completeCallback);
}
