using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class BrowseMessagePopupModel : DependencyObject
{
	public Action<BrowseMessagePopupModel> OnBrowseMessagePopupInfoClicked;

	public Action<BrowseMessagePopupModel> OnBrowseMessagePopupCloseClicked;

	public static readonly DependencyProperty BrowseMessagePopupUserControlVisibilityProperty = DependencyProperty.Register("BrowseMessagePopupUserControlVisibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public string Text1 { get; set; }

	public string Text2 { get; set; }

	public string Text3 { get; set; }

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

	public void ProcessOnBrowseMessagePopupInfoClicked(BrowseMessagePopupModel browseMessagePopupModel)
	{
		OnBrowseMessagePopupInfoClicked?.Invoke(browseMessagePopupModel);
	}

	public void ProcessOnBrowseMessagePopupCloseClicked(BrowseMessagePopupModel browseMessagePopupModel)
	{
		OnBrowseMessagePopupCloseClicked?.Invoke(browseMessagePopupModel);
	}
}
