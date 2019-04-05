using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ReviewsPopupViewModel : BaseModel<ReviewsPopupViewModel>
{
	public static readonly DependencyProperty HeaderTextProperty = BaseModel<ReviewsPopupViewModel>.CreateDependencyProperty("HeaderText", TYPES.STRING);

	public static readonly DependencyProperty BackButtonTextProperty = BaseModel<ReviewsPopupViewModel>.CreateDependencyProperty("BackButtonText", TYPES.STRING);

	public static readonly DependencyProperty ReviewModelsProperty = BaseModel<ReviewsPopupViewModel>.CreateDependencyProperty("ReviewModels", typeof(ObservableCollection<ReviewModel>));

	public static readonly DependencyProperty BackCommandProperty = BaseModel<ReviewsPopupViewModel>.CreateDependencyProperty("BackCommand", typeof(DynamicRoutedCommand));

	public string HeaderText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(HeaderTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HeaderTextProperty, value);
			});
		}
	}

	public string BackButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BackButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonTextProperty, value);
			});
		}
	}

	public ObservableCollection<ReviewModel> ReviewModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<ReviewModel>)GetValue(ReviewModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReviewModelsProperty, value);
			});
		}
	}

	public DynamicRoutedCommand BackCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(BackCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand NextPagePressedCommand { get; set; }

	public DynamicRoutedCommand PreviousPagePressedCommand { get; set; }

	public DynamicRoutedCommand PageNumberPressedCommand { get; set; }
}
