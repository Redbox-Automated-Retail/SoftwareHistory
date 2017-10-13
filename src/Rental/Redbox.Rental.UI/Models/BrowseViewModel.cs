using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class BrowseViewModel : DependencyObject
{
	public Action OnCheckOutButtonClicked;

	public Action OnToggleTitleFamilyButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseGroupSelectionProperty = DependencyProperty.Register("BrowseGroupSelection", typeof(DisplayGroupSelectionModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayGroupItemsProperty = DependencyProperty.Register("DisplayGroupItems", typeof(List<DisplayGroupSelectionItemModel>), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonTextProperty = DependencyProperty.Register("CheckOutButtonText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonStyleProperty = DependencyProperty.Register("CheckOutButtonStyle", typeof(Style), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToCartLabel1TextProperty = DependencyProperty.Register("AddToCartLabel1Text", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToCartLabel2TextProperty = DependencyProperty.Register("AddToCartLabel2Text", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToADACartLabelTextProperty = DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToCartLabelVisibilityProperty = DependencyProperty.Register("AddToCartLabelVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SwitchProductFamilyButtonVisibilityProperty = DependencyProperty.Register("SwitchProductFamilyButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddToADACartLabelVisibilityProperty = DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BackgroundImageVisibilityProperty = DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SwitchProductFamilyButtonTextProperty = DependencyProperty.Register("SwitchProductFamilyButtonText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

	public static readonly DependencyProperty MiniCartControlModelProperty = DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseFilterBarModelProperty = DependencyProperty.Register("BrowseFilterBarModel", typeof(FilterBarModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseTopMenuModelProperty = DependencyProperty.Register("BrowseTopMenuModel", typeof(TopMenuModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public BrowseControlModel BrowseProductControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseProductControlModelProperty, value);
			});
		}
	}

	public BrowseControlModel MiniCartControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(MiniCartControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartControlModelProperty, value);
			});
		}
	}

	public FilterBarModel BrowseFilterBarModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (FilterBarModel)GetValue(BrowseFilterBarModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseFilterBarModelProperty, value);
			});
		}
	}

	public TopMenuModel BrowseTopMenuModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TopMenuModel)GetValue(BrowseTopMenuModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseTopMenuModelProperty, value);
			});
		}
	}

	public string CheckOutButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CheckOutButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonTextProperty, value);
			});
		}
	}

	public Style CheckOutButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(CheckOutButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckOutButtonStyleProperty, value);
			});
		}
	}

	public string AddToCartLabel1Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddToCartLabel1TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToCartLabel1TextProperty, value);
			});
		}
	}

	public string AddToCartLabel2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddToCartLabel2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToCartLabel2TextProperty, value);
			});
		}
	}

	public string AddToADACartLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddToADACartLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToADACartLabelTextProperty, value);
			});
		}
	}

	public Visibility AddToADACartLabelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddToADACartLabelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToADACartLabelVisibilityProperty, value);
			});
		}
	}

	public Visibility AddToCartLabelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddToCartLabelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddToCartLabelVisibilityProperty, value);
			});
		}
	}

	public Visibility SwitchProductFamilyButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SwitchProductFamilyButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SwitchProductFamilyButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility BackgroundImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundImageVisibilityProperty, value);
			});
		}
	}

	public string SwitchProductFamilyButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SwitchProductFamilyButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SwitchProductFamilyButtonTextProperty, value);
			});
		}
	}

	public bool IsADAAccessible { get; set; }

	public DisplayGroupSelectionModel BrowseGroupSelection
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayGroupSelectionModel)GetValue(BrowseGroupSelectionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseGroupSelectionProperty, value);
			});
		}
	}

	public event Action<BrowseViewModel> OnBrowseProductControlModelChanged;

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}

	public void ProcessOnCheckOutButtonClicked()
	{
		if (OnCheckOutButtonClicked != null)
		{
			OnCheckOutButtonClicked();
		}
	}

	public void ProcessOnToggleTitleFamilyButtonClicked()
	{
		if (OnToggleTitleFamilyButtonClicked != null)
		{
			OnToggleTitleFamilyButtonClicked();
		}
	}

	private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BrowseViewModel { OnBrowseProductControlModelChanged: not null } browseViewModel)
		{
			browseViewModel.OnBrowseProductControlModelChanged(browseViewModel);
		}
	}
}
