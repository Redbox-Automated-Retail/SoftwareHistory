using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class StarzSubscriptionProduct : SubscriptionProduct, IStarzSubscriptionProduct, ISubscriptionProduct, IKioskProduct
{
	public DateTime EffectiveStartDate { get; set; }

	public DateTime EffectiveEndDate { get; set; }

	public bool IsPromotional { get; set; }

	public string DisplayQRCodeURL { get; set; }

	public string MyBagQRCodeURL { get; set; }

	public string OfferDetailsQRCodeURL { get; set; }

	public IList<IStarzOfferPeriod> OfferPeriods { get; private set; }

	public string BrowseOfferHeader { get; set; }

	public string BrowseOfferDescription { get; set; }

	public string BrowseOfferButton { get; set; }

	public string MyBagProductTitle { get; set; }

	public string MyBagProductDescription { get; set; }

	public string MyBagOfferHeader { get; set; }

	public string MyBagOfferDescription { get; set; }

	public string MyBagOfferSubText { get; set; }

	public string RegisteredTrademarkSymbol { get; set; }

	public string RegisteredTrademarkSymbolNewCP { get; set; }

	public string RegisteredTrademarkSymbolMessage { get; set; }

	public string RegisteredTrademarkSymbolNewCPMessage { get; set; }

	public string ConfirmationTitleText1 { get; set; }

	public string ConfirmationTitleText2 { get; set; }

	public string ConfirmationMessageText1 { get; set; }

	public string ConfirmationMessageText2 { get; set; }

	public string ConfirmationNewCPTitleText1 { get; set; }

	public string ConfirmationNewCPTitleText2 { get; set; }

	public string ConfirmationNewCPMessageText1 { get; set; }

	public string ConfirmationNewCPMessageText2 { get; set; }

	public string ConfirmationNewCPSubMessageText { get; set; }

	public string TitleText { get; set; }

	public string TitleClause2Text { get; set; }

	public string TitleClause3Text { get; set; }

	public string LeftTitleText { get; set; }

	public string RightTitleText { get; set; }

	public string LeftSubtitleText { get; set; }

	public string RightSubtitleText { get; set; }

	public string LeftSubtitle2Text { get; set; }

	public string RightSubtitle2Text { get; set; }

	public string SubtitleText { get; set; }

	public string SubtitleAsteriskText { get; set; }

	public string SubtitleClause2Text { get; set; }

	public string Subtitle2Text { get; set; }

	public string RegisteredTrademarkSymbolBullet { get; set; }

	public string Bullet1Text1 { get; set; }

	public string Bullet1Text2 { get; set; }

	public string Bullet2Text { get; set; }

	public string Bullet3Text { get; set; }

	public string AcceptButtonText { get; set; }

	public string LegalText { get; set; }

	public string AcceptanceTerms { get; set; }

	public bool IsFreeTrial => base.Price == 0m;

	public StarzSubscriptionProduct(Store store)
		: base(store)
	{
		base.SubscriptionType = SubscriptionType.Starz;
		OfferPeriods = new List<IStarzOfferPeriod>();
	}
}
