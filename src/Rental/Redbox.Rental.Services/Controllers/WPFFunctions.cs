using System.Windows;

namespace Redbox.Rental.Services.Controllers;

public class WPFFunctions
{
	public static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		if (!string.IsNullOrEmpty(styleName))
		{
			object obj = frameworkElement?.FindResource(styleName);
			if (obj != null)
			{
				result = obj as Style;
			}
		}
		return result;
	}
}
