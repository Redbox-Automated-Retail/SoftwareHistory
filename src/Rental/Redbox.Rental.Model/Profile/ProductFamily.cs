using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class ProductFamily : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long ProductFamilyId { get; set; }

	public string ProductFamilyName { get; set; }

	public List<long> Genres { get; set; }
}
