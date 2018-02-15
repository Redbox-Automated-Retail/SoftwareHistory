using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsView")]
public partial class TitleDetailsUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private TitleDetailModel Model => base.DataContext as TitleDetailModel;

	public TitleDetailsUserControl()
	{
		InitializeComponent();
		base.Loaded += delegate
		{
			Keyboard.Focus(this);
		};
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}

	private UserControl GetBrowseItemUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is UserControl))
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
		return frameworkElement as UserControl;
	}

	private void GoBack(string reason, bool popOnly = false)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBack("go back", popOnly);
	}

	private void OnGoBack_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		GoBack("go_back");
		HandleWPFHit();
	}

	private void Collection_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel browseItemModel = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		_ = browseItemModel?.Data;
		Model.ProcessCollectionButtonHit(browseItemModel);
		HandleWPFHit();
	}

	private void Buy_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessBuyButtonHit(model, e.Parameter);
		HandleWPFHit();
	}

	private void RentPrimary_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessAddPrimaryItem(model, e.Parameter);
		HandleWPFHit();
	}

	private void RentBluRay_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessAddBluRay(model, e.Parameter);
		ServiceLocator.Instance.GetService<IViewService>();
		HandleWPFHit();
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void MoreInfo_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessMoreInfoHit(model);
		HandleWPFHit();
	}

	private void StatisticsPopup_Executed(object sender, ExecutedRoutedEventArgs e)
	{
	}
}
