using System;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Controllers;

public class BaseLoyaltyController : BaseViewController
{
	protected static ILoyaltyService loyaltyService => ServiceLocator.Instance.GetService<ILoyaltyService>();

	protected static void GetEstimateAccruedPoints(Action<IEstimatePointsAccruedResult> processEstimateAccruedPoints)
	{
		if (!PersonalizationFunctions.IsLoyaltySignedIn)
		{
			return;
		}
		bool? flag = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.NeedLookupOfLoyaltyPointsToBeEarned;
		if (!flag.HasValue || flag != true || loyaltyService == null)
		{
			return;
		}
		Action<IEstimatePointsAccruedResult> estimatPointsAccuedResultCallback = delegate(IEstimatePointsAccruedResult estimatePointsAccruedResult)
		{
			Task.Factory.StartNew(delegate
			{
				processEstimateAccruedPoints(estimatePointsAccruedResult);
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		};
		loyaltyService.EstimatePointsAccrued(BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart, estimatPointsAccuedResultCallback);
	}

	protected static void GetEstimateRedemptionPoints(Action<IEstimateRedemptionPointsResult> processEstimateRedemptionPoints)
	{
		if (!PersonalizationFunctions.IsLoyaltySignedIn || loyaltyService == null)
		{
			return;
		}
		Action<IEstimateRedemptionPointsResult> estimatRedemptionPointsResultCallback = delegate(IEstimateRedemptionPointsResult estimateRedemptionPointsResult)
		{
			Task.Factory.StartNew(delegate
			{
				processEstimateRedemptionPoints(estimateRedemptionPointsResult);
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		};
		loyaltyService.EstimateRedemptionPoints(BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart, estimatRedemptionPointsResultCallback);
	}

	protected static TitleType GetItemTitleType(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		return rentalShoppingCartItem.TitleType;
	}
}
