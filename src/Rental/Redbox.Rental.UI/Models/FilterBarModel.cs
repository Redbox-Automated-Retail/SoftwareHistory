using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class FilterBarModel : DependencyObject
{
	public static readonly DependencyProperty FilterBarTextProperty = DependencyProperty.Register("FilterBarText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FilterBarTextVisibilityProperty = DependencyProperty.Register("FilterBarTextVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty MoreCollectionsButtonTextProperty = DependencyProperty.Register("MoreCollectionsButtonText", typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MoreCollectionsButtonVisibilityProperty = DependencyProperty.Register("MoreCollectionsButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty StartOverButtonVisibilityProperty = DependencyProperty.Register("StartOverButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public string StartOverButtonText { get; set; }

	public Style StartOverButtonStyle { get; set; }

	public string BackButtonText { get; set; }

	public List<BrowseFilter> Filters { get; set; }

	public Thickness FilterMargin { get; set; }

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

	public event Action OnStartOverButtonClicked;

	public event Action OnBackButtonClicked;

	public event Action OnMoreCollectionsButtonClicked;

	public event Action<BrowseFilter> OnBrowseFilterButtonClicked;

	public void ProcessOnStartOverButtonClicked()
	{
		if (OnStartOverButtonClicked != null)
		{
			OnStartOverButtonClicked();
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
}
