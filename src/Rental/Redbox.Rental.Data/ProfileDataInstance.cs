using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Data;

public class ProfileDataInstance : IProfileDataInstance
{
	private Dictionary<long, Banner> _banners = new Dictionary<long, Banner>();

	private Dictionary<long, Genre> _genres = new Dictionary<long, Genre>();

	private Dictionary<long, Market> _markets = new Dictionary<long, Market>();

	private Dictionary<long, Title> _titles = new Dictionary<long, Title>();

	private Dictionary<long, ProductFamily> _productFamilies = new Dictionary<long, ProductFamily>();

	private Dictionary<long, ProductGroup> _productGroups = new Dictionary<long, ProductGroup>();

	private Dictionary<long, ProductGroupXREF> _titleRollupXREF = new Dictionary<long, ProductGroupXREF>();

	private Dictionary<long, ProductGroupXREF> _multiDiscXREF = new Dictionary<long, ProductGroupXREF>();

	private Dictionary<long, ProductRating> _productRatings = new Dictionary<long, ProductRating>();

	private Dictionary<long, ProductType> _productTypes = new Dictionary<long, ProductType>();

	private Dictionary<long, Store> _stores = new Dictionary<long, Store>();

	private Dictionary<long, SubPlatform> _subPlatforms = new Dictionary<long, SubPlatform>();

	private Dictionary<long, Vendor> _vendors = new Dictionary<long, Vendor>();

	private List<ProfileVersion> _profileVersion = new List<ProfileVersion>();

	public string ImagesPath { get; set; }

	public Dictionary<long, Banner> Banners
	{
		get
		{
			return _banners;
		}
		set
		{
			_banners = value;
		}
	}

	public Dictionary<long, Genre> Genres
	{
		get
		{
			return _genres;
		}
		set
		{
			_genres = value;
		}
	}

	public Dictionary<long, Market> Markets
	{
		get
		{
			return _markets;
		}
		set
		{
			_markets = value;
		}
	}

	public Dictionary<long, Title> Titles
	{
		get
		{
			return _titles;
		}
		set
		{
			_titles = value;
		}
	}

	public Dictionary<long, ProductFamily> ProductFamilies
	{
		get
		{
			return _productFamilies;
		}
		set
		{
			_productFamilies = value;
		}
	}

	public Dictionary<long, ProductGroup> ProductGroups
	{
		get
		{
			return _productGroups;
		}
		set
		{
			_productGroups = value;
		}
	}

	public Dictionary<long, ProductGroupXREF> TitleRollupXREF
	{
		get
		{
			return _titleRollupXREF;
		}
		set
		{
			_titleRollupXREF = value;
		}
	}

	public Dictionary<long, ProductGroupXREF> MultiDiscXREF
	{
		get
		{
			return _multiDiscXREF;
		}
		set
		{
			_multiDiscXREF = value;
		}
	}

	public Dictionary<long, ProductGroupXREF> MultiDiscWorkaroundXREF { get; set; } = new Dictionary<long, ProductGroupXREF>();

	public Dictionary<long, ProductRating> ProductRatings
	{
		get
		{
			return _productRatings;
		}
		set
		{
			_productRatings = value;
		}
	}

	public Dictionary<long, ProductType> ProductTypes
	{
		get
		{
			return _productTypes;
		}
		set
		{
			_productTypes = value;
		}
	}

	public Dictionary<long, Store> Stores
	{
		get
		{
			return _stores;
		}
		set
		{
			_stores = value;
		}
	}

	public Dictionary<long, SubPlatform> SubPlatforms
	{
		get
		{
			return _subPlatforms;
		}
		set
		{
			_subPlatforms = value;
		}
	}

	public Dictionary<long, Vendor> Vendors
	{
		get
		{
			return _vendors;
		}
		set
		{
			_vendors = value;
		}
	}

	public List<ProfileVersion> ProfileVersion
	{
		get
		{
			return _profileVersion;
		}
		set
		{
			_profileVersion = value;
		}
	}

	public string OriginMachine { get; set; }

	public DateTime GeneratedOn { get; set; }

	public IProfileLuaView ProfileLuaView { get; set; }
}
