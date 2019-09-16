using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class AfterSwipeOfferService : IAfterSwipeOfferService
{
	private static AfterSwipeOfferService _instance;

	private Action<bool> _continueAction;

	private List<CustomerOffer> _offerList;

	private ITitleProduct _newProduct;

	private IPromotion _previousPromotion;

	private CustomerOffer _selectedOffer;

	public static IAfterSwipeOfferService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new AfterSwipeOfferService();
			}
			return _instance;
		}
	}

	private AfterSwipeOfferService()
	{
	}

	public void StartOfferFlow(Action<bool> callbackAction)
	{
		LogHelper.Instance.Log("AfterSwipeOfferService: StartOfferFlow called.");
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ShowAfterSwipePersonalizedOffers)
		{
			OrchestrationService.ViewShow("please_wait_view", null);
			_continueAction = callbackAction;
			IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
			_previousPromotion = service.GetPromotion();
			_newProduct = null;
			Redbox.Rental.Services.Session.Session.Instance.PromoOfferState = new PromoOfferState();
			ICustomerOfferService service2 = ServiceLocator.Instance.GetService<ICustomerOfferService>();
			IStoreManager service3 = ServiceLocator.Instance.GetService<IStoreManager>();
			IRentalShoppingCartService service4 = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			int num = (int)(service3?.KioskId ?? 0);
			service2.GetOffers(languageCode: OrchestrationService.GetCulture()?.Substring(0, 2), kioskId: num.ToString(), storefrontEventType: StorefrontEventType.OnSwipe.ToString(), sessionId: service4.CurrentRentalShoppingCart.ShoppingSessionId.ToString(), completeCallback: ProcessCustomerOffers);
		}
		else
		{
			LogHelper.Instance.Log("AfterSwipeOfferService: ShowAfterSwipePersonalizedOffers config is false, not getting offers.");
			callbackAction(obj: true);
		}
	}

	private void ProcessCustomerOffers(IRemoteServiceResult result)
	{
		if (result.Success)
		{
			try
			{
				List<CustomerOffer> property = result.GetProperty<List<CustomerOffer>>("NewOffers");
				List<CustomerOffer> property2 = result.GetProperty<List<CustomerOffer>>("StoredOffers");
				_offerList = new List<CustomerOffer>();
				if ((property?.Count ?? 0) > 0)
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: Found {property.Count} new offers");
					AddOffersToOfferList(property);
				}
				if ((property2?.Count ?? 0) > 0)
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: Found {property2.Count} stored offers");
					AddOffersToOfferList(property2);
				}
				if (_offerList.Count > 0)
				{
					PresentOffersViewInfo parameter = new PresentOffersViewInfo
					{
						CustomerOffers = _offerList,
						CancelAction = ContinueWithOriginalCart,
						ContinueAction = ContinueWithSelectedOffer,
						TimeoutAction = ContinueWithOriginalCart
					};
					Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferPresented = true;
					OrchestrationService.ViewShow("PresentOffersView", parameter);
					return;
				}
				LogHelper.Instance.Log("AfterSwipeOfferService: No applicable titles found for returned offers.");
				if ((property == null || property.Count == 0) && (property2 == null || property2.Count == 0))
				{
					LogHelper.Instance.Log("AfterSwipeOfferService: No Customer offers returned.");
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("AfterSwipeOfferService: Error processing offers", e);
			}
		}
		else
		{
			LogHelper.Instance.Log("AfterSwipeOfferService: Unable to retrieve customer offers.");
			if (result.Errors != null)
			{
				foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
				{
					LogHelper.Instance.Log($"{error.Code}: {error.Description} - {error.Details}");
				}
			}
		}
		_continueAction(obj: true);
	}

	private void AddOffersToOfferList(List<CustomerOffer> newOffers)
	{
		foreach (CustomerOffer newOffer in newOffers)
		{
			if (newOffer.CustomerSegmentType == CustomerSegmentType.BasketStretch)
			{
				SetApplicableProducts(newOffer);
				if (newOffer.ApplicableProducts.Count > 0)
				{
					_offerList.Add(newOffer);
				}
			}
			else
			{
				_offerList.Add(newOffer);
			}
		}
	}

	private void ContinueWithSelectedOffer(CustomerOffer offer)
	{
		_selectedOffer = offer;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer = offer;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = true;
		if (offer.CustomerSegmentType == CustomerSegmentType.BasketStretch)
		{
			SelectProductForOffer(offer);
		}
		else if (offer.OfferScopeType != OfferScopeType.Storeable)
		{
			CreateAndShowNewCart(null);
		}
		else
		{
			ContinueWithOriginalCart();
		}
	}

	private void SelectProductForOffer(CustomerOffer customerOffer)
	{
		string offerBrowseTitle = GetOfferBrowseTitle(customerOffer);
		PromoOfferBrowseInfo promoOfferBrowseInfo = new PromoOfferBrowseInfo();
		promoOfferBrowseInfo.applicableProducts = customerOffer.ApplicableProducts;
		promoOfferBrowseInfo.CancelAction = ContinueWithOriginalCart;
		promoOfferBrowseInfo.ContinueAction = CreateAndShowNewCart;
		promoOfferBrowseInfo.TimeoutAction = ContinueWithOriginalCart;
		promoOfferBrowseInfo.OfferTitleText = offerBrowseTitle;
		promoOfferBrowseInfo.DiscountedPriceCalc = CalcPromoPrice;
		promoOfferBrowseInfo.AllowMnp = false;
		OrchestrationService.ViewShow("promo_offer_browse_view", promoOfferBrowseInfo);
	}

	private string GetOfferBrowseTitle(CustomerOffer customerOffer)
	{
		string singularTitleFamilyStringFromOffer = CustomerOfferFunctions.GetSingularTitleFamilyStringFromOffer(customerOffer);
		return customerOffer.DiscountType switch
		{
			OfferDiscountType.AmountOff => string.Format(Resources.promo_amount_off_browse_title_text, singularTitleFamilyStringFromOffer, customerOffer.Amount.ToString("#.00")), 
			OfferDiscountType.PercentOff => string.Format(Resources.promo_percent_off_browse_title_text, singularTitleFamilyStringFromOffer, customerOffer.Amount.ToString("##")), 
			OfferDiscountType.Free => string.Format(Resources.promo_free_rental_offer_browse_title_text, singularTitleFamilyStringFromOffer), 
			_ => string.Empty, 
		};
	}

	private void AddNewProductToCart(ITitleProduct newProduct)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		service.RemoveMultiDiscDiscsFromRentalShoppingCart(service.CurrentRentalShoppingCart.ShoppingSessionId);
		IRentalShoppingCart currentRentalShoppingCart = service.CurrentRentalShoppingCart;
		IRentalShoppingCartItem rentalShoppingCartItem = service.CreateRentalShoppingCartItemFromTitleProduct(newProduct);
		rentalShoppingCartItem.SourceType = ItemSourceType.Browse;
		rentalShoppingCartItem.Action = RentalShoppingCartItemAction.rental;
		rentalShoppingCartItem.Quantity = 1m;
		rentalShoppingCartItem.Status = LineItemStatus.Pending;
		rentalShoppingCartItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(newProduct);
		currentRentalShoppingCart.AddItem(rentalShoppingCartItem);
		_newProduct = newProduct;
	}

	private void CreateAndShowNewCart(ITitleProduct newProduct)
	{
		_newProduct = newProduct;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedProduct = newProduct;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		service.RemoveMultiDiscDiscsFromRentalShoppingCart(service.CurrentRentalShoppingCart.ShoppingSessionId);
		SetPromoForSelectedOffer(newProduct, service.CurrentRentalShoppingCart);
		NewCartConfirmviewParameters parameter = new NewCartConfirmviewParameters
		{
			NewCart = service.CurrentRentalShoppingCart,
			promoProduct = newProduct,
			CancelAction = ContinueWithOriginalCart,
			ContinueAction = ContinueWithCurrentCart,
			TimeoutAction = ContinueWithOriginalCart
		};
		OrchestrationService.ViewShow("new_cart_confirm_view", parameter);
	}

	private void SetPromoForSelectedOffer(ITitleProduct newProduct, IRentalShoppingCart shoppingCart)
	{
		IRentalShoppingCartItem applyToLineItem = shoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem i) => i.ProductId == newProduct?.ProductId);
		ServiceLocator.Instance.GetService<IPromotionService>().SetPromoFromCustomerOffer(shoppingCart, _selectedOffer, applyToLineItem);
	}

	private decimal CalcPromoAmount(decimal? itemPrice)
	{
		decimal num = 0m;
		if (itemPrice.HasValue)
		{
			if (_selectedOffer.DiscountType == OfferDiscountType.AmountOff)
			{
				num = _selectedOffer.Amount;
			}
			else if (_selectedOffer.DiscountType == OfferDiscountType.PercentOff)
			{
				num = itemPrice.Value * (_selectedOffer.Amount / 100m);
			}
			else if (_selectedOffer.DiscountType == OfferDiscountType.Free)
			{
				num = itemPrice.Value;
			}
			num = Math.Min(num, itemPrice.Value);
		}
		return num;
	}

	private decimal CalcPromoPrice(decimal itemPrice)
	{
		return itemPrice - CalcPromoAmount(itemPrice);
	}

	private void SetApplicableProducts(CustomerOffer offer)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<ITitleProduct> products = null;
		List<long> include = null;
		List<long> exclude = null;
		if (offer.IncludeTitles)
		{
			include = offer.TitleIds;
		}
		else
		{
			exclude = offer.TitleIds;
		}
		ErrorList errorList = service.GetProductsForCustomerOffer(titleTypes: CustomerOfferFunctions.GetTitleTypesFromFormatIds(offer.FormatIds), titleFamily: offer.TitleFamily ?? TitleFamily.All, includeTitleTypes: offer.IncludeFormats, include: include, exclude: exclude, includeOlderThan: offer.IncludeOlderThan, isAdaView: ApplicationState.Instance.IsADAAccessible, products: out products);
		if (errorList.Count > 0)
		{
			errorList.ToLogHelper();
		}
		else if (products == null)
		{
			LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetProducts return null");
		}
		else
		{
			offer.ApplicableProducts = products;
		}
	}

	private void ContinueWithCurrentCart()
	{
		_offerList = null;
		if (_selectedOffer != null && _selectedOffer.PromotionCode != null)
		{
			PromotionFunctions.AddPromoCodeToAnalytics(_selectedOffer.PromotionCode);
		}
		_continueAction(obj: true);
	}

	private void ContinueWithOriginalCart()
	{
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = false;
		_offerList = null;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (_newProduct != null)
		{
			service.RemoveItemFromRentalShoppingCartByProductId(service.CurrentRentalShoppingCart.ShoppingSessionId, _newProduct.ProductId, userRemoved: true);
			_newProduct = null;
		}
		ServiceLocator.Instance.GetService<IPromotionService>().SetPromotion(_previousPromotion);
		_continueAction(obj: true);
	}

	private List<ITitleProduct> GetProductsForOffer()
	{
		return null;
	}
}
