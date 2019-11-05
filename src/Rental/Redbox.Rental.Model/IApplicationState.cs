using System.Collections.Generic;
using Redbox.Rental.Model.EmailOptInCampaign;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IApplicationState
{
	bool IsADAAccessible { get; set; }

	bool IsABEOn { get; }

	string InCartTemplateName { get; set; }

	bool VerboseLogging { get; }

	bool EnableOfflineBadCardList { get; set; }

	List<IKioskProduct> BrowseProducts { get; set; }

	bool IsStandAlone { get; set; }

	bool NeedToReloadCarousel { get; set; }

	IEmailOptInCampaign EmailOptInCampaign { get; set; }

	bool IsInMaintenanceMode { get; }

	bool HasMaintenanceModeReason(MaintenanceModeSource source, string code);

	List<IMaintenanceModeReason> GetMaintenanceModeReasons();

	void AddMaintenanceModeReason(MaintenanceModeSource source, string code);

	void RemoveMaintenanceModeReason(MaintenanceModeSource source, string code);

	void RemoveMaintenanceModeReason(MaintenanceModeSource source);
}
