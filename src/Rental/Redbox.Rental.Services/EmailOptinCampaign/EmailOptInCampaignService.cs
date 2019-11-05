using System.Collections.Generic;
using Redbox.Rental.Model.EmailOptInCampaign;

namespace Redbox.Rental.Services.EmailOptinCampaign;

public class EmailOptInCampaignService
{
	public class Constants
	{
		public const string EmailOptInCampaignBundleResourceName = "opt_in_campaign";

		public const string CampaignId = "campaign_id";

		public const string EmailTemplateName = "email_template_name";

		public const string CampaignImage = "campaign_image";

		public const string ProgramName = "program_name";
	}

	public static IEmailOptInCampaign LoadEmailOptInCampaign()
	{
		IEmailOptInCampaign result = null;
		List<Dictionary<string, object>> resourceTable = OrchestrationService.GetResourceTable("opt_in_campaign");
		if (resourceTable != null && resourceTable.Count > 0)
		{
			Dictionary<string, object> dictionary = resourceTable[0];
			if (dictionary != null)
			{
				dictionary.TryGetValue("campaign_id", out var value);
				dictionary.TryGetValue("email_template_name", out var value2);
				dictionary.TryGetValue("campaign_image", out var value3);
				object resourceProperty = OrchestrationService.GetResourceProperty("opt_in_campaign", "program_name");
				result = new EmailOptInCampaign
				{
					ProgramName = resourceProperty?.ToString(),
					CampaignId = value?.ToString(),
					TemplateName = value2?.ToString(),
					ImageName = value3?.ToString()
				};
			}
		}
		return result;
	}
}
