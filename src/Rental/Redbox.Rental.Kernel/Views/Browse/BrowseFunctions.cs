using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel.Views.Browse;

public static class BrowseFunctions
{
	private static Dictionary<string, LuaInterfacedBrowseControl> _browseControls = new Dictionary<string, LuaInterfacedBrowseControl>();

	private static Dictionary<string, MiniCartControl> _miniCartControls = new Dictionary<string, MiniCartControl>();

	private static Dictionary<string, AccessibilityControl> _accessibilityControls = new Dictionary<string, AccessibilityControl>();

	private static Dictionary<string, TopMenuControl> _topMenuControls = new Dictionary<string, TopMenuControl>();

	[KernelFunction(Name = "kernel.draw_images_set")]
	internal static void DrawImagesSet(string browseSetName, string browseFilterSelectedFamily, LuaTable products, int pageNumber, string gotoGroupName, string selectedGroupName, Action<int> onPageNumberChanged, Action<int> onProductsShown, Action<LuaTable> onItemSelected, Action<string> onAddItemSelected, Action<int, LuaTable> onUpdateItemMapping, Func<LuaTable, bool> onIsProductSelected, Func<LuaTable, bool> onIsProductInStock, Func<bool> isShoppingCartFull, Func<LuaTable, string> onGetTestPrice, Func<LuaTable, string> onGetEarlyReleaseDate, Func<bool> isTitleMarketingMode, Func<bool> onIsComingSoonOptinEnabled, Func<LuaTable, bool> onIsComingSoonProductRegistered, Action<LuaTable> onRegisterEmailReminder, Action<LuaTable> onUnregisterEmailReminder)
	{
		ClearOldActors(browseSetName);
		BrowseViewFilter browseMode = Session.Instance.BrowseState.BrowseMode;
		BrowseSort browseSort = Session.Instance.BrowseState.BrowseSelectedSort.ConvertToBrowseSort(browseMode);
		TitleFamily titleFamily = browseFilterSelectedFamily.ConvertToTitleFamily();
		List<IBrowseItem> list = CreateBrowseItems(products, selectedGroupName);
		LuaInterfacedBrowseControl productBrowseControl;
		if (browseSetName == BrowseSetNames.Collections.ToString())
		{
			productBrowseControl = CreateCollectionsBrowseView(list);
		}
		else if (browseSort == BrowseSort.Alpha && browseSetName == BrowseSetNames.Main.ToString())
		{
			productBrowseControl = CreateAToZBrowseView(list);
		}
		else
		{
			productBrowseControl = CreateNormalBrowseView(list);
		}
		if (productBrowseControl != null)
		{
			productBrowseControl.BrowseSetName = browseSetName;
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			productBrowseControl.isADAAccessible = service != null && service.Filter["ada_accessible"] == "True";
			productBrowseControl.ShowPaddleShadow = true;
			productBrowseControl.OnLuaItemSelected = onItemSelected;
			productBrowseControl.OnLuaAddItemSelected = onAddItemSelected;
			productBrowseControl.OnPageNumberChanged = onPageNumberChanged;
			productBrowseControl.OnLuaItemsShown = onProductsShown;
			productBrowseControl.OnLuaUpdateItemMapping = onUpdateItemMapping;
			SetPageNumber(pageNumber, gotoGroupName, list, productBrowseControl);
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
			list.ForEach(delegate(IBrowseItem x)
			{
				x.OnBeforeRender += productBrowseControl.ConfigureBrowseItem;
			});
			BundleFunctions.GetDictionary("strings");
			productBrowseControl.NoResultsText = string.Format("{0}", (titleFamily == TitleFamily.Games) ? BundleFunctions.GetDictionaryStringValue("strings", "filter_games_empty") : BundleFunctions.GetDictionaryStringValue("strings", "filter_movies_empty"));
			productBrowseControl.Render();
		}
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
		List<IBrowseItem> items = CreateBrowseItems(products, null);
		AccessibilityControl accessibilityControl = new AccessibilityControl
		{
			BrowseSetName = browseSetName,
			SourceBrowseControl = _browseControls[BrowseSetNames.Main.ToString()],
			SelectedTitleFamily = selectedTitleFamily2,
			Items = items,
			Width = 526,
			Height = 146,
			Y = 622,
			X = 230
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
		List<IBrowseItem> source = CreateBrowseItems(luaTable, null);
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

	private static void SetPageNumber(int pageNumber, string gotoGroupName, List<IBrowseItem> browseItems, LuaInterfacedBrowseControl productBrowseControl)
	{
		int num = pageNumber;
		if (!string.IsNullOrEmpty(gotoGroupName))
		{
			IBrowseItem browseItem = browseItems.Find((IBrowseItem x) => x.Name == gotoGroupName);
			if (browseItem != null)
			{
				int browseItemIndex = browseItems.IndexOf(browseItem) + 1;
				num = productBrowseControl.GetPageOfBrowseItemIndex(browseItemIndex);
			}
			if (num == -1)
			{
				num = pageNumber;
			}
		}
		productBrowseControl.CurrentPage = num;
		productBrowseControl.OnPageNumberChanged(productBrowseControl.CurrentPage);
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
			ColumnCount = 3,
			RowCount = 1,
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

	private static List<IBrowseItem> CreateBrowseItems(LuaTable products, string selectedGroupName)
	{
		List<IBrowseItem> list = new List<IBrowseItem>();
		IGroupHeaderBrowseItem groupHeaderBrowseItem = null;
		ProductFlagCollection.Instance.Clear();
		foreach (object key in products.Keys)
		{
			if (!string.IsNullOrEmpty(products[key].ToString()))
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
				if (browseItem != null)
				{
					browseItem.Data = luaTable;
					list.Add(browseItem);
				}
				AssignGroupDisplayCharacter(ref groupHeaderBrowseItem, browseItem, selectedGroupName);
			}
			else
			{
				groupHeaderBrowseItem = new GroupHeaderBrowseItem();
				list.Add(groupHeaderBrowseItem);
			}
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

	private static void AssignGroupDisplayCharacter(ref IGroupHeaderBrowseItem groupHeaderBrowseItem, IBrowseItem browseItem, string selectedGroupName)
	{
		if (groupHeaderBrowseItem != null && browseItem != null)
		{
			char c = ((browseItem.SortName != null) ? browseItem.SortName.ToUpper()[0] : ' ');
			string text = "#";
			if (c >= 'A' && c <= 'Z')
			{
				text = c.ToString();
			}
			groupHeaderBrowseItem.Name = text;
			groupHeaderBrowseItem.IsSelected = text == selectedGroupName;
			groupHeaderBrowseItem = null;
		}
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
