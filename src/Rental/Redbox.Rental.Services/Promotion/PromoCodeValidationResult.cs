using Redbox.Rental.Model;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.Services.Promotion;

public class PromoCodeValidationResult : IPromoCodeValidationResult
{
	public bool IsStandAlone { get; set; }

	public bool IsOnline { get; set; }

	public string ServerName { get; set; }

	public bool Success { get; set; }

	public string StoreNumber { get; set; }

	public string PromoCode { get; set; }

	public string Response { get; set; }

	public decimal Amount { get; set; }

	public PromotionIntentCode IntentCode { get; set; }

	public int RentQuantity { get; set; }

	public PromotionRentFormat RentFormat { get; set; }

	public int GetQuantity { get; set; }

	public PromotionGetFormat GetFormat { get; set; }

	public ICampaignTitles CampaignTitles { get; set; }
}
