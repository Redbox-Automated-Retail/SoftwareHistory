namespace Redbox.Rental.Model.Profile;

public class ProductType : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long ProductTypeId { get; set; }

	public string ProductTypeName { get; set; }

	public long ProductFamilyId { get; set; }
}
