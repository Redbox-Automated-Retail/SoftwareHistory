namespace Redbox.Rental.Model.Profile;

public class ProductRating : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long RatingId { get; set; }

	public long ProductFamilyId { get; set; }

	public string Name { get; set; }

	public string RatingDescription { get; set; }
}
