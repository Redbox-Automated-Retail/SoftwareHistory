namespace Redbox.Rental.Model.Profile;

public class SubPlatform : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long SubPlatformId { get; set; }

	public string SubPlatformName { get; set; }
}
