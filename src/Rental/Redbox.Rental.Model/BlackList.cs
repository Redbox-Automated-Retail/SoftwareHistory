namespace Redbox.Rental.Model;

public class BlackList
{
	public string Key { get; set; }

	public string PatternHash { get; set; }

	public BlackListType BlacklistTypeID { get; set; }

	public string Value { get; set; }
}
