using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "BrowseView")]
public partial class BrowseViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private BrowseViewModel _browseViewModel;

	public BrowseViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return _browseViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(TitleRollupOverlay);
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_browseViewModel = base.DataContext as BrowseViewModel;
		if (_browseViewModel != null && _browseViewModel.BrowseProductControlModel != null)
		{
			_browseViewModel.BrowseProductControlModel.OnSelectedDisplayProductUserControlSizeAndPositionChanged += _browseControlModel_OnSelectedDisplayProductUserControlSizeAndPositionChanged;
			_browseViewModel.OnBrowseProductControlModelChanged += _browseControlModel_OnBrowseProductControlModelChanged;
		}
	}

	private void _browseControlModel_OnBrowseProductControlModelChanged(BrowseViewModel browseViewModel)
	{
		if (browseViewModel != null)
		{
			browseViewModel.BrowseProductControlModel.OnSelectedDisplayProductUserControlSizeAndPositionChanged += _browseControlModel_OnSelectedDisplayProductUserControlSizeAndPositionChanged;
		}
	}

	private void _browseControlModel_OnSelectedDisplayProductUserControlSizeAndPositionChanged(BrowseControlModel obj)
	{
		if (_browseViewModel != null)
		{
			TitleRollupOverlay.Width = _browseViewModel.BrowseProductControlModel.SelectedBrowseItemSizeAndPosition.Width;
			TitleRollupOverlay.Height = _browseViewModel.BrowseProductControlModel.SelectedBrowseItemSizeAndPosition.Height;
			TitleRollupOverlay.Margin = new Thickness(_browseViewModel.BrowseProductControlModel.SelectedBrowseItemSizeAndPosition.Left, _browseViewModel.BrowseProductControlModel.SelectedBrowseItemSizeAndPosition.Top + BrowseProductControl.Margin.Top, 0.0, 0.0);
		}
	}
}
