using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class CollectionMessageViewController : BaseViewController
{
	public static void CheckCollectionsPopup(string reason)
	{
		if (OrchestrationService.IsCollectionsPopupEnabled())
		{
			ConfigureCollectionMessageViewController(reason);
		}
	}

	public static void ConfigureCollectionMessageViewController(string reason)
	{
		BaseViewController.AnalyticsService?.AddData(new NameValueData("CollectionsPopupReason", reason));
		IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance?.BrowseState;
		if (browseState != null)
		{
			browseState.CollectionsPopupShown = true;
		}
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer("idle_timer"))?.Suspend();
		OrchestrationService.ResetApplicationIdleTimerTimeout();
		List<DisplayCollectionModel> displayCollectionModels = GetDisplayCollectionModels();
		if (displayCollectionModels == null || displayCollectionModels.Count <= 1)
		{
			return;
		}
		IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
		if (viewService == null)
		{
			return;
		}
		viewService.Push("collection_message_view");
		viewService.Show();
		if (!(BaseViewController.GetViewUserControl() is CollectionMessageViewUserControl collectionMessageViewUserControl))
		{
			return;
		}
		CollectionMessageModel collectionMessageModel = new CollectionMessageModel
		{
			Title = Resources.collections_popup_text,
			SeeAllCollectionsButtonText = Resources.collections_popup_see_all_collections,
			TimeOut = 15000,
			DisplayCollectionModel1 = displayCollectionModels[0],
			DisplayCollectionModel2 = displayCollectionModels[1]
		};
		collectionMessageModel.OnTimeOut = delegate
		{
			OrchestrationService.InvokeIdleTimerExpireSessionCallback();
		};
		collectionMessageModel.OnCancelDialog = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("CollectionMessageCancel");
			viewService?.GoBackSafe("collection message view cancelled", popOnly: false);
		};
		collectionMessageModel.OnSeeAllCollections = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("CollectionMessageSeeAll");
			if (viewService.Peek(1) != "browse_view")
			{
				browseState.PushNonBrowseViewBrowseStateSet();
			}
			browseState.PushCollectionsBrowseStateSet();
			viewService?.GoBackSafe("collection message view cancelled", popOnly: false);
			if (viewService.Peek() != "browse_view")
			{
				viewService?.Push("browse_view");
				viewService?.Show();
			}
		};
		collectionMessageModel.OnCollectionSelected += delegate(IBrowseItemModel browseItemModel, object parameter)
		{
			CollectionProduct collectionProduct = browseItemModel?.Data as CollectionProduct;
			BaseViewController.AnalyticsService?.AddButtonPressEvent("CollectionMessageView")?.AddData(new CollectionData(collectionProduct.Id, collectionProduct.ProductName));
			if (viewService.Peek(1) != "browse_view")
			{
				browseState.PushNonBrowseViewBrowseStateSet();
			}
			browseState?.PushCollectionBrowseStateSet(collectionProduct);
			viewService?.GoBackSafe("collection message collection selected", popOnly: false);
			if (viewService.Peek() != "browse_view")
			{
				viewService?.Push("browse_view");
				viewService?.Show();
			}
		};
		collectionMessageViewUserControl.DataContext = collectionMessageModel;
		CommandManager.InvalidateRequerySuggested();
	}

	private static List<DisplayCollectionModel> GetDisplayCollectionModels()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service.GetCollectionProducts(out products);
		List<IKioskProduct> list = products.Take(2).ToList();
		if (list != null && list.Count < 2)
		{
			IKioskProduct kioskProduct = products.FirstOrDefault((IKioskProduct x) => x is ICollectionProduct && (x as ICollectionProduct).IsTop20);
			if (kioskProduct != null)
			{
				while (list.Count < 2)
				{
					list.Add(kioskProduct);
				}
			}
		}
		List<DisplayCollectionModel> list2 = new List<DisplayCollectionModel>();
		if (list.Count > 1)
		{
			foreach (IKioskProduct item in list)
			{
				DisplayCollectionModel displayCollectionModel = BrowseFunctions.CreateDisplayCollectionModelFromCollectionProduct(item as CollectionProduct);
				if (displayCollectionModel != null)
				{
					list2.Add(displayCollectionModel);
				}
			}
		}
		return list2;
	}
}
