using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Xps.Packaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewController : BaseViewController
{
	private const string LoyaltyTimeoutTimerName = "loyalty_help_view_timer";

	public static void ShowFullHelpView(bool allowFMA, bool restartTimer, string analyticsContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			ShowFaq = true,
			ShowTerms = true,
			ShowRedboxCard = true,
			ShowPrivacy = true,
			ShowPerks = true,
			ShowMobileFaq = true,
			AnalysisContext = analyticsContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			CreateAnalyticsEvent("buttonPress", "back");
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			string obj = service?.Peek(1);
			if (restartTimer)
			{
				OrchestrationService.ResetApplicationIdleTimerTimeout();
			}
			if (obj == service?.GetStartOverViewName() && !OrchestrationService.IsPersonalizationSessionSignedIn())
			{
				ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
			}
			else
			{
				OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
			}
		};
		if (allowFMA)
		{
			helpViewControllerParameters.ClickRedboxLogoAction = delegate
			{
				OrchestrationService.ExecuteScript("activate_fa");
			};
		}
		if (restartTimer)
		{
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.StartIdleTimer("idle_timer");
		}
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowTermsAndConditions(string analysisContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			ShowFaq = true,
			ShowTerms = true,
			ShowRedboxCard = true,
			ShowPrivacy = true,
			ShowPerks = true,
			ShowMobileFaq = true,
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			CreateAnalyticsEvent("buttonPress", "back");
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.Terms;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.ResetApplicationIdleTimerTimeout();
		OrchestrationService.StartIdleTimer("idle_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowMobileFaqAndPrivacy(string analysisContext, string idleTimerName = "idle_timer")
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			ShowFaq = false,
			ShowTerms = false,
			ShowRedboxCard = false,
			ShowPrivacy = true,
			ShowPerks = false,
			ShowMobileFaq = true,
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			CreateAnalyticsEvent("buttonPress", "back");
			OrchestrationService.StopIdleTimer("loyalty_help_view_timer");
			OrchestrationService.RemoveIdleTimer("loyalty_help_view_timer");
			OrchestrationService.ResumeIdleTimer(idleTimerName);
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			if (ServiceLocator.Instance.GetService<IResourceBundleService>().GetResource("document-mobile-faqs") != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.MobileFaq;
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.Privacy;
			}
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.SuspendIdleTimer(idleTimerName);
		OrchestrationService.CreateIdleTimer("loyalty_help_view_timer");
		OrchestrationService.StartIdleTimer(40000, helpViewControllerParameters.GoBackAction, "loyalty_help_view_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowJoinPerks(Action goBackAction)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			ShowFaq = false,
			ShowTerms = false,
			ShowRedboxCard = false,
			ShowPrivacy = false,
			ShowPerks = false,
			ShowMobileFaq = false,
			AnalysisContext = "perks_terms",
			GoBackAction = delegate
			{
				OrchestrationService.StopIdleTimer("loyalty_help_view_timer");
				OrchestrationService.RemoveIdleTimer("loyalty_help_view_timer");
				goBackAction?.Invoke();
			}
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.PlayPass;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.CreateIdleTimer("loyalty_help_view_timer");
		OrchestrationService.StartIdleTimer(40000, helpViewControllerParameters.GoBackAction, "loyalty_help_view_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowHelpView(bool giftCardEnabled, Action goBackAction, Action clickRedboxLogoAction, bool showAsPopup)
	{
		OrchestrationService.ViewShow("help_document_view", new HelpViewControllerParameters
		{
			GoBackAction = goBackAction,
			ClickRedboxLogoAction = clickRedboxLogoAction,
			ShowFaq = !showAsPopup,
			ShowTerms = !showAsPopup,
			ShowRedboxCard = !showAsPopup,
			ShowPrivacy = !showAsPopup,
			ShowPerks = !showAsPopup,
			ShowMobileFaq = !showAsPopup
		});
	}

	public static void ConfigureHelpView(HelpViewControllerParameters parameters)
	{
		OrchestrationService.SetCurrentUICulture();
		HelpViewUserControl helpViewUserControl = (HelpViewUserControl)BaseViewController.GetViewUserControl();
		if (helpViewUserControl == null)
		{
			return;
		}
		if (!string.IsNullOrWhiteSpace(parameters.AnalysisContext))
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("context", parameters.AnalysisContext);
		}
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		HelpViewModel viewModel = (HelpViewModel)helpViewUserControl.DataContext;
		if (viewModel == null)
		{
			viewModel = new HelpViewModel();
		}
		viewModel.OnHelpDocumentChanged += delegate(HelpDocuments helpDocument)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocument;
			}
		};
		viewModel.OnHelpDocumentPageChanged += delegate(int pageNumber)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = pageNumber;
			}
		};
		viewModel.OnHelpDocumentButtonPress += delegate(HelpDocuments helpDocument)
		{
			HelpViewModel.HelpDocumentInfo value2 = null;
			viewModel.HelpDocumentInfoList.TryGetValue(helpDocument, out value2);
			if (value2 != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocument;
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
				viewModel.CurrentHelpDocument = helpDocument;
				viewModel.CurrentHelpDocumentPage = 1;
				viewModel.XpsDocument = LoadXpsDocument(value2);
				SetTitle(viewModel);
				viewModel.ShowContactText = viewModel.CurrentHelpDocument == HelpDocuments.Faq || viewModel.CurrentHelpDocument == HelpDocuments.MobileFaq;
				CreateAnalyticsEvent("buttonPress", value2.AnalyticsButtonName);
			}
		};
		viewModel.OnGoBack += parameters.GoBackAction;
		viewModel.OnRedboxLogoCommand += parameters.ClickRedboxLogoAction;
		viewModel.OnHelpPaddleButtonPressed += delegate(string source)
		{
			string arg = ((source == "Left") ? "help-page-back," : "help-page-next,");
			arg = $"{arg}{viewModel.TitleText}";
			CreateAnalyticsEvent("pageChange", arg);
		};
		if (service != null)
		{
			viewModel.StoreNumberText = service["StoreNumber"];
		}
		viewModel.BackButtonText = Resources.back_button;
		viewModel.ContactText = Resources.contact_redbox_label;
		viewModel.PageCounterText = Resources.help_page_counter;
		viewModel.StoreNumberLabel = Resources.kiosk_id_label;
		viewModel.CloseButtonText = Resources.close_button;
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		bool value = false;
		service2.TryGetValue<bool>(ConfigFile.System, ConfigCategory.General, ConfigSetting.GiftCardsEnabled, out value);
		BuildHelpDocInfoList(value, viewModel, service2);
		HelpDocuments helpDocuments = Redbox.Rental.Services.Session.Session.Instance?.HelpState?.CurrentHelpDocument ?? HelpDocuments.Faq;
		BuildHelpDocButtons(parameters, viewModel, service2, helpDocuments);
		viewModel.CurrentHelpDocument = helpDocuments;
		viewModel.XpsDocument = LoadXpsDocument(viewModel.HelpDocumentInfoList[helpDocuments]);
		SetTitle(viewModel);
		viewModel.ShowContactText = helpDocuments == HelpDocuments.Faq || viewModel.CurrentHelpDocument == HelpDocuments.MobileFaq;
		ConfigureTTS(viewModel);
		helpViewUserControl.DataContext = viewModel;
		viewModel.CurrentHelpDocumentPage = Redbox.Rental.Services.Session.Session.Instance?.HelpState?.CurrentHelpDocumentPage ?? 1;
	}

	private static void BuildHelpDocButtons(HelpViewControllerParameters parameters, HelpViewModel viewModel, IConfigurationService config, HelpDocuments currentHelpDoc)
	{
		viewModel.HelpButtonList = new ObservableCollection<HelpButtonData>();
		if (parameters.ShowFaq)
		{
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowFAQUpdated, out var value);
			viewModel.HelpButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.faq_button,
				UpdatedText = Resources.help_updated_button,
				ShowUpdatedFlag = value,
				DocumentToShow = HelpDocuments.Faq,
				IsEnabled = (currentHelpDoc != HelpDocuments.Faq)
			});
		}
		if (parameters.ShowTerms)
		{
			bool value = false;
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowTermsUpdated, out value);
			viewModel.HelpButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.transaction_terms_button,
				UpdatedText = Resources.help_updated_button,
				ShowUpdatedFlag = value,
				DocumentToShow = HelpDocuments.Terms,
				IsEnabled = (currentHelpDoc != HelpDocuments.Terms)
			});
		}
		if (parameters.ShowRedboxCard)
		{
			bool value2 = false;
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.General, ConfigSetting.GiftCardsEnabled, out value2);
			if (value2)
			{
				bool value = false;
				config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowRedboxCardUpdated, out value);
				viewModel.HelpButtonList.Add(new HelpButtonData
				{
					ButtonText = Resources.terms_redbox_card_button,
					UpdatedText = Resources.help_updated_button,
					ShowUpdatedFlag = value,
					DocumentToShow = HelpDocuments.TermsReboxCard,
					IsEnabled = (currentHelpDoc != HelpDocuments.TermsReboxCard)
				});
			}
		}
		if (parameters.ShowPrivacy)
		{
			bool value = false;
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowPrivacyUpdated, out value);
			viewModel.HelpButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.privacy_button,
				UpdatedText = Resources.help_updated_button,
				ShowUpdatedFlag = value,
				DocumentToShow = HelpDocuments.Privacy,
				IsEnabled = (currentHelpDoc != HelpDocuments.Privacy)
			});
		}
		if (parameters.ShowPerks && ServiceLocator.Instance.GetService<IResourceBundleService>().GetResource("document-perks") != null)
		{
			bool value = false;
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowPerksUpdated, out value);
			viewModel.HelpButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.play_pass_button,
				UpdatedText = Resources.help_updated_button,
				ShowUpdatedFlag = value,
				DocumentToShow = HelpDocuments.PlayPass,
				IsEnabled = (currentHelpDoc != HelpDocuments.PlayPass)
			});
		}
		if (parameters.ShowMobileFaq && ServiceLocator.Instance.GetService<IResourceBundleService>().GetResource("document-mobile-faqs") != null)
		{
			bool value = false;
			config.TryGetValue<bool>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.ShowMobileFaqUpdated, out value);
			viewModel.HelpButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.mobile_faq_button,
				UpdatedText = Resources.help_updated_button,
				ShowUpdatedFlag = value,
				DocumentToShow = HelpDocuments.MobileFaq,
				IsEnabled = (currentHelpDoc != HelpDocuments.MobileFaq)
			});
		}
	}

	private static void BuildHelpDocInfoList(bool giftCardEnabled, HelpViewModel viewModel, IConfigurationService config)
	{
		string documentName = "document-faqs";
		string documentName2 = "document-terms";
		string documentName3 = "document-terms-redbox-card";
		string documentName4 = "document-privacy-policy";
		string documentName5 = "document-perks";
		string documentName6 = "document-mobile-faqs";
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.FAQDocument, out var value))
		{
			documentName = value;
		}
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.TermsDocument, out value))
		{
			documentName2 = value;
		}
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.RedboxCardDocument, out value))
		{
			documentName3 = value;
		}
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.PrivacyDocument, out value))
		{
			documentName4 = value;
		}
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.PerksDocument, out value))
		{
			documentName5 = value;
		}
		if (config.TryGetValue<string>(ConfigFile.System, ConfigCategory.Documents, ConfigSetting.MobileFaqDocument, out value))
		{
			documentName6 = value;
		}
		viewModel.HelpDocumentInfoList = new Dictionary<HelpDocuments, HelpViewModel.HelpDocumentInfo>();
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Faq, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName,
			DefaultDocumentName = "document-faqs",
			Title = Resources.faq_title,
			Visible = true,
			AnalyticsButtonName = "show_faq",
			AbeTitle = Resources.faq_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Terms, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName2,
			DefaultDocumentName = "document-terms",
			Title = Resources.terms_of_use_title,
			Visible = true,
			AnalyticsButtonName = "show_terms",
			AbeTitle = Resources.terms_of_use_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.TermsReboxCard, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName3,
			DefaultDocumentName = "document-terms-redbox-card",
			Title = Resources.terms_of_use_title,
			Visible = giftCardEnabled,
			AnalyticsButtonName = "show_terms_redbox_card",
			AbeTitle = Resources.redbox_card_terms_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Privacy, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName4,
			DefaultDocumentName = "document-privacy-policy",
			Title = Resources.privacy_policy_title,
			Visible = true,
			AnalyticsButtonName = "show_privacy",
			AbeTitle = Resources.privacy_policy_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.PlayPass, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName5,
			DefaultDocumentName = "document-perks",
			Title = Resources.play_pass_title,
			Visible = true,
			AnalyticsButtonName = "show_play_pass",
			AbeTitle = Resources.play_pass_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.MobileFaq, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = documentName6,
			DefaultDocumentName = "document-mobile-faqs",
			Title = Resources.mobile_faq_title,
			Visible = true,
			AnalyticsButtonName = "show_mobile_faq",
			AbeTitle = Resources.mobile_faq_title_abe
		});
	}

	private static void SetTitle(HelpViewModel viewModel)
	{
		HelpViewModel.HelpDocumentInfo value = null;
		viewModel.HelpDocumentInfoList.TryGetValue(viewModel.CurrentHelpDocument, out value);
		if (value != null)
		{
			viewModel.TitleText = value.Title;
		}
	}

	private static XpsDocument LoadXpsDocument(HelpViewModel.HelpDocumentInfo helpDocumentInfo)
	{
		XpsDocument xpsDocument = null;
		if (helpDocumentInfo != null)
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				xpsDocument = service.GetXpsDocument(helpDocumentInfo.DocumentName);
				if (xpsDocument == null)
				{
					LogHelper.Instance.Log("Unable to get Xps document {0}", helpDocumentInfo.DocumentName);
					if (!helpDocumentInfo.DocumentName.Equals(helpDocumentInfo.DefaultDocumentName, StringComparison.CurrentCultureIgnoreCase))
					{
						xpsDocument = service.GetXpsDocument(helpDocumentInfo.DefaultDocumentName);
						if (xpsDocument == null)
						{
							LogHelper.Instance.Log("Unable to get default Xps document {0}", helpDocumentInfo.DefaultDocumentName);
						}
					}
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("Help document info missing in LoadXpsDocument");
		}
		return xpsDocument;
	}

	internal static void CreateAnalyticsEvent(string subItemType, string subItemValue)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return;
		}
		IShoppingSession currentSession = service.GetCurrentSession();
		if (currentSession != null)
		{
			_ = currentSession.Id;
			ISessionAnalyticsService service2 = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			if (service2 != null && service2.SessionExists(currentSession.Id.ToString()))
			{
				service2.AddNode("viewEvents", "viewEvent", currentSession.Id.ToString());
				service2.AddAttribute("viewEvent", "subItemType", subItemType, currentSession.Id.ToString());
				service2.AddAttribute("viewEvent", "subItemValue", subItemValue, currentSession.Id.ToString());
			}
		}
	}

	private static HelpViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as HelpViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(HelpViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => HelpViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	internal static void LeaveHelpView()
	{
	}
}
