using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Promotion;

namespace Redbox.Rental.Services;

public class PromotionService : IPromotionService
{
	public class Constants
	{
		public static string IncludedTitles = "INCLUDED_TITLES";

		public static string ExcludedTitles = "EXCLUDED_TITLES";

		public const string Store = "store";

		public const string Code = "code";

		public const string Response = "response";

		public const string Amount = "amount";

		public const string PromotionIntent = "promotion_intent";

		public const string RentQuantity = "rent_qty";

		public const string RentFormat = "rent_format";

		public const string GetQuantity = "get_qty";

		public const string GetFormat = "get_format";

		public const string CampaignTitles = "campaign_titles";

		public const string IsOnline = "is_online";

		public const string IsStandAlone = "stand_alone";

		public const string ServerName = "server_name";

		public const string GameRequired = "GameRequired";
	}

	private static IPromotionService _instance;

	private Action<IPromoCodeValidationResult> _validatePromoCodeResultCallback;

	private IPromotion _promotion;

	private List<Dictionary<string, object>> _inCartPromoRules;

	public static IPromotionService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PromotionService();
			}
			return _instance;
		}
	}

	public bool IsFreeInCartOfferRunning
	{
		get
		{
			if (!(ApplicationState.Instance.InCartTemplateName == "FREE DVD I") && !(ApplicationState.Instance.InCartTemplateName == "FREE BLU-RAY J"))
			{
				return ApplicationState.Instance.InCartTemplateName == "FREE GAME K";
			}
			return true;
		}
	}

	public bool NeedToShowFreeInCartMessage
	{
		get
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (GetPromotion() == null && IsFreeInCartOfferRunning)
			{
				IPromoCodeValidationInfo promoCodeValidationInfo = ValidateFreeInCartOffer();
				if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
				{
					return !(service?.CurrentRentalShoppingCart?.InCartPromoOfferMade ?? false);
				}
			}
			return false;
		}
	}

	public bool IsRentTwoInCartOfferRunning => ApplicationState.Instance.InCartTemplateName == "RTWO MONEY OFF L";

	public bool NeedToShowRentTwoInCartMessage
	{
		get
		{
			int num = Instance.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0;
			bool flag = num == 1 && !(RentalShoppingCart?.InCartPromoOfferMade ?? false);
			bool flag2 = num > 1;
			if (IsRentTwoInCartOfferRunning)
			{
				IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
				if ((rentalShoppingCart == null || !rentalShoppingCart.InCartDoNotShow) && GetPromotion() == null)
				{
					return flag | flag2;
				}
			}
			return false;
		}
	}

	private IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private IRentalShoppingCart RentalShoppingCart => RentalShoppingCartService?.CurrentRentalShoppingCart;

	public IPromotion GetPromotion()
	{
		return _promotion;
	}

	public void SetPromotion(IPromotion promotion)
	{
		_promotion = promotion;
	}

	public void ClearPromotion()
	{
		_promotion = null;
	}

	public void ValidatePromoCodeOnServer(string promoCode, Action<IPromoCodeValidationResult> promoCodeValidationResultCallback)
	{
		_validatePromoCodeResultCallback = promoCodeValidationResultCallback;
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		RemoteServiceCallback completeCallback = ProcessPromoCodeValidationResult;
		long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
		service?.ValidatePromotionCode(num.ToString(), promoCode, completeCallback);
	}

	private void ProcessPromoCodeValidationResult(IRemoteServiceResult result)
	{
		IPromoCodeValidationResult promoCodeValidationResult = new PromoCodeValidationResult
		{
			Success = (result?.Success ?? false)
		};
		if (result != null)
		{
			try
			{
				promoCodeValidationResult.IsStandAlone = result.Properties.ContainsKey("stand_alone") && (bool)result.Properties["stand_alone"];
				promoCodeValidationResult.IsOnline = result.Properties.ContainsKey("is_online") && (bool)result.Properties["is_online"];
				promoCodeValidationResult.ServerName = ((!result.Properties.ContainsKey("server_name")) ? null : result.Properties["server_name"]?.ToString());
				promoCodeValidationResult.StoreNumber = ((!result.Properties.ContainsKey("store")) ? null : result.Properties["store"]?.ToString());
				promoCodeValidationResult.PromoCode = ((!result.Properties.ContainsKey("code")) ? null : result.Properties["code"]?.ToString());
				promoCodeValidationResult.Response = ((!result.Properties.ContainsKey("response")) ? null : result.Properties["response"]?.ToString());
				if (result.Properties.ContainsKey("get_qty") && int.TryParse(result.Properties["get_qty"]?.ToString(), out var result2))
				{
					promoCodeValidationResult.GetQuantity = result2;
				}
				if (result.Properties.ContainsKey("rent_qty") && int.TryParse(result.Properties["rent_qty"]?.ToString(), out var result3))
				{
					promoCodeValidationResult.RentQuantity = result3;
				}
				if (result.Properties.ContainsKey("amount") && decimal.TryParse(result.Properties["amount"]?.ToString(), out var result4))
				{
					promoCodeValidationResult.Amount = result4;
				}
				if (result.Properties.ContainsKey("get_format") && Enum.TryParse<Redbox.Rental.Model.PromotionGetFormat>(result.Properties["get_format"]?.ToString(), out var result5))
				{
					promoCodeValidationResult.GetFormat = result5;
				}
				if (result.Properties.ContainsKey("rent_format") && Enum.TryParse<Redbox.Rental.Model.PromotionRentFormat>(result.Properties["rent_format"]?.ToString(), out var result6))
				{
					promoCodeValidationResult.RentFormat = result6;
				}
				if (result.Properties.ContainsKey("promotion_intent") && Enum.TryParse<Redbox.Rental.Model.PromotionIntentCode>(result.Properties["promotion_intent"]?.ToString(), out var result7))
				{
					promoCodeValidationResult.IntentCode = result7;
				}
				if (result.Properties.ContainsKey("campaign_titles"))
				{
					ICampaignTitles campaignTitles = new CampaignTitles
					{
						Titles = new List<long>()
					};
					if (result.Properties["campaign_titles"] is IDictionary<string, object> dictionary)
					{
						campaignTitles.Include = (bool)dictionary["include"];
						if (dictionary["include"] is ArrayList arrayList)
						{
							foreach (object item in arrayList)
							{
								if (long.TryParse(item?.ToString(), out var result8))
								{
									campaignTitles.Titles.Add(result8);
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Error processing promotion code validation result. {0}", e);
				promoCodeValidationResult.Response = PromotionValidationResponse.Error.ToString();
			}
		}
		if (_validatePromoCodeResultCallback != null)
		{
			_validatePromoCodeResultCallback(promoCodeValidationResult);
		}
	}

	public InCartOfferMessageText GetInCartOfferMessageText()
	{
		InCartOfferMessageText inCartOfferMessageText = new InCartOfferMessageText();
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.OfferLanguage == OrchestrationService.GetCulture() && (!IsRentTwoInCartOfferRunning || ((Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) == 1 && promotionRule.OfferMessageVersion == "1") || ((Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) > 1 && promotionRule.OfferMessageVersion == "2")))
				{
					inCartOfferMessageText.Header = promotionRule.InCartPromoHeaderMessage;
					inCartOfferMessageText.Message = promotionRule.InCartPromoBodyMessage;
					IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.DVD, useAdjustedPrices: false);
					if (pricingRecord != null && !string.IsNullOrEmpty(promotionRule.InCartPromoHeaderMessage))
					{
						inCartOfferMessageText.Message = string.Format(promotionRule.InCartPromoBodyMessage, $"{pricingRecord.ExtraNight:f2}");
					}
					inCartOfferMessageText.YesButtonText = promotionRule.InCartPromoYesButton;
					inCartOfferMessageText.NoButtonText = promotionRule.InCartPromoNoButton;
				}
			}
		}
		return inCartOfferMessageText;
	}

	public bool CartQualifiesForInCartOffer(IRentalShoppingCart rentalShoppingCart)
	{
		bool flag = false;
		if (rentalShoppingCart != null)
		{
			LogHelper.Instance.Log("CartSatisfiesInCartOffer BEGIN");
			if (_inCartPromoRules != null)
			{
				if (!IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning)
				{
					if (rentalShoppingCart.Promotion == null)
					{
						flag = true;
						bool flag2 = false;
						int num = 0;
						foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
						{
							num++;
							PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
							if (!string.IsNullOrEmpty(promotionRule.OfferCondition))
							{
								int num2 = 0;
								if (promotionRule.OfferCondition == PromotionRule.Constants.Product)
								{
									num2 = rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true)?.Count ?? 0;
								}
								else if (promotionRule.OfferCondition == PromotionRule.Constants.Cart)
								{
									num2 = rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true)?.Count ?? 0;
								}
								else if (promotionRule.OfferCondition == PromotionRule.Constants.BrowseMinimumQuantity)
								{
									num2 = BrowseCountByCriteria(rentalShoppingCart, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts);
								}
								if (num2 < promotionRule.MinimumQuantity)
								{
									LogIfVerboseLoggingIsOn(string.Format(">> {2}, min_qty={0} true_count={1}", promotionRule.MinimumQuantity, num2, promotionRule.OfferCondition));
									LogHelper.Instance.Log("> failed condition {0}", num);
									flag = false;
								}
								if (num2 > promotionRule.MaximumQuantity)
								{
									LogIfVerboseLoggingIsOn(string.Format(">> {2}, max_qty={0} true_count={1}", promotionRule.MaximumQuantity, num2, promotionRule.OfferCondition));
									LogHelper.Instance.Log("> failed condition {0}", num);
									flag = false;
								}
								LogIfVerboseLoggingIsOn($"> offer condition ({num}) {promotionRule.OfferCondition} returns {flag}");
							}
							if (promotionRule.AskOnlyOnce == true)
							{
								flag2 = true;
							}
							if (promotionRule.IncludeProducts == null || promotionRule.IncludeProducts.Count <= 0)
							{
								continue;
							}
							IDataService service = ServiceLocator.Instance.GetService<IDataService>();
							bool flag3 = true;
							if (service != null)
							{
								foreach (long includeProduct in promotionRule.IncludeProducts)
								{
									IKioskProduct kioskProduct = null;
									service.GetProduct(includeProduct, excludeTitleRollup: false, out kioskProduct);
									if (kioskProduct is ITitleProduct { IsRentableAndInStock: not false })
									{
										flag3 = false;
										break;
									}
								}
							}
							if (flag3)
							{
								LogHelper.Instance.Log(">  no titles in the offer are in stock, returning False");
								flag = false;
							}
						}
						if (flag2 && rentalShoppingCart.InCartPromoOfferMade)
						{
							LogHelper.Instance.Log(">  offer already presented, returning False");
							flag = false;
						}
					}
					else
					{
						LogHelper.Instance.Log("customer already entered a promo code, CartQualifiesForInCartOffer = false");
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("InCartPromoRules == nil at CartQualifiesForInCartOffer");
			}
		}
		return flag;
	}

	private IPromoCodeValidationInfo CartSatisfiesInCartOffer(IRentalShoppingCart rentalShoppingCart)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		if (rentalShoppingCart != null && _inCartPromoRules != null && !IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning && rentalShoppingCart.InCartPromoOfferMade)
		{
			promoCodeValidationInfo.IsCartValid = true;
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (!string.IsNullOrEmpty(promotionRule.SatisfyCondition))
				{
					if (promotionRule.SatisfyCondition == PromotionRule.Constants.Product)
					{
						promoCodeValidationInfo.QualifyingItems = rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true);
					}
					else if (promotionRule.SatisfyCondition == PromotionRule.Constants.Cart)
					{
						promoCodeValidationInfo.QualifyingItems = rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true);
					}
					if (promoCodeValidationInfo.QualifyingItems.Count() < promotionRule.MinimumQuantity)
					{
						promoCodeValidationInfo.IsCartValid = false;
					}
					if (promoCodeValidationInfo.QualifyingItems.Count() > promotionRule.MaximumQuantity)
					{
						promoCodeValidationInfo.IsCartValid = false;
					}
				}
			}
		}
		return promoCodeValidationInfo;
	}

	private PromotionRule GetRedemptionCodePromotionRule()
	{
		PromotionRule result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (!string.IsNullOrEmpty(promotionRule.RedemptionCode))
				{
					result = promotionRule;
				}
			}
		}
		return result;
	}

	private void LogIfVerboseLoggingIsOn(string message)
	{
		if (ApplicationState.Instance.VerboseLogging)
		{
			LogHelper.Instance.Log(message);
		}
	}

	public int BrowseCountByCriteria(IRentalShoppingCart rentalShoppingCart, TitleType titleType, TitleFamily titleFamily, List<long> includeProductList, List<long> excludeProductList)
	{
		int num = 0;
		if (includeProductList == null)
		{
			includeProductList = new List<long>();
		}
		if (excludeProductList == null)
		{
			excludeProductList = new List<long>();
		}
		LogHelper.Instance.Log(string.Format("> BrowseCountBy pt: {0}, pf: {1}, include list: {2}, exclude list: {3}", titleType, titleFamily, string.Join(", ", includeProductList), string.Join(", ", excludeProductList)));
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<IKioskProduct> products = null;
			service.GetProducts(BrowseViewFilter.All, titleFamily, titleType, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
			foreach (IKioskProduct item in products)
			{
				ITitleProduct titleProduct = item as ITitleProduct;
				if (titleProduct != null && titleProduct.IsRentableAndInStock && !titleProduct.IsEmptyCase && (!titleProduct.DiscNumber.HasValue || titleProduct.DiscNumber == 1) && (includeProductList.Count == 0 || includeProductList.Contains(titleProduct.ProductId)) && (excludeProductList.Count == 0 || !excludeProductList.Contains(titleProduct.ProductId)) && rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == titleProduct.ProductId && x.Action == RentalShoppingCartItemAction.rental) == null)
				{
					num++;
				}
			}
		}
		LogHelper.Instance.Log("> BrowseCountBy result: {0}", num);
		return num;
	}

	public IPromoCodeValidationInfo ValidateRegularInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		if (!IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning)
		{
			PromotionRule redemptionCodePromotionRule = GetRedemptionCodePromotionRule();
			IPromotion promotion = GetPromotion();
			promoCodeValidationInfo = CartSatisfiesInCartOffer(rentalShoppingCart);
			if (redemptionCodePromotionRule != null && promoCodeValidationInfo.IsCartValid && PromoCodeMatchesInCartRedemptionCode(promotion, redemptionCodePromotionRule))
			{
				if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleProductType)
				{
					ComputeProductTypePromo(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleFixedAmount)
				{
					ComputeFixedAmountPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleQuantityBasedAmount)
				{
					ComputeQuantityBasedAmountPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRulePostOfferFirstNight)
				{
					ComputePostOfferPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
			}
		}
		return promoCodeValidationInfo;
	}

	public void SetShoppingCartDiscount(string shoppingSessionId, IPromotion newPromotion)
	{
		(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(shoppingSessionId))?.ShoppingCart?.SetDiscount(newPromotion.Name, newPromotion.Price);
	}

	private void ComputePostOfferPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		foreach (IRentalShoppingCartItem item in (from x in rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).Take(promotionRule.MaximumQuantity).ToList())
		{
			result.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = item.PricingRecord.InitialNight
			});
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeQuantityBasedAmountPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		List<IRentalShoppingCartItem> list = (from x in rentalShoppingCart.GetItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		int num = list?.Count() ?? 0;
		LogHelper.Instance.Log("qty_based_amount = {0} * {1}", promotionRule.RedemptionAmount, num);
		if (num > promotionRule.MaximumQuantity)
		{
			LogHelper.Instance.Log("> user exceeded the max qty on a qty-based-amount in-cart, resetting discount to max allowed");
			num = promotionRule.MaximumQuantity;
		}
		decimal result2 = 0m;
		decimal.TryParse(promotionRule.RedemptionAmount, out result2);
		foreach (IRentalShoppingCartItem item in list)
		{
			if (result.PromotionItems.Count() < num)
			{
				decimal promotionAmount = Math.Min(result2, item?.PricingRecord?.InitialNight ?? 0m);
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = promotionAmount
				});
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeFixedAmountPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		if (!decimal.TryParse(promotionRule.RedemptionAmount, out var result2))
		{
			return;
		}
		List<IRentalShoppingCartItem> list = (from x in rentalShoppingCart.GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, includeMultiNightPricedItems: false, includeLoyaltyRedeemedItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal num = result2;
		foreach (IRentalShoppingCartItem item in list)
		{
			if (num > 0m)
			{
				decimal num2 = Math.Min(num, item?.PricingRecord?.InitialNight ?? 0m);
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num2
				});
				num -= num2;
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeProductTypePromo(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		if (promotionRule.TitleFamily == TitleFamily.All || (promotionRule.TitleType == TitleType.All && promotionRule.TitleFamily != TitleFamily.Games))
		{
			return;
		}
		IPricingRecord productTypePricingRecord = ServiceLocator.Instance.GetService<IPricingService>().GetProductTypePricingRecord(promotionRule.TitleFamily, promotionRule.TitleType, useAdjustedPrices: true);
		decimal num = 0m;
		if (productTypePricingRecord != null)
		{
			string redemptionAmount = promotionRule.RedemptionAmount;
			if (!(redemptionAmount == "initial_night"))
			{
				if (redemptionAmount == "extra_night")
				{
					num = productTypePricingRecord.ExtraNight;
				}
			}
			else
			{
				num = productTypePricingRecord.InitialNight;
			}
		}
		List<IRentalShoppingCartItem> list = (from x in rentalShoppingCart.GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, includeMultiNightPricedItems: false, includeLoyaltyRedeemedItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal num2 = num;
		foreach (IRentalShoppingCartItem item in list)
		{
			if (num2 > 0m)
			{
				decimal num3 = Math.Min(num2, item?.PricingRecord?.InitialNight ?? 0m);
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num3
				});
				num2 -= num3;
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private bool PromoCodeMatchesInCartRedemptionCode(IPromotion promotion, PromotionRule promotionRule)
	{
		if (promotion != null)
		{
			return promotion?.Name == promotionRule?.RedemptionCode;
		}
		return true;
	}

	public IPromotionRule GetInCartPromoActionRule()
	{
		IPromotionRule result = null;
		if (!IsFreeInCartOfferRunning && _inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				IPromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.OfferAction != null)
				{
					result = promotionRule;
					break;
				}
			}
		}
		return result;
	}

	public string GetInCartPromoCode()
	{
		string result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.RedemptionCode != null)
				{
					result = promotionRule.RedemptionCode;
					break;
				}
			}
		}
		return result;
	}

	private decimal GetRedemptionAmountFromInCartPromo()
	{
		decimal result = 0m;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.RedemptionAmount != null)
				{
					decimal result2 = 0m;
					if (decimal.TryParse(promotionRule.RedemptionAmount, out result2))
					{
						result = result2;
						return result;
					}
				}
			}
		}
		return result;
	}

	private TitleFamily GetTitleFamilyFromInCartPromo()
	{
		TitleFamily result = TitleFamily.All;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule.Keys.Contains(PromotionRule.Constants.ProductFamily))
				{
					result = new PromotionRule(inCartPromoRule).TitleFamily;
				}
			}
		}
		return result;
	}

	private TitleType GetTitleTypeFromInCartPromo()
	{
		TitleType result = TitleType.All;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule.Keys.Contains(PromotionRule.Constants.ProductType))
				{
					result = new PromotionRule(inCartPromoRule).TitleType;
				}
			}
		}
		return result;
	}

	public void SetFreeInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		SetPromoToFixedPriceInCart(rentalShoppingCart, 0m);
	}

	public void SetRentTwoInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		decimal redemptionAmountFromInCartPromo = GetRedemptionAmountFromInCartPromo();
		if (rentalShoppingCart != null)
		{
			SetPromoToFixedPriceInCart(rentalShoppingCart, redemptionAmountFromInCartPromo);
		}
	}

	public void SetPromoToFixedPriceInCart(IRentalShoppingCart rentalShoppingCart, decimal promoValue)
	{
		string inCartPromoCode = GetInCartPromoCode();
		if (inCartPromoCode != null && rentalShoppingCart != null)
		{
			IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = inCartPromoCode,
				OriginalPrice = promoValue,
				Price = promoValue,
				Quantity = 1,
				Intent = Redbox.Rental.Model.PromotionIntentCode.FixedValue,
				OriginalPercent = 0m,
				Percent = 0m,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null
			};
			SetPromotion(promotion);
			SetShoppingCartDiscount(rentalShoppingCart?.ShoppingSessionId.ToString(), promotion);
		}
	}

	public IPromoCodeValidationInfo ValidateFreeInCartOffer()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		TitleFamily titleFamilyFromInCartPromo = GetTitleFamilyFromInCartPromo();
		TitleType titleTypeFromInCartPromo = GetTitleTypeFromInCartPromo();
		List<long> excludeProducts = null;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service.CurrentRentalShoppingCart != null)
		{
			IRentalShoppingCartItem rentalShoppingCartItem = (from x in service.CurrentRentalShoppingCart.GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, titleTypeFromInCartPromo, titleFamilyFromInCartPromo, null, excludeProducts, null, includeMultiNightPricedItems: false, includeLoyaltyRedeemedItems: false)
				orderby x?.PricingRecord?.InitialNight ?? 0m, x.ProductId
				select x).FirstOrDefault();
			promoCodeValidationInfo.IsCartValid = rentalShoppingCartItem != null;
			promoCodeValidationInfo.Price = rentalShoppingCartItem?.PricingRecord?.InitialNight ?? 0m;
			if (rentalShoppingCartItem != null)
			{
				promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = rentalShoppingCartItem,
					PromotionAmount = promoCodeValidationInfo.Price
				});
				promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartItem);
			}
		}
		return promoCodeValidationInfo;
	}

	public IPromoCodeValidationInfo ValidateRentTwoInCartPromo()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<long> inCartExcludeTitleList = GetInCartExcludeTitleList();
		if (RentalShoppingCart != null)
		{
			List<IRentalShoppingCartItem> list = (from x in RentalShoppingCart.GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, TitleType.All, TitleFamily.All, null, inCartExcludeTitleList, null, includeMultiNightPricedItems: false, includeLoyaltyRedeemedItems: false)
				orderby x?.PricingRecord?.InitialNight ?? 0m, x.ProductId
				select x).ToList();
			promoCodeValidationInfo.IsCartValid = list.Count() > 1;
			decimal redemptionAmountFromInCartPromo = GetRedemptionAmountFromInCartPromo();
			foreach (IRentalShoppingCartItem item in list)
			{
				if (promoCodeValidationInfo.QualifyingItems.Count() < 2)
				{
					decimal num = Math.Min(redemptionAmountFromInCartPromo, item?.PricingRecord?.InitialNight ?? 0m);
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					redemptionAmountFromInCartPromo -= num;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	public List<long> GetInCartIncludeTitleList()
	{
		return GetInCartTitleList(Include: true);
	}

	public List<long> GetInCartExcludeTitleList()
	{
		return GetInCartTitleList(Include: false);
	}

	private List<long> GetInCartTitleList(bool Include)
	{
		List<long> list = new List<long>();
		if (_inCartPromoRules != null && !IsFreeInCartOfferRunning)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule == null)
				{
					continue;
				}
				object value = null;
				inCartPromoRule.TryGetValue("offer_action", out value);
				if (value == null)
				{
					continue;
				}
				object value2 = null;
				string key = (Include ? "product_id_list" : "exclude_id_list");
				if (!inCartPromoRule.TryGetValue(key, out value2) || value2 == null)
				{
					continue;
				}
				string[] array = value2.ToString().Split(',');
				if (array == null)
				{
					continue;
				}
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (long.TryParse(array2[i].Trim(), out var result))
					{
						list.Add(result);
					}
				}
			}
		}
		return list;
	}

	public void LoadInCartData()
	{
		List<Dictionary<string, object>> resourceTable = OrchestrationService.GetResourceTable("in_cart_config_settings");
		Dictionary<string, object> dictionary = ((resourceTable != null && resourceTable.Count > 0) ? resourceTable[0] : null);
		bool flag = false;
		if (dictionary != null)
		{
			object value = null;
			dictionary.TryGetValue("template_name", out value);
			string text = value?.ToString();
			if (text != "DEFAULT NO OFFER")
			{
				flag = true;
				LogHelper.Instance.Log("> InCart template loaded: {0}", text);
				List<Dictionary<string, object>> resourceTable2 = OrchestrationService.GetResourceTable(text);
				if (resourceTable2 != null)
				{
					object resourceProperty = OrchestrationService.GetResourceProperty(text, "variables_used");
					if (resourceProperty != null)
					{
						string[] array = resourceProperty.ToString().Split(',');
						Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
						string[] array2 = array;
						foreach (string text2 in array2)
						{
							object value2 = null;
							if (dictionary.TryGetValue(text2, out value2))
							{
								dictionary2[text2] = value2.ToString();
								LogHelper.Instance.Log("> Loaded {0} = {1}", text2, value2.ToString());
							}
						}
						LogHelper.Instance.Log("> Variable count: {0}, macro count: {1}", array.Length, dictionary2.Count);
						if (array.Length == dictionary2.Count)
						{
							foreach (KeyValuePair<string, string> item in dictionary2)
							{
								LogHelper.Instance.Log("> making replacements for: {0}", item.Key);
								string newValue = item.Value;
								if (item.Key == Constants.IncludedTitles || item.Key == Constants.ExcludedTitles)
								{
									newValue = AddMultiDiscs(dictionary2[item.Key]);
								}
								foreach (Dictionary<string, object> item2 in resourceTable2)
								{
									Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
									foreach (KeyValuePair<string, object> item3 in item2)
									{
										string text3 = item3.Value?.ToString() ?? null;
										string text4 = $"{{{{{item.Key}}}}}";
										if (text3.Contains(text4))
										{
											string text5 = text3.Replace(text4, newValue);
											LogHelper.Instance.Log("> replacing {0} with {1}", item3.Value, text5);
											dictionary3[item3.Key] = text5;
										}
									}
									foreach (KeyValuePair<string, string> item4 in dictionary3)
									{
										item2[item4.Key] = item4.Value;
									}
								}
							}
							_inCartPromoRules = resourceTable2;
							ApplicationState.Instance.InCartTemplateName = text;
							LogHelper.Instance.Log("> InCart template loaded successfully");
						}
						else
						{
							LogHelper.Instance.Log("> Not all template variables were defined in promotion.config, unable to use template");
						}
					}
					else
					{
						LogHelper.Instance.Log("> InCart template variable names not defined in resource header");
					}
				}
				else
				{
					LogHelper.Instance.Log("> InCart template resource file {0} missing or unable to load", text);
				}
			}
		}
		if (!flag)
		{
			_inCartPromoRules = OrchestrationService.GetResourceTable("in_cart_promotion_v2");
			object resourceProperty2 = OrchestrationService.GetResourceProperty("in_cart_promotion_v2", "program_name");
			ApplicationState.Instance.InCartTemplateName = resourceProperty2?.ToString();
			LogHelper.Instance.Log("InCart loaded: {0}", resourceProperty2?.ToString());
		}
	}

	private string GetValue(string key)
	{
		string result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				foreach (KeyValuePair<string, object> item in inCartPromoRule)
				{
					if (item.Key == key)
					{
						result = item.Value.ToString();
					}
				}
			}
		}
		return result;
	}

	private string AddMultiDiscs(string productIdList)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<string> list = new List<string>(productIdList.Split(','));
			foreach (string item in list)
			{
				if (!long.TryParse(item, out var result))
				{
					continue;
				}
				IKioskProduct kioskProduct = null;
				service.GetProduct(result, excludeTitleRollup: false, out kioskProduct);
				if (!(kioskProduct is MDVProduct mDVProduct))
				{
					continue;
				}
				foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
				{
					if (!productIdList.Contains(titleProduct.ProductId.ToString()))
					{
						list.Add(titleProduct.ProductId.ToString());
					}
				}
			}
			productIdList = list.Join(",");
		}
		return productIdList;
	}
}
