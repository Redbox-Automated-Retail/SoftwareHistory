using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Promotion;
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
			ConfigureTextAndVisibility(shoppingCartViewModel);
			ConfigureCommands(shoppingCartViewModel);
			ConfigureShoppingCartDisplay(shoppingCartViewModel);
			ConfigureTTS(shoppingCartViewModel);
			viewUserControl.DataContext = shoppingCartViewModel;
			BaseLoyaltyController.GetEstimateAccruedPoints(ProcessEstimateAccruedPointsResult);
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
		UpdateLoyaltyStatus(shoppingCartViewModelFromView);
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
		service.GetProduct(rentalShoppingCartItem.ProductId, excludeTitleRollup: true, out kioskProduct);
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		displayCheckoutProductModel.ProductTitleText = titleProduct?.ProductName;
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
					arg3 = ((!rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned.HasValue) ? Resources.loyalty_retrieving_points : string.Format(Resources.loyalty_earning_points, rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned));
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
		shoppingCartViewModel.OnTermsAndPrivacyButtonClicked += delegate
		{
			HelpViewController.ShowTermsAndConditions();
		};
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
		shoppingCartViewModel.OnUseCreditsButtonClicked += delegate
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "credits_button");
			IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null)
			{
				rentalShoppingCart.UseCredits = CreditElection.OptedIn;
			}
			OrchestrationService.ExecuteScript("check_out");
		};
		shoppingCartViewModel.OnUpdateBagButtonClicked += delegate
		{
			OrchestrationService.ViewShow("shopping_cart_update_ada_view");
		};
		shoppingCartViewModel.OnCancelBrowseItemModel += RemoveItemFromShoppingCart;
		shoppingCartViewModel.OnPayButtonClicked += ShoppingCartViewModel_OnPayButtonClicked;
	}

	private static void ValidatePromoCode(string promoCode)
	{
		LogHelper.Instance.Log("calling show promo callback");
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimer = true,
			MessageText = Resources.promo_code_validating_message
		});
		OrchestrationService.AnalyticsAddNode("promoCodes", "promoCode");
		OrchestrationService.AnalyticsAddAttribute("promoCode", "code", promoCode);
		if (promoCode == PromotionService.Instance?.GetInCartPromoCode())
		{
			LogHelper.Instance.Log("User manualy entered the InCartPromoCode - clearing!");
			promoCode = null;
		}
		PromotionService.Instance?.ValidatePromoCodeOnServer(promoCode, ProcessPromoCodeValidationResult);
	}

	private static void ProcessPromoCodeValidationResult(IPromoCodeValidationResult promoCodeValidationResult)
	{
		Task.Factory.StartNew(delegate
		{
			bool flag = true;
			if (promoCodeValidationResult.Success)
			{
				OrchestrationService.AnalyticsAddAttribute("promoCode", "valid", "True");
				decimal num = promoCodeValidationResult?.Amount ?? 0m;
				decimal num2 = ((promoCodeValidationResult.IntentCode != Redbox.Rental.Model.PromotionIntentCode.PercentOffCartTotal) ? 0m : (promoCodeValidationResult?.Amount ?? 0m));
				IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = promoCodeValidationResult.PromoCode,
					OriginalPrice = num,
					Price = num,
					Quantity = 1,
					Intent = promoCodeValidationResult.IntentCode,
					OriginalPercent = num2,
					Percent = num2,
					RentQuantity = (byte)promoCodeValidationResult.RentQuantity,
					RentFormat = promoCodeValidationResult.RentFormat,
					GetQuantity = (byte)promoCodeValidationResult.GetQuantity,
					GetFormat = promoCodeValidationResult.GetFormat,
					CampaignTitles = promoCodeValidationResult.CampaignTitles
				};
				PromotionService.Instance.SetPromotion(promotion);
				IPromoCodeValidationInfo promoCodeValidationInfo = BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.UpdateServiceFee();
				Action action = delegate
				{
					BaseViewController.GetViewService().UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_complete)");
				};
				if (promoCodeValidationResult.IntentCode == Redbox.Rental.Model.PromotionIntentCode.FirstNightGameGameRequired && !promoCodeValidationInfo.IsCartValid && !promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing)
				{
					promoCodeValidationResult.Success = false;
					promoCodeValidationResult.Response = "GameRequired";
				}
				else if (!promoCodeValidationInfo.IsCartValid && promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing)
				{
					MessagePopupViewController.ShowMultiNightPricingPromoCodeInvalidWarning(action, action, promoCodeValidationInfo.Title);
				}
				else if (promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.RxGx)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(action, action, promoCodeValidationInfo.Message);
					}
					else
					{
						action();
					}
				}
				else if ((promotion?.CampaignTitles?.Titles?.Count ?? 0) > 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(action, action);
					}
					else
					{
						action();
					}
				}
				else
				{
					OrchestrationService.AnalyticsAddAttribute("promoCode", "valid", num.ToString());
					action();
				}
			}
			if (!promoCodeValidationResult.Success)
			{
				Action action2 = delegate
				{
					BaseViewController.GetViewService().UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_error)");
				};
				OrchestrationService.AnalyticsAddAttribute("promoCode", "valid", "False");
				OrchestrationService.AnalyticsAddAttribute("promoCode", "value", "0.00");
				OrchestrationService.AnalyticsAddViewEvent("context", "promoCodeDecline");
				if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeAlreadyUsed.ToString())
				{
					MessagePopupViewController.ShowPromoCodeAlreadyUsedWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeNotValidAtThisLocation.ToString())
				{
					MessagePopupViewController.ShowPromoCodeNotValidAtThisLocationWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.Error.ToString() || promoCodeValidationResult.Response == "Server Busy" || promoCodeValidationResult.Response == "Communications Failure")
				{
					flag = false;
					MessagePopupViewController.ShowPromoCodeValidationFailureWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == "GameRequired")
				{
					MessagePopupViewController.ShowPromoCodeGameRequiredWarning(action2, action2);
				}
				else
				{
					MessagePopupViewController.ShowPromoCodeNotValidWarning(action2, action2);
				}
				OrchestrationService.AnalyticsAddAttribute("promoCode", "online", flag.ToString());
			}
			OrchestrationService.AnalyticsAddAttribute("promoCode", "serverFailureReason", promoCodeValidationResult.Response);
		}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
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
		LogHelper.Instance.Log("Setting session_use_credits to OptedOut");
		rentalShoppingCart.UseCredits = CreditElection.OptedOut;
		if (promotion != null && promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.RxGx)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
			{
				OrchestrationService.ExecuteScript("check_out");
				return;
			}
			string message = $"{promoCodeValidationInfo.Title}{Environment.NewLine + Environment.NewLine}{promoCodeValidationInfo.Message}";
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
				OrchestrationService.ViewShow("browse_view");
			}, message);
		}
		else if (promotion != null && promotion.CampaignTitles != null)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo2 = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo2 != null && promoCodeValidationInfo2.IsCartValid)
			{
				OrchestrationService.ExecuteScript("check_out");
				return;
			}
			BaseViewController.SessionAnalyticsService?.AddViewEvent("context", "titlelevelpromoWarning");
			MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(delegate
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
				OrchestrationService.ViewShow("browse_view");
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
		OrchestrationService.ViewShow("browse_view");
	}

	private static void ConfigureTextAndVisibility(ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
			shoppingCartViewModel.LabelTop = Resources.my_bag;
			shoppingCartViewModel.PriceHeaderText = Resources.summary;
			shoppingCartViewModel.BackButtonText = Resources.back_button;
			bool flag = rentalShoppingCart != null && Redbox.Rental.Services.Configuration.Configuration.Instance.EnableCreditsButton && rentalShoppingCart.Promotion == null && !rentalShoppingCart.HasItemsToBeRedeemed && (!rentalShoppingCart.InCartPromoOfferMade || rentalShoppingCart.InCartPromoOfferCancel) && !rentalShoppingCart.Items.Any((IRentalShoppingCartItem x) => x.PricingRecord.IsMultiNightPrice);
			shoppingCartViewModel.UseCreditsButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.UseCreditsButtonEnabled = (rentalShoppingCart?.Count ?? 0) > 0;
			shoppingCartViewModel.TermsText = (flag ? Resources.credit_label_bottom_text : Resources.by_pressing_pay);
			shoppingCartViewModel.UseCreditsButtonText = Resources.use_credits_button;
			shoppingCartViewModel.PassiveSaleNote = Resources.passive_sale_note;
			int num = (int)((rentalShoppingCart?.DefaultServiceFee ?? 0m) * 100m);
			shoppingCartViewModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
			shoppingCartViewModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
			shoppingCartViewModel.TermsAndPrivacyButtonText = Resources.terms_privacy;
			shoppingCartViewModel.PromoCodeButtonText = Resources.promo_code_button;
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
			UpdateLoyaltyStatus(shoppingCartViewModel);
		}
	}

	private static void UpdateLoyaltyStatus(ShoppingCartViewModel shoppingCartViewModel)
	{
		shoppingCartViewModel.LoyaltySystemErrorText = ((PersonalizationFunctions.IsSignedIn && PersonalizationFunctions.IsLoyaltyEnabled && PersonalizationFunctions.IsLoyaltySignInError) ? Resources.error_points_down : null);
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
