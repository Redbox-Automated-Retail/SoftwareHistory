using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class MarketingScreenSaverModel : BaseModel<MarketingScreenSaverModel>
{
	public Action OnLeaveLastImage;

	public static readonly DependencyProperty IsAnimationOnProperty = BaseModel<MarketingScreenSaverModel>.CreateDependencyProperty("IsAnimationOn", typeof(bool), false);

	public ObservableCollection<ClickableImageModel> ImageModels { get; set; }

	public DynamicRoutedCommand BrowseCommand { get; set; }

	public string StartButtonText { get; set; }

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}
}
