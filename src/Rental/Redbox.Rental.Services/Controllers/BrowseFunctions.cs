using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class BrowseFunctions
{
	public class Constants
	{
		public class IdleTimers
		{
			public const string IdleTimerName = "idle_timer";

			public const string KeyboardIdleTimerName = "keyboard_idle_timer";

			public const string ShoppingCartUpdateADAIdleTimerName = "shopping_cart_update_ada_idle_timer";
		}

		public static string ComingSoon = "ComingSoon";
	}

	private static bool _purchaseWarningShown;

	private static bool _adaPurchaseWarningShown;

	private static bool _digitalCodeWarningShown;

	private static bool _subPlatformWarningShown;

	private static bool _multiDiscWarningShown;

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static bool IsPurchase => (Redbox.Rental.Services.Session.Session.Instance?.BrowseState.BrowseMode ?? BrowseViewFilter.All) == BrowseViewFilter.ForSale;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase)
	{
		AddProductToCart(titleProduct, isPurchase, null, ItemSourceType.Browse);
	}

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord)
	{
		AddProductToCart(titleProduct, isPurchase, pricingRecord, ItemSourceType.Browse);
	}

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		if (titleProduct != null)
		{
			if (isPurchase)
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", $"buy,{titleProduct.ProductId}");
			}
			else
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "shopping-cart-add-item");
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView = service?.Peek();
			_purchaseWarningShown = false;
			_adaPurchaseWarningShown = false;
			_digitalCodeWarningShown = false;
			_subPlatformWarningShown = false;
			_multiDiscWarningShown = false;
			AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
		}
	}

	public static void ShowCartFullMessage()
	{
		Action continueAction = delegate
		{
			BrowseState.ShoppingCartFullMessagesShown = true;
			BrowseState.IsShoppingCartFullMessagesOpen = false;
			ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("MessagePopupViewController.ShowMessageCartFull", popOnly: false);
		};
		MessagePopupViewController.ShowMessageCartFull(delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			BrowseState.IsShoppingCartFullMessagesOpen = true;
			service?.GoBackSafe("MessagePopupViewController.ShowMessageCartFull", popOnly: false);
			BrowseState.IsShoppingCartFullMessagesOpen = false;
			service.Push("idle_timeout_message_view");
			service.Show();
		}, continueAction);
	}

	public static DisplayCollectionModel CreateDisplayCollectionModelFromCollectionProduct(CollectionProduct collectionProduct)
	{
		DisplayCollectionModel displayCollectionModel = null;
		if (collectionProduct != null)
		{
			displayCollectionModel = new DisplayCollectionModel
			{
				Name = collectionProduct.ProductName,
				Data = collectionProduct,
				Text = collectionProduct.ProductName
			};
			for (int i = 0; i < 3; i++)
			{
				BitmapImage bitmapImage = null;
				bitmapImage = ((collectionProduct == null || !(collectionProduct.KioskProducts?.Count > i)) ? Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt : OrchestrationService.GetProductBitmapImage(collectionProduct?.KioskProducts[i]));
				switch (i)
				{
				case 0:
					displayCollectionModel.TopImage = bitmapImage;
					break;
				case 1:
					displayCollectionModel.MiddleImage = bitmapImage;
					break;
				case 2:
					displayCollectionModel.BottomImage = bitmapImage;
					break;
				}
			}
		}
		return displayCollectionModel;
	}

	public static void ConfigureProductFlag(DisplayProductModel displayProductModel, ProductFlags productFlags, bool hideFlag, FrameworkElement frameworkElementToGetStyleFrom, ITitleProduct titleProduct, DisplayProductModelConstants displayProductModelConstants)
	{
		ProductFlag productFlag = ProductFlagCollection.Instance.FirstOrDefault((ProductFlag x) => x.Flag == productFlags);
		displayProductModel.FlagVisibility = ((hideFlag || productFlag == null || productFlag.Flag == ProductFlags.None) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.FlagStyle = WPFFunctions.GetStyle(frameworkElementToGetStyleFrom, productFlag?.FlagStyleName);
		if (displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B" && productFlag != null && productFlag.Flag == ProductFlags.OutOfStock && titleProduct.TitleFamily == TitleFamily.Movies)
		{
			displayProductModel.FlagVisibility = Visibility.Collapsed;
			displayProductModel.FlagStyle = null;
		}
		if (productFlag != null && productFlag.Flag == ProductFlags.FlashDeal)
		{
			displayProductModel.FlagText = Resources.flag_flash_deal;
		}
		else
		{
			displayProductModel.FlagText = BundleFunctions.GetDictionaryStringValue("strings", productFlag?.TextKeyName);
		}
		if (displayProductModel.FlagVisibility == Visibility.Visible)
		{
			displayProductModel.FlagMargin = new Thickness(0.0, 45.0, 0.0, 0.0);
		}
	}

	public static ProductFlags GetProductFlag(ITitleProduct titleProduct)
	{
		ProductFlags result = ProductFlags.None;
		if (titleProduct != null)
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			bool flag = !titleProduct.IsGame || (service?.FlashDealFlagForGamesEnabled ?? true);
			if ((titleProduct.HasDeal && BrowseState.BrowseMode != BrowseViewFilter.ForSale && BrowseState.BrowseSelectedSort != BrowseSelectedSort.DigitalCode) & flag)
			{
				result = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.EnableFlashDeals) ? ProductFlags.HotDeal : ProductFlags.FlashDeal);
			}
			else if (titleProduct.Genres.Contains(1002L))
			{
				result = ProductFlags.AwardWinner;
			}
			else if (titleProduct.Genres.Contains(1104L))
			{
				result = ProductFlags.AwardNominee;
			}
			else
			{
				double totalDays = (DateTime.Now - titleProduct.MerchandiseDate).TotalDays;
				bool flag2 = ServiceLocator.Instance.GetService<IDataService>()?.GetIsProductThinned(titleProduct.ProductId) ?? false;
				if (titleProduct.Genres.Contains(1025L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games)
				{
					result = ProductFlags.BackAgain;
				}
				else if (titleProduct.Genres.Contains(1095L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Movies && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.TV)
				{
					result = ProductFlags.Classics;
				}
				else if (totalDays <= 28.0)
				{
					result = ProductFlags.New;
				}
				else if ((totalDays >= 105.0 && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games) & flag2)
				{
					result = ProductFlags.LastChance;
				}
			}
		}
		return result;
	}

	public static BannerType GetProductBannerType(ITitleProduct titleProduct, bool showDvdBanners = false)
	{
		BannerType result = BannerType.None;
		if (titleProduct != null)
		{
			if (titleProduct.TitleType == TitleType.Bluray)
			{
				result = BannerType.Bluray;
			}
			else if (titleProduct.TitleType == TitleType._4KUHD)
			{
				result = BannerType._4KUHD;
			}
			else if (showDvdBanners && titleProduct.TitleType == TitleType.DVD && !(titleProduct is TitleRollupProduct))
			{
				result = BannerType.DVD;
			}
			else if (titleProduct.TitleType == TitleType.DigitalCode)
			{
				result = BannerType.DigitalCode;
			}
		}
		return result;
	}

	public static string GetProductDisplayName(ITitleProduct titleProduct)
	{
		string result = null;
		if (titleProduct != null && titleProduct.ProductName != null)
		{
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string text = value?.GetName(Resources.ResourceManager);
			List<string> list = new List<string>
			{
				text,
				$"({text})",
				text.Replace(" ", null),
				string.Format("({0})", text.Replace(" ", null))
			};
			string text2 = value?.GetAlternateNames(Resources.ResourceManager);
			if (!string.IsNullOrEmpty(text2))
			{
				List<string> list2 = text2?.Split(',')?.ToList();
				if (list2 != null)
				{
					foreach (string item in list2)
					{
						list.Add(item);
						list.Add($"({item})");
					}
				}
			}
			result = RemoveEndingText(titleProduct.ProductName, list);
		}
		return result;
	}

	private static string RemoveEndingText(string source, List<string> searchForAndRemoveEndingTextList)
	{
		string text = source?.Trim();
		if (!string.IsNullOrEmpty(text) && searchForAndRemoveEndingTextList != null)
		{
			foreach (string searchForAndRemoveEndingText in searchForAndRemoveEndingTextList)
			{
				if (!string.IsNullOrEmpty(searchForAndRemoveEndingText) && text.ToUpper().EndsWith(searchForAndRemoveEndingText.ToUpper()) && text.Length > searchForAndRemoveEndingText.Length)
				{
					text = text.Substring(0, text.Length - searchForAndRemoveEndingText.Length)?.Trim();
				}
			}
		}
		return text;
	}

	public static void ConfigureBanner(DisplayProductModel displayProductModel, ITitleProduct titleProduct, bool hideBanner, BannerType bannerType, FrameworkElement frameworkElementToGetStyleFrom)
	{
		displayProductModel.BannerVisibility = ((hideBanner || bannerType == BannerType.None) ? Visibility.Collapsed : Visibility.Visible);
		if (bannerType == BannerType.None)
		{
			return;
		}
		Redbox.Rental.Model.Browse.Banner banner = (displayProductModel.Banner = BannerCollection.Instance.Find((Redbox.Rental.Model.Browse.Banner x) => x.BannerType == bannerType));
		if (banner.BannerType == BannerType.ComingSoon)
		{
			if (titleProduct != null)
			{
				displayProductModel.BannerText = string.Format("{1} {0:M/d}", titleProduct.MerchandiseDate, Resources.ResourceManager.GetString(banner.TextResourceName));
			}
		}
		else
		{
			displayProductModel.BannerText = Resources.ResourceManager.GetString(banner.TextResourceName);
		}
		displayProductModel.BannerBackgroundBrush = WPFFunctions.GetBrush(frameworkElementToGetStyleFrom, banner.BannerStyleName);
	}

	private static void AddProductToCartIfUserApprovesWarnings(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		if (CheckForADAPurchaseWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForMatureRatingWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckFor4kWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForBluRayWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForSubPlatformWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForPurchaseWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForDigitalCodeWarning(titleProduct, isPurchase, pricingRecord, itemSourceType) && CheckForAdditionalDiscWarning(titleProduct, isPurchase, pricingRecord, itemSourceType))
		{
			IRentalShoppingCartItem rentalShoppingCartItem = RentalShoppingCartService?.CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
			if (rentalShoppingCartItem != null)
			{
				rentalShoppingCartItem.SourceType = itemSourceType;
				rentalShoppingCartItem.Action = (isPurchase ? RentalShoppingCartItemAction.purchase : RentalShoppingCartItemAction.rental);
				rentalShoppingCartItem.Quantity = 1m;
				rentalShoppingCartItem.Status = LineItemStatus.Pending;
				rentalShoppingCartItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(titleProduct);
				RentalShoppingCartService?.CurrentRentalShoppingCart?.AddItem(rentalShoppingCartItem, pricingRecord);
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			IBrowseState browseState = BrowseState;
			if (browseState != null && browseState.ProductAddedFromCarousel)
			{
				OrchestrationService.ViewShow("browse_view", GetBrowseViewparameters());
				BrowseState.ProductAddedFromCarousel = false;
			}
			bool flag = service?.Stack.Any((IViewFrameInstance vf) => vf.ViewFrame?.ViewName == "recommended_titles_popup_view") ?? false;
			if (BrowseState?.ItemAddedFromView == "recommended_titles_popup_view" || ((BrowseState?.ItemAddedFromView == "product_detail_view") & flag))
			{
				service?.UnwindToSafe("browse_view", discard: true, "recommended_titles_popup_view");
			}
			else if (BrowseState?.ItemAddedFromView == "product_detail_view")
			{
				service?.GoBack("Item Added", popOnly: false);
			}
			if (BrowseState?.ItemAddedFromView == "multi_night_pricing_popup_view")
			{
				service?.UnwindToSafe("browse_view", discard: true, "multi_night_pricing_popup_view");
			}
			if (BrowseState?.ItemAddedFromView == "option_selection_dialog_view")
			{
				service?.UnwindToSafe("browse_view", discard: true, "option_selection_dialog_view");
			}
			IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
			if (rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull)
			{
				ShowCartFullMessage();
			}
		}
	}

	public static BrowseViewParameters GetBrowseViewparameters()
	{
		BrowseViewParameters browseViewParameters = new BrowseViewParameters();
		if (ApplicationState.Instance.IsADAAccessible)
		{
			browseViewParameters.ViewAnalyticsName = "browse_view_ada_accessible";
		}
		else
		{
			browseViewParameters.ViewAnalyticsName = "browse_view";
		}
		return browseViewParameters;
	}

	public static void ShowRelatedProductsDialog(ITitleProduct selectedProduct)
	{
		IRecommendedTitles recommendedTitles = RecommendedTitlesPopupViewController.GenerateRecommendedTitles(selectedProduct);
		bool flag = false;
		if (recommendedTitles != null && recommendedTitles.Count > 0)
		{
			int numberOfTitlesToShow = 6;
			List<IBrowseItemModel> list = RecommendedTitlesPopupViewController.CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
			if (list != null && list.Count > 0)
			{
				flag = true;
				RecommendedTitlesPopupInfo recommendedTitlesPopupInfo = new RecommendedTitlesPopupInfo();
				recommendedTitlesPopupInfo.RecommendedTitles = recommendedTitles;
				recommendedTitlesPopupInfo.NumberOfTitlesToShow = numberOfTitlesToShow;
				recommendedTitlesPopupInfo.ShowOfferText = true;
				recommendedTitlesPopupInfo.ShownFromView = "option_selection_dialog_view";
				OrchestrationService.ViewShow("recommended_titles_popup_view", recommendedTitlesPopupInfo);
			}
		}
		if (!flag)
		{
			Action action = delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "related_movies_gotit");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBack("RelatedMoviesGotIt", popOnly: false);
			};
			MessagePopupViewController.ShowRelatedTitlesNotFoundMessage(action, action);
		}
	}

	private static bool CheckForAdditionalDiscWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool flag = true;
		if (!_multiDiscWarningShown && !isPurchase && (titleProduct.RequiredAdditionalTitles?.Count ?? 0) > 0)
		{
			foreach (long requiredAdditionalTitle in titleProduct.RequiredAdditionalTitles)
			{
				if (!RentalShoppingCartService.CurrentRentalShoppingCart.HasItem(requiredAdditionalTitle))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				string title = string.Format(Resources.multi_disc_warning_title, titleProduct.RequiredAdditionalTitles.Count + 1);
				string empty = string.Empty;
				string multi_disc_warning_button_text = Resources.multi_disc_warning_button_text;
				IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
				IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
				IPricingRecord oneDayProductPricingRecord = service.GetOneDayProductPricingRecord(titleProduct.ProductId);
				decimal initialNight = oneDayProductPricingRecord.InitialNight;
				foreach (long requiredAdditionalTitle2 in titleProduct.RequiredAdditionalTitles)
				{
					oneDayProductPricingRecord = service.GetOneDayProductPricingRecord(requiredAdditionalTitle2);
					if (oneDayProductPricingRecord != null)
					{
						initialNight += oneDayProductPricingRecord.InitialNight;
					}
				}
				IKioskProduct kioskProduct = null;
				if (titleProduct.RequiredAdditionalTitles.Count > 1)
				{
					string text = "";
					for (int i = 0; i < titleProduct.RequiredAdditionalTitles.Count - 1; i++)
					{
						kioskProduct = null;
						service2.GetProduct(titleProduct.RequiredAdditionalTitles[i], excludeTitleRollup: true, out kioskProduct);
						if (kioskProduct != null)
						{
							if (i > 0)
							{
								text += ",";
							}
							text = text + " " + kioskProduct.ProductName;
						}
					}
					kioskProduct = null;
					service2.GetProduct(titleProduct.RequiredAdditionalTitles[titleProduct.RequiredAdditionalTitles.Count - 1], excludeTitleRollup: true, out kioskProduct);
					string arg = kioskProduct?.ProductName;
					empty = string.Format(Resources.multi_disc_warning_multiple_disc_message, text, arg, initialNight.ToString("0.00"));
				}
				else
				{
					kioskProduct = null;
					service2.GetProduct(titleProduct.RequiredAdditionalTitles[titleProduct.RequiredAdditionalTitles.Count - 1], excludeTitleRollup: true, out kioskProduct);
					string arg2 = kioskProduct?.ProductName;
					empty = string.Format(Resources.multi_disc_warning_2_disc_message, arg2, initialNight.ToString("0.00"));
				}
				_multiDiscWarningShown = true;
				MessagePopupViewController.ShowMultiDiscAlert(title, empty, multi_disc_warning_button_text, delegate
				{
					ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
				}, delegate
				{
					BrowseState._4kWarningShown = true;
					SessionAnalyticsService?.AddViewEvent("buttonPress", "Okay");
					ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("multi_disc_message_view:continue", BrowseState.ProductAddedFromCarousel ? true : false);
					AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
				}, "MultiDiscWarningDialog");
			}
		}
		return flag;
	}

	private static bool CheckForADAPurchaseWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if ((ApplicationState.Instance.IsADAAccessible && !_adaPurchaseWarningShown) & isPurchase)
		{
			result = false;
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string arg = string.Format("{0} {1}", value?.GetName(Resources.ResourceManager), (titleProduct.TitleFamily == TitleFamily.Games) ? BundleFunctions.GetDictionaryStringValue("strings", "game") : null).Trim();
			string arg2 = $"{titleProduct.PurchasePrice:n2}";
			string value2 = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "ada_purchase_price_message"), arg, BundleFunctions.GetDictionaryStringValue("strings", "currency_symbol"), arg2);
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "abe_purchase_price_message");
			string value3 = null;
			if (!string.IsNullOrEmpty(dictionaryStringValue))
			{
				value3 = (ApplicationState.Instance.IsABEOn ? string.Format(dictionaryStringValue, arg, BundleFunctions.GetDictionaryStringValue("strings", "currency_symbol"), arg2) : null);
			}
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["ADAPurchaseWarning:Header"] = value2;
				service["ABEPurchaseMessage:Header"] = value3;
			}
			MessagePopupViewController.ShowADAPurchaseWarning(delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("ada_purchase_message_view:hide()", popOnly: false);
			}, delegate
			{
				_adaPurchaseWarningShown = true;
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("ada_purchase_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			});
		}
		return result;
	}

	private static bool CheckForBluRayWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if (titleProduct.TitleType == TitleType.Bluray && !BrowseState.BluRayWarningShown)
		{
			result = false;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["MatureWarning:Header"] = BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert_title");
				service["Bluray:Message"] = BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert");
			}
			MessagePopupViewController.ShowBlurayAlert(delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("bluray_message_view:hide()", popOnly: false);
			}, delegate
			{
				BrowseState.BluRayWarningShown = true;
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("bluray_message_view:continue", BrowseState.ProductAddedFromCarousel ? true : false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			});
		}
		return result;
	}

	private static bool CheckFor4kWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if (titleProduct.TitleType == TitleType._4KUHD && !BrowseState._4kWarningShown)
		{
			result = false;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["MatureWarning:Header"] = Resources.four_k_alert_title;
				service["Bluray:Message"] = Resources.four_k_alert;
			}
			MessagePopupViewController.Show4KAlert(delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "cancel");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("4k_message_view:hide()", popOnly: false);
			}, delegate
			{
				BrowseState._4kWarningShown = true;
				SessionAnalyticsService?.AddViewEvent("buttonPress", "gotit");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("4k_message_view:continue", BrowseState.ProductAddedFromCarousel ? true : false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			}, "4KDialog");
		}
		return result;
	}

	private static bool CheckForSubPlatformWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if (!_subPlatformWarningShown)
		{
			ITitleProduct titleProduct2 = titleProduct;
			if (titleProduct2 != null && titleProduct2.SubPlatformId.HasValue)
			{
				ITitleProduct titleProduct3 = titleProduct;
				if (titleProduct3 != null)
				{
					_ = titleProduct3.ProductTypeId;
					if (true)
					{
						IDataService service = ServiceLocator.Instance.GetService<IDataService>();
						SubPlatform subPlatform = service?.GetSubPlatforms()?.FirstOrDefault((SubPlatform x) => x.SubPlatformId == titleProduct.SubPlatformId);
						ProductType productType = service?.GetProductTypes()?.FirstOrDefault((ProductType x) => x.ProductTypeId == titleProduct.ProductTypeId);
						if (subPlatform != null)
						{
							if (productType != null)
							{
								string arg = subPlatform.SubPlatformName.Replace("Compatible", string.Empty).Replace("Required", string.Empty).Replace("XboxOne", string.Empty)
									.Replace("PS4 ", string.Empty)
									.Replace("Move", "Playstation Move")
									.Replace("Eye Camera", "Playstation Eye Camera");
								string arg2 = string.Format("\n{0}{1} {2}", arg, BundleFunctions.GetDictionaryStringValue("strings", "subplatform_for"), productType?.ProductTypeName);
								result = false;
								IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
								if (service2 != null)
								{
									service2["Subplatform:Header"] = BundleFunctions.GetDictionaryStringValue("strings", "subplatform_warning_title");
									service2["Subplatform:Message"] = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "subplatform_warning"), arg2);
								}
								MessagePopupViewController.ShowSubPlatformWarning(delegate
								{
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
								}, delegate
								{
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("subplatform_message_view:hide()", popOnly: false);
								}, delegate
								{
									_subPlatformWarningShown = true;
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("subplatform_message_view:continue", BrowseState.ProductAddedFromCarousel ? true : false);
									AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
								});
							}
							else
							{
								LogHelper.Instance.Log("Error: Unable to find product type with id {0}", titleProduct.ProductTypeId);
							}
						}
						else
						{
							LogHelper.Instance.Log("Error: Unable to find subPlatorm with id {0}", titleProduct.SubPlatformId);
						}
					}
				}
			}
		}
		return result;
	}

	private static bool CheckForPurchaseWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if ((!_purchaseWarningShown & isPurchase) && titleProduct.ProductId != 1139 && titleProduct.ProductTypeId != 17)
		{
			result = false;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["PurchaseWarning:Header"] = BundleFunctions.GetDictionaryStringValue("strings", "products_used");
				service["PurchaseWarning:Message"] = BundleFunctions.GetDictionaryStringValue("strings", "purchase_warning");
			}
			MessagePopupViewController.ShowPurchaseWarning(delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("purchase_message_view:hide()", popOnly: false);
			}, delegate
			{
				_purchaseWarningShown = true;
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("purchase_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			});
		}
		return result;
	}

	private static bool CheckForDigitalCodeWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if (!_digitalCodeWarningShown && titleProduct.ProductTypeId == 17)
		{
			result = false;
			MessagePopupViewController.ShowDigitalCodeAlert(delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "cancel-digitalcodeincart-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert back button", popOnly: false);
			}, delegate
			{
				_digitalCodeWarningShown = true;
				SessionAnalyticsService?.AddViewEvent("buttonPress", "ok-digitalcodeincart-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert ok button", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert time out", popOnly: false);
			});
		}
		return result;
	}

	private static bool CheckForMatureRatingWarning(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord, ItemSourceType itemSourceType)
	{
		bool result = true;
		if (titleProduct != null && (titleProduct.RatingId == 8 || titleProduct.RatingId == 18 || titleProduct.RatingId == 20 || titleProduct.RatingId == 21) && ((!BrowseState.MovieMatureWarningShown && titleProduct.TitleFamily == TitleFamily.Movies) || (!BrowseState.MatureGameWarningShown && titleProduct.TitleFamily == TitleFamily.Games)))
		{
			result = false;
			List<string> list = new List<string>();
			list.Add(titleProduct.TitleFamily.ToString());
			list.Add("matureWarning");
			Ratings result2 = Ratings.None;
			Enum.TryParse<Ratings>(titleProduct?.RatingId?.ToString() ?? string.Empty, out result2);
			RatingsProperty ratingsProperty = ((result2 != Ratings.None) ? RatingsProperties.Properties[result2] : null);
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				if (titleProduct.TitleFamily == TitleFamily.Movies)
				{
					string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "mature_movie_warning");
					dictionaryStringValue = string.Format(dictionaryStringValue, ratingsProperty?.Name);
					service["MatureWarning:Header"] = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "esrb_mature_name"), ratingsProperty?.Name);
					service["MatureWarning:Message"] = dictionaryStringValue;
					service["MatureWarning:TTSMessage"] = dictionaryStringValue.Replace("OK", "Enter");
				}
				else if (titleProduct.TitleFamily == TitleFamily.Games)
				{
					string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "mature_game_warning");
					dictionaryStringValue2 = string.Format(dictionaryStringValue2, ratingsProperty?.Name).Replace("M", BundleFunctions.GetDictionaryStringValue("strings", "esrb_mature"));
					service["MatureWarning:Header"] = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "esrb_mature_game_name"), ratingsProperty?.Code);
					service["MatureWarning:Message"] = dictionaryStringValue2;
					service["MatureWarning:TTSMessage"] = dictionaryStringValue2.Replace("OK", "Enter");
				}
			}
			MessagePopupViewController.ShowMatureWarning(delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("rrated_message_view:hide()", popOnly: false);
			}, delegate
			{
				if (titleProduct.TitleFamily == TitleFamily.Movies)
				{
					BrowseState.MovieMatureWarningShown = true;
				}
				else if (titleProduct.TitleFamily == TitleFamily.Games)
				{
					BrowseState.MatureGameWarningShown = true;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("rrated_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase, pricingRecord, itemSourceType);
			}, list);
		}
		return result;
	}
}
