using System;

namespace Redbox.Rental.Model.Profile;

public class Store : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long StoreId { get; set; }

	public string Address { get; set; }

	public string ReturnTime { get; set; }

	public decimal RentalTaxRate { get; set; }

	public decimal PurchaseTaxRate { get; set; }

	public decimal DigitalPurchaseTaxRate { get; set; }

	public decimal PurchasePrice { get; set; }

	public long VendorId { get; set; }

	public long MarketId { get; set; }

	public long BannerId { get; set; }

	public bool Sellthru { get; set; }

	public DateTime? OpenDate { get; set; }

	public bool InvSync { get; set; }

	public string Address1 { get; set; }

	public string Address2 { get; set; }

	public string City { get; set; }

	public string County { get; set; }

	public string State { get; set; }

	public string Zip { get; set; }

	public int? CollectionMethod { get; set; }

	public decimal ServiceFee { get; set; }

	public decimal ServiceFeeTaxRate { get; set; }
}
