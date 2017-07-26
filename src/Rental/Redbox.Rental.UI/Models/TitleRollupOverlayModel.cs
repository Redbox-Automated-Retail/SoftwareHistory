using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class TitleRollupOverlayModel : DependencyObject
{
	public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(16.0));

	public static readonly DependencyProperty BlurayButtonTextProperty = DependencyProperty.Register("BlurayButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonVisibilityProperty = DependencyProperty.Register("BlurayButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BlurayButtonEnabledProperty = DependencyProperty.Register("BlurayButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty DVDButtonTextProperty = DependencyProperty.Register("DVDButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonVisibilityProperty = DependencyProperty.Register("DVDButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty DVDButtonEnabledProperty = DependencyProperty.Register("DVDButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

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

	public DisplayProductModel DisplayProductModel { get; set; }

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

	public event DisplayProductModelEvent OnTitleRollupOverlayCancel;

	public event DisplayProductModelEvent OnTitleRollupOverlayTimeout;

	public event DisplayProductModelEvent OnTitleRollupOverlayAddDVD;

	public event DisplayProductModelEvent OnTitleRollupOverlayAddBluray;

	public event VisibilityChanged OnTitleRollupOverlayVisibilityChange;

	public void ProcessTitleRollupVisibilityChange(Visibility newVisibility)
	{
		if (OnTitleRollupOverlayVisibilityChange != null)
		{
			OnTitleRollupOverlayVisibilityChange(newVisibility);
		}
	}

	public void ProcessTitleRollupOverlayAddBluray(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAddBluray(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayAddDVD(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayAddDVD != null)
		{
			OnTitleRollupOverlayAddDVD(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayCancel(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayCancel != null)
		{
			OnTitleRollupOverlayCancel(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayTimeout(DisplayProductModel displayProductMode)
	{
		if (OnTitleRollupOverlayTimeout != null)
		{
			OnTitleRollupOverlayTimeout(displayProductMode);
		}
	}
}
