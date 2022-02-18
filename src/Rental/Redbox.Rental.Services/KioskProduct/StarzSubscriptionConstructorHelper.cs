using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.KioskProduct;

public static class StarzSubscriptionConstructorHelper
{
	public static void PopulateHardCodedFields(StarzSubscriptionProduct starzSubscriptionProduct)
	{
		if (starzSubscriptionProduct != null)
		{
			switch (starzSubscriptionProduct.SubscriptionId)
			{
			case "STARZ_OFFER_1":
				starzSubscriptionProduct.Price = 0m;
				starzSubscriptionProduct.MyBagOfferHeader = Resources.starz_offer_1_my_bag_header;
				starzSubscriptionProduct.MyBagOfferDescription = Resources.starz_offer_1_my_bag_description;
				starzSubscriptionProduct.MyBagOfferSubText = Resources.starz_offer_1_my_bag_sub_text;
				starzSubscriptionProduct.MyBagProductTitle = Resources.starz_offer_1_my_bag_title;
				starzSubscriptionProduct.MyBagProductDescription = Resources.starz_offer_1_my_bag_title_line_2;
				starzSubscriptionProduct.AcceptanceTerms = Resources.starz_offer_1_acceptance_terms;
				starzSubscriptionProduct.DisplayQRCodeURL = Resources.starz_offer_1_display_qr_code;
				starzSubscriptionProduct.MyBagQRCodeURL = Resources.starz_offer_1_my_bag_qr_code;
				starzSubscriptionProduct.OfferDetailsQRCodeURL = Resources.starz_offer_1_offer_details_qr_code;
				starzSubscriptionProduct.ConfirmationTitleText1 = Resources.starz_offer_1_signup_confirmation_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbol = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationTitleText2 = Resources.starz_offer_1_signup_confirmation_title_2;
				starzSubscriptionProduct.ConfirmationMessageText1 = Resources.starz_offer_1_signup_confirmation_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText1 = Resources.starz_offer_1_signup_confirmation_new_customer_title;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCP = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPMessageText1 = Resources.starz_offer_1_signup_confirmation_new_customer_message_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCPMessage = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationNewCPMessageText2 = Resources.starz_offer_1_signup_confirmation_new_customer_message_2;
				starzSubscriptionProduct.ConfirmationNewCPSubMessageText = string.Empty;
				starzSubscriptionProduct.TitleText = Resources.starz_offer_1_title;
				starzSubscriptionProduct.TitleClause2Text = Resources.starz_offer_1_title_clause2;
				starzSubscriptionProduct.SubtitleText = Resources.starz_offer_1_subtitle;
				starzSubscriptionProduct.SubtitleAsteriskText = Resources.starz_offer_1_subtitle_asterisk;
				starzSubscriptionProduct.Bullet1Text1 = Resources.starz_offer_1_bullet1_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolBullet = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.Bullet1Text2 = Resources.starz_offer_1_bullet1_2;
				starzSubscriptionProduct.Bullet2Text = Resources.starz_offer_1_bullet2;
				starzSubscriptionProduct.Bullet3Text = Resources.starz_offer_1_bullet3;
				starzSubscriptionProduct.LegalText = Resources.starz_offer_1_legal;
				starzSubscriptionProduct.AcceptButtonText = Resources.starz_offer_accept_button_text_free_trial;
				break;
			case "STARZ_OFFER_2":
				starzSubscriptionProduct.Price = 5m;
				starzSubscriptionProduct.MyBagOfferHeader = Resources.starz_offer_2_my_bag_header;
				starzSubscriptionProduct.MyBagOfferDescription = Resources.starz_offer_2_my_bag_description;
				starzSubscriptionProduct.MyBagOfferSubText = Resources.starz_offer_2_my_bag_sub_text;
				starzSubscriptionProduct.MyBagProductTitle = Resources.starz_offer_2_my_bag_title;
				starzSubscriptionProduct.MyBagProductDescription = Resources.starz_offer_2_my_bag_title_line_2;
				starzSubscriptionProduct.AcceptanceTerms = Resources.starz_offer_2_acceptance_terms;
				starzSubscriptionProduct.DisplayQRCodeURL = Resources.starz_offer_2_display_qr_code;
				starzSubscriptionProduct.MyBagQRCodeURL = Resources.starz_offer_2_my_bag_qr_code;
				starzSubscriptionProduct.OfferDetailsQRCodeURL = Resources.starz_offer_2_offer_details_qr_code;
				starzSubscriptionProduct.ConfirmationTitleText1 = Resources.starz_offer_2_signup_confirmation_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbol = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationTitleText2 = Resources.starz_offer_2_signup_confirmation_title_2;
				starzSubscriptionProduct.ConfirmationMessageText1 = Resources.starz_offer_2_signup_confirmation_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText1 = Resources.starz_offer_2_signup_confirmation_new_customer_title;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCP = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPMessageText1 = Resources.starz_offer_2_signup_confirmation_new_customer_message_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCPMessage = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationNewCPMessageText2 = Resources.starz_offer_2_signup_confirmation_new_customer_message_2;
				starzSubscriptionProduct.ConfirmationNewCPSubMessageText = string.Empty;
				starzSubscriptionProduct.TitleText = Resources.starz_offer_2_title;
				starzSubscriptionProduct.TitleClause2Text = Resources.starz_offer_2_title_clause2;
				starzSubscriptionProduct.TitleClause3Text = Resources.starz_offer_2_title_clause3;
				starzSubscriptionProduct.SubtitleText = Resources.starz_offer_2_subtitle;
				starzSubscriptionProduct.SubtitleAsteriskText = Resources.starz_offer_2_subtitle_asterisk;
				starzSubscriptionProduct.SubtitleClause2Text = Resources.starz_offer_2_subtitle_clause_2;
				starzSubscriptionProduct.Bullet1Text1 = Resources.starz_offer_2_bullet1_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolBullet = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.Bullet1Text2 = Resources.starz_offer_2_bullet1_2;
				starzSubscriptionProduct.Bullet2Text = Resources.starz_offer_2_bullet2;
				starzSubscriptionProduct.Bullet3Text = Resources.starz_offer_2_bullet3;
				starzSubscriptionProduct.LegalText = Resources.starz_offer_2_legal;
				starzSubscriptionProduct.AcceptButtonText = Resources.starz_offer_accept_button_text_default;
				break;
			case "STARZ_OFFER_3":
				starzSubscriptionProduct.Price = 0m;
				starzSubscriptionProduct.MyBagOfferHeader = Resources.starz_offer_3_my_bag_header;
				starzSubscriptionProduct.MyBagOfferDescription = Resources.starz_offer_3_my_bag_description;
				starzSubscriptionProduct.MyBagOfferSubText = Resources.starz_offer_3_my_bag_sub_text;
				starzSubscriptionProduct.MyBagProductTitle = Resources.starz_offer_3_my_bag_title;
				starzSubscriptionProduct.MyBagProductDescription = Resources.starz_offer_3_my_bag_title_line_2;
				starzSubscriptionProduct.AcceptanceTerms = Resources.starz_offer_3_acceptance_terms;
				starzSubscriptionProduct.DisplayQRCodeURL = Resources.starz_offer_3_display_qr_code;
				starzSubscriptionProduct.MyBagQRCodeURL = Resources.starz_offer_3_my_bag_qr_code;
				starzSubscriptionProduct.OfferDetailsQRCodeURL = Resources.starz_offer_3_offer_details_qr_code;
				starzSubscriptionProduct.ConfirmationTitleText1 = Resources.starz_offer_3_signup_confirmation_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbol = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationTitleText2 = Resources.starz_offer_3_signup_confirmation_title_2;
				starzSubscriptionProduct.ConfirmationMessageText1 = Resources.starz_offer_3_signup_confirmation_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText1 = Resources.starz_offer_3_signup_confirmation_new_customer_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCP = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationNewCPTitleText2 = Resources.starz_offer_3_signup_confirmation_new_customer_title_2;
				starzSubscriptionProduct.ConfirmationNewCPMessageText1 = Resources.starz_offer_3_signup_confirmation_new_customer_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCPMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPSubMessageText = Resources.starz_offer_3_signup_confirmation_new_customer_sub_message;
				starzSubscriptionProduct.TitleText = Resources.starz_offer_3_title;
				starzSubscriptionProduct.TitleClause2Text = Resources.starz_offer_3_title_clause2;
				starzSubscriptionProduct.TitleClause3Text = Resources.starz_offer_3_title_clause3;
				starzSubscriptionProduct.SubtitleText = Resources.starz_offer_3_subtitle;
				starzSubscriptionProduct.SubtitleAsteriskText = Resources.starz_offer_3_subtitle_asterisk;
				starzSubscriptionProduct.SubtitleClause2Text = Resources.starz_offer_3_subtitle_clause_2;
				starzSubscriptionProduct.Subtitle2Text = Resources.starz_offer_3_subtitle2;
				starzSubscriptionProduct.Bullet1Text1 = Resources.starz_offer_3_bullet1_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolBullet = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.Bullet1Text2 = Resources.starz_offer_3_bullet1_2;
				starzSubscriptionProduct.Bullet2Text = Resources.starz_offer_3_bullet2;
				starzSubscriptionProduct.Bullet3Text = Resources.starz_offer_3_bullet3;
				starzSubscriptionProduct.LegalText = Resources.starz_offer_3_legal;
				starzSubscriptionProduct.AcceptButtonText = Resources.starz_offer_accept_button_text_free_trial;
				break;
			case "STARZ_OFFER_4":
				starzSubscriptionProduct.Price = 0m;
				starzSubscriptionProduct.BrowseOfferHeader = Resources.starz_offer_4_browse_header;
				starzSubscriptionProduct.BrowseOfferDescription = Resources.starz_offer_4_browse_description;
				starzSubscriptionProduct.BrowseOfferButton = Resources.starz_offer_4_browse_button;
				starzSubscriptionProduct.MyBagOfferHeader = Resources.starz_offer_4_my_bag_header;
				starzSubscriptionProduct.MyBagOfferDescription = Resources.starz_offer_4_my_bag_description;
				starzSubscriptionProduct.MyBagOfferSubText = Resources.starz_offer_4_my_bag_sub_text;
				starzSubscriptionProduct.MyBagProductTitle = Resources.starz_offer_4_my_bag_title;
				starzSubscriptionProduct.MyBagProductDescription = Resources.starz_offer_4_my_bag_title_line_2;
				starzSubscriptionProduct.AcceptanceTerms = Resources.starz_offer_4_acceptance_terms;
				starzSubscriptionProduct.DisplayQRCodeURL = Resources.starz_offer_4_display_qr_code;
				starzSubscriptionProduct.MyBagQRCodeURL = Resources.starz_offer_4_my_bag_qr_code;
				starzSubscriptionProduct.OfferDetailsQRCodeURL = Resources.starz_offer_4_offer_details_qr_code;
				starzSubscriptionProduct.ConfirmationTitleText1 = Resources.starz_offer_4_signup_confirmation_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbol = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationTitleText2 = Resources.starz_offer_4_signup_confirmation_title_2;
				starzSubscriptionProduct.ConfirmationMessageText1 = Resources.starz_offer_4_signup_confirmation_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPTitleText1 = Resources.starz_offer_4_signup_confirmation_new_customer_title_1;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCP = Resources.registered_trademark_symbol;
				starzSubscriptionProduct.ConfirmationNewCPTitleText2 = Resources.starz_offer_4_signup_confirmation_new_customer_title_2;
				starzSubscriptionProduct.ConfirmationNewCPMessageText1 = Resources.starz_offer_4_signup_confirmation_new_customer_message;
				starzSubscriptionProduct.RegisteredTrademarkSymbolNewCPMessage = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPMessageText2 = string.Empty;
				starzSubscriptionProduct.ConfirmationNewCPSubMessageText = Resources.starz_offer_4_signup_confirmation_new_customer_sub_message;
				starzSubscriptionProduct.LeftTitleText = Resources.starz_offer_4_left_title;
				starzSubscriptionProduct.RightTitleText = Resources.starz_offer_4_right_title;
				starzSubscriptionProduct.LeftSubtitleText = Resources.starz_offer_4_left_subtitle;
				starzSubscriptionProduct.RightSubtitleText = Resources.starz_offer_4_right_subtitle;
				starzSubscriptionProduct.LeftSubtitle2Text = Resources.starz_offer_4_left_subtitle_2;
				starzSubscriptionProduct.RightSubtitle2Text = Resources.starz_offer_4_right_subtitle_2;
				starzSubscriptionProduct.LegalText = Resources.starz_offer_4_legal;
				starzSubscriptionProduct.AcceptButtonText = Resources.starz_offer_accept_button_text_free_trial;
				break;
			}
		}
	}
}
