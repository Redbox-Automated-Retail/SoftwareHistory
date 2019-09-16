using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class RecommendedTitlesPopupViewController : BaseViewController
{
	private static string _shownFromView;

	public static void ConfigureRecommendedTitlesPopupView(RecommendedTitlesPopupInfo recommendedTitlesPopupInfo)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		OrchestrationService.AnalyticsAddViewEvent("context", (recommendedTitlesPopupInfo.ShownFromView == "option_selection_dialog_view") ? "RelatedMoviesDialog" : "RecommendedTitlesPopup");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			_shownFromView = recommendedTitlesPopupInfo.ShownFromView;
			RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel = new RecommendedTitlesPopupViewModel();
			recommendedTitlesPopupViewModel.HeaderText = ((recommendedTitlesPopupInfo.ShownFromView == "option_selection_dialog_view") ? Resources.related_movies_popup_header : Resources.recommended_titles_popup_header);
			recommendedTitlesPopupViewModel.OfferText = ((recommendedTitlesPopupInfo.ShownFromView == "option_selection_dialog_view") ? Resources.related_movies_popup_offer : Resources.recommended_titles_popup_offer);
			recommendedTitlesPopupViewModel.OfferTextVisibility = ((!recommendedTitlesPopupInfo.ShowOfferText) ? Visibility.Hidden : Visibility.Visible);
			ConfigureRecommendedTitlesBrowse(recommendedTitlesPopupViewModel, recommendedTitlesPopupInfo.RecommendedTitles, recommendedTitlesPopupInfo.NumberOfTitlesToShow);
			recommendedTitlesPopupViewModel.CancelButtonText = Resources.no_thanks_button;
			recommendedTitlesPopupViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(recommendedTitlesPopupViewModel.OnCancelButtonClicked, new Action(RecommendedTitlesPopupViewModel_OnCancelButtonClicked));
			ConfigureTTS(recommendedTitlesPopupViewModel);
			viewUserControl.DataContext = recommendedTitlesPopupViewModel;
		}
	}

	public static void LeaveRecommendedTitlesPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	public static IRecommendedTitles UnrollRecommendedTitles(IRecommendedTitles recommendedTitles)
	{
		List<IRecommendedTitleId> list = new List<IRecommendedTitleId>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			foreach (IRecommendedTitleId recommendedTitle in recommendedTitles)
			{
				if (recommendedTitle.IdType == RecommendedTitlesIdType.TitleID)
				{
					list.Add(recommendedTitle);
				}
				else
				{
					if (recommendedTitle.IdType != RecommendedTitlesIdType.ProductGroupID)
					{
						continue;
					}
					service.GetTitleRollupProduct(recommendedTitle.Id, out var kioskProduct);
					if (kioskProduct == null)
					{
						continue;
					}
					ITitleRollupProduct titleRollupProduct = ((kioskProduct.KioskProductType == KioskProductType.TitleRollup) ? (kioskProduct as ITitleRollupProduct) : null);
					if (titleRollupProduct == null)
					{
						continue;
					}
					foreach (ITitleProduct item in titleRollupProduct.KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
					{
						list.Add(new RecommendedTitleId
						{
							Id = item.ProductId,
							IdType = RecommendedTitlesIdType.TitleID
						});
					}
				}
			}
		}
		return new RecommendedTitles(list)
		{
			OfferCode = recommendedTitles.OfferCode
		};
	}

	public static IRecommendedTitles GenerateRecommendedTitles(ITitleProduct titleProduct)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service == null)
		{
			return null;
		}
		ErrorList products = service.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleProduct.TitleType, BrowseSort.SortDate, null, null, null, isADAView: true, null, null, out var products2);
		if (products.Count > 0)
		{
			products.ToLogHelper();
			return null;
		}
		if (products2 == null)
		{
			return null;
		}
		List<IKioskProduct> list = (from kp in products2
			where kp is ITitleProduct && (((ITitleProduct)kp).Genres.Intersect(titleProduct.Genres).Count() > 0 || ((ITitleProduct)kp).Genres.Contains(1093L)) && ((ITitleProduct)kp).IsRentableAndInStock
			orderby !((ITitleProduct)kp).Genres.Contains(1093L) descending
			select kp).ToList();
		List<IRecommendedTitleId> list2 = new List<IRecommendedTitleId>();
		foreach (IKioskProduct item in list)
		{
			list2.Add(new RecommendedTitleId
			{
				Id = ((ITitleProduct)item).ProductId,
				IdType = RecommendedTitlesIdType.TitleID
			});
		}
		return new RecommendedTitles(list2);
	}

	public static List<IBrowseItemModel> CreateBrowseItemsFromRecommendedTitles(IRecommendedTitles recommendedTitles, int numberOfTitlesToShow)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			foreach (IRecommendedTitleId recommendedTitle in recommendedTitles)
			{
				IKioskProduct kioskProduct = null;
				if (recommendedTitle.IdType == RecommendedTitlesIdType.TitleID)
				{
					service.GetProduct(recommendedTitle.Id, excludeTitleRollup: true, out kioskProduct);
				}
				else if (recommendedTitle.IdType == RecommendedTitlesIdType.ProductGroupID)
				{
					service.GetTitleRollupProduct(recommendedTitle.Id, out kioskProduct);
				}
				if (kioskProduct == null)
				{
					continue;
				}
				ITitleRollupProduct titleRollupProduct = ((kioskProduct.KioskProductType == KioskProductType.TitleRollup) ? (kioskProduct as ITitleRollupProduct) : null);
				ITitleProduct titleProduct = ((kioskProduct.KioskProductType == KioskProductType.Title) ? (kioskProduct as ITitleProduct) : null);
				if ((titleRollupProduct == null || !titleRollupProduct.IsRentableAndInStock || titleRollupProduct.IsEmptyCase) && (titleProduct == null || !titleProduct.IsRentableAndInStock || titleProduct.IsEmptyCase))
				{
					continue;
				}
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					Header = kioskProduct.ProductName,
					DisplayProductModelConstants = GetDisplayProductModelConstants(),
					AddButtonStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style")
				};
				displayProductModel.ConfigureForRecommendedTitlesPopup();
				displayProductModel.ButtonData.Clear();
				if (titleRollupProduct != null)
				{
					foreach (ITitleProduct item in titleRollupProduct.KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
					{
						AddFormatButtonData(displayProductModel, item);
					}
				}
				else
				{
					AddFormatButtonData(displayProductModel, titleProduct);
				}
				list.Add(displayProductModel);
			}
		}
		return list.Take(numberOfTitlesToShow).ToList();
	}

	private static RecommendedTitlesPopupViewModel GetRecommendedTitlesPopupViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as RecommendedTitlesPopupViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			recommendedTitlesPopupViewModel.OnGetSpeechControl = () => RecommendedTitlesPopupViewTextToSpeech.GetSpeechControl(GetRecommendedTitlesPopupViewModel);
		}
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("medium", 15000);
		IIdleTimer obj = service?.CreateIdleTimer("RecommendedTitlesPopupViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "timeOut");
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("RecommendedTitlesPopupViewController time out", popOnly: false);
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("RecommendedTitlesPopupViewTimer"))?.Stop();
		service?.RemoveIdleTimer("RecommendedTitlesPopupViewTimer");
	}

	private static void ConfigureRecommendedTitlesBrowse(RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel, IRecommendedTitles recommendedTitles, int numberOfTitlesToShow)
	{
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.RowCount = 1;
		browseControlModel.ColumnCount = 4;
		browseControlModel.BrowseItemMargin = new Thickness(0.0, 32.0, 0.0, 32.0);
		browseControlModel.ShowBorderItems = true;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		browseControlModel.ShowNoResultsMessage = false;
		browseControlModel.BrowseItems = CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels = browseControlModel.BrowseItems;
		for (int i = 0; i < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; i++)
		{
			browseControlModel.BrowseControls.Add(new DisplayProductBrowseUserControl
			{
				Name = $"product_{i + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		browseControlModel.OnBrowseItemModelSelected += BrowseProductControl_OnBrowseItemModelSelected;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddBrowseItemModel;
		recommendedTitlesPopupViewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void BrowseProductControl_OnBrowseItemModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel.Data is IKioskProduct { KioskProductType: var kioskProductType } kioskProduct && (kioskProductType == KioskProductType.Title || kioskProductType == KioskProductType.TitleRollup))
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", $"related_movies_title_details,{titleProduct.ProductId}");
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesTitleDetails")?.AddData(ProductData.ToProductData(titleProduct));
			}
			if (titleProduct != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductDetailShownFromView = service?.Peek();
				OrchestrationService.ViewShow("product_detail_view");
			}
		}
	}

	private static void BrowseProductControl_OnAddBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct) || GetRecommendedTitlesPopupViewModel() == null)
		{
			return;
		}
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.TitleRollup:
		{
			TitleRollupProduct rollupProduct = kioskProduct as TitleRollupProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				ITitleProduct titleProduct2 = kioskProduct as ITitleProduct;
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", $"related_movies_add_title,{titleProduct2.ProductId}", ProductData.ToProductData(titleProduct2));
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesAddTitle")?.AddData(ProductData.ToProductData(titleProduct2));
			}
			else
			{
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "recommended-titles-popup-add-item");
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedAddItem");
			}
			OrchestrationService.ViewShow("rental_popup_view", new RentalFormatSelectionViewParameter(rollupProduct, isPurchase: false));
			break;
		}
		case KioskProductType.Title:
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", $"related_movies_add_title,{titleProduct.ProductId}", ProductData.ToProductData(titleProduct));
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesAddTitle")?.AddData(ProductData.ToProductData(titleProduct));
			}
			else
			{
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "recommended-titles-popup-add-item");
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedAddItem");
			}
			if (titleProduct.HasMultiNightPrice)
			{
				OrchestrationService.ViewShow("multi_night_pricing_popup_view", new MultiNightPricingPopupInfo
				{
					TitleProduct = titleProduct,
					ShownFromView = "recommended_titles_popup_view"
				});
			}
			else
			{
				BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false);
			}
			break;
		}
		}
	}

	private static void RecommendedTitlesPopupViewModel_OnCancelButtonClicked()
	{
		if (_shownFromView == "option_selection_dialog_view")
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "related_movies_nothanks");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedNoThanks");
			OrchestrationService.UnwindToSafe("browse_view", discard: true, "recommended_titles_popup_view");
		}
		else
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "recommended_titles_popup_no_thanks");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedNoThanks");
			OrchestrationService.ViewGoBackSafe("RecommendedTitlesPopupViewController cancel", popOnly: false);
		}
	}

	private static DisplayProductModelConstants GetDisplayProductModelConstants()
	{
		return new DisplayProductModelConstants
		{
			StarringLabel = Resources.starring_label,
			GenresLabel = Resources.genres_label,
			OneNightPriceLabel = Resources.one_night_price_label,
			OneNightReturnTimeLabel = Resources.return_time_label,
			MultiNightPriceLabel = Resources.multi_night_price_label,
			MultiNightReturnTimeLabel = Resources.return_time_label,
			AdImage = null,
			AdHeight = 20,
			MainHeight = 610,
			ShowCollections = false,
			CollectionTextLine1 = Resources.title_detail_collection_text,
			BuyText = Resources.title_detail_buy,
			TitleDetailFormatButtonTextValues = new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues(),
			Add = Resources.add,
			ComingSoonLabel = Resources.coming_label,
			OutOfStockLine1Text = Resources.out_of_stock_line1,
			OutOfStockLine2Text = Resources.out_of_stock_line2,
			OutOfStockLine3Text = Resources.out_of_stock_line3,
			Movies = Resources.Movies,
			Games = Resources.Games,
			Movie = Resources.Movie.ToLower(),
			Game = Resources.game.ToLower(),
			FontMuseo20Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_museo_slab_900_20"),
			FontMuseo16Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_museo_slab_900_16"),
			PriceFor = Resources.price_for,
			Night = Resources.night,
			PricingService = ServiceLocator.Instance.GetService<IPricingService>(),
			RentalShoppingCartService = BaseViewController.RentalShoppingCartService,
			IsInCartBrowse = false
		};
	}

	private static void AddFormatButtonData(DisplayProductModel browseItemModel, ITitleProduct product)
	{
		DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(product, isPurchase: false, displayProductModelConstants.TitleDetailFormatButtonTextValues);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			TilteDetailRentalFormatButtonData tilteDetailRentalFormatButtonData = new TilteDetailRentalFormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled);
			if (!product.IsMovie)
			{
				tilteDetailRentalFormatButtonData.Line1Text = displayProductModelConstants.Add;
			}
			browseItemModel.ButtonData.Add(tilteDetailRentalFormatButtonData);
		}
	}
}
