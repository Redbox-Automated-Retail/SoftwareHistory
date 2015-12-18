namespace Redbox.Rental.Model.Profile;

public class ProductGroupXREF : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long ProductId { get; set; }

	public long ProductGroupId { get; set; }
}
