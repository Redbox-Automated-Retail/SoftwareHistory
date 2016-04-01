using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.Views.HelpView;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewController
{
	internal static void UpdateMacrosAndText(HelpViewUserControl helpViewUserControl)
	{
		if (helpViewUserControl == null)
		{
			return;
		}
		HelpViewUserControl.HelpDocumentInfo value = null;
		helpViewUserControl.HelpDocumentInfoList.TryGetValue(helpViewUserControl.CurrentHelpDocument, out value);
		if (value != null)
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				string text = null;
				string arg = string.Format(arg2: service["Help:Title"] = helpViewUserControl.CurrentHelpDocument switch
				{
					HelpDocuments.Faq => "Rental Frequently Asked Questions", 
					HelpDocuments.Terms => "Rental Terms", 
					_ => value.Title, 
				}, format: "Page {0} of {1} in document {2} is as follows:", arg0: helpViewUserControl.CurrentHelpDocumentPage, arg1: helpViewUserControl.HelpDocumentPageCount);
				service["Help:DocumentText"] = $"{arg} {helpViewUserControl.CurrentPageText}";
			}
			helpViewUserControl.SetString("contact_redbox_label", (helpViewUserControl.CurrentHelpDocument == HelpDocuments.Faq) ? BundleFunctions.GetDictionaryStringValue("strings", "contact_redbox_label") : null);
		}
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service2 != null && service2.TTSEnabled)
		{
			service2.TTSRepeatSequence();
		}
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

	public static void ConfigureHelpView(bool giftCardEnabled, Action goBackAction, Action clickRedboxLogoAction)
	{
		IActor actor = ActorFunctions.GetActor("HelpViewUserControl");
		if (actor == null)
		{
			return;
		}
		HelpViewUserControl helpViewUserControl = actor.WPFFrameworkElement as HelpViewUserControl;
		if (helpViewUserControl == null)
		{
			return;
		}
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		helpViewUserControl.OnHelpDocumentChanged += delegate(HelpDocuments helpDocument)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocument;
		};
		helpViewUserControl.OnHelpDocumentPageChanged += delegate(int pageNumber)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = pageNumber;
		};
		helpViewUserControl.OnHelpDocumentButtonPress += delegate
		{
			UpdateMacrosAndText(helpViewUserControl);
			HelpViewUserControl.HelpDocumentInfo value = null;
			helpViewUserControl.HelpDocumentInfoList.TryGetValue(helpViewUserControl.CurrentHelpDocument, out value);
			if (value != null)
			{
				CreateAnalyticsEvent("buttonPress", value.AnalyticsButtonName);
			}
		};
		helpViewUserControl.OnGoBack += goBackAction;
		helpViewUserControl.OnRedboxLogoCommand += clickRedboxLogoAction;
		helpViewUserControl.OnHelpPaddleButtonPressed += delegate(string source)
		{
			UpdateMacrosAndText(helpViewUserControl);
			string text = ((source == "Left") ? "help-page-back," : "help-page-next,");
			if (macroService != null)
			{
				text = string.Format("{0}{1}", text, macroService["Help:Title"]);
			}
			CreateAnalyticsEvent("pageChange", text);
		};
		helpViewUserControl.HelpDocumentInfoList.Add(HelpDocuments.Faq, new HelpViewUserControl.HelpDocumentInfo
		{
			DocumentName = "document-faqs",
			Title = BundleFunctions.GetDictionaryStringValue("strings", "faq_title"),
			Visible = true,
			AnalyticsButtonName = "show_faq"
		});
		helpViewUserControl.HelpDocumentInfoList.Add(HelpDocuments.Terms, new HelpViewUserControl.HelpDocumentInfo
		{
			DocumentName = "document-terms",
			Title = BundleFunctions.GetDictionaryStringValue("strings", "terms_of_use_title"),
			Visible = true,
			AnalyticsButtonName = "show_terms"
		});
		helpViewUserControl.HelpDocumentInfoList.Add(HelpDocuments.TermsReboxCard, new HelpViewUserControl.HelpDocumentInfo
		{
			DocumentName = "document-terms-redbox-card",
			Title = BundleFunctions.GetDictionaryStringValue("strings", "terms_of_use_title"),
			Visible = giftCardEnabled,
			AnalyticsButtonName = "show_terms_redbox_card"
		});
		helpViewUserControl.HelpDocumentInfoList.Add(HelpDocuments.Privacy, new HelpViewUserControl.HelpDocumentInfo
		{
			DocumentName = "document-privacy-policy",
			Title = BundleFunctions.GetDictionaryStringValue("strings", "privacy_policy_title"),
			Visible = true,
			AnalyticsButtonName = "show_privacy"
		});
		BundleFunctions.GetDictionary("strings");
		helpViewUserControl.SetString("ButtonGoBack_Text", BundleFunctions.GetDictionaryStringValue("strings", "back_button"));
		helpViewUserControl.SetString("contact_redbox_label", BundleFunctions.GetDictionaryStringValue("strings", "contact_redbox_label"));
		helpViewUserControl.SetString("help_page_counter", BundleFunctions.GetDictionaryStringValue("strings", "help_page_counter"));
		if (macroService != null)
		{
			helpViewUserControl.SetString("store_number_text", macroService["StoreNumber"]);
		}
		helpViewUserControl.SetString("ButtonFAQ_Text", BundleFunctions.GetDictionaryStringValue("strings", "faq_button"));
		helpViewUserControl.SetString("ButtonTerms_Text", BundleFunctions.GetDictionaryStringValue("strings", "terms_button"));
		helpViewUserControl.SetString("ButtonTermsRedboxCard_Text", BundleFunctions.GetDictionaryStringValue("strings", "terms_redbox_card_button"));
		helpViewUserControl.SetString("ButtonPrivacy_Text", BundleFunctions.GetDictionaryStringValue("strings", "privacy_button"));
		helpViewUserControl.CurrentHelpDocument = Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument;
		helpViewUserControl.CurrentHelpDocumentPage = Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage;
		UpdateMacrosAndText(helpViewUserControl);
	}
}
