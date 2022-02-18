using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController : BaseViewController
{
	private class GenericErrorMessageParameters
	{
		public string CultureSpecificTitle { get; set; }

		public string CultureSpecificMessage { get; set; }

		public string CultureSpecificButtonText { get; set; }

		public string InnerGridStyle { get; set; }

		public int? TimeOut { get; set; }

		public Action ContinueAction { get; set; }

		public Action TimeOutAction { get; set; }

		public string ActorTag { get; set; }

		public UserControl ContentUserControl { get; set; }

		public List<string> Context { get; set; }
	}

	private class GenericDialogParameters
	{
		public string CultureSpecificTitle { get; set; }

		public string CultureSpecificMessage { get; set; }

		public string CultureSpecificCancelButtonText { get; set; }

		public string CultureSpecificContinueButtonText { get; set; }

		public Action TimeOutAction { get; set; }

		public Action BackAction { get; set; }

		public Action ContinueAction { get; set; }

		public string ViewName { get; set; }

		public int? Timeout { get; set; }

		public List<string> Context { get; set; }
	}

	public static void ReenableTimer()
	{
		((MessagePopupViewUserControl)BaseViewController.GetViewUserControl())?.ReenableTimer();
	}

	public static void ShowRecommendationOnPickupPrompt(Action timeOutAction, Action pickupAction, Action continueBrowsingAction)
	{
		LogHelper.Instance.Log("Showing Recommendation On Pickup prompt");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.reco_need_more_time, (string)null, (string)null, (string)null, Resources.pickup, (string)null, (string)null, $"{Resources.keep_browsing_1} {Resources.keep_browsing_2}", "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), pickupAction, continueBrowsingAction, timeOutAction, "RecommendationOnPickupPrompt", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
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

	private static void ShowGenericErrorMessage(GenericErrorMessageParameters genericErrorMessageParameters)
	{
		LogHelper.Instance.Log("Showing Generic Error Message: {0}", genericErrorMessageParameters?.CultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(genericErrorMessageParameters?.CultureSpecificTitle, "message_popup_dialog_title_generic_error_style", genericErrorMessageParameters?.CultureSpecificMessage, "message_popup_dialog_message_generic_error_style", null, null, null, genericErrorMessageParameters?.CultureSpecificButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style6", "message_popup_dialog_button_grid_generic_error_style", (genericErrorMessageParameters != null && genericErrorMessageParameters.InnerGridStyle != null) ? genericErrorMessageParameters.InnerGridStyle : "message_popup_dialog_inner_grid_medium_generic_error_style", null, "message_popup_dialog_message_dock_panel_generic_error_style", (genericErrorMessageParameters != null && genericErrorMessageParameters.TimeOut.HasValue) ? genericErrorMessageParameters.TimeOut.Value : GetDefaultMessageTimeOut(30000, "long"), null, genericErrorMessageParameters?.ContinueAction, genericErrorMessageParameters?.TimeOutAction, (genericErrorMessageParameters != null && genericErrorMessageParameters.ActorTag != null) ? genericErrorMessageParameters.ActorTag : "GenericErrorMessage", genericErrorMessageParameters?.ContentUserControl, "Transparent", new Thickness(0.0, 0.0, 0.0, 0.0), (genericErrorMessageParameters?.Context != null) ? genericErrorMessageParameters.Context : new List<string> { "GenericErrorMessage" }, null, Visibility.Visible, isAutoSized: true));
	}

	private static void ShowGenericErrorMessageWithQRCode(GenericErrorMessageParameters genericErrorMessageParameters)
	{
		LogHelper.Instance.Log("Showing Generic Error Message with QR Code: {0}", genericErrorMessageParameters?.CultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(genericErrorMessageParameters?.CultureSpecificTitle, "message_popup_dialog_title_generic_error_style", genericErrorMessageParameters?.CultureSpecificMessage, "message_popup_dialog_message_generic_error_style2", null, null, null, genericErrorMessageParameters?.CultureSpecificButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style6", "message_popup_dialog_button_grid_generic_error_style2", (genericErrorMessageParameters != null && genericErrorMessageParameters.InnerGridStyle != null) ? genericErrorMessageParameters.InnerGridStyle : "message_popup_dialog_inner_grid_large_generic_error_style", null, "message_popup_dialog_message_dock_panel_generic_error_style", (genericErrorMessageParameters != null && genericErrorMessageParameters.TimeOut.HasValue) ? genericErrorMessageParameters.TimeOut.Value : GetDefaultMessageTimeOut(30000, "long"), null, genericErrorMessageParameters?.ContinueAction, genericErrorMessageParameters?.TimeOutAction, (genericErrorMessageParameters != null && genericErrorMessageParameters.ActorTag != null) ? genericErrorMessageParameters.ActorTag : "GenericErrorMessageWithQRCode", genericErrorMessageParameters?.ContentUserControl, "Transparent", new Thickness(0.0, 0.0, 0.0, 0.0), (genericErrorMessageParameters?.Context != null) ? genericErrorMessageParameters.Context : new List<string> { "GenericErrorMessageWithQRCode" }, null, Visibility.Collapsed, isAutoSized: true));
	}

	public static void ShowStarzExistingAccountErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.starz_signup_existing_account_error_title,
			CultureSpecificMessage = Resources.starz_signup_existing_account_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Starz signup error existing Redbox Account" }
		});
	}

	public static void ShowStarzGenericErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.starz_signup_generic_error_title,
			CultureSpecificMessage = Resources.starz_signup_generic_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Starz signup generic error" }
		});
	}

	public static void ShowStarzExistingSubscriptionErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.starz_signup_existing_subscription_error_title,
			CultureSpecificMessage = Resources.starz_signup_existing_subscription_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Starz signup existing subscription error" }
		});
	}

	public static void ShowStarzEligibilityErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.starz_signup_eligibility_error_title,
			CultureSpecificMessage = Resources.starz_signup_eligibility_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Starz signup eligibility error" }
		});
	}

	public static void ShowStarzAuthorizeErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.auth_error_starz_auth_declined_title,
			CultureSpecificMessage = Resources.auth_error_starz_auth_declined_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Starz signup authorize error" }
		});
	}

	public static void ShowAuthErrorMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction, List<string> analyticsContexts = null)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = cultureSpecificTitle,
			CultureSpecificMessage = cultureSpecificMessage,
			CultureSpecificButtonText = cultureSpecificButtonText,
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			Context = analyticsContexts
		});
	}

	public static void ShowLogOutWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Log out warning message");
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.sign_out_dialog_title,
			CultureSpecificMessage = Resources.sign_out_dialog_message,
			CultureSpecificCancelButtonText = Resources.back_button,
			CultureSpecificContinueButtonText = Resources.sign_out_button,
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "SignOutWarningMessage",
			Timeout = GetDefaultMessageTimeOut(30000, "long")
		});
	}

	public static void ShowSimpleOkMessage(string cultureSpecificTitle, string cultureSpecificMessage, Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Ok Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "message_popup_dialog_title_rbpurple_style", cultureSpecificMessage, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, Resources.okay, "font_museo_slab_900_28", "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "OkMessage", (UserControl)null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowLoyaltyIsDownOnLoginMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down on login message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.personalization_service_error_KLI0005, "message_popup_dialog_title_rbpurple_style", (string)null, (string)null, (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "LoyaltyIsDownMessage", (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowMissingRedboxPlusRentalItemMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing missing redbox+ rental item message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_missing_item_error_title,
			CultureSpecificMessage = Resources.redbox_plus_missing_item_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction
		});
	}

	public static void ShowRedboxPlusCartFullMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ cart full message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_cart_full_message_title,
			CultureSpecificMessage = Resources.redbox_plus_cart_full_message_body,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier1Message(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental already used tier1 message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_already_used_tier1_header,
			CultureSpecificMessage = Resources.redbox_plus_already_used_tier1_message,
			CultureSpecificButtonText = Resources.redbox_plus_already_used_close_button_text,
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction,
			InnerGridStyle = "message_popup_dialog_inner_grid_large_generic_error_style"
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier2Message(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental already used tier2 message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_already_used_tier2_header,
			CultureSpecificMessage = Resources.redbox_plus_already_used_tier2_message,
			CultureSpecificButtonText = Resources.redbox_plus_already_used_close_button_text,
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalApplied(Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental appllied message");
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_added_free_1_night_rental_title,
			CultureSpecificContinueButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction
		});
	}

	public static void ShowRedboxPlusPerksPointErrorMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ cart full message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_perks_points_redemption_error_title,
			CultureSpecificMessage = Resources.redbox_plus_perks_points_redemption_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction
		});
	}

	public static void ShowRedboxPlusMultiNightPriceErrorMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ multi-night price error message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_multi_night_error_title,
			CultureSpecificMessage = Resources.redbox_plus_multi_night_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction
		});
	}

	public static void ShowRedboxPlusGenericErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_signup_generic_error_title,
			CultureSpecificMessage = Resources.redbox_plus_signup_generic_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Redbox+ signup generic error" }
		});
	}

	public static void ShowRedboxPlusExistingSubscriptionErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_signup_existing_subscription_error_title,
			CultureSpecificMessage = Resources.redbox_plus_signup_existing_subscription_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Redbox+ signup existing subscription error" }
		});
	}

	public static void ShowRedboxPlusExistingAccountErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.redbox_plus_signup_existing_account_error_title,
			CultureSpecificMessage = Resources.redbox_plus_signup_existing_account_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Redbox+ signup error existing Redbox Account" }
		});
	}

	public static void ShowOfferTimeoutMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing offer dialog timeout message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.offer_timeout_title, "message_popup_dialog_title_black_style", Resources.offer_timeout_message, "font_montserrat_small_black", Resources.no_thanks_button_text, "font_museo_slab_900_28", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.yes_button_mixed_case, "font_museo_slab_900_28", "message_popup_warmpurple_button_style", "message_popup_dialog_button_grid_style", "message_popup_dialog_inner_grid_style5", (string)null, (string)null, GetDefaultMessageTimeOut(30000, "short"), backAction, continueAction, timeOutAction, "OfferTimeoutMessage", (UserControl)null, "White", new Thickness(128.0, 96.0, 128.0, 96.0), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowSwapPromoForRedboxPlusBenefitMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing swap promo for Redbox+ free one night rental message");
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.swap_promo_with_redbox_plus_benefit,
			CultureSpecificContinueButtonText = Resources.yes_button,
			CultureSpecificCancelButtonText = Resources.no_button,
			BackAction = backAction,
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction,
			Timeout = GetDefaultMessageTimeOut(30000, "short"),
			ViewName = "SwapPromoForRedboxPlusFreeOneNightRental"
		});
	}

	public static void ShowSwapRedboxPlusBenefitForPromoMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing swap Redbox+ benefit for promo message");
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.swap_redbox_plus_benefit_with_promo,
			CultureSpecificContinueButtonText = Resources.yes_button,
			CultureSpecificCancelButtonText = Resources.no_button,
			BackAction = backAction,
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction,
			Timeout = GetDefaultMessageTimeOut(30000, "short"),
			ViewName = "SwapRedboxPlusBenefitForPromo"
		});
	}

	public static void ShowRedboxPlusAuthorizeErrorMessage(Action continueAction)
	{
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.auth_error_redbox_plus_auth_declined_title,
			CultureSpecificMessage = Resources.auth_error_redbox_plus_auth_declined_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = continueAction,
			Context = new List<string> { "Redbox+ signup authorize error" }
		});
	}

	public static void ShowLoyaltyIsDownMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.loyalty_down_dialog_title, "message_popup_dialog_title_rbpurple_style", Resources.loyalty_down_dialog_text, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "LoyaltyIsDownMessage", (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowInCartOfferMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer message");
		InCartOfferMessageText inCartOfferMessageText = PromotionService.Instance.GetInCartOfferMessageText();
		ShowMessagePopupView(new ShowMessagePopupViewParameter(inCartOfferMessageText.Header, "message_popup_dialog_title_style2", inCartOfferMessageText.Message, (string)null, (PromotionService.Instance.IsFreeInCartOfferRunning || (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.ValidateRentTwoInCartPromo().IsCartValid)) ? null : inCartOfferMessageText.NoButtonText, (string)null, (string)null, inCartOfferMessageText.YesButtonText, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "InCartOfferMessage", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowGiftCardNotEnabledMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing gift card not enabled message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.gc_not_available_title, "message_popup_dialog_title_style2", Resources.gc_not_available, (string)null, (string)null, (string)null, (string)null, Resources.continue_button, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "GiftCardNotEnabledMessage", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, (string)null, Resources.do_not_use_promo_button, "font_target_small_medium", (string)null, Resources.use_promo_button, "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing title level promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, null, null, null, null, Resources.continue_button, null, null, null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "TitleLevelPromoCodeInvalidWarning", null, null, default(Thickness), "titlelevelpromoWarning"));
	}

	public static void ShowFirstNightFreePromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing first night free promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", message, (string)null, Resources.do_not_use_promo_button, "font_target_small_medium", (string)null, Resources.use_promo_button, "font_target_small_medium", (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "FirstNightFreePromoCodeInvalidWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
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
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, Resources.continue_button, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "PromoCodeInvalidForCartItemsWarning", (UserControl)null, (string)null, default(Thickness), (string)null, (string)null, Visibility.Collapsed));
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

	public static void ShowPromoCodeDiscountRestrictionWarning(string title, string continueButtonText, Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code discount restriction warning message");
		ShowGenericDialogMessage3(new GenericDialogParameters
		{
			CultureSpecificTitle = title,
			CultureSpecificMessage = Resources.promo_code_discount_restriction_warning_message,
			CultureSpecificCancelButtonText = Resources.no_thanks_button.ToUpper(),
			CultureSpecificContinueButtonText = continueButtonText,
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "PromoCodeDiscountRestrictionWarningPopupView",
			Context = new List<string> { "PromoCodeDiscountRestrictionWarningPopupView" }
		});
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
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, Resources.back_uppercase_button, Resources.okay_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "SubPlatformWarning", "subplatformWarning"));
	}

	public static void ShowADAPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ADA Purchase Warning message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(ServiceLocator.Instance.GetService<IMacroService>()["ADAPurchaseWarning:Header"], "message_popup_dialog_title_style2", (string)null, (string)null, Resources.back_uppercase_button, Resources.okay_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ADAPurchaseWarning", (string)null));
	}

	public static void ShowPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["PurchaseWarning:Header"];
		string messageText = service["PurchaseWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, Resources.back_uppercase_button, Resources.okay_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PurchaseWarning", "purchaseWarning"));
	}

	public static void ShowMatureWarning(Action timeOutAction, Action backAction, Action continueAction, List<string> analyticsContexts)
	{
		LogHelper.Instance.Log("Showing Mature Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["MatureWarning:Header"];
		string messageText = service["MatureWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, "message_popup_dialog_title_style2", messageText, null, Resources.back_uppercase_button, Resources.okay_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning", analyticsContexts));
	}

	public static void Show4KAlert(Action timeOutAction, Action backAction, Action continueAction, string analyticsContext)
	{
		LogHelper.Instance.Log("Showing 4K UHD Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.four_k_alert_title, "message_popup_dialog_title_style3", Resources.four_k_alert, "message_popup_dialog_mesg_style3", Resources.cancel_button, "font_museo_slab_900_20", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.got_it_button, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "4KUHDAlert", null, "White", new Thickness(180.0, 192.0, 180.0, 192.0), analyticsContext, "4K_warning_popup_view"));
	}

	private static void ShowGenericDialogMessage(GenericDialogParameters genericDialogParameters)
	{
		LogHelper.Instance.Log($"Showing Generic dialog message {genericDialogParameters?.CultureSpecificTitle}");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(genericDialogParameters?.CultureSpecificTitle, "message_popup_dialog_title_generic_dialog_style", genericDialogParameters?.CultureSpecificMessage, "message_popup_dialog_message_generic_error_style", genericDialogParameters?.CultureSpecificCancelButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style6", genericDialogParameters?.CultureSpecificContinueButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style6", "message_popup_dialog_button_grid_generic_error_style", "message_popup_dialog_inner_grid_medium_generic_error_style", null, "message_popup_dialog_message_dock_panel_generic_error_style", (genericDialogParameters != null && genericDialogParameters.Timeout.HasValue) ? genericDialogParameters.Timeout.Value : GetDefaultMessageTimeOut(), genericDialogParameters?.BackAction, genericDialogParameters?.ContinueAction, genericDialogParameters?.TimeOutAction, "GenericDialogMessage", null, "White", default(Thickness), genericDialogParameters?.Context, genericDialogParameters?.ViewName, Visibility.Visible, isAutoSized: true));
	}

	private static void ShowGenericDialogMessage2(GenericDialogParameters genericDialogParameters)
	{
		LogHelper.Instance.Log($"Showing Generic dialog message 2 {genericDialogParameters?.CultureSpecificTitle}");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(genericDialogParameters?.CultureSpecificTitle, "message_popup_dialog_title_generic_dialog_style", genericDialogParameters?.CultureSpecificMessage, "message_popup_dialog_message_generic_error_style", genericDialogParameters?.CultureSpecificCancelButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style8", genericDialogParameters?.CultureSpecificContinueButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style9", "message_popup_dialog_button_grid_generic_error_style", "message_popup_dialog_inner_grid_medium_generic_error_style2", null, "message_popup_dialog_message_dock_panel_generic_error_style2", (genericDialogParameters != null && genericDialogParameters.Timeout.HasValue) ? genericDialogParameters.Timeout.Value : GetDefaultMessageTimeOut(), genericDialogParameters?.BackAction, genericDialogParameters?.ContinueAction, genericDialogParameters?.TimeOutAction, "GenericDialogMessage2", null, "Transparent", default(Thickness), genericDialogParameters?.Context, genericDialogParameters?.ViewName, Visibility.Visible, isAutoSized: true));
	}

	private static void ShowGenericDialogMessage3(GenericDialogParameters genericDialogParameters)
	{
		LogHelper.Instance.Log($"Showing Generic dialog message 3 {genericDialogParameters?.CultureSpecificTitle}");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(genericDialogParameters?.CultureSpecificTitle, "message_popup_dialog_title_generic_dialog_style", genericDialogParameters?.CultureSpecificMessage, "message_popup_dialog_message_generic_error_style", genericDialogParameters?.CultureSpecificCancelButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style10", genericDialogParameters?.CultureSpecificContinueButtonText, "font_montserrat_extrabold_20", "message_popup_dialog_button_style11", "message_popup_dialog_button_grid_generic_error_style", "message_popup_dialog_inner_grid_medium_generic_error_style3", null, "message_popup_dialog_message_dock_panel_generic_error_style", (genericDialogParameters != null && genericDialogParameters.Timeout.HasValue) ? genericDialogParameters.Timeout.Value : GetDefaultMessageTimeOut(), genericDialogParameters?.BackAction, genericDialogParameters?.ContinueAction, genericDialogParameters?.TimeOutAction, "GenericDialogMessage3", null, "Transparent", default(Thickness), genericDialogParameters?.Context, genericDialogParameters?.ViewName, Visibility.Visible, isAutoSized: true));
	}

	public static void ShowItemInCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowGenericDialogMessage2(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.item_in_cart_quit_popup_message,
			CultureSpecificMessage = null,
			CultureSpecificCancelButtonText = Resources.keep_browsing_button.ToUpper(),
			CultureSpecificContinueButtonText = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "ItemInCartQuitMessagePopupView",
			Context = new List<string> { "ItemInCartQuitMessagePopupView" }
		});
	}

	public static void ShowSignedInEmptyCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowGenericDialogMessage2(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.signed_in_empty_cart_quit_popup_message,
			CultureSpecificMessage = null,
			CultureSpecificCancelButtonText = Resources.keep_browsing_button.ToUpper(),
			CultureSpecificContinueButtonText = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "SignedInEmptyCartQuitMessagePopupView",
			Context = new List<string> { "SignedInEmptyCartQuitMessagePopupView" }
		});
	}

	public static void ShowSignedInItemInCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowGenericDialogMessage2(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.signed_in_item_in_cart_quit_popup_message,
			CultureSpecificMessage = null,
			CultureSpecificCancelButtonText = Resources.keep_browsing_button.ToUpper(),
			CultureSpecificContinueButtonText = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "SignedInItemInCartQuitMessagePopupView",
			Context = new List<string> { "SignedInItemInCartQuitMessagePopupView" }
		});
	}

	public static void ShowRemoveSubscriptionWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.subscription_removal_title,
			CultureSpecificMessage = Resources.subscription_removal_message,
			CultureSpecificCancelButtonText = Resources.cancel_button.ToUpper(),
			CultureSpecificContinueButtonText = Resources.remove_button,
			TimeOutAction = timeOutAction,
			BackAction = backAction,
			ContinueAction = continueAction,
			ViewName = "remove_subscription_popup_view",
			Context = new List<string> { "RemoveSubscription" }
		});
	}

	public static void ShowMultiDiscAlert(string title, string message, string buttonText, Action timeOutAction, Action continueAction, string analyticsContext)
	{
		LogHelper.Instance.Log("Showing Multi Disc Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, "message_popup_dialog_title_style3", message, "message_popup_dialog_mesg_style3", null, null, null, buttonText, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", null, null, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "MultiDiscAlert", null, "White", new Thickness(185.0, 144.0, 185.0, 124.0), analyticsContext, "multi_disc_warning_popup_view"));
	}

	public static void ShowDigitalCodeAlert(Action backAction, Action okAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.digital_code_alert_title, Resources.digital_code_alert_message, Resources.back_uppercase_button, Resources.okay_button, GetDefaultMessageTimeOut(), backAction, okAction, timeOutAction, "DigitalCodeAlert", "digitalcodeincart_message_view"));
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.title_thanks,
			CultureSpecificMessage = Resources.inventory_updated_message,
			TimeOut = GetDefaultMessageTimeOut(5000, "short"),
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReturnSuccess"
		});
	}

	public static void ShowMessageCartFull(Action timeoutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Cart Full message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.cart_full_header, Resources.cart_full_body, (string)null, Resources.continue_button, OrchestrationService.GetIdleTimerTimeout(), (Action)null, continueAction, timeoutAction, "CartFull", (string)null));
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Hardware Error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter((string)null, Resources.hardware_error_message, (string)null, Resources.continue_button, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewHardwareError", (string)null));
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Backward Inserted message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.return_error, Resources.backward_insert_error, (string)null, Resources.continue_button, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnBackwardInserted", (string)null));
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Unrecognized Barcode message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.oops, Resources.unrecognized_barcode_returned_message, (string)null, Resources.continue_button, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnUnrecognizedBarcode", (string)null));
	}

	public static void ShowMessagePopupViewCardNotRecognized(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.invalid_card_title,
			CultureSpecificMessage = Resources.invalid_card_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewCardNotRecognized",
			Context = new List<string> { "cardNotRecognized" }
		});
	}

	public static void ShowMessagePopupViewCheckoutCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Tampered card reader message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.card_reader_tampered_message_title,
			CultureSpecificMessage = Resources.card_reader_tampered_checkout_flow_message_body,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			Context = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewReservationCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Tampered card reader message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.card_reader_tampered_message_title,
			CultureSpecificMessage = Resources.card_reader_tampered_reservation_flow_message_body,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			Context = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewSwipedCardIsChipEnabled(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Swiped Card is chip enabled message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.swiped_card_is_chip_enabled_title,
			CultureSpecificMessage = Resources.swiped_card_is_chip_enabled_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			Context = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewBadChipReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad chip read error message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.bad_chip_read_title,
			CultureSpecificMessage = Resources.bad_chip_read_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewBadChipReadError",
			Context = new List<string> { "badChipReadError" }
		});
	}

	public static void ShowMessagePopupViewBadContactlessReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad contactless read error message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.bad_contactless_read_title,
			CultureSpecificMessage = Resources.bad_contactless_read_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewBadContactlessReadError",
			Context = new List<string> { "badContactlessReadError" }
		});
	}

	public static void ShowMessagePopupViewReservationMobileWalletError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Reservation Mobile Wallet error message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.reservation_mobile_device_error_title,
			CultureSpecificMessage = Resources.reservation_mobile_device_error_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReservationMobileWalletError",
			Context = new List<string> { "reservationMobileWalletError" }
		});
	}

	public static void ShowMessagePopupViewCardNotRecognizedTechnicalFallback(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized Technical Fallback message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.technical_fallback_title,
			CultureSpecificMessage = Resources.technical_fallback_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewCardNotRecognizedTechnicalFallback",
			Context = new List<string> { "cardNotRecognizedTechnicalFallback" }
		});
	}

	public static void ShowMessagePopupViewGoogleWorkaroundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Google Workaround error message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.google_workaround_error_title,
			CultureSpecificMessage = Resources.google_workaround_error_message,
			CultureSpecificButtonText = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewGoogleWorkaroundError",
			Context = new List<string> { "googleWorkaroundError" }
		});
	}

	public static void ShowMessagePopupViewGoogleNoPassFoundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Google No Pass Found error message popup");
		BitmapImage qRCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.mobile_pass_login_qr_code_url, returnDefaultOnError: true);
		ShowGenericErrorMessageWithQRCode(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.mobile_pass_login_google_no_pass_found_error_header,
			CultureSpecificMessage = Resources.mobile_pass_login_google_no_pass_found_error_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewGoogleNoPassFoundError",
			ContentUserControl = new QRCodeDisplayUserControl
			{
				DataContext = new
				{
					MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "message_popup_qr_code_message_style2"),
					MessageText = string.Empty,
					QRCodeImage = qRCodeImage
				}
			},
			Context = new List<string> { "googleNoPassFoundError" }
		});
	}

	public static void ShowMessagePopupViewMobilePaymentCardNoPassFoundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Mobile Payment Card No Pass Found error message popup");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = Resources.mobile_payment_card_no_pass_found_error_title,
			CultureSpecificMessage = Resources.mobile_payment_card_no_pass_found_error_message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewMobilePaymentCardNoPassFoundError",
			Context = new List<string> { "mobilePaymentCardNoPassFoundError" }
		});
	}

	public static void ShowPerksPassLoginErrorMessage(string title, string message, Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Perks Pass login error message");
		ShowGenericErrorMessage(new GenericErrorMessageParameters
		{
			CultureSpecificTitle = title,
			CultureSpecificMessage = message,
			CultureSpecificButtonText = Resources.got_it_button.ToUpper(),
			ContinueAction = continueAction,
			TimeOutAction = timeOutAction,
			Context = new List<string> { "PerksPassLoginErrorMessage" },
			InnerGridStyle = "message_popup_dialog_inner_grid_large_generic_error_style"
		});
	}

	public static void ShowPerksPassLoginDialogMessageAndQRCode(string title, string message, Action continueAction, Action timeOutAction, BitmapImage qrCodeImage)
	{
		LogHelper.Instance.Log("Showing Perks Pass login dialog message and QR code");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, "message_popup_dialog_title_generic_dialog_style", message, "message_popup_dialog_message_generic_error_style2", null, null, null, Resources.got_it_button.ToUpper(), "font_montserrat_extrabold_20", "message_popup_dialog_button_style6", "message_popup_dialog_button_grid_generic_error_style2", "message_popup_dialog_inner_grid_large_generic_error_style", null, "message_popup_dialog_message_dock_panel_generic_error_style", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "PerksPassLoginDialogMessageAndQACode", new QRCodeDisplayUserControl
		{
			DataContext = new
			{
				MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "message_popup_qr_code_message_style2"),
				MessageText = string.Empty,
				QRCodeImage = qrCodeImage
			}
		}, "White", new Thickness(0.0, 0.0, 0.0, 0.0), new List<string> { "PerksPassLoginDialogMessageAndQRCode" }, null, Visibility.Collapsed, isAutoSized: true));
	}

	public static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Vend Error message popup");
		ShowCriticalErrorView(new ShowMessagePopupViewParameter(Resources.vend_error_title, "message_popup_dialog_title_style2", Resources.vend_error_message, "message_popup_dialog_mesg_style2", (string)null, Resources.continue_button, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewVendError", (string)null));
	}

	public static void ShowMessagePopupViewDigitalCode(string price, Action backAction, Action buyAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.digital_code_popup_title, null, Resources.digital_code_popup_message, null, Resources.back_uppercase_button, "font_target_smaller_overlays", null, string.Format(Resources.digital_code_popup_buy_button, price), "font_black_target_smaller_overlays", "classic_wide_yellow_button_style", null, null, null, null, GetDefaultMessageTimeOut(), backAction, buyAction, timeOutAction, "ViewDigitalCode", new DigitalCodePopupUserControl
		{
			DataContext = new
			{
				MessageLine1 = Resources.digital_code_popup_message_line_1,
				MessageLine2 = Resources.digital_code_popup_message_line_2,
				MessageLine3 = Resources.digital_code_popup_message_line_3
			}
		}, null, default(Thickness), "whatdigitalcode_message_view"));
	}

	public static void ShowQRCodeDialog(BitmapImage qrCodeImage, string url, Action backAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing QR Code message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.qr_code_dialog_title, "message_popup_dialog_title_style6", null, null, Resources.done_button, "font_montserrat_extrabold_20", "message_popup_dialog_button_style7", null, null, null, null, null, null, "message_popup_dialog_message_dock_panel_style", GetDefaultMessageTimeOut(), backAction, null, timeOutAction, "ViewQRCodeDialog", new QRCodeDisplayUserControl
		{
			DataContext = new
			{
				MessageText = string.Format(Resources.qr_code_dialog_message, url),
				QRCodeImage = qrCodeImage
			}
		}, "White", new Thickness(176.0, 92.0, 176.0, 116.0), "qr_code_dialog_view"));
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

	public static void ShowBrowseMessagePopupLearnMorePopup(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Browse Message Popup Learn More Popup");
		ShowGenericDialogMessage(new GenericDialogParameters
		{
			CultureSpecificTitle = Resources.browse_message_popup_learn_more_popup_title,
			CultureSpecificMessage = Resources.browse_message_popup_learn_more_popup_message,
			CultureSpecificContinueButtonText = Resources.close_button.ToUpper(),
			TimeOutAction = timeOutAction,
			ContinueAction = continueAction,
			ViewName = "browse_message_popup_learn_more_popup_view",
			Context = new List<string> { "browse_message_popup_learn_more_popup_view" }
		});
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
			MessagePopupViewModel messagePopupViewModel = (MessagePopupViewModel)(messagePopupViewUserControl.DataContext = new MessagePopupViewModel
			{
				Title = parameters.TitleText,
				Message = parameters.MessageText,
				BackgroundOpacity = 0.6,
				TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.TitleStyle ?? "message_popup_dialog_title_style"),
				MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageStyle ?? "message_popup_dialog_mesg_style"),
				Button1Text = parameters.Button1Text,
				Button1TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1TextStyle ?? "button1_text_style"),
				Button1Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1Style ?? "button1_style"),
				Button1Visibility = ((Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode || string.IsNullOrEmpty(parameters.Button1Text)) ? Visibility.Collapsed : Visibility.Visible),
				Button2Text = parameters.Button2Text,
				Button2TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2TextStyle ?? "button2_text_style"),
				Button2Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2Style ?? "button2_style"),
				ButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.ButtonGridStyle ?? "message_popup_dialog_button_grid_style"),
				Button2Visibility = ((Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode || string.IsNullOrEmpty(parameters.Button2Text)) ? Visibility.Collapsed : Visibility.Visible),
				InnerGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.InnerGridStyle ?? null),
				FloatingButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.FloatingButtonGridStyle ?? "message_popup_dialog_floating_button_grid_style"),
				MessageDockPanelStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageDockPanelStyle ?? "message_popup_dialog_message_dock_panel_style"),
				TimeOut = (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode ? 10000 : parameters.Timeout),
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
				MainGridMargin = (parameters.MainGridMargin.Equals(thickness) ? new Thickness(128.0, 96.0, 128.0, 96.0) : parameters.MainGridMargin),
				FixedSizeGridVisibility = (parameters.IsAutoSized ? Visibility.Collapsed : Visibility.Visible),
				AutoSizedGridVisibility = ((!parameters.IsAutoSized) ? Visibility.Collapsed : Visibility.Visible),
				ButtonGridSeparatorLineVisibility = parameters.ButtonGridSeparatorLineVisibility
			});
			LogHelper.Instance.Log("Setting MessagePopupView: Title:{0} Message:{1}", parameters.TitleText, parameters.MessageText);
			ConfigureTTS(messagePopupViewModel);
			if (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode)
			{
				ConfigureCountdownTimer(messagePopupViewModel);
				return;
			}
			messagePopupViewModel.CountdownTimerText = string.Empty;
			messagePopupViewModel.CountdownTimerGridVisibility = Visibility.Collapsed;
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

	private static void ConfigureCountdownTimer(MessagePopupViewModel messagePopupViewModel)
	{
		if (messagePopupViewModel.TimeOut <= 0)
		{
			return;
		}
		int secondsRemaining = messagePopupViewModel.TimeOut / 1000;
		messagePopupViewModel.CountdownTimerText = secondsRemaining.ToString("0:00");
		messagePopupViewModel.CountdownTimerGridVisibility = Visibility.Visible;
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		(timerService?.CreateTimer("MessagePopupCountdownTimer", 1000, 1000, delegate
		{
			secondsRemaining--;
			messagePopupViewModel.CountdownTimerText = secondsRemaining.ToString("0:00");
			if (secondsRemaining <= 1)
			{
				(timerService?.GetTimer("MessagePopupCountdownTimer"))?.Stop();
				timerService?.RemoveTimer("MessagePopupCountdownTimer");
			}
		}))?.Start();
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
