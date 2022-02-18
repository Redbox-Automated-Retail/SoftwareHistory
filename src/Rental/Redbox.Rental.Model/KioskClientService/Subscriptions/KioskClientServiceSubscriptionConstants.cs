namespace Redbox.Rental.Model.KioskClientService.Subscriptions;

public class KioskClientServiceSubscriptionConstants
{
	public class TestData
	{
		public class GetSubscriptionPlans
		{
			public const string LiveCall = "GetSubscriptionPlans.LiveCall";

			public const string BrowseStarzSubscriptionPlan = "GetSubscriptionPlans.BrowseStarzSubscriptionPlan";

			public const string MyBagStarzSubscriptionPlan = "GetSubscriptionPlans.MyBagStarzSubscriptionPlan";
		}

		public const string CategoryName = "ProxyServiceSubscriptions";
	}

	public class SubscriptionIds
	{
		public const string RedboxPlusSubscriptionId = "RBPLUS";

		public const string StarzSubscriptionId = "STARZ";
	}

	public class SubscriptionBenefits
	{
		public const string RedboxPlusExtraNightCoverageId = "EXT_NGHT_1500";
	}
}
