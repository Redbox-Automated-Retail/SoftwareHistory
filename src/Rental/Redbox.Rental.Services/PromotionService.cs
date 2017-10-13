using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services;

public class PromotionService : IPromotion
{
	public class PromotionRule
	{
		public string SatisfyCondition { get; set; }

		public RentalShoppingCartItemAction? Action { get; set; }

		public RentalShoppingCartItemSequence? Sequence { get; set; }

		public TitleType TitleType { get; set; }

		public TitleFamily TitleFamily { get; set; }

		public List<string> IncludeProducts { get; set; }

		public List<string> ExcludeProducts { get; set; }

		public List<TitleType> ProductTypes { get; set; }

		public int MinimumQuantity { get; set; }

		public int MaximumQuantity { get; set; }

		public string RedemptionRule { get; set; }

		public string RedemptionAmount { get; set; }

		public string RedemptionCode { get; set; }

		public PromotionRule(Dictionary<string, object> rule)
		{
			if (rule.TryGetValue(Constants.SatisfyCondition, out var value))
			{
				SatisfyCondition = value.ToString();
			}
			if (rule.TryGetValue(Constants.RedemptionCode, out value))
			{
				RedemptionCode = value.ToString();
			}
			if (rule.TryGetValue(Constants.RedemptionRule, out value))
			{
				RedemptionRule = value.ToString();
			}
			if (rule.TryGetValue(Constants.RedemptionAmount, out value))
			{
				RedemptionAmount = value.ToString();
			}
			if (rule.TryGetValue(Constants.ItemAction, out value) && Enum.TryParse<RentalShoppingCartItemAction>(value.ToString(), out var result))
			{
				Action = result;
			}
			if (rule.TryGetValue(Constants.AddSequence, out value) && Enum.TryParse<RentalShoppingCartItemSequence>(value.ToString(), out var result2))
			{
				Sequence = result2;
			}
			TitleType result3 = TitleType.All;
			if (rule.TryGetValue(Constants.ProductType, out value))
			{
				Enum.TryParse<TitleType>(value.ToString(), out result3);
			}
			TitleType = result3;
			TitleFamily result4 = TitleFamily.All;
			if (rule.TryGetValue(Constants.ProductFamily, out value))
			{
				Enum.TryParse<TitleFamily>(value.ToString(), out result4);
			}
			TitleFamily = result4;
			if (rule.TryGetValue(Constants.ProductIdList, out value))
			{
				string text = value?.ToString();
				if (text != null)
				{
					IncludeProducts = new List<string>(text.Split(','));
				}
			}
			if (rule.TryGetValue(Constants.ExcludeIdList, out value))
			{
				string text2 = value?.ToString();
				if (text2 != null)
				{
					ExcludeProducts = new List<string>(text2.Split(','));
				}
			}
			if (rule.TryGetValue(Constants.ProductTypeList, out value))
			{
				string text3 = value?.ToString();
				if (!string.IsNullOrEmpty(text3))
				{
					string[] array = text3.Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						if (Enum.TryParse<TitleType>(array[i], out result3))
						{
							if (ProductTypes == null)
							{
								ProductTypes = new List<TitleType>();
							}
							ProductTypes.Add(result3);
						}
					}
				}
			}
			MinimumQuantity = -1;
			if (rule.TryGetValue(Constants.MinimumQuantity, out value))
			{
				int result5 = 0;
				if (int.TryParse(value.ToString(), out result5))
				{
					MinimumQuantity = result5;
				}
			}
			MaximumQuantity = 100000;
			if (rule.TryGetValue(Constants.MaximumQuantity, out value))
			{
				int result6 = 0;
				if (int.TryParse(value.ToString(), out result6))
				{
					MaximumQuantity = result6;
				}
			}
		}
	}

	public class Constants
	{
		public static string IncludedTitles = "INCLUDED_TITLES";

		public static string ExcludedTitles = "EXCLUDED_TITLES";

		public static string Cart = "cart";

		public static string Product = "product";

		public static string ItemAction = "item_action";

		public static string AddSequence = "add_sequence";

		public static string ProductType = "product_type";

		public static string ProductFamily = "product_family";

		public static string ProductIdList = "product_id_list";

		public static string ExcludeIdList = "exclude_id_list";

		public static string ProductTypeList = "product_type_list";

		public static string MinimumQuantity = "min_qty";

		public static string MaximumQuantity = "max_qty";

		public static string RedemptionCode = "redemption_code";

		public static string RedemptionRule = "redemption_rule";

		public static string RedemptionAmount = "redemption_amount";

		public static string SatisfyCondition = "satisfy_condition";

		public static string RedemptionRuleProductType = "product_type";

		public static string RedemptionRuleFixedAmount = "fixed_amount";

		public static string RedemptionRuleQuantityBasedAmount = "qty_based_amount";

		public static string RedemptionRulePostOfferFirstNight = "post_offer_first_night";
	}

	private static IPromotion _instance;

	private Promotion _promotion;

	private List<Dictionary<string, object>> _inCartPromoRules;

	public static IPromotion Instance
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

	public bool IsFreeInCartOfferValid
	{
		get
		{
			bool result = false;
			string value = GetValue("product_family");
			if (!string.IsNullOrEmpty(value))
			{
				TitleFamily result2 = TitleFamily.All;
				Enum.TryParse<TitleFamily>(value, out result2);
				string value2 = GetValue("product_type");
				if (!string.IsNullOrEmpty(value2))
				{
					TitleType result3 = TitleType.All;
					Enum.TryParse<TitleType>(value2, out result3);
					List<string> list = new List<string>();
					string[] array = GetValue("exclude_id_list")?.Split(',') ?? null;
					if (array != null)
					{
						list.AddRange(array);
					}
					IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
					if (service.CurrentRentalShoppingCart != null)
					{
						foreach (IRentalShoppingCartItem item in service.CurrentRentalShoppingCart.Items)
						{
							if (item.Action == RentalShoppingCartItemAction.rental && !list.Contains(item.ProductId.ToString()) && item.TitleFamily == result2 && (item.TitleType == result3 || result3 == TitleType.All))
							{
								result = true;
								break;
							}
						}
					}
				}
			}
			return result;
		}
	}

	public bool IsRentTwoInCartOfferRunning => ApplicationState.Instance.InCartTemplateName == "RTWO MONEY OFF L";

	public Promotion GetPromotion()
	{
		return _promotion;
	}

	public void SetPromotion(Promotion promotion)
	{
		_promotion = promotion;
	}

	public void ClearPromotion()
	{
		_promotion = null;
	}

	public bool CartSatisfiesInCartOffer(IRentalShoppingCart rentalShoppingCart)
	{
		PromotionRule resultPromotionRule = null;
		return CartSatisfiesInCartOffer(rentalShoppingCart, out resultPromotionRule);
	}

	public bool CartSatisfiesInCartOffer(IRentalShoppingCart rentalShoppingCart, out PromotionRule resultPromotionRule)
	{
		bool flag = false;
		resultPromotionRule = null;
		if (rentalShoppingCart != null)
		{
			LogHelper.Instance.Log("CartSatisfiesInCartOffer BEGIN");
			if (_inCartPromoRules != null)
			{
				if (!IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning && rentalShoppingCart.InCartPromoOfferMade)
				{
					flag = true;
					int num = 0;
					foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
					{
						num++;
						PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
						if (!string.IsNullOrEmpty(promotionRule.SatisfyCondition))
						{
							int num2 = 0;
							if (promotionRule.SatisfyCondition == Constants.Product)
							{
								num2 = CartCountByCriteria(rentalShoppingCart, promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes);
							}
							else if (promotionRule.SatisfyCondition == Constants.Cart)
							{
								num2 = CartCountByCriteria(rentalShoppingCart, promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null);
							}
							if (num2 < promotionRule.MinimumQuantity)
							{
								LogIfVerboseLoggingIsOn($">> c:min_qty={promotionRule.MinimumQuantity} true_count={num2}");
								LogHelper.Instance.Log("> failed condition {0}", num);
								flag = false;
							}
							if (num2 > promotionRule.MaximumQuantity)
							{
								LogIfVerboseLoggingIsOn($">> c:max_qty={promotionRule.MaximumQuantity} true_count={num2}");
								LogHelper.Instance.Log("> failed condition {0}", num);
								flag = false;
							}
							LogIfVerboseLoggingIsOn($"> offer condition ({num}) {promotionRule.SatisfyCondition} returns {flag}");
						}
						if (!string.IsNullOrEmpty(promotionRule.RedemptionCode))
						{
							resultPromotionRule = promotionRule;
						}
					}
				}
			}
			else
			{
				LogIfVerboseLoggingIsOn("> In cart Promo is null");
			}
		}
		return flag;
	}

	private void LogIfVerboseLoggingIsOn(string message)
	{
		if (ApplicationState.Instance.VerboseLogging)
		{
			LogHelper.Instance.Log(message);
		}
	}

	public int CartCountByCriteria(IRentalShoppingCart rentalShoppingCart, RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<string> includeProducts, List<string> excludeProducts, List<TitleType> productTypes)
	{
		List<IRentalShoppingCartItem> matchingItems;
		return CartCountByCriteria(rentalShoppingCart, action, sequence, titleType, titleFamily, includeProducts, excludeProducts, productTypes, out matchingItems);
	}

	public int CartCountByCriteria(IRentalShoppingCart rentalShoppingCart, RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<string> includeProducts, List<string> excludeProducts, List<TitleType> productTypes, out List<IRentalShoppingCartItem> matchingItems)
	{
		int num = 0;
		LogHelper.Instance.Log("> CartCountBy a: {0}, s: {1}, pt: {2}, pf: {3}, incl: {4}, excl: {5}, ptlst: {6}", action, sequence, titleType, titleFamily, includeProducts?.Join(","), excludeProducts?.Join(","), productTypes?.Join(","));
		matchingItems = new List<IRentalShoppingCartItem>();
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			if ((!action.HasValue || item.Action == action) && (!sequence.HasValue || item.Sequence == sequence) && (titleType == TitleType.All || item.TitleType == titleType) && (titleFamily == TitleFamily.All || item.TitleFamily == titleFamily) && (includeProducts == null || includeProducts.Count == 0 || includeProducts.Contains(item.ProductId.ToString())) && (excludeProducts == null || excludeProducts.Count == 0 || !excludeProducts.Contains(item.ProductId.ToString())) && (productTypes == null || productTypes.Count == 0 || productTypes.Contains(item.TitleType)) && (!item.DiscNumber.HasValue || item.DiscNumber == 1))
			{
				num++;
				matchingItems.Add(item);
			}
		}
		LogHelper.Instance.Log("> returns {0}", num);
		return num;
	}

	public void UpdateInCartPromoValue(IRentalShoppingCart rentalShoppingCart)
	{
		if (IsFreeInCartOfferRunning || IsRentTwoInCartOfferRunning)
		{
			return;
		}
		PromotionRule resultPromotionRule = null;
		Promotion promotion = GetPromotion();
		bool flag = CartSatisfiesInCartOffer(rentalShoppingCart, out resultPromotionRule);
		if ((flag || (promotion != null && resultPromotionRule != null)) && PromoCodeMatchesInCartRedemptionCode(promotion, resultPromotionRule) && PromoCodeIsValid(promotion, flag, rentalShoppingCart))
		{
			decimal? promoPrice = null;
			ComputeProductTypePromo(resultPromotionRule, ref promoPrice);
			ComputeFixedAmountPromoPrice(resultPromotionRule, ref promoPrice);
			ComputeQuantityBasedAmountPromoPrice(rentalShoppingCart, resultPromotionRule, ref promoPrice);
			ComputePostOfferPromoPrice(rentalShoppingCart, resultPromotionRule, ref promoPrice);
			if (promoPrice.HasValue)
			{
				Promotion promotion2 = new Promotion
				{
					Name = resultPromotionRule.RedemptionCode,
					OriginalPrice = promoPrice.Value,
					Price = promoPrice.Value,
					Quantity = 1,
					Intent = PromotionIntentCode.FixedValue,
					OriginalPercent = 0m,
					Percent = 0m,
					RentQuantity = 0,
					RentFormat = null,
					GetQuantity = 0,
					GetFormat = null,
					CampaignTitles = null
				};
				SetPromotion(promotion2);
				(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(rentalShoppingCart?.ShoppingSessionId.ToString()))?.ShoppingCart?.SetDiscount(promotion2.Name, promotion2.Price);
				LogHelper.Instance.Log("UpdateInCartPromoValue: A Promocode is being used, setting session_use_credits to OptedOut");
				rentalShoppingCart.UseCredits = CreditElection.OptedOut;
			}
		}
	}

	private void ComputePostOfferPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, ref decimal? promoPrice)
	{
		if (!(promotionRule.RedemptionRule == Constants.RedemptionRulePostOfferFirstNight))
		{
			return;
		}
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		int val = CartCountByCriteria(rentalShoppingCart, promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, out var matchingItems);
		List<decimal> list = new List<decimal>();
		foreach (IRentalShoppingCartItem item in matchingItems)
		{
			IPricingRecord productTypePricingRecord = service.GetProductTypePricingRecord(item.TitleFamily, item.TitleType, forRecommendationOnPickupFlow: false);
			if (productTypePricingRecord != null)
			{
				list.Add(productTypePricingRecord.InitialNight);
			}
		}
		list.Sort();
		promoPrice = list.GetRange(0, Math.Min(val, promotionRule.MaximumQuantity)).Sum();
	}

	private void ComputeQuantityBasedAmountPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, ref decimal? promoPrice)
	{
		if (promotionRule.RedemptionRule == Constants.RedemptionRuleQuantityBasedAmount)
		{
			int num = CartCountByCriteria(rentalShoppingCart, promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes);
			LogHelper.Instance.Log("qty_based_amount = {0} * {1}", promotionRule.RedemptionAmount, num);
			if (num > promotionRule.MaximumQuantity)
			{
				LogHelper.Instance.Log("> user exceeded the max qty on a qty-based-amount in-cart, resetting discount to max allowed");
				num = promotionRule.MaximumQuantity;
			}
			decimal result = 0m;
			decimal.TryParse(promotionRule.RedemptionAmount, out result);
			promoPrice = result * (decimal)num;
		}
	}

	private void ComputeFixedAmountPromoPrice(PromotionRule promotionRule, ref decimal? promoPrice)
	{
		if (promotionRule.RedemptionRule == Constants.RedemptionRuleFixedAmount && decimal.TryParse(promotionRule.RedemptionAmount, out var result))
		{
			promoPrice = result;
		}
	}

	private void ComputeProductTypePromo(PromotionRule promotionRule, ref decimal? promoPrice)
	{
		if (!(promotionRule.RedemptionRule == Constants.RedemptionRuleProductType) || promotionRule.TitleFamily == TitleFamily.All || (promotionRule.TitleType == TitleType.All && promotionRule.TitleFamily != TitleFamily.Games))
		{
			return;
		}
		IPricingRecord productTypePricingRecord = ServiceLocator.Instance.GetService<IPricingService>().GetProductTypePricingRecord(promotionRule.TitleFamily, promotionRule.TitleType, forRecommendationOnPickupFlow: false);
		if (productTypePricingRecord == null)
		{
			return;
		}
		string redemptionAmount = promotionRule.RedemptionAmount;
		if (!(redemptionAmount == "initial_night"))
		{
			if (redemptionAmount == "extra_night")
			{
				promoPrice = productTypePricingRecord.ExtraNight;
			}
		}
		else
		{
			promoPrice = productTypePricingRecord.InitialNight;
		}
	}

	private bool PromoCodeMatchesInCartRedemptionCode(Promotion promotion, PromotionRule promotionRule)
	{
		if (promotion != null)
		{
			if (promotion != null && promotionRule != null)
			{
				return promotion.Name == promotionRule.RedemptionCode;
			}
			return false;
		}
		return true;
	}

	private bool PromoCodeIsValid(Promotion promotion, bool discountValid, IRentalShoppingCart rentalShoppingCart)
	{
		bool num = (promotion == null) | discountValid;
		if (!num)
		{
			rentalShoppingCart?.ClearPromoCode();
		}
		return num;
	}

	public int GetRentTwoInCartOfferItemCount()
	{
		int num = 0;
		List<string> list = new List<string>();
		string[] array = GetValue("exclude_id_list")?.Split(',') ?? null;
		if (array != null)
		{
			list.AddRange(array);
		}
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service.CurrentRentalShoppingCart != null)
		{
			foreach (IRentalShoppingCartItem item in service.CurrentRentalShoppingCart.Items)
			{
				if (item.Action == RentalShoppingCartItemAction.rental && !list.Contains(item.ProductId.ToString()))
				{
					num++;
				}
			}
		}
		return num;
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
