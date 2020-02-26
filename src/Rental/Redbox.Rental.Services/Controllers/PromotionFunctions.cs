using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PromotionFunctions
{
	public static void ValidatePromoCode(string promoCode, Action<IPromoCodeValidationResult> doneCallback, Action<IPromoCodeValidationResult> afterErrorMessageCallback)
	{
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimerOnEnter = true,
			EnableIdleTimerOnLeave = true,
			MessageText = Resources.promo_code_validating_message
		});
		if (promoCode == PromotionService.Instance?.GetInCartPromoCode())
		{
			LogHelper.Instance.Log("User manualy entered the InCartPromoCode - clearing!");
			promoCode = null;
		}
		PromotionService.Instance?.ValidatePromoCodeOnServer(promoCode, delegate(IPromoCodeValidationResult result)
		{
			ProcessPromoCodeValidationResult(result, doneCallback, afterErrorMessageCallback);
		});
	}

	private static void ProcessPromoCodeValidationResult(IPromoCodeValidationResult promoCodeValidationResult, Action<IPromoCodeValidationResult> doneCallback, Action<IPromoCodeValidationResult> afterErrorMessageCallback)
	{
		Task.Factory.StartNew(delegate
		{
			Action action = delegate
			{
				doneCallback(promoCodeValidationResult);
			};
			Action action2 = delegate
			{
				afterErrorMessageCallback(promoCodeValidationResult);
			};
			if (promoCodeValidationResult.Success)
			{
				decimal num = promoCodeValidationResult?.Amount ?? 0m;
				decimal num2 = ((promoCodeValidationResult.IntentCode != Redbox.Rental.Model.PromotionIntentCode.PercentOffCartTotal) ? 0m : (promoCodeValidationResult?.Amount ?? 0m));
				IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = promoCodeValidationResult.PromoCode,
					OriginalPrice = num,
					Price = num,
					Quantity = 1,
					Intent = promoCodeValidationResult.IntentCode,
					OriginalPercent = num2,
					Percent = num2,
					RentQuantity = (byte)promoCodeValidationResult.RentQuantity,
					RentFormat = promoCodeValidationResult.RentFormat,
					GetQuantity = (byte)promoCodeValidationResult.GetQuantity,
					GetFormat = promoCodeValidationResult.GetFormat,
					CampaignTitles = promoCodeValidationResult.CampaignTitles,
					ProductTypeId = promoCodeValidationResult.ProductTypeId,
					FormatIds = promoCodeValidationResult.FormatIds,
					ActionCode = promoCodeValidationResult.ActionCode
				};
				PromotionService.Instance?.SetPromotion(promotion);
				IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				IPromoCodeValidationInfo promoCodeValidationInfo = service.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				service.CurrentRentalShoppingCart.UpdateServiceFee();
				service.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
				if (promoCodeValidationResult.IntentCode == Redbox.Rental.Model.PromotionIntentCode.FirstNightGameGameRequired && !promoCodeValidationInfo.IsCartValid && !promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing)
				{
					promoCodeValidationResult.Success = false;
					promoCodeValidationResult.Response = "GameRequired";
				}
				else if (!promoCodeValidationInfo.IsCartValid && (promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing || promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal))
				{
					MessagePopupViewController.ShowPromoCodeInvalidForCartItemsWarning(action, action, promoCodeValidationInfo.Title);
				}
				else if (promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.RxGx)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(action, action, promoCodeValidationInfo.Title + Environment.NewLine + Environment.NewLine + promoCodeValidationInfo.Message);
					}
					else
					{
						action();
					}
				}
				else if (promotion.Intent == Redbox.Rental.Model.PromotionIntentCode.FirstNightFreeItem && (promotion.CampaignTitles?.Titles?.Count ?? 0) <= 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						if (promotion.ProductTypeId.HasValue)
						{
							string firstNightFreePromoCodeInvalidWarningMessage = GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.ProductTypeId);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(action, action, firstNightFreePromoCodeInvalidWarningMessage);
						}
						else if ((promotion.FormatIds?.Count ?? 0) > 0)
						{
							string firstNightFreePromoCodeInvalidWarningMessage2 = GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.FormatIds);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(action, action, firstNightFreePromoCodeInvalidWarningMessage2);
						}
						else
						{
							MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(action, action);
						}
					}
					else
					{
						action();
					}
				}
				else if ((promotion.CampaignTitles?.Titles?.Count ?? 0) > 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(action, action);
					}
					else
					{
						action();
					}
				}
				else
				{
					action();
				}
			}
			if (!promoCodeValidationResult.Success)
			{
				ServiceLocator.Instance.GetService<IPersonalizationService>()?.ClearSelectedPromo();
				if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeAlreadyUsed.ToString())
				{
					MessagePopupViewController.ShowPromoCodeAlreadyUsedWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeNotValidAtThisLocation.ToString())
				{
					MessagePopupViewController.ShowPromoCodeNotValidAtThisLocationWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeForOnDemand.ToString())
				{
					MessagePopupViewController.ShowPromoCodeForOnDemandWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.Error.ToString() || promoCodeValidationResult.Response == "Server Busy" || promoCodeValidationResult.Response == "Communications Failure")
				{
					MessagePopupViewController.ShowPromoCodeValidationFailureWarning(action2, action2);
				}
				else if (promoCodeValidationResult.Response == "GameRequired")
				{
					MessagePopupViewController.ShowPromoCodeGameRequiredWarning(action2, action2);
				}
				else
				{
					MessagePopupViewController.ShowPromoCodeNotValidWarning(action2, action2);
				}
			}
		}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	public static string GetFirstNightFreePromoCodeInvalidWarningMessage(int? productTypeId)
	{
		if (!productTypeId.HasValue)
		{
			return string.Empty;
		}
		string empty = string.Empty;
		return string.Format(arg0: (productTypeId == 1) ? Resources.promo_code_first_night_free_movie : ((productTypeId != 2) ? Resources.promo_code_first_night_free_item : Resources.promo_code_first_night_free_game), format: Resources.promo_code_first_night_free_message);
	}

	public static string GetFirstNightFreePromoCodeInvalidWarningMessage(List<int> formatIds)
	{
		if (formatIds == null || formatIds.Count == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		if (formatIds.Count == 1)
		{
			text = ((TitleType)formatIds[0]).GetDisplayDescription();
		}
		else if (formatIds.Count == 2)
		{
			text = ((TitleType)formatIds[0]).GetDisplayDescription() + " " + Resources.promo_code_first_night_free_or + " " + ((TitleType)formatIds[1]).GetDisplayDescription();
		}
		else
		{
			for (int i = 0; i < formatIds.Count; i++)
			{
				text = ((i >= formatIds.Count - 2) ? ((i >= formatIds.Count - 1) ? (text + ((TitleType)formatIds[i]).GetDisplayDescription()) : (text + ((TitleType)formatIds[i]).GetDisplayDescription() + ", " + Resources.promo_code_first_night_free_or + " ")) : (text + ((TitleType)formatIds[i]).GetDisplayDescription() + ", "));
			}
		}
		return string.Format(Resources.promo_code_first_night_free_message, text);
	}
}
