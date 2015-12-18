namespace Redbox.Rental.Model.Profile;

public class Market : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long MarketId { get; set; }

	public string MarketName { get; set; }
}
