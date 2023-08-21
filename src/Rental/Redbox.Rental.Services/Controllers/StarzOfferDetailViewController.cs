using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.Starz;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class StarzOfferDetailViewController : BaseViewController
{
	internal static void ConfigureView(IStarzSubscriptionProduct starzSubscriptionProduct)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			StarzOfferDetailViewModel starzOfferDetailViewModel = new StarzOfferDetailViewModel
			{
				StarzSubscriptionProduct = starzSubscriptionProduct
			};
			ConfigureTextAndButtons(starzOfferDetailViewModel);
			ConfigureActions(starzOfferDetailViewModel);
			ConfigureTTS(starzOfferDetailViewModel);
			viewUserControl.DataContext = starzOfferDetailViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ConfigureTextAndButtons(StarzOfferDetailViewModel starzOfferDetailViewModel)
	{
		BaseViewController.SetCurrentUICulture();
		UserControl offerUserControl = null;
		switch (starzOfferDetailViewModel?.StarzSubscriptionProduct?.SubscriptionId)
		{
		case "STARZ_OFFER_1":
		case "STARZ_OFFER_2":
		case "STARZ_OFFER_3":
			offerUserControl = new StarzOfferDetailSingleColumnUserControl();
			break;
		case "STARZ_OFFER_4":
			offerUserControl = new StarzOfferDetailTwoColumnsUserControl();
			break;
		default:
			LogHelper.Instance.Log("Error: Unable to find user control for Starz Offer: " + starzOfferDetailViewModel?.StarzSubscriptionProduct?.SubscriptionId);
			break;
		}
		starzOfferDetailViewModel.OfferUserControl = offerUserControl;
		bool flag = (ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart).HasSubscriptionItem(starzOfferDetailViewModel.StarzSubscriptionProduct);
		starzOfferDetailViewModel.AcceptButtonLegalText = (flag ? null : Resources.starz_offer_accept_button_legal_text);
		starzOfferDetailViewModel.NotReadyYetText = (flag ? null : Resources.starz_offer_not_ready_yet);
		starzOfferDetailViewModel.ScanQRCodeButtonText = (flag ? null : Resources.starz_offer_scan_qr_code);
		starzOfferDetailViewModel.ScanQRCodeButtonVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		starzOfferDetailViewModel.TermsButton1Text = Resources.starz_offer_terms_button_1_text;
		starzOfferDetailViewModel.TermsButton2Text = Resources.starz_offer_terms_button_2_text;
		starzOfferDetailViewModel.TermsButton1Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		starzOfferDetailViewModel.TermsButton2Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		starzOfferDetailViewModel.BackButtonText = Resources.back_button.ToUpper();
		starzOfferDetailViewModel.BackButtonVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		starzOfferDetailViewModel.TitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.TitleText;
		starzOfferDetailViewModel.TitleClause2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.TitleClause2Text;
		starzOfferDetailViewModel.TitleClause3Text = starzOfferDetailViewModel.StarzSubscriptionProduct.TitleClause3Text;
		starzOfferDetailViewModel.SubtitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.SubtitleText;
		starzOfferDetailViewModel.SubtitleAsteriskText = starzOfferDetailViewModel.StarzSubscriptionProduct.SubtitleAsteriskText;
		starzOfferDetailViewModel.SubtitleClause2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.SubtitleClause2Text;
		starzOfferDetailViewModel.Subtitle2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.Subtitle2Text;
		starzOfferDetailViewModel.Subtitle2Visibility = (string.IsNullOrEmpty(starzOfferDetailViewModel.StarzSubscriptionProduct.Subtitle2Text) ? Visibility.Collapsed : Visibility.Visible);
		starzOfferDetailViewModel.Bullet1Text1 = starzOfferDetailViewModel.StarzSubscriptionProduct.Bullet1Text1;
		starzOfferDetailViewModel.RegisteredTrademarkSymbol = starzOfferDetailViewModel.StarzSubscriptionProduct.RegisteredTrademarkSymbolBullet;
		starzOfferDetailViewModel.Bullet1Text2 = starzOfferDetailViewModel.StarzSubscriptionProduct.Bullet1Text2;
		starzOfferDetailViewModel.Bullet2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.Bullet2Text;
		starzOfferDetailViewModel.Bullet3Text = starzOfferDetailViewModel.StarzSubscriptionProduct.Bullet3Text;
		starzOfferDetailViewModel.LeftTitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.LeftTitleText;
		starzOfferDetailViewModel.RightTitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.RightTitleText;
		starzOfferDetailViewModel.LeftSubtitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.LeftSubtitleText;
		starzOfferDetailViewModel.RightSubtitleText = starzOfferDetailViewModel.StarzSubscriptionProduct.RightSubtitleText;
		starzOfferDetailViewModel.LeftSubtitle2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.LeftSubtitle2Text;
		starzOfferDetailViewModel.RightSubtitle2Text = starzOfferDetailViewModel.StarzSubscriptionProduct.RightSubtitle2Text;
		starzOfferDetailViewModel.AcceptButtonText = (flag ? Resources.starz_offer_accept_button_text_already_in_cart : starzOfferDetailViewModel.StarzSubscriptionProduct.AcceptButtonText);
		starzOfferDetailViewModel.LegalText = starzOfferDetailViewModel.StarzSubscriptionProduct.LegalText;
	}

	private static void ConfigureActions(StarzOfferDetailViewModel starzOfferDetailViewModel)
	{
		starzOfferDetailViewModel.AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AcceptButtonCommand_Execute);
		starzOfferDetailViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackButtonCommand_Execute);
		starzOfferDetailViewModel.ScanQRCodeButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), ScanQACodeButtonCommand_Execute);
		starzOfferDetailViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), TermsButtonCommand_Execute);
	}

	private static void AcceptButtonCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		StarzOfferDetailViewModel viewModel = BaseViewController.GetViewModel<StarzOfferDetailViewModel>();
		if (viewModel == null)
		{
			return;
		}
		if ((ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart).HasSubscriptionItem(viewModel.StarzSubscriptionProduct))
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzOfferViewInBagButton").AddData(ProductData.ToProductData(viewModel.StarzSubscriptionProduct));
			if (OrchestrationService.PeekViewFrameByName("shopping_cart_view", out var _) == null)
			{
				if (OrchestrationService.PeekViewFrameByName("browse_view", out var _) != null)
				{
					OrchestrationService.UnwindToSafe("browse_view", discard: true, "StarzOfferDetailViewInBagButton");
				}
			}
			else
			{
				OrchestrationService.UnwindToSafe("shopping_cart_view", discard: true, "StarzOfferDetailViewInBagButton");
			}
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzOfferDetailAcceptButton").AddData(ProductData.ToProductData(viewModel.StarzSubscriptionProduct));
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn == true)
			{
				StarzSignupService.StartStarzSignup(viewModel.StarzSubscriptionProduct);
			}
			else
			{
				PersonalizationFunctions.ShowStarzSignInSignUpDialog(viewModel.StarzSubscriptionProduct);
			}
		}
	}

	private static void ScanQACodeButtonCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzOfferScanQRCodeButton");
		StarzOfferDetailViewModel viewModel = BaseViewController.GetViewModel<StarzOfferDetailViewModel>();
		string text = viewModel?.StarzSubscriptionProduct?.DisplayQRCodeURL;
		string text2 = viewModel?.StarzSubscriptionProduct?.OfferDetailsQRCodeURL;
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			Action action = delegate
			{
				OrchestrationService.ViewGoBackSafe("QR Code Done", popOnly: false);
			};
			BitmapImage bitmapImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(text2, returnDefaultOnError: true);
			if (bitmapImage != null)
			{
				MessagePopupViewController.ShowQRCodeDialog(bitmapImage, text, action, action);
			}
			else
			{
				LogHelper.Instance.Log("Unable to get Starz offer QR code image for url " + text2);
			}
		}
		else
		{
			LogHelper.Instance.Log("Starz offer QR code url is null");
		}
	}

	private static void TermsButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzOfferTermsButton");
		HelpViewController.ShowSubscriptions();
	}

	private static void BackButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzOfferDetailBackButton");
		OrchestrationService.ViewGoBack("StarzOfferDetailView Back Button", popOnly: false);
	}

	private static void ConfigureTTS(StarzOfferDetailViewModel starzOfferDetailViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || starzOfferDetailViewModel == null)
		{
			return;
		}
		starzOfferDetailViewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_" + OrchestrationService.PeekView()
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 5000
			};
			Func<string, string> makeTextSpeakable = (string source) => source.Replace("/", " per ").Replace(" Mo ", " Month ").Replace("add’l", "additional");
			part.Refresh = delegate
			{
				part.Clear();
				part.Texts.Add(new TextPart
				{
					Text = "Starz Subscription offer: " + makeTextSpeakable(starzOfferDetailViewModel.TitleText)
				});
				part.Texts.Add(new TextPart
				{
					Text = starzOfferDetailViewModel.SubtitleText
				});
				part.Texts.Add(new TextPart
				{
					Text = starzOfferDetailViewModel.Subtitle2Text
				});
				part.Texts.Add(new TextPart
				{
					Text = "Subscription details: " + makeTextSpeakable(starzOfferDetailViewModel.Bullet1Text1)
				});
				part.Texts.Add(new TextPart
				{
					Text = makeTextSpeakable(starzOfferDetailViewModel.RegisteredTrademarkSymbol)
				});
				part.Texts.Add(new TextPart
				{
					Text = makeTextSpeakable(starzOfferDetailViewModel.Bullet1Text2)
				});
				part.Texts.Add(new TextPart
				{
					Text = makeTextSpeakable(starzOfferDetailViewModel.Bullet2Text)
				});
				part.Texts.Add(new TextPart
				{
					Text = makeTextSpeakable(starzOfferDetailViewModel.Bullet3Text)
				});
				part.Texts.Add(new TextPart
				{
					Text = starzOfferDetailViewModel.AcceptButtonLegalText
				});
				part.Texts.Add(new TextPart
				{
					Text = "To accept this subscription offer press ENTER"
				});
				part.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen press CANCEL"
				});
				part.Texts.Add(new TextPart
				{
					Text = "For terms and conditions press 1"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Legal footnote: " + makeTextSpeakable(starzOfferDetailViewModel.LegalText)
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						AcceptButtonCommand_Execute();
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						BackButtonCommand_Execute();
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						TermsButtonCommand_Execute();
					}
				});
			};
			obj.SpeechParts.Add(part);
			int i = 0;
			obj.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.Sequence = i++;
			});
			return obj;
		};
	}

	internal static void ClosingView()
	{
	}
}
