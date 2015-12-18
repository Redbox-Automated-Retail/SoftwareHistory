using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public class TitleRollupBrowseItem : MovieTitleBrowseItem, ITitleRollupBrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	public TitleRollupBrowseItem(LuaTable luaTable)
		: base(luaTable)
	{
	}
}
