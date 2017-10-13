using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class TitleRollupUtilities
{
	public class TitleRollupOverlayButtonTextValues
	{
		private string _titleRollupOverlayUnavailableText;

		private string _titleRollupOverlayCartText;

		private string _titleRollupOverlayOosText;

		private string _titleRollupOverlayBuyText;

		private string _titleRollupOverlayAddText;

		private string _currencySymbol;

		public string TitleRollupOverlayUnavailableText
		{
			get
			{
				if (_titleRollupOverlayUnavailableText == null)
				{
					_titleRollupOverlayUnavailableText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_unavailable_text");
				}
				return _titleRollupOverlayUnavailableText;
			}
		}

		public string TitleRollupOverlayCartText
		{
			get
			{
				if (_titleRollupOverlayCartText == null)
				{
					_titleRollupOverlayCartText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_cart_text");
				}
				return _titleRollupOverlayCartText;
			}
		}

		public string TitleRollupOverlayOosText
		{
			get
			{
				if (_titleRollupOverlayOosText == null)
				{
					_titleRollupOverlayOosText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_oos_text");
				}
				return _titleRollupOverlayOosText;
			}
		}

		public string TitleRollupOverlayBuyText
		{
			get
			{
				if (_titleRollupOverlayBuyText == null)
				{
					_titleRollupOverlayBuyText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_buy_text");
				}
				return _titleRollupOverlayBuyText;
			}
		}

		public string TitleRollupOverlayAddText
		{
			get
			{
				if (_titleRollupOverlayAddText == null)
				{
					_titleRollupOverlayAddText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_add_text");
				}
				return _titleRollupOverlayAddText;
			}
		}

		public string CurrencySymbol
		{
			get
			{
				if (_currencySymbol == null)
				{
					_currencySymbol = BundleFunctions.GetDictionaryStringValue("strings", "currency_symbol");
				}
				return _currencySymbol;
			}
		}
	}

	public static void ConfigureTitleRollupOverlay(IBrowseItemModel browseItemModel, TitleRollupOverlayModel titleRollupOverlayModel, bool showDigitalCodeInfo, bool isPurchase)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			titleRollupOverlayModel.BrowseItemModel = browseItemModel;
			TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD), isPurchase);
			titleRollupOverlayModel.DVDButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel.DVDButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel.DVDButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			titleRollupOverlayModel.DVDButtonMarginGridRowHeight = titleRollupOverlayButtonProperties.MarginGridRowHeight;
			titleRollupOverlayModel.DVDButtonTextGridRowHeight = titleRollupOverlayButtonProperties.TextGridRowHeight;
			titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray), isPurchase);
			titleRollupOverlayModel.BlurayButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel.BlurayButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel.BlurayButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			titleRollupOverlayModel.BlurayButtonMarginGridRowHeight = titleRollupOverlayButtonProperties.MarginGridRowHeight;
			titleRollupOverlayModel.BlurayButtonTextGridRowHeight = titleRollupOverlayButtonProperties.TextGridRowHeight;
			titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode), isPurchase);
			titleRollupOverlayModel.DigitalCodeButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel.DigitalCodeButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel.DigitalCodeButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			titleRollupOverlayModel.DigitalCodeButtonMarginGridRowHeight = titleRollupOverlayButtonProperties.MarginGridRowHeight;
			titleRollupOverlayModel.DigitalCodeButtonTextGridRowHeight = titleRollupOverlayButtonProperties.TextGridRowHeight;
			titleRollupOverlayModel.DigitalCodeInfoVisibility = ((!showDigitalCodeInfo || titleRollupOverlayModel.DigitalCodeButtonVisibility != Visibility.Visible || !titleRollupOverlayModel.DigitalCodeButtonEnabled) ? Visibility.Hidden : Visibility.Visible);
			titleRollupOverlayModel.DigitalCodeInfoText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_digital_code_info");
			if (titleRollupOverlayModel.DigitalCodeButtonVisibility != Visibility.Visible)
			{
				titleRollupOverlayModel.GridDigitalCodeButtonGridRowHeight = new GridLength(0.0);
				titleRollupOverlayModel.GridDigitalCodeButtonMarginGridRowHeight = new GridLength(0.0);
			}
			else
			{
				titleRollupOverlayModel.GridDigitalCodeButtonGridRowHeight = new GridLength(20.0, GridUnitType.Star);
				titleRollupOverlayModel.GridDigitalCodeButtonMarginGridRowHeight = new GridLength(10.0, GridUnitType.Star);
			}
			titleRollupOverlayModel.Visibility = Visibility.Visible;
		}
	}

	public static TitleRollupOverlayButtonProperties GetTitleRollupOverlayButtonProperties(ITitleProduct titleProduct, bool isPurchase)
	{
		return GetTitleRollupOverlayButtonProperties(titleProduct, isPurchase, new TitleRollupOverlayButtonTextValues());
	}

	public static TitleRollupOverlayButtonProperties GetTitleRollupOverlayButtonProperties(ITitleProduct titleProduct, bool isPurchase, TitleRollupOverlayButtonTextValues titleRollupOverlayButtonTextValues)
	{
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = new TitleRollupOverlayButtonProperties
		{
			Visibility = Visibility.Collapsed
		};
		if (titleProduct != null && titleRollupOverlayButtonTextValues != null)
		{
			string arg = ((titleProduct.TitleType == TitleType.DVD) ? "DVD" : ((titleProduct.TitleType == TitleType.Bluray) ? "BLU-RAY" : ((titleProduct.TitleType == TitleType.DigitalCode) ? BundleFunctions.GetDictionaryStringValue("strings", "digital_code").ToUpper() : null)));
			titleRollupOverlayButtonProperties.ButtonText = titleRollupOverlayButtonTextValues.TitleRollupOverlayUnavailableText;
			if (ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.HasItem(titleProduct.ProductId) ?? false)
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(titleRollupOverlayButtonTextValues.TitleRollupOverlayCartText, arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
			}
			else if (!titleProduct.InStock)
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(titleRollupOverlayButtonTextValues.TitleRollupOverlayOosText, arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
			}
			else if (isPurchase)
			{
				decimal moviePurchasePrice = titleProduct.MoviePurchasePrice;
				if (moviePurchasePrice > 0m && titleProduct.IsSellable)
				{
					titleRollupOverlayButtonProperties.ButtonText = string.Format(titleRollupOverlayButtonTextValues.TitleRollupOverlayBuyText, arg, titleRollupOverlayButtonTextValues.CurrencySymbol, $"{moviePurchasePrice:N2}");
					titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
					titleRollupOverlayButtonProperties.Enabled = true;
				}
			}
			else if (titleProduct.IsAvailable && titleProduct.InStock)
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(titleRollupOverlayButtonTextValues.TitleRollupOverlayAddText, arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
				titleRollupOverlayButtonProperties.Enabled = true;
			}
			if (titleRollupOverlayButtonProperties.ButtonText.Contains("\n"))
			{
				titleRollupOverlayButtonProperties.MarginGridRowHeight = new GridLength(15.0, GridUnitType.Star);
				titleRollupOverlayButtonProperties.TextGridRowHeight = new GridLength(70.0, GridUnitType.Star);
			}
			else
			{
				titleRollupOverlayButtonProperties.MarginGridRowHeight = new GridLength(30.0, GridUnitType.Star);
				titleRollupOverlayButtonProperties.TextGridRowHeight = new GridLength(40.0, GridUnitType.Star);
			}
			if (titleProduct.TitleType == TitleType.DigitalCode && !isPurchase)
			{
				titleRollupOverlayButtonProperties.Visibility = Visibility.Hidden;
				titleRollupOverlayButtonProperties.Enabled = false;
			}
		}
		return titleRollupOverlayButtonProperties;
	}
}
