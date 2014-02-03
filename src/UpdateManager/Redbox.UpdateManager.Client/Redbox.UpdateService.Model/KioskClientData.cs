namespace Redbox.UpdateService.Model;

internal class KioskClientData
{
	public long KioskClientID { get; set; }

	public int? KioskStatusId { get; set; }

	public string KioskStatusName { get; set; }

	public string City { get; set; }

	public string County { get; set; }

	public string State { get; set; }

	public string Zip { get; set; }

	public long? VendorId { get; set; }

	public string VendorName { get; set; }

	public long? MarketId { get; set; }

	public string MarketName { get; set; }

	public long? BannerId { get; set; }

	public string BannerName { get; set; }

	public TimeZoneData TimeZoneData { get; set; }

	public decimal? DayLightSavingsTimeOffset { get; set; }

	public decimal? GreenwichMeanTimeOffset { get; set; }

	public string DisplayText { get; set; }
}
