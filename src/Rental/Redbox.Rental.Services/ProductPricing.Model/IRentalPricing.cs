namespace ProductPricing.Model;

public interface IRentalPricing : IPricing
{
	PriceLevelType PriceLevelType { get; set; }

	string PriceLevelTypeValue { get; set; }

	decimal InitialNight { get; set; }

	int InitialDays { get; set; }
}
