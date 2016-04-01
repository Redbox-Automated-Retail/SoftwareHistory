using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Views.Controls;

namespace Redbox.Rental.Services.Views.HelpView;

public partial class HelpViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	public class HelpDocumentInfo
	{
		public string DocumentName { get; set; }

		public string Title { get; set; }

		public bool Visible { get; set; }

		public string AnalyticsButtonName { get; set; }
	}

	private List<Border> BorderButtons = new List<Border>();

	private Dictionary<HelpDocuments, HelpDocumentInfo> m_helpDocumentInfoList = new Dictionary<HelpDocuments, HelpDocumentInfo>();

	private HelpDocuments m_currentHelpDocument;

	private int m_currentHelpDocumentPage;

	private string m_helpPageCounter;

	private FixedDocumentSequence m_fixedDocumentSequence;

	private XpsDocument m_xpsDoc;

	public IActor Actor { get; set; }

	public HelpDocuments CurrentHelpDocument
	{
		get
		{
			return m_currentHelpDocument;
		}
		set
		{
			m_currentHelpDocument = value;
			foreach (Border borderButton in BorderButtons)
			{
				borderButton.IsEnabled = (HelpDocuments)borderButton.Tag != m_currentHelpDocument;
				HelpDocumentInfo value2 = null;
				HelpDocumentInfoList.TryGetValue((HelpDocuments)borderButton.Tag, out value2);
				borderButton.Visibility = ((value2 != null && !value2.Visible) ? Visibility.Hidden : Visibility.Visible);
			}
			LoadXpsDocument();
			SetTitle();
			if (OnHelpDocumentChanged != null)
			{
				OnHelpDocumentChanged(m_currentHelpDocument);
			}
		}
	}

	public string CurrentPageText
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (m_xpsDoc != null)
			{
				IXpsFixedDocumentSequenceReader fixedDocumentSequenceReader = m_xpsDoc.FixedDocumentSequenceReader;
				if (fixedDocumentSequenceReader != null)
				{
					IXpsFixedDocumentReader xpsFixedDocumentReader = fixedDocumentSequenceReader.FixedDocuments[0];
					if (xpsFixedDocumentReader != null)
					{
						IXpsFixedPageReader xpsFixedPageReader = xpsFixedDocumentReader.FixedPages[Math.Max(0, CurrentHelpDocumentPage - 1)];
						if (xpsFixedPageReader != null)
						{
							XmlReader xmlReader = xpsFixedPageReader.XmlReader;
							if (xmlReader != null)
							{
								while (xmlReader.Read())
								{
									if (xmlReader.Name == "Glyphs" && xmlReader.HasAttributes)
									{
										string attribute = xmlReader.GetAttribute("UnicodeString");
										stringBuilder.Append(attribute);
									}
								}
							}
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}

	public int HelpDocumentPageCount
	{
		get
		{
			if (m_fixedDocumentSequence == null)
			{
				return 0;
			}
			return m_fixedDocumentSequence.DocumentPaginator.PageCount;
		}
	}

	public int CurrentHelpDocumentPage
	{
		get
		{
			return m_currentHelpDocumentPage;
		}
		set
		{
			if (m_fixedDocumentSequence != null && value <= m_fixedDocumentSequence.DocumentPaginator.PageCount)
			{
				m_currentHelpDocumentPage = value;
				UpdateDocumentPageVisual(m_currentHelpDocumentPage);
			}
			UpdatePageNumber();
			UpdatePaddleVisibility();
			if (OnHelpDocumentPageChanged != null)
			{
				OnHelpDocumentPageChanged(m_currentHelpDocumentPage);
			}
		}
	}

	public Dictionary<HelpDocuments, HelpDocumentInfo> HelpDocumentInfoList => m_helpDocumentInfoList;

	public event WPFHitHandler OnWPFHit;

	public event HelpDocumentChangedHandler OnHelpDocumentChanged;

	public event HelpDocumentChangedHandler OnHelpDocumentButtonPress;

	public event HelpDocumentPageChangedHandler OnHelpDocumentPageChanged;

	public event HelpPaddleButtonPressed OnHelpPaddleButtonPressed;

	public event Action OnGoBack;

	public event Action OnRedboxLogoCommand;

	public HelpViewUserControl()
	{
		InitializeComponent();
		faq_button.Tag = HelpDocuments.Faq;
		terms_button.Tag = HelpDocuments.Terms;
		terms_redbox_card_button.Tag = HelpDocuments.TermsReboxCard;
		privacy_button.Tag = HelpDocuments.Privacy;
		BorderButtons.Add(faq_button);
		BorderButtons.Add(terms_button);
		BorderButtons.Add(terms_redbox_card_button);
		BorderButtons.Add(privacy_button);
		RegisterRoutedCommand("ShowDocument", Commands.ShowDocument);
		RegisterRoutedCommand("PageLeft", Commands.PageLeft);
		RegisterRoutedCommand("PageRight", Commands.PageRight);
		RegisterRoutedCommand("GoBack", Commands.GoBack);
	}

	public void SetString(string name, string value)
	{
		switch (name)
		{
		case "ButtonGoBack_Text":
			ButtonGoBack_Text.Text = value;
			break;
		case "ButtonFAQ_Text":
			ButtonFAQ_Text.Text = value;
			break;
		case "ButtonTerms_Text":
			ButtonTerms_Text.Text = value;
			break;
		case "ButtonTermsRedboxCard_Text":
			ButtonTermsRedboxCard_Text.Text = value;
			break;
		case "ButtonPrivacy_Text":
			ButtonPrivacy_Text.Text = value;
			break;
		case "title_label":
			title_label.Text = value;
			break;
		case "contact_redbox_label":
			contact_redbox_label.Text = value;
			break;
		case "store_number_text":
			store_number_text.Text = value;
			break;
		case "help_page_counter":
			m_helpPageCounter = value;
			break;
		}
	}

	private void SetTitle()
	{
		HelpDocumentInfo value = null;
		m_helpDocumentInfoList.TryGetValue(m_currentHelpDocument, out value);
		if (value != null)
		{
			title_label.Text = value.Title;
		}
	}

	private void UpdatePageNumber()
	{
		page_counter.Text = ((!string.IsNullOrEmpty(m_helpPageCounter) && m_fixedDocumentSequence != null) ? string.Format(m_helpPageCounter, CurrentHelpDocumentPage, m_fixedDocumentSequence.DocumentPaginator.PageCount) : null);
	}

	private int ChangePageNumber(int changeAmount)
	{
		int val = CurrentHelpDocumentPage + changeAmount;
		CurrentHelpDocumentPage = Math.Min(Math.Max(1, val), (m_fixedDocumentSequence == null) ? 1 : m_fixedDocumentSequence.DocumentPaginator.PageCount);
		return CurrentHelpDocumentPage;
	}

	private void LoadXpsDocument()
	{
		m_xpsDoc = null;
		HelpDocumentInfo value = null;
		m_helpDocumentInfoList.TryGetValue(m_currentHelpDocument, out value);
		if (value != null)
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				m_xpsDoc = service.GetXpsDocument(value.DocumentName);
				if (m_xpsDoc == null)
				{
					LogHelper.Instance.Log("Unable to get Xps document {0}", value.DocumentName);
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("Help document name missing for help document {0}", m_currentHelpDocument);
		}
		m_fixedDocumentSequence = ((m_xpsDoc != null) ? m_xpsDoc.GetFixedDocumentSequence() : null);
		UpdateDocumentPageVisual(1);
	}

	private void UpdateDocumentPageVisual(int pageNumber)
	{
		VisualBrush fill = null;
		if (m_fixedDocumentSequence != null && m_fixedDocumentSequence.DocumentPaginator != null)
		{
			DocumentPage page = m_fixedDocumentSequence.DocumentPaginator.GetPage(Math.Max(0, m_currentHelpDocumentPage - 1));
			if (page != null)
			{
				fill = new VisualBrush(page.Visual);
			}
		}
		DocumentPageVisual.Fill = fill;
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (OnGoBack != null)
		{
			OnGoBack();
		}
		HandleWPFHit();
	}

	private void ChangePage(int changeAmount)
	{
		ChangePageNumber(changeAmount);
		UpdateDocumentPageVisual(m_currentHelpDocumentPage);
		UpdatePageNumber();
		UpdatePaddleVisibility();
	}

	private void UpdatePaddleVisibility()
	{
		left_arrow.Visibility = ((m_fixedDocumentSequence == null || CurrentHelpDocumentPage == 1) ? Visibility.Hidden : Visibility.Visible);
		right_arrow.Visibility = ((m_fixedDocumentSequence == null || CurrentHelpDocumentPage == m_fixedDocumentSequence.DocumentPaginator.PageCount) ? Visibility.Hidden : Visibility.Visible);
	}

	private void PageLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ChangePage(-1);
		AnimatePaddleClick(e.OriginalSource);
		HandleWPFHit();
		CallOnHelpPaddleButtonPressed("Left");
	}

	private void PageRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ChangePage(1);
		AnimatePaddleClick(e.OriginalSource);
		HandleWPFHit();
		CallOnHelpPaddleButtonPressed("Right");
	}

	private void CallOnHelpPaddleButtonPressed(string source)
	{
		if (OnHelpPaddleButtonPressed != null)
		{
			OnHelpPaddleButtonPressed(source);
		}
	}

	private void HandleWPFHit()
	{
		if (OnWPFHit != null)
		{
			OnWPFHit(Actor);
		}
	}

	private void AnimatePaddleClick(object paddle)
	{
	}

	private void ShowDocumentCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		bool flag = false;
		if (e.OriginalSource is Border border)
		{
			CurrentHelpDocument = (HelpDocuments)border.Tag;
			flag = true;
		}
		else if (e.Parameter != null)
		{
			try
			{
				CurrentHelpDocument = (HelpDocuments)Enum.Parse(typeof(HelpDocuments), e.Parameter.ToString());
				flag = true;
			}
			catch
			{
			}
		}
		if (flag)
		{
			CurrentHelpDocumentPage = 1;
			if (OnHelpDocumentButtonPress != null)
			{
				OnHelpDocumentButtonPress(CurrentHelpDocument);
			}
		}
		HandleWPFHit();
	}

	private void RedboxLogoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (OnRedboxLogoCommand != null)
		{
			OnRedboxLogoCommand();
		}
	}
}
