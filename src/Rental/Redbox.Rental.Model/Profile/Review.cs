using System;

namespace Redbox.Rental.Model.Profile;

public class Review
{
	public long TitleId { get; set; }

	public string DisplayName { get; set; }

	public DateTime ReviewDate { get; set; }

	public int Stars { get; set; }

	public string ReviewText { get; set; }
}
