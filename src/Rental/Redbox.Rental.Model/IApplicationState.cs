using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IApplicationState
{
	bool IsADAAccessible { get; set; }

	bool IsABEOn { get; }

	string InCartTemplateName { get; set; }

	bool VerboseLogging { get; }

	List<IKioskProduct> BrowseProducts { get; set; }
}
