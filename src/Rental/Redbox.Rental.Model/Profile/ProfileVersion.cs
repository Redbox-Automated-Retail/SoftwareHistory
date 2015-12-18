namespace Redbox.Rental.Model.Profile;

public class ProfileVersion : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public string VersionValue { get; set; }
}
