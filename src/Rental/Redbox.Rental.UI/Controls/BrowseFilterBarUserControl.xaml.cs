using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseFilterBarUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private FilterBarModel FilterBarModel => base.DataContext as FilterBarModel;

	public event WPFHitHandler OnWPFHit;

	public BrowseFilterBarUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(startover_button);
		obj?.SetStyle(BackButton);
		obj?.SetStyle(MoreCollectionsButton);
	}

	private void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
	}

	private void StartOverCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnStartOverButtonClicked();
		HandleWPFHit();
	}

	private void StartOverCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(e.NewValue is FilterBarModel filterBarModel))
		{
			return;
		}
		FilterButtonContainer.Children.Clear();
		if (filterBarModel.Filters == null)
		{
			return;
		}
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (BrowseFilter filter in filterBarModel.Filters)
		{
			RoundedButton roundedButton = new RoundedButton
			{
				DataContext = filter,
				Style = GetStyle(this, "classic_rubine_radio_button_style"),
				CornerRadius = 16.0,
				Width = filter.Width,
				Height = filter.Height,
				Margin = filterBarModel.FilterMargin,
				Command = Commands.BrowseFilterCommand
			};
			roundedButton.SetBinding(UIElement.IsEnabledProperty, new Binding
			{
				Path = new PropertyPath("IsButtonEnabled"),
				Source = filter
			});
			TextBlock textBlock = new TextBlock
			{
				Text = filter.Text,
				Style = GetStyle(this, "font_montserrat_regular_14")
			};
			textBlock.SetBinding(TextBlock.TextProperty, new Binding
			{
				Path = new PropertyPath("Text"),
				Source = filter
			});
			roundedButton.Content = textBlock;
			FilterButtonContainer.Children.Add(roundedButton);
			theme?.SetStyle(roundedButton);
		}
	}

	private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		object obj = frameworkElement.FindResource(styleName);
		if (obj != null)
		{
			result = obj as Style;
		}
		return result;
	}

	private void BrowseFilterCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.OriginalSource is RoundedButton { DataContext: BrowseFilter dataContext })
		{
			FilterBarModel?.ProcessOnBrowseFilterButtonClicked(dataContext);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnBackButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void MoreCollectionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnMoreCollectionsButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void HomeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnHomeButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void FormatFilterPopupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnFormatFilterButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}
}
