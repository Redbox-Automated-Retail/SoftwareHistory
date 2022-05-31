using System;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class ReturnVisitPromo
{
	public string PromoCode { get; set; }

	public decimal Amount { get; set; }

	public DateTime ExpirationDate { get; set; }
}
