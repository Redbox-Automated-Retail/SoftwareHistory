using System.Collections.Generic;

namespace Redbox.Rental.Services.Controllers;

public class ReservationData
{
	public List<ReservationTitle> Titles { get; set; }

	public bool AppliedCredit { get; set; }

	public bool AppliedTitleMarketing { get; set; }

	public bool AppliedHiveOnlinePromo { get; set; }

	public decimal HiveOnlinePromoDiscount { get; set; }

	public string PromoCode { get; set; }

	public ReservationData()
	{
		Titles = new List<ReservationTitle>();
	}
}
