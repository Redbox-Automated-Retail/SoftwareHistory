using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class ProductGroup : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long ProductGroupId { get; set; }

	public string ProductGroupName { get; set; }

	public int ProductGroupTypeId { get; set; }

	public List<long> ProductIds { get; set; }
}
