using System.Windows;
using System.Windows.Data;

namespace Redbox.Rental.UI.Views;

[ValueConversion(typeof(string), typeof(Visibility))]
public class TextToVisibilityConverter : ReadOnlyConverter
{
	public override object Convert(object value)
	{
		if (value is string && string.IsNullOrEmpty(value as string))
		{
			return Visibility.Visible;
		}
		return Visibility.Collapsed;
	}
}
