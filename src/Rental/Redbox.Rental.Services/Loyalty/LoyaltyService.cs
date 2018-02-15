using System;
using System.Collections;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.Loyalty;

public class LoyaltyService : ILoyaltyService
{
	private Action<IEstimatePointsAccruedResult> _estimatePointsAccruedResultCallback;

	private Action<IEstimateRedemptionPointsResult> _estimateRedemptionPointsResultCallback;

	public void EstimatePointsAccrued(IRentalShoppingCart rentalShoppingCart, Action<IEstimatePointsAccruedResult> estimatPointsAccuedResultCallback)
	{
		_estimatePointsAccruedResultCallback = estimatPointsAccuedResultCallback;
		RemoteServiceCallback completeCallback = ProcessEstimatePointsAccruedResult;
		ServiceLocator.Instance.GetService<ILoyaltyServices>()?.EstimatePointsAccrued(Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.CustomerProfileNumber, completeCallback);
	}

	private void ProcessEstimatePointsAccruedResult(IRemoteServiceResult result)
	{
		IEstimatePointsAccruedResult estimatePointsAccruedResult = new EstimatedPointsAccruedResult
		{
			Success = (result?.Success ?? false),
			ProductEstimatedPointsAccruedList = new List<IProductEstimatedPointsAccrued>()
		};
		if (result != null && result.Success && result.Properties.ContainsKey("Items") && result.Properties["Items"] is ArrayList arrayList)
		{
			foreach (object item in arrayList)
			{
				IDictionary<string, object> dictionary = item as IDictionary<string, object>;
				long result2 = 0L;
				long productId = ((dictionary != null && dictionary.ContainsKey("ProductId")) ? (long.TryParse(dictionary["ProductId"].ToString(), out result2) ? result2 : 0) : 0);
				int result3 = 0;
				int value = ((dictionary != null && dictionary.ContainsKey("Points")) ? (int.TryParse(dictionary["Points"].ToString(), out result3) ? result3 : 0) : 0);
				estimatePointsAccruedResult.ProductEstimatedPointsAccruedList.Add(new ProductEstimatePointsAccrued
				{
					ProductId = productId,
					LoyaltyPointsRecord = new LoyaltyPointsRecord(null)
					{
						PointsToBeEarned = value
					}
				});
			}
		}
		if (_estimatePointsAccruedResultCallback != null)
		{
			_estimatePointsAccruedResultCallback(estimatePointsAccruedResult);
		}
	}

	public void EstimateRedemptionPoints(IRentalShoppingCart rentalShoppingCart, Action<IEstimateRedemptionPointsResult> estimatRedemptionPointsResultCallback)
	{
		_estimateRedemptionPointsResultCallback = estimatRedemptionPointsResultCallback;
		RemoteServiceCallback completeCallback = ProcessEstimateRedemptionPointsResult;
		ServiceLocator.Instance.GetService<ILoyaltyServices>()?.EstimateRedemptionPoints(Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.CustomerProfileNumber, completeCallback);
	}

	private void ProcessEstimateRedemptionPointsResult(IRemoteServiceResult result)
	{
		IEstimateRedemptionPointsResult estimateRedemptionPointsResult = new EstimateRedemptionPointsResult
		{
			Success = (result?.Success ?? false),
			ProductEstimatedRedemptionPointsList = new List<IProductEstimatedRedemptionPoints>()
		};
		if (result != null && result.Success && result.Properties.ContainsKey("Items") && result.Properties["Items"] is ArrayList arrayList)
		{
			foreach (object item in arrayList)
			{
				IDictionary<string, object> dictionary = item as IDictionary<string, object>;
				long result2 = 0L;
				long productId = (dictionary.ContainsKey("ProductId") ? (long.TryParse(dictionary["ProductId"].ToString(), out result2) ? result2 : 0) : 0);
				int result3 = 0;
				int value = (dictionary.ContainsKey("Points") ? (int.TryParse(dictionary["Points"].ToString(), out result3) ? result3 : 0) : 0);
				estimateRedemptionPointsResult.ProductEstimatedRedemptionPointsList.Add(new ProductEstimatedRedemptionPoints
				{
					ProductId = productId,
					LoyaltyPointsRecord = new LoyaltyPointsRecord(null)
					{
						PointsToRedeem = value
					}
				});
			}
		}
		if (_estimatePointsAccruedResultCallback != null)
		{
			_estimateRedemptionPointsResultCallback(estimateRedemptionPointsResult);
		}
	}
}
