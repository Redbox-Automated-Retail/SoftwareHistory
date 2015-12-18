using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Kernel.Views.Browse;

public class LuaInterfacedBrowseControl : BrowseControl
{
	public bool isADAAccessible { get; set; }

	public Func<bool> IsShoppingCartFull { get; set; }

	public Func<LuaTable, bool> OnIsProductSelected { get; set; }

	public Func<LuaTable, bool> OnIsProductInStock { get; set; }

	public Func<LuaTable, bool> OnIsProductThinned { get; set; }

	public BrowseViewFilter BrowseViewFilter { get; set; }

	public BrowseSort BrowseSort { get; set; }

	public TitleFamily BrowseTitleFamily { get; set; }

	public Func<bool> IsTitleMarketingMode { get; set; }

	public Func<LuaTable, string> OnGetTestPrice { get; set; }

	public Func<LuaTable, string> OnGetEarlyReleaseDate { get; set; }

	public Func<bool> OnIsComingSoonOptinEnabled { get; set; }

	public Func<LuaTable, bool> OnIsComingSoonProductRegistered { get; set; }

	public Action<LuaTable> OnRegisterEmailReminder { get; set; }

	public Action<LuaTable> OnUnregisterEmailReminder { get; set; }

	public Action<LuaTable> OnLuaItemSelected { get; set; }

	public Action<string> OnLuaAddItemSelected { get; set; }

	public Action<int, LuaTable> OnLuaUpdateItemMapping { get; set; }

	public Action<int> OnLuaItemsShown { get; set; }

	public LuaInterfacedBrowseControl()
	{
		base.OnItemSelected += LuaInterfacedBrowseControl_OnItemSelected;
		base.OnAddItemSelected += LuaInterfacedBrowseControl_OnAddItemSelected;
		base.OnUpdateItemMapping += LuaInterfacedBrowseControl_OnUpdateItemMapping;
		base.OnBrowseItemShown += LuaInterfacedBrowseControl_OnBrowseItemShown;
	}

	public void ConfigureBrowseItem(IBrowseItem browseItem)
	{
		if (browseItem is IMovieOrGameBrowseItem movieOrGameBrowseItem)
		{
			ConfigureMovieOrGameBrowseItem(movieOrGameBrowseItem);
		}
		else if (browseItem is ICollectionBrowseItem browseItem2)
		{
			ConfigureCollectionBrowseItem(browseItem2);
		}
	}

	public override void NotifyItemAction(IBrowseItem browseItem, string action)
	{
		ProductBrowseItemActions productBrowseItemActions = ProductBrowseItemActions.None;
		switch ((ProductBrowseItemActions)Enum.Parse(typeof(ProductBrowseItemActions), action))
		{
		case ProductBrowseItemActions.RegisterEmailReminder:
			if (OnRegisterEmailReminder != null && browseItem is ILuaBasedBrowseItem { LuaObject: not null } luaBasedBrowseItem2)
			{
				OnRegisterEmailReminder(luaBasedBrowseItem2.LuaObject);
			}
			break;
		case ProductBrowseItemActions.UnregisterEmailReminder:
			if (OnUnregisterEmailReminder != null && browseItem is ILuaBasedBrowseItem { LuaObject: not null } luaBasedBrowseItem)
			{
				OnUnregisterEmailReminder(luaBasedBrowseItem.LuaObject);
			}
			break;
		default:
			base.NotifyItemAction(browseItem, action);
			break;
		}
	}

	public static void ConfigureCollectionBrowseItem(IBrowseItem browseItem)
	{
		if (!(browseItem is ICollectionBrowseItem collectionBrowseItem) || !(collectionBrowseItem.LuaObject["kiosk_products"] is LuaTable luaTable))
		{
			return;
		}
		int num = 1;
		List<Image> list = new List<Image>();
		while (list.Count() < 3 && num <= luaTable.Keys.Count)
		{
			if (luaTable[num] is LuaTable luaTable2)
			{
				bool result = false;
				bool.TryParse(string.Format("{0}", luaTable2["coming_soon"]), out result);
				if (!result)
				{
					string text = luaTable2["image_file"].ToString();
					Image image = null;
					if (DataServiceFunctions.DataServiceRegisterImage(text, text))
					{
						IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
						if (service != null)
						{
							image = service.GetImage(text);
						}
					}
					if (image == null)
					{
						image = ServiceLocator.Instance.GetService<IResourceBundleService>().GetBitmap("image_movie_missing_box_art");
					}
					list.Add(image);
				}
			}
			num++;
		}
		for (int i = list.Count; i < 3; i++)
		{
			Image bitmap = ServiceLocator.Instance.GetService<IResourceBundleService>().GetBitmap("image_movie_missing_box_art");
			list.Add(bitmap);
		}
		collectionBrowseItem.Image = ImageCombinerLibrary.BuildDynamicBrowseImage(list.ToArray());
	}

