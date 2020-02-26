using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Model.Marketing;

public class Ad
{
	public AdLocation Location { get; set; }

	public string ImageName { get; set; }

	public int? MinimumDisplay { get; set; }
}
