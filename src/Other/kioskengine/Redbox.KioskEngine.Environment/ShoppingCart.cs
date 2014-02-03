using System;
using System.Collections.Generic;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCart : IShoppingCart
{
	private readonly IDictionary<string, IShoppingCartLineItemGroup> m_groups = new Dictionary<string, IShoppingCartLineItemGroup>();

	public string Email { get; internal set; }

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

	public ShoppingCart(Guid shoppingSessionId)
	{
		CreatedOn = DateTime.Now;
		ShoppingSessionId = shoppingSessionId;
	}

	public static IShoppingCart Load(Guid shoppingSessionId)
	{
		ShoppingCart shoppingCart = null;
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "SELECT TOP 1 * FROM ShoppingCart WHERE SessionID = @ID ORDER BY ID DESC";
			vistaDBCommand.Parameters.Add("@ID", shoppingSessionId);
			int? num = null;
			using (VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader())
			{
				while (vistaDBDataReader.Read())
				{
					num = (int)vistaDBDataReader["ID"];
					ShoppingCart shoppingCart2 = new ShoppingCart(shoppingSessionId);
					shoppingCart2.FirstName = (string)vistaDBDataReader["FirstName"];
					shoppingCart2.LastName = (string)vistaDBDataReader["LastName"];
					shoppingCart2.ExpiryMonth = (string)vistaDBDataReader["ExpiryMonth"];
					shoppingCart2.ExpiryYear = (string)vistaDBDataReader["ExpiryYear"];
					shoppingCart2.LastFour = (string)vistaDBDataReader["LastFour"];
					shoppingCart2.CreatedOn = (DateTime)vistaDBDataReader["CreatedOn"];
					shoppingCart2.Type = (ShoppingCartType)Convert.ToInt32(vistaDBDataReader["Type"]);
					shoppingCart2.DiscountCode = (string)vistaDBDataReader["DiscountCode"];
					shoppingCart2.DiscountValue = (decimal)vistaDBDataReader["DiscountValue"];
					shoppingCart2.UseCredits = (CreditElection)Convert.ToInt32(vistaDBDataReader["UseCredits"]);
					shoppingCart2.Email = (string)vistaDBDataReader["Email"];
					shoppingCart2.PostalCode = (string)vistaDBDataReader["PostalCode"];
					shoppingCart = shoppingCart2;
					if (vistaDBDataReader["ReferenceNumber"] != null)
					{
						shoppingCart.ReferenceNumber = Convert.ToInt64(vistaDBDataReader["ReferenceNumber"]);
					}
				}
			}
			if (shoppingCart == null)
			{
				return null;
			}
			using VistaDBCommand vistaDBCommand2 = new VistaDBCommand();
			vistaDBCommand2.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand2.CommandText = "SELECT * FROM ShoppingCartLineItemGroup WHERE ShoppingCartID = @ID";
			vistaDBCommand2.Parameters.Add("@ID", num);
			using VistaDBDataReader vistaDBDataReader2 = vistaDBCommand2.ExecuteReader();
			while (vistaDBDataReader2.Read())
			{
				ShoppingCartLineItemGroup shoppingCartLineItemGroup = new ShoppingCartLineItemGroup();
				((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).TaxRate = (decimal)vistaDBDataReader2["TaxRate"];
				((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).TaxAmount = (decimal)vistaDBDataReader2["TaxAmount"];
				((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).SubTotal = (decimal)vistaDBDataReader2["SubTotal"];
				((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).GrandTotal = (decimal)vistaDBDataReader2["GrandTotal"];
				((ShoppingCartTotal)shoppingCartLineItemGroup.Totals).DiscountedSubtotal = (decimal)vistaDBDataReader2["DiscountedSubTotal"];
				string key = (string)vistaDBDataReader2["Name"];
				shoppingCart.Groups[key] = shoppingCartLineItemGroup;
				int num2 = (int)vistaDBDataReader2["ID"];
				using VistaDBCommand vistaDBCommand3 = new VistaDBCommand();
				vistaDBCommand3.Connection = ShoppingSessionService.Instance.Connection;
				vistaDBCommand3.CommandText = "SELECT * FROM ShoppingCartLineItem WHERE ShoppingCartLineItemGroupID = @ID";
				vistaDBCommand3.Parameters.Add("@ID", num2);
				using VistaDBDataReader vistaDBDataReader3 = vistaDBCommand3.ExecuteReader();
				while (vistaDBDataReader3.Read())
				{
					IShoppingCartLineItem shoppingCartLineItem = shoppingCartLineItemGroup.AddLineItem((string)vistaDBDataReader3["Barcode"], (LineItemStatus)Convert.ToInt32(vistaDBDataReader3["Status"]), (decimal)vistaDBDataReader3["Price"], (decimal)vistaDBDataReader3["Quantity"], (string)vistaDBDataReader3["ProductFamily"], (string)vistaDBDataReader3["ProductType"], (ItemSourceType)Convert.ToInt32(vistaDBDataReader3["SourceType"]));
					int num3 = (int)vistaDBDataReader3["ID"];
					using VistaDBCommand vistaDBCommand4 = new VistaDBCommand();
					vistaDBCommand4.Connection = ShoppingSessionService.Instance.Connection;
					vistaDBCommand4.CommandText = "SELECT * FROM ShoppingCartLineItemPrice WHERE ShoppingCartLineItemID = @ID";
					vistaDBCommand4.Parameters.Add("@ID", num3);
					using VistaDBDataReader vistaDBDataReader4 = vistaDBCommand4.ExecuteReader();
					while (vistaDBDataReader4.Read())
					{
						shoppingCartLineItem.SetPrice((string)vistaDBDataReader4["Name"], (decimal)vistaDBDataReader4["Value"]);
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSession.LoadShoppingCart.", e);
		}
		return shoppingCart;
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
		if (!Groups.ContainsKey(name))
		{
			return null;
		}
		return Groups[name];
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

	public void Save(Guid shoppingSessionId)
	{
		int? num;
		using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
		{
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "\r\n                    INSERT INTO ShoppingCart(SessionID, Type, LastName, FirstName, LastFour, ExpiryMonth, ExpiryYear, PostalCode, Email, DiscountCode, DiscountValue, ReferenceNumber, CreatedOn, UseCredits) \r\n                    VALUES(@SessionID, @Type, @LastName, @FirstName, @LastFour, @ExpiryMonth, @ExpiryYear, @PostalCode, @Email, @DiscountCode, @DiscountValue, @ReferenceNumber, @CreatedOn, @UseCredits);\r\n                    SELECT @@IDENTITY;";
			vistaDBCommand.Parameters.Add("@SessionID", shoppingSessionId);
			vistaDBCommand.Parameters.Add("@Type", Type);
			vistaDBCommand.Parameters.Add("@LastName", LastName);
			vistaDBCommand.Parameters.Add("@FirstName", FirstName);
			vistaDBCommand.Parameters.Add("@LastFour", LastFour);
			vistaDBCommand.Parameters.Add("@ExpiryMonth", ExpiryMonth);
			vistaDBCommand.Parameters.Add("@ExpiryYear", ExpiryYear);
			vistaDBCommand.Parameters.Add("@PostalCode", PostalCode);
			vistaDBCommand.Parameters.Add("@Email", Email);
			vistaDBCommand.Parameters.Add("@DiscountCode", DiscountCode);
			vistaDBCommand.Parameters.Add("@DiscountValue", DiscountValue);
			vistaDBCommand.Parameters.Add("@ReferenceNumber", ReferenceNumber);
			vistaDBCommand.Parameters.Add("@CreatedOn", CreatedOn);
			vistaDBCommand.Parameters.Add("@UseCredits", UseCredits);
			num = (int)vistaDBCommand.ExecuteScalar();
		}
		foreach (string key in Groups.Keys)
		{
			IShoppingCartLineItemGroup shoppingCartLineItemGroup = Groups[key];
			int? num2;
			using (VistaDBCommand vistaDBCommand2 = new VistaDBCommand())
			{
				vistaDBCommand2.Connection = ShoppingSessionService.Instance.Connection;
				vistaDBCommand2.CommandText = "\r\n                        INSERT INTO ShoppingCartLineItemGroup(ShoppingCartID, Name, TaxRate, SubTotal, TaxAmount, GrandTotal, DiscountedSubTotal)\r\n                        VALUES(@ShoppingCartID, @Name, @TaxRate, @SubTotal, @TaxAmount, @GrandTotal, @DiscountedSubTotal);\r\n                        SELECT @@IDENTITY;";
				vistaDBCommand2.Parameters.Add("@ShoppingCartID", num);
				vistaDBCommand2.Parameters.Add("@Name", key);
				vistaDBCommand2.Parameters.Add("@TaxRate", shoppingCartLineItemGroup.Totals.TaxRate);
				vistaDBCommand2.Parameters.Add("@SubTotal", shoppingCartLineItemGroup.Totals.SubTotal);
				vistaDBCommand2.Parameters.Add("@TaxAmount", shoppingCartLineItemGroup.Totals.TaxAmount);
				vistaDBCommand2.Parameters.Add("@GrandTotal", shoppingCartLineItemGroup.Totals.GrandTotal);
				vistaDBCommand2.Parameters.Add("@DiscountedSubTotal", shoppingCartLineItemGroup.Totals.DiscountedSubtotal);
				num2 = (int)vistaDBCommand2.ExecuteScalar();
			}
			foreach (IShoppingCartLineItem item in shoppingCartLineItemGroup.Items)
			{
				int? num3;
				using (VistaDBCommand vistaDBCommand3 = new VistaDBCommand())
				{
					vistaDBCommand3.Connection = ShoppingSessionService.Instance.Connection;
					vistaDBCommand3.CommandText = "\r\n                            INSERT INTO ShoppingCartLineItem(ShoppingCartLineItemGroupID, Barcode, Status, Price, Quantity, ProductFamily, ProductType, SourceType)\r\n                            VALUES(@ShoppingCartLineItemGroupID, @Barcode, @Status, @Price, @Quantity, @ProductFamily, @ProductType, @SourceType);\r\n                            SELECT @@IDENTITY;";
					vistaDBCommand3.Parameters.Add("@ShoppingCartLineItemGroupID", num2);
					vistaDBCommand3.Parameters.Add("@Barcode", item.Barcode);
					vistaDBCommand3.Parameters.Add("@Price", item.Price);
					vistaDBCommand3.Parameters.Add("@Status", item.Status);
					vistaDBCommand3.Parameters.Add("@Quantity", item.Quantity);
					vistaDBCommand3.Parameters.Add("@ProductFamily", item.ProductFamily);
					vistaDBCommand3.Parameters.Add("@ProductType", item.ProductType);
					vistaDBCommand3.Parameters.Add("@SourceType", item.SourceType);
					num3 = (int)vistaDBCommand3.ExecuteScalar();
				}
				foreach (KeyValuePair<string, decimal> price in item.Prices)
				{
					using VistaDBCommand vistaDBCommand4 = new VistaDBCommand();
					vistaDBCommand4.Connection = ShoppingSessionService.Instance.Connection;
					vistaDBCommand4.CommandText = "\r\n                                INSERT INTO ShoppingCartLineItemPrice(ShoppingCartLineItemID, Name, Value) \r\n                                VALUES(@ShoppingCartLineItemID, @Name, @Value);";
					vistaDBCommand4.Parameters.Add("@ShoppingCartLineItemID", num3);
					vistaDBCommand4.Parameters.Add("@Name", price.Key);
					vistaDBCommand4.Parameters.Add("@Value", price.Value);
					vistaDBCommand4.ExecuteNonQuery();
				}
			}
		}
	}
}
