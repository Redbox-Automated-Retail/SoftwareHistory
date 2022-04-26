using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
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
			Action doneAction = delegate
			{
				doneCallback(promoCodeValidationResult);
			};
			Action action = delegate
			{
				afterErrorMessageCallback(promoCodeValidationResult);
			};
			if (promoCodeValidationResult.Success)
			{
				IPromotion promotion = CreatePromotionFromPromoCodeValidationResult(promoCodeValidationResult);
				PromotionService.Instance?.SetPromotion(promotion);
				IRentalShoppingCartService shoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				IPromoCodeValidationInfo promoCodeValidationInfo = shoppingCartService.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				shoppingCartService.CurrentRentalShoppingCart.UpdateServiceFee();
				shoppingCartService.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
				if (promoCodeValidationResult.IntentCode == PromotionIntentCode.FirstNightGameGameRequired && !promoCodeValidationInfo.IsCartValid && !promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing)
				{
					promoCodeValidationResult.Success = false;
					promoCodeValidationResult.Response = "GameRequired";
				}
				else if (!promoCodeValidationInfo.IsCartValid && (promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal || promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing))
				{
					MessagePopupViewController.ShowPromoCodeInvalidForCartItemsWarning(doneAction, doneAction, promoCodeValidationInfo.Title, promoCodeValidationInfo.Message);
				}
				else if (!promoCodeValidationInfo.IsCartValid && promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction && !promotion.AllowFullDiscount && promotion.DefaultPromo != null)
				{
					IPromotion defaultPromotion = CreatePromotionFromDefaultPromo(promotion.DefaultPromo);
					if (defaultPromotion != null)
					{
						string arg = ((defaultPromotion.Intent == PromotionIntentCode.PercentOffCartTotal) ? $"{defaultPromotion.Percent:P0}" : $"{defaultPromotion.Price:C}");
						MessagePopupViewController.ShowPromoCodeDiscountRestrictionWarning(string.Format(Resources.promo_code_discount_restriction_warning_title, arg), string.Format(Resources.apply_promo_amount_off_button, arg), delegate
						{
							doneAction();
						}, delegate
						{
							doneAction();
						}, delegate
						{
							PromotionService.Instance?.SetPromotion(defaultPromotion);
							shoppingCartService.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
							shoppingCartService.CurrentRentalShoppingCart.UpdateServiceFee();
							shoppingCartService.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
							doneAction();
						});
						ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddViewEvent(new DiscountRestrictionEvent(promotion.Name, defaultPromotion.Name));
					}
					else
					{
						doneAction();
					}
				}
				else if (promotion.Intent == PromotionIntentCode.RxGx)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(doneAction, doneAction, promoCodeValidationInfo.Title + Environment.NewLine + Environment.NewLine + promoCodeValidationInfo.Message);
					}
					else
					{
						doneAction();
					}
				}
				else if (promotion.Intent == PromotionIntentCode.FirstNightFreeItem && (promotion.CampaignTitles?.Titles?.Count ?? 0) <= 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						if (promotion.ProductTypeId.HasValue)
						{
							string firstNightFreePromoCodeInvalidWarningMessage = GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.ProductTypeId);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(doneAction, doneAction, firstNightFreePromoCodeInvalidWarningMessage);
						}
						else if ((promotion.FormatIds?.Count ?? 0) > 0)
						{
							string firstNightFreePromoCodeInvalidWarningMessage2 = GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.FormatIds);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(doneAction, doneAction, firstNightFreePromoCodeInvalidWarningMessage2);
						}
						else
						{
							MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(doneAction, doneAction);
						}
					}
					else
					{
						doneAction();
					}
				}
				else if ((promotion.CampaignTitles?.Titles?.Count ?? 0) > 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(doneAction, doneAction);
					}
					else
					{
						doneAction();
					}
				}
				else
				{
					doneAction();
				}
			}
			if (!promoCodeValidationResult.Success)
			{
				ServiceLocator.Instance.GetService<IPersonalizationService>()?.ClearSelectedPromo();
				if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeAlreadyUsed.ToString())
				{
					MessagePopupViewController.ShowPromoCodeAlreadyUsedWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeNotValidAtThisLocation.ToString())
				{
					MessagePopupViewController.ShowPromoCodeNotValidAtThisLocationWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeForOnDemand.ToString())
				{
					MessagePopupViewController.ShowPromoCodeForOnDemandWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.Error.ToString() || promoCodeValidationResult.Response == "Server Busy" || promoCodeValidationResult.Response == "Communications Failure")
				{
					MessagePopupViewController.ShowPromoCodeValidationFailureWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == "GameRequired")
				{
					MessagePopupViewController.ShowPromoCodeGameRequiredWarning(action, action);
				}
				else
				{
					MessagePopupViewController.ShowPromoCodeNotValidWarning(action, action);
				}
			}
		}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private static IPromotion CreatePromotionFromPromoCodeValidationResult(IPromoCodeValidationResult promoCodeValidationResult)
	{
		decimal amount = promoCodeValidationResult.Amount;
		decimal num = ((promoCodeValidationResult.IntentCode == PromotionIntentCode.PercentOffCartTotal) ? amount : 0m);
		IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
		{
			Name = promoCodeValidationResult.PromoCode,
			OriginalPrice = amount,
			Price = amount,
			Quantity = 1,
			Intent = promoCodeValidationResult.IntentCode,
			OriginalPercent = num,
			Percent = num,
			RentQuantity = (byte)promoCodeValidationResult.RentQuantity,
			RentFormat = promoCodeValidationResult.RentFormat,
			GetQuantity = (byte)promoCodeValidationResult.GetQuantity,
			GetFormat = promoCodeValidationResult.GetFormat,
			CampaignTitles = promoCodeValidationResult.CampaignTitles,
			ProductTypeId = promoCodeValidationResult.ProductTypeId,
			FormatIds = promoCodeValidationResult.FormatIds,
			ActionCode = promoCodeValidationResult.ActionCode,
			AllowFullDiscount = (promoCodeValidationResult.AllowFullDiscount ?? true)
		};
		if (promoCodeValidationResult.DefaultPromo != null)
		{
			promotion.DefaultPromo = new DefaultPromo
			{
				PromoCode = promoCodeValidationResult.DefaultPromo.PromoCode,
				PromotionIntentCode = promoCodeValidationResult.DefaultPromo.PromotionIntentCode,
				Amount = promoCodeValidationResult.DefaultPromo.Amount
			};
		}
		IStoredPromoCode storedPromoCode = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.StoredPromoCodes?.FirstOrDefault((IStoredPromoCode x) => x.PromoCode == promoCodeValidationResult.PromoCode);
		if (storedPromoCode != null)
		{
			promotion.CampaignId = storedPromoCode.CampaignId;
		}
		return promotion;
	}

	private static IPromotion CreatePromotionFromDefaultPromo(IDefaultPromo defaultPromo)
	{
		if (defaultPromo != null && !string.IsNullOrEmpty(defaultPromo.PromoCode) && !string.IsNullOrEmpty(defaultPromo.PromotionIntentCode) && defaultPromo.Amount.HasValue && Enum.TryParse<PromotionIntentCode>(defaultPromo.PromotionIntentCode, out var result) && (result == PromotionIntentCode.FixedValue || result == PromotionIntentCode.PercentOffCartTotal))
		{
			decimal num = (decimal)defaultPromo.Amount.Value;
			decimal num2 = ((result == PromotionIntentCode.PercentOffCartTotal) ? num : 0m);
			return new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = defaultPromo.PromoCode,
				OriginalPrice = num,
				Price = num,
				Quantity = 1,
				Intent = result,
				OriginalPercent = num2,
				Percent = num2,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null,
				ProductTypeId = null,
				FormatIds = null,
				ActionCode = PromotionActionCode.Rental,
				AllowFullDiscount = true,
				DefaultPromo = null,
				CampaignId = 0
			};
		}
		return null;
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
