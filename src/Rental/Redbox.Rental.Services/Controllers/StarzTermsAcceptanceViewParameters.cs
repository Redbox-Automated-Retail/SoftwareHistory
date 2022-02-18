using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class StarzTermsAcceptanceViewParameters
{
	public string EmailAddress { get; set; }

	public IStarzSubscriptionProduct StarzSubscriptionProduct { get; set; }
}
