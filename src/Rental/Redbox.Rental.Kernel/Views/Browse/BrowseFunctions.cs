using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel.Views.Browse;

public static class BrowseFunctions
{
	private static Dictionary<string, LuaInterfacedBrowseControl> _browseControls = new Dictionary<string, LuaInterfacedBrowseControl>();

	private static Dictionary<string, MiniCartControl> _miniCartControls = new Dictionary<string, MiniCartControl>();

	private static Dictionary<string, AccessibilityControl> _accessibilityControls = new Dictionary<string, AccessibilityControl>();

	private static Dictionary<string, TopMenuControl> _topMenuControls = new Dictionary<string, TopMenuControl>();

	private static Dictionary<string, BrowseGroupSelectionControl> _browseGroupSelectionControls = new Dictionary<string, BrowseGroupSelectionControl>();

	private static Dictionary<string, BrowseFilterBarControl> _browseFilterBarControls = new Dictionary<string, BrowseFilterBarControl>();

	internal static bool IsADAMode => ApplicationState.Instance?.IsADAAccessible ?? false;

	internal static bool IsSpanish => BundleFunctions.GetCulture() == "es-MX";

	internal static bool IsForSaleButtonEnabled
	{
		get
		{
			if (Session.Instance.BrowseState.TitleFamily == TitleFamily.Movies)
			{
				return RentalDataFunctions.GetCanSellUsedMovies();
			}
			if (Session.Instance.BrowseState.TitleFamily == TitleFamily.Games)
			{
				return RentalDataFunctions.GetCanSellUsedGames();
			}
			return false;
		}
	}

	[KernelFunction(Name = "kernel.reset_browse_controls")]
	internal static void ResetBrowseControls()
	{
		_browseControls = new Dictionary<string, LuaInterfacedBrowseControl>();
		_miniCartControls = new Dictionary<string, MiniCartControl>();
		_accessibilityControls = new Dictionary<string, AccessibilityControl>();
		_topMenuControls = new Dictionary<string, TopMenuControl>();
		_browseGroupSelectionControls = new Dictionary<string, BrowseGroupSelectionControl>();
		_browseFilterBarControls = new Dictionary<string, BrowseFilterBarControl>();
	}

