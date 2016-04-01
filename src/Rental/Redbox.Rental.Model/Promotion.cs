namespace Redbox.Rental.Model;

public class Promotion
{
	public string Name { get; set; }

	public decimal OriginalPrice { get; set; }

	public decimal Price { get; set; }

	public int Quantity { get; set; }

	public PromotionIntentCode Intent { get; set; }

	public decimal OriginalPercent { get; set; }

	public decimal Percent { get; set; }

	public byte RentQuantity { get; set; }

	public PromotionRentFormat? RentFormat { get; set; }

	public byte GetQuantity { get; set; }

	public PromotionGetFormat? GetFormat { get; set; }

	public CampaignTitles CampaignTitles { get; set; }
}
