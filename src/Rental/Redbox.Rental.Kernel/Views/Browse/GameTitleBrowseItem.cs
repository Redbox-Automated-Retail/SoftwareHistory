using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public class GameTitleBrowseItem : MovieOrGameBrowseItem, IGameTitleBrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	public class Constants
	{
		public const string DefaultImageName = "image_game_missing_box_art";
	}

	public GameTitleBrowseItem(LuaTable luaTable)
		: base(luaTable)
	{
	}

	protected override string GetDefaultImageName()
	{
		return "image_game_missing_box_art";
	}
}