	[KernelFunction(Name = "kernel.draw_images_set")]
	internal static void DrawImagesSet(string browseSetName, string browseFilterSelectedFamily, LuaTable products, int pageNumber, Action<int, string> onPageNumberChanged, Action<int> onProductsShown, Action<LuaTable> onItemSelected, Action<string> onAddItemSelected, Action<int, LuaTable> onUpdateItemMapping, Func<LuaTable, bool> onIsProductSelected, Func<LuaTable, bool> onIsProductInStock, Func<bool> isShoppingCartFull, Func<LuaTable, string> onGetTestPrice, Func<LuaTable, string> onGetEarlyReleaseDate, Func<bool> isTitleMarketingMode, Func<bool> onIsComingSoonOptinEnabled, Func<LuaTable, bool> onIsComingSoonProductRegistered, Action<LuaTable> onRegisterEmailReminder, Action<LuaTable> onUnregisterEmailReminder)
	{
		ClearOldActors(browseSetName);
		BrowseViewFilter browseMode = Session.Instance.BrowseState.BrowseMode;
		BrowseSort browseSort = Session.Instance.BrowseState.BrowseSelectedSort.ConvertToBrowseSort(browseMode);
		TitleFamily titleFamily = browseFilterSelectedFamily.ConvertToTitleFamily();
		List<IBrowseItem> browseItems = CreateBrowseItems(products, browseSort == BrowseSort.Alpha);
		bool flag = browseSort == BrowseSort.Alpha && browseSetName == BrowseSetNames.Main.ToString();
		LuaInterfacedBrowseControl productBrowseControl;
		if (browseSetName == BrowseSetNames.Collections.ToString())
		{
			productBrowseControl = CreateCollectionsBrowseView(browseItems);
		}
		else if (flag)
		{
			if (IsADAMode)
			{
				productBrowseControl = CreateABEAToZBrowseView(browseItems);
			}
			else
			{
				productBrowseControl = CreateAToZBrowseView(browseItems);
			}
		}
		else
		{
			productBrowseControl = CreateNormalBrowseView(browseItems);
		}
		if (productBrowseControl == null)
		{
			return;
		}
		productBrowseControl.BrowseSetName = browseSetName;
		productBrowseControl.isADAAccessible = IsADAMode;
		productBrowseControl.ShowPaddleShadow = true;
		productBrowseControl.OnLuaItemSelected = onItemSelected;
		productBrowseControl.OnLuaAddItemSelected = onAddItemSelected;
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = productBrowseControl;
		luaInterfacedBrowseControl.OnPageNumberChanged = (Action<int, PageChangeSource>)Delegate.Combine(luaInterfacedBrowseControl.OnPageNumberChanged, (Action<int, PageChangeSource>)delegate(int newPageNumber, PageChangeSource pageChangeSource)
		{
			if (onPageNumberChanged != null)
			{
				onPageNumberChanged(newPageNumber, pageChangeSource.ToString());
			}
		});
		productBrowseControl.OnLuaItemsShown = onProductsShown;
		productBrowseControl.OnLuaUpdateItemMapping = onUpdateItemMapping;
		SetPageNumber(pageNumber, productBrowseControl);
		_browseControls[browseSetName] = productBrowseControl;
		productBrowseControl.IsShoppingCartFull = isShoppingCartFull;
		productBrowseControl.OnIsProductSelected = onIsProductSelected;
		productBrowseControl.OnIsProductInStock = onIsProductInStock;
		productBrowseControl.OnIsProductThinned = IsProductThinned;
		productBrowseControl.OnGetTestPrice = onGetTestPrice;
		productBrowseControl.OnGetEarlyReleaseDate = onGetEarlyReleaseDate;
		productBrowseControl.IsTitleMarketingMode = isTitleMarketingMode;
		productBrowseControl.BrowseViewFilter = browseMode;
		productBrowseControl.BrowseSort = browseSort;
		productBrowseControl.BrowseTitleFamily = titleFamily;
		productBrowseControl.OnIsComingSoonOptinEnabled = onIsComingSoonOptinEnabled;
		productBrowseControl.OnIsComingSoonProductRegistered = onIsComingSoonProductRegistered;
		productBrowseControl.OnRegisterEmailReminder = onRegisterEmailReminder;
		productBrowseControl.OnUnregisterEmailReminder = onUnregisterEmailReminder;
		browseItems.ForEach(delegate(IBrowseItem x)
		{
			x.OnBeforeRender += productBrowseControl.ConfigureBrowseItem;
		});
		BundleFunctions.GetDictionary("strings");
		productBrowseControl.NoResultsText = string.Format("{0}", (titleFamily == TitleFamily.Games) ? BundleFunctions.GetDictionaryStringValue("strings", "filter_games_empty") : BundleFunctions.GetDictionaryStringValue("strings", "filter_movies_empty"));
		productBrowseControl.Render();
		if (!flag)
		{
			return;
		}
		ClearOldBrowseGroupSelectionControlActors(browseSetName);
		List<string> list = new List<string> { "#" };
		for (int num = 65; num < 91; num++)
		{
			list.Add(((char)num).ToString());
		}
		List<IBrowseGroup> list2 = new List<IBrowseGroup>();
		foreach (string eachGroupName in list)
		{
			bool enabled = browseItems.Any((IBrowseItem x) => x is IGroupHeaderBrowseItem && ((IGroupHeaderBrowseItem)x).Name == eachGroupName);
			list2.Add(new BrowseGroup
			{
				Name = eachGroupName,
				Width = 35,
				Height = 35,
				Enabled = enabled,
				IsSelected = (eachGroupName == Session.Instance.BrowseState.SelectedAlphaGroupName && browseItems.Any((IBrowseItem x) => x is IGroupHeaderBrowseItem && ((IGroupHeaderBrowseItem)x).Name == eachGroupName))
			});
		}
		BrowseGroupSelectionControl browseGroupSelectionControl = new BrowseGroupSelectionControl
		{
			X = 30,
			Y = (IsADAMode ? 510 : 85),
			BrowseGroups = list2
		};
		_browseGroupSelectionControls[browseSetName] = browseGroupSelectionControl;
		browseGroupSelectionControl.OnGroupSelected += delegate(IBrowseGroup browseGroup)
		{
			string value = null;
			if (browseGroup.Enabled)
			{
				foreach (IBrowseItem item in productBrowseControl.Items)
				{
					if (item is IGroupHeaderBrowseItem groupHeaderBrowseItem)
					{
						groupHeaderBrowseItem.IsSelected = false;
					}
				}
				if (productBrowseControl.Items.FirstOrDefault((IBrowseItem x) => x.Name == browseGroup.Name) is IGroupHeaderBrowseItem groupHeaderBrowseItem2)
				{
					groupHeaderBrowseItem2.IsSelected = true;
					productBrowseControl.ScrollToGroupHeaderBrowseItemPage(groupHeaderBrowseItem2);
				}
				Session.Instance.BrowseState.SelectedAlphaGroupName = browseGroup?.Name;
			}
			else if (!string.IsNullOrEmpty(browseGroup?.Name) && !browseItems.Any((IBrowseItem x) => x.Name == browseGroup?.Name))
			{
				value = $"There are no titles starting with the letter {browseGroup?.Name}.";
			}
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service != null && service.TTSEnabled)
			{
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				if (service2 != null)
				{
					service2["TTS:SelectedAlphaGroupEmptyMessage"] = value;
				}
			}
		};
		browseGroupSelectionControl.Render();
	}

	[KernelFunction(Name = "kernel.get_visible_browse_products")]
	internal static LuaTable GetVisibleBrowseProducts(string browseSetName)
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		LuaInterfacedBrowseControl value = null;
		_browseControls.TryGetValue(browseSetName, out value);
		if (value != null && value.LastRenderedBrowseItems != null)
		{
			int num = 1;
			foreach (IBrowseItem lastRenderedBrowseItem in value.LastRenderedBrowseItems)
			{
				luaTable[num++] = lastRenderedBrowseItem.Data as LuaTable;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.get_product_from_actor")]
	internal static LuaTable GetProductFromActor(string actorName)
	{
		LuaTable result = null;
		IActor actor = ActorFunctions.GetActor(actorName);
		if (actor != null && actor.Tag != null && actor.Tag is IBrowseItem browseItem)
		{
			result = browseItem.Data as LuaTable;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.draw_action_bar")]
	internal static void DrawActionBar(int shoppingCartItemCount, Action onCheckoutButtonClick, bool showNoThanks)
	{
		ActionBarControl actionBarControl = new ActionBarControl();
		actionBarControl.X = 0;
		actionBarControl.Y = 622;
		actionBarControl.Height = 146;
		actionBarControl.Width = 1024;
		actionBarControl.NumberOfCartItems = shoppingCartItemCount;
		actionBarControl.ShowNoThanks = showNoThanks;
		actionBarControl.OnCheckoutButtonClick += delegate
		{
			onCheckoutButtonClick();
		};
		actionBarControl.Render();
	}

	[KernelFunction(Name = "kernel.draw_mini_cart")]
	internal static void DrawMiniCart(string browseSetName, LuaTable shoppingCartItems, bool inCartTitleMarketing, Action<LuaTable> onRemoveProduct, bool addToCartLabelEnabled)
	{
		ClearOldMiniCartActors(browseSetName);
		List<IBrowseItem> items = CreateBrowseItemsFromShoppingCartItems(shoppingCartItems, inCartTitleMarketing);
		MiniCartControl miniCartControl = new MiniCartControl
		{
			BrowseSetName = browseSetName,
			Items = items,
			Width = 580,
			Height = 118,
			Y = 636,
			X = 208,
			AddToCartLabelEnabled = addToCartLabelEnabled
		};
		miniCartControl.OnCancelItem += delegate(IBrowseItem browseItem)
		{
			if (browseItem is IMovieOrGameBrowseItem movieOrGameBrowseItem)
			{
				onRemoveProduct(movieOrGameBrowseItem.LuaObject);
			}
		};
		_miniCartControls[browseSetName] = miniCartControl;
		miniCartControl.Render();
	}

	[KernelFunction(Name = "kernel.draw_accessibility_control")]
	internal static void DrawAccessibilityControl(string browseSetName, LuaTable products, Action<string> onAddItemSelected, Action onSwitchProductFamily, string selectedTitleFamily)
	{
		ClearOldAccessibilityControlActors(browseSetName);
		TitleFamily selectedTitleFamily2 = (TitleFamily)Enum.Parse(typeof(TitleFamily), selectedTitleFamily);
		LuaInterfacedBrowseControl value = null;
		_browseControls.TryGetValue(BrowseSetNames.Main.ToString(), out value);
		if (value == null)
		{
			return;
		}
		BrowseViewFilter browseMode = Session.Instance.BrowseState.BrowseMode;
		BrowseSort browseSort = Session.Instance.BrowseState.BrowseSelectedSort.ConvertToBrowseSort(browseMode);
		List<IBrowseItem> items = CreateBrowseItems(products, browseSort == BrowseSort.Alpha);
		AccessibilityControl accessibilityControl = new AccessibilityControl
		{
			BrowseSetName = browseSetName,
			SourceBrowseControl = _browseControls[BrowseSetNames.Main.ToString()],
			SelectedTitleFamily = selectedTitleFamily2,
			Items = items,
			Width = 526,
			Height = 146,
			Y = 622,
			X = 230,
			InvertRowsAndColumns = (browseSort == BrowseSort.Alpha)
		};
		accessibilityControl.OnAddItemSelected += delegate(IBrowseItem browseItem, string actorName)
		{
			if (onAddItemSelected != null)
			{
				onAddItemSelected(actorName);
			}
		};
		accessibilityControl.OnSwitchProductFamily += delegate
		{
			if (onSwitchProductFamily != null)
			{
				onSwitchProductFamily();
			}
		};
		_accessibilityControls[browseSetName] = accessibilityControl;
		accessibilityControl.Render();
	}

	[KernelFunction(Name = "kernel.draw_collections_popup_dialog")]
	internal static void DrawCollectionsPopUpDialog(int x, int y, int width, int height, Action onSeeAllCollections, Action<LuaTable> onCollectionSelected, Action onCancel)
	{
		LuaTable luaTable = RentalDataFunctions.RentalGetCollectionProducts(null, null, null, null);
		List<IBrowseItem> source = CreateBrowseItems(luaTable, createAlphaGroups: false);
		List<IBrowseItem> list = new List<IBrowseItem>();
		list.AddRange(source.Take(2));
		if (list.Count < 2)
		{
			foreach (object key in luaTable.Keys)
			{
				if (!string.IsNullOrEmpty(luaTable[key].ToString()))
				{
					LuaTable luaTable2 = luaTable[key] as LuaTable;
					if (luaTable2["is_top_20"] != null && (bool)luaTable2["is_top_20"])
					{
						list.Add(new CollectionBrowseItem(luaTable2));
						break;
					}
				}
			}
		}
		list.ForEach(delegate(IBrowseItem browseItem)
		{
			browseItem.OnBeforeRender += LuaInterfacedBrowseControl.ConfigureCollectionBrowseItem;
		});
		CollectionsPopUpDialogControl collectionsPopUpDialogControl = new CollectionsPopUpDialogControl
		{
			Position = new Rectangle(x, y, width, height),
			BrowseItems = list
		};
		collectionsPopUpDialogControl.OnCancel += delegate
		{
			collectionsPopUpDialogControl.ClearActors();
			if (onCancel != null)
			{
				onCancel();
			}
		};
		collectionsPopUpDialogControl.OnSeeAllCollections += delegate
		{
			collectionsPopUpDialogControl.ClearActors();
			if (onSeeAllCollections != null)
			{
				onSeeAllCollections();
			}
		};
		collectionsPopUpDialogControl.OnCollectionSelected += delegate(IBrowseItem browseItem)
		{
			collectionsPopUpDialogControl.ClearActors();
			ILuaBasedBrowseItem luaBasedBrowseItem = browseItem as ILuaBasedBrowseItem;
			if (onCollectionSelected != null && luaBasedBrowseItem != null && luaBasedBrowseItem.LuaObject != null)
			{
				onCollectionSelected(luaBasedBrowseItem.LuaObject);
			}
		};
		collectionsPopUpDialogControl.ClearActors();
		collectionsPopUpDialogControl.Render();
	}

	[KernelFunction(Name = "kernel.draw_browse_filter_bar")]
	internal static void DrawBrowseFilterBar(string browseViewName, Action startOverAction, Action browseFilterButtonAction)
	{
		ClearOldBrowseFilterBarControlActors(browseViewName);
		IBrowseState browseState = Session.Instance.BrowseState;
		IConfiguration instance = Configuration.Instance;
		BrowseFilterBarControl browseFilterBarControl = new BrowseFilterBarControl
		{
			X = 0,
			Y = 563,
			Height = 59,
			Width = 1024,
			StartOverAction = startOverAction,
			UpdateBrowseViewAction = browseFilterButtonAction,
			Buttons = CreateBrowseFilterButtons(instance),
			ButtonSpacing = ((IsADAMode || browseState.TitleFamily == TitleFamily.Games) ? 8 : 16)
		};
		BrowseFilterBarControl browseFilterBarControl2 = browseFilterBarControl;
		browseFilterBarControl2.BrowseFilterButtonAction = (BrowseFilterButtonEventHandler)Delegate.Combine(browseFilterBarControl2.BrowseFilterButtonAction, new BrowseFilterButtonEventHandler(HandleFilterButtonClick));
		int num = 160 + (864 - browseFilterBarControl.Buttons.Sum((BrowseFilterButton x) => x.Width + browseFilterBarControl.ButtonSpacing)) / 2;
		browseFilterBarControl.ButtonLeftOffset = ((browseState.TitleFamily == TitleFamily.Movies && !IsADAMode) ? (num - 20) : num);
		_browseFilterBarControls[browseViewName] = browseFilterBarControl;
		browseFilterBarControl.Render();
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			return;
		}
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (service2 != null)
		{
			service2["TTS:BrowsingFilter"] = Session.Instance.BrowseState.BrowseSelectedSort;
			service2["TTS:BrowsingFamilySingular"] = ((browseState.TitleFamily == TitleFamily.Movies) ? "movie" : "game");
			service2["TTS:SwitchToFamily"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "Games" : "Movies");
			service2["TTS:BrowsingFamily"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "Movies" : "Games");
			service2["TTS:FilterOptions"] = "Various Options";
			service2["TTS:TabName_2"] = ((browseState.TitleFamily == TitleFamily.Movies) ? string.Format("{0} {1}", BundleFunctions.GetDictionaryStringValue("strings", "genre_1093a"), BundleFunctions.GetDictionaryStringValue("strings", "genre_1093b")) : BundleFunctions.GetDictionaryStringValue("strings", "genre_1026"));
			service2["TTS:TabName_3"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "genre_1000" : "genre_1006");
			service2["TTS:TabName_4"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "genre_1005" : "genre_1028");
			service2["TTS:TabName_5"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "genre_1009" : "genre_1030");
			service2["TTS:TabName_6"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "genre_1004" : "genre_1034");
			service2["TTS:TabName_7"] = BundleFunctions.GetDictionaryStringValue("strings", (browseState.TitleFamily == TitleFamily.Movies) ? "genre_1006" : "genre_1036");
			for (int num2 = 0; num2 < 10; num2++)
			{
				BrowseFilterButton browseFilterButton = ((browseFilterBarControl.Buttons.Count > num2) ? browseFilterBarControl.Buttons[num2] : null);
				service2[$"TTS:FilterName_{num2 + 1}"] = ((browseFilterButton != null) ? BundleFunctions.GetDictionaryStringValue("strings", browseFilterButton.Name) : null);
				service2[$"TTS:FilterButton_{num2 + 1}"] = browseFilterButton?.Name;
			}
		}
		service.TTSRepeatSequence();
	}

	internal static void HandleFilterButtonClick(BrowseFilterBarControl sender, BrowseFilterButton browseFilterButton)
	{
		if (IsADAMode)
		{
			Session.Instance.BrowseState.Genre = null;
		}
		switch (browseFilterButton.ButtonType)
		{
		case BrowseFilterButtonType.AllNewest:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Newest", TitleType.All, "browse-filter-family,Newest", 1);
			break;
		case BrowseFilterButtonType.BluRay:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Blu-ray", TitleType.Bluray, "browse-filter-family,Blu-ray", 1);
			break;
		case BrowseFilterButtonType.ForSale:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.ForSale, "for-sale", TitleType.All, "browse-filter-family,for-sale", 1);
			break;
		case BrowseFilterButtonType.ComingSoon:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.ComingSoon, "coming-soon", TitleType.All, "browse-filter-family,Coming-soon", 1);
			break;
		case BrowseFilterButtonType.AtoZ:
			Session.Instance.BrowseState.SelectedAlphaGroupName = null;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "alpha", TitleType.All, "browse-filter-family,alpha", 1);
			break;
		case BrowseFilterButtonType.XboxOne:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Xbox One", TitleType.XboxOne, "browse-filter-family,Xbox One", 1);
			break;
		case BrowseFilterButtonType.Xbox360:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Xbox 360", TitleType.Xbox360, "browse-filter-family,Xbox 360", 1);
			break;
		case BrowseFilterButtonType.PS3:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "PS3", TitleType.PS3, "browse-filter-family,PS3", 1);
			break;
		case BrowseFilterButtonType.PS4:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "PS4", TitleType.PS4, "browse-filter-family,PS4", 1);
			break;
		case BrowseFilterButtonType.Wii:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Wii", TitleType.Wii, "browse-filter-family,Wii", 1);
			break;
		case BrowseFilterButtonType.WiiU:
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Wii U", TitleType.WiiU, "browse-filter-family,Wii U", 1);
			break;
		case BrowseFilterButtonType.Top20:
			Session.Instance.BrowseState.Genre = Genres.Top20Movies;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Top 20", TitleType.All, "browse-filter-family,Top 20", 1);
			break;
		case BrowseFilterButtonType.Action:
			Session.Instance.BrowseState.Genre = Genres.ActionAndAdventure;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Action", TitleType.All, "browse-filter-family,Action", 1);
			break;
		case BrowseFilterButtonType.Drama:
			Session.Instance.BrowseState.Genre = Genres.Drama;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Drama", TitleType.All, "browse-filter-family,Drama", 1);
			break;
		case BrowseFilterButtonType.Horror:
			Session.Instance.BrowseState.Genre = Genres.Horror;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Horror", TitleType.All, "browse-filter-family,Horror", 1);
			break;
		case BrowseFilterButtonType.Comedy:
			Session.Instance.BrowseState.Genre = Genres.Comedy;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Comedy", TitleType.All, "browse-filter-family,Comedy", 1);
			break;
		case BrowseFilterButtonType.Family:
			Session.Instance.BrowseState.Genre = Genres.FamilyMovies;
			SetBrowseFilters(sender.UpdateBrowseViewAction, BrowseViewFilter.All, "Family", TitleType.All, "browse-filter-family,Family", 1);
			break;
		}
	}

	internal static void SetBrowseFilters(Action updateBrowseViewAction, BrowseViewFilter browseMode, string browseSelectedSort, TitleType titleType, string analyticAttributeValue, int? pageNumber)
	{
		IBrowseState browseState = Session.Instance.BrowseState;
		browseState.BrowseMode = browseMode;
		browseState.BrowseSelectedSort = browseSelectedSort;
		browseState.TitleType = titleType;
		if (pageNumber.HasValue)
		{
			browseState.PageNumber = pageNumber.Value;
		}
		if (!string.IsNullOrEmpty(analyticAttributeValue))
		{
			ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			IShoppingSessionService service2 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service != null && service2 != null)
			{
				string bufferedSessionID = service2.GetCurrentSession().Id.ToString();
				service.AddNode("viewEvents", "viewEvent", bufferedSessionID);
				service.AddAttribute("viewEvent", "subItemType", "filterChange", bufferedSessionID);
				service.AddAttribute("viewEvent", "subItemValue", analyticAttributeValue, bufferedSessionID);
			}
		}
		updateBrowseViewAction?.Invoke();
	}

	internal static List<BrowseFilterButton> CreateBrowseFilterButtons(IConfiguration configuration)
	{
		List<BrowseFilterButton> list = new List<BrowseFilterButton>();
		IBrowseState browseState = Session.Instance.BrowseState;
		int width = (IsADAMode ? 75 : 94);
		if (browseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.BluRay,
				Name = "blu_ray_button",
				Height = 30,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == "Blu-ray")
			});
		}
		list.Add(new BrowseFilterButton
		{
			ButtonType = BrowseFilterButtonType.AllNewest,
			Name = "newest_button",
			Height = 30,
			Width = width,
			IsSelected = (browseState.BrowseSelectedSort == "Newest" && (!IsADAMode || !browseState.Genre.HasValue))
		});
		if (browseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.AtoZ,
				Name = "abc_button",
				Height = 30,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == "alpha")
			});
		}
		if (Session.Instance.BrowseState.TitleFamily == TitleFamily.Games)
		{
			if (configuration.EnabledGameTypes[TitleType.Xbox360] && configuration.GameHasContentTypes[TitleType.Xbox360])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.Xbox360,
					Name = "xbox_360_button",
					Height = 30,
					Width = 94,
					IsSelected = (browseState.BrowseSelectedSort == "Xbox 360")
				});
			}
			if (configuration.EnabledGameTypes[TitleType.XboxOne] && configuration.GameHasContentTypes[TitleType.XboxOne])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.XboxOne,
					Name = "xbox_one_button",
					Height = 30,
					Width = 94,
					IsSelected = (browseState.BrowseSelectedSort == "Xbox One")
				});
			}
			if (configuration.EnabledGameTypes[TitleType.PS3] && configuration.GameHasContentTypes[TitleType.PS3])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.PS3,
					Name = "ps3_button",
					Height = 30,
					Width = 74,
					IsSelected = (browseState.BrowseSelectedSort == "PS3")
				});
			}
			if (configuration.EnabledGameTypes[TitleType.PS4] && configuration.GameHasContentTypes[TitleType.PS4])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.PS4,
					Name = "ps4_button",
					Height = 30,
					Width = 74,
					IsSelected = (browseState.BrowseSelectedSort == "PS4")
				});
			}
			if (configuration.EnabledGameTypes[TitleType.Wii] && configuration.GameHasContentTypes[TitleType.Wii])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.Wii,
					Name = "wii_button",
					Height = 30,
					Width = 74,
					IsSelected = (browseState.BrowseSelectedSort == "Wii")
				});
			}
			if (configuration.EnabledGameTypes[TitleType.WiiU] && configuration.GameHasContentTypes[TitleType.WiiU])
			{
				list.Add(new BrowseFilterButton
				{
					ButtonType = BrowseFilterButtonType.WiiU,
					Name = "wii_u_button",
					Height = 30,
					Width = 74,
					IsSelected = (browseState.BrowseSelectedSort == "Wii U")
				});
			}
		}
		if (IsForSaleButtonEnabled)
		{
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.ForSale,
				Name = "for_sale_button",
				Height = 30,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == "for-sale")
			});
		}
		if (IsADAMode && Session.Instance.BrowseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Top20,
				Name = "top_20_filter_button",
				Height = 30,
				Width = (IsSpanish ? 105 : 75),
				IsSelected = (browseState.BrowseSelectedSort == "Top 20" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Top20Movies))
			});
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Action,
				Name = "action_filter_button",
				Height = 30,
				Width = 75,
				IsSelected = (browseState.BrowseSelectedSort == "Action" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.ActionAndAdventure))
			});
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Drama,
				Name = "drama_filter_button",
				Height = 30,
				Width = 75,
				IsSelected = (browseState.BrowseSelectedSort == "Drama" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Drama))
			});
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Horror,
				Name = "horror_filter_button",
				Height = 30,
				Width = 75,
				IsSelected = (browseState.BrowseSelectedSort == "Horror" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Horror))
			});
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Comedy,
				Name = "comedy_filter_button",
				Height = 30,
				Width = 75,
				IsSelected = (browseState.BrowseSelectedSort == "Comedy" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Comedy))
			});
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.Family,
				Name = "family_filter_button",
				Height = 30,
				Width = 75,
				IsSelected = (browseState.BrowseSelectedSort == "Family" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.FamilyMovies))
			});
		}
		if (!IsADAMode)
		{
			list.Add(new BrowseFilterButton
			{
				ButtonType = BrowseFilterButtonType.ComingSoon,
				Name = "coming_soon_button",
				Height = 30,
				Width = 110,
				IsSelected = (browseState.BrowseSelectedSort == "coming-soon")
			});
		}
		return list;
	}

	[KernelFunction(Name = "kernel.draw_top_menu")]
	internal static void DrawTopMenu(string browseViewName, int? selectedGenre, string selectedProductFamily, bool disableGames, bool collectionsEnabled, Action<int?, string> onGenreFilterChange, Action<string> onProductFamiltyFilterChange, Action onSelectCollections)
	{
		ClearOldTopMenuControlActors(browseViewName);
		TopMenuControl topMenuControl = new TopMenuControl
		{
			BrowseSetName = browseViewName,
			Width = 1024,
			Height = 74,
			Y = 0,
			X = 0
		};
		_topMenuControls[browseViewName] = topMenuControl;
		CreateMenuButtons(topMenuControl, browseViewName, selectedGenre, selectedProductFamily, collectionsEnabled, disableGames);
		topMenuControl.OnMenuButtonClick += delegate(MenuButton menuButton)
		{
			if (menuButton is GenreMenuButton genreMenuButton && onGenreFilterChange != null)
			{
				onGenreFilterChange(genreMenuButton.FilterValue, genreMenuButton.Text);
			}
			else if (menuButton is CollectionsMenuButton && onSelectCollections != null)
			{
				onSelectCollections();
			}
			else if (menuButton is ProductFamilyMenuButton productFamilyMenuButton && onProductFamiltyFilterChange != null)
			{
				onProductFamiltyFilterChange(productFamilyMenuButton.FilterValue);
			}
		};
		topMenuControl.Render();
	}

	private static void CreateMenuButtons(TopMenuControl topMenuControl, string browseViewName, int? selectedGenre, string selectedProductFamily, bool collectionsEnabled, bool disableGames)
	{
		new List<MenuButton>();
		switch (browseViewName)
		{
		case "browse_view_collections":
		case "browse_view":
		case "browse_filter_alpha_view":
		{
			TitleFamily titleFamily = (TitleFamily)Enum.Parse(typeof(TitleFamily), selectedProductFamily);
			bool highlightSelectedGenre = browseViewName != "browse_view_collections";
			if (titleFamily == TitleFamily.Movies)
			{
				MenuButton menuButton = null;
				menuButton = ((!collectionsEnabled) ? ((MenuButton)CreateGenreMenuButton(1093, highlightSelectedGenre, selectedGenre, titleFamily)) : ((MenuButton)new CollectionsMenuButton
				{
					Text = BundleFunctions.GetDictionaryStringValue("strings", "Collections"),
					IsSelected = (browseViewName == "browse_view_collections")
				}));
				topMenuControl.AddMenuButtons(new List<MenuButton>
				{
					CreateGenreMenuButton(null, highlightSelectedGenre, selectedGenre, titleFamily),
					menuButton,
					CreateGenreMenuButton(1000, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1005, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1009, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1004, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1006, highlightSelectedGenre, selectedGenre, titleFamily),
					disableGames ? null : CreateProductFamilyMenuButton(TitleFamily.Games, ArrowDirection.Right)
				});
			}
			else
			{
				topMenuControl.AddMenuButtons(new List<MenuButton>
				{
					CreateGenreMenuButton(null, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1026, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1006, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1028, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1030, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1034, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateGenreMenuButton(1036, highlightSelectedGenre, selectedGenre, titleFamily),
					CreateProductFamilyMenuButton(TitleFamily.Movies, ArrowDirection.Right)
				});
			}
			break;
		}
		case "browse_view_collection":
			topMenuControl.AddMenuButtons(new List<MenuButton>
			{
				CreateProductFamilyMenuButton(TitleFamily.Movies, ArrowDirection.Left),
				CreateProductFamilyMenuButton(TitleFamily.Games, ArrowDirection.Right, 880)
			});
			break;
		}
	}

	private static ProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection)
	{
		return CreateProductFamilyMenuButton(productFamily, arrowDirection, -1);
	}

	private static ProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection, int x)
	{
		string key = ((productFamily == TitleFamily.Movies) ? "To_Movies" : "To_Games");
		ProductFamilyMenuButton productFamilyMenuButton = new ProductFamilyMenuButton
		{
			Text = BundleFunctions.GetDictionaryStringValue("strings", key),
			FilterValue = productFamily.ToString(),
			ArrowDirection = arrowDirection
		};
		if (x > 0)
		{
			productFamilyMenuButton.X = x;
			productFamilyMenuButton.FixedPosition = true;
		}
		return productFamilyMenuButton;
	}

	private static GenreMenuButton CreateGenreMenuButton(int? genre, bool highlightSelectedGenre, int? selectedGenre, TitleFamily titleFamily)
	{
		GenreMenuButton obj = new GenreMenuButton
		{
			FilterValue = genre,
			IsSelected = (highlightSelectedGenre && selectedGenre == genre)
		};
		string key = (genre.HasValue ? $"genre_{genre}" : ((titleFamily == TitleFamily.Movies) ? "Movies_ALL" : "Games_ALL"));
		obj.Text = BundleFunctions.GetDictionaryStringValue("strings", key);
		return obj;
	}

	private static List<IBrowseItem> CreateBrowseItemsFromShoppingCartItems(LuaTable shoppingCartItems, bool inCartTitleMarketing)
	{
		List<IBrowseItem> list = new List<IBrowseItem>();
		if (ServiceLocator.Instance.GetService(typeof(IDataService)) is IDataService dataService)
		{
			foreach (object key in shoppingCartItems.Keys)
			{
				if (string.IsNullOrEmpty(shoppingCartItems[key].ToString()) || !(shoppingCartItems[key] is LuaTable luaTable))
				{
					continue;
				}
				IBrowseItem browseItem = null;
				string text = luaTable["product_family"] as string;
				if (text == TitleFamily.Movies.ToString())
				{
					browseItem = new MovieTitleBrowseItem(luaTable);
				}
				else if (text == TitleFamily.Games.ToString())
				{
					browseItem = new GameTitleBrowseItem(luaTable);
				}
				if (browseItem == null)
				{
					continue;
				}
				MovieOrGameBrowseItem movieOrGameBrowseItem = browseItem as MovieOrGameBrowseItem;
				List<IKioskProduct> products = null;
				long? productIdFromLuaTable = GetProductIdFromLuaTable(luaTable);
				if (productIdFromLuaTable.HasValue && productIdFromLuaTable != 0)
				{
					List<long> include = new List<long> { productIdFromLuaTable.Value };
					dataService.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, include, null, isADAView: false, out products);
					if (products.Count > 0 && products[0] is ITitleProduct)
					{
						browseItem.ImageName = ((ITitleProduct)products[0]).ImageFile;
					}
					else if (movieOrGameBrowseItem != null)
					{
						browseItem.ImageName = movieOrGameBrowseItem.RegisteredImageName;
					}
				}
				browseItem.ImageOpacity = 1f;
				if (movieOrGameBrowseItem != null)
				{
					movieOrGameBrowseItem.ImageBorderColor = Color.FromArgb(162, 16, 24);
					movieOrGameBrowseItem.ImageBorderStrokeWeight = 2f;
					string text2 = luaTable["add_sequence"] as string;
					if (!inCartTitleMarketing || text2 == "post_offer")
					{
						movieOrGameBrowseItem.ShowCancelButton = true;
					}
					if (luaTable["action"] as string == "purchase")
					{
						Banner banner = BannerCollection.Instance.Find((Banner x) => x.BannerType == BannerType.Purchase);
						if (banner != null)
						{
							movieOrGameBrowseItem.Banner = banner;
							movieOrGameBrowseItem.BannerText = BundleFunctions.GetDictionaryStringValue("strings", "mini_cart_purchase_text");
						}
					}
					if (products.Count > 0 && products[0] is MDVProduct)
					{
						int num = (products[0] as MDVProduct).TitleProducts.Count();
						Banner banner2 = BannerCollection.Instance.Find((Banner x) => x.BannerType == BannerType.MultipleDisc);
						if (banner2 != null)
						{
							movieOrGameBrowseItem.Banner = banner2;
							if (BundleFunctions.GetDictionary("strings") != null)
							{
								string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "mini_cart_discs_text");
								if (!string.IsNullOrEmpty(dictionaryStringValue))
								{
									movieOrGameBrowseItem.BannerText = string.Format(dictionaryStringValue, num);
								}
							}
						}
					}
				}
				list.Add(browseItem);
			}
		}
		while (list.Count < 5)
		{
			list.Add(new MovieOrGameBrowseItem(null));
		}
		return list;
	}

	private static void SetPageNumber(int pageNumber, LuaInterfacedBrowseControl productBrowseControl)
	{
		productBrowseControl.CurrentPage = pageNumber;
		productBrowseControl.OnPageNumberChanged(productBrowseControl.CurrentPage, PageChangeSource.None);
	}

	private static void ClearOldActors(string browseSetName)
	{
		LuaInterfacedBrowseControl value = null;
		_browseControls.TryGetValue(browseSetName, out value);
		value?.ClearBrowseItemActors();
	}

	private static void ClearOldMiniCartActors(string browseSetName)
	{
		MiniCartControl value = null;
		_miniCartControls.TryGetValue(browseSetName, out value);
		value?.ClearActors();
	}

	private static void ClearOldTopMenuControlActors(string browseViewName)
	{
		TopMenuControl value = null;
		_topMenuControls.TryGetValue(browseViewName, out value);
		value?.ClearActors();
	}

	private static void ClearOldAccessibilityControlActors(string browseSetName)
	{
		AccessibilityControl value = null;
		_accessibilityControls.TryGetValue(browseSetName, out value);
		value?.ClearActors();
	}

	private static void ClearOldBrowseGroupSelectionControlActors(string browseSetName)
	{
		BrowseGroupSelectionControl value = null;
		_browseGroupSelectionControls.TryGetValue(browseSetName, out value);
		value?.ClearActors();
	}

	private static void ClearOldBrowseFilterBarControlActors(string browseSetName)
	{
		BrowseFilterBarControl value = null;
		_browseFilterBarControls.TryGetValue(browseSetName, out value);
		value?.ClearActors();
	}

	private static LuaInterfacedBrowseControl CreateCollectionsBrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			ColumnCount = 3,
			RowCount = 2,
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = true,
			ShowBackground = true,
			ShowPageNumbers = true
		};
		luaInterfacedBrowseControl.ItemMargins.Left = 12;
		luaInterfacedBrowseControl.ItemMargins.Right = 12;
		luaInterfacedBrowseControl.ItemMargins.Top = 12;
		luaInterfacedBrowseControl.ItemMargins.Bottom = 4;
		luaInterfacedBrowseControl.Margins.Top = 15;
		luaInterfacedBrowseControl.Margins.Left = 64;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static LuaInterfacedBrowseControl CreateNormalBrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			ColumnCount = 5,
			RowCount = 2,
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = true,
			ShowBackground = true,
			ShowPageNumbers = true
		};
		luaInterfacedBrowseControl.ItemMargins.Left = 18;
		luaInterfacedBrowseControl.ItemMargins.Right = 18;
		luaInterfacedBrowseControl.ItemMargins.Top = 36;
		luaInterfacedBrowseControl.Margins.Top = 7;
		luaInterfacedBrowseControl.Margins.Left = 44;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static LuaInterfacedBrowseControl CreateAToZBrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			ColumnCount = 3,
			RowCount = 6,
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = false,
			ShowPageNumbers = false,
			ShowTitle = true,
			ShowBackground = false,
			ShowSeparator = true,
			SeparatorPadding = 8,
			LayoutDirection = BrowseControl.BrowseLayoutDirection.TopToBottomThenLeftToRight
		};
		luaInterfacedBrowseControl.ItemMargins.Left = 10;
		luaInterfacedBrowseControl.ItemMargins.Right = 10;
		luaInterfacedBrowseControl.ItemMargins.Top = 5;
		luaInterfacedBrowseControl.ItemMargins.Bottom = 0;
		luaInterfacedBrowseControl.Margins.Top = 60;
		luaInterfacedBrowseControl.Margins.Left = 60;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static LuaInterfacedBrowseControl CreateABEAToZBrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			ColumnCount = 2,
			RowCount = 5,
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = false,
			ShowPageNumbers = false,
			ShowTitle = true,
			ShowBackground = false,
			ShowSeparator = true,
			SeparatorPadding = 8,
			LayoutDirection = BrowseControl.BrowseLayoutDirection.TopToBottomThenLeftToRight
		};
		luaInterfacedBrowseControl.ItemMargins.Left = 10;
		luaInterfacedBrowseControl.ItemMargins.Right = 10;
		luaInterfacedBrowseControl.ItemMargins.Top = 5;
		luaInterfacedBrowseControl.ItemMargins.Bottom = 0;
		luaInterfacedBrowseControl.Margins.Bottom = 60;
		luaInterfacedBrowseControl.Margins.Left = 60;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static LuaInterfacedBrowseControl CreateMegaBrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = true,
			ShowBackground = true,
			ShowPageNumbers = true
		};
		luaInterfacedBrowseControl.ColumnCount = 7;
		luaInterfacedBrowseControl.RowCount = 3;
		luaInterfacedBrowseControl.ItemMargins.Top = 20;
		luaInterfacedBrowseControl.ItemMargins.Left = 10;
		luaInterfacedBrowseControl.ItemMargins.Right = 10;
		luaInterfacedBrowseControl.Margins.Top = 20;
		luaInterfacedBrowseControl.Margins.Left = 44;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static LuaInterfacedBrowseControl CreateADABrowseView(List<IBrowseItem> browseItems)
	{
		LuaInterfacedBrowseControl luaInterfacedBrowseControl = new LuaInterfacedBrowseControl
		{
			ColumnCount = 2,
			RowCount = 5,
			Width = 1024,
			Height = 494,
			Y = 70,
			Items = browseItems,
			ShowBorderItems = true,
			ShowBackground = true,
			ShowPageNumbers = true
		};
		luaInterfacedBrowseControl.ItemMargins.Left = 18;
		luaInterfacedBrowseControl.ItemMargins.Right = 18;
		luaInterfacedBrowseControl.ItemMargins.Top = 18;
		luaInterfacedBrowseControl.Margins.Left = 44;
		luaInterfacedBrowseControl.Margins.Right = luaInterfacedBrowseControl.Margins.Left;
		return luaInterfacedBrowseControl;
	}

	private static List<IBrowseItem> CreateBrowseItems(LuaTable products, bool createAlphaGroups)
	{
		List<IBrowseItem> list = new List<IBrowseItem>();
		IGroupHeaderBrowseItem groupHeaderBrowseItem = null;
		ProductFlagCollection.Instance.Clear();
		foreach (object key in products.Keys)
		{
			if (!(products[key] is LuaTable luaTable) || !InCartCanShow(luaTable))
			{
				continue;
			}
			IBrowseItem browseItem = null;
			string text = luaTable["kiosk_product_type"].ToString();
			if (text == 3.ToString())
			{
				browseItem = new TitleRollupBrowseItem(luaTable);
			}
			else if (text == 1.ToString() || text == 2.ToString())
			{
				bool result = false;
				bool.TryParse(luaTable["is_movie"].ToString(), out result);
				if (result)
				{
					browseItem = new MovieTitleBrowseItem(luaTable);
				}
				else
				{
					bool result2 = false;
					bool.TryParse(luaTable["is_game"].ToString(), out result2);
					if (result2)
					{
						browseItem = new GameTitleBrowseItem(luaTable);
					}
				}
			}
			else if (text == 4.ToString())
			{
				browseItem = new CollectionBrowseItem(luaTable);
			}
			if (browseItem == null)
			{
				continue;
			}
			if (createAlphaGroups)
			{
				string alphaGroupCharacter = GetAlphaGroupCharacter(browseItem);
				if (groupHeaderBrowseItem == null || groupHeaderBrowseItem.Name != alphaGroupCharacter)
				{
					groupHeaderBrowseItem = new GroupHeaderBrowseItem
					{
						Name = alphaGroupCharacter,
						IsSelected = (alphaGroupCharacter == Session.Instance.BrowseState.SelectedAlphaGroupName)
					};
					list.Add(groupHeaderBrowseItem);
				}
			}
			browseItem.Data = luaTable;
			list.Add(browseItem);
		}
		return list;
	}

	private static bool InCartCanShow(LuaTable tempLuaTable)
	{
		bool result = true;
		if (tempLuaTable != null && tempLuaTable["incart_can_show"] != null)
		{
			bool.TryParse(string.Format("{0}", tempLuaTable["incart_can_show"]), out result);
		}
		return result;
	}

	private static string GetAlphaGroupCharacter(IBrowseItem browseItem)
	{
		char c = ((browseItem != null && browseItem.SortName != null) ? browseItem.SortName.ToUpper()[0] : ' ');
		string result = "#";
		if (c >= 'A' && c <= 'Z')
		{
			result = c.ToString();
		}
		return result;
	}

	private static bool IsProductThinned(LuaTable luaKioskProduct)
	{
		return IsProductThinned(GetProductIdFromLuaTable(luaKioskProduct) ?? 0);
	}

	[KernelFunction(Name = "kernel.is_product_thinned")]
	private static bool IsProductThinned(long productId)
	{
		if (ServiceLocator.Instance.GetService(typeof(IDataService)) is IDataService dataService)
		{
			return dataService.GetIsProductThinned(productId);
		}
		return false;
	}

	private static long? GetProductIdFromLuaTable(LuaTable luaTable)
	{
		long? result = null;
		if (luaTable != null)
		{
			object obj = luaTable["product_id"];
			if (obj != null)
			{
				long result2 = 0L;
				if (long.TryParse(obj.ToString(), out result2))
				{
					result = result2;
				}
			}
		}
		return result;
	}
}
