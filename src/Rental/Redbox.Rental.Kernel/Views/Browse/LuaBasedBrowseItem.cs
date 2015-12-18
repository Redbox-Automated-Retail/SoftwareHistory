using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public class LuaBasedBrowseItem : BrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	public LuaTable LuaObject { get; set; }

	public LuaBasedBrowseItem(LuaTable luaTable)
	{
		LuaObject = luaTable;
		base.Name = GetLuaTableValue("product_name");
		base.ImageName = GetLuaTableValue("image_file");
		base.SortName = GetLuaTableValue("sort_name");
	}

	private string GetLuaTableValue(string propertyName)
	{
		string result = null;
		if (LuaObject != null)
		{
			result = LuaObject[propertyName]?.ToString();
		}
		return result;
	}
}
