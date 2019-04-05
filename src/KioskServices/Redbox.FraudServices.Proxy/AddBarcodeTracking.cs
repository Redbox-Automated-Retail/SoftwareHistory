namespace Redbox.FraudServices.Proxy;

public class AddBarcodeTracking
{
	public string SourceName { get; set; }

	public int BarcodesRead { get; set; }

	public int SecureBarcodesRead { get; set; }

	public string Barcode { get; set; }

	public int CameraType { get; set; }

	public string ScanDateTime { get; set; }

	public int DecoderUsed { get; set; }

	public bool MarkedAsFraud { get; set; }

	public int ScanAttempt { get; set; }
}
