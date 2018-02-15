using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseTopMenuUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private TopMenuModel BrowseTopMenuModel => base.DataContext as TopMenuModel;

	public event WPFHitHandler OnWPFHit;

	public BrowseTopMenuUserControl()
	{
		InitializeComponent();
	}

	private void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
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

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(e.NewValue is TopMenuModel topMenuModel))
		{
			return;
		}
		NonFixedButtonContainer.Children.Clear();
		NonFixedButtonContainer.ColumnDefinitions.Clear();
		FixedButtonContainer.Children.Clear();
		if (topMenuModel.MenuButtons == null)
		{
			return;
		}
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (BrowseMenuButton menuButton in topMenuModel.MenuButtons)
		{
			RoundedButton roundedButton = new RoundedButton
			{
				DataContext = menuButton,
				Style = GetStyle(this, "classic_menu_button_style"),
				CornerRadius = 0.0,
				Height = menuButton.Height,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			roundedButton.Click += MenuButton_Click;
			roundedButton.SetBinding(UIElement.IsEnabledProperty, new Binding
			{
				Path = new PropertyPath("IsButtonEnabled"),
				Source = menuButton
			});
			TextBlock textBlock = new TextBlock
			{
				Text = menuButton.Text,
				Style = GetStyle(this, menuButton.TextStyleName),
				TextWrapping = TextWrapping.Wrap,
				TextAlignment = TextAlignment.Center
			};
			textBlock.SetBinding(TextBlock.TextProperty, new Binding
			{
				Path = new PropertyPath("Text"),
				Source = menuButton
			});
			BrowseProductFamilyMenuButton browseProductFamilyMenuButton = menuButton as BrowseProductFamilyMenuButton;
			StackPanel stackPanel = null;
			if (browseProductFamilyMenuButton != null)
			{
				stackPanel = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				UserControl userControl = new UserControl
				{
					Style = GetStyle(this, (browseProductFamilyMenuButton.ArrowDirection == ArrowDirection.Right) ? "right_arrow" : "left_arrow"),
					Margin = new Thickness(10.0, 0.0, 10.0, 0.0),
					VerticalAlignment = VerticalAlignment.Center
				};
				UIElement element = ((browseProductFamilyMenuButton.ArrowDirection == ArrowDirection.Right) ? ((UIElement)textBlock) : ((UIElement)userControl));
				UIElement element2 = ((browseProductFamilyMenuButton.ArrowDirection == ArrowDirection.Right) ? ((UIElement)userControl) : ((UIElement)textBlock));
				stackPanel.Children.Add(element);
				stackPanel.Children.Add(element2);
			}
			if (menuButton.FixedPosition)
			{
				roundedButton.VerticalAlignment = VerticalAlignment.Top;
				roundedButton.HorizontalAlignment = HorizontalAlignment.Left;
				roundedButton.Width = menuButton.Width;
				roundedButton.Margin = new Thickness(menuButton.X, menuButton.Y, 0.0, 0.0);
			}
			roundedButton.Content = ((stackPanel != null) ? ((object)stackPanel) : ((object)textBlock));
			if (menuButton.FixedPosition)
			{
				FixedButtonContainer.Children.Add(roundedButton);
			}
			else
			{
				ColumnDefinition columnDefinition = new ColumnDefinition();
				if (menuButton.Width != 0)
				{
					columnDefinition.Width = new GridLength(menuButton.Width);
				}
				NonFixedButtonContainer.ColumnDefinitions.Add(columnDefinition);
				int value = topMenuModel.MenuButtons.IndexOf(menuButton);
				Grid.SetColumn(roundedButton, value);
				Grid.SetRow(roundedButton, 0);
				roundedButton.HorizontalAlignment = HorizontalAlignment.Stretch;
				NonFixedButtonContainer.Children.Add(roundedButton);
			}
			theme?.SetStyle(roundedButton);
		}
	}

	private void MenuButton_Click(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is RoundedButton { DataContext: BrowseMenuButton dataContext })
		{
			BrowseTopMenuModel?.ProcessOnBrowseMenuButtonClicked(dataContext);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}
}
