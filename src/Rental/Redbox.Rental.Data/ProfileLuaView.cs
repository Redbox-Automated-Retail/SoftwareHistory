using System.Collections.Generic;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Data;

public class ProfileLuaView : IProfileLuaView
{
	private IProfileDataInstance _instance;

	private Dictionary<string, IProfileLuaTable> _tables = new Dictionary<string, IProfileLuaTable>();

	public Dictionary<string, IProfileLuaTable> Tables => _tables;

	public ProfileLuaView(IProfileDataInstance instance)
	{
		_instance = instance;
		_tables["Store"] = new ProfileLuaTable(_instance.Stores.ToLuaList());
		_tables["Genres"] = new ProfileLuaTable(_instance.Genres.ToLuaList());
		_tables["Version"] = new ProfileLuaTable(_instance.ProfileVersion.ToLuaList());
		_tables["ProductType"] = new ProfileLuaTable(_instance.ProductTypes.ToLuaList());
		_tables["ProductFamily"] = new ProfileLuaTable(_instance.ProductFamilies.ToLuaList());
		_tables["SubPlatform"] = new ProfileLuaTable(_instance.SubPlatforms.ToLuaList());
		_tables["ProductCatalog"] = new ProfileLuaTable(_instance.Titles.ToLuaList());
		_tables["ProductRating"] = new ProfileLuaTable(_instance.ProductRatings.ToLuaList());
		_tables["ProductGroup"] = new ProfileLuaTable(_instance.ProductGroups.ToLuaList());
		_tables["TitleRollupXREF"] = new ProfileLuaTable(_instance.TitleRollupXREF.ToLuaList());
		_tables["MultiDiscXREF"] = new ProfileLuaTable(_instance.MultiDiscXREF.ToLuaList());
	}
}
