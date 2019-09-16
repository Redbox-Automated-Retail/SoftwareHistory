using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.FraudServices.Proxy.ComponentModel;

public interface IUploadDiscImageService
{
	bool IsActive { get; set; }

	void UploadDiscImage(int kioskId, string barcode, ImageType imageType, long? transactionId, byte[] image, bool? hardwareSupportsIRScan, string cpAccountNumber, RemoteServiceCallback completeCallback);
}
