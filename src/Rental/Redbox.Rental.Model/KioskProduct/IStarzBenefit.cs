namespace Redbox.Rental.Model.KioskProduct;

public interface IStarzBenefit
{
	string Id { get; set; }

	string Description { get; set; }

	int Quantity { get; set; }

	StarzBenefitPeriod Period { get; set; }

	StarzBenefitType StarzBenefitType { get; set; }
}
