using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PresentOffersViewController : BaseViewController
{
	private static Action<CustomerOffer> _continueAction;

	private static Action _cancelAction;

	private const string _viewTimerName = "PresentOffersViewTimer";

	public static void ConfigureView(PresentOffersViewInfo parameters)
	{
		_cancelAction = parameters.CancelAction;
		_continueAction = parameters.ContinueAction;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			PresentOffersViewModel presentOffersViewModel = new PresentOffersViewModel();
			presentOffersViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CancelButtonClickedAction();
			}, (Func<bool>)null);
			DynamicRoutedCommand continueCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(CustomerOffer o)
			{
				ContinueButtonClickedAction(o);
			});
			PopulateModelText(presentOffersViewModel, parameters, continueCommand);
			ConfigureTTS(presentOffersViewModel);
			viewUserControl.DataContext = presentOffersViewModel;
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int value = ((service != null && service.TTSEnabled) ? 90000 : 25000);
			(ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("PresentOffersViewTimer", value, value, delegate
			{
				LogHelper.Instance.Log("present_offers_view_timer executed");
				parameters.TimeoutAction();
			}))?.Start();
		}
	}

	private static void ContinueButtonClickedAction(CustomerOffer o)
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "ContinueButton");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PresentOffersContinue")?.AddData(new OfferData(o.DiscountType.ToString(), o.PromotionCode, o.Amount));
		OrchestrationService.ViewGoBackSafe("PresentOffersView_OfferSelected", popOnly: false);
		_continueAction(o);
	}

	private static void CancelButtonClickedAction()
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "NoThanksButton");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PresentOffersCancel");
		OrchestrationService.ViewGoBackSafe("PresentOffersView_NoThanks", popOnly: false);
		_cancelAction();
	}

	private static void PopulateModelText(PresentOffersViewModel viewModel, PresentOffersViewInfo parameters, DynamicRoutedCommand continueCommand)
	{
		viewModel.CancelButtonText = Resources.no_thanks_button;
		CustomerOffer customerOffer = parameters.CustomerOffers.FirstOrDefault();
		viewModel.LegalText = Resources.offer_legal_text;
		if (parameters.CustomerOffers.Count > 1)
		{
			viewModel.CancelButtonVisibility = Visibility.Visible;
			viewModel.MessageVisibility = Visibility.Visible;
			viewModel.ExpirationVisibility = Visibility.Collapsed;
		}
		else
		{
			viewModel.CancelButtonVisibility = Visibility.Collapsed;
			viewModel.MessageVisibility = Visibility.Collapsed;
		}
		if (customerOffer != null && customerOffer.OfferScopeType == OfferScopeType.Session)
		{
			viewModel.MessageText = Resources.offer_session_message;
			viewModel.LegalText = Resources.offer_legal_text;
			viewModel.ExpirationVisibility = Visibility.Collapsed;
		}
		else
		{
			viewModel.MessageVisibility = Visibility.Collapsed;
			viewModel.ExpirationVisibility = Visibility.Visible;
			viewModel.ExpirationText = string.Format(Resources.offer_expires_message, parameters.CustomerOffers.First().ExpirationDate.Value.ToString("M/d/yy"));
			viewModel.LegalText = string.Format(Resources.offer_legal_text_with_expiration_date, parameters.CustomerOffers.First().ExpirationDate.Value.ToString("M/d/yy"));
		}
		PopulateOfferInfo(viewModel, parameters, continueCommand);
	}

	private static void PopulateOfferInfo(PresentOffersViewModel viewModel, PresentOffersViewInfo parameters, DynamicRoutedCommand continueCommand)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		viewModel.Offers = new ObservableCollection<OfferInfoModel>();
		bool flag = parameters.CustomerOffers.Count == 1 || parameters.CustomerOffers[0].CustomerSegmentType != CustomerSegmentType.BasketStretch;
		foreach (CustomerOffer customerOffer in parameters.CustomerOffers)
		{
			OrchestrationService.AnalyticsAddViewEvent("offerDisplayed", customerOffer.PromotionCode);
			OfferInfoModel offerInfoModel = new OfferInfoModel();
			offerInfoModel.ControlWidth = (flag ? 1024 : 510);
			offerInfoModel.OneUpVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
			offerInfoModel.TwoUpVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
			offerInfoModel.CancelButtonVisibility = ((!flag || customerOffer.OfferScopeType == OfferScopeType.Storeable) ? Visibility.Collapsed : Visibility.Visible);
			offerInfoModel.CancelButtonCommand = viewModel.CancelButtonCommand;
			offerInfoModel.ContinueButtonCommand = continueCommand;
			offerInfoModel.CustomerOffer = customerOffer;
			TitleFamily? titleFamilyFromOffer = CustomerOfferFunctions.GetTitleFamilyFromOffer(customerOffer);
			string singularTitleFamilyString = CustomerOfferFunctions.GetSingularTitleFamilyString(titleFamilyFromOffer);
			string pluralTitleFamilyString = CustomerOfferFunctions.GetPluralTitleFamilyString(titleFamilyFromOffer);
			offerInfoModel.StyledTextLines = new ObservableCollection<StyledTextModel>();
			if (customerOffer.OfferScopeType == OfferScopeType.Session)
			{
				SetTextLinesForInCartOffer(viewUserControl, flag, customerOffer, offerInfoModel, singularTitleFamilyString, pluralTitleFamilyString);
			}
			else if (customerOffer.OfferScopeType == OfferScopeType.Storeable)
			{
				SetTextLinesForNextVisitOffer(viewUserControl, flag, customerOffer, offerInfoModel, singularTitleFamilyString);
			}
			else if (customerOffer.OfferScopeType == OfferScopeType.StoredRedeemable)
			{
				SetTextLinesForStoredRedeemableOffer(viewUserControl, flag, customerOffer, offerInfoModel, singularTitleFamilyString);
			}
			viewModel.Offers.Add(offerInfoModel);
		}
	}

	private static void SetTextLinesForStoredRedeemableOffer(FrameworkElement viewUserControl, bool isSingle, CustomerOffer offer, OfferInfoModel offerInfoModel, string singularFamily)
	{
		StyledTextModel styledTextModel = new StyledTextModel();
		styledTextModel.Text = string.Format(Resources.offer_info_hold_up);
		styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_header_bold");
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		styledTextModel = new StyledTextModel();
		styledTextModel.Text = string.Format(Resources.offer_info_still_want_question);
		styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_header");
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		styledTextModel = new StyledTextModel();
		if (offer.DiscountType == OfferDiscountType.AmountOff)
		{
			styledTextModel.Text = string.Format(Resources.offer_info_amount_off, offer.Amount);
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_big_bold_1up" : "offer_info_big_bold_2up");
			offerInfoModel.StyledTextLines.Add(styledTextModel);
			styledTextModel = new StyledTextModel();
			styledTextModel.Text = string.Format(Resources.offer_info_1_night_rental, singularFamily.ToUpper());
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_bold");
		}
		else
		{
			styledTextModel.Text = Resources.offer_info_free_1_night;
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_less_big_bold" : "offer_info_big_bold_2up");
			offerInfoModel.StyledTextLines.Add(styledTextModel);
			styledTextModel = new StyledTextModel();
			styledTextModel.Text = string.Format(Resources.offer_info_disc_rental_caps, singularFamily.ToUpper());
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_bold");
		}
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		offerInfoModel.ContinueButtonText = Resources.apply_now_button_text;
		offerInfoModel.CancelButtonText = Resources.skip_button_text;
	}

	private static void SetTextLinesForNextVisitOffer(FrameworkElement viewUserControl, bool isSingle, CustomerOffer offer, OfferInfoModel offerInfoModel, string singularFamily)
	{
		StyledTextModel styledTextModel = new StyledTextModel();
		styledTextModel.Text = string.Format(Resources.offer_next_visit_header);
		styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_medium_1up" : "offer_info_medium_2up");
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		styledTextModel = new StyledTextModel();
		if (offer.DiscountType == OfferDiscountType.AmountOff)
		{
			styledTextModel.Text = string.Format(Resources.offer_info_amount_off, offer.Amount);
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_big_bold_1up" : "offer_info_big_bold_2up");
			offerInfoModel.StyledTextLines.Add(styledTextModel);
			styledTextModel = new StyledTextModel();
			styledTextModel.Text = string.Format(Resources.offer_info_next_1_night_rental, singularFamily.ToUpper());
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_bold");
		}
		else
		{
			styledTextModel.Text = Resources.offer_info_free_1_night;
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_less_big_bold" : "offer_info_big_bold_2up");
			offerInfoModel.StyledTextLines.Add(styledTextModel);
			styledTextModel = new StyledTextModel();
			styledTextModel.Text = string.Format(Resources.offer_info_disc_rental_caps, singularFamily.ToUpper());
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_medium_bold");
		}
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		styledTextModel = new StyledTextModel();
		styledTextModel.Text = BuildFormatListLineText(offer);
		styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_small_serif");
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		offerInfoModel.ContinueButtonText = Resources.got_it_button;
	}

	private static string BuildFormatListLineText(CustomerOffer offer)
	{
		List<string> list = new List<string>();
		bool flag = true;
		bool flag2 = false;
		List<TitleType> list2;
		if ((offer.FormatIds?.Count ?? 0) > 0 && !offer.FormatIds.Contains(0))
		{
			List<TitleType> titleTypesFromFormatIds = CustomerOfferFunctions.GetTitleTypesFromFormatIds(offer.FormatIds);
			list2 = ((!offer.IncludeFormats) ? CustomerOfferFunctions.GetIncludedTitleTypes(titleTypesFromFormatIds) : titleTypesFromFormatIds);
			flag = CustomerOfferFunctions.AreAllGameTypesIncluded(list2, includeFormats: true);
		}
		else
		{
			list2 = new List<TitleType>();
			if (offer.TitleFamily == TitleFamily.All || offer.TitleFamily == TitleFamily.Movies)
			{
				list2.Add(TitleType.DVD);
				list2.Add(TitleType.Bluray);
				if (CustomerOfferFunctions.KioskHas4kDiscs())
				{
					list2.Add(TitleType._4KUHD);
				}
			}
			if (offer.TitleFamily == TitleFamily.All || offer.TitleFamily == TitleFamily.Games)
			{
				flag2 = true;
			}
		}
		foreach (TitleType item in list2)
		{
			if (item.GetTitleFamily() != TitleFamily.Games)
			{
				if ((item != TitleType._4KUHD || CustomerOfferFunctions.KioskHas4kDiscs()) && item != TitleType.DigitalCode)
				{
					list.Add(GetTitleTypeDisplayString(item));
				}
			}
			else if (flag)
			{
				flag2 = true;
			}
			else
			{
				list.Add(GetTitleTypeDisplayString(item));
			}
		}
		if (flag2)
		{
			list.Add(CustomerOfferFunctions.GetSingularTitleFamilyString(TitleFamily.Games));
		}
		string result = string.Empty;
		if (list.Count > 0)
		{
			result = "(";
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0 && list.Count > 2 && i < list.Count - 1)
				{
					result += ",";
				}
				if (i > 0)
				{
					result += " ";
				}
				if (i == list.Count - 1)
				{
					result = result + Resources.or + " ";
				}
				result += list[i];
			}
			result += ")";
		}
		return result;
	}

	private static string GetTitleTypeDisplayString(TitleType type)
	{
		return type switch
		{
			TitleType.DVD => Resources.dvd, 
			TitleType.Bluray => Resources.bluray_tm, 
			TitleType._4KUHD => Resources.four_k_uhd, 
			_ => type.GetDisplayDescription(), 
		};
	}

	private static void SetTextLinesForInCartOffer(FrameworkElement viewUserControl, bool isSingle, CustomerOffer offer, OfferInfoModel offerInfoModel, string singularFamily, string pluralFamily)
	{
		StyledTextModel styledTextModel = new StyledTextModel();
		styledTextModel.Text = string.Format(Resources.offer_info_add_a_one_night_rental, singularFamily.ToUpper());
		styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_medium_1up" : "offer_info_medium_2up");
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		styledTextModel = new StyledTextModel();
		if (offer.DiscountType == OfferDiscountType.AmountOff)
		{
			styledTextModel.Text = string.Format(Resources.offer_info_amount_off, offer.Amount);
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_big_bold_1up" : "offer_info_big_bold_2up");
		}
		else if (offer.DiscountType == OfferDiscountType.Free)
		{
			styledTextModel.Text = Resources.offer_info_free;
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_big_bold_1up" : "offer_info_big_bold_2up");
		}
		else if (offer.DiscountType == OfferDiscountType.PercentOff)
		{
			styledTextModel.Text = string.Format(Resources.offer_info_percent_off, offer.Amount);
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, isSingle ? "offer_info_big_bold_1up" : "offer_info_big_bold_2up");
		}
		offerInfoModel.CancelButtonText = Resources.no_thanks_button_text;
		offerInfoModel.ContinueButtonText = string.Format(Resources.offer_info_continue_button_text, pluralFamily);
		offerInfoModel.StyledTextLines.Add(styledTextModel);
		if (isSingle)
		{
			styledTextModel = new StyledTextModel();
			styledTextModel.Text = Resources.offer_session_message;
			styledTextModel.Style = WPFFunctions.GetStyle(viewUserControl, "offer_info_small_1up");
			offerInfoModel.StyledTextLines.Add(styledTextModel);
		}
	}

	private static void ConfigureTTS(PresentOffersViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || viewModel == null)
		{
			return;
		}
		viewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_present_offers_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 10000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel?.CancelButtonCommand.Execute(null);
					}
				});
				int count = viewModel.Offers.Count;
				if (count == 1)
				{
					part.Texts.Add(new TextPart
					{
						Text = "You have one offer available."
					});
				}
				else
				{
					part.Texts.Add(new TextPart
					{
						Text = $"You have {count} offers available."
					});
				}
				int num = 1;
				foreach (OfferInfoModel offer in viewModel.Offers)
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (count > 1)
					{
						string arg = "next";
						switch (num)
						{
						case 1:
							arg = "first";
							break;
						case 2:
							arg = "2nd";
							break;
						case 3:
							arg = "third";
							break;
						}
						stringBuilder.Append($"The {arg} offer is ");
					}
					foreach (StyledTextModel styledTextLine in offer.StyledTextLines)
					{
						stringBuilder.Append(" ");
						stringBuilder.Append(styledTextLine.Text);
					}
					part.Texts.Add(new TextPart
					{
						Text = stringBuilder.ToString()
					});
					part.Events.Add(new MapKeyPressToAction
					{
						KeyCode = num.ToString(),
						Action = delegate
						{
							offer.ContinueButtonCommand.Execute(offer.CustomerOffer);
						}
					});
					part.Texts.Add(new TextPart
					{
						Text = $"To Accept this offer press {num}"
					});
					num++;
				}
				if (count > 1)
				{
					part.Texts.Add(new TextPart
					{
						Text = "These Offers are good during this visit only."
					});
					part.Texts.Add(new TextPart
					{
						Text = "To decline all offers and continue with your current cart press Cancel."
					});
				}
				else
				{
					part.Texts.Add(new TextPart
					{
						Text = "To decline this offer and continue with your current cart press Cancel."
					});
				}
				part.Texts.Add(new TextPart
				{
					Text = viewModel?.LegalText
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

	public static void LeaveView()
	{
		OrchestrationService.RemoveTimer("PresentOffersViewTimer");
	}
}
