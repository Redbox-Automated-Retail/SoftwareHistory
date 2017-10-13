using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Pricing;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCart : IRentalShoppingCart
{
	public class PromoCodeInfo : IPromoCodeInfo
	{
		public bool IsCartValid { get; set; }

		public string Title { get; set; }

		public string Message { get; set; }
	}

	private bool _inCartTitleMarketing;

	private IBlurayUpsellInfo _blurayUpsellInfo;

	private IComingSoonOptinInfo _comingSoonOptinInfo;

	private readonly object _syncObject = new object();

	private IList<IRentalShoppingCartItem> _items = new List<IRentalShoppingCartItem>();

	private ITotals _totals;

	private CreditElection _useCredits;

	public int MaximumCapacity => 5;

	public Guid ShoppingSessionId { get; private set; }

	public IList<IRentalShoppingCartItem> Items
	{
		get
		{
			lock (_syncObject)
			{
				List<IRentalShoppingCartItem> list = new List<IRentalShoppingCartItem>();
				list.AddRange(_items);
				return list;
			}
		}
	}

	public int Count
	{
		get
		{
			lock (_syncObject)
			{
				return _items.Count;
			}
		}
	}

	public bool IsFull
	{
		get
		{
			lock (_syncObject)
			{
				return _items.Count == MaximumCapacity;
			}
		}
	}

	public IBlurayUpsellInfo BlurayUpsellInfo
	{
		get
		{
			if (_blurayUpsellInfo == null)
			{
				_blurayUpsellInfo = new BlurayUpsellInfo(this);
			}
			return _blurayUpsellInfo;
		}
	}

	public IComingSoonOptinInfo ComingSoonOptInInfo
	{
		get
		{
			if (_comingSoonOptinInfo == null)
			{
				_comingSoonOptinInfo = new ComingSoonOptinInfo();
			}
			return _comingSoonOptinInfo;
		}
	}

	public Promotion Promotion => PromotionService.Instance.GetPromotion();

	public ITotals Totals
	{
		get
		{
			if (_totals == null)
			{
				_totals = new Totals(this);
			}
			return _totals;
		}
	}

	public CreditElection UseCredits
	{
		get
		{
			return _useCredits;
		}
		set
		{
			_useCredits = value;
			ShoppingSession?.ShoppingCart?.SetUseCredits(_useCredits);
		}
	}

	private IShoppingSession ShoppingSession => ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(ShoppingSessionId.ToString());

	public bool InCartPromoOfferMade { get; set; }

	public bool InCartPromoOfferCancel { get; set; }

	public bool InCartTitleMarketing
	{
		get
		{
			return _inCartTitleMarketing;
		}
		set
		{
			_inCartTitleMarketing = value;
		}
	}

	public bool PickupRecommendationsShown { get; set; }

	public event RentalShoppingCartChanged OnChange;

	public RentalShoppingCart(Guid shoppingSessionId)
	{
		ShoppingSessionId = shoppingSessionId;
	}

	public void AddItem(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		lock (_syncObject)
		{
			if (rentalShoppingCartItem != null)
			{
				SetRentalShoppingCartItemPricing(rentalShoppingCartItem);
				rentalShoppingCartItem.Sequence = (InCartPromoOfferMade ? RentalShoppingCartItemSequence.post_offer : RentalShoppingCartItemSequence.pre_offer);
				_items.Add(rentalShoppingCartItem);
				LogHelper.Instance.Log("Adding to Rental Shopping Cart: {0} Item: {1}", ShoppingSessionId, rentalShoppingCartItem);
				Totals.UpdateSubTotalGroups();
				if (!InCartPromoOfferCancel)
				{
					PromotionService.Instance.UpdateInCartPromoValue(this);
				}
				ValidateAndUpdatePromoCode();
			}
		}
		if (OnChange != null)
		{
			OnChange(this);
		}
	}

	public void RemoveItem(IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved)
	{
		bool flag = false;
		lock (_syncObject)
		{
			if (rentalShoppingCartItem != null && _items.Contains(rentalShoppingCartItem))
			{
				_items.Remove(rentalShoppingCartItem);
				flag = true;
				Totals.UpdateSubTotalGroups();
				ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
				if (service != null && service.SessionExists(ShoppingSessionId.ToString()))
				{
					service.AddNode("cartItems", "cartItem", ShoppingSessionId.ToString());
					Dictionary<string, string> attributes = new Dictionary<string, string>
					{
						{
							"productID",
							rentalShoppingCartItem.ProductId.ToString()
						},
						{
							"action",
							rentalShoppingCartItem.Action.ToString()
						},
						{
							"price",
							rentalShoppingCartItem.Price.ToString()
						},
						{
							"quantity",
							rentalShoppingCartItem.Quantity.ToString()
						},
						{
							"taxRate",
							rentalShoppingCartItem.TaxRate.ToString()
						},
						{
							"removed",
							userRemoved.ToString()
						},
						{
							"status",
							rentalShoppingCartItem.Status.ToString()
						},
						{
							"productFamily",
							rentalShoppingCartItem.TitleFamily.ToString()
						},
						{
							"productType",
							rentalShoppingCartItem.TitleType.ToString()
						},
						{
							"sourceType",
							rentalShoppingCartItem.SourceType.ToString()
						}
					};
					service.AddAttributes("cartItem", attributes, ShoppingSessionId.ToString());
				}
				if (!string.IsNullOrEmpty(rentalShoppingCartItem.Barcode))
				{
					ShoppingSession?.ShoppingCart?.RemoveLineItem(rentalShoppingCartItem.Barcode);
				}
				LogHelper.Instance.Log("User removed item {0} {1}", rentalShoppingCartItem.SortName, rentalShoppingCartItem.ProductId);
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ShoppingCartFullMessagesShown = false;
				Promotion promotion = Promotion;
				if (promotion != null && promotion.Intent == PromotionIntentCode.FirstNightGameGameRequired && !PromotionService.Instance.IsFreeInCartOfferRunning && !PromotionService.Instance.IsRentTwoInCartOfferRunning && _items.Count((IRentalShoppingCartItem x) => x.TitleFamily == TitleFamily.Games) == 0)
				{
					ClearPromoCode();
				}
				if (PromotionService.Instance.IsFreeInCartOfferRunning)
				{
					if (!PromotionService.Instance.IsFreeInCartOfferValid && InCartPromoOfferMade)
					{
						ClearPromoCode();
						InCartPromoOfferMade = false;
					}
				}
				else if (PromotionService.Instance.IsRentTwoInCartOfferRunning)
				{
					if (PromotionService.Instance.GetRentTwoInCartOfferItemCount() < 2 && InCartPromoOfferMade)
					{
						ClearPromoCode();
						InCartPromoOfferMade = false;
						InCartPromoOfferCancel = false;
					}
				}
				else if (!InCartPromoOfferCancel)
				{
					PromotionService.Instance.UpdateInCartPromoValue(this);
				}
				ValidateAndUpdatePromoCode();
			}
		}
		if (flag && OnChange != null)
		{
			OnChange(this);
		}
	}

	public void Clear()
	{
		bool flag = false;
		lock (_syncObject)
		{
			if (_items.Count > 0)
			{
				flag = true;
			}
			_items.Clear();
			Totals.UpdateSubTotalGroups();
		}
		if (flag && OnChange != null)
		{
			OnChange(this);
		}
	}

	public IRentalShoppingCartItem GetItem(int index)
	{
		IRentalShoppingCartItem result = null;
		lock (_syncObject)
		{
			if (index < _items.Count)
			{
				result = _items[index];
			}
		}
		return result;
	}

	public bool HasItem(long productId)
	{
		lock (_syncObject)
		{
			return _items.Any((IRentalShoppingCartItem x) => x.ProductId == productId);
		}
	}

	public bool HasItemWithAction(long productId, RentalShoppingCartItemAction action)
	{
		lock (_syncObject)
		{
			return _items.Any((IRentalShoppingCartItem x) => x.ProductId == productId && x.Action == action);
		}
	}

	public bool HasMultiDiscItem()
	{
		lock (_syncObject)
		{
			return _items.Any((IRentalShoppingCartItem x) => x.MultiDisc);
		}
	}

	public int GetCount(long productId)
	{
		lock (_syncObject)
		{
			return _items.Count((IRentalShoppingCartItem x) => x.ProductId == productId);
		}
	}

	public void ClearPromoCode()
	{
		LogHelper.Instance.Log("> Clear Promo Code was called");
		PromotionService.Instance?.ClearPromotion();
		ShoppingSession?.ShoppingCart?.ClearDiscount();
	}

	public void SetShoppingCartGroups()
	{
		IShoppingSession shoppingSession = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(ShoppingSessionId.ToString());
		if (shoppingSession == null || shoppingSession.ShoppingCart == null)
		{
			return;
		}
		foreach (ISubTotalGroup subTotalGroup in Totals.SubTotalGroups)
		{
			shoppingSession.ShoppingCart.SetGroup(subTotalGroup.Action.ToString(), subTotalGroup.TaxRate, subTotalGroup.Amount, subTotalGroup.SubTotal, subTotalGroup.TaxAmount, subTotalGroup.GrantTotal);
		}
	}

	public IPromoCodeInfo ValidateAndUpdatePromoCode()
	{
		PromoCodeInfo result = new PromoCodeInfo();
		if (Promotion != null)
		{
			if (Promotion.Intent == PromotionIntentCode.RxGx)
			{
				ValidateRxGxPromo(result, Promotion);
			}
			else if (Promotion.CampaignTitles != null && Promotion.CampaignTitles.Titles != null)
			{
				ValidateTitleSpecificPromo(result, Promotion);
			}
		}
		return result;
	}

	private void SetRentalShoppingCartItemPricing(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		IPricingRecord pricingRecord = service?.GetProductPricingRecord(rentalShoppingCartItem.ProductId);
		if (service != null && rentalShoppingCartItem.TitleType == TitleType.DigitalCode)
		{
			rentalShoppingCartItem.PricingRecord = new PricingRecord
			{
				Purchase = rentalShoppingCartItem.PurchasePrice
			};
			rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.Purchase;
			rentalShoppingCartItem.TaxRate = service.GetDigitalPurchaseTaxRate() / 100m;
		}
		else
		{
			if (service == null || pricingRecord == null)
			{
				return;
			}
			if (rentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
			{
				bool flag = rentalShoppingCartItem.ProductId == 1139;
				rentalShoppingCartItem.PricingRecord = new PricingRecord
				{
					Purchase = (flag ? service.GetEmptyCasePrice() : rentalShoppingCartItem.PurchasePrice),
					ExpirationPrice = (flag ? service.GetEmptyCasePrice() : pricingRecord.ExpirationPrice)
				};
				rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.Purchase;
				rentalShoppingCartItem.TaxRate = service.GetPurchaseTaxRate() / 100m;
			}
			else if (rentalShoppingCartItem.Action == RentalShoppingCartItemAction.rental)
			{
				rentalShoppingCartItem.PricingRecord = pricingRecord.Clone();
				rentalShoppingCartItem.TaxRate = service.GetRentalTaxRate() / 100m;
				if (rentalShoppingCartItem.MultiDisc && rentalShoppingCartItem.DiscNumber > 1)
				{
					rentalShoppingCartItem.PricingRecord.InitialNight = 0m;
					rentalShoppingCartItem.PricingRecord.ExtraNight = 0m;
					rentalShoppingCartItem.PricingRecord.NonReturn = 0m;
					rentalShoppingCartItem.PricingRecord.ExpirationPrice = 0m;
					rentalShoppingCartItem.Price = 0m;
				}
				else
				{
					rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
				}
			}
		}
	}

	private void ValidateTitleSpecificPromo(IPromoCodeInfo result, Promotion promotion)
	{
		List<long> list = new List<long>();
		foreach (IRentalShoppingCartItem item in Items.Where((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental))
		{
			list.Add(item.ProductId);
		}
		int num = list.Intersect(promotion.CampaignTitles.Titles).ToList().Count();
		if (promotion.CampaignTitles.Include && num > 0)
		{
			result.IsCartValid = true;
		}
		if (!promotion.CampaignTitles.Include && num != list.Count())
		{
			result.IsCartValid = true;
		}
		if (result.IsCartValid)
		{
			promotion.Price = promotion.OriginalPrice;
			promotion.Percent = promotion.OriginalPercent;
		}
		else
		{
			promotion.Price = 0m;
			promotion.Percent = 0m;
		}
		ShoppingSession?.ShoppingCart?.SetDiscount(promotion.Name, promotion.Price);
	}

	private void ValidateRxGxPromo(IPromoCodeInfo result, Promotion promotion)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (service != null)
		{
			decimal dvdPrice = service.GetPriceSetting(TitleFamily.Movies, TitleType.DVD, "initial_night", RentalShoppingCartItemAction.rental) ?? 0m;
			decimal bluRayPrice = service.GetPriceSetting(TitleFamily.Movies, TitleType.Bluray, "initial_night", RentalShoppingCartItemAction.rental) ?? 0m;
			decimal gamePrice = service.GetPriceSetting(TitleFamily.Games, TitleType.All, "initial_night", RentalShoppingCartItemAction.rental) ?? 0m;
			if (promotion.RentQuantity != 0 && promotion.RentFormat.HasValue && promotion.GetQuantity != 0 && promotion.GetFormat.HasValue)
			{
				int val = RxGxGetRequiredCount("Dvd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(PromotionGetFormat.Dvd, 0, null);
				val = Math.Max(val, 0);
				int val2 = RxGxGetRequiredCount("BluRay", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(PromotionGetFormat.BluRay, 0, null);
				val2 = Math.Max(val2, 0);
				int val3 = RxGxGetRequiredCount("Game", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(PromotionGetFormat.Game, 0, null);
				val3 = Math.Max(val3, 0);
				int val4 = RxGxGetRequiredCount("Any", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(PromotionGetFormat.Any, promotion.RentQuantity, promotion.RentFormat);
				val4 = Math.Max(val4, 0);
				bool isCartValid = val == 0 && val2 == 0 && val3 == 0 && val4 == 0;
				promotion.Price = RxGxGetPromoCodeValue(isCartValid, promotion.GetQuantity, promotion.GetFormat.Value, dvdPrice, bluRayPrice, gamePrice);
				ShoppingSession?.ShoppingCart.SetDiscount(promotion.Name, promotion.Price);
				result.IsCartValid = isCartValid;
				result.Title = RxGxGetTitle(promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
				result.Message = RxGxGetMessage(val, val2, val3, val4);
			}
		}
	}

	private string RxGxGetMessage(int dvdsRemaining, int bluRaysRemaining, int gamesRemaining, int anyRemaining)
	{
		string result = null;
		if (dvdsRemaining > 0 || bluRaysRemaining > 0 || gamesRemaining > 0 || anyRemaining > 0)
		{
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "promo_code_rxgx_message");
			string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "and");
			int count = 0;
			List<string> conditions = new List<string>();
			Action<int, string> action = delegate(int amountRemaining, string format)
			{
				if (amountRemaining > 0)
				{
					conditions.Add(string.Format("{0} {1}{2}", amountRemaining, BundleFunctions.GetDictionaryStringValue("strings", format), (amountRemaining > 1) ? "s" : ""));
					count++;
				}
			};
			action(dvdsRemaining, "dvd");
			action(bluRaysRemaining, "blu_ray");
			action(gamesRemaining, "game");
			action(anyRemaining, "other_item");
			result = string.Format(dictionaryStringValue, string.Join($" {dictionaryStringValue2} ", conditions));
		}
		return result;
	}

	private string RxGxGetTitle(int rentQuantity, PromotionRentFormat rentFormat, int getQuantity, PromotionGetFormat getFormat)
	{
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "promo_code_rxgx_title");
		string text = null;
		switch (rentFormat)
		{
		case PromotionRentFormat.Dvd:
			text = BundleFunctions.GetDictionaryStringValue("strings", "dvd");
			break;
		case PromotionRentFormat.BluRay:
			text = BundleFunctions.GetDictionaryStringValue("strings", "blu_ray");
			break;
		case PromotionRentFormat.Game:
			text = BundleFunctions.GetDictionaryStringValue("strings", "game");
			break;
		}
		if (rentQuantity > 0)
		{
			text = $"{text}s";
		}
		string text2 = null;
		switch (getFormat)
		{
		case PromotionGetFormat.Any:
			text2 = BundleFunctions.GetDictionaryStringValue("strings", "any");
			break;
		case PromotionGetFormat.Dvd:
			text2 = BundleFunctions.GetDictionaryStringValue("strings", "dvd");
			break;
		case PromotionGetFormat.BluRay:
			text2 = BundleFunctions.GetDictionaryStringValue("strings", "blu_ray");
			break;
		case PromotionGetFormat.Game:
			text2 = BundleFunctions.GetDictionaryStringValue("strings", "game");
			break;
		}
		if (getQuantity > 0 && getFormat != PromotionGetFormat.Any)
		{
			text2 = $"{text2}s";
		}
		return string.Format(dictionaryStringValue, rentQuantity, text, getQuantity, text2);
	}

	private decimal RxGxGetPromoCodeValue(bool isCartValid, byte getQuantity, PromotionGetFormat getFormat, decimal dvdPrice, decimal bluRayPrice, decimal gamePrice)
	{
		if (!isCartValid)
		{
			return 0m;
		}
		switch (getFormat)
		{
		case PromotionGetFormat.Dvd:
			return dvdPrice * (decimal)getQuantity;
		case PromotionGetFormat.BluRay:
			return bluRayPrice * (decimal)getQuantity;
		case PromotionGetFormat.Game:
			return gamePrice * (decimal)getQuantity;
		case PromotionGetFormat.Any:
		{
			List<decimal> list = new List<decimal>();
			foreach (IRentalShoppingCartItem item in Items.Where((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental))
			{
				list.Add(item.Price);
			}
			list = list.OrderBy((decimal price) => price).ToList();
			decimal result = 0m;
			for (int num = 0; num < getQuantity; num++)
			{
				result += list[num];
			}
			return result;
		}
		default:
			return 0m;
		}
	}

	private int RxGxGetRequiredCount(string format, byte rentQuantity, PromotionRentFormat rentFormat, byte getQuantity, PromotionGetFormat getFormat)
	{
		int num = 0;
		if (format == rentFormat.ToString())
		{
			num += rentQuantity;
		}
		if (format == getFormat.ToString())
		{
			num += getQuantity;
		}
		return num;
	}

	private int RxGxGetCartItemCount(PromotionGetFormat format, int excludeQuantity, PromotionRentFormat? excludeFormat)
	{
		int num = 0;
		int num2 = 0;
		foreach (IRentalShoppingCartItem item in Items.Where((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental))
		{
			if (num2 < excludeQuantity)
			{
				PromotionRentFormat? promotionRentFormat = excludeFormat;
				PromotionRentFormat promotionRentFormat2 = PromotionRentFormat.Game;
				if (promotionRentFormat.GetValueOrDefault() != promotionRentFormat2 || !promotionRentFormat.HasValue || item.TitleFamily != TitleFamily.Games)
				{
					promotionRentFormat = excludeFormat;
					promotionRentFormat2 = PromotionRentFormat.Dvd;
					if (promotionRentFormat.GetValueOrDefault() != promotionRentFormat2 || !promotionRentFormat.HasValue || item.TitleType != TitleType.DVD)
					{
						promotionRentFormat = excludeFormat;
						promotionRentFormat2 = PromotionRentFormat.BluRay;
						if (promotionRentFormat.GetValueOrDefault() != promotionRentFormat2 || !promotionRentFormat.HasValue || item.TitleType != TitleType.Bluray)
						{
							goto IL_00bc;
						}
					}
				}
				num2++;
				continue;
			}
			goto IL_00bc;
			IL_00bc:
			if (format == PromotionGetFormat.Any || (format == PromotionGetFormat.Dvd && item.TitleType == TitleType.DVD) || (format == PromotionGetFormat.BluRay && item.TitleType == TitleType.Bluray) || (format == PromotionGetFormat.Game && item.TitleFamily == TitleFamily.Games))
			{
				num++;
			}
		}
		return num;
	}
}
