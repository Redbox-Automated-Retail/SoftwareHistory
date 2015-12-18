namespace Redbox.Rental.Model.Profile;

public class Genre : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long GenreId { get; set; }

	public string GenreName { get; set; }
}
