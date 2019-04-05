using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class ReviewList : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long TitleId { get; set; }

	public List<Review> Reviews { get; set; }
}
