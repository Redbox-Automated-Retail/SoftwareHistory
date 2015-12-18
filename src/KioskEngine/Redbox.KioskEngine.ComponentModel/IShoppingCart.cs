using System;
using System.Collections.Generic;
using System.Security;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingCart
{
	string Email { get; }

	string LastFour { get; }

	bool Blocklisted { get; }

	string LastName { get; }

	string FirstName { get; }

	SecureString TrackData { get; }

	string ExpiryYear { get; }

	string ExpiryMonth { get; }

	string PostalCode { get; }

	DateTime CreatedOn { get; }

	string DiscountCode { get; }

	long? ReferenceNumber { get; }

	decimal DiscountValue { get; }

	CreditElection UseCredits { get; }

	ShoppingCartType Type { get; }

	IDictionary<string, IShoppingCartLineItemGroup> Groups { get; }

	void SetDiscount(string code, decimal value);

	void ClearDiscount();

	void SetUseCredits(CreditElection value);

	void SetType(ShoppingCartType type);

	void SetEmail(string email);

	void SetPostalCode(string postalCode);

	bool SetAccountData();

	int GetLineItemCount();

	void ClearAccountData();

	void ClearTrackData();

	void RemoveLineItem(string barcode);

	void SetGroup(string groupName, decimal taxRate, decimal subTotal, decimal discountedSubtotal, decimal taxAmount, decimal grandTotal);

	void SetReferenceNumber(long referenceNumber);

	IShoppingCartLineItemGroup GetGroup(string name);

	void SetBlocklisted(bool value);
}
