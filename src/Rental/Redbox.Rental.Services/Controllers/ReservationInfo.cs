using System.Collections.Generic;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Services.Controllers;

public class ReservationInfo
{
	public List<string> Barcodes { get; set; }

	public List<long> CustomerHistory { get; set; }

	public string ReferenceNumber { get; set; }

	public string EmailAddress { get; set; }

	public ReservationData ReservationData { get; set; }

	public IPricingSet PricingSet { get; set; }

	public decimal DefaultServiceFee { get; set; }

	public ReservationInfo()
	{
		Barcodes = new List<string>();
		ReservationData = new ReservationData();
		CustomerHistory = new List<long>();
	}
}
