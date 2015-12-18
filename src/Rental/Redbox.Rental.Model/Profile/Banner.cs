namespace Redbox.Rental.Model.Profile;

public class Banner : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long BannerId { get; set; }

	public string BannerName { get; set; }
}
