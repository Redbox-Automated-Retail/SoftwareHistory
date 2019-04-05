using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class StarRatingUserControl : UserControl, IComponentConnector
{
	private static DependencyProperty NumberOfStarsProperty = DependencyProperty.Register("NumberOfStars", typeof(int?), typeof(StarRatingUserControl), new PropertyMetadata(0));

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

	public StarRatingUserControl()
	{
		InitializeComponent();
	}
}
