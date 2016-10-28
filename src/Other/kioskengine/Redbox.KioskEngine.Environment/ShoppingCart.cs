using System;
using System.Collections.Generic;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCart : IShoppingCart
{
	private readonly IDictionary<string, IShoppingCartLineItemGroup> m_groups = new Dictionary<string, IShoppingCartLineItemGroup>();

	public string Email { get; internal set; }

	public bool Blocklisted { get; internal set; }

	public string LastFour { get; internal set; }

	public string LastName { get; internal set; }

	public string FirstName { get; internal set; }

	public SecureString TrackData { get; internal set; }

	public string ExpiryYear { get; internal set; }

	public string ExpiryMonth { get; internal set; }

	public string PostalCode { get; internal set; }

	public DateTime CreatedOn { get; internal set; }

	public long? ReferenceNumber { get; internal set; }

	public string DiscountCode { get; internal set; }

	public decimal DiscountValue { get; internal set; }

	public CreditElection UseCredits { get; internal set; }

	public ShoppingCartType Type { get; internal set; }

	public Guid ShoppingSessionId { get; private set; }

	public IDictionary<string, IShoppingCartLineItemGroup> Groups => m_groups;

	public bool AuthAcceptFlag { get; internal set; }

	public int AuthRuleId { get; internal set; }

	public decimal AuthAmount { get; internal set; }

	public bool SkipAuthRule { get; internal set; }

	public bool PlayPassPromptAccepted { get; internal set; }

	public int PlayPassPointsEarned { get; internal set; }

	public ShoppingCart(Guid shoppingSessionId)
	{
		CreatedOn = DateTime.Now;
		ShoppingSessionId = shoppingSessionId;
	}

	public void SetDiscount(string code, decimal value)
	{
		LogHelper.Instance.Log("SetDiscount on Shopping Cart for Shopping Session '{0}': code={1}, value={2}", ShoppingSessionId, code, value);
		DiscountCode = code;
		DiscountValue = value;
	}

	public void ClearDiscount()
	{
		LogHelper.Instance.Log("ClearDiscount on Shopping Cart for Shopping Session '{0}'", ShoppingSessionId);
		DiscountCode = null;
		DiscountValue = 0m;
	}

	public void SetBlocklisted(bool value)
	{
		LogHelper.Instance.Log("SetBlocklisted on Shopping Cart for Shopping Session '{0}': value={1}", ShoppingSessionId, value);
		Blocklisted = value;
	}

	public void SetUseCredits(CreditElection value)
	{
		LogHelper.Instance.Log("SetUseCredits on Shopping Cart for Shopping Session '{0}': value={1}", ShoppingSessionId, value);
		UseCredits = value;
	}

	public void SetType(ShoppingCartType type)
	{
		LogHelper.Instance.Log("SetType on Shopping Cart for Shopping Session '{0}': type={1}", ShoppingSessionId, type);
		Type = type;
	}

	public void SetEmail(string email)
	{
		LogHelper.Instance.Log("SetEmail on Shopping Cart for Shopping Session '{0}'", ShoppingSessionId);
		Email = email;
	}

	public bool SetAccountData()
	{
		LogHelper.Instance.Log("SetAccountData on Shopping Cart for Shopping Session '{0}'", ShoppingSessionId);
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service == null || !service.HasValidData())
		{
			LogHelper.Instance.Log("...No valid track data available.");
			return false;
		}
		TrackData = CardHelper.CopyToSecure(service.GetTrack2());
		LastName = service.GetLastName();
		LastFour = service.GetLastFour();
		FirstName = service.GetFirstName();
		ExpiryYear = service.GetExpiryYear();
		ExpiryMonth = service.GetExpiryMonth();
		LogHelper.Instance.Log("...Track data transferred to account data.");
		return true;
	}

	public void ClearAccountData()
	{
		TrackData.Clear();
		TrackData.Dispose();
		LastName = null;
		LastFour = null;
		FirstName = null;
		ExpiryYear = null;
		ExpiryMonth = null;
		LogHelper.Instance.Log("Account Data on Shopping Cart cleared for Shopping Session '{0}'", ShoppingSessionId);
	}

	public void ClearTrackData()
	{
		TrackData.Clear();
		TrackData.Dispose();
	}

	public IShoppingCartLineItemGroup GetGroup(string name)
	{
		LogHelper.Instance.Log("Get Group '{0}' on Shopping Cart for Shopping Session '{1}'", name, ShoppingSessionId);
		return Groups.ContainsKey(name) ? Groups[name] : null;
	}

	public void SetGroup(string groupName, decimal taxRate, decimal subTotal, decimal discountedSubtotal, decimal taxAmount, decimal grandTotal)
	{
		ShoppingCartLineItemGroup shoppingCartLineItemGroup;
		if (!Groups.ContainsKey(groupName))
		{
			shoppingCartLineItemGroup = new ShoppingCartLineItemGroup();
			Groups[groupName] = shoppingCartLineItemGroup;
		}
		else
		{
			shoppingCartLineItemGroup = (ShoppingCartLineItemGroup)Groups[groupName];
		}
		((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).TaxRate = taxRate;
		((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).SubTotal = subTotal;
		((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).TaxAmount = taxAmount;
		((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).GrandTotal = grandTotal;
		((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).DiscountedSubtotal = discountedSubtotal;
		LogHelper.Instance.Log("SetGroup on Shopping Cart for Shopping Session '{0}': taxRate={1}, subTotal={2}, taxAmount={3}, grandTotal={4}, discountedSubtotal={5}", ShoppingSessionId, taxRate, subTotal, taxAmount, grandTotal, discountedSubtotal);
	}

	public int GetLineItemCount()
	{
		int num = 0;
		foreach (IShoppingCartLineItemGroup value in Groups.Values)
		{
			num += value.Items.Count;
		}
		LogHelper.Instance.Log("GetLineItemCount on Shopping Cart for Shopping Session '{0}': {1}", ShoppingSessionId, num);
		return num;
	}

	public void RemoveLineItem(string barcode)
	{
		LogHelper.Instance.Log("Remove line item for barcode '{0}' on Shopping Cart for Shopping Session '{1}'.", barcode, ShoppingSessionId);
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in Groups)
		{
			foreach (IShoppingCartLineItem item in new List<IShoppingCartLineItem>(group.Value.Items))
			{
				if (string.Compare(item.Barcode, barcode, ignoreCase: true) == 0)
				{
					LogHelper.Instance.Log("...Removing line item for barcode '{0}'.", barcode);
					group.Value.Items.Remove(item);
				}
			}
			if (group.Value.Items.Count == 0)
			{
				LogHelper.Instance.Log("...Group '{0}' is empty, mark for removal.", group.Key);
				list.Add(group.Key);
			}
		}
		foreach (string item2 in list)
		{
			LogHelper.Instance.Log("...Remove group '{0}'.", item2);
			Groups.Remove(item2);
		}
	}

	public void SetPostalCode(string postalCode)
	{
		PostalCode = postalCode;
		LogHelper.Instance.Log("Set Postal Code on Shopping Cart for Shopping Session '{0}': postalCode={1}", ShoppingSessionId, postalCode);
	}

	public void SetReferenceNumber(long referenceNumber)
	{
		ReferenceNumber = referenceNumber;
		LogHelper.Instance.Log("Set Reference Number on Shopping Cart for Shopping Session '{0}': referenceNumber={1}", ShoppingSessionId, referenceNumber);
	}

	public void SetVariableAuthValues(bool authAcceptFlag, int authRuleId, decimal authAmount, bool skipAuthRule)
	{
		LogHelper.Instance.Log("SetVariableAuthValues on Shopping Cart for Shopping Session '{0}': authAcceptFlag={1}, authRuleId={2}, authAmount={3}, skipAuthRule={4}", ShoppingSessionId, authAcceptFlag, authRuleId, authAmount, skipAuthRule);
		AuthAcceptFlag = authAcceptFlag;
		AuthRuleId = authRuleId;
		AuthAmount = authAmount;
		SkipAuthRule = skipAuthRule;
	}

	public void SetPlayPassPromptAccepted(bool playPassPromptAccepted, int playPassPointsEarned)
	{
		LogHelper.Instance.Log("SetPlayPassPromptAccepted on Shopping Cart for Shopping Session '{0}': playPassPromptAccepted={1}, playPassPointsEarned={2}", ShoppingSessionId, playPassPromptAccepted, playPassPointsEarned);
		PlayPassPromptAccepted = playPassPromptAccepted;
		PlayPassPointsEarned = playPassPointsEarned;
	}
}
