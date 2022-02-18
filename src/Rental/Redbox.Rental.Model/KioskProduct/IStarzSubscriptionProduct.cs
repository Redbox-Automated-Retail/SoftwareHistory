using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public interface IStarzSubscriptionProduct : ISubscriptionProduct, IKioskProduct
{
	DateTime EffectiveStartDate { get; set; }

	DateTime EffectiveEndDate { get; set; }

	string BrowseOfferHeader { get; }

	string BrowseOfferDescription { get; }

	string BrowseOfferButton { get; }

	string MyBagProductTitle { get; }

	string MyBagProductDescription { get; }

	string MyBagOfferHeader { get; }

	string MyBagOfferDescription { get; }

	string MyBagOfferSubText { get; }

	string TitleText { get; }

	string TitleClause2Text { get; }

	string TitleClause3Text { get; }

	string SubtitleText { get; }

	string SubtitleAsteriskText { get; }

	string SubtitleClause2Text { get; }

	string Subtitle2Text { get; }

	string RegisteredTrademarkSymbolBullet { get; }

	string Bullet1Text1 { get; }

	string Bullet1Text2 { get; }

	string Bullet2Text { get; }

	string Bullet3Text { get; }

	string LeftTitleText { get; }

	string RightTitleText { get; }

	string LeftSubtitleText { get; }

	string RightSubtitleText { get; }

	string LeftSubtitle2Text { get; }

	string RightSubtitle2Text { get; }

	string AcceptButtonText { get; }

	string LegalText { get; }

	string DisplayQRCodeURL { get; }

	string MyBagQRCodeURL { get; }

	string OfferDetailsQRCodeURL { get; }

	string AcceptanceTerms { get; }

	bool IsFreeTrial { get; }

	bool IsPromotional { get; }

	IList<IStarzOfferPeriod> OfferPeriods { get; }

	string RegisteredTrademarkSymbol { get; }

	string RegisteredTrademarkSymbolNewCP { get; }

	string RegisteredTrademarkSymbolMessage { get; }

	string RegisteredTrademarkSymbolNewCPMessage { get; }

	string ConfirmationTitleText1 { get; }

	string ConfirmationTitleText2 { get; }

	string ConfirmationMessageText1 { get; }

	string ConfirmationMessageText2 { get; }

	string ConfirmationNewCPTitleText1 { get; }

	string ConfirmationNewCPTitleText2 { get; }

	string ConfirmationNewCPMessageText1 { get; }

	string ConfirmationNewCPMessageText2 { get; }

	string ConfirmationNewCPSubMessageText { get; }
}
