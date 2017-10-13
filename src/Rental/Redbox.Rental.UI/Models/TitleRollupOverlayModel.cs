using System;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class TitleRollupOverlayModel : DependencyObject
{
	public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(16.0));

	public static readonly DependencyProperty DigitalCodeInfoVisibilityProperty = DependencyProperty.Register("DigitalCodeInfoVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Hidden));

	public static readonly DependencyProperty DigitalCodeInfoTextProperty = DependencyProperty.Register("DigitalCodeInfoText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DigitalCodeButtonTextProperty = DependencyProperty.Register("DigitalCodeButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DigitalCodeButtonMarginGridRowHeightProperty = DependencyProperty.Register("DigitalCodeButtonMarginGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DigitalCodeButtonTextGridRowHeightProperty = DependencyProperty.Register("DigitalCodeButtonTextGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DigitalCodeButtonVisibilityProperty = DependencyProperty.Register("DigitalCodeButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty DigitalCodeButtonEnabledProperty = DependencyProperty.Register("DigitalCodeButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BlurayButtonTextProperty = DependencyProperty.Register("BlurayButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonMarginGridRowHeightProperty = DependencyProperty.Register("BlurayButtonMarginGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonTextGridRowHeightProperty = DependencyProperty.Register("BlurayButtonTextGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonVisibilityProperty = DependencyProperty.Register("BlurayButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BlurayButtonEnabledProperty = DependencyProperty.Register("BlurayButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty DVDButtonTextProperty = DependencyProperty.Register("DVDButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonMarginGridRowHeightProperty = DependencyProperty.Register("DVDButtonMarginGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonTextGridRowHeightProperty = DependencyProperty.Register("DVDButtonTextGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonVisibilityProperty = DependencyProperty.Register("DVDButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty DVDButtonEnabledProperty = DependencyProperty.Register("DVDButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty GridDigitalCodeButtonGridRowHeightProperty = DependencyProperty.Register("GridDigitalCodeButtonGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GridDigitalCodeButtonMarginGridRowHeightProperty = DependencyProperty.Register("GridDigitalCodeButtonMarginGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	private TimeSpan _timeout = TimeSpan.FromSeconds(10.0);

	public double FontSize
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(FontSizeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FontSizeProperty, value);
			});
		}
	}

	public string DigitalCodeInfoText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DigitalCodeInfoTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeInfoTextProperty, value);
			});
		}
	}

	public string DigitalCodeButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DigitalCodeButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeButtonTextProperty, value);
			});
		}
	}

	public GridLength DigitalCodeButtonMarginGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(DigitalCodeButtonMarginGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeButtonMarginGridRowHeightProperty, value);
			});
		}
	}

	public GridLength DigitalCodeButtonTextGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(DigitalCodeButtonTextGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeButtonTextGridRowHeightProperty, value);
			});
		}
	}

	public Visibility DigitalCodeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DigitalCodeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility DigitalCodeInfoVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DigitalCodeInfoVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeInfoVisibilityProperty, value);
			});
		}
	}

	public string BlurayButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BlurayButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonTextProperty, value);
			});
		}
	}

	public GridLength BlurayButtonMarginGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(BlurayButtonMarginGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonMarginGridRowHeightProperty, value);
			});
		}
	}

	public GridLength BlurayButtonTextGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(BlurayButtonTextGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonTextGridRowHeightProperty, value);
			});
		}
	}

	public Visibility BlurayButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BlurayButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonVisibilityProperty, value);
			});
		}
	}

	public string DVDButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DVDButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonTextProperty, value);
			});
		}
	}

	public GridLength DVDButtonMarginGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(DVDButtonMarginGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonMarginGridRowHeightProperty, value);
			});
		}
	}

	public GridLength DVDButtonTextGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(DVDButtonTextGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonTextGridRowHeightProperty, value);
			});
		}
	}

	public Visibility DVDButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DVDButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonVisibilityProperty, value);
			});
		}
	}

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

	public bool DVDButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(DVDButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonEnabledProperty, value);
			});
		}
	}

	public bool BlurayButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(BlurayButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonEnabledProperty, value);
			});
		}
	}

	public bool DigitalCodeButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(DigitalCodeButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DigitalCodeButtonEnabledProperty, value);
			});
		}
	}

	public GridLength GridDigitalCodeButtonGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(GridDigitalCodeButtonGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GridDigitalCodeButtonGridRowHeightProperty, value);
			});
		}
	}

	public GridLength GridDigitalCodeButtonMarginGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(GridDigitalCodeButtonMarginGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GridDigitalCodeButtonMarginGridRowHeightProperty, value);
			});
		}
	}

	public IBrowseItemModel BrowseItemModel { get; set; }

	public TimeSpan Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
		}
	}

	public event BrowseItemModelEvent OnTitleRollupOverlayCancel;

	public event BrowseItemModelEvent OnTitleRollupOverlayTimeout;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddDVD;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddBluray;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddDigitalCode;

	public event BrowseItemModelEvent OnTitleRollupOverlayDigitalCodeInfo;

	public event VisibilityChanged OnTitleRollupOverlayVisibilityChange;

	public void ProcessTitleRollupVisibilityChange(Visibility newVisibility)
	{
		if (OnTitleRollupOverlayVisibilityChange != null)
		{
			OnTitleRollupOverlayVisibilityChange(newVisibility);
		}
	}

	public void ProcessTitleRollupOverlayDigitalCodeInfo(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayDigitalCodeInfo != null)
		{
			OnTitleRollupOverlayDigitalCodeInfo(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddDigitalCode(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddDigitalCode != null)
		{
			OnTitleRollupOverlayAddDigitalCode(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddBluray(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAddBluray(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddDVD != null)
		{
			OnTitleRollupOverlayAddDVD(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayCancel != null)
		{
			OnTitleRollupOverlayCancel(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayTimeout != null)
		{
			OnTitleRollupOverlayTimeout(browseItemModel, parameter);
		}
	}
}
