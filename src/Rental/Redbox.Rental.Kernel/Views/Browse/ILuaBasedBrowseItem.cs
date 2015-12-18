using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public interface ILuaBasedBrowseItem : IBrowseItem
{
	LuaTable LuaObject { get; set; }
}
