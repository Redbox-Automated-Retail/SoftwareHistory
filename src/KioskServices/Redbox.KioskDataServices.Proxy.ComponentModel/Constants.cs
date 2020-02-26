namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class Constants
{
	public class Messages
	{
		public const string AddKioskSession = "AddKioskSession";

		public const string AddKioskSessionJson = "AddKioskSessionJson";
	}

	public class ApiUrls
	{
		public const string AddKioskSession = "api/KioskData/AddKioskSession";

		public const string AddKioskSessionJson = "api/KioskData/AddKioskSessionJson";

		public const string KioskAlert = "api/KioskData/KioskAlert";

		public const string AuditEvent = "api/KioskData/AuditEvent";

		public const string KioskFunctionCheck = "api/KioskData/KioskFunctionCheck";

		public const string KioskVersion = "api/KioskData/KioskVersion";
	}

	public class RequestHeaderKeys
	{
		public const string RedboxKioskId = "x-redbox-kioskid";

		public const string RedboxActivityId = "x-redbox-activityid";
	}
}
