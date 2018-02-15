using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class BrowseViewModel : DependencyObject
{
	public Action OnCheckOutButtonClicked;

	public Action OnToggleTitleFamilyButtonClicked;

	public Action OnSignInButtonClicked;

	public Action OnSignOutButtonClicked;

	public Action OnMyPerksButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseGroupSelectionProperty = DependencyProperty.Register("BrowseGroupSelection", typeof(DisplayGroupSelectionModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DisplayGroupItemsProperty = DependencyProperty.Register("DisplayGroupItems", typeof(List<DisplayGroupSelectionItemModel>), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonTextProperty = DependencyProperty.Register("CheckOutButtonText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CheckOutButtonStyleProperty = DependencyProperty.Register("CheckOutButtonStyle", typeof(Style), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AddToADACartLabelTextProperty = DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SwitchProductFamilyButtonVisibilityProperty = DependencyProperty.Register("SwitchProductFamilyButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddToADACartLabelVisibilityProperty = DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty MiniCartVisibilityProperty = DependencyProperty.Register("MiniCartVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BackgroundImageVisibilityProperty = DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SwitchProductFamilyButtonTextProperty = DependencyProperty.Register("SwitchProductFamilyButtonText", typeof(string), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

	public static readonly DependencyProperty MiniCartControlModelProperty = DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseFilterBarModelProperty = DependencyProperty.Register("BrowseFilterBarModel", typeof(FilterBarModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseTopMenuModelProperty = DependencyProperty.Register("BrowseTopMenuModel", typeof(TopMenuModel), typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SignInButtonVisibilityProperty = DependencyProperty.Register("SignInButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SignOutButtonVisibilityProperty = DependencyProperty.Register("SignOutButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty MyPerksButtonVisibilityProperty = DependencyProperty.Register("MyPerksButtonVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty EmptyCartSpacesVisibilityProperty = DependencyProperty.Register("EmptyCartSpacesVisibilityVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty EmptyCartAddItemMessageVisibilityProperty = DependencyProperty.Register("EmptyCartAddItemMessageVisibility", typeof(Visibility), typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public string SignInButtonText { get; set; }

	public string SignOutButtonText { get; set; }

	public string MyPerksButtonText { get; set; }

	public string DontMissYourPerksText { get; set; }

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

	public Visibility MiniCartVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MiniCartVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MiniCartVisibilityProperty, value);
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

	public Visibility SignInButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility SignOutButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignOutButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignOutButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility MyPerksButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MyPerksButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MyPerksButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility EmptyCartSpacesVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartSpacesVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmptyCartSpacesVisibilityProperty, value);
			});
		}
	}

	public Visibility EmptyCartAddItemMessageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartAddItemMessageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmptyCartAddItemMessageVisibilityProperty, value);
			});
		}
	}

	public string AddToCartLabel1Text { get; set; }

	public string AddToCartLabel2Text { get; set; }

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

	public void ProcessOnSignInButtonClicked()
	{
		OnSignInButtonClicked?.Invoke();
	}

	public void ProcessOnSignOutButtonClicked()
	{
		OnSignOutButtonClicked?.Invoke();
	}

	public void ProcessOnMyPerksButtonClicked()
	{
		OnMyPerksButtonClicked?.Invoke();
	}

	private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BrowseViewModel { OnBrowseProductControlModelChanged: not null } browseViewModel)
		{
			browseViewModel.OnBrowseProductControlModelChanged(browseViewModel);
		}
	}
}
