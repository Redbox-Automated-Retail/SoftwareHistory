using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Data;

internal static class ProfileExtensions
{
	internal static List<ILuaKeyValue> ToLuaList<T>(this List<T> keyValues)
	{
		List<ILuaKeyValue> list = new List<ILuaKeyValue>();
		keyValues.ForEach(delegate(T item)
		{
			if (item is ILuaKeyValue)
			{
				list.Add((ILuaKeyValue)(object)item);
			}
		});
		return list;
	}

	internal static List<ILuaKeyValue> ToLuaList<T>(this Dictionary<long, T> keyValues)
	{
		List<ILuaKeyValue> list = new List<ILuaKeyValue>();
		keyValues.ToList().ForEach(delegate(KeyValuePair<long, T> item)
		{
			if (item.Value is ILuaKeyValue)
			{
				list.Add((ILuaKeyValue)(object)item.Value);
			}
		});
		return list;
	}
}
