using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.KioskProduct;

public class StarzOfferPeriod : IStarzOfferPeriod
{
	public int SequenceNum { get; set; }

	public int PeriodLength { get; set; }

	public bool IsPromoPrice { get; set; }

	public decimal Price { get; set; }

	public string PriceText => Price.ToString("#.##");

	public decimal TaxRate { get; set; }

	public StarzBillingPeriod BillingPeriod { get; set; }

	public StarzPeriodUnit PeriodUnit { get; set; }

	public IList<IStarzBenefit> Benefits { get; set; }

	public StarzOfferPeriod()
	{
		Benefits = new List<IStarzBenefit>();
	}
}
