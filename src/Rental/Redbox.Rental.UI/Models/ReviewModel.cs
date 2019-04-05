using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ReviewModel : BaseModel<ReviewModel>
{
	public static DependencyProperty NumberOfStarsProperty = BaseModel<ReviewModel>.CreateDependencyProperty("NumberOfStars", typeof(int?));

	public int? NumberOfStars
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int?)GetValue(NumberOfStarsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NumberOfStarsProperty, value);
			});
		}
	}

	public string ReviewerName { get; set; }

	public string ReviewDate { get; set; }

	public string ReviewText { get; set; }
}
