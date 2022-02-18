using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.KioskProduct;

public class StarzBenefit : IStarzBenefit
{
	public string Id { get; set; }

	public string Description { get; set; }

	public int Quantity { get; set; }

	public StarzBenefitPeriod Period { get; set; }

	public StarzBenefitType StarzBenefitType { get; set; }
}