	private void ConfigureMovieOrGameBrowseItem(IMovieOrGameBrowseItem movieOrGameBrowseItem)
	{
		bool titleMarketingMode = IsTitleMarketingMode != null && IsTitleMarketingMode();
		bool flag = IsShoppingCartFull != null && IsShoppingCartFull();
		bool result = false;
		bool.TryParse(GetLuaValue(movieOrGameBrowseItem, "is_available"), out result);
		bool flag2 = OnIsProductSelected != null && OnIsProductSelected(movieOrGameBrowseItem.LuaObject);
		bool result2 = false;
		bool isProductThinned = OnIsProductThinned != null && OnIsProductThinned(movieOrGameBrowseItem.LuaObject);
		bool.TryParse(GetLuaValue(movieOrGameBrowseItem, "show_out_of_stock"), out result2);
		bool isInStock = GetIsInStock(movieOrGameBrowseItem);
		bool flag3 = GetLuaValue(movieOrGameBrowseItem, "product_id") == 1139.ToString();
		int result3 = 1;
		int.TryParse(GetLuaValue(movieOrGameBrowseItem, "product_type_id"), out result3);
		TitleType titleTypeFromProductTypeId = TitleProduct.GetTitleTypeFromProductTypeId(result3);
		bool flag4 = OnIsComingSoonOptinEnabled != null && OnIsComingSoonOptinEnabled();
		bool flag5 = flag4 && OnIsComingSoonProductRegistered != null && OnIsComingSoonProductRegistered(movieOrGameBrowseItem.LuaObject);
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		LuaTable luaTable = movieOrGameBrowseItem.LuaObject["genres"] as LuaTable;
		flag6 = luaTable != null && luaTable[1002] != null;
		flag7 = luaTable != null && luaTable[1025] != null;
		flag8 = luaTable != null && luaTable[1095] != null;
		ProductFlags flag9 = GetProductFlag(movieOrGameBrowseItem, flag, titleMarketingMode, flag3, isInStock, result, result2, flag2, flag6, flag7, flag8, isProductThinned);
		movieOrGameBrowseItem.IsComingSoonOptinEnabled = flag4;
		movieOrGameBrowseItem.IsEmailReminderRegistered = flag5;
		movieOrGameBrowseItem.IsSelected = flag2;
		movieOrGameBrowseItem.IsInStock = isInStock;
		BannerType bannerType = BannerType.None;
		if (BrowseSort != BrowseSort.Alpha)
		{
			if (BrowseViewFilter == BrowseViewFilter.ComingSoon && base.BrowseSetName == BrowseSetNames.Main.ToString())
			{
				bannerType = BannerType.ComingSoon;
				string text = OnGetEarlyReleaseDate(movieOrGameBrowseItem.LuaObject);
				if (text != null && text.Length >= 8 && DateFunctions.ParseDateString(text, out var date))
				{
					movieOrGameBrowseItem.BannerText = string.Format("{1} {0:M/d}", date, BundleFunctions.GetDictionaryStringValue("strings", "coming_label"));
				}
				if (flag5)
				{
					movieOrGameBrowseItem.EmailOptinButtonText = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_reminder_set_button_text"));
				}
				else
				{
					movieOrGameBrowseItem.EmailOptinButtonText = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_button_text"));
				}
			}
			else if (titleTypeFromProductTypeId == TitleType.Bluray)
			{
				bannerType = BannerType.Bluray;
			}
		}
		if (bannerType != BannerType.None)
		{
			movieOrGameBrowseItem.Banner = BannerCollection.Instance.Find((Banner x) => x.BannerType == bannerType);
		}
		movieOrGameBrowseItem.AllowAddEvenIfSelected = flag3;
		movieOrGameBrowseItem.ShowAddButton = (!flag & result) && (!movieOrGameBrowseItem.IsSelected || movieOrGameBrowseItem.AllowAddEvenIfSelected) && movieOrGameBrowseItem.IsInStock && (BrowseViewFilter != BrowseViewFilter.ComingSoon || base.BrowseSetName != BrowseSetNames.Main.ToString());
		movieOrGameBrowseItem.ShowIndexOfAddButton = isADAAccessible;
		movieOrGameBrowseItem.ShowFlag = BrowseSort != BrowseSort.Alpha;
		if ((BrowseViewFilter != BrowseViewFilter.ComingSoon || base.BrowseSetName != BrowseSetNames.Main.ToString()) && (flag || !result || (movieOrGameBrowseItem.IsSelected && !movieOrGameBrowseItem.AllowAddEvenIfSelected) || !movieOrGameBrowseItem.IsInStock))
		{
			if (BrowseSort == BrowseSort.Alpha)
			{
				movieOrGameBrowseItem.ImageOpacity = 0.2f;
			}
			else
			{
				movieOrGameBrowseItem.ImageOpacity = 0.4f;
			}
		}
		else
		{
			movieOrGameBrowseItem.ImageOpacity = 1f;
		}
		if (flag9 != ProductFlags.None)
		{
			ProductFlag productFlag = ProductFlagCollection.Instance.Find((ProductFlag x) => x.Flag == flag9);
			if (productFlag != null)
			{
				movieOrGameBrowseItem.Flag = productFlag;
			}
		}
	}

