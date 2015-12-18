using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public interface IProfileLuaTable
{
	List<ILuaKeyValue> Table { get; }

	Dictionary<string, ILuaKeyValue> Data { get; }
}
