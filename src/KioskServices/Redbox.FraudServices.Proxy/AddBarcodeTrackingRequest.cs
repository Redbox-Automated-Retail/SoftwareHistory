using System.Collections.Generic;

namespace Redbox.FraudServices.Proxy;

public class AddBarcodeTrackingRequest
{
	public int KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string HalVersion { get; set; }

	public List<AddBarcodeTracking> BarcodeTrackingList { get; set; }
}
