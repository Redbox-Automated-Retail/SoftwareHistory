using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartService : IRentalShoppingCartService
{
	private readonly object m_syncObject = new object();

	private readonly IDictionary<Guid, IRentalShoppingCart> _rentalShoppingCarts = new Dictionary<Guid, IRentalShoppingCart>();

	public static RentalShoppingCartService Instance => Singleton<RentalShoppingCartService>.Instance;

	public bool CurrentRentalShoppingCartIsFull => CurrentRentalShoppingCart?.IsFull ?? false;

	public IRentalShoppingCart CurrentRentalShoppingCart
	{
		get
		{
			IRentalShoppingCart result = null;
			IShoppingSession currentShoppingSession = GetCurrentShoppingSession();
			if (currentShoppingSession != null)
			{
				result = GetRentalShoppingCart(currentShoppingSession.Id);
			}
			return result;
		}
	}

	public event RentalShoppingCartAdded OnRentalShoppingCartAdded;

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(IRentalShoppingCartService), Instance);
	}

	public IRentalShoppingCart GetRentalShoppingCart(Guid id)
	{
		IRentalShoppingCart result = null;
		lock (m_syncObject)
		{
			if (_rentalShoppingCarts.ContainsKey(id))
			{
				result = _rentalShoppingCarts[id];
			}
			else
			{
				LogHelper.Instance.Log("Rental Shopping Cart with id: '{0}' was not found.", id);
			}
			return result;
		}
	}

	public IRentalShoppingCart CreateNew(Guid id)
	{
		IRentalShoppingCart value = null;
		Store store = (ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService)?.GetStore();
		lock (m_syncObject)
		{
			if (!_rentalShoppingCarts.TryGetValue(id, out value))
			{
				value = new RentalShoppingCart(id, (!Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled) ? 0m : (store?.ServiceFee ?? 0m), (!Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled) ? 0m : (store?.ServiceFeeTaxRate ?? 0m));
				_rentalShoppingCarts[id] = value;
				LogHelper.Instance.Log("Creating Rental Shopping Cart with id: {0}", id);
			}
		}
		if (value != null && OnRentalShoppingCartAdded != null)
		{
			OnRentalShoppingCartAdded(value);
		}
		return value;
	}

	public bool RemoveRentalShoppingCart(Guid id)
	{
		bool result = false;
		lock (m_syncObject)
		{
			IRentalShoppingCart value = null;
			if (_rentalShoppingCarts.TryGetValue(id, out value))
			{
				result = _rentalShoppingCarts.Remove(id);
				LogHelper.Instance.Log("Removing Rental Shopping Cart with id: {0}", id);
			}
		}
		return result;
	}

	public void AddItemToRentalShoppingCart(Guid shoppingCartSessionId, IRentalShoppingCartItem newRentalShoppingCartItem)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			rentalShoppingCart.AddItem(newRentalShoppingCartItem);
			return;
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.AddItemToRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void AddMultiDiscDiscsToRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			{
				foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
				{
					dataService.GetProduct(item.ProductId, excludeTitleRollup: false, out var kioskProduct);
					if (kioskProduct == null || !(kioskProduct is MDVProduct mDVProduct))
					{
						continue;
					}
					foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
					{
						int? discNumber = titleProduct.DiscNumber;
						int num = 1;
						if (discNumber.GetValueOrDefault() > num && discNumber.HasValue && !rentalShoppingCart.HasItem(titleProduct.ProductId))
						{
							IRentalShoppingCartItem rentalShoppingCartItem = CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
							rentalShoppingCartItem.SourceType = ItemSourceType.Browse;
							rentalShoppingCartItem.Action = RentalShoppingCartItemAction.rental;
							rentalShoppingCartItem.Quantity = 1m;
							rentalShoppingCartItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(titleProduct);
							rentalShoppingCartItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(titleProduct);
							rentalShoppingCartItem.Tags = PersonalizationFunctions.GetTags(titleProduct);
							rentalShoppingCart.AddItem(rentalShoppingCartItem);
							LogHelper.Instance.Log("MDV - adding product id {0}", titleProduct.ProductId);
						}
					}
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.AddMultiDiscDiscsToCurrentRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemoveMultiDiscDiscsFromRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			List<IRentalShoppingCartItem> list = new List<IRentalShoppingCartItem>();
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				dataService.GetProduct(item.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null && kioskProduct is TitleProduct { DiscNumber: not null, DiscNumber: var discNumber } && discNumber > 1)
				{
					list.Add(item);
				}
			}
			{
				foreach (IRentalShoppingCartItem item2 in list)
				{
					LogHelper.Instance.Log("MDV - removing product id {0}", item2.ProductId);
					rentalShoppingCart.RemoveItem(item2, userRemoved: false);
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveMultiDiscDiscsFromRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemoveNonReservedProductsFromRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			List<IRentalShoppingCartItem> list = new List<IRentalShoppingCartItem>();
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				if (item.SourceType != ItemSourceType.Reservation)
				{
					list.Add(item);
				}
			}
			{
				foreach (IRentalShoppingCartItem item2 in list)
				{
					LogHelper.Instance.Log("removing non-reserved product id {0}", item2.ProductId);
					rentalShoppingCart.RemoveItem(item2, userRemoved: false);
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveNonReservedProductsFromRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemovePromoOfferFromCart(Guid shoppingSessionId)
	{
		IPromoOfferState promoOfferState = Redbox.Rental.Services.Session.Session.Instance.PromoOfferState;
		if (promoOfferState != null && promoOfferState.WasOfferAccepted && promoOfferState.SelectedOffer != null)
		{
			if (promoOfferState.SelectedProduct != null)
			{
				RemoveItemFromRentalShoppingCartByProductId(shoppingSessionId, promoOfferState.SelectedProduct.ProductId, userRemoved: true);
			}
			IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
			RemoveOfferDiscountsFromCart(shoppingSessionId, promoOfferState);
			service.ClearPromotion();
			promoOfferState.SelectedOffer = null;
			promoOfferState.SelectedProduct = null;
			promoOfferState.WasOfferAccepted = false;
		}
	}

	private void RemoveOfferDiscountsFromCart(Guid shoppingSessionId, IPromoOfferState offerState)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingSessionId);
		if (rentalShoppingCart == null)
		{
			return;
		}
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			if (offerState.SelectedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
			{
				RemoveMultiNightOfferFromItem(item);
			}
		}
	}

	public IRentalShoppingCartItem CreateRentalShoppingCartItemFromProductId(long productId)
	{
		IRentalShoppingCartItem result = null;
		(ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService).GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
		if (kioskProduct != null)
		{
			if (kioskProduct is ITitleProduct titleProduct)
			{
				result = CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId. Product with id {0} is not a title product", productId);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId. Unable to find product with id {0}", productId);
		}
		return result;
	}

	public IRentalShoppingCartItem CreateRentalShoppingCartItemFromTitleProduct(ITitleProduct titleProduct)
	{
		RentalShoppingCartItem result = null;
		if (titleProduct != null)
		{
			_ = titleProduct.RatingId ?? 0;
			_ = titleProduct is MDVProduct;
			result = new RentalShoppingCartItem(titleProduct);
			titleProduct?.Genres.ForEach(delegate(long x)
			{
				try
				{
					result.Genres.Add((Genres)x);
				}
				catch
				{
					LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromTitleProduct. Unable to cast value {0} as a Genre", x);
				}
			});
		}
		return result;
	}

	public void RemoveItemFromRentalShoppingCart(Guid shoppingCartSessionId, int index, bool userRemoved)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			if (index < rentalShoppingCart.Count)
			{
				IRentalShoppingCartItem item = rentalShoppingCart.GetItem(index);
				if (item != null)
				{
					RemoveItemFromRentalShoppingCart(rentalShoppingCart, item, userRemoved);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCart.  index is out of range");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
		}
	}

	public void RemoveItemFromRentalShoppingCartByProductId(Guid shoppingCartSessionId, long productId, bool userRemoved)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IRentalShoppingCartItem rentalShoppingCartItem = rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == productId);
			if (rentalShoppingCartItem != null)
			{
				RemoveItemFromRentalShoppingCart(rentalShoppingCart, rentalShoppingCartItem, userRemoved);
				return;
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCartByProductId.  Unable to find rental shopping cart item with product id {0}", productId);
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCartByProductId.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
		}
	}

	private void RemoveItemFromRentalShoppingCart(IRentalShoppingCart rentalShoppingCart, IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved)
	{
		if (rentalShoppingCart != null && rentalShoppingCartItem != null)
		{
			rentalShoppingCart.RemoveItem(rentalShoppingCartItem, userRemoved);
			rentalShoppingCart.ValidateAndUpdatePromoCode();
		}
	}

	public void SendRentalShoppingCartToAnalytics(IRentalShoppingCart rentalShoppingCart)
	{
		LogHelper.Instance.Log("Gather Up Cart Data for Analytics");
		decimal num = 0m;
		decimal num2 = 0m;
		decimal num3 = 0m;
		if (!rentalShoppingCart.PickupRecommendationsShown)
		{
			num = rentalShoppingCart.Totals.SubTotal;
			num2 = rentalShoppingCart.Totals.TaxAmount;
			num3 = rentalShoppingCart.Totals.GrandTotal;
		}
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddShoppingCart(rentalShoppingCart);
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service == null || !service.SessionExists(rentalShoppingCart.ShoppingSessionId.ToString()) || !service.IsOpen(rentalShoppingCart.ShoppingSessionId.ToString()))
		{
			return;
		}
		service.AddElement("shoppingCart", "subTotal", num.ToString(), rentalShoppingCart.ShoppingSessionId.ToString());
		service.AddElement("shoppingCart", "taxAmount", num2.ToString(), rentalShoppingCart.ShoppingSessionId.ToString());
		service.AddElement("shoppingCart", "grandTotal", num3.ToString(), rentalShoppingCart.ShoppingSessionId.ToString());
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			rentalShoppingCart.AddCartItemToAnalytics(item, userRemoved: false, service);
		}
	}

	public List<string> AssignBarcodesToVend(IRentalShoppingCart rentalShoppingCart)
	{
		LogHelper.Instance.Log("RentalShoppingCartService.AssignBarcodesToVend");
		List<string> list = new List<string>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
		if (rentalShoppingCart != null)
		{
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				list2.Add(new SearchBarcodeForProduct
				{
					ProductId = item.ProductId,
					Barcode = item.Barcode,
					Action = item.Action
				});
			}
			List<IBarcodeForProductResult> list3 = service?.GetBarcodesForProducts(list2, null);
			if (list3 != null)
			{
				foreach (IBarcodeForProductResult eachBarcodeForProductResult in list3)
				{
					if (eachBarcodeForProductResult.BarcodeAssigned)
					{
						IRentalShoppingCartItem rentalShoppingCartItem = rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == eachBarcodeForProductResult.ProductId && x.Action == eachBarcodeForProductResult.Action);
						if (rentalShoppingCartItem != null)
						{
							rentalShoppingCartItem.Barcode = eachBarcodeForProductResult.Barcode;
							LogHelper.Instance.Log($"Assigning barcode {eachBarcodeForProductResult.Barcode} to rental shopping cart item with product {eachBarcodeForProductResult.ProductId} and action {eachBarcodeForProductResult.Action}");
							if (eachBarcodeForProductResult.Status.HasValue)
							{
								rentalShoppingCartItem.Status = eachBarcodeForProductResult.Status.Value;
							}
						}
					}
					if (!string.IsNullOrEmpty(eachBarcodeForProductResult.Barcode))
					{
						list.Add(eachBarcodeForProductResult.Barcode);
					}
				}
			}
		}
		return list;
	}

	private IShoppingSession GetCurrentShoppingSession()
	{
		return ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession();
	}

	public void AddMultiNightOfferToItem(IRentalShoppingCartItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.UseDiscountsForPmnpOffers)
		{
			LogHelper.Instance.Log("The UseDiscountsForPmnpOffers config setting is true, creating a discount for the item");
			AddMultiNightDiscountPromo(rentalShoppingCartItem, selectedOffer);
		}
		else
		{
			LogHelper.Instance.Log("The UseDiscountsForPmnpOffers config setting is false, setting the mnp price for the item");
			SetMultiNightPriceForOffer(rentalShoppingCartItem, selectedOffer);
		}
	}

	private void AddMultiNightDiscountPromo(IRentalShoppingCartItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		IPromotion promotion = ServiceLocator.Instance.GetService<IPromotionService>().GetPromotion();
		if (promotion == null)
		{
			LogHelper.Instance.LogError("", "Unable to add Multi Night Offer to item when no Promotion has been assigned.");
			return;
		}
		OfferProduct offerProd = selectedOffer.OfferProducts.FirstOrDefault((OfferProduct o) => o.ProductId == rentalShoppingCartItem.ProductId);
		if (offerProd != null && offerProd.NumberOfNights.HasValue && offerProd.DiscountedPrice.HasValue && !promotion.PromotionItems.Any((IPromotionItem d) => d.RentalShoppingCartItem.ProductId == offerProd.ProductId))
		{
			rentalShoppingCartItem.PricingRecord.InitialDays = offerProd.NumberOfNights;
			decimal num = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value + rentalShoppingCartItem.PricingRecord.ExtraNight * (decimal)(offerProd.NumberOfNights.Value - 1);
			rentalShoppingCartItem.PricingRecord.InitialNight = num;
			rentalShoppingCartItem.Price = num;
			promotion.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartItem,
				PromotionAmount = rentalShoppingCartItem.PricingRecord.ExtraNight * (decimal)(offerProd.NumberOfNights.Value - 1) - offerProd.DiscountedPrice.Value
			});
			promotion.Price = promotion.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
	}

	private void SetMultiNightPriceForOffer(IRentalShoppingCartItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		OfferProduct offerProduct = selectedOffer?.OfferProducts?.FirstOrDefault((OfferProduct o) => o.ProductId == rentalShoppingCartItem.ProductId);
		if (offerProduct != null && offerProduct.NumberOfNights.HasValue && offerProduct.DiscountedPrice.HasValue)
		{
			IRentalShoppingCartItem rentalShoppingCartItem2 = rentalShoppingCartItem;
			if (rentalShoppingCartItem2 != null && rentalShoppingCartItem2.PricingRecord?.InitialDays == 1)
			{
				rentalShoppingCartItem.PricingRecord.InitialDays = offerProduct.NumberOfNights;
				decimal num = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value + offerProduct.DiscountedPrice.Value * (decimal)(offerProduct.NumberOfNights.Value - 1);
				rentalShoppingCartItem.PricingRecord.InitialNight = num;
				rentalShoppingCartItem.Price = num;
			}
		}
	}

	public void RemoveMultiNightOfferFromItem(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.UseDiscountsForPmnpOffers)
		{
			RemoveMultiNightDiscountPromo(rentalShoppingCartItem);
		}
		else
		{
			RemoveMultiNightPriceForOffer(rentalShoppingCartItem);
		}
	}

	private void RemoveMultiNightDiscountPromo(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		IPromotion promotion = ServiceLocator.Instance.GetService<IPromotionService>().GetPromotion();
		if (promotion == null)
		{
			return;
		}
		rentalShoppingCartItem.PricingRecord.InitialDays = 1;
		rentalShoppingCartItem.PricingRecord.InitialNight = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value;
		rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
		IPromotionItem promotionItem = promotion.PromotionItems.FirstOrDefault((IPromotionItem d) => d.RentalShoppingCartItem.ProductId == rentalShoppingCartItem.ProductId);
		if (promotionItem != null)
		{
			promotion.PromotionItems.Remove(promotionItem);
			promotion.Price = promotion.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
	}

	private void RemoveMultiNightPriceForOffer(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		rentalShoppingCartItem.PricingRecord.InitialDays = 1;
		rentalShoppingCartItem.PricingRecord.InitialNight = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value;
		rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
	}

	private RentalShoppingCartService()
	{
	}
}
