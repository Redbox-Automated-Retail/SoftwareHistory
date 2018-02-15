using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCart : IRentalShoppingCart
{
	private delegate IPromoCodeValidationInfo PromotionValidationFunction(IPromotion promotion, bool includeMultiNightPricingItems);

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

	public IPromotion Promotion => PromotionService.Instance.GetPromotion();

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

	public bool NeedLookupOfLoyaltyPointsToBeEarned => Items.Count(delegate(IRentalShoppingCartItem x)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = x.LoyaltyPointsRecord;
		return loyaltyPointsRecord == null || !loyaltyPointsRecord.PointsToBeEarned.HasValue;
	}) > 0;

	public bool HasItemsToBeRedeemed => Items.Any((IRentalShoppingCartItem x) => x.LoyaltyPointsRecord?.ToBeRedeemed ?? false);

	private IShoppingSession ShoppingSession => ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(ShoppingSessionId.ToString());

	public bool InCartPromoOfferMade { get; set; }

	public bool InCartPromoOfferCancel { get; set; }

	public bool InCartPromoOfferAccepted
	{
		get
		{
			if (InCartPromoOfferMade)
			{
				return !InCartPromoOfferCancel;
			}
			return false;
		}
	}

	public bool InCartDoNotShow { get; set; }

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
		AddItem(rentalShoppingCartItem, null);
	}

	public void AddItem(IRentalShoppingCartItem rentalShoppingCartItem, IPricingRecord pricingRecord)
	{
		lock (_syncObject)
		{
			if (rentalShoppingCartItem != null)
			{
				SetRentalShoppingCartItemPricing(rentalShoppingCartItem, pricingRecord);
				rentalShoppingCartItem.Sequence = (InCartPromoOfferMade ? RentalShoppingCartItemSequence.post_offer : RentalShoppingCartItemSequence.pre_offer);
				_items.Add(rentalShoppingCartItem);
				LogHelper.Instance.Log("Adding to Rental Shopping Cart: {0} Item: {1}", ShoppingSessionId, rentalShoppingCartItem);
				Totals.UpdateSubTotalGroups();
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
				ValidateAndUpdatePromoCode();
				ClearLoyaltyPointsToBeEarned();
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

	public IPromoCodeValidationInfo ValidateAndUpdatePromoCode()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		if (PromotionService.Instance.IsFreeInCartOfferRunning)
		{
			promoCodeValidationInfo = PromotionService.Instance.ValidateFreeInCartOffer();
			if (!promoCodeValidationInfo.IsCartValid && InCartPromoOfferMade)
			{
				ClearPromoCode();
				InCartPromoOfferMade = false;
			}
		}
		else if (PromotionService.Instance.IsRentTwoInCartOfferRunning)
		{
			if ((PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) < 2 && InCartPromoOfferMade)
			{
				ClearPromoCode();
				InCartPromoOfferMade = false;
				InCartPromoOfferCancel = false;
			}
		}
		else if (InCartPromoOfferAccepted)
		{
			promoCodeValidationInfo = PromotionService.Instance.ValidateRegularInCartPromo(this);
			if (promoCodeValidationInfo.IsCartValid && Promotion == null)
			{
				PromotionService.Instance.SetPromoToFixedPriceInCart(this, promoCodeValidationInfo.Price);
			}
		}
		IPromotion promotion = Promotion;
		if (promotion != null && promotion.Intent == PromotionIntentCode.FirstNightGameGameRequired && !PromotionService.Instance.IsFreeInCartOfferRunning)
		{
			promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateFirstNightGame, Promotion);
			if (!promoCodeValidationInfo.IsCartValid)
			{
				ClearPromoCode();
			}
		}
		if (Promotion != null)
		{
			if (!InCartPromoOfferAccepted)
			{
				if (Promotion.Intent == PromotionIntentCode.RxGx)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateRxGxPromo, Promotion);
				}
				else if (Promotion.Intent == PromotionIntentCode.PercentOffCartTotal)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidatePercentOffPromo, Promotion);
				}
				else if (Promotion.Intent == PromotionIntentCode.FirstNightDVD)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateFirstNightDvd, Promotion);
				}
				else if (Promotion.Intent == PromotionIntentCode.FirstNightBluray)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateFirstNightBluRay, Promotion);
				}
				else if (Promotion.Intent == PromotionIntentCode.FirstNightGame || Promotion.Intent == PromotionIntentCode.FirstNightGameGameRequired)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateFirstNightGame, Promotion);
				}
				else if (Promotion.Intent == PromotionIntentCode.FixedValue)
				{
					promoCodeValidationInfo = ValidatePromoConsideringMultiNightPricing(ValidateFixedValuePromo, Promotion);
				}
			}
			Promotion.QualifyingItems.Clear();
			Promotion.PromotionItems.Clear();
			Promotion.Price = promoCodeValidationInfo.Price;
			Promotion.Percent = promoCodeValidationInfo.Percent;
			if (promoCodeValidationInfo.IsCartValid)
			{
				promoCodeValidationInfo.QualifyingItems.ForEach(delegate(IRentalShoppingCartItem x)
				{
					Promotion.QualifyingItems.Add(x);
				});
				promoCodeValidationInfo.PromotionItems.ForEach(delegate(IPromotionItem x)
				{
					Promotion.PromotionItems.Add(x);
				});
			}
			ShoppingSession?.ShoppingCart.SetDiscount(Promotion.Name, Promotion.Price);
			LogHelper.Instance.Log("Validate Promo {0}.  IsValid:{1} {2}  Total Promo discounts: {3}", Promotion?.Intent, promoCodeValidationInfo.IsCartValid, promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing ? "InvalidBecauseOfMultiNightPricing" : null, promoCodeValidationInfo.Price);
			LogHelper.Instance.Log("Promo Qualifying items: {0}", promoCodeValidationInfo.QualifyingItems.Select((IRentalShoppingCartItem x) => x.ProductId).ToList().Join(", "));
			LogHelper.Instance.Log("Promo Redemption items: {0}", promoCodeValidationInfo.PromotionItems.Select((IPromotionItem x) => $"{x.RentalShoppingCartItem?.ProductId} {x.PromotionAmount:C2}").ToList().Join(", "));
			LogHelper.Instance.Log("Loyalty Redemption items: {0}", (from x in Items
				where x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false
				select $"{x.ProductId} {x?.PricingRecord?.InitialNight ?? 0m:C2}").ToList().Join(", "));
			LogHelper.Instance.Log("Total Loyalty discounts {0}", Items.Where((IRentalShoppingCartItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false).Sum((IRentalShoppingCartItem x) => x?.PricingRecord?.InitialNight ?? 0m));
			LogHelper.Instance.Log("Total discounts {0}", Totals.DiscountAmountUsed);
		}
		return promoCodeValidationInfo;
	}

	public void ClearLoyaltyPointsToBeEarned()
	{
		Items.ForEach(delegate(IRentalShoppingCartItem x)
		{
			if (x.LoyaltyPointsRecord != null)
			{
				x.LoyaltyPointsRecord.PointsToBeEarned = null;
			}
		});
	}

	public void ClearAllLoyaltyPointsRecords()
	{
		Items.ForEach(delegate(IRentalShoppingCartItem x)
		{
			x.LoyaltyPointsRecord = null;
		});
	}

	private void SetRentalShoppingCartItemPricing(IRentalShoppingCartItem rentalShoppingCartItem, IPricingRecord pricingRecord)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (service == null)
		{
			LogHelper.Instance.Log("RentalShoppingCart.SetRentalShoppingCartItemPricing - pricing service cannot be null", LogEntryType.Error);
		}
		else if (rentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
		{
			if (pricingRecord == null)
			{
				pricingRecord = rentalShoppingCartItem.TitleProduct.PurchasePriceRecord;
			}
			rentalShoppingCartItem.PricingRecord = pricingRecord.Clone();
			rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.Purchase;
			if (rentalShoppingCartItem.TitleType == TitleType.DigitalCode)
			{
				rentalShoppingCartItem.TaxRate = service.GetDigitalPurchaseTaxRate() / 100m;
			}
			else
			{
				rentalShoppingCartItem.TaxRate = service.GetPurchaseTaxRate() / 100m;
			}
		}
		else if (rentalShoppingCartItem.Action == RentalShoppingCartItemAction.rental)
		{
			if (pricingRecord == null)
			{
				pricingRecord = service.GetOneDayProductPricingRecord(rentalShoppingCartItem.TitleProduct);
			}
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

	private IRentalShoppingCartFilterCriteria GetPromoEligibleItemsFilterCriteria(bool includeMultiNightPricedItems = false)
	{
		RentalShoppingCartFilterCriteria obj = new RentalShoppingCartFilterCriteria
		{
			Action = RentalShoppingCartItemAction.rental,
			Sequence = null,
			TitleType = TitleType.All,
			TitleFamily = TitleFamily.All
		};
		bool? flag = Promotion?.CampaignTitles?.HasTitles;
		obj.IncludeProducts = ((flag.HasValue && flag == true && Promotion.CampaignTitles.Include) ? Promotion.CampaignTitles.Titles : null);
		flag = Promotion?.CampaignTitles?.HasTitles;
		obj.ExcludeProducts = ((flag.HasValue && flag == true && !Promotion.CampaignTitles.Include) ? Promotion.CampaignTitles.Titles : null);
		obj.IncludeLoyaltyRedeemedItems = false;
		obj.IncludeMultiNightPricedItems = includeMultiNightPricedItems;
		return obj;
	}

	private IPromoCodeValidationInfo ValidateFixedValuePromo(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(includeMultiNightPricedItems))
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		promoCodeValidationInfo.IsCartValid = list.Any();
		decimal originalPrice = promotion.OriginalPrice;
		foreach (IRentalShoppingCartItem item in list)
		{
			if (originalPrice > 0m)
			{
				decimal num = Math.Min(originalPrice, item?.PricingRecord?.InitialNight ?? 0m);
				promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num
				});
				promoCodeValidationInfo.QualifyingItems.Add(item);
				originalPrice -= num;
			}
		}
		promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightDvd(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.DVD, includeMultiNightPricedItems);
	}

	private IPromoCodeValidationInfo ValidateFirstNightBluRay(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.Bluray, includeMultiNightPricedItems);
	}

	private IPromoCodeValidationInfo ValidateFirstNightByTitleType(IPromotion promotion, TitleType titleType, bool includeMultiNightPricedItems)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(includeMultiNightPricedItems);
		promoEligibleItemsFilterCriteria.TitleType = titleType;
		IRentalShoppingCartItem rentalShoppingCartItem = (from x in GetItemsByCriteria(promoEligibleItemsFilterCriteria)
			orderby x.PricingRecord.InitialNight, x.ProductId
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
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightGame(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(includeMultiNightPricedItems);
		promoEligibleItemsFilterCriteria.TitleFamily = TitleFamily.Games;
		IRentalShoppingCartItem rentalShoppingCartItem = (from x in GetItemsByCriteria(promoEligibleItemsFilterCriteria)
			orderby x.PricingRecord.InitialNight, x.ProductId
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
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidatePromoConsideringMultiNightPricing(PromotionValidationFunction validationFunction, IPromotion promotion)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = validationFunction(promotion, includeMultiNightPricingItems: false);
		if (!promoCodeValidationInfo.IsCartValid && validationFunction(promotion, includeMultiNightPricingItems: true).IsCartValid)
		{
			promoCodeValidationInfo.Title = Resources.promo_code_error_multi_night_pricing;
			promoCodeValidationInfo.Message = null;
			promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidatePercentOffPromo(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartItem> list = GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(includeMultiNightPricedItems)).ToList();
		decimal arg = list.Sum((IRentalShoppingCartItem x) => x?.PricingRecord?.InitialNight ?? 0m);
		Func<decimal, decimal> func = (decimal originalValue) => decimal.Round(originalValue * (promotion?.Percent ?? 0m), 2, MidpointRounding.AwayFromZero);
		decimal price = func(arg);
		promoCodeValidationInfo.IsCartValid = list.Any();
		promoCodeValidationInfo.Price = price;
		promoCodeValidationInfo.Percent = Promotion.Percent;
		foreach (IRentalShoppingCartItem item in list)
		{
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = func(item.PricingRecord.InitialNight)
			});
			promoCodeValidationInfo.QualifyingItems.Add(item);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateRxGxPromo(IPromotion promotion, bool includeMultiNightPricedItems)
	{
		IPromoCodeValidationInfo result = new PromoCodeValidationInfo();
		if (promotion.RentQuantity != 0 && promotion.RentFormat.HasValue && promotion.GetQuantity != 0 && promotion.GetFormat.HasValue)
		{
			List<IRentalShoppingCartItem> itemsByCriteria = GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, GetTitleTypeFromPromotionFormat(promotion.RentFormat.ToString()), GetTitleFamilyFromPromotionFormat(promotion.RentFormat.ToString()), null, null, null, includeMultiNightPricedItems: true, includeLoyaltyRedeemedItems: true);
			List<IRentalShoppingCartItem> rentItems = (from x in itemsByCriteria
				orderby x.PricingRecord.IsMultiNightPrice descending, x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false descending
				select x).ThenBy(delegate(IRentalShoppingCartItem x)
			{
				bool? flag2 = promotion?.CampaignTitles?.HasTitles;
				return !flag2.HasValue || flag2 != true || !promotion.CampaignTitles.Include || promotion.CampaignTitles.Titles.Contains(x.ProductId);
			}).ThenBy(delegate(IRentalShoppingCartItem x)
			{
				bool? flag2 = promotion?.CampaignTitles?.HasTitles;
				return !flag2.HasValue || flag2 != true || promotion.CampaignTitles.Include || !promotion.CampaignTitles.Titles.Contains(x.ProductId);
			}).ThenByDescending((IRentalShoppingCartItem x) => x.PricingRecord.InitialNight)
				.ThenBy((IRentalShoppingCartItem x) => x.ProductId)
				.Take(promotion.RentQuantity)
				.ToList();
			IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(includeMultiNightPricedItems);
			promoEligibleItemsFilterCriteria.TitleFamily = GetTitleFamilyFromPromotionFormat(promotion.GetFormat.ToString());
			promoEligibleItemsFilterCriteria.TitleType = GetTitleTypeFromPromotionFormat(promotion.GetFormat.ToString());
			List<IRentalShoppingCartItem> list = (from x in (from x in GetItemsByCriteria(promoEligibleItemsFilterCriteria)
					where !rentItems.Contains(x)
					select x).ToList()
				orderby x?.PricingRecord?.InitialNight ?? 1000m, x.ProductId
				select x).Take(promotion.GetQuantity).ToList();
			List<IRentalShoppingCartItem> list2 = new List<IRentalShoppingCartItem>();
			list2.AddRange(list);
			list2.AddRange(rentItems);
			int num = RxGxGetRequiredCount("Dvd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num2 = Math.Max(num - list2.Count((IRentalShoppingCartItem x) => x.TitleType == TitleType.DVD), 0);
			int num3 = RxGxGetRequiredCount("BluRay", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num4 = Math.Max(num3 - list2.Count((IRentalShoppingCartItem x) => x.TitleType == TitleType.Bluray), 0);
			int num5 = RxGxGetRequiredCount("Game", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num6 = Math.Max(num5 - list2.Count((IRentalShoppingCartItem x) => x.TitleFamily == TitleFamily.Games), 0);
			int num7 = Math.Max(RxGxGetRequiredCount("Any", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) + num + num3 + num5 - num2 - num4 - num6 - list2.Count(), 0);
			bool flag = num2 == 0 && num4 == 0 && num6 == 0 && num7 == 0;
			result.IsCartValid = flag;
			result.Price = (flag ? list.Sum((IRentalShoppingCartItem x) => x.PricingRecord.InitialNight) : 0m);
			result.Title = RxGxGetTitle(promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			result.Message = RxGxGetMessage(num2, num4, num6, num7);
			rentItems.ForEach(delegate(IRentalShoppingCartItem x)
			{
				result.QualifyingItems.Add(x);
			});
			list.ForEach(delegate(IRentalShoppingCartItem x)
			{
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = x,
					PromotionAmount = (x?.PricingRecord?.InitialNight ?? 0m)
				});
			});
		}
		return result;
	}

	private TitleFamily GetTitleFamilyFromPromotionFormat(string format)
	{
		if (format == PromotionGetFormat.Game.ToString())
		{
			return TitleFamily.Games;
		}
		return TitleFamily.All;
	}

	private TitleType GetTitleTypeFromPromotionFormat(string format)
	{
		if (format == PromotionGetFormat.Dvd.ToString())
		{
			return TitleType.DVD;
		}
		if (format == PromotionGetFormat.BluRay.ToString())
		{
			return TitleType.Bluray;
		}
		return TitleType.All;
	}

	private string RxGxGetMessage(int dvdsRemaining, int bluRaysRemaining, int gamesRemaining, int anyRemaining)
	{
		string result = null;
		if (dvdsRemaining > 0 || bluRaysRemaining > 0 || gamesRemaining > 0 || anyRemaining > 0)
		{
			string promo_code_rxgx_message = Resources.promo_code_rxgx_message;
			int count = 0;
			List<string> conditions = new List<string>();
			Action<int, string> action = delegate(int amountRemaining, string format)
			{
				if (amountRemaining > 0)
				{
					conditions.Add(string.Format("{0} {1}{2}", amountRemaining, format, (amountRemaining > 1) ? "s" : ""));
					count++;
				}
			};
			action(dvdsRemaining, Resources.dvd);
			action(bluRaysRemaining, Resources.blu_ray);
			action(gamesRemaining, Resources.game);
			action(anyRemaining, Resources.other_item);
			result = string.Format(promo_code_rxgx_message, string.Join($" {Resources.and} ", conditions));
		}
		return result;
	}

	private string RxGxGetTitle(int rentQuantity, PromotionRentFormat rentFormat, int getQuantity, PromotionGetFormat getFormat)
	{
		string promo_code_rxgx_title = Resources.promo_code_rxgx_title;
		string text = null;
		switch (rentFormat)
		{
		case PromotionRentFormat.Dvd:
			text = Resources.dvd;
			break;
		case PromotionRentFormat.BluRay:
			text = Resources.blu_ray;
			break;
		case PromotionRentFormat.Game:
			text = Resources.game;
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
		return string.Format(promo_code_rxgx_title, rentQuantity, text, getQuantity, text2);
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

	public List<IRentalShoppingCartItem> GetItemsByCriteria(IRentalShoppingCartFilterCriteria filterCriteria)
	{
		return GetItemsByCriteria(filterCriteria.Action, filterCriteria.Sequence, filterCriteria.TitleType, filterCriteria.TitleFamily, filterCriteria.IncludeProducts, filterCriteria.ExcludeProducts, filterCriteria.ProductTypes, filterCriteria.IncludeMultiNightPricedItems, filterCriteria.IncludeLoyaltyRedeemedItems);
	}

	public List<IRentalShoppingCartItem> GetItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, bool includeMultiNightPricedItems, bool includeLoyaltyRedeemedItems)
	{
		List<IRentalShoppingCartItem> list = new List<IRentalShoppingCartItem>();
		foreach (IRentalShoppingCartItem item in Items)
		{
			if ((!action.HasValue || item.Action == action) && (!sequence.HasValue || item.Sequence == sequence) && (titleType == TitleType.All || item.TitleType == titleType) && (titleFamily == TitleFamily.All || item.TitleFamily == titleFamily) && (includeProducts == null || includeProducts.Count == 0 || includeProducts.Contains(item.ProductId)) && (excludeProducts == null || excludeProducts.Count == 0 || !excludeProducts.Contains(item.ProductId)) && (productTypes == null || productTypes.Count == 0 || productTypes.Contains(item.TitleType)) && (!item.DiscNumber.HasValue || item.DiscNumber == 1) && (includeMultiNightPricedItems || !(item?.PricingRecord?.IsMultiNightPrice ?? false)) && (includeLoyaltyRedeemedItems || !(item?.LoyaltyPointsRecord?.ToBeRedeemed ?? false)))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
