namespace Redbox.FraudServices.Proxy.ComponentModel;

public class Constants
{
	public class Messages
	{
		public const string BarcodeTracking = "BarcodeTracking";

		public const string UploadDiscImage = "UploadDiscImage";
	}

	public class ApiUrls
	{
		public const string BarcodeTracking = "api/Fraud/addbarcodetracking";

		public const string AwsBarcodeTracking = "api/frauddata/BarcodeTracking";

		public const string UploadDiscImage = "api/Fraud/discimage";

		public const string AwsUploadDiscImage = "api/frauddata/DiscImage";
	}
}
