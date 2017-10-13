using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class CollectionProduct : KioskProduct, ICollectionProduct, IKioskProduct
{
	protected List<IKioskProduct> _kioskProducts = new List<IKioskProduct>();

	private Dictionary<long, ITitleProduct> _allTitleProducts;

	private Dictionary<long, MDVProduct> _allMDVProducts;

	private Dictionary<long, TitleRollupProduct> _allTitleRollupProducts;

	private IProfileDataInstance _profile;

	public List<IKioskProduct> KioskProducts => _kioskProducts;

	private Collection _collection { get; set; }

	public override string ProductName => _collection.Name;

	public override string ProductDescription => _collection.Name;

	public string ImageFile { get; set; }

	public long Id => _collection.Id;

	public string Name => _collection.Name;

	public int SortOrder => _collection.SortOrder;

	public List<long> Titles => _collection.Titles;

	public DateTime? EffectiveStartDate => _collection.EffectiveStartDate;

	public DateTime? EffectiveEndDate => _collection.EffectiveEndDate;

	public string LegalInfo => _collection.LegalInfo;

	public bool IsTop20 => _collection.IsTop20;

	public bool CanShow
	{
		get
		{
			if (!IsValidProduct)
			{
				return false;
			}
			if (EffectiveStartDate.HasValue)
			{
				DateTime now = DateTime.Now;
				DateTime? effectiveStartDate = EffectiveStartDate;
				if (now < effectiveStartDate)
				{
					return false;
				}
			}
			if (EffectiveEndDate.HasValue)
			{
				DateTime now = DateTime.Now;
				DateTime? effectiveStartDate = EffectiveEndDate;
				if (now > effectiveStartDate)
				{
					return false;
				}
			}
			List<IKioskProduct> availableKioskProducts = AvailableKioskProducts;
			if (availableKioskProducts.Count == 0)
			{
				return false;
			}
			if (availableKioskProducts.ToTitleProducts().Count((ITitleProduct p) => p.InStock) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public List<IKioskProduct> AvailableKioskProducts
	{
		get
		{
			List<ITitleProduct> source = (from p in _kioskProducts.ToTitleProducts()
				where (p.IsRentable || p.IsSellable) && (p.InStock || p.ShowOutOfStock)
				select p).ToList();
			List<ITitleProduct> list = source.Where((ITitleProduct p) => p.InStock).ToList();
			List<ITitleProduct> collection = source.Where((ITitleProduct p) => !p.InStock).ToList();
			list.AddRange(collection);
			return list.ToKioskProducts();
		}
	}

	public CollectionProduct(Collection collection, Dictionary<long, ITitleProduct> titleProducts, Dictionary<long, MDVProduct> mdvProducts, Dictionary<long, TitleRollupProduct> titleRollupProducts, IProfileDataInstance profile, Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.Collection;
		_collection = collection;
		_allTitleProducts = titleProducts;
		_allMDVProducts = mdvProducts;
		_allTitleRollupProducts = titleRollupProducts;
		_profile = profile;
		LoadKioskProducts();
	}

	private void LoadKioskProducts()
	{
		_collection.Titles.ForEach(delegate(long id)
		{
			if (_allTitleProducts.ContainsKey(id))
			{
				ITitleProduct title = _allTitleProducts[id];
				TitleRollupProduct titleRollupProduct = (from item in _allTitleRollupProducts
					where item.Value.KioskProducts.Contains(title)
					select item.Value).FirstOrDefault();
				if (titleRollupProduct != null)
				{
					if (!_kioskProducts.Contains(titleRollupProduct))
					{
						titleRollupProduct.Collections.Add(_collection.Id);
						_kioskProducts.Add(titleRollupProduct);
					}
				}
				else
				{
					MDVProduct mDVProduct = (from item in _allMDVProducts
						where item.Value.TitleProducts.Contains(title)
						select item.Value).FirstOrDefault();
					if (mDVProduct != null)
					{
						if (!_kioskProducts.Contains(mDVProduct))
						{
							mDVProduct.Collections.Add(_collection.Id);
							_kioskProducts.Add(mDVProduct);
						}
					}
					else
					{
						title.Collections.Add(_collection.Id);
						_kioskProducts.Add(title);
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("CollectionProduct.LoadKioskProducts - ProductGroupId {0} has a product id {1} that does not exist.", _collection.Id, id);
			}
		});
		if (_kioskProducts.Count() == 0)
		{
			IsValidProduct = false;
		}
	}

	public bool GetInCartCanShow(TitleFamily titleFamily, TitleType titleType, List<long> include, List<long> exclude)
	{
		if (!CanShow)
		{
			return false;
		}
		List<IKioskProduct> inCartAvailableKioskProducts = GetInCartAvailableKioskProducts(titleFamily, titleType, include, exclude);
		if (inCartAvailableKioskProducts.Count == 0)
		{
			return false;
		}
		if (inCartAvailableKioskProducts.ToTitleProducts().Count((ITitleProduct p) => p.InStock) == 0)
		{
			return false;
		}
		return true;
	}

	public List<IKioskProduct> GetInCartAvailableKioskProducts(TitleFamily titleFamily, TitleType titleType, List<long> include, List<long> exclude)
	{
		return AvailableKioskProducts.ToTitleProducts().InCartTitleProductFilters(titleFamily, titleType, include, exclude).ToKioskProducts();
	}
}
