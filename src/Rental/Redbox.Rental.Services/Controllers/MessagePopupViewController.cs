using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController : BaseViewController
{
	public static void ReenableTimer()
	{
		((MessagePopupViewUserControl)BaseViewController.GetViewUserControl())?.ReenableTimer();
	}

	public static void ShowRecommendationOnPickupPrompt(Action timeOutAction, Action pickupAction, Action continueBrowsingAction)
	{
		LogHelper.Instance.Log("Showing Recommendation On Pickup prompt");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.reco_need_more_time, (string)null, (string)null, (string)null, Resources.pickup, (string)null, (string)null, $"{Resources.keep_browsing_1} {Resources.keep_browsing_2}", "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), pickupAction, continueBrowsingAction, timeOutAction, "RecommendationOnPickupPrompt", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowSimpleMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction, string context = null)
	{
		LogHelper.Instance.Log("Showing Simple Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "message_popup_dialog_title_rbpurple_style", cultureSpecificMessage, "font_montserrat_small_rbpurple", null, null, null, cultureSpecificButtonText, "button_text_auto_size_style", "message_popup_warmpurple_auto_size_button_style", null, null, null, null, GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "SimpleMessage", null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), context));
	}

	public static void ShowRelatedTitlesNotFoundMessage(Action continueAction, Action timeOutAction, string context = null)
	{
		LogHelper.Instance.Log("Showing Simple Message: {0}", Resources.related_movies_popup_header);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.related_movies_popup_header, "message_popup_dialog_title_rbpurple_style", Resources.related_movies_not_found_message, "font_montserrat_small_rbpurple", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "RelatedTitlesNotFoundMessage", null, "Transparent", new Thickness(52.0, 250.0, 52.0, 220.0), context));
	}

	public static void ShowAuthErrorMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction, List<string> analyticsContexts = null)
	{
		LogHelper.Instance.Log("Showing Auth Error Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "message_popup_dialog_title_style4", cultureSpecificMessage, "message_popup_dialog_mesg_style4", null, null, null, cultureSpecificButtonText, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "SimpleMessage", null, "Transparent", default(Thickness), analyticsContexts));
	}

	public static void ShowLogOutWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Log out warning message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.sign_out_dialog_title, "message_popup_dialog_title_rbpurple_style", Resources.sign_out_dialog_message, "font_montserrat_small_rbpurple", Resources.back_button, "font_museoslab_900_28_rubine", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.sign_out_button, "message_popup_button_text_style", OrchestrationService.IsCultureSpanish() ? "message_popup_warmpurple_button_signout_dialog_style" : "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "SignOutWarningMessage", (UserControl)null, "Transparent", OrchestrationService.IsCultureSpanish() ? new Thickness(180.0, 208.0, 180.0, 208.0) : new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowSimpleOkMessage(string cultureSpecificTitle, string cultureSpecificMessage, Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Ok Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "message_popup_dialog_title_rbpurple_style", cultureSpecificMessage, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, Resources.okay, "font_museo_slab_900_28", "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "OkMessage", (UserControl)null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowLoyaltyIsDownOnLoginMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down on login message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.personalization_service_error_KLI0005, "message_popup_dialog_title_rbpurple_style", (string)null, (string)null, (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "LoyaltyIsDownMessage", (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowOfferTimeoutMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing offer dialog timeout message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.offer_timeout_title, "message_popup_dialog_title_black_style", Resources.offer_timeout_message, "font_montserrat_small_black", Resources.no_thanks_button_text, "font_museo_slab_900_28", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.yes_button_mixed_case, "font_museo_slab_900_28", "message_popup_warmpurple_button_style", "message_popup_dialog_button_grid_style", "message_popup_dialog_inner_grid_style5", (string)null, (string)null, GetDefaultMessageTimeOut(30000, "short"), backAction, continueAction, timeOutAction, "OfferTimeoutMessage", (UserControl)null, "White", new Thickness(128.0, 96.0, 128.0, 96.0), (string)null, (string)null));
	}

	public static void ShowLoyaltyIsDownMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.loyalty_down_dialog_title, "message_popup_dialog_title_rbpurple_style", Resources.loyalty_down_dialog_text, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "LoyaltyIsDownMessage", (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowInCartOfferMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer message");
		InCartOfferMessageText inCartOfferMessageText = PromotionService.Instance.GetInCartOfferMessageText();
		ShowMessagePopupView(new ShowMessagePopupViewParameter(inCartOfferMessageText.Header, "message_popup_dialog_title_style2", inCartOfferMessageText.Message, (string)null, (PromotionService.Instance.IsFreeInCartOfferRunning || (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.ValidateRentTwoInCartPromo().IsCartValid)) ? null : inCartOfferMessageText.NoButtonText, (string)null, (string)null, inCartOfferMessageText.YesButtonText, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "InCartOfferMessage", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowGiftCardNotEnabledMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing gift card not enabled message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.gc_not_available_title, "message_popup_dialog_title_style2", Resources.gc_not_available, (string)null, (string)null, (string)null, (string)null, Resources.continue_button, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "GiftCardNotEnabledMessage", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, (string)null, Resources.do_not_use_promo_button, "font_target_small_medium", (string)null, Resources.use_promo_button, "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing title level promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, null, null, null, null, Resources.continue_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "TitleLevelPromoCodeInvalidWarning", null, null, default(Thickness), "titlelevelpromoWarning"));
	}

	public static void ShowFirstNightFreePromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing first night free promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", message, (string)null, Resources.do_not_use_promo_button, "font_target_small_medium", (string)null, Resources.use_promo_button, "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "FirstNightFreePromoCodeInvalidWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowFirstNightFreePromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing first night free promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", message, null, null, null, null, Resources.continue_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "FirstNightFreePromoCodeInvalidWarning", null, null, default(Thickness), "firstnightfreepromoWarning"));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.oops, null, message, null, Resources.do_not_use_promo_button, "font_target_small_medium", null, Resources.use_promo_button, "font_target_small_medium", null, null, null, null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", null, null, default(Thickness), "rxgxWarning"));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.oops, null, message, null, null, null, null, Resources.continue_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeRxGxInvalidWarning", null, null, default(Thickness), "rxgxWarning"));
	}

	public static void ShowPromoCodeInvalidForCartItemsWarning(Action timeOutAction, Action continueAction, string title)
	{
		LogHelper.Instance.Log("Showing promo code invalid for cart items warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, Resources.continue_button, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "PromoCodeInvalidForCartItemsWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowPromoCodeAlreadyUsedWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code already used warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_already_used_message_title, null, Resources.promo_code_already_used_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeAlreadyUsedWarning", null, null, default(Thickness), "promoAlreadyUsed"));
	}

	public static void ShowPromoCodeNotValidAtThisLocationWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid at this location warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_error_title, null, Resources.promo_code_not_valid_location_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidAtThisLocationWarning", null, null, default(Thickness), "promoInvalidLocation"));
	}

	public static void ShowPromoCodeForOnDemandWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code for on demand warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_error_title, null, Resources.promo_code_for_on_demand_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeForOnDemandWarning", null, null, default(Thickness), "promoForOnDemand"));
	}

	public static void ShowPromoCodeValidationFailureWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code validation failure warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_validation_failure_message_title, null, Resources.promo_code_validation_failure_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeValidationFailureWarning", null, null, default(Thickness), "promoCantBeUsed"));
	}

	public static void ShowPromoCodeGameRequiredWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code game required warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_game_required_title, null, Resources.promo_code_game_required_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeGameRequiredWarning", null, null, default(Thickness), "promoCantBeUsed"));
	}

	public static void ShowPromoCodeNotValidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_not_valid_message_title, null, Resources.promo_code_not_valid_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidWarning", null, null, default(Thickness), "promoNotValid"));
	}

	public static void ShowInCartCancelWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer cancel warning message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.in_cart_remove_item_warning_header, (string)null, Resources.in_cart_remove_item_warning_message, (string)null, Resources.back_button, Resources.continue_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "InCartCancelWarning", (string)null));
	}

	public static void ShowSubPlatformWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Sub Platform Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["Subplatform:Header"];
		string messageText = service["Subplatform:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "SubPlatformWarning", "subplatformWarning"));
	}

	public static void ShowADAPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ADA Purchase Warning message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(ServiceLocator.Instance.GetService<IMacroService>()["ADAPurchaseWarning:Header"], "message_popup_dialog_title_style2", (string)null, (string)null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ADAPurchaseWarning", (string)null));
	}

	public static void ShowPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["PurchaseWarning:Header"];
		string messageText = service["PurchaseWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PurchaseWarning", "purchaseWarning"));
	}

	public static void ShowMatureWarning(Action timeOutAction, Action backAction, Action continueAction, List<string> analyticsContexts)
	{
		LogHelper.Instance.Log("Showing Mature Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["MatureWarning:Header"];
		string messageText = service["MatureWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, "message_popup_dialog_title_style2", messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning", analyticsContexts));
	}

	public static void Show4KAlert(Action timeOutAction, Action backAction, Action continueAction, string analyticsContext)
	{
		LogHelper.Instance.Log("Showing 4K UHD Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.four_k_alert_title, "message_popup_dialog_title_style3", Resources.four_k_alert, "message_popup_dialog_mesg_style3", Resources.cancel_button, "font_museo_slab_900_20", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.got_it_button, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "4KUHDAlert", null, "White", new Thickness(180.0, 192.0, 180.0, 192.0), analyticsContext, "4K_warning_popup_view"));
	}

	public static void ShowMultiDiscAlert(string title, string message, string buttonText, Action timeOutAction, Action continueAction, string analyticsContext)
	{
		LogHelper.Instance.Log("Showing Multi Disc Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, "message_popup_dialog_title_style3", message, "message_popup_dialog_mesg_style3", null, null, null, buttonText, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "MultiDiscAlert", null, "White", new Thickness(185.0, 144.0, 185.0, 124.0), analyticsContext, "multi_disc_warning_popup_view"));
	}

	public static void ShowDigitalCodeAlert(Action backAction, Action okAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_title"), BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_message"), BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, okAction, timeOutAction, "DigitalCodeAlert", "digitalcodeincart_message_view"));
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "title_thanks"), BundleFunctions.GetDictionaryStringValue("strings", "inventory_updated_message"), (string)null, (string)null, GetDefaultMessageTimeOut(5000, "short"), (Action)null, (Action)null, timeOutAction, "ViewReturnSuccess", (string)null));
	}

	public static void ShowMessageCartFull(Action timeoutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Cart Full message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "cart_full_header"), BundleFunctions.GetDictionaryStringValue("strings", "cart_full_body"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), OrchestrationService.GetIdleTimerTimeout(), (Action)null, continueAction, timeoutAction, "CartFull", (string)null));
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Hardware Error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter((string)null, BundleFunctions.GetDictionaryStringValue("strings", "hardware_error_message"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewHardwareError", (string)null));
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Backward Inserted message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "return_error"), BundleFunctions.GetDictionaryStringValue("strings", "backward_insert_error"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnBackwardInserted", (string)null));
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Unrecognized Barcode message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "oops"), BundleFunctions.GetDictionaryStringValue("strings", "unrecognized_barcode_returned_message"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnUnrecognizedBarcode", (string)null));
	}

	public static void ShowMessagePopupViewCardNotRecognized(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(null, Resources.invalid_card_message, null, Resources.continue_button, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewCardNotRecognized", "cardNotRecognized"));
	}

	public static void ShowMessagePopupViewCheckoutCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		ShowMessagePopupViewCardReaderTampered(timeOutAction, continueAction, Resources.card_reader_tampered_checkout_flow_message_body);
	}

	public static void ShowMessagePopupViewReservationCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		ShowMessagePopupViewCardReaderTampered(timeOutAction, continueAction, Resources.card_reader_tampered_reservation_flow_message_body);
	}

	private static void ShowMessagePopupViewCardReaderTampered(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing Tampered card reader message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.card_reader_tampered_message_title, "message_popup_dialog_title_style4", message, "message_popup_dialog_mesg_style4", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewSwipedCardIsChipEnabled", null, "Transparent", default(Thickness), "swipedCardIsChipEnabled"));
	}

	public static void ShowMessagePopupViewSwipedCardIsChipEnabled(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Swiped Card is chip enabled message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.swiped_card_is_chip_enabled_title, "message_popup_dialog_title_style4", Resources.swiped_card_is_chip_enabled_message, "message_popup_dialog_mesg_style4", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewSwipedCardIsChipEnabled", null, "Transparent", default(Thickness), "swipedCardIsChipEnabled"));
	}

	public static void ShowMessagePopupViewBadChipReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad chip read error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.bad_chip_read_title, "message_popup_dialog_title_style4", Resources.bad_chip_read_message, "message_popup_dialog_mesg_style4", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewBadChipReadError", null, "Transparent", default(Thickness), "badChipReadError"));
	}

	public static void ShowMessagePopupViewBadContactlessReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad contactless read error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.bad_contactless_read_title, "message_popup_dialog_title_style4", Resources.bad_contactless_read_message, "message_popup_dialog_mesg_style4", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewBadContactlessReadError", null, "Transparent", default(Thickness), "badContactlessReadError"));
	}

	public static void ShowMessagePopupViewCardNotRecognizedTechnicalFallback(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized Technical Fallback message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.technical_fallback_title, "message_popup_dialog_title_style4", Resources.technical_fallback_message, "message_popup_dialog_mesg_style4", null, null, null, Resources.got_it_button, "font_museo_slab_900_20", "message_popup_dialog_button_style4", "message_popup_dialog_button_grid_style4", "message_popup_dialog_inner_grid_style4", "message_popup_dialog_floating_button_grid_style4", "message_popup_dialog_message_dock_panel_style4", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "ViewCardNotRecognizedTechnicalFallback", null, "Transparent", default(Thickness), "cardNotRecognizedTechnicalFallback"));
	}

	public static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Vend Error message popup");
		ShowCriticalErrorView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "vend_error_title"), "message_popup_dialog_title_style2", BundleFunctions.GetDictionaryStringValue("strings", "vend_error_message"), "message_popup_dialog_mesg_style2", (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewVendError", (string)null));
	}

	public static void ShowMessagePopupViewDigitalCode(string price, Action backAction, Action buyAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_title"), null, BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), "font_target_smaller_overlays", null, string.Format(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_buy_button"), price), "font_black_target_smaller_overlays", "classic_wide_yellow_button_style", null, null, null, null, GetDefaultMessageTimeOut(), backAction, buyAction, timeOutAction, "ViewDigitalCode", new DigitalCodePopupUserControl
		{
			DataContext = new
			{
				MessageLine1 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_1"),
				MessageLine2 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_2"),
				MessageLine3 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_3")
			}
		}, null, default(Thickness), "whatdigitalcode_message_view"));
	}

	public static void ShowReserveOnlineRentDialog(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Reserve Online message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.reserve_online_title, "message_popup_dialog_title_style3", Resources.reserve_online_text, "message_popup_dialog_mesg_style3", Resources.back_button, "font_museo_slab_900_20", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.got_it_button, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ReserveOnlineRentDialog", null, "White", new Thickness(180.0, 192.0, 180.0, 192.0), "ReserveOnlineRentDialog"));
	}

	public static void ShowMachineFullWarning(Action timeOutAction, Action continueAction)
	{
		OrchestrationService.SetCurrentUICulture();
		LogHelper.Instance.Log("Showing machine full warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.machine_full_title, null, Resources.machine_full_message, null, null, null, null, Resources.okay_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "MachineFullWarning", null, null, default(Thickness), "machineFullWarning"));
	}

	public static void HideMessagePopupView(bool popOnly)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service.Peek() == "message_popup_view")
		{
			MessagePopupViewStopTimer();
			service.GoBackSafe("messageviewmanager:hide()", popOnly);
		}
	}

	public static bool IsMessagePopupViewShowing()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			return service.Peek() == "message_popup_view";
		}
		return false;
	}

	public static void MessagePopupViewStopTimer()
	{
		IActor actor = ActorFunctions.GetActor("MessagePopupViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl)
		{
			messagePopupViewUserControl.StopTimer();
		}
	}

	public static void ShowMessagePopupView(ShowMessagePopupViewParameter parameter)
	{
		OrchestrationService.ViewShow("message_popup_view", parameter);
		if (!string.IsNullOrWhiteSpace(parameter?.ActorTag))
		{
			BaseViewController.AnalyticsService?.AddEventProperty("ShowMessageActorTag", parameter.ActorTag);
		}
	}

	public static void ShowCriticalErrorView(ShowMessagePopupViewParameter parameter)
	{
		OrchestrationService.ShowCriticalErrorView("message_popup_view", parameter);
	}

	public static void LeaveMessagePopupView(IViewFrameInstance viewFrameInstance)
	{
		MessagePopupViewUserControl messagePopupViewUserControl = (MessagePopupViewUserControl)BaseViewController.GetViewUserControl(viewFrameInstance.Id);
		if (messagePopupViewUserControl != null && messagePopupViewUserControl.DataContext is MessagePopupViewModel messagePopupViewModel)
		{
			messagePopupViewModel.BackgroundOpacity = 0.0;
		}
	}

	public static void ConfigureMessagePopupView(ShowMessagePopupViewParameter parameters)
	{
		if (parameters.AnalyticsContextList != null)
		{
			foreach (string item in parameters.AnalyticsContextList.Where((string c) => !string.IsNullOrWhiteSpace(c)))
			{
				_ = item;
			}
		}
		Thickness thickness = default(Thickness);
		MessagePopupViewUserControl messagePopupViewUserControl = (MessagePopupViewUserControl)BaseViewController.GetViewUserControl();
		if (messagePopupViewUserControl != null)
		{
			ServiceLocator.Instance.GetService<IViewService>();
			MessagePopupViewModel viewModel = (MessagePopupViewModel)(messagePopupViewUserControl.DataContext = new MessagePopupViewModel
			{
				Title = parameters.TitleText,
				Message = parameters.MessageText,
				BackgroundOpacity = 0.6,
				TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.TitleStyle ?? "message_popup_dialog_title_style"),
				MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageStyle ?? "message_popup_dialog_mesg_style"),
				Button1Text = parameters.Button1Text,
				Button1TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1TextStyle ?? "button1_text_style"),
				Button1Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1Style ?? "button1_style"),
				Button1Visibility = (string.IsNullOrEmpty(parameters.Button1Text) ? Visibility.Collapsed : Visibility.Visible),
				Button2Text = parameters.Button2Text,
				Button2TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2TextStyle ?? "button2_text_style"),
				Button2Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2Style ?? "button2_style"),
				ButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.ButtonGridStyle ?? "message_popup_dialog_button_grid_style"),
				Button2Visibility = (string.IsNullOrEmpty(parameters.Button2Text) ? Visibility.Collapsed : Visibility.Visible),
				InnerGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.InnerGridStyle ?? null),
				FloatingButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.FloatingButtonGridStyle ?? "message_popup_dialog_floating_button_grid_style"),
				MessageDockPanelStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageDockPanelStyle ?? "message_popup_dialog_message_dock_panel_style"),
				TimeOut = parameters.Timeout,
				OnButton1Clicked = delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("MessagePopupButton1", parameters.Button1Text);
					parameters.Button1Action?.Invoke();
				},
				OnButton2Clicked = delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("MessagePopupButton2", parameters.Button2Text);
					parameters.Button2Action?.Invoke();
				},
				OnTimeOut = parameters.TimeOutAction,
				ContentUserControl = parameters.ContentUserControl,
				BorderBrushColor = (string.IsNullOrEmpty(parameters.BorderBrushColor) ? "#E42045" : parameters.BorderBrushColor),
				MainGridMargin = (parameters.MainGridMargin.Equals(thickness) ? new Thickness(128.0, 96.0, 128.0, 96.0) : parameters.MainGridMargin)
			});
			LogHelper.Instance.Log("Setting MessagePopupView: Title:{0} Message:{1}", parameters.TitleText, parameters.MessageText);
			ConfigureTTS(viewModel);
		}
	}

	private static void ConfigureTTS(MessagePopupViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => MessagePopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static MessagePopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MessagePopupViewModel;
		}
		return null;
	}

	private static int GetDefaultMessageTimeOut()
	{
		return GetDefaultMessageTimeOut(15000, "medium");
	}

	private static int GetDefaultMessageTimeOut(int defaultTimeout, string timeoutType)
	{
		return OrchestrationService.TTSGetTimeout(timeoutType, defaultTimeout);
	}
}
