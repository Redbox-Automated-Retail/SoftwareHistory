using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForCartEmpty = "CheckForCartEmpty";

		public const string CartIsEmpty = "CartIsEmpty";

		public const string ListCartItems = "ListCartItems";

		public const string DailyRentalCharge = "DailyRentalCharges";
	}

	private static int CartCount
	{
		get
		{
			int result = 0;
			if (RentalShoppingCartService?.CurrentRentalShoppingCart != null)
			{
				result = RentalShoppingCartService.CurrentRentalShoppingCart.Count;
			}
			return result;
		}
	}

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	public static ISpeechControl GetSpeechControl(Func<ShoppingCartViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_shopping_cart_view";
		AddSpeechPartCheckForCartEmpty(result);
		AddSpeechPartCartIsEmpty(result, getBrowseViewModelFunction);
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		AddSpeechPartDailyRentalCharge(result);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForCartEmpty(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "CheckForCartEmpty",
			StartPause = 250
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			string text = ((CartCount == 0) ? "CartIsEmpty" : "ListCartItems");
			speechPart.RegularExpression = new RegularExpression
			{
				Expression = "Jump",
				Value = "Jump",
				Success = text,
				Failure = text
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartCartIsEmpty(ISpeechControl speechControl, Func<ShoppingCartViewModel> getShoppingCartViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "CartIsEmpty",
			Loop = true,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ShoppingCartViewModel shoppingCartViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnBackButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnUpdateBagButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Your Cart is Empty."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, Press the CANCEL key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To update you cart, press 3."
				});
				AddSignInEventAndText(speechPart, shoppingCartViewModel);
				AddUsePointsEventAndText(speechPart, shoppingCartViewModel);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSignInEventAndText(ISpeechPart speechPart, ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel.SignInButtonVisibility == Visibility.Visible)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To sign in press 4."
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "4",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnSignInButtonClicked();
				}
			});
		}
	}

	private static void AddUsePointsEventAndText(ISpeechPart speechPart, ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel.UsePointsButtonVisibility == Visibility.Visible)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To use points press 4."
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "4",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnUsePointsButtonClicked();
				}
			});
		}
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<ShoppingCartViewModel> getShoppingCartViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 2,
			Name = "ListCartItems",
			Loop = true,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ShoppingCartViewModel shoppingCartViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				if (shoppingCartViewModel.PayButtonEnabled)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "ENTER",
						Action = delegate
						{
							shoppingCartViewModel.ProcessOnPayButtonClicked();
						}
					});
				}
				if (shoppingCartViewModel.PromoCodeButtonEnabled)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "1",
						Action = delegate
						{
							shoppingCartViewModel.ProcessOnPromoCodeButtonClicked();
						}
					});
				}
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnTermsAndPrivacyButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnBackButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnUpdateBagButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "5",
					Function = "TTSSkipToSpeechPart",
					Value = "DailyRentalCharges"
				});
				if (shoppingCartViewModel.UseCreditsButtonEnabled)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "6",
						Action = delegate
						{
							shoppingCartViewModel.ProcessOnUseCreditsButtonClicked();
						}
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = string.Format("Your cart has {0} title{1}.", CartCount, (CartCount != 1) ? "s" : null)
				});
				if (CartCount > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "The titles in your cart are as follows:"
					});
					int num = 0;
					foreach (IRentalShoppingCartItem item in RentalShoppingCartService.CurrentRentalShoppingCart.Items)
					{
						DisplayCheckoutProductModel displayCheckoutProductModel = shoppingCartViewModel.DisplayProductModels[num];
						num++;
						string text = null;
						text = ((item.Action != RentalShoppingCartItemAction.purchase) ? displayCheckoutProductModel.ProductDescriptionLine2Text.Replace("|", $"{item.Price:C}. ") : $"{item.Price:C} {item.Action}");
						speechPart.Texts.Add(new TextPart
						{
							Text = $"{item.Action} title {displayCheckoutProductModel.Name} {text}."
						});
					}
				}
				if (!string.IsNullOrEmpty(shoppingCartViewModel?.PricingModel?.DiscountValueText))
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = string.Format("The discount amount is {0}.", shoppingCartViewModel?.PricingModel?.DiscountValueText.Replace("-", string.Empty))
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = $"The subtotal amount is {shoppingCartViewModel?.PricingModel?.SubtotalValueText} and the tax is {shoppingCartViewModel?.PricingModel?.TaxValueText}."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = $"The total amount that will be charged to your credit card will be ${shoppingCartViewModel?.PricingModel?.TotalValueText}."
				});
				if (shoppingCartViewModel.PayButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To pay with a credit card, press ENTER."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To update your cart press 3."
				});
				if (shoppingCartViewModel.PromoCodeButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "If you have a promo code, and would like to use it now, press 1."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "By paying with credit card or by using credits you agree to the terms."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To hear our terms and privacy in full, press 2."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, press the CANCEL key."
				});
				AddSignInEventAndText(speechPart, shoppingCartViewModel);
				AddUsePointsEventAndText(speechPart, shoppingCartViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = "To know more about our daily rental charges, press 5."
				});
				if (shoppingCartViewModel.UseCreditsButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To use credits, press 6."
					});
				}
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartDailyRentalCharge(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 3,
			Name = "DailyRentalCharges",
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "CANCEL",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "MINUS",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "ENTER",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = Resources.passive_sale_note
			});
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddNextAudioSnippetEvent(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "TAB",
			Function = "TTSNextAudioSnippet"
		});
	}
}
