using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class FormatButtonData
{
	public string Text { get; set; }

	public string Line2Text { get; set; }

	public TitleType Format { get; set; }

	public bool IsEnabled { get; set; }

	public FormatButtonData(string text, TitleType format, string line2Text = null, bool isEnabled = true)
	{
		Text = text;
		Line2Text = line2Text;
		Format = format;
		IsEnabled = isEnabled;
	}
}
