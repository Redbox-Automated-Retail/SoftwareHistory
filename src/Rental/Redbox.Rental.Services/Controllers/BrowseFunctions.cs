using System;
using System.Linq;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class BrowseFunctions
{
	public class Constants
	{
		public static string IdleTimerName = "idle_timer";

		public static string ComingSoon = "ComingSoon";
	}

	private static bool _purchaseWarningShown;

	private static bool _adaPurchaseWarningShown;

	private static bool _digitalCodeWarningShown;

	private static bool _subPlatformWarningShown;

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static bool IsPurchase => (Redbox.Rental.Services.Session.Session.Instance?.BrowseState.BrowseMode ?? BrowseViewFilter.All) == BrowseViewFilter.ForSale;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase)
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
			AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
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

	private static void AddProductToCartIfUserApprovesWarnings(ITitleProduct titleProduct, bool isPurchase)
	{
		if (CheckForADAPurchaseWarning(titleProduct, isPurchase) && CheckForMatureRatingWarning(titleProduct, isPurchase) && CheckForBluRayWarning(titleProduct, isPurchase) && CheckForSubPlatformWarning(titleProduct, isPurchase) && CheckForPurchaseWarning(titleProduct, isPurchase) && CheckForDigitalCodeWarning(titleProduct, isPurchase))
		{
			IRentalShoppingCartItem rentalShoppingCartItem = RentalShoppingCartService?.CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
			if (rentalShoppingCartItem != null)
			{
				rentalShoppingCartItem.SourceType = ItemSourceType.Browse;
				rentalShoppingCartItem.Action = (isPurchase ? RentalShoppingCartItemAction.purchase : RentalShoppingCartItemAction.rental);
				rentalShoppingCartItem.Quantity = 1m;
				rentalShoppingCartItem.Status = LineItemStatus.Pending;
				RentalShoppingCartService?.CurrentRentalShoppingCart?.AddItem(rentalShoppingCartItem);
			}
			IBrowseState browseState = BrowseState;
			if (browseState != null && browseState.ProductAddedFromCarousel)
			{
				OrchestrationService.ExecuteScript("browse_products");
			}
			if (BrowseState?.ItemAddedFromView == "product_detail_view")
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBack("Item Added", popOnly: false);
			}
			IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
			if (rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull)
			{
				ShowCartFullMessage();
			}
		}
	}

	private static bool CheckForADAPurchaseWarning(ITitleProduct titleProduct, bool isPurchase)
	{
		bool result = true;
		if ((ApplicationState.Instance.IsADAAccessible && !_adaPurchaseWarningShown) & isPurchase)
		{
			result = false;
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string arg = string.Format("{0} {1}", (value == null) ? null : ((value.Name == "Digital Code") ? BundleFunctions.GetDictionaryStringValue("strings", "digital_code") : value.Name), (titleProduct.TitleFamily == TitleFamily.Games) ? BundleFunctions.GetDictionaryStringValue("strings", "game") : null).Trim();
			string arg2 = $"{((titleProduct.TitleFamily == TitleFamily.Games) ? titleProduct.GamePurchasePrice : titleProduct.MoviePurchasePrice):n2}";
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
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
			});
		}
		return result;
	}

	private static bool CheckForBluRayWarning(ITitleProduct titleProduct, bool isPurchase)
	{
		bool result = true;
		if (titleProduct.TitleType == TitleType.Bluray && !BrowseState.BluRayWarningShown)
		{
			result = false;
			SessionAnalyticsService?.AddViewEvent("context", "blurayWarning");
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
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
			}, BrowseState.ProductAddedFromCarousel && BrowseState.MovieMatureWarningShown);
		}
		return result;
	}

	private static bool CheckForSubPlatformWarning(ITitleProduct titleProduct, bool isPurchase)
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
								SessionAnalyticsService?.AddViewEvent("context", "subplatformWarning");
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
									AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
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

	private static bool CheckForPurchaseWarning(ITitleProduct titleProduct, bool isPurchase)
	{
		bool result = true;
		if ((!_purchaseWarningShown & isPurchase) && titleProduct.ProductId != 1139 && titleProduct.ProductTypeId != 17)
		{
			result = false;
			SessionAnalyticsService?.AddViewEvent("context", "purchaseWarning");
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
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
			});
		}
		return result;
	}

	private static bool CheckForDigitalCodeWarning(ITitleProduct titleProduct, bool isPurchase)
	{
		bool result = true;
		if (!_digitalCodeWarningShown && titleProduct.ProductTypeId == 17)
		{
			result = false;
			SessionAnalyticsService?.AddViewEvent("context", "digitalcodeincart_message_view");
			MessagePopupViewController.ShowDigitalCodeAlert(delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "cancel-digitalcodeincart-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert back button", popOnly: false);
			}, delegate
			{
				_digitalCodeWarningShown = true;
				SessionAnalyticsService?.AddViewEvent("buttonPress", "ok-digitalcodeincart-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert ok button", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert time out", popOnly: false);
			});
		}
		return result;
	}

	private static bool CheckForMatureRatingWarning(ITitleProduct titleProduct, bool isPurchase)
	{
		bool result = true;
		if (titleProduct != null && (titleProduct.RatingId == 8 || titleProduct.RatingId == 18 || titleProduct.RatingId == 20 || titleProduct.RatingId == 21) && ((!BrowseState.MovieMatureWarningShown && titleProduct.TitleFamily == TitleFamily.Movies) || (!BrowseState.MatureGameWarningShown && titleProduct.TitleFamily == TitleFamily.Games)))
		{
			result = false;
			SessionAnalyticsService?.AddViewEvent("context", titleProduct.TitleFamily.ToString());
			SessionAnalyticsService?.AddViewEvent("context", "matureWarning");
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
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("rrated_message_view:continue", BrowseState.ProductAddedFromCarousel ? true : false);
				AddProductToCartIfUserApprovesWarnings(titleProduct, isPurchase);
			});
		}
		return result;
	}
}
