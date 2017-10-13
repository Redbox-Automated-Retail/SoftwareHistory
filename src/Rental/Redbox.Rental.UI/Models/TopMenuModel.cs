using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class TopMenuModel : DependencyObject
{
	public static readonly DependencyProperty MenuBackgroundStyleProperty = DependencyProperty.Register("MenuBackgroundStyle", typeof(Style), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MenuTextStyleProperty = DependencyProperty.Register("MenuTextStyle", typeof(Style), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	private List<BrowseMenuButton> _menuButtons = new List<BrowseMenuButton>();

	public List<BrowseMenuButton> MenuButtons
	{
		get
		{
			return _menuButtons;
		}
		set
		{
			_menuButtons = value;
		}
	}

	public Visibility MenuBackgroundVisibility { get; set; }

	public string MenuBackgroundText { get; set; }

	public Style MenuTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MenuTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MenuTextStyleProperty, value);
			});
		}
	}

	public Style MenuBackgroundStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MenuBackgroundStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MenuBackgroundStyleProperty, value);
			});
		}
	}

	public event Action<BrowseMenuButton> OnBrowseMenuButtonClicked;

	public void ProcessOnBrowseMenuButtonClicked(BrowseMenuButton browseMenuButton)
	{
		if (OnBrowseMenuButtonClicked != null)
		{
			OnBrowseMenuButtonClicked(browseMenuButton);
		}
	}
}
