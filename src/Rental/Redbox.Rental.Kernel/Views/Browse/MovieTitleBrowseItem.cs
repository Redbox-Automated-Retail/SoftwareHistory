using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public class MovieTitleBrowseItem : MovieOrGameBrowseItem, IMovieTitleBrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	public class Constants
	{
		public const string DefaultImageName = "image_movie_missing_box_art";
	}

	public MovieTitleBrowseItem(LuaTable luaTable)
		: base(luaTable)
	{
	}

	protected override string GetDefaultImageName()
	{
		return "image_movie_missing_box_art";
	}
}
