using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ShoppingSessionFunctions
{
	internal const string LegacyActiveSessionId = "__active__";

	[KernelFunction(Name = "kernel.sessionstartnew")]
	internal static void StartNewSession(string storeNumber)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.StartNewSession(storeNumber, "__active__");
	}

	[KernelFunction(Name = "kernel.sessionabandon")]
	internal static void AbandonSession(string reason)
	{
		AbandonSession("__active__", reason);
	}

	[KernelFunction(Name = "kernel.sessioncommit")]
	internal static void CommitSession(string reason)
	{
		CommitSession("__active__", reason);
	}

	[KernelFunction(Name = "kernel.sessionaddevent")]
	internal static void AddNewEvent(string type, string description)
	{
		AddNewEvent("__active__", type, description);
	}

	[KernelFunction(Name = "kernel.sessionsetdiscount")]
	internal static void SetDiscount(string code, decimal value)
	{
		SetDiscount("__active__", code, value);
	}

	[KernelFunction(Name = "kernel.sessioncleardiscount")]
	internal static void ClearDiscount()
	{
		ClearDiscount("__active__");
	}

	[KernelFunction(Name = "kernel.sessioncaptureaccountdata")]
	internal static bool CaptureAccountData()
	{
		return CaptureAccountData("__active__");
	}

	[KernelFunction(Name = "kernel.sessionsetpostalcode")]
	internal static void SetPostalCode(string postalCode)
	{
		SetPostalCode("__active__", postalCode);
	}

	[KernelFunction(Name = "kernel.sessionsetemail")]
	internal static void SetEmail(string email)
	{
		SetEmail("__active__", email);
	}

	[KernelFunction(Name = "kernel.sessionaddlineitemgroup")]
	internal static void AddLineItemGroup(string groupName, decimal taxRate, decimal subTotal, decimal discountedSubtotal, decimal taxAmount, decimal grandTotal)
	{
		AddLineItemGroup("__active__", groupName, taxRate, subTotal, discountedSubtotal, taxAmount, grandTotal);
	}

	[KernelFunction(Name = "kernel.sessionaddlineitem")]
	internal static void AddLineItem(string groupName, string barcode, string status, decimal price, decimal quantity, LuaTable prices)
	{
		AddLineItem("__active__", groupName, barcode, status, price, quantity, prices);
	}

	[KernelFunction(Name = "kernel.sessionaddlineitem2")]
	internal static void AddLineItem(string sessionId, string groupName, string barcode, string status, decimal price, decimal quantity, LuaTable prices)
	{
		AddLineItem(sessionId, groupName, barcode, status, price, quantity, prices, string.Empty, string.Empty, ItemSourceType.Browse.ToString());
	}

	[KernelFunction(Name = "kernel.sessionsave")]
	internal static void SaveSession()
	{
		SaveSession("__active__");
	}

	[KernelFunction(Name = "kernel.sessionsetlineitemstatus")]
	internal static void SetLineItemStatus(string barcode, string status)
	{
		SetLineItemStatus("__active__", barcode, status);
	}

	[KernelFunction(Name = "kernel.sessionremovelineitem")]
	internal static void RemoveLineItem(string barcode)
	{
		RemoveLineItem("__active__", barcode);
	}

	[KernelFunction(Name = "kernel.sessionend")]
	internal static void SessionEnd()
	{
		SessionEnd("__active__");
	}

	[KernelFunction(Name = "kernel.sessionsetcarttype")]
	internal static void SessionSetCartType(string type)
	{
		SessionSetCartType("__active__", type);
	}

	[KernelFunction(Name = "kernel.sessionstartnew2")]
	internal static string StartNewSession2(string storeNumber)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		string result = null;
		if (service != null)
		{
			result = service.StartNewSession(storeNumber, null);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.sessionabandon2")]
	internal static void AbandonSession(string sessionId, string reason)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.Abandon(reason);
	}

	[KernelFunction(Name = "kernel.sessioncommit2")]
	internal static void CommitSession(string sessionId, string reason)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.Commit(reason);
	}

	[KernelFunction(Name = "kernel.sessionaddevent2")]
	internal static void AddNewEvent(string sessionId, string type, string description)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.AddEvent(Enum<ShoppingSessionEventType>.ParseIgnoringCase(type, ShoppingSessionEventType.Commit), description);
	}

	[KernelFunction(Name = "kernel.sessionadderror")]
	internal static void AddNewError(string sessionId, string code, string description, string details)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.AddError(code, description, details);
	}

	[KernelFunction(Name = "kernel.sessionaddwarning")]
	internal static void AddNewWarning(string sessionId, string code, string description, string details)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.AddWarning(code, description, details);
	}

	[KernelFunction(Name = "kernel.sessionsetdiscount2")]
	internal static void SetDiscount(string sessionId, string code, decimal value)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetDiscount(code, value);
	}

	[KernelFunction(Name = "kernel.sessioncleardiscount2")]
	internal static void ClearDiscount(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.ClearDiscount();
	}

	[KernelFunction(Name = "kernel.sessionsetusecredits")]
	internal static void SetUseCredits(string sessionId, CreditElection value)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetUseCredits(value);
	}

	[KernelFunction(Name = "kernel.sessioncaptureaccountdata2")]
	internal static bool CaptureAccountData(string sessionId)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return false;
		}
		return service.GetSession(sessionId)?.ShoppingCart.SetAccountData() ?? false;
	}

	[KernelFunction(Name = "kernel.sessionsetpostalcode2")]
	internal static void SetPostalCode(string sessionId, string postalCode)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetPostalCode(postalCode);
	}

	[KernelFunction(Name = "kernel.sessionsetemail2")]
	internal static void SetEmail(string sessionId, string email)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetEmail(email);
	}

	[KernelFunction(Name = "kernel.sessionaddlineitemgroup2")]
	internal static void AddLineItemGroup(string sessionId, string groupName, decimal taxRate, decimal subTotal, decimal discountedSubtotal, decimal taxAmount, decimal grandTotal)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetGroup(groupName, taxRate, subTotal, discountedSubtotal, taxAmount, grandTotal);
	}

	[KernelFunction(Name = "kernel.sessionaddlineitem3")]
	internal static void AddLineItem(string sessionId, string groupName, string barcode, string status, decimal price, decimal quantity, LuaTable prices, string productFamily, string productType)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return;
		}
		IShoppingSession session = service.GetSession(sessionId);
		if (session == null)
		{
			return;
		}
		IShoppingCartLineItemGroup shoppingCartLineItemGroup = session.ShoppingCart.GetGroup(groupName);
		if (shoppingCartLineItemGroup == null)
		{
			return;
		}
		IShoppingCartLineItem shoppingCartLineItem = shoppingCartLineItemGroup.AddLineItem(barcode, Enum<LineItemStatus>.ParseIgnoringCase(status, LineItemStatus.Pending), price, quantity, productFamily, productType, ItemSourceType.Browse);
		if (shoppingCartLineItem == null || prices == null)
		{
			return;
		}
		foreach (object key in prices.Keys)
		{
			shoppingCartLineItem.SetPrice(key.ToString(), Convert.ToDecimal(prices[key]));
		}
		prices.Dispose();
	}

	[KernelFunction(Name = "kernel.sessionaddlineitem4")]
	internal static void AddLineItem(string sessionId, string groupName, string barcode, string status, decimal price, decimal quantity, LuaTable prices, string productFamily, string productType, string sourceType)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return;
		}
		IShoppingSession session = service.GetSession(sessionId);
		if (session == null)
		{
			return;
		}
		IShoppingCartLineItemGroup shoppingCartLineItemGroup = session.ShoppingCart.GetGroup(groupName);
		if (shoppingCartLineItemGroup == null)
		{
			return;
		}
		IShoppingCartLineItem shoppingCartLineItem = shoppingCartLineItemGroup.AddLineItem(barcode, Enum<LineItemStatus>.ParseIgnoringCase(status, LineItemStatus.Pending), price, quantity, productFamily, productType, Enum<ItemSourceType>.ParseIgnoringCase(sourceType, ItemSourceType.Browse));
		if (shoppingCartLineItem == null || prices == null)
		{
			return;
		}
		foreach (object key in prices.Keys)
		{
			shoppingCartLineItem.SetPrice(key.ToString(), Convert.ToDecimal(prices[key]));
		}
		prices.Dispose();
	}

	[KernelFunction(Name = "kernel.sessionsave2")]
	internal static void SaveSession(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.Save();
	}

	[KernelFunction(Name = "kernel.sessionsetlineitemstatus2")]
	internal static void SetLineItemStatus(string sessionId, string barcode, string status)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return;
		}
		IShoppingSession session = service.GetSession(sessionId);
		if (session == null)
		{
			return;
		}
		foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in session.ShoppingCart.Groups)
		{
			foreach (IShoppingCartLineItem item in group.Value.Items)
			{
				if (string.Compare(item.Barcode, barcode, ignoreCase: true) == 0)
				{
					item.SetStatus(Enum<LineItemStatus>.ParseIgnoringCase(status, LineItemStatus.NotFullfilled));
				}
			}
		}
	}

	[KernelFunction(Name = "kernel.sessionremovelineitem2")]
	internal static void RemoveLineItem(string sessionId, string barcode)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.RemoveLineItem(barcode);
	}

	[KernelFunction(Name = "kernel.sessionend2")]
	internal static void SessionEnd(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.End();
	}

	[KernelFunction(Name = "kernel.sessionsetcarttype2")]
	internal static void SessionSetCartType(string sessionId, string type)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetType(Enum<ShoppingCartType>.ParseIgnoringCase(type, ShoppingCartType.Online));
	}

	[KernelFunction(Name = "kernel.sessionsetcurrent")]
	internal static void SetCurrentSessionId(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.SetCurrentSession(sessionId);
	}

	[KernelFunction(Name = "kernel.sessiongetcurrent")]
	internal static string GetCurrentSessionId()
	{
		return ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession()?.Id.ToString();
	}

	[KernelFunction(Name = "kernel.sessiongetstatus")]
	internal static string GetSessionStatus(string sessionId)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return null;
		}
		IShoppingSession session = service.GetSession(sessionId);
		if (session == null)
		{
			return null;
		}
		return session.Status.ToString();
	}
}
