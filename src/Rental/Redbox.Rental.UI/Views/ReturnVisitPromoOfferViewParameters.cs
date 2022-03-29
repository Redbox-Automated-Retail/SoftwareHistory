using System;

namespace Redbox.Rental.UI.Views;

public class ReturnVisitPromoOfferViewParameters
{
	public string Name { get; set; }

	public decimal Price { get; set; }

	public DateTime ExpirationDate { get; set; }

	public Action CloseAction { get; set; }
}
