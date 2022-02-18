using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class BrowseMessagePopupModel : DependencyObject
{
	public Action OnBrowseMessagePopupInfoClicked;

	public Action OnBrowseMessagePopupCloseClicked;

	public static readonly DependencyProperty BrowseMessagePopupUserControlVisibilityProperty = DependencyProperty.Register("BrowseMessagePopupUserControlVisibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty StarzImageVisibilityProperty = DependencyProperty.Register("StarzImageVisibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty Text1VisibilityProperty = DependencyProperty.Register("Text1Visibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty StarzText1VisibilityProperty = DependencyProperty.Register("StarzText1Visibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty Text2VisibilityProperty = DependencyProperty.Register("Text2Visibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty StarzText2VisibilityProperty = DependencyProperty.Register("StarzText2Visibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public string Text1 { get; set; }

	public string StarzText1 { get; set; }

	public string Text2 { get; set; }

	public string StarzText2 { get; set; }

	public string InfoButtonText { get; set; }

	public Visibility BrowseMessagePopupUserControlVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BrowseMessagePopupUserControlVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseMessagePopupUserControlVisibilityProperty, value);
			});
		}
	}

	public Visibility StarzImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StarzImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarzImageVisibilityProperty, value);
			});
		}
	}

	public Visibility Text1Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(Text1VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Text1VisibilityProperty, value);
			});
		}
	}

	public Visibility StarzText1Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StarzText1VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarzText1VisibilityProperty, value);
			});
		}
	}

	public Visibility Text2Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(Text2VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Text2VisibilityProperty, value);
			});
		}
	}

	public Visibility StarzText2Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(StarzText2VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StarzText2VisibilityProperty, value);
			});
		}
	}

	public void ProcessOnBrowseMessagePopupInfoClicked()
	{
		OnBrowseMessagePopupInfoClicked?.Invoke();
	}

	public void ProcessOnBrowseMessagePopupCloseClicked()
	{
		OnBrowseMessagePopupCloseClicked?.Invoke();
	}
}
