using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class CheckMarkButtonModel : DependencyObject
{
	private bool _isSelected;

	private bool _showArrow;

	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	private static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(8.0, 0.0, 8.0, 0.0)));

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(null));

	private static readonly DependencyProperty TextPaddingProperty = DependencyProperty.Register("TextPadding", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(24.0, 0.0, 24.0, 0.0)));

	private static readonly DependencyProperty ArrowImageVisibilityProperty = DependencyProperty.Register("ArrowImageVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register("ImageMargin", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(-35.0, -35.0, 0.0, 0.0)));

	private static readonly DependencyProperty ImageVisibilityProperty = DependencyProperty.Register("ImageVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Hidden));

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			Style = (_isSelected ? SelectedStyle : UnselectedStyle);
			ImageVisibility = ((!_isSelected) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	public bool ShowArrow
	{
		get
		{
			return _showArrow;
		}
		set
		{
			_showArrow = value;
			TextPadding = (_showArrow ? new Thickness(24.0, 0.0, 4.0, 0.0) : new Thickness(24.0, 0.0, 24.0, 0.0));
			ArrowImageVisibility = ((!_showArrow) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private Style UnselectedStyle { get; set; }

	private Style SelectedStyle { get; set; }

	public Visibility Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(VisibilityProperty, value);
			});
		}
	}

	private Style Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StyleProperty, value);
			});
		}
	}

	public Thickness Margin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(MarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MarginProperty, value);
			});
		}
	}

	public string Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextProperty, value);
			});
		}
	}

	private Thickness TextPadding
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(TextPaddingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextPaddingProperty, value);
			});
		}
	}

	private Visibility ArrowImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ArrowImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ArrowImageVisibilityProperty, value);
			});
		}
	}

	public Thickness ImageMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(ImageMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageMarginProperty, value);
			});
		}
	}

	private Visibility ImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageVisibilityProperty, value);
			});
		}
	}

	public event Action<CheckMarkButtonModel> OnClicked;

	public CheckMarkButtonModel(Style unselectedStyle, Style selectedStyle)
	{
		UnselectedStyle = unselectedStyle;
		SelectedStyle = selectedStyle;
	}

	public void ProcessOnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		OnClicked?.Invoke(checkMarkButtonModel);
	}
}
