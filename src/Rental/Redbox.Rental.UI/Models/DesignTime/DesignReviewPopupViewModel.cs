using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.Models.DesignTime;

public class DesignReviewPopupViewModel : BaseModel<DesignReviewPopupViewModel>
{
	public static readonly DependencyProperty HeaderTextProperty = BaseModel<DesignReviewPopupViewModel>.CreateDependencyProperty("HeaderText", TYPES.STRING);

	public static readonly DependencyProperty BackButtonTextProperty = BaseModel<DesignReviewPopupViewModel>.CreateDependencyProperty("BackButtonText", TYPES.STRING);

	public static readonly DependencyProperty ReviewModelsProperty = BaseModel<DesignReviewPopupViewModel>.CreateDependencyProperty("ReviewModels", typeof(ObservableCollection<ReviewModel>));

	public static readonly DependencyProperty BackCommandProperty = BaseModel<DesignReviewPopupViewModel>.CreateDependencyProperty("BackCommand", typeof(DynamicRoutedCommand));

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

	public DesignReviewPopupViewModel()
	{
		BackButtonText = Resources.back_button;
		HeaderText = Resources.reviews_title;
		ReviewModels = new ObservableCollection<ReviewModel>
		{
			new ReviewModel
			{
				NumberOfStars = 2,
				ReviewerName = "Grumpy Cat",
				ReviewDate = "July 7, 2017",
				ReviewText = "Cat ipsum dolor, ask for petting for find something else more interesting hack up furballs, so swat mice around the house. Flop over roll over and sun my belly or refuse to drink water except out of someone's glass sweet beast, but furrier and more furrier hairball. Mewl for food at 4am i like cats because they are fat and fluffy. Have my breakfast spaghetti yarn cat ears lick paws slap owner's face at 5am until human fills food dish purr as loud as possible, be the most annoying cat ever."
			},
			new ReviewModel
			{
				NumberOfStars = 4,
				ReviewerName = "Grumpy Cat",
				ReviewDate = "July 7, 2017",
				ReviewText = "Cat ipsum dolor, ask for petting for find something else more interesting hack up furballs, so swat mice around the house. Flop over roll over and sun my belly or refuse to drink water except out of someone's glass sweet beast, but furrier and more furrier hairball. Mewl for food at 4am i like cats because they are fat and fluffy. Have my breakfast spaghetti yarn cat ears lick paws slap owner's face at 5am until human fills food dish purr as loud as possible, be the most annoying cat ever."
			},
			new ReviewModel
			{
				NumberOfStars = 5,
				ReviewerName = "Grumpy Cat",
				ReviewDate = "July 7, 2017",
				ReviewText = "Best movie of all time"
			},
			new ReviewModel
			{
				NumberOfStars = 1,
				ReviewerName = "Grumpy Cat",
				ReviewDate = "July 7, 2017",
				ReviewText = "Worst movie of all time"
			}
		};
	}
}
