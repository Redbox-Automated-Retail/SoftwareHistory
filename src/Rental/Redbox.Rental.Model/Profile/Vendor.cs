namespace Redbox.Rental.Model.Profile;

public class Vendor : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long VendorId { get; set; }

	public string VendorName { get; set; }
}
