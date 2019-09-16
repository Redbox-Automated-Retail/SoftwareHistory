using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCart : IRentalShoppingCart
{
	private delegate IPromoCodeValidationInfo PromotionValidationFunction(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters);

	private class PromotionFunctionFilters
	{
		public bool IncludeMultiNightPricingItems { get; set; }

		public bool IncludeFlashDeals { get; set; }
	}

	private bool? _serviceFeeEnabled;

	private bool _inCartTitleMarketing;

	private IUpsellInfo _upsellInfo;

	private IComingSoonOptinInfo _comingSoonOptinInfo;

	private readonly object _syncObject = new object();

	private IList<IRentalShoppingCartItem> _items = new List<IRentalShoppingCartItem>();

	private ITotals _totals;

	private IServiceFee _serviceFee;

	private decimal _defaultServiceFee;

	private decimal _defaultServiceFeeTaxRate;

	private bool _isAfterVend;

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
				return _items.Count >= MaximumCapacity;
			}
		}
	}

	public IUpsellInfo UpsellInfo
	{
		get
		{
			if (_upsellInfo == null)
			{
				_upsellInfo = new UpsellInfo(this);
			}
			return _upsellInfo;
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

	public decimal DefaultServiceFee => _defaultServiceFee;

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

	public OptInConfirmationStatus MarketingOptInConfirmationStatus { get; set; }

	public bool NeedLookupOfLoyaltyPointsToBeEarned => Items.Count(delegate(IRentalShoppingCartItem x)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = x.LoyaltyPointsRecord;
		return loyaltyPointsRecord == null || !loyaltyPointsRecord.PointsToBeEarned.HasValue;
	}) > 0;

	public bool HasItemsToBeRedeemed => Items.Any((IRentalShoppingCartItem x) => x.LoyaltyPointsRecord?.ToBeRedeemed ?? false);

	public IServiceFee ServiceFee
	{
		get
		{
			if (_serviceFee == null)
			{
				_serviceFee = new ServiceFee();
			}
			return _serviceFee;
		}
		set
		{
			_serviceFee = value;
		}
	}

	public bool ServiceFeeEnabled
	{
		get
		{
			if (!_serviceFeeEnabled.HasValue)
			{
				Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
				_serviceFeeEnabled = Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled && store != null && store.ServiceFee > 0m;
			}
			return _serviceFeeEnabled.Value;
		}
	}

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

	public bool PickupRecommendationsSelected
	{
		get
		{
			if (PickupRecommendationsShown)
			{
				return Items.Any((IRentalShoppingCartItem i) => i.SourceType == ItemSourceType.Recommendation);
			}
			return false;
		}
	}

	public event RentalShoppingCartChanged OnChange;

	public RentalShoppingCart(Guid shoppingSessionId, decimal defaultServiceFee, decimal defaultServiceFeeTaxRate)
	{
		ShoppingSessionId = shoppingSessionId;
		_defaultServiceFee = defaultServiceFee;
		_defaultServiceFeeTaxRate = defaultServiceFeeTaxRate;
	}

	public void SetDefaultServiceFee(decimal defaultServiceFee)
	{
		_defaultServiceFee = defaultServiceFee;
		UpdateServiceFee();
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
				if (!HasItem(rentalShoppingCartItem.ProductId))
				{
					SetRentalShoppingCartItemPricing(rentalShoppingCartItem, pricingRecord);
					rentalShoppingCartItem.Sequence = (InCartPromoOfferMade ? RentalShoppingCartItemSequence.post_offer : RentalShoppingCartItemSequence.pre_offer);
					_items.Add(rentalShoppingCartItem);
					LogHelper.Instance.Log("Adding to Rental Shopping Cart: {0} Item: {1}", ShoppingSessionId, rentalShoppingCartItem);
					Totals.UpdateSubTotalGroups();
					ValidateAndUpdatePromoCode();
					UpdateServiceFee();
				}
				else
				{
					LogHelper.Instance.Log($"Unable to add item with product id {rentalShoppingCartItem.ProductId}.  Product id is already in the shopping cart.");
				}
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
					AddCartItemToAnalytics(rentalShoppingCartItem, userRemoved, service);
				}
				LogHelper.Instance.Log("User removed item {0} {1}", rentalShoppingCartItem.SortName, rentalShoppingCartItem.ProductId);
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ShoppingCartFullMessagesShown = false;
				ValidateAndUpdatePromoCode();
				UpdateServiceFee();
				ClearLoyaltyPointsToBeEarned();
			}
		}
		if (flag && OnChange != null)
		{
			OnChange(this);
		}
	}

	public void AddCartItemToAnalytics(IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved, ISessionAnalyticsService analytics)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("productID", rentalShoppingCartItem.ProductId.ToString());
		dictionary.Add("action", rentalShoppingCartItem.Action.ToString());
		dictionary.Add("price", rentalShoppingCartItem.Price.ToString());
		dictionary.Add("quantity", rentalShoppingCartItem.Quantity.ToString());
		dictionary.Add("taxRate", rentalShoppingCartItem.TaxRate.ToString());
		dictionary.Add("removed", userRemoved.ToString());
		dictionary.Add("status", rentalShoppingCartItem.Status.ToString());
		dictionary.Add("productFamily", rentalShoppingCartItem.TitleFamily.ToString());
		dictionary.Add("productType", rentalShoppingCartItem.TitleType.ToString());
		if (!userRemoved)
		{
			dictionary.Add("barcode", rentalShoppingCartItem.Barcode ?? string.Empty);
		}
		dictionary.Add("sourceType", rentalShoppingCartItem.SourceType.ToString());
		if (!InCartPromoOfferCancel && InCartPromoOfferMade)
		{
			dictionary.Add("add_sequence", rentalShoppingCartItem.Sequence.ToString());
		}
		dictionary.Add("productPricingId", rentalShoppingCartItem.PricingRecord.ProductPricingId.ToString());
		if (rentalShoppingCartItem.Action != RentalShoppingCartItemAction.purchase)
		{
			dictionary.Add("extraNight", rentalShoppingCartItem.PricingRecord.ExtraNight.ToString());
			dictionary.Add("initialDays", rentalShoppingCartItem.PricingRecord.InitialDays.ToString());
			dictionary.Add("defaultInitialNightPrice", rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.ToString());
			dictionary.Add("defaultExtraNightPrice", rentalShoppingCartItem.PricingRecord.DefaultExtraNightPrice.ToString());
		}
		analytics.AddNode("cartItems", "cartItem", dictionary, ShoppingSessionId.ToString());
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
	}

	public void UpdateServiceFee()
	{
		decimal obj = ServiceFee?.ActualAmount ?? 0m;
		decimal num = Items.Where((IRentalShoppingCartItem x) => x?.TitleProduct?.IsEmptyCase ?? false).Sum((IRentalShoppingCartItem x) => x.PurchasePrice);
		Items.Count(delegate(IRentalShoppingCartItem x)
		{
			bool? obj2;
			if (x == null)
			{
				obj2 = null;
			}
			else
			{
				ITitleProduct titleProduct = x.TitleProduct;
				obj2 = ((titleProduct != null) ? new bool?(!titleProduct.IsEmptyCase) : ((bool?)null));
			}
			return obj2 ?? false;
		});
		ServiceFee.DefaultAmount = _defaultServiceFee;
		ServiceFee.TaxRate = _defaultServiceFeeTaxRate;
		if (Totals.SubTotal - num > 0m)
		{
			ServiceFee.ActualAmount = _defaultServiceFee;
		}
		else
		{
			ServiceFee.ActualAmount = 0m;
		}
		if (obj != (ServiceFee?.ActualAmount ?? 0m))
		{
			LogHelper.Instance.Log($"Setting service fee: {ServiceFee?.ActualAmount ?? 0m}");
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
		if (promotion != null && promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightGameGameRequired && !PromotionService.Instance.IsFreeInCartOfferRunning)
		{
			promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightGame, Promotion);
			if (!promoCodeValidationInfo.IsCartValid)
			{
				ClearPromoCode();
			}
		}
		if (Promotion != null)
		{
			if (!InCartPromoOfferAccepted)
			{
				if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.RxGx)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateRxGxPromo, Promotion);
				}
				else if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.PercentOffCartTotal)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidatePercentOffPromo, Promotion);
				}
				else if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightDVD)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightDvd, Promotion);
				}
				else if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightBluray)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightBluRay, Promotion);
				}
				else if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightGame || Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightGameGameRequired)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightGame, Promotion);
				}
				else if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightFreeItem)
				{
					promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightFree, Promotion);
				}
				else
				{
					if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FixedValue)
					{
						ICampaignTitles campaignTitles = Promotion.CampaignTitles;
						if (campaignTitles != null && campaignTitles.Titles?.Count > 0)
						{
							promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateTitleSpecificPromo, Promotion);
							goto IL_0310;
						}
					}
					if (Promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FixedValue)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFixedValuePromo, Promotion);
					}
				}
			}
			goto IL_0310;
		}
		goto IL_05bb;
		IL_05bb:
		return promoCodeValidationInfo;
		IL_0310:
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
		LogHelper.Instance.Log(string.Format("Validate Promo {0}.  IsValid:{1}{2}{3}  Total Promo discounts: {4}", Promotion?.Intent, promoCodeValidationInfo.IsCartValid, promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing ? " InvalidBecauseOfMultiNightPricing" : null, promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal ? " InvalidBecauseOfFlashDeal" : null, promoCodeValidationInfo.Price));
		LogHelper.Instance.Log("Promo Qualifying items: {0}", promoCodeValidationInfo.QualifyingItems.Select((IRentalShoppingCartItem x) => x.ProductId).ToList().Join(", "));
		LogHelper.Instance.Log("Promo Redemption items: {0}", promoCodeValidationInfo.PromotionItems.Select((IPromotionItem x) => $"{x.RentalShoppingCartItem?.ProductId} {x.PromotionAmount:C2}").ToList().Join(", "));
		LogHelper.Instance.Log("Loyalty Redemption items: {0}", (from x in Items
			where x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false
			select $"{x.ProductId} {x?.PricingRecord?.InitialNight ?? 0m:C2}").ToList().Join(", "));
		LogHelper.Instance.Log("Total Loyalty discounts {0}", Items.Where((IRentalShoppingCartItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false).Sum((IRentalShoppingCartItem x) => x?.PricingRecord?.InitialNight ?? 0m));
		LogHelper.Instance.Log("Total discounts {0}", Totals.DiscountAmountUsed);
		goto IL_05bb;
	}

	public void ReValidatePromoCodeAfterVend()
	{
		_isAfterVend = true;
		if (Promotion != null)
		{
			Promotion.IsValidatedPostVend = true;
			if (Promotion.IsProductSpecific)
			{
				ValidateAndUpdatePromoCode();
			}
		}
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

	public void Abandon(string reason)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ComingSoonOptInEnabled)
		{
			List<IOptInItem> optInItems = ComingSoonOptInInfo.OptInItems;
			if (optInItems != null && optInItems.Count > 0)
			{
				string storeNumber = ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue<string>("Store", "ID");
				ServiceLocator.Instance.GetService<IPromotionServices>()?.KioskOptIn(storeNumber, optInItems, null);
			}
		}
		ShoppingSession?.Abandon(reason);
		ShoppingSession?.End();
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
		else
		{
			if (rentalShoppingCartItem.Action != RentalShoppingCartItemAction.rental)
			{
				return;
			}
			if (pricingRecord == null)
			{
				pricingRecord = service.GetOneDayProductPricingRecord(rentalShoppingCartItem.TitleProduct);
			}
			if (pricingRecord == null)
			{
				LogHelper.Instance.Log($"RentalShoppingCart.SetRentalShoppingCartItemPricing - pricing record was not found for product ID {rentalShoppingCartItem.TitleProduct.ProductId}", LogEntryType.Error);
				return;
			}
			rentalShoppingCartItem.PricingRecord = pricingRecord.Clone();
			rentalShoppingCartItem.TaxRate = service.GetRentalTaxRate() / 100m;
			if (rentalShoppingCartItem.MultiDisc && rentalShoppingCartItem.DiscNumber > 1)
			{
				rentalShoppingCartItem.PricingRecord.InitialNight = 0m;
				rentalShoppingCartItem.PricingRecord.ExtraNight = 0m;
				rentalShoppingCartItem.PricingRecord.NonReturn = 0m;
				rentalShoppingCartItem.PricingRecord.ExpirationPrice = 0m;
				rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice = 0m;
				rentalShoppingCartItem.PricingRecord.DefaultExtraNightPrice = 0m;
				rentalShoppingCartItem.PricingRecord.PromoValue = 0m;
				rentalShoppingCartItem.Price = 0m;
			}
			else
			{
				rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
			}
		}
	}

	private IRentalShoppingCartFilterCriteria GetPromoEligibleItemsFilterCriteria(PromotionFunctionFilters promotionFunctionFilters)
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
		obj.ProductTypeId = Promotion?.ProductTypeId;
		obj.FormatIds = Promotion?.FormatIds;
		obj.IncludeLoyaltyRedeemedItems = false;
		obj.IncludeMultiNightPricedItems = promotionFunctionFilters?.IncludeMultiNightPricingItems ?? false;
		obj.IncludeFlashDeals = promotionFunctionFilters?.IncludeFlashDeals ?? false;
		return obj;
	}

	private IPromoCodeValidationInfo ValidateTitleSpecificPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		promoCodeValidationInfo.IsCartValid = list.Any();
		foreach (IRentalShoppingCartItem item in list)
		{
			decimal promotionAmount = Math.Min(promotion.OriginalPrice, item?.PricingRecord?.InitialNight ?? 0m);
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = promotionAmount
			});
			promoCodeValidationInfo.QualifyingItems.Add(item);
		}
		promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFixedValuePromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
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

	private IPromoCodeValidationInfo ValidateFirstNightDvd(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.DVD, promotionFunctionFilters);
	}

	private IPromoCodeValidationInfo ValidateFirstNightBluRay(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.Bluray, promotionFunctionFilters);
	}

	private IPromoCodeValidationInfo ValidateFirstNightByTitleType(IPromotion promotion, TitleType titleType, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters);
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
		if ((promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightDVD || promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightBluray) && rentalShoppingCartItem == null)
		{
			promoEligibleItemsFilterCriteria.TitleType = TitleType.All;
			List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(promoEligibleItemsFilterCriteria)
				orderby x.PricingRecord.InitialNight, x.ProductId
				select x).ToList();
			IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
			IPricingRecord pricingRecord = service?.GetActiveProductTypePricingRecord(TitleFamily.Movies, titleType);
			if (pricingRecord == null)
			{
				pricingRecord = service?.GetProductTypePricingRecord(TitleFamily.Movies, titleType, useAdjustedPrices: true);
			}
			promoCodeValidationInfo.IsCartValid = list.Any();
			decimal num = pricingRecord?.PromoValue ?? 0m;
			foreach (IRentalShoppingCartItem item in list)
			{
				if (num > 0m)
				{
					decimal num2 = Math.Min(num, item?.PricingRecord?.InitialNight ?? 0m);
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num2
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					num -= num2;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightGame(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters);
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
		if (promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightGame && rentalShoppingCartItem == null)
		{
			promoEligibleItemsFilterCriteria.TitleFamily = TitleFamily.All;
			List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(promoEligibleItemsFilterCriteria)
				orderby x.PricingRecord.InitialNight, x.ProductId
				select x).ToList();
			IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
			IPricingRecord pricingRecord = service?.GetActiveProductTypePricingRecord(TitleFamily.Games, TitleType.All);
			if (pricingRecord == null)
			{
				pricingRecord = service?.GetProductTypePricingRecord(TitleFamily.Games, TitleType.All, useAdjustedPrices: true);
			}
			promoCodeValidationInfo.IsCartValid = list.Any();
			decimal num = pricingRecord?.PromoValue ?? 0m;
			foreach (IRentalShoppingCartItem item in list)
			{
				if (num > 0m)
				{
					decimal num2 = Math.Min(num, item?.PricingRecord?.InitialNight ?? 0m);
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num2
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					num -= num2;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightFree(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartItem rentalShoppingCartItem = (from x in GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
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

	private IPromoCodeValidationInfo ValidatePromoConsideringConditions(PromotionValidationFunction validationFunction, IPromotion promotion)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = validationFunction(promotion, new PromotionFunctionFilters());
		if (!promoCodeValidationInfo.IsCartValid)
		{
			if (validationFunction(promotion, new PromotionFunctionFilters
			{
				IncludeMultiNightPricingItems = true
			}).IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items;
				promoCodeValidationInfo.Message = null;
				promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
			}
			else if (validationFunction(promotion, new PromotionFunctionFilters
			{
				IncludeFlashDeals = true
			}).IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items;
				promoCodeValidationInfo.Message = null;
				promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal = true;
			}
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidatePercentOffPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartItem> list = GetItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters)).ToList();
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

	private IPromoCodeValidationInfo ValidateRxGxPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo result = new PromoCodeValidationInfo();
		if (promotion.RentQuantity != 0 && promotion.RentFormat.HasValue && promotion.GetQuantity != 0 && promotion.GetFormat.HasValue)
		{
			List<IRentalShoppingCartItem> rentItems = GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, GetTitleTypeFromPromotionFormat(promotion.RentFormat.Value), GetTitleFamilyFromPromotionFormat(promotion.RentFormat.Value), null, null, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: false, includeLoyaltyRedeemedItems: false).OrderBy(delegate(IRentalShoppingCartItem x)
			{
				Redbox.Rental.Model.PromotionRentFormat? rentFormat = promotion.RentFormat;
				Redbox.Rental.Model.PromotionRentFormat promotionRentFormat = Redbox.Rental.Model.PromotionRentFormat.Any;
				return rentFormat.GetValueOrDefault() == promotionRentFormat && rentFormat.HasValue && promotion.GetFormat != Redbox.Rental.Model.PromotionGetFormat.Any && GetTitleTypeFromPromotionFormat(promotion.GetFormat.Value) == x.TitleType;
			}).ThenByDescending((IRentalShoppingCartItem x) => x.PricingRecord.InitialNight).ThenByDescending((IRentalShoppingCartItem x) => x.ProductId)
				.Take(promotion.RentQuantity)
				.ToList();
			List<IRentalShoppingCartItem> list = (from x in GetItemsByCriteria(RentalShoppingCartItemAction.rental, null, GetTitleTypeFromPromotionFormat(promotion.GetFormat.Value), GetTitleFamilyFromPromotionFormat(promotion.GetFormat.Value), null, null, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: false, includeLoyaltyRedeemedItems: false)
				where !rentItems.Contains(x)
				orderby x.PricingRecord.InitialNight, x.ProductId
				select x).Take(promotion.GetQuantity).ToList();
			List<IRentalShoppingCartItem> list2 = new List<IRentalShoppingCartItem>();
			list2.AddRange(list);
			list2.AddRange(rentItems);
			int num = RxGxGetRequiredCount("Dvd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num2 = Math.Max(num - list2.Count((IRentalShoppingCartItem x) => x.TitleType == TitleType.DVD), 0);
			int num3 = RxGxGetRequiredCount("BluRay", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num4 = Math.Max(num3 - list2.Count((IRentalShoppingCartItem x) => x.TitleType == TitleType.Bluray), 0);
			int num5 = RxGxGetRequiredCount("FourKUhd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num6 = Math.Max(num5 - list2.Count((IRentalShoppingCartItem x) => x.TitleType == TitleType._4KUHD), 0);
			int num7 = RxGxGetRequiredCount("Game", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num8 = Math.Max(num7 - list2.Count((IRentalShoppingCartItem x) => x.TitleFamily == TitleFamily.Games), 0);
			int num9 = Math.Max(RxGxGetRequiredCount("Any", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) + num + num3 + num5 + num7 - num2 - num4 - num6 - num8 - list2.Count(), 0);
			bool flag = num2 == 0 && num4 == 0 && num6 == 0 && num8 == 0 && num9 == 0;
			result.IsCartValid = flag;
			result.Price = (flag ? list.Sum((IRentalShoppingCartItem x) => x.PricingRecord.InitialNight) : 0m);
			result.Title = RxGxGetTitle(promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			result.Message = RxGxGetMessage(num2, num4, num6, num8, num9);
			rentItems.ForEach(delegate(IRentalShoppingCartItem x)
			{
				result.QualifyingItems.Add(x);
			});
			list.ForEach(delegate(IRentalShoppingCartItem x)
			{
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = x,
					PromotionAmount = x.PricingRecord.InitialNight
				});
			});
		}
		return result;
	}

	private TitleFamily GetTitleFamilyFromPromotionFormat(Redbox.Rental.Model.PromotionRentFormat rentFormat)
	{
		if (rentFormat == Redbox.Rental.Model.PromotionRentFormat.Game)
		{
			return TitleFamily.Games;
		}
		return TitleFamily.All;
	}

	private TitleFamily GetTitleFamilyFromPromotionFormat(Redbox.Rental.Model.PromotionGetFormat getFormat)
	{
		if (getFormat == Redbox.Rental.Model.PromotionGetFormat.Game)
		{
			return TitleFamily.Games;
		}
		return TitleFamily.All;
	}

	private TitleType GetTitleTypeFromPromotionFormat(Redbox.Rental.Model.PromotionRentFormat rentFormat)
	{
		return rentFormat switch
		{
			Redbox.Rental.Model.PromotionRentFormat.Dvd => TitleType.DVD, 
			Redbox.Rental.Model.PromotionRentFormat.BluRay => TitleType.Bluray, 
			Redbox.Rental.Model.PromotionRentFormat.FourKUhd => TitleType._4KUHD, 
			_ => TitleType.All, 
		};
	}

	private TitleType GetTitleTypeFromPromotionFormat(Redbox.Rental.Model.PromotionGetFormat getFormat)
	{
		return getFormat switch
		{
			Redbox.Rental.Model.PromotionGetFormat.Dvd => TitleType.DVD, 
			Redbox.Rental.Model.PromotionGetFormat.BluRay => TitleType.Bluray, 
			Redbox.Rental.Model.PromotionGetFormat.FourKUhd => TitleType._4KUHD, 
			_ => TitleType.All, 
		};
	}

	private string RxGxGetMessage(int dvdsRemaining, int bluRaysRemaining, int fourKUhdsRemaining, int gamesRemaining, int anyRemaining)
	{
		string result = string.Empty;
		if (dvdsRemaining > 0 || bluRaysRemaining > 0 || fourKUhdsRemaining > 0 || gamesRemaining > 0 || anyRemaining > 0)
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
			action(fourKUhdsRemaining, Resources._4k_uhd);
			action(gamesRemaining, Resources.game);
			action(anyRemaining, Resources.other_item);
			result = string.Format(promo_code_rxgx_message, string.Join($" {Resources.and} ", conditions));
		}
		return result;
	}

	private string RxGxGetTitle(int rentQuantity, Redbox.Rental.Model.PromotionRentFormat rentFormat, int getQuantity, Redbox.Rental.Model.PromotionGetFormat getFormat)
	{
		string promo_code_rxgx_title = Resources.promo_code_rxgx_title;
		string empty = string.Empty;
		empty = rentFormat switch
		{
			Redbox.Rental.Model.PromotionRentFormat.Dvd => Resources.dvd, 
			Redbox.Rental.Model.PromotionRentFormat.BluRay => Resources.blu_ray, 
			Redbox.Rental.Model.PromotionRentFormat.FourKUhd => Resources._4k_uhd, 
			Redbox.Rental.Model.PromotionRentFormat.Game => Resources.game, 
			Redbox.Rental.Model.PromotionRentFormat.Any => Resources.any, 
			_ => string.Empty, 
		};
		if (rentQuantity > 1 && rentFormat != Redbox.Rental.Model.PromotionRentFormat.Any)
		{
			empty = $"{empty}s";
		}
		string empty2 = string.Empty;
		empty2 = getFormat switch
		{
			Redbox.Rental.Model.PromotionGetFormat.Dvd => Resources.dvd, 
			Redbox.Rental.Model.PromotionGetFormat.BluRay => Resources.blu_ray, 
			Redbox.Rental.Model.PromotionGetFormat.FourKUhd => Resources._4k_uhd, 
			Redbox.Rental.Model.PromotionGetFormat.Game => Resources.game, 
			Redbox.Rental.Model.PromotionGetFormat.Any => Resources.any, 
			_ => string.Empty, 
		};
		if (getQuantity > 1 && getFormat != Redbox.Rental.Model.PromotionGetFormat.Any)
		{
			empty2 = $"{empty2}s";
		}
		return string.Format(promo_code_rxgx_title, rentQuantity, empty, getQuantity, empty2);
	}

	private int RxGxGetRequiredCount(string format, byte rentQuantity, Redbox.Rental.Model.PromotionRentFormat rentFormat, byte getQuantity, Redbox.Rental.Model.PromotionGetFormat getFormat)
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
		return GetItemsByCriteria(filterCriteria.Action, filterCriteria.Sequence, filterCriteria.TitleType, filterCriteria.TitleFamily, filterCriteria.IncludeProducts, filterCriteria.ExcludeProducts, filterCriteria.ProductTypes, filterCriteria.ProductTypeId, filterCriteria.FormatIds, filterCriteria.IncludeMultiNightPricedItems, filterCriteria.IncludeFlashDeals, filterCriteria.IncludeLoyaltyRedeemedItems);
	}

	public List<IRentalShoppingCartItem> GetItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, int? productTypeId, List<int> formatIds, bool includeMultiNightPricedItems, bool includeFlashDeals, bool includeLoyaltyRedeemedItems)
	{
		List<IRentalShoppingCartItem> list = new List<IRentalShoppingCartItem>();
		foreach (IRentalShoppingCartItem item in Items)
		{
			if ((!action.HasValue || item.Action == action) && (!sequence.HasValue || item.Sequence == sequence) && (titleType == TitleType.All || item.TitleType == titleType) && (titleFamily == TitleFamily.All || item.TitleFamily == titleFamily) && (includeProducts == null || includeProducts.Count == 0 || includeProducts.Contains(item.ProductId)) && (excludeProducts == null || excludeProducts.Count == 0 || !excludeProducts.Contains(item.ProductId)) && (productTypes == null || productTypes.Count == 0 || productTypes.Contains(item.TitleType)) && (!productTypeId.HasValue || productTypeId == 0 || (TitleFamily?)item.TitleFamily == (TitleFamily?)productTypeId) && (formatIds == null || formatIds.Count == 0 || formatIds.Contains((int)item.TitleType)) && (!item.DiscNumber.HasValue || item.DiscNumber == 1) && (includeMultiNightPricedItems || !(item?.PricingRecord?.IsMultiNightPrice ?? false)) && (includeFlashDeals || !item.HasDeal) && (includeLoyaltyRedeemedItems || !(item?.LoyaltyPointsRecord?.ToBeRedeemed ?? false)) && (!_isAfterVend || item.Status == LineItemStatus.Fullfilled))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public IRentalShoppingCart ShallowClone()
	{
		RentalShoppingCart rentalShoppingCart = new RentalShoppingCart(ShoppingSessionId, _defaultServiceFee, _defaultServiceFeeTaxRate);
		foreach (IRentalShoppingCartItem item in Items)
		{
			rentalShoppingCart.AddItem(item, item.PricingRecord);
		}
		return rentalShoppingCart;
	}
}
