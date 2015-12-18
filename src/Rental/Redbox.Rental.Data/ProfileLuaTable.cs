using System.Collections.Generic;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Data;

public class ProfileLuaTable : IProfileLuaTable
{
	private List<ILuaKeyValue> _table;

	private Dictionary<string, ILuaKeyValue> _data = new Dictionary<string, ILuaKeyValue>();

	public List<ILuaKeyValue> Table => _table;

	public Dictionary<string, ILuaKeyValue> Data => _data;

	public ProfileLuaTable(List<ILuaKeyValue> table)
	{
		_table = table;
		_table.ForEach(delegate(ILuaKeyValue row)
		{
			_data[row.Key] = row;
		});
	}
}