	private string GetLuaValue(IMovieOrGameBrowseItem movieOrGameBrowseItem, string propertyName)
	{
		string result = null;
		object obj = movieOrGameBrowseItem.LuaObject[propertyName];
		if (obj != null)
		{
			result = obj.ToString();
		}
		return result;
	}

	private ProductFlags GetProductFlag(IMovieOrGameBrowseItem movieOrGameBrowseItem, bool shoppingCartFull, bool titleMarketingMode, bool isEmptyCase, bool inStock, bool isAvailable, bool showOutOfStock, bool isProductSelected, bool isAwardWinner, bool isRedboxReplayMovie, bool isClassic, bool isProductThinned)
	{
		ProductFlags result = ProductFlags.None;
		DateTime date = DateTime.Now;
		DateFunctions.ParseDateString(movieOrGameBrowseItem.LuaObject["merchandise_date"].ToString(), out date);
		double totalDays = (DateTime.Now - date).TotalDays;
		DateTime date2 = DateTime.Now;
		DateFunctions.ParseDateString(movieOrGameBrowseItem.LuaObject["sort_date"].ToString(), out date2);
		_ = (DateTime.Now - date2).TotalDays;
		if (!shoppingCartFull && !titleMarketingMode)
		{
			if (isEmptyCase && !inStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!isAvailable)
			{
				result = ProductFlags.ComingSoon;
			}
			else if (!inStock & showOutOfStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!isProductSelected)
			{
				if (isAwardWinner)
				{
					result = ProductFlags.AwardWinner;
				}
				else
				{
					string text = ((OnGetTestPrice != null) ? OnGetTestPrice(movieOrGameBrowseItem.LuaObject) : null);
					if (!string.IsNullOrEmpty(text))
					{
						movieOrGameBrowseItem.FlagText = text;
						result = ProductFlags.PriceTest;
					}
					else if (((totalDays <= 14.0) & isRedboxReplayMovie) && BrowseTitleFamily != TitleFamily.Games)
					{
						result = ProductFlags.BackAgain;
					}
					else if (((totalDays <= 14.0) & isClassic) && BrowseTitleFamily != TitleFamily.Movies)
					{
						result = ProductFlags.Classics;
					}
					else if (totalDays <= 28.0)
					{
						result = ProductFlags.New;
					}
					else if ((totalDays >= 105.0 && BrowseTitleFamily != TitleFamily.Games) & isProductThinned)
					{
						result = ProductFlags.LastChance;
					}
				}
			}
		}
		return result;
	}

	private bool GetIsInStock(IMovieOrGameBrowseItem movieOrGameBrowseItem)
	{
		bool result = false;
		if (movieOrGameBrowseItem is ITitleRollupBrowseItem)
		{
			LuaTable luaTable = movieOrGameBrowseItem.LuaObject["kiosk_products"] as LuaTable;
			int result2 = 0;
			int.TryParse(movieOrGameBrowseItem.LuaObject["dvd_index"].ToString(), out result2);
			int result3 = 0;
			int.TryParse(movieOrGameBrowseItem.LuaObject["blu_ray_index"].ToString(), out result3);
			if (luaTable != null && OnIsProductInStock != null)
			{
				LuaTable luaTable2 = luaTable[result2] as LuaTable;
				LuaTable luaTable3 = luaTable[result3] as LuaTable;
				result = (luaTable2 != null && OnIsProductInStock(luaTable2)) || (luaTable3 != null && OnIsProductInStock(luaTable3));
			}
		}
		else
		{
			result = OnIsProductInStock != null && OnIsProductInStock(movieOrGameBrowseItem.LuaObject);
		}
		return result;
	}

	private void LuaInterfacedBrowseControl_OnBrowseItemShown(List<IBrowseItem> browseItems)
	{
		if (OnLuaItemsShown != null && browseItems != null)
		{
			OnLuaItemsShown(browseItems.Count((IBrowseItem x) => !(x is IGroupHeaderBrowseItem)));
		}
	}

	private void LuaInterfacedBrowseControl_OnAddItemSelected(IBrowseItem browseItem, string actorName)
	{
		if (browseItem is LuaBasedBrowseItem && OnLuaAddItemSelected != null)
		{
			OnLuaAddItemSelected(actorName);
		}
	}

	private void LuaInterfacedBrowseControl_OnUpdateItemMapping(int index, IBrowseItem browseItem)
	{
		if (browseItem is LuaBasedBrowseItem luaBasedBrowseItem && OnLuaUpdateItemMapping != null)
		{
			OnLuaUpdateItemMapping(index, luaBasedBrowseItem.LuaObject);
		}
	}

	private void LuaInterfacedBrowseControl_OnItemSelected(IBrowseItem browseItem)
	{
		if (browseItem is LuaBasedBrowseItem luaBasedBrowseItem && OnLuaItemSelected != null)
		{
			OnLuaItemSelected(luaBasedBrowseItem.LuaObject);
		}
	}
}
