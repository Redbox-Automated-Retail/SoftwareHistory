using System.Collections.Generic;

namespace Redbox.FraudServices.Proxy;

public class UploadDiscImageRequest
{
	public int KioskId { get; set; }

	public string Barcode { get; set; }

	public byte ImageType { get; set; }

	public long? TransactionId { get; set; }

	public Dictionary<string, string> PropertyBag { get; set; }

	public string Image { get; set; }
}
