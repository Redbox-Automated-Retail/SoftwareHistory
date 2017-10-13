using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.ShoppingCart;

public class BlurayUpsellInfo : IBlurayUpsellInfo
{
	private IList<IBlurayUpsellItem> _items = new List<IBlurayUpsellItem>();

	private IRentalShoppingCart _rentalShoppingCart;

	public bool DoNotShowAgain { get; set; }

	public IList<IBlurayUpsellItem> Items => _items;

	public bool DialogNeedsToBeShown
	{
		get
		{
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
			bool value = false;
			service2?.TryGetValue<bool>("system", "General", "BluRayUpsellEnabled", out value);
			if (value && !DoNotShowAgain && CheckForInCartOffer() && (service == null || !service.TTSEnabled))
			{
				return CreateBlurayUpsellItems().Count > 0;
			}
			return false;
		}
	}

	public BlurayUpsellInfo(IRentalShoppingCart rentalShoppingCart)
	{
		_rentalShoppingCart = rentalShoppingCart;
	}

	public void AddMissingBlurayUpsellItems()
	{
		foreach (BlurayUpsellItem item in CreateBlurayUpsellItems())
		{
			_items.Add(item);
		}
	}

	public void RejectUndecidedUpsellItems()
	{
		_items?.ForEach(delegate(IBlurayUpsellItem x)
		{
			if (x.OfferResponse == BlurayUpsellItemOfferResponse.none)
			{
				x.OfferResponse = BlurayUpsellItemOfferResponse.rejected;
			}
		});
	}

	public void RemoveUndecidedUpsellItems()
	{
		List<IBlurayUpsellItem> itemsToRemove = new List<IBlurayUpsellItem>();
		_items?.Where((IBlurayUpsellItem x) => x.OfferResponse == BlurayUpsellItemOfferResponse.none).ForEach(delegate(IBlurayUpsellItem x)
		{
			itemsToRemove.Add(x);
		});
		itemsToRemove.ForEach(delegate(IBlurayUpsellItem x)
		{
			_items.Remove(x);
		});
	}

	public void SetUpsellOffer(long dvdProductId, BlurayUpsellItemOfferResponse offerResponse)
	{
		IBlurayUpsellItem blurayUpsellItem = _items.FirstOrDefault((IBlurayUpsellItem x) => x.DVDProductId == dvdProductId);
		if (blurayUpsellItem == null)
		{
			return;
		}
		blurayUpsellItem.OfferResponse = offerResponse;
		if (offerResponse == BlurayUpsellItemOfferResponse.accepted)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			IRentalShoppingCartItem rentalShoppingCartItem = _rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == blurayUpsellItem.DVDProductId);
			if (rentalShoppingCartItem != null)
			{
				_rentalShoppingCart.RemoveItem(rentalShoppingCartItem, userRemoved: false);
			}
			IRentalShoppingCartItem rentalShoppingCartItem2 = service.CreateRentalShoppingCartItemFromProductId(blurayUpsellItem.BlurayProductId);
			rentalShoppingCartItem2.Quantity = 1m;
			rentalShoppingCartItem2.Action = RentalShoppingCartItemAction.rental;
			rentalShoppingCartItem2.SourceType = ItemSourceType.BlurayUpsell;
			_rentalShoppingCart.AddItem(rentalShoppingCartItem2);
		}
	}

	private List<BlurayUpsellItem> CreateBlurayUpsellItems()
	{
		List<BlurayUpsellItem> list = new List<BlurayUpsellItem>();
		if (_rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			foreach (IRentalShoppingCartItem item in _rentalShoppingCart.Items.Where((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental && x.TitleType == TitleType.DVD && !_items.Any((IBlurayUpsellItem y) => y.DVDProductId == x.ProductId)))
			{
				dataService.GetProduct(item.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct == null || !(kioskProduct is ITitleProduct { TitleType: TitleType.DVD } titleProduct) || titleProduct.TitleRollupProductGroupId <= 0)
				{
					continue;
				}
				IKioskProduct kioskProduct2 = null;
				dataService?.GetTitleRollupProduct(titleProduct.TitleRollupProductGroupId, out kioskProduct2);
				if (kioskProduct2 is TitleRollupProduct titleRollupProduct)
				{
					ITitleProduct titleProduct2 = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray);
					if (titleProduct2 != null && titleProduct2.IsAvailable && titleProduct2.InStock && !_rentalShoppingCart.HasItem(titleProduct2.ProductId))
					{
						list.Add(new BlurayUpsellItem
						{
							DVDProductId = item.ProductId,
							BlurayProductId = titleProduct2.ProductId,
							OfferResponse = BlurayUpsellItemOfferResponse.none,
							ProductGroupId = titleProduct.TitleRollupProductGroupId
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
