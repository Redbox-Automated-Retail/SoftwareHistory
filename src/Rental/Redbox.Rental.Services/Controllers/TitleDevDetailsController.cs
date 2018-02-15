using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class TitleDevDetailsController : BaseViewController
{
	private static TitleDetailDevModel _model;

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			_model = GetModel();
			viewUserControl.DataContext = _model;
		}
	}

	public static TitleDetailDevModel GetModel()
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		ServiceLocator.Instance.GetService<IStoreManager>();
		TitleDetailDevModel titleDetailDevModel = new TitleDetailDevModel();
		TitleRollupProduct titleRollupProduct = (TitleRollupProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		string text = titleRollupProduct.ProductId.ToString();
		string productName = titleRollupProduct.ProductName;
		Dictionary<string, BarcodeProduct> barCodeProducts = titleRollupProduct.BarCodeProducts;
		IPricingRecord pricingRecord = service?.GetOneDayProductPricingRecord(titleRollupProduct.ProductId);
		titleDetailDevModel.OnCloseButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnCloseButtonClicked, new Action(GoBackAction));
		titleDetailDevModel.OnBlurayButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnBlurayButtonClicked, new Action(BluRayAction));
		titleDetailDevModel.OnDvdButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnDvdButtonClicked, new Action(DVDAction));
		titleDetailDevModel.OnTitleRollupButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnTitleRollupButtonClicked, new Action(TitleRollUpAction));
		titleDetailDevModel.OnDigitalButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnDigitalButtonClicked, new Action(DigitalCodeAction));
		titleDetailDevModel.DevTitle = text + " - " + productName;
		titleDetailDevModel.ComingSoonDays = titleRollupProduct.ComingSoonDays;
		DateTime dateTime = (titleRollupProduct.NationalStreetDate.HasValue ? titleRollupProduct.NationalStreetDate.Value : DateTime.MinValue);
		titleDetailDevModel.NationalStreetDate = ((dateTime == DateTime.MinValue) ? string.Empty : dateTime.ToString("yyyyMMddHHmmss"));
		titleDetailDevModel.BoxOfficeGross = titleRollupProduct.BoxOfficeGross;
		titleDetailDevModel.ClosedCaptioned = titleRollupProduct.ClosedCaptioned;
		titleDetailDevModel.SellThruNew = titleRollupProduct.SellThruNew;
		titleDetailDevModel.ProductTypeId = titleRollupProduct.ProductTypeId;
		titleDetailDevModel.MerchandiseDate = titleRollupProduct.MerchandiseDate.ToShortDateString();
		titleDetailDevModel.SortDate = titleRollupProduct.SortDate.ToShortDateString();
		dateTime = (titleRollupProduct.ReleaseDate.HasValue ? titleRollupProduct.ReleaseDate.Value : DateTime.MinValue);
		titleDetailDevModel.ReleaseDate = ((dateTime == DateTime.MinValue) ? string.Empty : dateTime.ToString("yyyyMMddHHmmss"));
		titleDetailDevModel.SellThru = titleRollupProduct.SellThru;
		titleDetailDevModel.PromoValue = pricingRecord.PromoValue;
		titleDetailDevModel.ExtraNight = pricingRecord.ExtraNight;
		titleDetailDevModel.ExpirationPrice = pricingRecord.ExpirationPrice;
		titleDetailDevModel.InitialNight = pricingRecord.InitialNight;
		titleDetailDevModel.PriceSetId = pricingRecord.ProductPricingId.ToString();
		titleDetailDevModel.Purchase = pricingRecord.Purchase;
		titleDetailDevModel.NonReturn = pricingRecord.NonReturn;
		titleDetailDevModel.NonReturnDays = pricingRecord.NonReturnDays;
		titleDetailDevModel.GenreIds = titleRollupProduct.Genres;
		titleDetailDevModel.IsComingSoon = titleRollupProduct.ComingSoon;
		titleDetailDevModel.InStock = titleRollupProduct.InStock;
		titleDetailDevModel.CanBeSold = titleRollupProduct.IsSellable;
		titleDetailDevModel.ProductFamilyName = titleRollupProduct.TitleFamily.ToString();
		titleDetailDevModel.ProductTypeName = titleRollupProduct.TitleType.ToString();
		titleDetailDevModel.IsAvailable = titleRollupProduct.IsAvailable;
		titleDetailDevModel.ShowOutOfStock = titleRollupProduct.ShowOutOfStock;
		titleDetailDevModel.Type = titleRollupProduct.KioskProductType.ToString();
		titleDetailDevModel.BarCodes = barCodeProducts;
		IBitmapCacheService service2 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		titleDetailDevModel.ImgFile = service2.GetBitmapImage(titleRollupProduct.ImageFile);
		titleDetailDevModel.IsBluRay = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.Bluray).Any();
		titleDetailDevModel.IsDigitalTitle = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DigitalCode).Any();
		return titleDetailDevModel;
	}

	private static void GoBackAction()
	{
		BaseViewController.StartIdleTimer();
		OrchestrationService.ViewGoBack("go back", popOnly: false);
	}

	private static void DVDAction()
	{
		BaseViewController.StopIdleTimer();
		TitleRollupProduct obj = (TitleRollupProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		TitleDetailDevModel model = viewUserControl.DataContext as TitleDetailDevModel;
		List<ITitleProduct> list = obj.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DVD).ToList();
		model.BarCodes = new Dictionary<string, BarcodeProduct>();
		foreach (ITitleProduct item in list)
		{
			item.BarCodeProducts.ToList().ForEach(delegate(KeyValuePair<string, BarcodeProduct> x)
			{
				model.BarCodes.Add(x.Key, x.Value);
			});
		}
		if (list.Count != 0)
		{
			model.Type = list.FirstOrDefault().TitleType.ToString();
		}
		viewUserControl.DataContext = model;
	}

	private static void TitleRollUpAction()
	{
		BaseViewController.StopIdleTimer();
		TitleRollupProduct titleRollupProduct = (TitleRollupProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		TitleDetailDevModel titleDetailDevModel = viewUserControl.DataContext as TitleDetailDevModel;
		titleDetailDevModel.BarCodes = titleRollupProduct.BarCodeProducts;
		titleDetailDevModel.Type = titleRollupProduct.KioskProductType.ToString();
		viewUserControl.DataContext = titleDetailDevModel;
	}

	private static void DigitalCodeAction()
	{
		BaseViewController.StopIdleTimer();
		TitleRollupProduct obj = (TitleRollupProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		TitleDetailDevModel model = viewUserControl.DataContext as TitleDetailDevModel;
		List<ITitleProduct> list = obj.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DigitalCode).ToList();
		model.BarCodes = new Dictionary<string, BarcodeProduct>();
		foreach (ITitleProduct item in list)
		{
			item.BarCodeProducts.ToList().ForEach(delegate(KeyValuePair<string, BarcodeProduct> x)
			{
				model.BarCodes.Add(x.Key, x.Value);
			});
		}
		if (list.Count != 0)
		{
			model.Type = list.FirstOrDefault().TitleType.ToString();
		}
		viewUserControl.DataContext = model;
	}

	private static void BluRayAction()
	{
		try
		{
			BaseViewController.StopIdleTimer();
			TitleRollupProduct obj = (TitleRollupProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
			FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
			TitleDetailDevModel model = viewUserControl.DataContext as TitleDetailDevModel;
			List<ITitleProduct> list = obj.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.Bluray).ToList();
			model.BarCodes = new Dictionary<string, BarcodeProduct>();
			foreach (ITitleProduct item in list)
			{
				item.BarCodeProducts.ToList().ForEach(delegate(KeyValuePair<string, BarcodeProduct> x)
				{
					model.BarCodes.Add(x.Key, x.Value);
				});
			}
			if (list.Count != 0)
			{
				model.Type = list.FirstOrDefault().TitleType.ToString();
			}
			viewUserControl.DataContext = model;
		}
		catch (Exception)
		{
		}
	}
}
