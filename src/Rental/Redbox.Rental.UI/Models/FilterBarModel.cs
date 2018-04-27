using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class FilterBarModel : DependencyObject
{
	public static readonly DependencyProperty FilterBarTextProperty = DependencyProperty.Register("FilterBarText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FilterBarTextVisibilityProperty = DependencyProperty.Register("FilterBarTextVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty MoreCollectionsButtonTextProperty = DependencyProperty.Register("MoreCollectionsButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MoreCollectionsButtonVisibilityProperty = DependencyProperty.Register("MoreCollectionsButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty StartOverButtonVisibilityProperty = DependencyProperty.Register("StartOverButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty HomeButtonVisibilityProperty = DependencyProperty.Register("HomeButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty FormatFilterButtonVisibilityProperty = DependencyProperty.Register("FormatFilterButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty FormatFilterButtonTextProperty = DependencyProperty.Register("FormatFilterButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(string.Empty));

	public static readonly DependencyProperty FormatFilterButtonStyleProperty = DependencyProperty.Register("FormatFilterButtonStyle", typeof(Style), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SelectedFormatProperty = DependencyProperty.Register("SelectedFormat", typeof(TitleType?), typeof(FilterBarModel), new FrameworkPropertyMetadata(null, SelectedFormatChanged));

	public string StartOverButtonText { get; set; }

	public Style StartOverButtonStyle { get; set; }

	public string HomeButtonText { get; set; }

	public string BackButtonText { get; set; }

	public List<BrowseFilter> Filters { get; set; }

	public Thickness FilterMargin { get; set; }

	public Dictionary<TitleType, string> FormatFilterButtonStrings { get; set; }

	public Style FormatFilterButtonUnselectedStyle { get; set; }

	public Style FormatFilterButtonSelectedStyle { get; set; }

	public Visibility MoreCollectionsButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MoreCollectionsButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MoreCollectionsButtonVisibilityProperty, value);
			});
		}
	}

	public string MoreCollectionsButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MoreCollectionsButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MoreCollectionsButtonTextProperty, value);
			});
		}
	}

	public Visibility StartOverButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StartOverButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StartOverButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility HomeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(HomeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HomeButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility BackButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility FormatFilterButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FormatFilterButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility FilterBarTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(FilterBarTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FilterBarTextVisibilityProperty, value);
			});
		}
	}

	public string FilterBarText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FilterBarTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FilterBarTextProperty, value);
			});
		}
	}

	public string FormatFilterButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FormatFilterButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonTextProperty, value);
			});
		}
	}

	public TitleType SelectedFormat
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TitleType)GetValue(SelectedFormatProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SelectedFormatProperty, value);
			});
		}
	}

	public Style FormatFilterButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(FormatFilterButtonStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FormatFilterButtonStyleProperty, value);
			});
		}
	}

	public event Action OnStartOverButtonClicked;

	public event Action OnHomeButtonClicked;

	public event Action OnBackButtonClicked;

	public event Action OnMoreCollectionsButtonClicked;

	public event Action<BrowseFilter> OnBrowseFilterButtonClicked;

	public event Action OnFormatFilterButtonClicked;

	public void ProcessOnStartOverButtonClicked()
	{
		if (OnStartOverButtonClicked != null)
		{
			OnStartOverButtonClicked();
		}
	}

	public void ProcessOnHomeButtonClicked()
	{
		OnHomeButtonClicked?.Invoke();
	}

	public void ProcessOnFormatFilterButtonClicked()
	{
		if (OnFormatFilterButtonClicked != null)
		{
			OnFormatFilterButtonClicked();
		}
	}

	public void ProcessOnBrowseFilterButtonClicked(BrowseFilter browseFilter)
	{
		if (OnBrowseFilterButtonClicked != null)
		{
			OnBrowseFilterButtonClicked(browseFilter);
		}
	}

	public void ProcessOnBackButtonClicked()
	{
		if (OnBackButtonClicked != null)
		{
			OnBackButtonClicked();
		}
	}

	public void ProcessOnMoreCollectionsButtonClicked()
	{
		if (OnMoreCollectionsButtonClicked != null)
		{
			OnMoreCollectionsButtonClicked();
		}
	}

	private static void SelectedFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
	{
		if (obj != null && obj is FilterBarModel)
		{
			FilterBarModel filterBarModel = (FilterBarModel)obj;
			TitleType titleType = ((args.NewValue != null) ? ((TitleType?)args.NewValue).Value : TitleType.All);
			if (filterBarModel.FormatFilterButtonStrings.ContainsKey(titleType))
			{
				filterBarModel.FormatFilterButtonText = filterBarModel.FormatFilterButtonStrings[titleType];
				filterBarModel.FormatFilterButtonStyle = ((titleType == TitleType.All) ? filterBarModel.FormatFilterButtonUnselectedStyle : filterBarModel.FormatFilterButtonSelectedStyle);
			}
		}
	}
}
