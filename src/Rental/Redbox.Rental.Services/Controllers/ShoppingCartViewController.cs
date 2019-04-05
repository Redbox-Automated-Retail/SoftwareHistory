using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewController : BaseLoyaltyController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static IPricingService PricingService => ServiceLocator.Instance.GetService<IPricingService>();

	public static void ConfigureShoppingCartView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			if (BrowseState != null)
			{
				BrowseState.CollectionsPopupShown = true;
				BrowseState.ItemAddedFromView = null;
			}
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			ShoppingCartViewModel shoppingCartViewModel = new ShoppingCartViewModel();
			if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
			{
				BaseViewController.RentalShoppingCartService.RemoveMultiDiscDiscsFromRentalShoppingCart(BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.ShoppingSessionId);
			}
			BaseLoyaltyController.GetEstimateAccruedPoints(ProcessEstimateAccruedPointsResult);
			ConfigureTextAndVisibility(shoppingCartViewModel);
			ConfigureCommands(shoppingCartViewModel);
			ConfigureShoppingCartDisplay(shoppingCartViewModel);
			ConfigureTTS(shoppingCartViewModel);
			viewUserControl.DataContext = shoppingCartViewModel;
			CommandManager.InvalidateRequerySuggested();
			(IdleTimerService?.GetIdleTimer("idle_timer"))?.Start();
		}
	}

	private static void ProcessEstimateAccruedPointsResult(IEstimatePointsAccruedResult estimatePointsAccruedResult)
	{
		ShoppingCartViewModel shoppingCartViewModelFromView = GetShoppingCartViewModelFromView();
		if (shoppingCartViewModelFromView == null)
		{
			return;
		}
		if (estimatePointsAccruedResult != null && estimatePointsAccruedResult.Success)
		{
			IList<IRentalShoppingCartItem> source = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.Items;
			foreach (IProductEstimatedPointsAccrued productEstimatedPointsAccrued in estimatePointsAccruedResult.ProductEstimatedPointsAccruedList)
			{
				IRentalShoppingCartItem rentalShoppingCartItem = source.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == productEstimatedPointsAccrued.ProductId);
				if (rentalShoppingCartItem != null)
				{
					if (rentalShoppingCartItem.LoyaltyPointsRecord == null)
					{
						rentalShoppingCartItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartItem);
						rentalShoppingCartItem.LoyaltyPointsRecord.Clone(productEstimatedPointsAccrued.LoyaltyPointsRecord);
					}
					else
					{
						rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned = productEstimatedPointsAccrued.LoyaltyPointsRecord.PointsToBeEarned;
					}
					DisplayCheckoutProductModel displayCheckoutProductModel = shoppingCartViewModelFromView.DisplayProductModels.FirstOrDefault((DisplayCheckoutProductModel x) => x.Data == rentalShoppingCartItem);
					if (displayCheckoutProductModel != null)
					{
						PopulateDisplayCheckoutProductModel(displayCheckoutProductModel);
					}
				}
			}
		}
		else
		{
			Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOutLoyaltySession();
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
		}
		UpdateLoyaltyStatus(shoppingCartViewModelFromView, BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart);
	}

	private static void ConfigureShoppingCartDisplay(ShoppingCartViewModel shoppingCartViewModel)
	{
		shoppingCartViewModel.ADASpacerVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		int addMovieButtonVisibility;
		if (!BaseViewController.IsADAMode)
		{
			IRentalShoppingCartService rentalShoppingCartService = BaseViewController.RentalShoppingCartService;
			if (rentalShoppingCartService != null && !rentalShoppingCartService.CurrentRentalShoppingCart.IsFull)
			{
				addMovieButtonVisibility = 0;
				goto IL_0035;
			}
		}
		addMovieButtonVisibility = 2;
		goto IL_0035;
		IL_0035:
		shoppingCartViewModel.AddMovieButtonVisibility = (Visibility)addMovieButtonVisibility;
		bool flag = (ServiceLocator.Instance.GetService<IDataService>()?.GetInStockGameCount() ?? 0) > 0;
		IRentalShoppingCartService rentalShoppingCartService2 = BaseViewController.RentalShoppingCartService;
		shoppingCartViewModel.AddGameButtonVisibility = ((rentalShoppingCartService2 == null || rentalShoppingCartService2.CurrentRentalShoppingCart.IsFull || BaseViewController.IsADAMode || !flag) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems(shoppingCartViewModel);
	}

	private static List<DisplayCheckoutProductModel> CreateDisplayProductModelsFromCartItems(ShoppingCartViewModel shoppingCartViewModel)
	{
		List<DisplayCheckoutProductModel> list = new List<DisplayCheckoutProductModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			bool flag = rentalShoppingCart.Items.Count > 0;
			if (flag)
			{
				foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
				{
					IKioskProduct kioskProduct = null;
					service.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
					if (kioskProduct != null)
					{
						DisplayCheckoutProductModel displayCheckoutProductModel = new DisplayCheckoutProductModel
						{
							Name = kioskProduct.ProductName,
							Data = item,
							Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
						};
						PopulateDisplayCheckoutProductModel(displayCheckoutProductModel);
						list.Add(displayCheckoutProductModel);
					}
				}
			}
			shoppingCartViewModel.UsePointsButtonEnabled = flag;
		}
		return list;
	}

	private static void PopulateDisplayCheckoutProductModel(DisplayCheckoutProductModel displayCheckoutProductModel)
	{
		if (!(displayCheckoutProductModel?.Data is IRentalShoppingCartItem rentalShoppingCartItem))
		{
			return;
		}
		displayCheckoutProductModel.ConfigureForCheckout();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		IKioskProduct kioskProduct = null;
		service.GetProduct(rentalShoppingCartItem.ProductId, excludeTitleRollup: false, out kioskProduct);
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		displayCheckoutProductModel.ProductTitleText = BrowseFunctions.GetProductDisplayName(titleProduct);
		MDVProduct mDVProduct = titleProduct as MDVProduct;
		displayCheckoutProductModel.MDVFieldVisibility = ((mDVProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		displayCheckoutProductModel.MDVNumberOfDiscsText = ((mDVProduct != null) ? $"({mDVProduct.TitleProducts.Count} {Resources.multi_disc_vend_discs_text_all_caps})**" : null);
		displayCheckoutProductModel.MDVDiscVendText = ((mDVProduct != null) ? $"{mDVProduct.TitleProducts.Count} {Resources.disc_vend}" : null);
		TitleTypeProperty value = null;
		TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
		string productTypeText = $" - {value?.GetName(Resources.ResourceManager)}";
		displayCheckoutProductModel.ProductTypeText = productTypeText;
		string text = null;
		if (rentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
		{
			text = Resources.cart_purchase;
		}
		else
		{
			string arg = $"{rentalShoppingCartItem?.PricingRecord?.InitialDays ?? 1}-{Resources.night_rental}";
			string arg2 = $"{Resources.extra_night} {rentalShoppingCartItem?.PricingRecord.ExtraNight:C}";
			text = $"{arg} | {arg2}";
		}
		string arg3 = null;
		string productDescriptionLine3Text = null;
		if (PersonalizationFunctions.IsLoyaltySignedIn)
		{
			if (rentalShoppingCartItem.LoyaltyPointsRecord != null)
			{
				if (!rentalShoppingCartItem.LoyaltyPointsRecord.ToBeRedeemed)
				{
					if (rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned.HasValue)
					{
						if (rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned != 0)
						{
							arg3 = string.Format(Resources.loyalty_earning_points, rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned);
						}
					}
					else
					{
						arg3 = Resources.loyalty_retrieving_points;
					}
				}
				else
				{
					productDescriptionLine3Text = string.Format(Resources.loyalty_redeeming_points, rentalShoppingCartItem.LoyaltyPointsRecord.PointsToRedeem);
				}
			}
			else
			{
				arg3 = Resources.loyalty_retrieving_points;
			}
		}
		displayCheckoutProductModel.ProductDescriptionLine2Text = $"{text}{arg3}";
		displayCheckoutProductModel.ProductDescriptionLine3Text = productDescriptionLine3Text;
		displayCheckoutProductModel.ProductDescriptionLine3TextVisibility = (string.IsNullOrEmpty(displayCheckoutProductModel.ProductDescriptionLine3Text) ? Visibility.Collapsed : Visibility.Visible);
		displayCheckoutProductModel.ProductCartPriceText = $"{rentalShoppingCartItem.Price:C}";
		displayCheckoutProductModel.TopCancelButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
		displayCheckoutProductModel.BottomCancelButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void ConfigureCommands(ShoppingCartViewModel shoppingCartViewModel)
	{
		shoppingCartViewModel.OnBackButtonClicked += ShoppingCartViewModel_OnBackButtonClicked;
		shoppingCartViewModel.OnAddMovieButtonClicked += ShoppingCartViewModel_OnAddMovieButtonClicked;
		shoppingCartViewModel.OnAddGameButtonClicked += ShoppingCartViewModel_OnAddGameButtonClicked;
		shoppingCartViewModel.OnSignInButtonClicked += ShoppingCartViewModel_OnSignInButtonClicked;
		shoppingCartViewModel.OnUsePointsButtonClicked += ShoppingCartViewModel_OnUsePointsButtonClicked;
		shoppingCartViewModel.OnLoaded += ShoppingCartViewModel_OnLoaded;
		shoppingCartViewModel.OnTermsAndPrivacyButtonClicked += delegate
		{
			HelpViewController.ShowTermsAndConditions();
		};
		int? num = (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession)?.StoredPromoCodes?.Count;
		if (!num.HasValue || num.GetValueOrDefault() == 0)
		{
			shoppingCartViewModel.OnPromoCodeButtonClicked += delegate
			{
				OrchestrationService.ViewShow("keyboard_promo_view", new KeyboardInfo
				{
					MaxLength = 31,
					KeyboardText = "",
					IsValidEntry = ValidationServices.IsValidPromoCode,
					CancelAction = delegate
					{
						OrchestrationService.ViewGoBack("go back", popOnly: false);
					},
					ContinueAction = ValidatePromoCode
				});
			};
		}
		else
		{
			shoppingCartViewModel.OnPromoCodeButtonClicked += delegate
			{
				OrchestrationService.ViewShow("apply_promo_view", new ApplyPromoInfo
				{
					ApplyPromoCodeAction = ValidatePromoCode
				});
			};
		}
		shoppingCartViewModel.OnUpdateBagButtonClicked += delegate
		{
			OrchestrationService.ViewShow("shopping_cart_update_ada_view");
		};
		shoppingCartViewModel.OnCancelBrowseItemModel += RemoveItemFromShoppingCart;
		shoppingCartViewModel.OnPayButtonClicked += ShoppingCartViewModel_OnPayButtonClicked;
	}

	private static void ShoppingCartViewModel_OnLoaded()
	{
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (!string.IsNullOrEmpty(personalizationSession?.SelectedPromo) && personalizationSession.SelectedPromo != rentalShoppingCart?.Promotion?.Name)
		{
			LogHelper.Instance.Log($"Validating promotion {personalizationSession.SelectedPromo} selected from My Perks screen.");
			ValidatePromoCode(personalizationSession.SelectedPromo);
		}
	}

	private static void ValidatePromoCode(string promoCode)
	{
		LogHelper.Instance.Log("calling show promo callback");
		Action<IPromoCodeValidationResult> doneCallback = delegate
		{
			BaseViewController.GetViewService().UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_complete)");
		};
		Action<IPromoCodeValidationResult> afterErrorMessageCallback = delegate
		{
			BaseViewController.GetViewService().UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_error)");
		};
		PromotionFunctions.ValidatePromoCode(promoCode, doneCallback, afterErrorMessageCallback);
	}

	private static void ShoppingCartViewModel_OnUsePointsButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "UsePoints");
		if (!PersonalizationFunctions.IsLoyaltySignedIn)
		{
			MessagePopupViewController.ShowLoyaltyIsDownMessage(delegate
			{
				OrchestrationService.ViewGoBackSafe("Loyalty is down message popup timeout", popOnly: false);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("Loyalty is down message popup continue", popOnly: false);
			});
		}
		else
		{
			int? num = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance;
			if (!num.HasValue || num.GetValueOrDefault() == 0)
			{
				PersonalizationFunctions.ShowNotEnoughPointsError();
			}
			else
			{
				PersonalizationFunctions.ShowUseMemberPoints();
			}
		}
	}

	private static void ShoppingCartViewModel_OnSignInButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "signin-cart");
		OrchestrationService.ViewShow("sign_in_view");
	}

	private static void RemoveItemFromShoppingCart(DisplayProductModel displayProductModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		object obj = displayProductModel?.Data;
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", $"delete {(obj as IRentalShoppingCartItem)?.ProductId}");
		ShoppingCartViewFunctions.RemoveItemFromShoppingCart(displayProductModel, parameter);
	}

	private static void ShoppingCartViewModel_OnPayButtonClicked()
	{
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null)
		{
			return;
		}
		IPromotion promotion = rentalShoppingCart.Promotion;
		if (promotion != null && promotion.Intent == PromotionIntentCode.RxGx)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
			{
				OrchestrationService.ExecuteScript("check_out");
				return;
			}
			MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup timed out", popOnly: false);
			}, delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "removePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "keepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			}, promoCodeValidationInfo.Title + Environment.NewLine + Environment.NewLine + promoCodeValidationInfo.Message);
		}
		else if (promotion != null && promotion.Intent == PromotionIntentCode.FirstNightFreeItem && (promotion.CampaignTitles?.Titles?.Count ?? 0) <= 0)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo2 = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo2 != null && promoCodeValidationInfo2.IsCartValid)
			{
				OrchestrationService.ExecuteScript("check_out");
				return;
			}
			BaseViewController.SessionAnalyticsService?.AddViewEvent("context", "firstnightfreepromoWarning");
			string empty = string.Empty;
			MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(message: promotion.ProductTypeId.HasValue ? PromotionFunctions.GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.ProductTypeId) : (((promotion.FormatIds?.Count ?? 0) <= 0) ? string.Empty : PromotionFunctions.GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.FormatIds)), timeOutAction: delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup timed out", popOnly: false);
			}, backAction: delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "removePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, continueAction: delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "keepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			});
		}
		else if (promotion != null && (promotion.CampaignTitles?.Titles?.Count ?? 0) > 0)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo3 = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo3 != null && promoCodeValidationInfo3.IsCartValid)
			{
				OrchestrationService.ExecuteScript("check_out");
				return;
			}
			BaseViewController.SessionAnalyticsService?.AddViewEvent("context", "titlelevelpromoWarning");
			MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup timed out", popOnly: false);
			}, delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "removePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "keepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			});
		}
		else
		{
			ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
		}
	}

	private static void ShowInCartOfferOrProceedCheckout(IRentalShoppingCart rentalShoppingCart)
	{
		if (!PromotionService.Instance.IsFreeInCartOfferRunning && !PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.CartQualifiesForInCartOffer(rentalShoppingCart) && !rentalShoppingCart.InCartPromoOfferMade)
		{
			ShoppingCartViewFunctions.ShowInCartOfferMessage();
		}
		else
		{
			OrchestrationService.ExecuteScript("check_out");
		}
	}

	private static void ShoppingCartViewModel_OnAddGameButtonClicked()
	{
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.InCartTitleMarketing = false;
		}
		BrowseProducts(TitleFamily.Games, null);
	}

	private static void ShoppingCartViewModel_OnBackButtonClicked()
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "back_button");
		BrowseProducts(BrowseState.TitleFamily, BrowseState.Genre);
	}

	private static void ShoppingCartViewModel_OnAddMovieButtonClicked()
	{
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.InCartTitleMarketing = false;
		}
		BrowseProducts(TitleFamily.Movies, null);
	}

	private static void BrowseProducts(TitleFamily titleFamily, Genres? genre)
	{
		BrowseState.TitleFamily = titleFamily;
		BrowseState.TitleType = TitleType.All;
		BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		BrowseState.Genre = genre;
		BrowseState.BrowseMode = BrowseViewFilter.All;
		BrowseState.PageNumber = 1;
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
	}

	private static void ConfigureTextAndVisibility(ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
			IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
			shoppingCartViewModel.LabelTop = Resources.my_bag;
			shoppingCartViewModel.PriceHeaderText = Resources.summary;
			shoppingCartViewModel.BackButtonText = Resources.back_button;
			shoppingCartViewModel.TermsText = Resources.by_pressing_pay;
			shoppingCartViewModel.PassiveSaleNote = Resources.passive_sale_note;
			shoppingCartViewModel.MDVFootnoteVisibility = ((!rentalShoppingCart.Items.Any((IRentalShoppingCartItem x) => x.MultiDisc)) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.MDVFootnote = (rentalShoppingCart.Items.Any((IRentalShoppingCartItem x) => x.MultiDisc) ? Resources.multi_disc_vend_footnote : null);
			int num = (int)((rentalShoppingCart?.DefaultServiceFee ?? 0m) * 100m);
			shoppingCartViewModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
			shoppingCartViewModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.TermsAndPrivacyButtonText = Resources.terms_privacy;
			shoppingCartViewModel.PromoCodeButtonText = Resources.promo_code_button;
			if ((personalizationSession?.StoredPromoCodes?.Count ?? 0) > 0)
			{
				shoppingCartViewModel.PromoCodeButtonText += $" ({personalizationSession.StoredPromoCodes.Count})";
			}
			shoppingCartViewModel.SignInButtonText = Resources.sign_in;
			shoppingCartViewModel.SignInButtonVisibility = ((!PersonalizationFunctions.IsSignInEnabled) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.UsePointsButtonText = Resources.use_points;
			shoppingCartViewModel.UsePointsButtonVisibility = ((!PersonalizationFunctions.IsLoyaltySignedIn) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.UsePointsButtonEnabled = (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.Count ?? 0) > 0;
			shoppingCartViewModel.DontMissYourPerksText = Resources.perks_dont_miss;
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label;
			shoppingCartViewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
			shoppingCartViewModel.PricingModel.SubtotalLabelText = Resources.subtotal_label;
			shoppingCartViewModel.PricingModel.TaxLabelText = Resources.tax_label;
			shoppingCartViewModel.PricingModel.TotalLabelText = Resources.total_label;
			shoppingCartViewModel.PointsEstimateIntroText = Resources.points_estimate_intro;
			shoppingCartViewModel.PointsEstimateEndText = Resources.points_estimate_end;
			shoppingCartViewModel.PricingModel.SubtotalValueText = $"{rentalShoppingCart?.Totals?.SubTotal:C}";
			string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
			shoppingCartViewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.PricingModel.TaxValueText = $"{rentalShoppingCart?.Totals?.TaxAmount:C}";
			shoppingCartViewModel.PricingModel.TotalValueText = $"{rentalShoppingCart?.Totals?.GrandTotal:f2}";
			shoppingCartViewModel.PromoCodeButtonEnabled = (rentalShoppingCart == null || !rentalShoppingCart.InCartPromoOfferMade || rentalShoppingCart == null || rentalShoppingCart.InCartPromoOfferCancel) && (rentalShoppingCart == null || rentalShoppingCart.Items.Any((IRentalShoppingCartItem x) => !x.PricingRecord.IsMultiNightPrice) || (rentalShoppingCart?.Items.Count ?? 0) == 0);
			string text2 = $"{((!((rentalShoppingCart?.Totals?.DiscountAmountUsed ?? 0m) != 0m)) ? ((decimal?)null) : rentalShoppingCart?.Totals?.DiscountAmountUsed):C}";
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = $"-{text2}";
			}
			shoppingCartViewModel.PricingModel.DiscountValueText = text2;
			shoppingCartViewModel.PricingModel.DiscountLineVisibility = ((!((rentalShoppingCart?.Totals?.DiscountAmountUsed ?? 0m) > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.PricingModel.ServiceFeeValueText = $"{rentalShoppingCart?.Totals?.ServiceFee:C}";
			shoppingCartViewModel.PricingModel.ServiceFeeLineVisibility = ((!((rentalShoppingCart?.Totals?.ServiceFee ?? 0m) > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.PayButtonEnabled = rentalShoppingCart != null && rentalShoppingCart.Count > 0;
			shoppingCartViewModel.PayButtonText = Resources.pay_button;
			FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
			shoppingCartViewModel.AddMovieButtonStyle = WPFFunctions.GetStyle(viewUserControl, OrchestrationService.IsCultureSpanish() ? "spanish_add_movie_button_style" : "english_add_movie_button_style");
			shoppingCartViewModel.AddMovieButtonText = Resources.add_movie_button_text;
			shoppingCartViewModel.AddGameButtonStyle = WPFFunctions.GetStyle(viewUserControl, OrchestrationService.IsCultureSpanish() ? "spanish_add_game_button_style" : "english_add_game_button_style");
			shoppingCartViewModel.AddGameButtonText = Resources.add_game_button_text;
			shoppingCartViewModel.UpdateBagButtonText = Resources.update_bag;
			shoppingCartViewModel.UpdateBagButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			UpdateLoyaltyStatus(shoppingCartViewModel, rentalShoppingCart);
		}
	}

	private static void UpdateLoyaltyStatus(ShoppingCartViewModel shoppingCartViewModel, IRentalShoppingCart shoppingCart)
	{
		shoppingCartViewModel.LoyaltySystemErrorText = ((PersonalizationFunctions.IsSignedIn && PersonalizationFunctions.IsLoyaltyEnabled && PersonalizationFunctions.IsLoyaltySignInError) ? Resources.error_points_down : null);
		int num = 0;
		num = shoppingCart.Items.Where((IRentalShoppingCartItem i) => !(i.LoyaltyPointsRecord?.ToBeRedeemed ?? true)).Sum((IRentalShoppingCartItem i) => i.LoyaltyPointsRecord?.PointsToBeEarned ?? 0);
		shoppingCartViewModel.PointsEstimateVisibility = ((!PersonalizationFunctions.IsLoyaltySignedIn || num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.PointsEstimateText = string.Format(Resources.points_estimate_text, num);
	}

	private static void ConfigureTTS(ShoppingCartViewModel shoppingCartViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			shoppingCartViewModel.OnGetSpeechControl = () => ShoppingCartViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView);
		}
	}

	private static ShoppingCartViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement shoppingCartViewUserControl = BaseViewController.GetViewUserControl();
		if (shoppingCartViewUserControl != null)
		{
			return shoppingCartViewUserControl.Dispatcher.Invoke(() => shoppingCartViewUserControl?.DataContext as ShoppingCartViewModel);
		}
		return null;
	}
}
