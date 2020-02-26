using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.ShoppingCart;

public class UpsellInfo : IUpsellInfo
{
	private IList<IUpsellItem> _items = new List<IUpsellItem>();

	private IRentalShoppingCart _rentalShoppingCart;

	private Dictionary<TitleType, bool> _doNotShowAgain = new Dictionary<TitleType, bool>();

	public IList<IUpsellItem> Items => _items;

	public UpsellInfo(IRentalShoppingCart rentalShoppingCart)
	{
		_rentalShoppingCart = rentalShoppingCart;
	}

	public bool GetDoNotShowAgain(TitleType titleType)
	{
		bool result = false;
		if (_doNotShowAgain.ContainsKey(titleType))
		{
			result = _doNotShowAgain[titleType];
		}
		return result;
	}

	public void SetDoNotShowAgain(TitleType titleType, bool value)
	{
		_doNotShowAgain[titleType] = value;
	}

	public bool GetDialogNeedsToBeShown(TitleType titleType)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (GetUpsellEnabled(titleType) && !GetDoNotShowAgain(titleType) && CheckForInCartOffer() && (service == null || !service.TTSEnabled))
		{
			return CreateUpsellItems(titleType, GetUpsellTitleType(titleType)).Count > 0;
		}
		return false;
	}

	public void AddMissingUpsellItems(TitleType originalTitleType)
	{
		foreach (UpsellItem item in CreateUpsellItems(originalTitleType, GetUpsellTitleType(originalTitleType)))
		{
			_items.Add(item);
		}
	}

	private bool GetUpsellEnabled(TitleType titleType)
	{
		bool result = false;
		switch (titleType)
		{
		case TitleType.DVD:
			result = Redbox.Rental.Services.Configuration.Configuration.Instance.BlurayUpsellEnabled;
			break;
		case TitleType.Bluray:
			result = Redbox.Rental.Services.Configuration.Configuration.Instance._4KUpsellEnabled;
			break;
		}
		return result;
	}

	private TitleType GetUpsellTitleType(TitleType originalTitleType)
	{
		if (originalTitleType != TitleType.Bluray)
		{
			return TitleType.Bluray;
		}
		return TitleType._4KUHD;
	}

	public void RejectUndecidedUpsellItems()
	{
		_items?.ForEach(delegate(IUpsellItem x)
		{
			if (x.OfferResponse == UpsellItemOfferResponse.none)
			{
				x.OfferResponse = UpsellItemOfferResponse.rejected;
			}
		});
	}

	public void RemoveUndecidedUpsellItems()
	{
		List<IUpsellItem> itemsToRemove = new List<IUpsellItem>();
		_items?.Where((IUpsellItem x) => x.OfferResponse == UpsellItemOfferResponse.none).ForEach(delegate(IUpsellItem x)
		{
			itemsToRemove.Add(x);
		});
		itemsToRemove.ForEach(delegate(IUpsellItem x)
		{
			_items.Remove(x);
		});
	}

	public void SetUpsellOffer(long originalProductId, UpsellItemOfferResponse offerResponse)
	{
		IUpsellItem upsellItem = _items.FirstOrDefault((IUpsellItem x) => x.OriginalProductId == originalProductId);
		if (upsellItem == null)
		{
			return;
		}
		upsellItem.OfferResponse = offerResponse;
		if (offerResponse == UpsellItemOfferResponse.accepted)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			IRentalShoppingCartItem rentalShoppingCartItem = _rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == upsellItem.OriginalProductId);
			if (rentalShoppingCartItem != null)
			{
				_rentalShoppingCart.RemoveItem(rentalShoppingCartItem, userRemoved: false);
			}
			IRentalShoppingCartItem rentalShoppingCartItem2 = service.CreateRentalShoppingCartItemFromProductId(upsellItem.UpsellProductId);
			rentalShoppingCartItem2.Quantity = 1m;
			rentalShoppingCartItem2.Action = RentalShoppingCartItemAction.rental;
			rentalShoppingCartItem2.SourceType = GetUpsellItemSourceType(rentalShoppingCartItem2.TitleType);
			rentalShoppingCartItem2.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(rentalShoppingCartItem2.TitleProduct);
			rentalShoppingCartItem2.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(rentalShoppingCartItem2.TitleProduct);
			rentalShoppingCartItem2.PersonalizationTitleTags = PersonalizationFunctions.GetPersonalizationTitleTags(rentalShoppingCartItem2.TitleProduct);
			_rentalShoppingCart.AddItem(rentalShoppingCartItem2);
		}
	}

	public IUpsellItem GetUpsellItemByRentalShoppingCartItem(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		IUpsellItem result = null;
		if (rentalShoppingCartItem != null)
		{
			foreach (IUpsellItem item in _items)
			{
				if ((item.OriginalProductId == rentalShoppingCartItem.ProductId && item.OfferResponse != UpsellItemOfferResponse.accepted) || (item.UpsellProductId == rentalShoppingCartItem.ProductId && item.OfferResponse == UpsellItemOfferResponse.accepted))
				{
					result = item;
					break;
				}
			}
		}
		return result;
	}

	private ItemSourceType GetUpsellItemSourceType(TitleType upsellTitleType)
	{
		if (upsellTitleType != TitleType._4KUHD)
		{
			return ItemSourceType.BlurayUpsell;
		}
		return ItemSourceType._4kUpsell;
	}

	private List<UpsellItem> CreateUpsellItems(TitleType originalTitleType, TitleType upsellTitleType1)
	{
		List<UpsellItem> list = new List<UpsellItem>();
		if (_rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			foreach (IRentalShoppingCartItem item in _rentalShoppingCart.Items.Where(delegate(IRentalShoppingCartItem x)
			{
				if (x.Action == RentalShoppingCartItemAction.rental && x.TitleType == originalTitleType)
				{
					IPricingRecord pricingRecord = x.PricingRecord;
					if (pricingRecord == null || !pricingRecord.IsMultiNightPrice)
					{
						return !_items.Any((IUpsellItem y) => y.MatchesOrginalOrUpsellProductId(x.ProductId));
					}
				}
				return false;
			}))
			{
				dataService.GetProduct(item.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct == null || !(kioskProduct is ITitleProduct titleProduct) || titleProduct.TitleType != originalTitleType || titleProduct.TitleRollupProductGroupId <= 0)
				{
					continue;
				}
				IKioskProduct kioskProduct2 = null;
				dataService?.GetTitleRollupProduct(titleProduct.TitleRollupProductGroupId, out kioskProduct2);
				if (kioskProduct2 is TitleRollupProduct titleRollupProduct)
				{
					ITitleProduct titleProduct2 = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == upsellTitleType1);
					if (titleProduct2 != null && titleProduct2.IsAvailable && titleProduct2.InStock && !_rentalShoppingCart.HasItem(titleProduct2.ProductId))
					{
						list.Add(new UpsellItem
						{
							OriginalProductId = item.ProductId,
							UpsellProductId = titleProduct2.ProductId,
							OfferResponse = UpsellItemOfferResponse.none,
							ProductGroupId = titleProduct.TitleRollupProductGroupId,
							TypeId = ((upsellTitleType1 == TitleType.Bluray) ? UpsellItemType.BluRay : UpsellItemType._4KUHD)
						});
					}
				}
			}
		}
		return list;
	}

	private bool CheckForInCartOffer()
	{
		bool num = ApplicationState.Instance.InCartTemplateName != null && ApplicationState.Instance.InCartTemplateName != "Default-No-Offer";
		bool flag = ApplicationState.Instance.InCartTemplateName == "ROGO GAME B" || ApplicationState.Instance.InCartTemplateName == "ROGO GAME FREE G" || ApplicationState.Instance.InCartTemplateName == "ROGO GAME FREE INCLUDE H";
		return !num | flag;
	}
}
