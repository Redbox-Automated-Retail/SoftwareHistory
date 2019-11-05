using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartService
{
	IRentalShoppingCart CurrentRentalShoppingCart { get; }

	bool CurrentRentalShoppingCartIsFull { get; }

	event RentalShoppingCartAdded OnRentalShoppingCartAdded;

	IRentalShoppingCart GetRentalShoppingCart(Guid id);

	IRentalShoppingCart CreateNew(Guid id);

	bool RemoveRentalShoppingCart(Guid id);

	void AddItemToRentalShoppingCart(Guid shoppingCartSessionId, IRentalShoppingCartItem rentalShoppingCartItem);

	void RemoveItemFromRentalShoppingCart(Guid id, int index, bool userRemoved);

	void RemoveItemFromRentalShoppingCartByProductId(Guid shoppingCartSessionId, long productId, bool userRemoved);

	void AddMultiDiscDiscsToRentalShoppingCart(Guid shoppingCartSessionId);

	void RemoveMultiDiscDiscsFromRentalShoppingCart(Guid shoppingCartSessionId);

	void RemoveNonReservedProductsFromRentalShoppingCart(Guid shoppingCartSessionId);

	IRentalShoppingCartItem CreateRentalShoppingCartItemFromTitleProduct(ITitleProduct titleProduct);

	IRentalShoppingCartItem CreateRentalShoppingCartItemFromProductId(long productId);

	void SendRentalShoppingCartToAnalytics(IRentalShoppingCart rentalShoppingCart);

	List<string> AssignBarcodesToVend(IRentalShoppingCart rentalShoppingCart);

	void RemovePromoOfferFromCart(Guid shoppingSessionId);

	void AddMultiNightOfferToItem(IRentalShoppingCartItem rentalShoppingCartItem, CustomerOffer selectedOffer);

	void RemoveMultiNightOfferFromItem(IRentalShoppingCartItem rentalShoppingCartItem);
}
