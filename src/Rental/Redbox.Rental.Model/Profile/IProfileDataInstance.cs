using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public interface IProfileDataInstance
{
	string ImagesPath { get; set; }

	Dictionary<long, Banner> Banners { get; set; }

	Dictionary<long, Genre> Genres { get; set; }

	Dictionary<long, Market> Markets { get; set; }

	Dictionary<long, Title> Titles { get; set; }

	Dictionary<long, ProductFamily> ProductFamilies { get; set; }

	Dictionary<long, ProductGroup> ProductGroups { get; set; }

	Dictionary<long, ProductGroupXREF> TitleRollupXREF { get; set; }

	Dictionary<long, ProductGroupXREF> MultiDiscXREF { get; set; }

	Dictionary<long, ProductRating> ProductRatings { get; set; }

	Dictionary<long, ProductType> ProductTypes { get; set; }

	Dictionary<long, Store> Stores { get; set; }

	Dictionary<long, SubPlatform> SubPlatforms { get; set; }

	Dictionary<long, Vendor> Vendors { get; set; }

	List<ProfileVersion> ProfileVersion { get; set; }

	string OriginMachine { get; set; }

	DateTime GeneratedOn { get; set; }

	IProfileLuaView ProfileLuaView { get; set; }
}
