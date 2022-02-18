using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public interface IStarzOfferPeriod
{
	int SequenceNum { get; set; }

	int PeriodLength { get; set; }

	bool IsPromoPrice { get; set; }

	decimal Price { get; set; }

	string PriceText { get; }

	decimal TaxRate { get; set; }

	StarzBillingPeriod BillingPeriod { get; set; }

	StarzPeriodUnit PeriodUnit { get; set; }

	IList<IStarzBenefit> Benefits { get; }
}
