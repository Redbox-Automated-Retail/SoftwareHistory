using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.UI.Models;

public class DisplayProductModelConstants
{
	public string StarringLabel { get; set; }

	public string GenresLabel { get; set; }

	public string OneNightPriceLabel { get; set; }

	public string OneNightReturnTimeLabel { get; set; }

	public string MultiNightPriceLabel { get; set; }

	public string MultiNightReturnTimeLabel { get; set; }

	public BitmapImage AdImage { get; set; }

	public int AdHeight { get; set; }

	public int MainHeight { get; set; }

	public bool ShowCollections { get; set; }

	public string CollectionTextLine1 { get; set; }

	public string BuyText { get; set; }

	public ITitleRollupFormatButtonTextValues TitleDetailFormatButtonTextValues { get; set; }

	public string Add { get; set; }

	public string ComingSoonLabel { get; set; }

	public string OutOfStockLine1Text { get; set; }

	public string OutOfStockLine2Text { get; set; }

	public string OutOfStockLine3Text { get; set; }

	public string Movies { get; set; }

	public string Games { get; set; }

	public Style FontMuseo16Style { get; set; }

	public Style FontMuseo20Style { get; set; }

	public string PriceFor { get; set; }

	public string Night { get; set; }

	public IPricingService PricingService { get; set; }

	public IRentalShoppingCartService RentalShoppingCartService { get; set; }

	public bool IsInCartBrowse { get; set; }
}
